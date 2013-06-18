namespace System.DirectoryServices
{
    using System;
    using System.Collections;
    using System.DirectoryServices.Interop;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
    public class PropertyCollection : IDictionary, ICollection, IEnumerable
    {
        private DirectoryEntry entry;
        internal Hashtable valueTable;

        internal PropertyCollection(DirectoryEntry entry)
        {
            this.entry = entry;
            Hashtable table = new Hashtable();
            this.valueTable = Hashtable.Synchronized(table);
        }

        public bool Contains(string propertyName)
        {
            object obj2;
            int ex = this.entry.AdsObject.GetEx(propertyName, out obj2);
            if (ex == 0)
            {
                return true;
            }
            if ((ex != -2147463155) && (ex != -2147463162))
            {
                throw COMExceptionHelper.CreateFormattedComException(ex);
            }
            return false;
        }

        public void CopyTo(PropertyValueCollection[] array, int index)
        {
            ((ICollection) this).CopyTo(array, index);
        }

        public IDictionaryEnumerator GetEnumerator()
        {
            if (!(this.entry.AdsObject is System.DirectoryServices.Interop.UnsafeNativeMethods.IAdsPropertyList))
            {
                throw new NotSupportedException(Res.GetString("DSCannotEmunerate"));
            }
            DirectoryEntry clone = this.entry.CloneBrowsable();
            clone.FillCache("");
            System.DirectoryServices.Interop.UnsafeNativeMethods.IAdsPropertyList adsObject = (System.DirectoryServices.Interop.UnsafeNativeMethods.IAdsPropertyList) clone.AdsObject;
            clone.propertiesAlreadyEnumerated = true;
            return new PropertyEnumerator(this.entry, clone);
        }

        void ICollection.CopyTo(Array array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            if (array.Rank != 1)
            {
                throw new ArgumentException(Res.GetString("OnlyAllowSingleDimension"), "array");
            }
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(Res.GetString("LessThanZero"), "index");
            }
            if (((index + this.Count) > array.Length) || ((index + this.Count) < index))
            {
                throw new ArgumentException(Res.GetString("DestinationArrayNotLargeEnough"));
            }
            foreach (PropertyValueCollection values in this)
            {
                array.SetValue(values, index);
                index++;
            }
        }

        void IDictionary.Add(object key, object value)
        {
            throw new NotSupportedException(Res.GetString("DSAddNotSupported"));
        }

        void IDictionary.Clear()
        {
            throw new NotSupportedException(Res.GetString("DSClearNotSupported"));
        }

        bool IDictionary.Contains(object value)
        {
            return this.Contains((string) value);
        }

        void IDictionary.Remove(object key)
        {
            throw new NotSupportedException(Res.GetString("DSRemoveNotSupported"));
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public int Count
        {
            get
            {
                if (!(this.entry.AdsObject is System.DirectoryServices.Interop.UnsafeNativeMethods.IAdsPropertyList))
                {
                    throw new NotSupportedException(Res.GetString("DSCannotCount"));
                }
                this.entry.FillCache("");
                System.DirectoryServices.Interop.UnsafeNativeMethods.IAdsPropertyList adsObject = (System.DirectoryServices.Interop.UnsafeNativeMethods.IAdsPropertyList) this.entry.AdsObject;
                return adsObject.PropertyCount;
            }
        }

        public PropertyValueCollection this[string propertyName]
        {
            get
            {
                if (propertyName == null)
                {
                    throw new ArgumentNullException("propertyName");
                }
                string key = propertyName.ToLower(CultureInfo.InvariantCulture);
                if (this.valueTable.Contains(key))
                {
                    return (PropertyValueCollection) this.valueTable[key];
                }
                PropertyValueCollection values = new PropertyValueCollection(this.entry, propertyName);
                this.valueTable.Add(key, values);
                return values;
            }
        }

        public ICollection PropertyNames
        {
            get
            {
                return new KeysCollection(this);
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
                return this;
            }
        }

        bool IDictionary.IsFixedSize
        {
            get
            {
                return true;
            }
        }

        bool IDictionary.IsReadOnly
        {
            get
            {
                return true;
            }
        }

        object IDictionary.this[object key]
        {
            get
            {
                return this[(string) key];
            }
            set
            {
                throw new NotSupportedException(Res.GetString("DSPropertySetSupported"));
            }
        }

        ICollection IDictionary.Keys
        {
            get
            {
                return new KeysCollection(this);
            }
        }

        public ICollection Values
        {
            get
            {
                return new ValuesCollection(this);
            }
        }

        private class KeysCollection : PropertyCollection.ValuesCollection
        {
            public KeysCollection(PropertyCollection props) : base(props)
            {
            }

            public override IEnumerator GetEnumerator()
            {
                base.props.entry.FillCache("");
                return new PropertyCollection.KeysEnumerator(base.props);
            }
        }

        private class KeysEnumerator : PropertyCollection.ValuesEnumerator
        {
            public KeysEnumerator(PropertyCollection collection) : base(collection)
            {
            }

            public override object Current
            {
                get
                {
                    System.DirectoryServices.Interop.UnsafeNativeMethods.IAdsPropertyList adsObject = (System.DirectoryServices.Interop.UnsafeNativeMethods.IAdsPropertyList) base.propCollection.entry.AdsObject;
                    return ((System.DirectoryServices.Interop.UnsafeNativeMethods.IAdsPropertyEntry) adsObject.Item(base.CurrentIndex)).Name;
                }
            }
        }

        private class PropertyEnumerator : IDictionaryEnumerator, IEnumerator, IDisposable
        {
            private string currentPropName;
            private DirectoryEntry entry;
            private DirectoryEntry parentEntry;

            public PropertyEnumerator(DirectoryEntry parent, DirectoryEntry clone)
            {
                this.entry = clone;
                this.parentEntry = parent;
            }

            public void Dispose()
            {
                this.Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (disposing)
                {
                    this.entry.Dispose();
                }
            }

            ~PropertyEnumerator()
            {
                this.Dispose(true);
            }

            public bool MoveNext()
            {
                object obj2;
                int errorCode = 0;
                try
                {
                    errorCode = ((System.DirectoryServices.Interop.UnsafeNativeMethods.IAdsPropertyList) this.entry.AdsObject).Next(out obj2);
                }
                catch (COMException exception)
                {
                    errorCode = exception.ErrorCode;
                    obj2 = null;
                }
                if (errorCode == 0)
                {
                    if (obj2 != null)
                    {
                        this.currentPropName = ((System.DirectoryServices.Interop.UnsafeNativeMethods.IAdsPropertyEntry) obj2).Name;
                    }
                    else
                    {
                        this.currentPropName = null;
                    }
                    return true;
                }
                this.currentPropName = null;
                return false;
            }

            public void Reset()
            {
                ((System.DirectoryServices.Interop.UnsafeNativeMethods.IAdsPropertyList) this.entry.AdsObject).Reset();
                this.currentPropName = null;
            }

            public object Current
            {
                get
                {
                    return this.Entry.Value;
                }
            }

            public DictionaryEntry Entry
            {
                get
                {
                    if (this.currentPropName == null)
                    {
                        throw new InvalidOperationException(Res.GetString("DSNoCurrentProperty"));
                    }
                    return new DictionaryEntry(this.currentPropName, new PropertyValueCollection(this.parentEntry, this.currentPropName));
                }
            }

            public object Key
            {
                get
                {
                    return this.Entry.Key;
                }
            }

            public object Value
            {
                get
                {
                    return this.Entry.Value;
                }
            }
        }

        private class ValuesCollection : ICollection, IEnumerable
        {
            protected PropertyCollection props;

            public ValuesCollection(PropertyCollection props)
            {
                this.props = props;
            }

            public void CopyTo(Array array, int index)
            {
                foreach (object obj2 in this)
                {
                    array.SetValue(obj2, index++);
                }
            }

            public virtual IEnumerator GetEnumerator()
            {
                return new PropertyCollection.ValuesEnumerator(this.props);
            }

            public int Count
            {
                get
                {
                    return this.props.Count;
                }
            }

            public bool IsReadOnly
            {
                get
                {
                    return true;
                }
            }

            public bool IsSynchronized
            {
                get
                {
                    return false;
                }
            }

            public object SyncRoot
            {
                get
                {
                    return ((ICollection) this.props).SyncRoot;
                }
            }
        }

        private class ValuesEnumerator : IEnumerator
        {
            private int currentIndex = -1;
            protected PropertyCollection propCollection;

            public ValuesEnumerator(PropertyCollection propCollection)
            {
                this.propCollection = propCollection;
            }

            public bool MoveNext()
            {
                this.currentIndex++;
                if (this.currentIndex >= this.propCollection.Count)
                {
                    this.currentIndex = -1;
                    return false;
                }
                return true;
            }

            public void Reset()
            {
                this.currentIndex = -1;
            }

            public virtual object Current
            {
                get
                {
                    System.DirectoryServices.Interop.UnsafeNativeMethods.IAdsPropertyList adsObject = (System.DirectoryServices.Interop.UnsafeNativeMethods.IAdsPropertyList) this.propCollection.entry.AdsObject;
                    return this.propCollection[((System.DirectoryServices.Interop.UnsafeNativeMethods.IAdsPropertyEntry) adsObject.Item(this.CurrentIndex)).Name];
                }
            }

            protected int CurrentIndex
            {
                get
                {
                    if (this.currentIndex == -1)
                    {
                        throw new InvalidOperationException(Res.GetString("DSNoCurrentValue"));
                    }
                    return this.currentIndex;
                }
            }
        }
    }
}

