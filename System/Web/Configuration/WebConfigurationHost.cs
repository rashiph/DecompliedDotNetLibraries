namespace System.Web.Configuration
{
    using Microsoft.Build.Utilities;
    using System;
    using System.Collections;
    using System.Configuration;
    using System.Configuration.Internal;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Web;
    using System.Web.Compilation;
    using System.Web.Configuration.Internal;
    using System.Web.Hosting;
    using System.Web.Util;

    internal sealed class WebConfigurationHost : DelegatingConfigHost, IInternalConfigWebHost
    {
        private string _appConfigPath;
        private VirtualPath _appPath;
        private string _appSiteID;
        private string _appSiteName;
        private IConfigMapPath _configMapPath;
        private IConfigMapPath2 _configMapPath2;
        private Hashtable _fileChangeCallbacks;
        private string _machineConfigFile;
        private string _rootWebConfigFile;
        internal const string DefaultSiteID = "1";
        private const string InternalConfigConfigurationFactoryTypeName = "System.Configuration.Internal.InternalConfigConfigurationFactory, System.Configuration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
        private const string InternalHostTypeName = "System.Configuration.Internal.InternalConfigHost, System.Configuration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
        internal const string MachineConfigName = "machine";
        internal const string MachineConfigPath = "machine";
        internal const char PathSeparator = '/';
        internal const string RootWebConfigName = "webroot";
        internal const string RootWebConfigPath = "machine/webroot";
        private static readonly string RootWebConfigPathAndDefaultSiteID = (RootWebConfigPathAndPathSeparator + "1");
        private static readonly string RootWebConfigPathAndPathSeparator = ("machine/webroot" + '/');
        private static IInternalConfigConfigurationFactory s_configurationFactory;
        private static string s_defaultSiteName;
        internal static readonly char[] s_slashSplit = new char[0x2f];
        private const string SysWebName = "system.web";

        internal WebConfigurationHost()
        {
            Type type = Type.GetType("System.Configuration.Internal.InternalConfigHost, System.Configuration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", true);
            base.Host = (IInternalConfigHost) Activator.CreateInstance(type, true);
        }

        private void ChooseAndInitConfigMapPath(bool useConfigMapPath, IConfigMapPath configMapPath, ConfigurationFileMap fileMap)
        {
            if (useConfigMapPath)
            {
                this._configMapPath = configMapPath;
            }
            else if (fileMap != null)
            {
                this._configMapPath = new UserMapPath(fileMap);
            }
            else if (HostingEnvironment.IsHosted)
            {
                this._configMapPath = HostingPreferredMapPath.GetInstance();
            }
            else
            {
                this._configMapPath = IISMapPath.GetInstance();
            }
            this._configMapPath2 = this._configMapPath as IConfigMapPath2;
        }

        [FileIOPermission(SecurityAction.Assert, AllFiles=FileIOPermissionAccess.PathDiscovery)]
        private string CombineAndValidatePath(string directory, string baseName)
        {
            try
            {
                string path = Path.Combine(directory, baseName);
                Path.GetFullPath(path);
                return path;
            }
            catch (PathTooLongException)
            {
                return null;
            }
            catch (NotSupportedException)
            {
                return null;
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        internal static string CombineConfigPath(string parentConfigPath, string childConfigPath)
        {
            if (string.IsNullOrEmpty(parentConfigPath))
            {
                return childConfigPath;
            }
            if (string.IsNullOrEmpty(childConfigPath))
            {
                return parentConfigPath;
            }
            return (parentConfigPath + '/' + childConfigPath);
        }

        public override object CreateConfigurationContext(string configPath, string locationSubPath)
        {
            return new WebContext(this.GetPathLevel(configPath), this._appSiteName, VirtualPath.GetVirtualPathString(this._appPath), VPathFromConfigPath(configPath), locationSubPath, this._appConfigPath);
        }

        public override object CreateDeprecatedConfigContext(string configPath)
        {
            return new HttpConfigurationContext(VPathFromConfigPath(configPath));
        }

        public override string GetConfigPathFromLocationSubPath(string configPath, string locationSubPath)
        {
            string str3;
            VirtualPath rootVirtualPath;
            if (IsVirtualPathConfigPath(configPath))
            {
                return CombineConfigPath(configPath, locationSubPath);
            }
            string siteID = null;
            int index = locationSubPath.IndexOf('/');
            if (index < 0)
            {
                str3 = locationSubPath;
                rootVirtualPath = VirtualPath.RootVirtualPath;
            }
            else
            {
                str3 = locationSubPath.Substring(0, index);
                rootVirtualPath = VirtualPath.CreateAbsolute(locationSubPath.Substring(index));
            }
            if (System.Web.Util.StringUtil.EqualsIgnoreCase(str3, this._appSiteID) || System.Web.Util.StringUtil.EqualsIgnoreCase(str3, this._appSiteName))
            {
                siteID = this._appSiteID;
            }
            else
            {
                siteID = str3;
            }
            return GetConfigPathFromSiteIDAndVPath(siteID, rootVirtualPath);
        }

        internal static string GetConfigPathFromLocationSubPathBasic(string configPath, string locationSubPath)
        {
            if (IsVirtualPathConfigPath(configPath))
            {
                return CombineConfigPath(configPath, locationSubPath);
            }
            return CombineConfigPath("machine/webroot", locationSubPath);
        }

        internal static string GetConfigPathFromSiteIDAndVPath(string siteID, VirtualPath vpath)
        {
            if ((vpath == null) || string.IsNullOrEmpty(siteID))
            {
                return "machine/webroot";
            }
            string str = vpath.VirtualPathStringNoTrailingSlash.ToLower(CultureInfo.InvariantCulture);
            string str2 = (siteID == "1") ? RootWebConfigPathAndDefaultSiteID : (RootWebConfigPathAndPathSeparator + siteID);
            if (str.Length > 1)
            {
                str2 = str2 + str;
            }
            return str2;
        }

        internal static void GetConfigPaths(IConfigMapPath configMapPath, WebLevel webLevel, VirtualPath virtualPath, string site, string locationSubPath, out VirtualPath appPath, out string appSiteName, out string appSiteID, out string configPath, out string locationConfigPath)
        {
            appPath = null;
            appSiteName = null;
            appSiteID = null;
            if ((webLevel == WebLevel.Machine) || (virtualPath == null))
            {
                if (!string.IsNullOrEmpty(site) && string.IsNullOrEmpty(locationSubPath))
                {
                    throw System.Web.Util.ExceptionUtil.ParameterInvalid("site");
                }
                if (webLevel == WebLevel.Machine)
                {
                    configPath = "machine";
                }
                else
                {
                    configPath = "machine/webroot";
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(site))
                {
                    configMapPath.ResolveSiteArgument(site, out appSiteName, out appSiteID);
                    if (string.IsNullOrEmpty(appSiteID))
                    {
                        throw new InvalidOperationException(System.Web.SR.GetString("Config_failed_to_resolve_site_id", new object[] { site }));
                    }
                }
                else
                {
                    if (HostingEnvironment.IsHosted)
                    {
                        appSiteName = HostingEnvironment.SiteNameNoDemand;
                        appSiteID = HostingEnvironment.SiteID;
                    }
                    if (string.IsNullOrEmpty(appSiteID))
                    {
                        configMapPath.GetDefaultSiteNameAndID(out appSiteName, out appSiteID);
                    }
                }
                configPath = GetConfigPathFromSiteIDAndVPath(appSiteID, virtualPath);
            }
            locationConfigPath = null;
            string siteID = null;
            VirtualPath vpath = null;
            if (locationSubPath != null)
            {
                locationConfigPath = GetConfigPathFromLocationSubPathBasic(configPath, locationSubPath);
                GetSiteIDAndVPathFromConfigPath(locationConfigPath, out siteID, out vpath);
                if (string.IsNullOrEmpty(appSiteID) && !string.IsNullOrEmpty(siteID))
                {
                    configMapPath.ResolveSiteArgument(siteID, out appSiteName, out appSiteID);
                    if (!string.IsNullOrEmpty(appSiteID))
                    {
                        locationConfigPath = GetConfigPathFromSiteIDAndVPath(appSiteID, vpath);
                    }
                    else if ((vpath == null) || (vpath.VirtualPathString == "/"))
                    {
                        appSiteName = siteID;
                        appSiteID = siteID;
                    }
                    else
                    {
                        appSiteName = null;
                        appSiteID = null;
                    }
                }
            }
            string appPathForPath = null;
            if (vpath != null)
            {
                appPathForPath = configMapPath.GetAppPathForPath(appSiteID, vpath.VirtualPathString);
            }
            else if (virtualPath != null)
            {
                appPathForPath = configMapPath.GetAppPathForPath(appSiteID, virtualPath.VirtualPathString);
            }
            if (appPathForPath != null)
            {
                appPath = VirtualPath.Create(appPathForPath);
            }
        }

        public override Type GetConfigType(string typeName, bool throwOnError)
        {
            return BuildManager.GetType(typeName, throwOnError);
        }

        public override string GetConfigTypeName(Type t)
        {
            return BuildManager.GetNormalizedTypeName(t);
        }

        private static string GetMachineConfigPathFromTargetFrameworkMoniker(string moniker)
        {
            TargetDotNetFrameworkVersion targetFrameworkVersionEnumFromMoniker = GetTargetFrameworkVersionEnumFromMoniker(moniker);
            if (targetFrameworkVersionEnumFromMoniker == TargetDotNetFrameworkVersion.Version40)
            {
                return null;
            }
            string pathToDotNetFrameworkFile = ToolLocationHelper.GetPathToDotNetFrameworkFile(@"config\machine.config", targetFrameworkVersionEnumFromMoniker);
            new FileIOPermission(FileIOPermissionAccess.PathDiscovery, pathToDotNetFrameworkFile).Demand();
            return pathToDotNetFrameworkFile;
        }

        private WebApplicationLevel GetPathLevel(string configPath)
        {
            if (IsVirtualPathConfigPath(configPath))
            {
                string str;
                VirtualPath path;
                if (this._appPath == null)
                {
                    return WebApplicationLevel.AboveApplication;
                }
                GetSiteIDAndVPathFromConfigPath(configPath, out str, out path);
                if (!System.Web.Util.StringUtil.EqualsIgnoreCase(this._appSiteID, str))
                {
                    return WebApplicationLevel.AboveApplication;
                }
                if (this._appPath == path)
                {
                    return WebApplicationLevel.AtApplication;
                }
                if (System.Web.Util.UrlPath.IsEqualOrSubpath(this._appPath.VirtualPathString, path.VirtualPathString))
                {
                    return WebApplicationLevel.BelowApplication;
                }
            }
            return WebApplicationLevel.AboveApplication;
        }

        public override void GetRestrictedPermissions(IInternalConfigRecord configRecord, out PermissionSet permissionSet, out bool isHostReady)
        {
            StaticGetRestrictedPermissions(configRecord, out permissionSet, out isHostReady);
        }

        internal static void GetSiteIDAndVPathFromConfigPath(string configPath, out string siteID, out VirtualPath vpath)
        {
            if (!IsVirtualPathConfigPath(configPath))
            {
                siteID = null;
                vpath = null;
            }
            else
            {
                int num3;
                int startIndex = "machine/webroot".Length + 1;
                int index = configPath.IndexOf('/', startIndex);
                if (index == -1)
                {
                    num3 = configPath.Length - startIndex;
                }
                else
                {
                    num3 = index - startIndex;
                }
                siteID = configPath.Substring(startIndex, num3);
                if (index == -1)
                {
                    vpath = VirtualPath.RootVirtualPath;
                }
                else
                {
                    vpath = VirtualPath.CreateAbsolute(configPath.Substring(index));
                }
            }
        }

        public override string GetStreamName(string configPath)
        {
            string str;
            VirtualPath path;
            string str2;
            string str3;
            if (IsMachineConfigPath(configPath))
            {
                if (string.IsNullOrEmpty(this._machineConfigFile))
                {
                    return this._configMapPath.GetMachineConfigFilename();
                }
                return this._machineConfigFile;
            }
            if (IsRootWebConfigPath(configPath))
            {
                if (string.IsNullOrEmpty(this._rootWebConfigFile))
                {
                    return this._configMapPath.GetRootWebConfigFilename();
                }
                return this._rootWebConfigFile;
            }
            GetSiteIDAndVPathFromConfigPath(configPath, out str, out path);
            if (this._configMapPath2 != null)
            {
                this._configMapPath2.GetPathConfigFilename(str, path, out str2, out str3);
            }
            else
            {
                this._configMapPath.GetPathConfigFilename(str, path.VirtualPathString, out str2, out str3);
            }
            if (str2 != null)
            {
                bool flag;
                bool flag2;
                System.Web.Util.FileUtil.PhysicalPathStatus(str2, true, false, out flag, out flag2);
                if (flag && flag2)
                {
                    return this.CombineAndValidatePath(str2, str3);
                }
            }
            return null;
        }

        private static TargetDotNetFrameworkVersion GetTargetFrameworkVersionEnumFromMoniker(string moniker)
        {
            if ((!moniker.Contains("3.5") && !moniker.Contains("3.0")) && !moniker.Contains("2.0"))
            {
                return TargetDotNetFrameworkVersion.Version40;
            }
            return TargetDotNetFrameworkVersion.Version20;
        }

        public override IDisposable Impersonate()
        {
            return new ApplicationImpersonationContext();
        }

        public override void Init(IInternalConfigRoot configRoot, params object[] hostInitParams)
        {
            bool useConfigMapPath = (bool) hostInitParams[0];
            IConfigMapPath configMapPath = (IConfigMapPath) hostInitParams[1];
            ConfigurationFileMap fileMap = (ConfigurationFileMap) hostInitParams[2];
            string path = (string) hostInitParams[3];
            string str2 = (string) hostInitParams[4];
            string str3 = (string) hostInitParams[5];
            if (hostInitParams.Length > 6)
            {
                string moniker = hostInitParams[6] as string;
                this._machineConfigFile = GetMachineConfigPathFromTargetFrameworkMoniker(moniker);
                if (!string.IsNullOrEmpty(this._machineConfigFile))
                {
                    this._rootWebConfigFile = Path.Combine(Path.GetDirectoryName(this._machineConfigFile), "web.config");
                }
            }
            base.Host.Init(configRoot, hostInitParams);
            this.ChooseAndInitConfigMapPath(useConfigMapPath, configMapPath, fileMap);
            path = System.Web.Util.UrlPath.RemoveSlashFromPathIfNeeded(path);
            this._appPath = VirtualPath.CreateAbsoluteAllowNull(path);
            this._appSiteName = str2;
            this._appSiteID = str3;
            if (!string.IsNullOrEmpty(this._appSiteID) && (this._appPath != null))
            {
                this._appConfigPath = GetConfigPathFromSiteIDAndVPath(this._appSiteID, this._appPath);
            }
        }

        public override void InitForConfiguration(ref string locationSubPath, out string configPath, out string locationConfigPath, IInternalConfigRoot configRoot, params object[] hostInitConfigurationParams)
        {
            WebLevel webLevel = (WebLevel) hostInitConfigurationParams[0];
            ConfigurationFileMap fileMap = (ConfigurationFileMap) hostInitConfigurationParams[1];
            VirtualPath virtualPath = VirtualPath.CreateAbsoluteAllowNull((string) hostInitConfigurationParams[2]);
            string site = (string) hostInitConfigurationParams[3];
            if (locationSubPath == null)
            {
                locationSubPath = (string) hostInitConfigurationParams[4];
            }
            base.Host.Init(configRoot, hostInitConfigurationParams);
            this.ChooseAndInitConfigMapPath(false, null, fileMap);
            GetConfigPaths(this._configMapPath, webLevel, virtualPath, site, locationSubPath, out this._appPath, out this._appSiteName, out this._appSiteID, out configPath, out locationConfigPath);
            this._appConfigPath = GetConfigPathFromSiteIDAndVPath(this._appSiteID, this._appPath);
            if (IsVirtualPathConfigPath(configPath))
            {
                string str2;
                VirtualPath path2;
                string str3;
                GetSiteIDAndVPathFromConfigPath(configPath, out str2, out path2);
                if (this._configMapPath2 != null)
                {
                    str3 = this._configMapPath2.MapPath(str2, path2);
                }
                else
                {
                    str3 = this._configMapPath.MapPath(str2, path2.VirtualPathString);
                }
                if (string.IsNullOrEmpty(str3))
                {
                    throw new ArgumentOutOfRangeException("site");
                }
            }
        }

        public override bool IsAboveApplication(string configPath)
        {
            return (this.GetPathLevel(configPath) == WebApplicationLevel.AboveApplication);
        }

        private bool IsApplication(string configPath)
        {
            VirtualPath appPathForPath;
            string str;
            VirtualPath path2;
            GetSiteIDAndVPathFromConfigPath(configPath, out str, out path2);
            if (this._configMapPath2 != null)
            {
                appPathForPath = this._configMapPath2.GetAppPathForPath(str, path2);
            }
            else
            {
                appPathForPath = VirtualPath.CreateAllowNull(this._configMapPath.GetAppPathForPath(str, path2.VirtualPathString));
            }
            return (appPathForPath == path2);
        }

        public override bool IsConfigRecordRequired(string configPath)
        {
            string str;
            VirtualPath path;
            string str2;
            if (!IsVirtualPathConfigPath(configPath))
            {
                return true;
            }
            GetSiteIDAndVPathFromConfigPath(configPath, out str, out path);
            if (this._configMapPath2 != null)
            {
                str2 = this._configMapPath2.MapPath(str, path);
            }
            else
            {
                str2 = this._configMapPath.MapPath(str, path.VirtualPathString);
            }
            return ((str2 == null) || System.Web.Util.FileUtil.DirectoryExists(str2, true));
        }

        public override bool IsDefinitionAllowed(string configPath, ConfigurationAllowDefinition allowDefinition, ConfigurationAllowExeDefinition allowExeDefinition)
        {
            switch (allowDefinition)
            {
                case ConfigurationAllowDefinition.MachineToApplication:
                    if (!string.IsNullOrEmpty(this._appConfigPath) && (configPath.Length > this._appConfigPath.Length))
                    {
                        return this.IsApplication(configPath);
                    }
                    return true;

                case ConfigurationAllowDefinition.Everywhere:
                    return true;

                case ConfigurationAllowDefinition.MachineOnly:
                    return (configPath.Length <= "machine".Length);

                case ConfigurationAllowDefinition.MachineToWebRoot:
                    return (configPath.Length <= "machine/webroot".Length);
            }
            throw System.Web.Util.ExceptionUtil.UnexpectedError("WebConfigurationHost::IsDefinitionAllowed");
        }

        public override bool IsFullTrustSectionWithoutAptcaAllowed(IInternalConfigRecord configRecord)
        {
            if (HostingEnvironment.IsHosted)
            {
                return HttpRuntime.HasAspNetHostingPermission(AspNetHostingPermissionLevel.Unrestricted);
            }
            return base.Host.IsFullTrustSectionWithoutAptcaAllowed(configRecord);
        }

        public override bool IsLocationApplicable(string configPath)
        {
            return IsVirtualPathConfigPath(configPath);
        }

        internal static bool IsMachineConfigPath(string configPath)
        {
            return (configPath.Length == "machine".Length);
        }

        internal static bool IsRootWebConfigPath(string configPath)
        {
            return (configPath.Length == "machine/webroot".Length);
        }

        public override bool IsTrustedConfigPath(string configPath)
        {
            return !IsVirtualPathConfigPath(configPath);
        }

        internal static bool IsValidSiteArgument(string site)
        {
            if (!string.IsNullOrEmpty(site))
            {
                char ch = site[0];
                char ch2 = site[site.Length - 1];
                if (((ch == '/') || (ch == '\\')) || ((ch2 == '/') || (ch2 == '\\')))
                {
                    return false;
                }
            }
            return true;
        }

        internal static bool IsVirtualPathConfigPath(string configPath)
        {
            return (configPath.Length > "machine/webroot".Length);
        }

        internal static System.Configuration.Configuration OpenConfiguration(WebLevel webLevel, ConfigurationFileMap fileMap, VirtualPath path, string site, string locationSubPath, string server, string userName, string password, IntPtr tokenHandle)
        {
            if (!IsValidSiteArgument(site))
            {
                throw System.Web.Util.ExceptionUtil.ParameterInvalid("site");
            }
            locationSubPath = ConfigurationFactory.NormalizeLocationSubPath(locationSubPath, null);
            if ((((!string.IsNullOrEmpty(server) && (server != ".")) && (!System.Web.Util.StringUtil.EqualsIgnoreCase(server, "127.0.0.1") && !System.Web.Util.StringUtil.EqualsIgnoreCase(server, "::1"))) && !System.Web.Util.StringUtil.EqualsIgnoreCase(server, "localhost")) && !System.Web.Util.StringUtil.EqualsIgnoreCase(server, Environment.MachineName))
            {
                object[] hostInitConfigurationParams = new object[9];
                hostInitConfigurationParams[0] = webLevel;
                hostInitConfigurationParams[2] = VirtualPath.GetVirtualPathString(path);
                hostInitConfigurationParams[3] = site;
                hostInitConfigurationParams[4] = locationSubPath;
                hostInitConfigurationParams[5] = server;
                hostInitConfigurationParams[6] = userName;
                hostInitConfigurationParams[7] = password;
                hostInitConfigurationParams[8] = tokenHandle;
                return ConfigurationFactory.Create(typeof(RemoteWebConfigurationHost), hostInitConfigurationParams);
            }
            if (string.IsNullOrEmpty(server))
            {
                if (!string.IsNullOrEmpty(userName))
                {
                    throw System.Web.Util.ExceptionUtil.ParameterInvalid("userName");
                }
                if (!string.IsNullOrEmpty(password))
                {
                    throw System.Web.Util.ExceptionUtil.ParameterInvalid("password");
                }
                if (tokenHandle != IntPtr.Zero)
                {
                    throw System.Web.Util.ExceptionUtil.ParameterInvalid("tokenHandle");
                }
            }
            if (fileMap != null)
            {
                fileMap = (ConfigurationFileMap) fileMap.Clone();
            }
            WebConfigurationFileMap map = fileMap as WebConfigurationFileMap;
            if ((map != null) && !string.IsNullOrEmpty(site))
            {
                map.Site = site;
            }
            return ConfigurationFactory.Create(typeof(WebConfigurationHost), new object[] { webLevel, fileMap, VirtualPath.GetVirtualPathString(path), site, locationSubPath });
        }

        public override bool PrefetchAll(string configPath, string streamName)
        {
            return !IsMachineConfigPath(configPath);
        }

        public override bool PrefetchSection(string sectionGroupName, string sectionName)
        {
            return ((System.Web.Util.StringUtil.StringStartsWith(sectionGroupName, "system.web") && ((sectionGroupName.Length == "system.web".Length) || (sectionGroupName["system.web".Length] == '/'))) || (string.IsNullOrEmpty(sectionGroupName) && (sectionName == "system.codedom")));
        }

        public override object StartMonitoringStreamForChanges(string streamName, StreamChangeCallback callback)
        {
            WebConfigurationHostFileChange change;
            lock (this)
            {
                change = new WebConfigurationHostFileChange(callback);
                ArrayList list = (ArrayList) this.FileChangeCallbacks[streamName];
                if (list == null)
                {
                    list = new ArrayList(1);
                    this.FileChangeCallbacks.Add(streamName, list);
                }
                list.Add(change);
            }
            HttpRuntime.FileChangesMonitor.StartMonitoringFile(streamName, new FileChangeEventHandler(change.OnFileChanged));
            return change;
        }

        internal static void StaticGetRestrictedPermissions(IInternalConfigRecord configRecord, out PermissionSet permissionSet, out bool isHostReady)
        {
            isHostReady = HttpRuntime.IsTrustLevelInitialized;
            permissionSet = null;
            if (isHostReady && IsVirtualPathConfigPath(configRecord.ConfigPath))
            {
                permissionSet = HttpRuntime.NamedPermissionSet;
            }
        }

        public override void StopMonitoringStreamForChanges(string streamName, StreamChangeCallback callback)
        {
            WebConfigurationHostFileChange target = null;
            lock (this)
            {
                ArrayList list = (ArrayList) this.FileChangeCallbacks[streamName];
                for (int i = 0; i < list.Count; i++)
                {
                    WebConfigurationHostFileChange change2 = (WebConfigurationHostFileChange) list[i];
                    if (object.ReferenceEquals(change2.Callback, callback))
                    {
                        target = change2;
                        list.RemoveAt(i);
                        if (list.Count == 0)
                        {
                            this.FileChangeCallbacks.Remove(streamName);
                        }
                        break;
                    }
                }
            }
            HttpRuntime.FileChangesMonitor.StopMonitoringFile(streamName, target);
        }

        [AspNetHostingPermission(SecurityAction.Demand, Level=AspNetHostingPermissionLevel.Medium)]
        string IInternalConfigWebHost.GetConfigPathFromSiteIDAndVPath(string siteID, string vpath)
        {
            return GetConfigPathFromSiteIDAndVPath(siteID, VirtualPath.CreateAbsoluteAllowNull(vpath));
        }

        [AspNetHostingPermission(SecurityAction.Demand, Level=AspNetHostingPermissionLevel.Medium)]
        void IInternalConfigWebHost.GetSiteIDAndVPathFromConfigPath(string configPath, out string siteID, out string vpath)
        {
            VirtualPath path;
            GetSiteIDAndVPathFromConfigPath(configPath, out siteID, out path);
            vpath = VirtualPath.GetVirtualPathString(path);
        }

        public override void VerifyDefinitionAllowed(string configPath, ConfigurationAllowDefinition allowDefinition, ConfigurationAllowExeDefinition allowExeDefinition, IConfigErrorInfo errorInfo)
        {
            if (!this.IsDefinitionAllowed(configPath, allowDefinition, allowExeDefinition))
            {
                ConfigurationAllowDefinition definition = allowDefinition;
                if (definition != ConfigurationAllowDefinition.MachineOnly)
                {
                    if (definition == ConfigurationAllowDefinition.MachineToWebRoot)
                    {
                        throw new ConfigurationErrorsException(System.Web.SR.GetString("Config_allow_definition_error_webroot"), errorInfo.Filename, errorInfo.LineNumber);
                    }
                    if (definition == ConfigurationAllowDefinition.MachineToApplication)
                    {
                        throw new ConfigurationErrorsException(System.Web.SR.GetString("Config_allow_definition_error_application"), errorInfo.Filename, errorInfo.LineNumber);
                    }
                    throw System.Web.Util.ExceptionUtil.UnexpectedError("WebConfigurationHost::VerifyDefinitionAllowed");
                }
                throw new ConfigurationErrorsException(System.Web.SR.GetString("Config_allow_definition_error_machine"), errorInfo.Filename, errorInfo.LineNumber);
            }
        }

        internal static string VPathFromConfigPath(string configPath)
        {
            if (!IsVirtualPathConfigPath(configPath))
            {
                return null;
            }
            int startIndex = "machine/webroot".Length + 1;
            int index = configPath.IndexOf('/', startIndex);
            if (index == -1)
            {
                return "/";
            }
            return configPath.Substring(index);
        }

        internal static IInternalConfigConfigurationFactory ConfigurationFactory
        {
            [ReflectionPermission(SecurityAction.Assert, Flags=ReflectionPermissionFlag.MemberAccess)]
            get
            {
                if (s_configurationFactory == null)
                {
                    s_configurationFactory = (IInternalConfigConfigurationFactory) Activator.CreateInstance(Type.GetType("System.Configuration.Internal.InternalConfigConfigurationFactory, System.Configuration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", true), true);
                }
                return s_configurationFactory;
            }
        }

        internal static string DefaultSiteName
        {
            get
            {
                if (s_defaultSiteName == null)
                {
                    s_defaultSiteName = System.Web.SR.GetString("DefaultSiteName");
                }
                return s_defaultSiteName;
            }
        }

        private Hashtable FileChangeCallbacks
        {
            get
            {
                if (this._fileChangeCallbacks == null)
                {
                    this._fileChangeCallbacks = new Hashtable(StringComparer.OrdinalIgnoreCase);
                }
                return this._fileChangeCallbacks;
            }
        }

        public override bool SupportsChangeNotifications
        {
            get
            {
                return true;
            }
        }

        public override bool SupportsLocation
        {
            get
            {
                return true;
            }
        }

        public override bool SupportsPath
        {
            get
            {
                return true;
            }
        }
    }
}

