namespace System.Diagnostics
{
    using System;
    using System.Collections;
    using System.Reflection;

    public class TraceListenerCollection : IList, ICollection, IEnumerable
    {
        private ArrayList list = new ArrayList(1);

        internal TraceListenerCollection()
        {
        }

        public int Add(TraceListener listener)
        {
            this.InitializeListener(listener);
            lock (TraceInternal.critSec)
            {
                return this.list.Add(listener);
            }
        }

        public void AddRange(TraceListener[] value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            for (int i = 0; i < value.Length; i++)
            {
                this.Add(value[i]);
            }
        }

        public void AddRange(TraceListenerCollection value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            int count = value.Count;
            for (int i = 0; i < count; i++)
            {
                this.Add(value[i]);
            }
        }

        public void Clear()
        {
            this.list = new ArrayList();
        }

        public bool Contains(TraceListener listener)
        {
            return ((IList) this).Contains(listener);
        }

        public void CopyTo(TraceListener[] listeners, int index)
        {
            ((ICollection) this).CopyTo(listeners, index);
        }

        public IEnumerator GetEnumerator()
        {
            return this.list.GetEnumerator();
        }

        public int IndexOf(TraceListener listener)
        {
            return ((IList) this).IndexOf(listener);
        }

        internal void InitializeListener(TraceListener listener)
        {
            if (listener == null)
            {
                throw new ArgumentNullException("listener");
            }
            listener.IndentSize = TraceInternal.IndentSize;
            listener.IndentLevel = TraceInternal.IndentLevel;
        }

        public void Insert(int index, TraceListener listener)
        {
            this.InitializeListener(listener);
            lock (TraceInternal.critSec)
            {
                this.list.Insert(index, listener);
            }
        }

        public void Remove(TraceListener listener)
        {
            ((IList) this).Remove(listener);
        }

        public void Remove(string name)
        {
            TraceListener listener = this[name];
            if (listener != null)
            {
                ((IList) this).Remove(listener);
            }
        }

        public void RemoveAt(int index)
        {
            lock (TraceInternal.critSec)
            {
                this.list.RemoveAt(index);
            }
        }

        void ICollection.CopyTo(Array array, int index)
        {
            lock (TraceInternal.critSec)
            {
                this.list.CopyTo(array, index);
            }
        }

        int IList.Add(object value)
        {
            TraceListener listener = value as TraceListener;
            if (listener == null)
            {
                throw new ArgumentException(SR.GetString("MustAddListener"), "value");
            }
            this.InitializeListener(listener);
            lock (TraceInternal.critSec)
            {
                return this.list.Add(value);
            }
        }

        bool IList.Contains(object value)
        {
            return this.list.Contains(value);
        }

        int IList.IndexOf(object value)
        {
            return this.list.IndexOf(value);
        }

        void IList.Insert(int index, object value)
        {
            TraceListener listener = value as TraceListener;
            if (listener == null)
            {
                throw new ArgumentException(SR.GetString("MustAddListener"), "value");
            }
            this.InitializeListener(listener);
            lock (TraceInternal.critSec)
            {
                this.list.Insert(index, value);
            }
        }

        void IList.Remove(object value)
        {
            lock (TraceInternal.critSec)
            {
                this.list.Remove(value);
            }
        }

        public int Count
        {
            get
            {
                return this.list.Count;
            }
        }

        public TraceListener this[int i]
        {
            get
            {
                return (TraceListener) this.list[i];
            }
            set
            {
                this.InitializeListener(value);
                this.list[i] = value;
            }
        }

        public TraceListener this[string name]
        {
            get
            {
                foreach (TraceListener listener in this)
                {
                    if (listener.Name == name)
                    {
                        return listener;
                    }
                }
                return null;
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
                return this;
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
                return this.list[index];
            }
            set
            {
                TraceListener listener = value as TraceListener;
                if (listener == null)
                {
                    throw new ArgumentException(SR.GetString("MustAddListener"), "value");
                }
                this.InitializeListener(listener);
                this.list[index] = listener;
            }
        }
    }
}

