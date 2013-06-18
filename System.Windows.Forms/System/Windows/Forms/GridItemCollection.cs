namespace System.Windows.Forms
{
    using System;
    using System.Collections;
    using System.Reflection;

    public class GridItemCollection : ICollection, IEnumerable
    {
        public static GridItemCollection Empty = new GridItemCollection(new GridItem[0]);
        internal GridItem[] entries;

        internal GridItemCollection(GridItem[] entries)
        {
            if (entries == null)
            {
                this.entries = new GridItem[0];
            }
            else
            {
                this.entries = entries;
            }
        }

        public IEnumerator GetEnumerator()
        {
            return this.entries.GetEnumerator();
        }

        void ICollection.CopyTo(Array dest, int index)
        {
            if (this.entries.Length > 0)
            {
                Array.Copy(this.entries, 0, dest, index, this.entries.Length);
            }
        }

        public int Count
        {
            get
            {
                return this.entries.Length;
            }
        }

        public GridItem this[int index]
        {
            get
            {
                return this.entries[index];
            }
        }

        public GridItem this[string label]
        {
            get
            {
                foreach (GridItem item in this.entries)
                {
                    if (item.Label == label)
                    {
                        return item;
                    }
                }
                return null;
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
    }
}

