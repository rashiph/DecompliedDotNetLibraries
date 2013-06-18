namespace System.ServiceModel.PeerResolvers
{
    using System;
    using System.Runtime.Serialization;
    using System.ServiceModel;

    [MessageContract(IsWrapped=false)]
    public class RefreshInfo
    {
        [MessageBodyMember(Name="Refresh", Namespace="http://schemas.microsoft.com/net/2006/05/peer")]
        private RefreshInfoDC body;

        public RefreshInfo()
        {
            this.body = new RefreshInfoDC();
        }

        public RefreshInfo(string meshId, Guid regId)
        {
            this.body = new RefreshInfoDC(meshId, regId);
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

        [DataContract(Name="RefreshInfo", Namespace="http://schemas.microsoft.com/net/2006/05/peer")]
        private class RefreshInfoDC
        {
            [DataMember(Name="MeshId")]
            public string MeshId;
            [DataMember(Name="RegistrationId")]
            public Guid RegistrationId;

            public RefreshInfoDC()
            {
            }

            public RefreshInfoDC(string meshId, Guid regId)
            {
                this.MeshId = meshId;
                this.RegistrationId = regId;
            }
        }
    }
}

