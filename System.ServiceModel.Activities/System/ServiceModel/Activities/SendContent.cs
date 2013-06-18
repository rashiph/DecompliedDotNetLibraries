namespace System.ServiceModel.Activities
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.ServiceModel.Description;

    public abstract class SendContent
    {
        private static SendContent defaultSendContent;

        internal SendContent()
        {
        }

        internal abstract void CacheMetadata(ActivityMetadata metadata, Activity owner, string operationName);
        internal abstract void ConfigureInternalSend(InternalSendMessage internalSendMessage, out ToRequest requestFormatter);
        internal abstract void ConfigureInternalSendReply(InternalSendMessage internalSendMessage, out ToReply responseFormatter);
        public static SendMessageContent Create(InArgument message)
        {
            return new SendMessageContent(message);
        }

        public static SendParametersContent Create(IDictionary<string, InArgument> parameters)
        {
            return new SendParametersContent(parameters);
        }

        public static SendMessageContent Create(InArgument message, Type declaredMessageType)
        {
            return new SendMessageContent(message) { DeclaredMessageType = declaredMessageType };
        }

        internal abstract void InferMessageDescription(OperationDescription operation, object owner, MessageDirection direction);

        internal static SendContent DefaultSendContent
        {
            get
            {
                if (defaultSendContent == null)
                {
                    defaultSendContent = new SendMessageContent();
                }
                return defaultSendContent;
            }
        }

        internal abstract bool IsFault { get; }
    }
}

