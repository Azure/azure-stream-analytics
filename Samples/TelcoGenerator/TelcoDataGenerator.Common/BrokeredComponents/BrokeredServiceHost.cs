//---------------------------------------------------------------------------------
// Microsoft (R)  Windows Azure ServiceBus Messaging sample
// 
// Copyright (c) Microsoft Corporation. All rights reserved.  
//
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES 
// OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
//---------------------------------------------------------------------------------

namespace Microsoft.Samples.ServiceBusMessaging
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;

    public class BrokeredServiceHost : ServiceHost
    {
        const string DefaultNamespace = "http://tempuri.org/";
        ReceiveMode receiveMode;
        bool requiresSession;
        Dictionary<string, string> actionMap = new Dictionary<string, string>();

        public BrokeredServiceHost(
            object singletonInstance, 
            Uri address,
            TokenProvider tokenProvider,
            ReceiveMode receiveMode,
            bool requiresSession)
            : base(singletonInstance)
        {
            AddBrokeredServiceBehavior(singletonInstance.GetType(), address, tokenProvider, receiveMode, requiresSession);
        }

        public BrokeredServiceHost(
            Type serviceType, 
            Uri address,
            TokenProvider tokenProvider,
            ReceiveMode receiveMode,
            bool requiresSession) 
            : base(serviceType)
        {
            AddBrokeredServiceBehavior(serviceType, address, tokenProvider, receiveMode, requiresSession);
        }

        private void AddBrokeredServiceBehavior(Type serviceType, Uri address, TokenProvider tokenProvider, ReceiveMode receiveMode, bool requiresSession)
        {
            this.receiveMode = receiveMode;
            this.requiresSession = requiresSession;
            ContractDescription contractDescription = CreateContractDescription(serviceType);

            ServiceEndpoint endpoint = new ServiceEndpoint(contractDescription, new BrokeredBinding(), address != null ? new EndpointAddress(address) : null);
            this.Description.Behaviors.Insert(0, new BrokeredServiceBehavior(endpoint, tokenProvider, receiveMode, this.actionMap));
        }

        private ContractDescription CreateContractDescription(Type serviceType)
        {
            ContractDescription result = new ContractDescription(serviceType.Name, DefaultNamespace);
            result.ContractType = serviceType;

            MethodInfo defaultOnReceiveMessageMethod = null;
            HashSet<Type> possibleReturnTypes = new HashSet<Type>() { typeof(void), typeof(Task) };

            foreach (MethodInfo method in serviceType.GetMethods(BindingFlags.Instance | BindingFlags.Public))
            {
                object[] onMessageAttributes = method.GetCustomAttributes(typeof(OnMessageAttribute), false);
                if (onMessageAttributes.Length > 0)
                {
                    OnMessageAttribute onMessageAttribute = (OnMessageAttribute)onMessageAttributes[0];
                    if (MethodMatches(method, possibleReturnTypes, typeof(BrokeredMessage)))
                    {
                        if (onMessageAttribute.Action == OnMessageAttribute.WildcardAction)
                        {
                            defaultOnReceiveMessageMethod = method;
                        }
                        this.actionMap.Add(onMessageAttribute.Action, method.Name);
                        result.Operations.Add(CreateOperationDescription(result, serviceType, method));
                    }
                }
            }

            if (defaultOnReceiveMessageMethod == null)
            {
                defaultOnReceiveMessageMethod = FindConventionOrUniqueMatchingMethod(serviceType, "OnReceiveMessage", possibleReturnTypes, typeof(BrokeredMessage));

                if (defaultOnReceiveMessageMethod != null && defaultOnReceiveMessageMethod.GetCustomAttributes(typeof(OnMessageAttribute), false).Length == 0)
                {
                    this.actionMap.Add(OnMessageAttribute.WildcardAction, defaultOnReceiveMessageMethod.Name);
                    result.Operations.Add(CreateOperationDescription(result, serviceType, defaultOnReceiveMessageMethod));
                }
            }

            if (this.actionMap.Count == 0)
            {
                throw new Exception("No methods found to handle messages.");
            }

            if (this.requiresSession)
            {
                result.SessionMode = SessionMode.Required;
            }

            return result;
        }

        internal static MethodInfo FindConventionOrUniqueMatchingMethod(Type type, string conventionMethodName, HashSet<Type> possibleReturnTypes, params Type[] parameters)
        {
            MethodInfo method = GetMethod(type, conventionMethodName, possibleReturnTypes, parameters);
            if (method == null)
            {
                return FindUniqueMatchingMethod(type, possibleReturnTypes, parameters);
            }
            else
            {
                return method;
            }
        }

        internal static MethodInfo GetMethod(Type type, string methodName, HashSet<Type> possibleReturnTypes, params Type[] parameters)
        {
            MethodInfo method = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public, null, CallingConventions.Any, parameters, null);
            return (method != null && possibleReturnTypes.Contains(method.ReturnType)) ? method : null;
        }

        internal static MethodInfo FindUniqueMatchingMethod(Type type, HashSet<Type> possibleReturnTypes, params Type[] parameters)
        {
            bool matchFound = false;
            MethodInfo match = null;
            foreach (MethodInfo method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
            {
                if (MethodMatches(method, possibleReturnTypes, parameters))
                {
                    if (!matchFound)
                    {
                        matchFound = true;
                        match = method;
                    }
                    else
                    {
                        match = null;
                        break;
                    }
                }
            }
            return match;
        }

        internal static bool MethodMatches(MethodInfo method, HashSet<Type> possibleReturnTypes, params Type[] parameterTypes)
        {
            if (possibleReturnTypes.Contains(method.ReturnType))
            {
                ParameterInfo[] parameters = method.GetParameters();
                if (parameters.Length == parameterTypes.Length)
                {
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        if (parameters[i].ParameterType != parameterTypes[i])
                        {
                            return false;
                        }
                    }
                    return true;
                }
            }
            return false;
        }

        private OperationDescription CreateOperationDescription(ContractDescription contract, Type serviceType, MethodInfo method)
        {
            string operationName = method.Name;
            ParameterInfo[] parameters = method.GetParameters();
            OperationDescription result = new OperationDescription(operationName, contract);
            result.SyncMethod = method;

            MessageDescription inputMessage = new MessageDescription(DefaultNamespace + serviceType.Name + "/" + operationName, MessageDirection.Input);
            inputMessage.Body.WrapperNamespace = DefaultNamespace;
            inputMessage.Body.WrapperName = operationName;
            for (int i = 0; i < parameters.Length; i++)
            {
                ParameterInfo parameter = parameters[i];
                MessagePartDescription part = new MessagePartDescription(parameter.Name, DefaultNamespace);
                part.Type = parameter.ParameterType;
                part.Index = i;
                inputMessage.Body.Parts.Add(part);
            }

            result.Messages.Add(inputMessage);

            AddOperationBehaviors(result, method);
            return result;
        }

        private void AddOperationBehaviors(OperationDescription result, MethodInfo method)
        {
            if (this.receiveMode == ReceiveMode.PeekLock)
            {
                result.Behaviors.Add(new ReceiveContextEnabledAttribute() { ManualControl = false });
            }

            foreach (object attribute in method.GetCustomAttributes(false))
            {
                if (attribute is IOperationBehavior)
                {
                    result.Behaviors.Add((IOperationBehavior)attribute);
                }
            }
        }

    }
}
