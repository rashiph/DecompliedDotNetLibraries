namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.ServiceModel.Security;

    public sealed class HttpDigestClientElement : ConfigurationElement
    {
        private ConfigurationPropertyCollection properties;

        internal void ApplyConfiguration(HttpDigestClientCredential digest)
        {
            if (digest == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("digest");
            }
            digest.AllowedImpersonationLevel = this.ImpersonationLevel;
        }

        public void Copy(HttpDigestClientElement from)
        {
            if (this.IsReadOnly())
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.ServiceModel.SR.GetString("ConfigReadOnly")));
            }
            if (from == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("from");
            }
            this.ImpersonationLevel = from.ImpersonationLevel;
        }

        [ConfigurationProperty("impersonationLevel", DefaultValue=2), ServiceModelEnumValidator(typeof(TokenImpersonationLevelHelper))]
        public TokenImpersonationLevel ImpersonationLevel
        {
            get
            {
                return (TokenImpersonationLevel) base["impersonationLevel"];
            }
            set
            {
                base["impersonationLevel"] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("impersonationLevel", typeof(TokenImpersonationLevel), TokenImpersonationLevel.Identification, null, new ServiceModelEnumValidator(typeof(TokenImpersonationLevelHelper)), ConfigurationPropertyOptions.None));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }
    }
}

