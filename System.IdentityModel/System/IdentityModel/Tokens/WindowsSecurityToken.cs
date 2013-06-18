namespace System.IdentityModel.Tokens
{
    using System;
    using System.Collections.ObjectModel;
    using System.IdentityModel;
    using System.Security.Principal;

    public class WindowsSecurityToken : SecurityToken, IDisposable
    {
        private string authenticationType;
        private bool disposed;
        private DateTime effectiveTime;
        private DateTime expirationTime;
        private string id;
        private System.Security.Principal.WindowsIdentity windowsIdentity;

        protected WindowsSecurityToken()
        {
        }

        public WindowsSecurityToken(System.Security.Principal.WindowsIdentity windowsIdentity) : this(windowsIdentity, SecurityUniqueId.Create().Value)
        {
        }

        public WindowsSecurityToken(System.Security.Principal.WindowsIdentity windowsIdentity, string id) : this(windowsIdentity, id, null)
        {
        }

        public WindowsSecurityToken(System.Security.Principal.WindowsIdentity windowsIdentity, string id, string authenticationType)
        {
            DateTime utcNow = DateTime.UtcNow;
            this.Initialize(id, authenticationType, utcNow, DateTime.UtcNow.AddHours(10.0), windowsIdentity, true);
        }

        public virtual void Dispose()
        {
            if (!this.disposed)
            {
                this.disposed = true;
                if (this.windowsIdentity != null)
                {
                    this.windowsIdentity.Dispose();
                    this.windowsIdentity = null;
                }
            }
        }

        protected void Initialize(string id, DateTime effectiveTime, DateTime expirationTime, System.Security.Principal.WindowsIdentity windowsIdentity, bool clone)
        {
            this.Initialize(id, null, effectiveTime, expirationTime, windowsIdentity, clone);
        }

        protected void Initialize(string id, string authenticationType, DateTime effectiveTime, DateTime expirationTime, System.Security.Principal.WindowsIdentity windowsIdentity, bool clone)
        {
            if (windowsIdentity == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("windowsIdentity");
            }
            if (id == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("id");
            }
            this.id = id;
            this.authenticationType = authenticationType;
            this.effectiveTime = effectiveTime;
            this.expirationTime = expirationTime;
            this.windowsIdentity = clone ? System.IdentityModel.SecurityUtils.CloneWindowsIdentityIfNecessary(windowsIdentity, authenticationType) : windowsIdentity;
        }

        protected void ThrowIfDisposed()
        {
            if (this.disposed)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ObjectDisposedException(base.GetType().FullName));
            }
        }

        public string AuthenticationType
        {
            get
            {
                return this.authenticationType;
            }
        }

        public override string Id
        {
            get
            {
                return this.id;
            }
        }

        public override ReadOnlyCollection<SecurityKey> SecurityKeys
        {
            get
            {
                return EmptyReadOnlyCollection<SecurityKey>.Instance;
            }
        }

        public override DateTime ValidFrom
        {
            get
            {
                return this.effectiveTime;
            }
        }

        public override DateTime ValidTo
        {
            get
            {
                return this.expirationTime;
            }
        }

        public virtual System.Security.Principal.WindowsIdentity WindowsIdentity
        {
            get
            {
                this.ThrowIfDisposed();
                return this.windowsIdentity;
            }
        }
    }
}

