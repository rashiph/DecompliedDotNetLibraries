namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.ServiceModel;
    using System.ServiceModel.Security;

    public sealed class WindowsServiceElement : ConfigurationElement
    {
        private ConfigurationPropertyCollection properties;

        internal void ApplyConfiguration(WindowsServiceCredential windows)
        {
            if (windows == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("windows");
            }
            windows.AllowAnonymousLogons = this.AllowAnonymousLogons;
            windows.IncludeWindowsGroups = this.IncludeWindowsGroups;
        }

        public void Copy(WindowsServiceElement from)
        {
            if (this.IsReadOnly())
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.ServiceModel.SR.GetString("ConfigReadOnly")));
            }
            if (from == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("from");
            }
            this.AllowAnonymousLogons = from.AllowAnonymousLogons;
            this.IncludeWindowsGroups = from.IncludeWindowsGroups;
        }

        [ConfigurationProperty("allowAnonymousLogons", DefaultValue=false)]
        public bool AllowAnonymousLogons
        {
            get
            {
                return (bool) base["allowAnonymousLogons"];
            }
            set
            {
                base["allowAnonymousLogons"] = value;
            }
        }

        [ConfigurationProperty("includeWindowsGroups", DefaultValue=true)]
        public bool IncludeWindowsGroups
        {
            get
            {
                return (bool) base["includeWindowsGroups"];
            }
            set
            {
                base["includeWindowsGroups"] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("includeWindowsGroups", typeof(bool), true, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("allowAnonymousLogons", typeof(bool), false, null, null, ConfigurationPropertyOptions.None));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }
    }
}

