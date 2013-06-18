namespace System.ServiceModel.PeerResolvers
{
    using System;
    using System.Runtime.Serialization;
    using System.ServiceModel;

    [MessageContract(IsWrapped=false)]
    public class ServiceSettingsResponseInfo
    {
        [MessageBodyMember(Name="ServiceSettings", Namespace="http://schemas.microsoft.com/net/2006/05/peer")]
        private ServiceSettingsResponseInfoDC body;

        public ServiceSettingsResponseInfo() : this(false)
        {
        }

        public ServiceSettingsResponseInfo(bool control)
        {
            this.body = new ServiceSettingsResponseInfoDC(control);
        }

        public bool HasBody()
        {
            return (this.body != null);
        }

        public bool ControlMeshShape
        {
            get
            {
                return this.body.ControlMeshShape;
            }
            set
            {
                this.body.ControlMeshShape = value;
            }
        }

        [DataContract(Name="ServiceSettingsResponseInfo", Namespace="http://schemas.microsoft.com/net/2006/05/peer")]
        private class ServiceSettingsResponseInfoDC
        {
            [DataMember(Name="ControlMeshShape")]
            public bool ControlMeshShape;

            public ServiceSettingsResponseInfoDC(bool control)
            {
                this.ControlMeshShape = control;
            }
        }
    }
}

