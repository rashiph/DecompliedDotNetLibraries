namespace System.ServiceModel.Activation
{
    using System;
    using System.Runtime.CompilerServices;

    [TypeForwardedFrom("System.ServiceModel, Version=3.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")]
    public abstract class HostedTransportConfiguration
    {
        protected HostedTransportConfiguration()
        {
        }

        public abstract Uri[] GetBaseAddresses(string virtualPath);
    }
}

