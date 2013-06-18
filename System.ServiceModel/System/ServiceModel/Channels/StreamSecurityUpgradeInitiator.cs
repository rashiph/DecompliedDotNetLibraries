namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel.Security;

    public abstract class StreamSecurityUpgradeInitiator : StreamUpgradeInitiator
    {
        protected StreamSecurityUpgradeInitiator()
        {
        }

        public abstract SecurityMessageProperty GetRemoteSecurity();
        internal static SecurityMessageProperty GetRemoteSecurity(StreamUpgradeInitiator upgradeInitiator)
        {
            StreamSecurityUpgradeInitiator initiator = upgradeInitiator as StreamSecurityUpgradeInitiator;
            if (initiator != null)
            {
                return initiator.GetRemoteSecurity();
            }
            return null;
        }
    }
}

