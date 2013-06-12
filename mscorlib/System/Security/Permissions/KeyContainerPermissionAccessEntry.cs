namespace System.Security.Permissions
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;

    [Serializable, ComVisible(true)]
    public sealed class KeyContainerPermissionAccessEntry
    {
        private KeyContainerPermissionFlags m_flags;
        private string m_keyContainerName;
        private int m_keySpec;
        private string m_keyStore;
        private string m_providerName;
        private int m_providerType;

        internal KeyContainerPermissionAccessEntry(KeyContainerPermissionAccessEntry accessEntry) : this(accessEntry.KeyStore, accessEntry.ProviderName, accessEntry.ProviderType, accessEntry.KeyContainerName, accessEntry.KeySpec, accessEntry.Flags)
        {
        }

        public KeyContainerPermissionAccessEntry(CspParameters parameters, KeyContainerPermissionFlags flags) : this(((parameters.Flags & CspProviderFlags.UseMachineKeyStore) == CspProviderFlags.UseMachineKeyStore) ? "Machine" : "User", parameters.ProviderName, parameters.ProviderType, parameters.KeyContainerName, parameters.KeyNumber, flags)
        {
        }

        public KeyContainerPermissionAccessEntry(string keyContainerName, KeyContainerPermissionFlags flags) : this(null, null, -1, keyContainerName, -1, flags)
        {
        }

        public KeyContainerPermissionAccessEntry(string keyStore, string providerName, int providerType, string keyContainerName, int keySpec, KeyContainerPermissionFlags flags)
        {
            this.m_providerName = (providerName == null) ? "*" : providerName;
            this.m_providerType = providerType;
            this.m_keyContainerName = (keyContainerName == null) ? "*" : keyContainerName;
            this.m_keySpec = keySpec;
            this.KeyStore = keyStore;
            this.Flags = flags;
        }

        public override bool Equals(object o)
        {
            KeyContainerPermissionAccessEntry entry = o as KeyContainerPermissionAccessEntry;
            if (entry == null)
            {
                return false;
            }
            if (entry.m_keyStore != this.m_keyStore)
            {
                return false;
            }
            if (entry.m_providerName != this.m_providerName)
            {
                return false;
            }
            if (entry.m_providerType != this.m_providerType)
            {
                return false;
            }
            if (entry.m_keyContainerName != this.m_keyContainerName)
            {
                return false;
            }
            if (entry.m_keySpec != this.m_keySpec)
            {
                return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            int num = 0;
            num |= (this.m_keyStore.GetHashCode() & 0xff) << 0x18;
            num |= (this.m_providerName.GetHashCode() & 0xff) << 0x10;
            num |= (this.m_providerType & 15) << 12;
            num |= (this.m_keyContainerName.GetHashCode() & 0xff) << 4;
            return (num | (this.m_keySpec & 15));
        }

        internal bool IsSubsetOf(KeyContainerPermissionAccessEntry target)
        {
            if ((target.m_keyStore != "*") && (this.m_keyStore != target.m_keyStore))
            {
                return false;
            }
            if ((target.m_providerName != "*") && (this.m_providerName != target.m_providerName))
            {
                return false;
            }
            if ((target.m_providerType != -1) && (this.m_providerType != target.m_providerType))
            {
                return false;
            }
            if ((target.m_keyContainerName != "*") && (this.m_keyContainerName != target.m_keyContainerName))
            {
                return false;
            }
            if ((target.m_keySpec != -1) && (this.m_keySpec != target.m_keySpec))
            {
                return false;
            }
            return true;
        }

        internal static bool IsUnrestrictedEntry(string keyStore, string providerName, int providerType, string keyContainerName, int keySpec)
        {
            if ((keyStore != "*") && (keyStore != null))
            {
                return false;
            }
            if ((providerName != "*") && (providerName != null))
            {
                return false;
            }
            if (providerType != -1)
            {
                return false;
            }
            if ((keyContainerName != "*") && (keyContainerName != null))
            {
                return false;
            }
            if (keySpec != -1)
            {
                return false;
            }
            return true;
        }

        public KeyContainerPermissionFlags Flags
        {
            get
            {
                return this.m_flags;
            }
            set
            {
                KeyContainerPermission.VerifyFlags(value);
                this.m_flags = value;
            }
        }

        public string KeyContainerName
        {
            get
            {
                return this.m_keyContainerName;
            }
            set
            {
                if (IsUnrestrictedEntry(this.KeyStore, this.ProviderName, this.ProviderType, value, this.KeySpec))
                {
                    throw new ArgumentException(Environment.GetResourceString("Arg_InvalidAccessEntry"));
                }
                if (value == null)
                {
                    this.m_keyContainerName = "*";
                }
                else
                {
                    this.m_keyContainerName = value;
                }
            }
        }

        public int KeySpec
        {
            get
            {
                return this.m_keySpec;
            }
            set
            {
                if (IsUnrestrictedEntry(this.KeyStore, this.ProviderName, this.ProviderType, this.KeyContainerName, value))
                {
                    throw new ArgumentException(Environment.GetResourceString("Arg_InvalidAccessEntry"));
                }
                this.m_keySpec = value;
            }
        }

        public string KeyStore
        {
            get
            {
                return this.m_keyStore;
            }
            set
            {
                if (IsUnrestrictedEntry(value, this.ProviderName, this.ProviderType, this.KeyContainerName, this.KeySpec))
                {
                    throw new ArgumentException(Environment.GetResourceString("Arg_InvalidAccessEntry"));
                }
                if (value == null)
                {
                    this.m_keyStore = "*";
                }
                else
                {
                    if (((value != "User") && (value != "Machine")) && (value != "*"))
                    {
                        throw new ArgumentException(Environment.GetResourceString("Argument_InvalidKeyStore", new object[] { value }), "value");
                    }
                    this.m_keyStore = value;
                }
            }
        }

        public string ProviderName
        {
            get
            {
                return this.m_providerName;
            }
            set
            {
                if (IsUnrestrictedEntry(this.KeyStore, value, this.ProviderType, this.KeyContainerName, this.KeySpec))
                {
                    throw new ArgumentException(Environment.GetResourceString("Arg_InvalidAccessEntry"));
                }
                if (value == null)
                {
                    this.m_providerName = "*";
                }
                else
                {
                    this.m_providerName = value;
                }
            }
        }

        public int ProviderType
        {
            get
            {
                return this.m_providerType;
            }
            set
            {
                if (IsUnrestrictedEntry(this.KeyStore, this.ProviderName, value, this.KeyContainerName, this.KeySpec))
                {
                    throw new ArgumentException(Environment.GetResourceString("Arg_InvalidAccessEntry"));
                }
                this.m_providerType = value;
            }
        }
    }
}

