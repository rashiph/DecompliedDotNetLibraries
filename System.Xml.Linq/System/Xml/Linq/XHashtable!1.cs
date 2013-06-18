namespace System.Xml.Linq
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal sealed class XHashtable<TValue>
    {
        private const int StartingHash = 0x15051505;
        private XHashtableState<TValue> state;

        public XHashtable(ExtractKeyDelegate<TValue> extractKey, int capacity)
        {
            this.state = new XHashtableState<TValue>(extractKey, capacity);
        }

        public TValue Add(TValue value)
        {
            TValue local;
        Label_0000:
            if (this.state.TryAdd(value, out local))
            {
                return local;
            }
            lock (((XHashtable<TValue>) this))
            {
                XHashtableState<TValue> state = this.state.Resize();
                Thread.MemoryBarrier();
                this.state = state;
                goto Label_0000;
            }
        }

        public bool TryGetValue(string key, int index, int count, out TValue value)
        {
            return this.state.TryGetValue(key, index, count, out value);
        }

        public delegate string ExtractKeyDelegate(TValue value);

        private sealed class XHashtableState
        {
            private int[] buckets;
            private const int EndOfList = 0;
            private Entry<TValue>[] entries;
            private XHashtable<TValue>.ExtractKeyDelegate extractKey;
            private const int FullList = -1;
            private int numEntries;

            public XHashtableState(XHashtable<TValue>.ExtractKeyDelegate extractKey, int capacity)
            {
                this.buckets = new int[capacity];
                this.entries = new Entry<TValue>[capacity];
                this.extractKey = extractKey;
            }

            private static int ComputeHashCode(string key, int index, int count)
            {
                int num = 0x15051505;
                int num2 = index + count;
                for (int i = index; i < num2; i++)
                {
                    num += (num << 7) ^ key[i];
                }
                num -= num >> 0x11;
                num -= num >> 11;
                num -= num >> 5;
                return (num & 0x7fffffff);
            }

            private bool FindEntry(int hashCode, string key, int index, int count, ref int entryIndex)
            {
                int next;
                int num = entryIndex;
                if (num == 0)
                {
                    next = this.buckets[hashCode & (this.buckets.Length - 1)];
                }
                else
                {
                    next = num;
                }
                while (next > 0)
                {
                    if (this.entries[next].HashCode == hashCode)
                    {
                        string strB = this.extractKey(this.entries[next].Value);
                        if (strB == null)
                        {
                            if (this.entries[next].Next <= 0)
                            {
                                goto Label_00E5;
                            }
                            this.entries[next].Value = default(TValue);
                            next = this.entries[next].Next;
                            if (num == 0)
                            {
                                this.buckets[hashCode & (this.buckets.Length - 1)] = next;
                            }
                            else
                            {
                                this.entries[num].Next = next;
                            }
                            continue;
                        }
                        if ((count == strB.Length) && (string.CompareOrdinal(key, index, strB, 0, count) == 0))
                        {
                            entryIndex = next;
                            return true;
                        }
                    }
                Label_00E5:
                    num = next;
                    next = this.entries[next].Next;
                }
                entryIndex = num;
                return false;
            }

            public XHashtable<TValue>.XHashtableState Resize()
            {
                if (this.numEntries < this.buckets.Length)
                {
                    return (XHashtable<TValue>.XHashtableState) this;
                }
                int capacity = 0;
                for (int i = 0; i < this.buckets.Length; i++)
                {
                    int index = this.buckets[i];
                    if (index == 0)
                    {
                        index = Interlocked.CompareExchange(ref this.buckets[i], -1, 0);
                    }
                    while (index > 0)
                    {
                        if (this.extractKey(this.entries[index].Value) != null)
                        {
                            capacity++;
                        }
                        if (this.entries[index].Next == 0)
                        {
                            index = Interlocked.CompareExchange(ref this.entries[index].Next, -1, 0);
                        }
                        else
                        {
                            index = this.entries[index].Next;
                        }
                    }
                }
                if (capacity < (this.buckets.Length / 2))
                {
                    capacity = this.buckets.Length;
                }
                else
                {
                    capacity = this.buckets.Length * 2;
                    if (capacity < 0)
                    {
                        throw new OverflowException();
                    }
                }
                XHashtable<TValue>.XHashtableState state = new XHashtable<TValue>.XHashtableState(this.extractKey, capacity);
                for (int j = 0; j < this.buckets.Length; j++)
                {
                    for (int k = this.buckets[j]; k > 0; k = this.entries[k].Next)
                    {
                        TValue local;
                        state.TryAdd(this.entries[k].Value, out local);
                    }
                }
                return state;
            }

            public bool TryAdd(TValue value, out TValue newValue)
            {
                newValue = value;
                string key = this.extractKey(value);
                if (key != null)
                {
                    int hashCode = XHashtable<TValue>.XHashtableState.ComputeHashCode(key, 0, key.Length);
                    int index = Interlocked.Increment(ref this.numEntries);
                    if ((index < 0) || (index >= this.buckets.Length))
                    {
                        return false;
                    }
                    this.entries[index].Value = value;
                    this.entries[index].HashCode = hashCode;
                    Thread.MemoryBarrier();
                    int entryIndex = 0;
                    while (!this.FindEntry(hashCode, key, 0, key.Length, ref entryIndex))
                    {
                        if (entryIndex == 0)
                        {
                            entryIndex = Interlocked.CompareExchange(ref this.buckets[hashCode & (this.buckets.Length - 1)], index, 0);
                        }
                        else
                        {
                            entryIndex = Interlocked.CompareExchange(ref this.entries[entryIndex].Next, index, 0);
                        }
                        if (entryIndex <= 0)
                        {
                            return (entryIndex == 0);
                        }
                    }
                    newValue = this.entries[entryIndex].Value;
                }
                return true;
            }

            public bool TryGetValue(string key, int index, int count, out TValue value)
            {
                int hashCode = XHashtable<TValue>.XHashtableState.ComputeHashCode(key, index, count);
                int entryIndex = 0;
                if (this.FindEntry(hashCode, key, index, count, ref entryIndex))
                {
                    value = this.entries[entryIndex].Value;
                    return true;
                }
                value = default(TValue);
                return false;
            }

            [StructLayout(LayoutKind.Sequential)]
            private struct Entry
            {
                public TValue Value;
                public int HashCode;
                public int Next;
            }
        }
    }
}

