namespace System.ServiceModel.Channels
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Security;

    internal abstract class StreamSecurityUpgradeInitiatorBase : StreamSecurityUpgradeInitiator
    {
        private bool isOpen;
        private string nextUpgrade;
        private EndpointAddress remoteAddress;
        private SecurityMessageProperty remoteSecurity;
        private bool securityUpgraded;
        private Uri via;

        protected StreamSecurityUpgradeInitiatorBase(string upgradeString, EndpointAddress remoteAddress, Uri via)
        {
            this.remoteAddress = remoteAddress;
            this.via = via;
            this.nextUpgrade = upgradeString;
        }

        public override IAsyncResult BeginInitiateUpgrade(Stream stream, AsyncCallback callback, object state)
        {
            if (stream == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("stream");
            }
            if (!this.isOpen)
            {
                this.Open(TimeSpan.Zero);
            }
            return this.OnBeginInitiateUpgrade(stream, callback, state);
        }

        internal override void Close(TimeSpan timeout)
        {
            base.Close(timeout);
            this.isOpen = false;
        }

        internal override void EndClose(IAsyncResult result)
        {
            base.EndClose(result);
            this.isOpen = false;
        }

        public override Stream EndInitiateUpgrade(IAsyncResult result)
        {
            if (result == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("result");
            }
            Stream stream = this.OnEndInitiateUpgrade(result, out this.remoteSecurity);
            this.securityUpgraded = true;
            return stream;
        }

        internal override void EndOpen(IAsyncResult result)
        {
            base.EndOpen(result);
            this.isOpen = true;
        }

        public override string GetNextUpgrade()
        {
            string nextUpgrade = this.nextUpgrade;
            this.nextUpgrade = null;
            return nextUpgrade;
        }

        public override SecurityMessageProperty GetRemoteSecurity()
        {
            if (!this.securityUpgraded)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("OperationInvalidBeforeSecurityNegotiation")));
            }
            return this.remoteSecurity;
        }

        public override Stream InitiateUpgrade(Stream stream)
        {
            if (stream == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("stream");
            }
            if (!this.isOpen)
            {
                this.Open(TimeSpan.Zero);
            }
            Stream stream2 = this.OnInitiateUpgrade(stream, out this.remoteSecurity);
            this.securityUpgraded = true;
            return stream2;
        }

        protected abstract IAsyncResult OnBeginInitiateUpgrade(Stream stream, AsyncCallback callback, object state);
        protected abstract Stream OnEndInitiateUpgrade(IAsyncResult result, out SecurityMessageProperty remoteSecurity);
        protected abstract Stream OnInitiateUpgrade(Stream stream, out SecurityMessageProperty remoteSecurity);
        internal override void Open(TimeSpan timeout)
        {
            base.Open(timeout);
            this.isOpen = true;
        }

        protected EndpointAddress RemoteAddress
        {
            get
            {
                return this.remoteAddress;
            }
        }

        protected Uri Via
        {
            get
            {
                return this.via;
            }
        }
    }
}

