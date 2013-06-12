namespace System.Web.UI.WebControls
{
    using System;
    using System.Collections;

    internal sealed class DummyDataSource : ICollection, IEnumerable
    {
        private int dataItemCount;

        internal DummyDataSource(int dataItemCount)
        {
            this.dataItemCount = dataItemCount;
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
            return new DummyDataSourceEnumerator(this.dataItemCount);
        }

        public int Count
        {
            get
            {
                return this.dataItemCount;
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        public object SyncRoot
        {
            get
            {
                return this;
            }
        }

        private class DummyDataSourceEnumerator : IEnumerator
        {
            private int count;
            private int index;

            public DummyDataSourceEnumerator(int count)
            {
                this.count = count;
                this.index = -1;
            }

            public bool MoveNext()
            {
                this.index++;
                return (this.index < this.count);
            }

            public void Reset()
            {
                this.index = -1;
            }

            public object Current
            {
                get
                {
                    return null;
                }
            }
        }
    }
}

