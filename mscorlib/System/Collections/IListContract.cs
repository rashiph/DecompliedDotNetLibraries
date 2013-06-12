namespace System.Collections
{
    using System;

    internal abstract class IListContract : IList, ICollection, IEnumerable
    {
        protected IListContract()
        {
        }

        void ICollection.CopyTo(Array array, int startIndex)
        {
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return null;
        }

        int IList.Add(object value)
        {
            return 0;
        }

        void IList.Clear()
        {
        }

        bool IList.Contains(object value)
        {
            return false;
        }

        int IList.IndexOf(object value)
        {
            return 0;
        }

        void IList.Insert(int index, object value)
        {
        }

        void IList.Remove(object value)
        {
        }

        void IList.RemoveAt(int index)
        {
        }

        int ICollection.Count
        {
            get
            {
                return 0;
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
                return null;
            }
        }

        bool IList.IsFixedSize
        {
            get
            {
                return false;
            }
        }

        bool IList.IsReadOnly
        {
            get
            {
                return false;
            }
        }

        object IList.this[int index]
        {
            get
            {
                return 0;
            }
            set
            {
            }
        }
    }
}

