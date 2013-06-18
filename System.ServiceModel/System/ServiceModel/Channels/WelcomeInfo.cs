namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.ServiceModel;

    [MessageContract(IsWrapped=false)]
    internal class WelcomeInfo
    {
        [MessageBodyMember(Name="Welcome", Namespace="http://schemas.microsoft.com/net/2006/05/peer")]
        private WelcomeInfoDC body;

        public WelcomeInfo()
        {
            this.body = new WelcomeInfoDC();
        }

        public WelcomeInfo(ulong nodeId, Referral[] referrals)
        {
            this.body = new WelcomeInfoDC(nodeId, referrals);
        }

        public bool HasBody()
        {
            return (this.body != null);
        }

        public ulong NodeId
        {
            get
            {
                return this.body.nodeId;
            }
        }

        public IList<Referral> Referrals
        {
            get
            {
                if (this.body.referrals == null)
                {
                    return null;
                }
                return Array.AsReadOnly<Referral>(this.body.referrals);
            }
        }

        [DataContract(Name="WelcomeInfo", Namespace="http://schemas.microsoft.com/net/2006/05/peer")]
        private class WelcomeInfoDC
        {
            [DataMember(Name="NodeId")]
            public ulong nodeId;
            [DataMember(Name="Referrals")]
            public Referral[] referrals;

            public WelcomeInfoDC()
            {
            }

            public WelcomeInfoDC(ulong nodeId, Referral[] referrals)
            {
                this.nodeId = nodeId;
                this.referrals = referrals;
            }
        }
    }
}

