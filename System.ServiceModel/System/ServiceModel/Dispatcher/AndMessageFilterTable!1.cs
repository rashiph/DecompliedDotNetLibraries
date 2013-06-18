namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Diagnostics;

    internal class AndMessageFilterTable<FilterData> : IMessageFilterTable<FilterData>, IDictionary<MessageFilter, FilterData>, ICollection<KeyValuePair<MessageFilter, FilterData>>, IEnumerable<KeyValuePair<MessageFilter, FilterData>>, IEnumerable
    {
        private Dictionary<MessageFilter, FilterDataPair<FilterData>> filterData;
        private Dictionary<MessageFilter, FilterData> filters;
        private MessageFilterTable<FilterDataPair<FilterData>> table;

        public AndMessageFilterTable()
        {
            this.filters = new Dictionary<MessageFilter, FilterData>();
            this.filterData = new Dictionary<MessageFilter, FilterDataPair<FilterData>>();
            this.table = new MessageFilterTable<FilterDataPair<FilterData>>();
        }

        public void Add(KeyValuePair<MessageFilter, FilterData> item)
        {
            this.Add(item.Key, item.Value);
        }

        public void Add(AndMessageFilter filter, FilterData data)
        {
            if (filter == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("filter");
            }
            this.filters.Add(filter, data);
            FilterDataPair<FilterData> pair = new FilterDataPair<FilterData>(filter, data);
            this.filterData.Add(filter, pair);
            this.table.Add(filter.Filter1, pair);
        }

        public void Add(MessageFilter filter, FilterData data)
        {
            if (filter == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("filter");
            }
            this.Add((AndMessageFilter) filter, data);
        }

        public void Clear()
        {
            this.filters.Clear();
            this.filterData.Clear();
            this.table.Clear();
        }

        public bool Contains(KeyValuePair<MessageFilter, FilterData> item)
        {
            return this.filters.Contains(item);
        }

        public bool ContainsKey(MessageFilter filter)
        {
            if (filter == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("filter");
            }
            return this.filters.ContainsKey(filter);
        }

        public void CopyTo(KeyValuePair<MessageFilter, FilterData>[] array, int arrayIndex)
        {
            this.filters.CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<MessageFilter, FilterData>> GetEnumerator()
        {
            return (IEnumerator<KeyValuePair<MessageFilter, FilterData>>) this.filters.GetEnumerator();
        }

        public bool GetMatchingFilter(Message message, out MessageFilter filter)
        {
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }
            FilterDataPair<FilterData> pair = this.InnerMatch(message);
            if (pair == null)
            {
                filter = null;
                return false;
            }
            filter = pair.filter;
            return true;
        }

        public bool GetMatchingFilter(MessageBuffer messageBuffer, out MessageFilter filter)
        {
            if (messageBuffer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("messageBuffer");
            }
            FilterDataPair<FilterData> pair = this.InnerMatch(messageBuffer);
            if (pair == null)
            {
                filter = null;
                return false;
            }
            filter = pair.filter;
            return true;
        }

        public bool GetMatchingFilters(Message message, ICollection<MessageFilter> results)
        {
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }
            if (results == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("results");
            }
            int count = results.Count;
            this.InnerMatch(message, results);
            return (count != results.Count);
        }

        public bool GetMatchingFilters(MessageBuffer messageBuffer, ICollection<MessageFilter> results)
        {
            if (messageBuffer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("messageBuffer");
            }
            if (results == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("results");
            }
            int count = results.Count;
            this.InnerMatch(messageBuffer, results);
            return (count != results.Count);
        }

        public bool GetMatchingValue(Message message, out FilterData data)
        {
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }
            FilterDataPair<FilterData> pair = this.InnerMatch(message);
            if (pair == null)
            {
                data = default(FilterData);
                return false;
            }
            data = pair.data;
            return true;
        }

        public bool GetMatchingValue(MessageBuffer messageBuffer, out FilterData data)
        {
            if (messageBuffer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("messageBuffer");
            }
            FilterDataPair<FilterData> pair = this.InnerMatch(messageBuffer);
            if (pair == null)
            {
                data = default(FilterData);
                return false;
            }
            data = pair.data;
            return true;
        }

        internal bool GetMatchingValue(Message message, out FilterData data, out bool addressMatched)
        {
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }
            List<FilterDataPair<FilterData>> results = new List<FilterDataPair<FilterData>>();
            addressMatched = this.table.GetMatchingValues(message, results);
            FilterDataPair<FilterData> pair = null;
            for (int i = 0; i < results.Count; i++)
            {
                if (results[i].filter.Filter2.Match(message))
                {
                    if (pair != null)
                    {
                        Collection<MessageFilter> filters = new Collection<MessageFilter> {
                            pair.filter,
                            results[i].filter
                        };
                        throw TraceUtility.ThrowHelperError(new MultipleFilterMatchesException(System.ServiceModel.SR.GetString("FilterMultipleMatches"), null, filters), message);
                    }
                    pair = results[i];
                }
            }
            if (pair == null)
            {
                data = default(FilterData);
                return false;
            }
            data = pair.data;
            return true;
        }

        public bool GetMatchingValues(Message message, ICollection<FilterData> results)
        {
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }
            if (results == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("results");
            }
            int count = results.Count;
            this.InnerMatchData(message, results);
            return (count != results.Count);
        }

        public bool GetMatchingValues(MessageBuffer messageBuffer, ICollection<FilterData> results)
        {
            if (messageBuffer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("messageBuffer");
            }
            if (results == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("results");
            }
            int count = results.Count;
            this.InnerMatchData(messageBuffer, results);
            return (count != results.Count);
        }

        private FilterDataPair<FilterData> InnerMatch(Message message)
        {
            List<FilterDataPair<FilterData>> results = new List<FilterDataPair<FilterData>>();
            this.table.GetMatchingValues(message, results);
            FilterDataPair<FilterData> pair = null;
            for (int i = 0; i < results.Count; i++)
            {
                if (results[i].filter.Filter2.Match(message))
                {
                    if (pair != null)
                    {
                        Collection<MessageFilter> filters = new Collection<MessageFilter> {
                            pair.filter,
                            results[i].filter
                        };
                        throw TraceUtility.ThrowHelperError(new MultipleFilterMatchesException(System.ServiceModel.SR.GetString("FilterMultipleMatches"), null, filters), message);
                    }
                    pair = results[i];
                }
            }
            return pair;
        }

        private FilterDataPair<FilterData> InnerMatch(MessageBuffer messageBuffer)
        {
            List<FilterDataPair<FilterData>> results = new List<FilterDataPair<FilterData>>();
            this.table.GetMatchingValues(messageBuffer, results);
            FilterDataPair<FilterData> pair = null;
            for (int i = 0; i < results.Count; i++)
            {
                if (results[i].filter.Filter2.Match(messageBuffer))
                {
                    if (pair != null)
                    {
                        Collection<MessageFilter> filters = new Collection<MessageFilter> {
                            pair.filter,
                            results[i].filter
                        };
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MultipleFilterMatchesException(System.ServiceModel.SR.GetString("FilterMultipleMatches"), null, filters));
                    }
                    pair = results[i];
                }
            }
            return pair;
        }

        private void InnerMatch(Message message, ICollection<MessageFilter> results)
        {
            List<FilterDataPair<FilterData>> list = new List<FilterDataPair<FilterData>>();
            this.table.GetMatchingValues(message, list);
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].filter.Filter2.Match(message))
                {
                    results.Add(list[i].filter);
                }
            }
        }

        private void InnerMatch(MessageBuffer messageBuffer, ICollection<MessageFilter> results)
        {
            List<FilterDataPair<FilterData>> list = new List<FilterDataPair<FilterData>>();
            this.table.GetMatchingValues(messageBuffer, list);
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].filter.Filter2.Match(messageBuffer))
                {
                    results.Add(list[i].filter);
                }
            }
        }

        private void InnerMatchData(Message message, ICollection<FilterData> results)
        {
            List<FilterDataPair<FilterData>> list = new List<FilterDataPair<FilterData>>();
            this.table.GetMatchingValues(message, list);
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].filter.Filter2.Match(message))
                {
                    results.Add(list[i].data);
                }
            }
        }

        private void InnerMatchData(MessageBuffer messageBuffer, ICollection<FilterData> results)
        {
            List<FilterDataPair<FilterData>> list = new List<FilterDataPair<FilterData>>();
            this.table.GetMatchingValues(messageBuffer, list);
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].filter.Filter2.Match(messageBuffer))
                {
                    results.Add(list[i].data);
                }
            }
        }

        public bool Remove(AndMessageFilter filter)
        {
            if (filter == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("filter");
            }
            if (this.filters.Remove(filter))
            {
                this.filterData.Remove(filter);
                this.table.Remove(filter.Filter1);
                return true;
            }
            return false;
        }

        public bool Remove(MessageFilter filter)
        {
            if (filter == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("filter");
            }
            AndMessageFilter filter2 = filter as AndMessageFilter;
            return ((filter2 != null) && this.Remove(filter2));
        }

        public bool Remove(KeyValuePair<MessageFilter, FilterData> item)
        {
            return (this.filters.Contains(item) && this.Remove(item.Key));
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public bool TryGetValue(MessageFilter filter, out FilterData data)
        {
            return this.filters.TryGetValue(filter, out data);
        }

        public int Count
        {
            get
            {
                return this.filters.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public FilterData this[MessageFilter filter]
        {
            get
            {
                return this.filters[filter];
            }
            set
            {
                if (this.filters.ContainsKey(filter))
                {
                    this.filters[filter] = value;
                    this.filterData[filter].data = value;
                }
                else
                {
                    this.Add(filter, value);
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

        public ICollection<FilterData> Values
        {
            get
            {
                return (ICollection<FilterData>) this.filters.Values;
            }
        }

        internal class FilterDataPair
        {
            internal FilterData data;
            internal AndMessageFilter filter;

            internal FilterDataPair(AndMessageFilter filter, FilterData data)
            {
                this.filter = filter;
                this.data = data;
            }
        }
    }
}

