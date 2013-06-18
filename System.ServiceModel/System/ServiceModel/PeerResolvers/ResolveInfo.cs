namespace System.ServiceModel.PeerResolvers
{
    using System;
    using System.Runtime.Serialization;
    using System.ServiceModel;

    [MessageContract(IsWrapped=false)]
    public class ResolveInfo
    {
        [MessageBodyMember(Name="Resolve", Namespace="http://schemas.microsoft.com/net/2006/05/peer")]
        private ResolveInfoDC body;

        public ResolveInfo()
        {
            this.body = new ResolveInfoDC();
        }

        public ResolveInfo(Guid clientId, string meshId, int maxAddresses)
        {
            this.body = new ResolveInfoDC(clientId, meshId, maxAddresses);
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

        public int MaxAddresses
        {
            get
            {
                return this.body.MaxAddresses;
            }
        }

        public string MeshId
        {
            get
            {
                return this.body.MeshId;
            }
        }

        [DataContract(Name="ResolveInfo", Namespace="http://schemas.microsoft.com/net/2006/05/peer")]
        private class ResolveInfoDC
        {
            [DataMember(Name="ClientId")]
            public Guid ClientId;
            [DataMember(Name="MaxAddresses")]
            public int MaxAddresses;
            [DataMember(Name="MeshId")]
            public string MeshId;

            public ResolveInfoDC()
            {
            }

            public ResolveInfoDC(Guid clientId, string meshId, int maxAddresses)
            {
                this.ClientId = clientId;
                this.MeshId = meshId;
                this.MaxAddresses = maxAddresses;
            }
        }
    }
}

