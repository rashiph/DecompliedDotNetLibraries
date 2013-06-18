namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.ServiceModel;
    using System.ServiceModel.Security;

    public sealed class SecureConversationServiceElement : ConfigurationElement
    {
        private ConfigurationPropertyCollection properties;

        internal void ApplyConfiguration(SecureConversationServiceCredential secureConversation)
        {
            if (secureConversation == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("secureConversation");
            }
            if (!string.IsNullOrEmpty(this.SecurityStateEncoderType))
            {
                Type c = Type.GetType(this.SecurityStateEncoderType, true);
                if (!typeof(SecurityStateEncoder).IsAssignableFrom(c))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.ServiceModel.SR.GetString("ConfigInvalidSecurityStateEncoderType", new object[] { this.SecurityStateEncoderType, typeof(SecurityStateEncoder).ToString() })));
                }
                secureConversation.SecurityStateEncoder = (SecurityStateEncoder) Activator.CreateInstance(c);
            }
        }

        public void Copy(SecureConversationServiceElement from)
        {
            if (this.IsReadOnly())
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.ServiceModel.SR.GetString("ConfigReadOnly")));
            }
            if (from == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("from");
            }
            this.SecurityStateEncoderType = from.SecurityStateEncoderType;
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("securityStateEncoderType", typeof(string), string.Empty, null, new StringValidator(0, 0x7fffffff, null), ConfigurationPropertyOptions.None));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }

        [StringValidator(MinLength=0), ConfigurationProperty("securityStateEncoderType", DefaultValue="")]
        public string SecurityStateEncoderType
        {
            get
            {
                return (string) base["securityStateEncoderType"];
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = string.Empty;
                }
                base["securityStateEncoderType"] = value;
            }
        }
    }
}

