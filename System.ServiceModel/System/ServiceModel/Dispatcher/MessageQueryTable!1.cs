namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    public class MessageQueryTable<TItem> : IDictionary<MessageQuery, TItem>, ICollection<KeyValuePair<MessageQuery, TItem>>, IEnumerable<KeyValuePair<MessageQuery, TItem>>, IEnumerable
    {
        private Dictionary<System.Type, MessageQueryCollection> collectionsByType;
        private Dictionary<MessageQuery, TItem> dictionary;

        public MessageQueryTable()
        {
            this.dictionary = new Dictionary<MessageQuery, TItem>();
            this.collectionsByType = new Dictionary<System.Type, MessageQueryCollection>();
        }

        public void Add(KeyValuePair<MessageQuery, TItem> item)
        {
            this.Add(item.Key, item.Value);
        }

        public void Add(MessageQuery key, TItem value)
        {
            MessageQueryCollection querys;
            if (key == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("key");
            }
            System.Type type = key.GetType();
            if (!this.collectionsByType.TryGetValue(type, out querys))
            {
                querys = key.CreateMessageQueryCollection();
                if (querys == null)
                {
                    querys = new SequentialMessageQueryCollection<TItem>();
                }
                this.collectionsByType.Add(type, querys);
            }
            querys.Add(key);
            this.dictionary.Add(key, value);
        }

        public void Clear()
        {
            this.collectionsByType.Clear();
            this.dictionary.Clear();
        }

        public bool Contains(KeyValuePair<MessageQuery, TItem> item)
        {
            return this.dictionary.Contains(item);
        }

        public bool ContainsKey(MessageQuery key)
        {
            return this.dictionary.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<MessageQuery, TItem>[] array, int arrayIndex)
        {
            this.dictionary.CopyTo(array, arrayIndex);
        }

        public IEnumerable<KeyValuePair<MessageQuery, TResult>> Evaluate<TResult>(Message message)
        {
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }
            return (IEnumerable<KeyValuePair<MessageQuery, TResult>>) new MessageEnumerable<TItem, TResult>((MessageQueryTable<TItem>) this, message);
        }

        public IEnumerable<KeyValuePair<MessageQuery, TResult>> Evaluate<TResult>(MessageBuffer buffer)
        {
            if (buffer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("buffer");
            }
            return (IEnumerable<KeyValuePair<MessageQuery, TResult>>) new MessageBufferEnumerable<TItem, TResult>((MessageQueryTable<TItem>) this, buffer);
        }

        public IEnumerator<KeyValuePair<MessageQuery, TItem>> GetEnumerator()
        {
            return this.dictionary.GetEnumerator();
        }

        public bool Remove(MessageQuery key)
        {
            if (!this.dictionary.Remove(key))
            {
                return false;
            }
            System.Type type = key.GetType();
            MessageQueryCollection querys = this.collectionsByType[type];
            querys.Remove(key);
            if (querys.Count == 0)
            {
                this.collectionsByType.Remove(type);
            }
            return true;
        }

        public bool Remove(KeyValuePair<MessageQuery, TItem> item)
        {
            return this.Remove(item.Key);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public bool TryGetValue(MessageQuery key, out TItem value)
        {
            return this.dictionary.TryGetValue(key, out value);
        }

        public int Count
        {
            get
            {
                return this.dictionary.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public TItem this[MessageQuery key]
        {
            get
            {
                return this.dictionary[key];
            }
            set
            {
                this.Add(key, value);
            }
        }

        public ICollection<MessageQuery> Keys
        {
            get
            {
                return this.dictionary.Keys;
            }
        }

        public ICollection<TItem> Values
        {
            get
            {
                return (ICollection<TItem>) this.dictionary.Values;
            }
        }

        private abstract class Enumerable<TSource, TResult> : IEnumerable<KeyValuePair<MessageQuery, TResult>>, IEnumerable
        {
            private TSource source;
            private MessageQueryTable<TItem> table;

            public Enumerable(MessageQueryTable<TItem> table, TSource source)
            {
                this.table = table;
                this.source = source;
            }

            public IEnumerator<KeyValuePair<MessageQuery, TResult>> GetEnumerator()
            {
                return new Enumerator<TItem, TSource, TResult>((MessageQueryTable<TItem>.Enumerable<TSource, TResult>) this);
            }

            protected abstract IEnumerator<KeyValuePair<MessageQuery, TResult>> GetInnerEnumerator(MessageQueryCollection collection);
            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            protected TSource Source
            {
                get
                {
                    return this.source;
                }
            }

            private class Enumerator : IEnumerator<KeyValuePair<MessageQuery, TResult>>, IDisposable, IEnumerator
            {
                private MessageQueryTable<TItem>.Enumerable<TSource, TResult> enumerable;
                private IEnumerator<KeyValuePair<MessageQuery, TResult>> innerEnumerator;
                private IEnumerator<MessageQueryCollection> outerEnumerator;

                public Enumerator(MessageQueryTable<TItem>.Enumerable<TSource, TResult> enumerable)
                {
                    this.outerEnumerator = enumerable.table.collectionsByType.Values.GetEnumerator();
                    this.enumerable = enumerable;
                }

                public void Dispose()
                {
                }

                public bool MoveNext()
                {
                    if ((this.innerEnumerator != null) && this.innerEnumerator.MoveNext())
                    {
                        return true;
                    }
                    if (!this.outerEnumerator.MoveNext())
                    {
                        return false;
                    }
                    MessageQueryCollection current = this.outerEnumerator.Current;
                    this.innerEnumerator = this.enumerable.GetInnerEnumerator(current);
                    return this.innerEnumerator.MoveNext();
                }

                public void Reset()
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
                }

                public KeyValuePair<MessageQuery, TResult> Current
                {
                    get
                    {
                        return this.innerEnumerator.Current;
                    }
                }

                object IEnumerator.Current
                {
                    get
                    {
                        return this.Current;
                    }
                }
            }
        }

        private class MessageBufferEnumerable<TResult> : MessageQueryTable<TItem>.Enumerable<MessageBuffer, TResult>
        {
            public MessageBufferEnumerable(MessageQueryTable<TItem> table, MessageBuffer buffer) : base(table, buffer)
            {
            }

            protected override IEnumerator<KeyValuePair<MessageQuery, TResult>> GetInnerEnumerator(MessageQueryCollection collection)
            {
                return collection.Evaluate<TResult>(base.Source).GetEnumerator();
            }
        }

        private class MessageEnumerable<TResult> : MessageQueryTable<TItem>.Enumerable<Message, TResult>
        {
            public MessageEnumerable(MessageQueryTable<TItem> table, Message message) : base(table, message)
            {
            }

            protected override IEnumerator<KeyValuePair<MessageQuery, TResult>> GetInnerEnumerator(MessageQueryCollection collection)
            {
                return collection.Evaluate<TResult>(base.Source).GetEnumerator();
            }
        }

        private class SequentialMessageQueryCollection : MessageQueryCollection
        {
            public override IEnumerable<KeyValuePair<MessageQuery, TResult>> Evaluate<TResult>(Message message)
            {
                return (IEnumerable<KeyValuePair<MessageQuery, TResult>>) new MessageSequentialResultEnumerable<TItem, TResult>((MessageQueryTable<TItem>.SequentialMessageQueryCollection) this, message);
            }

            public override IEnumerable<KeyValuePair<MessageQuery, TResult>> Evaluate<TResult>(MessageBuffer buffer)
            {
                return (IEnumerable<KeyValuePair<MessageQuery, TResult>>) new MessageBufferSequentialResultEnumerable<TItem, TResult>((MessageQueryTable<TItem>.SequentialMessageQueryCollection) this, buffer);
            }

            private class MessageBufferSequentialResultEnumerable<TResult> : MessageQueryTable<TItem>.SequentialMessageQueryCollection.SequentialResultEnumerable<MessageBuffer, TResult>
            {
                public MessageBufferSequentialResultEnumerable(MessageQueryTable<TItem>.SequentialMessageQueryCollection collection, MessageBuffer buffer) : base(collection, buffer)
                {
                }

                protected override TResult Evaluate(MessageQuery query)
                {
                    return query.Evaluate<TResult>(base.Source);
                }
            }

            private class MessageSequentialResultEnumerable<TResult> : MessageQueryTable<TItem>.SequentialMessageQueryCollection.SequentialResultEnumerable<Message, TResult>
            {
                public MessageSequentialResultEnumerable(MessageQueryTable<TItem>.SequentialMessageQueryCollection collection, Message message) : base(collection, message)
                {
                }

                protected override TResult Evaluate(MessageQuery query)
                {
                    return query.Evaluate<TResult>(base.Source);
                }
            }

            private abstract class SequentialResultEnumerable<TSource, TResult> : IEnumerable<KeyValuePair<MessageQuery, TResult>>, IEnumerable
            {
                private MessageQueryTable<TItem>.SequentialMessageQueryCollection collection;
                private TSource source;

                public SequentialResultEnumerable(MessageQueryTable<TItem>.SequentialMessageQueryCollection collection, TSource source)
                {
                    this.collection = collection;
                    this.source = source;
                }

                protected abstract TResult Evaluate(MessageQuery query);
                public IEnumerator<KeyValuePair<MessageQuery, TResult>> GetEnumerator()
                {
                    return new SequentialResultEnumerator<TItem, TSource, TResult>((MessageQueryTable<TItem>.SequentialMessageQueryCollection.SequentialResultEnumerable<TSource, TResult>) this);
                }

                IEnumerator IEnumerable.GetEnumerator()
                {
                    return this.GetEnumerator();
                }

                private MessageQueryTable<TItem>.SequentialMessageQueryCollection Collection
                {
                    get
                    {
                        return this.collection;
                    }
                }

                protected TSource Source
                {
                    get
                    {
                        return this.source;
                    }
                }

                private class SequentialResultEnumerator : IEnumerator<KeyValuePair<MessageQuery, TResult>>, IDisposable, IEnumerator
                {
                    private MessageQueryTable<TItem>.SequentialMessageQueryCollection.SequentialResultEnumerable<TSource, TResult> enumerable;
                    private IEnumerator<MessageQuery> queries;

                    public SequentialResultEnumerator(MessageQueryTable<TItem>.SequentialMessageQueryCollection.SequentialResultEnumerable<TSource, TResult> enumerable)
                    {
                        this.enumerable = enumerable;
                        this.queries = enumerable.Collection.GetEnumerator();
                    }

                    public void Dispose()
                    {
                    }

                    public bool MoveNext()
                    {
                        return this.queries.MoveNext();
                    }

                    public void Reset()
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
                    }

                    public KeyValuePair<MessageQuery, TResult> Current
                    {
                        get
                        {
                            MessageQuery current = this.queries.Current;
                            return new KeyValuePair<MessageQuery, TResult>(current, this.enumerable.Evaluate(current));
                        }
                    }

                    object IEnumerator.Current
                    {
                        get
                        {
                            return this.Current;
                        }
                    }
                }
            }
        }
    }
}

