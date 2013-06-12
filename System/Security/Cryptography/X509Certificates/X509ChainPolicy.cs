namespace System.Security.Cryptography.X509Certificates
{
    using System;
    using System.Globalization;
    using System.Security.Cryptography;

    public sealed class X509ChainPolicy
    {
        private OidCollection m_applicationPolicy;
        private OidCollection m_certificatePolicy;
        private X509Certificate2Collection m_extraStore;
        private X509RevocationFlag m_revocationFlag;
        private X509RevocationMode m_revocationMode;
        private TimeSpan m_timeout;
        private X509VerificationFlags m_verificationFlags;
        private DateTime m_verificationTime;

        public X509ChainPolicy()
        {
            this.Reset();
        }

        public void Reset()
        {
            this.m_applicationPolicy = new OidCollection();
            this.m_certificatePolicy = new OidCollection();
            this.m_revocationMode = X509RevocationMode.Online;
            this.m_revocationFlag = X509RevocationFlag.ExcludeRoot;
            this.m_verificationFlags = X509VerificationFlags.NoFlag;
            this.m_verificationTime = DateTime.Now;
            this.m_timeout = new TimeSpan(0, 0, 0);
            this.m_extraStore = new X509Certificate2Collection();
        }

        public OidCollection ApplicationPolicy
        {
            get
            {
                return this.m_applicationPolicy;
            }
        }

        public OidCollection CertificatePolicy
        {
            get
            {
                return this.m_certificatePolicy;
            }
        }

        public X509Certificate2Collection ExtraStore
        {
            get
            {
                return this.m_extraStore;
            }
        }

        public X509RevocationFlag RevocationFlag
        {
            get
            {
                return this.m_revocationFlag;
            }
            set
            {
                if ((value < X509RevocationFlag.EndCertificateOnly) || (value > X509RevocationFlag.ExcludeRoot))
                {
                    throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, SR.GetString("Arg_EnumIllegalVal"), new object[] { "value" }));
                }
                this.m_revocationFlag = value;
            }
        }

        public X509RevocationMode RevocationMode
        {
            get
            {
                return this.m_revocationMode;
            }
            set
            {
                if ((value < X509RevocationMode.NoCheck) || (value > X509RevocationMode.Offline))
                {
                    throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, SR.GetString("Arg_EnumIllegalVal"), new object[] { "value" }));
                }
                this.m_revocationMode = value;
            }
        }

        public TimeSpan UrlRetrievalTimeout
        {
            get
            {
                return this.m_timeout;
            }
            set
            {
                this.m_timeout = value;
            }
        }

        public X509VerificationFlags VerificationFlags
        {
            get
            {
                return this.m_verificationFlags;
            }
            set
            {
                if ((value < X509VerificationFlags.NoFlag) || (value > X509VerificationFlags.AllFlags))
                {
                    throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, SR.GetString("Arg_EnumIllegalVal"), new object[] { "value" }));
                }
                this.m_verificationFlags = value;
            }
        }

        public DateTime VerificationTime
        {
            get
            {
                return this.m_verificationTime;
            }
            set
            {
                this.m_verificationTime = value;
            }
        }
    }
}

