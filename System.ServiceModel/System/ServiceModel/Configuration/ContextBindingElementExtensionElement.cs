namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.Net.Security;
    using System.Runtime.CompilerServices;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Security;

    [TypeForwardedFrom("System.WorkflowServices, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
    public class ContextBindingElementExtensionElement : BindingElementExtensionElement
    {
        internal const string ContextExchangeMechanismName = "contextExchangeMechanism";
        internal const string ContextManagementEnabledName = "contextManagementEnabled";
        private ConfigurationPropertyCollection properties;
        private const string ProtectionLevelName = "protectionLevel";

        protected internal override BindingElement CreateBindingElement()
        {
            return new ContextBindingElement(this.ProtectionLevel, this.ContextExchangeMechanism, this.ClientCallbackAddress, this.ContextManagementEnabled);
        }

        public override System.Type BindingElementType
        {
            get
            {
                return typeof(ContextBindingElement);
            }
        }

        [ConfigurationProperty("clientCallbackAddress", DefaultValue=null)]
        public Uri ClientCallbackAddress
        {
            get
            {
                return (Uri) base["clientCallbackAddress"];
            }
            set
            {
                base["clientCallbackAddress"] = value;
            }
        }

        [ServiceModelEnumValidator(typeof(ContextExchangeMechanismHelper)), ConfigurationProperty("contextExchangeMechanism", DefaultValue=0)]
        public System.ServiceModel.Channels.ContextExchangeMechanism ContextExchangeMechanism
        {
            get
            {
                return (System.ServiceModel.Channels.ContextExchangeMechanism) base["contextExchangeMechanism"];
            }
            set
            {
                base["contextExchangeMechanism"] = value;
            }
        }

        [ConfigurationProperty("contextManagementEnabled", DefaultValue=true)]
        public bool ContextManagementEnabled
        {
            get
            {
                return (bool) base["contextManagementEnabled"];
            }
            set
            {
                base["contextManagementEnabled"] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("clientCallbackAddress", typeof(Uri), null, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("contextExchangeMechanism", typeof(System.ServiceModel.Channels.ContextExchangeMechanism), System.ServiceModel.Channels.ContextExchangeMechanism.ContextSoapHeader, null, new ServiceModelEnumValidator(typeof(ContextExchangeMechanismHelper)), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("protectionLevel", typeof(System.Net.Security.ProtectionLevel), System.Net.Security.ProtectionLevel.Sign, null, new ServiceModelEnumValidator(typeof(ProtectionLevelHelper)), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("contextManagementEnabled", typeof(bool), true, null, null, ConfigurationPropertyOptions.None));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }

        [ServiceModelEnumValidator(typeof(ProtectionLevelHelper)), ConfigurationProperty("protectionLevel", DefaultValue=1)]
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

