namespace System.Security.Permissions
{
    using System;
    using System.Security;

    [Serializable, AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Struct | AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple=true, Inherited=false), HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public sealed class DataProtectionPermissionAttribute : CodeAccessSecurityAttribute
    {
        private DataProtectionPermissionFlags m_flags;

        public DataProtectionPermissionAttribute(SecurityAction action) : base(action)
        {
        }

        public override IPermission CreatePermission()
        {
            if (base.Unrestricted)
            {
                return new DataProtectionPermission(PermissionState.Unrestricted);
            }
            return new DataProtectionPermission(this.m_flags);
        }

        public DataProtectionPermissionFlags Flags
        {
            get
            {
                return this.m_flags;
            }
            set
            {
                DataProtectionPermission.VerifyFlags(value);
                this.m_flags = value;
            }
        }

        public bool ProtectData
        {
            get
            {
                return ((this.m_flags & DataProtectionPermissionFlags.ProtectData) != DataProtectionPermissionFlags.NoFlags);
            }
            set
            {
                this.m_flags = value ? (this.m_flags | DataProtectionPermissionFlags.ProtectData) : (this.m_flags & ~DataProtectionPermissionFlags.ProtectData);
            }
        }

        public bool ProtectMemory
        {
            get
            {
                return ((this.m_flags & DataProtectionPermissionFlags.ProtectMemory) != DataProtectionPermissionFlags.NoFlags);
            }
            set
            {
                this.m_flags = value ? (this.m_flags | DataProtectionPermissionFlags.ProtectMemory) : (this.m_flags & ~DataProtectionPermissionFlags.ProtectMemory);
            }
        }

        public bool UnprotectData
        {
            get
            {
                return ((this.m_flags & DataProtectionPermissionFlags.UnprotectData) != DataProtectionPermissionFlags.NoFlags);
            }
            set
            {
                this.m_flags = value ? (this.m_flags | DataProtectionPermissionFlags.UnprotectData) : (this.m_flags & ~DataProtectionPermissionFlags.UnprotectData);
            }
        }

        public bool UnprotectMemory
        {
            get
            {
                return ((this.m_flags & DataProtectionPermissionFlags.UnprotectMemory) != DataProtectionPermissionFlags.NoFlags);
            }
            set
            {
                this.m_flags = value ? (this.m_flags | DataProtectionPermissionFlags.UnprotectMemory) : (this.m_flags & ~DataProtectionPermissionFlags.UnprotectMemory);
            }
        }
    }
}

