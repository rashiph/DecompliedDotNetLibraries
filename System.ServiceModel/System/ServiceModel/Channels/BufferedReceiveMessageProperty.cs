namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Dispatcher;

    internal sealed class BufferedReceiveMessageProperty
    {
        private static MessageBuffer dummyMessageBuffer = Message.CreateMessage(MessageVersion.Default, string.Empty).CreateBufferedCopy(1);
        private MessageBuffer messageBuffer;
        private const string PropertyName = "BufferedReceiveMessageProperty";

        internal BufferedReceiveMessageProperty(ref MessageRpc rpc)
        {
            this.RequestContext = new BufferedRequestContext(rpc.RequestContext);
            rpc.RequestContext = this.RequestContext;
            this.Notification = rpc.InvokeNotification;
        }

        public void RegisterForReplay(OperationContext operationContext)
        {
            this.messageBuffer = (MessageBuffer) operationContext.IncomingMessageProperties["_RequestMessageBuffer_"];
            operationContext.IncomingMessageProperties["_RequestMessageBuffer_"] = dummyMessageBuffer;
        }

        public void ReplayRequest()
        {
            Message requestMessage = this.messageBuffer.CreateMessage();
            requestMessage.Properties["_RequestMessageBuffer_"] = this.messageBuffer;
            this.RequestContext.ReInitialize(requestMessage);
        }

        public static bool TryGet(Message message, out BufferedReceiveMessageProperty property)
        {
            return TryGet(message.Properties, out property);
        }

        public static bool TryGet(MessageProperties properties, out BufferedReceiveMessageProperty property)
        {
            object obj2 = null;
            if (properties.TryGetValue("BufferedReceiveMessageProperty", out obj2))
            {
                property = obj2 as BufferedReceiveMessageProperty;
            }
            else
            {
                property = null;
            }
            return (property != null);
        }

        public static string Name
        {
            get
            {
                return "BufferedReceiveMessageProperty";
            }
        }

        internal IInvokeReceivedNotification Notification { get; private set; }

        public BufferedRequestContext RequestContext { get; private set; }

        public object UserState { get; set; }
    }
}

