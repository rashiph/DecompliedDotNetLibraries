namespace Microsoft.Build.Utilities
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Shared;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Resources;
    using System.Runtime;
    using System.Text;

    public class FlatTrackingData
    {
        private IDictionary<string, DateTime> dependencyTable;
        private List<string> excludedInputPaths;
        private IDictionary<string, DateTime> lastWriteTimeUtcCache;
        private TaskLoggingHelper Log;
        private const int MaxLogCount = 100;
        private List<string> missingFiles;
        private DateTime missingFileTimeUtc;
        private string newestFileName;
        private DateTime newestFileTimeUtc;
        private string newestTLogFileName;
        private DateTime newestTLogTimeUtc;
        private string oldestFileName;
        private DateTime oldestFileTimeUtc;
        private bool skipMissingFiles;
        private ITaskItem[] tlogFiles;
        private string tlogMarker;
        private bool tlogsAvailable;
        private bool treatRootMarkersAsEntries;

        public FlatTrackingData(ITaskItem[] tlogFiles, bool skipMissingFiles)
        {
            this.dependencyTable = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);
            this.tlogMarker = string.Empty;
            this.oldestFileName = string.Empty;
            this.oldestFileTimeUtc = DateTime.MaxValue;
            this.newestFileName = string.Empty;
            this.newestFileTimeUtc = DateTime.MinValue;
            this.missingFileTimeUtc = DateTime.MinValue;
            this.missingFiles = new List<string>();
            this.newestTLogTimeUtc = DateTime.MinValue;
            this.newestTLogFileName = string.Empty;
            this.lastWriteTimeUtcCache = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);
            this.excludedInputPaths = new List<string>();
            this.InternalConstruct(null, tlogFiles, null, skipMissingFiles, DateTime.MinValue, null);
        }

        public FlatTrackingData(ITaskItem[] tlogFiles, DateTime missingFileTimeUtc)
        {
            this.dependencyTable = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);
            this.tlogMarker = string.Empty;
            this.oldestFileName = string.Empty;
            this.oldestFileTimeUtc = DateTime.MaxValue;
            this.newestFileName = string.Empty;
            this.newestFileTimeUtc = DateTime.MinValue;
            this.missingFileTimeUtc = DateTime.MinValue;
            this.missingFiles = new List<string>();
            this.newestTLogTimeUtc = DateTime.MinValue;
            this.newestTLogFileName = string.Empty;
            this.lastWriteTimeUtcCache = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);
            this.excludedInputPaths = new List<string>();
            this.InternalConstruct(null, tlogFiles, null, false, missingFileTimeUtc, null);
        }

        public FlatTrackingData(ITask ownerTask, ITaskItem[] tlogFiles, bool skipMissingFiles)
        {
            this.dependencyTable = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);
            this.tlogMarker = string.Empty;
            this.oldestFileName = string.Empty;
            this.oldestFileTimeUtc = DateTime.MaxValue;
            this.newestFileName = string.Empty;
            this.newestFileTimeUtc = DateTime.MinValue;
            this.missingFileTimeUtc = DateTime.MinValue;
            this.missingFiles = new List<string>();
            this.newestTLogTimeUtc = DateTime.MinValue;
            this.newestTLogFileName = string.Empty;
            this.lastWriteTimeUtcCache = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);
            this.excludedInputPaths = new List<string>();
            this.InternalConstruct(ownerTask, tlogFiles, null, skipMissingFiles, DateTime.MinValue, null);
        }

        public FlatTrackingData(ITaskItem[] tlogFiles, ITaskItem[] tlogFilesToIgnore, DateTime missingFileTimeUtc)
        {
            this.dependencyTable = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);
            this.tlogMarker = string.Empty;
            this.oldestFileName = string.Empty;
            this.oldestFileTimeUtc = DateTime.MaxValue;
            this.newestFileName = string.Empty;
            this.newestFileTimeUtc = DateTime.MinValue;
            this.missingFileTimeUtc = DateTime.MinValue;
            this.missingFiles = new List<string>();
            this.newestTLogTimeUtc = DateTime.MinValue;
            this.newestTLogFileName = string.Empty;
            this.lastWriteTimeUtcCache = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);
            this.excludedInputPaths = new List<string>();
            this.InternalConstruct(null, tlogFiles, tlogFilesToIgnore, false, missingFileTimeUtc, null);
        }

        public FlatTrackingData(ITask ownerTask, ITaskItem[] tlogFiles, DateTime missingFileTimeUtc)
        {
            this.dependencyTable = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);
            this.tlogMarker = string.Empty;
            this.oldestFileName = string.Empty;
            this.oldestFileTimeUtc = DateTime.MaxValue;
            this.newestFileName = string.Empty;
            this.newestFileTimeUtc = DateTime.MinValue;
            this.missingFileTimeUtc = DateTime.MinValue;
            this.missingFiles = new List<string>();
            this.newestTLogTimeUtc = DateTime.MinValue;
            this.newestTLogFileName = string.Empty;
            this.lastWriteTimeUtcCache = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);
            this.excludedInputPaths = new List<string>();
            this.InternalConstruct(ownerTask, tlogFiles, null, false, missingFileTimeUtc, null);
        }

        public FlatTrackingData(ITaskItem[] tlogFiles, ITaskItem[] tlogFilesToIgnore, DateTime missingFileTimeUtc, string[] excludedInputPaths, IDictionary<string, DateTime> sharedLastWriteTimeUtcCache)
        {
            this.dependencyTable = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);
            this.tlogMarker = string.Empty;
            this.oldestFileName = string.Empty;
            this.oldestFileTimeUtc = DateTime.MaxValue;
            this.newestFileName = string.Empty;
            this.newestFileTimeUtc = DateTime.MinValue;
            this.missingFileTimeUtc = DateTime.MinValue;
            this.missingFiles = new List<string>();
            this.newestTLogTimeUtc = DateTime.MinValue;
            this.newestTLogFileName = string.Empty;
            this.lastWriteTimeUtcCache = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);
            this.excludedInputPaths = new List<string>();
            if (sharedLastWriteTimeUtcCache != null)
            {
                this.lastWriteTimeUtcCache = sharedLastWriteTimeUtcCache;
            }
            this.InternalConstruct(null, tlogFiles, tlogFilesToIgnore, false, missingFileTimeUtc, excludedInputPaths);
        }

        private void ConstructFileTable()
        {
            string tLogRootingMarker = null;
            bool flag2;
            bool flag3;
            try
            {
                tLogRootingMarker = DependencyTableCache.FormatNormalizedTlogRootingMarker(this.tlogFiles);
            }
            catch (ArgumentException exception)
            {
                FileTracker.LogWarningWithCodeFromResources(this.Log, "Tracking_RebuildingDueToInvalidTLog", new object[] { exception.Message });
                return;
            }
            if (this.tlogsAvailable)
            {
                DependencyTableCacheEntry cachedEntry = null;
                lock (DependencyTableCache.DependencyTable)
                {
                    cachedEntry = DependencyTableCache.GetCachedEntry(tLogRootingMarker);
                }
                if (cachedEntry != null)
                {
                    this.dependencyTable = (IDictionary<string, DateTime>) cachedEntry.DependencyTable;
                    this.UpdateFileEntryDetails();
                    FileTracker.LogMessageFromResources(this.Log, MessageImportance.Low, "Tracking_TrackingCached", new object[0]);
                    foreach (ITaskItem item in cachedEntry.TlogFiles)
                    {
                        FileTracker.LogMessage(this.Log, MessageImportance.Low, "\t{0}", new object[] { item.ItemSpec });
                    }
                    return;
                }
                FileTracker.LogMessageFromResources(this.Log, MessageImportance.Low, "Tracking_TrackingLogs", new object[0]);
                flag2 = false;
                flag3 = false;
                string itemSpec = null;
                foreach (ITaskItem item2 in this.tlogFiles)
                {
                    try
                    {
                        FileTracker.LogMessage(this.Log, MessageImportance.Low, "\t{0}", new object[] { item2.ItemSpec });
                        DateTime lastWriteTimeUtc = NativeMethods.GetLastWriteTimeUtc(item2.ItemSpec);
                        if (lastWriteTimeUtc > this.newestTLogTimeUtc)
                        {
                            this.newestTLogTimeUtc = lastWriteTimeUtc;
                            this.newestTLogFileName = item2.ItemSpec;
                        }
                        using (StreamReader reader = File.OpenText(item2.ItemSpec))
                        {
                            string key = reader.ReadLine();
                            while (key != null)
                            {
                                if (key.Length == 0)
                                {
                                    flag2 = true;
                                    itemSpec = item2.ItemSpec;
                                    goto Label_02B7;
                                }
                                if (key[0] == '#')
                                {
                                    key = reader.ReadLine();
                                    continue;
                                }
                                if ((key[0] == '^') && this.TreatRootMarkersAsEntries)
                                {
                                    key = key.Substring(1);
                                    if (key.Length != 0)
                                    {
                                        goto Label_0242;
                                    }
                                    flag2 = true;
                                    itemSpec = item2.ItemSpec;
                                    goto Label_02B7;
                                }
                                if (key[0] == '^')
                                {
                                    key = reader.ReadLine();
                                    continue;
                                }
                            Label_0242:
                                if (!this.dependencyTable.ContainsKey(key) && !FileTracker.FileIsExcludedFromDependencies(key))
                                {
                                    this.RecordEntryDetails(key, true);
                                }
                                key = reader.ReadLine();
                            }
                        }
                    }
                    catch (Exception exception2)
                    {
                        if (ExceptionHandling.NotExpectedException(exception2))
                        {
                            throw;
                        }
                        FileTracker.LogWarningWithCodeFromResources(this.Log, "Tracking_RebuildingDueToInvalidTLog", new object[] { exception2.Message });
                        break;
                    }
                Label_02B7:
                    if (flag2)
                    {
                        FileTracker.LogWarningWithCodeFromResources(this.Log, "Tracking_RebuildingDueToInvalidTLogContents", new object[] { itemSpec });
                        break;
                    }
                }
            }
            else
            {
                lock (DependencyTableCache.DependencyTable)
                {
                    if (DependencyTableCache.DependencyTable.ContainsKey(tLogRootingMarker))
                    {
                        DependencyTableCache.DependencyTable.Remove(tLogRootingMarker);
                    }
                }
                return;
            }
            lock (DependencyTableCache.DependencyTable)
            {
                if (flag2 || flag3)
                {
                    if (DependencyTableCache.DependencyTable.ContainsKey(tLogRootingMarker))
                    {
                        DependencyTableCache.DependencyTable.Remove(tLogRootingMarker);
                    }
                    this.dependencyTable = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);
                }
                else
                {
                    DependencyTableCache.DependencyTable[tLogRootingMarker] = new DependencyTableCacheEntry(this.tlogFiles, (IDictionary) this.dependencyTable);
                }
            }
        }

        public bool FileIsExcludedFromDependencyCheck(string fileName)
        {
            foreach (string str in this.excludedInputPaths)
            {
                if (fileName.StartsWith(str))
                {
                    return true;
                }
            }
            return false;
        }

        public static void FinalizeTLogs(bool trackedOperationsSucceeded, ITaskItem[] readTLogNames, ITaskItem[] writeTLogNames, ITaskItem[] trackedFilesToRemoveFromTLogs)
        {
            FlatTrackingData data = new FlatTrackingData(readTLogNames, true);
            FlatTrackingData data2 = new FlatTrackingData(writeTLogNames, true);
            if (!trackedOperationsSucceeded)
            {
                data.DependencyTable.Clear();
                data.SaveTlog();
                data2.DependencyTable.Clear();
                data2.SaveTlog();
            }
            else if ((trackedFilesToRemoveFromTLogs != null) && (trackedFilesToRemoveFromTLogs.Length > 0))
            {
                IDictionary<string, ITaskItem> trackedFilesToRemove = new Dictionary<string, ITaskItem>(StringComparer.OrdinalIgnoreCase);
                foreach (ITaskItem item in trackedFilesToRemoveFromTLogs)
                {
                    trackedFilesToRemove.Add(FileUtilities.NormalizePath(item.ItemSpec), item);
                }
                data2.SaveTlog(fullTrackedPath => !trackedFilesToRemove.ContainsKey(fullTrackedPath));
                data.SaveTlog(fullTrackedPath => !trackedFilesToRemove.ContainsKey(fullTrackedPath));
            }
            else
            {
                data2.SaveTlog();
                data.SaveTlog();
            }
        }

        public DateTime GetLastWriteTimeUtc(string file)
        {
            DateTime minValue = DateTime.MinValue;
            if (!this.lastWriteTimeUtcCache.TryGetValue(file, out minValue))
            {
                minValue = NativeMethods.GetLastWriteTimeUtc(file);
                this.lastWriteTimeUtcCache[file] = minValue;
            }
            return minValue;
        }

        private void InternalConstruct(ITask ownerTask, ITaskItem[] tlogFilesLocal, ITaskItem[] tlogFilesToIgnore, bool skipMissingFiles, DateTime missingFileTimeUtc, string[] excludedInputPaths)
        {
            if (ownerTask != null)
            {
                this.Log = new TaskLoggingHelper(ownerTask);
                this.Log.TaskResources = AssemblyResources.PrimaryResources;
                this.Log.HelpKeywordPrefix = "MSBuild.";
            }
            ITaskItem[] itemArray = TrackedDependencies.ExpandWildcards(tlogFilesLocal);
            if (tlogFilesToIgnore != null)
            {
                ITaskItem[] itemArray2 = TrackedDependencies.ExpandWildcards(tlogFilesToIgnore);
                if (itemArray2.Length > 0)
                {
                    HashSet<string> set = new HashSet<string>();
                    List<ITaskItem> list = new List<ITaskItem>();
                    foreach (ITaskItem item in itemArray2)
                    {
                        set.Add(item.ItemSpec);
                    }
                    foreach (ITaskItem item2 in itemArray)
                    {
                        if (!set.Contains(item2.ItemSpec))
                        {
                            list.Add(item2);
                        }
                    }
                    this.tlogFiles = list.ToArray();
                }
                else
                {
                    this.tlogFiles = itemArray;
                }
            }
            else
            {
                this.tlogFiles = itemArray;
            }
            if ((this.tlogFiles == null) || (this.tlogFiles.Length == 0))
            {
                this.tlogMarker = tlogFilesLocal[0].ItemSpec.Replace("*", "1");
                this.tlogMarker = this.tlogMarker.Replace("?", "2");
            }
            if (excludedInputPaths != null)
            {
                foreach (string str in excludedInputPaths)
                {
                    string str2 = FileUtilities.EnsureTrailingSlash(FileUtilities.NormalizePath(str)).ToUpperInvariant();
                    this.excludedInputPaths.Add(str2);
                }
            }
            this.tlogsAvailable = TrackedDependencies.ItemsExist(this.tlogFiles);
            this.skipMissingFiles = skipMissingFiles;
            this.missingFileTimeUtc = missingFileTimeUtc.ToUniversalTime();
            if (this.tlogFiles != null)
            {
                this.ConstructFileTable();
            }
        }

        public static bool IsUpToDate(Task hostTask, UpToDateCheckType upToDateCheckType, ITaskItem[] readTLogNames, ITaskItem[] writeTLogNames)
        {
            FlatTrackingData inputs = new FlatTrackingData(hostTask, readTLogNames, DateTime.MaxValue);
            FlatTrackingData outputs = new FlatTrackingData(hostTask, writeTLogNames, DateTime.MinValue);
            bool flag = IsUpToDate(hostTask.Log, upToDateCheckType, inputs, outputs);
            if (!flag)
            {
                inputs.DependencyTable.Clear();
                inputs.SaveTlog();
                outputs.DependencyTable.Clear();
                outputs.SaveTlog();
            }
            return flag;
        }

        public static bool IsUpToDate(TaskLoggingHelper Log, UpToDateCheckType upToDateCheckType, FlatTrackingData inputs, FlatTrackingData outputs)
        {
            bool flag = false;
            ResourceManager taskResources = Log.TaskResources;
            Log.TaskResources = AssemblyResources.PrimaryResources;
            inputs.UpdateFileEntryDetails();
            outputs.UpdateFileEntryDetails();
            if ((!inputs.TlogsAvailable || !outputs.TlogsAvailable) || (inputs.DependencyTable.Count == 0))
            {
                Log.LogMessageFromResources(MessageImportance.Low, "Tracking_LogFilesNotAvailable", new object[0]);
            }
            else if ((inputs.MissingFiles.Count > 0) || (outputs.MissingFiles.Count > 0))
            {
                if (inputs.MissingFiles.Count > 0)
                {
                    Log.LogMessageFromResources(MessageImportance.Low, "Tracking_MissingInputs", new object[0]);
                }
                if (inputs.MissingFiles.Count > 100)
                {
                    FileTracker.LogMessageFromResources(Log, MessageImportance.Low, "Tracking_InputsNotShown", new object[] { inputs.MissingFiles.Count });
                }
                else
                {
                    foreach (string str in inputs.MissingFiles)
                    {
                        FileTracker.LogMessage(Log, MessageImportance.Low, "\t" + str, new object[0]);
                    }
                }
                if (outputs.MissingFiles.Count > 0)
                {
                    Log.LogMessageFromResources(MessageImportance.Low, "Tracking_MissingOutputs", new object[0]);
                }
                if (outputs.MissingFiles.Count > 100)
                {
                    FileTracker.LogMessageFromResources(Log, MessageImportance.Low, "Tracking_OutputsNotShown", new object[] { outputs.MissingFiles.Count });
                }
                else
                {
                    foreach (string str2 in outputs.MissingFiles)
                    {
                        FileTracker.LogMessage(Log, MessageImportance.Low, "\t" + str2, new object[0]);
                    }
                }
            }
            else if ((upToDateCheckType == UpToDateCheckType.InputOrOutputNewerThanTracking) && (inputs.NewestFileTimeUtc > inputs.NewestTLogTimeUtc))
            {
                Log.LogMessageFromResources(MessageImportance.Low, "Tracking_DependencyWasModifiedAt", new object[] { inputs.NewestFileName, inputs.NewestFileTimeUtc });
            }
            else if ((upToDateCheckType == UpToDateCheckType.InputOrOutputNewerThanTracking) && (outputs.NewestFileTimeUtc > outputs.NewestTLogTimeUtc))
            {
                Log.LogMessage(MessageImportance.Low, "Tracking_DependencyWasModifiedAt", new object[] { outputs.NewestFileName, outputs.NewestFileTimeUtc });
            }
            else if ((upToDateCheckType == UpToDateCheckType.InputNewerThanOutput) && (inputs.NewestFileTimeUtc > outputs.NewestFileTimeUtc))
            {
                Log.LogMessageFromResources(MessageImportance.Low, "Tracking_DependencyWasModifiedAt", new object[] { inputs.NewestFileName, inputs.NewestFileTimeUtc });
            }
            else if ((upToDateCheckType == UpToDateCheckType.InputNewerThanTracking) && (inputs.NewestFileTimeUtc > inputs.NewestTLogTimeUtc))
            {
                Log.LogMessageFromResources(MessageImportance.Low, "Tracking_DependencyWasModifiedAt", new object[] { inputs.NewestFileName, inputs.NewestFileTimeUtc });
            }
            else if ((upToDateCheckType == UpToDateCheckType.InputNewerThanTracking) && (inputs.NewestFileTimeUtc > outputs.NewestTLogTimeUtc))
            {
                Log.LogMessageFromResources(MessageImportance.Low, "Tracking_DependencyWasModifiedAt", new object[] { inputs.NewestFileName, inputs.NewestFileTimeUtc });
            }
            else
            {
                flag = true;
                Log.LogMessageFromResources(MessageImportance.Normal, "Tracking_UpToDate", new object[0]);
            }
            Log.TaskResources = taskResources;
            return flag;
        }

        private void RecordEntryDetails(string tlogEntry, bool populateTable)
        {
            if (!this.FileIsExcludedFromDependencyCheck(tlogEntry))
            {
                DateTime lastWriteTimeUtc = this.GetLastWriteTimeUtc(tlogEntry);
                if (!this.skipMissingFiles || (lastWriteTimeUtc != DateTime.MinValue))
                {
                    if (lastWriteTimeUtc == DateTime.MinValue)
                    {
                        if (populateTable)
                        {
                            this.dependencyTable[tlogEntry] = this.missingFileTimeUtc.ToUniversalTime();
                        }
                        this.missingFiles.Add(tlogEntry);
                    }
                    else if (populateTable)
                    {
                        this.dependencyTable[tlogEntry] = lastWriteTimeUtc;
                    }
                    if (lastWriteTimeUtc > this.newestFileTimeUtc)
                    {
                        this.newestFileTimeUtc = lastWriteTimeUtc;
                        this.newestFileName = tlogEntry;
                    }
                    if (lastWriteTimeUtc < this.oldestFileTimeUtc)
                    {
                        this.oldestFileTimeUtc = lastWriteTimeUtc;
                        this.oldestFileName = tlogEntry;
                    }
                }
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void SaveTlog()
        {
            this.SaveTlog(null);
        }

        public void SaveTlog(DependencyFilter includeInTLog)
        {
            if ((this.tlogFiles != null) && (this.tlogFiles.Length > 0))
            {
                string key = DependencyTableCache.FormatNormalizedTlogRootingMarker(this.tlogFiles);
                lock (DependencyTableCache.DependencyTable)
                {
                    if (DependencyTableCache.DependencyTable.ContainsKey(key))
                    {
                        DependencyTableCache.DependencyTable.Remove(key);
                    }
                }
                string itemSpec = this.tlogFiles[0].ItemSpec;
                foreach (ITaskItem item in this.tlogFiles)
                {
                    File.WriteAllText(item.ItemSpec, "", Encoding.Unicode);
                }
                using (StreamWriter writer = new StreamWriter(itemSpec, false, Encoding.Unicode))
                {
                    foreach (string str3 in this.dependencyTable.Keys)
                    {
                        if ((includeInTLog == null) || includeInTLog(str3))
                        {
                            writer.WriteLine(str3);
                        }
                    }
                    return;
                }
            }
            if (this.tlogMarker != string.Empty)
            {
                string directoryName = Path.GetDirectoryName(this.tlogMarker);
                if (!Directory.Exists(directoryName))
                {
                    Directory.CreateDirectory(directoryName);
                }
                File.WriteAllText(this.tlogMarker, "");
            }
        }

        public void UpdateFileEntryDetails()
        {
            this.oldestFileName = string.Empty;
            this.oldestFileTimeUtc = DateTime.MaxValue;
            this.newestFileName = string.Empty;
            this.newestFileTimeUtc = DateTime.MinValue;
            this.newestTLogFileName = string.Empty;
            this.newestTLogTimeUtc = DateTime.MinValue;
            this.MissingFiles.Clear();
            foreach (ITaskItem item in this.tlogFiles)
            {
                DateTime lastWriteTimeUtc = NativeMethods.GetLastWriteTimeUtc(item.ItemSpec);
                if (lastWriteTimeUtc > this.newestTLogTimeUtc)
                {
                    this.newestTLogTimeUtc = lastWriteTimeUtc;
                    this.newestTLogFileName = item.ItemSpec;
                }
            }
            foreach (string str in this.DependencyTable.Keys)
            {
                this.RecordEntryDetails(str, false);
            }
        }

        public IDictionary<string, DateTime> DependencyTable
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.dependencyTable;
            }
        }

        public List<string> MissingFiles
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.missingFiles;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.missingFiles = value;
            }
        }

        public string NewestFileName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.newestFileName;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.newestFileName = value;
            }
        }

        public DateTime NewestFileTime
        {
            get
            {
                return this.newestFileTimeUtc.ToLocalTime();
            }
            set
            {
                this.newestFileTimeUtc = value.ToUniversalTime();
            }
        }

        public DateTime NewestFileTimeUtc
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.newestFileTimeUtc;
            }
            set
            {
                this.newestFileTimeUtc = value.ToUniversalTime();
            }
        }

        public string NewestTLogFileName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.newestTLogFileName;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.newestTLogFileName = value;
            }
        }

        public DateTime NewestTLogTime
        {
            get
            {
                return this.newestTLogTimeUtc.ToLocalTime();
            }
            set
            {
                this.newestTLogTimeUtc = value.ToUniversalTime();
            }
        }

        public DateTime NewestTLogTimeUtc
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.newestTLogTimeUtc;
            }
            set
            {
                this.newestTLogTimeUtc = value.ToUniversalTime();
            }
        }

        public string OldestFileName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.oldestFileName;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.oldestFileName = value;
            }
        }

        public DateTime OldestFileTime
        {
            get
            {
                return this.oldestFileTimeUtc.ToLocalTime();
            }
            set
            {
                this.oldestFileTimeUtc = value.ToUniversalTime();
            }
        }

        public DateTime OldestFileTimeUtc
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.oldestFileTimeUtc;
            }
            set
            {
                this.oldestFileTimeUtc = value.ToUniversalTime();
            }
        }

        public bool SkipMissingFiles
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.skipMissingFiles;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.skipMissingFiles = value;
            }
        }

        public ITaskItem[] TlogFiles
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.tlogFiles;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.tlogFiles = value;
            }
        }

        public bool TlogsAvailable
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.tlogsAvailable;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.tlogsAvailable = value;
            }
        }

        public bool TreatRootMarkersAsEntries
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.treatRootMarkersAsEntries;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.treatRootMarkersAsEntries = value;
            }
        }
    }
}

