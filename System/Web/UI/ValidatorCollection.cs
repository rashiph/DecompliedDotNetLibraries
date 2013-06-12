namespace System.Web.UI
{
    using System;
    using System.Collections;
    using System.Reflection;

    public sealed class ValidatorCollection : ICollection, IEnumerable
    {
        private ArrayList data = new ArrayList();

        public void Add(IValidator validator)
        {
            this.data.Add(validator);
        }

        public bool Contains(IValidator validator)
        {
            return this.data.Contains(validator);
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
            return this.data.GetEnumerator();
        }

        public void Remove(IValidator validator)
        {
            this.data.Remove(validator);
        }

        public int Count
        {
            get
            {
                return this.data.Count;
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

        public IValidator this[int index]
        {
            get
            {
                return (IValidator) this.data[index];
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

