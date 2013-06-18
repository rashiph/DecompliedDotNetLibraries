namespace System.ServiceModel.Activation
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Security;
    using System.ServiceModel;

    internal sealed class HostingMessageProperty : IAspNetMessageProperty
    {
        [SecurityCritical]
        private HostedThreadData currentThreadData;
        [SecurityCritical]
        private HostedImpersonationContext impersonationContext;
        private const string name = "webhost";

        [SecurityCritical]
        internal HostingMessageProperty(HostedHttpRequestAsyncResult result)
        {
            if (ServiceHostingEnvironment.AspNetCompatibilityEnabled)
            {
                if ((result.ImpersonationContext != null) && result.ImpersonationContext.IsImpersonated)
                {
                    this.impersonationContext = result.ImpersonationContext;
                    this.impersonationContext.AddRef();
                }
                this.currentThreadData = result.HostedThreadData;
            }
            this.OriginalRequestUri = result.OriginalRequestUri;
        }

        [SecurityCritical]
        public IDisposable ApplyIntegrationContext()
        {
            if (ServiceHostingEnvironment.AspNetCompatibilityEnabled)
            {
                return this.currentThreadData.CreateContext();
            }
            return null;
        }

        [SecuritySafeCritical]
        public void Close()
        {
            if (this.impersonationContext != null)
            {
                this.impersonationContext.Release();
                this.impersonationContext = null;
            }
        }

        [SecurityCritical]
        public IDisposable Impersonate()
        {
            if (this.ImpersonationContext != null)
            {
                return this.ImpersonationContext.Impersonate();
            }
            return null;
        }

        private HostedImpersonationContext ImpersonationContext
        {
            [SecuritySafeCritical]
            get
            {
                return this.impersonationContext;
            }
        }

        internal static string Name
        {
            get
            {
                return "webhost";
            }
        }

        public Uri OriginalRequestUri { get; private set; }
    }
}

