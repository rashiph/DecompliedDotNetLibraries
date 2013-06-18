namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime.Serialization;
    using System.ServiceModel;

    [DataContract(Name="Referral", Namespace="http://schemas.microsoft.com/net/2006/05/peer")]
    internal class Referral
    {
        [DataMember(Name="Address")]
        private PeerNodeAddress address;
        [DataMember(Name="NodeId")]
        private ulong nodeId;

        public Referral(ulong nodeId, PeerNodeAddress address)
        {
            this.nodeId = nodeId;
            this.address = address;
        }

        public PeerNodeAddress Address
        {
            get
            {
                return this.address;
            }
            set
            {
                this.address = value;
            }
        }

        public ulong NodeId
        {
            get
            {
                return this.nodeId;
            }
            set
            {
                this.nodeId = value;
            }
        }
    }
}

