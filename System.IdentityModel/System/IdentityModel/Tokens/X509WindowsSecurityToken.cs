namespace System.IdentityModel.Tokens
{
    using System;
    using System.IdentityModel;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Principal;

    public class X509WindowsSecurityToken : X509SecurityToken
    {
        private string authenticationType;
        private bool disposed;
        private System.Security.Principal.WindowsIdentity windowsIdentity;

        public X509WindowsSecurityToken(X509Certificate2 certificate, System.Security.Principal.WindowsIdentity windowsIdentity) : this(certificate, windowsIdentity, null, true)
        {
        }

        public X509WindowsSecurityToken(X509Certificate2 certificate, System.Security.Principal.WindowsIdentity windowsIdentity, string id) : this(certificate, windowsIdentity, null, id, true)
        {
        }

        internal X509WindowsSecurityToken(X509Certificate2 certificate, System.Security.Principal.WindowsIdentity windowsIdentity, string authenticationType, bool clone) : this(certificate, windowsIdentity, authenticationType, SecurityUniqueId.Create().Value, clone)
        {
        }

        public X509WindowsSecurityToken(X509Certificate2 certificate, System.Security.Principal.WindowsIdentity windowsIdentity, string authenticationType, string id) : this(certificate, windowsIdentity, authenticationType, id, true)
        {
        }

        internal X509WindowsSecurityToken(X509Certificate2 certificate, System.Security.Principal.WindowsIdentity windowsIdentity, string authenticationType, string id, bool clone) : base(certificate, id, clone)
        {
            if (windowsIdentity == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("windowsIdentity");
            }
            this.authenticationType = authenticationType;
            this.windowsIdentity = clone ? System.IdentityModel.SecurityUtils.CloneWindowsIdentityIfNecessary(windowsIdentity, authenticationType) : windowsIdentity;
        }

        public override void Dispose()
        {
            try
            {
                if (!this.disposed)
                {
                    this.disposed = true;
                    this.windowsIdentity.Dispose();
                }
            }
            finally
            {
                base.Dispose();
            }
        }

        public string AuthenticationType
        {
            get
            {
                return this.authenticationType;
            }
        }

        public System.Security.Principal.WindowsIdentity WindowsIdentity
        {
            get
            {
                base.ThrowIfDisposed();
                return this.windowsIdentity;
            }
        }
    }
}

