namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    public interface IInstanceContextInitializer
    {
        void Initialize(InstanceContext instanceContext, Message message);
    }
}

