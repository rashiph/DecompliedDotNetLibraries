namespace System.ComponentModel
{
    using System;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, SharedState=true)]
    public abstract class License : IDisposable
    {
        protected License()
        {
        }

        public abstract void Dispose();

        public abstract string LicenseKey { get; }
    }
}

