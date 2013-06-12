namespace System.Security.Permissions
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.AccessControl;

    [Serializable, AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Struct | AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple=true, Inherited=false), ComVisible(true)]
    public sealed class RegistryPermissionAttribute : CodeAccessSecurityAttribute
    {
        private string m_changeAcl;
        private string m_create;
        private string m_read;
        private string m_viewAcl;
        private string m_write;

        public RegistryPermissionAttribute(SecurityAction action) : base(action)
        {
        }

        [SecuritySafeCritical]
        public override IPermission CreatePermission()
        {
            if (base.m_unrestricted)
            {
                return new RegistryPermission(PermissionState.Unrestricted);
            }
            RegistryPermission permission = new RegistryPermission(PermissionState.None);
            if (this.m_read != null)
            {
                permission.SetPathList(RegistryPermissionAccess.Read, this.m_read);
            }
            if (this.m_write != null)
            {
                permission.SetPathList(RegistryPermissionAccess.Write, this.m_write);
            }
            if (this.m_create != null)
            {
                permission.SetPathList(RegistryPermissionAccess.Create, this.m_create);
            }
            if (this.m_viewAcl != null)
            {
                permission.SetPathList(AccessControlActions.View, this.m_viewAcl);
            }
            if (this.m_changeAcl != null)
            {
                permission.SetPathList(AccessControlActions.Change, this.m_changeAcl);
            }
            return permission;
        }

        [Obsolete("Please use the ViewAndModify property instead.")]
        public string All
        {
            get
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_GetMethod"));
            }
            set
            {
                this.m_read = value;
                this.m_write = value;
                this.m_create = value;
            }
        }

        public string ChangeAccessControl
        {
            get
            {
                return this.m_changeAcl;
            }
            set
            {
                this.m_changeAcl = value;
            }
        }

        public string Create
        {
            get
            {
                return this.m_create;
            }
            set
            {
                this.m_create = value;
            }
        }

        public string Read
        {
            get
            {
                return this.m_read;
            }
            set
            {
                this.m_read = value;
            }
        }

        public string ViewAccessControl
        {
            get
            {
                return this.m_viewAcl;
            }
            set
            {
                this.m_viewAcl = value;
            }
        }

        public string ViewAndModify
        {
            get
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_GetMethod"));
            }
            set
            {
                this.m_read = value;
                this.m_write = value;
                this.m_create = value;
            }
        }

        public string Write
        {
            get
            {
                return this.m_write;
            }
            set
            {
                this.m_write = value;
            }
        }
    }
}

