namespace System.ServiceModel.Channels
{
    using System;
    using System.ComponentModel;
    using System.Net.Security;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Security;
    using System.Xml;

    public class WindowsStreamSecurityBindingElement : StreamUpgradeBindingElement, ITransportTokenAssertionProvider, IPolicyExportExtension
    {
        private System.Net.Security.ProtectionLevel protectionLevel;

        public WindowsStreamSecurityBindingElement()
        {
            this.protectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign;
        }

        protected WindowsStreamSecurityBindingElement(WindowsStreamSecurityBindingElement elementToBeCloned) : base(elementToBeCloned)
        {
            this.protectionLevel = elementToBeCloned.protectionLevel;
        }

        public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            context.BindingParameters.Add(this);
            return context.BuildInnerChannelFactory<TChannel>();
        }

        public override IChannelListener<TChannel> BuildChannelListener<TChannel>(BindingContext context) where TChannel: class, IChannel
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            context.BindingParameters.Add(this);
            return context.BuildInnerChannelListener<TChannel>();
        }

        public override StreamUpgradeProvider BuildClientStreamUpgradeProvider(BindingContext context)
        {
            return new WindowsStreamSecurityUpgradeProvider(this, context, true);
        }

        public override StreamUpgradeProvider BuildServerStreamUpgradeProvider(BindingContext context)
        {
            return new WindowsStreamSecurityUpgradeProvider(this, context, false);
        }

        public override bool CanBuildChannelFactory<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            context.BindingParameters.Add(this);
            return context.CanBuildInnerChannelFactory<TChannel>();
        }

        public override bool CanBuildChannelListener<TChannel>(BindingContext context) where TChannel: class, IChannel
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            context.BindingParameters.Add(this);
            return context.CanBuildInnerChannelListener<TChannel>();
        }

        public override BindingElement Clone()
        {
            return new WindowsStreamSecurityBindingElement(this);
        }

        public override T GetProperty<T>(BindingContext context) where T: class
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            if (typeof(T) == typeof(ISecurityCapabilities))
            {
                return (T) new SecurityCapabilities(true, true, true, this.protectionLevel, this.protectionLevel);
            }
            if (typeof(T) == typeof(IdentityVerifier))
            {
                return (T) IdentityVerifier.CreateDefault();
            }
            return context.GetInnerProperty<T>();
        }

        public XmlElement GetTransportTokenAssertion()
        {
            XmlDocument document = new XmlDocument();
            XmlElement element = document.CreateElement("msf", "WindowsTransportSecurity", "http://schemas.microsoft.com/ws/2006/05/framing/policy");
            XmlElement newChild = document.CreateElement("msf", "ProtectionLevel", "http://schemas.microsoft.com/ws/2006/05/framing/policy");
            newChild.AppendChild(document.CreateTextNode(this.ProtectionLevel.ToString()));
            element.AppendChild(newChild);
            return element;
        }

        internal static void ImportPolicy(MetadataImporter importer, PolicyConversionContext policyContext)
        {
            XmlElement node = PolicyConversionContext.FindAssertion(policyContext.GetBindingAssertions(), "WindowsTransportSecurity", "http://schemas.microsoft.com/ws/2006/05/framing/policy", true);
            if (node != null)
            {
                WindowsStreamSecurityBindingElement item = new WindowsStreamSecurityBindingElement();
                XmlReader reader = new XmlNodeReader(node);
                reader.ReadStartElement();
                string str = null;
                if (reader.IsStartElement("ProtectionLevel", "http://schemas.microsoft.com/ws/2006/05/framing/policy") && !reader.IsEmptyElement)
                {
                    str = reader.ReadElementContentAsString();
                }
                if (string.IsNullOrEmpty(str))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.ServiceModel.SR.GetString("ExpectedElementMissing", new object[] { "ProtectionLevel", "http://schemas.microsoft.com/ws/2006/05/framing/policy" })));
                }
                item.ProtectionLevel = (System.Net.Security.ProtectionLevel) Enum.Parse(typeof(System.Net.Security.ProtectionLevel), str);
                policyContext.BindingElements.Add(item);
            }
        }

        internal override bool IsMatch(BindingElement b)
        {
            if (b == null)
            {
                return false;
            }
            WindowsStreamSecurityBindingElement element = b as WindowsStreamSecurityBindingElement;
            if (element == null)
            {
                return false;
            }
            if (this.protectionLevel != element.protectionLevel)
            {
                return false;
            }
            return true;
        }

        void IPolicyExportExtension.ExportPolicy(MetadataExporter exporter, PolicyConversionContext context)
        {
            if (exporter == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("exporter");
            }
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            SecurityBindingElement.ExportPolicyForTransportTokenAssertionProviders(exporter, context);
        }

        [DefaultValue(2)]
        public System.Net.Security.ProtectionLevel ProtectionLevel
        {
            get
            {
                return this.protectionLevel;
            }
            set
            {
                ProtectionLevelHelper.Validate(value);
                this.protectionLevel = value;
            }
        }
    }
}

