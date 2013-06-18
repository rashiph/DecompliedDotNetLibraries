namespace System.Web.UI.WebControls
{
    using System;
    using System.Collections;
    using System.Reflection;

    public sealed class RepeaterItemCollection : ICollection, IEnumerable
    {
        private ArrayList items;

        public RepeaterItemCollection(ArrayList items)
        {
            this.items = items;
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
            return this.items.GetEnumerator();
        }

        public int Count
        {
            get
            {
                return this.items.Count;
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

        public RepeaterItem this[int index]
        {
            get
            {
                return (RepeaterItem) this.items[index];
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

