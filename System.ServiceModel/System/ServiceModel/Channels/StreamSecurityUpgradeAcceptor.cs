namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel.Security;

    public abstract class StreamSecurityUpgradeAcceptor : StreamUpgradeAcceptor
    {
        protected StreamSecurityUpgradeAcceptor()
        {
        }

        public abstract SecurityMessageProperty GetRemoteSecurity();
    }
}

