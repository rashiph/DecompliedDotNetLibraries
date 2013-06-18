namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Diagnostics;

    public class EndpointDispatcher
    {
        private MessageFilter addressFilter;
        private bool addressFilterSetExplicit;
        private System.ServiceModel.Dispatcher.ChannelDispatcher channelDispatcher;
        private MessageFilter contractFilter;
        private string contractName;
        private string contractNamespace;
        private ServiceChannel datagramChannel;
        private System.ServiceModel.Dispatcher.DispatchRuntime dispatchRuntime;
        private MessageFilter endpointFilter;
        private int filterPriority;
        private string id;
        private bool isSystemEndpoint;
        private Uri listenUri;
        private System.ServiceModel.EndpointAddress originalAddress;
        private string perfCounterBaseId;
        private string perfCounterId;

        private EndpointDispatcher(EndpointDispatcher baseEndpoint, IEnumerable<AddressHeader> headers)
        {
            EndpointAddressBuilder builder = new EndpointAddressBuilder(baseEndpoint.EndpointAddress);
            foreach (AddressHeader header in headers)
            {
                builder.Headers.Add(header);
            }
            System.ServiceModel.EndpointAddress address = builder.ToEndpointAddress();
            this.addressFilter = new EndpointAddressMessageFilter(address);
            this.contractFilter = baseEndpoint.ContractFilter;
            this.contractName = baseEndpoint.ContractName;
            this.contractNamespace = baseEndpoint.ContractNamespace;
            this.dispatchRuntime = baseEndpoint.DispatchRuntime;
            this.filterPriority = baseEndpoint.FilterPriority + 1;
            this.originalAddress = address;
            if (PerformanceCounters.PerformanceCountersEnabled)
            {
                this.perfCounterId = baseEndpoint.perfCounterId;
                this.perfCounterBaseId = baseEndpoint.perfCounterBaseId;
            }
            this.id = baseEndpoint.id;
        }

        public EndpointDispatcher(System.ServiceModel.EndpointAddress address, string contractName, string contractNamespace) : this(address, contractName, contractNamespace, false)
        {
        }

        public EndpointDispatcher(System.ServiceModel.EndpointAddress address, string contractName, string contractNamespace, bool isSystemEndpoint)
        {
            this.originalAddress = address;
            this.contractName = contractName;
            this.contractNamespace = contractNamespace;
            if (address != null)
            {
                this.addressFilter = new EndpointAddressMessageFilter(address);
            }
            else
            {
                this.addressFilter = new MatchAllMessageFilter();
            }
            this.contractFilter = new MatchAllMessageFilter();
            this.dispatchRuntime = new System.ServiceModel.Dispatcher.DispatchRuntime(this);
            this.filterPriority = 0;
            this.isSystemEndpoint = isSystemEndpoint;
        }

        internal EndpointDispatcher(System.ServiceModel.EndpointAddress address, string contractName, string contractNamespace, string id, bool isSystemEndpoint) : this(address, contractName, contractNamespace)
        {
            this.id = id;
            this.isSystemEndpoint = isSystemEndpoint;
        }

        internal static EndpointDispatcher AddEndpointDispatcher(EndpointDispatcher baseEndpoint, IEnumerable<AddressHeader> headers)
        {
            EndpointDispatcher item = new EndpointDispatcher(baseEndpoint, headers);
            baseEndpoint.ChannelDispatcher.Endpoints.Add(item);
            return item;
        }

        internal void Attach(System.ServiceModel.Dispatcher.ChannelDispatcher channelDispatcher)
        {
            if (channelDispatcher == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("channelDispatcher");
            }
            if (this.channelDispatcher != null)
            {
                Exception exception = new InvalidOperationException(System.ServiceModel.SR.GetString("SFxEndpointDispatcherMultipleChannelDispatcher0"));
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception);
            }
            this.channelDispatcher = channelDispatcher;
            this.listenUri = channelDispatcher.Listener.Uri;
        }

        internal void Detach(System.ServiceModel.Dispatcher.ChannelDispatcher channelDispatcher)
        {
            if (channelDispatcher == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("channelDispatcher");
            }
            if (this.channelDispatcher != channelDispatcher)
            {
                Exception exception = new InvalidOperationException(System.ServiceModel.SR.GetString("SFxEndpointDispatcherDifferentChannelDispatcher0"));
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception);
            }
            this.ReleasePerformanceCounters();
            this.channelDispatcher = null;
        }

        internal void ReleasePerformanceCounters()
        {
            if (PerformanceCounters.PerformanceCountersEnabled)
            {
                PerformanceCounters.ReleasePerformanceCountersForEndpoint(this.perfCounterId, this.perfCounterBaseId);
            }
        }

        internal bool SetPerfCounterId()
        {
            Uri listenUri = null;
            if (null != this.ListenUri)
            {
                listenUri = this.ListenUri;
            }
            else
            {
                System.ServiceModel.EndpointAddress endpointAddress = this.EndpointAddress;
                if (null != endpointAddress)
                {
                    listenUri = endpointAddress.Uri;
                }
            }
            if (null != listenUri)
            {
                this.perfCounterBaseId = listenUri.AbsoluteUri.ToUpperInvariant();
                this.perfCounterId = this.perfCounterBaseId + "/" + this.contractName.ToUpperInvariant();
                return true;
            }
            return false;
        }

        private void ThrowIfDisposedOrImmutable()
        {
            System.ServiceModel.Dispatcher.ChannelDispatcher channelDispatcher = this.channelDispatcher;
            if (channelDispatcher != null)
            {
                channelDispatcher.ThrowIfDisposedOrImmutable();
            }
        }

        public MessageFilter AddressFilter
        {
            get
            {
                return this.addressFilter;
            }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                this.ThrowIfDisposedOrImmutable();
                this.addressFilter = value;
                this.addressFilterSetExplicit = true;
            }
        }

        internal bool AddressFilterSetExplicit
        {
            get
            {
                return this.addressFilterSetExplicit;
            }
        }

        public System.ServiceModel.Dispatcher.ChannelDispatcher ChannelDispatcher
        {
            get
            {
                return this.channelDispatcher;
            }
        }

        public MessageFilter ContractFilter
        {
            get
            {
                return this.contractFilter;
            }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                this.ThrowIfDisposedOrImmutable();
                this.contractFilter = value;
            }
        }

        public string ContractName
        {
            get
            {
                return this.contractName;
            }
        }

        public string ContractNamespace
        {
            get
            {
                return this.contractNamespace;
            }
        }

        internal ServiceChannel DatagramChannel
        {
            get
            {
                return this.datagramChannel;
            }
            set
            {
                this.datagramChannel = value;
            }
        }

        public System.ServiceModel.Dispatcher.DispatchRuntime DispatchRuntime
        {
            get
            {
                return this.dispatchRuntime;
            }
        }

        public System.ServiceModel.EndpointAddress EndpointAddress
        {
            get
            {
                EndpointAddressBuilder builder;
                if (this.channelDispatcher == null)
                {
                    return this.originalAddress;
                }
                if ((this.originalAddress != null) && (this.originalAddress.Identity != null))
                {
                    return this.originalAddress;
                }
                IChannelListener listener = this.channelDispatcher.Listener;
                EndpointIdentity property = listener.GetProperty<EndpointIdentity>();
                if ((this.originalAddress != null) && (property == null))
                {
                    return this.originalAddress;
                }
                if (this.originalAddress != null)
                {
                    builder = new EndpointAddressBuilder(this.originalAddress);
                }
                else
                {
                    builder = new EndpointAddressBuilder {
                        Uri = listener.Uri
                    };
                }
                builder.Identity = property;
                return builder.ToEndpointAddress();
            }
        }

        internal MessageFilter EndpointFilter
        {
            get
            {
                if (this.endpointFilter == null)
                {
                    MessageFilter addressFilter = this.addressFilter;
                    MessageFilter contractFilter = this.contractFilter;
                    if (contractFilter is MatchAllMessageFilter)
                    {
                        this.endpointFilter = addressFilter;
                    }
                    else
                    {
                        this.endpointFilter = new AndMessageFilter(addressFilter, contractFilter);
                    }
                }
                return this.endpointFilter;
            }
        }

        public int FilterPriority
        {
            get
            {
                return this.filterPriority;
            }
            set
            {
                this.filterPriority = value;
            }
        }

        internal string Id
        {
            get
            {
                return this.id;
            }
            set
            {
                this.id = value;
            }
        }

        public bool IsSystemEndpoint
        {
            get
            {
                return this.isSystemEndpoint;
            }
        }

        internal Uri ListenUri
        {
            get
            {
                return this.listenUri;
            }
        }

        internal System.ServiceModel.EndpointAddress OriginalAddress
        {
            get
            {
                return this.originalAddress;
            }
        }

        internal string PerfCounterBaseId
        {
            get
            {
                return this.perfCounterBaseId;
            }
        }

        internal string PerfCounterId
        {
            get
            {
                return this.perfCounterId;
            }
        }

        internal int PerfCounterInstanceId { get; set; }
    }
}

