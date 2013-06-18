namespace System.Web.Configuration
{
    using System;
    using System.Configuration.Internal;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Web;
    using System.Web.Hosting;

    internal class HttpConfigurationSystem : IInternalConfigSystem
    {
        internal const string ApplicationHostConfigFileName = "applicationHost.config";
        internal const string ConfigSystemTypeString = "System.Configuration.Internal.ConfigSystem, System.Configuration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
        internal const string InetsrvDirectoryName = "inetsrv";
        private const string InternalConfigSettingsFactoryTypeString = "System.Configuration.Internal.InternalConfigSettingsFactory, System.Configuration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
        internal const string MachineConfigFilename = "machine.config";
        internal const string MachineConfigSubdirectory = "Config";
        internal const string RootWebConfigFilename = "web.config";
        private static WebConfigurationHost s_configHost;
        private static IConfigMapPath s_configMapPath;
        private static IInternalConfigRoot s_configRoot;
        private static IInternalConfigSettingsFactory s_configSettingsFactory;
        private static IConfigSystem s_configSystem;
        private static FileChangeEventHandler s_fileChangeEventHandler;
        private static HttpConfigurationSystem s_httpConfigSystem;
        private static bool s_initComplete;
        private static volatile bool s_inited;
        private static object s_initLock = new object();
        private static string s_MachineConfigurationDirectory;
        private static string s_MachineConfigurationFilePath;
        private static string s_MsCorLibDirectory;
        private static string s_RootWebConfigurationFilePath;
        internal const string WebConfigFileName = "web.config";

        private HttpConfigurationSystem()
        {
        }

        internal static void AddFileDependency(string file)
        {
            if (!string.IsNullOrEmpty(file) && UseHttpConfigurationSystem)
            {
                if (s_fileChangeEventHandler == null)
                {
                    s_fileChangeEventHandler = new FileChangeEventHandler(s_httpConfigSystem.OnConfigFileChanged);
                }
                HttpRuntime.FileChangesMonitor.StartMonitoringFile(file, s_fileChangeEventHandler);
            }
        }

        internal static void CompleteInit()
        {
            s_configSettingsFactory.CompleteInit();
            s_configSettingsFactory = null;
        }

        internal static void EnsureInit(IConfigMapPath configMapPath, bool listenToFileChanges, bool initComplete)
        {
            if (!s_inited)
            {
                lock (s_initLock)
                {
                    if (!s_inited)
                    {
                        s_initComplete = initComplete;
                        if (configMapPath == null)
                        {
                            configMapPath = IISMapPath.GetInstance();
                        }
                        s_configMapPath = configMapPath;
                        s_configSystem = (IConfigSystem) Activator.CreateInstance(Type.GetType("System.Configuration.Internal.ConfigSystem, System.Configuration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", true), true);
                        object[] hostInitParams = new object[6];
                        hostInitParams[0] = true;
                        hostInitParams[1] = s_configMapPath;
                        hostInitParams[3] = HostingEnvironment.ApplicationVirtualPath;
                        hostInitParams[4] = HostingEnvironment.SiteNameNoDemand;
                        hostInitParams[5] = HostingEnvironment.SiteID;
                        s_configSystem.Init(typeof(WebConfigurationHost), hostInitParams);
                        s_configRoot = s_configSystem.Root;
                        s_configHost = (WebConfigurationHost) s_configSystem.Host;
                        HttpConfigurationSystem internalConfigSystem = new HttpConfigurationSystem();
                        if (listenToFileChanges)
                        {
                            s_configRoot.ConfigChanged += new InternalConfigEventHandler(internalConfigSystem.OnConfigurationChanged);
                        }
                        s_configSettingsFactory = (IInternalConfigSettingsFactory) Activator.CreateInstance(Type.GetType("System.Configuration.Internal.InternalConfigSettingsFactory, System.Configuration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", true), true);
                        s_configSettingsFactory.SetConfigurationSystem(internalConfigSystem, initComplete);
                        s_httpConfigSystem = internalConfigSystem;
                        s_inited = true;
                    }
                }
            }
        }

        internal static object GetApplicationSection(string sectionName)
        {
            return CachedPathData.GetApplicationPathData().ConfigRecord.GetSection(sectionName);
        }

        internal static object GetSection(string sectionName)
        {
            HttpContext current = HttpContext.Current;
            if (current != null)
            {
                return current.GetSection(sectionName);
            }
            return GetApplicationSection(sectionName);
        }

        internal static object GetSection(string sectionName, string path)
        {
            return GetSection(sectionName, VirtualPath.CreateNonRelativeAllowNull(path));
        }

        internal static object GetSection(string sectionName, VirtualPath path)
        {
            return CachedPathData.GetVirtualPathData(path, true).ConfigRecord.GetSection(sectionName);
        }

        internal static IInternalConfigRecord GetUniqueConfigRecord(string configPath)
        {
            if (!UseHttpConfigurationSystem)
            {
                return null;
            }
            return s_configRoot.GetUniqueConfigRecord(configPath);
        }

        internal void OnConfigFileChanged(object sender, FileChangeEvent e)
        {
            HttpRuntime.OnConfigChange();
        }

        internal void OnConfigurationChanged(object sender, InternalConfigEventArgs e)
        {
            HttpRuntime.OnConfigChange();
        }

        object IInternalConfigSystem.GetSection(string configKey)
        {
            return GetSection(configKey);
        }

        void IInternalConfigSystem.RefreshConfig(string sectionName)
        {
        }

        internal static bool IsSet
        {
            get
            {
                return (s_httpConfigSystem != null);
            }
        }

        internal static string MachineConfigurationDirectory
        {
            get
            {
                if (s_MachineConfigurationDirectory == null)
                {
                    s_MachineConfigurationDirectory = Path.Combine(MsCorLibDirectory, "Config");
                }
                return s_MachineConfigurationDirectory;
            }
        }

        internal static string MachineConfigurationFilePath
        {
            get
            {
                if (s_MachineConfigurationFilePath == null)
                {
                    s_MachineConfigurationFilePath = Path.Combine(MachineConfigurationDirectory, "machine.config");
                }
                return s_MachineConfigurationFilePath;
            }
        }

        internal static string MsCorLibDirectory
        {
            [FileIOPermission(SecurityAction.Assert, AllFiles=FileIOPermissionAccess.PathDiscovery)]
            get
            {
                if (s_MsCorLibDirectory == null)
                {
                    s_MsCorLibDirectory = RuntimeEnvironment.GetRuntimeDirectory();
                }
                return s_MsCorLibDirectory;
            }
        }

        internal static string RootWebConfigurationFilePath
        {
            get
            {
                if (s_RootWebConfigurationFilePath == null)
                {
                    s_RootWebConfigurationFilePath = Path.Combine(MachineConfigurationDirectory, "web.config");
                }
                return s_RootWebConfigurationFilePath;
            }
            set
            {
                s_RootWebConfigurationFilePath = value;
                if (s_RootWebConfigurationFilePath == null)
                {
                    s_RootWebConfigurationFilePath = Path.Combine(MachineConfigurationDirectory, "web.config");
                }
            }
        }

        bool IInternalConfigSystem.SupportsUserConfig
        {
            get
            {
                return false;
            }
        }

        internal static bool UseHttpConfigurationSystem
        {
            get
            {
                if (!s_inited)
                {
                    lock (s_initLock)
                    {
                        if (!s_inited)
                        {
                            s_inited = true;
                        }
                    }
                }
                return (s_httpConfigSystem != null);
            }
        }
    }
}

