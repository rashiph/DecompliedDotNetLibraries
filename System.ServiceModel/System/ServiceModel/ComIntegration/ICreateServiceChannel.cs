namespace System.ServiceModel.ComIntegration
{
    using System.Runtime.Remoting.Proxies;

    internal interface ICreateServiceChannel
    {
        RealProxy CreateChannel();
    }
}

