namespace System.Security.Cryptography
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.AccessControl;

    [ComVisible(true)]
    public sealed class CspParameters
    {
        public string KeyContainerName;
        public int KeyNumber;
        private System.Security.AccessControl.CryptoKeySecurity m_cryptoKeySecurity;
        private int m_flags;
        private SecureString m_keyPassword;
        private IntPtr m_parentWindowHandle;
        public string ProviderName;
        public int ProviderType;

        public CspParameters() : this(Utils.DefaultRsaProviderType, null, null)
        {
        }

        public CspParameters(int dwTypeIn) : this(dwTypeIn, null, null)
        {
        }

        internal CspParameters(CspParameters parameters)
        {
            this.ProviderType = parameters.ProviderType;
            this.ProviderName = parameters.ProviderName;
            this.KeyContainerName = parameters.KeyContainerName;
            this.KeyNumber = parameters.KeyNumber;
            this.Flags = parameters.Flags;
            this.m_cryptoKeySecurity = parameters.m_cryptoKeySecurity;
            this.m_keyPassword = parameters.m_keyPassword;
            this.m_parentWindowHandle = parameters.m_parentWindowHandle;
        }

        public CspParameters(int dwTypeIn, string strProviderNameIn) : this(dwTypeIn, strProviderNameIn, null)
        {
        }

        public CspParameters(int dwTypeIn, string strProviderNameIn, string strContainerNameIn) : this(dwTypeIn, strProviderNameIn, strContainerNameIn, CspProviderFlags.NoFlags)
        {
        }

        internal CspParameters(int providerType, string providerName, string keyContainerName, CspProviderFlags flags)
        {
            this.ProviderType = providerType;
            this.ProviderName = providerName;
            this.KeyContainerName = keyContainerName;
            this.KeyNumber = -1;
            this.Flags = flags;
        }

        public CspParameters(int providerType, string providerName, string keyContainerName, System.Security.AccessControl.CryptoKeySecurity cryptoKeySecurity, IntPtr parentWindowHandle) : this(providerType, providerName, keyContainerName)
        {
            this.m_cryptoKeySecurity = cryptoKeySecurity;
            this.m_parentWindowHandle = parentWindowHandle;
        }

        public CspParameters(int providerType, string providerName, string keyContainerName, System.Security.AccessControl.CryptoKeySecurity cryptoKeySecurity, SecureString keyPassword) : this(providerType, providerName, keyContainerName)
        {
            this.m_cryptoKeySecurity = cryptoKeySecurity;
            this.m_keyPassword = keyPassword;
        }

        public System.Security.AccessControl.CryptoKeySecurity CryptoKeySecurity
        {
            get
            {
                return this.m_cryptoKeySecurity;
            }
            set
            {
                this.m_cryptoKeySecurity = value;
            }
        }

        public CspProviderFlags Flags
        {
            get
            {
                return (CspProviderFlags) this.m_flags;
            }
            set
            {
                int num = 0xff;
                int num2 = (int) value;
                if ((num2 & ~num) != 0)
                {
                    throw new ArgumentException(Environment.GetResourceString("Arg_EnumIllegalVal", new object[] { (int) value }), "value");
                }
                this.m_flags = num2;
            }
        }

        public SecureString KeyPassword
        {
            get
            {
                return this.m_keyPassword;
            }
            set
            {
                this.m_keyPassword = value;
                this.m_parentWindowHandle = IntPtr.Zero;
            }
        }

        public IntPtr ParentWindowHandle
        {
            get
            {
                return this.m_parentWindowHandle;
            }
            set
            {
                this.m_parentWindowHandle = value;
                this.m_keyPassword = null;
            }
        }
    }
}

