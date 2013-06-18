namespace System.Configuration
{
    using System;
    using System.Collections.Specialized;
    using System.Configuration.Internal;
    using System.Runtime;

    public static class ConfigurationManager
    {
        private static IInternalConfigSystem s_configSystem;
        private static Exception s_initError;
        private static object s_initLock = new object();
        private static InitState s_initState = InitState.NotStarted;

        internal static void CompleteConfigInit()
        {
            lock (s_initLock)
            {
                s_initState = InitState.Completed;
            }
        }

        private static void EnsureConfigurationSystem()
        {
            lock (s_initLock)
            {
                if (s_initState < InitState.Usable)
                {
                    s_initState = InitState.Started;
                    try
                    {
                        try
                        {
                            s_configSystem = new ClientConfigurationSystem();
                            s_initState = InitState.Usable;
                        }
                        catch (Exception exception)
                        {
                            s_initError = new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_client_config_init_error"), exception);
                            throw s_initError;
                        }
                    }
                    catch
                    {
                        s_initState = InitState.Completed;
                        throw;
                    }
                }
            }
        }

        public static object GetSection(string sectionName)
        {
            if (string.IsNullOrEmpty(sectionName))
            {
                return null;
            }
            PrepareConfigSystem();
            return s_configSystem.GetSection(sectionName);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static System.Configuration.Configuration OpenExeConfiguration(ConfigurationUserLevel userLevel)
        {
            return OpenExeConfigurationImpl(null, false, userLevel, null);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static System.Configuration.Configuration OpenExeConfiguration(string exePath)
        {
            return OpenExeConfigurationImpl(null, false, ConfigurationUserLevel.None, exePath);
        }

        private static System.Configuration.Configuration OpenExeConfigurationImpl(ConfigurationFileMap fileMap, bool isMachine, ConfigurationUserLevel userLevel, string exePath)
        {
            if ((!isMachine && (((fileMap == null) && (exePath == null)) || ((fileMap != null) && (((ExeConfigurationFileMap) fileMap).ExeConfigFilename == null)))) && ((s_configSystem != null) && (s_configSystem.GetType() != typeof(ClientConfigurationSystem))))
            {
                throw new ArgumentException(System.Configuration.SR.GetString("Config_configmanager_open_noexe"));
            }
            return ClientConfigurationHost.OpenExeConfiguration(fileMap, isMachine, userLevel, exePath);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static System.Configuration.Configuration OpenMachineConfiguration()
        {
            return OpenExeConfigurationImpl(null, true, ConfigurationUserLevel.None, null);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static System.Configuration.Configuration OpenMappedExeConfiguration(ExeConfigurationFileMap fileMap, ConfigurationUserLevel userLevel)
        {
            return OpenExeConfigurationImpl(fileMap, false, userLevel, null);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static System.Configuration.Configuration OpenMappedMachineConfiguration(ConfigurationFileMap fileMap)
        {
            return OpenExeConfigurationImpl(fileMap, true, ConfigurationUserLevel.None, null);
        }

        private static void PrepareConfigSystem()
        {
            if (s_initState < InitState.Usable)
            {
                EnsureConfigurationSystem();
            }
            if (s_initError != null)
            {
                throw s_initError;
            }
        }

        public static void RefreshSection(string sectionName)
        {
            if (!string.IsNullOrEmpty(sectionName))
            {
                PrepareConfigSystem();
                s_configSystem.RefreshConfig(sectionName);
            }
        }

        internal static void SetConfigurationSystem(IInternalConfigSystem configSystem, bool initComplete)
        {
            lock (s_initLock)
            {
                if (s_initState != InitState.NotStarted)
                {
                    throw new InvalidOperationException(System.Configuration.SR.GetString("Config_system_already_set"));
                }
                s_configSystem = configSystem;
                if (initComplete)
                {
                    s_initState = InitState.Completed;
                }
                else
                {
                    s_initState = InitState.Usable;
                }
            }
        }

        internal static void SetInitError(Exception initError)
        {
            s_initError = initError;
        }

        public static NameValueCollection AppSettings
        {
            get
            {
                object section = GetSection("appSettings");
                if ((section == null) || !(section is NameValueCollection))
                {
                    throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_appsettings_declaration_invalid"));
                }
                return (NameValueCollection) section;
            }
        }

        public static ConnectionStringSettingsCollection ConnectionStrings
        {
            get
            {
                object obj2 = GetSection("connectionStrings");
                if ((obj2 == null) || (obj2.GetType() != typeof(ConnectionStringsSection)))
                {
                    throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_connectionstrings_declaration_invalid"));
                }
                ConnectionStringsSection section = (ConnectionStringsSection) obj2;
                return section.ConnectionStrings;
            }
        }

        internal static bool SetConfigurationSystemInProgress
        {
            get
            {
                return ((InitState.NotStarted < s_initState) && (s_initState < InitState.Completed));
            }
        }

        internal static bool SupportsUserConfig
        {
            get
            {
                PrepareConfigSystem();
                return s_configSystem.SupportsUserConfig;
            }
        }

        private enum InitState
        {
            NotStarted,
            Started,
            Usable,
            Completed
        }
    }
}

