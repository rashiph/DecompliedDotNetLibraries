namespace System.Collections
{
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Security.Permissions;
    using System.Threading;

    [Serializable, DebuggerTypeProxy(typeof(Hashtable.HashtableDebugView)), DebuggerDisplay("Count = {Count}"), ComVisible(true)]
    public class Hashtable : IDictionary, ICollection, IEnumerable, ISerializable, IDeserializationCallback, ICloneable
    {
        private IEqualityComparer _keycomparer;
        private object _syncRoot;
        private bucket[] buckets;
        private const string ComparerName = "Comparer";
        private int count;
        private const string HashCodeProviderName = "HashCodeProvider";
        private const string HashSizeName = "HashSize";
        private const int InitialSize = 3;
        private volatile bool isWriterInProgress;
        private const string KeyComparerName = "KeyComparer";
        private ICollection keys;
        private const string KeysName = "Keys";
        private float loadFactor;
        private const string LoadFactorName = "LoadFactor";
        private int loadsize;
        private SerializationInfo m_siInfo;
        private int occupancy;
        private ICollection values;
        private const string ValuesName = "Values";
        private volatile int version;
        private const string VersionName = "Version";

        public Hashtable() : this(0, (float) 1f)
        {
        }

        internal Hashtable(bool trash)
        {
        }

        public Hashtable(IDictionary d) : this(d, (float) 1f)
        {
        }

        public Hashtable(IEqualityComparer equalityComparer) : this(0, (float) 1f, equalityComparer)
        {
        }

        public Hashtable(int capacity) : this(capacity, (float) 1f)
        {
        }

        public Hashtable(IDictionary d, IEqualityComparer equalityComparer) : this(d, (float) 1f, equalityComparer)
        {
        }

        public Hashtable(IDictionary d, float loadFactor) : this(d, loadFactor, (IEqualityComparer) null)
        {
        }

        [Obsolete("Please use Hashtable(IEqualityComparer) instead.")]
        public Hashtable(IHashCodeProvider hcp, IComparer comparer) : this(0, 1f, hcp, comparer)
        {
        }

        public Hashtable(int capacity, IEqualityComparer equalityComparer) : this(capacity, (float) 1f, equalityComparer)
        {
        }

        public Hashtable(int capacity, float loadFactor)
        {
            if (capacity < 0)
            {
                throw new ArgumentOutOfRangeException("capacity", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if ((loadFactor < 0.1f) || (loadFactor > 1f))
            {
                throw new ArgumentOutOfRangeException("loadFactor", Environment.GetResourceString("ArgumentOutOfRange_HashtableLoadFactor", new object[] { 0.1, 1.0 }));
            }
            this.loadFactor = 0.72f * loadFactor;
            double num = ((float) capacity) / this.loadFactor;
            if (num > 2147483647.0)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_HTCapacityOverflow"));
            }
            int num2 = (num > 3.0) ? HashHelpers.GetPrime((int) num) : 3;
            this.buckets = new bucket[num2];
            this.loadsize = (int) (this.loadFactor * num2);
            this.isWriterInProgress = false;
        }

        protected Hashtable(SerializationInfo info, StreamingContext context)
        {
            this.m_siInfo = info;
        }

        [Obsolete("Please use Hashtable(IDictionary, IEqualityComparer) instead.")]
        public Hashtable(IDictionary d, IHashCodeProvider hcp, IComparer comparer) : this(d, 1f, hcp, comparer)
        {
        }

        public Hashtable(IDictionary d, float loadFactor, IEqualityComparer equalityComparer) : this((d != null) ? d.Count : 0, loadFactor, equalityComparer)
        {
            if (d == null)
            {
                throw new ArgumentNullException("d", Environment.GetResourceString("ArgumentNull_Dictionary"));
            }
            IDictionaryEnumerator enumerator = d.GetEnumerator();
            while (enumerator.MoveNext())
            {
                this.Add(enumerator.Key, enumerator.Value);
            }
        }

        [Obsolete("Please use Hashtable(int, IEqualityComparer) instead.")]
        public Hashtable(int capacity, IHashCodeProvider hcp, IComparer comparer) : this(capacity, 1f, hcp, comparer)
        {
        }

        public Hashtable(int capacity, float loadFactor, IEqualityComparer equalityComparer) : this(capacity, loadFactor)
        {
            this._keycomparer = equalityComparer;
        }

        [Obsolete("Please use Hashtable(IDictionary, float, IEqualityComparer) instead.")]
        public Hashtable(IDictionary d, float loadFactor, IHashCodeProvider hcp, IComparer comparer) : this((d != null) ? d.Count : 0, loadFactor, hcp, comparer)
        {
            if (d == null)
            {
                throw new ArgumentNullException("d", Environment.GetResourceString("ArgumentNull_Dictionary"));
            }
            IDictionaryEnumerator enumerator = d.GetEnumerator();
            while (enumerator.MoveNext())
            {
                this.Add(enumerator.Key, enumerator.Value);
            }
        }

        [Obsolete("Please use Hashtable(int, float, IEqualityComparer) instead.")]
        public Hashtable(int capacity, float loadFactor, IHashCodeProvider hcp, IComparer comparer) : this(capacity, loadFactor)
        {
            if ((hcp == null) && (comparer == null))
            {
                this._keycomparer = null;
            }
            else
            {
                this._keycomparer = new CompatibleComparer(comparer, hcp);
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public virtual void Add(object key, object value)
        {
            this.Insert(key, value, true);
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public virtual void Clear()
        {
            if ((this.count != 0) || (this.occupancy != 0))
            {
                Thread.BeginCriticalRegion();
                this.isWriterInProgress = true;
                for (int i = 0; i < this.buckets.Length; i++)
                {
                    this.buckets[i].hash_coll = 0;
                    this.buckets[i].key = null;
                    this.buckets[i].val = null;
                }
                this.count = 0;
                this.occupancy = 0;
                this.UpdateVersion();
                this.isWriterInProgress = false;
                Thread.EndCriticalRegion();
            }
        }

        public virtual object Clone()
        {
            bucket[] buckets = this.buckets;
            Hashtable hashtable = new Hashtable(this.count, this._keycomparer) {
                version = this.version,
                loadFactor = this.loadFactor,
                count = 0
            };
            int length = buckets.Length;
            while (length > 0)
            {
                length--;
                object key = buckets[length].key;
                if ((key != null) && (key != buckets))
                {
                    hashtable[key] = buckets[length].val;
                }
            }
            return hashtable;
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public virtual bool Contains(object key)
        {
            return this.ContainsKey(key);
        }

        public virtual bool ContainsKey(object key)
        {
            uint num;
            uint num2;
            Hashtable.bucket bucket;
            if (key == null)
            {
                throw new ArgumentNullException("key", Environment.GetResourceString("ArgumentNull_Key"));
            }
            Hashtable.bucket[] buckets = this.buckets;
            uint num3 = this.InitHash(key, buckets.Length, out num, out num2);
            int num4 = 0;
            int index = (int) (num % buckets.Length);
            do
            {
                bucket = buckets[index];
                if (bucket.key == null)
                {
                    return false;
                }
                if (((bucket.hash_coll & 0x7fffffff) == num3) && this.KeyEquals(bucket.key, key))
                {
                    return true;
                }
                index = (int) ((index + num2) % ((ulong) buckets.Length));
            }
            while ((bucket.hash_coll < 0) && (++num4 < buckets.Length));
            return false;
        }

        public virtual bool ContainsValue(object value)
        {
            if (value == null)
            {
                int length = this.buckets.Length;
                while (--length >= 0)
                {
                    if (((this.buckets[length].key != null) && (this.buckets[length].key != this.buckets)) && (this.buckets[length].val == null))
                    {
                        return true;
                    }
                }
            }
            else
            {
                int index = this.buckets.Length;
                while (--index >= 0)
                {
                    object val = this.buckets[index].val;
                    if ((val != null) && val.Equals(value))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void CopyEntries(Array array, int arrayIndex)
        {
            bucket[] buckets = this.buckets;
            int length = buckets.Length;
            while (--length >= 0)
            {
                object key = buckets[length].key;
                if ((key != null) && (key != this.buckets))
                {
                    DictionaryEntry entry = new DictionaryEntry(key, buckets[length].val);
                    array.SetValue(entry, arrayIndex++);
                }
            }
        }

        private void CopyKeys(Array array, int arrayIndex)
        {
            bucket[] buckets = this.buckets;
            int length = buckets.Length;
            while (--length >= 0)
            {
                object key = buckets[length].key;
                if ((key != null) && (key != this.buckets))
                {
                    array.SetValue(key, arrayIndex++);
                }
            }
        }

        public virtual void CopyTo(Array array, int arrayIndex)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array", Environment.GetResourceString("ArgumentNull_Array"));
            }
            if (array.Rank != 1)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_RankMultiDimNotSupported"));
            }
            if (arrayIndex < 0)
            {
                throw new ArgumentOutOfRangeException("arrayIndex", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if ((array.Length - arrayIndex) < this.Count)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_ArrayPlusOffTooSmall"));
            }
            this.CopyEntries(array, arrayIndex);
        }

        private void CopyValues(Array array, int arrayIndex)
        {
            bucket[] buckets = this.buckets;
            int length = buckets.Length;
            while (--length >= 0)
            {
                object key = buckets[length].key;
                if ((key != null) && (key != this.buckets))
                {
                    array.SetValue(buckets[length].val, arrayIndex++);
                }
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        private void expand()
        {
            int prime = HashHelpers.GetPrime(this.buckets.Length * 2);
            this.rehash(prime);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public virtual IDictionaryEnumerator GetEnumerator()
        {
            return new HashtableEnumerator(this, 3);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        protected virtual int GetHash(object key)
        {
            if (this._keycomparer != null)
            {
                return this._keycomparer.GetHashCode(key);
            }
            return key.GetHashCode();
        }

        [SecurityCritical]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            lock (this.SyncRoot)
            {
                int version = this.version;
                info.AddValue("LoadFactor", this.loadFactor);
                info.AddValue("Version", (int) this.version);
                if (this._keycomparer == null)
                {
                    info.AddValue("Comparer", null, typeof(IComparer));
                    info.AddValue("HashCodeProvider", null, typeof(IHashCodeProvider));
                }
                else if (this._keycomparer is CompatibleComparer)
                {
                    CompatibleComparer comparer = this._keycomparer as CompatibleComparer;
                    info.AddValue("Comparer", comparer.Comparer, typeof(IComparer));
                    info.AddValue("HashCodeProvider", comparer.HashCodeProvider, typeof(IHashCodeProvider));
                }
                else
                {
                    info.AddValue("KeyComparer", this._keycomparer, typeof(IEqualityComparer));
                }
                info.AddValue("HashSize", this.buckets.Length);
                object[] array = new object[this.count];
                object[] objArray2 = new object[this.count];
                this.CopyKeys(array, 0);
                this.CopyValues(objArray2, 0);
                info.AddValue("Keys", array, typeof(object[]));
                info.AddValue("Values", objArray2, typeof(object[]));
                if (this.version != version)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EnumFailedVersion"));
                }
            }
        }

        private uint InitHash(object key, int hashsize, out uint seed, out uint incr)
        {
            uint num = (uint) (this.GetHash(key) & 0x7fffffff);
            seed = num;
            incr = 1 + ((uint) (((seed >> 5) + 1) % (hashsize - 1)));
            return num;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        private void Insert(object key, object nvalue, bool add)
        {
            uint num;
            uint num2;
            if (key == null)
            {
                throw new ArgumentNullException("key", Environment.GetResourceString("ArgumentNull_Key"));
            }
            if (this.count >= this.loadsize)
            {
                this.expand();
            }
            else if ((this.occupancy > this.loadsize) && (this.count > 100))
            {
                this.rehash();
            }
            uint num3 = this.InitHash(key, this.buckets.Length, out num, out num2);
            int num4 = 0;
            int index = -1;
            int num6 = (int) (num % this.buckets.Length);
        Label_0071:
            if (((index == -1) && (this.buckets[num6].key == this.buckets)) && (this.buckets[num6].hash_coll < 0))
            {
                index = num6;
            }
            if ((this.buckets[num6].key == null) || ((this.buckets[num6].key == this.buckets) && ((this.buckets[num6].hash_coll & 0x80000000L) == 0L)))
            {
                if (index != -1)
                {
                    num6 = index;
                }
                Thread.BeginCriticalRegion();
                this.isWriterInProgress = true;
                this.buckets[num6].val = nvalue;
                this.buckets[num6].key = key;
                this.buckets[num6].hash_coll |= (int) num3;
                this.count++;
                this.UpdateVersion();
                this.isWriterInProgress = false;
                Thread.EndCriticalRegion();
            }
            else if (((this.buckets[num6].hash_coll & 0x7fffffff) == num3) && this.KeyEquals(this.buckets[num6].key, key))
            {
                if (add)
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_AddingDuplicate__", new object[] { this.buckets[num6].key, key }));
                }
                Thread.BeginCriticalRegion();
                this.isWriterInProgress = true;
                this.buckets[num6].val = nvalue;
                this.UpdateVersion();
                this.isWriterInProgress = false;
                Thread.EndCriticalRegion();
            }
            else
            {
                if ((index == -1) && (this.buckets[num6].hash_coll >= 0))
                {
                    this.buckets[num6].hash_coll |= -2147483648;
                    this.occupancy++;
                }
                num6 = (int) ((num6 + num2) % ((ulong) this.buckets.Length));
                if (++num4 < this.buckets.Length)
                {
                    goto Label_0071;
                }
                if (index == -1)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_HashInsertFailed"));
                }
                Thread.BeginCriticalRegion();
                this.isWriterInProgress = true;
                this.buckets[index].val = nvalue;
                this.buckets[index].key = key;
                this.buckets[index].hash_coll |= (int) num3;
                this.count++;
                this.UpdateVersion();
                this.isWriterInProgress = false;
                Thread.EndCriticalRegion();
            }
        }

        protected virtual bool KeyEquals(object item, object key)
        {
            if (object.ReferenceEquals(this.buckets, item))
            {
                return false;
            }
            if (object.ReferenceEquals(item, key))
            {
                return true;
            }
            if (this._keycomparer != null)
            {
                return this._keycomparer.Equals(item, key);
            }
            return ((item != null) && item.Equals(key));
        }

        public virtual void OnDeserialization(object sender)
        {
            if (this.buckets == null)
            {
                if (this.m_siInfo == null)
                {
                    throw new SerializationException(Environment.GetResourceString("Serialization_InvalidOnDeser"));
                }
                int num = 0;
                IComparer comparer = null;
                IHashCodeProvider hashCodeProvider = null;
                object[] objArray = null;
                object[] objArray2 = null;
                SerializationInfoEnumerator enumerator = this.m_siInfo.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    switch (enumerator.Name)
                    {
                        case "LoadFactor":
                            this.loadFactor = this.m_siInfo.GetSingle("LoadFactor");
                            break;

                        case "HashSize":
                            num = this.m_siInfo.GetInt32("HashSize");
                            break;

                        case "KeyComparer":
                            this._keycomparer = (IEqualityComparer) this.m_siInfo.GetValue("KeyComparer", typeof(IEqualityComparer));
                            break;

                        case "Comparer":
                            comparer = (IComparer) this.m_siInfo.GetValue("Comparer", typeof(IComparer));
                            break;

                        case "HashCodeProvider":
                            hashCodeProvider = (IHashCodeProvider) this.m_siInfo.GetValue("HashCodeProvider", typeof(IHashCodeProvider));
                            break;

                        case "Keys":
                            objArray = (object[]) this.m_siInfo.GetValue("Keys", typeof(object[]));
                            break;

                        case "Values":
                            objArray2 = (object[]) this.m_siInfo.GetValue("Values", typeof(object[]));
                            break;
                    }
                }
                this.loadsize = (int) (this.loadFactor * num);
                if ((this._keycomparer == null) && ((comparer != null) || (hashCodeProvider != null)))
                {
                    this._keycomparer = new CompatibleComparer(comparer, hashCodeProvider);
                }
                this.buckets = new bucket[num];
                if (objArray == null)
                {
                    throw new SerializationException(Environment.GetResourceString("Serialization_MissingKeys"));
                }
                if (objArray2 == null)
                {
                    throw new SerializationException(Environment.GetResourceString("Serialization_MissingValues"));
                }
                if (objArray.Length != objArray2.Length)
                {
                    throw new SerializationException(Environment.GetResourceString("Serialization_KeyValueDifferentSizes"));
                }
                for (int i = 0; i < objArray.Length; i++)
                {
                    if (objArray[i] == null)
                    {
                        throw new SerializationException(Environment.GetResourceString("Serialization_NullKey"));
                    }
                    this.Insert(objArray[i], objArray2[i], true);
                }
                this.version = this.m_siInfo.GetInt32("Version");
                this.m_siInfo = null;
            }
        }

        private void putEntry(bucket[] newBuckets, object key, object nvalue, int hashcode)
        {
            uint num = (uint) hashcode;
            uint num2 = (uint) (1 + (((num >> 5) + 1) % (newBuckets.Length - 1)));
            int index = (int) (num % newBuckets.Length);
        Label_0017:
            if ((newBuckets[index].key == null) || (newBuckets[index].key == this.buckets))
            {
                newBuckets[index].val = nvalue;
                newBuckets[index].key = key;
                newBuckets[index].hash_coll |= hashcode;
            }
            else
            {
                if (newBuckets[index].hash_coll >= 0)
                {
                    newBuckets[index].hash_coll |= -2147483648;
                    this.occupancy++;
                }
                index = (int) ((index + num2) % ((ulong) newBuckets.Length));
                goto Label_0017;
            }
        }

        private void rehash()
        {
            this.rehash(this.buckets.Length);
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        private void rehash(int newsize)
        {
            this.occupancy = 0;
            Hashtable.bucket[] newBuckets = new Hashtable.bucket[newsize];
            for (int i = 0; i < this.buckets.Length; i++)
            {
                Hashtable.bucket bucket = this.buckets[i];
                if ((bucket.key != null) && (bucket.key != this.buckets))
                {
                    this.putEntry(newBuckets, bucket.key, bucket.val, bucket.hash_coll & 0x7fffffff);
                }
            }
            Thread.BeginCriticalRegion();
            this.isWriterInProgress = true;
            this.buckets = newBuckets;
            this.loadsize = (int) (this.loadFactor * newsize);
            this.UpdateVersion();
            this.isWriterInProgress = false;
            Thread.EndCriticalRegion();
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public virtual void Remove(object key)
        {
            uint num;
            uint num2;
            Hashtable.bucket bucket;
            if (key == null)
            {
                throw new ArgumentNullException("key", Environment.GetResourceString("ArgumentNull_Key"));
            }
            uint num3 = this.InitHash(key, this.buckets.Length, out num, out num2);
            int num4 = 0;
            int index = (int) (num % this.buckets.Length);
        Label_003A:
            bucket = this.buckets[index];
            if (((bucket.hash_coll & 0x7fffffff) == num3) && this.KeyEquals(bucket.key, key))
            {
                Thread.BeginCriticalRegion();
                this.isWriterInProgress = true;
                this.buckets[index].hash_coll &= -2147483648;
                if (this.buckets[index].hash_coll != 0)
                {
                    this.buckets[index].key = this.buckets;
                }
                else
                {
                    this.buckets[index].key = null;
                }
                this.buckets[index].val = null;
                this.count--;
                this.UpdateVersion();
                this.isWriterInProgress = false;
                Thread.EndCriticalRegion();
            }
            else
            {
                index = (int) ((index + num2) % ((ulong) this.buckets.Length));
                if ((bucket.hash_coll < 0) && (++num4 < this.buckets.Length))
                {
                    goto Label_003A;
                }
            }
        }

        [HostProtection(SecurityAction.LinkDemand, Synchronization=true)]
        public static Hashtable Synchronized(Hashtable table)
        {
            if (table == null)
            {
                throw new ArgumentNullException("table");
            }
            return new SyncHashtable(table);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return new HashtableEnumerator(this, 3);
        }

        internal virtual KeyValuePairs[] ToKeyValuePairsArray()
        {
            KeyValuePairs[] pairsArray = new KeyValuePairs[this.count];
            int num = 0;
            bucket[] buckets = this.buckets;
            int length = buckets.Length;
            while (--length >= 0)
            {
                object key = buckets[length].key;
                if ((key != null) && (key != this.buckets))
                {
                    pairsArray[num++] = new KeyValuePairs(key, buckets[length].val);
                }
            }
            return pairsArray;
        }

        private void UpdateVersion()
        {
            this.version++;
        }

        [Obsolete("Please use KeyComparer properties.")]
        protected IComparer comparer
        {
            get
            {
                if (this._keycomparer is CompatibleComparer)
                {
                    return ((CompatibleComparer) this._keycomparer).Comparer;
                }
                if (this._keycomparer != null)
                {
                    throw new ArgumentException(Environment.GetResourceString("Arg_CannotMixComparisonInfrastructure"));
                }
                return null;
            }
            set
            {
                if (this._keycomparer is CompatibleComparer)
                {
                    CompatibleComparer comparer = (CompatibleComparer) this._keycomparer;
                    this._keycomparer = new CompatibleComparer(value, comparer.HashCodeProvider);
                }
                else
                {
                    if (this._keycomparer != null)
                    {
                        throw new ArgumentException(Environment.GetResourceString("Arg_CannotMixComparisonInfrastructure"));
                    }
                    this._keycomparer = new CompatibleComparer(value, null);
                }
            }
        }

        public virtual int Count
        {
            get
            {
                return this.count;
            }
        }

        protected IEqualityComparer EqualityComparer
        {
            get
            {
                return this._keycomparer;
            }
        }

        [Obsolete("Please use EqualityComparer property.")]
        protected IHashCodeProvider hcp
        {
            get
            {
                if (this._keycomparer is CompatibleComparer)
                {
                    return ((CompatibleComparer) this._keycomparer).HashCodeProvider;
                }
                if (this._keycomparer != null)
                {
                    throw new ArgumentException(Environment.GetResourceString("Arg_CannotMixComparisonInfrastructure"));
                }
                return null;
            }
            set
            {
                if (this._keycomparer is CompatibleComparer)
                {
                    CompatibleComparer comparer = (CompatibleComparer) this._keycomparer;
                    this._keycomparer = new CompatibleComparer(comparer.Comparer, value);
                }
                else
                {
                    if (this._keycomparer != null)
                    {
                        throw new ArgumentException(Environment.GetResourceString("Arg_CannotMixComparisonInfrastructure"));
                    }
                    this._keycomparer = new CompatibleComparer(null, value);
                }
            }
        }

        public virtual bool IsFixedSize
        {
            get
            {
                return false;
            }
        }

        public virtual bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public virtual bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        public virtual object this[object key]
        {
            get
            {
                uint num;
                uint num2;
                Hashtable.bucket bucket;
                int version;
                int num7;
                if (key == null)
                {
                    throw new ArgumentNullException("key", Environment.GetResourceString("ArgumentNull_Key"));
                }
                Hashtable.bucket[] buckets = this.buckets;
                uint num3 = this.InitHash(key, buckets.Length, out num, out num2);
                int num4 = 0;
                int index = (int) (num % buckets.Length);
            Label_0038:
                num7 = 0;
                do
                {
                    version = this.version;
                    bucket = buckets[index];
                    if ((++num7 % 8) == 0)
                    {
                        Thread.Sleep(1);
                    }
                }
                while (this.isWriterInProgress || (version != this.version));
                if (bucket.key != null)
                {
                    if (((bucket.hash_coll & 0x7fffffff) == num3) && this.KeyEquals(bucket.key, key))
                    {
                        return bucket.val;
                    }
                    index = (int) ((index + num2) % ((ulong) buckets.Length));
                    if ((bucket.hash_coll < 0) && (++num4 < buckets.Length))
                    {
                        goto Label_0038;
                    }
                }
                return null;
            }
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            set
            {
                this.Insert(key, value, false);
            }
        }

        public virtual ICollection Keys
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                if (this.keys == null)
                {
                    this.keys = new KeyCollection(this);
                }
                return this.keys;
            }
        }

        public virtual object SyncRoot
        {
            get
            {
                if (this._syncRoot == null)
                {
                    Interlocked.CompareExchange<object>(ref this._syncRoot, new object(), null);
                }
                return this._syncRoot;
            }
        }

        public virtual ICollection Values
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                if (this.values == null)
                {
                    this.values = new ValueCollection(this);
                }
                return this.values;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct bucket
        {
            public object key;
            public object val;
            public int hash_coll;
        }

        internal class HashtableDebugView
        {
            private Hashtable hashtable;

            public HashtableDebugView(Hashtable hashtable)
            {
                if (hashtable == null)
                {
                    throw new ArgumentNullException("hashtable");
                }
                this.hashtable = hashtable;
            }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public KeyValuePairs[] Items
            {
                get
                {
                    return this.hashtable.ToKeyValuePairsArray();
                }
            }
        }

        [Serializable]
        private class HashtableEnumerator : IDictionaryEnumerator, IEnumerator, ICloneable
        {
            private int bucket;
            private bool current;
            private object currentKey;
            private object currentValue;
            internal const int DictEntry = 3;
            private int getObjectRetType;
            private Hashtable hashtable;
            internal const int Keys = 1;
            internal const int Values = 2;
            private int version;

            internal HashtableEnumerator(Hashtable hashtable, int getObjRetType)
            {
                this.hashtable = hashtable;
                this.bucket = hashtable.buckets.Length;
                this.version = hashtable.version;
                this.current = false;
                this.getObjectRetType = getObjRetType;
            }

            public object Clone()
            {
                return base.MemberwiseClone();
            }

            public virtual bool MoveNext()
            {
                if (this.version != this.hashtable.version)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EnumFailedVersion"));
                }
                while (this.bucket > 0)
                {
                    this.bucket--;
                    object key = this.hashtable.buckets[this.bucket].key;
                    if ((key != null) && (key != this.hashtable.buckets))
                    {
                        this.currentKey = key;
                        this.currentValue = this.hashtable.buckets[this.bucket].val;
                        this.current = true;
                        return true;
                    }
                }
                this.current = false;
                return false;
            }

            public virtual void Reset()
            {
                if (this.version != this.hashtable.version)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EnumFailedVersion"));
                }
                this.current = false;
                this.bucket = this.hashtable.buckets.Length;
                this.currentKey = null;
                this.currentValue = null;
            }

            public virtual object Current
            {
                get
                {
                    if (!this.current)
                    {
                        throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EnumOpCantHappen"));
                    }
                    if (this.getObjectRetType == 1)
                    {
                        return this.currentKey;
                    }
                    if (this.getObjectRetType == 2)
                    {
                        return this.currentValue;
                    }
                    return new DictionaryEntry(this.currentKey, this.currentValue);
                }
            }

            public virtual DictionaryEntry Entry
            {
                get
                {
                    if (!this.current)
                    {
                        throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EnumOpCantHappen"));
                    }
                    return new DictionaryEntry(this.currentKey, this.currentValue);
                }
            }

            public virtual object Key
            {
                [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
                get
                {
                    if (!this.current)
                    {
                        throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EnumNotStarted"));
                    }
                    return this.currentKey;
                }
            }

            public virtual object Value
            {
                [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
                get
                {
                    if (!this.current)
                    {
                        throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EnumOpCantHappen"));
                    }
                    return this.currentValue;
                }
            }
        }

        [Serializable]
        private class KeyCollection : ICollection, IEnumerable
        {
            private Hashtable _hashtable;

            internal KeyCollection(Hashtable hashtable)
            {
                this._hashtable = hashtable;
            }

            public virtual void CopyTo(Array array, int arrayIndex)
            {
                if (array == null)
                {
                    throw new ArgumentNullException("array");
                }
                if (array.Rank != 1)
                {
                    throw new ArgumentException(Environment.GetResourceString("Arg_RankMultiDimNotSupported"));
                }
                if (arrayIndex < 0)
                {
                    throw new ArgumentOutOfRangeException("arrayIndex", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
                }
                if ((array.Length - arrayIndex) < this._hashtable.count)
                {
                    throw new ArgumentException(Environment.GetResourceString("Arg_ArrayPlusOffTooSmall"));
                }
                this._hashtable.CopyKeys(array, arrayIndex);
            }

            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            public virtual IEnumerator GetEnumerator()
            {
                return new Hashtable.HashtableEnumerator(this._hashtable, 1);
            }

            public virtual int Count
            {
                get
                {
                    return this._hashtable.count;
                }
            }

            public virtual bool IsSynchronized
            {
                get
                {
                    return this._hashtable.IsSynchronized;
                }
            }

            public virtual object SyncRoot
            {
                get
                {
                    return this._hashtable.SyncRoot;
                }
            }
        }

        [Serializable]
        private class SyncHashtable : Hashtable, IEnumerable
        {
            protected Hashtable _table;

            internal SyncHashtable(Hashtable table) : base(false)
            {
                this._table = table;
            }

            internal SyncHashtable(SerializationInfo info, StreamingContext context) : base(info, context)
            {
                this._table = (Hashtable) info.GetValue("ParentTable", typeof(Hashtable));
                if (this._table == null)
                {
                    throw new SerializationException(Environment.GetResourceString("Serialization_InsufficientState"));
                }
            }

            public override void Add(object key, object value)
            {
                lock (this._table.SyncRoot)
                {
                    this._table.Add(key, value);
                }
            }

            public override void Clear()
            {
                lock (this._table.SyncRoot)
                {
                    this._table.Clear();
                }
            }

            public override object Clone()
            {
                lock (this._table.SyncRoot)
                {
                    return Hashtable.Synchronized((Hashtable) this._table.Clone());
                }
            }

            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            public override bool Contains(object key)
            {
                return this._table.Contains(key);
            }

            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            public override bool ContainsKey(object key)
            {
                return this._table.ContainsKey(key);
            }

            public override bool ContainsValue(object key)
            {
                lock (this._table.SyncRoot)
                {
                    return this._table.ContainsValue(key);
                }
            }

            public override void CopyTo(Array array, int arrayIndex)
            {
                lock (this._table.SyncRoot)
                {
                    this._table.CopyTo(array, arrayIndex);
                }
            }

            public override IDictionaryEnumerator GetEnumerator()
            {
                return this._table.GetEnumerator();
            }

            [SecurityCritical]
            public override void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                if (info == null)
                {
                    throw new ArgumentNullException("info");
                }
                lock (this._table.SyncRoot)
                {
                    info.AddValue("ParentTable", this._table, typeof(Hashtable));
                }
            }

            public override void OnDeserialization(object sender)
            {
            }

            public override void Remove(object key)
            {
                lock (this._table.SyncRoot)
                {
                    this._table.Remove(key);
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this._table.GetEnumerator();
            }

            internal override KeyValuePairs[] ToKeyValuePairsArray()
            {
                return this._table.ToKeyValuePairsArray();
            }

            public override int Count
            {
                get
                {
                    return this._table.Count;
                }
            }

            public override bool IsFixedSize
            {
                get
                {
                    return this._table.IsFixedSize;
                }
            }

            public override bool IsReadOnly
            {
                get
                {
                    return this._table.IsReadOnly;
                }
            }

            public override bool IsSynchronized
            {
                get
                {
                    return true;
                }
            }

            public override object this[object key]
            {
                [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
                get
                {
                    return this._table[key];
                }
                set
                {
                    lock (this._table.SyncRoot)
                    {
                        this._table[key] = value;
                    }
                }
            }

            public override ICollection Keys
            {
                get
                {
                    lock (this._table.SyncRoot)
                    {
                        return this._table.Keys;
                    }
                }
            }

            public override object SyncRoot
            {
                get
                {
                    return this._table.SyncRoot;
                }
            }

            public override ICollection Values
            {
                get
                {
                    lock (this._table.SyncRoot)
                    {
                        return this._table.Values;
                    }
                }
            }
        }

        [Serializable]
        private class ValueCollection : ICollection, IEnumerable
        {
            private Hashtable _hashtable;

            internal ValueCollection(Hashtable hashtable)
            {
                this._hashtable = hashtable;
            }

            public virtual void CopyTo(Array array, int arrayIndex)
            {
                if (array == null)
                {
                    throw new ArgumentNullException("array");
                }
                if (array.Rank != 1)
                {
                    throw new ArgumentException(Environment.GetResourceString("Arg_RankMultiDimNotSupported"));
                }
                if (arrayIndex < 0)
                {
                    throw new ArgumentOutOfRangeException("arrayIndex", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
                }
                if ((array.Length - arrayIndex) < this._hashtable.count)
                {
                    throw new ArgumentException(Environment.GetResourceString("Arg_ArrayPlusOffTooSmall"));
                }
                this._hashtable.CopyValues(array, arrayIndex);
            }

            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            public virtual IEnumerator GetEnumerator()
            {
                return new Hashtable.HashtableEnumerator(this._hashtable, 2);
            }

            public virtual int Count
            {
                get
                {
                    return this._hashtable.count;
                }
            }

            public virtual bool IsSynchronized
            {
                get
                {
                    return this._hashtable.IsSynchronized;
                }
            }

            public virtual object SyncRoot
            {
                get
                {
                    return this._hashtable.SyncRoot;
                }
            }
        }
    }
}

