namespace System.Diagnostics
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Security.Permissions;

    [Serializable]
    public class PerformanceCounterPermissionEntryCollection : CollectionBase
    {
        private PerformanceCounterPermission owner;

        internal PerformanceCounterPermissionEntryCollection(PerformanceCounterPermission owner, ResourcePermissionBaseEntry[] entries)
        {
            this.owner = owner;
            for (int i = 0; i < entries.Length; i++)
            {
                base.InnerList.Add(new PerformanceCounterPermissionEntry(entries[i]));
            }
        }

        public int Add(PerformanceCounterPermissionEntry value)
        {
            return base.List.Add(value);
        }

        public void AddRange(PerformanceCounterPermissionEntry[] value)
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

        public void AddRange(PerformanceCounterPermissionEntryCollection value)
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

        public bool Contains(PerformanceCounterPermissionEntry value)
        {
            return base.List.Contains(value);
        }

        public void CopyTo(PerformanceCounterPermissionEntry[] array, int index)
        {
            base.List.CopyTo(array, index);
        }

        public int IndexOf(PerformanceCounterPermissionEntry value)
        {
            return base.List.IndexOf(value);
        }

        public void Insert(int index, PerformanceCounterPermissionEntry value)
        {
            base.List.Insert(index, value);
        }

        protected override void OnClear()
        {
            this.owner.Clear();
        }

        protected override void OnInsert(int index, object value)
        {
            this.owner.AddPermissionAccess((PerformanceCounterPermissionEntry) value);
        }

        protected override void OnRemove(int index, object value)
        {
            this.owner.RemovePermissionAccess((PerformanceCounterPermissionEntry) value);
        }

        protected override void OnSet(int index, object oldValue, object newValue)
        {
            this.owner.RemovePermissionAccess((PerformanceCounterPermissionEntry) oldValue);
            this.owner.AddPermissionAccess((PerformanceCounterPermissionEntry) newValue);
        }

        public void Remove(PerformanceCounterPermissionEntry value)
        {
            base.List.Remove(value);
        }

        public PerformanceCounterPermissionEntry this[int index]
        {
            get
            {
                return (PerformanceCounterPermissionEntry) base.List[index];
            }
            set
            {
                base.List[index] = value;
            }
        }
    }
}

