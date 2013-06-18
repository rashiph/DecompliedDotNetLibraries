namespace System.ServiceModel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Net;
    using System.Runtime.Serialization;

    [DataContract(Name="PeerNodeAddress", Namespace="http://schemas.microsoft.com/net/2006/05/peer"), KnownType(typeof(IPAddress[]))]
    public sealed class PeerNodeAddress
    {
        private System.ServiceModel.EndpointAddress endpointAddress;
        private ReadOnlyCollection<IPAddress> ipAddresses;
        private string servicePath;

        public PeerNodeAddress(System.ServiceModel.EndpointAddress endpointAddress, ReadOnlyCollection<IPAddress> ipAddresses)
        {
            if (endpointAddress == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("endpointAddress"));
            }
            if (ipAddresses == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("ipAddresses"));
            }
            this.Initialize(endpointAddress, ipAddresses);
        }

        private void Initialize(System.ServiceModel.EndpointAddress endpointAddress, ReadOnlyCollection<IPAddress> ipAddresses)
        {
            this.endpointAddress = endpointAddress;
            this.servicePath = this.endpointAddress.Uri.PathAndQuery.ToUpperInvariant();
            this.ipAddresses = ipAddresses;
        }

        public System.ServiceModel.EndpointAddress EndpointAddress
        {
            get
            {
                return this.endpointAddress;
            }
        }

        [DataMember(Name="EndpointAddress")]
        internal EndpointAddress10 InnerEPR
        {
            get
            {
                if (this.endpointAddress != null)
                {
                    return EndpointAddress10.FromEndpointAddress(this.endpointAddress);
                }
                return null;
            }
            set
            {
                this.endpointAddress = (value == null) ? null : value.ToEndpointAddress();
            }
        }

        public ReadOnlyCollection<IPAddress> IPAddresses
        {
            get
            {
                if (this.ipAddresses == null)
                {
                    this.ipAddresses = new ReadOnlyCollection<IPAddress>(new IPAddress[0]);
                }
                return this.ipAddresses;
            }
        }

        [DataMember(Name="IPAddresses")]
        internal IList<IPAddress> ipAddressesDataMember
        {
            get
            {
                return this.ipAddresses;
            }
            set
            {
                this.ipAddresses = new ReadOnlyCollection<IPAddress>((value == null) ? ((IList<IPAddress>) new IPAddress[0]) : value);
            }
        }

        internal string ServicePath
        {
            get
            {
                if (this.servicePath == null)
                {
                    this.servicePath = this.endpointAddress.Uri.PathAndQuery.ToUpperInvariant();
                }
                return this.servicePath;
            }
        }
    }
}

