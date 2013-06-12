namespace System.Security.Permissions
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Util;

    [Serializable, ComVisible(true), AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Struct | AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple=true, Inherited=false)]
    public sealed class PublisherIdentityPermissionAttribute : CodeAccessSecurityAttribute
    {
        private string m_certFile;
        private string m_signedFile;
        private string m_x509cert;

        public PublisherIdentityPermissionAttribute(SecurityAction action) : base(action)
        {
            this.m_x509cert = null;
            this.m_certFile = null;
            this.m_signedFile = null;
        }

        [SecuritySafeCritical]
        public override IPermission CreatePermission()
        {
            if (base.m_unrestricted)
            {
                return new PublisherIdentityPermission(PermissionState.Unrestricted);
            }
            if (this.m_x509cert != null)
            {
                return new PublisherIdentityPermission(new System.Security.Cryptography.X509Certificates.X509Certificate(Hex.DecodeHexString(this.m_x509cert)));
            }
            if (this.m_certFile != null)
            {
                return new PublisherIdentityPermission(System.Security.Cryptography.X509Certificates.X509Certificate.CreateFromCertFile(this.m_certFile));
            }
            if (this.m_signedFile != null)
            {
                return new PublisherIdentityPermission(System.Security.Cryptography.X509Certificates.X509Certificate.CreateFromSignedFile(this.m_signedFile));
            }
            return new PublisherIdentityPermission(PermissionState.None);
        }

        public string CertFile
        {
            get
            {
                return this.m_certFile;
            }
            set
            {
                this.m_certFile = value;
            }
        }

        public string SignedFile
        {
            get
            {
                return this.m_signedFile;
            }
            set
            {
                this.m_signedFile = value;
            }
        }

        public string X509Certificate
        {
            get
            {
                return this.m_x509cert;
            }
            set
            {
                this.m_x509cert = value;
            }
        }
    }
}

