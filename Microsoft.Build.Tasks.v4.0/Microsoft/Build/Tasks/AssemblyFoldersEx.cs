namespace Microsoft.Build.Tasks
{
    using Microsoft.Win32;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;

    internal class AssemblyFoldersEx : IEnumerable
    {
        private ArrayList directoryNames = new ArrayList();

        internal AssemblyFoldersEx(string registryKeyRoot, string targetRuntimeVersion, string registryKeySuffix, string osVersion, string platform, GetRegistrySubKeyNames getRegistrySubKeyNames, GetRegistrySubKeyDefaultValue getRegistrySubKeyDefaultValue, ProcessorArchitecture targetProcessorArchitecture, OpenBaseKey openBaseKey)
        {
            bool flag = Environment.Is64BitOperatingSystem;
            bool flag2 = (targetProcessorArchitecture == ProcessorArchitecture.Amd64) || (targetProcessorArchitecture == ProcessorArchitecture.IA64);
            if (flag)
            {
                if (flag2)
                {
                    this.FindUnderRegistryHive(RegistryView.Registry64, registryKeyRoot, targetRuntimeVersion, registryKeySuffix, osVersion, platform, getRegistrySubKeyNames, getRegistrySubKeyDefaultValue, openBaseKey);
                    this.FindUnderRegistryHive(RegistryView.Registry32, registryKeyRoot, targetRuntimeVersion, registryKeySuffix, osVersion, platform, getRegistrySubKeyNames, getRegistrySubKeyDefaultValue, openBaseKey);
                }
                else
                {
                    this.FindUnderRegistryHive(RegistryView.Registry32, registryKeyRoot, targetRuntimeVersion, registryKeySuffix, osVersion, platform, getRegistrySubKeyNames, getRegistrySubKeyDefaultValue, openBaseKey);
                    this.FindUnderRegistryHive(RegistryView.Registry64, registryKeyRoot, targetRuntimeVersion, registryKeySuffix, osVersion, platform, getRegistrySubKeyNames, getRegistrySubKeyDefaultValue, openBaseKey);
                }
            }
            else
            {
                this.FindUnderRegistryHive(RegistryView.Default, registryKeyRoot, targetRuntimeVersion, registryKeySuffix, osVersion, platform, getRegistrySubKeyNames, getRegistrySubKeyDefaultValue, openBaseKey);
            }
        }

        private static void AddCandidateVersion(SortedDictionary<Version, ArrayList> targetFrameworkVersionToRegistryVersions, string version, Version candidateVersion)
        {
            ArrayList list = null;
            if (targetFrameworkVersionToRegistryVersions.TryGetValue(candidateVersion, out list))
            {
                list.Add(version);
            }
            else
            {
                list = new ArrayList();
                list.Add(version);
                targetFrameworkVersionToRegistryVersions.Add(candidateVersion, list);
            }
        }

        private void FindDirectories(RegistryView view, RegistryHive hive, string registryKeyRoot, string targetRuntimeVersion, string registryKeySuffix, string osVersion, string platform, GetRegistrySubKeyNames getRegistrySubKeyNames, GetRegistrySubKeyDefaultValue getRegistrySubKeyDefaultValue, OpenBaseKey openBaseKey)
        {
            RegistryKey baseKey = openBaseKey(hive, view);
            IEnumerable versions = getRegistrySubKeyNames(baseKey, registryKeyRoot);
            if (versions != null)
            {
                ArrayList list = GatherVersionStrings(targetRuntimeVersion, versions);
                ArrayList list2 = new ArrayList();
                ReverseVersionComparer comparer = ReverseVersionComparer.Comparer;
                foreach (string str in list)
                {
                    string subKey = registryKeyRoot + @"\" + str + @"\" + registryKeySuffix;
                    IEnumerable enumerable2 = getRegistrySubKeyNames(baseKey, subKey);
                    ArrayList list3 = new ArrayList();
                    foreach (string str3 in enumerable2)
                    {
                        list3.Add(str3);
                    }
                    list3.Sort(comparer);
                    foreach (string str4 in list3)
                    {
                        list2.Add(subKey + @"\" + str4);
                    }
                }
                ArrayList list4 = new ArrayList();
                foreach (string str5 in list2)
                {
                    IEnumerable enumerable3 = getRegistrySubKeyNames(baseKey, str5);
                    ArrayList c = new ArrayList();
                    foreach (string str6 in enumerable3)
                    {
                        c.Add(str5 + @"\" + str6);
                    }
                    c.Sort(comparer);
                    list4.AddRange(c);
                    list4.Add(str5);
                }
                foreach (string str7 in list4)
                {
                    if (!string.IsNullOrEmpty(platform) || !string.IsNullOrEmpty(osVersion))
                    {
                        RegistryKey keyPlatform = baseKey.OpenSubKey(str7, false);
                        if ((keyPlatform != null) && (keyPlatform.ValueCount > 0))
                        {
                            if ((platform != null) && (platform.Length > 0))
                            {
                                string str8 = keyPlatform.GetValue("Platform", null) as string;
                                if (!string.IsNullOrEmpty(str8) && !this.MatchingPlatformExists(platform, str8))
                                {
                                    continue;
                                }
                            }
                            if ((osVersion != null) && (osVersion.Length > 0))
                            {
                                Version v = VersionUtilities.ConvertToVersion(osVersion);
                                if (!this.IsVersionInsideRange(v, keyPlatform))
                                {
                                    continue;
                                }
                            }
                        }
                    }
                    string str9 = getRegistrySubKeyDefaultValue(baseKey, str7);
                    if (str9 != null)
                    {
                        this.directoryNames.Add(str9);
                    }
                }
            }
        }

        private void FindUnderRegistryHive(RegistryView view, string registryKeyRoot, string targetRuntimeVersion, string registryKeySuffix, string osVersion, string platform, GetRegistrySubKeyNames getRegistrySubKeyNames, GetRegistrySubKeyDefaultValue getRegistrySubKeyDefaultValue, OpenBaseKey openBaseKey)
        {
            this.FindDirectories(view, RegistryHive.CurrentUser, registryKeyRoot, targetRuntimeVersion, registryKeySuffix, osVersion, platform, getRegistrySubKeyNames, getRegistrySubKeyDefaultValue, openBaseKey);
            this.FindDirectories(view, RegistryHive.LocalMachine, registryKeyRoot, targetRuntimeVersion, registryKeySuffix, osVersion, platform, getRegistrySubKeyNames, getRegistrySubKeyDefaultValue, openBaseKey);
        }

        internal static ArrayList GatherVersionStrings(string targetRuntimeVersion, IEnumerable versions)
        {
            ArrayList c = new ArrayList();
            Version version = VersionUtilities.ConvertToVersion(targetRuntimeVersion);
            ArrayList list2 = new ArrayList();
            SortedDictionary<Version, ArrayList> targetFrameworkVersionToRegistryVersions = new SortedDictionary<Version, ArrayList>(ReverseVersionGenericComparer.Comparer);
            foreach (string str in versions)
            {
                if ((str.Length > 0) && (string.Compare(str.Substring(0, 1), "v", StringComparison.OrdinalIgnoreCase) == 0))
                {
                    Version version2 = VersionUtilities.ConvertToVersion(str);
                    if (version2 == null)
                    {
                        if (string.Compare(str, 0, targetRuntimeVersion, 0, targetRuntimeVersion.Length, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            c.Add(str);
                        }
                    }
                    else
                    {
                        Version candidateVersion = null;
                        if (version2.Build > 0xff)
                        {
                            candidateVersion = new Version(version2.Major, version2.Minor);
                        }
                        else if (version2.Revision != -1)
                        {
                            candidateVersion = new Version(version2.Major, version2.Minor, version2.Build);
                        }
                        else
                        {
                            candidateVersion = version2;
                        }
                        bool flag = false;
                        if ((version == null) && (string.Compare(str, 0, targetRuntimeVersion, 0, targetRuntimeVersion.Length, StringComparison.OrdinalIgnoreCase) == 0))
                        {
                            flag = true;
                        }
                        bool flag2 = (version != null) && (version >= candidateVersion);
                        if ((candidateVersion != null) && (flag2 || flag))
                        {
                            AddCandidateVersion(targetFrameworkVersionToRegistryVersions, str, candidateVersion);
                        }
                    }
                }
            }
            foreach (KeyValuePair<Version, ArrayList> pair in targetFrameworkVersionToRegistryVersions)
            {
                ArrayList list3 = pair.Value;
                list3.Sort(ReverseVersionComparer.Comparer);
                foreach (string str2 in list3)
                {
                    list2.Add(str2);
                }
            }
            list2.AddRange(c);
            return list2;
        }

        public IEnumerator GetEnumerator()
        {
            return this.directoryNames.GetEnumerator();
        }

        private bool IsVersionInsideRange(Version v, RegistryKey keyPlatform)
        {
            bool flag = true;
            if (v != null)
            {
                string str = keyPlatform.GetValue("MinOSVersion", null) as string;
                Version version = (str == null) ? null : VersionUtilities.ConvertToVersion(str);
                if ((version != null) && (version > v))
                {
                    flag = false;
                }
                string str2 = keyPlatform.GetValue("MaxOSVersion", null) as string;
                Version version2 = (str2 == null) ? null : VersionUtilities.ConvertToVersion(str2);
                if ((version2 != null) && (version2 < v))
                {
                    flag = false;
                }
            }
            return flag;
        }

        private bool MatchingPlatformExists(string platform, string platformValue)
        {
            if ((platformValue != null) && (platformValue.Length > 0))
            {
                foreach (string str in platformValue.Split(new char[] { ';' }))
                {
                    if (string.Compare(str, platform, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}

