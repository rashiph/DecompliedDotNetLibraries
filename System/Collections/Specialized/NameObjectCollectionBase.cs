namespace System.Collections.Specialized
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.Threading;

    [Serializable]
    public abstract class NameObjectCollectionBase : ICollection, IEnumerable, ISerializable, IDeserializationCallback
    {
        private ArrayList _entriesArray;
        private Hashtable _entriesTable;
        private IEqualityComparer _keyComparer;
        private KeysCollection _keys;
        private NameObjectEntry _nullKeyEntry;
        private bool _readOnly;
        private SerializationInfo _serializationInfo;
        [NonSerialized]
        private object _syncRoot;
        private int _version;
        private const string ComparerName = "Comparer";
        private const string CountName = "Count";
        private static StringComparer defaultComparer = StringComparer.InvariantCultureIgnoreCase;
        private const string HashCodeProviderName = "HashProvider";
        private const string KeyComparerName = "KeyComparer";
        private const string KeysName = "Keys";
        private const string ReadOnlyName = "ReadOnly";
        private const string ValuesName = "Values";
        private const string VersionName = "Version";

        protected NameObjectCollectionBase() : this(defaultComparer)
        {
        }

        protected NameObjectCollectionBase(IEqualityComparer equalityComparer)
        {
            this._keyComparer = (equalityComparer == null) ? defaultComparer : equalityComparer;
            this.Reset();
        }

        internal NameObjectCollectionBase(DBNull dummy)
        {
        }

        protected NameObjectCollectionBase(int capacity)
        {
            this._keyComparer = StringComparer.InvariantCultureIgnoreCase;
            this.Reset(capacity);
        }

        [Obsolete("Please use NameObjectCollectionBase(IEqualityComparer) instead.")]
        protected NameObjectCollectionBase(IHashCodeProvider hashProvider, IComparer comparer)
        {
            this._keyComparer = new System.Collections.Specialized.CompatibleComparer(comparer, hashProvider);
            this.Reset();
        }

        protected NameObjectCollectionBase(int capacity, IEqualityComparer equalityComparer) : this(equalityComparer)
        {
            this.Reset(capacity);
        }

        protected NameObjectCollectionBase(SerializationInfo info, StreamingContext context)
        {
            this._serializationInfo = info;
        }

        [Obsolete("Please use NameObjectCollectionBase(Int32, IEqualityComparer) instead.")]
        protected NameObjectCollectionBase(int capacity, IHashCodeProvider hashProvider, IComparer comparer)
        {
            this._keyComparer = new System.Collections.Specialized.CompatibleComparer(comparer, hashProvider);
            this.Reset(capacity);
        }

        protected void BaseAdd(string name, object value)
        {
            if (this._readOnly)
            {
                throw new NotSupportedException(SR.GetString("CollectionReadOnly"));
            }
            NameObjectEntry entry = new NameObjectEntry(name, value);
            if (name != null)
            {
                if (this._entriesTable[name] == null)
                {
                    this._entriesTable.Add(name, entry);
                }
            }
            else if (this._nullKeyEntry == null)
            {
                this._nullKeyEntry = entry;
            }
            this._entriesArray.Add(entry);
            this._version++;
        }

        protected void BaseClear()
        {
            if (this._readOnly)
            {
                throw new NotSupportedException(SR.GetString("CollectionReadOnly"));
            }
            this.Reset();
        }

        protected object BaseGet(int index)
        {
            NameObjectEntry entry = (NameObjectEntry) this._entriesArray[index];
            return entry.Value;
        }

        protected object BaseGet(string name)
        {
            NameObjectEntry entry = this.FindEntry(name);
            if (entry == null)
            {
                return null;
            }
            return entry.Value;
        }

        protected string[] BaseGetAllKeys()
        {
            int count = this._entriesArray.Count;
            string[] strArray = new string[count];
            for (int i = 0; i < count; i++)
            {
                strArray[i] = this.BaseGetKey(i);
            }
            return strArray;
        }

        protected object[] BaseGetAllValues()
        {
            int count = this._entriesArray.Count;
            object[] objArray = new object[count];
            for (int i = 0; i < count; i++)
            {
                objArray[i] = this.BaseGet(i);
            }
            return objArray;
        }

        protected object[] BaseGetAllValues(Type type)
        {
            int count = this._entriesArray.Count;
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            object[] objArray = (object[]) SecurityUtils.ArrayCreateInstance(type, count);
            for (int i = 0; i < count; i++)
            {
                objArray[i] = this.BaseGet(i);
            }
            return objArray;
        }

        protected string BaseGetKey(int index)
        {
            NameObjectEntry entry = (NameObjectEntry) this._entriesArray[index];
            return entry.Key;
        }

        protected bool BaseHasKeys()
        {
            return (this._entriesTable.Count > 0);
        }

        protected void BaseRemove(string name)
        {
            if (this._readOnly)
            {
                throw new NotSupportedException(SR.GetString("CollectionReadOnly"));
            }
            if (name != null)
            {
                this._entriesTable.Remove(name);
                for (int i = this._entriesArray.Count - 1; i >= 0; i--)
                {
                    if (this._keyComparer.Equals(name, this.BaseGetKey(i)))
                    {
                        this._entriesArray.RemoveAt(i);
                    }
                }
            }
            else
            {
                this._nullKeyEntry = null;
                for (int j = this._entriesArray.Count - 1; j >= 0; j--)
                {
                    if (this.BaseGetKey(j) == null)
                    {
                        this._entriesArray.RemoveAt(j);
                    }
                }
            }
            this._version++;
        }

        protected void BaseRemoveAt(int index)
        {
            if (this._readOnly)
            {
                throw new NotSupportedException(SR.GetString("CollectionReadOnly"));
            }
            string key = this.BaseGetKey(index);
            if (key != null)
            {
                this._entriesTable.Remove(key);
            }
            else
            {
                this._nullKeyEntry = null;
            }
            this._entriesArray.RemoveAt(index);
            this._version++;
        }

        protected void BaseSet(int index, object value)
        {
            if (this._readOnly)
            {
                throw new NotSupportedException(SR.GetString("CollectionReadOnly"));
            }
            NameObjectEntry entry = (NameObjectEntry) this._entriesArray[index];
            entry.Value = value;
            this._version++;
        }

        protected void BaseSet(string name, object value)
        {
            if (this._readOnly)
            {
                throw new NotSupportedException(SR.GetString("CollectionReadOnly"));
            }
            NameObjectEntry entry = this.FindEntry(name);
            if (entry != null)
            {
                entry.Value = value;
                this._version++;
            }
            else
            {
                this.BaseAdd(name, value);
            }
        }

        private NameObjectEntry FindEntry(string key)
        {
            if (key != null)
            {
                return (NameObjectEntry) this._entriesTable[key];
            }
            return this._nullKeyEntry;
        }

        public virtual IEnumerator GetEnumerator()
        {
            return new NameObjectKeysEnumerator(this);
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.SerializationFormatter)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            info.AddValue("ReadOnly", this._readOnly);
            if (this._keyComparer == defaultComparer)
            {
                info.AddValue("HashProvider", System.Collections.Specialized.CompatibleComparer.DefaultHashCodeProvider, typeof(IHashCodeProvider));
                info.AddValue("Comparer", System.Collections.Specialized.CompatibleComparer.DefaultComparer, typeof(IComparer));
            }
            else if (this._keyComparer == null)
            {
                info.AddValue("HashProvider", null, typeof(IHashCodeProvider));
                info.AddValue("Comparer", null, typeof(IComparer));
            }
            else if (this._keyComparer is System.Collections.Specialized.CompatibleComparer)
            {
                System.Collections.Specialized.CompatibleComparer comparer = (System.Collections.Specialized.CompatibleComparer) this._keyComparer;
                info.AddValue("HashProvider", comparer.HashCodeProvider, typeof(IHashCodeProvider));
                info.AddValue("Comparer", comparer.Comparer, typeof(IComparer));
            }
            else
            {
                info.AddValue("KeyComparer", this._keyComparer, typeof(IEqualityComparer));
            }
            int count = this._entriesArray.Count;
            info.AddValue("Count", count);
            string[] strArray = new string[count];
            object[] objArray = new object[count];
            for (int i = 0; i < count; i++)
            {
                NameObjectEntry entry = (NameObjectEntry) this._entriesArray[i];
                strArray[i] = entry.Key;
                objArray[i] = entry.Value;
            }
            info.AddValue("Keys", strArray, typeof(string[]));
            info.AddValue("Values", objArray, typeof(object[]));
            info.AddValue("Version", this._version);
        }

        public virtual void OnDeserialization(object sender)
        {
            if (this._keyComparer == null)
            {
                if (this._serializationInfo == null)
                {
                    throw new SerializationException();
                }
                SerializationInfo info = this._serializationInfo;
                this._serializationInfo = null;
                bool boolean = false;
                int capacity = 0;
                string[] strArray = null;
                object[] objArray = null;
                IHashCodeProvider hashCodeProvider = null;
                IComparer comparer = null;
                bool flag2 = false;
                int num2 = 0;
                SerializationInfoEnumerator enumerator = info.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    switch (enumerator.Name)
                    {
                        case "ReadOnly":
                            boolean = info.GetBoolean("ReadOnly");
                            break;

                        case "HashProvider":
                            hashCodeProvider = (IHashCodeProvider) info.GetValue("HashProvider", typeof(IHashCodeProvider));
                            break;

                        case "Comparer":
                            comparer = (IComparer) info.GetValue("Comparer", typeof(IComparer));
                            break;

                        case "KeyComparer":
                            this._keyComparer = (IEqualityComparer) info.GetValue("KeyComparer", typeof(IEqualityComparer));
                            break;

                        case "Count":
                            capacity = info.GetInt32("Count");
                            break;

                        case "Keys":
                            strArray = (string[]) info.GetValue("Keys", typeof(string[]));
                            break;

                        case "Values":
                            objArray = (object[]) info.GetValue("Values", typeof(object[]));
                            break;

                        case "Version":
                            flag2 = true;
                            num2 = info.GetInt32("Version");
                            break;
                    }
                }
                if (this._keyComparer == null)
                {
                    if ((comparer == null) || (hashCodeProvider == null))
                    {
                        throw new SerializationException();
                    }
                    this._keyComparer = new System.Collections.Specialized.CompatibleComparer(comparer, hashCodeProvider);
                }
                if ((strArray == null) || (objArray == null))
                {
                    throw new SerializationException();
                }
                this.Reset(capacity);
                for (int i = 0; i < capacity; i++)
                {
                    this.BaseAdd(strArray[i], objArray[i]);
                }
                this._readOnly = boolean;
                if (flag2)
                {
                    this._version = num2;
                }
            }
        }

        private void Reset()
        {
            this._entriesArray = new ArrayList();
            this._entriesTable = new Hashtable(this._keyComparer);
            this._nullKeyEntry = null;
            this._version++;
        }

        private void Reset(int capacity)
        {
            this._entriesArray = new ArrayList(capacity);
            this._entriesTable = new Hashtable(capacity, this._keyComparer);
            this._nullKeyEntry = null;
            this._version++;
        }

        void ICollection.CopyTo(Array array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            if (array.Rank != 1)
            {
                throw new ArgumentException(SR.GetString("Arg_MultiRank"));
            }
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index", SR.GetString("IndexOutOfRange", new object[] { index.ToString(CultureInfo.CurrentCulture) }));
            }
            if ((array.Length - index) < this._entriesArray.Count)
            {
                throw new ArgumentException(SR.GetString("Arg_InsufficientSpace"));
            }
            IEnumerator enumerator = this.GetEnumerator();
            while (enumerator.MoveNext())
            {
                array.SetValue(enumerator.Current, index++);
            }
        }

        internal IEqualityComparer Comparer
        {
            get
            {
                return this._keyComparer;
            }
            set
            {
                this._keyComparer = value;
            }
        }

        public virtual int Count
        {
            get
            {
                return this._entriesArray.Count;
            }
        }

        protected bool IsReadOnly
        {
            get
            {
                return this._readOnly;
            }
            set
            {
                this._readOnly = value;
            }
        }

        public virtual KeysCollection Keys
        {
            get
            {
                if (this._keys == null)
                {
                    this._keys = new KeysCollection(this);
                }
                return this._keys;
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
                if (this._syncRoot == null)
                {
                    Interlocked.CompareExchange(ref this._syncRoot, new object(), null);
                }
                return this._syncRoot;
            }
        }

        [Serializable]
        public class KeysCollection : ICollection, IEnumerable
        {
            private NameObjectCollectionBase _coll;

            internal KeysCollection(NameObjectCollectionBase coll)
            {
                this._coll = coll;
            }

            public virtual string Get(int index)
            {
                return this._coll.BaseGetKey(index);
            }

            public IEnumerator GetEnumerator()
            {
                return new NameObjectCollectionBase.NameObjectKeysEnumerator(this._coll);
            }

            void ICollection.CopyTo(Array array, int index)
            {
                if (array == null)
                {
                    throw new ArgumentNullException("array");
                }
                if (array.Rank != 1)
                {
                    throw new ArgumentException(SR.GetString("Arg_MultiRank"));
                }
                if (index < 0)
                {
                    throw new ArgumentOutOfRangeException("index", SR.GetString("IndexOutOfRange", new object[] { index.ToString(CultureInfo.CurrentCulture) }));
                }
                if ((array.Length - index) < this._coll.Count)
                {
                    throw new ArgumentException(SR.GetString("Arg_InsufficientSpace"));
                }
                IEnumerator enumerator = this.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    array.SetValue(enumerator.Current, index++);
                }
            }

            public int Count
            {
                get
                {
                    return this._coll.Count;
                }
            }

            public string this[int index]
            {
                get
                {
                    return this.Get(index);
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
                    return ((ICollection) this._coll).SyncRoot;
                }
            }
        }

        internal class NameObjectEntry
        {
            internal string Key;
            internal object Value;

            internal NameObjectEntry(string name, object value)
            {
                this.Key = name;
                this.Value = value;
            }
        }

        [Serializable]
        internal class NameObjectKeysEnumerator : IEnumerator
        {
            private NameObjectCollectionBase _coll;
            private int _pos;
            private int _version;

            internal NameObjectKeysEnumerator(NameObjectCollectionBase coll)
            {
                this._coll = coll;
                this._version = this._coll._version;
                this._pos = -1;
            }

            public bool MoveNext()
            {
                if (this._version != this._coll._version)
                {
                    throw new InvalidOperationException(SR.GetString("InvalidOperation_EnumFailedVersion"));
                }
                if (this._pos < (this._coll.Count - 1))
                {
                    this._pos++;
                    return true;
                }
                this._pos = this._coll.Count;
                return false;
            }

            public void Reset()
            {
                if (this._version != this._coll._version)
                {
                    throw new InvalidOperationException(SR.GetString("InvalidOperation_EnumFailedVersion"));
                }
                this._pos = -1;
            }

            public object Current
            {
                get
                {
                    if ((this._pos < 0) || (this._pos >= this._coll.Count))
                    {
                        throw new InvalidOperationException(SR.GetString("InvalidOperation_EnumOpCantHappen"));
                    }
                    return this._coll.BaseGetKey(this._pos);
                }
            }
        }
    }
}

