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
    using System.Xml.XPath;

    [DataContract]
    public class XPathMessageFilterTable<TFilterData> : IMessageFilterTable<TFilterData>, IDictionary<MessageFilter, TFilterData>, ICollection<KeyValuePair<MessageFilter, TFilterData>>, IEnumerable<KeyValuePair<MessageFilter, TFilterData>>, IEnumerable
    {
        internal Dictionary<MessageFilter, TFilterData> filters;
        private InverseQueryMatcher iqMatcher;

        public XPathMessageFilterTable()
        {
            this.Init(-1);
        }

        public XPathMessageFilterTable(int capacity)
        {
            if (capacity < 0)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("capacity", capacity, System.ServiceModel.SR.GetString("FilterCapacityNegative")));
            }
            this.Init(capacity);
        }

        public void Add(KeyValuePair<MessageFilter, TFilterData> item)
        {
            this.Add(item.Key, item.Value);
        }

        public void Add(MessageFilter filter, TFilterData data)
        {
            this.Add((XPathMessageFilter) filter, data);
        }

        public void Add(XPathMessageFilter filter, TFilterData data)
        {
            this.Add(filter, data, false);
        }

        internal void Add(XPathMessageFilter filter, TFilterData data, bool forceExternal)
        {
            if (filter == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("filter");
            }
            this.filters.Add(filter, data);
            this.iqMatcher.Add(filter.XPath, filter.Namespaces, filter, forceExternal);
        }

        public void Clear()
        {
            this.iqMatcher.Clear();
            this.filters.Clear();
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

        public IEnumerator<KeyValuePair<MessageFilter, TFilterData>> GetEnumerator()
        {
            return this.filters.GetEnumerator();
        }

        public bool GetMatchingFilter(Message message, out MessageFilter filter)
        {
            Collection<MessageFilter> results = new Collection<MessageFilter>();
            this.GetMatchingFilters(message, results);
            if (results.Count > 1)
            {
                throw TraceUtility.ThrowHelperError(new MultipleFilterMatchesException(System.ServiceModel.SR.GetString("FilterMultipleMatches"), null, results), message);
            }
            if (results.Count == 1)
            {
                filter = results[0];
                return true;
            }
            filter = null;
            return false;
        }

        public bool GetMatchingFilter(MessageBuffer messageBuffer, out MessageFilter filter)
        {
            Collection<MessageFilter> results = new Collection<MessageFilter>();
            this.GetMatchingFilters(messageBuffer, results);
            if (results.Count > 1)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MultipleFilterMatchesException(System.ServiceModel.SR.GetString("FilterMultipleMatches"), null, results));
            }
            if (results.Count == 1)
            {
                filter = results[0];
                return true;
            }
            filter = null;
            return false;
        }

        public bool GetMatchingFilter(SeekableXPathNavigator navigator, out MessageFilter filter)
        {
            Collection<MessageFilter> results = new Collection<MessageFilter>();
            this.GetMatchingFilters(navigator, results);
            if (results.Count > 1)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MultipleFilterMatchesException(System.ServiceModel.SR.GetString("FilterMultipleMatches"), null, results));
            }
            if (results.Count == 1)
            {
                filter = results[0];
                return true;
            }
            filter = null;
            return false;
        }

        public bool GetMatchingFilter(XPathNavigator navigator, out MessageFilter filter)
        {
            Collection<MessageFilter> results = new Collection<MessageFilter>();
            this.GetMatchingFilters(navigator, results);
            if (results.Count > 1)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MultipleFilterMatchesException(System.ServiceModel.SR.GetString("FilterMultipleMatches"), null, results));
            }
            if (results.Count == 1)
            {
                filter = results[0];
                return true;
            }
            filter = null;
            return false;
        }

        public bool GetMatchingFilters(Message message, ICollection<MessageFilter> results)
        {
            if (message == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }
            if (results == null)
            {
                throw TraceUtility.ThrowHelperArgumentNull("results", message);
            }
            if (this.CanMatch)
            {
                int count = results.Count;
                this.iqMatcher.ReleaseResult(this.iqMatcher.Match(message, false, results));
                return (count != results.Count);
            }
            return false;
        }

        public bool GetMatchingFilters(MessageBuffer messageBuffer, ICollection<MessageFilter> results)
        {
            if (messageBuffer == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("messageBuffer");
            }
            if (results == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("results");
            }
            if (this.CanMatch)
            {
                int count = results.Count;
                this.iqMatcher.ReleaseResult(this.iqMatcher.Match(messageBuffer, results));
                return (count != results.Count);
            }
            return false;
        }

        public bool GetMatchingFilters(SeekableXPathNavigator navigator, ICollection<MessageFilter> results)
        {
            if (navigator == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("navigator");
            }
            if (results == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("results");
            }
            if (this.CanMatch)
            {
                int count = results.Count;
                this.iqMatcher.ReleaseResult(this.iqMatcher.Match(navigator, results));
                return (count != results.Count);
            }
            return false;
        }

        public bool GetMatchingFilters(XPathNavigator navigator, ICollection<MessageFilter> results)
        {
            if (navigator == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("navigator");
            }
            if (results == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("results");
            }
            if (this.CanMatch)
            {
                int count = results.Count;
                this.iqMatcher.ReleaseResult(this.iqMatcher.Match(navigator, results));
                return (count != results.Count);
            }
            return false;
        }

        public bool GetMatchingValue(Message message, out TFilterData data)
        {
            if (message == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }
            if (this.CanMatch)
            {
                return this.ProcessMatch(this.iqMatcher.Match(message, false, null), out data);
            }
            data = default(TFilterData);
            return false;
        }

        public bool GetMatchingValue(MessageBuffer messageBuffer, out TFilterData data)
        {
            if (messageBuffer == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("messageBuffer");
            }
            if (this.CanMatch)
            {
                return this.ProcessMatch(this.iqMatcher.Match(messageBuffer, null), out data);
            }
            data = default(TFilterData);
            return false;
        }

        public bool GetMatchingValue(SeekableXPathNavigator navigator, out TFilterData data)
        {
            if (navigator == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("navigator");
            }
            if (this.CanMatch)
            {
                return this.ProcessMatch(this.iqMatcher.Match(navigator, null), out data);
            }
            data = default(TFilterData);
            return false;
        }

        public bool GetMatchingValue(XPathNavigator navigator, out TFilterData data)
        {
            if (navigator == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("navigator");
            }
            if (this.CanMatch)
            {
                return this.ProcessMatch(this.iqMatcher.Match(navigator, null), out data);
            }
            data = default(TFilterData);
            return false;
        }

        public bool GetMatchingValues(Message message, ICollection<TFilterData> results)
        {
            if (message == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }
            if (results == null)
            {
                throw TraceUtility.ThrowHelperArgumentNull("results", message);
            }
            if (this.CanMatch)
            {
                int count = results.Count;
                this.ProcessMatches(this.iqMatcher.Match(message, false, null), results);
                return (count != results.Count);
            }
            return false;
        }

        public bool GetMatchingValues(MessageBuffer messageBuffer, ICollection<TFilterData> results)
        {
            if (messageBuffer == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("messageBuffer");
            }
            if (results == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("results");
            }
            if (this.CanMatch)
            {
                int count = results.Count;
                this.ProcessMatches(this.iqMatcher.Match(messageBuffer, null), results);
                return (count != results.Count);
            }
            return false;
        }

        public bool GetMatchingValues(SeekableXPathNavigator navigator, ICollection<TFilterData> results)
        {
            if (navigator == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("navigator");
            }
            if (results == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("results");
            }
            if (this.CanMatch)
            {
                int count = results.Count;
                this.ProcessMatches(this.iqMatcher.Match(navigator, null), results);
                return (count != results.Count);
            }
            return false;
        }

        public bool GetMatchingValues(XPathNavigator navigator, ICollection<TFilterData> results)
        {
            if (navigator == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("navigator");
            }
            if (results == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("results");
            }
            if (this.CanMatch)
            {
                int count = results.Count;
                this.ProcessMatches(this.iqMatcher.Match(navigator, null), results);
                return (count != results.Count);
            }
            return false;
        }

        private void Init(int capacity)
        {
            if (capacity <= 0)
            {
                this.filters = new Dictionary<MessageFilter, TFilterData>();
            }
            else
            {
                this.filters = new Dictionary<MessageFilter, TFilterData>(capacity);
            }
            if (this.iqMatcher == null)
            {
                this.iqMatcher = new InverseQueryMatcher(true);
            }
        }

        [OnDeserializing]
        private void OnDeserializing(StreamingContext context)
        {
            this.Init(-1);
        }

        private bool ProcessMatch(FilterResult result, out TFilterData data)
        {
            bool flag = false;
            data = default(TFilterData);
            MessageFilter singleMatch = result.GetSingleMatch();
            if (singleMatch != null)
            {
                data = this.filters[singleMatch];
                flag = true;
            }
            this.iqMatcher.ReleaseResult(result);
            return flag;
        }

        private void ProcessMatches(FilterResult result, ICollection<TFilterData> results)
        {
            Collection<MessageFilter> matchList = result.Processor.MatchList;
            int num = 0;
            int count = matchList.Count;
            while (num < count)
            {
                results.Add(this.filters[matchList[num]]);
                num++;
            }
            this.iqMatcher.ReleaseResult(result);
        }

        public bool Remove(XPathMessageFilter filter)
        {
            if (filter == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("filter");
            }
            if (this.filters.Remove(filter))
            {
                this.iqMatcher.Remove(filter);
                return true;
            }
            return false;
        }

        public bool Remove(MessageFilter filter)
        {
            if (filter == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("filter");
            }
            XPathMessageFilter filter2 = filter as XPathMessageFilter;
            return ((filter2 != null) && this.Remove(filter2));
        }

        public bool Remove(KeyValuePair<MessageFilter, TFilterData> item)
        {
            if (this.filters.Remove(item))
            {
                this.iqMatcher.Remove((XPathMessageFilter) item.Key);
                return true;
            }
            return false;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public void TrimToSize()
        {
            this.iqMatcher.Trim();
        }

        public bool TryGetValue(MessageFilter filter, out TFilterData data)
        {
            return this.filters.TryGetValue(filter, out data);
        }

        private bool CanMatch
        {
            get
            {
                return ((this.filters.Count > 0) && (null != this.iqMatcher));
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
        private Entry<TFilterData>[] Entries
        {
            get
            {
                Entry<TFilterData>[] entryArray = new Entry<TFilterData>[this.Count];
                int num = 0;
                foreach (KeyValuePair<MessageFilter, TFilterData> pair in this.filters)
                {
                    entryArray[num++] = new Entry<TFilterData>(pair.Key, pair.Value);
                }
                return entryArray;
            }
            set
            {
                this.Init(value.Length);
                for (int i = 0; i < value.Length; i++)
                {
                    this.Add(value[i].filter, value[i].data);
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
                if (this.filters.ContainsKey(filter))
                {
                    this.filters[filter] = value;
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

        [DataMember]
        public int NodeQuota
        {
            get
            {
                return this.iqMatcher.NodeQuota;
            }
            set
            {
                if (value <= 0)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("NodeQuota", value, System.ServiceModel.SR.GetString("FilterQuotaRange")));
                }
                if (this.iqMatcher == null)
                {
                    this.iqMatcher = new InverseQueryMatcher(true);
                }
                this.iqMatcher.NodeQuota = value;
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

            internal Entry(MessageFilter f, TFilterData d)
            {
                this.filter = f;
                this.data = d;
            }
        }
    }
}

