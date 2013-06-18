namespace System.ServiceModel.PeerResolvers
{
    using System;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.ServiceModel;

    [MessageContract(IsWrapped=false)]
    public class RefreshResponseInfo
    {
        [MessageBodyMember(Name="RefreshResponse", Namespace="http://schemas.microsoft.com/net/2006/05/peer")]
        private RefreshResponseInfoDC body;

        public RefreshResponseInfo() : this(TimeSpan.Zero, RefreshResult.RegistrationNotFound)
        {
        }

        public RefreshResponseInfo(TimeSpan registrationLifetime, RefreshResult result)
        {
            this.body = new RefreshResponseInfoDC(registrationLifetime, result);
        }

        public bool HasBody()
        {
            return (this.body != null);
        }

        public TimeSpan RegistrationLifetime
        {
            get
            {
                return this.body.RegistrationLifetime;
            }
            set
            {
                if (value < TimeSpan.Zero)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value, System.ServiceModel.SR.GetString("SFxTimeoutOutOfRange0")));
                }
                if (TimeoutHelper.IsTooLarge(value))
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value, System.ServiceModel.SR.GetString("SFxTimeoutOutOfRangeTooBig")));
                }
                this.body.RegistrationLifetime = value;
            }
        }

        public RefreshResult Result
        {
            get
            {
                return this.body.Result;
            }
            set
            {
                this.body.Result = value;
            }
        }

        [DataContract(Name="RefreshResponseInfo", Namespace="http://schemas.microsoft.com/net/2006/05/peer")]
        private class RefreshResponseInfoDC
        {
            [DataMember(Name="RegistrationLifetime")]
            public TimeSpan RegistrationLifetime;
            [DataMember(Name="Result")]
            public RefreshResult Result;

            public RefreshResponseInfoDC(TimeSpan registrationLifetime, RefreshResult result)
            {
                this.RegistrationLifetime = registrationLifetime;
                this.Result = result;
            }
        }
    }
}

