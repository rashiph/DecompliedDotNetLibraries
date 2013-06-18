namespace System.DirectoryServices.Protocols
{
    using System;
    using System.DirectoryServices;
    using System.Net;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Permissions;

    public abstract class DirectoryConnection
    {
        internal X509CertificateCollection certificatesCollection;
        internal TimeSpan connectionTimeOut = new TimeSpan(0, 0, 30);
        internal NetworkCredential directoryCredential;
        internal DirectoryIdentifier directoryIdentifier;

        protected DirectoryConnection()
        {
            Utility.CheckOSVersion();
            this.certificatesCollection = new X509CertificateCollection();
        }

        internal NetworkCredential GetCredential()
        {
            return this.directoryCredential;
        }

        [DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
        public abstract DirectoryResponse SendRequest(DirectoryRequest request);

        public X509CertificateCollection ClientCertificates
        {
            get
            {
                return this.certificatesCollection;
            }
        }

        public virtual NetworkCredential Credential
        {
            [SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.UnmanagedCode), EnvironmentPermission(SecurityAction.Assert, Unrestricted=true), DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
            set
            {
                this.directoryCredential = (value != null) ? new NetworkCredential(value.UserName, value.Password, value.Domain) : null;
            }
        }

        public virtual DirectoryIdentifier Directory
        {
            get
            {
                return this.directoryIdentifier;
            }
        }

        public virtual TimeSpan Timeout
        {
            get
            {
                return this.connectionTimeOut;
            }
            set
            {
                if (value < TimeSpan.Zero)
                {
                    throw new ArgumentException(System.DirectoryServices.Protocols.Res.GetString("NoNegativeTime"), "value");
                }
                this.connectionTimeOut = value;
            }
        }
    }
}

