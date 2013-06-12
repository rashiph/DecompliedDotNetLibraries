namespace System.Net.Configuration
{
    using System;
    using System.Configuration;
    using System.Net;

    public sealed class AuthenticationModulesSection : ConfigurationSection
    {
        private readonly ConfigurationProperty authenticationModules = new ConfigurationProperty(null, typeof(AuthenticationModuleElementCollection), null, ConfigurationPropertyOptions.IsDefaultCollection);
        private ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();

        public AuthenticationModulesSection()
        {
            this.properties.Add(this.authenticationModules);
        }

        protected override void InitializeDefault()
        {
            this.AuthenticationModules.Add(new AuthenticationModuleElement(typeof(NegotiateClient).AssemblyQualifiedName));
            this.AuthenticationModules.Add(new AuthenticationModuleElement(typeof(KerberosClient).AssemblyQualifiedName));
            this.AuthenticationModules.Add(new AuthenticationModuleElement(typeof(NtlmClient).AssemblyQualifiedName));
            this.AuthenticationModules.Add(new AuthenticationModuleElement(typeof(DigestClient).AssemblyQualifiedName));
            this.AuthenticationModules.Add(new AuthenticationModuleElement(typeof(BasicClient).AssemblyQualifiedName));
        }

        protected override void PostDeserialize()
        {
            if (!base.EvaluationContext.IsMachineLevel)
            {
                try
                {
                    ExceptionHelper.UnmanagedPermission.Demand();
                }
                catch (Exception exception)
                {
                    throw new ConfigurationErrorsException(System.SR.GetString("net_config_section_permission", new object[] { "authenticationModules" }), exception);
                }
            }
        }

        [ConfigurationProperty("", IsDefaultCollection=true)]
        public AuthenticationModuleElementCollection AuthenticationModules
        {
            get
            {
                return (AuthenticationModuleElementCollection) base[this.authenticationModules];
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return this.properties;
            }
        }
    }
}

