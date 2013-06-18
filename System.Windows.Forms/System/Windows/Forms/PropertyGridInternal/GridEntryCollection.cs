namespace System.Windows.Forms.PropertyGridInternal
{
    using System;
    using System.Windows.Forms;

    internal class GridEntryCollection : GridItemCollection
    {
        private GridEntry owner;

        public GridEntryCollection(GridEntry owner, GridEntry[] entries) : base(entries)
        {
            this.owner = owner;
        }

        public void AddRange(GridEntry[] value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (base.entries != null)
            {
                GridEntry[] array = new GridEntry[base.entries.Length + value.Length];
                base.entries.CopyTo(array, 0);
                value.CopyTo(array, base.entries.Length);
                base.entries = array;
            }
            else
            {
                base.entries = (GridEntry[]) value.Clone();
            }
        }

        public void Clear()
        {
            base.entries = new GridEntry[0];
        }

        public void CopyTo(Array dest, int index)
        {
            base.entries.CopyTo(dest, index);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if ((disposing && (this.owner != null)) && (base.entries != null))
            {
                for (int i = 0; i < base.entries.Length; i++)
                {
                    if (base.entries[i] != null)
                    {
                        ((GridEntry) base.entries[i]).Dispose();
                        base.entries[i] = null;
                    }
                }
                base.entries = new GridEntry[0];
            }
        }

        ~GridEntryCollection()
        {
            this.Dispose(false);
        }

        internal GridEntry GetEntry(int index)
        {
            return (GridEntry) base.entries[index];
        }

        internal int GetEntry(GridEntry child)
        {
            return Array.IndexOf<GridItem>(base.entries, child);
        }
    }
}

