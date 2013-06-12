namespace System.ComponentModel
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [ComVisible(true), HostProtection(SecurityAction.LinkDemand, Synchronization=true)]
    public class EventDescriptorCollection : IList, ICollection, IEnumerable
    {
        private IComparer comparer;
        public static readonly EventDescriptorCollection Empty = new EventDescriptorCollection(null, true);
        private int eventCount;
        private EventDescriptor[] events;
        private bool eventsOwned;
        private string[] namedSort;
        private bool needSort;
        private bool readOnly;

        public EventDescriptorCollection(EventDescriptor[] events)
        {
            this.eventsOwned = true;
            this.events = events;
            if (events == null)
            {
                this.events = new EventDescriptor[0];
                this.eventCount = 0;
            }
            else
            {
                this.eventCount = this.events.Length;
            }
            this.eventsOwned = true;
        }

        public EventDescriptorCollection(EventDescriptor[] events, bool readOnly) : this(events)
        {
            this.readOnly = readOnly;
        }

        private EventDescriptorCollection(EventDescriptor[] events, int eventCount, string[] namedSort, IComparer comparer)
        {
            this.eventsOwned = true;
            this.eventsOwned = false;
            if (namedSort != null)
            {
                this.namedSort = (string[]) namedSort.Clone();
            }
            this.comparer = comparer;
            this.events = events;
            this.eventCount = eventCount;
            this.needSort = true;
        }

        public int Add(EventDescriptor value)
        {
            if (this.readOnly)
            {
                throw new NotSupportedException();
            }
            this.EnsureSize(this.eventCount + 1);
            this.events[this.eventCount++] = value;
            return (this.eventCount - 1);
        }

        public void Clear()
        {
            if (this.readOnly)
            {
                throw new NotSupportedException();
            }
            this.eventCount = 0;
        }

        public bool Contains(EventDescriptor value)
        {
            return (this.IndexOf(value) >= 0);
        }

        private void EnsureEventsOwned()
        {
            if (!this.eventsOwned)
            {
                this.eventsOwned = true;
                if (this.events != null)
                {
                    EventDescriptor[] destinationArray = new EventDescriptor[this.Count];
                    Array.Copy(this.events, 0, destinationArray, 0, this.Count);
                    this.events = destinationArray;
                }
            }
            if (this.needSort)
            {
                this.needSort = false;
                this.InternalSort(this.namedSort);
            }
        }

        private void EnsureSize(int sizeNeeded)
        {
            if (sizeNeeded > this.events.Length)
            {
                if ((this.events == null) || (this.events.Length == 0))
                {
                    this.eventCount = 0;
                    this.events = new EventDescriptor[sizeNeeded];
                }
                else
                {
                    this.EnsureEventsOwned();
                    EventDescriptor[] destinationArray = new EventDescriptor[Math.Max(sizeNeeded, this.events.Length * 2)];
                    Array.Copy(this.events, 0, destinationArray, 0, this.eventCount);
                    this.events = destinationArray;
                }
            }
        }

        public virtual EventDescriptor Find(string name, bool ignoreCase)
        {
            EventDescriptor descriptor = null;
            if (ignoreCase)
            {
                for (int j = 0; j < this.Count; j++)
                {
                    if (string.Equals(this.events[j].Name, name, StringComparison.OrdinalIgnoreCase))
                    {
                        return this.events[j];
                    }
                }
                return descriptor;
            }
            for (int i = 0; i < this.Count; i++)
            {
                if (string.Equals(this.events[i].Name, name, StringComparison.Ordinal))
                {
                    return this.events[i];
                }
            }
            return descriptor;
        }

        public IEnumerator GetEnumerator()
        {
            if (this.events.Length == this.eventCount)
            {
                return this.events.GetEnumerator();
            }
            return new ArraySubsetEnumerator(this.events, this.eventCount);
        }

        public int IndexOf(EventDescriptor value)
        {
            return Array.IndexOf<EventDescriptor>(this.events, value, 0, this.eventCount);
        }

        public void Insert(int index, EventDescriptor value)
        {
            if (this.readOnly)
            {
                throw new NotSupportedException();
            }
            this.EnsureSize(this.eventCount + 1);
            if (index < this.eventCount)
            {
                Array.Copy(this.events, index, this.events, index + 1, this.eventCount - index);
            }
            this.events[index] = value;
            this.eventCount++;
        }

        protected void InternalSort(string[] names)
        {
            if ((this.events != null) && (this.events.Length != 0))
            {
                this.InternalSort(this.comparer);
                if ((names != null) && (names.Length > 0))
                {
                    ArrayList list = new ArrayList(this.events);
                    int num = 0;
                    int length = this.events.Length;
                    for (int i = 0; i < names.Length; i++)
                    {
                        for (int k = 0; k < length; k++)
                        {
                            EventDescriptor descriptor = (EventDescriptor) list[k];
                            if ((descriptor != null) && descriptor.Name.Equals(names[i]))
                            {
                                this.events[num++] = descriptor;
                                list[k] = null;
                                break;
                            }
                        }
                    }
                    for (int j = 0; j < length; j++)
                    {
                        if (list[j] != null)
                        {
                            this.events[num++] = (EventDescriptor) list[j];
                        }
                    }
                }
            }
        }

        protected void InternalSort(IComparer sorter)
        {
            if (sorter == null)
            {
                TypeDescriptor.SortDescriptorArray(this);
            }
            else
            {
                Array.Sort(this.events, sorter);
            }
        }

        public void Remove(EventDescriptor value)
        {
            if (this.readOnly)
            {
                throw new NotSupportedException();
            }
            int index = this.IndexOf(value);
            if (index != -1)
            {
                this.RemoveAt(index);
            }
        }

        public void RemoveAt(int index)
        {
            if (this.readOnly)
            {
                throw new NotSupportedException();
            }
            if (index < (this.eventCount - 1))
            {
                Array.Copy(this.events, index + 1, this.events, index, (this.eventCount - index) - 1);
            }
            this.events[this.eventCount - 1] = null;
            this.eventCount--;
        }

        public virtual EventDescriptorCollection Sort()
        {
            return new EventDescriptorCollection(this.events, this.eventCount, this.namedSort, this.comparer);
        }

        public virtual EventDescriptorCollection Sort(string[] names)
        {
            return new EventDescriptorCollection(this.events, this.eventCount, names, this.comparer);
        }

        public virtual EventDescriptorCollection Sort(IComparer comparer)
        {
            return new EventDescriptorCollection(this.events, this.eventCount, this.namedSort, comparer);
        }

        public virtual EventDescriptorCollection Sort(string[] names, IComparer comparer)
        {
            return new EventDescriptorCollection(this.events, this.eventCount, names, comparer);
        }

        void ICollection.CopyTo(Array array, int index)
        {
            this.EnsureEventsOwned();
            Array.Copy(this.events, 0, array, index, this.Count);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        int IList.Add(object value)
        {
            return this.Add((EventDescriptor) value);
        }

        void IList.Clear()
        {
            this.Clear();
        }

        bool IList.Contains(object value)
        {
            return this.Contains((EventDescriptor) value);
        }

        int IList.IndexOf(object value)
        {
            return this.IndexOf((EventDescriptor) value);
        }

        void IList.Insert(int index, object value)
        {
            this.Insert(index, (EventDescriptor) value);
        }

        void IList.Remove(object value)
        {
            this.Remove((EventDescriptor) value);
        }

        void IList.RemoveAt(int index)
        {
            this.RemoveAt(index);
        }

        public int Count
        {
            get
            {
                return this.eventCount;
            }
        }

        public virtual EventDescriptor this[int index]
        {
            get
            {
                if (index >= this.eventCount)
                {
                    throw new IndexOutOfRangeException();
                }
                this.EnsureEventsOwned();
                return this.events[index];
            }
        }

        public virtual EventDescriptor this[string name]
        {
            get
            {
                return this.Find(name, false);
            }
        }

        int ICollection.Count
        {
            get
            {
                return this.Count;
            }
        }

        bool ICollection.IsSynchronized
        {
            get
            {
                return false;
            }
        }

        object ICollection.SyncRoot
        {
            get
            {
                return null;
            }
        }

        bool IList.IsFixedSize
        {
            get
            {
                return this.readOnly;
            }
        }

        bool IList.IsReadOnly
        {
            get
            {
                return this.readOnly;
            }
        }

        object IList.this[int index]
        {
            get
            {
                return this[index];
            }
            set
            {
                if (this.readOnly)
                {
                    throw new NotSupportedException();
                }
                if (index >= this.eventCount)
                {
                    throw new IndexOutOfRangeException();
                }
                this.EnsureEventsOwned();
                this.events[index] = (EventDescriptor) value;
            }
        }
    }
}

