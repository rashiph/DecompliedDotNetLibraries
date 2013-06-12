namespace System.Runtime.Remoting.Channels
{
    using System;

    internal class ChannelServicesData
    {
        internal bool fRegisterWellKnownChannels;
        internal long remoteCalls;
        internal CrossAppDomainChannel xadmessageSink;
        internal CrossContextChannel xctxmessageSink;
    }
}

