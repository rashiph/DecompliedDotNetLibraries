namespace System.Configuration
{
    using System;
    using System.Configuration.Internal;
    using System.IO;
    using System.Net;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Security.Policy;
    using System.Security.Principal;

    internal sealed class ClientConfigurationHost : DelegatingConfigHost, IInternalConfigClientHost
    {
        private ClientConfigPaths _configPaths;
        private string _exePath;
        private ExeConfigurationFileMap _fileMap;
        private bool _initComplete;
        private const string ConfigExtension = ".config";
        internal const string ExeConfigName = "EXE";
        internal const string ExeConfigPath = "MACHINE/EXE";
        internal const string LocalUserConfigName = "LOCAL_USER";
        internal const string LocalUserConfigPath = "MACHINE/EXE/ROAMING_USER/LOCAL_USER";
        private const string MachineConfigFilename = "machine.config";
        internal const string MachineConfigName = "MACHINE";
        internal const string MachineConfigPath = "MACHINE";
        private const string MachineConfigSubdirectory = "Config";
        internal const string RoamingUserConfigName = "ROAMING_USER";
        internal const string RoamingUserConfigPath = "MACHINE/EXE/ROAMING_USER";
        private static object s_init = new object();
        private static string s_machineConfigFilePath;
        private static object s_version = new object();

        internal ClientConfigurationHost()
        {
            base.Host = new InternalConfigHost();
        }

        public override object CreateConfigurationContext(string configPath, string locationSubPath)
        {
            return new ExeContext(this.GetUserLevel(configPath), this.ConfigPaths.ApplicationUri);
        }

        public override object CreateDeprecatedConfigContext(string configPath)
        {
            return null;
        }

        public override void DeleteStream(string streamName)
        {
            if (!this.IsFile(streamName))
            {
                throw ExceptionUtil.UnexpectedError("ClientConfigurationHost::Delete");
            }
            base.Host.DeleteStream(streamName);
        }

        [SecurityPermission(SecurityAction.Assert, ControlEvidence=true)]
        public override void GetRestrictedPermissions(IInternalConfigRecord configRecord, out PermissionSet permissionSet, out bool isHostReady)
        {
            string streamName;
            bool flag = this.IsFile(configRecord.StreamName);
            if (flag)
            {
                streamName = UrlPath.ConvertFileNameToUrl(configRecord.StreamName);
            }
            else
            {
                streamName = configRecord.StreamName;
            }
            Evidence evidence = new Evidence();
            evidence.AddHostEvidence<Url>(new Url(streamName));
            evidence.AddHostEvidence<Zone>(Zone.CreateFromUrl(streamName));
            if (!flag)
            {
                evidence.AddHostEvidence<Site>(Site.CreateFromUrl(streamName));
            }
            permissionSet = SecurityManager.GetStandardSandbox(evidence);
            isHostReady = true;
        }

        public override string GetStreamName(string configPath)
        {
            string str3;
            string name = ConfigPathUtility.GetName(configPath);
            if (this._fileMap != null)
            {
                string str2;
                if (((str2 = name) != null) && !(str2 == "MACHINE"))
                {
                    if (str2 == "EXE")
                    {
                        return this._fileMap.ExeConfigFilename;
                    }
                    if (str2 == "ROAMING_USER")
                    {
                        return this._fileMap.RoamingUserConfigFilename;
                    }
                    if (str2 == "LOCAL_USER")
                    {
                        return this._fileMap.LocalUserConfigFilename;
                    }
                }
                return this._fileMap.MachineConfigFilename;
            }
            if (((str3 = name) != null) && !(str3 == "MACHINE"))
            {
                if (str3 == "EXE")
                {
                    return this.ConfigPaths.ApplicationConfigUri;
                }
                if (str3 == "ROAMING_USER")
                {
                    return this.ConfigPaths.RoamingConfigFilename;
                }
                if (str3 == "LOCAL_USER")
                {
                    return this.ConfigPaths.LocalConfigFilename;
                }
            }
            return MachineConfigFilePath;
        }

        public override string GetStreamNameForConfigSource(string streamName, string configSource)
        {
            if (this.IsFile(streamName))
            {
                return base.Host.GetStreamNameForConfigSource(streamName, configSource);
            }
            int num = streamName.LastIndexOf('/');
            if (num < 0)
            {
                return null;
            }
            return (streamName.Substring(0, num + 1) + configSource.Replace('\\', '/'));
        }

        public override object GetStreamVersion(string streamName)
        {
            if (this.IsFile(streamName))
            {
                return base.Host.GetStreamVersion(streamName);
            }
            return s_version;
        }

        private ConfigurationUserLevel GetUserLevel(string configPath)
        {
            switch (ConfigPathUtility.GetName(configPath))
            {
                case "MACHINE":
                    return ConfigurationUserLevel.None;

                case "EXE":
                    return ConfigurationUserLevel.None;

                case "LOCAL_USER":
                    return ConfigurationUserLevel.PerUserRoamingAndLocal;

                case "ROAMING_USER":
                    return ConfigurationUserLevel.PerUserRoaming;
            }
            return ConfigurationUserLevel.None;
        }

        [SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.ControlPrincipal | SecurityPermissionFlag.UnmanagedCode)]
        public override IDisposable Impersonate()
        {
            return WindowsIdentity.Impersonate(IntPtr.Zero);
        }

        public override void Init(IInternalConfigRoot configRoot, params object[] hostInitParams)
        {
            try
            {
                ConfigurationFileMap map = (ConfigurationFileMap) hostInitParams[0];
                this._exePath = (string) hostInitParams[1];
                base.Host.Init(configRoot, hostInitParams);
                this._initComplete = configRoot.IsDesignTime;
                if ((map != null) && !string.IsNullOrEmpty(this._exePath))
                {
                    throw ExceptionUtil.UnexpectedError("ClientConfigurationHost::Init");
                }
                if (string.IsNullOrEmpty(this._exePath))
                {
                    this._exePath = null;
                }
                if (map != null)
                {
                    this._fileMap = new ExeConfigurationFileMap();
                    if (!string.IsNullOrEmpty(map.MachineConfigFilename))
                    {
                        this._fileMap.MachineConfigFilename = Path.GetFullPath(map.MachineConfigFilename);
                    }
                    ExeConfigurationFileMap map2 = map as ExeConfigurationFileMap;
                    if (map2 != null)
                    {
                        if (!string.IsNullOrEmpty(map2.ExeConfigFilename))
                        {
                            this._fileMap.ExeConfigFilename = Path.GetFullPath(map2.ExeConfigFilename);
                        }
                        if (!string.IsNullOrEmpty(map2.RoamingUserConfigFilename))
                        {
                            this._fileMap.RoamingUserConfigFilename = Path.GetFullPath(map2.RoamingUserConfigFilename);
                        }
                        if (!string.IsNullOrEmpty(map2.LocalUserConfigFilename))
                        {
                            this._fileMap.LocalUserConfigFilename = Path.GetFullPath(map2.LocalUserConfigFilename);
                        }
                    }
                }
            }
            catch (SecurityException)
            {
                throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_client_config_init_security"));
            }
            catch
            {
                throw ExceptionUtil.UnexpectedError("ClientConfigurationHost::Init");
            }
        }

        public override void InitForConfiguration(ref string locationSubPath, out string configPath, out string locationConfigPath, IInternalConfigRoot configRoot, params object[] hostInitConfigurationParams)
        {
            locationSubPath = null;
            configPath = (string) hostInitConfigurationParams[2];
            locationConfigPath = null;
            this.Init(configRoot, hostInitConfigurationParams);
        }

        public override bool IsConfigRecordRequired(string configPath)
        {
            switch (ConfigPathUtility.GetName(configPath))
            {
                case "MACHINE":
                case "EXE":
                    return true;

                case "ROAMING_USER":
                    if (!this.HasRoamingConfig)
                    {
                        return this.HasLocalConfig;
                    }
                    return true;

                case "LOCAL_USER":
                    return this.HasLocalConfig;
            }
            return false;
        }

        public override bool IsDefinitionAllowed(string configPath, ConfigurationAllowDefinition allowDefinition, ConfigurationAllowExeDefinition allowExeDefinition)
        {
            string str;
            switch (allowExeDefinition)
            {
                case ConfigurationAllowExeDefinition.MachineToRoamingUser:
                    str = "MACHINE/EXE/ROAMING_USER";
                    break;

                case ConfigurationAllowExeDefinition.MachineToLocalUser:
                    return true;

                case ConfigurationAllowExeDefinition.MachineOnly:
                    str = "MACHINE";
                    break;

                case ConfigurationAllowExeDefinition.MachineToApplication:
                    str = "MACHINE/EXE";
                    break;

                default:
                    throw ExceptionUtil.UnexpectedError("ClientConfigurationHost::IsDefinitionAllowed");
            }
            return (configPath.Length <= str.Length);
        }

        public override bool IsInitDelayed(IInternalConfigRecord configRecord)
        {
            return (!this._initComplete && this.IsUserConfig(configRecord.ConfigPath));
        }

        public override bool IsTrustedConfigPath(string configPath)
        {
            return (configPath == "MACHINE");
        }

        private bool IsUserConfig(string configPath)
        {
            if (!StringUtil.EqualsIgnoreCase(configPath, "MACHINE/EXE/ROAMING_USER"))
            {
                return StringUtil.EqualsIgnoreCase(configPath, "MACHINE/EXE/ROAMING_USER/LOCAL_USER");
            }
            return true;
        }

        internal static System.Configuration.Configuration OpenExeConfiguration(ConfigurationFileMap fileMap, bool isMachine, ConfigurationUserLevel userLevel, string exePath)
        {
            ExeConfigurationFileMap map;
            string str;
            ConfigurationUserLevel level = userLevel;
            if (((level != ConfigurationUserLevel.None) && (level != ConfigurationUserLevel.PerUserRoaming)) && (level != ConfigurationUserLevel.PerUserRoamingAndLocal))
            {
                throw ExceptionUtil.ParameterInvalid("userLevel");
            }
            if (fileMap != null)
            {
                if (string.IsNullOrEmpty(fileMap.MachineConfigFilename))
                {
                    throw ExceptionUtil.ParameterNullOrEmpty("fileMap.MachineConfigFilename");
                }
                map = fileMap as ExeConfigurationFileMap;
                if (map != null)
                {
                    switch (userLevel)
                    {
                        case ConfigurationUserLevel.None:
                            goto Label_0059;

                        case ConfigurationUserLevel.PerUserRoaming:
                            goto Label_0071;

                        case ConfigurationUserLevel.PerUserRoamingAndLocal:
                            if (string.IsNullOrEmpty(map.LocalUserConfigFilename))
                            {
                                throw ExceptionUtil.ParameterNullOrEmpty("fileMap.LocalUserConfigFilename");
                            }
                            goto Label_0071;
                    }
                }
            }
            goto Label_00A1;
        Label_0059:
            if (!string.IsNullOrEmpty(map.ExeConfigFilename))
            {
                goto Label_00A1;
            }
            throw ExceptionUtil.ParameterNullOrEmpty("fileMap.ExeConfigFilename");
        Label_0071:
            if (!string.IsNullOrEmpty(map.RoamingUserConfigFilename))
            {
                goto Label_0059;
            }
            throw ExceptionUtil.ParameterNullOrEmpty("fileMap.RoamingUserConfigFilename");
        Label_00A1:
            str = null;
            if (isMachine)
            {
                str = "MACHINE";
            }
            else
            {
                switch (userLevel)
                {
                    case ConfigurationUserLevel.None:
                        str = "MACHINE/EXE";
                        break;

                    case ConfigurationUserLevel.PerUserRoaming:
                        str = "MACHINE/EXE/ROAMING_USER";
                        break;

                    case ConfigurationUserLevel.PerUserRoamingAndLocal:
                        str = "MACHINE/EXE/ROAMING_USER/LOCAL_USER";
                        break;
                }
            }
            return new System.Configuration.Configuration(null, typeof(ClientConfigurationHost), new object[] { fileMap, exePath, str });
        }

        public override Stream OpenStreamForRead(string streamName)
        {
            if (this.IsFile(streamName))
            {
                return base.Host.OpenStreamForRead(streamName);
            }
            if (streamName == null)
            {
                return null;
            }
            WebClient client = new WebClient();
            try
            {
                client.Credentials = CredentialCache.DefaultCredentials;
            }
            catch
            {
            }
            byte[] buffer = null;
            try
            {
                buffer = client.DownloadData(streamName);
            }
            catch
            {
            }
            if (buffer == null)
            {
                return null;
            }
            return new MemoryStream(buffer);
        }

        public override Stream OpenStreamForWrite(string streamName, string templateStreamName, ref object writeContext)
        {
            if (!this.IsFile(streamName))
            {
                throw ExceptionUtil.UnexpectedError("ClientConfigurationHost::OpenStreamForWrite");
            }
            return base.Host.OpenStreamForWrite(streamName, templateStreamName, ref writeContext);
        }

        public override bool PrefetchAll(string configPath, string streamName)
        {
            return !this.IsFile(streamName);
        }

        public override bool PrefetchSection(string sectionGroupName, string sectionName)
        {
            return (sectionGroupName == "system.net");
        }

        internal void RefreshConfigPaths()
        {
            if (((this._configPaths != null) && !this._configPaths.HasEntryAssembly) && (this._exePath == null))
            {
                ClientConfigPaths.RefreshCurrent();
                this._configPaths = null;
            }
        }

        public override void RequireCompleteInit(IInternalConfigRecord record)
        {
            lock (this)
            {
                if (!this._initComplete)
                {
                    this._initComplete = true;
                    ClientConfigPaths.RefreshCurrent();
                    this._configPaths = null;
                    ClientConfigPaths configPaths = this.ConfigPaths;
                }
            }
        }

        string IInternalConfigClientHost.GetExeConfigPath()
        {
            return "MACHINE/EXE";
        }

        string IInternalConfigClientHost.GetLocalUserConfigPath()
        {
            return "MACHINE/EXE/ROAMING_USER/LOCAL_USER";
        }

        string IInternalConfigClientHost.GetRoamingUserConfigPath()
        {
            return "MACHINE/EXE/ROAMING_USER";
        }

        bool IInternalConfigClientHost.IsExeConfig(string configPath)
        {
            return StringUtil.EqualsIgnoreCase(configPath, "MACHINE/EXE");
        }

        bool IInternalConfigClientHost.IsLocalUserConfig(string configPath)
        {
            return StringUtil.EqualsIgnoreCase(configPath, "MACHINE/EXE/ROAMING_USER/LOCAL_USER");
        }

        bool IInternalConfigClientHost.IsRoamingUserConfig(string configPath)
        {
            return StringUtil.EqualsIgnoreCase(configPath, "MACHINE/EXE/ROAMING_USER");
        }

        public override void VerifyDefinitionAllowed(string configPath, ConfigurationAllowDefinition allowDefinition, ConfigurationAllowExeDefinition allowExeDefinition, IConfigErrorInfo errorInfo)
        {
            if (!this.IsDefinitionAllowed(configPath, allowDefinition, allowExeDefinition))
            {
                ConfigurationAllowExeDefinition definition = allowExeDefinition;
                if (definition != ConfigurationAllowExeDefinition.MachineOnly)
                {
                    if (definition == ConfigurationAllowExeDefinition.MachineToApplication)
                    {
                        throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_allow_exedefinition_error_application"), errorInfo);
                    }
                    if (definition == ConfigurationAllowExeDefinition.MachineToRoamingUser)
                    {
                        throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_allow_exedefinition_error_roaminguser"), errorInfo);
                    }
                    throw ExceptionUtil.UnexpectedError("ClientConfigurationHost::VerifyDefinitionAllowed");
                }
                throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_allow_exedefinition_error_machine"), errorInfo);
            }
        }

        internal ClientConfigPaths ConfigPaths
        {
            get
            {
                if (this._configPaths == null)
                {
                    this._configPaths = ClientConfigPaths.GetPaths(this._exePath, this._initComplete);
                }
                return this._configPaths;
            }
        }

        internal bool HasLocalConfig
        {
            get
            {
                if (this._fileMap != null)
                {
                    return !string.IsNullOrEmpty(this._fileMap.LocalUserConfigFilename);
                }
                return this.ConfigPaths.HasLocalConfig;
            }
        }

        internal bool HasRoamingConfig
        {
            get
            {
                if (this._fileMap != null)
                {
                    return !string.IsNullOrEmpty(this._fileMap.RoamingUserConfigFilename);
                }
                return this.ConfigPaths.HasRoamingConfig;
            }
        }

        internal bool IsAppConfigHttp
        {
            get
            {
                return !this.IsFile(this.GetStreamName("MACHINE/EXE"));
            }
        }

        internal static string MachineConfigFilePath
        {
            [FileIOPermission(SecurityAction.Assert, AllFiles=FileIOPermissionAccess.PathDiscovery)]
            get
            {
                if (s_machineConfigFilePath == null)
                {
                    s_machineConfigFilePath = Path.Combine(Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), "Config"), "machine.config");
                }
                return s_machineConfigFilePath;
            }
        }

        public override bool SupportsLocation
        {
            get
            {
                return false;
            }
        }

        public override bool SupportsPath
        {
            get
            {
                return false;
            }
        }

        public override bool SupportsRefresh
        {
            get
            {
                return true;
            }
        }
    }
}

