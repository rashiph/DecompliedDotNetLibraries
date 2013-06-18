namespace Microsoft.Transactions.Wsat.Messaging
{
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Runtime;
    using System.ServiceModel.Channels;

    internal class SupportingTokenBindingElement : BindingElement
    {
        private Microsoft.Transactions.Wsat.Protocol.ProtocolVersion protocolVersion;
        private SupportingTokenServiceCredentials serverCreds;

        private SupportingTokenBindingElement(SupportingTokenBindingElement other) : base(other)
        {
            this.serverCreds = new SupportingTokenServiceCredentials();
            this.protocolVersion = other.ProtocolVersion;
        }

        public SupportingTokenBindingElement(Microsoft.Transactions.Wsat.Protocol.ProtocolVersion protocolVersion)
        {
            this.serverCreds = new SupportingTokenServiceCredentials();
            this.protocolVersion = protocolVersion;
        }

        public override IChannelListener<TChannel> BuildChannelListener<TChannel>(BindingContext context) where TChannel: class, IChannel
        {
            return new SupportingTokenChannelListener<TChannel>(this, context, this.serverCreds.TokenResolver);
        }

        public override bool CanBuildChannelListener<TChannel>(BindingContext context) where TChannel: class, IChannel
        {
            return context.CanBuildInnerChannelListener<TChannel>();
        }

        public override BindingElement Clone()
        {
            return new SupportingTokenBindingElement(this);
        }

        public override T GetProperty<T>(BindingContext context) where T: class
        {
            return context.GetInnerProperty<T>();
        }

        public Microsoft.Transactions.Wsat.Protocol.ProtocolVersion ProtocolVersion
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.protocolVersion;
            }
        }

        public SupportingTokenServiceCredentials ServiceCredentials
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.serverCreds;
            }
        }
    }
}

