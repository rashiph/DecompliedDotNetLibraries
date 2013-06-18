namespace Microsoft.JScript
{
    using System;
    using System.Collections;

    internal sealed class SimpleHashtableEnumerator : IDictionaryEnumerator, IEnumerator
    {
        private int count;
        private HashtableEntry currentEntry;
        private int index;
        private HashtableEntry[] table;

        internal SimpleHashtableEnumerator(HashtableEntry[] table)
        {
            this.table = table;
            this.count = table.Length;
            this.index = -1;
            this.currentEntry = null;
        }

        public bool MoveNext()
        {
            HashtableEntry[] table = this.table;
            if (this.currentEntry != null)
            {
                this.currentEntry = this.currentEntry.next;
                if (this.currentEntry != null)
                {
                    return true;
                }
            }
            int index = ++this.index;
            int count = this.count;
            while (index < count)
            {
                if (table[index] != null)
                {
                    this.index = index;
                    this.currentEntry = table[index];
                    return true;
                }
                index++;
            }
            return false;
        }

        public void Reset()
        {
            this.index = -1;
            this.currentEntry = null;
        }

        public object Current
        {
            get
            {
                return this.Key;
            }
        }

        public DictionaryEntry Entry
        {
            get
            {
                return new DictionaryEntry(this.Key, this.Value);
            }
        }

        public object Key
        {
            get
            {
                return this.currentEntry.key;
            }
        }

        public object Value
        {
            get
            {
                return this.currentEntry.value;
            }
        }
    }
}

