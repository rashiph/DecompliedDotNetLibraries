namespace System.Web.Util
{
    using System;
    using System.Collections;

    internal class EmptyCollection : ICollection, IEnumerable, IEnumerator
    {
        private static EmptyCollection s_theEmptyCollection = new EmptyCollection();

        private EmptyCollection()
        {
        }

        public void CopyTo(Array array, int index)
        {
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this;
        }

        bool IEnumerator.MoveNext()
        {
            return false;
        }

        void IEnumerator.Reset()
        {
        }

        public int Count
        {
            get
            {
                return 0;
            }
        }

        internal static EmptyCollection Instance
        {
            get
            {
                return s_theEmptyCollection;
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

        object IEnumerator.Current
        {
            get
            {
                return null;
            }
        }
    }
}

