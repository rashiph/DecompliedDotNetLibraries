namespace System.ServiceModel.Channels
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Security;

    internal abstract class StreamSecurityUpgradeAcceptorBase : StreamSecurityUpgradeAcceptor
    {
        private SecurityMessageProperty remoteSecurity;
        private bool securityUpgraded;
        private string upgradeString;

        protected StreamSecurityUpgradeAcceptorBase(string upgradeString)
        {
            this.upgradeString = upgradeString;
        }

        public override Stream AcceptUpgrade(Stream stream)
        {
            if (stream == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("stream");
            }
            Stream stream2 = this.OnAcceptUpgrade(stream, out this.remoteSecurity);
            this.securityUpgraded = true;
            return stream2;
        }

        public override IAsyncResult BeginAcceptUpgrade(Stream stream, AsyncCallback callback, object state)
        {
            if (stream == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("stream");
            }
            return this.OnBeginAcceptUpgrade(stream, callback, state);
        }

        public override bool CanUpgrade(string contentType)
        {
            if (this.securityUpgraded)
            {
                return false;
            }
            return (contentType == this.upgradeString);
        }

        public override Stream EndAcceptUpgrade(IAsyncResult result)
        {
            if (result == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("result");
            }
            Stream stream = this.OnEndAcceptUpgrade(result, out this.remoteSecurity);
            this.securityUpgraded = true;
            return stream;
        }

        public override SecurityMessageProperty GetRemoteSecurity()
        {
            return this.remoteSecurity;
        }

        protected abstract Stream OnAcceptUpgrade(Stream stream, out SecurityMessageProperty remoteSecurity);
        protected abstract IAsyncResult OnBeginAcceptUpgrade(Stream stream, AsyncCallback callback, object state);
        protected abstract Stream OnEndAcceptUpgrade(IAsyncResult result, out SecurityMessageProperty remoteSecurity);
    }
}

