namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Shared;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;

    internal static class GlobalAssemblyCache
    {
        internal static readonly GetGacEnumerator gacEnumerator = new GetGacEnumerator(GlobalAssemblyCache.GetGacNativeEnumerator);
        internal static readonly GetPathFromFusionName pathFromFusionName = new GetPathFromFusionName(GlobalAssemblyCache.RetreivePathFromFusionName);

        private static string CheckForFullFusionNameInGac(AssemblyNameExtension assemblyName, string targetProcessorArchitecture, GetPathFromFusionName getPathFromFusionName)
        {
            string fullName = assemblyName.FullName;
            if ((targetProcessorArchitecture != null) && !assemblyName.HasProcessorArchitectureInFusionName)
            {
                fullName = fullName + ", ProcessorArchitecture=" + targetProcessorArchitecture;
            }
            return getPathFromFusionName(fullName);
        }

        private static SortedDictionary<Version, SortedDictionary<AssemblyNameExtension, string>> GenerateListOfAssembliesByRuntime(string strongName, GetAssemblyRuntimeVersion getRuntimeVersion, Version targetedRuntime, Microsoft.Build.Shared.FileExists fileExists, GetPathFromFusionName getPathFromFusionName, GetGacEnumerator getGacEnumerator, bool specificVersion)
        {
            Microsoft.Build.Shared.ErrorUtilities.VerifyThrowArgumentNull(targetedRuntime, "targetedRuntime");
            IEnumerable<AssemblyNameExtension> enumerable = getGacEnumerator(strongName);
            SortedDictionary<Version, SortedDictionary<AssemblyNameExtension, string>> dictionary = new SortedDictionary<Version, SortedDictionary<AssemblyNameExtension, string>>(ReverseVersionGenericComparer.Comparer);
            if (enumerable != null)
            {
                foreach (AssemblyNameExtension extension in enumerable)
                {
                    string str = getPathFromFusionName(extension.FullName);
                    if (!string.IsNullOrEmpty(str) && fileExists(str))
                    {
                        Version version = VersionUtilities.ConvertToVersion(getRuntimeVersion(str));
                        if ((version != null) && ((targetedRuntime.CompareTo(version) >= 0) || specificVersion))
                        {
                            SortedDictionary<AssemblyNameExtension, string> dictionary2 = null;
                            dictionary.TryGetValue(version, out dictionary2);
                            if (dictionary2 == null)
                            {
                                dictionary2 = new SortedDictionary<AssemblyNameExtension, string>(AssemblyNameReverseVersionComparer.GenericComparer);
                                dictionary.Add(version, dictionary2);
                            }
                            if (!dictionary2.ContainsKey(extension))
                            {
                                dictionary2.Add(extension, str);
                            }
                        }
                    }
                }
            }
            return dictionary;
        }

        internal static IEnumerable<AssemblyNameExtension> GetGacNativeEnumerator(string strongName)
        {
            IEnumerable<AssemblyNameExtension> enumerable = null;
            try
            {
                enumerable = new Microsoft.Build.Tasks.NativeMethods.AssemblyCacheEnum(strongName);
            }
            catch (FileLoadException)
            {
                return null;
            }
            return enumerable;
        }

        internal static string GetGacPath()
        {
            int pcchPath = 0;
            Microsoft.Build.Tasks.NativeMethods.GetCachePath(AssemblyCacheFlags.GAC, null, ref pcchPath);
            StringBuilder cachePath = new StringBuilder(pcchPath);
            Microsoft.Build.Tasks.NativeMethods.GetCachePath(AssemblyCacheFlags.GAC, cachePath, ref pcchPath);
            return cachePath.ToString();
        }

        internal static string GetLocation(AssemblyNameExtension strongName, ProcessorArchitecture targetProcessorArchitecture, GetAssemblyRuntimeVersion getRuntimeVersion, Version targetedRuntimeVersion, bool fullFusionName, Microsoft.Build.Shared.FileExists fileExists, GetPathFromFusionName getPathFromFusionName, GetGacEnumerator getGacEnumerator, bool specificVersion)
        {
            string str = null;
            if (((strongName.GetPublicKeyToken() == null) || (strongName.GetPublicKeyToken().Length == 0)) && (strongName.FullName.IndexOf("PublicKeyToken", StringComparison.OrdinalIgnoreCase) != -1))
            {
                return str;
            }
            getPathFromFusionName = getPathFromFusionName ?? pathFromFusionName;
            getGacEnumerator = getGacEnumerator ?? gacEnumerator;
            if (!strongName.HasProcessorArchitectureInFusionName)
            {
                if ((targetProcessorArchitecture != ProcessorArchitecture.MSIL) && (targetProcessorArchitecture != ProcessorArchitecture.None))
                {
                    string str2 = ResolveAssemblyReference.ProcessorArchitectureToString(targetProcessorArchitecture);
                    if (fullFusionName)
                    {
                        str = CheckForFullFusionNameInGac(strongName, str2, getPathFromFusionName);
                    }
                    else
                    {
                        str = GetLocationImpl(strongName, str2, getRuntimeVersion, targetedRuntimeVersion, fileExists, getPathFromFusionName, getGacEnumerator, specificVersion);
                    }
                    if ((str != null) && (str.Length > 0))
                    {
                        return str;
                    }
                }
                if (fullFusionName)
                {
                    str = CheckForFullFusionNameInGac(strongName, "MSIL", getPathFromFusionName);
                }
                else
                {
                    str = GetLocationImpl(strongName, "MSIL", getRuntimeVersion, targetedRuntimeVersion, fileExists, getPathFromFusionName, getGacEnumerator, specificVersion);
                }
                if ((str != null) && (str.Length > 0))
                {
                    return str;
                }
            }
            if (fullFusionName)
            {
                str = CheckForFullFusionNameInGac(strongName, null, getPathFromFusionName);
            }
            else
            {
                str = GetLocationImpl(strongName, null, getRuntimeVersion, targetedRuntimeVersion, fileExists, getPathFromFusionName, getGacEnumerator, specificVersion);
            }
            if ((str != null) && (str.Length > 0))
            {
                return str;
            }
            return null;
        }

        private static string GetLocationImpl(AssemblyNameExtension assemblyName, string targetProcessorArchitecture, GetAssemblyRuntimeVersion getRuntimeVersion, Version targetedRuntime, Microsoft.Build.Shared.FileExists fileExists, GetPathFromFusionName getPathFromFusionName, GetGacEnumerator getGacEnumerator, bool specificVersion)
        {
            Microsoft.Build.Shared.ErrorUtilities.VerifyThrowArgumentNull(assemblyName, "assemblyName");
            Microsoft.Build.Shared.ErrorUtilities.VerifyThrow(assemblyName.FullName != null, "Got a null assembly name fullname.");
            string fullName = assemblyName.FullName;
            if ((targetProcessorArchitecture != null) && !assemblyName.HasProcessorArchitectureInFusionName)
            {
                fullName = fullName + ", ProcessorArchitecture=" + targetProcessorArchitecture;
            }
            string str2 = string.Empty;
            SortedDictionary<Version, SortedDictionary<AssemblyNameExtension, string>> dictionary = GenerateListOfAssembliesByRuntime(fullName, getRuntimeVersion, targetedRuntime, fileExists, getPathFromFusionName, getGacEnumerator, specificVersion);
            if (dictionary != null)
            {
                foreach (SortedDictionary<AssemblyNameExtension, string> dictionary2 in dictionary.Values)
                {
                    if (dictionary2.Count <= 0)
                    {
                        continue;
                    }
                    foreach (KeyValuePair<AssemblyNameExtension, string> pair in dictionary2)
                    {
                        str2 = pair.Value;
                        break;
                    }
                    if (!string.IsNullOrEmpty(str2))
                    {
                        return str2;
                    }
                }
            }
            return str2;
        }

        internal static string RetreivePathFromFusionName(string strongName)
        {
            IAssemblyCache cache;
            Microsoft.Build.Shared.ErrorUtilities.VerifyThrowArgumentNull(strongName, "assemblyName");
            uint num = Microsoft.Build.Tasks.NativeMethods.CreateAssemblyCache(out cache, 0);
            Microsoft.Build.Shared.ErrorUtilities.VerifyThrow(num == 0, "CreateAssemblyCache failed, hr {0}", num);
            ASSEMBLY_INFO pAsmInfo = new ASSEMBLY_INFO {
                cbAssemblyInfo = (uint) Marshal.SizeOf(typeof(ASSEMBLY_INFO))
            };
            cache.QueryAssemblyInfo(0, strongName, ref pAsmInfo);
            if (pAsmInfo.cbAssemblyInfo == 0)
            {
                return null;
            }
            pAsmInfo.pszCurrentAssemblyPathBuf = new string(new char[pAsmInfo.cchBuf]);
            cache.QueryAssemblyInfo(0, strongName, ref pAsmInfo);
            return pAsmInfo.pszCurrentAssemblyPathBuf;
        }
    }
}

