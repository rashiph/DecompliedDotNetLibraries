namespace System.Xml
{
    using System;

    public class NameTable : XmlNameTable
    {
        private int count;
        private Entry[] entries;
        private int hashCodeRandomizer;
        private int mask = 0x1f;

        public NameTable()
        {
            this.entries = new Entry[this.mask + 1];
            this.hashCodeRandomizer = Environment.TickCount;
        }

        public override string Add(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            int length = key.Length;
            if (length == 0)
            {
                return string.Empty;
            }
            int hashCode = length + this.hashCodeRandomizer;
            for (int i = 0; i < key.Length; i++)
            {
                hashCode += (hashCode << 7) ^ key[i];
            }
            hashCode -= hashCode >> 0x11;
            hashCode -= hashCode >> 11;
            hashCode -= hashCode >> 5;
            for (Entry entry = this.entries[hashCode & this.mask]; entry != null; entry = entry.next)
            {
                if ((entry.hashCode == hashCode) && entry.str.Equals(key))
                {
                    return entry.str;
                }
            }
            return this.AddEntry(key, hashCode);
        }

        public override string Add(char[] key, int start, int len)
        {
            if (len == 0)
            {
                return string.Empty;
            }
            int hashCode = len + this.hashCodeRandomizer;
            hashCode += (hashCode << 7) ^ key[start];
            int num2 = start + len;
            for (int i = start + 1; i < num2; i++)
            {
                hashCode += (hashCode << 7) ^ key[i];
            }
            hashCode -= hashCode >> 0x11;
            hashCode -= hashCode >> 11;
            hashCode -= hashCode >> 5;
            for (Entry entry = this.entries[hashCode & this.mask]; entry != null; entry = entry.next)
            {
                if ((entry.hashCode == hashCode) && TextEquals(entry.str, key, start, len))
                {
                    return entry.str;
                }
            }
            return this.AddEntry(new string(key, start, len), hashCode);
        }

        private string AddEntry(string str, int hashCode)
        {
            int index = hashCode & this.mask;
            Entry entry = new Entry(str, hashCode, this.entries[index]);
            this.entries[index] = entry;
            if (this.count++ == this.mask)
            {
                this.Grow();
            }
            return entry.str;
        }

        public override string Get(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (value.Length == 0)
            {
                return string.Empty;
            }
            int num2 = value.Length + this.hashCodeRandomizer;
            for (int i = 0; i < value.Length; i++)
            {
                num2 += (num2 << 7) ^ value[i];
            }
            num2 -= num2 >> 0x11;
            num2 -= num2 >> 11;
            num2 -= num2 >> 5;
            for (Entry entry = this.entries[num2 & this.mask]; entry != null; entry = entry.next)
            {
                if ((entry.hashCode == num2) && entry.str.Equals(value))
                {
                    return entry.str;
                }
            }
            return null;
        }

        public override string Get(char[] key, int start, int len)
        {
            if (len == 0)
            {
                return string.Empty;
            }
            int num = len + this.hashCodeRandomizer;
            num += (num << 7) ^ key[start];
            int num2 = start + len;
            for (int i = start + 1; i < num2; i++)
            {
                num += (num << 7) ^ key[i];
            }
            num -= num >> 0x11;
            num -= num >> 11;
            num -= num >> 5;
            for (Entry entry = this.entries[num & this.mask]; entry != null; entry = entry.next)
            {
                if ((entry.hashCode == num) && TextEquals(entry.str, key, start, len))
                {
                    return entry.str;
                }
            }
            return null;
        }

        private void Grow()
        {
            int num = (this.mask * 2) + 1;
            Entry[] entries = this.entries;
            Entry[] entryArray2 = new Entry[num + 1];
            for (int i = 0; i < entries.Length; i++)
            {
                Entry next;
                for (Entry entry = entries[i]; entry != null; entry = next)
                {
                    int index = entry.hashCode & num;
                    next = entry.next;
                    entry.next = entryArray2[index];
                    entryArray2[index] = entry;
                }
            }
            this.entries = entryArray2;
            this.mask = num;
        }

        private static bool TextEquals(string str1, char[] str2, int str2Start, int str2Length)
        {
            if (str1.Length != str2Length)
            {
                return false;
            }
            for (int i = 0; i < str1.Length; i++)
            {
                if (str1[i] != str2[str2Start + i])
                {
                    return false;
                }
            }
            return true;
        }

        private class Entry
        {
            internal int hashCode;
            internal NameTable.Entry next;
            internal string str;

            internal Entry(string str, int hashCode, NameTable.Entry next)
            {
                this.str = str;
                this.hashCode = hashCode;
                this.next = next;
            }
        }
    }
}

