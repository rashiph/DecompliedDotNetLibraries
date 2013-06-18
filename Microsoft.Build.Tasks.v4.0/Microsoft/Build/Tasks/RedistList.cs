namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Shared;
    using Microsoft.Build.Utilities;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Xml;

    internal sealed class RedistList
    {
        private readonly List<AssemblyEntry> assemblyList = new List<AssemblyEntry>();
        private Dictionary<AssemblyNameExtension, NGen<bool>> assemblyNameInRedist = new Dictionary<AssemblyNameExtension, NGen<bool>>(AssemblyNameComparer.genericComparer);
        private Dictionary<string, Hashtable> cachedBlackList = new Dictionary<string, Hashtable>(StringComparer.OrdinalIgnoreCase);
        private string cachedKey = string.Empty;
        private static readonly Hashtable cachedRedistList = new Hashtable(StringComparer.OrdinalIgnoreCase);
        private static object cachedRedistListLock = new object();
        private AssemblyEntry cachedValue;
        private ArrayList errorFilenames = new ArrayList();
        private ArrayList errors = new ArrayList();
        private const string matchPattern = "*.xml";
        private const string redistListFolder = "RedistList";
        private static Dictionary<string, string[]> redistListPathCache;
        private static object redistListPathCacheLock = new object();
        private readonly Hashtable simpleNameMap = new Hashtable(StringComparer.OrdinalIgnoreCase);
        private static readonly IComparer<AssemblyEntry> sortByVersionDescending = new SortByVersionDescending();
        private ArrayList whiteListErrorFilenames = new ArrayList();
        private ArrayList whiteListErrors = new ArrayList();

        private RedistList(AssemblyTableInfo[] assemblyTableInfos)
        {
            if (assemblyTableInfos == null)
            {
                throw new ArgumentNullException("assemblyTableInfos");
            }
            foreach (AssemblyTableInfo info in assemblyTableInfos)
            {
                ReadFile(info, this.assemblyList, this.errors, this.errorFilenames);
            }
            this.BuildMap();
        }

        private void BuildMap()
        {
            this.assemblyList.Sort(sortByVersionDescending);
            for (int i = 0; i < this.assemblyList.Count; i++)
            {
                AssemblyEntry entry = this.assemblyList[i];
                if (!this.simpleNameMap.ContainsKey(entry.SimpleName))
                {
                    this.simpleNameMap.Add(entry.SimpleName, i);
                }
            }
        }

        internal AssemblyEntry[] FindAssemblyNameFromSimpleName(string simpleName)
        {
            List<AssemblyEntry> list = new List<AssemblyEntry>();
            if (this.simpleNameMap.ContainsKey(simpleName))
            {
                int num = (int) this.simpleNameMap[simpleName];
                for (int i = num; i < this.assemblyList.Count; i++)
                {
                    AssemblyEntry item = this.assemblyList[i];
                    if (!string.Equals(simpleName, item.SimpleName, StringComparison.OrdinalIgnoreCase))
                    {
                        break;
                    }
                    list.Add(item);
                    this.cachedKey = item.FullName;
                    this.cachedValue = item;
                }
            }
            return list.ToArray();
        }

        public bool FrameworkAssemblyEntryInRedist(AssemblyNameExtension assemblyName)
        {
            Microsoft.Build.Shared.ErrorUtilities.VerifyThrowArgumentNull(assemblyName, "assemblyName");
            NGen<bool> gen = 0;
            if (!this.assemblyNameInRedist.TryGetValue(assemblyName, out gen))
            {
                string simpleName = GetSimpleName(assemblyName.Name);
                if (this.simpleNameMap.ContainsKey(simpleName))
                {
                    int num = (int) this.simpleNameMap[simpleName];
                    for (int i = num; i < this.assemblyList.Count; i++)
                    {
                        AssemblyEntry entry = this.assemblyList[i];
                        if (!string.Equals(simpleName, entry.SimpleName, StringComparison.OrdinalIgnoreCase))
                        {
                            break;
                        }
                        if (entry.RedistName.StartsWith("Microsoft-Windows-CLRCoreComp", StringComparison.OrdinalIgnoreCase))
                        {
                            AssemblyNameExtension extension = assemblyName;
                            AssemblyNameExtension assemblyNameExtension = entry.AssemblyNameExtension;
                            if (extension.PartialNameCompare(assemblyNameExtension, PartialComparisonFlags.PublicKeyToken | PartialComparisonFlags.Culture | PartialComparisonFlags.SimpleName))
                            {
                                gen = 1;
                                break;
                            }
                        }
                    }
                }
                this.assemblyNameInRedist.Add(assemblyName, gen);
            }
            return (bool) gen;
        }

        internal Hashtable GenerateBlackList(AssemblyTableInfo[] whiteListAssemblyTableInfo)
        {
            if (this.assemblyList.Count == 0)
            {
                return null;
            }
            Array.Sort<AssemblyTableInfo>(whiteListAssemblyTableInfo);
            StringBuilder builder = (whiteListAssemblyTableInfo.Length > 0) ? new StringBuilder(whiteListAssemblyTableInfo[0].Descriptor) : new StringBuilder();
            for (int i = 1; i < whiteListAssemblyTableInfo.Length; i++)
            {
                builder.Append(';');
                builder.Append(whiteListAssemblyTableInfo[i].Descriptor);
            }
            string key = builder.ToString();
            Hashtable hashtable = null;
            this.cachedBlackList.TryGetValue(key, out hashtable);
            if (hashtable != null)
            {
                return hashtable;
            }
            List<AssemblyEntry> list = new List<AssemblyEntry>();
            Hashtable hashtable2 = new Hashtable(StringComparer.OrdinalIgnoreCase);
            foreach (AssemblyTableInfo info in whiteListAssemblyTableInfo)
            {
                List<AssemblyEntry> assembliesList = new List<AssemblyEntry>();
                int count = this.whiteListErrors.Count;
                string str2 = ReadFile(info, assembliesList, this.whiteListErrors, this.whiteListErrorFilenames);
                if (!string.IsNullOrEmpty(str2))
                {
                    list.AddRange(assembliesList);
                    if (!hashtable2.ContainsKey(str2))
                    {
                        hashtable2[str2] = null;
                    }
                }
                else if (this.whiteListErrors.Count == count)
                {
                    this.whiteListErrors.Add(new Exception(Microsoft.Build.Shared.ResourceUtilities.FormatResourceString("ResolveAssemblyReference.NoSubSetRedistListName", new object[] { info.Path })));
                    this.whiteListErrorFilenames.Add(info.Path);
                }
            }
            Hashtable hashtable3 = new Hashtable(StringComparer.OrdinalIgnoreCase);
            bool flag = hashtable2.Count > 0;
            foreach (AssemblyEntry entry in this.assemblyList)
            {
                string fullName = entry.FullName;
                string redistName = entry.RedistName;
                if (!string.IsNullOrEmpty(redistName))
                {
                    string str5 = fullName + "," + redistName;
                    if ((flag && !hashtable3.ContainsKey(str5)) && hashtable2.ContainsKey(redistName))
                    {
                        hashtable3[str5] = fullName;
                    }
                }
            }
            foreach (AssemblyEntry entry2 in list)
            {
                hashtable3.Remove(entry2.FullName + "," + entry2.RedistName);
            }
            Hashtable hashtable4 = new Hashtable(StringComparer.OrdinalIgnoreCase);
            foreach (string str6 in hashtable3.Values)
            {
                hashtable4[str6] = null;
            }
            this.cachedBlackList[key] = hashtable4;
            return hashtable4;
        }

        public static RedistList GetFrameworkList20()
        {
            string pathToDotNetFramework = ToolLocationHelper.GetPathToDotNetFramework(TargetDotNetFrameworkVersion.Version20);
            string[] redistListPathsFromDisk = new string[0];
            if (pathToDotNetFramework != null)
            {
                redistListPathsFromDisk = GetRedistListPathsFromDisk(pathToDotNetFramework);
            }
            AssemblyTableInfo[] assemblyTables = new AssemblyTableInfo[redistListPathsFromDisk.Length];
            for (int i = 0; i < redistListPathsFromDisk.Length; i++)
            {
                assemblyTables[i] = new AssemblyTableInfo(redistListPathsFromDisk[i], pathToDotNetFramework);
            }
            return GetRedistList(assemblyTables);
        }

        public static RedistList GetFrameworkList30()
        {
            return GetFrameworkListFromReferenceAssembliesPath(TargetDotNetFrameworkVersion.Version30);
        }

        public static RedistList GetFrameworkList35()
        {
            return GetFrameworkListFromReferenceAssembliesPath(TargetDotNetFrameworkVersion.Version35);
        }

        private static RedistList GetFrameworkListFromReferenceAssembliesPath(TargetDotNetFrameworkVersion version)
        {
            string pathToDotNetFrameworkReferenceAssemblies = ToolLocationHelper.GetPathToDotNetFrameworkReferenceAssemblies(version);
            string[] strArray = (pathToDotNetFrameworkReferenceAssemblies == null) ? new string[0] : GetRedistListPathsFromDisk(pathToDotNetFrameworkReferenceAssemblies);
            AssemblyTableInfo[] assemblyTables = new AssemblyTableInfo[strArray.Length];
            for (int i = 0; i < strArray.Length; i++)
            {
                assemblyTables[i] = new AssemblyTableInfo(strArray[i], pathToDotNetFrameworkReferenceAssemblies);
            }
            return GetRedistList(assemblyTables);
        }

        public static RedistList GetRedistList(AssemblyTableInfo[] assemblyTables)
        {
            if (assemblyTables == null)
            {
                throw new ArgumentNullException("assemblyTables");
            }
            Array.Sort<AssemblyTableInfo>(assemblyTables);
            StringBuilder builder = (assemblyTables.Length > 0) ? new StringBuilder(assemblyTables[0].Descriptor) : new StringBuilder();
            for (int i = 1; i < assemblyTables.Length; i++)
            {
                builder.Append(';');
                builder.Append(assemblyTables[i].Descriptor);
            }
            string key = builder.ToString();
            lock (cachedRedistListLock)
            {
                if (cachedRedistList.ContainsKey(key))
                {
                    return (RedistList) cachedRedistList[key];
                }
                RedistList list = new RedistList(assemblyTables);
                cachedRedistList.Add(key, list);
                return list;
            }
        }

        public static RedistList GetRedistListFromPath(string path)
        {
            string[] strArray = (path == null) ? new string[0] : GetRedistListPathsFromDisk(path);
            AssemblyTableInfo[] assemblyTables = new AssemblyTableInfo[strArray.Length];
            for (int i = 0; i < strArray.Length; i++)
            {
                assemblyTables[i] = new AssemblyTableInfo(strArray[i], path);
            }
            return GetRedistList(assemblyTables);
        }

        public static string[] GetRedistListPathsFromDisk(string frameworkDirectory)
        {
            Microsoft.Build.Shared.ErrorUtilities.VerifyThrowArgumentNull(frameworkDirectory, "frameworkDirectory");
            lock (redistListPathCacheLock)
            {
                if (redistListPathCache == null)
                {
                    redistListPathCache = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
                }
                if (!redistListPathCache.ContainsKey(frameworkDirectory))
                {
                    string path = Path.Combine(frameworkDirectory, "RedistList");
                    if (Directory.Exists(path))
                    {
                        string[] files = Directory.GetFiles(path, "*.xml");
                        redistListPathCache.Add(frameworkDirectory, files);
                        return redistListPathCache[frameworkDirectory];
                    }
                }
                else
                {
                    return redistListPathCache[frameworkDirectory];
                }
            }
            return new string[0];
        }

        private static string GetSimpleName(string assemblyName)
        {
            if (assemblyName == null)
            {
                throw new ArgumentNullException("assemblyName");
            }
            int index = assemblyName.IndexOf(",", StringComparison.Ordinal);
            if (index <= 0)
            {
                return assemblyName;
            }
            return assemblyName.Substring(0, index);
        }

        private AssemblyEntry GetUnifiedAssemblyEntry(string assemblyName)
        {
            if (assemblyName == null)
            {
                throw new ArgumentNullException("assemblyName");
            }
            if (!string.Equals(this.cachedKey, assemblyName, StringComparison.OrdinalIgnoreCase))
            {
                this.cachedKey = string.Empty;
                this.cachedValue = null;
                string simpleName = GetSimpleName(assemblyName);
                if (this.simpleNameMap.ContainsKey(simpleName))
                {
                    int num = (int) this.simpleNameMap[simpleName];
                    AssemblyNameExtension extension = new AssemblyNameExtension(this.assemblyList[num].FullName);
                    for (int i = num; i < this.assemblyList.Count; i++)
                    {
                        AssemblyEntry entry = this.assemblyList[i];
                        if (!string.Equals(simpleName, entry.SimpleName, StringComparison.OrdinalIgnoreCase))
                        {
                            break;
                        }
                        AssemblyNameExtension extension2 = new AssemblyNameExtension(assemblyName);
                        AssemblyNameExtension assemblyNameExtension = entry.AssemblyNameExtension;
                        if (extension2.EqualsIgnoreVersion(assemblyNameExtension) && (extension.Version <= assemblyNameExtension.Version))
                        {
                            this.cachedKey = assemblyName;
                            this.cachedValue = entry;
                            break;
                        }
                    }
                }
            }
            return this.cachedValue;
        }

        public string GetUnifiedAssemblyName(string assemblyName)
        {
            AssemblyEntry unifiedAssemblyEntry = this.GetUnifiedAssemblyEntry(assemblyName);
            if (unifiedAssemblyEntry != null)
            {
                return unifiedAssemblyEntry.FullName;
            }
            return assemblyName;
        }

        public bool IsFrameworkAssembly(string assemblyName)
        {
            AssemblyEntry unifiedAssemblyEntry = this.GetUnifiedAssemblyEntry(assemblyName);
            return (((unifiedAssemblyEntry != null) && !string.IsNullOrEmpty(unifiedAssemblyEntry.RedistName)) && unifiedAssemblyEntry.RedistName.StartsWith("Microsoft-Windows-CLRCoreComp", StringComparison.OrdinalIgnoreCase));
        }

        public bool IsPrerequisiteAssembly(string assemblyName)
        {
            AssemblyEntry unifiedAssemblyEntry = this.GetUnifiedAssemblyEntry(assemblyName);
            return ((unifiedAssemblyEntry != null) && unifiedAssemblyEntry.InGAC);
        }

        internal bool? IsRedistRoot(string assemblyName)
        {
            AssemblyEntry unifiedAssemblyEntry = this.GetUnifiedAssemblyEntry(assemblyName);
            if (unifiedAssemblyEntry != null)
            {
                return unifiedAssemblyEntry.IsRedistRoot;
            }
            return null;
        }

        internal static string ReadFile(AssemblyTableInfo assemblyTableInfo, List<AssemblyEntry> assembliesList, ArrayList errorsList, ArrayList errorFilenamesList)
        {
            string path = assemblyTableInfo.Path;
            string redistName = null;
            XmlTextReader reader = null;
            Dictionary<string, AssemblyEntry> dictionary = new Dictionary<string, AssemblyEntry>(StringComparer.OrdinalIgnoreCase);
            try
            {
                reader = new XmlTextReader(path);
                bool flag = false;
                while (reader.Read())
                {
                    if ((reader.NodeType != XmlNodeType.Element) || !string.Equals(reader.Name, "FileList", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                    flag = true;
                    reader.MoveToFirstAttribute();
                    do
                    {
                        if (string.Equals(reader.Name, "Redist", StringComparison.OrdinalIgnoreCase))
                        {
                            redistName = reader.Value;
                            break;
                        }
                    }
                    while (reader.MoveToNextAttribute());
                    reader.MoveToElement();
                    break;
                }
                if (flag)
                {
                    flag = false;
                    while (reader.Read())
                    {
                        if ((reader.NodeType == XmlNodeType.Element) && string.Equals(reader.Name, "File", StringComparison.OrdinalIgnoreCase))
                        {
                            string str3;
                            string str4;
                            string str5;
                            string str6;
                            string str7;
                            string str8;
                            bool flag2;
                            bool flag3;
                            flag = true;
                            Dictionary<string, string> dictionary2 = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                            reader.MoveToFirstAttribute();
                            do
                            {
                                dictionary2.Add(reader.Name, reader.Value);
                            }
                            while (reader.MoveToNextAttribute());
                            dictionary2.TryGetValue("AssemblyName", out str3);
                            dictionary2.TryGetValue("Version", out str4);
                            dictionary2.TryGetValue("PublicKeyToken", out str5);
                            dictionary2.TryGetValue("Culture", out str6);
                            dictionary2.TryGetValue("InGAC", out str7);
                            dictionary2.TryGetValue("IsRedistRoot", out str8);
                            if (!bool.TryParse(str7, out flag2))
                            {
                                flag2 = true;
                            }
                            bool? isRedistRoot = null;
                            if (bool.TryParse(str8, out flag3))
                            {
                                isRedistRoot = new bool?(flag3);
                            }
                            if (((!string.IsNullOrEmpty(str3) && !string.IsNullOrEmpty(str4)) && !string.IsNullOrEmpty(str5)) && !string.IsNullOrEmpty(str6))
                            {
                                AssemblyEntry entry = new AssemblyEntry(str3, str4, str5, str6, flag2, isRedistRoot, redistName, assemblyTableInfo.FrameworkDirectory);
                                string key = string.Format(CultureInfo.InvariantCulture, "{0},{1}", new object[] { entry.FullName, !entry.IsRedistRoot.HasValue ? "null" : entry.IsRedistRoot.ToString() });
                                AssemblyEntry entry2 = null;
                                dictionary.TryGetValue(key, out entry2);
                                if ((entry2 == null) || ((entry2 != null) && entry.InGAC))
                                {
                                    dictionary[key] = entry;
                                }
                            }
                            reader.MoveToElement();
                        }
                    }
                }
            }
            catch (XmlException exception)
            {
                errorsList.Add(exception);
                errorFilenamesList.Add(path);
            }
            catch (Exception exception2)
            {
                if (Microsoft.Build.Shared.ExceptionHandling.NotExpectedException(exception2))
                {
                    throw;
                }
                errorsList.Add(exception2);
                errorFilenamesList.Add(path);
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
            }
            foreach (AssemblyEntry entry3 in dictionary.Values)
            {
                assembliesList.Add(entry3);
            }
            return redistName;
        }

        internal string RedistName(string assemblyName)
        {
            AssemblyEntry unifiedAssemblyEntry = this.GetUnifiedAssemblyEntry(assemblyName);
            if (unifiedAssemblyEntry != null)
            {
                return unifiedAssemblyEntry.RedistName;
            }
            return null;
        }

        internal int Count
        {
            get
            {
                return this.assemblyList.Count;
            }
        }

        internal string[] ErrorFileNames
        {
            get
            {
                return (string[]) this.errorFilenames.ToArray(typeof(string));
            }
        }

        internal Exception[] Errors
        {
            get
            {
                return (Exception[]) this.errors.ToArray(typeof(Exception));
            }
        }

        internal static string RedistListFolder
        {
            get
            {
                return "RedistList";
            }
        }

        internal string[] WhiteListErrorFileNames
        {
            get
            {
                return (string[]) this.whiteListErrorFilenames.ToArray(typeof(string));
            }
        }

        internal Exception[] WhiteListErrors
        {
            get
            {
                return (Exception[]) this.whiteListErrors.ToArray(typeof(Exception));
            }
        }

        internal class SortByVersionDescending : IComparer, IComparer<AssemblyEntry>
        {
            public int Compare(AssemblyEntry firstEntry, AssemblyEntry secondEntry)
            {
                if ((firstEntry == null) || (secondEntry == null))
                {
                    return 0;
                }
                AssemblyNameExtension assemblyNameExtension = firstEntry.AssemblyNameExtension;
                AssemblyNameExtension extension2 = secondEntry.AssemblyNameExtension;
                int num = string.Compare(assemblyNameExtension.Name, extension2.Name, StringComparison.OrdinalIgnoreCase);
                if (num != 0)
                {
                    return num;
                }
                int num2 = assemblyNameExtension.Version.CompareTo(extension2.Version);
                if (num2 == 0)
                {
                    return 0;
                }
                return -num2;
            }

            public int Compare(object a, object b)
            {
                AssemblyEntry firstEntry = a as AssemblyEntry;
                AssemblyEntry secondEntry = b as AssemblyEntry;
                return this.Compare(firstEntry, secondEntry);
            }
        }
    }
}

