using Microsoft.Build.Utilities;
using System;
using System.Collections.Concurrent;
using System.Configuration;
using System.Web;
using System.Web.Compilation;
using System.Web.Configuration;
using System.Web.Hosting;

internal class MTConfigUtil
{
    private static VirtualPath s_appVirtualPath;
    private static readonly ConcurrentDictionary<VirtualPath, System.Configuration.Configuration> s_configurations = new ConcurrentDictionary<VirtualPath, System.Configuration.Configuration>();
    private static string s_machineConfigPath;
    private static readonly ConcurrentDictionary<Tuple<Type, VirtualPath>, ConfigurationSection> s_sections = new ConcurrentDictionary<Tuple<Type, VirtualPath>, ConfigurationSection>();
    private static bool? s_useMTConfig;

    private static S GetAppConfig<S>() where S: ConfigurationSection
    {
        return GetConfig<S>((VirtualPath) null);
    }

    internal static CompilationSection GetCompilationAppConfig()
    {
        if (!UseMTConfig)
        {
            return RuntimeConfig.GetAppConfig().Compilation;
        }
        return GetAppConfig<CompilationSection>();
    }

    internal static CompilationSection GetCompilationConfig()
    {
        if (!UseMTConfig)
        {
            return RuntimeConfig.GetConfig().Compilation;
        }
        return GetConfig<CompilationSection>();
    }

    internal static CompilationSection GetCompilationConfig(string vpath)
    {
        if (!UseMTConfig)
        {
            return RuntimeConfig.GetConfig(vpath).Compilation;
        }
        return GetConfig<CompilationSection>(vpath);
    }

    internal static CompilationSection GetCompilationConfig(HttpContext context)
    {
        if (!UseMTConfig)
        {
            return RuntimeConfig.GetConfig(context).Compilation;
        }
        return GetConfig<CompilationSection>(context);
    }

    internal static CompilationSection GetCompilationConfig(VirtualPath vpath)
    {
        if (!UseMTConfig)
        {
            return RuntimeConfig.GetConfig(vpath).Compilation;
        }
        return GetConfig<CompilationSection>(vpath);
    }

    private static S GetConfig<S>() where S: ConfigurationSection
    {
        HttpContext current = HttpContext.Current;
        if (current != null)
        {
            return GetConfig<S>(current);
        }
        return GetAppConfig<S>();
    }

    private static S GetConfig<S>(string vpath) where S: ConfigurationSection
    {
        return GetConfig<S>(VirtualPath.CreateNonRelativeAllowNull(vpath));
    }

    private static S GetConfig<S>(HttpContext context) where S: ConfigurationSection
    {
        return GetConfig<S>(context.ConfigurationPath);
    }

    private static S GetConfig<S>(VirtualPath vpath) where S: ConfigurationSection
    {
        ConfigurationSection configHelper;
        Tuple<Type, VirtualPath> key = new Tuple<Type, VirtualPath>(typeof(S), vpath);
        if (!s_sections.TryGetValue(key, out configHelper))
        {
            configHelper = GetConfigHelper<S>(vpath);
            s_sections.TryAdd(key, configHelper);
        }
        return (configHelper as S);
    }

    private static S GetConfigHelper<S>(VirtualPath vpath) where S: ConfigurationSection
    {
        string physicalPath = null;
        if ((vpath == null) || !vpath.IsWithinAppRoot)
        {
            vpath = HostingEnvironment.ApplicationVirtualPathObject;
            physicalPath = HostingEnvironment.ApplicationPhysicalPath;
        }
        else
        {
            if (!vpath.DirectoryExists())
            {
                vpath = vpath.Parent;
            }
            physicalPath = HostingEnvironment.MapPath(vpath);
        }
        System.Configuration.Configuration configuration = GetConfiguration(vpath, physicalPath);
        if (typeof(S) == typeof(CompilationSection))
        {
            return (configuration.GetSection("system.web/compilation") as S);
        }
        if (typeof(S) == typeof(PagesSection))
        {
            return (configuration.GetSection("system.web/pages") as S);
        }
        if (typeof(S) != typeof(ProfileSection))
        {
            throw new InvalidOperationException(System.Web.SR.GetString("Config_section_not_supported", new object[] { typeof(S).FullName }));
        }
        return (configuration.GetSection("system.web/profile") as S);
    }

