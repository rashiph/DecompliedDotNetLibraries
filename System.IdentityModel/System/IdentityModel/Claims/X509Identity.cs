namespace System.IdentityModel.Claims
{
    using System;
    using System.IdentityModel;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Principal;

    internal class X509Identity : GenericIdentity, IDisposable
    {
        private X509Certificate2 certificate;
        private bool disposable;
        private bool disposed;
        private string name;
        private const string Thumbprint = "; ";
        private X500DistinguishedName x500DistinguishedName;
        private const string X509 = "X509";

        public X509Identity(X500DistinguishedName x500DistinguishedName) : base("X509", "X509")
        {
            this.disposable = true;
            this.x500DistinguishedName = x500DistinguishedName;
        }

        public X509Identity(X509Certificate2 certificate) : this(certificate, true, true)
        {
        }

        internal X509Identity(X509Certificate2 certificate, bool clone, bool disposable) : base("X509", "X509")
        {
            this.disposable = true;
            this.certificate = clone ? new X509Certificate2(certificate) : certificate;
            this.disposable = clone || disposable;
        }

        public X509Identity Clone()
        {
            if (this.certificate == null)
            {
                return new X509Identity(this.x500DistinguishedName);
            }
            return new X509Identity(this.certificate);
        }

        public void Dispose()
        {
            if (this.disposable && !this.disposed)
            {
                this.disposed = true;
                if (this.certificate != null)
                {
                    this.certificate.Reset();
                }
            }
        }

        private string GetName()
        {
            if (this.x500DistinguishedName != null)
            {
                return this.x500DistinguishedName.Name;
            }
            string name = this.certificate.SubjectName.Name;
            if (!string.IsNullOrEmpty(name))
            {
                return name;
            }
            name = this.certificate.GetNameInfo(X509NameType.DnsName, false);
            if (!string.IsNullOrEmpty(name))
            {
                return name;
            }
            name = this.certificate.GetNameInfo(X509NameType.SimpleName, false);
            if (!string.IsNullOrEmpty(name))
            {
                return name;
            }
            name = this.certificate.GetNameInfo(X509NameType.EmailName, false);
            if (!string.IsNullOrEmpty(name))
            {
                return name;
            }
            name = this.certificate.GetNameInfo(X509NameType.UpnName, false);
            if (!string.IsNullOrEmpty(name))
            {
                return name;
            }
            return string.Empty;
        }

        private void ThrowIfDisposed()
        {
            if (this.disposed)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ObjectDisposedException(base.GetType().FullName));
            }
        }

        public override string Name
        {
            get
            {
                this.ThrowIfDisposed();
                if (this.name == null)
                {
                    this.name = this.GetName() + "; " + this.certificate.Thumbprint;
                }
                return this.name;
            }
        }
    }
}

