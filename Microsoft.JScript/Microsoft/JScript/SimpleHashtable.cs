namespace Microsoft.JScript
{
    using System;
    using System.Collections;
    using System.Reflection;

    public sealed class SimpleHashtable
    {
        internal int count;
        private HashtableEntry[] table;
        private uint threshold;

        public SimpleHashtable(uint threshold)
        {
            if (threshold < 8)
            {
                threshold = 8;
            }
            this.table = new HashtableEntry[(threshold * 2) - 1];
            this.count = 0;
            this.threshold = threshold;
        }

        public IDictionaryEnumerator GetEnumerator()
        {
            return new SimpleHashtableEnumerator(this.table);
        }

        private HashtableEntry GetHashtableEntry(object key, uint hashCode)
        {
            int index = (int) (hashCode % this.table.Length);
            HashtableEntry entry = this.table[index];
            if (entry != null)
            {
                HashtableEntry entry2;
                if (entry.key == key)
                {
                    return entry;
                }
                for (entry2 = entry.next; entry2 != null; entry2 = entry2.next)
                {
                    if (entry2.key == key)
                    {
                        return entry2;
                    }
                }
                if ((entry.hashCode == hashCode) && entry.key.Equals(key))
                {
                    entry.key = key;
                    return entry;
                }
                for (entry2 = entry.next; entry2 != null; entry2 = entry2.next)
                {
                    if ((entry2.hashCode == hashCode) && entry2.key.Equals(key))
                    {
                        entry2.key = key;
                        return entry2;
                    }
                }
            }
            return null;
        }

        internal object IgnoreCaseGet(string name)
        {
            uint index = 0;
            uint length = (uint) this.table.Length;
            while (index < length)
            {
                for (HashtableEntry entry = this.table[index]; entry != null; entry = entry.next)
                {
                    if (string.Compare((string) entry.key, name, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return entry.value;
                    }
                }
                index++;
            }
            return null;
        }

        private void Rehash()
        {
            HashtableEntry[] table = this.table;
            uint num = this.threshold = (uint) (table.Length + 1);
            uint num2 = (num * 2) - 1;
            HashtableEntry[] entryArray2 = this.table = new HashtableEntry[num2];
            uint index = num - 1;
            while (index-- > 0)
            {
                HashtableEntry next = table[index];
                while (next != null)
                {
                    HashtableEntry entry2 = next;
                    next = next.next;
                    int num4 = (int) (entry2.hashCode % num2);
                    entry2.next = entryArray2[num4];
                    entryArray2[num4] = entry2;
                }
            }
        }

        public void Remove(object key)
        {
            uint hashCode = (uint) key.GetHashCode();
            int index = (int) (hashCode % this.table.Length);
            HashtableEntry next = this.table[index];
            this.count--;
            while (((next != null) && (next.hashCode == hashCode)) && ((next.key == key) || next.key.Equals(key)))
            {
                next = next.next;
            }
            this.table[index] = next;
            while (next != null)
            {
                HashtableEntry entry2 = next.next;
                while (((entry2 != null) && (entry2.hashCode == hashCode)) && ((entry2.key == key) || entry2.key.Equals(key)))
                {
                    entry2 = entry2.next;
                }
                next.next = entry2;
                next = entry2;
            }
        }

        public object this[object key]
        {
            get
            {
                HashtableEntry hashtableEntry = this.GetHashtableEntry(key, (uint) key.GetHashCode());
                if (hashtableEntry == null)
                {
                    return null;
                }
                return hashtableEntry.value;
            }
            set
            {
                uint hashCode = (uint) key.GetHashCode();
                HashtableEntry hashtableEntry = this.GetHashtableEntry(key, hashCode);
                if (hashtableEntry != null)
                {
                    hashtableEntry.value = value;
                }
                else
                {
                    if (++this.count >= this.threshold)
                    {
                        this.Rehash();
                    }
                    int index = (int) (hashCode % this.table.Length);
                    this.table[index] = new HashtableEntry(key, value, hashCode, this.table[index]);
                }
            }
        }
    }
}

