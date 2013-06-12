namespace System.Collections
{
    using System;
    using System.Reflection;

    [Serializable]
    internal sealed class EmptyReadOnlyDictionaryInternal : IDictionary, ICollection, IEnumerable
    {
        public void Add(object key, object value)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key", Environment.GetResourceString("ArgumentNull_Key"));
            }
            if (!key.GetType().IsSerializable)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_NotSerializable"), "key");
            }
            if ((value != null) && !value.GetType().IsSerializable)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_NotSerializable"), "value");
            }
            throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ReadOnly"));
        }

        public void Clear()
        {
            throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ReadOnly"));
        }

        public bool Contains(object key)
        {
            return false;
        }

        public void CopyTo(Array array, int index)
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
            if ((array.Length - index) < this.Count)
            {
                throw new ArgumentException(Environment.GetResourceString("ArgumentOutOfRange_Index"), "index");
            }
        }

        public IDictionaryEnumerator GetEnumerator()
        {
            return new NodeEnumerator();
        }

        public void Remove(object key)
        {
            throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ReadOnly"));
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new NodeEnumerator();
        }

        public int Count
        {
            get
            {
                return 0;
            }
        }

        public bool IsFixedSize
        {
            get
            {
                return true;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return true;
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        public object this[object key]
        {
            get
            {
                if (key == null)
                {
                    throw new ArgumentNullException("key", Environment.GetResourceString("ArgumentNull_Key"));
                }
                return null;
            }
            set
            {
                if (key == null)
                {
                    throw new ArgumentNullException("key", Environment.GetResourceString("ArgumentNull_Key"));
                }
                if (!key.GetType().IsSerializable)
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_NotSerializable"), "key");
                }
                if ((value != null) && !value.GetType().IsSerializable)
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_NotSerializable"), "value");
                }
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ReadOnly"));
            }
        }

        public ICollection Keys
        {
            get
            {
                return new object[0];
            }
        }

        public object SyncRoot
        {
            get
            {
                return this;
            }
        }

        public ICollection Values
        {
            get
            {
                return new object[0];
            }
        }

        private sealed class NodeEnumerator : IDictionaryEnumerator, IEnumerator
        {
            public bool MoveNext()
            {
                return false;
            }

            public void Reset()
            {
            }

            public object Current
            {
                get
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EnumOpCantHappen"));
                }
            }

            public DictionaryEntry Entry
            {
                get
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EnumOpCantHappen"));
                }
            }

            public object Key
            {
                get
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EnumOpCantHappen"));
                }
            }

            public object Value
            {
                get
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EnumOpCantHappen"));
                }
            }
        }
    }
}

