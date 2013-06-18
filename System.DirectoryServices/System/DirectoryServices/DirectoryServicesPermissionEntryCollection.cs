namespace System.DirectoryServices
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Security.Permissions;

    [Serializable]
    public class DirectoryServicesPermissionEntryCollection : CollectionBase
    {
        private DirectoryServicesPermission owner;

        internal DirectoryServicesPermissionEntryCollection()
        {
        }

        internal DirectoryServicesPermissionEntryCollection(DirectoryServicesPermission owner, ResourcePermissionBaseEntry[] entries)
        {
            this.owner = owner;
            for (int i = 0; i < entries.Length; i++)
            {
                base.InnerList.Add(new DirectoryServicesPermissionEntry(entries[i]));
            }
        }

        public int Add(DirectoryServicesPermissionEntry value)
        {
            return base.List.Add(value);
        }

        public void AddRange(DirectoryServicesPermissionEntry[] value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            for (int i = 0; i < value.Length; i++)
            {
                this.Add(value[i]);
            }
        }

        public void AddRange(DirectoryServicesPermissionEntryCollection value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            int count = value.Count;
            for (int i = 0; i < count; i++)
            {
                this.Add(value[i]);
            }
        }

        public bool Contains(DirectoryServicesPermissionEntry value)
        {
            return base.List.Contains(value);
        }

        public void CopyTo(DirectoryServicesPermissionEntry[] array, int index)
        {
            base.List.CopyTo(array, index);
        }

        public int IndexOf(DirectoryServicesPermissionEntry value)
        {
            return base.List.IndexOf(value);
        }

        public void Insert(int index, DirectoryServicesPermissionEntry value)
        {
            base.List.Insert(index, value);
        }

        protected override void OnClear()
        {
            this.owner.Clear();
        }

        protected override void OnInsert(int index, object value)
        {
            this.owner.AddPermissionAccess((DirectoryServicesPermissionEntry) value);
        }

        protected override void OnRemove(int index, object value)
        {
            this.owner.RemovePermissionAccess((DirectoryServicesPermissionEntry) value);
        }

        protected override void OnSet(int index, object oldValue, object newValue)
        {
            this.owner.RemovePermissionAccess((DirectoryServicesPermissionEntry) oldValue);
            this.owner.AddPermissionAccess((DirectoryServicesPermissionEntry) newValue);
        }

        public void Remove(DirectoryServicesPermissionEntry value)
        {
            base.List.Remove(value);
        }

        public DirectoryServicesPermissionEntry this[int index]
        {
            get
            {
                return (DirectoryServicesPermissionEntry) base.List[index];
            }
            set
            {
                base.List[index] = value;
            }
        }
    }
}

