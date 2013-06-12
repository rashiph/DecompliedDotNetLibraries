namespace System.Diagnostics
{
    using System;
    using System.Security.Permissions;

    [Serializable]
    public sealed class EventLogPermission : ResourcePermissionBase
    {
        private EventLogPermissionEntryCollection innerCollection;

        public EventLogPermission()
        {
            this.SetNames();
        }

        public EventLogPermission(PermissionState state) : base(state)
        {
            this.SetNames();
        }

        public EventLogPermission(EventLogPermissionEntry[] permissionAccessEntries)
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

        public EventLogPermission(EventLogPermissionAccess permissionAccess, string machineName)
        {
            this.SetNames();
            this.AddPermissionAccess(new EventLogPermissionEntry(permissionAccess, machineName));
        }

        internal void AddPermissionAccess(EventLogPermissionEntry entry)
        {
            base.AddPermissionAccess(entry.GetBaseEntry());
        }

        internal void Clear()
        {
            base.Clear();
        }

        internal void RemovePermissionAccess(EventLogPermissionEntry entry)
        {
            base.RemovePermissionAccess(entry.GetBaseEntry());
        }

        private void SetNames()
        {
            base.PermissionAccessType = typeof(EventLogPermissionAccess);
            base.TagNames = new string[] { "Machine" };
        }

        public EventLogPermissionEntryCollection PermissionEntries
        {
            get
            {
                if (this.innerCollection == null)
                {
                    this.innerCollection = new EventLogPermissionEntryCollection(this, base.GetPermissionEntries());
                }
                return this.innerCollection;
            }
        }
    }
}

