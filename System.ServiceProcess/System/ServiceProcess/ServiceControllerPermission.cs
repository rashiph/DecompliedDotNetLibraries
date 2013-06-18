namespace System.ServiceProcess
{
    using System;
    using System.Security.Permissions;

    [Serializable]
    public sealed class ServiceControllerPermission : ResourcePermissionBase
    {
        private ServiceControllerPermissionEntryCollection innerCollection;

        public ServiceControllerPermission()
        {
            this.SetNames();
        }

        public ServiceControllerPermission(PermissionState state) : base(state)
        {
            this.SetNames();
        }

        public ServiceControllerPermission(ServiceControllerPermissionEntry[] permissionAccessEntries)
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

        public ServiceControllerPermission(ServiceControllerPermissionAccess permissionAccess, string machineName, string serviceName)
        {
            this.SetNames();
            this.AddPermissionAccess(new ServiceControllerPermissionEntry(permissionAccess, machineName, serviceName));
        }

        internal void AddPermissionAccess(ServiceControllerPermissionEntry entry)
        {
            base.AddPermissionAccess(entry.GetBaseEntry());
        }

        internal void Clear()
        {
            base.Clear();
        }

        internal void RemovePermissionAccess(ServiceControllerPermissionEntry entry)
        {
            base.RemovePermissionAccess(entry.GetBaseEntry());
        }

        private void SetNames()
        {
            base.PermissionAccessType = typeof(ServiceControllerPermissionAccess);
            base.TagNames = new string[] { "Machine", "Service" };
        }

        public ServiceControllerPermissionEntryCollection PermissionEntries
        {
            get
            {
                if (this.innerCollection == null)
                {
                    this.innerCollection = new ServiceControllerPermissionEntryCollection(this, base.GetPermissionEntries());
                }
                return this.innerCollection;
            }
        }
    }
}

