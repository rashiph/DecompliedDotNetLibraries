namespace System.Web.Profile
{
    using System;
    using System.Configuration;
    using System.Configuration.Provider;
    using System.Runtime.InteropServices;
    using System.Web;
    using System.Web.Compilation;
    using System.Web.Configuration;
    using System.Web.Hosting;
    using System.Web.Util;

    public static class ProfileManager
    {
        private static bool s_AutomaticSaveEnabled;
        private static ProfilePropertySettingsCollection s_dynamicProperties = new ProfilePropertySettingsCollection();
        private static bool s_Enabled;
        private static Exception s_InitException;
        private static bool s_Initialized;
        private static bool s_InitializeDefaultProvider;
        private static bool s_InitializedEnabled;
        private static bool s_InitializedProviders;
        private static object s_Lock = new object();
        private static ProfileProvider s_Provider;
        private static ProfileProviderCollection s_Providers;

        public static void AddDynamicProfileProperty(ProfilePropertySettings property)
        {
            BuildManager.ThrowIfPreAppStartNotRunning();
            s_dynamicProperties.Add(property);
        }

        public static int DeleteInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate)
        {
            return Provider.DeleteInactiveProfiles(authenticationOption, userInactiveSinceDate);
        }

        public static bool DeleteProfile(string username)
        {
            SecUtility.CheckParameter(ref username, true, true, true, 0, "username");
            return (Provider.DeleteProfiles(new string[] { username }) != 0);
        }

        public static int DeleteProfiles(ProfileInfoCollection profiles)
        {
            if (profiles == null)
            {
                throw new ArgumentNullException("profiles");
            }
            if (profiles.Count < 1)
            {
                throw new ArgumentException(System.Web.SR.GetString("Parameter_collection_empty", new object[] { "profiles" }), "profiles");
            }
            foreach (ProfileInfo info in profiles)
            {
                SecUtility.CheckParameter(ref info.UserName, true, true, true, 0, "UserName");
            }
            return Provider.DeleteProfiles(profiles);
        }

        public static int DeleteProfiles(string[] usernames)
        {
            SecUtility.CheckArrayParameter(ref usernames, true, true, true, 0, "usernames");
            return Provider.DeleteProfiles(usernames);
        }

        public static ProfileInfoCollection FindInactiveProfilesByUserName(ProfileAuthenticationOption authenticationOption, string usernameToMatch, DateTime userInactiveSinceDate)
        {
            int num;
            SecUtility.CheckParameter(ref usernameToMatch, true, true, false, 0, "usernameToMatch");
            return Provider.FindInactiveProfilesByUserName(authenticationOption, usernameToMatch, userInactiveSinceDate, 0, 0x7fffffff, out num);
        }

        public static ProfileInfoCollection FindInactiveProfilesByUserName(ProfileAuthenticationOption authenticationOption, string usernameToMatch, DateTime userInactiveSinceDate, int pageIndex, int pageSize, out int totalRecords)
        {
            if (pageIndex < 0)
            {
                throw new ArgumentException(System.Web.SR.GetString("PageIndex_bad"), "pageIndex");
            }
            if (pageSize < 1)
            {
                throw new ArgumentException(System.Web.SR.GetString("PageSize_bad"), "pageSize");
            }
            SecUtility.CheckParameter(ref usernameToMatch, true, true, false, 0, "usernameToMatch");
            return Provider.FindInactiveProfilesByUserName(authenticationOption, usernameToMatch, userInactiveSinceDate, pageIndex, pageSize, out totalRecords);
        }

        public static ProfileInfoCollection FindProfilesByUserName(ProfileAuthenticationOption authenticationOption, string usernameToMatch)
        {
            int num;
            SecUtility.CheckParameter(ref usernameToMatch, true, true, false, 0, "usernameToMatch");
            return Provider.FindProfilesByUserName(authenticationOption, usernameToMatch, 0, 0x7fffffff, out num);
        }

        public static ProfileInfoCollection FindProfilesByUserName(ProfileAuthenticationOption authenticationOption, string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            if (pageIndex < 0)
            {
                throw new ArgumentException(System.Web.SR.GetString("PageIndex_bad"), "pageIndex");
            }
            if (pageSize < 1)
            {
                throw new ArgumentException(System.Web.SR.GetString("PageSize_bad"), "pageSize");
            }
            SecUtility.CheckParameter(ref usernameToMatch, true, true, false, 0, "usernameToMatch");
            return Provider.FindProfilesByUserName(authenticationOption, usernameToMatch, pageIndex, pageSize, out totalRecords);
        }

        public static ProfileInfoCollection GetAllInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate)
        {
            int num;
            return Provider.GetAllInactiveProfiles(authenticationOption, userInactiveSinceDate, 0, 0x7fffffff, out num);
        }

        public static ProfileInfoCollection GetAllInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate, int pageIndex, int pageSize, out int totalRecords)
        {
            return Provider.GetAllInactiveProfiles(authenticationOption, userInactiveSinceDate, pageIndex, pageSize, out totalRecords);
        }

        public static ProfileInfoCollection GetAllProfiles(ProfileAuthenticationOption authenticationOption)
        {
            int num;
            return Provider.GetAllProfiles(authenticationOption, 0, 0x7fffffff, out num);
        }

        public static ProfileInfoCollection GetAllProfiles(ProfileAuthenticationOption authenticationOption, int pageIndex, int pageSize, out int totalRecords)
        {
            return Provider.GetAllProfiles(authenticationOption, pageIndex, pageSize, out totalRecords);
        }

        public static int GetNumberOfInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate)
        {
            return Provider.GetNumberOfInactiveProfiles(authenticationOption, userInactiveSinceDate);
        }

        public static int GetNumberOfProfiles(ProfileAuthenticationOption authenticationOption)
        {
            return Provider.GetNumberOfInactiveProfiles(authenticationOption, DateTime.Now.AddDays(1.0));
        }

        private static void Initialize(bool throwIfNotEnabled)
        {
            InitializeEnabled(true);
            if (s_InitException != null)
            {
                throw s_InitException;
            }
            if (throwIfNotEnabled && !s_Enabled)
            {
                throw new ProviderException(System.Web.SR.GetString("Profile_not_enabled"));
            }
        }

        private static void InitializeEnabled(bool initProviders)
        {
            if ((!s_Initialized || !s_InitializedProviders) || !s_InitializeDefaultProvider)
            {
                lock (s_Lock)
                {
                    if ((!s_Initialized || !s_InitializedProviders) || !s_InitializeDefaultProvider)
                    {
                        try
                        {
                            ProfileSection profileAppConfig = MTConfigUtil.GetProfileAppConfig();
                            if (!s_InitializedEnabled)
                            {
                                s_Enabled = profileAppConfig.Enabled && HttpRuntime.HasAspNetHostingPermission(AspNetHostingPermissionLevel.Low);
                                s_AutomaticSaveEnabled = s_Enabled && profileAppConfig.AutomaticSaveEnabled;
                                s_InitializedEnabled = true;
                            }
                            if ((initProviders && s_Enabled) && (!s_InitializedProviders || !s_InitializeDefaultProvider))
                            {
                                InitProviders(profileAppConfig);
                            }
                        }
                        catch (Exception exception)
                        {
                            s_InitException = exception;
                        }
                        s_Initialized = true;
                    }
                }
            }
        }

        private static void InitProviders(ProfileSection config)
        {
            if (!s_InitializedProviders)
            {
                s_Providers = new ProfileProviderCollection();
                if (config.Providers != null)
                {
                    ProvidersHelper.InstantiateProviders(config.Providers, s_Providers, typeof(ProfileProvider));
                }
                s_InitializedProviders = true;
            }
            bool flag = !HostingEnvironment.IsHosted || (BuildManager.PreStartInitStage == PreStartInitStage.AfterPreStartInit);
            if (!s_InitializeDefaultProvider && flag)
            {
                s_Providers.SetReadOnly();
                if (config.DefaultProvider == null)
                {
                    throw new ProviderException(System.Web.SR.GetString("Profile_default_provider_not_specified"));
                }
                s_Provider = s_Providers[config.DefaultProvider];
                if (s_Provider == null)
                {
                    throw new ConfigurationErrorsException(System.Web.SR.GetString("Profile_default_provider_not_found"), config.ElementInformation.Properties["providers"].Source, config.ElementInformation.Properties["providers"].LineNumber);
                }
                s_InitializeDefaultProvider = true;
            }
        }

        public static string ApplicationName
        {
            get
            {
                return Provider.ApplicationName;
            }
            set
            {
                Provider.ApplicationName = value;
            }
        }

        public static bool AutomaticSaveEnabled
        {
            get
            {
                HttpRuntime.CheckAspNetHostingPermission(AspNetHostingPermissionLevel.Low, "Feature_not_supported_at_this_level");
                InitializeEnabled(false);
                return s_AutomaticSaveEnabled;
            }
        }

        internal static ProfilePropertySettingsCollection DynamicProfileProperties
        {
            get
            {
                return s_dynamicProperties;
            }
        }

        public static bool Enabled
        {
            get
            {
                if (!s_Initialized && !s_InitializedEnabled)
                {
                    InitializeEnabled(false);
                }
                return s_Enabled;
            }
        }

        public static ProfileProvider Provider
        {
            get
            {
                HttpRuntime.CheckAspNetHostingPermission(AspNetHostingPermissionLevel.Low, "Feature_not_supported_at_this_level");
                Initialize(true);
                if (s_Provider == null)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("Profile_default_provider_not_found"));
                }
                return s_Provider;
            }
        }

        public static ProfileProviderCollection Providers
        {
            get
            {
                HttpRuntime.CheckAspNetHostingPermission(AspNetHostingPermissionLevel.Low, "Feature_not_supported_at_this_level");
                Initialize(true);
                return s_Providers;
            }
        }
    }
}

