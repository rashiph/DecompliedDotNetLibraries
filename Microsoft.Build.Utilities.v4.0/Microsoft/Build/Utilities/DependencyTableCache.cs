namespace Microsoft.Build.Utilities
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Shared;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime;
    using System.Text;

    internal static class DependencyTableCache
    {
        private static Dictionary<string, DependencyTableCacheEntry> dependencyTableCache = new Dictionary<string, DependencyTableCacheEntry>(StringComparer.OrdinalIgnoreCase);
        private static readonly char[] numerals = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
        private static TaskItemItemSpecIgnoreCaseComparer taskItemComparer = new TaskItemItemSpecIgnoreCaseComparer();

        private static bool DependencyTableIsUpToDate(DependencyTableCacheEntry dependencyTable)
        {
            DateTime tableTime = dependencyTable.TableTime;
            foreach (ITaskItem item in dependencyTable.TlogFiles)
            {
                if (NativeMethods.GetLastWriteTimeUtc(FileUtilities.NormalizePath(item.ItemSpec)) > tableTime)
                {
                    return false;
                }
            }
            return true;
        }

        internal static string FormatNormalizedTlogRootingMarker(ITaskItem[] tlogFiles)
        {
            HashSet<ITaskItem> source = new HashSet<ITaskItem>(taskItemComparer);
            for (int i = 0; i < tlogFiles.Length; i++)
            {
                ITaskItem item = new TaskItem(tlogFiles[i]) {
                    ItemSpec = NormalizeTlogPath(tlogFiles[i].ItemSpec)
                };
                source.Add(item);
            }
            return FileTracker.FormatRootingMarker(source.ToArray<ITaskItem>());
        }

        internal static DependencyTableCacheEntry GetCachedEntry(string tLogRootingMarker)
        {
            if (DependencyTable.ContainsKey(tLogRootingMarker))
            {
                DependencyTableCacheEntry dependencyTable = DependencyTable[tLogRootingMarker];
                if (DependencyTableIsUpToDate(dependencyTable))
                {
                    return dependencyTable;
                }
                DependencyTable.Remove(tLogRootingMarker);
            }
            return null;
        }

        internal static string NormalizeTlogPath(string tlogPath)
        {
            if (tlogPath.IndexOfAny(numerals) == -1)
            {
                return tlogPath;
            }
            int num = 0;
            StringBuilder builder = new StringBuilder();
            num = tlogPath.Length - 1;
            while ((num >= 0) && (tlogPath[num] != '\\'))
            {
                if ((tlogPath[num] == '.') || (tlogPath[num] == '-'))
                {
                    builder.Append(tlogPath[num]);
                    int num2 = num - 1;
                    while (((num2 >= 0) && (tlogPath[num2] != '\\')) && ((tlogPath[num2] >= '0') && (tlogPath[num2] <= '9')))
                    {
                        num2--;
                    }
                    if ((num2 >= 0) && (tlogPath[num2] == '.'))
                    {
                        builder.Append("]DI[");
                        builder.Append(tlogPath[num2]);
                        num = num2;
                    }
                }
                else
                {
                    builder.Append(tlogPath[num]);
                }
                num--;
            }
            StringBuilder builder2 = new StringBuilder(num + builder.Length);
            if (num >= 0)
            {
                builder2.Append(tlogPath.Substring(0, num + 1));
            }
            for (int i = builder.Length - 1; i >= 0; i--)
            {
                builder2.Append(builder[i]);
            }
            return builder2.ToString();
        }

        internal static Dictionary<string, DependencyTableCacheEntry> DependencyTable
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return dependencyTableCache;
            }
        }

        private class TaskItemItemSpecIgnoreCaseComparer : IEqualityComparer<ITaskItem>
        {
            public bool Equals(ITaskItem x, ITaskItem y)
            {
                return (object.ReferenceEquals(x, y) || ((!object.ReferenceEquals(x, null) && !object.ReferenceEquals(y, null)) && string.Equals(x.ItemSpec, y.ItemSpec, StringComparison.OrdinalIgnoreCase)));
            }

            public int GetHashCode(ITaskItem obj)
            {
                if (obj == null)
                {
                    return 0;
                }
                return StringComparer.OrdinalIgnoreCase.GetHashCode(obj.ItemSpec);
            }
        }
    }
}

