namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    public interface IDispatchMessageInspector
    {
        object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext);
        void BeforeSendReply(ref Message reply, object correlationState);
    }
}

