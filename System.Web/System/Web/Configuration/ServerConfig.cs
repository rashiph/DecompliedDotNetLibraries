namespace System.Web.Configuration
{
    using Microsoft.Win32;
    using System;
    using System.Collections.Generic;
    using System.Security.Permissions;
    using System.Threading;
    using System.Web.Compilation;
    using System.Web.Hosting;

    internal static class ServerConfig
    {
        private static Dictionary<string, ExpressServerConfig> s_expressConfigs;
        private static object s_expressConfigsLock = new object();
        private static string s_iisExpressVersion;
        private static int s_iisMajorVersion = 0;
        private static int s_useServerConfig = -1;

        internal static IServerConfig GetDefaultDomainInstance(string version)
        {
            if (version == null)
            {
                return GetInstance();
            }
            ExpressServerConfig config = null;
            lock (s_expressConfigsLock)
            {
                if (s_expressConfigs == null)
                {
                    if (!Thread.GetDomain().IsDefaultAppDomain())
                    {
                        throw new InvalidOperationException();
                    }
                    s_expressConfigs = new Dictionary<string, ExpressServerConfig>(3);
                }
                if (!s_expressConfigs.TryGetValue(version, out config))
                {
                    config = new ExpressServerConfig(version);
                    s_expressConfigs[version] = config;
                }
            }
            return config;
        }

        internal static IServerConfig GetInstance()
        {
            if (UseMetabase)
            {
                return MetabaseServerConfig.GetInstance();
            }
            if (IISExpressVersion == null)
            {
                return ProcessHostServerConfig.GetInstance();
            }
            return ExpressServerConfig.GetInstance(IISExpressVersion);
        }

        internal static string IISExpressVersion
        {
            get
            {
                return s_iisExpressVersion;
            }
            set
            {
                if (Thread.GetDomain().IsDefaultAppDomain() || ((s_iisExpressVersion != null) && (s_iisExpressVersion != value)))
                {
                    throw new InvalidOperationException();
                }
                s_iisExpressVersion = value;
            }
        }

        internal static bool UseMetabase
        {
            [RegistryPermission(SecurityAction.Assert, Read=@"HKEY_LOCAL_MACHINE\Software\Microsoft\InetStp")]
            get
            {
                if (IISExpressVersion != null)
                {
                    return false;
                }
                if (s_iisMajorVersion == 0)
                {
                    int num;
                    try
                    {
                        object obj2 = Registry.GetValue(@"HKEY_LOCAL_MACHINE\Software\Microsoft\InetStp", "MajorVersion", 0);
                        num = (obj2 != null) ? ((int) obj2) : -1;
                    }
                    catch (ArgumentException)
                    {
                        num = -1;
                    }
                    Interlocked.CompareExchange(ref s_iisMajorVersion, num, 0);
                }
                return (s_iisMajorVersion <= 6);
            }
        }

        internal static bool UseServerConfig
        {
            get
            {
                if (s_useServerConfig == -1)
                {
                    int num = 0;
                    if (!HostingEnvironment.IsHosted)
                    {
                        num = 1;
                    }
                    else if (HostingEnvironment.ApplicationHostInternal is ISAPIApplicationHost)
                    {
                        num = 1;
                    }
                    else if (HostingEnvironment.IsUnderIISProcess && !BuildManagerHost.InClientBuildManager)
                    {
                        num = 1;
                    }
                    Interlocked.CompareExchange(ref s_useServerConfig, num, -1);
                }
                return (s_useServerConfig == 1);
            }
        }
    }
}

