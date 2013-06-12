namespace System.Diagnostics
{
    using System;
    using System.Security.Permissions;

    [Serializable]
    public sealed class PerformanceCounterPermission : ResourcePermissionBase
    {
        private PerformanceCounterPermissionEntryCollection innerCollection;

        public PerformanceCounterPermission()
        {
            this.SetNames();
        }

        public PerformanceCounterPermission(PermissionState state) : base(state)
        {
            this.SetNames();
        }

        public PerformanceCounterPermission(PerformanceCounterPermissionEntry[] permissionAccessEntries)
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

        public PerformanceCounterPermission(PerformanceCounterPermissionAccess permissionAccess, string machineName, string categoryName)
        {
            this.SetNames();
            this.AddPermissionAccess(new PerformanceCounterPermissionEntry(permissionAccess, machineName, categoryName));
        }

        internal void AddPermissionAccess(PerformanceCounterPermissionEntry entry)
        {
            base.AddPermissionAccess(entry.GetBaseEntry());
        }

        internal void Clear()
        {
            base.Clear();
        }

        internal void RemovePermissionAccess(PerformanceCounterPermissionEntry entry)
        {
            base.RemovePermissionAccess(entry.GetBaseEntry());
        }

        private void SetNames()
        {
            base.PermissionAccessType = typeof(PerformanceCounterPermissionAccess);
            base.TagNames = new string[] { "Machine", "Category" };
        }

        public PerformanceCounterPermissionEntryCollection PermissionEntries
        {
            get
            {
                if (this.innerCollection == null)
                {
                    this.innerCollection = new PerformanceCounterPermissionEntryCollection(this, base.GetPermissionEntries());
                }
                return this.innerCollection;
            }
        }
    }
}

