namespace System.Data.SqlClient
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Reflection;

    [Serializable, ListBindable(false)]
    public sealed class SqlErrorCollection : ICollection, IEnumerable
    {
        private ArrayList errors = new ArrayList();

        internal SqlErrorCollection()
        {
        }

        internal void Add(SqlError error)
        {
            this.errors.Add(error);
        }

        public void CopyTo(Array array, int index)
        {
            this.errors.CopyTo(array, index);
        }

        public void CopyTo(SqlError[] array, int index)
        {
            this.errors.CopyTo(array, index);
        }

        public IEnumerator GetEnumerator()
        {
            return this.errors.GetEnumerator();
        }

        public int Count
        {
            get
            {
                return this.errors.Count;
            }
        }

        public SqlError this[int index]
        {
            get
            {
                return (SqlError) this.errors[index];
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
                return this;
            }
        }
    }
}

