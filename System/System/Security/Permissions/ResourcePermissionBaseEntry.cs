namespace System.Security.Permissions
{
    using System;

    [Serializable]
    public class ResourcePermissionBaseEntry
    {
        private string[] accessPath;
        private int permissionAccess;

        public ResourcePermissionBaseEntry()
        {
            this.permissionAccess = 0;
            this.accessPath = new string[0];
        }

        public ResourcePermissionBaseEntry(int permissionAccess, string[] permissionAccessPath)
        {
            if (permissionAccessPath == null)
            {
                throw new ArgumentNullException("permissionAccessPath");
            }
            this.permissionAccess = permissionAccess;
            this.accessPath = permissionAccessPath;
        }

        public int PermissionAccess
        {
            get
            {
                return this.permissionAccess;
            }
        }

        public string[] PermissionAccessPath
        {
            get
            {
                return this.accessPath;
            }
        }
    }
}

