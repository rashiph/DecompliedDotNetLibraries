namespace System.Runtime.Serialization.Configuration
{
    using System;
    using System.Configuration;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Security;

    public sealed class DeclaredTypeElement : ConfigurationElement
    {
        private ConfigurationPropertyCollection properties;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public DeclaredTypeElement()
        {
        }

        public DeclaredTypeElement(string typeName) : this()
        {
            if (string.IsNullOrEmpty(typeName))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("typeName");
            }
            this.Type = typeName;
        }

        [SecuritySafeCritical]
        protected override void PostDeserialize()
        {
            if (!base.EvaluationContext.IsMachineLevel && !PartialTrustHelpers.IsInFullTrust())
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.Runtime.Serialization.SR.GetString("ConfigDataContractSerializerSectionLoadError")));
            }
        }

        [ConfigurationProperty("", DefaultValue=null, Options=ConfigurationPropertyOptions.IsDefaultCollection)]
        public TypeElementCollection KnownTypes
        {
            get
            {
                return (TypeElementCollection) base[""];
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("", typeof(TypeElementCollection), null, null, null, ConfigurationPropertyOptions.IsDefaultCollection));
                    propertys.Add(new ConfigurationProperty("type", typeof(string), string.Empty, null, new DeclaredTypeValidator(), ConfigurationPropertyOptions.IsKey));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }

        [ConfigurationProperty("type", DefaultValue="", Options=ConfigurationPropertyOptions.IsKey), DeclaredTypeValidator]
        public string Type
        {
            get
            {
                return (string) base["type"];
            }
            set
            {
                base["type"] = value;
            }
        }
    }
}

