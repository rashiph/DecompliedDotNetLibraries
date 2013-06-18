namespace System.ServiceModel.Channels
{
    using System;
    using System.Net.Security;
    using System.ServiceModel;

    internal sealed class InternalDuplexBindingElement : BindingElement
    {
        private InputChannelDemuxer clientChannelDemuxer;
        private bool providesCorrelation;

        public InternalDuplexBindingElement() : this(false)
        {
        }

        internal InternalDuplexBindingElement(bool providesCorrelation)
        {
            this.providesCorrelation = providesCorrelation;
        }

        private InternalDuplexBindingElement(InternalDuplexBindingElement elementToBeCloned) : base(elementToBeCloned)
        {
            this.clientChannelDemuxer = elementToBeCloned.ClientChannelDemuxer;
            this.providesCorrelation = elementToBeCloned.ProvidesCorrelation;
        }

        public static void AddDuplexFactorySupport(BindingContext context, ref InternalDuplexBindingElement internalDuplexBindingElement)
        {
            if ((((((!context.CanBuildInnerChannelFactory<IDuplexChannel>() && (context.RemainingBindingElements.Find<CompositeDuplexBindingElement>() != null)) && (context.CanBuildInnerChannelFactory<IOutputChannel>() && context.CanBuildInnerChannelListener<IInputChannel>())) && !context.CanBuildInnerChannelFactory<IRequestChannel>()) && !context.CanBuildInnerChannelFactory<IRequestSessionChannel>()) && !context.CanBuildInnerChannelFactory<IOutputSessionChannel>()) && !context.CanBuildInnerChannelFactory<IDuplexSessionChannel>())
            {
                if (internalDuplexBindingElement == null)
                {
                    internalDuplexBindingElement = new InternalDuplexBindingElement();
                }
                context.RemainingBindingElements.Insert(0, internalDuplexBindingElement);
            }
        }

        public static void AddDuplexListenerSupport(BindingContext context, ref InternalDuplexBindingElement internalDuplexBindingElement)
        {
            if ((((((!context.CanBuildInnerChannelListener<IDuplexChannel>() && (context.RemainingBindingElements.Find<CompositeDuplexBindingElement>() != null)) && (context.CanBuildInnerChannelFactory<IOutputChannel>() && context.CanBuildInnerChannelListener<IInputChannel>())) && !context.CanBuildInnerChannelListener<IReplyChannel>()) && !context.CanBuildInnerChannelListener<IReplySessionChannel>()) && !context.CanBuildInnerChannelListener<IInputSessionChannel>()) && !context.CanBuildInnerChannelListener<IDuplexSessionChannel>())
            {
                if (internalDuplexBindingElement == null)
                {
                    internalDuplexBindingElement = new InternalDuplexBindingElement();
                }
                context.RemainingBindingElements.Insert(0, internalDuplexBindingElement);
            }
        }

        public static void AddDuplexListenerSupport(CustomBinding binding, ref InternalDuplexBindingElement internalDuplexBindingElement)
        {
            if ((((((!binding.CanBuildChannelListener<IDuplexChannel>(new object[0]) && (binding.Elements.Find<CompositeDuplexBindingElement>() != null)) && (binding.CanBuildChannelFactory<IOutputChannel>(new object[0]) && binding.CanBuildChannelListener<IInputChannel>(new object[0]))) && !binding.CanBuildChannelListener<IReplyChannel>(new object[0])) && !binding.CanBuildChannelListener<IReplySessionChannel>(new object[0])) && !binding.CanBuildChannelListener<IInputSessionChannel>(new object[0])) && !binding.CanBuildChannelListener<IDuplexSessionChannel>(new object[0]))
            {
                if (internalDuplexBindingElement == null)
                {
                    internalDuplexBindingElement = new InternalDuplexBindingElement();
                }
                binding.Elements.Insert(0, internalDuplexBindingElement);
            }
        }

        public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            if (!this.CanBuildChannelFactory<TChannel>(context))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("TChannel", System.ServiceModel.SR.GetString("ChannelTypeNotSupported", new object[] { typeof(TChannel) }));
            }
            IChannelFactory<IOutputChannel> innerChannelFactory = context.Clone().BuildInnerChannelFactory<IOutputChannel>();
            if (this.clientChannelDemuxer == null)
            {
                this.clientChannelDemuxer = new InputChannelDemuxer(context);
            }
            else
            {
                context.RemainingBindingElements.Clear();
            }
            LocalAddressProvider localAddressProvider = context.BindingParameters.Remove<LocalAddressProvider>();
            return (IChannelFactory<TChannel>) new InternalDuplexChannelFactory(this, context, this.clientChannelDemuxer, innerChannelFactory, localAddressProvider);
        }

        public override IChannelListener<TChannel> BuildChannelListener<TChannel>(BindingContext context) where TChannel: class, IChannel
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            if (typeof(TChannel) != typeof(IDuplexChannel))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("TChannel", System.ServiceModel.SR.GetString("ChannelTypeNotSupported", new object[] { typeof(TChannel) }));
            }
            return (IChannelListener<TChannel>) new InternalDuplexChannelListener(this, context);
        }

        public override bool CanBuildChannelFactory<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            return (((typeof(TChannel) == typeof(IDuplexChannel)) && context.CanBuildInnerChannelFactory<IOutputChannel>()) && context.CanBuildInnerChannelListener<IInputChannel>());
        }

        public override bool CanBuildChannelListener<TChannel>(BindingContext context) where TChannel: class, IChannel
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            return (((typeof(TChannel) == typeof(IDuplexChannel)) && context.CanBuildInnerChannelFactory<IOutputChannel>()) && context.CanBuildInnerChannelListener<IInputChannel>());
        }

        public override BindingElement Clone()
        {
            return new InternalDuplexBindingElement(this);
        }

        public override T GetProperty<T>(BindingContext context) where T: class
        {
            if ((typeof(T) == typeof(ISecurityCapabilities)) && !this.ProvidesCorrelation)
            {
                return GetSecurityCapabilities<T>(context.GetInnerProperty<ISecurityCapabilities>());
            }
            return context.GetInnerProperty<T>();
        }

        internal static T GetSecurityCapabilities<T>(ISecurityCapabilities lowerCapabilities)
        {
            if (lowerCapabilities != null)
            {
                return (T) new SecurityCapabilities(lowerCapabilities.SupportsClientAuthentication, false, lowerCapabilities.SupportsClientWindowsIdentity, lowerCapabilities.SupportedRequestProtectionLevel, ProtectionLevel.None);
            }
            return null;
        }

        internal override bool IsMatch(BindingElement b)
        {
            if (b == null)
            {
                return false;
            }
            return (b is InternalDuplexBindingElement);
        }

        internal InputChannelDemuxer ClientChannelDemuxer
        {
            get
            {
                return this.clientChannelDemuxer;
            }
        }

        internal bool ProvidesCorrelation
        {
            get
            {
                return this.providesCorrelation;
            }
        }
    }
}

