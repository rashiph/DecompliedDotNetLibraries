namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime.Serialization;
    using System.ServiceModel;

    [MessageContract(IsWrapped=false)]
    internal class ConnectInfo
    {
        [MessageBodyMember(Name="Connect", Namespace="http://schemas.microsoft.com/net/2006/05/peer")]
        private ConnectInfoDC body;

        public ConnectInfo()
        {
            this.body = new ConnectInfoDC();
        }

        public ConnectInfo(ulong nodeId, PeerNodeAddress address)
        {
            this.body = new ConnectInfoDC(nodeId, address);
        }

        public bool HasBody()
        {
            return (this.body != null);
        }

        public PeerNodeAddress Address
        {
            get
            {
                return this.body.address;
            }
        }

        public ulong NodeId
        {
            get
            {
                return this.body.nodeId;
            }
        }

        [DataContract(Name="ConnectInfo", Namespace="http://schemas.microsoft.com/net/2006/05/peer")]
        private class ConnectInfoDC
        {
            [DataMember(Name="Address")]
            public PeerNodeAddress address;
            [DataMember(Name="NodeId")]
            public ulong nodeId;

            public ConnectInfoDC()
            {
            }

            public ConnectInfoDC(ulong nodeId, PeerNodeAddress address)
            {
                this.nodeId = nodeId;
                this.address = address;
            }
        }
    }
}

