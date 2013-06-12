namespace System.Security.Permissions
{
    using System;
    using System.Security;

    [Serializable, AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Struct | AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple=true, Inherited=false)]
    public sealed class StorePermissionAttribute : CodeAccessSecurityAttribute
    {
        private StorePermissionFlags m_flags;

        public StorePermissionAttribute(SecurityAction action) : base(action)
        {
        }

        public override IPermission CreatePermission()
        {
            if (base.Unrestricted)
            {
                return new StorePermission(PermissionState.Unrestricted);
            }
            return new StorePermission(this.m_flags);
        }

        public bool AddToStore
        {
            get
            {
                return ((this.m_flags & StorePermissionFlags.AddToStore) != StorePermissionFlags.NoFlags);
            }
            set
            {
                this.m_flags = value ? (this.m_flags | StorePermissionFlags.AddToStore) : (this.m_flags & ~StorePermissionFlags.AddToStore);
            }
        }

        public bool CreateStore
        {
            get
            {
                return ((this.m_flags & StorePermissionFlags.CreateStore) != StorePermissionFlags.NoFlags);
            }
            set
            {
                this.m_flags = value ? (this.m_flags | StorePermissionFlags.CreateStore) : (this.m_flags & ~StorePermissionFlags.CreateStore);
            }
        }

        public bool DeleteStore
        {
            get
            {
                return ((this.m_flags & StorePermissionFlags.DeleteStore) != StorePermissionFlags.NoFlags);
            }
            set
            {
                this.m_flags = value ? (this.m_flags | StorePermissionFlags.DeleteStore) : (this.m_flags & ~StorePermissionFlags.DeleteStore);
            }
        }

        public bool EnumerateCertificates
        {
            get
            {
                return ((this.m_flags & StorePermissionFlags.EnumerateCertificates) != StorePermissionFlags.NoFlags);
            }
            set
            {
                this.m_flags = value ? (this.m_flags | StorePermissionFlags.EnumerateCertificates) : (this.m_flags & ~StorePermissionFlags.EnumerateCertificates);
            }
        }

        public bool EnumerateStores
        {
            get
            {
                return ((this.m_flags & StorePermissionFlags.EnumerateStores) != StorePermissionFlags.NoFlags);
            }
            set
            {
                this.m_flags = value ? (this.m_flags | StorePermissionFlags.EnumerateStores) : (this.m_flags & ~StorePermissionFlags.EnumerateStores);
            }
        }

        public StorePermissionFlags Flags
        {
            get
            {
                return this.m_flags;
            }
            set
            {
                StorePermission.VerifyFlags(value);
                this.m_flags = value;
            }
        }

        public bool OpenStore
        {
            get
            {
                return ((this.m_flags & StorePermissionFlags.OpenStore) != StorePermissionFlags.NoFlags);
            }
            set
            {
                this.m_flags = value ? (this.m_flags | StorePermissionFlags.OpenStore) : (this.m_flags & ~StorePermissionFlags.OpenStore);
            }
        }

        public bool RemoveFromStore
        {
            get
            {
                return ((this.m_flags & StorePermissionFlags.RemoveFromStore) != StorePermissionFlags.NoFlags);
            }
            set
            {
                this.m_flags = value ? (this.m_flags | StorePermissionFlags.RemoveFromStore) : (this.m_flags & ~StorePermissionFlags.RemoveFromStore);
            }
        }
    }
}

