namespace System.ComponentModel
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Reflection;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, Synchronization=true)]
    public class PropertyDescriptorCollection : IList, IDictionary, ICollection, IEnumerable
    {
        private IDictionary cachedFoundProperties;
        private bool cachedIgnoreCase;
        private IComparer comparer;
        public static readonly PropertyDescriptorCollection Empty = new PropertyDescriptorCollection(null, true);
        private string[] namedSort;
        private bool needSort;
        private int propCount;
        private PropertyDescriptor[] properties;
        private bool propsOwned;
        private bool readOnly;

        public PropertyDescriptorCollection(PropertyDescriptor[] properties)
        {
            this.propsOwned = true;
            this.properties = properties;
            if (properties == null)
            {
                this.properties = new PropertyDescriptor[0];
                this.propCount = 0;
            }
            else
            {
                this.propCount = properties.Length;
            }
            this.propsOwned = true;
        }

        public PropertyDescriptorCollection(PropertyDescriptor[] properties, bool readOnly) : this(properties)
        {
            this.readOnly = readOnly;
        }

        private PropertyDescriptorCollection(PropertyDescriptor[] properties, int propCount, string[] namedSort, IComparer comparer)
        {
            this.propsOwned = true;
            this.propsOwned = false;
            if (namedSort != null)
            {
                this.namedSort = (string[]) namedSort.Clone();
            }
            this.comparer = comparer;
            this.properties = properties;
            this.propCount = propCount;
            this.needSort = true;
        }

        public int Add(PropertyDescriptor value)
        {
            if (this.readOnly)
            {
                throw new NotSupportedException();
            }
            this.EnsureSize(this.propCount + 1);
            this.properties[this.propCount++] = value;
            return (this.propCount - 1);
        }

        public void Clear()
        {
            if (this.readOnly)
            {
                throw new NotSupportedException();
            }
            this.propCount = 0;
            this.cachedFoundProperties = null;
        }

        public bool Contains(PropertyDescriptor value)
        {
            return (this.IndexOf(value) >= 0);
        }

        public void CopyTo(Array array, int index)
        {
            this.EnsurePropsOwned();
            Array.Copy(this.properties, 0, array, index, this.Count);
        }

        private void EnsurePropsOwned()
        {
            if (!this.propsOwned)
            {
                this.propsOwned = true;
                if (this.properties != null)
                {
                    PropertyDescriptor[] destinationArray = new PropertyDescriptor[this.Count];
                    Array.Copy(this.properties, 0, destinationArray, 0, this.Count);
                    this.properties = destinationArray;
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
            if (sizeNeeded > this.properties.Length)
            {
                if ((this.properties == null) || (this.properties.Length == 0))
                {
                    this.propCount = 0;
                    this.properties = new PropertyDescriptor[sizeNeeded];
                }
                else
                {
                    this.EnsurePropsOwned();
                    PropertyDescriptor[] destinationArray = new PropertyDescriptor[Math.Max(sizeNeeded, this.properties.Length * 2)];
                    Array.Copy(this.properties, 0, destinationArray, 0, this.propCount);
                    this.properties = destinationArray;
                }
            }
        }

        public virtual PropertyDescriptor Find(string name, bool ignoreCase)
        {
            lock (this)
            {
                PropertyDescriptor descriptor = null;
                if ((this.cachedFoundProperties == null) || (this.cachedIgnoreCase != ignoreCase))
                {
                    this.cachedIgnoreCase = ignoreCase;
                    this.cachedFoundProperties = new HybridDictionary(ignoreCase);
                }
                object obj2 = this.cachedFoundProperties[name];
                if (obj2 != null)
                {
                    return (PropertyDescriptor) obj2;
                }
                for (int i = 0; i < this.propCount; i++)
                {
                    if (ignoreCase)
                    {
                        if (!string.Equals(this.properties[i].Name, name, StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }
                        this.cachedFoundProperties[name] = this.properties[i];
                        descriptor = this.properties[i];
                        break;
                    }
                    if (this.properties[i].Name.Equals(name))
                    {
                        this.cachedFoundProperties[name] = this.properties[i];
                        descriptor = this.properties[i];
                        break;
                    }
                }
                return descriptor;
            }
        }

        public virtual IEnumerator GetEnumerator()
        {
            this.EnsurePropsOwned();
            if (this.properties.Length != this.propCount)
            {
                PropertyDescriptor[] destinationArray = new PropertyDescriptor[this.propCount];
                Array.Copy(this.properties, 0, destinationArray, 0, this.propCount);
                return destinationArray.GetEnumerator();
            }
            return this.properties.GetEnumerator();
        }

        public int IndexOf(PropertyDescriptor value)
        {
            return Array.IndexOf<PropertyDescriptor>(this.properties, value, 0, this.propCount);
        }

        public void Insert(int index, PropertyDescriptor value)
        {
            if (this.readOnly)
            {
                throw new NotSupportedException();
            }
            this.EnsureSize(this.propCount + 1);
            if (index < this.propCount)
            {
                Array.Copy(this.properties, index, this.properties, index + 1, this.propCount - index);
            }
            this.properties[index] = value;
            this.propCount++;
        }

        protected void InternalSort(string[] names)
        {
            if ((this.properties != null) && (this.properties.Length != 0))
            {
                this.InternalSort(this.comparer);
                if ((names != null) && (names.Length > 0))
                {
                    ArrayList list = new ArrayList(this.properties);
                    int num = 0;
                    int length = this.properties.Length;
                    for (int i = 0; i < names.Length; i++)
                    {
                        for (int k = 0; k < length; k++)
                        {
                            PropertyDescriptor descriptor = (PropertyDescriptor) list[k];
                            if ((descriptor != null) && descriptor.Name.Equals(names[i]))
                            {
                                this.properties[num++] = descriptor;
                                list[k] = null;
                                break;
                            }
                        }
                    }
                    for (int j = 0; j < length; j++)
                    {
                        if (list[j] != null)
                        {
                            this.properties[num++] = (PropertyDescriptor) list[j];
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
                Array.Sort(this.properties, sorter);
            }
        }

        public void Remove(PropertyDescriptor value)
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
            if (index < (this.propCount - 1))
            {
                Array.Copy(this.properties, index + 1, this.properties, index, (this.propCount - index) - 1);
            }
            this.properties[this.propCount - 1] = null;
            this.propCount--;
        }

        public virtual PropertyDescriptorCollection Sort()
        {
            return new PropertyDescriptorCollection(this.properties, this.propCount, this.namedSort, this.comparer);
        }

        public virtual PropertyDescriptorCollection Sort(string[] names)
        {
            return new PropertyDescriptorCollection(this.properties, this.propCount, names, this.comparer);
        }

        public virtual PropertyDescriptorCollection Sort(IComparer comparer)
        {
            return new PropertyDescriptorCollection(this.properties, this.propCount, this.namedSort, comparer);
        }

        public virtual PropertyDescriptorCollection Sort(string[] names, IComparer comparer)
        {
            return new PropertyDescriptorCollection(this.properties, this.propCount, names, comparer);
        }

        void IDictionary.Add(object key, object value)
        {
            PropertyDescriptor descriptor = value as PropertyDescriptor;
            if (descriptor == null)
            {
                throw new ArgumentException("value");
            }
            this.Add(descriptor);
        }

        void IDictionary.Clear()
        {
            this.Clear();
        }

        bool IDictionary.Contains(object key)
        {
            return ((key is string) && (this[(string) key] != null));
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return new PropertyDescriptorEnumerator(this);
        }

        void IDictionary.Remove(object key)
        {
            if (key is string)
            {
                PropertyDescriptor descriptor = this[(string) key];
                if (descriptor != null)
                {
                    ((IList) this).Remove(descriptor);
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        int IList.Add(object value)
        {
            return this.Add((PropertyDescriptor) value);
        }

        void IList.Clear()
        {
            this.Clear();
        }

        bool IList.Contains(object value)
        {
            return this.Contains((PropertyDescriptor) value);
        }

        int IList.IndexOf(object value)
        {
            return this.IndexOf((PropertyDescriptor) value);
        }

        void IList.Insert(int index, object value)
        {
            this.Insert(index, (PropertyDescriptor) value);
        }

        void IList.Remove(object value)
        {
            this.Remove((PropertyDescriptor) value);
        }

        void IList.RemoveAt(int index)
        {
            this.RemoveAt(index);
        }

        public int Count
        {
            get
            {
                return this.propCount;
            }
        }

        public virtual PropertyDescriptor this[int index]
        {
            get
            {
                if (index >= this.propCount)
                {
                    throw new IndexOutOfRangeException();
                }
                this.EnsurePropsOwned();
                return this.properties[index];
            }
        }

        public virtual PropertyDescriptor this[string name]
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

        bool IDictionary.IsFixedSize
        {
            get
            {
                return this.readOnly;
            }
        }

        bool IDictionary.IsReadOnly
        {
            get
            {
                return this.readOnly;
            }
        }

        object IDictionary.this[object key]
        {
            get
            {
                if (key is string)
                {
                    return this[(string) key];
                }
                return null;
            }
            set
            {
                if (this.readOnly)
                {
                    throw new NotSupportedException();
                }
                if ((value != null) && !(value is PropertyDescriptor))
                {
                    throw new ArgumentException("value");
                }
                int index = -1;
                if (key is int)
                {
                    index = (int) key;
                    if ((index < 0) || (index >= this.propCount))
                    {
                        throw new IndexOutOfRangeException();
                    }
                }
                else
                {
                    if (!(key is string))
                    {
                        throw new ArgumentException("key");
                    }
                    for (int i = 0; i < this.propCount; i++)
                    {
                        if (this.properties[i].Name.Equals((string) key))
                        {
                            index = i;
                            break;
                        }
                    }
                }
                if (index == -1)
                {
                    this.Add((PropertyDescriptor) value);
                }
                else
                {
                    this.EnsurePropsOwned();
                    this.properties[index] = (PropertyDescriptor) value;
                    if ((this.cachedFoundProperties != null) && (key is string))
                    {
                        this.cachedFoundProperties[key] = value;
                    }
                }
            }
        }

        ICollection IDictionary.Keys
        {
            get
            {
                string[] strArray = new string[this.propCount];
                for (int i = 0; i < this.propCount; i++)
                {
                    strArray[i] = this.properties[i].Name;
                }
                return strArray;
            }
        }

        ICollection IDictionary.Values
        {
            get
            {
                if (this.properties.Length != this.propCount)
                {
                    PropertyDescriptor[] destinationArray = new PropertyDescriptor[this.propCount];
                    Array.Copy(this.properties, 0, destinationArray, 0, this.propCount);
                    return destinationArray;
                }
                return (ICollection) this.properties.Clone();
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
                if (index >= this.propCount)
                {
                    throw new IndexOutOfRangeException();
                }
                if ((value != null) && !(value is PropertyDescriptor))
                {
                    throw new ArgumentException("value");
                }
                this.EnsurePropsOwned();
                this.properties[index] = (PropertyDescriptor) value;
            }
        }

        private class PropertyDescriptorEnumerator : IDictionaryEnumerator, IEnumerator
        {
            private int index = -1;
            private PropertyDescriptorCollection owner;

            public PropertyDescriptorEnumerator(PropertyDescriptorCollection owner)
            {
                this.owner = owner;
            }

            public bool MoveNext()
            {
                if (this.index < (this.owner.Count - 1))
                {
                    this.index++;
                    return true;
                }
                return false;
            }

            public void Reset()
            {
                this.index = -1;
            }

            public object Current
            {
                get
                {
                    return this.Entry;
                }
            }

            public DictionaryEntry Entry
            {
                get
                {
                    PropertyDescriptor descriptor = this.owner[this.index];
                    return new DictionaryEntry(descriptor.Name, descriptor);
                }
            }

            public object Key
            {
                get
                {
                    return this.owner[this.index].Name;
                }
            }

            public object Value
            {
                get
                {
                    return this.owner[this.index].Name;
                }
            }
        }
    }
}

