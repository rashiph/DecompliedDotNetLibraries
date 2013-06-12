namespace System.Diagnostics
{
    using System;
    using System.Configuration;
    using System.Threading;

    internal static class DiagnosticsConfiguration
    {
        private static System.Diagnostics.SystemDiagnosticsSection configSection;
        private static System.Diagnostics.InitState initState = System.Diagnostics.InitState.NotInitialized;

        internal static bool CanInitialize()
        {
            return ((initState != System.Diagnostics.InitState.Initializing) && !ConfigurationManagerInternalFactory.Instance.SetConfigurationSystemInProgress);
        }

        private static System.Diagnostics.SystemDiagnosticsSection GetConfigSection()
        {
            return (System.Diagnostics.SystemDiagnosticsSection) System.Configuration.PrivilegedConfigurationManager.GetSection("system.diagnostics");
        }

        internal static void Initialize()
        {
            lock (TraceInternal.critSec)
            {
                if ((initState == System.Diagnostics.InitState.NotInitialized) && !ConfigurationManagerInternalFactory.Instance.SetConfigurationSystemInProgress)
                {
                    initState = System.Diagnostics.InitState.Initializing;
                    try
                    {
                        configSection = GetConfigSection();
                    }
                    finally
                    {
                        initState = System.Diagnostics.InitState.Initialized;
                    }
                }
            }
        }

        internal static bool IsInitialized()
        {
            return (initState == System.Diagnostics.InitState.Initialized);
        }

        internal static bool IsInitializing()
        {
            return (initState == System.Diagnostics.InitState.Initializing);
        }

        internal static void Refresh()
        {
            ConfigurationManager.RefreshSection("system.diagnostics");
            System.Diagnostics.SystemDiagnosticsSection configSection = DiagnosticsConfiguration.configSection;
            if (configSection != null)
            {
                if (configSection.Switches != null)
                {
                    foreach (SwitchElement element in configSection.Switches)
                    {
                        element.ResetProperties();
                    }
                }
                if (configSection.SharedListeners != null)
                {
                    foreach (ListenerElement element2 in configSection.SharedListeners)
                    {
                        element2.ResetProperties();
                    }
                }
                if (configSection.Sources != null)
                {
                    foreach (SourceElement element3 in configSection.Sources)
                    {
                        element3.ResetProperties();
                    }
                }
            }
            DiagnosticsConfiguration.configSection = null;
            initState = System.Diagnostics.InitState.NotInitialized;
            Initialize();
        }

        internal static bool AssertUIEnabled
        {
            get
            {
                Initialize();
                System.Diagnostics.SystemDiagnosticsSection configSection = DiagnosticsConfiguration.configSection;
                if ((configSection != null) && (configSection.Assert != null))
                {
                    return configSection.Assert.AssertUIEnabled;
                }
                return true;
            }
        }

        internal static bool AutoFlush
        {
            get
            {
                Initialize();
                System.Diagnostics.SystemDiagnosticsSection configSection = DiagnosticsConfiguration.configSection;
                return (((configSection != null) && (configSection.Trace != null)) && configSection.Trace.AutoFlush);
            }
        }

        internal static string ConfigFilePath
        {
            get
            {
                Initialize();
                System.Diagnostics.SystemDiagnosticsSection configSection = DiagnosticsConfiguration.configSection;
                if (configSection != null)
                {
                    return configSection.ElementInformation.Source;
                }
                return string.Empty;
            }
        }

        internal static int IndentSize
        {
            get
            {
                Initialize();
                System.Diagnostics.SystemDiagnosticsSection configSection = DiagnosticsConfiguration.configSection;
                if ((configSection != null) && (configSection.Trace != null))
                {
                    return configSection.Trace.IndentSize;
                }
                return 4;
            }
        }

        internal static string LogFileName
        {
            get
            {
                Initialize();
                System.Diagnostics.SystemDiagnosticsSection configSection = DiagnosticsConfiguration.configSection;
                if ((configSection != null) && (configSection.Assert != null))
                {
                    return configSection.Assert.LogFileName;
                }
                return string.Empty;
            }
        }

        internal static int PerfomanceCountersFileMappingSize
        {
            get
            {
                for (int i = 0; !CanInitialize() && (i <= 5); i++)
                {
                    if (i == 5)
                    {
                        return 0x80000;
                    }
                    Thread.Sleep(200);
                }
                Initialize();
                System.Diagnostics.SystemDiagnosticsSection configSection = DiagnosticsConfiguration.configSection;
                if ((configSection == null) || (configSection.PerfCounters == null))
                {
                    return 0x80000;
                }
                int fileMappingSize = configSection.PerfCounters.FileMappingSize;
                if (fileMappingSize < 0x8000)
                {
                    fileMappingSize = 0x8000;
                }
                if (fileMappingSize > 0x2000000)
                {
                    fileMappingSize = 0x2000000;
                }
                return fileMappingSize;
            }
        }

        internal static ListenerElementsCollection SharedListeners
        {
            get
            {
                Initialize();
                System.Diagnostics.SystemDiagnosticsSection configSection = DiagnosticsConfiguration.configSection;
                if (configSection != null)
                {
                    return configSection.SharedListeners;
                }
                return null;
            }
        }

        internal static SourceElementsCollection Sources
        {
            get
            {
                Initialize();
                System.Diagnostics.SystemDiagnosticsSection configSection = DiagnosticsConfiguration.configSection;
                if ((configSection != null) && (configSection.Sources != null))
                {
                    return configSection.Sources;
                }
                return null;
            }
        }

        internal static SwitchElementsCollection SwitchSettings
        {
            get
            {
                Initialize();
                System.Diagnostics.SystemDiagnosticsSection configSection = DiagnosticsConfiguration.configSection;
                if (configSection != null)
                {
                    return configSection.Switches;
                }
                return null;
            }
        }

        internal static System.Diagnostics.SystemDiagnosticsSection SystemDiagnosticsSection
        {
            get
            {
                Initialize();
                return configSection;
            }
        }

        internal static bool UseGlobalLock
        {
            get
            {
                Initialize();
                System.Diagnostics.SystemDiagnosticsSection configSection = DiagnosticsConfiguration.configSection;
                if ((configSection != null) && (configSection.Trace != null))
                {
                    return configSection.Trace.UseGlobalLock;
                }
                return true;
            }
        }
    }
}

