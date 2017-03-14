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
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Reflection;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;

    class BrokeredServiceBehavior : IServiceBehavior
    {
        ServiceEndpoint endpoint;
        TokenProvider tokenProvider;
        ReceiveMode receiveMode;
        Dictionary<string, string> actionMap;

        public BrokeredServiceBehavior(ServiceEndpoint endpoint, TokenProvider tokenProvider, ReceiveMode receiveMode, Dictionary<string, string> actionMap)
        {
            this.endpoint = endpoint;
            this.tokenProvider = tokenProvider;
            this.receiveMode = receiveMode;
            this.actionMap = actionMap;
        }

        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            Type serviceType = serviceDescription.ServiceType;

            serviceDescription.Endpoints.Add(endpoint);
            ChannelDispatcher dispatcher = CreateChannelDispatcher(endpoint, serviceType, actionMap);
            serviceHostBase.ChannelDispatchers.Add(dispatcher);
        }

        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            Type serviceType = serviceDescription.ServiceType;
            // Validate the Service Type
        }

        private ChannelDispatcher CreateChannelDispatcher(ServiceEndpoint endpoint, Type serviceType, Dictionary<string, string> actionMap)
        {
            EndpointAddress address = endpoint.Address;
            Binding binding = endpoint.Binding;
            BindingParameterCollection bindingParameters = new BindingParameterCollection();
            ((IEndpointBehavior)new TransportClientEndpointBehavior(tokenProvider)).AddBindingParameters(endpoint, bindingParameters);
            if (receiveMode == ReceiveMode.PeekLock)
            {
                IReceiveContextSettings receiveContextSettings = binding.GetProperty<IReceiveContextSettings>(bindingParameters);
                Debug.Assert(receiveContextSettings != null);
                receiveContextSettings.Enabled = true;
            }

            IChannelListener channelListener;
            if (endpoint.Contract.SessionMode == SessionMode.Required)
            {
                channelListener = binding.BuildChannelListener<IInputSessionChannel>(address.Uri, bindingParameters);
            }
            else
            {
                channelListener = binding.BuildChannelListener<IInputChannel>(address.Uri, bindingParameters);
            }
            ChannelDispatcher channelDispatcher = new ChannelDispatcher(channelListener, binding.Namespace + ":" + binding.Name, binding);
            channelDispatcher.MessageVersion = binding.MessageVersion;

            EndpointDispatcher endpointDispatcher = new EndpointDispatcher(address, endpoint.Contract.Name, endpoint.Contract.Namespace, false);
            endpointDispatcher.DispatchRuntime.Type = serviceType;
            endpointDispatcher.AddressFilter = new EndpointAddressMessageFilter(endpoint.Address);
            endpointDispatcher.ContractFilter = new MatchAllMessageFilter();
            endpointDispatcher.FilterPriority = 0;
            endpointDispatcher.DispatchRuntime.OperationSelector = new BrokeredOperationSelector(actionMap);

            foreach (OperationDescription operation in endpoint.Contract.Operations)
            {
                DispatchOperation operationDispatcher = new DispatchOperation(endpointDispatcher.DispatchRuntime, operation.Name, string.Empty);
                operationDispatcher.Formatter = new BrokeredFormatter();

                Debug.Assert(operation.SyncMethod != null);
                if (operation.SyncMethod.ReturnType == typeof(void))
                {
                    operationDispatcher.Invoker = new SyncOperationInvoker(operation.SyncMethod);
                }
                else
                {
                    Debug.Assert(operation.SyncMethod.ReturnType == typeof(Task));
                    operationDispatcher.Invoker = new TaskMethodInvoker(operation.SyncMethod);
                }

                foreach (IOperationBehavior operationBehavior in operation.Behaviors)
                {
                    operationBehavior.ApplyDispatchBehavior(operation, operationDispatcher);
                }
                endpointDispatcher.DispatchRuntime.Operations.Add(operationDispatcher);
            }

            channelDispatcher.ReceiveContextEnabled = (receiveMode == ReceiveMode.PeekLock);
            SetErrorHandler(channelDispatcher, serviceType);

            channelDispatcher.Endpoints.Add(endpointDispatcher);
            return channelDispatcher;
        }

        void SetErrorHandler(ChannelDispatcher channelDispatcher, Type serviceType)
        {
            MethodInfo onErrorMethod = BrokeredServiceHost.FindConventionOrUniqueMatchingMethod(serviceType, "OnError", new HashSet<Type>() { typeof(void) }, typeof(Exception));
            if (onErrorMethod != null)
            {
                channelDispatcher.ErrorHandlers.Add(new InvokeMethodErrorHandler(onErrorMethod));
            }
        }

        class InvokeMethodErrorHandler : IErrorHandler
        {
            MethodInfo method;

            public InvokeMethodErrorHandler(MethodInfo method)
            {
                this.method = method;
            }

            public bool HandleError(Exception error)
            {
                try
                {
                    method.Invoke(OperationContext.Current.InstanceContext.GetServiceInstance(), new object[] { error });
                }
                catch (Exception e) { }
                return true;
            }

            public void ProvideFault(Exception error, MessageVersion ver, ref System.ServiceModel.Channels.Message fault) { }
        }

        class SyncOperationInvoker : IOperationInvoker
        {
            int inputLength;
            MethodInfo syncMethod;

            public SyncOperationInvoker(MethodInfo syncMethod)
            {
                this.syncMethod = syncMethod;
                this.inputLength = syncMethod.GetParameters().Length;
            }

            public object[] AllocateInputs()
            {
                return new object[inputLength];
            }

            public object Invoke(object instance, object[] inputs, out object[] outputs)
            {
                outputs = new object[0];
                return this.syncMethod.Invoke(instance, inputs);
            }

            public IAsyncResult InvokeBegin(object instance, object[] inputs, AsyncCallback callback, object state)
            {
                throw new NotImplementedException();
            }

            public object InvokeEnd(object instance, out object[] outputs, IAsyncResult result)
            {
                throw new NotImplementedException();
            }

            public bool IsSynchronous
            {
                get { return true; }
            }
        }

        class TaskMethodInvoker : IOperationInvoker
        {
            MethodInfo taskMethod;
            int inputParameterCount;

            public TaskMethodInvoker(MethodInfo taskMethod)
            {
                // only handles operation return types of void
                Debug.Assert(taskMethod != null && taskMethod.ReturnType == typeof(Task));
                this.taskMethod = taskMethod;
                this.inputParameterCount = taskMethod.GetParameters().Length;
            }

            public object[] AllocateInputs()
            {
                return new object[inputParameterCount];
            }

            public object Invoke(object instance, object[] inputs, out object[] outputs)
            {
                throw new NotImplementedException();
            }

            public IAsyncResult InvokeBegin(object instance, object[] inputs, AsyncCallback callback, object state)
            {
                IAsyncResult result;
                Task task = (Task)this.taskMethod.Invoke(instance, inputs);
                result = ConvertTaskToAsyncResult(task, callback, state);
                return result;
            }

            private static IAsyncResult ConvertTaskToAsyncResult(Task task, AsyncCallback callback, object state)
            {
                if (task.Status == TaskStatus.Created)
                {
                    throw new InvalidOperationException("Task not started");
                }

                var tcs = new TaskCompletionSource<object>(state);

                task.ContinueWith(
                    t =>
                    {
                        if (t.IsFaulted)
                        {
                            tcs.TrySetException(t.Exception.InnerExceptions);
                        }
                        else if (t.IsCanceled)
                        {
                            tcs.TrySetCanceled();
                        }
                        else
                        {
                            tcs.TrySetResult(null);
                        }

                        if (callback != null)
                        {
                            callback(tcs.Task);
                        }
                    },
                    TaskContinuationOptions.ExecuteSynchronously);

                return tcs.Task;
            }

            public object InvokeEnd(object instance, out object[] outputs, IAsyncResult result)
            {
                outputs = new object[0];
                Task task = result as Task;

                if (task.IsFaulted)
                {
                    Debug.Assert(task.Exception != null, "Task.IsFaulted guarantees non-null exception.");
                    throw task.Exception;
                }

                if (task.IsCanceled)
                {
                    throw new TaskCanceledException(task);
                }

                return null;
            }

            public bool IsSynchronous
            {
                get { return false; }
            }
        }
    }
}