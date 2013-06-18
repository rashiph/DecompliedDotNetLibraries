namespace Microsoft.Build.Utilities
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Shared;
    using System;
    using System.Collections.Generic;
    using System.IO;

    public static class TrackedDependencies
    {
        public static ITaskItem[] ExpandWildcards(ITaskItem[] expand)
        {
            if (expand == null)
            {
                return null;
            }
            List<ITaskItem> list = new List<ITaskItem>(expand.Length);
            foreach (ITaskItem item in expand)
            {
                if (FileMatcher.HasWildcards(item.ItemSpec))
                {
                    string[] files;
                    string directoryName = Path.GetDirectoryName(item.ItemSpec);
                    string fileName = Path.GetFileName(item.ItemSpec);
                    if (!FileMatcher.HasWildcards(directoryName) && Directory.Exists(directoryName))
                    {
                        files = Directory.GetFiles(directoryName, fileName);
                    }
                    else
                    {
                        files = FileMatcher.GetFiles(null, item.ItemSpec);
                    }
                    foreach (string str3 in files)
                    {
                        TaskItem item2 = new TaskItem(item) {
                            ItemSpec = str3
                        };
                        list.Add(item2);
                    }
                }
                else
                {
                    list.Add(item);
                }
            }
            return list.ToArray();
        }

        internal static bool ItemsExist(ITaskItem[] files)
        {
            bool flag = true;
            if ((files != null) && (files.Length > 0))
            {
                foreach (ITaskItem item in files)
                {
                    if (!FileUtilities.FileExistsNoThrow(item.ItemSpec))
                    {
                        return false;
                    }
                }
                return flag;
            }
            return false;
        }
    }
}

