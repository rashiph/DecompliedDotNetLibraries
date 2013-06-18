namespace Microsoft.Build.Shared
{
    using Microsoft.Win32;
    using System;
    using System.IO;
    using System.Runtime;
    using System.Text;

    internal static class FrameworkLocationHelper
    {
        private static readonly DirectoryExists directoryExists = new DirectoryExists(Directory.Exists);
        private const string dotNetFrameworkAssemblyFoldersRegistryKeyV30 = @"SOFTWARE\Microsoft\.NETFramework\AssemblyFolders\v3.0";
        private const string dotNetFrameworkAssemblyFoldersRegistryKeyV35 = @"SOFTWARE\Microsoft\.NETFramework\AssemblyFolders\v3.5";
        private const string dotNetFrameworkAssemblyFoldersRegistryPath = @"SOFTWARE\Microsoft\.NETFramework\AssemblyFolders";
        private const string dotNetFrameworkRegistryKeyV11 = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v1.1.4322";
        private const string dotNetFrameworkRegistryKeyV20 = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v2.0.50727";
        private const string dotNetFrameworkRegistryKeyV30 = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v3.0\Setup";
        private const string dotNetFrameworkRegistryKeyV35 = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v3.5";
        private const string dotNetFrameworkRegistryKeyV40 = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full";
        private const string dotNetFrameworkRegistryPath = @"SOFTWARE\Microsoft\.NETFramework";
        internal const string dotNetFrameworkSdkInstallKeyValueV11 = "SDKInstallRootv1.1";
        internal const string dotNetFrameworkSdkInstallKeyValueV20 = "SDKInstallRootv2.0";
        internal const string dotNetFrameworkSdkInstallKeyValueV35 = "InstallationFolder";
        internal const string dotNetFrameworkSdkInstallKeyValueV40 = "InstallationFolder";
        private const string dotNetFrameworkSdkRegistryPathV35 = @"SOFTWARE\Microsoft\Microsoft SDKs\Windows\v7.0A";
        private const string dotNetFrameworkSdkRegistryPathV40 = @"SOFTWARE\Microsoft\Microsoft SDKs\Windows\v7.0A";
        private const string dotNetFrameworkSetupRegistryInstalledName = "Install";
        private const string dotNetFrameworkSetupRegistryPath = @"SOFTWARE\Microsoft\NET Framework Setup\NDP";
        internal const string dotNetFrameworkVersionFolderPrefixV11 = "v1.1";
        internal const string dotNetFrameworkVersionFolderPrefixV20 = "v2.0";
        internal const string dotNetFrameworkVersionFolderPrefixV30 = "v3.0";
        internal const string dotNetFrameworkVersionFolderPrefixV35 = "v3.5";
        internal const string dotNetFrameworkVersionFolderPrefixV40 = "v4.0";
        private const string dotNetFrameworkVersionV11 = "v1.1.4322";
        private const string dotNetFrameworkVersionV20 = "v2.0.50727";
        private const string dotNetFrameworkVersionV30 = "v3.0";
        private const string dotNetFrameworkVersionV40 = "v4.0";
        internal const string fullDotNetFrameworkRegistryKey = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\.NETFramework";
        internal const string fullDotNetFrameworkSdkRegistryKeyV35 = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SDKs\Windows\v7.0A";
        internal const string fullDotNetFrameworkSdkRegistryKeyV40 = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SDKs\Windows\v7.0A";
        private static readonly GetDirectories getDirectories = new GetDirectories(Directory.GetDirectories);
        private static string pathToCurrentDotNetFrameworkV11;
        private static string pathToCurrentDotNetFrameworkV20;
        private static string pathToCurrentDotNetFrameworkV30;
        private static string pathToCurrentDotNetFrameworkV35;
        private static string pathToCurrentDotNetFrameworkV40;
        private static string pathToDotNetFramework32V11;
        private static string pathToDotNetFramework32V20;
        private static string pathToDotNetFramework32V30;
        private static string pathToDotNetFramework32V35;
        private static string pathToDotNetFramework32V40;
        private static string pathToDotNetFramework64V11;
        private static string pathToDotNetFramework64V20;
        private static string pathToDotNetFramework64V30;
        private static string pathToDotNetFramework64V35;
        private static string pathToDotNetFramework64V40;
        private static string pathToDotNetFrameworkReferenceAssembliesV30;
        private static string pathToDotNetFrameworkReferenceAssembliesV35;
        private static string pathToDotNetFrameworkSdkV11;
        private static string pathToDotNetFrameworkSdkV20;
        private static string pathToDotNetFrameworkSdkV35;
        private static string pathToDotNetFrameworkSdkV40;
        internal static readonly string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        internal static readonly string programFiles32 = GenerateProgramFiles32();
        internal static readonly string programFiles64 = GenerateProgramFiles64();
        internal static readonly string programFilesReferenceAssemblyLocation = GenerateProgramFilesReferenceAssemblyRoot();
        private const string referenceAssembliesRegistryValueName = "All Assemblies In";
        internal const string secondaryDotNetFrameworkSdkInstallKeyValueV35 = "CurrentInstallFolder";
        internal const string secondaryDotNetFrameworkSdkInstallKeyValueV40 = "CurrentInstallFolder";
        private const string secondaryDotNetFrameworkSdkRegistryPathV35 = @"SOFTWARE\Microsoft\Microsoft SDKs\Windows";
        private const string secondaryDotNetFrameworkSdkRegistryPathV40 = @"SOFTWARE\Microsoft\Microsoft SDKs\Windows";

        internal static bool CheckForFrameworkInstallation(string registryEntryToCheckInstall, string registryValueToCheckInstall)
        {
            string environmentVariable = Environment.GetEnvironmentVariable("COMPLUS_INSTALLROOT");
            string str2 = Environment.GetEnvironmentVariable("COMPLUS_VERSION");
            if (string.IsNullOrEmpty(environmentVariable) && string.IsNullOrEmpty(str2))
            {
                return (string.Compare("1", FindRegistryValueUnderKey(registryEntryToCheckInstall, registryValueToCheckInstall), StringComparison.OrdinalIgnoreCase) == 0);
            }
            return true;
        }

        internal static string ConstructDotNetFrameworkPathFromRuntimeInfo(string requestedVersion)
        {
            uint num5;
            StringBuilder builder;
            StringBuilder builder2;
            ErrorUtilities.VerifyThrowArgumentLength(requestedVersion, "requestedVersion");
            if (requestedVersion.Length > 20)
            {
                throw new ArgumentOutOfRangeException("requestedVersion");
            }
            int capacity = 0x108;
            int num2 = 0x19;
            do
            {
                uint num3;
                uint num4;
                builder = new StringBuilder(capacity);
                builder2 = new StringBuilder(num2);
                num5 = NativeMethodsShared.GetRequestedRuntimeInfo(string.Empty, requestedVersion, string.Empty, 0x10, 0x40, builder, capacity, out num3, builder2, num2, out num4);
                capacity *= 2;
                num2 *= 2;
            }
            while (num5 == 0x8007007a);
            if (num5 == 0)
            {
                return Path.Combine(builder.ToString(), builder2.ToString());
            }
            return null;
        }

        internal static string FindDotNetFrameworkPath(string currentRuntimePath, string prefix, string frameworkVersion, DirectoryExists directoryExists, GetDirectories getDirectories, DotNetFrameworkArchitecture architecture, bool useHeuristic)
        {
            string[] strArray;
            if (Path.GetFileName(currentRuntimePath).StartsWith(prefix, StringComparison.Ordinal) && (architecture == DotNetFrameworkArchitecture.Current))
            {
                return currentRuntimePath;
            }
            string str2 = null;
            if (architecture == DotNetFrameworkArchitecture.Current)
            {
                str2 = ConstructDotNetFrameworkPathFromRuntimeInfo(frameworkVersion);
            }
            if ((str2 != null) || !useHeuristic)
            {
                return str2;
            }
            string directoryName = Path.GetDirectoryName(currentRuntimePath);
            string pattern = prefix + "*";
            if (directoryName.Contains("64") && (architecture == DotNetFrameworkArchitecture.Bitness32))
            {
                int index = directoryName.IndexOf("64", StringComparison.OrdinalIgnoreCase);
                string str5 = directoryName;
                directoryName = str5.Substring(0, index) + str5.Substring(index + 2, (str5.Length - index) - 2);
            }
            else if (!directoryName.Contains("64") && (architecture == DotNetFrameworkArchitecture.Bitness64))
            {
                directoryName = directoryName + "64";
            }
            if (directoryExists(directoryName))
            {
                strArray = getDirectories(directoryName, pattern);
            }
            else
            {
                return null;
            }
            if (strArray.Length == 0)
            {
                return null;
            }
            string strB = strArray[0];
            if (!strB.EndsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                for (int i = 1; i < strArray.Length; i++)
                {
                    if (strArray[i].EndsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    {
                        return strArray[i];
                    }
                    if (string.Compare(strArray[i], strB, StringComparison.OrdinalIgnoreCase) > 0)
                    {
                        strB = strArray[i];
                    }
                }
            }
            return strB;
        }

        private static string FindRegistryValueUnderKey(string registryBaseKeyName, string registryKeyName)
        {
            RegistryKey key = Registry.LocalMachine.OpenSubKey(registryBaseKeyName);
            if (key == null)
            {
                return null;
            }
            object obj2 = key.GetValue(registryKeyName);
            if (obj2 == null)
            {
                return null;
            }
            return obj2.ToString();
        }

        internal static string GenerateProgramFiles32()
        {
            string environmentVariable = Environment.GetEnvironmentVariable("ProgramFiles(x86)");
            if (string.IsNullOrEmpty(environmentVariable))
            {
                environmentVariable = programFiles;
            }
            return environmentVariable;
        }

        internal static string GenerateProgramFiles64()
        {
            if (string.Equals(programFiles, programFiles32))
            {
                return Environment.GetEnvironmentVariable("ProgramW6432");
            }
            return programFiles;
        }

        internal static string GenerateProgramFilesReferenceAssemblyRoot()
        {
            return Path.GetFullPath(Path.Combine(programFiles32, @"Reference Assemblies\Microsoft\Framework"));
        }

        private static string GenerateReferenceAssemblyDirectory(string versionPrefix)
        {
            string path = Path.Combine(programFilesReferenceAssemblyLocation, versionPrefix);
            string str2 = null;
            if (Directory.Exists(path))
            {
                str2 = path;
            }
            return str2;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal static string GetPathToDotNetFramework(Version version)
        {
            return GetPathToDotNetFramework(version, DotNetFrameworkArchitecture.Current);
        }

        internal static string GetPathToDotNetFramework(Version version, DotNetFrameworkArchitecture architecture)
        {
            string str = version.Major + "." + version.Minor;
            switch (str)
            {
                case "1.1":
                    return GetPathToDotNetFrameworkV11(architecture);

                case "2.0":
                    return GetPathToDotNetFrameworkV20(architecture);

                case "3.0":
                    return GetPathToDotNetFrameworkV30(architecture);

                case "3.5":
                    return GetPathToDotNetFrameworkV35(architecture);

                case "4.0":
                    return GetPathToDotNetFrameworkV40(architecture);
            }
            ErrorUtilities.ThrowArgument("FrameworkLocationHelper.UnsupportedFrameworkVersion", new object[] { str });
            return null;
        }

        internal static string GetPathToDotNetFrameworkV11(DotNetFrameworkArchitecture architecture)
        {
            switch (architecture)
            {
                case DotNetFrameworkArchitecture.Current:
                    if (pathToCurrentDotNetFrameworkV11 == null)
                    {
                        break;
                    }
                    return pathToCurrentDotNetFrameworkV11;

                case DotNetFrameworkArchitecture.Bitness32:
                    if (pathToDotNetFramework32V11 == null)
                    {
                        break;
                    }
                    return pathToDotNetFramework32V11;

                case DotNetFrameworkArchitecture.Bitness64:
                    if (pathToDotNetFramework64V11 == null)
                    {
                        break;
                    }
                    return pathToDotNetFramework64V11;

                default:
                    ErrorUtilities.ThrowInternalErrorUnreachable();
                    return null;
            }
            if (CheckForFrameworkInstallation(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\v1.1.4322", "Install"))
            {
                string str = FindDotNetFrameworkPath(Path.GetDirectoryName(typeof(object).Module.FullyQualifiedName), "v1.1", "v1.1.4322", directoryExists, getDirectories, architecture, false);
                switch (architecture)
                {
                    case DotNetFrameworkArchitecture.Current:
                        pathToCurrentDotNetFrameworkV11 = str;
                        return str;

                    case DotNetFrameworkArchitecture.Bitness32:
                        pathToDotNetFramework32V11 = str;
                        return str;

                    case DotNetFrameworkArchitecture.Bitness64:
                        pathToDotNetFramework64V11 = str;
                        return str;
                }
                ErrorUtilities.ThrowInternalErrorUnreachable();
            }
            return null;
        }

        internal static string GetPathToDotNetFrameworkV20(DotNetFrameworkArchitecture architecture)
        {
            switch (architecture)
            {
                case DotNetFrameworkArchitecture.Current:
                    if (pathToCurrentDotNetFrameworkV20 == null)
                    {
                        break;
                    }
                    return pathToCurrentDotNetFrameworkV20;

                case DotNetFrameworkArchitecture.Bitness32:
                    if (pathToDotNetFramework32V20 == null)
                    {
                        break;
                    }
                    return pathToDotNetFramework32V20;

                case DotNetFrameworkArchitecture.Bitness64:
                    if (pathToDotNetFramework64V20 == null)
                    {
                        break;
                    }
                    return pathToDotNetFramework64V20;

                default:
                    ErrorUtilities.ThrowInternalErrorUnreachable();
                    return null;
            }
            if (CheckForFrameworkInstallation(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\v2.0.50727", "Install"))
            {
                string str = FindDotNetFrameworkPath(Path.GetDirectoryName(typeof(object).Module.FullyQualifiedName), "v2.0", "v2.0.50727", directoryExists, getDirectories, architecture, true);
                if ((str != null) && !File.Exists(Path.Combine(str, "msbuild.exe")))
                {
                    str = null;
                }
                switch (architecture)
                {
                    case DotNetFrameworkArchitecture.Current:
                        pathToCurrentDotNetFrameworkV20 = str;
                        return str;

                    case DotNetFrameworkArchitecture.Bitness32:
                        pathToDotNetFramework32V20 = str;
                        return str;

                    case DotNetFrameworkArchitecture.Bitness64:
                        pathToDotNetFramework64V20 = str;
                        return str;
                }
                ErrorUtilities.ThrowInternalErrorUnreachable();
            }
            return null;
        }

        internal static string GetPathToDotNetFrameworkV30(DotNetFrameworkArchitecture architecture)
        {
            switch (architecture)
            {
                case DotNetFrameworkArchitecture.Current:
                    if (pathToCurrentDotNetFrameworkV30 == null)
                    {
                        break;
                    }
                    return pathToCurrentDotNetFrameworkV30;

                case DotNetFrameworkArchitecture.Bitness32:
                    if (pathToDotNetFramework32V30 == null)
                    {
                        break;
                    }
                    return pathToDotNetFramework32V30;

                case DotNetFrameworkArchitecture.Bitness64:
                    if (pathToDotNetFramework64V30 == null)
                    {
                        break;
                    }
                    return pathToDotNetFramework64V30;

                default:
                    ErrorUtilities.ThrowInternalErrorUnreachable();
                    return null;
            }
            if (CheckForFrameworkInstallation(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\v3.0\Setup", "InstallSuccess"))
            {
                string str = FindDotNetFrameworkPath(Path.GetDirectoryName(typeof(object).Module.FullyQualifiedName), "v3.0", "v3.0", directoryExists, getDirectories, architecture, true);
                switch (architecture)
                {
                    case DotNetFrameworkArchitecture.Current:
                        pathToCurrentDotNetFrameworkV30 = str;
                        return str;

                    case DotNetFrameworkArchitecture.Bitness32:
                        pathToDotNetFramework32V30 = str;
                        return str;

                    case DotNetFrameworkArchitecture.Bitness64:
                        pathToDotNetFramework64V30 = str;
                        return str;
                }
                ErrorUtilities.ThrowInternalErrorUnreachable();
            }
            return null;
        }

        internal static string GetPathToDotNetFrameworkV35(DotNetFrameworkArchitecture architecture)
        {
            switch (architecture)
            {
                case DotNetFrameworkArchitecture.Current:
                    if (pathToCurrentDotNetFrameworkV35 == null)
                    {
                        break;
                    }
                    return pathToCurrentDotNetFrameworkV35;

                case DotNetFrameworkArchitecture.Bitness32:
                    if (pathToDotNetFramework32V35 == null)
                    {
                        break;
                    }
                    return pathToDotNetFramework32V35;

                case DotNetFrameworkArchitecture.Bitness64:
                    if (pathToDotNetFramework64V35 == null)
                    {
                        break;
                    }
                    return pathToDotNetFramework64V35;

                default:
                    ErrorUtilities.ThrowInternalErrorUnreachable();
                    return null;
            }
            if (CheckForFrameworkInstallation(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\v3.5", "Install"))
            {
                string str = FindDotNetFrameworkPath(Path.GetDirectoryName(typeof(object).Module.FullyQualifiedName), "v3.5", "v3.5", directoryExists, getDirectories, architecture, true);
                if ((str != null) && !File.Exists(Path.Combine(str, "msbuild.exe")))
                {
                    str = null;
                }
                switch (architecture)
                {
                    case DotNetFrameworkArchitecture.Current:
                        pathToCurrentDotNetFrameworkV35 = str;
                        return str;

                    case DotNetFrameworkArchitecture.Bitness32:
                        pathToDotNetFramework32V35 = str;
                        return str;

                    case DotNetFrameworkArchitecture.Bitness64:
                        pathToDotNetFramework64V35 = str;
                        return str;
                }
                ErrorUtilities.ThrowInternalErrorUnreachable();
            }
            return null;
        }

        internal static string GetPathToDotNetFrameworkV40(DotNetFrameworkArchitecture architecture)
        {
            switch (architecture)
            {
                case DotNetFrameworkArchitecture.Current:
                    if (pathToCurrentDotNetFrameworkV40 == null)
                    {
                        break;
                    }
                    return pathToCurrentDotNetFrameworkV40;

                case DotNetFrameworkArchitecture.Bitness32:
                    if (pathToDotNetFramework32V40 == null)
                    {
                        break;
                    }
                    return pathToDotNetFramework32V40;

                case DotNetFrameworkArchitecture.Bitness64:
                    if (pathToDotNetFramework64V40 == null)
                    {
                        break;
                    }
                    return pathToDotNetFramework64V40;

                default:
                    ErrorUtilities.ThrowInternalErrorUnreachable();
                    return null;
            }
            if (CheckForFrameworkInstallation(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full", "Install"))
            {
                string str = FindDotNetFrameworkPath(Path.GetDirectoryName(typeof(object).Module.FullyQualifiedName), "v4.0", "v4.0", directoryExists, getDirectories, architecture, true);
                if ((str != null) && !File.Exists(Path.Combine(str, "msbuild.exe")))
                {
                    str = null;
                }
                switch (architecture)
                {
                    case DotNetFrameworkArchitecture.Current:
                        pathToCurrentDotNetFrameworkV40 = str;
                        return str;

                    case DotNetFrameworkArchitecture.Bitness32:
                        pathToDotNetFramework32V40 = str;
                        return str;

                    case DotNetFrameworkArchitecture.Bitness64:
                        pathToDotNetFramework64V40 = str;
                        return str;
                }
                ErrorUtilities.ThrowInternalErrorUnreachable();
            }
            return null;
        }

        internal static string PathToDotNetFrameworkReferenceAssembliesV30
        {
            get
            {
                if (pathToDotNetFrameworkReferenceAssembliesV30 == null)
                {
                    pathToDotNetFrameworkReferenceAssembliesV30 = FindRegistryValueUnderKey(@"SOFTWARE\Microsoft\.NETFramework\AssemblyFolders\v3.0", "All Assemblies In");
                    if (pathToDotNetFrameworkReferenceAssembliesV30 == null)
                    {
                        pathToDotNetFrameworkReferenceAssembliesV30 = GenerateReferenceAssemblyDirectory("v3.0");
                    }
                }
                return pathToDotNetFrameworkReferenceAssembliesV30;
            }
        }

        internal static string PathToDotNetFrameworkReferenceAssembliesV35
        {
            get
            {
                if (pathToDotNetFrameworkReferenceAssembliesV35 == null)
                {
                    pathToDotNetFrameworkReferenceAssembliesV35 = FindRegistryValueUnderKey(@"SOFTWARE\Microsoft\.NETFramework\AssemblyFolders\v3.5", "All Assemblies In");
                    if (pathToDotNetFrameworkReferenceAssembliesV35 == null)
                    {
                        pathToDotNetFrameworkReferenceAssembliesV35 = GenerateReferenceAssemblyDirectory("v3.5");
                    }
                }
                return pathToDotNetFrameworkReferenceAssembliesV35;
            }
        }

        internal static string PathToDotNetFrameworkSdkV11
        {
            get
            {
                if (pathToDotNetFrameworkSdkV11 == null)
                {
                    pathToDotNetFrameworkSdkV11 = FindRegistryValueUnderKey(@"SOFTWARE\Microsoft\.NETFramework", "SDKInstallRootv1.1");
                }
                return pathToDotNetFrameworkSdkV11;
            }
        }

        internal static string PathToDotNetFrameworkSdkV20
        {
            get
            {
                if (pathToDotNetFrameworkSdkV20 == null)
                {
                    pathToDotNetFrameworkSdkV20 = FindRegistryValueUnderKey(@"SOFTWARE\Microsoft\.NETFramework", "SDKInstallRootv2.0");
                }
                return pathToDotNetFrameworkSdkV20;
            }
        }

        internal static string PathToDotNetFrameworkSdkV35
        {
            get
            {
                if (pathToDotNetFrameworkSdkV35 == null)
                {
                    pathToDotNetFrameworkSdkV35 = FindRegistryValueUnderKey(@"SOFTWARE\Microsoft\Microsoft SDKs\Windows\v7.0A", "InstallationFolder");
                    if (string.IsNullOrEmpty(pathToDotNetFrameworkSdkV35))
                    {
                        pathToDotNetFrameworkSdkV35 = FindRegistryValueUnderKey(@"SOFTWARE\Microsoft\Microsoft SDKs\Windows", "CurrentInstallFolder");
                    }
                }
                return pathToDotNetFrameworkSdkV35;
            }
        }

        internal static string PathToDotNetFrameworkSdkV40
        {
            get
            {
                if (pathToDotNetFrameworkSdkV40 == null)
                {
                    pathToDotNetFrameworkSdkV40 = FindRegistryValueUnderKey(@"SOFTWARE\Microsoft\Microsoft SDKs\Windows\v7.0A", "InstallationFolder");
                    if (string.IsNullOrEmpty(pathToDotNetFrameworkSdkV40))
                    {
                        pathToDotNetFrameworkSdkV40 = FindRegistryValueUnderKey(@"SOFTWARE\Microsoft\Microsoft SDKs\Windows", "CurrentInstallFolder");
                    }
                }
                return pathToDotNetFrameworkSdkV40;
            }
        }

        internal static string PathToDotNetFrameworkV11
        {
            get
            {
                return (pathToCurrentDotNetFrameworkV11 ?? GetPathToDotNetFrameworkV11(DotNetFrameworkArchitecture.Current));
            }
        }

        internal static string PathToDotNetFrameworkV20
        {
            get
            {
                return (pathToCurrentDotNetFrameworkV20 ?? GetPathToDotNetFrameworkV20(DotNetFrameworkArchitecture.Current));
            }
        }

        internal static string PathToDotNetFrameworkV30
        {
            get
            {
                return (pathToCurrentDotNetFrameworkV30 ?? GetPathToDotNetFrameworkV30(DotNetFrameworkArchitecture.Current));
            }
        }

        internal static string PathToDotNetFrameworkV35
        {
            get
            {
                return (pathToCurrentDotNetFrameworkV35 ?? GetPathToDotNetFrameworkV35(DotNetFrameworkArchitecture.Current));
            }
        }

        internal static string PathToDotNetFrameworkV40
        {
            get
            {
                return (pathToCurrentDotNetFrameworkV40 ?? GetPathToDotNetFrameworkV40(DotNetFrameworkArchitecture.Current));
            }
        }
    }
}

