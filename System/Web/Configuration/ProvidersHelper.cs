namespace System.Web.Configuration
{
    using System;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Configuration.Provider;
    using System.Security.Permissions;
    using System.Web;

    public static class ProvidersHelper
    {
        [AspNetHostingPermission(SecurityAction.Demand, Level=AspNetHostingPermissionLevel.Low)]
        public static ProviderBase InstantiateProvider(ProviderSettings providerSettings, Type providerType)
        {
            ProviderBase base2 = null;
            try
            {
                string str = (providerSettings.Type == null) ? null : providerSettings.Type.Trim();
                if (string.IsNullOrEmpty(str))
                {
                    throw new ArgumentException(System.Web.SR.GetString("Provider_no_type_name"));
                }
                Type c = ConfigUtil.GetType(str, "type", providerSettings, true, true);
                if (!providerType.IsAssignableFrom(c))
                {
                    throw new ArgumentException(System.Web.SR.GetString("Provider_must_implement_type", new object[] { providerType.ToString() }));
                }
                base2 = (ProviderBase) HttpRuntime.CreatePublicInstance(c);
                NameValueCollection parameters = providerSettings.Parameters;
                NameValueCollection config = new NameValueCollection(parameters.Count, StringComparer.Ordinal);
                foreach (string str2 in parameters)
                {
                    config[str2] = parameters[str2];
                }
                base2.Initialize(providerSettings.Name, config);
            }
            catch (Exception exception)
            {
                if (exception is ConfigurationException)
                {
                    throw;
                }
                throw new ConfigurationErrorsException(exception.Message, exception, providerSettings.ElementInformation.Properties["type"].Source, providerSettings.ElementInformation.Properties["type"].LineNumber);
            }
            return base2;
        }

        [AspNetHostingPermission(SecurityAction.Demand, Level=AspNetHostingPermissionLevel.Low)]
        public static void InstantiateProviders(ProviderSettingsCollection configProviders, ProviderCollection providers, Type providerType)
        {
            foreach (ProviderSettings settings in configProviders)
            {
                providers.Add(InstantiateProvider(settings, providerType));
            }
        }
    }
}

