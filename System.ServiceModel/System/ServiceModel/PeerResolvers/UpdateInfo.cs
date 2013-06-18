namespace System.ServiceModel.PeerResolvers
{
    using System;
    using System.Runtime.Serialization;
    using System.ServiceModel;

    [MessageContract(IsWrapped=false)]
    public class UpdateInfo
    {
        [MessageBodyMember(Name="Update", Namespace="http://schemas.microsoft.com/net/2006/05/peer")]
        private UpdateInfoDC body;

        public UpdateInfo()
        {
            this.body = new UpdateInfoDC();
        }

        public UpdateInfo(Guid registrationId, Guid client, string meshId, PeerNodeAddress address)
        {
            this.body = new UpdateInfoDC(registrationId, client, meshId, address);
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

        public Guid RegistrationId
        {
            get
            {
                return this.body.RegistrationId;
            }
        }

        [DataContract(Name="Update", Namespace="http://schemas.microsoft.com/net/2006/05/peer")]
        private class UpdateInfoDC
        {
            [DataMember(Name="ClientId")]
            public Guid ClientId;
            [DataMember(Name="MeshId")]
            public string MeshId;
            [DataMember(Name="NodeAddress")]
            public PeerNodeAddress NodeAddress;
            [DataMember(Name="RegistrationId")]
            public Guid RegistrationId;

            public UpdateInfoDC()
            {
            }

            public UpdateInfoDC(Guid registrationId, Guid client, string meshId, PeerNodeAddress address)
            {
                this.ClientId = client;
                this.MeshId = meshId;
                this.NodeAddress = address;
                this.RegistrationId = registrationId;
            }
        }
    }
}

