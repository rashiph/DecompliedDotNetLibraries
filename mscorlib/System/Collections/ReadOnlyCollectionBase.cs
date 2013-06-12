namespace System.Collections
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true)]
    public abstract class ReadOnlyCollectionBase : ICollection, IEnumerable
    {
        private ArrayList list;

        protected ReadOnlyCollectionBase()
        {
        }

        public virtual IEnumerator GetEnumerator()
        {
            return this.InnerList.GetEnumerator();
        }

        void ICollection.CopyTo(Array array, int index)
        {
            this.InnerList.CopyTo(array, index);
        }

        public virtual int Count
        {
            get
            {
                return this.InnerList.Count;
            }
        }

        protected ArrayList InnerList
        {
            get
            {
                if (this.list == null)
                {
                    this.list = new ArrayList();
                }
                return this.list;
            }
        }

        bool ICollection.IsSynchronized
        {
            get
            {
                return this.InnerList.IsSynchronized;
            }
        }

        object ICollection.SyncRoot
        {
            get
            {
                return this.InnerList.SyncRoot;
            }
        }
    }
}

