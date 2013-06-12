namespace System.Diagnostics
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Security.Permissions;

    [Serializable]
    public class EventLogPermissionEntryCollection : CollectionBase
    {
        private EventLogPermission owner;

        internal EventLogPermissionEntryCollection(EventLogPermission owner, ResourcePermissionBaseEntry[] entries)
        {
            this.owner = owner;
            for (int i = 0; i < entries.Length; i++)
            {
                base.InnerList.Add(new EventLogPermissionEntry(entries[i]));
            }
        }

        public int Add(EventLogPermissionEntry value)
        {
            return base.List.Add(value);
        }

        public void AddRange(EventLogPermissionEntry[] value)
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

        public void AddRange(EventLogPermissionEntryCollection value)
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

        public bool Contains(EventLogPermissionEntry value)
        {
            return base.List.Contains(value);
        }

        public void CopyTo(EventLogPermissionEntry[] array, int index)
        {
            base.List.CopyTo(array, index);
        }

        public int IndexOf(EventLogPermissionEntry value)
        {
            return base.List.IndexOf(value);
        }

        public void Insert(int index, EventLogPermissionEntry value)
        {
            base.List.Insert(index, value);
        }

        protected override void OnClear()
        {
            this.owner.Clear();
        }

        protected override void OnInsert(int index, object value)
        {
            this.owner.AddPermissionAccess((EventLogPermissionEntry) value);
        }

        protected override void OnRemove(int index, object value)
        {
            this.owner.RemovePermissionAccess((EventLogPermissionEntry) value);
        }

        protected override void OnSet(int index, object oldValue, object newValue)
        {
            this.owner.RemovePermissionAccess((EventLogPermissionEntry) oldValue);
            this.owner.AddPermissionAccess((EventLogPermissionEntry) newValue);
        }

        public void Remove(EventLogPermissionEntry value)
        {
            base.List.Remove(value);
        }

        public EventLogPermissionEntry this[int index]
        {
            get
            {
                return (EventLogPermissionEntry) base.List[index];
            }
            set
            {
                base.List[index] = value;
            }
        }
    }
}

