namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    public class EndpointAddressMessageFilter : MessageFilter
    {
        private EndpointAddress address;
        private UriComparer comparer;
        private EndpointAddressMessageFilterHelper helper;
        private bool includeHostNameInComparison;

        public EndpointAddressMessageFilter(EndpointAddress address) : this(address, false)
        {
        }

        public EndpointAddressMessageFilter(EndpointAddress address, bool includeHostNameInComparison)
        {
            if (address == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("address");
            }
            this.address = address;
            this.includeHostNameInComparison = includeHostNameInComparison;
            this.helper = new EndpointAddressMessageFilterHelper(this.address);
            if (includeHostNameInComparison)
            {
                this.comparer = HostUriComparer.Value;
            }
            else
            {
                this.comparer = NoHostUriComparer.Value;
            }
        }

        protected internal override IMessageFilterTable<FilterData> CreateFilterTable<FilterData>()
        {
            return new EndpointAddressMessageFilterTable<FilterData>();
        }

        public override bool Match(Message message)
        {
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }
            Uri to = message.Headers.To;
            Uri uri = this.address.Uri;
            return (((to != null) && this.comparer.Equals(uri, to)) && this.helper.Match(message));
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

        internal bool ComparePort
        {
            set
            {
                this.comparer.ComparePort = value;
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
                return this.includeHostNameInComparison;
            }
        }

        internal sealed class HostUriComparer : EndpointAddressMessageFilter.UriComparer
        {
            internal static readonly EndpointAddressMessageFilter.UriComparer Value = new EndpointAddressMessageFilter.HostUriComparer();

            private HostUriComparer()
            {
            }

            protected override bool CompareHost
            {
                get
                {
                    return true;
                }
            }
        }

        internal sealed class NoHostUriComparer : EndpointAddressMessageFilter.UriComparer
        {
            internal static readonly EndpointAddressMessageFilter.UriComparer Value = new EndpointAddressMessageFilter.NoHostUriComparer();

            private NoHostUriComparer()
            {
            }

            protected override bool CompareHost
            {
                get
                {
                    return false;
                }
            }
        }

        internal abstract class UriComparer : EqualityComparer<Uri>
        {
            protected UriComparer()
            {
                this.ComparePort = true;
            }

            public override bool Equals(Uri u1, Uri u2)
            {
                return EndpointAddress.UriEquals(u1, u2, true, this.CompareHost, this.ComparePort);
            }

            public override int GetHashCode(Uri uri)
            {
                return EndpointAddress.UriGetHashCode(uri, this.CompareHost, this.ComparePort);
            }

            protected abstract bool CompareHost { get; }

            internal bool ComparePort { get; set; }
        }
    }
}

