namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.Net.Security;
    using System.ServiceModel.Channels;

    public sealed class WindowsStreamSecurityElement : BindingElementExtensionElement
    {
        private ConfigurationPropertyCollection properties;

        public override void ApplyConfiguration(BindingElement bindingElement)
        {
            base.ApplyConfiguration(bindingElement);
            WindowsStreamSecurityBindingElement element = (WindowsStreamSecurityBindingElement) bindingElement;
            element.ProtectionLevel = this.ProtectionLevel;
        }

        public override void CopyFrom(ServiceModelExtensionElement from)
        {
            base.CopyFrom(from);
            WindowsStreamSecurityElement element = (WindowsStreamSecurityElement) from;
            this.ProtectionLevel = element.ProtectionLevel;
        }

        protected internal override BindingElement CreateBindingElement()
        {
            WindowsStreamSecurityBindingElement bindingElement = new WindowsStreamSecurityBindingElement();
            this.ApplyConfiguration(bindingElement);
            return bindingElement;
        }

        protected internal override void InitializeFrom(BindingElement bindingElement)
        {
            base.InitializeFrom(bindingElement);
            WindowsStreamSecurityBindingElement element = (WindowsStreamSecurityBindingElement) bindingElement;
            this.ProtectionLevel = element.ProtectionLevel;
        }

        public override System.Type BindingElementType
        {
            get
            {
                return typeof(WindowsStreamSecurityBindingElement);
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("protectionLevel", typeof(System.Net.Security.ProtectionLevel), System.Net.Security.ProtectionLevel.EncryptAndSign, null, new StandardRuntimeEnumValidator(typeof(System.Net.Security.ProtectionLevel)), ConfigurationPropertyOptions.None));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }

        [StandardRuntimeEnumValidator(typeof(System.Net.Security.ProtectionLevel)), ConfigurationProperty("protectionLevel", DefaultValue=2)]
        public System.Net.Security.ProtectionLevel ProtectionLevel
        {
            get
            {
                return (System.Net.Security.ProtectionLevel) base["protectionLevel"];
            }
            set
            {
                base["protectionLevel"] = value;
            }
        }
    }
}

