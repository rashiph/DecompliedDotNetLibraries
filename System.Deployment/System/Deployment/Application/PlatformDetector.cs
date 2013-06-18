namespace System.Deployment.Application
{
    using Microsoft.Win32;
    using System;
    using System.ComponentModel;
    using System.Deployment.Application.Manifest;
    using System.Deployment.Application.Win32InterOp;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Text;

    internal static class PlatformDetector
    {
        private const int MAX_PATH = 260;
        private static Product[] Products = new Product[] { new Product("workstation", 1), new Product("domainController", 2), new Product("server", 3) };
        private const uint RUNTIME_INFO_CONSIDER_POST_2_0 = 0x80;
        private const uint RUNTIME_INFO_DONT_RETURN_DIRECTORY = 0x10;
        private const uint RUNTIME_INFO_DONT_RETURN_VERSION = 0x20;
        private const uint RUNTIME_INFO_DONT_SHOW_ERROR_DIALOG = 0x40;
        private const uint RUNTIME_INFO_EMULATE_EXE_LAUNCH = 0x100;
        private const uint RUNTIME_INFO_REQUEST_AMD64 = 4;
        private const uint RUNTIME_INFO_REQUEST_IA64 = 2;
        private const uint RUNTIME_INFO_REQUEST_X86 = 8;
        private const uint RUNTIME_INFO_UPGRADE_VERSION = 1;
        private static Suite[] Suites = new Suite[] { new Suite("server", 0x80000000), new Suite("workstation", 0x40000000), new Suite("smallbusiness", 1), new Suite("enterprise", 2), new Suite("backoffice", 4), new Suite("communications", 8), new Suite("terminal", 0x10), new Suite("smallbusinessRestricted", 0x20), new Suite("embeddednt", 0x40), new Suite("datacenter", 0x80), new Suite("singleuserts", 0x100), new Suite("personal", 0x200), new Suite("blade", 0x400), new Suite("embeddedrestricted", 0x800) };
        private const byte VER_AND = 6;
        private const uint VER_BUILDNUMBER = 4;
        private const byte VER_EQUAL = 1;
        private const byte VER_GREATER = 2;
        private const byte VER_GREATER_EQUAL = 3;
        private const byte VER_LESS = 4;
        private const byte VER_LESS_EQUAL = 5;
        private const uint VER_MAJORVERSION = 2;
        private const uint VER_MINORVERSION = 1;
        private const uint VER_NT_DOMAIN_CONTROLLER = 2;
        private const uint VER_NT_SERVER = 3;
        private const uint VER_NT_WORKSTATION = 1;
        private const byte VER_OR = 7;
        private const uint VER_PLATFORMID = 8;
        private const uint VER_PRODUCT_TYPE = 0x80;
        private const uint VER_SERVER_NT = 0x80000000;
        private const uint VER_SERVICEPACKMAJOR = 0x20;
        private const uint VER_SERVICEPACKMINOR = 0x10;
        private const uint VER_SUITE_BACKOFFICE = 4;
        private const uint VER_SUITE_BLADE = 0x400;
        private const uint VER_SUITE_COMMUNICATIONS = 8;
        private const uint VER_SUITE_DATACENTER = 0x80;
        private const uint VER_SUITE_EMBEDDED_RESTRICTED = 0x800;
        private const uint VER_SUITE_EMBEDDEDNT = 0x40;
        private const uint VER_SUITE_ENTERPRISE = 2;
        private const uint VER_SUITE_PERSONAL = 0x200;
        private const uint VER_SUITE_SINGLEUSERTS = 0x100;
        private const uint VER_SUITE_SMALLBUSINESS = 1;
        private const uint VER_SUITE_SMALLBUSINESS_RESTRICTED = 0x20;
        private const uint VER_SUITE_TERMINAL = 0x10;
        private const uint VER_SUITENAME = 0x40;
        private const uint VER_WORKSTATION_NT = 0x40000000;
        private const uint Windows9XMajorVersion = 4;

        private static string BuildTFM(string targetVersion, string profile)
        {
            if (string.IsNullOrEmpty(profile) || "Full".Equals(profile, StringComparison.OrdinalIgnoreCase))
            {
                return string.Format(".NETFramework,Version=v{0}", targetVersion);
            }
            return string.Format(".NETFramework,Version=v{0},Profile={1}", targetVersion, profile);
        }

        public static bool CheckCompatibleFramework(CompatibleFramework framework, ref Version clrVersion, ref string clrVersionString, string clrProcArch)
        {
            Logger.AddMethodCall("CheckCompatibleFramework called targetVersion:" + framework.TargetVersion + " profile:" + framework.Profile);
            Version versionRequired = new Version(framework.TargetVersion);
            string setupKeyPath = null;
            string str2 = null;
            string setupValueName = null;
            bool detectInstallValue = false;
            bool flag2 = false;
            if (versionRequired.Major < 4)
            {
                if (versionRequired.Major == 2)
                {
                    setupKeyPath = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v" + framework.TargetVersion;
                    setupValueName = "Install";
                    detectInstallValue = true;
                }
                else if (versionRequired.Minor == 0)
                {
                    setupKeyPath = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v3.0\Setup";
                    setupValueName = "InstallSuccess";
                    detectInstallValue = true;
                }
                else
                {
                    setupKeyPath = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v" + versionRequired.ToString(2);
                    setupValueName = "Version";
                    if ("Client".Equals(framework.Profile, StringComparison.OrdinalIgnoreCase))
                    {
                        str2 = @"SOFTWARE\Microsoft\NET Framework Setup\DotNetClient\v" + versionRequired.ToString(2);
                    }
                }
                flag2 = DetectFrameworkInRegistry(setupKeyPath, setupValueName, versionRequired, detectInstallValue) || ((str2 != null) && DetectFrameworkInRegistry(str2, setupValueName, versionRequired, detectInstallValue));
            }
            else
            {
                flag2 = DetectTFMInRegistry(framework.SupportedRuntime, framework.TargetVersion, framework.Profile);
            }
            if (!flag2)
            {
                return false;
            }
            Version v = new Version(framework.SupportedRuntime);
            if (!VerifyCLRVersionInfo(v, clrProcArch))
            {
                Logger.AddWarningInformation(string.Format(CultureInfo.CurrentUICulture, Resources.GetString("CLRMissingForFoundFramework"), new object[] { framework.SupportedRuntime, FormatFrameworkString(framework) }));
                return false;
            }
            clrVersionString = framework.SupportedRuntime;
            clrVersion = new Version(clrVersionString);
            return true;
        }

        public static bool DetectFrameworkInRegistry(string setupKeyPath, string setupValueName, Version versionRequired, bool detectInstallValue)
        {
            Logger.AddMethodCall("DetectFrameworkInRegistry(" + setupKeyPath + ", " + setupValueName + ", " + versionRequired.ToString() + ", " + detectInstallValue.ToString() + ") called");
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(setupKeyPath))
            {
                if (key != null)
                {
                    object obj2 = key.GetValue(setupValueName);
                    if (detectInstallValue)
                    {
                        if ((obj2 is int) && (((int) obj2) != 0))
                        {
                            return true;
                        }
                    }
                    else if (obj2 is string)
                    {
                        Version version = new Version((string) obj2);
                        if (version >= versionRequired)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public static bool DetectTFMInRegistry(string clrVersion, string frameworkVersion, string profile)
        {
            string regKey = (@"SOFTWARE\Microsoft\.NETFramework\v" + clrVersion + @"\SKUs\") + BuildTFM(frameworkVersion, profile);
            return DoesRegistryKeyExist(Registry.LocalMachine, regKey);
        }

        private static bool DoesRegistryKeyExist(RegistryKey regRoot, string regKey)
        {
            bool flag = false;
            using (RegistryKey key = regRoot.OpenSubKey(regKey, false))
            {
                if (key != null)
                {
                    flag = true;
                }
            }
            return flag;
        }

        public static string FormatFrameworkString(CompatibleFramework framework)
        {
            if (string.IsNullOrEmpty(framework.Profile))
            {
                return string.Format(CultureInfo.CurrentUICulture, Resources.GetString("FrameworkNameNoProfile"), new object[] { framework.TargetVersion });
            }
            return string.Format(CultureInfo.CurrentUICulture, Resources.GetString("FrameworkNameWithProfile"), new object[] { framework.TargetVersion, framework.Profile });
        }

        private static NetFX35SP1SKU GetPlatformNetFx35SKU(System.Deployment.Application.NativeMethods.IAssemblyCache AssemblyCache, bool targetOtherCLR, System.Deployment.Application.NativeMethods.CCorRuntimeHost RuntimeHost, string tempDir)
        {
            ReferenceIdentity refId = new ReferenceIdentity("Sentinel.v3.5Client, Version=3.5.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a,processorArchitecture=msil");
            ReferenceIdentity identity2 = new ReferenceIdentity("System.Data.Entity, Version=3.5.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089,processorArchitecture=msil");
            bool flag = false;
            bool flag2 = false;
            if (VerifyGACDependency(AssemblyCache, targetOtherCLR, RuntimeHost, refId, tempDir))
            {
                flag = true;
            }
            if (VerifyGACDependency(AssemblyCache, targetOtherCLR, RuntimeHost, identity2, tempDir))
            {
                flag2 = true;
            }
            if (flag && !flag2)
            {
                return NetFX35SP1SKU.Client35SP1;
            }
            if (flag && flag2)
            {
                return NetFX35SP1SKU.Full35SP1;
            }
            return NetFX35SP1SKU.No35SP1;
        }

        public static bool IsCLRDependencyText(string clrTextName)
        {
            if ((string.Compare(clrTextName, "Microsoft-Windows-CLRCoreComp", StringComparison.OrdinalIgnoreCase) != 0) && (string.Compare(clrTextName, "Microsoft.Windows.CommonLanguageRuntime", StringComparison.OrdinalIgnoreCase) != 0))
            {
                return false;
            }
            return true;
        }

        private static bool IsNetFX35SP1ClientSignatureAsm(ReferenceIdentity ra)
        {
            DefinitionIdentity identity = new DefinitionIdentity("Sentinel.v3.5Client, Version=3.5.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a,processorArchitecture=msil");
            return identity.Matches(ra, true);
        }

        private static bool IsNetFX35SP1FullSignatureAsm(ReferenceIdentity ra)
        {
            DefinitionIdentity identity = new DefinitionIdentity("System.Data.Entity, Version=3.5.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089,processorArchitecture=msil");
            return identity.Matches(ra, true);
        }

        public static bool IsSupportedProcessorArchitecture(string arch)
        {
            if ((string.Compare(arch, "msil", StringComparison.OrdinalIgnoreCase) == 0) || (string.Compare(arch, "x86", StringComparison.OrdinalIgnoreCase) == 0))
            {
                return true;
            }
            System.Deployment.Application.NativeMethods.SYSTEM_INFO sysInfo = new System.Deployment.Application.NativeMethods.SYSTEM_INFO();
            bool flag = false;
            try
            {
                System.Deployment.Application.NativeMethods.GetNativeSystemInfo(ref sysInfo);
                flag = true;
            }
            catch (EntryPointNotFoundException)
            {
                Logger.AddInternalState("In IsSupportedProcessorArchitecture: GetNativeSystemInfo API from kernel32.dll is not found.");
                flag = false;
            }
            if (!flag)
            {
                System.Deployment.Application.NativeMethods.GetSystemInfo(ref sysInfo);
                Logger.AddInternalState("In IsSupportedProcessorArchitecture: GetSystemInfo called.");
            }
            switch (sysInfo.uProcessorInfo.wProcessorArchitecture)
            {
                case 6:
                    return (string.Compare(arch, "ia64", StringComparison.OrdinalIgnoreCase) == 0);

                case 9:
                    return (string.Compare(arch, "amd64", StringComparison.OrdinalIgnoreCase) == 0);
            }
            return false;
        }

        public static bool VerifyCLRVersionInfo(Version v, string procArch)
        {
            bool flag = true;
            NameMap[] nmArray = new NameMap[] { new NameMap("x86", 8), new Product("ia64", 2), new Product("amd64", 4) };
            uint runtimeInfoFlags = NameMap.MapNameToMask(procArch, nmArray) | 0x1c1;
            StringBuilder pDirectory = new StringBuilder(260);
            StringBuilder pVersion = new StringBuilder("v65535.65535.65535".Length);
            uint dwDirectoryLength = 0;
            uint dwLength = 0;
            string pwszVersion = v.ToString(3);
            pwszVersion = "v" + pwszVersion;
            try
            {
                System.Deployment.Application.NativeMethods.GetRequestedRuntimeInfo(null, pwszVersion, null, 0, runtimeInfoFlags, pDirectory, (uint) pDirectory.Capacity, out dwDirectoryLength, pVersion, (uint) pVersion.Capacity, out dwLength);
            }
            catch (COMException exception)
            {
                flag = false;
                if (exception.ErrorCode != -2146232576)
                {
                    throw;
                }
            }
            return flag;
        }

        public static bool VerifyGACDependency(System.Deployment.Application.NativeMethods.IAssemblyCache AssemblyCache, bool targetOtherClr, System.Deployment.Application.NativeMethods.CCorRuntimeHost RuntimeHost, ReferenceIdentity refId, string tempDir)
        {
            if (string.Compare(refId.ProcessorArchitecture, "msil", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return VerifyGACDependencyWhidbey(AssemblyCache, targetOtherClr, RuntimeHost, refId);
            }
            if (!VerifyGACDependencyXP(refId, tempDir))
            {
                return VerifyGACDependencyWhidbey(AssemblyCache, targetOtherClr, RuntimeHost, refId);
            }
            return true;
        }

        public static bool VerifyGACDependencyWhidbey(System.Deployment.Application.NativeMethods.IAssemblyCache AssemblyCache, bool targetOtherClr, System.Deployment.Application.NativeMethods.CCorRuntimeHost RuntimeHost, ReferenceIdentity refId)
        {
            ReferenceIdentity identity;
            System.Deployment.Application.NativeMethods.IAssemblyName name;
            System.Deployment.Application.NativeMethods.IAssemblyEnum enum2;
            string str = refId.ToString();
            string text = null;
            if (targetOtherClr)
            {
                try
                {
                    text = RuntimeHost.ApplyPolicyInOtherRuntime(str);
                    goto Label_0048;
                }
                catch (ArgumentException)
                {
                    return false;
                }
                catch (COMException)
                {
                    return false;
                }
            }
            try
            {
                text = AppDomain.CurrentDomain.ApplyPolicy(str);
            }
            catch (ArgumentException)
            {
                return false;
            }
            catch (COMException)
            {
                return false;
            }
        Label_0048:
            identity = new ReferenceIdentity(text);
            identity.ProcessorArchitecture = refId.ProcessorArchitecture;
            string assemblyName = identity.ToString();
            Logger.AddPhaseInformation(Resources.GetString("DetectingDependentAssembly"), new object[] { str, assemblyName });
            SystemUtils.AssemblyInfo info = null;
            info = SystemUtils.QueryAssemblyInfo(AssemblyCache, SystemUtils.QueryAssemblyInfoFlags.All, assemblyName);
            if ((info != null) || (identity.ProcessorArchitecture != null))
            {
                return (info != null);
            }
            System.Deployment.Application.NativeMethods.CreateAssemblyNameObject(out name, identity.ToString(), 1, IntPtr.Zero);
            System.Deployment.Application.NativeMethods.CreateAssemblyEnum(out enum2, null, name, 2, IntPtr.Zero);
            return (enum2.GetNextAssembly(null, out name, 0) == 0);
        }

        public static bool VerifyGACDependencyXP(ReferenceIdentity refId, string tempDir)
        {
            if (!PlatformSpecific.OnXPOrAbove)
            {
                return false;
            }
            using (TempFile file = new TempFile(tempDir, ".manifest"))
            {
                ManifestGenerator.GenerateGACDetectionManifest(refId, file.Path);
                System.Deployment.Application.NativeMethods.ACTCTXW actCtx = new System.Deployment.Application.NativeMethods.ACTCTXW(file.Path);
                IntPtr hActCtx = System.Deployment.Application.NativeMethods.CreateActCtxW(actCtx);
                if (hActCtx != System.Deployment.Application.NativeMethods.INVALID_HANDLE_VALUE)
                {
                    System.Deployment.Application.NativeMethods.ReleaseActCtx(hActCtx);
                    return true;
                }
                return false;
            }
        }

        public static bool VerifyOSDependency(ref OSDependency osd)
        {
            System.Deployment.Application.NativeMethods.OSVersionInfoEx ex;
            OperatingSystem oSVersion = Environment.OSVersion;
            if (oSVersion.Version.Major == 4L)
            {
                if (oSVersion.Version.Major < osd.dwMajorVersion)
                {
                    return false;
                }
                return true;
            }
            ex = new System.Deployment.Application.NativeMethods.OSVersionInfoEx {
                dwOSVersionInfoSize = Marshal.SizeOf(ex),
                dwMajorVersion = osd.dwMajorVersion,
                dwMinorVersion = osd.dwMinorVersion,
                dwBuildNumber = osd.dwBuildNumber,
                dwPlatformId = 0,
                szCSDVersion = null,
                wServicePackMajor = osd.wServicePackMajor,
                wServicePackMinor = osd.wServicePackMinor,
                wSuiteMask = (osd.suiteName != null) ? ((ushort) NameMap.MapNameToMask(osd.suiteName, Suites)) : ((ushort) 0),
                bProductType = (osd.productName != null) ? ((byte) NameMap.MapNameToMask(osd.productName, Products)) : ((byte) 0),
                bReserved = 0
            };
            ulong conditionMask = 0L;
            uint dwTypeMask = (uint) ((((((2 | ((osd.dwMinorVersion != 0) ? 1 : 0)) | ((osd.dwBuildNumber != 0) ? 4 : 0)) | ((osd.suiteName != null) ? 0x40 : 0)) | ((osd.productName != null) ? 0x80 : 0)) | ((osd.wServicePackMajor != 0) ? 0x20 : 0)) | ((osd.wServicePackMinor != 0) ? 0x10 : 0));
            conditionMask = System.Deployment.Application.NativeMethods.VerSetConditionMask(conditionMask, 2, 3);
            if (osd.dwMinorVersion != 0)
            {
                conditionMask = System.Deployment.Application.NativeMethods.VerSetConditionMask(conditionMask, 1, 3);
            }
            if (osd.dwBuildNumber != 0)
            {
                conditionMask = System.Deployment.Application.NativeMethods.VerSetConditionMask(conditionMask, 4, 3);
            }
            if (osd.suiteName != null)
            {
                conditionMask = System.Deployment.Application.NativeMethods.VerSetConditionMask(conditionMask, 0x40, 6);
            }
            if (osd.productName != null)
            {
                conditionMask = System.Deployment.Application.NativeMethods.VerSetConditionMask(conditionMask, 0x80, 1);
            }
            if (osd.wServicePackMajor != 0)
            {
                conditionMask = System.Deployment.Application.NativeMethods.VerSetConditionMask(conditionMask, 0x20, 3);
            }
            if (osd.wServicePackMinor != 0)
            {
                conditionMask = System.Deployment.Application.NativeMethods.VerSetConditionMask(conditionMask, 0x10, 3);
            }
            bool flag = System.Deployment.Application.NativeMethods.VerifyVersionInfo(ex, dwTypeMask, conditionMask);
            if (!flag)
            {
                int error = Marshal.GetLastWin32Error();
                if (error != 0x47e)
                {
                    throw new Win32Exception(error);
                }
            }
            return flag;
        }

        public static void VerifyPlatformDependencies(AssemblyManifest appManifest, AssemblyManifest deployManifest, string tempDir)
        {
            Logger.AddMethodCall("VerifyPlatformDependencies called.");
            string description = null;
            Uri supportUri = deployManifest.Description.SupportUri;
            DependentOS dependentOS = appManifest.DependentOS;
            if (dependentOS != null)
            {
                OSDependency osd = new OSDependency(dependentOS.MajorVersion, dependentOS.MinorVersion, dependentOS.BuildNumber, dependentOS.ServicePackMajor, dependentOS.ServicePackMinor, null, null);
                if (!VerifyOSDependency(ref osd))
                {
                    StringBuilder builder = new StringBuilder();
                    string str2 = string.Concat(new object[] { dependentOS.MajorVersion, ".", dependentOS.MinorVersion, ".", dependentOS.BuildNumber, ".", dependentOS.ServicePackMajor, dependentOS.ServicePackMinor });
                    builder.AppendFormat(Resources.GetString("PlatformMicrosoftWindowsOperatingSystem"), str2);
                    description = builder.ToString();
                    if (dependentOS.SupportUrl != null)
                    {
                        supportUri = dependentOS.SupportUrl;
                    }
                    throw new DependentPlatformMissingException(string.Format(CultureInfo.CurrentUICulture, Resources.GetString("ErrorMessage_PlatformDetectionFailed"), new object[] { description }), supportUri);
                }
            }
            Version v = Constants.V2CLRVersion;
            string str3 = v.ToString(3);
            string processorArchitecture = appManifest.Identity.ProcessorArchitecture;
            Uri supportUrl = supportUri;
            if (appManifest.CLRDependentAssembly != null)
            {
                v = appManifest.CLRDependentAssembly.Identity.Version;
                str3 = v.ToString(3);
                processorArchitecture = appManifest.CLRDependentAssembly.Identity.ProcessorArchitecture;
                if (appManifest.CLRDependentAssembly.SupportUrl != null)
                {
                    supportUrl = appManifest.CLRDependentAssembly.SupportUrl;
                }
                if (appManifest.CLRDependentAssembly.Description != null)
                {
                    description = appManifest.CLRDependentAssembly.Description;
                }
            }
            if (deployManifest.CompatibleFrameworks == null)
            {
                if (v >= Constants.V4CLRVersion)
                {
                    throw new InvalidDeploymentException(ExceptionTypes.ManifestSemanticValidation, Resources.GetString("Ex_SemanticallyInvalidDeploymentManifest"), new InvalidDeploymentException(ExceptionTypes.InvalidManifest, Resources.GetString("Ex_DepMissingCompatibleFrameworks")));
                }
                if (!VerifyCLRVersionInfo(v, processorArchitecture))
                {
                    StringBuilder builder2 = new StringBuilder();
                    if (description == null)
                    {
                        builder2.AppendFormat(Resources.GetString("PlatformMicrosoftCommonLanguageRuntime"), str3);
                        description = builder2.ToString();
                    }
                    supportUri = supportUrl;
                    throw new SupportedRuntimeMissingException(string.Format(CultureInfo.CurrentUICulture, Resources.GetString("ErrorMessage_PlatformDetectionFailed"), new object[] { description }), supportUri, str3);
                }
            }
            else
            {
                bool flag = false;
                for (int i = 0; i < deployManifest.CompatibleFrameworks.Frameworks.Count; i++)
                {
                    if (CheckCompatibleFramework(deployManifest.CompatibleFrameworks.Frameworks[i], ref v, ref str3, processorArchitecture))
                    {
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                {
                    if (deployManifest.CompatibleFrameworks.SupportUrl != null)
                    {
                        supportUri = deployManifest.CompatibleFrameworks.SupportUrl;
                    }
                    else
                    {
                        supportUri = supportUrl;
                    }
                    throw new CompatibleFrameworkMissingException(string.Format(CultureInfo.CurrentUICulture, Resources.GetString("ErrorMessage_CompatiblePlatformDetectionFailed"), new object[] { FormatFrameworkString(deployManifest.CompatibleFrameworks.Frameworks[0]) }), supportUri, deployManifest.CompatibleFrameworks);
                }
            }
            Logger.AddPhaseInformation(Resources.GetString("CompatibleRuntimeFound"), new object[] { str3 });
            bool fetchRuntimeHost = false;
            if (v < Constants.V4CLRVersion)
            {
                fetchRuntimeHost = true;
            }
            using (System.Deployment.Application.NativeMethods.CCorRuntimeHost host = null)
            {
                System.Deployment.Application.NativeMethods.IAssemblyCache assemblyCache = System.Deployment.Application.NativeMethods.GetAssemblyCacheInterface(str3, fetchRuntimeHost, out host);
                if ((assemblyCache == null) || (fetchRuntimeHost && (host == null)))
                {
                    StringBuilder builder3 = new StringBuilder();
                    builder3.AppendFormat(Resources.GetString("PlatformMicrosoftCommonLanguageRuntime"), str3);
                    description = builder3.ToString();
                    supportUri = supportUrl;
                    throw new DependentPlatformMissingException(string.Format(CultureInfo.CurrentUICulture, Resources.GetString("ErrorMessage_PlatformDetectionFailed"), new object[] { description }), supportUri);
                }
                bool flag3 = false;
                bool flag4 = false;
                if (fetchRuntimeHost && !PolicyKeys.SkipSKUDetection())
                {
                    foreach (DependentAssembly assembly in appManifest.DependentAssemblies)
                    {
                        if (assembly.IsPreRequisite && IsNetFX35SP1ClientSignatureAsm(assembly.Identity))
                        {
                            flag3 = true;
                        }
                        if (assembly.IsPreRequisite && IsNetFX35SP1FullSignatureAsm(assembly.Identity))
                        {
                            flag4 = true;
                        }
                    }
                    if (((GetPlatformNetFx35SKU(assemblyCache, fetchRuntimeHost, host, tempDir) == NetFX35SP1SKU.Client35SP1) && !flag3) && !flag4)
                    {
                        description = ".NET Framework 3.5 SP1";
                        throw new DependentPlatformMissingException(string.Format(CultureInfo.CurrentUICulture, Resources.GetString("ErrorMessage_PlatformDetectionFailed"), new object[] { description }));
                    }
                }
                foreach (DependentAssembly assembly2 in appManifest.DependentAssemblies)
                {
                    if (assembly2.IsPreRequisite && !IsCLRDependencyText(assembly2.Identity.Name))
                    {
                        if (!fetchRuntimeHost && ((IsNetFX35SP1ClientSignatureAsm(assembly2.Identity) || IsNetFX35SP1FullSignatureAsm(assembly2.Identity)) || "framework".Equals(assembly2.Group, StringComparison.OrdinalIgnoreCase)))
                        {
                            Logger.AddPhaseInformation(Resources.GetString("SkippingSentinalDependentAssembly"), new object[] { assembly2.Identity.ToString() });
                        }
                        else if (!VerifyGACDependency(assemblyCache, fetchRuntimeHost, host, assembly2.Identity, tempDir))
                        {
                            if (assembly2.Description != null)
                            {
                                description = assembly2.Description;
                            }
                            else
                            {
                                ReferenceIdentity identity = assembly2.Identity;
                                StringBuilder builder4 = new StringBuilder();
                                builder4.AppendFormat(Resources.GetString("PlatformDependentAssemblyVersion"), identity.Name, identity.Version);
                                description = builder4.ToString();
                            }
                            if (assembly2.SupportUrl != null)
                            {
                                supportUri = assembly2.SupportUrl;
                            }
                            throw new DependentPlatformMissingException(string.Format(CultureInfo.CurrentUICulture, Resources.GetString("ErrorMessage_PlatformGACDetectionFailed"), new object[] { description }), supportUri);
                        }
                    }
                }
            }
        }

        public class NameMap
        {
            public uint mask;
            public string name;

            public NameMap(string Name, uint Mask)
            {
                this.name = Name;
                this.mask = Mask;
            }

            public static string MapMaskToName(uint mask, PlatformDetector.NameMap[] nmArray)
            {
                foreach (PlatformDetector.NameMap map in nmArray)
                {
                    if (map.mask == mask)
                    {
                        return map.name;
                    }
                }
                return null;
            }

            public static uint MapNameToMask(string name, PlatformDetector.NameMap[] nmArray)
            {
                foreach (PlatformDetector.NameMap map in nmArray)
                {
                    if (map.name == name)
                    {
                        return map.mask;
                    }
                }
                return 0;
            }
        }

        private enum NetFX35SP1SKU
        {
            No35SP1,
            Client35SP1,
            Full35SP1
        }

        public class OSDependency
        {
            public uint dwBuildNumber;
            public uint dwMajorVersion;
            public uint dwMinorVersion;
            public string productName;
            public string suiteName;
            public ushort wServicePackMajor;
            public ushort wServicePackMinor;

            public OSDependency()
            {
            }

            public OSDependency(System.Deployment.Application.NativeMethods.OSVersionInfoEx osvi)
            {
                this.dwMajorVersion = osvi.dwMajorVersion;
                this.dwMinorVersion = osvi.dwMinorVersion;
                this.dwMajorVersion = osvi.dwBuildNumber;
                this.dwMajorVersion = osvi.wServicePackMajor;
                this.dwMajorVersion = osvi.wServicePackMinor;
                this.suiteName = PlatformDetector.NameMap.MapMaskToName(osvi.wSuiteMask, PlatformDetector.Suites);
                this.productName = PlatformDetector.NameMap.MapMaskToName(osvi.bProductType, PlatformDetector.Products);
            }

            public OSDependency(uint dwMajorVersion, uint dwMinorVersion, uint dwBuildNumber, ushort wServicePackMajor, ushort wServicePackMinor, string suiteName, string productName)
            {
                this.dwMajorVersion = dwMajorVersion;
                this.dwMinorVersion = dwMinorVersion;
                this.dwBuildNumber = dwBuildNumber;
                this.wServicePackMajor = wServicePackMajor;
                this.wServicePackMinor = wServicePackMinor;
                this.suiteName = suiteName;
                this.productName = productName;
            }
        }

        public class Product : PlatformDetector.NameMap
        {
            public Product(string Name, uint Mask) : base(Name, Mask)
            {
            }
        }

        public class Suite : PlatformDetector.NameMap
        {
            public Suite(string Name, uint Mask) : base(Name, Mask)
            {
            }
        }
    }
}

