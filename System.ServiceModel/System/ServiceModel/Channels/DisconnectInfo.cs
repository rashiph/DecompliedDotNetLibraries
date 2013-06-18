namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.ServiceModel;

    [MessageContract(IsWrapped=false)]
    internal class DisconnectInfo
    {
        [MessageBodyMember(Name="Disconnect", Namespace="http://schemas.microsoft.com/net/2006/05/peer")]
        private DisconnectInfoDC body;

        public DisconnectInfo()
        {
            this.body = new DisconnectInfoDC();
        }

        public DisconnectInfo(DisconnectReason reason, Referral[] referrals)
        {
            this.body = new DisconnectInfoDC(reason, referrals);
        }

        public bool HasBody()
        {
            return (this.body != null);
        }

        public DisconnectReason Reason
        {
            get
            {
                return this.body.reason;
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

        [DataContract(Name="DisconnectInfo", Namespace="http://schemas.microsoft.com/net/2006/05/peer")]
        private class DisconnectInfoDC
        {
            [DataMember(Name="Reason")]
            public DisconnectReason reason;
            [DataMember(Name="Referrals")]
            public Referral[] referrals;

            public DisconnectInfoDC()
            {
            }

            public DisconnectInfoDC(DisconnectReason reason, Referral[] referrals)
            {
                this.reason = reason;
                this.referrals = referrals;
            }
        }
    }
}

