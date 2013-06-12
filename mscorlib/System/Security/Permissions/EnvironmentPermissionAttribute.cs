namespace System.Security.Permissions
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [Serializable, AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Struct | AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple=true, Inherited=false), ComVisible(true)]
    public sealed class EnvironmentPermissionAttribute : CodeAccessSecurityAttribute
    {
        private string m_read;
        private string m_write;

        public EnvironmentPermissionAttribute(SecurityAction action) : base(action)
        {
        }

        [SecuritySafeCritical]
        public override IPermission CreatePermission()
        {
            if (base.m_unrestricted)
            {
                return new EnvironmentPermission(PermissionState.Unrestricted);
            }
            EnvironmentPermission permission = new EnvironmentPermission(PermissionState.None);
            if (this.m_read != null)
            {
                permission.SetPathList(EnvironmentPermissionAccess.Read, this.m_read);
            }
            if (this.m_write != null)
            {
                permission.SetPathList(EnvironmentPermissionAccess.Write, this.m_write);
            }
            return permission;
        }

        public string All
        {
            get
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_GetMethod"));
            }
            set
            {
                this.m_write = value;
                this.m_read = value;
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

