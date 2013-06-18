namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    public class PrefixEndpointAddressMessageFilter : MessageFilter
    {
        private EndpointAddress address;
        private UriPrefixTable<object> addressTable;
        private EndpointAddressMessageFilterHelper helper;
        private HostNameComparisonMode hostNameComparisonMode;

        public PrefixEndpointAddressMessageFilter(EndpointAddress address) : this(address, false)
        {
        }

        public PrefixEndpointAddressMessageFilter(EndpointAddress address, bool includeHostNameInComparison)
        {
            if (address == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("address");
            }
            this.address = address;
            this.helper = new EndpointAddressMessageFilterHelper(this.address);
            this.hostNameComparisonMode = includeHostNameInComparison ? HostNameComparisonMode.Exact : HostNameComparisonMode.StrongWildcard;
            this.addressTable = new UriPrefixTable<object>();
            this.addressTable.RegisterUri(this.address.Uri, this.hostNameComparisonMode, new object());
        }

        protected internal override IMessageFilterTable<FilterData> CreateFilterTable<FilterData>()
        {
            return new PrefixEndpointAddressMessageFilterTable<FilterData>();
        }

        public override bool Match(Message message)
        {
            object obj2;
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }
            Uri to = message.Headers.To;
            return (((to != null) && this.addressTable.TryLookupUri(to, this.hostNameComparisonMode, out obj2)) && this.helper.Match(message));
        }

        public override bool Match(MessageBuffer messageBuffer)
        {
            bool flag;
            if (messageBuffer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("messageBuffer");
            }
            Message message = messageBuffer.CreateMessage();
            try
            {
                flag = this.Match(message);
            }
            finally
            {
                message.Close();
            }
            return flag;
        }

        public EndpointAddress Address
        {
            get
            {
                return this.address;
            }
        }

        internal Dictionary<string, EndpointAddressProcessor.HeaderBit[]> HeaderLookup
        {
            get
            {
                return this.helper.HeaderLookup;
            }
        }

        public bool IncludeHostNameInComparison
        {
            get
            {
                return (this.hostNameComparisonMode == HostNameComparisonMode.Exact);
            }
        }
    }
}

