namespace System.Security.Permissions
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Security.AccessControl;

    [Serializable, AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Struct | AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple=true, Inherited=false), ComVisible(true)]
    public sealed class FileIOPermissionAttribute : CodeAccessSecurityAttribute
    {
        [OptionalField(VersionAdded=2)]
        private FileIOPermissionAccess m_allFiles;
        [OptionalField(VersionAdded=2)]
        private FileIOPermissionAccess m_allLocalFiles;
        private string m_append;
        private string m_changeAccess;
        private string m_pathDiscovery;
        private string m_read;
        private string m_viewAccess;
        private string m_write;

        public FileIOPermissionAttribute(SecurityAction action) : base(action)
        {
        }

        public override IPermission CreatePermission()
        {
            if (base.m_unrestricted)
            {
                return new FileIOPermission(PermissionState.Unrestricted);
            }
            FileIOPermission permission = new FileIOPermission(PermissionState.None);
            if (this.m_read != null)
            {
                permission.SetPathList(FileIOPermissionAccess.Read, this.m_read);
            }
            if (this.m_write != null)
            {
                permission.SetPathList(FileIOPermissionAccess.Write, this.m_write);
            }
            if (this.m_append != null)
            {
                permission.SetPathList(FileIOPermissionAccess.Append, this.m_append);
            }
            if (this.m_pathDiscovery != null)
            {
                permission.SetPathList(FileIOPermissionAccess.PathDiscovery, this.m_pathDiscovery);
            }
            if (this.m_viewAccess != null)
            {
                permission.SetPathList(FileIOPermissionAccess.NoAccess, AccessControlActions.View, new string[] { this.m_viewAccess }, false);
            }
            if (this.m_changeAccess != null)
            {
                permission.SetPathList(FileIOPermissionAccess.NoAccess, AccessControlActions.Change, new string[] { this.m_changeAccess }, false);
            }
            permission.AllFiles = this.m_allFiles;
            permission.AllLocalFiles = this.m_allLocalFiles;
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
                this.m_append = value;
                this.m_pathDiscovery = value;
            }
        }

        public FileIOPermissionAccess AllFiles
        {
            get
            {
                return this.m_allFiles;
            }
            set
            {
                this.m_allFiles = value;
            }
        }

        public FileIOPermissionAccess AllLocalFiles
        {
            get
            {
                return this.m_allLocalFiles;
            }
            set
            {
                this.m_allLocalFiles = value;
            }
        }

        public string Append
        {
            get
            {
                return this.m_append;
            }
            set
            {
                this.m_append = value;
            }
        }

        public string ChangeAccessControl
        {
            get
            {
                return this.m_changeAccess;
            }
            set
            {
                this.m_changeAccess = value;
            }
        }

        public string PathDiscovery
        {
            get
            {
                return this.m_pathDiscovery;
            }
            set
            {
                this.m_pathDiscovery = value;
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
                return this.m_viewAccess;
            }
            set
            {
                this.m_viewAccess = value;
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
                this.m_append = value;
                this.m_pathDiscovery = value;
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

