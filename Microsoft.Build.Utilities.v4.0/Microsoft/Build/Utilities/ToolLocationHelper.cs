namespace Microsoft.Build.Utilities
{
    using Microsoft.Build.Shared;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.Versioning;
    using System.Text;
    using System.Xml;

    public static class ToolLocationHelper
    {
        private static Dictionary<string, FrameworkName> cachedHighestFrameworkNameForTargetFrameworkIdentifier;
        private static Dictionary<string, IList<string>> cachedReferenceAssemblyPaths;
        private static Dictionary<string, string> cachedTargetFrameworkDisplayNames;
        private static Dictionary<string, string> chainedReferenceAssemblyPath;
        private static string dotnet40ReferenceAssemblyPath;
        private static readonly FrameworkName dotNetFourFrameworkName = new FrameworkName(".NETFramework", dotNetFrameworkVersion40);
        private const string dotNetFrameworkIdentifier = ".NETFramework";
        private static readonly Version dotNetFrameworkVersion11 = new Version(1, 1);
        private static readonly Version dotNetFrameworkVersion20 = new Version(2, 0);
        private static readonly Version dotNetFrameworkVersion30 = new Version(3, 0);
        private static readonly Version dotNetFrameworkVersion35 = new Version(3, 5);
        private static readonly Version dotNetFrameworkVersion40 = new Version(4, 0);
        private const string frameworkReferenceRootPath = @"Reference Assemblies\Microsoft\Framework";
        private static object locker = new object();
        private const string redistListFile = "FrameworkList.xml";
        private const string redistListFolder = "RedistList";
        private const string subsetExtension = ".xml";
        private const string subsetListFolder = "SubsetList";
        private const string subsetPattern = "*.xml";
        private const string subTypeFolder = "SubType";
        private static List<string> targetFrameworkMonikers = null;

        internal static string ChainReferenceAssemblyPath(string targetFrameworkDirectory)
        {
            string fullPath = Path.GetFullPath(targetFrameworkDirectory);
            lock (locker)
            {
                chainedReferenceAssemblyPath = chainedReferenceAssemblyPath ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                cachedTargetFrameworkDisplayNames = cachedTargetFrameworkDisplayNames ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                string str2 = null;
                if (chainedReferenceAssemblyPath.TryGetValue(fullPath, out str2))
                {
                    return str2;
                }
            }
            string path = Path.Combine(Path.Combine(fullPath, "RedistList"), "FrameworkList.xml");
            if (!File.Exists(path))
            {
                lock (locker)
                {
                    chainedReferenceAssemblyPath[fullPath] = null;
                    cachedTargetFrameworkDisplayNames[fullPath] = null;
                }
                return null;
            }
            string str5 = null;
            string str6 = null;
            try
            {
                using (XmlTextReader reader = new XmlTextReader(path))
                {
                    while (reader.Read())
                    {
                        if ((reader.NodeType == XmlNodeType.Element) && string.Equals(reader.Name, "FileList", StringComparison.OrdinalIgnoreCase))
                        {
                            reader.MoveToFirstAttribute();
                            do
                            {
                                if (string.Equals(reader.Name, "IncludeFramework", StringComparison.OrdinalIgnoreCase))
                                {
                                    str5 = reader.Value;
                                }
                                else if (string.Equals(reader.Name, "Name", StringComparison.OrdinalIgnoreCase))
                                {
                                    str6 = reader.Value;
                                }
                            }
                            while (reader.MoveToNextAttribute());
                            reader.MoveToElement();
                            goto Label_01C3;
                        }
                    }
                }
            }
            catch (XmlException exception)
            {
                ErrorUtilities.ThrowInvalidOperation("ToolsLocationHelper.InvalidRedistFile", new object[] { path, exception.Message });
            }
            catch (Exception exception2)
            {
                if (ExceptionHandling.NotExpectedException(exception2))
                {
                    throw;
                }
                ErrorUtilities.ThrowInvalidOperation("ToolsLocationHelper.InvalidRedistFile", new object[] { path, exception2.Message });
            }
        Label_01C3:
            if (str6 != null)
            {
                lock (locker)
                {
                    cachedTargetFrameworkDisplayNames[fullPath] = str6;
                }
            }
            string fullName = string.Empty;
            try
            {
                if (!string.IsNullOrEmpty(str5))
                {
                    fullName = fullPath;
                    fullName = Directory.GetParent(fullName).FullName;
                    fullName = Path.Combine(fullName, str5);
                    fullName = Path.GetFullPath(fullName);
                    if (!Directory.Exists(fullName))
                    {
                        fullName = null;
                    }
                }
                lock (locker)
                {
                    chainedReferenceAssemblyPath[fullPath] = fullName;
                }
                return fullName;
            }
            catch (Exception exception3)
            {
                if (ExceptionHandling.IsCriticalException(exception3))
                {
                    throw;
                }
                ErrorUtilities.ThrowInvalidOperation("ToolsLocationHelper.CouldNotCreateChain", new object[] { fullPath, fullName, exception3.Message });
            }
            return null;
        }

        internal static void ClearStaticCaches()
        {
            lock (locker)
            {
                if (chainedReferenceAssemblyPath != null)
                {
                    chainedReferenceAssemblyPath.Clear();
                }
                if (cachedHighestFrameworkNameForTargetFrameworkIdentifier != null)
                {
                    cachedHighestFrameworkNameForTargetFrameworkIdentifier.Clear();
                }
                if (targetFrameworkMonikers != null)
                {
                    targetFrameworkMonikers.Clear();
                }
                if (cachedTargetFrameworkDisplayNames != null)
                {
                    cachedTargetFrameworkDisplayNames.Clear();
                }
                if (cachedReferenceAssemblyPaths != null)
                {
                    cachedReferenceAssemblyPaths.Clear();
                }
            }
        }

        private static string ConvertDotNetFrameworkArchitectureToProcessorArchitecture(Microsoft.Build.Utilities.DotNetFrameworkArchitecture architecture)
        {
            switch (architecture)
            {
                case Microsoft.Build.Utilities.DotNetFrameworkArchitecture.Current:
                    return ProcessorArchitecture.CurrentProcessArchitecture;

                case Microsoft.Build.Utilities.DotNetFrameworkArchitecture.Bitness32:
                    return "x86";

                case Microsoft.Build.Utilities.DotNetFrameworkArchitecture.Bitness64:
                {
                    NativeMethodsShared.SYSTEM_INFO lpSystemInfo = new NativeMethodsShared.SYSTEM_INFO();
                    NativeMethodsShared.GetNativeSystemInfo(ref lpSystemInfo);
                    ushort wProcessorArchitecture = lpSystemInfo.wProcessorArchitecture;
                    switch (wProcessorArchitecture)
                    {
                        case 0:
                            return null;

                        case 6:
                            return "IA64";
                    }
                    if (wProcessorArchitecture != 9)
                    {
                        return null;
                    }
                    return "AMD64";
                }
            }
            ErrorUtilities.ThrowInternalErrorUnreachable();
            return null;
        }

        private static Version ConvertTargetFrameworkVersionToVersion(string targetFrameworkVersion)
        {
            if (!string.IsNullOrEmpty(targetFrameworkVersion) && targetFrameworkVersion.Substring(0, 1).Equals("v", StringComparison.OrdinalIgnoreCase))
            {
                targetFrameworkVersion = targetFrameworkVersion.Substring(1);
            }
            return new Version(targetFrameworkVersion);
        }

        private static void CreateReferenceAssemblyPathsCache()
        {
            lock (locker)
            {
                if (cachedReferenceAssemblyPaths == null)
                {
                    cachedReferenceAssemblyPaths = new Dictionary<string, IList<string>>(StringComparer.OrdinalIgnoreCase);
                }
            }
        }

        private static string GenerateReferenceAssemblyCacheKey(string targetFrameworkRootPath, FrameworkName frameworkName)
        {
            return (targetFrameworkRootPath + "|" + frameworkName.FullName);
        }

        internal static string GenerateReferenceAssemblyPath(string targetFrameworkRootPath, FrameworkName frameworkName)
        {
            ErrorUtilities.VerifyThrowArgumentNull(targetFrameworkRootPath, "targetFrameworkRootPath");
            ErrorUtilities.VerifyThrowArgumentNull(frameworkName, "frameworkName");
            try
            {
                string str = targetFrameworkRootPath;
                str = Path.Combine(Path.Combine(str, frameworkName.Identifier), "v" + frameworkName.Version.ToString());
                if (!string.IsNullOrEmpty(frameworkName.Profile))
                {
                    str = Path.Combine(Path.Combine(str, "Profile"), frameworkName.Profile);
                }
                return Path.GetFullPath(str);
            }
            catch (Exception exception)
            {
                if (ExceptionHandling.NotExpectedException(exception))
                {
                    throw;
                }
                ErrorUtilities.ThrowInvalidOperation("ToolsLocationHelper.CouldNotGenerateReferenceAssemblyDirectory", new object[] { targetFrameworkRootPath, frameworkName.ToString(), exception.Message });
                return null;
            }
        }

        public static string GetDisplayNameForTargetFrameworkDirectory(string targetFrameworkDirectory, FrameworkName frameworkName)
        {
            string str;
            lock (locker)
            {
                if ((cachedTargetFrameworkDisplayNames != null) && cachedTargetFrameworkDisplayNames.TryGetValue(targetFrameworkDirectory, out str))
                {
                    return str;
                }
            }
            ChainReferenceAssemblyPath(targetFrameworkDirectory);
            lock (locker)
            {
                if (cachedTargetFrameworkDisplayNames.TryGetValue(targetFrameworkDirectory, out str))
                {
                    return str;
                }
            }
            StringBuilder builder = new StringBuilder();
            builder.Append(frameworkName.Identifier);
            builder.Append(" ");
            builder.Append("v" + frameworkName.Version.ToString());
            if (!string.IsNullOrEmpty(frameworkName.Profile))
            {
                builder.Append(" ");
                builder.Append(frameworkName.Profile);
            }
            str = builder.ToString();
            lock (locker)
            {
                cachedTargetFrameworkDisplayNames[targetFrameworkDirectory] = str;
            }
            return str;
        }

        public static string GetDotNetFrameworkRootRegistryKey(TargetDotNetFrameworkVersion version)
        {
            return @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\.NETFramework";
        }

        public static string GetDotNetFrameworkSdkInstallKeyValue(TargetDotNetFrameworkVersion version)
        {
            switch (version)
            {
                case TargetDotNetFrameworkVersion.Version11:
                    return "SDKInstallRootv1.1";

                case TargetDotNetFrameworkVersion.Version20:
                    return "SDKInstallRootv2.0";

                case TargetDotNetFrameworkVersion.Version35:
                    return "InstallationFolder";

                case TargetDotNetFrameworkVersion.Version40:
                    return "InstallationFolder";
            }
            ErrorUtilities.VerifyThrowArgument(false, "ToolLocationHelper.UnsupportedFrameworkVersion", version.ToString());
            return null;
        }

        public static string GetDotNetFrameworkSdkRootRegistryKey(TargetDotNetFrameworkVersion version)
        {
            switch (version)
            {
                case TargetDotNetFrameworkVersion.Version11:
                case TargetDotNetFrameworkVersion.Version20:
                    return @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\.NETFramework";

                case TargetDotNetFrameworkVersion.Version35:
                    return @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SDKs\Windows\v7.0A";

                case TargetDotNetFrameworkVersion.Version40:
                    return @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SDKs\Windows\v7.0A";
            }
            ErrorUtilities.VerifyThrowArgument(false, "ToolLocationHelper.UnsupportedFrameworkVersion", version.ToString());
            return null;
        }

        public static string GetDotNetFrameworkVersionFolderPrefix(TargetDotNetFrameworkVersion version)
        {
            switch (version)
            {
                case TargetDotNetFrameworkVersion.Version11:
                    return "v1.1";

                case TargetDotNetFrameworkVersion.Version20:
                    return "v2.0";

                case TargetDotNetFrameworkVersion.Version30:
                    return "v3.0";

                case TargetDotNetFrameworkVersion.Version35:
                    return "v3.5";

                case TargetDotNetFrameworkVersion.Version40:
                    return "v4.0";
            }
            ErrorUtilities.VerifyThrowArgument(false, "ToolLocationHelper.UnsupportedFrameworkVersion", version.ToString());
            return null;
        }

        internal static IList<string> GetFrameworkIdentifiers(string frameworkReferenceRoot)
        {
            if (string.IsNullOrEmpty(frameworkReferenceRoot))
            {
                throw new ArgumentException("Invalid frameworkReferenceRoot", "frameworkReferenceRoot");
            }
            List<string> list = new List<string>();
            bool flag = false;
            bool flag2 = false;
            bool flag3 = false;
            DirectoryInfo info = new DirectoryInfo(frameworkReferenceRoot);
            if (info.Exists)
            {
                if (frameworkReferenceRoot.Equals(FrameworkLocationHelper.programFilesReferenceAssemblyLocation, StringComparison.OrdinalIgnoreCase))
                {
                    flag3 = true;
                }
                foreach (DirectoryInfo info2 in info.GetDirectories())
                {
                    if (flag3 && ((string.Compare(info2.Name, "v3.0", StringComparison.OrdinalIgnoreCase) == 0) || (string.Compare(info2.Name, "v3.5", StringComparison.OrdinalIgnoreCase) == 0)))
                    {
                        flag = true;
                    }
                    else
                    {
                        if (string.Compare(info2.Name, ".NETFramework", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            flag2 = true;
                        }
                        list.Add(info2.Name);
                    }
                }
            }
            if (flag3 && !flag2)
            {
                if (!flag)
                {
                    string pathToDotNetFramework = GetPathToDotNetFramework(TargetDotNetFrameworkVersion.Version20);
                    if ((pathToDotNetFramework != null) && Directory.Exists(pathToDotNetFramework))
                    {
                        list.Add(".NETFramework");
                    }
                    return list;
                }
                list.Add(".NETFramework");
            }
            return list;
        }

        private static IList<string> GetFrameworkProfiles(string frameworkReferenceRoot, string frameworkIdentifier, string frameworkVersion)
        {
            if (string.IsNullOrEmpty(frameworkReferenceRoot))
            {
                throw new ArgumentException("Invalid frameworkReferenceRoot", "frameworkReferenceRoot");
            }
            if (string.IsNullOrEmpty(frameworkIdentifier))
            {
                throw new ArgumentException("Invalid frameworkIdentifier", "frameworkIdentifier");
            }
            if (string.IsNullOrEmpty(frameworkVersion))
            {
                throw new ArgumentException("Invalid frameworkVersion", "frameworkVersion");
            }
            List<string> list = new List<string>();
            DirectoryInfo info = new DirectoryInfo(Path.Combine(Path.Combine(Path.Combine(frameworkReferenceRoot, frameworkIdentifier), frameworkVersion), "Profiles"));
            if (info.Exists)
            {
                foreach (DirectoryInfo info2 in info.GetDirectories())
                {
                    Version version = VersionUtilities.ConvertToVersion(frameworkVersion);
                    IList<string> pathToReferenceAssemblies = GetPathToReferenceAssemblies(new FrameworkName(frameworkIdentifier, version, info2.Name));
                    if ((pathToReferenceAssemblies != null) && (pathToReferenceAssemblies.Count > 0))
                    {
                        list.Add(info2.Name);
                    }
                }
            }
            return list;
        }

        private static IList<string> GetFrameworkVersions(string frameworkReferenceRoot, string frameworkIdentifier)
        {
            if (string.IsNullOrEmpty(frameworkReferenceRoot))
            {
                throw new ArgumentException("Invalid frameworkReferenceRoot", "frameworkReferenceRoot");
            }
            if (string.IsNullOrEmpty(frameworkIdentifier))
            {
                throw new ArgumentException("Invalid frameworkIdentifier", "frameworkIdentifier");
            }
            List<string> list = new List<string>();
            if (string.Compare(frameworkIdentifier, ".NETFramework", StringComparison.OrdinalIgnoreCase) == 0)
            {
                IList<string> collection = GetFx35AndEarlierVersions(frameworkReferenceRoot);
                if (collection.Count > 0)
                {
                    list.AddRange(collection);
                }
            }
            DirectoryInfo info = new DirectoryInfo(Path.Combine(frameworkReferenceRoot, frameworkIdentifier));
            if (info.Exists)
            {
                foreach (DirectoryInfo info2 in info.GetDirectories())
                {
                    if ((info2.Name.Length >= 4) && info2.Name.StartsWith("v", StringComparison.OrdinalIgnoreCase))
                    {
                        Version result = null;
                        if (Version.TryParse(info2.Name.Substring(1), out result))
                        {
                            list.Add(info2.Name);
                        }
                    }
                }
            }
            list.Sort(new VersionComparer());
            return list;
        }

        private static IList<string> GetFx35AndEarlierVersions(string frameworkReferenceRoot)
        {
            IList<string> list = new List<string>();
            if (GetPathToDotNetFramework(TargetDotNetFrameworkVersion.Version35) != null)
            {
                if (GetPathToDotNetFramework(TargetDotNetFrameworkVersion.Version20) != null)
                {
                    list.Add("v2.0");
                }
                if (Directory.Exists(Path.Combine(frameworkReferenceRoot, "v3.0")))
                {
                    list.Add("v3.0");
                }
                if (Directory.Exists(Path.Combine(frameworkReferenceRoot, "v3.5")))
                {
                    list.Add("v3.5");
                }
            }
            return list;
        }

        internal static IList<string> GetPathAndChainReferenceAssemblyLocations(string targetFrameworkRootPath, FrameworkName frameworkName, bool chain)
        {
            List<string> list = new List<string>();
            string path = GenerateReferenceAssemblyPath(targetFrameworkRootPath, frameworkName);
            if (Directory.Exists(path))
            {
                list.Add(path);
                if (!chain)
                {
                    return list;
                }
                while (!string.IsNullOrEmpty(path))
                {
                    path = ChainReferenceAssemblyPath(path);
                    if (!string.IsNullOrEmpty(path))
                    {
                        if (list.Contains(path))
                        {
                            return list;
                        }
                        list.Add(path);
                    }
                    else if (path == null)
                    {
                        list.Clear();
                        return list;
                    }
                }
            }
            return list;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static string GetPathToDotNetFramework(TargetDotNetFrameworkVersion version)
        {
            return GetPathToDotNetFramework(version, Microsoft.Build.Utilities.DotNetFrameworkArchitecture.Current);
        }

        public static string GetPathToDotNetFramework(TargetDotNetFrameworkVersion version, Microsoft.Build.Utilities.DotNetFrameworkArchitecture architecture)
        {
            Version version2 = null;
            switch (version)
            {
                case TargetDotNetFrameworkVersion.Version11:
                    version2 = dotNetFrameworkVersion11;
                    break;

                case TargetDotNetFrameworkVersion.Version20:
                    version2 = dotNetFrameworkVersion20;
                    break;

                case TargetDotNetFrameworkVersion.Version30:
                    version2 = dotNetFrameworkVersion30;
                    break;

                case TargetDotNetFrameworkVersion.Version35:
                    version2 = dotNetFrameworkVersion35;
                    break;

                case TargetDotNetFrameworkVersion.Version40:
                    version2 = dotNetFrameworkVersion40;
                    break;

                default:
                    ErrorUtilities.VerifyThrowArgument(false, "ToolLocationHelper.UnsupportedFrameworkVersion", version.ToString());
                    return null;
            }
            Microsoft.Build.Shared.DotNetFrameworkArchitecture current = Microsoft.Build.Shared.DotNetFrameworkArchitecture.Current;
            switch (architecture)
            {
                case Microsoft.Build.Utilities.DotNetFrameworkArchitecture.Current:
                    current = Microsoft.Build.Shared.DotNetFrameworkArchitecture.Current;
                    break;

                case Microsoft.Build.Utilities.DotNetFrameworkArchitecture.Bitness32:
                    current = Microsoft.Build.Shared.DotNetFrameworkArchitecture.Bitness32;
                    break;

                case Microsoft.Build.Utilities.DotNetFrameworkArchitecture.Bitness64:
                    current = Microsoft.Build.Shared.DotNetFrameworkArchitecture.Bitness64;
                    break;

                default:
                    ErrorUtilities.ThrowInternalErrorUnreachable();
                    return null;
            }
            return FrameworkLocationHelper.GetPathToDotNetFramework(version2, current);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static string GetPathToDotNetFrameworkFile(string fileName, TargetDotNetFrameworkVersion version)
        {
            return GetPathToDotNetFrameworkFile(fileName, version, Microsoft.Build.Utilities.DotNetFrameworkArchitecture.Current);
        }

        public static string GetPathToDotNetFrameworkFile(string fileName, TargetDotNetFrameworkVersion version, Microsoft.Build.Utilities.DotNetFrameworkArchitecture architecture)
        {
            string pathToDotNetFramework = GetPathToDotNetFramework(version, architecture);
            if (pathToDotNetFramework == null)
            {
                return null;
            }
            return Path.Combine(pathToDotNetFramework, fileName);
        }

        public static string GetPathToDotNetFrameworkReferenceAssemblies(TargetDotNetFrameworkVersion version)
        {
            switch (version)
            {
                case TargetDotNetFrameworkVersion.Version11:
                    return FrameworkLocationHelper.PathToDotNetFrameworkV11;

                case TargetDotNetFrameworkVersion.Version20:
                    return FrameworkLocationHelper.PathToDotNetFrameworkV20;

                case TargetDotNetFrameworkVersion.Version30:
                    return FrameworkLocationHelper.PathToDotNetFrameworkReferenceAssembliesV30;

                case TargetDotNetFrameworkVersion.Version35:
                    return FrameworkLocationHelper.PathToDotNetFrameworkReferenceAssembliesV35;

                case TargetDotNetFrameworkVersion.Version40:
                    if (dotnet40ReferenceAssemblyPath == null)
                    {
                        string path = GenerateReferenceAssemblyPath(FrameworkLocationHelper.programFilesReferenceAssemblyLocation, dotNetFourFrameworkName);
                        if (Directory.Exists(path))
                        {
                            dotnet40ReferenceAssemblyPath = path;
                            dotnet40ReferenceAssemblyPath = FileUtilities.EnsureTrailingSlash(dotnet40ReferenceAssemblyPath);
                        }
                    }
                    return dotnet40ReferenceAssemblyPath;
            }
            ErrorUtilities.VerifyThrowArgument(false, "ToolLocationHelper.UnsupportedFrameworkVersion", version.ToString());
            return null;
        }

        public static string GetPathToDotNetFrameworkSdk(TargetDotNetFrameworkVersion version)
        {
            switch (version)
            {
                case TargetDotNetFrameworkVersion.Version11:
                    return FrameworkLocationHelper.PathToDotNetFrameworkSdkV11;

                case TargetDotNetFrameworkVersion.Version20:
                    return FrameworkLocationHelper.PathToDotNetFrameworkSdkV20;

                case TargetDotNetFrameworkVersion.Version35:
                    return FrameworkLocationHelper.PathToDotNetFrameworkSdkV35;

                case TargetDotNetFrameworkVersion.Version40:
                    return FrameworkLocationHelper.PathToDotNetFrameworkSdkV40;
            }
            ErrorUtilities.VerifyThrowArgument(false, "ToolLocationHelper.UnsupportedFrameworkVersion", version.ToString());
            return null;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static string GetPathToDotNetFrameworkSdkFile(string fileName, TargetDotNetFrameworkVersion version)
        {
            return GetPathToDotNetFrameworkSdkFile(fileName, version, Microsoft.Build.Utilities.DotNetFrameworkArchitecture.Current, true);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static string GetPathToDotNetFrameworkSdkFile(string fileName, TargetDotNetFrameworkVersion version, Microsoft.Build.Utilities.DotNetFrameworkArchitecture architecture)
        {
            return GetPathToDotNetFrameworkSdkFile(fileName, version, architecture, false);
        }

        internal static string GetPathToDotNetFrameworkSdkFile(string fileName, string pathToSdk, string processorArchitecture)
        {
            if (((pathToSdk == null) || (fileName == null)) || (processorArchitecture == null))
            {
                return null;
            }
            string str2 = processorArchitecture;
            if (str2 != null)
            {
                if (!(str2 == "AMD64"))
                {
                    if (str2 == "IA64")
                    {
                        pathToSdk = Path.Combine(pathToSdk, "ia64");
                    }
                    else if (str2 == "x86")
                    {
                    }
                }
                else
                {
                    pathToSdk = Path.Combine(pathToSdk, "x64");
                }
            }
            string str = Path.Combine(pathToSdk, fileName);
            if (!new FileInfo(str).Exists)
            {
                return null;
            }
            return str;
        }

        internal static string GetPathToDotNetFrameworkSdkFile(string fileName, TargetDotNetFrameworkVersion version, Microsoft.Build.Utilities.DotNetFrameworkArchitecture architecture, bool canFallBackIfNecessary)
        {
            string pathToDotNetFrameworkSdk = GetPathToDotNetFrameworkSdk(version);
            string str2 = null;
            if (pathToDotNetFrameworkSdk != null)
            {
                pathToDotNetFrameworkSdk = Path.Combine(pathToDotNetFrameworkSdk, "bin");
                if (version == TargetDotNetFrameworkVersion.Version40)
                {
                    pathToDotNetFrameworkSdk = Path.Combine(pathToDotNetFrameworkSdk, "NETFX 4.0 Tools");
                }
                string processorArchitecture = ConvertDotNetFrameworkArchitectureToProcessorArchitecture(architecture);
                str2 = GetPathToDotNetFrameworkSdkFile(fileName, pathToDotNetFrameworkSdk, processorArchitecture);
                if ((str2 != null) || !canFallBackIfNecessary)
                {
                    return str2;
                }
                if (!string.Equals(ProcessorArchitecture.CurrentProcessArchitecture, processorArchitecture, StringComparison.OrdinalIgnoreCase))
                {
                    str2 = GetPathToDotNetFrameworkSdkFile(fileName, pathToDotNetFrameworkSdk, ProcessorArchitecture.CurrentProcessArchitecture);
                }
                if ((str2 == null) && !string.Equals("x86", ProcessorArchitecture.CurrentProcessArchitecture, StringComparison.OrdinalIgnoreCase))
                {
                    str2 = GetPathToDotNetFrameworkSdkFile(fileName, pathToDotNetFrameworkSdk, "x86");
                }
            }
            return str2;
        }

        public static IList<string> GetPathToReferenceAssemblies(FrameworkName frameworkName)
        {
            ErrorUtilities.VerifyThrowArgumentNull(frameworkName, "frameworkName");
            return GetPathToReferenceAssemblies(FrameworkLocationHelper.programFilesReferenceAssemblyLocation, frameworkName);
        }

        public static IList<string> GetPathToReferenceAssemblies(string targetFrameworkRootPath, FrameworkName frameworkName)
        {
            ErrorUtilities.VerifyThrowArgumentLength(targetFrameworkRootPath, "targetFrameworkRootPath");
            ErrorUtilities.VerifyThrowArgumentNull(frameworkName, "frameworkName");
            string key = GenerateReferenceAssemblyCacheKey(targetFrameworkRootPath, frameworkName);
            CreateReferenceAssemblyPathsCache();
            lock (locker)
            {
                IList<string> list;
                if (cachedReferenceAssemblyPaths.TryGetValue(key, out list))
                {
                    return list;
                }
            }
            IList<string> list2 = GetPathAndChainReferenceAssemblyLocations(targetFrameworkRootPath, frameworkName, true);
            if ((string.Equals(frameworkName.Identifier, ".NETFramework", StringComparison.OrdinalIgnoreCase) && (list2.Count == 0)) && string.IsNullOrEmpty(frameworkName.Profile))
            {
                list2 = HandleLegacyDotNetFrameworkReferenceAssemblyPaths(null, null, frameworkName);
            }
            lock (locker)
            {
                cachedReferenceAssemblyPaths[key] = list2;
            }
            for (int i = 0; i < list2.Count; i++)
            {
                if (!list2[i].EndsWith(@"\", StringComparison.Ordinal))
                {
                    list2[i] = list2[i] + @"\";
                }
            }
            return list2;
        }

        public static IList<string> GetPathToReferenceAssemblies(string targetFrameworkIdentifier, string targetFrameworkVersion, string targetFrameworkProfile)
        {
            ErrorUtilities.VerifyThrowArgumentLength(targetFrameworkVersion, "targetFrameworkVersion");
            ErrorUtilities.VerifyThrowArgumentLength(targetFrameworkIdentifier, "targetFrameworkIdentifier");
            ErrorUtilities.VerifyThrowArgumentNull(targetFrameworkProfile, "targetFrameworkProfile");
            Version version = ConvertTargetFrameworkVersionToVersion(targetFrameworkVersion);
            FrameworkName frameworkName = new FrameworkName(targetFrameworkIdentifier, version, targetFrameworkProfile);
            return GetPathToReferenceAssemblies(frameworkName);
        }

        public static string GetPathToStandardLibraries(string targetFrameworkIdentifier, string targetFrameworkVersion, string targetFrameworkProfile)
        {
            foreach (string str in GetPathToReferenceAssemblies(targetFrameworkIdentifier, targetFrameworkVersion, targetFrameworkProfile))
            {
                if (File.Exists(Path.Combine(str, "mscorlib.dll")))
                {
                    return FileUtilities.EnsureNoTrailingSlash(str);
                }
            }
            return string.Empty;
        }

        public static string GetPathToSystemFile(string fileName)
        {
            return Path.Combine(PathToSystem, fileName);
        }

        public static string GetProgramFilesReferenceAssemblyRoot()
        {
            return FrameworkLocationHelper.programFilesReferenceAssemblyLocation;
        }

        public static IList<string> GetSupportedTargetFrameworks()
        {
            lock (locker)
            {
                if (targetFrameworkMonikers == null)
                {
                    targetFrameworkMonikers = new List<string>();
                    foreach (string str in GetFrameworkIdentifiers(FrameworkLocationHelper.programFilesReferenceAssemblyLocation))
                    {
                        foreach (string str2 in GetFrameworkVersions(FrameworkLocationHelper.programFilesReferenceAssemblyLocation, str))
                        {
                            Version version = VersionUtilities.ConvertToVersion(str2);
                            targetFrameworkMonikers.Add(new FrameworkName(str, version, null).FullName);
                            foreach (string str3 in GetFrameworkProfiles(FrameworkLocationHelper.programFilesReferenceAssemblyLocation, str, str2))
                            {
                                targetFrameworkMonikers.Add(new FrameworkName(str, version, str3).FullName);
                            }
                        }
                    }
                }
            }
            return targetFrameworkMonikers;
        }

        private static IList<string> HandleLegacy20(VersionToPath PathToDotNetFramework)
        {
            List<string> list = new List<string>();
            string item = VersionToDotNetFrameworkPath(PathToDotNetFramework, TargetDotNetFrameworkVersion.Version20);
            if (item != null)
            {
                list.Add(item);
            }
            return list;
        }

        private static IList<string> HandleLegacy30(VersionToPath PathToDotNetFramework, VersionToPath PathToReferenceAssemblies)
        {
            List<string> list = new List<string>();
            string item = VersionToDotNetReferenceAssemblies(PathToReferenceAssemblies, TargetDotNetFrameworkVersion.Version30);
            string str2 = VersionToDotNetFrameworkPath(PathToDotNetFramework, TargetDotNetFrameworkVersion.Version30);
            if ((item != null) && (str2 != null))
            {
                list.Add(item);
                list.Add(str2);
            }
            else
            {
                return list;
            }
            IList<string> collection = HandleLegacy20(PathToDotNetFramework);
            list.AddRange(collection);
            return list;
        }

        private static IList<string> HandleLegacy35(VersionToPath PathToDotNetFramework, VersionToPath PathToReferenceAssemblies)
        {
            List<string> list = new List<string>();
            string item = VersionToDotNetReferenceAssemblies(PathToReferenceAssemblies, TargetDotNetFrameworkVersion.Version35);
            string str2 = VersionToDotNetFrameworkPath(PathToDotNetFramework, TargetDotNetFrameworkVersion.Version35);
            if ((item != null) && (str2 != null))
            {
                list.Add(item);
                list.Add(str2);
            }
            else
            {
                return list;
            }
            IList<string> collection = HandleLegacy30(PathToDotNetFramework, PathToReferenceAssemblies);
            list.AddRange(collection);
            return list;
        }

        internal static IList<string> HandleLegacyDotNetFrameworkReferenceAssemblyPaths(VersionToPath PathToDotNetFramework, VersionToPath PathToReferenceAssemblies, FrameworkName frameworkName)
        {
            if (frameworkName.Version == dotNetFrameworkVersion20)
            {
                return HandleLegacy20(PathToDotNetFramework);
            }
            if (frameworkName.Version == dotNetFrameworkVersion30)
            {
                return HandleLegacy30(PathToDotNetFramework, PathToReferenceAssemblies);
            }
            if (frameworkName.Version == dotNetFrameworkVersion35)
            {
                return HandleLegacy35(PathToDotNetFramework, PathToReferenceAssemblies);
            }
            return new List<string>();
        }

        public static FrameworkName HighestVersionOfTargetFrameworkIdentifier(string targetFrameworkRootDirectory, string frameworkIdentifier)
        {
            ErrorUtilities.VerifyThrowArgumentLength(targetFrameworkRootDirectory, "targetFrameworkRootDirectory");
            ErrorUtilities.VerifyThrowArgumentLength(frameworkIdentifier, "frameworkIdentifier");
            string key = targetFrameworkRootDirectory + ";" + frameworkIdentifier;
            FrameworkName name = null;
            bool flag = false;
            lock (locker)
            {
                if (cachedHighestFrameworkNameForTargetFrameworkIdentifier == null)
                {
                    cachedHighestFrameworkNameForTargetFrameworkIdentifier = new Dictionary<string, FrameworkName>(StringComparer.OrdinalIgnoreCase);
                }
                else
                {
                    flag = cachedHighestFrameworkNameForTargetFrameworkIdentifier.TryGetValue(key, out name);
                }
                if (flag)
                {
                    return name;
                }
                IList<string> frameworkVersions = GetFrameworkVersions(targetFrameworkRootDirectory, frameworkIdentifier);
                if (frameworkVersions.Count > 0)
                {
                    Version version = ConvertTargetFrameworkVersionToVersion(frameworkVersions[frameworkVersions.Count - 1]);
                    name = new FrameworkName(frameworkIdentifier, version);
                }
                cachedHighestFrameworkNameForTargetFrameworkIdentifier.Add(key, name);
            }
            return name;
        }

        internal static string VersionToDotNetFrameworkPath(VersionToPath PathToDotNetFramework, TargetDotNetFrameworkVersion version)
        {
            if (PathToDotNetFramework == null)
            {
                return GetPathToDotNetFramework(version);
            }
            return PathToDotNetFramework(version);
        }

        internal static string VersionToDotNetReferenceAssemblies(VersionToPath PathToDotReferenceAssemblies, TargetDotNetFrameworkVersion version)
        {
            if (PathToDotReferenceAssemblies == null)
            {
                return GetPathToDotNetFrameworkReferenceAssemblies(version);
            }
            return PathToDotReferenceAssemblies(version);
        }

        public static string PathToSystem
        {
            get
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.System);
            }
        }

        private class VersionComparer : IComparer<string>
        {
            public int Compare(string versionX, string versionY)
            {
                return new Version(versionX.Substring(1)).CompareTo(new Version(versionY.Substring(1)));
            }
        }

        internal delegate string VersionToPath(TargetDotNetFrameworkVersion version);
    }
}

