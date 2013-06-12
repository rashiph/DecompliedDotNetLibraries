namespace System.Net
{
    using System;

    internal enum ContextAttribute
    {
        Authority = 6,
        ClientSpecifiedSpn = 0x1b,
        ConnectionInfo = 90,
        DceInfo = 3,
        EndpointBindings = 0x1a,
        IssuerListInfoEx = 0x59,
        Lifespan = 2,
        LocalCertificate = 0x54,
        Names = 1,
        NegotiationInfo = 12,
        PackageInfo = 10,
        RemoteCertificate = 0x53,
        RootStore = 0x55,
        Sizes = 0,
        StreamSizes = 4,
        UniqueBindings = 0x19
    }
}

