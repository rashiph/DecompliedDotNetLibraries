namespace System.ServiceModel.PeerResolvers
{
    using System;
    using System.ComponentModel;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    public class PeerResolverSettings
    {
        private PeerCustomResolverSettings customSettings = new PeerCustomResolverSettings();
        private PeerResolverMode mode;
        private PeerReferralPolicy referralPolicy;

        public PeerCustomResolverSettings Custom
        {
            get
            {
                return this.customSettings;
            }
        }

        public PeerResolverMode Mode
        {
            get
            {
                return this.mode;
            }
            set
            {
                if (!PeerResolverModeHelper.IsDefined(value))
                {
                    PeerExceptionHelper.ThrowArgument_InvalidResolverMode(value);
                }
                this.mode = value;
            }
        }

        public PeerReferralPolicy ReferralPolicy
        {
            get
            {
                return this.referralPolicy;
            }
            set
            {
                if (!PeerReferralPolicyHelper.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidEnumArgumentException("value", (int) value, typeof(PeerReferralPolicy)));
                }
                this.referralPolicy = value;
            }
        }
    }
}

