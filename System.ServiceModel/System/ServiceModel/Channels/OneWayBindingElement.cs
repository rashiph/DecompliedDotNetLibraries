namespace System.ServiceModel.Channels
{
    using System;
    using System.ComponentModel;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Security;
    using System.Xml;

    public sealed class OneWayBindingElement : BindingElement, IPolicyExportExtension
    {
        private System.ServiceModel.Channels.ChannelPoolSettings channelPoolSettings;
        private int maxAcceptedChannels;
        private static MessagePartSpecification oneWaySignedMessageParts;
        private bool packetRoutable;

        public OneWayBindingElement()
        {
            this.channelPoolSettings = new System.ServiceModel.Channels.ChannelPoolSettings();
            this.packetRoutable = false;
            this.maxAcceptedChannels = 10;
        }

        private OneWayBindingElement(OneWayBindingElement elementToBeCloned) : base(elementToBeCloned)
        {
            this.channelPoolSettings = elementToBeCloned.ChannelPoolSettings.Clone();
            this.packetRoutable = elementToBeCloned.PacketRoutable;
            this.maxAcceptedChannels = elementToBeCloned.maxAcceptedChannels;
        }

        public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            if (typeof(TChannel) != typeof(IOutputChannel))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("TChannel", System.ServiceModel.SR.GetString("ChannelTypeNotSupported", new object[] { typeof(TChannel) }));
            }
            if (context.CanBuildInnerChannelFactory<IDuplexChannel>())
            {
                return (IChannelFactory<TChannel>) new DuplexOneWayChannelFactory(this, context);
            }
            if (context.CanBuildInnerChannelFactory<IDuplexSessionChannel>())
            {
                return (IChannelFactory<TChannel>) new DuplexSessionOneWayChannelFactory(this, context);
            }
            if (!context.CanBuildInnerChannelFactory<IRequestChannel>())
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("OneWayInternalTypeNotSupported", new object[] { context.Binding.Name })));
            }
            return (IChannelFactory<TChannel>) new RequestOneWayChannelFactory(this, context);
        }

        public override IChannelListener<TChannel> BuildChannelListener<TChannel>(BindingContext context) where TChannel: class, IChannel
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            if (typeof(TChannel) != typeof(IInputChannel))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("TChannel", System.ServiceModel.SR.GetString("ChannelTypeNotSupported", new object[] { typeof(TChannel) }));
            }
            if (context.CanBuildInnerChannelListener<IDuplexChannel>())
            {
                return (IChannelListener<TChannel>) new DuplexOneWayChannelListener(this, context);
            }
            if (context.CanBuildInnerChannelListener<IDuplexSessionChannel>())
            {
                return (IChannelListener<TChannel>) new DuplexSessionOneWayChannelListener(this, context);
            }
            if (!context.CanBuildInnerChannelListener<IReplyChannel>())
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("OneWayInternalTypeNotSupported", new object[] { context.Binding.Name })));
            }
            return (IChannelListener<TChannel>) new ReplyOneWayChannelListener(this, context);
        }

        public override bool CanBuildChannelFactory<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            if (typeof(TChannel) != typeof(IOutputChannel))
            {
                return false;
            }
            return (context.CanBuildInnerChannelFactory<IDuplexChannel>() || (context.CanBuildInnerChannelFactory<IDuplexSessionChannel>() || context.CanBuildInnerChannelFactory<IRequestChannel>()));
        }

        public override bool CanBuildChannelListener<TChannel>(BindingContext context) where TChannel: class, IChannel
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            if (typeof(TChannel) != typeof(IInputChannel))
            {
                return false;
            }
            return (context.CanBuildInnerChannelListener<IDuplexChannel>() || (context.CanBuildInnerChannelListener<IDuplexSessionChannel>() || context.CanBuildInnerChannelListener<IReplyChannel>()));
        }

        public override BindingElement Clone()
        {
            return new OneWayBindingElement(this);
        }

        public override T GetProperty<T>(BindingContext context) where T: class
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            if (!(typeof(T) == typeof(ChannelProtectionRequirements)))
            {
                return context.GetInnerProperty<T>();
            }
            ChannelProtectionRequirements requirements = new ChannelProtectionRequirements();
            if (this.PacketRoutable)
            {
                requirements.IncomingSignatureParts.AddParts(OneWaySignedMessageParts);
                requirements.OutgoingSignatureParts.AddParts(OneWaySignedMessageParts);
            }
            ChannelProtectionRequirements innerProperty = context.GetInnerProperty<ChannelProtectionRequirements>();
            if (innerProperty != null)
            {
                requirements.Add(innerProperty);
            }
            return (T) requirements;
        }

        internal override bool IsMatch(BindingElement b)
        {
            if (b == null)
            {
                return false;
            }
            OneWayBindingElement element = b as OneWayBindingElement;
            if (element == null)
            {
                return false;
            }
            if (!this.channelPoolSettings.IsMatch(element.ChannelPoolSettings))
            {
                return false;
            }
            if (this.packetRoutable != element.PacketRoutable)
            {
                return false;
            }
            if (this.maxAcceptedChannels != element.MaxAcceptedChannels)
            {
                return false;
            }
            return true;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeChannelPoolSettings()
        {
            return this.channelPoolSettings.InternalShouldSerialize();
        }

        void IPolicyExportExtension.ExportPolicy(MetadataExporter exporter, PolicyConversionContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            if (context.BindingElements != null)
            {
                OneWayBindingElement element = context.BindingElements.Find<OneWayBindingElement>();
                if (element != null)
                {
                    XmlDocument document = new XmlDocument();
                    XmlElement item = document.CreateElement("ow", "OneWay", "http://schemas.microsoft.com/ws/2005/05/routing/policy");
                    if (element.PacketRoutable)
                    {
                        XmlElement newChild = document.CreateElement("ow", "PacketRoutable", "http://schemas.microsoft.com/ws/2005/05/routing/policy");
                        item.AppendChild(newChild);
                    }
                    context.GetBindingAssertions().Add(item);
                }
            }
        }

        public System.ServiceModel.Channels.ChannelPoolSettings ChannelPoolSettings
        {
            get
            {
                return this.channelPoolSettings;
            }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                this.channelPoolSettings = value;
            }
        }

        [DefaultValue(10)]
        public int MaxAcceptedChannels
        {
            get
            {
                return this.maxAcceptedChannels;
            }
            set
            {
                if (value <= 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value, System.ServiceModel.SR.GetString("ValueMustBePositive")));
                }
                this.maxAcceptedChannels = value;
            }
        }

        private static MessagePartSpecification OneWaySignedMessageParts
        {
            get
            {
                if (oneWaySignedMessageParts == null)
                {
                    MessagePartSpecification specification = new MessagePartSpecification(new XmlQualifiedName[] { new XmlQualifiedName("PacketRoutable", "http://schemas.microsoft.com/ws/2005/05/routing") });
                    specification.MakeReadOnly();
                    oneWaySignedMessageParts = specification;
                }
                return oneWaySignedMessageParts;
            }
        }

        [DefaultValue(false)]
        public bool PacketRoutable
        {
            get
            {
                return this.packetRoutable;
            }
            set
            {
                this.packetRoutable = value;
            }
        }
    }
}

