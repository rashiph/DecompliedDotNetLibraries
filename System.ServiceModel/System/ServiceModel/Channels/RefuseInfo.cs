namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.ServiceModel;

    [MessageContract(IsWrapped=false)]
    internal class RefuseInfo
    {
        [MessageBodyMember(Name="Refuse", Namespace="http://schemas.microsoft.com/net/2006/05/peer")]
        private RefuseInfoDC body;

        public RefuseInfo()
        {
            this.body = new RefuseInfoDC();
        }

        public RefuseInfo(RefuseReason reason, Referral[] referrals)
        {
            this.body = new RefuseInfoDC(reason, referrals);
        }

        public bool HasBody()
        {
            return (this.body != null);
        }

        public RefuseReason Reason
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

        [DataContract(Name="RefuseInfo", Namespace="http://schemas.microsoft.com/net/2006/05/peer")]
        private class RefuseInfoDC
        {
            [DataMember(Name="Reason")]
            public RefuseReason reason;
            [DataMember(Name="Referrals")]
            public Referral[] referrals;

            public RefuseInfoDC()
            {
            }

            public RefuseInfoDC(RefuseReason reason, Referral[] referrals)
            {
                this.reason = reason;
                this.referrals = referrals;
            }
        }
    }
}

