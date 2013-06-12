namespace System.Runtime.CompilerServices
{
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComVisible(false)]
    public sealed class ConditionalWeakTable<TKey, TValue> where TKey: class where TValue: class
    {
        private int[] _buckets;
        private Entry<TKey, TValue>[] _entries;
        private int _freeList;
        private const int _initialCapacity = 5;
        private bool _invalid;
        private object _lock;

        [SecuritySafeCritical]
        public ConditionalWeakTable()
        {
            this._buckets = new int[0];
            this._entries = new Entry<TKey, TValue>[0];
            this._freeList = -1;
            this._lock = new object();
            this.Resize();
        }

        [SecuritySafeCritical]
        public void Add(TKey key, TValue value)
        {
            if (key == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);
            }
            lock (this._lock)
            {
                this.VerifyIntegrity();
                this._invalid = true;
                if (this.FindEntry(key) != -1)
                {
                    this._invalid = false;
                    ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_AddingDuplicate);
                }
                this.CreateEntry(key, value);
                this._invalid = false;
            }
        }

        [SecurityCritical]
        private void CreateEntry(TKey key, TValue value)
        {
            if (this._freeList == -1)
            {
                this.Resize();
            }
            int num = RuntimeHelpers.GetHashCode(key) & 0x7fffffff;
            int index = num % this._buckets.Length;
            int num3 = this._freeList;
            this._freeList = this._entries[num3].next;
            this._entries[num3].hashCode = num;
            this._entries[num3].depHnd = new DependentHandle(key, value);
            this._entries[num3].next = this._buckets[index];
            this._buckets[index] = num3;
        }

        [SecuritySafeCritical]
        ~ConditionalWeakTable()
        {
            if (!Environment.HasShutdownStarted && (this._lock != null))
            {
                lock (this._lock)
                {
                    if (!this._invalid)
                    {
                        Entry<TKey, TValue>[] entryArray = this._entries;
                        this._invalid = true;
                        this._entries = null;
                        this._buckets = null;
                        for (int i = 0; i < entryArray.Length; i++)
                        {
                            entryArray[i].depHnd.Free();
                        }
                    }
                }
            }
        }

        [SecurityCritical]
        private int FindEntry(TKey key)
        {
            int num = RuntimeHelpers.GetHashCode(key) & 0x7fffffff;
            for (int i = this._buckets[num % this._buckets.Length]; i != -1; i = this._entries[i].next)
            {
                if ((this._entries[i].hashCode == num) && (this._entries[i].depHnd.GetPrimary() == key))
                {
                    return i;
                }
            }
            return -1;
        }

        public TValue GetOrCreateValue(TKey key)
        {
            return this.GetValue(key, k => Activator.CreateInstance<TValue>());
        }

        [SecuritySafeCritical]
        public TValue GetValue(TKey key, CreateValueCallback<TKey, TValue> createValueCallback)
        {
            TValue local;
            if (createValueCallback == null)
            {
                throw new ArgumentNullException("createValueCallback");
            }
            if (this.TryGetValue(key, out local))
            {
                return local;
            }
            TValue local2 = createValueCallback(key);
            lock (this._lock)
            {
                this.VerifyIntegrity();
                this._invalid = true;
                if (this.TryGetValueWorker(key, out local))
                {
                    this._invalid = false;
                    return local;
                }
                this.CreateEntry(key, local2);
                this._invalid = false;
                return local2;
            }
        }

        [SecuritySafeCritical]
        public bool Remove(TKey key)
        {
            if (key == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);
            }
            lock (this._lock)
            {
                this.VerifyIntegrity();
                this._invalid = true;
                int num = RuntimeHelpers.GetHashCode(key) & 0x7fffffff;
                int index = num % this._buckets.Length;
                int num3 = -1;
                for (int i = this._buckets[index]; i != -1; i = this._entries[i].next)
                {
                    if ((this._entries[i].hashCode == num) && (this._entries[i].depHnd.GetPrimary() == key))
                    {
                        if (num3 == -1)
                        {
                            this._buckets[index] = this._entries[i].next;
                        }
                        else
                        {
                            this._entries[num3].next = this._entries[i].next;
                        }
                        this._entries[i].depHnd.Free();
                        this._entries[i].next = this._freeList;
                        this._freeList = i;
                        this._invalid = false;
                        return true;
                    }
                    num3 = i;
                }
                this._invalid = false;
                return false;
            }
        }

        [SecurityCritical]
        private void Resize()
        {
            int num2;
            int length = this._buckets.Length;
            bool flag = false;
            for (num2 = 0; num2 < this._entries.Length; num2++)
            {
                if (this._entries[num2].depHnd.IsAllocated && (this._entries[num2].depHnd.GetPrimary() == null))
                {
                    flag = true;
                    break;
                }
            }
            if (!flag)
            {
                length = HashHelpers.GetPrime((this._buckets.Length == 0) ? 6 : (this._buckets.Length * 2));
            }
            int num3 = -1;
            int[] numArray = new int[length];
            for (int i = 0; i < length; i++)
            {
                numArray[i] = -1;
            }
            Entry<TKey, TValue>[] entryArray = new Entry<TKey, TValue>[length];
            num2 = 0;
            while (num2 < this._entries.Length)
            {
                DependentHandle depHnd = this._entries[num2].depHnd;
                if (depHnd.IsAllocated && (depHnd.GetPrimary() != null))
                {
                    int index = this._entries[num2].hashCode % length;
                    entryArray[num2].depHnd = depHnd;
                    entryArray[num2].hashCode = this._entries[num2].hashCode;
                    entryArray[num2].next = numArray[index];
                    numArray[index] = num2;
                }
                else
                {
                    this._entries[num2].depHnd.Free();
                    entryArray[num2].depHnd = new DependentHandle();
                    entryArray[num2].next = num3;
                    num3 = num2;
                }
                num2++;
            }
            while (num2 != entryArray.Length)
            {
                entryArray[num2].depHnd = new DependentHandle();
                entryArray[num2].next = num3;
                num3 = num2;
                num2++;
            }
            this._buckets = numArray;
            this._entries = entryArray;
            this._freeList = num3;
        }

        [SecuritySafeCritical]
        public bool TryGetValue(TKey key, out TValue value)
        {
            if (key == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);
            }
            lock (this._lock)
            {
                this.VerifyIntegrity();
                return this.TryGetValueWorker(key, out value);
            }
        }

        [SecurityCritical]
        private bool TryGetValueWorker(TKey key, out TValue value)
        {
            int index = this.FindEntry(key);
            if (index != -1)
            {
                object primary = null;
                object secondary = null;
                this._entries[index].depHnd.GetPrimaryAndSecondary(out primary, out secondary);
                if (primary != null)
                {
                    value = (TValue) secondary;
                    return true;
                }
            }
            value = default(TValue);
            return false;
        }

        private void VerifyIntegrity()
        {
            if (this._invalid)
            {
                throw new InvalidOperationException(Environment.GetResourceString("CollectionCorrupted"));
            }
        }

        public delegate TValue CreateValueCallback(TKey key);

        [StructLayout(LayoutKind.Sequential)]
        private struct Entry
        {
            public DependentHandle depHnd;
            public int hashCode;
            public int next;
        }
    }
}

