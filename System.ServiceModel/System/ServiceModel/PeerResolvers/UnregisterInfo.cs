namespace System.ServiceModel.PeerResolvers
{
    using System;
    using System.Runtime.Serialization;
    using System.ServiceModel;

    [MessageContract(IsWrapped=false)]
    public class UnregisterInfo
    {
        [MessageBodyMember(Name="Unregister", Namespace="http://schemas.microsoft.com/net/2006/05/peer")]
        private UnregisterInfoDC body;

        public UnregisterInfo()
        {
            this.body = new UnregisterInfoDC();
        }

        public UnregisterInfo(string meshId, Guid registrationId)
        {
            this.body = new UnregisterInfoDC(meshId, registrationId);
        }

        public bool HasBody()
        {
            return (this.body != null);
        }

        public string MeshId
        {
            get
            {
                return this.body.MeshId;
            }
        }

        public Guid RegistrationId
        {
            get
            {
                return this.body.RegistrationId;
            }
        }

        [DataContract(Name="UnregisterInfo", Namespace="http://schemas.microsoft.com/net/2006/05/peer")]
        private class UnregisterInfoDC
        {
            [DataMember(Name="MeshId")]
            public string MeshId;
            [DataMember(Name="RegistrationId")]
            public Guid RegistrationId;

            public UnregisterInfoDC()
            {
            }

            public UnregisterInfoDC(string meshId, Guid registrationId)
            {
                this.MeshId = meshId;
                this.RegistrationId = registrationId;
            }
        }
    }
}

