namespace System.ComponentModel
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, SharedState=true)]
    public class ListSortDescriptionCollection : IList, ICollection, IEnumerable
    {
        private ArrayList sorts;

        public ListSortDescriptionCollection()
        {
            this.sorts = new ArrayList();
        }

        public ListSortDescriptionCollection(ListSortDescription[] sorts)
        {
            this.sorts = new ArrayList();
            if (sorts != null)
            {
                for (int i = 0; i < sorts.Length; i++)
                {
                    this.sorts.Add(sorts[i]);
                }
            }
        }

        public bool Contains(object value)
        {
            return this.sorts.Contains(value);
        }

        public void CopyTo(Array array, int index)
        {
            this.sorts.CopyTo(array, index);
        }

        public int IndexOf(object value)
        {
            return this.sorts.IndexOf(value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.sorts.GetEnumerator();
        }

        int IList.Add(object value)
        {
            throw new InvalidOperationException(SR.GetString("CantModifyListSortDescriptionCollection"));
        }

        void IList.Clear()
        {
            throw new InvalidOperationException(SR.GetString("CantModifyListSortDescriptionCollection"));
        }

        void IList.Insert(int index, object value)
        {
            throw new InvalidOperationException(SR.GetString("CantModifyListSortDescriptionCollection"));
        }

        void IList.Remove(object value)
        {
            throw new InvalidOperationException(SR.GetString("CantModifyListSortDescriptionCollection"));
        }

        void IList.RemoveAt(int index)
        {
            throw new InvalidOperationException(SR.GetString("CantModifyListSortDescriptionCollection"));
        }

        public int Count
        {
            get
            {
                return this.sorts.Count;
            }
        }

        public ListSortDescription this[int index]
        {
            get
            {
                return (ListSortDescription) this.sorts[index];
            }
            set
            {
                throw new InvalidOperationException(SR.GetString("CantModifyListSortDescriptionCollection"));
            }
        }

        bool ICollection.IsSynchronized
        {
            get
            {
                return true;
            }
        }

        object ICollection.SyncRoot
        {
            get
            {
                return this;
            }
        }

        bool IList.IsFixedSize
        {
            get
            {
                return true;
            }
        }

        bool IList.IsReadOnly
        {
            get
            {
                return true;
            }
        }

        object IList.this[int index]
        {
            get
            {
                return this[index];
            }
            set
            {
                throw new InvalidOperationException(SR.GetString("CantModifyListSortDescriptionCollection"));
            }
        }
    }
}

