namespace System.DirectoryServices
{
    using System;
    using System.Security.Permissions;

    [Serializable]
    public sealed class DirectoryServicesPermission : ResourcePermissionBase
    {
        private DirectoryServicesPermissionEntryCollection innerCollection;

        public DirectoryServicesPermission()
        {
            this.SetNames();
        }

        public DirectoryServicesPermission(PermissionState state) : base(state)
        {
            this.SetNames();
        }

        public DirectoryServicesPermission(DirectoryServicesPermissionEntry[] permissionAccessEntries)
        {
            if (permissionAccessEntries == null)
            {
                throw new ArgumentNullException("permissionAccessEntries");
            }
            this.SetNames();
            for (int i = 0; i < permissionAccessEntries.Length; i++)
            {
                this.AddPermissionAccess(permissionAccessEntries[i]);
            }
        }

        public DirectoryServicesPermission(DirectoryServicesPermissionAccess permissionAccess, string path)
        {
            this.SetNames();
            this.AddPermissionAccess(new DirectoryServicesPermissionEntry(permissionAccess, path));
        }

        internal void AddPermissionAccess(DirectoryServicesPermissionEntry entry)
        {
            base.AddPermissionAccess(entry.GetBaseEntry());
        }

        internal void Clear()
        {
            base.Clear();
        }

        internal void RemovePermissionAccess(DirectoryServicesPermissionEntry entry)
        {
            base.RemovePermissionAccess(entry.GetBaseEntry());
        }

        private void SetNames()
        {
            base.PermissionAccessType = typeof(DirectoryServicesPermissionAccess);
            base.TagNames = new string[] { "Path" };
        }

        public DirectoryServicesPermissionEntryCollection PermissionEntries
        {
            get
            {
                if (this.innerCollection == null)
                {
                    this.innerCollection = new DirectoryServicesPermissionEntryCollection(this, base.GetPermissionEntries());
                }
                return this.innerCollection;
            }
        }
    }
}

