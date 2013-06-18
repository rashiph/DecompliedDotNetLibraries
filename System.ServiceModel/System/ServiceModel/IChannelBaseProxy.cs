namespace System.ServiceModel
{
    using System.ServiceModel.Channels;

    internal interface IChannelBaseProxy
    {
        ServiceChannel GetServiceChannel();
    }
}

