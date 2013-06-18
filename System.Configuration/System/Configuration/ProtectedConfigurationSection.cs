namespace System.Configuration
{
    using System;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Runtime;
    using System.Security.Permissions;
    using System.Xml;

    public sealed class ProtectedConfigurationSection : ConfigurationSection
    {
        private static readonly ConfigurationProperty _propDefaultProvider = new ConfigurationProperty("defaultProvider", typeof(string), "RsaProtectedConfigurationProvider", null, ConfigurationProperty.NonEmptyStringValidator, ConfigurationPropertyOptions.None);
        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();
        private static readonly ConfigurationProperty _propProviders = new ConfigurationProperty("providers", typeof(ProtectedProviderSettings), new ProtectedProviderSettings(), ConfigurationPropertyOptions.None);
        private const string EncryptedSectionTemplate = "<{0} {1}=\"{2}\"> {3} </{0}>";

        static ProtectedConfigurationSection()
        {
            _properties.Add(_propProviders);
            _properties.Add(_propDefaultProvider);
        }

        [PermissionSet(SecurityAction.Assert, Unrestricted=true)]
        private ProtectedConfigurationProvider CreateAndInitializeProviderWithAssert(Type t, ProviderSettings pn)
        {
            ProtectedConfigurationProvider provider = (ProtectedConfigurationProvider) System.Configuration.TypeUtil.CreateInstanceWithReflectionPermission(t);
            NameValueCollection parameters = pn.Parameters;
            NameValueCollection config = new NameValueCollection(parameters.Count);
            foreach (string str in parameters)
            {
                config[str] = parameters[str];
            }
            provider.Initialize(pn.Name, config);
            return provider;
        }

        internal static string DecryptSection(string encryptedXml, ProtectedConfigurationProvider provider)
        {
            XmlDocument document = new XmlDocument();
            document.LoadXml(encryptedXml);
            return provider.Decrypt(document.DocumentElement).OuterXml;
        }

        internal static string EncryptSection(string clearXml, ProtectedConfigurationProvider provider)
        {
            XmlDocument document = new XmlDocument {
                PreserveWhitespace = true
            };
            document.LoadXml(clearXml);
            string name = document.DocumentElement.Name;
            return provider.Encrypt(document.DocumentElement).OuterXml;
        }

        internal static string FormatEncryptedSection(string encryptedXml, string sectionName, string providerName)
        {
            return string.Format(CultureInfo.InvariantCulture, "<{0} {1}=\"{2}\"> {3} </{0}>", new object[] { sectionName, "configProtectionProvider", providerName, encryptedXml });
        }

        internal ProtectedConfigurationProviderCollection GetAllProviders()
        {
            ProtectedConfigurationProviderCollection providers = new ProtectedConfigurationProviderCollection();
            foreach (ProviderSettings settings in this.Providers)
            {
                providers.Add(this.InstantiateProvider(settings));
            }
            return providers;
        }

        internal ProtectedConfigurationProvider GetProviderFromName(string providerName)
        {
            ProviderSettings pn = this.Providers[providerName];
            if (pn == null)
            {
                throw new Exception(System.Configuration.SR.GetString("ProtectedConfigurationProvider_not_found", new object[] { providerName }));
            }
            return this.InstantiateProvider(pn);
        }

        private ProtectedConfigurationProvider InstantiateProvider(ProviderSettings pn)
        {
            Type typeWithReflectionPermission = System.Configuration.TypeUtil.GetTypeWithReflectionPermission(pn.Type, true);
            if (!typeof(ProtectedConfigurationProvider).IsAssignableFrom(typeWithReflectionPermission))
            {
                throw new Exception(System.Configuration.SR.GetString("WrongType_of_Protected_provider"));
            }
            if (!System.Configuration.TypeUtil.IsTypeAllowedInConfig(typeWithReflectionPermission))
            {
                throw new Exception(System.Configuration.SR.GetString("Type_from_untrusted_assembly", new object[] { typeWithReflectionPermission.FullName }));
            }
            return this.CreateAndInitializeProviderWithAssert(typeWithReflectionPermission, pn);
        }

        private ProtectedProviderSettings _Providers
        {
            get
            {
                return (ProtectedProviderSettings) base[_propProviders];
            }
        }

        [ConfigurationProperty("defaultProvider", DefaultValue="RsaProtectedConfigurationProvider")]
        public string DefaultProvider
        {
            get
            {
                return (string) base[_propDefaultProvider];
            }
            set
            {
                base[_propDefaultProvider] = value;
            }
        }

        protected internal override ConfigurationPropertyCollection Properties
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return _properties;
            }
        }

        [ConfigurationProperty("providers")]
        public ProviderSettingsCollection Providers
        {
            get
            {
                return this._Providers.Providers;
            }
        }
    }
}

