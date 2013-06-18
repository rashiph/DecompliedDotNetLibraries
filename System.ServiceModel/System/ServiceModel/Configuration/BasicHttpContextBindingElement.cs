namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    [TypeForwardedFrom("System.WorkflowServices, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
    public class BasicHttpContextBindingElement : BasicHttpBindingElement
    {
        private const string ContextManagementEnabledName = "contextManagementEnabled";
        private ConfigurationPropertyCollection properties;

        public BasicHttpContextBindingElement()
        {
        }

        public BasicHttpContextBindingElement(string name) : base(name)
        {
        }

        protected internal override void InitializeFrom(Binding binding)
        {
            base.InitializeFrom(binding);
            BasicHttpContextBinding binding2 = (BasicHttpContextBinding) binding;
            if (!binding2.ContextManagementEnabled)
            {
                this.ContextManagementEnabled = binding2.ContextManagementEnabled;
            }
        }

        protected override void OnApplyConfiguration(Binding binding)
        {
            base.OnApplyConfiguration(binding);
            if (base.ElementInformation.Properties["allowCookies"].ValueOrigin == PropertyValueOrigin.Default)
            {
                ((BasicHttpBinding) binding).AllowCookies = true;
            }
            else if (!base.AllowCookies)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.ServiceModel.SR.GetString("BasicHttpContextBindingRequiresAllowCookie", new object[] { base.Name, "" }));
            }
            ((BasicHttpContextBinding) binding).ContextManagementEnabled = this.ContextManagementEnabled;
        }

        protected override System.Type BindingElementType
        {
            get
            {
                return typeof(BasicHttpContextBinding);
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
                    ConfigurationPropertyCollection properties = base.Properties;
                    properties.Add(new ConfigurationProperty("contextManagementEnabled", typeof(bool), true, null, null, ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

