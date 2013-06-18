namespace System.ServiceModel.Channels
{
    using System;
    using System.ComponentModel;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.PeerResolvers;

    public sealed class PeerCustomResolverBindingElement : PeerResolverBindingElement
    {
        private EndpointAddress address;
        private System.ServiceModel.Channels.Binding binding;
        private string bindingConfiguration;
        private string bindingSection;
        private ClientCredentials credentials;
        private PeerReferralPolicy referralPolicy;
        private PeerResolver resolver;

        public PeerCustomResolverBindingElement()
        {
        }

        public PeerCustomResolverBindingElement(PeerCustomResolverBindingElement other) : base(other)
        {
            this.address = other.address;
            this.bindingConfiguration = other.bindingConfiguration;
            this.bindingSection = other.bindingSection;
            this.binding = other.binding;
            this.resolver = other.resolver;
            this.credentials = other.credentials;
        }

        public PeerCustomResolverBindingElement(PeerCustomResolverSettings settings)
        {
            if (settings != null)
            {
                this.address = settings.Address;
                this.binding = settings.Binding;
                this.resolver = settings.Resolver;
                this.bindingConfiguration = settings.BindingConfiguration;
                this.bindingSection = settings.BindingSection;
            }
        }

        public PeerCustomResolverBindingElement(BindingContext context, PeerCustomResolverSettings settings) : this(settings)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("context"));
            }
            this.credentials = context.BindingParameters.Find<ClientCredentials>();
        }

        public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("context"));
            }
            context.BindingParameters.Add(this);
            this.credentials = context.BindingParameters.Find<ClientCredentials>();
            return context.BuildInnerChannelFactory<TChannel>();
        }

        public override IChannelListener<TChannel> BuildChannelListener<TChannel>(BindingContext context) where TChannel: class, IChannel
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("context"));
            }
            context.BindingParameters.Add(this);
            this.credentials = context.BindingParameters.Find<ClientCredentials>();
            return context.BuildInnerChannelListener<TChannel>();
        }

        public override bool CanBuildChannelFactory<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("context"));
            }
            this.credentials = context.BindingParameters.Find<ClientCredentials>();
            context.BindingParameters.Add(this);
            return context.CanBuildInnerChannelFactory<TChannel>();
        }

        public override bool CanBuildChannelListener<TChannel>(BindingContext context) where TChannel: class, IChannel
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("context"));
            }
            this.credentials = context.BindingParameters.Find<ClientCredentials>();
            context.BindingParameters.Add(this);
            return context.CanBuildInnerChannelListener<TChannel>();
        }

        public override BindingElement Clone()
        {
            return new PeerCustomResolverBindingElement(this);
        }

        public override PeerResolver CreatePeerResolver()
        {
            if (this.resolver == null)
            {
                if ((this.address == null) || ((this.binding == null) && (string.IsNullOrEmpty(this.bindingSection) || string.IsNullOrEmpty(this.bindingConfiguration))))
                {
                    PeerExceptionHelper.ThrowArgument_InsufficientResolverSettings();
                }
                if (this.binding == null)
                {
                    this.binding = ConfigLoader.LookupBinding(this.bindingSection, this.bindingConfiguration);
                    if (this.binding == null)
                    {
                        PeerExceptionHelper.ThrowArgument_InsufficientResolverSettings();
                    }
                }
                this.resolver = new PeerDefaultCustomResolverClient();
            }
            if (this.resolver != null)
            {
                this.resolver.Initialize(this.address, this.binding, this.credentials, this.referralPolicy);
                if (this.resolver is PeerDefaultCustomResolverClient)
                {
                    (this.resolver as PeerDefaultCustomResolverClient).BindingName = this.bindingSection;
                    (this.resolver as PeerDefaultCustomResolverClient).BindingConfigurationName = this.bindingConfiguration;
                }
            }
            return this.resolver;
        }

        public override T GetProperty<T>(BindingContext context) where T: class
        {
            return context.GetInnerProperty<T>();
        }

        public EndpointAddress Address
        {
            get
            {
                return this.address;
            }
            set
            {
                this.address = value;
            }
        }

        public System.ServiceModel.Channels.Binding Binding
        {
            get
            {
                return this.binding;
            }
            set
            {
                this.binding = value;
            }
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

