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

    internal class EndpointAddressMessageFilterTable<TFilterData> : IMessageFilterTable<TFilterData>, IDictionary<MessageFilter, TFilterData>, ICollection<KeyValuePair<MessageFilter, TFilterData>>, IEnumerable<KeyValuePair<MessageFilter, TFilterData>>, IEnumerable
    {
        protected Dictionary<MessageFilter, Candidate<TFilterData>> candidates;
        protected Dictionary<MessageFilter, TFilterData> filters;
        private Dictionary<string, EndpointAddressProcessor.HeaderBit[]> headerLookup;
        private int nextBit;
        private WeakReference processorPool;
        private int size;
        private Dictionary<Uri, CandidateSet<TFilterData>> toHostLookup;
        private Dictionary<Uri, CandidateSet<TFilterData>> toNoHostLookup;

        public EndpointAddressMessageFilterTable()
        {
            this.processorPool = new WeakReference(null);
            this.size = 0;
            this.nextBit = 0;
            this.filters = new Dictionary<MessageFilter, TFilterData>();
            this.candidates = new Dictionary<MessageFilter, Candidate<TFilterData>>();
            this.headerLookup = new Dictionary<string, EndpointAddressProcessor.HeaderBit[]>();
            this.InitializeLookupTables();
        }

        public void Add(KeyValuePair<MessageFilter, TFilterData> item)
        {
            this.Add(item.Key, item.Value);
        }

        public virtual void Add(EndpointAddressMessageFilter filter, TFilterData data)
        {
            CandidateSet<TFilterData> set;
            if (filter == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("filter");
            }
            this.filters.Add(filter, data);
            byte[] mask = this.BuildMask(filter.HeaderLookup);
            Candidate<TFilterData> candidate = new Candidate<TFilterData>(filter, data, mask, filter.HeaderLookup);
            this.candidates.Add(filter, candidate);
            Uri key = filter.Address.Uri;
            if (filter.IncludeHostNameInComparison)
            {
                if (!this.toHostLookup.TryGetValue(key, out set))
                {
                    set = new CandidateSet<TFilterData>();
                    this.toHostLookup.Add(key, set);
                }
            }
            else if (!this.toNoHostLookup.TryGetValue(key, out set))
            {
                set = new CandidateSet<TFilterData>();
                this.toNoHostLookup.Add(key, set);
            }
            set.candidates.Add(candidate);
            this.IncrementQNameCount(set, filter.Address);
        }

        public virtual void Add(MessageFilter filter, TFilterData data)
        {
            if (filter == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("filter");
            }
            this.Add((EndpointAddressMessageFilter) filter, data);
        }

        protected byte[] BuildMask(Dictionary<string, EndpointAddressProcessor.HeaderBit[]> headerLookup)
        {
            byte[] mask = null;
            foreach (KeyValuePair<string, EndpointAddressProcessor.HeaderBit[]> pair in headerLookup)
            {
                EndpointAddressProcessor.HeaderBit[] bitArray;
                if (this.headerLookup.TryGetValue(pair.Key, out bitArray))
                {
                    if (bitArray.Length < pair.Value.Length)
                    {
                        int length = bitArray.Length;
                        Array.Resize<EndpointAddressProcessor.HeaderBit>(ref bitArray, pair.Value.Length);
                        for (int j = length; j < pair.Value.Length; j++)
                        {
                            bitArray[j] = new EndpointAddressProcessor.HeaderBit(this.nextBit++);
                        }
                        this.headerLookup[pair.Key] = bitArray;
                    }
                }
                else
                {
                    bitArray = new EndpointAddressProcessor.HeaderBit[pair.Value.Length];
                    for (int k = 0; k < pair.Value.Length; k++)
                    {
                        bitArray[k] = new EndpointAddressProcessor.HeaderBit(this.nextBit++);
                    }
                    this.headerLookup.Add(pair.Key, bitArray);
                }
                for (int i = 0; i < pair.Value.Length; i++)
                {
                    bitArray[i].AddToMask(ref mask);
                }
            }
            if (this.nextBit == 0)
            {
                this.size = 0;
                return mask;
            }
            this.size = ((this.nextBit - 1) / 8) + 1;
            return mask;
        }

        public void Clear()
        {
            this.size = 0;
            this.nextBit = 0;
            this.filters.Clear();
            this.candidates.Clear();
            this.headerLookup.Clear();
            this.ClearLookupTables();
        }

        protected virtual void ClearLookupTables()
        {
            if (this.toHostLookup != null)
            {
                this.toHostLookup.Clear();
            }
            if (this.toNoHostLookup != null)
            {
                this.toNoHostLookup.Clear();
            }
        }

        public bool Contains(KeyValuePair<MessageFilter, TFilterData> item)
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

        public void CopyTo(KeyValuePair<MessageFilter, TFilterData>[] array, int arrayIndex)
        {
            this.filters.CopyTo(array, arrayIndex);
        }

        private EndpointAddressProcessor CreateProcessor(int length)
        {
            EndpointAddressProcessor processor = null;
            lock (this.processorPool)
            {
                ProcessorPool<TFilterData> target = this.processorPool.Target as ProcessorPool<TFilterData>;
                if (target != null)
                {
                    processor = target.Pop();
                }
            }
            if (processor != null)
            {
                processor.Clear(length);
                return processor;
            }
            return new EndpointAddressProcessor(length);
        }

        protected void DecrementQNameCount(CandidateSet<TFilterData> cset, EndpointAddress address)
        {
            for (int i = 0; i < address.Headers.Count; i++)
            {
                EndpointAddressProcessor.QName name;
                AddressHeader header = address.Headers[i];
                name.name = header.Name;
                name.ns = header.Namespace;
                int num2 = cset.qnames[name];
                if (num2 == 1)
                {
                    cset.qnames.Remove(name);
                }
                else
                {
                    cset.qnames[name] = num2 - 1;
                }
            }
        }

        public IEnumerator<KeyValuePair<MessageFilter, TFilterData>> GetEnumerator()
        {
            return (IEnumerator<KeyValuePair<MessageFilter, TFilterData>>) this.filters.GetEnumerator();
        }

        public bool GetMatchingFilter(Message message, out MessageFilter filter)
        {
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }
            Candidate<TFilterData> candidate = this.InnerMatch(message);
            if (candidate != null)
            {
                filter = candidate.filter;
                return true;
            }
            filter = null;
            return false;
        }

        public bool GetMatchingFilter(MessageBuffer messageBuffer, out MessageFilter filter)
        {
            if (messageBuffer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("messageBuffer");
            }
            Message message = messageBuffer.CreateMessage();
            Candidate<TFilterData> candidate = null;
            try
            {
                candidate = this.InnerMatch(message);
            }
            finally
            {
                message.Close();
            }
            if (candidate != null)
            {
                filter = candidate.filter;
                return true;
            }
            filter = null;
            return false;
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
            this.InnerMatchFilters(message, results);
            return (count != results.Count);
        }

        public bool GetMatchingFilters(MessageBuffer messageBuffer, ICollection<MessageFilter> results)
        {
            bool flag;
            if (messageBuffer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("messageBuffer");
            }
            if (results == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("results");
            }
            Message message = messageBuffer.CreateMessage();
            try
            {
                int count = results.Count;
                this.InnerMatchFilters(message, results);
                flag = count != results.Count;
            }
            finally
            {
                message.Close();
            }
            return flag;
        }

        public bool GetMatchingValue(Message message, out TFilterData data)
        {
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }
            Candidate<TFilterData> candidate = this.InnerMatch(message);
            if (candidate == null)
            {
                data = default(TFilterData);
                return false;
            }
            data = candidate.data;
            return true;
        }

        public bool GetMatchingValue(MessageBuffer messageBuffer, out TFilterData data)
        {
            if (messageBuffer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("messageBuffer");
            }
            Message message = messageBuffer.CreateMessage();
            Candidate<TFilterData> candidate = null;
            try
            {
                candidate = this.InnerMatch(message);
            }
            finally
            {
                message.Close();
            }
            if (candidate == null)
            {
                data = default(TFilterData);
                return false;
            }
            data = candidate.data;
            return true;
        }

        public bool GetMatchingValues(Message message, ICollection<TFilterData> results)
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

        public bool GetMatchingValues(MessageBuffer messageBuffer, ICollection<TFilterData> results)
        {
            bool flag;
            if (messageBuffer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("messageBuffer");
            }
            if (results == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("results");
            }
            Message message = messageBuffer.CreateMessage();
            try
            {
                int count = results.Count;
                this.InnerMatchData(message, results);
                flag = count != results.Count;
            }
            finally
            {
                message.Close();
            }
            return flag;
        }

        private Candidate<TFilterData> GetSingleMatch(CandidateSet<TFilterData> cset, Message message)
        {
            int count = cset.candidates.Count;
            if (cset.qnames.Count == 0)
            {
                switch (count)
                {
                    case 0:
                        return null;

                    case 1:
                        return cset.candidates[0];
                }
                Collection<MessageFilter> filters = new Collection<MessageFilter>();
                for (int j = 0; j < count; j++)
                {
                    filters.Add(cset.candidates[j].filter);
                }
                throw TraceUtility.ThrowHelperError(new MultipleFilterMatchesException(System.ServiceModel.SR.GetString("FilterMultipleMatches"), null, filters), message);
            }
            EndpointAddressProcessor processor = this.CreateProcessor(this.size);
            processor.ProcessHeaders(message, cset.qnames, this.headerLookup);
            Candidate<TFilterData> candidate = null;
            List<Candidate<TFilterData>> candidates = cset.candidates;
            for (int i = 0; i < count; i++)
            {
                if (processor.TestMask(candidates[i].mask))
                {
                    if (candidate != null)
                    {
                        Collection<MessageFilter> collection2 = new Collection<MessageFilter> {
                            candidate.filter,
                            candidates[i].filter
                        };
                        throw TraceUtility.ThrowHelperError(new MultipleFilterMatchesException(System.ServiceModel.SR.GetString("FilterMultipleMatches"), null, collection2), message);
                    }
                    candidate = candidates[i];
                }
            }
            this.ReleaseProcessor(processor);
            return candidate;
        }

        protected void IncrementQNameCount(CandidateSet<TFilterData> cset, EndpointAddress address)
        {
            for (int i = 0; i < address.Headers.Count; i++)
            {
                EndpointAddressProcessor.QName name;
                int num;
                AddressHeader header = address.Headers[i];
                name.name = header.Name;
                name.ns = header.Namespace;
                if (cset.qnames.TryGetValue(name, out num))
                {
                    cset.qnames[name] = num + 1;
                }
                else
                {
                    cset.qnames.Add(name, 1);
                }
            }
        }

        protected virtual void InitializeLookupTables()
        {
            this.toHostLookup = new Dictionary<Uri, CandidateSet<TFilterData>>(EndpointAddressMessageFilter.HostUriComparer.Value);
            this.toNoHostLookup = new Dictionary<Uri, CandidateSet<TFilterData>>(EndpointAddressMessageFilter.NoHostUriComparer.Value);
        }

        private Candidate<TFilterData> InnerMatch(Message message)
        {
            Uri to = message.Headers.To;
            if (to == null)
            {
                return null;
            }
            CandidateSet<TFilterData> cset = null;
            Candidate<TFilterData> candidate = null;
            if (this.TryMatchCandidateSet(to, true, out cset))
            {
                candidate = this.GetSingleMatch(cset, message);
            }
            if (!this.TryMatchCandidateSet(to, false, out cset))
            {
                return candidate;
            }
            Candidate<TFilterData> singleMatch = this.GetSingleMatch(cset, message);
            if (singleMatch == null)
            {
                return candidate;
            }
            if (candidate != null)
            {
                Collection<MessageFilter> filters = new Collection<MessageFilter> {
                    candidate.filter,
                    singleMatch.filter
                };
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MultipleFilterMatchesException(System.ServiceModel.SR.GetString("FilterMultipleMatches"), null, filters));
            }
            return singleMatch;
        }

        private void InnerMatchData(Message message, ICollection<TFilterData> results)
        {
            Uri to = message.Headers.To;
            if (to != null)
            {
                CandidateSet<TFilterData> set;
                if (this.TryMatchCandidateSet(to, true, out set))
                {
                    this.InnerMatchData(message, results, set);
                }
                if (this.TryMatchCandidateSet(to, false, out set))
                {
                    this.InnerMatchData(message, results, set);
                }
            }
        }

        private void InnerMatchData(Message message, ICollection<TFilterData> results, CandidateSet<TFilterData> cset)
        {
            EndpointAddressProcessor processor = this.CreateProcessor(this.size);
            processor.ProcessHeaders(message, cset.qnames, this.headerLookup);
            List<Candidate<TFilterData>> candidates = cset.candidates;
            for (int i = 0; i < candidates.Count; i++)
            {
                if (processor.TestMask(candidates[i].mask))
                {
                    results.Add(candidates[i].data);
                }
            }
            this.ReleaseProcessor(processor);
        }

        protected void InnerMatchFilters(Message message, ICollection<MessageFilter> results)
        {
            Uri to = message.Headers.To;
            if (to != null)
            {
                CandidateSet<TFilterData> set;
                if (this.TryMatchCandidateSet(to, true, out set))
                {
                    this.InnerMatchFilters(message, results, set);
                }
                if (this.TryMatchCandidateSet(to, false, out set))
                {
                    this.InnerMatchFilters(message, results, set);
                }
            }
        }

        private void InnerMatchFilters(Message message, ICollection<MessageFilter> results, CandidateSet<TFilterData> cset)
        {
            EndpointAddressProcessor processor = this.CreateProcessor(this.size);
            processor.ProcessHeaders(message, cset.qnames, this.headerLookup);
            List<Candidate<TFilterData>> candidates = cset.candidates;
            for (int i = 0; i < candidates.Count; i++)
            {
                if (processor.TestMask(candidates[i].mask))
                {
                    results.Add(candidates[i].filter);
                }
            }
            this.ReleaseProcessor(processor);
        }

        protected void RebuildMasks()
        {
            this.nextBit = 0;
            this.size = 0;
            this.headerLookup.Clear();
            foreach (Candidate<TFilterData> candidate in this.candidates.Values)
            {
                candidate.mask = this.BuildMask(candidate.headerLookup);
            }
        }

        private void ReleaseProcessor(EndpointAddressProcessor processor)
        {
            lock (this.processorPool)
            {
                ProcessorPool<TFilterData> target = this.processorPool.Target as ProcessorPool<TFilterData>;
                if (target == null)
                {
                    target = new ProcessorPool<TFilterData>();
                    this.processorPool.Target = target;
                }
                target.Push(processor);
            }
        }

        public virtual bool Remove(EndpointAddressMessageFilter filter)
        {
            if (filter == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("filter");
            }
            if (!this.filters.Remove(filter))
            {
                return false;
            }
            Candidate<TFilterData> item = this.candidates[filter];
            Uri key = filter.Address.Uri;
            CandidateSet<TFilterData> cset = null;
            if (filter.IncludeHostNameInComparison)
            {
                cset = this.toHostLookup[key];
            }
            else
            {
                cset = this.toNoHostLookup[key];
            }
            this.candidates.Remove(filter);
            if (cset.candidates.Count == 1)
            {
                if (filter.IncludeHostNameInComparison)
                {
                    this.toHostLookup.Remove(key);
                }
                else
                {
                    this.toNoHostLookup.Remove(key);
                }
            }
            else
            {
                this.DecrementQNameCount(cset, filter.Address);
                cset.candidates.Remove(item);
            }
            this.RebuildMasks();
            return true;
        }

        public bool Remove(KeyValuePair<MessageFilter, TFilterData> item)
        {
            return (this.filters.Contains(item) && this.Remove(item.Key));
        }

        public virtual bool Remove(MessageFilter filter)
        {
            if (filter == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("filter");
            }
            EndpointAddressMessageFilter filter2 = filter as EndpointAddressMessageFilter;
            return ((filter2 != null) && this.Remove(filter2));
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public bool TryGetValue(MessageFilter filter, out TFilterData data)
        {
            return this.filters.TryGetValue(filter, out data);
        }

        internal virtual bool TryMatchCandidateSet(Uri to, bool includeHostNameInComparison, out CandidateSet<TFilterData> cset)
        {
            if (includeHostNameInComparison)
            {
                return this.toHostLookup.TryGetValue(to, out cset);
            }
            return this.toNoHostLookup.TryGetValue(to, out cset);
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
                    this.candidates[filter].data = value;
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

        public ICollection<TFilterData> Values
        {
            get
            {
                return (ICollection<TFilterData>) this.filters.Values;
            }
        }

        internal class Candidate
        {
            internal TFilterData data;
            internal MessageFilter filter;
            internal Dictionary<string, EndpointAddressProcessor.HeaderBit[]> headerLookup;
            internal byte[] mask;

            internal Candidate(MessageFilter filter, TFilterData data, byte[] mask, Dictionary<string, EndpointAddressProcessor.HeaderBit[]> headerLookup)
            {
                this.filter = filter;
                this.data = data;
                this.mask = mask;
                this.headerLookup = headerLookup;
            }
        }

        internal class CandidateSet
        {
            internal List<EndpointAddressMessageFilterTable<TFilterData>.Candidate> candidates;
            internal Dictionary<EndpointAddressProcessor.QName, int> qnames;

            internal CandidateSet()
            {
                this.qnames = new Dictionary<EndpointAddressProcessor.QName, int>(EndpointAddressProcessor.QNameComparer);
                this.candidates = new List<EndpointAddressMessageFilterTable<TFilterData>.Candidate>();
            }
        }

        internal class ProcessorPool
        {
            private EndpointAddressProcessor processor;

            internal ProcessorPool()
            {
            }

            internal EndpointAddressProcessor Pop()
            {
                EndpointAddressProcessor processor = this.processor;
                if (processor != null)
                {
                    this.processor = processor.next;
                    processor.next = null;
                    return processor;
                }
                return null;
            }

            internal void Push(EndpointAddressProcessor p)
            {
                p.next = this.processor;
                this.processor = p;
            }
        }
    }
}

