namespace System.Collections.Generic
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.ServiceModel;

    [ComVisible(false)]
    public class SynchronizedReadOnlyCollection<T> : IList<T>, ICollection<T>, IEnumerable<T>, IList, ICollection, IEnumerable
    {
        private IList<T> items;
        private object sync;

        public SynchronizedReadOnlyCollection()
        {
            this.items = new List<T>();
            this.sync = new object();
        }

        public SynchronizedReadOnlyCollection(object syncRoot)
        {
            if (syncRoot == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("syncRoot"));
            }
            this.items = new List<T>();
            this.sync = syncRoot;
        }

        public SynchronizedReadOnlyCollection(object syncRoot, IEnumerable<T> list)
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

        public SynchronizedReadOnlyCollection(object syncRoot, params T[] list)
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

        internal SynchronizedReadOnlyCollection(object syncRoot, List<T> list, bool makeCopy)
        {
            if (syncRoot == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("syncRoot"));
            }
            if (list == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("list"));
            }
            if (makeCopy)
            {
                this.items = new List<T>(list);
            }
            else
            {
                this.items = list;
            }
            this.sync = syncRoot;
        }

        public bool Contains(T value)
        {
            lock (this.sync)
            {
                return this.items.Contains(value);
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

        public int IndexOf(T value)
        {
            lock (this.sync)
            {
                return this.items.IndexOf(value);
            }
        }

        void ICollection<T>.Add(T value)
        {
            this.ThrowReadOnly();
        }

        void ICollection<T>.Clear()
        {
            this.ThrowReadOnly();
        }

        bool ICollection<T>.Remove(T value)
        {
            this.ThrowReadOnly();
            return false;
        }

        void IList<T>.Insert(int index, T value)
        {
            this.ThrowReadOnly();
        }

        void IList<T>.RemoveAt(int index)
        {
            this.ThrowReadOnly();
        }

        void ICollection.CopyTo(Array array, int index)
        {
            ICollection items = this.items as ICollection;
            if (items == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.ServiceModel.SR.GetString("SFxCopyToRequiresICollection")));
            }
            lock (this.sync)
            {
                items.CopyTo(array, index);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            lock (this.sync)
            {
                IEnumerable items = this.items;
                if (items != null)
                {
                    return items.GetEnumerator();
                }
                return new EnumeratorAdapter<T>(this.items);
            }
        }

        int IList.Add(object value)
        {
            this.ThrowReadOnly();
            return 0;
        }

        void IList.Clear()
        {
            this.ThrowReadOnly();
        }

        bool IList.Contains(object value)
        {
            SynchronizedReadOnlyCollection<T>.VerifyValueType(value);
            return this.Contains((T) value);
        }

        int IList.IndexOf(object value)
        {
            SynchronizedReadOnlyCollection<T>.VerifyValueType(value);
            return this.IndexOf((T) value);
        }

        void IList.Insert(int index, object value)
        {
            this.ThrowReadOnly();
        }

        void IList.Remove(object value)
        {
            this.ThrowReadOnly();
        }

        void IList.RemoveAt(int index)
        {
            this.ThrowReadOnly();
        }

        private void ThrowReadOnly()
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.ServiceModel.SR.GetString("SFxCollectionReadOnly")));
        }

        private static void VerifyValueType(object value)
        {
            if (!(value is T) && ((value != null) || typeof(T).IsValueType))
            {
                Type type = (value == null) ? typeof(object) : value.GetType();
                string message = System.ServiceModel.SR.GetString("SFxCollectionWrongType2", new object[] { type.ToString(), typeof(T).ToString() });
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(message));
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
        }

        protected IList<T> Items
        {
            get
            {
                return this.items;
            }
        }

        bool ICollection<T>.IsReadOnly
        {
            get
            {
                return true;
            }
        }

        T IList<T>.this[int index]
        {
            get
            {
                return this[index];
            }
            set
            {
                this.ThrowReadOnly();
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
                return true;
            }
        }

        bool IList.IsReadOnly
        {
            get
            {
                return true;
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
                this.ThrowReadOnly();
            }
        }

        private sealed class EnumeratorAdapter : IEnumerator, IDisposable
        {
            private IEnumerator<T> e;
            private IList<T> list;

            public EnumeratorAdapter(IList<T> list)
            {
                this.list = list;
                this.e = list.GetEnumerator();
            }

            public void Dispose()
            {
                this.e.Dispose();
            }

            public bool MoveNext()
            {
                return this.e.MoveNext();
            }

            public void Reset()
            {
                this.e = this.list.GetEnumerator();
            }

            public object Current
            {
                get
                {
                    return this.e.Current;
                }
            }
        }
    }
}

