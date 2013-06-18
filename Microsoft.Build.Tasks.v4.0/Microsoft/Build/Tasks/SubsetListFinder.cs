namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Shared;
    using System;
    using System.Collections.Generic;
    using System.IO;

    internal class SubsetListFinder
    {
        private const string subsetListFolder = "SubsetList";
        private static Dictionary<string, string[]> subsetListPathCache;
        private static object subsetListPathCacheLock = new object();
        private string[] subsetToSearchFor;

        internal SubsetListFinder(string[] subsetToSearchFor)
        {
            Microsoft.Build.Shared.ErrorUtilities.VerifyThrowArgumentNull(subsetToSearchFor, "subsetToSearchFor");
            this.subsetToSearchFor = subsetToSearchFor;
        }

        public string[] GetSubsetListPathsFromDisk(string frameworkDirectory)
        {
            Microsoft.Build.Shared.ErrorUtilities.VerifyThrowArgumentNull(frameworkDirectory, "frameworkDirectory");
            if (this.subsetToSearchFor.Length > 0)
            {
                lock (subsetListPathCacheLock)
                {
                    if (subsetListPathCache == null)
                    {
                        subsetListPathCache = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
                    }
                    string str = string.Join(";", this.subsetToSearchFor);
                    string key = frameworkDirectory + ":" + str;
                    string[] strArray = null;
                    subsetListPathCache.TryGetValue(key, out strArray);
                    if (strArray != null)
                    {
                        return strArray;
                    }
                    string str3 = Path.Combine(frameworkDirectory, "SubsetList");
                    List<string> list = new List<string>();
                    foreach (string str4 in this.subsetToSearchFor)
                    {
                        string path = Path.Combine(str3, str4 + ".xml");
                        if (File.Exists(path))
                        {
                            list.Add(path);
                        }
                    }
                    subsetListPathCache[key] = list.ToArray();
                    return subsetListPathCache[key];
                }
            }
            return new string[0];
        }

        public static string SubsetListFolder
        {
            get
            {
                return "SubsetList";
            }
        }
    }
}

