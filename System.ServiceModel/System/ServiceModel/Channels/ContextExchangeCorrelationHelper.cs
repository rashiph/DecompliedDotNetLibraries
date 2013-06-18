namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;

    internal static class ContextExchangeCorrelationHelper
    {
        public static string CorrelationName = "wsc-instanceId";

        public static void AddIncomingContextCorrelationData(Message message)
        {
            CorrelationDataMessageProperty.AddData(message, CorrelationName, () => GetContextCorrelationData(message));
        }

        public static void AddOutgoingCorrelationCallbackData(CorrelationCallbackMessageProperty callback, Message message, bool client)
        {
            Func<string> func = null;
            Func<string> func2 = null;
            if (client)
            {
                if (func == null)
                {
                    func = () => GetCallbackContextCorrelationData(message);
                }
                callback.AddData(CorrelationName, func);
            }
            else
            {
                if (func2 == null)
                {
                    func2 = () => GetContextCorrelationData(message);
                }
                callback.AddData(CorrelationName, func2);
            }
        }

        public static string GetCallbackContextCorrelationData(Message message)
        {
            CallbackContextMessageProperty property;
            string str = null;
            if (CallbackContextMessageProperty.TryGet(message, out property))
            {
                IDictionary<string, string> context = property.Context;
                if (context != null)
                {
                    context.TryGetValue("instanceId", out str);
                }
            }
            return (str ?? string.Empty);
        }

        public static string GetContextCorrelationData(Message message)
        {
            ContextMessageProperty contextMessageProperty = null;
            string str = null;
            if (ContextMessageProperty.TryGet(message, out contextMessageProperty))
            {
                contextMessageProperty.Context.TryGetValue("instanceId", out str);
            }
            return (str ?? string.Empty);
        }

        public static string GetContextCorrelationData(OperationContext operationContext)
        {
            ContextMessageProperty contextMessageProperty = null;
            string str = null;
            if (ContextMessageProperty.TryGet(operationContext.OutgoingMessageProperties, out contextMessageProperty))
            {
                contextMessageProperty.Context.TryGetValue("instanceId", out str);
            }
            return (str ?? string.Empty);
        }
    }
}