    private static System.Configuration.Configuration GetConfiguration(VirtualPath vpath, string physicalPath)
    {
        System.Configuration.Configuration configurationHelper;
        if (!s_configurations.TryGetValue(vpath, out configurationHelper))
        {
            configurationHelper = GetConfigurationHelper(vpath, physicalPath);
            s_configurations.TryAdd(vpath, configurationHelper);
        }
        return configurationHelper;
    }

    private static System.Configuration.Configuration GetConfigurationHelper(VirtualPath vpath, string physicalPath)
    {
        WebConfigurationFileMap fileMap = new WebConfigurationFileMap(MachineConfigPath);
        VirtualPath virtualPath = vpath;
        while ((virtualPath != null) && virtualPath.IsWithinAppRoot)
        {
            string virtualPathStringNoTrailingSlash = virtualPath.VirtualPathStringNoTrailingSlash;
            if (physicalPath == null)
            {
                physicalPath = HostingEnvironment.MapPath(virtualPath);
            }
            fileMap.VirtualDirectories.Add(virtualPathStringNoTrailingSlash, new VirtualDirectoryMapping(physicalPath, IsAppRoot(virtualPath)));
            virtualPath = virtualPath.Parent;
            physicalPath = null;
        }
        return WebConfigurationManager.OpenMappedWebConfiguration(fileMap, vpath.VirtualPathStringNoTrailingSlash, HostingEnvironment.SiteName);
    }

    internal static PagesSection GetPagesAppConfig()
    {
        if (!UseMTConfig)
        {
            return RuntimeConfig.GetAppConfig().Pages;
        }
        return GetAppConfig<PagesSection>();
    }

    internal static PagesSection GetPagesConfig()
    {
        if (!UseMTConfig)
        {
            return RuntimeConfig.GetConfig().Pages;
        }
        return GetConfig<PagesSection>();
    }

    internal static PagesSection GetPagesConfig(string vpath)
    {
        if (!UseMTConfig)
        {
            return RuntimeConfig.GetConfig(vpath).Pages;
        }
        return GetConfig<PagesSection>(vpath);
    }

    internal static PagesSection GetPagesConfig(HttpContext context)
    {
        if (!UseMTConfig)
        {
            return RuntimeConfig.GetConfig(context).Pages;
        }
        return GetConfig<PagesSection>(context);
    }

    internal static PagesSection GetPagesConfig(VirtualPath vpath)
    {
        if (!UseMTConfig)
        {
            return RuntimeConfig.GetConfig(vpath).Pages;
        }
        return GetConfig<PagesSection>(vpath);
    }

    internal static ProfileSection GetProfileAppConfig()
    {
        if (!UseMTConfig)
        {
            return RuntimeConfig.GetAppConfig().Profile;
        }
        return GetAppConfig<ProfileSection>();
    }

    private static bool IsAppRoot(VirtualPath path)
    {
        if (s_appVirtualPath == null)
        {
            s_appVirtualPath = VirtualPath.Create(HttpRuntime.AppDomainAppVirtualPathObject.VirtualPathStringNoTrailingSlash);
        }
        VirtualPath path2 = VirtualPath.Create(path.VirtualPathStringNoTrailingSlash);
        return s_appVirtualPath.Equals(path2);
    }

    private static string MachineConfigPath
    {
        get
        {
            if (s_machineConfigPath == null)
            {
                s_machineConfigPath = ToolLocationHelper.GetPathToDotNetFrameworkFile(@"config\machine.config", TargetDotNetFrameworkVersion.Version20);
                if (string.IsNullOrEmpty(s_machineConfigPath))
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("Downlevel_requires_35"));
                }
            }
            return s_machineConfigPath;
        }
    }

    private static bool UseMTConfig
    {
        get
        {
            if (!s_useMTConfig.HasValue)
            {
                s_useMTConfig = new bool?(BuildManagerHost.InClientBuildManager && (MultiTargetingUtil.IsTargetFramework20 || MultiTargetingUtil.IsTargetFramework35));
            }
            return s_useMTConfig.Value;
        }
    }
}

