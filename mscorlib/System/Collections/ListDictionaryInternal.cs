namespace System.Collections
{
    using System;
    using System.Reflection;
    using System.Threading;

    [Serializable]
    internal class ListDictionaryInternal : IDictionary, ICollection, IEnumerable
    {
        [NonSerialized]
        private object _syncRoot;
        private int count;
        private DictionaryNode head;
        private int version;

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
            this.version++;
            DictionaryNode node = null;
            DictionaryNode head = this.head;
            while (head != null)
            {
                if (head.key.Equals(key))
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_AddingDuplicate__", new object[] { head.key, key }));
                }
                node = head;
                head = head.next;
            }
            if (head != null)
            {
                head.value = value;
            }
            else
            {
                DictionaryNode node3 = new DictionaryNode {
                    key = key,
                    value = value
                };
                if (node != null)
                {
                    node.next = node3;
                }
                else
                {
                    this.head = node3;
                }
                this.count++;
            }
        }

        public void Clear()
        {
            this.count = 0;
            this.head = null;
            this.version++;
        }

        public bool Contains(object key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key", Environment.GetResourceString("ArgumentNull_Key"));
            }
            for (DictionaryNode node = this.head; node != null; node = node.next)
            {
                if (node.key.Equals(key))
                {
                    return true;
                }
            }
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
            for (DictionaryNode node = this.head; node != null; node = node.next)
            {
                array.SetValue(new DictionaryEntry(node.key, node.value), index);
                index++;
            }
        }

        public IDictionaryEnumerator GetEnumerator()
        {
            return new NodeEnumerator(this);
        }

        public void Remove(object key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key", Environment.GetResourceString("ArgumentNull_Key"));
            }
            this.version++;
            DictionaryNode node = null;
            DictionaryNode head = this.head;
            while (head != null)
            {
                if (head.key.Equals(key))
                {
                    break;
                }
                node = head;
                head = head.next;
            }
            if (head != null)
            {
                if (head == this.head)
                {
                    this.head = head.next;
                }
                else
                {
                    node.next = head.next;
                }
                this.count--;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new NodeEnumerator(this);
        }

        public int Count
        {
            get
            {
                return this.count;
            }
        }

        public bool IsFixedSize
        {
            get
            {
                return false;
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

        public object this[object key]
        {
            get
            {
                if (key == null)
                {
                    throw new ArgumentNullException("key", Environment.GetResourceString("ArgumentNull_Key"));
                }
                for (DictionaryNode node = this.head; node != null; node = node.next)
                {
                    if (node.key.Equals(key))
                    {
                        return node.value;
                    }
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
                this.version++;
                DictionaryNode node = null;
                DictionaryNode head = this.head;
                while (head != null)
                {
                    if (head.key.Equals(key))
                    {
                        break;
                    }
                    node = head;
                    head = head.next;
                }
                if (head != null)
                {
                    head.value = value;
                }
                else
                {
                    DictionaryNode node3 = new DictionaryNode {
                        key = key,
                        value = value
                    };
                    if (node != null)
                    {
                        node.next = node3;
                    }
                    else
                    {
                        this.head = node3;
                    }
                    this.count++;
                }
            }
        }

        public ICollection Keys
        {
            get
            {
                return new NodeKeyValueCollection(this, true);
            }
        }

        public object SyncRoot
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

        public ICollection Values
        {
            get
            {
                return new NodeKeyValueCollection(this, false);
            }
        }

        [Serializable]
        private class DictionaryNode
        {
            public object key;
            public ListDictionaryInternal.DictionaryNode next;
            public object value;
        }

        private class NodeEnumerator : IDictionaryEnumerator, IEnumerator
        {
            private ListDictionaryInternal.DictionaryNode current;
            private ListDictionaryInternal list;
            private bool start;
            private int version;

            public NodeEnumerator(ListDictionaryInternal list)
            {
                this.list = list;
                this.version = list.version;
                this.start = true;
                this.current = null;
            }

            public bool MoveNext()
            {
                if (this.version != this.list.version)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EnumFailedVersion"));
                }
                if (this.start)
                {
                    this.current = this.list.head;
                    this.start = false;
                }
                else if (this.current != null)
                {
                    this.current = this.current.next;
                }
                return (this.current != null);
            }

            public void Reset()
            {
                if (this.version != this.list.version)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EnumFailedVersion"));
                }
                this.start = true;
                this.current = null;
            }

            public object Current
            {
                get
                {
                    return this.Entry;
                }
            }

            public DictionaryEntry Entry
            {
                get
                {
                    if (this.current == null)
                    {
                        throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EnumOpCantHappen"));
                    }
                    return new DictionaryEntry(this.current.key, this.current.value);
                }
            }

            public object Key
            {
                get
                {
                    if (this.current == null)
                    {
                        throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EnumOpCantHappen"));
                    }
                    return this.current.key;
                }
            }

            public object Value
            {
                get
                {
                    if (this.current == null)
                    {
                        throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EnumOpCantHappen"));
                    }
                    return this.current.value;
                }
            }
        }

        private class NodeKeyValueCollection : ICollection, IEnumerable
        {
            private bool isKeys;
            private ListDictionaryInternal list;

            public NodeKeyValueCollection(ListDictionaryInternal list, bool isKeys)
            {
                this.list = list;
                this.isKeys = isKeys;
            }

            void ICollection.CopyTo(Array array, int index)
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
                if ((array.Length - index) < this.list.Count)
                {
                    throw new ArgumentException(Environment.GetResourceString("ArgumentOutOfRange_Index"), "index");
                }
                for (ListDictionaryInternal.DictionaryNode node = this.list.head; node != null; node = node.next)
                {
                    array.SetValue(this.isKeys ? node.key : node.value, index);
                    index++;
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return new NodeKeyValueEnumerator(this.list, this.isKeys);
            }

            int ICollection.Count
            {
                get
                {
                    int num = 0;
                    for (ListDictionaryInternal.DictionaryNode node = this.list.head; node != null; node = node.next)
                    {
                        num++;
                    }
                    return num;
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
                    return this.list.SyncRoot;
                }
            }

            private class NodeKeyValueEnumerator : IEnumerator
            {
                private ListDictionaryInternal.DictionaryNode current;
                private bool isKeys;
                private ListDictionaryInternal list;
                private bool start;
                private int version;

                public NodeKeyValueEnumerator(ListDictionaryInternal list, bool isKeys)
                {
                    this.list = list;
                    this.isKeys = isKeys;
                    this.version = list.version;
                    this.start = true;
                    this.current = null;
                }

                public bool MoveNext()
                {
                    if (this.version != this.list.version)
                    {
                        throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EnumFailedVersion"));
                    }
                    if (this.start)
                    {
                        this.current = this.list.head;
                        this.start = false;
                    }
                    else if (this.current != null)
                    {
                        this.current = this.current.next;
                    }
                    return (this.current != null);
                }

                public void Reset()
                {
                    if (this.version != this.list.version)
                    {
                        throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EnumFailedVersion"));
                    }
                    this.start = true;
                    this.current = null;
                }

                public object Current
                {
                    get
                    {
                        if (this.current == null)
                        {
                            throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EnumOpCantHappen"));
                        }
                        if (!this.isKeys)
                        {
                            return this.current.value;
                        }
                        return this.current.key;
                    }
                }
            }
        }
    }
}

