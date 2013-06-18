namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.ServiceModel.Channels;

    internal class ThreadSafeMessageFilterTable<FilterData> : IMessageFilterTable<FilterData>, IDictionary<MessageFilter, FilterData>, ICollection<KeyValuePair<MessageFilter, FilterData>>, IEnumerable<KeyValuePair<MessageFilter, FilterData>>, IEnumerable
    {
        private object syncRoot;
        private MessageFilterTable<FilterData> table;

        internal ThreadSafeMessageFilterTable()
        {
            this.table = new MessageFilterTable<FilterData>();
            this.syncRoot = new object();
        }

        public void Add(MessageFilter key, FilterData value)
        {
            lock (this.syncRoot)
            {
                this.table.Add(key, value);
            }
        }

        internal void Add(MessageFilter filter, FilterData data, int priority)
        {
            lock (this.syncRoot)
            {
                this.table.Add(filter, data, priority);
            }
        }

        public void Clear()
        {
            lock (this.syncRoot)
            {
                this.table.Clear();
            }
        }

        public bool ContainsKey(MessageFilter key)
        {
            lock (this.syncRoot)
            {
                return this.table.ContainsKey(key);
            }
        }

        public bool GetMatchingFilter(Message message, out MessageFilter filter)
        {
            lock (this.syncRoot)
            {
                return this.table.GetMatchingFilter(message, out filter);
            }
        }

        public bool GetMatchingFilter(MessageBuffer buffer, out MessageFilter filter)
        {
            lock (this.syncRoot)
            {
                return this.table.GetMatchingFilter(buffer, out filter);
            }
        }

        public bool GetMatchingFilters(Message message, ICollection<MessageFilter> results)
        {
            lock (this.syncRoot)
            {
                return this.table.GetMatchingFilters(message, results);
            }
        }

        public bool GetMatchingFilters(MessageBuffer buffer, ICollection<MessageFilter> results)
        {
            lock (this.syncRoot)
            {
                return this.table.GetMatchingFilters(buffer, results);
            }
        }

        public bool GetMatchingValue(Message message, out FilterData data)
        {
            lock (this.syncRoot)
            {
                return this.table.GetMatchingValue(message, out data);
            }
        }

        public bool GetMatchingValue(MessageBuffer buffer, out FilterData data)
        {
            lock (this.syncRoot)
            {
                return this.table.GetMatchingValue(buffer, out data);
            }
        }

        public bool GetMatchingValues(Message message, ICollection<FilterData> results)
        {
            lock (this.syncRoot)
            {
                return this.table.GetMatchingValues(message, results);
            }
        }

        public bool GetMatchingValues(MessageBuffer buffer, ICollection<FilterData> results)
        {
            lock (this.syncRoot)
            {
                return this.table.GetMatchingValues(buffer, results);
            }
        }

        public bool Remove(MessageFilter key)
        {
            lock (this.syncRoot)
            {
                return this.table.Remove(key);
            }
        }

        void ICollection<KeyValuePair<MessageFilter, FilterData>>.Add(KeyValuePair<MessageFilter, FilterData> item)
        {
            lock (this.syncRoot)
            {
                this.table.Add(item);
            }
        }

        bool ICollection<KeyValuePair<MessageFilter, FilterData>>.Contains(KeyValuePair<MessageFilter, FilterData> item)
        {
            lock (this.syncRoot)
            {
                return this.table.Contains(item);
            }
        }

        void ICollection<KeyValuePair<MessageFilter, FilterData>>.CopyTo(KeyValuePair<MessageFilter, FilterData>[] array, int arrayIndex)
        {
            lock (this.syncRoot)
            {
                this.table.CopyTo(array, arrayIndex);
            }
        }

        bool ICollection<KeyValuePair<MessageFilter, FilterData>>.Remove(KeyValuePair<MessageFilter, FilterData> item)
        {
            lock (this.syncRoot)
            {
                return this.table.Remove(item);
            }
        }

        IEnumerator<KeyValuePair<MessageFilter, FilterData>> IEnumerable<KeyValuePair<MessageFilter, FilterData>>.GetEnumerator()
        {
            lock (this.syncRoot)
            {
                return this.table.GetEnumerator();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            lock (this.syncRoot)
            {
                return ((IEnumerable<KeyValuePair<MessageFilter, FilterData>>) this).GetEnumerator();
            }
        }

        public bool TryGetValue(MessageFilter filter, out FilterData data)
        {
            lock (this.syncRoot)
            {
                return this.table.TryGetValue(filter, out data);
            }
        }

        public int Count
        {
            get
            {
                lock (this.syncRoot)
                {
                    return this.table.Count;
                }
            }
        }

        public int DefaultPriority
        {
            get
            {
                lock (this.syncRoot)
                {
                    return this.table.DefaultPriority;
                }
            }
            set
            {
                lock (this.syncRoot)
                {
                    this.table.DefaultPriority = value;
                }
            }
        }

        public FilterData this[MessageFilter key]
        {
            get
            {
                lock (this.syncRoot)
                {
                    return this.table[key];
                }
            }
            set
            {
                lock (this.syncRoot)
                {
                    this.table[key] = value;
                }
            }
        }

        public ICollection<MessageFilter> Keys
        {
            get
            {
                lock (this.syncRoot)
                {
                    return this.table.Keys;
                }
            }
        }

        internal object SyncRoot
        {
            get
            {
                return this.syncRoot;
            }
        }

        bool ICollection<KeyValuePair<MessageFilter, FilterData>>.IsReadOnly
        {
            get
            {
                lock (this.syncRoot)
                {
                    return this.table.IsReadOnly;
                }
            }
        }

        public ICollection<FilterData> Values
        {
            get
            {
                lock (this.syncRoot)
                {
                    return this.table.Values;
                }
            }
        }
    }
}

