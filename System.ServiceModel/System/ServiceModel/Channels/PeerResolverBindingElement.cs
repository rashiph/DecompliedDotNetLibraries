namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.PeerResolvers;

    public abstract class PeerResolverBindingElement : BindingElement
    {
        protected PeerResolverBindingElement()
        {
        }

        protected PeerResolverBindingElement(PeerResolverBindingElement other) : base(other)
        {
        }

        public abstract PeerResolver CreatePeerResolver();

        public abstract PeerReferralPolicy ReferralPolicy { get; set; }
    }
}

