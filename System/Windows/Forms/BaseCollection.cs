namespace System.Windows.Forms
{
    using System;
    using System.Collections;
    using System.ComponentModel;

    public class BaseCollection : MarshalByRefObject, ICollection, IEnumerable
    {
        public void CopyTo(Array ar, int index)
        {
            this.List.CopyTo(ar, index);
        }

        public IEnumerator GetEnumerator()
        {
            return this.List.GetEnumerator();
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), Browsable(false)]
        public virtual int Count
        {
            get
            {
                return this.List.Count;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced)]
        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced)]
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

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced)]
        public object SyncRoot
        {
            get
            {
                return this;
            }
        }
    }
}

