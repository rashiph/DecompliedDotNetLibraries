namespace System.Collections
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Threading;

    [Serializable, DebuggerDisplay("Count = {Count}"), ComVisible(true), DebuggerTypeProxy(typeof(Queue.QueueDebugView))]
    public class Queue : ICollection, IEnumerable, ICloneable
    {
        private object[] _array;
        private int _growFactor;
        private int _head;
        private const int _MinimumGrow = 4;
        private const int _ShrinkThreshold = 0x20;
        private int _size;
        [NonSerialized]
        private object _syncRoot;
        private int _tail;
        private int _version;

        public Queue() : this(0x20, 2f)
        {
        }

        public Queue(ICollection col) : this((col == null) ? 0x20 : col.Count)
        {
            if (col == null)
            {
                throw new ArgumentNullException("col");
            }
            IEnumerator enumerator = col.GetEnumerator();
            while (enumerator.MoveNext())
            {
                this.Enqueue(enumerator.Current);
            }
        }

        public Queue(int capacity) : this(capacity, 2f)
        {
        }

        public Queue(int capacity, float growFactor)
        {
            if (capacity < 0)
            {
                throw new ArgumentOutOfRangeException("capacity", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if ((growFactor < 1.0) || (growFactor > 10.0))
            {
                throw new ArgumentOutOfRangeException("growFactor", Environment.GetResourceString("ArgumentOutOfRange_QueueGrowFactor", new object[] { 1, 10 }));
            }
            this._array = new object[capacity];
            this._head = 0;
            this._tail = 0;
            this._size = 0;
            this._growFactor = (int) (growFactor * 100f);
        }

        public virtual void Clear()
        {
            if (this._head < this._tail)
            {
                Array.Clear(this._array, this._head, this._size);
            }
            else
            {
                Array.Clear(this._array, this._head, this._array.Length - this._head);
                Array.Clear(this._array, 0, this._tail);
            }
            this._head = 0;
            this._tail = 0;
            this._size = 0;
            this._version++;
        }

        public virtual object Clone()
        {
            Queue queue = new Queue(this._size) {
                _size = this._size
            };
            int length = this._size;
            int num2 = ((this._array.Length - this._head) < length) ? (this._array.Length - this._head) : length;
            Array.Copy(this._array, this._head, queue._array, 0, num2);
            length -= num2;
            if (length > 0)
            {
                Array.Copy(this._array, 0, queue._array, this._array.Length - this._head, length);
            }
            queue._version = this._version;
            return queue;
        }

        public virtual bool Contains(object obj)
        {
            int index = this._head;
            int num2 = this._size;
            while (num2-- > 0)
            {
                if (obj == null)
                {
                    if (this._array[index] == null)
                    {
                        return true;
                    }
                }
                else if ((this._array[index] != null) && this._array[index].Equals(obj))
                {
                    return true;
                }
                index = (index + 1) % this._array.Length;
            }
            return false;
        }

        public virtual void CopyTo(Array array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            if (array.Rank != 1)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_RankMultiDimNotSupported"));
            }
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("ArgumentOutOfRange_Index"));
            }
            if ((array.Length - index) < this._size)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
            }
            int length = this._size;
            if (length != 0)
            {
                int num3 = ((this._array.Length - this._head) < length) ? (this._array.Length - this._head) : length;
                Array.Copy(this._array, this._head, array, index, num3);
                length -= num3;
                if (length > 0)
                {
                    Array.Copy(this._array, 0, array, (index + this._array.Length) - this._head, length);
                }
            }
        }

        public virtual object Dequeue()
        {
            if (this.Count == 0)
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EmptyQueue"));
            }
            object obj2 = this._array[this._head];
            this._array[this._head] = null;
            this._head = (this._head + 1) % this._array.Length;
            this._size--;
            this._version++;
            return obj2;
        }

        public virtual void Enqueue(object obj)
        {
            if (this._size == this._array.Length)
            {
                int capacity = (int) ((this._array.Length * this._growFactor) / 100L);
                if (capacity < (this._array.Length + 4))
                {
                    capacity = this._array.Length + 4;
                }
                this.SetCapacity(capacity);
            }
            this._array[this._tail] = obj;
            this._tail = (this._tail + 1) % this._array.Length;
            this._size++;
            this._version++;
        }

        internal object GetElement(int i)
        {
            return this._array[(this._head + i) % this._array.Length];
        }

        public virtual IEnumerator GetEnumerator()
        {
            return new QueueEnumerator(this);
        }

        public virtual object Peek()
        {
            if (this.Count == 0)
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EmptyQueue"));
            }
            return this._array[this._head];
        }

        private void SetCapacity(int capacity)
        {
            object[] destinationArray = new object[capacity];
            if (this._size > 0)
            {
                if (this._head < this._tail)
                {
                    Array.Copy(this._array, this._head, destinationArray, 0, this._size);
                }
                else
                {
                    Array.Copy(this._array, this._head, destinationArray, 0, this._array.Length - this._head);
                    Array.Copy(this._array, 0, destinationArray, this._array.Length - this._head, this._tail);
                }
            }
            this._array = destinationArray;
            this._head = 0;
            this._tail = (this._size == capacity) ? 0 : this._size;
            this._version++;
        }

        [HostProtection(SecurityAction.LinkDemand, Synchronization=true)]
        public static Queue Synchronized(Queue queue)
        {
            if (queue == null)
            {
                throw new ArgumentNullException("queue");
            }
            return new SynchronizedQueue(queue);
        }

        public virtual object[] ToArray()
        {
            object[] destinationArray = new object[this._size];
            if (this._size != 0)
            {
                if (this._head < this._tail)
                {
                    Array.Copy(this._array, this._head, destinationArray, 0, this._size);
                    return destinationArray;
                }
                Array.Copy(this._array, this._head, destinationArray, 0, this._array.Length - this._head);
                Array.Copy(this._array, 0, destinationArray, this._array.Length - this._head, this._tail);
            }
            return destinationArray;
        }

        public virtual void TrimToSize()
        {
            this.SetCapacity(this._size);
        }

        public virtual int Count
        {
            get
            {
                return this._size;
            }
        }

        public virtual bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        public virtual object SyncRoot
        {
            get
            {
                if (this._syncRoot == null)
                {
                    Interlocked.CompareExchange(ref this._syncRoot, new object(), null);
                }
                return this._syncRoot;
            }
        }

        internal class QueueDebugView
        {
            private Queue queue;

            public QueueDebugView(Queue queue)
            {
                if (queue == null)
                {
                    throw new ArgumentNullException("queue");
                }
                this.queue = queue;
            }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public object[] Items
            {
                get
                {
                    return this.queue.ToArray();
                }
            }
        }

        [Serializable]
        private class QueueEnumerator : IEnumerator, ICloneable
        {
            private int _index;
            private Queue _q;
            private int _version;
            private object currentElement;

            internal QueueEnumerator(Queue q)
            {
                this._q = q;
                this._version = this._q._version;
                this._index = 0;
                this.currentElement = this._q._array;
                if (this._q._size == 0)
                {
                    this._index = -1;
                }
            }

            public object Clone()
            {
                return base.MemberwiseClone();
            }

            public virtual bool MoveNext()
            {
                if (this._version != this._q._version)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EnumFailedVersion"));
                }
                if (this._index < 0)
                {
                    this.currentElement = this._q._array;
                    return false;
                }
                this.currentElement = this._q.GetElement(this._index);
                this._index++;
                if (this._index == this._q._size)
                {
                    this._index = -1;
                }
                return true;
            }

            public virtual void Reset()
            {
                if (this._version != this._q._version)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EnumFailedVersion"));
                }
                if (this._q._size == 0)
                {
                    this._index = -1;
                }
                else
                {
                    this._index = 0;
                }
                this.currentElement = this._q._array;
            }

            public virtual object Current
            {
                get
                {
                    if (this.currentElement != this._q._array)
                    {
                        return this.currentElement;
                    }
                    if (this._index == 0)
                    {
                        throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EnumNotStarted"));
                    }
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EnumEnded"));
                }
            }
        }

        [Serializable]
        private class SynchronizedQueue : Queue
        {
            private Queue _q;
            private object root;

            internal SynchronizedQueue(Queue q)
            {
                this._q = q;
                this.root = this._q.SyncRoot;
            }

            public override void Clear()
            {
                lock (this.root)
                {
                    this._q.Clear();
                }
            }

            public override object Clone()
            {
                lock (this.root)
                {
                    return new Queue.SynchronizedQueue((Queue) this._q.Clone());
                }
            }

            public override bool Contains(object obj)
            {
                lock (this.root)
                {
                    return this._q.Contains(obj);
                }
            }

            public override void CopyTo(Array array, int arrayIndex)
            {
                lock (this.root)
                {
                    this._q.CopyTo(array, arrayIndex);
                }
            }

            public override object Dequeue()
            {
                lock (this.root)
                {
                    return this._q.Dequeue();
                }
            }

            public override void Enqueue(object value)
            {
                lock (this.root)
                {
                    this._q.Enqueue(value);
                }
            }

            public override IEnumerator GetEnumerator()
            {
                lock (this.root)
                {
                    return this._q.GetEnumerator();
                }
            }

            public override object Peek()
            {
                lock (this.root)
                {
                    return this._q.Peek();
                }
            }

            public override object[] ToArray()
            {
                lock (this.root)
                {
                    return this._q.ToArray();
                }
            }

            public override void TrimToSize()
            {
                lock (this.root)
                {
                    this._q.TrimToSize();
                }
            }

            public override int Count
            {
                get
                {
                    lock (this.root)
                    {
                        return this._q.Count;
                    }
                }
            }

            public override bool IsSynchronized
            {
                get
                {
                    return true;
                }
            }

            public override object SyncRoot
            {
                get
                {
                    return this.root;
                }
            }
        }
    }
}

