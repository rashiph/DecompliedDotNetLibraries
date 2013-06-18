namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.ServiceModel;

    public interface IChannelInitializer
    {
        void Initialize(IClientChannel channel);
    }
}

