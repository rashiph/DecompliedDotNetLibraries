namespace System.ServiceModel.PeerResolvers
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    internal interface IPeerResolverClient : IPeerResolverContract, IClientChannel, IContextChannel, IChannel, ICommunicationObject, IExtensibleObject<IContextChannel>, IDisposable
    {
    }
}

