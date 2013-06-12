namespace System.Web.Compilation
{
    using Microsoft.Build.Utilities;
    using Microsoft.CSharp;
    using Microsoft.VisualBasic;
    using Microsoft.Win32;
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Runtime.Versioning;
    using System.Security.Permissions;
    using System.Web;
    using System.Web.Configuration;

    internal class MultiTargetingUtil
    {
        internal static readonly FrameworkName FrameworkNameV20 = CreateFrameworkName(".NETFramework,Version=v2.0");
        internal static readonly FrameworkName FrameworkNameV30 = CreateFrameworkName(".NETFramework,Version=v3.0");
        internal static readonly FrameworkName FrameworkNameV35 = CreateFrameworkName(".NETFramework,Version=v3.5");
        internal static readonly FrameworkName FrameworkNameV40 = CreateFrameworkName(".NETFramework,Version=v4.0");
        private static string s_configTargetFrameworkAttributeName = "targetFramework";
        private static string s_configTargetFrameworkMoniker = null;
        private static object s_configTargetFrameworkMonikerLock = new object();
        private static bool s_initializedConfigTargetFrameworkMoniker = false;
        private static List<FrameworkName> s_knownFrameworkNames = null;
        private static FrameworkName s_latestFrameworkName = null;
        private static FrameworkName s_targetFrameworkName = null;
        private static object s_targetFrameworkNameLock = new object();
        internal static Version Version35 = new Version(3, 5);
        internal static Version Version40 = new Version(4, 0);

        internal static FrameworkName CreateFrameworkName(string name)
        {
            return new FrameworkName(name);
        }

        internal static void EnsureFrameworkNamesInitialized()
        {
            if (s_targetFrameworkName == null)
            {
                lock (s_targetFrameworkNameLock)
                {
                    if (s_targetFrameworkName == null)
                    {
                        InitializeKnownAndLatestFrameworkNames();
                        InitializeTargetFrameworkName();
                    }
                }
            }
        }

        private static string GetCompilerVersionFor20Or35()
        {
            string cSharpCompilerVersion = GetCSharpCompilerVersion();
            string visualBasicCompilerVersion = GetVisualBasicCompilerVersion();
            cSharpCompilerVersion = ReplaceCompilerVersionFor20Or35(cSharpCompilerVersion);
            visualBasicCompilerVersion = ReplaceCompilerVersionFor20Or35(visualBasicCompilerVersion);
            Version versionFromVString = CompilationUtil.GetVersionFromVString(cSharpCompilerVersion);
            Version version2 = CompilationUtil.GetVersionFromVString(visualBasicCompilerVersion);
            if (versionFromVString > version2)
            {
                return cSharpCompilerVersion;
            }
            return visualBasicCompilerVersion;
        }

        private static string GetCSharpCompilerVersion()
        {
            return CompilationUtil.GetCompilerVersion(typeof(CSharpCodeProvider));
        }

        private static Version GetFrameworkNameVersion(FrameworkName name)
        {
            if (name == null)
            {
                return null;
            }
            return name.Version;
        }

        [RegistryPermission(SecurityAction.Assert, Unrestricted=true)]
        private static Version GetInstalledTargetVersion(int majorVersion)
        {
            string keyName = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\NET Framework Setup\NDP\v" + majorVersion + @"\Full";
            try
            {
                string str2 = Registry.GetValue(keyName, "TargetVersion", null) as string;
                if (!string.IsNullOrEmpty(str2))
                {
                    return new Version(str2);
                }
            }
            catch
            {
            }
            return null;
        }

        private static Version GetVersion(string version)
        {
            if (!string.IsNullOrEmpty(version) && char.IsDigit(version[0]))
            {
                try
                {
                    return new Version(version);
                }
                catch
                {
                }
            }
            return null;
        }

        private static string GetVisualBasicCompilerVersion()
        {
            return CompilationUtil.GetCompilerVersion(typeof(VBCodeProvider));
        }

        private static void InitializeKnownAndLatestFrameworkNames()
        {
            IList<string> supportedTargetFrameworks = ToolLocationHelper.GetSupportedTargetFrameworks();
            Version version = null;
            s_knownFrameworkNames = new List<FrameworkName>();
            foreach (string str in supportedTargetFrameworks)
            {
                FrameworkName item = new FrameworkName(str);
                s_knownFrameworkNames.Add(item);
                Version frameworkNameVersion = GetFrameworkNameVersion(item);
                if ((s_latestFrameworkName == null) || (version < frameworkNameVersion))
                {
                    s_latestFrameworkName = item;
                    version = frameworkNameVersion;
                }
            }
        }

        private static void InitializeTargetFrameworkName()
        {
            string configTargetFrameworkMoniker = ConfigTargetFrameworkMoniker;
            if (!WebConfigExists)
            {
                s_targetFrameworkName = FrameworkNameV40;
                ValidateCompilerVersionFor40AndAbove();
            }
            else if (configTargetFrameworkMoniker == null)
            {
                if (BuildManagerHost.SupportsMultiTargeting)
                {
                    InitializeTargetFrameworkNameFor20Or35();
                }
                else
                {
                    s_targetFrameworkName = FrameworkNameV40;
                }
            }
            else
            {
                InitializeTargetFrameworkNameFor40AndAbove(configTargetFrameworkMoniker);
            }
        }

        private static void InitializeTargetFrameworkNameFor20Or35()
        {
            string compilerVersion = GetCompilerVersionFor20Or35();
            if (CompilationUtil.IsCompilerVersion35(compilerVersion))
            {
                s_targetFrameworkName = FrameworkNameV35;
            }
            else
            {
                if ((compilerVersion != "v2.0") && (compilerVersion != null))
                {
                    throw new ConfigurationErrorsException(System.Web.SR.GetString("Compiler_version_20_35_required", new object[] { s_configTargetFrameworkAttributeName }));
                }
                s_targetFrameworkName = FrameworkNameV30;
            }
        }

        private static void InitializeTargetFrameworkNameFor40AndAbove(string targetFrameworkMoniker)
        {
            ValidateTargetFrameworkMoniker(targetFrameworkMoniker);
            ValidateCompilerVersionFor40AndAbove();
        }

        private static string ReplaceCompilerVersionFor20Or35(string compilerVersion)
        {
            if (CompilationUtil.IsCompilerVersion35(compilerVersion))
            {
                return compilerVersion;
            }
            return "v2.0";
        }

        private static void ReportInvalidCompilerVersion(string compilerVersion)
        {
            throw new ConfigurationErrorsException(System.Web.SR.GetString("Invalid_attribute_value", new object[] { compilerVersion, "system.codedom/compilers/compiler/ProviderOption/CompilerVersion" }));
        }

        private static void ValidateCompilerVersionFor40AndAbove()
        {
            ValidateCompilerVersionFor40AndAbove(GetCSharpCompilerVersion());
            ValidateCompilerVersionFor40AndAbove(GetVisualBasicCompilerVersion());
        }

        private static void ValidateCompilerVersionFor40AndAbove(string compilerVersion)
        {
            if (compilerVersion != null)
            {
                Exception exception = null;
                if ((compilerVersion.Length < 4) || (compilerVersion[0] != 'v'))
                {
                    ReportInvalidCompilerVersion(compilerVersion);
                }
                try
                {
                    if (CompilationUtil.GetVersionFromVString(compilerVersion) < Version40)
                    {
                        throw new ConfigurationErrorsException(System.Web.SR.GetString("Compiler_version_40_required", new object[] { s_configTargetFrameworkAttributeName }));
                    }
                }
                catch (ArgumentNullException exception2)
                {
                    exception = exception2;
                }
                catch (ArgumentOutOfRangeException exception3)
                {
                    exception = exception3;
                }
                catch (ArgumentException exception4)
                {
                    exception = exception4;
                }
                catch (FormatException exception5)
                {
                    exception = exception5;
                }
                catch (OverflowException exception6)
                {
                    exception = exception6;
                }
                if (exception != null)
                {
                    ReportInvalidCompilerVersion(compilerVersion);
                }
            }
        }

        [RegistryPermission(SecurityAction.Assert, Unrestricted=true)]
        private static void ValidateTargetFrameworkMoniker(string targetFrameworkMoniker)
        {
            CompilationSection compilation = RuntimeConfig.GetAppConfig().Compilation;
            int lineNumber = compilation.ElementInformation.LineNumber;
            string source = compilation.ElementInformation.Source;
            try
            {
                string str2 = targetFrameworkMoniker;
                if (GetVersion(targetFrameworkMoniker) != null)
                {
                    str2 = ".NETFramework,Version=v" + str2;
                }
                s_targetFrameworkName = CreateFrameworkName(str2);
            }
            catch (ArgumentException exception)
            {
                throw new ConfigurationErrorsException(System.Web.SR.GetString("Invalid_target_framework_version", new object[] { s_configTargetFrameworkAttributeName, targetFrameworkMoniker, exception.Message }), source, lineNumber);
            }
            Version frameworkNameVersion = GetFrameworkNameVersion(s_targetFrameworkName);
            if (frameworkNameVersion < Version40)
            {
                throw new ConfigurationErrorsException(System.Web.SR.GetString("Invalid_lower_target_version", new object[] { s_configTargetFrameworkAttributeName }), source, lineNumber);
            }
            Version version3 = GetFrameworkNameVersion(LatestFrameworkName);
            if ((version3 == null) || (version3 < frameworkNameVersion))
            {
                Version installedTargetVersion = GetInstalledTargetVersion(frameworkNameVersion.Major);
                if ((installedTargetVersion == null) || (installedTargetVersion < frameworkNameVersion))
                {
                    try
                    {
                        FrameworkName name = new FrameworkName(s_targetFrameworkName.Identifier, s_targetFrameworkName.Version);
                        Version version = Environment.Version;
                        string str3 = string.Concat(new object[] { version.Major, ".", version.Minor, ".", version.Build });
                        string str4 = @"SOFTWARE\Microsoft\.NETFramework\v" + str3 + @"\SKUs";
                        foreach (string str5 in Registry.LocalMachine.OpenSubKey(str4).GetSubKeyNames())
                        {
                            try
                            {
                                FrameworkName name2 = CreateFrameworkName(str5);
                                FrameworkName name3 = new FrameworkName(name2.Identifier, name2.Version);
                                if (string.Equals(name.FullName, name3.FullName, StringComparison.OrdinalIgnoreCase))
                                {
                                    return;
                                }
                            }
                            catch
                            {
                            }
                        }
                    }
                    catch
                    {
                    }
                    throw new ConfigurationErrorsException(System.Web.SR.GetString("Invalid_higher_target_version", new object[] { s_configTargetFrameworkAttributeName }), source, lineNumber);
                }
            }
        }

        internal static string ConfigTargetFrameworkMoniker
        {
            get
            {
                if (!s_initializedConfigTargetFrameworkMoniker)
                {
                    lock (s_configTargetFrameworkMonikerLock)
                    {
                        if (!s_initializedConfigTargetFrameworkMoniker)
                        {
                            string targetFramework = RuntimeConfig.GetAppConfig().Compilation.TargetFramework;
                            if (targetFramework != null)
                            {
                                targetFramework = targetFramework.Trim();
                            }
                            s_configTargetFrameworkMoniker = targetFramework;
                            s_initializedConfigTargetFrameworkMoniker = true;
                        }
                    }
                }
                return s_configTargetFrameworkMoniker;
            }
        }

        internal static bool EnableReferenceAssemblyResolution
        {
            get
            {
                return BuildManagerHost.InClientBuildManager;
            }
        }

        internal static bool IsTargetFramework20
        {
            get
            {
                if (!object.Equals(TargetFrameworkName, FrameworkNameV20))
                {
                    return object.Equals(TargetFrameworkName, FrameworkNameV30);
                }
                return true;
            }
        }

        internal static bool IsTargetFramework35
        {
            get
            {
                return object.Equals(TargetFrameworkName, FrameworkNameV35);
            }
        }

        internal static bool IsTargetFramework40OrAbove
        {
            get
            {
                return (TargetFrameworkVersion.Major >= 4);
            }
        }

        internal static List<FrameworkName> KnownFrameworkNames
        {
            get
            {
                EnsureFrameworkNamesInitialized();
                return s_knownFrameworkNames;
            }
        }

        internal static FrameworkName LatestFrameworkName
        {
            get
            {
                EnsureFrameworkNamesInitialized();
                return s_latestFrameworkName;
            }
        }

        internal static FrameworkName TargetFrameworkName
        {
            get
            {
                EnsureFrameworkNamesInitialized();
                return s_targetFrameworkName;
            }
            set
            {
                s_targetFrameworkName = value;
            }
        }

        internal static Version TargetFrameworkVersion
        {
            get
            {
                return GetFrameworkNameVersion(TargetFrameworkName);
            }
        }

        private static bool WebConfigExists
        {
            get
            {
                VirtualPath appDomainAppVirtualPathObject = HttpRuntime.AppDomainAppVirtualPathObject;
                return ((appDomainAppVirtualPathObject != null) && File.Exists(appDomainAppVirtualPathObject.SimpleCombine("web.config").MapPath()));
            }
        }
    }
}

