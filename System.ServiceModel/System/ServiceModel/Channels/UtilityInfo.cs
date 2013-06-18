namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime.Serialization;
    using System.ServiceModel;

    [MessageContract(IsWrapped=false)]
    internal class UtilityInfo
    {
        [MessageBodyMember(Name="LinkUtility", Namespace="http://schemas.microsoft.com/net/2006/05/peer")]
        private UtilityInfoDC body;

        public UtilityInfo()
        {
            this.body = new UtilityInfoDC();
        }

        public UtilityInfo(uint useful, uint total)
        {
            this.body = new UtilityInfoDC(useful, total);
        }

        public bool HasBody()
        {
            return (this.body != null);
        }

        public uint Total
        {
            get
            {
                return this.body.total;
            }
        }

        public uint Useful
        {
            get
            {
                return this.body.useful;
            }
        }

        [DataContract(Name="LinkUtilityInfo", Namespace="http://schemas.microsoft.com/net/2006/05/peer")]
        private class UtilityInfoDC
        {
            [DataMember(Name="Total")]
            public uint total;
            [DataMember(Name="Useful")]
            public uint useful;

            public UtilityInfoDC()
            {
            }

            public UtilityInfoDC(uint useful, uint total)
            {
                this.useful = useful;
                this.total = total;
            }
        }
    }
}

