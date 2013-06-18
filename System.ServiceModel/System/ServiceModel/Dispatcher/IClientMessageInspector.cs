namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    public interface IClientMessageInspector
    {
        void AfterReceiveReply(ref Message reply, object correlationState);
        object BeforeSendRequest(ref Message request, IClientChannel channel);
    }
}

