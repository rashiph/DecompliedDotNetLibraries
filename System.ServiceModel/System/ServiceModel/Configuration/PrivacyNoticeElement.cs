namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.ServiceModel.Channels;

    public class PrivacyNoticeElement : BindingElementExtensionElement
    {
        private ConfigurationPropertyCollection properties;

        public override void ApplyConfiguration(BindingElement bindingElement)
        {
            base.ApplyConfiguration(bindingElement);
            PrivacyNoticeBindingElement element = (PrivacyNoticeBindingElement) bindingElement;
            element.Url = this.Url;
            element.Version = this.Version;
        }

        public override void CopyFrom(ServiceModelExtensionElement from)
        {
            base.CopyFrom(from);
            PrivacyNoticeElement element = (PrivacyNoticeElement) from;
            this.Url = element.Url;
            this.Version = element.Version;
        }

        protected internal override BindingElement CreateBindingElement()
        {
            PrivacyNoticeBindingElement bindingElement = new PrivacyNoticeBindingElement();
            this.ApplyConfiguration(bindingElement);
            return bindingElement;
        }

        protected internal override void InitializeFrom(BindingElement bindingElement)
        {
            base.InitializeFrom(bindingElement);
            PrivacyNoticeBindingElement element = (PrivacyNoticeBindingElement) bindingElement;
            this.Url = element.Url;
            this.Version = element.Version;
        }

        public override System.Type BindingElementType
        {
            get
            {
                return typeof(PrivacyNoticeBindingElement);
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("url", typeof(Uri), null, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("version", typeof(int), 0, null, new IntegerValidator(0, 0x7fffffff, false), ConfigurationPropertyOptions.None));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }

        [ConfigurationProperty("url")]
        public Uri Url
        {
            get
            {
                return (Uri) base["url"];
            }
            set
            {
                base["url"] = value;
            }
        }

        [IntegerValidator(MinValue=0), ConfigurationProperty("version", DefaultValue=0)]
        public int Version
        {
            get
            {
                return (int) base["version"];
            }
            set
            {
                base["version"] = value;
            }
        }
    }
}

