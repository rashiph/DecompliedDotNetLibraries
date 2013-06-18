namespace System.DirectoryServices
{
    using System;
    using System.Security.Permissions;

    [Serializable]
    public class DirectoryServicesPermissionEntry
    {
        private string path;
        private DirectoryServicesPermissionAccess permissionAccess;

        internal DirectoryServicesPermissionEntry(ResourcePermissionBaseEntry baseEntry)
        {
            this.permissionAccess = (DirectoryServicesPermissionAccess) baseEntry.PermissionAccess;
            this.path = baseEntry.PermissionAccessPath[0];
        }

        public DirectoryServicesPermissionEntry(DirectoryServicesPermissionAccess permissionAccess, string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            this.permissionAccess = permissionAccess;
            this.path = path;
        }

        internal ResourcePermissionBaseEntry GetBaseEntry()
        {
            return new ResourcePermissionBaseEntry((int) this.PermissionAccess, new string[] { this.Path });
        }

        public string Path
        {
            get
            {
                return this.path;
            }
        }

        public DirectoryServicesPermissionAccess PermissionAccess
        {
            get
            {
                return this.permissionAccess;
            }
        }
    }
}

