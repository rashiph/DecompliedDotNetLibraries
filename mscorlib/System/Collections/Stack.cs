namespace System.Collections
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Threading;

    [Serializable, DebuggerDisplay("Count = {Count}"), ComVisible(true), DebuggerTypeProxy(typeof(Stack.StackDebugView))]
    public class Stack : ICollection, IEnumerable, ICloneable
    {
        private object[] _array;
        private const int _defaultCapacity = 10;
        private int _size;
        [NonSerialized]
        private object _syncRoot;
        private int _version;

        public Stack()
        {
            this._array = new object[10];
            this._size = 0;
            this._version = 0;
        }

        public Stack(ICollection col) : this((col == null) ? 0x20 : col.Count)
        {
            if (col == null)
            {
                throw new ArgumentNullException("col");
            }
            IEnumerator enumerator = col.GetEnumerator();
            while (enumerator.MoveNext())
            {
                this.Push(enumerator.Current);
            }
        }

        public Stack(int initialCapacity)
        {
            if (initialCapacity < 0)
            {
                throw new ArgumentOutOfRangeException("initialCapacity", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if (initialCapacity < 10)
            {
                initialCapacity = 10;
            }
            this._array = new object[initialCapacity];
            this._size = 0;
            this._version = 0;
        }

        public virtual void Clear()
        {
            Array.Clear(this._array, 0, this._size);
            this._size = 0;
            this._version++;
        }

        public virtual object Clone()
        {
            Stack stack = new Stack(this._size) {
                _size = this._size
            };
            Array.Copy(this._array, 0, stack._array, 0, this._size);
            stack._version = this._version;
            return stack;
        }

        public virtual bool Contains(object obj)
        {
            int index = this._size;
            while (index-- > 0)
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
                throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if ((array.Length - index) < this._size)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
            }
            int num = 0;
            if (array is object[])
            {
                object[] objArray = (object[]) array;
                while (num < this._size)
                {
                    objArray[num + index] = this._array[(this._size - num) - 1];
                    num++;
                }
            }
            else
            {
                while (num < this._size)
                {
                    array.SetValue(this._array[(this._size - num) - 1], (int) (num + index));
                    num++;
                }
            }
        }

        public virtual IEnumerator GetEnumerator()
        {
            return new StackEnumerator(this);
        }

        public virtual object Peek()
        {
            if (this._size == 0)
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EmptyStack"));
            }
            return this._array[this._size - 1];
        }

        public virtual object Pop()
        {
            if (this._size == 0)
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EmptyStack"));
            }
            this._version++;
            object obj2 = this._array[--this._size];
            this._array[this._size] = null;
            return obj2;
        }

        public virtual void Push(object obj)
        {
            if (this._size == this._array.Length)
            {
                object[] destinationArray = new object[2 * this._array.Length];
                Array.Copy(this._array, 0, destinationArray, 0, this._size);
                this._array = destinationArray;
            }
            this._array[this._size++] = obj;
            this._version++;
        }

        [HostProtection(SecurityAction.LinkDemand, Synchronization=true)]
        public static Stack Synchronized(Stack stack)
        {
            if (stack == null)
            {
                throw new ArgumentNullException("stack");
            }
            return new SyncStack(stack);
        }

        public virtual object[] ToArray()
        {
            object[] objArray = new object[this._size];
            for (int i = 0; i < this._size; i++)
            {
                objArray[i] = this._array[(this._size - i) - 1];
            }
            return objArray;
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
                    Interlocked.CompareExchange<object>(ref this._syncRoot, new object(), null);
                }
                return this._syncRoot;
            }
        }

        internal class StackDebugView
        {
            private Stack stack;

            public StackDebugView(Stack stack)
            {
                if (stack == null)
                {
                    throw new ArgumentNullException("stack");
                }
                this.stack = stack;
            }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public object[] Items
            {
                get
                {
                    return this.stack.ToArray();
                }
            }
        }

        [Serializable]
        private class StackEnumerator : IEnumerator, ICloneable
        {
            private int _index;
            private Stack _stack;
            private int _version;
            private object currentElement;

            internal StackEnumerator(Stack stack)
            {
                this._stack = stack;
                this._version = this._stack._version;
                this._index = -2;
                this.currentElement = null;
            }

            [SecuritySafeCritical]
            public object Clone()
            {
                return base.MemberwiseClone();
            }

            public virtual bool MoveNext()
            {
                bool flag;
                if (this._version != this._stack._version)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EnumFailedVersion"));
                }
                if (this._index == -2)
                {
                    this._index = this._stack._size - 1;
                    flag = this._index >= 0;
                    if (flag)
                    {
                        this.currentElement = this._stack._array[this._index];
                    }
                    return flag;
                }
                if (this._index == -1)
                {
                    return false;
                }
                flag = --this._index >= 0;
                if (flag)
                {
                    this.currentElement = this._stack._array[this._index];
                    return flag;
                }
                this.currentElement = null;
                return flag;
            }

            public virtual void Reset()
            {
                if (this._version != this._stack._version)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EnumFailedVersion"));
                }
                this._index = -2;
                this.currentElement = null;
            }

            public virtual object Current
            {
                get
                {
                    if (this._index == -2)
                    {
                        throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EnumNotStarted"));
                    }
                    if (this._index == -1)
                    {
                        throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EnumEnded"));
                    }
                    return this.currentElement;
                }
            }
        }

        [Serializable]
        private class SyncStack : Stack
        {
            private object _root;
            private Stack _s;

            internal SyncStack(Stack stack)
            {
                this._s = stack;
                this._root = stack.SyncRoot;
            }

            public override void Clear()
            {
                lock (this._root)
                {
                    this._s.Clear();
                }
            }

            public override object Clone()
            {
                lock (this._root)
                {
                    return new Stack.SyncStack((Stack) this._s.Clone());
                }
            }

            public override bool Contains(object obj)
            {
                lock (this._root)
                {
                    return this._s.Contains(obj);
                }
            }

            public override void CopyTo(Array array, int arrayIndex)
            {
                lock (this._root)
                {
                    this._s.CopyTo(array, arrayIndex);
                }
            }

            public override IEnumerator GetEnumerator()
            {
                lock (this._root)
                {
                    return this._s.GetEnumerator();
                }
            }

            public override object Peek()
            {
                lock (this._root)
                {
                    return this._s.Peek();
                }
            }

            public override object Pop()
            {
                lock (this._root)
                {
                    return this._s.Pop();
                }
            }

            public override void Push(object value)
            {
                lock (this._root)
                {
                    this._s.Push(value);
                }
            }

            public override object[] ToArray()
            {
                lock (this._root)
                {
                    return this._s.ToArray();
                }
            }

            public override int Count
            {
                get
                {
                    lock (this._root)
                    {
                        return this._s.Count;
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
                    return this._root;
                }
            }
        }
    }
}

