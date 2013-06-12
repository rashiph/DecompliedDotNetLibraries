namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal class HashLookup<TKey, TValue>
    {
        private int[] buckets;
        private IEqualityComparer<TKey> comparer;
        private int count;
        private int freeList;
        private Slot<TKey, TValue>[] slots;

        internal HashLookup() : this(null)
        {
        }

        internal HashLookup(IEqualityComparer<TKey> comparer)
        {
            this.comparer = comparer;
            this.buckets = new int[7];
            this.slots = new Slot<TKey, TValue>[7];
            this.freeList = -1;
        }

        internal bool Add(TKey key, TValue value)
        {
            return !this.Find(key, true, false, ref value);
        }

        private bool AreKeysEqual(TKey key1, TKey key2)
        {
            if (this.comparer != null)
            {
                return this.comparer.Equals(key1, key2);
            }
            return (((key1 == null) && (key2 == null)) || ((key1 != null) && key1.Equals(key2)));
        }

        private bool Find(TKey key, bool add, bool set, ref TValue value)
        {
            int keyHashCode = this.GetKeyHashCode(key);
            for (int i = this.buckets[keyHashCode % this.buckets.Length] - 1; i >= 0; i = this.slots[i].next)
            {
                if ((this.slots[i].hashCode == keyHashCode) && this.AreKeysEqual(this.slots[i].key, key))
                {
                    if (set)
                    {
                        this.slots[i].value = value;
                        return true;
                    }
                    value = this.slots[i].value;
                    return true;
                }
            }
            if (add)
            {
                int freeList;
                if (this.freeList >= 0)
                {
                    freeList = this.freeList;
                    this.freeList = this.slots[freeList].next;
                }
                else
                {
                    if (this.count == this.slots.Length)
                    {
                        this.Resize();
                    }
                    freeList = this.count;
                    this.count++;
                }
                int index = keyHashCode % this.buckets.Length;
                this.slots[freeList].hashCode = keyHashCode;
                this.slots[freeList].key = key;
                this.slots[freeList].value = value;
                this.slots[freeList].next = this.buckets[index] - 1;
                this.buckets[index] = freeList + 1;
            }
            return false;
        }

        private int GetKeyHashCode(TKey key)
        {
            return (0x7fffffff & ((this.comparer == null) ? ((key == null) ? 0 : key.GetHashCode()) : this.comparer.GetHashCode(key)));
        }

        internal bool Remove(TKey key)
        {
            int keyHashCode = this.GetKeyHashCode(key);
            int index = keyHashCode % this.buckets.Length;
            int num3 = -1;
            for (int i = this.buckets[index] - 1; i >= 0; i = this.slots[i].next)
            {
                if ((this.slots[i].hashCode == keyHashCode) && this.AreKeysEqual(this.slots[i].key, key))
                {
                    if (num3 < 0)
                    {
                        this.buckets[index] = this.slots[i].next + 1;
                    }
                    else
                    {
                        this.slots[num3].next = this.slots[i].next;
                    }
                    this.slots[i].hashCode = -1;
                    this.slots[i].key = default(TKey);
                    this.slots[i].value = default(TValue);
                    this.slots[i].next = this.freeList;
                    this.freeList = i;
                    return true;
                }
                num3 = i;
            }
            return false;
        }

        private void Resize()
        {
            int num = (this.count * 2) + 1;
            int[] numArray = new int[num];
            Slot<TKey, TValue>[] destinationArray = new Slot<TKey, TValue>[num];
            Array.Copy(this.slots, 0, destinationArray, 0, this.count);
            for (int i = 0; i < this.count; i++)
            {
                int index = destinationArray[i].hashCode % num;
                destinationArray[i].next = numArray[index] - 1;
                numArray[index] = i + 1;
            }
            this.buckets = numArray;
            this.slots = destinationArray;
        }

        internal bool TryGetValue(TKey key, ref TValue value)
        {
            return this.Find(key, false, false, ref value);
        }

        internal int Count
        {
            get
            {
                return this.count;
            }
        }

        internal TValue this[TKey key]
        {
            set
            {
                TValue local = value;
                this.Find(key, false, true, ref local);
            }
        }

        internal KeyValuePair<TKey, TValue> this[int index]
        {
            get
            {
                return new KeyValuePair<TKey, TValue>(this.slots[index].key, this.slots[index].value);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct Slot
        {
            internal int hashCode;
            internal TKey key;
            internal TValue value;
            internal int next;
        }
    }
}

