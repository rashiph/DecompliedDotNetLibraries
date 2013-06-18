namespace System.Runtime.Serialization.Configuration
{
    using System.Configuration;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Security.Permissions;

    public sealed class DataContractSerializerSection : ConfigurationSection
    {
        private ConfigurationPropertyCollection properties;

        [SecurityCritical, ConfigurationPermission(SecurityAction.Assert, Unrestricted=true)]
        internal static DataContractSerializerSection UnsafeGetSection()
        {
            DataContractSerializerSection section = (DataContractSerializerSection) ConfigurationManager.GetSection(ConfigurationStrings.DataContractSerializerSectionPath);
            if (section == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.Runtime.Serialization.SR.GetString("ConfigDataContractSerializerSectionLoadError")));
            }
            return section;
        }

        [ConfigurationProperty("declaredTypes", DefaultValue=null)]
        public DeclaredTypeElementCollection DeclaredTypes
        {
            get
            {
                return (DeclaredTypeElementCollection) base["declaredTypes"];
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("declaredTypes", typeof(DeclaredTypeElementCollection), null, null, null, ConfigurationPropertyOptions.None));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }
    }
}

