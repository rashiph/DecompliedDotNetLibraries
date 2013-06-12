namespace System.Web.Util
{
    using System;
    using System.Collections.Specialized;
    using System.Runtime.CompilerServices;
    using System.Web;

    internal static class AppSettings
    {
        private static bool _allowRelaxedHttpUserName;
        private static bool _allowRelaxedRelativeUrl;
        private static object _appSettingsLock = new object();
        private static int _maxHttpCollectionKeys = 0x3e8;
        private static int _maxJsonDeserializerMembers = 0x3e8;
        private static bool _restrictXmlControls;
        private static bool _scriptResourceAllowNonJsFiles;
        private static volatile bool _settingsInitialized = false;
        private static bool _useHostHeaderForRequestUrl;
        private static bool _useLegacyEncryption;
        private static bool _useLegacyFormsAuthenticationTicketCompatibility;
        private static bool _useLegacyMachineKeyEncryption;
        private const int DefaultMaxHttpCollectionKeys = 0x3e8;
        private const int DefaultMaxJsonDeserializerMembers = 0x3e8;

        private static void EnsureSettingsLoaded()
        {
            if (!_settingsInitialized)
            {
                lock (_appSettingsLock)
                {
                    if (!_settingsInitialized)
                    {
                        NameValueCollection section = null;
                        try
                        {
                            CachedPathData applicationPathData = CachedPathData.GetApplicationPathData();
                            if ((applicationPathData != null) && (applicationPathData.ConfigRecord != null))
                            {
                                section = applicationPathData.ConfigRecord.GetSection("appSettings") as NameValueCollection;
                            }
                        }
                        finally
                        {
                            if ((section == null) || !bool.TryParse(section["aspnet:UseHostHeaderForRequestUrl"], out _useHostHeaderForRequestUrl))
                            {
                                _useHostHeaderForRequestUrl = false;
                            }
                            if ((section == null) || !bool.TryParse(section["aspnet:ScriptResourceAllowNonJsFiles"], out _scriptResourceAllowNonJsFiles))
                            {
                                _scriptResourceAllowNonJsFiles = false;
                            }
                            if ((section == null) || !bool.TryParse(section["aspnet:UseLegacyEncryption"], out _useLegacyEncryption))
                            {
                                _useLegacyEncryption = false;
                            }
                            if ((section == null) || !bool.TryParse(section["aspnet:UseLegacyMachineKeyEncryption"], out _useLegacyMachineKeyEncryption))
                            {
                                _useLegacyMachineKeyEncryption = false;
                            }
                            if ((section == null) || !bool.TryParse(section["aspnet:AllowRelaxedRelativeUrl"], out _allowRelaxedRelativeUrl))
                            {
                                _allowRelaxedRelativeUrl = false;
                            }
                            if ((section == null) || !bool.TryParse(section["aspnet:RestrictXmlControls"], out _restrictXmlControls))
                            {
                                _restrictXmlControls = false;
                            }
                            if ((section == null) || !bool.TryParse(section["aspnet:UseLegacyFormsAuthenticationTicketCompatibility"], out _useLegacyFormsAuthenticationTicketCompatibility))
                            {
                                _useLegacyFormsAuthenticationTicketCompatibility = false;
                            }
                            if ((section == null) || !bool.TryParse(section["aspnet:AllowRelaxedHttpUserName"], out _allowRelaxedHttpUserName))
                            {
                                _allowRelaxedHttpUserName = false;
                            }
                            if (((section == null) || !int.TryParse(section["aspnet:MaxHttpCollectionKeys"], out _maxHttpCollectionKeys)) || (_maxHttpCollectionKeys < 0))
                            {
                                _maxHttpCollectionKeys = 0x3e8;
                            }
                            if (((section == null) || !int.TryParse(section["aspnet:MaxJsonDeserializerMembers"], out _maxJsonDeserializerMembers)) || (_maxJsonDeserializerMembers < 0))
                            {
                                _maxJsonDeserializerMembers = 0x3e8;
                            }
                            _settingsInitialized = true;
                        }
                    }
                }
            }
        }

        internal static bool AllowRelaxedHttpUserName
        {
            get
            {
                EnsureSettingsLoaded();
                return _allowRelaxedHttpUserName;
            }
        }

        internal static bool AllowRelaxedRelativeUrl
        {
            get
            {
                EnsureSettingsLoaded();
                return _allowRelaxedRelativeUrl;
            }
        }

        internal static int MaxHttpCollectionKeys
        {
            get
            {
                EnsureSettingsLoaded();
                return _maxHttpCollectionKeys;
            }
        }

        internal static int MaxJsonDeserializerMembers
        {
            get
            {
                EnsureSettingsLoaded();
                return _maxJsonDeserializerMembers;
            }
        }

        internal static bool RestrictXmlControls
        {
            get
            {
                EnsureSettingsLoaded();
                return _restrictXmlControls;
            }
        }

        internal static bool ScriptResourceAllowNonJsFiles
        {
            get
            {
                EnsureSettingsLoaded();
                return _scriptResourceAllowNonJsFiles;
            }
        }

        internal static bool UseHostHeaderForRequestUrl
        {
            get
            {
                EnsureSettingsLoaded();
                return _useHostHeaderForRequestUrl;
            }
        }

        internal static bool UseLegacyEncryption
        {
            get
            {
                EnsureSettingsLoaded();
                return _useLegacyEncryption;
            }
        }

        internal static bool UseLegacyFormsAuthenticationTicketCompatibility
        {
            get
            {
                EnsureSettingsLoaded();
                return _useLegacyFormsAuthenticationTicketCompatibility;
            }
        }

        internal static bool UseLegacyMachineKeyEncryption
        {
            get
            {
                EnsureSettingsLoaded();
                return _useLegacyMachineKeyEncryption;
            }
        }
    }
}

