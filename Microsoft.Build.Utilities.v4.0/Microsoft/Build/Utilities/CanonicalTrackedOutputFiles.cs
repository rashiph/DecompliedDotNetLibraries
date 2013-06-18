namespace Microsoft.Build.Utilities
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Shared;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime;
    using System.Text;

    public class CanonicalTrackedOutputFiles
    {
        private Dictionary<string, Dictionary<string, DateTime>> dependencyTable;
        private TaskLoggingHelper Log;
        private bool tlogAvailable;
        private ITaskItem[] tlogFiles;

        public CanonicalTrackedOutputFiles(ITaskItem[] tlogFiles)
        {
            this.InternalConstruct(null, tlogFiles, true);
        }

        public CanonicalTrackedOutputFiles(ITask ownerTask, ITaskItem[] tlogFiles)
        {
            this.InternalConstruct(ownerTask, tlogFiles, true);
        }

        public CanonicalTrackedOutputFiles(ITask ownerTask, ITaskItem[] tlogFiles, bool constructOutputsFromTLogs)
        {
            this.InternalConstruct(ownerTask, tlogFiles, constructOutputsFromTLogs);
        }

        public void AddComputedOutputForSourceRoot(string sourceKey, string computedOutput)
        {
            Dictionary<string, DateTime> sourceKeyOutputs = this.GetSourceKeyOutputs(sourceKey);
            this.AddOutput(sourceKeyOutputs, computedOutput);
        }

        public void AddComputedOutputsForSourceRoot(string sourceKey, ITaskItem[] computedOutputs)
        {
            Dictionary<string, DateTime> sourceKeyOutputs = this.GetSourceKeyOutputs(sourceKey);
            foreach (ITaskItem item in computedOutputs)
            {
                this.AddOutput(sourceKeyOutputs, FileUtilities.NormalizePath(item.ItemSpec));
            }
        }

        public void AddComputedOutputsForSourceRoot(string sourceKey, string[] computedOutputs)
        {
            Dictionary<string, DateTime> sourceKeyOutputs = this.GetSourceKeyOutputs(sourceKey);
            foreach (string str in computedOutputs)
            {
                this.AddOutput(sourceKeyOutputs, str);
            }
        }

        private void AddOutput(Dictionary<string, DateTime> dependencies, string computedOutput)
        {
            string key = FileUtilities.NormalizePath(computedOutput).ToUpperInvariant();
            if (!dependencies.ContainsKey(key))
            {
                DateTime lastWriteTimeUtc;
                if (FileUtilities.FileExistsNoThrow(key))
                {
                    lastWriteTimeUtc = NativeMethods.GetLastWriteTimeUtc(key);
                }
                else
                {
                    lastWriteTimeUtc = DateTime.MinValue;
                }
                dependencies.Add(key, lastWriteTimeUtc);
            }
        }

        private void ConstructOutputTable()
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
                    this.dependencyTable = (Dictionary<string, Dictionary<string, DateTime>>) cachedEntry.DependencyTable;
                    FileTracker.LogMessageFromResources(this.Log, MessageImportance.Low, "Tracking_WriteTrackingCached", new object[0]);
                    foreach (ITaskItem item in cachedEntry.TlogFiles)
                    {
                        FileTracker.LogMessage(this.Log, MessageImportance.Low, "\t{0}", new object[] { item.ItemSpec });
                    }
                    return;
                }
                FileTracker.LogMessageFromResources(this.Log, MessageImportance.Low, "Tracking_WriteTrackingLogs", new object[0]);
                flag2 = false;
                flag3 = false;
                string itemSpec = null;
                foreach (ITaskItem item2 in this.tlogFiles)
                {
                    FileTracker.LogMessage(this.Log, MessageImportance.Low, "\t{0}", new object[] { item2.ItemSpec });
                    try
                    {
                        using (StreamReader reader = File.OpenText(item2.ItemSpec))
                        {
                            string key = reader.ReadLine();
                            while (key != null)
                            {
                                if (key.Length == 0)
                                {
                                    flag2 = true;
                                    itemSpec = item2.ItemSpec;
                                    goto Label_0305;
                                }
                                if (key[0] == '^')
                                {
                                    key = key.Substring(1);
                                    if (key.Length == 0)
                                    {
                                        flag2 = true;
                                        itemSpec = item2.ItemSpec;
                                    }
                                    else
                                    {
                                        Dictionary<string, DateTime> dictionary;
                                        if (!this.dependencyTable.TryGetValue(key, out dictionary))
                                        {
                                            dictionary = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);
                                            this.dependencyTable.Add(key, dictionary);
                                        }
                                        do
                                        {
                                            key = reader.ReadLine();
                                            if (key != null)
                                            {
                                                if (key.Length == 0)
                                                {
                                                    flag2 = true;
                                                    itemSpec = item2.ItemSpec;
                                                    break;
                                                }
                                                if ((((key[0] != '^') && (key[0] != '#')) && !dictionary.ContainsKey(key)) && (FileTracker.FileIsUnderPath(key, path) || !FileTracker.FileIsExcludedFromDependencies(key)))
                                                {
                                                    DateTime lastWriteTimeUtc = NativeMethods.GetLastWriteTimeUtc(key);
                                                    dictionary.Add(key, lastWriteTimeUtc);
                                                }
                                            }
                                        }
                                        while ((key != null) && (key[0] != '^'));
                                        if (!flag2)
                                        {
                                            continue;
                                        }
                                    }
                                    goto Label_0305;
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
                Label_0305:
                    if (flag2)
                    {
                        FileTracker.LogWarningWithCodeFromResources(this.Log, "Tracking_RebuildingDueToInvalidTLogContents", new object[] { itemSpec });
                        break;
                    }
                }
            }
            else
            {
                FileTracker.LogMessageFromResources(this.Log, MessageImportance.Low, "Tracking_TrackingLogNotAvailable", new object[0]);
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
                    this.dependencyTable = new Dictionary<string, Dictionary<string, DateTime>>(StringComparer.OrdinalIgnoreCase);
                }
                else
                {
                    DependencyTableCache.DependencyTable[tLogRootingMarker] = new DependencyTableCacheEntry(this.tlogFiles, this.dependencyTable);
                }
            }
        }

        private Dictionary<string, DateTime> GetSourceKeyOutputs(string sourceKey)
        {
            Dictionary<string, DateTime> dictionary;
            if (!this.dependencyTable.TryGetValue(sourceKey, out dictionary))
            {
                dictionary = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);
                this.dependencyTable.Add(sourceKey, dictionary);
            }
            return dictionary;
        }

        private void InternalConstruct(ITask ownerTask, ITaskItem[] tlogFiles, bool constructOutputsFromTLogs)
        {
            if (ownerTask != null)
            {
                this.Log = new TaskLoggingHelper(ownerTask);
                this.Log.TaskResources = AssemblyResources.PrimaryResources;
                this.Log.HelpKeywordPrefix = "MSBuild.";
            }
            this.tlogFiles = TrackedDependencies.ExpandWildcards(tlogFiles);
            this.tlogAvailable = TrackedDependencies.ItemsExist(this.tlogFiles);
            this.dependencyTable = new Dictionary<string, Dictionary<string, DateTime>>(StringComparer.OrdinalIgnoreCase);
            if ((this.tlogFiles != null) && constructOutputsFromTLogs)
            {
                this.ConstructOutputTable();
            }
        }

        public ITaskItem[] OutputsForNonCompositeSource(params ITaskItem[] sources)
        {
            Dictionary<string, ITaskItem> outputs = new Dictionary<string, ITaskItem>(StringComparer.OrdinalIgnoreCase);
            List<ITaskItem> list = new List<ITaskItem>();
            string str = FileTracker.FormatRootingMarker(sources);
            foreach (ITaskItem item in sources)
            {
                string sourceKey = FileUtilities.NormalizePath(item.ItemSpec);
                this.OutputsForSourceRoot(outputs, sourceKey);
            }
            if (outputs.Count == 0)
            {
                FileTracker.LogMessageFromResources(this.Log, MessageImportance.Low, "Tracking_OutputForRootNotFound", new object[] { str });
            }
            else
            {
                list.AddRange(outputs.Values);
                if (outputs.Count > 100)
                {
                    FileTracker.LogMessageFromResources(this.Log, MessageImportance.Low, "Tracking_OutputsNotShown", new object[] { outputs.Count });
                }
                else
                {
                    FileTracker.LogMessageFromResources(this.Log, MessageImportance.Low, "Tracking_OutputsFor", new object[] { str });
                    foreach (ITaskItem item2 in list)
                    {
                        FileTracker.LogMessage(this.Log, MessageImportance.Low, "\t" + item2, new object[0]);
                    }
                }
            }
            return list.ToArray();
        }

        public ITaskItem[] OutputsForSource(params ITaskItem[] sources)
        {
            Dictionary<string, ITaskItem> outputs = new Dictionary<string, ITaskItem>(StringComparer.OrdinalIgnoreCase);
            if (!this.tlogAvailable)
            {
                return null;
            }
            string str = FileTracker.FormatRootingMarker(sources);
            List<ITaskItem> list = new List<ITaskItem>();
            foreach (string str2 in this.dependencyTable.Keys)
            {
                string str3 = str2.ToUpperInvariant();
                if ((str.Contains(str3) || str3.Contains(str)) || this.RootContainsAllSubRootComponents(str, str3))
                {
                    this.OutputsForSourceRoot(outputs, str3);
                }
            }
            if (outputs.Count == 0)
            {
                FileTracker.LogMessageFromResources(this.Log, MessageImportance.Low, "Tracking_OutputForRootNotFound", new object[] { str });
            }
            else
            {
                list.AddRange(outputs.Values);
                if (outputs.Count > 100)
                {
                    FileTracker.LogMessageFromResources(this.Log, MessageImportance.Low, "Tracking_OutputsNotShown", new object[] { outputs.Count });
                }
                else
                {
                    FileTracker.LogMessageFromResources(this.Log, MessageImportance.Low, "Tracking_OutputsFor", new object[] { str });
                    foreach (ITaskItem item in list)
                    {
                        FileTracker.LogMessage(this.Log, MessageImportance.Low, "\t" + item, new object[0]);
                    }
                }
            }
            return list.ToArray();
        }

        private void OutputsForSourceRoot(Dictionary<string, ITaskItem> outputs, string sourceKey)
        {
            Dictionary<string, DateTime> dictionary;
            if (this.dependencyTable.TryGetValue(sourceKey, out dictionary))
            {
                foreach (string str in dictionary.Keys)
                {
                    ITaskItem item;
                    if (!outputs.TryGetValue(str, out item))
                    {
                        outputs.Add(str, new TaskItem(str));
                    }
                }
            }
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
            Dictionary<string, DateTime> dictionary;
            if (this.dependencyTable.TryGetValue(rootingMarker, out dictionary))
            {
                Dictionary<string, DateTime> dictionary2 = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);
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
                        dictionary2.Add(str, DateTime.Now);
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
            Dictionary<string, DateTime> dictionary;
            if (this.dependencyTable.TryGetValue(rootingMarker, out dictionary))
            {
                dictionary.Remove(FileUtilities.NormalizePath(dependencyToRemove.ItemSpec));
            }
            else
            {
                FileTracker.LogMessageFromResources(this.Log, MessageImportance.Normal, "Tracking_WriteLogEntryNotFound", new object[] { rootingMarker });
            }
        }

        public void RemoveEntriesForSource(ITaskItem source)
        {
            this.RemoveEntriesForSource(new ITaskItem[] { source }, null);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void RemoveEntriesForSource(ITaskItem[] source)
        {
            this.RemoveEntriesForSource(source, null);
        }

        public void RemoveEntriesForSource(ITaskItem source, ITaskItem correspondingOutput)
        {
            this.RemoveEntriesForSource(new ITaskItem[] { source }, new ITaskItem[] { correspondingOutput });
        }

        public void RemoveEntriesForSource(ITaskItem[] source, ITaskItem[] correspondingOutputs)
        {
            string key = FileTracker.FormatRootingMarker(source, correspondingOutputs);
            this.dependencyTable.Remove(key);
            foreach (ITaskItem item in source)
            {
                this.dependencyTable.Remove(FileUtilities.NormalizePath(item.ItemSpec));
            }
        }

        public bool RemoveOutputForSourceRoot(string sourceRoot, string outputPathToRemove)
        {
            if (!this.DependencyTable.ContainsKey(sourceRoot))
            {
                return true;
            }
            bool flag = this.DependencyTable[sourceRoot].Remove(outputPathToRemove);
            if (this.DependencyTable[sourceRoot].Count == 0)
            {
                this.DependencyTable.Remove(sourceRoot);
            }
            return flag;
        }

        public string[] RemoveRootsWithSharedOutputs(ITaskItem[] sources)
        {
            Dictionary<string, DateTime> dictionary;
            ErrorUtilities.VerifyThrowArgumentNull(sources, "sources");
            List<string> list = new List<string>();
            string key = FileTracker.FormatRootingMarker(sources);
            if (this.DependencyTable.TryGetValue(key, out dictionary))
            {
                foreach (KeyValuePair<string, Dictionary<string, DateTime>> pair in this.DependencyTable)
                {
                    if (!key.Equals(pair.Key, StringComparison.Ordinal))
                    {
                        foreach (string str2 in dictionary.Keys)
                        {
                            if (pair.Value.ContainsKey(str2))
                            {
                                list.Add(pair.Key);
                                break;
                            }
                        }
                    }
                }
                foreach (string str3 in list)
                {
                    this.DependencyTable.Remove(str3);
                }
            }
            return list.ToArray();
        }

        internal bool RootContainsAllSubRootComponents(string compositeRoot, string compositeSubRoot)
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
                        Dictionary<string, DateTime> dictionary = this.dependencyTable[str3];
                        writer.WriteLine("^" + str3);
                        foreach (string str4 in dictionary.Keys)
                        {
                            if ((includeInTLog == null) || includeInTLog(str4))
                            {
                                writer.WriteLine(str4);
                            }
                        }
                    }
                }
            }
        }

        public Dictionary<string, Dictionary<string, DateTime>> DependencyTable
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.dependencyTable;
            }
        }
    }
}

