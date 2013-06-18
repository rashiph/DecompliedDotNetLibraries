namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.ServiceModel.Channels;

    public interface IDispatchMessageFormatter
    {
        void DeserializeRequest(Message message, object[] parameters);
        Message SerializeReply(MessageVersion messageVersion, object[] parameters, object result);
    }
}

