namespace System.Security.Permissions
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [Serializable, AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Struct | AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple=true, Inherited=false), ComVisible(true)]
    public sealed class KeyContainerPermissionAttribute : CodeAccessSecurityAttribute
    {
        private KeyContainerPermissionFlags m_flags;
        private string m_keyContainerName;
        private int m_keySpec;
        private string m_keyStore;
        private string m_providerName;
        private int m_providerType;

        public KeyContainerPermissionAttribute(SecurityAction action) : base(action)
        {
            this.m_providerType = -1;
            this.m_keySpec = -1;
        }

        public override IPermission CreatePermission()
        {
            if (base.m_unrestricted)
            {
                return new KeyContainerPermission(PermissionState.Unrestricted);
            }
            if (KeyContainerPermissionAccessEntry.IsUnrestrictedEntry(this.m_keyStore, this.m_providerName, this.m_providerType, this.m_keyContainerName, this.m_keySpec))
            {
                return new KeyContainerPermission(this.m_flags);
            }
            KeyContainerPermission permission = new KeyContainerPermission(KeyContainerPermissionFlags.NoFlags);
            KeyContainerPermissionAccessEntry accessEntry = new KeyContainerPermissionAccessEntry(this.m_keyStore, this.m_providerName, this.m_providerType, this.m_keyContainerName, this.m_keySpec, this.m_flags);
            permission.AccessEntries.Add(accessEntry);
            return permission;
        }

        public KeyContainerPermissionFlags Flags
        {
            get
            {
                return this.m_flags;
            }
            set
            {
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
                this.m_keyContainerName = value;
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
                this.m_keyStore = value;
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
                this.m_providerName = value;
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
                this.m_providerType = value;
            }
        }
    }
}

