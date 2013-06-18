namespace System.DirectoryServices
{
    using System;
    using System.Security;
    using System.Security.Permissions;

    [Serializable, AttributeUsage(AttributeTargets.Event | AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Struct | AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple=true, Inherited=false)]
    public class DirectoryServicesPermissionAttribute : CodeAccessSecurityAttribute
    {
        private string path;
        private DirectoryServicesPermissionAccess permissionAccess;

        public DirectoryServicesPermissionAttribute(SecurityAction action) : base(action)
        {
            this.path = "*";
            this.permissionAccess = DirectoryServicesPermissionAccess.Browse;
        }

        public override IPermission CreatePermission()
        {
            if (base.Unrestricted)
            {
                return new DirectoryServicesPermission(PermissionState.Unrestricted);
            }
            DirectoryServicesPermissionAccess permissionAccess = this.permissionAccess;
            return new DirectoryServicesPermission(permissionAccess, this.Path);
        }

        public string Path
        {
            get
            {
                return this.path;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.path = value;
            }
        }

        public DirectoryServicesPermissionAccess PermissionAccess
        {
            get
            {
                return this.permissionAccess;
            }
            set
            {
                this.permissionAccess = value;
            }
        }
    }
}

