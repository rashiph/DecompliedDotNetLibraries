namespace System.Net.Configuration
{
    using System;
    using System.Globalization;

    internal static class ConfigurationStrings
    {
        internal const string Address = "address";
        internal const string AlwaysUseCompletionPortsForAccept = "alwaysUseCompletionPortsForAccept";
        internal const string AlwaysUseCompletionPortsForConnect = "alwaysUseCompletionPortsForConnect";
        internal const string AuthenticationModulesSectionName = "authenticationModules";
        internal const string AutoDetect = "autoDetect";
        internal const string BypassList = "bypasslist";
        internal const string BypassOnLocal = "bypassonlocal";
        internal const string CheckCertificateName = "checkCertificateName";
        internal const string CheckCertificateRevocationList = "checkCertificateRevocationList";
        internal const string ClientDomain = "clientDomain";
        internal const string ConnectionManagementSectionName = "connectionManagement";
        internal const string DefaultCredentials = "defaultCredentials";
        internal const string DefaultFtpCachePolicy = "defaultFtpCachePolicy";
        internal const string DefaultHttpCachePolicy = "defaultHttpCachePolicy";
        internal const string DefaultPolicyLevel = "defaultPolicyLevel";
        internal const string DefaultProxySectionName = "defaultProxy";
        internal const string DeliveryMethod = "deliveryMethod";
        internal const string DisableAllCaching = "disableAllCaching";
        internal const string DnsRefreshTimeout = "dnsRefreshTimeout";
        internal const string DownloadTimeout = "downloadTimeout";
        internal const string Enabled = "enabled";
        internal const string EnableDnsRoundRobin = "enableDnsRoundRobin";
        internal const string EnableSsl = "enableSsl";
        internal const string EncryptionPolicy = "encryptionPolicy";
        internal const string Expect100Continue = "expect100Continue";
        internal const string File = "file:";
        internal const string From = "from";
        internal const string Ftp = "ftp:";
        internal const string Host = "host";
        internal const string Http = "http:";
        internal const string HttpListener = "httpListener";
        internal const string Https = "https:";
        internal const string HttpWebRequest = "httpWebRequest";
        internal const string IPProtectionLevel = "ipProtectionLevel";
        internal const string Ipv6 = "ipv6";
        internal const string IsPrivateCache = "isPrivateCache";
        internal const string MailSettingsSectionName = "mailSettings";
        internal const string MaxConnection = "maxconnection";
        internal const string MaximumAge = "maximumAge";
        internal const string MaximumErrorResponseLength = "maximumErrorResponseLength";
        internal const string MaximumResponseHeadersLength = "maximumResponseHeadersLength";
        internal const string MaximumStale = "maximumStale";
        internal const string maximumUnauthorizedUploadLength = "maximumUnauthorizedUploadLength";
        internal const string MinimumFresh = "minimumFresh";
        internal const string Module = "module";
        internal const string Name = "name";
        internal const string Network = "network";
        internal const string Password = "password";
        internal const string PerformanceCounters = "performanceCounters";
        internal const string PickupDirectoryFromIis = "pickupDirectoryFromIis";
        internal const string PickupDirectoryLocation = "pickupDirectoryLocation";
        internal const string PolicyLevel = "policyLevel";
        internal const string Port = "port";
        internal const string Prefix = "prefix";
        internal const string Proxy = "proxy";
        internal const string ProxyAddress = "proxyaddress";
        internal const string RequestCachingSectionName = "requestCaching";
        internal const string ScriptLocation = "scriptLocation";
        internal const string SectionGroupName = "system.net";
        internal const string ServicePointManager = "servicePointManager";
        internal const string SettingsSectionName = "settings";
        internal const string SmtpSectionName = "smtp";
        internal const string Socket = "socket";
        internal const string SpecifiedPickupDirectory = "specifiedPickupDirectory";
        internal const string TargetName = "targetName";
        internal const string Type = "type";
        internal const string UnescapeRequestUrl = "unescapeRequestUrl";
        internal const string UnspecifiedMaximumAge = "unspecifiedMaximumAge";
        internal const string UseDefaultCredentials = "useDefaultCredentials";
        internal const string UseNagleAlgorithm = "useNagleAlgorithm";
        internal const string UserName = "userName";
        internal const string UseSystemDefault = "usesystemdefault";
        internal const string UseUnsafeHeaderParsing = "useUnsafeHeaderParsing";
        internal const string WebProxyScript = "webProxyScript";
        internal const string WebRequestModulesSectionName = "webRequestModules";

        private static string GetSectionPath(string sectionName)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}/{1}", new object[] { "system.net", sectionName });
        }

        private static string GetSectionPath(string sectionName, string subSectionName)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}/{1}/{2}", new object[] { "system.net", sectionName, subSectionName });
        }

        internal static string AuthenticationModulesSectionPath
        {
            get
            {
                return GetSectionPath("authenticationModules");
            }
        }

        internal static string ConnectionManagementSectionPath
        {
            get
            {
                return GetSectionPath("connectionManagement");
            }
        }

        internal static string DefaultProxySectionPath
        {
            get
            {
                return GetSectionPath("defaultProxy");
            }
        }

        internal static string RequestCachingSectionPath
        {
            get
            {
                return GetSectionPath("requestCaching");
            }
        }

        internal static string SettingsSectionPath
        {
            get
            {
                return GetSectionPath("settings");
            }
        }

        internal static string SmtpSectionPath
        {
            get
            {
                return GetSectionPath("mailSettings", "smtp");
            }
        }

        internal static string WebRequestModulesSectionPath
        {
            get
            {
                return GetSectionPath("webRequestModules");
            }
        }
    }
}

