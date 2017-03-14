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

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class OnMessageAttribute : Attribute
    {
        internal static string WildcardAction = "*";
        string action = OnMessageAttribute.WildcardAction;

        public string Action
        {
            get
            {
                return action;
            }
            set
            {
                action = value;
            }
        }
    }
}
