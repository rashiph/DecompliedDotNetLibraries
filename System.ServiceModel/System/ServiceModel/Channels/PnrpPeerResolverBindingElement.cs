namespace System.ServiceModel.Channels
{
    using System;
    using System.ComponentModel;
    using System.ServiceModel;
    using System.ServiceModel.PeerResolvers;

    public sealed class PnrpPeerResolverBindingElement : PeerResolverBindingElement
    {
        private PeerReferralPolicy referralPolicy;

        public PnrpPeerResolverBindingElement()
        {
        }

        private PnrpPeerResolverBindingElement(PnrpPeerResolverBindingElement elementToBeCloned) : base(elementToBeCloned)
        {
            this.referralPolicy = elementToBeCloned.referralPolicy;
        }

        public PnrpPeerResolverBindingElement(PeerReferralPolicy referralPolicy)
        {
            this.referralPolicy = referralPolicy;
        }

        public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("context"));
            }
            context.BindingParameters.Add(this);
            return context.BuildInnerChannelFactory<TChannel>();
        }

        public override IChannelListener<TChannel> BuildChannelListener<TChannel>(BindingContext context) where TChannel: class, IChannel
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("context"));
            }
            context.BindingParameters.Add(this);
            return context.BuildInnerChannelListener<TChannel>();
        }

        public override bool CanBuildChannelFactory<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("context"));
            }
            context.BindingParameters.Add(this);
            return context.CanBuildInnerChannelFactory<TChannel>();
        }

        public override bool CanBuildChannelListener<TChannel>(BindingContext context) where TChannel: class, IChannel
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("context"));
            }
            context.BindingParameters.Add(this);
            return context.CanBuildInnerChannelListener<TChannel>();
        }

        public override BindingElement Clone()
        {
            return new PnrpPeerResolverBindingElement(this);
        }

        public override PeerResolver CreatePeerResolver()
        {
            return new PnrpPeerResolver(this.referralPolicy);
        }

        public override T GetProperty<T>(BindingContext context) where T: class
        {
            return context.GetInnerProperty<T>();
        }

        public override PeerReferralPolicy ReferralPolicy
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

