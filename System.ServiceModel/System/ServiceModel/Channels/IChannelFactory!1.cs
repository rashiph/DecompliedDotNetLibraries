namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;

    public interface IChannelFactory<TChannel> : IChannelFactory, ICommunicationObject
    {
        TChannel CreateChannel(EndpointAddress to);
        TChannel CreateChannel(EndpointAddress to, Uri via);
    }
}

