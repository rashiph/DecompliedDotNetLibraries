namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Diagnostics;

    [DataContract]
    public class MessageFilterTable<TFilterData> : IMessageFilterTable<TFilterData>, IDictionary<MessageFilter, TFilterData>, ICollection<KeyValuePair<MessageFilter, TFilterData>>, IEnumerable<KeyValuePair<MessageFilter, TFilterData>>, IEnumerable
    {
        private int defaultPriority;
        private Dictionary<MessageFilter, TFilterData> filters;
        private Dictionary<System.Type, System.Type> filterTypeMappings;
        private static readonly TableEntryComparer<TFilterData> staticComparerInstance;
        private SortedBuffer<FilterTableEntry<TFilterData>, TableEntryComparer<TFilterData>> tables;

        static MessageFilterTable()
        {
            MessageFilterTable<TFilterData>.staticComparerInstance = new TableEntryComparer<TFilterData>();
        }

        public MessageFilterTable() : this(0)
        {
        }

        public MessageFilterTable(int defaultPriority)
        {
            this.Init(defaultPriority);
        }

        public void Add(KeyValuePair<MessageFilter, TFilterData> item)
        {
            this.Add(item.Key, item.Value);
        }

        public void Add(MessageFilter filter, TFilterData data)
        {
            this.Add(filter, data, this.defaultPriority);
        }

        public void Add(MessageFilter filter, TFilterData data, int priority)
        {
            if (filter == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("filter");
            }
            if (this.filters.ContainsKey(filter))
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("filter", System.ServiceModel.SR.GetString("FilterExists"));
            }
            System.Type key = filter.GetType();
            System.Type type2 = null;
            IMessageFilterTable<TFilterData> table = null;
            if (!this.filterTypeMappings.TryGetValue(key, out type2))
            {
                table = this.CreateFilterTable(filter);
                this.ValidateTable(table);
                this.filterTypeMappings.Add(key, table.GetType());
                FilterTableEntry<TFilterData> item = new FilterTableEntry<TFilterData>(priority, table);
                int index = this.tables.IndexOf(item);
                if (index >= 0)
                {
                    table = this.tables[index].table;
                }
                else
                {
                    this.tables.Add(item);
                }
                table.Add(filter, data);
            }
            else
            {
                for (int i = 0; i < this.tables.Count; i++)
                {
                    if ((this.tables[i].priority == priority) && this.tables[i].table.GetType().Equals(type2))
                    {
                        table = this.tables[i].table;
                        break;
                    }
                }
                if (table == null)
                {
                    table = this.CreateFilterTable(filter);
                    this.ValidateTable(table);
                    if (!table.GetType().Equals(type2))
                    {
                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("FilterTableTypeMismatch")));
                    }
                    table.Add(filter, data);
                    this.tables.Add(new FilterTableEntry<TFilterData>(priority, table));
                }
                else
                {
                    table.Add(filter, data);
                }
            }
            this.filters.Add(filter, data);
        }

        public void Clear()
        {
            this.filters.Clear();
            this.tables.Clear();
        }

        public bool Contains(KeyValuePair<MessageFilter, TFilterData> item)
        {
            return this.filters.Contains(item);
        }

        public bool ContainsKey(MessageFilter filter)
        {
            return this.filters.ContainsKey(filter);
        }

        public void CopyTo(KeyValuePair<MessageFilter, TFilterData>[] array, int arrayIndex)
        {
            this.filters.CopyTo(array, arrayIndex);
        }

        private void CreateEmptyTables()
        {
            this.filterTypeMappings = new Dictionary<System.Type, System.Type>();
            this.filters = new Dictionary<MessageFilter, TFilterData>();
            this.tables = new SortedBuffer<FilterTableEntry<TFilterData>, TableEntryComparer<TFilterData>>(MessageFilterTable<TFilterData>.staticComparerInstance);
        }

        protected virtual IMessageFilterTable<TFilterData> CreateFilterTable(MessageFilter filter)
        {
            IMessageFilterTable<TFilterData> table = filter.CreateFilterTable<TFilterData>();
            if (table == null)
            {
                return new SequentialMessageFilterTable<TFilterData>();
            }
            return table;
        }

        public IEnumerator<KeyValuePair<MessageFilter, TFilterData>> GetEnumerator()
        {
            return this.filters.GetEnumerator();
        }

        public bool GetMatchingFilter(Message message, out MessageFilter filter)
        {
            int priority = -2147483648;
            filter = null;
            for (int i = 0; i < this.tables.Count; i++)
            {
                MessageFilter filter2;
                if ((priority > this.tables[i].priority) && (filter != null))
                {
                    break;
                }
                priority = this.tables[i].priority;
                if (this.tables[i].table.GetMatchingFilter(message, out filter2))
                {
                    if (filter != null)
                    {
                        Collection<MessageFilter> filters = new Collection<MessageFilter> {
                            filter,
                            filter2
                        };
                        throw TraceUtility.ThrowHelperError(new MultipleFilterMatchesException(System.ServiceModel.SR.GetString("FilterMultipleMatches"), null, filters), message);
                    }
                    filter = filter2;
                }
            }
            return (filter != null);
        }

        public bool GetMatchingFilter(MessageBuffer buffer, out MessageFilter filter)
        {
            int priority = -2147483648;
            filter = null;
            for (int i = 0; i < this.tables.Count; i++)
            {
                MessageFilter filter2;
                if ((priority > this.tables[i].priority) && (filter != null))
                {
                    break;
                }
                priority = this.tables[i].priority;
                if (this.tables[i].table.GetMatchingFilter(buffer, out filter2))
                {
                    if (filter != null)
                    {
                        Collection<MessageFilter> filters = new Collection<MessageFilter> {
                            filter,
                            filter2
                        };
                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MultipleFilterMatchesException(System.ServiceModel.SR.GetString("FilterMultipleMatches"), null, filters));
                    }
                    filter = filter2;
                }
            }
            return (filter != null);
        }

        public bool GetMatchingFilters(Message message, ICollection<MessageFilter> results)
        {
            if (results == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("results");
            }
            int count = results.Count;
            int priority = -2147483648;
            for (int i = 0; i < this.tables.Count; i++)
            {
                if ((priority > this.tables[i].priority) && (count != results.Count))
                {
                    break;
                }
                priority = this.tables[i].priority;
                this.tables[i].table.GetMatchingFilters(message, results);
            }
            return (count != results.Count);
        }

        public bool GetMatchingFilters(MessageBuffer buffer, ICollection<MessageFilter> results)
        {
            if (results == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("results");
            }
            int count = results.Count;
            int priority = -2147483648;
            for (int i = 0; i < this.tables.Count; i++)
            {
                if ((priority > this.tables[i].priority) && (count != results.Count))
                {
                    break;
                }
                priority = this.tables[i].priority;
                this.tables[i].table.GetMatchingFilters(buffer, results);
            }
            return (count != results.Count);
        }

        public bool GetMatchingValue(Message message, out TFilterData data)
        {
            bool flag = false;
            int priority = -2147483648;
            data = default(TFilterData);
            for (int i = 0; i < this.tables.Count; i++)
            {
                TFilterData local;
                if ((priority > this.tables[i].priority) && flag)
                {
                    return flag;
                }
                priority = this.tables[i].priority;
                if (this.tables[i].table.GetMatchingValue(message, out local))
                {
                    if (flag)
                    {
                        throw TraceUtility.ThrowHelperError(new MultipleFilterMatchesException(System.ServiceModel.SR.GetString("FilterMultipleMatches"), null, null), message);
                    }
                    data = local;
                    flag = true;
                }
            }
            return flag;
        }

        public bool GetMatchingValue(MessageBuffer buffer, out TFilterData data)
        {
            return this.GetMatchingValue(buffer, null, out data);
        }

        internal bool GetMatchingValue(Message message, out TFilterData data, out bool addressMatched)
        {
            bool flag = false;
            int priority = -2147483648;
            data = default(TFilterData);
            addressMatched = false;
            for (int i = 0; i < this.tables.Count; i++)
            {
                bool matchingValue;
                TFilterData local;
                if ((priority > this.tables[i].priority) && flag)
                {
                    return flag;
                }
                priority = this.tables[i].priority;
                IMessageFilterTable<TFilterData> table = this.tables[i].table;
                AndMessageFilterTable<TFilterData> table2 = table as AndMessageFilterTable<TFilterData>;
                if (table2 != null)
                {
                    bool flag3;
                    matchingValue = table2.GetMatchingValue(message, out local, out flag3);
                    addressMatched |= flag3;
                }
                else
                {
                    matchingValue = table.GetMatchingValue(message, out local);
                }
                if (matchingValue)
                {
                    if (flag)
                    {
                        throw TraceUtility.ThrowHelperError(new MultipleFilterMatchesException(System.ServiceModel.SR.GetString("FilterMultipleMatches"), null, null), message);
                    }
                    addressMatched = true;
                    data = local;
                    flag = true;
                }
            }
            return flag;
        }

        internal bool GetMatchingValue(MessageBuffer buffer, Message messageToReadHeaders, out TFilterData data)
        {
            bool flag = false;
            int priority = -2147483648;
            data = default(TFilterData);
            for (int i = 0; i < this.tables.Count; i++)
            {
                TFilterData local;
                if ((priority > this.tables[i].priority) && flag)
                {
                    return flag;
                }
                priority = this.tables[i].priority;
                bool matchingValue = false;
                if ((messageToReadHeaders != null) && (this.tables[i].table is ActionMessageFilterTable<TFilterData>))
                {
                    matchingValue = this.tables[i].table.GetMatchingValue(messageToReadHeaders, out local);
                }
                else
                {
                    matchingValue = this.tables[i].table.GetMatchingValue(buffer, out local);
                }
                if (matchingValue)
                {
                    if (flag)
                    {
                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MultipleFilterMatchesException(System.ServiceModel.SR.GetString("FilterMultipleMatches"), null, null));
                    }
                    data = local;
                    flag = true;
                }
            }
            return flag;
        }

        public bool GetMatchingValues(Message message, ICollection<TFilterData> results)
        {
            if (results == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("results");
            }
            int count = results.Count;
            int priority = -2147483648;
            for (int i = 0; i < this.tables.Count; i++)
            {
                if ((priority > this.tables[i].priority) && (count != results.Count))
                {
                    break;
                }
                priority = this.tables[i].priority;
                this.tables[i].table.GetMatchingValues(message, results);
            }
            return (count != results.Count);
        }

        public bool GetMatchingValues(MessageBuffer buffer, ICollection<TFilterData> results)
        {
            if (results == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("results");
            }
            int count = results.Count;
            int priority = -2147483648;
            for (int i = 0; i < this.tables.Count; i++)
            {
                if ((priority > this.tables[i].priority) && (count != results.Count))
                {
                    break;
                }
                priority = this.tables[i].priority;
                this.tables[i].table.GetMatchingValues(buffer, results);
            }
            return (count != results.Count);
        }

        public int GetPriority(MessageFilter filter)
        {
            TFilterData local1 = this.filters[filter];
            for (int i = 0; i < this.tables.Count; i++)
            {
                if (this.tables[i].table.ContainsKey(filter))
                {
                    return this.tables[i].priority;
                }
            }
            throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new InvalidOperationException(System.ServiceModel.SR.GetString("FilterTableInvalidForLookup")));
        }

        private void Init(int defaultPriority)
        {
            this.CreateEmptyTables();
            this.defaultPriority = defaultPriority;
        }

        [OnDeserializing]
        private void OnDeserializing(StreamingContext context)
        {
            this.Init(0);
        }

        public bool Remove(KeyValuePair<MessageFilter, TFilterData> item)
        {
            return (this.filters.Contains(item) && this.Remove(item.Key));
        }

        public bool Remove(MessageFilter filter)
        {
            for (int i = 0; i < this.tables.Count; i++)
            {
                if (this.tables[i].table.Remove(filter))
                {
                    if (this.tables[i].table.Count == 0)
                    {
                        this.tables.RemoveAt(i);
                    }
                    return this.filters.Remove(filter);
                }
            }
            return false;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public bool TryGetValue(MessageFilter filter, out TFilterData data)
        {
            return this.filters.TryGetValue(filter, out data);
        }

        private void ValidateTable(IMessageFilterTable<TFilterData> table)
        {
            if (base.GetType().IsInstanceOfType(table))
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("FilterBadTableType")));
            }
        }

        public int Count
        {
            get
            {
                return this.filters.Count;
            }
        }

        [DataMember]
        public int DefaultPriority
        {
            get
            {
                return this.defaultPriority;
            }
            set
            {
                this.defaultPriority = value;
            }
        }

        [DataMember]
        private Entry<TFilterData>[] Entries
        {
            get
            {
                Entry<TFilterData>[] entryArray = new Entry<TFilterData>[this.Count];
                int num = 0;
                foreach (KeyValuePair<MessageFilter, TFilterData> pair in this.filters)
                {
                    entryArray[num++] = new Entry<TFilterData>(pair.Key, pair.Value, this.GetPriority(pair.Key));
                }
                return entryArray;
            }
            set
            {
                for (int i = 0; i < value.Length; i++)
                {
                    Entry<TFilterData> entry = value[i];
                    this.Add(entry.filter, entry.data, entry.priority);
                }
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public TFilterData this[MessageFilter filter]
        {
            get
            {
                return this.filters[filter];
            }
            set
            {
                if (this.ContainsKey(filter))
                {
                    int priority = this.GetPriority(filter);
                    this.Remove(filter);
                    this.Add(filter, value, priority);
                }
                else
                {
                    this.Add(filter, value, this.defaultPriority);
                }
            }
        }

        public ICollection<MessageFilter> Keys
        {
            get
            {
                return this.filters.Keys;
            }
        }

        public ICollection<TFilterData> Values
        {
            get
            {
                return (ICollection<TFilterData>) this.filters.Values;
            }
        }

        [DataContract]
        private class Entry
        {
            [DataMember(IsRequired=true)]
            internal TFilterData data;
            [DataMember(IsRequired=true)]
            internal MessageFilter filter;
            [DataMember(IsRequired=true)]
            internal int priority;

            internal Entry(MessageFilter f, TFilterData d, int p)
            {
                this.filter = f;
                this.data = d;
                this.priority = p;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct FilterTableEntry
        {
            internal IMessageFilterTable<TFilterData> table;
            internal int priority;
            internal FilterTableEntry(int pri, IMessageFilterTable<TFilterData> t)
            {
                this.priority = pri;
                this.table = t;
            }
        }

        private class TableEntryComparer : IComparer<MessageFilterTable<TFilterData>.FilterTableEntry>
        {
            public int Compare(MessageFilterTable<TFilterData>.FilterTableEntry x, MessageFilterTable<TFilterData>.FilterTableEntry y)
            {
                int num = y.priority.CompareTo(x.priority);
                if (num != 0)
                {
                    return num;
                }
                return x.table.GetType().FullName.CompareTo(y.table.GetType().FullName);
            }

            public bool Equals(MessageFilterTable<TFilterData>.FilterTableEntry x, MessageFilterTable<TFilterData>.FilterTableEntry y)
            {
                if (y.priority.CompareTo(x.priority) != 0)
                {
                    return false;
                }
                return x.table.GetType().FullName.Equals(y.table.GetType().FullName);
            }

            public int GetHashCode(MessageFilterTable<TFilterData>.FilterTableEntry table)
            {
                return table.GetHashCode();
            }
        }
    }
}

