namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.Net.Security;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Security;

    [TypeForwardedFrom("System.WorkflowServices, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
    public class NetTcpContextBindingElement : NetTcpBindingElement
    {
        private const string ContextManagementEnabledName = "contextManagementEnabled";
        private const string ContextProtectionLevelName = "contextProtectionLevel";
        private ConfigurationPropertyCollection properties;

        public NetTcpContextBindingElement()
        {
        }

        public NetTcpContextBindingElement(string name) : base(name)
        {
        }

        protected internal override void InitializeFrom(Binding binding)
        {
            base.InitializeFrom(binding);
            NetTcpContextBinding binding2 = (NetTcpContextBinding) binding;
            this.ClientCallbackAddress = binding2.ClientCallbackAddress;
            if (!binding2.ContextManagementEnabled)
            {
                this.ContextManagementEnabled = binding2.ContextManagementEnabled;
            }
            this.ContextProtectionLevel = binding2.ContextProtectionLevel;
        }

        protected override void OnApplyConfiguration(Binding binding)
        {
            base.OnApplyConfiguration(binding);
            NetTcpContextBinding binding2 = (NetTcpContextBinding) binding;
            binding2.ClientCallbackAddress = this.ClientCallbackAddress;
            binding2.ContextManagementEnabled = this.ContextManagementEnabled;
            binding2.ContextProtectionLevel = this.ContextProtectionLevel;
        }

        protected override System.Type BindingElementType
        {
            get
            {
                return typeof(NetTcpContextBinding);
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

        [ConfigurationProperty("contextProtectionLevel", DefaultValue=1), ServiceModelEnumValidator(typeof(ProtectionLevelHelper))]
        public ProtectionLevel ContextProtectionLevel
        {
            get
            {
                return (ProtectionLevel) base["contextProtectionLevel"];
            }
            set
            {
                base["contextProtectionLevel"] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = base.Properties;
                    properties.Add(new ConfigurationProperty("clientCallbackAddress", typeof(Uri), null, null, null, ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("contextManagementEnabled", typeof(bool), true, null, null, ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("contextProtectionLevel", typeof(ProtectionLevel), ProtectionLevel.Sign, null, new ServiceModelEnumValidator(typeof(ProtectionLevelHelper)), ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

