namespace System.Security.Cryptography
{
    using System;
    using System.Security.Permissions;

    [Serializable, HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public sealed class CngProvider : IEquatable<CngProvider>
    {
        private string m_provider;
        private static CngProvider s_msSmartCardKsp;
        private static CngProvider s_msSoftwareKsp;

        public CngProvider(string provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }
            if (provider.Length == 0)
            {
                throw new ArgumentException(System.SR.GetString("Cryptography_InvalidProviderName", new object[] { provider }), "provider");
            }
            this.m_provider = provider;
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as CngProvider);
        }

        public bool Equals(CngProvider other)
        {
            if (object.ReferenceEquals(other, null))
            {
                return false;
            }
            return this.m_provider.Equals(other.Provider);
        }

        public override int GetHashCode()
        {
            return this.m_provider.GetHashCode();
        }

        public static bool operator ==(CngProvider left, CngProvider right)
        {
            if (object.ReferenceEquals(left, null))
            {
                return object.ReferenceEquals(right, null);
            }
            return left.Equals(right);
        }

        public static bool operator !=(CngProvider left, CngProvider right)
        {
            if (object.ReferenceEquals(left, null))
            {
                return !object.ReferenceEquals(right, null);
            }
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return this.m_provider.ToString();
        }

        public static CngProvider MicrosoftSmartCardKeyStorageProvider
        {
            get
            {
                if (s_msSmartCardKsp == null)
                {
                    s_msSmartCardKsp = new CngProvider("Microsoft Smart Card Key Storage Provider");
                }
                return s_msSmartCardKsp;
            }
        }

        public static CngProvider MicrosoftSoftwareKeyStorageProvider
        {
            get
            {
                if (s_msSoftwareKsp == null)
                {
                    s_msSoftwareKsp = new CngProvider("Microsoft Software Key Storage Provider");
                }
                return s_msSoftwareKsp;
            }
        }

        public string Provider
        {
            get
            {
                return this.m_provider;
            }
        }
    }
}

