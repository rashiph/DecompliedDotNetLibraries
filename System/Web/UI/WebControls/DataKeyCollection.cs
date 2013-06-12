namespace System.Web.UI.WebControls
{
    using System;
    using System.Collections;
    using System.Reflection;

    public sealed class DataKeyCollection : ICollection, IEnumerable
    {
        private ArrayList keys;

        public DataKeyCollection(ArrayList keys)
        {
            this.keys = keys;
        }

        public void CopyTo(Array array, int index)
        {
            IEnumerator enumerator = this.GetEnumerator();
            while (enumerator.MoveNext())
            {
                array.SetValue(enumerator.Current, index++);
            }
        }

        public IEnumerator GetEnumerator()
        {
            return this.keys.GetEnumerator();
        }

        public int Count
        {
            get
            {
                return this.keys.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        public object this[int index]
        {
            get
            {
                return this.keys[index];
            }
        }

        public object SyncRoot
        {
            get
            {
                return this;
            }
        }
    }
}

