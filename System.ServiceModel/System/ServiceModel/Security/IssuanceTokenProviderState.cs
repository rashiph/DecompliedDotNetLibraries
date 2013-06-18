namespace System.ServiceModel.Security
{
    using System;
    using System.IdentityModel.Tokens;
    using System.ServiceModel;

    internal class IssuanceTokenProviderState : IDisposable
    {
        private string context;
        private bool isNegotiationCompleted;
        private EndpointAddress remoteAddress;
        private GenericXmlSecurityToken serviceToken;
        private EndpointAddress targetAddress;

        private void CheckCompleted()
        {
            if (!this.IsNegotiationCompleted)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("NegotiationIsNotCompleted")));
            }
        }

        public virtual void Dispose()
        {
        }

        public void SetServiceToken(GenericXmlSecurityToken serviceToken)
        {
            if (this.IsNegotiationCompleted)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("NegotiationIsCompleted")));
            }
            this.serviceToken = serviceToken;
            this.isNegotiationCompleted = true;
        }

        public string Context
        {
            get
            {
                return this.context;
            }
            set
            {
                this.context = value;
            }
        }

        public bool IsNegotiationCompleted
        {
            get
            {
                return this.isNegotiationCompleted;
            }
        }

        public EndpointAddress RemoteAddress
        {
            get
            {
                return this.remoteAddress;
            }
            set
            {
                this.remoteAddress = value;
            }
        }

        public GenericXmlSecurityToken ServiceToken
        {
            get
            {
                this.CheckCompleted();
                return this.serviceToken;
            }
        }

        public EndpointAddress TargetAddress
        {
            get
            {
                return this.targetAddress;
            }
            set
            {
                this.targetAddress = value;
            }
        }
    }
}

