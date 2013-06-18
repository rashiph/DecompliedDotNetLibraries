namespace System.ServiceModel.Security
{
    using System;
    using System.IdentityModel.Tokens;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;

    internal class SecurityProtocolCorrelationState
    {
        private ServiceModelActivity activity;
        private System.ServiceModel.Security.SignatureConfirmations signatureConfirmations;
        private SecurityToken token;

        public SecurityProtocolCorrelationState(SecurityToken token)
        {
            this.token = token;
            this.activity = DiagnosticUtility.ShouldUseActivity ? ServiceModelActivity.Current : null;
        }

        internal ServiceModelActivity Activity
        {
            get
            {
                return this.activity;
            }
        }

        internal System.ServiceModel.Security.SignatureConfirmations SignatureConfirmations
        {
            get
            {
                return this.signatureConfirmations;
            }
            set
            {
                this.signatureConfirmations = value;
            }
        }

        public SecurityToken Token
        {
            get
            {
                return this.token;
            }
        }
    }
}

