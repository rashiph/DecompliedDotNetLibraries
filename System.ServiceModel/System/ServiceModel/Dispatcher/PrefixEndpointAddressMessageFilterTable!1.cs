namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    internal class PrefixEndpointAddressMessageFilterTable<TFilterData> : EndpointAddressMessageFilterTable<TFilterData>
    {
        private UriPrefixTable<EndpointAddressMessageFilterTable<TFilterData>.CandidateSet> toHostTable;
        private UriPrefixTable<EndpointAddressMessageFilterTable<TFilterData>.CandidateSet> toNoHostTable;

        public override void Add(EndpointAddressMessageFilter filter, TFilterData data)
        {
            if (filter == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("filter");
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException("EndpointAddressMessageFilter cannot be added to PrefixEndpointAddressMessageFilterTable"));
        }

        public override void Add(MessageFilter filter, TFilterData data)
        {
            if (filter == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("filter");
            }
            this.Add((PrefixEndpointAddressMessageFilter) filter, data);
        }

        public void Add(PrefixEndpointAddressMessageFilter filter, TFilterData data)
        {
            EndpointAddressMessageFilterTable<TFilterData>.CandidateSet set;
            if (filter == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("filter");
            }
            base.filters.Add(filter, data);
            byte[] mask = base.BuildMask(filter.HeaderLookup);
            EndpointAddressMessageFilterTable<TFilterData>.Candidate candidate = new EndpointAddressMessageFilterTable<TFilterData>.Candidate(filter, data, mask, filter.HeaderLookup);
            base.candidates.Add(filter, candidate);
            Uri to = filter.Address.Uri;
            if (!this.TryMatchCandidateSet(to, filter.IncludeHostNameInComparison, out set))
            {
                set = new EndpointAddressMessageFilterTable<TFilterData>.CandidateSet();
                this.GetAddressTable(filter.IncludeHostNameInComparison).RegisterUri(to, this.GetComparisonMode(filter.IncludeHostNameInComparison), set);
            }
            set.candidates.Add(candidate);
            base.IncrementQNameCount(set, filter.Address);
        }

        protected override void ClearLookupTables()
        {
            this.toHostTable = new UriPrefixTable<EndpointAddressMessageFilterTable<TFilterData>.CandidateSet>();
            this.toNoHostTable = new UriPrefixTable<EndpointAddressMessageFilterTable<TFilterData>.CandidateSet>();
        }

        private UriPrefixTable<EndpointAddressMessageFilterTable<TFilterData>.CandidateSet> GetAddressTable(bool includeHostNameInComparison)
        {
            if (!includeHostNameInComparison)
            {
                return this.toNoHostTable;
            }
            return this.toHostTable;
        }

        private HostNameComparisonMode GetComparisonMode(bool includeHostNameInComparison)
        {
            if (!includeHostNameInComparison)
            {
                return HostNameComparisonMode.StrongWildcard;
            }
            return HostNameComparisonMode.Exact;
        }

        protected override void InitializeLookupTables()
        {
            this.toHostTable = new UriPrefixTable<EndpointAddressMessageFilterTable<TFilterData>.CandidateSet>();
            this.toNoHostTable = new UriPrefixTable<EndpointAddressMessageFilterTable<TFilterData>.CandidateSet>();
        }

        public override bool Remove(EndpointAddressMessageFilter filter)
        {
            if (filter == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("filter");
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException("EndpointAddressMessageFilter cannot be removed from PrefixEndpointAddressMessageFilterTable"));
        }

        public override bool Remove(MessageFilter filter)
        {
            if (filter == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("filter");
            }
            PrefixEndpointAddressMessageFilter filter2 = filter as PrefixEndpointAddressMessageFilter;
            return ((filter2 != null) && this.Remove(filter2));
        }

        public bool Remove(PrefixEndpointAddressMessageFilter filter)
        {
            if (filter == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("filter");
            }
            if (!base.filters.Remove(filter))
            {
                return false;
            }
            EndpointAddressMessageFilterTable<TFilterData>.Candidate item = base.candidates[filter];
            Uri to = filter.Address.Uri;
            EndpointAddressMessageFilterTable<TFilterData>.CandidateSet cset = null;
            if (this.TryMatchCandidateSet(to, filter.IncludeHostNameInComparison, out cset))
            {
                if (cset.candidates.Count == 1)
                {
                    this.GetAddressTable(filter.IncludeHostNameInComparison).UnregisterUri(to, this.GetComparisonMode(filter.IncludeHostNameInComparison));
                }
                else
                {
                    base.DecrementQNameCount(cset, filter.Address);
                    cset.candidates.Remove(item);
                }
            }
            base.candidates.Remove(filter);
            base.RebuildMasks();
            return true;
        }

        internal override bool TryMatchCandidateSet(Uri to, bool includeHostNameInComparison, out EndpointAddressMessageFilterTable<TFilterData>.CandidateSet cset)
        {
            return this.GetAddressTable(includeHostNameInComparison).TryLookupUri(to, this.GetComparisonMode(includeHostNameInComparison), out cset);
        }
    }
}

