namespace System.Collections
{
    using System;

    internal class ICollectionContract : ICollection, IEnumerable
    {
        void ICollection.CopyTo(Array array, int index)
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
    }
}

