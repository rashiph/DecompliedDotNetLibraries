namespace System.Web.UI.WebControls
{
    using System;
    using System.Collections;
    using System.Reflection;

    public class DetailsViewRowCollection : ICollection, IEnumerable
    {
        private ArrayList _rows;

        public DetailsViewRowCollection(ArrayList rows)
        {
            this._rows = rows;
        }

        public void CopyTo(DetailsViewRow[] array, int index)
        {
            ((ICollection) this).CopyTo(array, index);
        }

        public IEnumerator GetEnumerator()
        {
            return this._rows.GetEnumerator();
        }

        void ICollection.CopyTo(Array array, int index)
        {
            IEnumerator enumerator = this.GetEnumerator();
            while (enumerator.MoveNext())
            {
                array.SetValue(enumerator.Current, index++);
            }
        }

        public int Count
        {
            get
            {
                return this._rows.Count;
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

        public DetailsViewRow this[int index]
        {
            get
            {
                return (DetailsViewRow) this._rows[index];
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

