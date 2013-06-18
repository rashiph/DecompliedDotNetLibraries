namespace System.Xaml.Hosting.Configuration
{
    using System;
    using System.Configuration;
    using System.Runtime.InteropServices;
    using System.Web.Configuration;
    using System.Xaml.Hosting;

    internal static class XamlHostingConfiguration
    {
        internal const string CollectionName = "";
        internal const string HttpHandlerType = "httpHandlerType";
        internal const string XamlHostingConfigGroup = "system.xaml.hosting";
        internal const string XamlHostingSection = "system.xaml.hosting/httpHandlers";
        internal const string XamlRootElementType = "xamlRootElementType";

        private static System.Xaml.Hosting.Configuration.XamlHostingSection LoadXamlHostingSection(string virtualPath)
        {
            return (System.Xaml.Hosting.Configuration.XamlHostingSection) WebConfigurationManager.GetSection("system.xaml.hosting/httpHandlers", virtualPath);
        }

        internal static bool TryGetHttpHandlerType(string virtualPath, Type hostedXamlType, out Type httpHandlerType)
        {
            System.Xaml.Hosting.Configuration.XamlHostingSection section = LoadXamlHostingSection(virtualPath);
            if (section == null)
            {
                ConfigurationErrorsException exception = new ConfigurationErrorsException(System.Xaml.Hosting.SR.ConfigSectionNotFound);
                throw FxTrace.Exception.AsError(exception);
            }
            return section.Handlers.TryGetHttpHandlerType(hostedXamlType, out httpHandlerType);
        }
    }
}

