namespace System.ServiceModel.Activities
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.ServiceModel.Description;

    public abstract class ReceiveContent
    {
        private static ReceiveContent defaultReceiveContent;

        internal ReceiveContent()
        {
        }

        internal abstract void CacheMetadata(ActivityMetadata metadata, Activity owner, string operationName);
        internal abstract void ConfigureInternalReceive(InternalReceiveMessage internalReceiveMessage, out FromRequest requestFormatter);
        internal abstract void ConfigureInternalReceiveReply(InternalReceiveMessage internalReceiveMessage, out FromReply responseFormatter);
        public static ReceiveMessageContent Create(OutArgument message)
        {
            return new ReceiveMessageContent(message);
        }

        public static ReceiveParametersContent Create(IDictionary<string, OutArgument> parameters)
        {
            return new ReceiveParametersContent(parameters);
        }

        public static ReceiveMessageContent Create(OutArgument message, Type declaredMessageType)
        {
            return new ReceiveMessageContent(message) { DeclaredMessageType = declaredMessageType };
        }

        internal abstract void InferMessageDescription(OperationDescription operation, object owner, MessageDirection direction);

        internal static ReceiveContent DefaultReceiveContent
        {
            get
            {
                if (defaultReceiveContent == null)
                {
                    defaultReceiveContent = new ReceiveMessageContent();
                }
                return defaultReceiveContent;
            }
        }
    }
}

