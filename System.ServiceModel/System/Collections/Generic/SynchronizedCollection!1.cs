namespace System.Collections.Generic
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.ServiceModel;

    [ComVisible(false)]
    public class SynchronizedCollection<T> : IList<T>, ICollection<T>, IEnumerable<T>, IList, ICollection, IEnumerable
    {
        private List<T> items;
        private object sync;

        public SynchronizedCollection()
        {
            this.items = new List<T>();
            this.sync = new object();
        }

        public SynchronizedCollection(object syncRoot)
        {
            if (syncRoot == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("syncRoot"));
            }
            this.items = new List<T>();
            this.sync = syncRoot;
        }

        public SynchronizedCollection(object syncRoot, IEnumerable<T> list)
        {
            if (syncRoot == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("syncRoot"));
            }
            if (list == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("list"));
            }
            this.items = new List<T>(list);
            this.sync = syncRoot;
        }

        public SynchronizedCollection(object syncRoot, params T[] list)
        {
            if (syncRoot == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("syncRoot"));
            }
            if (list == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("list"));
            }
            this.items = new List<T>(list.Length);
            for (int i = 0; i < list.Length; i++)
            {
                this.items.Add(list[i]);
            }
            this.sync = syncRoot;
        }

        public void Add(T item)
        {
            lock (this.sync)
            {
                int count = this.items.Count;
                this.InsertItem(count, item);
            }
        }

        public void Clear()
        {
            lock (this.sync)
            {
                this.ClearItems();
            }
        }

        protected virtual void ClearItems()
        {
            this.items.Clear();
        }

        public bool Contains(T item)
        {
            lock (this.sync)
            {
                return this.items.Contains(item);
            }
        }

        public void CopyTo(T[] array, int index)
        {
            lock (this.sync)
            {
                this.items.CopyTo(array, index);
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            lock (this.sync)
            {
                return this.items.GetEnumerator();
            }
        }

        public int IndexOf(T item)
        {
            lock (this.sync)
            {
                return this.InternalIndexOf(item);
            }
        }

        public void Insert(int index, T item)
        {
            lock (this.sync)
            {
                if ((index < 0) || (index > this.items.Count))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("index", index, System.ServiceModel.SR.GetString("ValueMustBeInRange", new object[] { 0, this.items.Count })));
                }
                this.InsertItem(index, item);
            }
        }

        protected virtual void InsertItem(int index, T item)
        {
            this.items.Insert(index, item);
        }

        private int InternalIndexOf(T item)
        {
            int count = this.items.Count;
            for (int i = 0; i < count; i++)
            {
                if (object.Equals(this.items[i], item))
                {
                    return i;
                }
            }
            return -1;
        }

        public bool Remove(T item)
        {
            lock (this.sync)
            {
                int index = this.InternalIndexOf(item);
                if (index < 0)
                {
                    return false;
                }
                this.RemoveItem(index);
                return true;
            }
        }

        public void RemoveAt(int index)
        {
            lock (this.sync)
            {
                if ((index < 0) || (index >= this.items.Count))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("index", index, System.ServiceModel.SR.GetString("ValueMustBeInRange", new object[] { 0, this.items.Count - 1 })));
                }
                this.RemoveItem(index);
            }
        }

        protected virtual void RemoveItem(int index)
        {
            this.items.RemoveAt(index);
        }

        protected virtual void SetItem(int index, T item)
        {
            this.items[index] = item;
        }

        void ICollection.CopyTo(Array array, int index)
        {
            lock (this.sync)
            {
                this.items.CopyTo(array, index);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.items.GetEnumerator();
        }

        int IList.Add(object value)
        {
            SynchronizedCollection<T>.VerifyValueType(value);
            lock (this.sync)
            {
                this.Add((T) value);
                return (this.Count - 1);
            }
        }

        bool IList.Contains(object value)
        {
            SynchronizedCollection<T>.VerifyValueType(value);
            return this.Contains((T) value);
        }

        int IList.IndexOf(object value)
        {
            SynchronizedCollection<T>.VerifyValueType(value);
            return this.IndexOf((T) value);
        }

        void IList.Insert(int index, object value)
        {
            SynchronizedCollection<T>.VerifyValueType(value);
            this.Insert(index, (T) value);
        }

        void IList.Remove(object value)
        {
            SynchronizedCollection<T>.VerifyValueType(value);
            this.Remove((T) value);
        }

        private static void VerifyValueType(object value)
        {
            if (value == null)
            {
                if (typeof(T).IsValueType)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("SynchronizedCollectionWrongTypeNull")));
                }
            }
            else if (!(value is T))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("SynchronizedCollectionWrongType1", new object[] { value.GetType().FullName })));
            }
        }

        public int Count
        {
            get
            {
                lock (this.sync)
                {
                    return this.items.Count;
                }
            }
        }

        public T this[int index]
        {
            get
            {
                lock (this.sync)
                {
                    return this.items[index];
                }
            }
            set
            {
                lock (this.sync)
                {
                    if ((index < 0) || (index >= this.items.Count))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("index", index, System.ServiceModel.SR.GetString("ValueMustBeInRange", new object[] { 0, this.items.Count - 1 })));
                    }
                    this.SetItem(index, value);
                }
            }
        }

        protected List<T> Items
        {
            get
            {
                return this.items;
            }
        }

        public object SyncRoot
        {
            get
            {
                return this.sync;
            }
        }

        bool ICollection<T>.IsReadOnly
        {
            get
            {
                return false;
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
                return this.sync;
            }
        }

        bool IList.IsFixedSize
        {
            get
            {
                return false;
            }
        }

        bool IList.IsReadOnly
        {
            get
            {
                return false;
            }
        }

        object IList.this[int index]
        {
            get
            {
                return this[index];
            }
            set
            {
                SynchronizedCollection<T>.VerifyValueType(value);
                this[index] = (T) value;
            }
        }
    }
}

