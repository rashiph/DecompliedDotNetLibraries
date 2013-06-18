namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    public interface ICallContextInitializer
    {
        void AfterInvoke(object correlationState);
        object BeforeInvoke(InstanceContext instanceContext, IClientChannel channel, Message message);
    }
}

