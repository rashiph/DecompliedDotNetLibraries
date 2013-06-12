namespace System.Data
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Globalization;

    public class InternalDataCollectionBase : ICollection, IEnumerable
    {
        internal static CollectionChangeEventArgs RefreshEventArgs = new CollectionChangeEventArgs(CollectionChangeAction.Refresh, null);

        public virtual void CopyTo(Array ar, int index)
        {
            this.List.CopyTo(ar, index);
        }

        public virtual IEnumerator GetEnumerator()
        {
            return this.List.GetEnumerator();
        }

        internal int NamesEqual(string s1, string s2, bool fCaseSensitive, CultureInfo locale)
        {
            if (fCaseSensitive)
            {
                if (string.Compare(s1, s2, false, locale) == 0)
                {
                    return 1;
                }
                return 0;
            }
            if (locale.CompareInfo.Compare(s1, s2, CompareOptions.IgnoreWidth | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreCase) != 0)
            {
                return 0;
            }
            if (string.Compare(s1, s2, false, locale) == 0)
            {
                return 1;
            }
            return -1;
        }

        [Browsable(false)]
        public virtual int Count
        {
            get
            {
                return this.List.Count;
            }
        }

        [Browsable(false)]
        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        [Browsable(false)]
        public bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        protected virtual ArrayList List
        {
            get
            {
                return null;
            }
        }

        [Browsable(false)]
        public object SyncRoot
        {
            get
            {
                return this;
            }
        }
    }
}

