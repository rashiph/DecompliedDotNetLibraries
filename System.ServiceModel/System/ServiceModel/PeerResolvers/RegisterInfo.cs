namespace System.ServiceModel.PeerResolvers
{
    using System;
    using System.Runtime.Serialization;
    using System.ServiceModel;

    [MessageContract(IsWrapped=false)]
    public class RegisterInfo
    {
        [MessageBodyMember(Name="Register", Namespace="http://schemas.microsoft.com/net/2006/05/peer")]
        private RegisterInfoDC body;

        public RegisterInfo()
        {
            this.body = new RegisterInfoDC();
        }

        public RegisterInfo(Guid client, string meshId, PeerNodeAddress address)
        {
            this.body = new RegisterInfoDC(client, meshId, address);
        }

        public bool HasBody()
        {
            return (this.body != null);
        }

        public Guid ClientId
        {
            get
            {
                return this.body.ClientId;
            }
        }

        public string MeshId
        {
            get
            {
                return this.body.MeshId;
            }
        }

        public PeerNodeAddress NodeAddress
        {
            get
            {
                return this.body.NodeAddress;
            }
        }

        [DataContract(Name="Register", Namespace="http://schemas.microsoft.com/net/2006/05/peer")]
        private class RegisterInfoDC
        {
            [DataMember(Name="ClientId")]
            public Guid ClientId;
            [DataMember(Name="MeshId")]
            public string MeshId;
            [DataMember(Name="NodeAddress")]
            public PeerNodeAddress NodeAddress;

            public RegisterInfoDC()
            {
            }

            public RegisterInfoDC(Guid client, string meshId, PeerNodeAddress address)
            {
                this.ClientId = client;
                this.MeshId = meshId;
                this.NodeAddress = address;
            }
        }
    }
}

