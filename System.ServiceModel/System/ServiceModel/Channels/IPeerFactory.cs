namespace System.ServiceModel.Channels
{
    using System;
    using System.Net;
    using System.ServiceModel;
    using System.Xml;

    internal interface IPeerFactory : ITransportFactorySettings, IDefaultCommunicationTimeouts
    {
        IPAddress ListenIPAddress { get; }

        long MaxBufferPoolSize { get; }

        int Port { get; }

        PeerNodeImplementation PrivatePeerNode { get; set; }

        XmlDictionaryReaderQuotas ReaderQuotas { get; }

        PeerResolver Resolver { get; }

        PeerSecurityManager SecurityManager { get; }
    }
}

