namespace System.DirectoryServices.Protocols
{
    using System;
    using System.Collections;
    using System.Reflection;

    public class SearchResultEntryCollection : ReadOnlyCollectionBase
    {
        internal SearchResultEntryCollection()
        {
        }

        internal int Add(SearchResultEntry entry)
        {
            return base.InnerList.Add(entry);
        }

        internal void Clear()
        {
            base.InnerList.Clear();
        }

        public bool Contains(SearchResultEntry value)
        {
            return base.InnerList.Contains(value);
        }

        public void CopyTo(SearchResultEntry[] values, int index)
        {
            base.InnerList.CopyTo(values, index);
        }

        public int IndexOf(SearchResultEntry value)
        {
            return base.InnerList.IndexOf(value);
        }

        public SearchResultEntry this[int index]
        {
            get
            {
                return (SearchResultEntry) base.InnerList[index];
            }
        }
    }
}

