namespace Microsoft.Build.Utilities
{
    using Microsoft.Build.Framework;
    using System;
    using System.Runtime;
    using System.Runtime.InteropServices;

    internal static class CanonicalTrackedFilesHelper
    {
        internal const int MaxLogCount = 100;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal static bool FilesExistAndRecordNewestWriteTime(ITaskItem[] files, TaskLoggingHelper log, out DateTime outputNewestTime, out string outputNewestFilename)
        {
            return FilesExistAndRecordRequestedWriteTime(files, log, true, out outputNewestTime, out outputNewestFilename);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal static bool FilesExistAndRecordOldestWriteTime(ITaskItem[] files, TaskLoggingHelper log, out DateTime outputOldestTime, out string outputOldestFilename)
        {
            return FilesExistAndRecordRequestedWriteTime(files, log, false, out outputOldestTime, out outputOldestFilename);
        }

        private static bool FilesExistAndRecordRequestedWriteTime(ITaskItem[] files, TaskLoggingHelper log, bool getNewest, out DateTime requestedTime, out string requestedFilename)
        {
            requestedTime = getNewest ? DateTime.MinValue : DateTime.MaxValue;
            requestedFilename = string.Empty;
            if ((files == null) || (files.Length == 0))
            {
                return false;
            }
            foreach (ITaskItem item in files)
            {
                DateTime lastWriteTimeUtc = Microsoft.Build.Utilities.NativeMethods.GetLastWriteTimeUtc(item.ItemSpec);
                if (lastWriteTimeUtc == DateTime.MinValue)
                {
                    FileTracker.LogMessageFromResources(log, MessageImportance.Low, "Tracking_OutputDoesNotExist", new object[] { item.ItemSpec });
                    return false;
                }
                if ((getNewest && (lastWriteTimeUtc > requestedTime)) || (!getNewest && (lastWriteTimeUtc < requestedTime)))
                {
                    requestedTime = lastWriteTimeUtc;
                    requestedFilename = item.ItemSpec;
                }
            }
            return true;
        }

        internal static bool RootContainsAllSubRootComponents(string compositeRoot, string compositeSubRoot)
        {
            bool flag = true;
            if (string.Compare(compositeRoot, compositeSubRoot, StringComparison.OrdinalIgnoreCase) == 0)
            {
                return true;
            }
            foreach (string str in compositeSubRoot.Split(new char[] { '|' }))
            {
                flag &= compositeRoot.Contains(str);
                if (!flag)
                {
                    return flag;
                }
            }
            return flag;
        }
    }
}

