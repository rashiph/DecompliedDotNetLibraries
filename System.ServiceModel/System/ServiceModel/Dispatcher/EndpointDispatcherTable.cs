namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Diagnostics;

    internal class EndpointDispatcherTable
    {
        private List<EndpointDispatcher> cachedEndpoints;
        private MessageFilterTable<EndpointDispatcher> filters;
        private const int optimizationThreshold = 2;
        private object thisLock;

        public EndpointDispatcherTable(object thisLock)
        {
            this.thisLock = thisLock;
        }

        public void AddEndpoint(EndpointDispatcher endpoint)
        {
            lock (this.ThisLock)
            {
                MessageFilter endpointFilter = endpoint.EndpointFilter;
                int filterPriority = endpoint.FilterPriority;
                if (this.filters == null)
                {
                    if (this.cachedEndpoints == null)
                    {
                        this.cachedEndpoints = new List<EndpointDispatcher>(2);
                    }
                    if (this.cachedEndpoints.Count < 2)
                    {
                        this.cachedEndpoints.Add(endpoint);
                    }
                    else
                    {
                        this.filters = new MessageFilterTable<EndpointDispatcher>();
                        for (int i = 0; i < this.cachedEndpoints.Count; i++)
                        {
                            int priority = this.cachedEndpoints[i].FilterPriority;
                            MessageFilter filter = this.cachedEndpoints[i].EndpointFilter;
                            this.filters.Add(filter, this.cachedEndpoints[i], priority);
                        }
                        this.filters.Add(endpointFilter, endpoint, filterPriority);
                        this.cachedEndpoints = null;
                    }
                }
                else
                {
                    this.filters.Add(endpointFilter, endpoint, filterPriority);
                }
            }
        }

        public EndpointDispatcher Lookup(Message message, out bool addressMatched)
        {
            EndpointDispatcher data = null;
            data = this.LookupInCache(message, out addressMatched);
            if (data == null)
            {
                lock (this.ThisLock)
                {
                    data = this.LookupInCache(message, out addressMatched);
                    if ((data == null) && (this.filters != null))
                    {
                        this.filters.GetMatchingValue(message, out data, out addressMatched);
                    }
                }
            }
            return data;
        }

        private EndpointDispatcher LookupInCache(Message message, out bool addressMatched)
        {
            EndpointDispatcher dispatcher = null;
            int num = -2147483648;
            bool flag = false;
            addressMatched = false;
            if ((this.cachedEndpoints != null) && (this.cachedEndpoints.Count > 0))
            {
                for (int i = 0; i < this.cachedEndpoints.Count; i++)
                {
                    bool flag2;
                    EndpointDispatcher dispatcher2 = this.cachedEndpoints[i];
                    int filterPriority = dispatcher2.FilterPriority;
                    MessageFilter endpointFilter = dispatcher2.EndpointFilter;
                    AndMessageFilter filter2 = endpointFilter as AndMessageFilter;
                    if (filter2 != null)
                    {
                        bool flag3;
                        flag2 = filter2.Match(message, out flag3);
                        addressMatched |= flag3;
                    }
                    else
                    {
                        flag2 = endpointFilter.Match(message);
                    }
                    if (flag2)
                    {
                        addressMatched = true;
                        if ((filterPriority > num) || (dispatcher == null))
                        {
                            dispatcher = dispatcher2;
                            num = filterPriority;
                            flag = false;
                        }
                        else if ((filterPriority == num) && (dispatcher != null))
                        {
                            flag = true;
                        }
                    }
                }
            }
            if (flag)
            {
                throw TraceUtility.ThrowHelperError(new MultipleFilterMatchesException(System.ServiceModel.SR.GetString("FilterMultipleMatches")), message);
            }
            return dispatcher;
        }

        public void RemoveEndpoint(EndpointDispatcher endpoint)
        {
            lock (this.ThisLock)
            {
                if (this.filters == null)
                {
                    if ((this.cachedEndpoints != null) && this.cachedEndpoints.Contains(endpoint))
                    {
                        this.cachedEndpoints.Remove(endpoint);
                    }
                }
                else
                {
                    MessageFilter endpointFilter = endpoint.EndpointFilter;
                    this.filters.Remove(endpointFilter);
                }
            }
        }

        public int Count
        {
            get
            {
                return (((this.cachedEndpoints != null) ? this.cachedEndpoints.Count : 0) + ((this.filters != null) ? this.filters.Count : 0));
            }
        }

        private object ThisLock
        {
            get
            {
                return this.thisLock;
            }
        }
    }
}

