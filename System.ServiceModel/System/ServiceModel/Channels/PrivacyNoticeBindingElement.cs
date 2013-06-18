namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.Xml;

    public sealed class PrivacyNoticeBindingElement : BindingElement, IPolicyExportExtension
    {
        private Uri url;
        private int version;

        public PrivacyNoticeBindingElement()
        {
            this.url = null;
        }

        public PrivacyNoticeBindingElement(PrivacyNoticeBindingElement elementToBeCloned) : base(elementToBeCloned)
        {
            this.url = elementToBeCloned.url;
            this.version = elementToBeCloned.version;
        }

        public override BindingElement Clone()
        {
            return new PrivacyNoticeBindingElement(this);
        }

        public override T GetProperty<T>(BindingContext context) where T: class
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            return context.GetInnerProperty<T>();
        }

        internal override bool IsMatch(BindingElement b)
        {
            if (b == null)
            {
                return false;
            }
            PrivacyNoticeBindingElement element = b as PrivacyNoticeBindingElement;
            if (element == null)
            {
                return false;
            }
            return ((this.url == element.url) && (this.version == element.version));
        }

        void IPolicyExportExtension.ExportPolicy(MetadataExporter exporter, PolicyConversionContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            if (context.BindingElements != null)
            {
                PrivacyNoticeBindingElement element = context.BindingElements.Find<PrivacyNoticeBindingElement>();
                if (element != null)
                {
                    XmlElement item = new XmlDocument().CreateElement("ic", "PrivacyNotice", "http://schemas.xmlsoap.org/ws/2005/05/identity");
                    item.InnerText = element.Url.ToString();
                    item.SetAttribute("Version", "http://schemas.xmlsoap.org/ws/2005/05/identity", XmlConvert.ToString(element.Version));
                    context.GetBindingAssertions().Add(item);
                }
            }
        }

        public Uri Url
        {
            get
            {
                return this.url;
            }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                this.url = value;
            }
        }

        public int Version
        {
            get
            {
                return this.version;
            }
            set
            {
                if (value < 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value, System.ServiceModel.SR.GetString("ValueMustBePositive")));
                }
                this.version = value;
            }
        }
    }
}

