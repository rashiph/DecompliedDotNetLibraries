namespace System.Security.Permissions
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [Serializable, ComVisible(true), AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Struct | AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple=true, Inherited=false)]
    public sealed class FileDialogPermissionAttribute : CodeAccessSecurityAttribute
    {
        private FileDialogPermissionAccess m_access;

        public FileDialogPermissionAttribute(SecurityAction action) : base(action)
        {
        }

        public override IPermission CreatePermission()
        {
            if (base.m_unrestricted)
            {
                return new FileDialogPermission(PermissionState.Unrestricted);
            }
            return new FileDialogPermission(this.m_access);
        }

        public bool Open
        {
            get
            {
                return ((this.m_access & FileDialogPermissionAccess.Open) != FileDialogPermissionAccess.None);
            }
            set
            {
                this.m_access = value ? (this.m_access | FileDialogPermissionAccess.Open) : (this.m_access & ~FileDialogPermissionAccess.Open);
            }
        }

        public bool Save
        {
            get
            {
                return ((this.m_access & FileDialogPermissionAccess.Save) != FileDialogPermissionAccess.None);
            }
            set
            {
                this.m_access = value ? (this.m_access | FileDialogPermissionAccess.Save) : (this.m_access & ~FileDialogPermissionAccess.Save);
            }
        }
    }
}

