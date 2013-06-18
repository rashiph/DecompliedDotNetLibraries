namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.ServiceModel.Channels;

    public interface IClientMessageFormatter
    {
        object DeserializeReply(Message message, object[] parameters);
        Message SerializeRequest(MessageVersion messageVersion, object[] parameters);
    }
}

