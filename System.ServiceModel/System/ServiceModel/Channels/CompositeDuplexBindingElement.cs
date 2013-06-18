namespace System.ServiceModel.Channels
{
    using System;
    using System.ComponentModel;
    using System.Net.Security;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Security;
    using System.Xml;

    public sealed class CompositeDuplexBindingElement : BindingElement, IPolicyExportExtension
    {
        private Uri clientBaseAddress;

        public CompositeDuplexBindingElement()
        {
        }

        private CompositeDuplexBindingElement(CompositeDuplexBindingElement elementToBeCloned) : base(elementToBeCloned)
        {
            this.clientBaseAddress = elementToBeCloned.ClientBaseAddress;
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
            return context.BuildInnerChannelFactory<TChannel>();
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
            if (context.ListenUriBaseAddress == null)
            {
                if (this.clientBaseAddress != null)
                {
                    context.ListenUriBaseAddress = this.clientBaseAddress;
                    context.ListenUriRelativeAddress = Guid.NewGuid().ToString();
                    context.ListenUriMode = ListenUriMode.Explicit;
                }
                else
                {
                    context.ListenUriRelativeAddress = string.Empty;
                    context.ListenUriMode = ListenUriMode.Unique;
                }
            }
            return context.BuildInnerChannelListener<TChannel>();
        }

        public override bool CanBuildChannelFactory<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            return ((typeof(TChannel) == typeof(IOutputChannel)) && context.CanBuildInnerChannelFactory<IOutputChannel>());
        }

        public override bool CanBuildChannelListener<TChannel>(BindingContext context) where TChannel: class, IChannel
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            return ((typeof(TChannel) == typeof(IInputChannel)) && context.CanBuildInnerChannelListener<IInputChannel>());
        }

        public override BindingElement Clone()
        {
            return new CompositeDuplexBindingElement(this);
        }

        private static XmlElement CreateCompositeDuplexAssertion()
        {
            XmlDocument document = new XmlDocument();
            return document.CreateElement("cdp", "CompositeDuplex", "http://schemas.microsoft.com/net/2006/06/duplex");
        }

        public override T GetProperty<T>(BindingContext context) where T: class
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            if (typeof(T) == typeof(ISecurityCapabilities))
            {
                ISecurityCapabilities innerProperty = context.GetInnerProperty<ISecurityCapabilities>();
                if (innerProperty != null)
                {
                    return (T) new SecurityCapabilities(innerProperty.SupportsClientAuthentication, false, innerProperty.SupportsClientWindowsIdentity, innerProperty.SupportedRequestProtectionLevel, ProtectionLevel.None);
                }
                return default(T);
            }
            if (!(typeof(T) == typeof(ChannelProtectionRequirements)))
            {
                return context.GetInnerProperty<T>();
            }
            ChannelProtectionRequirements protectionRequirements = this.GetProtectionRequirements();
            protectionRequirements.Add(context.GetInnerProperty<ChannelProtectionRequirements>() ?? new ChannelProtectionRequirements());
            return (T) protectionRequirements;
        }

        private ChannelProtectionRequirements GetProtectionRequirements()
        {
            ChannelProtectionRequirements requirements = new ChannelProtectionRequirements();
            XmlQualifiedName name = new XmlQualifiedName(XD.UtilityDictionary.UniqueEndpointHeaderName.Value, XD.UtilityDictionary.UniqueEndpointHeaderNamespace.Value);
            MessagePartSpecification parts = new MessagePartSpecification(new XmlQualifiedName[] { name });
            parts.MakeReadOnly();
            requirements.IncomingSignatureParts.AddParts(parts);
            requirements.OutgoingSignatureParts.AddParts(parts);
            return requirements;
        }

        internal override bool IsMatch(BindingElement b)
        {
            if (b == null)
            {
                return false;
            }
            CompositeDuplexBindingElement element = b as CompositeDuplexBindingElement;
            if (element == null)
            {
                return false;
            }
            return (this.clientBaseAddress == element.clientBaseAddress);
        }

        void IPolicyExportExtension.ExportPolicy(MetadataExporter exporter, PolicyConversionContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            exporter.State[typeof(SupportedAddressingMode).Name] = SupportedAddressingMode.NonAnonymous;
            context.GetBindingAssertions().Add(CreateCompositeDuplexAssertion());
        }

        [DefaultValue((string) null)]
        public Uri ClientBaseAddress
        {
            get
            {
                return this.clientBaseAddress;
            }
            set
            {
                this.clientBaseAddress = value;
            }
        }
    }
}

