namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.ServiceModel.Channels;

    public sealed class CompositeDuplexElement : BindingElementExtensionElement
    {
        private ConfigurationPropertyCollection properties;

        public override void ApplyConfiguration(BindingElement bindingElement)
        {
            base.ApplyConfiguration(bindingElement);
            CompositeDuplexBindingElement element = (CompositeDuplexBindingElement) bindingElement;
            if (base.ElementInformation.Properties["clientBaseAddress"].ValueOrigin != PropertyValueOrigin.Default)
            {
                element.ClientBaseAddress = this.ClientBaseAddress;
            }
        }

        public override void CopyFrom(ServiceModelExtensionElement from)
        {
            base.CopyFrom(from);
            CompositeDuplexElement element = (CompositeDuplexElement) from;
            this.ClientBaseAddress = element.ClientBaseAddress;
        }

        protected internal override BindingElement CreateBindingElement()
        {
            CompositeDuplexBindingElement bindingElement = new CompositeDuplexBindingElement();
            this.ApplyConfiguration(bindingElement);
            return bindingElement;
        }

        protected internal override void InitializeFrom(BindingElement bindingElement)
        {
            base.InitializeFrom(bindingElement);
            CompositeDuplexBindingElement element = (CompositeDuplexBindingElement) bindingElement;
            this.ClientBaseAddress = element.ClientBaseAddress;
        }

        public override System.Type BindingElementType
        {
            get
            {
                return typeof(CompositeDuplexBindingElement);
            }
        }

        [ConfigurationProperty("clientBaseAddress", DefaultValue=null)]
        public Uri ClientBaseAddress
        {
            get
            {
                return (Uri) base["clientBaseAddress"];
            }
            set
            {
                base["clientBaseAddress"] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("clientBaseAddress", typeof(Uri), null, null, null, ConfigurationPropertyOptions.None));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }
    }
}

