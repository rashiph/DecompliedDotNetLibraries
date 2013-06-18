namespace Microsoft.Build.Utilities
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Shared;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime;
    using System.Text;
    using System.Threading.Tasks;

    public class CanonicalTrackedInputFiles
    {
        private Dictionary<string, Dictionary<string, string>> dependencyTable;
        private HashSet<string> excludedInputPaths;
        private ConcurrentDictionary<string, DateTime> lastWriteTimeCache;
        private TaskLoggingHelper Log;
        private bool maintainCompositeRootingMarkers;
        private ITaskItem[] outputFileGroup;
        private ITaskItem[] outputFiles;
        private string outputNewest;
        private DateTime outputNewestTime;
        private CanonicalTrackedOutputFiles outputs;
        private ITaskItem[] sourceFiles;
        private ITaskItem[] sourcesNeedingCompilation;
        private bool tlogAvailable;
        private ITaskItem[] tlogFiles;
        private bool useMinimalRebuildOptimization;

        public CanonicalTrackedInputFiles(ITaskItem[] tlogFiles, ITaskItem[] sourceFiles, CanonicalTrackedOutputFiles outputs, bool useMinimalRebuildOptimization, bool maintainCompositeRootingMarkers)
        {
            this.outputNewestTime = DateTime.MinValue;
            this.outputNewest = "";
            this.excludedInputPaths = new HashSet<string>(StringComparer.Ordinal);
            this.lastWriteTimeCache = new ConcurrentDictionary<string, DateTime>(StringComparer.Ordinal);
            this.InternalConstruct(null, tlogFiles, sourceFiles, null, null, outputs, useMinimalRebuildOptimization, maintainCompositeRootingMarkers);
        }

        public CanonicalTrackedInputFiles(ITaskItem[] tlogFiles, ITaskItem[] sourceFiles, ITaskItem[] excludedInputPaths, CanonicalTrackedOutputFiles outputs, bool useMinimalRebuildOptimization, bool maintainCompositeRootingMarkers)
        {
            this.outputNewestTime = DateTime.MinValue;
            this.outputNewest = "";
            this.excludedInputPaths = new HashSet<string>(StringComparer.Ordinal);
            this.lastWriteTimeCache = new ConcurrentDictionary<string, DateTime>(StringComparer.Ordinal);
            this.InternalConstruct(null, tlogFiles, sourceFiles, null, excludedInputPaths, outputs, useMinimalRebuildOptimization, maintainCompositeRootingMarkers);
        }

        public CanonicalTrackedInputFiles(ITask ownerTask, ITaskItem[] tlogFiles, ITaskItem[] sourceFiles, ITaskItem[] excludedInputPaths, CanonicalTrackedOutputFiles outputs, bool useMinimalRebuildOptimization, bool maintainCompositeRootingMarkers)
        {
            this.outputNewestTime = DateTime.MinValue;
            this.outputNewest = "";
            this.excludedInputPaths = new HashSet<string>(StringComparer.Ordinal);
            this.lastWriteTimeCache = new ConcurrentDictionary<string, DateTime>(StringComparer.Ordinal);
            this.InternalConstruct(ownerTask, tlogFiles, sourceFiles, null, excludedInputPaths, outputs, useMinimalRebuildOptimization, maintainCompositeRootingMarkers);
        }

        public CanonicalTrackedInputFiles(ITask ownerTask, ITaskItem[] tlogFiles, ITaskItem[] sourceFiles, ITaskItem[] excludedInputPaths, ITaskItem[] outputs, bool useMinimalRebuildOptimization, bool maintainCompositeRootingMarkers)
        {
            this.outputNewestTime = DateTime.MinValue;
            this.outputNewest = "";
            this.excludedInputPaths = new HashSet<string>(StringComparer.Ordinal);
            this.lastWriteTimeCache = new ConcurrentDictionary<string, DateTime>(StringComparer.Ordinal);
            this.InternalConstruct(ownerTask, tlogFiles, sourceFiles, outputs, excludedInputPaths, null, useMinimalRebuildOptimization, maintainCompositeRootingMarkers);
        }

        public CanonicalTrackedInputFiles(ITask ownerTask, ITaskItem[] tlogFiles, ITaskItem sourceFile, ITaskItem[] excludedInputPaths, CanonicalTrackedOutputFiles outputs, bool useMinimalRebuildOptimization, bool maintainCompositeRootingMarkers)
        {
            this.outputNewestTime = DateTime.MinValue;
            this.outputNewest = "";
            this.excludedInputPaths = new HashSet<string>(StringComparer.Ordinal);
            this.lastWriteTimeCache = new ConcurrentDictionary<string, DateTime>(StringComparer.Ordinal);
            ITaskItem[] sourceFiles = new ITaskItem[] { sourceFile };
            this.InternalConstruct(ownerTask, tlogFiles, sourceFiles, null, excludedInputPaths, outputs, useMinimalRebuildOptimization, maintainCompositeRootingMarkers);
        }

        private void CheckIfSourceNeedsCompilation(ConcurrentQueue<ITaskItem> sourcesNeedingCompilationList, bool allOutputFilesExist, ITaskItem source)
        {
            if (!this.tlogAvailable || (this.outputFileGroup == null))
            {
                source.SetMetadata("_trackerCompileReason", "Tracking_SourceWillBeCompiledAsNoTrackingLog");
                sourcesNeedingCompilationList.Enqueue(source);
            }
            else if (!this.useMinimalRebuildOptimization && !allOutputFilesExist)
            {
                source.SetMetadata("_trackerCompileReason", "Tracking_SourceOutputsNotAvailable");
                sourcesNeedingCompilationList.Enqueue(source);
            }
            else if (!this.IsUpToDate(source))
            {
                if (string.IsNullOrEmpty(source.GetMetadata("_trackerCompileReason")))
                {
                    source.SetMetadata("_trackerCompileReason", "Tracking_SourceWillBeCompiled");
                }
                sourcesNeedingCompilationList.Enqueue(source);
            }
            else if (!this.useMinimalRebuildOptimization && (this.outputNewestTime == DateTime.MinValue))
            {
                source.SetMetadata("_trackerCompileReason", "Tracking_SourceNotInTrackingLog");
                sourcesNeedingCompilationList.Enqueue(source);
            }
        }

        private int CompareTaskItems(ITaskItem left, ITaskItem right)
        {
            return string.Compare(left.ItemSpec, right.ItemSpec, StringComparison.Ordinal);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ITaskItem[] ComputeSourcesNeedingCompilation()
        {
            return this.ComputeSourcesNeedingCompilation(true);
        }

        public ITaskItem[] ComputeSourcesNeedingCompilation(bool searchForSubRootsInCompositeRootingMarkers)
        {
            if (this.outputFiles != null)
            {
                this.outputFileGroup = this.outputFiles;
            }
            else if (((this.sourceFiles != null) && (this.outputs != null)) && this.maintainCompositeRootingMarkers)
            {
                this.outputFileGroup = this.outputs.OutputsForSource(this.sourceFiles);
            }
            else if ((this.sourceFiles != null) && (this.outputs != null))
            {
                this.outputFileGroup = this.outputs.OutputsForNonCompositeSource(this.sourceFiles);
            }
            if (this.maintainCompositeRootingMarkers)
            {
                return this.ComputeSourcesNeedingCompilationFromCompositeRootingMarker(searchForSubRootsInCompositeRootingMarkers);
            }
            return this.ComputeSourcesNeedingCompilationFromPrimaryFiles();
        }

        internal ITaskItem[] ComputeSourcesNeedingCompilationFromCompositeRootingMarker(bool searchForSubRootsInCompositeRootingMarkers)
        {
            Dictionary<string, ITaskItem> sourceDependencies = new Dictionary<string, ITaskItem>(StringComparer.OrdinalIgnoreCase);
            if (this.tlogAvailable)
            {
                DateTime time;
                DateTime time2;
                string str = FileTracker.FormatRootingMarker(this.sourceFiles);
                List<ITaskItem> list = new List<ITaskItem>();
                foreach (string str2 in this.dependencyTable.Keys)
                {
                    string compositeSubRoot = str2.ToUpperInvariant();
                    if (searchForSubRootsInCompositeRootingMarkers)
                    {
                        if (compositeSubRoot.Contains(str) || CanonicalTrackedFilesHelper.RootContainsAllSubRootComponents(str, compositeSubRoot))
                        {
                            this.SourceDependenciesForOutputRoot(sourceDependencies, compositeSubRoot, this.outputFileGroup);
                        }
                    }
                    else if (compositeSubRoot.Equals(str, StringComparison.Ordinal))
                    {
                        this.SourceDependenciesForOutputRoot(sourceDependencies, compositeSubRoot, this.outputFileGroup);
                    }
                }
                if (sourceDependencies.Count == 0)
                {
                    FileTracker.LogMessageFromResources(this.Log, MessageImportance.Low, "Tracking_DependenciesForRootNotFound", new object[] { str });
                    return this.sourceFiles;
                }
                list.AddRange(sourceDependencies.Values);
                ITaskItem[] files = list.ToArray();
                string outputNewestFilename = string.Empty;
                string outputOldestFilename = string.Empty;
                if ((CanonicalTrackedFilesHelper.FilesExistAndRecordNewestWriteTime(files, this.Log, out time, out outputNewestFilename) && CanonicalTrackedFilesHelper.FilesExistAndRecordOldestWriteTime(this.outputFileGroup, this.Log, out time2, out outputOldestFilename)) && (time <= time2))
                {
                    FileTracker.LogMessageFromResources(this.Log, MessageImportance.Normal, "Tracking_AllOutputsAreUpToDate", new object[0]);
                    return new ITaskItem[0];
                }
                if (sourceDependencies.Count > 100)
                {
                    FileTracker.LogMessageFromResources(this.Log, MessageImportance.Low, "Tracking_InputsNotShown", new object[] { sourceDependencies.Count });
                }
                else
                {
                    FileTracker.LogMessageFromResources(this.Log, MessageImportance.Low, "Tracking_InputsFor", new object[] { str });
                    foreach (ITaskItem item in files)
                    {
                        FileTracker.LogMessage(this.Log, MessageImportance.Low, "\t" + item, new object[0]);
                    }
                }
                FileTracker.LogMessageFromResources(this.Log, MessageImportance.Low, "Tracking_InputNewerThanOutput", new object[] { outputNewestFilename, outputOldestFilename });
            }
            return this.sourceFiles;
        }

        internal ITaskItem[] ComputeSourcesNeedingCompilationFromPrimaryFiles()
        {
            if (this.sourcesNeedingCompilation == null)
            {
                ConcurrentQueue<ITaskItem> sourcesNeedingCompilationList = new ConcurrentQueue<ITaskItem>();
                bool allOutputFilesExist = false;
                if (this.tlogAvailable && !this.useMinimalRebuildOptimization)
                {
                    allOutputFilesExist = this.FilesExistAndRecordNewestWriteTime(this.outputFileGroup);
                }
                Parallel.For(0, this.sourceFiles.Length, delegate (int index) {
                    this.CheckIfSourceNeedsCompilation(sourcesNeedingCompilationList, allOutputFilesExist, this.sourceFiles[index]);
                });
                this.sourcesNeedingCompilation = sourcesNeedingCompilationList.ToArray();
            }
            if (this.sourcesNeedingCompilation.Length == 0)
            {
                FileTracker.LogMessageFromResources(this.Log, MessageImportance.Normal, "Tracking_AllOutputsAreUpToDate", new object[0]);
                this.sourcesNeedingCompilation = new ITaskItem[0];
            }
            else
            {
                Array.Sort<ITaskItem>(this.sourcesNeedingCompilation, new Comparison<ITaskItem>(this.CompareTaskItems));
                foreach (ITaskItem item in this.sourcesNeedingCompilation)
                {
                    string metadata = item.GetMetadata("_trackerModifiedPath");
                    string str2 = item.GetMetadata("_trackerOutputFile");
                    if (!string.IsNullOrEmpty(metadata))
                    {
                        FileTracker.LogMessageFromResources(this.Log, MessageImportance.Low, "Tracking_DependencyWasModifiedAt", new object[] { item.GetMetadata("_trackerModifiedPath"), item.GetMetadata("_trackerModifiedTime") });
                    }
                    else if (!string.IsNullOrEmpty(str2))
                    {
                        FileTracker.LogMessageFromResources(this.Log, MessageImportance.Low, "Tracking_OutputDoesNotExist", new object[] { item.GetMetadata("_trackerOutputFile") });
                    }
                    FileTracker.LogMessageFromResources(this.Log, MessageImportance.Low, item.GetMetadata("_trackerCompileReason"), new object[] { item.ItemSpec });
                    item.RemoveMetadata("_trackerModifiedPath");
                    item.RemoveMetadata("_trackerModifiedTime");
                    item.RemoveMetadata("_trackerOutputFile");
                    item.RemoveMetadata("_trackerCompileReason");
                }
            }
            return this.sourcesNeedingCompilation;
        }

        private void ConstructDependencyTable()
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
            string path = FileUtilities.EnsureTrailingSlash(Directory.GetCurrentDirectory());
            if (this.tlogAvailable)
            {
                DependencyTableCacheEntry cachedEntry = null;
                lock (DependencyTableCache.DependencyTable)
                {
                    cachedEntry = DependencyTableCache.GetCachedEntry(tLogRootingMarker);
                }
                if (cachedEntry != null)
                {
                    this.dependencyTable = (Dictionary<string, Dictionary<string, string>>) cachedEntry.DependencyTable;
                    FileTracker.LogMessageFromResources(this.Log, MessageImportance.Low, "Tracking_ReadTrackingCached", new object[0]);
                    foreach (ITaskItem item2 in cachedEntry.TlogFiles)
                    {
                        FileTracker.LogMessage(this.Log, MessageImportance.Low, "\t{0}", new object[] { item2.ItemSpec });
                    }
                    return;
                }
                flag2 = false;
                flag3 = false;
                string itemSpec = null;
                FileTracker.LogMessageFromResources(this.Log, MessageImportance.Low, "Tracking_ReadTrackingLogs", new object[0]);
                foreach (ITaskItem item3 in this.tlogFiles)
                {
                    try
                    {
                        FileTracker.LogMessage(this.Log, MessageImportance.Low, "\t{0}", new object[] { item3.ItemSpec });
                        using (StreamReader reader = File.OpenText(item3.ItemSpec))
                        {
                            string key = reader.ReadLine();
                            while (key != null)
                            {
                                Dictionary<string, string> dictionary;
                                Dictionary<string, string> dictionary2;
                                if (key.Length == 0)
                                {
                                    flag2 = true;
                                    itemSpec = item3.ItemSpec;
                                    goto Label_04A1;
                                }
                                if (key[0] == '#')
                                {
                                    goto Label_044D;
                                }
                                bool flag4 = false;
                                if (key[0] == '^')
                                {
                                    key = key.Substring(1);
                                    if (key.Length == 0)
                                    {
                                        flag2 = true;
                                        itemSpec = item3.ItemSpec;
                                        goto Label_04A1;
                                    }
                                    flag4 = true;
                                }
                                if (!flag4)
                                {
                                    goto Label_0442;
                                }
                                if (!this.maintainCompositeRootingMarkers)
                                {
                                    dictionary2 = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                                    if (key.Contains("|"))
                                    {
                                        foreach (ITaskItem item4 in this.sourceFiles)
                                        {
                                            dictionary2.Add(FileUtilities.NormalizePath(item4.ItemSpec), null);
                                        }
                                    }
                                    else
                                    {
                                        dictionary2.Add(key, null);
                                    }
                                }
                                else
                                {
                                    dictionary2 = null;
                                }
                                if (!this.dependencyTable.TryGetValue(key, out dictionary))
                                {
                                    dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                                    if (!this.maintainCompositeRootingMarkers)
                                    {
                                        dictionary.Add(key, null);
                                    }
                                    this.dependencyTable.Add(key, dictionary);
                                }
                                key = reader.ReadLine();
                                if (!this.maintainCompositeRootingMarkers)
                                {
                                    goto Label_0439;
                                }
                                while (key != null)
                                {
                                    if (key.Length == 0)
                                    {
                                        flag2 = true;
                                        itemSpec = item3.ItemSpec;
                                        break;
                                    }
                                    if ((key[0] == '#') || (key[0] == '^'))
                                    {
                                        break;
                                    }
                                    if (!dictionary.ContainsKey(key) && (FileTracker.FileIsUnderPath(key, path) || !FileTracker.FileIsExcludedFromDependencies(key)))
                                    {
                                        dictionary.Add(key, null);
                                    }
                                    key = reader.ReadLine();
                                }
                                continue;
                            Label_038D:
                                if (key.Length == 0)
                                {
                                    flag2 = true;
                                    itemSpec = item3.ItemSpec;
                                    continue;
                                }
                                if ((key[0] == '#') || (key[0] == '^'))
                                {
                                    continue;
                                }
                                if (dictionary2.ContainsKey(key))
                                {
                                    if (!this.dependencyTable.TryGetValue(key, out dictionary))
                                    {
                                        dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                                        dictionary.Add(key, null);
                                        this.dependencyTable.Add(key, dictionary);
                                    }
                                }
                                else if (!dictionary.ContainsKey(key) && (FileTracker.FileIsUnderPath(key, path) || !FileTracker.FileIsExcludedFromDependencies(key)))
                                {
                                    dictionary.Add(key, null);
                                }
                                key = reader.ReadLine();
                            Label_0439:
                                if (key != null)
                                {
                                    goto Label_038D;
                                }
                                continue;
                            Label_0442:
                                key = reader.ReadLine();
                                continue;
                            Label_044D:
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
                Label_04A1:
                    if (flag2)
                    {
                        FileTracker.LogWarningWithCodeFromResources(this.Log, "Tracking_RebuildingDueToInvalidTLogContents", new object[] { itemSpec });
                        break;
                    }
                }
            }
            else
            {
                foreach (ITaskItem item in this.tlogFiles)
                {
                    if (!FileUtilities.FileExistsNoThrow(item.ItemSpec))
                    {
                        FileTracker.LogMessageFromResources(this.Log, MessageImportance.Low, "Tracking_SingleLogFileNotAvailable", new object[] { item.ItemSpec });
                    }
                }
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
                    this.dependencyTable = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
                }
                else
                {
                    DependencyTableCache.DependencyTable[tLogRootingMarker] = new DependencyTableCacheEntry(this.tlogFiles, this.dependencyTable);
                }
            }
        }

        public bool FileIsExcludedFromDependencyCheck(string fileName)
        {
            string directoryNameOfFullPath = FileUtilities.GetDirectoryNameOfFullPath(fileName);
            return this.excludedInputPaths.Contains(directoryNameOfFullPath);
        }

        private bool FilesExistAndRecordNewestWriteTime(ITaskItem[] files)
        {
            return CanonicalTrackedFilesHelper.FilesExistAndRecordNewestWriteTime(files, this.Log, out this.outputNewestTime, out this.outputNewest);
        }

        private void InternalConstruct(ITask ownerTask, ITaskItem[] tlogFiles, ITaskItem[] sourceFiles, ITaskItem[] outputFiles, ITaskItem[] excludedInputPaths, CanonicalTrackedOutputFiles outputs, bool useMinimalRebuildOptimization, bool maintainCompositeRootingMarkers)
        {
            if (ownerTask != null)
            {
                this.Log = new TaskLoggingHelper(ownerTask);
                this.Log.TaskResources = AssemblyResources.PrimaryResources;
                this.Log.HelpKeywordPrefix = "MSBuild.";
            }
            this.tlogFiles = TrackedDependencies.ExpandWildcards(tlogFiles);
            this.tlogAvailable = TrackedDependencies.ItemsExist(this.tlogFiles);
            this.sourceFiles = sourceFiles;
            this.outputs = outputs;
            this.outputFiles = outputFiles;
            this.useMinimalRebuildOptimization = useMinimalRebuildOptimization;
            this.maintainCompositeRootingMarkers = maintainCompositeRootingMarkers;
            if (excludedInputPaths != null)
            {
                foreach (ITaskItem item in excludedInputPaths)
                {
                    string str = FileUtilities.EnsureNoTrailingSlash(FileUtilities.NormalizePath(item.ItemSpec)).ToUpperInvariant();
                    this.excludedInputPaths.Add(str);
                }
            }
            this.dependencyTable = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
            if (this.tlogFiles != null)
            {
                this.ConstructDependencyTable();
            }
        }

        private bool IsUpToDate(ITaskItem sourceFile)
        {
            Dictionary<string, string> dictionary;
            string key = FileUtilities.NormalizePath(sourceFile.ItemSpec);
            bool flag = this.dependencyTable.TryGetValue(key, out dictionary);
            DateTime outputNewestTime = this.outputNewestTime;
            if ((this.useMinimalRebuildOptimization && (this.outputs != null)) && flag)
            {
                Dictionary<string, DateTime> dictionary2;
                outputNewestTime = DateTime.MinValue;
                if (!this.outputs.DependencyTable.TryGetValue(key, out dictionary2))
                {
                    sourceFile.SetMetadata("_trackerCompileReason", "Tracking_SourceOutputsNotAvailable");
                    return false;
                }
                DateTime lastWriteTimeUtc = NativeMethods.GetLastWriteTimeUtc(key);
                foreach (string str2 in dictionary2.Keys)
                {
                    DateTime time3 = NativeMethods.GetLastWriteTimeUtc(str2);
                    if (time3 > DateTime.MinValue)
                    {
                        if (time3 < lastWriteTimeUtc)
                        {
                            sourceFile.SetMetadata("_trackerModifiedPath", key);
                            sourceFile.SetMetadata("_trackerModifiedTime", lastWriteTimeUtc.ToLocalTime().ToString());
                            return false;
                        }
                        if (time3 > outputNewestTime)
                        {
                            outputNewestTime = time3;
                        }
                    }
                    else
                    {
                        sourceFile.SetMetadata("_trackerOutputFile", str2);
                        return false;
                    }
                }
            }
            if (flag)
            {
                foreach (string str3 in dictionary.Keys)
                {
                    if (!this.FileIsExcludedFromDependencyCheck(str3))
                    {
                        DateTime minValue = DateTime.MinValue;
                        if (!this.lastWriteTimeCache.TryGetValue(str3, out minValue))
                        {
                            minValue = NativeMethods.GetLastWriteTimeUtc(str3);
                            this.lastWriteTimeCache[str3] = minValue;
                        }
                        if (minValue > DateTime.MinValue)
                        {
                            if (minValue > outputNewestTime)
                            {
                                sourceFile.SetMetadata("_trackerModifiedPath", str3);
                                sourceFile.SetMetadata("_trackerModifiedTime", minValue.ToLocalTime().ToString());
                                return false;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
            sourceFile.SetMetadata("_trackerCompileReason", "Tracking_SourceNotInTrackingLog");
            return false;
        }

        public void RemoveDependenciesFromEntryIfMissing(ITaskItem source)
        {
            this.RemoveDependenciesFromEntryIfMissing(new ITaskItem[] { source }, null);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void RemoveDependenciesFromEntryIfMissing(ITaskItem[] source)
        {
            this.RemoveDependenciesFromEntryIfMissing(source, null);
        }

        private void RemoveDependenciesFromEntryIfMissing(string rootingMarker)
        {
            Dictionary<string, string> dictionary;
            if (this.dependencyTable.TryGetValue(rootingMarker, out dictionary))
            {
                Dictionary<string, string> dictionary2 = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                int num = 0;
                foreach (string str in dictionary.Keys)
                {
                    if (num++ > 0)
                    {
                        if (FileUtilities.FileExistsNoThrow(str))
                        {
                            dictionary2.Add(str, dictionary[str]);
                        }
                    }
                    else
                    {
                        dictionary2.Add(str, str);
                    }
                }
                this.dependencyTable[rootingMarker] = dictionary2;
            }
        }

        public void RemoveDependenciesFromEntryIfMissing(ITaskItem source, ITaskItem correspondingOutput)
        {
            this.RemoveDependenciesFromEntryIfMissing(new ITaskItem[] { source }, new ITaskItem[] { correspondingOutput });
        }

        public void RemoveDependenciesFromEntryIfMissing(ITaskItem[] source, ITaskItem[] correspondingOutputs)
        {
            if (correspondingOutputs != null)
            {
                ErrorUtilities.VerifyThrowArgument(source.Length == correspondingOutputs.Length, "Tracking_SourcesAndCorrespondingOutputMismatch");
            }
            string rootingMarker = FileTracker.FormatRootingMarker(source, correspondingOutputs);
            this.RemoveDependenciesFromEntryIfMissing(rootingMarker);
            for (int i = 0; i < source.Length; i++)
            {
                if (correspondingOutputs != null)
                {
                    rootingMarker = FileTracker.FormatRootingMarker(source[i], correspondingOutputs[i]);
                }
                else
                {
                    rootingMarker = FileTracker.FormatRootingMarker(source[i]);
                }
                this.RemoveDependenciesFromEntryIfMissing(rootingMarker);
            }
        }

        public void RemoveDependencyFromEntry(ITaskItem[] sources, ITaskItem dependencyToRemove)
        {
            string rootingMarker = FileTracker.FormatRootingMarker(sources);
            this.RemoveDependencyFromEntry(rootingMarker, dependencyToRemove);
        }

        public void RemoveDependencyFromEntry(ITaskItem source, ITaskItem dependencyToRemove)
        {
            string rootingMarker = FileTracker.FormatRootingMarker(source);
            this.RemoveDependencyFromEntry(rootingMarker, dependencyToRemove);
        }

        private void RemoveDependencyFromEntry(string rootingMarker, ITaskItem dependencyToRemove)
        {
            Dictionary<string, string> dictionary;
            if (this.dependencyTable.TryGetValue(rootingMarker, out dictionary))
            {
                dictionary.Remove(FileUtilities.NormalizePath(dependencyToRemove.ItemSpec));
            }
            else
            {
                FileTracker.LogMessageFromResources(this.Log, MessageImportance.Normal, "Tracking_ReadLogEntryNotFound", new object[] { rootingMarker });
            }
        }

        public void RemoveEntriesForSource(ITaskItem source)
        {
            this.RemoveEntriesForSource(new ITaskItem[] { source });
        }

        public void RemoveEntriesForSource(ITaskItem[] source)
        {
            string key = FileTracker.FormatRootingMarker(source);
            this.dependencyTable.Remove(key);
            foreach (ITaskItem item in source)
            {
                this.dependencyTable.Remove(FileUtilities.NormalizePath(item.ItemSpec));
            }
        }

        public void RemoveEntryForSourceRoot(string rootingMarker)
        {
            this.dependencyTable.Remove(rootingMarker);
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
                    if (!this.maintainCompositeRootingMarkers)
                    {
                        foreach (string str3 in this.dependencyTable.Keys)
                        {
                            if (!str3.Contains("|"))
                            {
                                Dictionary<string, string> dictionary = this.dependencyTable[str3];
                                writer.WriteLine("^" + str3);
                                foreach (string str4 in dictionary.Keys)
                                {
                                    if ((str4 != str3) && ((includeInTLog == null) || includeInTLog(str4)))
                                    {
                                        writer.WriteLine(str4);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (string str5 in this.dependencyTable.Keys)
                        {
                            Dictionary<string, string> dictionary2 = this.dependencyTable[str5];
                            writer.WriteLine("^" + str5);
                            foreach (string str6 in dictionary2.Keys)
                            {
                                if ((includeInTLog == null) || includeInTLog(str6))
                                {
                                    writer.WriteLine(str6);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void SourceDependenciesForOutputRoot(Dictionary<string, ITaskItem> sourceDependencies, string sourceKey, ITaskItem[] filesToIgnore)
        {
            Dictionary<string, string> dictionary;
            bool flag2 = (filesToIgnore != null) && (filesToIgnore.Length > 0);
            if (this.dependencyTable.TryGetValue(sourceKey, out dictionary))
            {
                foreach (string str in dictionary.Keys)
                {
                    ITaskItem item2;
                    bool flag = false;
                    if (flag2)
                    {
                        foreach (ITaskItem item in filesToIgnore)
                        {
                            if (string.Equals(str, item.ItemSpec, StringComparison.OrdinalIgnoreCase))
                            {
                                flag = true;
                                break;
                            }
                        }
                    }
                    if (!flag && !sourceDependencies.TryGetValue(str, out item2))
                    {
                        sourceDependencies.Add(str, new TaskItem(str));
                    }
                }
            }
        }

        public Dictionary<string, Dictionary<string, string>> DependencyTable
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.dependencyTable;
            }
        }

        internal ITaskItem[] SourcesNeedingCompilation
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.sourcesNeedingCompilation;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.sourcesNeedingCompilation = value;
            }
        }
    }
}

