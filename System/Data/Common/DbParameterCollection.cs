namespace System.Data.Common
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Data;
    using System.Reflection;

    public abstract class DbParameterCollection : MarshalByRefObject, IDataParameterCollection, IList, ICollection, IEnumerable
    {
        protected DbParameterCollection()
        {
        }

        public abstract int Add(object value);
        public abstract void AddRange(Array values);
        public abstract void Clear();
        public abstract bool Contains(object value);
        public abstract bool Contains(string value);
        public abstract void CopyTo(Array array, int index);
        [EditorBrowsable(EditorBrowsableState.Never)]
        public abstract IEnumerator GetEnumerator();
        protected abstract DbParameter GetParameter(int index);
        protected abstract DbParameter GetParameter(string parameterName);
        public abstract int IndexOf(object value);
        public abstract int IndexOf(string parameterName);
        public abstract void Insert(int index, object value);
        public abstract void Remove(object value);
        public abstract void RemoveAt(int index);
        public abstract void RemoveAt(string parameterName);
        protected abstract void SetParameter(int index, DbParameter value);
        protected abstract void SetParameter(string parameterName, DbParameter value);

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public abstract int Count { get; }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public abstract bool IsFixedSize { get; }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never)]
        public abstract bool IsReadOnly { get; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public abstract bool IsSynchronized { get; }

        public DbParameter this[int index]
        {
            get
            {
                return this.GetParameter(index);
            }
            set
            {
                this.SetParameter(index, value);
            }
        }

        public DbParameter this[string parameterName]
        {
            get
            {
                return this.GetParameter(parameterName);
            }
            set
            {
                this.SetParameter(parameterName, value);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public abstract object SyncRoot { get; }

        object IList.this[int index]
        {
            get
            {
                return this.GetParameter(index);
            }
            set
            {
                this.SetParameter(index, (DbParameter) value);
            }
        }

        object IDataParameterCollection.this[string parameterName]
        {
            get
            {
                return this.GetParameter(parameterName);
            }
            set
            {
                this.SetParameter(parameterName, (DbParameter) value);
            }
        }
    }
}

