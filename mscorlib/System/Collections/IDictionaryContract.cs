namespace System.Collections
{
    using System;

    internal class IDictionaryContract : IDictionary, ICollection, IEnumerable
    {
        void ICollection.CopyTo(Array array, int index)
        {
        }

        void IDictionary.Add(object key, object value)
        {
        }

        void IDictionary.Clear()
        {
        }

        bool IDictionary.Contains(object key)
        {
            return false;
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return null;
        }

        void IDictionary.Remove(object key)
        {
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return null;
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

        bool IDictionary.IsFixedSize
        {
            get
            {
                return false;
            }
        }

        bool IDictionary.IsReadOnly
        {
            get
            {
                return false;
            }
        }

        object IDictionary.this[object key]
        {
            get
            {
                return null;
            }
            set
            {
            }
        }

        ICollection IDictionary.Keys
        {
            get
            {
                return null;
            }
        }

        ICollection IDictionary.Values
        {
            get
            {
                return null;
            }
        }
    }
}

