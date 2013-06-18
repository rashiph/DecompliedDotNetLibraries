namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.ServiceModel.Channels;

    public sealed class SslStreamSecurityElement : BindingElementExtensionElement
    {
        private ConfigurationPropertyCollection properties;

        public override void ApplyConfiguration(BindingElement bindingElement)
        {
            base.ApplyConfiguration(bindingElement);
            SslStreamSecurityBindingElement element = (SslStreamSecurityBindingElement) bindingElement;
            element.RequireClientCertificate = this.RequireClientCertificate;
        }

        public override void CopyFrom(ServiceModelExtensionElement from)
        {
            base.CopyFrom(from);
            SslStreamSecurityElement element = (SslStreamSecurityElement) from;
            this.RequireClientCertificate = element.RequireClientCertificate;
        }

        protected internal override BindingElement CreateBindingElement()
        {
            SslStreamSecurityBindingElement bindingElement = new SslStreamSecurityBindingElement();
            this.ApplyConfiguration(bindingElement);
            return bindingElement;
        }

        protected internal override void InitializeFrom(BindingElement bindingElement)
        {
            base.InitializeFrom(bindingElement);
            SslStreamSecurityBindingElement element = (SslStreamSecurityBindingElement) bindingElement;
            this.RequireClientCertificate = element.RequireClientCertificate;
        }

        public override System.Type BindingElementType
        {
            get
            {
                return typeof(SslStreamSecurityBindingElement);
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("requireClientCertificate", typeof(bool), false, null, null, ConfigurationPropertyOptions.None));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }

        [ConfigurationProperty("requireClientCertificate", DefaultValue=false)]
        public bool RequireClientCertificate
        {
            get
            {
                return (bool) base["requireClientCertificate"];
            }
            set
            {
                base["requireClientCertificate"] = value;
            }
        }
    }
}

