namespace System.ServiceModel.PeerResolvers
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.ServiceModel;

    [MessageContract(IsWrapped=false)]
    public class ResolveResponseInfo
    {
        [MessageBodyMember(Name="ResolveResponse", Namespace="http://schemas.microsoft.com/net/2006/05/peer")]
        private ResolveResponseInfoDC body;

        public ResolveResponseInfo() : this(null)
        {
        }

        public ResolveResponseInfo(PeerNodeAddress[] addresses)
        {
            this.body = new ResolveResponseInfoDC(addresses);
        }

        public bool HasBody()
        {
            return (this.body != null);
        }

        public IList<PeerNodeAddress> Addresses
        {
            get
            {
                return this.body.Addresses;
            }
            set
            {
                this.body.Addresses = value;
            }
        }

        [DataContract(Name="ResolveResponseInfo", Namespace="http://schemas.microsoft.com/net/2006/05/peer")]
        private class ResolveResponseInfoDC
        {
            [DataMember(Name="Addresses")]
            public IList<PeerNodeAddress> Addresses;

            public ResolveResponseInfoDC(PeerNodeAddress[] addresses)
            {
                this.Addresses = (IList<PeerNodeAddress>) addresses;
            }
        }
    }
}

