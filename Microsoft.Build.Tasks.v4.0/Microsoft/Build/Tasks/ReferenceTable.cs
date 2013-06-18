namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Shared;
    using Microsoft.Build.Utilities;
    using Microsoft.Internal.Performance;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;
    using System.Threading;

    internal sealed class ReferenceTable
    {
        private string[] allowedAssemblyExtensions;
        private CheckIfAssemblyInGac checkIfAssemblyIsInGac;
        private Microsoft.Build.Tasks.Resolver[] compiledSearchPaths;
        private bool copyLocalDependenciesWhenParentReferenceInGac;
        private Microsoft.Build.Shared.DirectoryExists directoryExists;
        private Microsoft.Build.Shared.FileExists fileExists;
        private bool findDependencies = true;
        private bool findRelatedFiles = true;
        private bool findSatellites = true;
        private bool findSerializationAssemblies = true;
        private string[] frameworkPaths;
        private GetAssemblyMetadata getAssemblyMetadata;
        private GetAssemblyName getAssemblyName;
        private Microsoft.Build.Tasks.GetDirectories getDirectories;
        private GetAssemblyRuntimeVersion getRuntimeVersion;
        private InstalledAssemblies installedAssemblies;
        private string[] latestTargetFrameworkDirectories;
        private List<string> listOfExcludedAssemblies;
        private TaskLoggingHelper log;
        private static Dictionary<string, Tuple<RedistList, string>> monikerToHighestRedistList = new Dictionary<string, Tuple<RedistList, string>>(StringComparer.OrdinalIgnoreCase);
        private OpenBaseKey openBaseKey;
        private Version projectTargetFramework;
        private Dictionary<AssemblyNameExtension, Reference> references = new Dictionary<AssemblyNameExtension, Reference>(AssemblyNameComparer.genericComparer);
        private string[] relatedFileExtensions;
        private DependentAssembly[] remappedAssemblies = new DependentAssembly[0];
        private Version targetedRuntimeVersion;
        private FrameworkName targetFrameworkMoniker;
        private static readonly Version TargetFrameworkVersion_40 = new Version("4.0");
        private System.Reflection.ProcessorArchitecture targetProcessorArchitecture;

        internal ReferenceTable(bool findDependencies, bool findSatellites, bool findSerializationAssemblies, bool findRelatedFiles, string[] searchPaths, string[] allowedAssemblyExtensions, string[] relatedFileExtensions, string[] candidateAssemblyFiles, string[] frameworkPaths, InstalledAssemblies installedAssemblies, System.Reflection.ProcessorArchitecture targetProcessorArchitecture, Microsoft.Build.Shared.FileExists fileExists, Microsoft.Build.Shared.DirectoryExists directoryExists, Microsoft.Build.Tasks.GetDirectories getDirectories, GetAssemblyName getAssemblyName, GetAssemblyMetadata getAssemblyMetadata, GetRegistrySubKeyNames getRegistrySubKeyNames, GetRegistrySubKeyDefaultValue getRegistrySubKeyDefaultValue, OpenBaseKey openBaseKey, GetAssemblyRuntimeVersion getRuntimeVersion, Version targetedRuntimeVersion, Version projectTargetFramework, FrameworkName targetFrameworkMoniker, TaskLoggingHelper log, string[] latestTargetFrameworkDirectories, bool copyLocalDependenciesWhenParentReferenceInGac, CheckIfAssemblyInGac checkIfAssemblyIsInGac)
        {
            this.log = log;
            this.findDependencies = findDependencies;
            this.findSatellites = findSatellites;
            this.findSerializationAssemblies = findSerializationAssemblies;
            this.findRelatedFiles = findRelatedFiles;
            this.frameworkPaths = frameworkPaths;
            this.allowedAssemblyExtensions = allowedAssemblyExtensions;
            this.relatedFileExtensions = relatedFileExtensions;
            this.installedAssemblies = installedAssemblies;
            this.targetProcessorArchitecture = targetProcessorArchitecture;
            this.fileExists = fileExists;
            this.directoryExists = directoryExists;
            this.getDirectories = getDirectories;
            this.getAssemblyName = getAssemblyName;
            this.getAssemblyMetadata = getAssemblyMetadata;
            this.getRuntimeVersion = getRuntimeVersion;
            this.projectTargetFramework = projectTargetFramework;
            this.targetedRuntimeVersion = targetedRuntimeVersion;
            this.openBaseKey = openBaseKey;
            this.targetFrameworkMoniker = targetFrameworkMoniker;
            this.latestTargetFrameworkDirectories = latestTargetFrameworkDirectories;
            this.copyLocalDependenciesWhenParentReferenceInGac = copyLocalDependenciesWhenParentReferenceInGac;
            this.checkIfAssemblyIsInGac = checkIfAssemblyIsInGac;
            this.compiledSearchPaths = AssemblyResolution.CompileSearchPaths(searchPaths, candidateAssemblyFiles, targetProcessorArchitecture, frameworkPaths, fileExists, getAssemblyName, getRegistrySubKeyNames, getRegistrySubKeyDefaultValue, openBaseKey, installedAssemblies, getRuntimeVersion, targetedRuntimeVersion);
        }

        internal void AddReference(AssemblyNameExtension assemblyName, Reference reference)
        {
            Microsoft.Build.Shared.ErrorUtilities.VerifyThrow(assemblyName.Name != null, "Got an empty assembly name.");
            this.references[assemblyName] = reference;
        }

        private static void AddToDependencyGraph(Dictionary<Reference, List<ReferenceAssemblyExtensionPair>> dependencyGraph, AssemblyNameExtension assemblyName, Reference assemblyReference)
        {
            foreach (Reference reference in assemblyReference.GetDependees())
            {
                List<ReferenceAssemblyExtensionPair> list = null;
                if (!dependencyGraph.TryGetValue(reference, out list))
                {
                    list = new List<ReferenceAssemblyExtensionPair>();
                    dependencyGraph.Add(reference, list);
                }
                list.Add(new ReferenceAssemblyExtensionPair(assemblyReference, assemblyName));
            }
        }

        private Hashtable BuildSimpleNameTable()
        {
            Hashtable hashtable = new Hashtable(StringComparer.CurrentCultureIgnoreCase);
            foreach (AssemblyNameExtension extension in this.References.Keys)
            {
                AssemblyNameReference reference;
                reference.assemblyName = extension;
                reference.reference = this.GetReference(extension);
                string name = extension.Name;
                if (!hashtable.ContainsKey(name))
                {
                    hashtable[name] = new ArrayList();
                }
                ((ArrayList) hashtable[name]).Add(reference);
            }
            return hashtable;
        }

        internal static void CalcuateParentAssemblyDirectories(Hashtable parentReferenceFolderHash, List<string> parentReferenceFolders, Reference parentReference)
        {
            string directoryName = parentReference.DirectoryName;
            string resolvedSearchPath = parentReference.ResolvedSearchPath;
            bool flag = false;
            bool flag2 = false;
            if (!string.IsNullOrEmpty(resolvedSearchPath))
            {
                flag = resolvedSearchPath.Equals("{gac}", StringComparison.OrdinalIgnoreCase);
                flag2 = resolvedSearchPath.Equals("{assemblyfolders}", StringComparison.OrdinalIgnoreCase);
            }
            if ((!parentReferenceFolderHash.ContainsKey(directoryName) && !flag) && !flag2)
            {
                parentReferenceFolderHash[directoryName] = string.Empty;
                parentReferenceFolders.Add(directoryName);
            }
        }

        private void ComputeClosure()
        {
            bool flag = true;
            int num = 0;
            do
            {
                bool flag2 = true;
                int num2 = 0;
                do
                {
                    this.ResolveAssemblyFilenames();
                    flag2 = this.FindAssociatedFiles();
                    num2++;
                    Microsoft.Build.Shared.ErrorUtilities.VerifyThrow(num2 < 0x186a0, "Maximum iterations exceeded while looking for dependencies.");
                }
                while (flag2);
                flag = false;
                foreach (Reference reference in this.References.Values)
                {
                    if (!reference.IsResolved && !reference.IsUnresolvable)
                    {
                        flag = true;
                        break;
                    }
                }
                num++;
                Microsoft.Build.Shared.ErrorUtilities.VerifyThrow(num < 0x186a0, "Maximum iterations exceeded while looking for resolvable references.");
            }
            while (flag);
        }

        internal void ComputeClosure(DependentAssembly[] remappedAssembliesValue, ITaskItem[] referenceAssemblyFiles, ITaskItem[] referenceAssemblyNames, ArrayList exceptions)
        {
            using (new CodeMarkerStartEnd(CodeMarkerEvent.perfMSBuildRARComputeClosureBegin, CodeMarkerEvent.perfMSBuildRARComputeClosureEnd))
            {
                this.references.Clear();
                this.remappedAssemblies = remappedAssembliesValue;
                this.SetPrimaryItems(referenceAssemblyFiles, referenceAssemblyNames, exceptions);
                this.ComputeClosure();
            }
        }

        private bool FindAssociatedFiles()
        {
            bool flag = false;
            ArrayList newEntries = new ArrayList();
            foreach (Reference reference in this.References.Values)
            {
                if (reference.IsResolved && !reference.DependenciesFound)
                {
                    reference.DependenciesFound = true;
                    try
                    {
                        bool flag2 = false;
                        string strA = Microsoft.Build.Shared.FileUtilities.EnsureTrailingSlash(reference.DirectoryName);
                        foreach (string str2 in this.frameworkPaths)
                        {
                            if (string.Compare(strA, str2, StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                flag2 = true;
                            }
                        }
                        if (!flag2 && !reference.EmbedInteropTypes)
                        {
                            if (this.findRelatedFiles)
                            {
                                this.FindRelatedFiles(reference);
                            }
                            if (this.findSatellites)
                            {
                                this.FindSatellites(reference);
                            }
                            if (this.findSerializationAssemblies)
                            {
                                this.FindSerializationAssemblies(reference);
                            }
                            if (this.findDependencies)
                            {
                                this.FindDependenciesAndScatterFiles(reference, newEntries);
                            }
                            if (newEntries.Count > 0)
                            {
                                break;
                            }
                        }
                    }
                    catch (PathTooLongException exception)
                    {
                        reference.AddError(new DependencyResolutionException(exception.Message, exception));
                    }
                }
            }
            foreach (DictionaryEntry entry in newEntries)
            {
                flag = true;
                this.AddReference((AssemblyNameExtension) entry.Key, (Reference) entry.Value);
            }
            return flag;
        }

        private static void FindCopyLocalItems(ITaskItem[] items, ArrayList copyLocalItems)
        {
            foreach (ITaskItem item in items)
            {
                bool flag;
                bool flag2 = MetadataConversionUtilities.TryConvertItemMetadataToBool(item, "CopyLocal", out flag);
                if (flag && flag2)
                {
                    copyLocalItems.Add(item);
                }
            }
        }

        private void FindDependenciesAndScatterFiles(Reference reference, ArrayList newEntries)
        {
            try
            {
                IEnumerable<UnifiedAssemblyName> unifiedDependencies = null;
                string[] scatterFiles = null;
                this.GetUnifiedAssemblyMetadata(reference, out unifiedDependencies, out scatterFiles);
                reference.AttachScatterFiles(scatterFiles);
                if (unifiedDependencies != null)
                {
                    foreach (UnifiedAssemblyName name in unifiedDependencies)
                    {
                        Reference reference2 = this.GetReference(name.PostUnified);
                        if (reference2 == null)
                        {
                            Reference reference3 = new Reference();
                            reference3.MakeDependentAssemblyReference(reference);
                            if (name.IsUnified)
                            {
                                reference3.AddPreUnificationVersion(reference.FullPath, name.PreUnified.Version, name.UnificationReason);
                            }
                            reference3.IsPrerequisite = name.IsPrerequisite;
                            DictionaryEntry entry = new DictionaryEntry(name.PostUnified, reference3);
                            newEntries.Add(entry);
                        }
                        else if (reference2 != reference)
                        {
                            reference2.AddSourceItems(reference.GetSourceItems());
                            reference2.AddDependee(reference);
                            if (name.IsUnified)
                            {
                                reference2.AddPreUnificationVersion(reference.FullPath, name.PreUnified.Version, name.UnificationReason);
                            }
                            reference2.IsPrerequisite = name.IsPrerequisite;
                        }
                    }
                }
            }
            catch (FileNotFoundException exception)
            {
                reference.AddError(new DependencyResolutionException(exception.Message, exception));
            }
            catch (FileLoadException exception2)
            {
                reference.AddError(new DependencyResolutionException(exception2.Message, exception2));
            }
            catch (BadImageFormatException exception3)
            {
                reference.AddError(new DependencyResolutionException(exception3.Message, exception3));
            }
            catch (COMException exception4)
            {
                reference.AddError(new DependencyResolutionException(exception4.Message, exception4));
            }
            catch (Exception exception5)
            {
                if (Microsoft.Build.Shared.ExceptionHandling.NotExpectedException(exception5))
                {
                    throw;
                }
                reference.AddError(new DependencyResolutionException(exception5.Message, exception5));
            }
        }

        private void FindRelatedFiles(Reference reference)
        {
            string fullPathWithoutExtension = reference.FullPathWithoutExtension;
            foreach (string str2 in this.relatedFileExtensions)
            {
                string path = fullPathWithoutExtension + str2;
                if (this.fileExists(path))
                {
                    reference.AddRelatedFileExtension(str2);
                }
            }
        }

        private void FindSatellites(Reference reference)
        {
            try
            {
                if (this.directoryExists(reference.DirectoryName))
                {
                    string[] strArray = this.getDirectories(reference.DirectoryName, "*.");
                    string str = reference.FileNameWithoutExtension + ".resources.dll";
                    foreach (string str2 in strArray)
                    {
                        string fileName = Path.GetFileName(str2);
                        if (CultureStringUtilities.IsValidCultureString(fileName))
                        {
                            string path = Path.Combine(str2, str);
                            if (this.fileExists(path))
                            {
                                reference.AddSatelliteFile(Path.Combine(fileName, str));
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                if (Microsoft.Build.Shared.ExceptionHandling.NotExpectedException(exception))
                {
                    throw;
                }
                if (this.log != null)
                {
                    this.log.LogErrorFromResources("ResolveAssemblyReference.ProblemFindingSatelliteAssemblies", new object[] { reference.FullPath, exception.Message });
                }
            }
        }

        private void FindSerializationAssemblies(Reference reference)
        {
            if (this.directoryExists(reference.DirectoryName))
            {
                string str = reference.FileNameWithoutExtension + ".XmlSerializers.dll";
                string path = Path.Combine(reference.DirectoryName, str);
                if (this.fileExists(path))
                {
                    reference.AddSerializationAssemblyFile(str);
                }
            }
        }

        private Tuple<RedistList, string> GetHighestVersionFullFrameworkForTFM(FrameworkName targetFrameworkMoniker)
        {
            RedistList redistList = null;
            Tuple<RedistList, string> tuple = null;
            if (targetFrameworkMoniker != null)
            {
                lock (monikerToHighestRedistList)
                {
                    if (monikerToHighestRedistList.TryGetValue(targetFrameworkMoniker.Identifier, out tuple))
                    {
                        return tuple;
                    }
                    IList<string> highestVersionReferenceAssemblyDirectories = null;
                    string fullName = null;
                    if ((this.latestTargetFrameworkDirectories != null) && (this.latestTargetFrameworkDirectories.Length > 0))
                    {
                        highestVersionReferenceAssemblyDirectories = new List<string>(this.latestTargetFrameworkDirectories);
                        fullName = string.Join(";", this.latestTargetFrameworkDirectories);
                    }
                    else if (targetFrameworkMoniker != null)
                    {
                        FrameworkName highestVersionMoniker = null;
                        highestVersionReferenceAssemblyDirectories = GetHighestVersionReferenceAssemblyDirectories(targetFrameworkMoniker, out highestVersionMoniker);
                        if (highestVersionMoniker != null)
                        {
                            fullName = highestVersionMoniker.FullName;
                        }
                    }
                    if ((highestVersionReferenceAssemblyDirectories != null) && (highestVersionReferenceAssemblyDirectories.Count > 0))
                    {
                        HashSet<string> set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                        List<AssemblyTableInfo> list3 = new List<AssemblyTableInfo>();
                        foreach (string str2 in highestVersionReferenceAssemblyDirectories)
                        {
                            foreach (string str3 in RedistList.GetRedistListPathsFromDisk(str2))
                            {
                                if (!set.Contains(str3))
                                {
                                    list3.Add(new AssemblyTableInfo(str3, str2));
                                    set.Add(str3);
                                }
                            }
                        }
                        redistList = RedistList.GetRedistList(list3.ToArray());
                    }
                    tuple = new Tuple<RedistList, string>(redistList, fullName);
                    monikerToHighestRedistList.Add(targetFrameworkMoniker.Identifier, tuple);
                }
            }
            return tuple;
        }

        private static IList<string> GetHighestVersionReferenceAssemblyDirectories(FrameworkName targetFrameworkMoniker, out FrameworkName highestVersionMoniker)
        {
            string programFilesReferenceAssemblyRoot = ToolLocationHelper.GetProgramFilesReferenceAssemblyRoot();
            highestVersionMoniker = ToolLocationHelper.HighestVersionOfTargetFrameworkIdentifier(programFilesReferenceAssemblyRoot, targetFrameworkMoniker.Identifier);
            if (highestVersionMoniker == null)
            {
                return new List<string>();
            }
            return ToolLocationHelper.GetPathToReferenceAssemblies(programFilesReferenceAssemblyRoot, highestVersionMoniker);
        }

        internal Reference GetReference(AssemblyNameExtension assemblyName)
        {
            Microsoft.Build.Shared.ErrorUtilities.VerifyThrow(assemblyName.Name != null, "Got an empty assembly name.");
            Reference reference = null;
            this.references.TryGetValue(assemblyName, out reference);
            return reference;
        }

        internal AssemblyNameExtension GetReferenceFromItemSpec(string itemSpec)
        {
            foreach (AssemblyNameExtension extension in this.references.Keys)
            {
                Reference reference = this.references[extension];
                if (reference.IsPrimary && reference.PrimarySourceItem.ItemSpec.Equals(itemSpec, StringComparison.OrdinalIgnoreCase))
                {
                    return extension;
                }
            }
            return null;
        }

        internal void GetReferenceItems(out ITaskItem[] primaryFiles, out ITaskItem[] dependencyFiles, out ITaskItem[] relatedFiles, out ITaskItem[] satelliteFiles, out ITaskItem[] serializationAssemblyFiles, out ITaskItem[] scatterFiles, out ITaskItem[] copyLocalFiles)
        {
            primaryFiles = new ITaskItem[0];
            dependencyFiles = new ITaskItem[0];
            relatedFiles = new ITaskItem[0];
            satelliteFiles = new ITaskItem[0];
            serializationAssemblyFiles = new ITaskItem[0];
            scatterFiles = new ITaskItem[0];
            copyLocalFiles = new ITaskItem[0];
            ArrayList list = new ArrayList();
            ArrayList list2 = new ArrayList();
            ArrayList relatedItems = new ArrayList();
            ArrayList satelliteItems = new ArrayList();
            ArrayList serializationAssemblyItems = new ArrayList();
            ArrayList scatterItems = new ArrayList();
            ArrayList copyLocalItems = new ArrayList();
            foreach (AssemblyNameExtension extension in this.References.Keys)
            {
                string fullName = extension.FullName;
                Reference reference = this.GetReference(extension);
                if (!reference.IsBadImage)
                {
                    reference.SetFinalCopyLocalState(extension, this.frameworkPaths, this.targetProcessorArchitecture, this.getRuntimeVersion, this.targetedRuntimeVersion, this.fileExists, this.copyLocalDependenciesWhenParentReferenceInGac, this, this.checkIfAssemblyIsInGac);
                    if ((reference.IsPrimary || !IsPseudoAssembly(extension.Name)) && reference.IsResolved)
                    {
                        ITaskItem item = SetItemMetadata(relatedItems, satelliteItems, serializationAssemblyItems, scatterItems, fullName, reference);
                        if (reference.IsPrimary)
                        {
                            if (!reference.IsBadImage)
                            {
                                list.Add(item);
                            }
                        }
                        else
                        {
                            list2.Add(item);
                        }
                    }
                }
            }
            primaryFiles = new ITaskItem[list.Count];
            list.CopyTo(primaryFiles, 0);
            dependencyFiles = (ITaskItem[]) list2.ToArray(typeof(ITaskItem));
            relatedFiles = (ITaskItem[]) relatedItems.ToArray(typeof(ITaskItem));
            satelliteFiles = (ITaskItem[]) satelliteItems.ToArray(typeof(ITaskItem));
            serializationAssemblyFiles = (ITaskItem[]) serializationAssemblyItems.ToArray(typeof(ITaskItem));
            scatterFiles = (ITaskItem[]) scatterItems.ToArray(typeof(ITaskItem));
            Array.Sort(primaryFiles, TaskItemSpecFilenameComparer.comparer);
            FindCopyLocalItems(primaryFiles, copyLocalItems);
            FindCopyLocalItems(dependencyFiles, copyLocalItems);
            FindCopyLocalItems(relatedFiles, copyLocalItems);
            FindCopyLocalItems(satelliteFiles, copyLocalItems);
            FindCopyLocalItems(serializationAssemblyFiles, copyLocalItems);
            FindCopyLocalItems(scatterFiles, copyLocalItems);
            copyLocalFiles = (ITaskItem[]) copyLocalItems.ToArray(typeof(ITaskItem));
        }

        private void GetUnifiedAssemblyMetadata(Reference reference, out IEnumerable<UnifiedAssemblyName> unifiedDependencies, out string[] scatterFiles)
        {
            if (reference.IsPrerequisite || reference.IsBadImage)
            {
                unifiedDependencies = null;
                scatterFiles = null;
            }
            else
            {
                AssemblyNameExtension[] dependencies = null;
                this.getAssemblyMetadata(reference.FullPath, out dependencies, out scatterFiles);
                unifiedDependencies = this.GetUnifiedAssemblyNames(dependencies);
            }
        }

        private IEnumerable<UnifiedAssemblyName> GetUnifiedAssemblyNames(IEnumerable<AssemblyNameExtension> preUnificationAssemblyNames)
        {
            foreach (AssemblyNameExtension iteratorVariable0 in preUnificationAssemblyNames)
            {
                UnificationReason iteratorVariable6;
                string iteratorVariable5;
                bool? iteratorVariable4;
                bool iteratorVariable3;
                Version iteratorVariable2;
                string name = iteratorVariable0.Name;
                AssemblyNameExtension assemblyName = new AssemblyNameExtension((AssemblyName) iteratorVariable0.AssemblyName.Clone());
                bool isUnified = this.UnifyAssemblyNameVersions(assemblyName, out iteratorVariable2, out iteratorVariable6, out iteratorVariable3, out iteratorVariable4, out iteratorVariable5);
                assemblyName.ReplaceVersion(iteratorVariable2);
                yield return new UnifiedAssemblyName(iteratorVariable0, assemblyName, isUnified, iteratorVariable6, iteratorVariable3, iteratorVariable4, iteratorVariable5);
            }
        }

        private bool InLatestRedistList(AssemblyNameExtension assemblyName, Reference reference)
        {
            bool flag = false;
            Tuple<RedistList, string> highestVersionFullFrameworkForTFM = this.GetHighestVersionFullFrameworkForTFM(this.targetFrameworkMoniker);
            if (((highestVersionFullFrameworkForTFM != null) && (highestVersionFullFrameworkForTFM.Item1 != null)) && highestVersionFullFrameworkForTFM.Item1.FrameworkAssemblyEntryInRedist(assemblyName))
            {
                flag = true;
            }
            return flag;
        }

        private static bool IsPseudoAssembly(string name)
        {
            return (string.Compare(name, "mscorlib", StringComparison.OrdinalIgnoreCase) == 0);
        }

        internal void LogAnotherFrameworkUnResolve(bool displayPrimaryReferenceMessage, AssemblyNameExtension assemblyName, Reference reference, ITaskItem referenceItem, string targetedFramework)
        {
            if (displayPrimaryReferenceMessage)
            {
                this.log.LogWarningWithCodeFromResources("ResolveAssemblyReference.PrimaryReferenceInAnotherFramework", new object[] { reference.PrimarySourceItem.ItemSpec, targetedFramework });
            }
            else
            {
                this.log.LogWarningWithCodeFromResources("ResolveAssemblyReference.DependencyReferenceInAnotherFramework", new object[] { referenceItem.ItemSpec, assemblyName.FullName, targetedFramework });
            }
        }

        internal void LogHigherVersionUnresolve(bool displayPrimaryReferenceMessage, AssemblyNameExtension assemblyName, Reference reference, ITaskItem referenceItem, string targetedFramework)
        {
            if (displayPrimaryReferenceMessage)
            {
                this.log.LogWarningWithCodeFromResources("ResolveAssemblyReference.PrimaryReferenceOutsideOfFramework", new object[] { reference.PrimarySourceItem.ItemSpec, reference.ReferenceVersion, reference.ExclusionListLoggingProperties.HighestVersionInRedist });
            }
            else
            {
                this.log.LogWarningWithCodeFromResources("ResolveAssemblyReference.DependencyReferenceOutsideOfFramework", new object[] { referenceItem.ItemSpec, assemblyName.FullName, reference.ReferenceVersion, reference.ExclusionListLoggingProperties.HighestVersionInRedist });
            }
        }

        internal void LogProfileExclusionUnresolve(bool displayPrimaryReferenceMessage, AssemblyNameExtension assemblyName, Reference reference, ITaskItem referenceItem, string targetedFramework)
        {
            if (displayPrimaryReferenceMessage)
            {
                this.log.LogWarningWithCodeFromResources("ResolveAssemblyReference.FailedToResolveReferenceBecausePrimaryAssemblyInExclusionList", new object[] { reference.PrimarySourceItem.ItemSpec, targetedFramework });
            }
            else
            {
                this.log.LogWarningWithCodeFromResources("ResolveAssemblyReference.FailBecauseDependentAssemblyInExclusionList", new object[] { referenceItem.ItemSpec, assemblyName.FullName, targetedFramework });
            }
        }

        internal bool MarkReferenceForExclusionDueToHigherThanCurrentFramework(AssemblyNameExtension assemblyName, Reference reference)
        {
            bool flag2 = false;
            if (((reference.ReferenceVersion != null) && (reference.ExclusionListLoggingProperties.HighestVersionInRedist != null)) && (reference.ReferenceVersion.CompareTo(reference.ExclusionListLoggingProperties.HighestVersionInRedist) > 0))
            {
                LogExclusionReason reason = new LogExclusionReason(this.LogHigherVersionUnresolve);
                reference.ExclusionListLoggingProperties.ExclusionReasonLogDelegate = reason;
                reference.ExclusionListLoggingProperties.IsInExclusionList = true;
                flag2 = true;
            }
            return flag2;
        }

        internal bool MarkReferencesExcludedDueToOtherFramework(AssemblyNameExtension assemblyName, Reference reference)
        {
            bool flag = false;
            string resolvedSearchPath = reference.ResolvedSearchPath;
            bool flag2 = resolvedSearchPath.Equals("{gac}", StringComparison.OrdinalIgnoreCase);
            bool flag3 = resolvedSearchPath.Equals("{assemblyfolders}", StringComparison.OrdinalIgnoreCase);
            if ((!flag2 && !flag3) && reference.IsResolved)
            {
                return false;
            }
            if (this.InLatestRedistList(assemblyName, reference))
            {
                LogExclusionReason reason = new LogExclusionReason(this.LogAnotherFrameworkUnResolve);
                reference.ExclusionListLoggingProperties.ExclusionReasonLogDelegate = reason;
                reference.ExclusionListLoggingProperties.IsInExclusionList = true;
                flag = true;
            }
            return flag;
        }

        internal bool MarkReferencesForExclusion(Hashtable exclusionList)
        {
            bool flag = false;
            this.listOfExcludedAssemblies = new List<string>();
            foreach (AssemblyNameExtension extension in this.References.Keys)
            {
                string fullName = extension.FullName;
                Reference reference = this.GetReference(extension);
                reference.ReferenceVersion = extension.Version;
                this.MarkReferenceWithHighestVersionInCurrentRedistList(extension, reference);
                if (!reference.CheckForSpecificVersionMetadataOnParentsReference(false))
                {
                    if ((exclusionList != null) && exclusionList.ContainsKey(fullName))
                    {
                        flag = true;
                        reference.ExclusionListLoggingProperties.ExclusionReasonLogDelegate = new LogExclusionReason(this.LogProfileExclusionUnresolve);
                        reference.ExclusionListLoggingProperties.IsInExclusionList = true;
                        this.listOfExcludedAssemblies.Add(fullName);
                    }
                    if (!reference.ExclusionListLoggingProperties.IsInExclusionList && this.MarkReferenceForExclusionDueToHigherThanCurrentFramework(extension, reference))
                    {
                        flag = true;
                        this.listOfExcludedAssemblies.Add(fullName);
                    }
                    if (!reference.ExclusionListLoggingProperties.IsInExclusionList && this.MarkReferencesExcludedDueToOtherFramework(extension, reference))
                    {
                        flag = true;
                        this.listOfExcludedAssemblies.Add(fullName);
                    }
                }
            }
            return flag;
        }

        internal void MarkReferenceWithHighestVersionInCurrentRedistList(AssemblyNameExtension assemblyName, Reference reference)
        {
            if (this.installedAssemblies != null)
            {
                AssemblyEntry entry = this.installedAssemblies.FindHighestVersionInRedistList(assemblyName);
                if (entry != null)
                {
                    reference.ExclusionListLoggingProperties.HighestVersionInRedist = entry.AssemblyNameExtension.Version;
                }
            }
        }

        private AssemblyNameExtension NameAssemblyFileReference(Reference reference, string assemblyFileName)
        {
            AssemblyNameExtension extension = null;
            if (!Path.IsPathRooted(assemblyFileName))
            {
                reference.FullPath = Path.GetFullPath(assemblyFileName);
            }
            else
            {
                reference.FullPath = assemblyFileName;
            }
            try
            {
                if (this.directoryExists(assemblyFileName))
                {
                    extension = new AssemblyNameExtension("*directory*");
                    reference.AddError(new ReferenceResolutionException(Microsoft.Build.Shared.ResourceUtilities.FormatResourceString("General.ExpectedFileGotDirectory", new object[] { reference.FullPath }), null));
                    reference.FullPath = string.Empty;
                }
                else
                {
                    if (this.fileExists(assemblyFileName))
                    {
                        extension = this.getAssemblyName(assemblyFileName);
                        if (extension != null)
                        {
                            reference.ResolvedSearchPath = assemblyFileName;
                        }
                    }
                    if (extension == null)
                    {
                        reference.AddError(new DependencyResolutionException(Microsoft.Build.Shared.ResourceUtilities.FormatResourceString("General.ExpectedFileMissing", new object[] { reference.FullPath }), null));
                    }
                }
            }
            catch (BadImageFormatException exception)
            {
                reference.AddError(new DependencyResolutionException(exception.Message, exception));
            }
            catch (UnauthorizedAccessException exception2)
            {
                reference.AddError(new DependencyResolutionException(exception2.Message, exception2));
            }
            if (extension == null)
            {
                extension = new AssemblyNameExtension(Path.GetFileNameWithoutExtension(assemblyFileName));
            }
            return extension;
        }

        private void RemoveDependencies(Reference removedReference, Dictionary<AssemblyNameExtension, Reference> referenceList, Dictionary<Reference, List<ReferenceAssemblyExtensionPair>> dependencyList)
        {
            List<ReferenceAssemblyExtensionPair> list = null;
            if (dependencyList.TryGetValue(removedReference, out list))
            {
                foreach (ReferenceAssemblyExtensionPair pair in list)
                {
                    Reference key = pair.Key;
                    key.RemoveDependee(removedReference);
                    if (!key.IsPrimary && (key.GetDependees().Count == 0))
                    {
                        referenceList.Remove(pair.Value);
                        this.RemoveDependencies(key, referenceList, dependencyList);
                    }
                }
            }
        }

        private void RemoveDependencyMarkedForExclusion(LogExclusionReason logExclusionReason, bool removeOnlyNoWarning, string subsetName, Dictionary<AssemblyNameExtension, Reference> goodReferences, List<Reference> removedReferences, AssemblyNameExtension assemblyName, Reference assemblyReference)
        {
            foreach (ITaskItem item in assemblyReference.GetSourceItems())
            {
                string itemSpec = item.ItemSpec;
                if (!assemblyReference.IsPrimary || (string.Compare(itemSpec, assemblyReference.PrimarySourceItem.ItemSpec, StringComparison.OrdinalIgnoreCase) != 0))
                {
                    AssemblyNameExtension referenceFromItemSpec = this.GetReferenceFromItemSpec(itemSpec);
                    if (referenceFromItemSpec != null)
                    {
                        Reference reference = this.references[referenceFromItemSpec];
                        if (!reference.WantSpecificVersion)
                        {
                            if (!removedReferences.Contains(reference))
                            {
                                removedReferences.Add(reference);
                                goodReferences.Remove(referenceFromItemSpec);
                            }
                            if (!removeOnlyNoWarning && (logExclusionReason != null))
                            {
                                logExclusionReason(false, assemblyName, assemblyReference, item, subsetName);
                            }
                        }
                    }
                }
            }
        }

        private static void RemovePrimaryReferenceMarkedForExclusion(LogExclusionReason logExclusionReason, bool removeOnlyNoWarning, string subsetName, List<Reference> removedReferences, AssemblyNameExtension assemblyName, Reference assemblyReference)
        {
            removedReferences.Add(assemblyReference);
            if (!removeOnlyNoWarning && (logExclusionReason != null))
            {
                logExclusionReason(true, assemblyName, assemblyReference, assemblyReference.PrimarySourceItem, subsetName);
            }
        }

        internal void RemoveReferencesMarkedForExclusion(bool removeOnlyNoWarning, string subsetName)
        {
            using (new CodeMarkerStartEnd(CodeMarkerEvent.perfMSBuildRARRemoveFromExclusionListBegin, CodeMarkerEvent.perfMSBuildRARRemoveFromExclusionListEnd))
            {
                Dictionary<AssemblyNameExtension, Reference> goodReferences = new Dictionary<AssemblyNameExtension, Reference>(AssemblyNameComparer.genericComparer);
                List<Reference> removedReferences = new List<Reference>();
                Dictionary<Reference, List<ReferenceAssemblyExtensionPair>> dependencyGraph = new Dictionary<Reference, List<ReferenceAssemblyExtensionPair>>();
                LogExclusionReason logExclusionReason = null;
                if (subsetName == null)
                {
                    subsetName = string.Empty;
                }
                foreach (AssemblyNameExtension extension in this.references.Keys)
                {
                    Reference assemblyReference = this.references[extension];
                    AddToDependencyGraph(dependencyGraph, extension, assemblyReference);
                    bool isInExclusionList = assemblyReference.ExclusionListLoggingProperties.IsInExclusionList;
                    logExclusionReason = assemblyReference.ExclusionListLoggingProperties.ExclusionReasonLogDelegate;
                    if (assemblyReference.IsPrimary)
                    {
                        if (!isInExclusionList || assemblyReference.WantSpecificVersion)
                        {
                            if (!removedReferences.Contains(assemblyReference))
                            {
                                goodReferences[extension] = assemblyReference;
                            }
                        }
                        else
                        {
                            RemovePrimaryReferenceMarkedForExclusion(logExclusionReason, removeOnlyNoWarning, subsetName, removedReferences, extension, assemblyReference);
                        }
                    }
                    ICollection sourceItems = assemblyReference.GetSourceItems();
                    if (!assemblyReference.IsPrimary || ((assemblyReference.IsPrimary && isInExclusionList) && ((sourceItems != null) && (sourceItems.Count > 1))))
                    {
                        bool flag2 = assemblyReference.CheckForSpecificVersionMetadataOnParentsReference(true);
                        if ((!isInExclusionList || flag2) && !removedReferences.Contains(assemblyReference))
                        {
                            goodReferences[extension] = assemblyReference;
                        }
                        if (isInExclusionList)
                        {
                            this.RemoveDependencyMarkedForExclusion(logExclusionReason, removeOnlyNoWarning, subsetName, goodReferences, removedReferences, extension, assemblyReference);
                        }
                    }
                }
                foreach (Reference reference2 in removedReferences)
                {
                    this.RemoveDependencies(reference2, goodReferences, dependencyGraph);
                }
                this.references = goodReferences;
            }
        }

        private void ResolveAssemblyFilenames()
        {
            foreach (AssemblyNameExtension extension in this.References.Keys)
            {
                Reference reference = this.GetReference(extension);
                if (!reference.IsResolved && !reference.IsUnresolvable)
                {
                    this.ResolveReference(extension, null, reference);
                }
            }
        }

        private static int ResolveAssemblyNameConflict(AssemblyNameReference assemblyReference0, AssemblyNameReference assemblyReference1)
        {
            int index = 0;
            Microsoft.Build.Shared.ErrorUtilities.VerifyThrow(assemblyReference0.assemblyName.FullName != null, "Got a null assembly name fullname. (0)");
            Microsoft.Build.Shared.ErrorUtilities.VerifyThrow(assemblyReference1.assemblyName.FullName != null, "Got a null assembly name fullname. (1)");
            string[] strArray = new string[] { assemblyReference0.assemblyName.FullName, assemblyReference1.assemblyName.FullName };
            Reference[] referenceArray = new Reference[] { assemblyReference0.reference, assemblyReference1.reference };
            AssemblyNameExtension[] extensionArray = new AssemblyNameExtension[] { assemblyReference0.assemblyName, assemblyReference1.assemblyName };
            bool[] flagArray = new bool[] { assemblyReference0.reference.IsPrimary, assemblyReference1.reference.IsPrimary };
            if (referenceArray[0].IsPrimary && referenceArray[1].IsPrimary)
            {
                flagArray[0] = false;
                flagArray[1] = false;
            }
            bool pfEquivalent = false;
            Microsoft.Build.Tasks.NativeMethods.AssemblyComparisonResult pResult = Microsoft.Build.Tasks.NativeMethods.AssemblyComparisonResult.ACR_Unknown;
            Microsoft.Build.Tasks.NativeMethods.CompareAssemblyIdentity(strArray[0], flagArray[0], strArray[1], flagArray[1], out pfEquivalent, out pResult);
            index = 0;
            ConflictLossReason insolubleConflict = ConflictLossReason.InsolubleConflict;
            if (referenceArray[0].IsPrimary && !referenceArray[1].IsPrimary)
            {
                index = 1;
                insolubleConflict = ConflictLossReason.WasNotPrimary;
            }
            else if (!referenceArray[0].IsPrimary && referenceArray[1].IsPrimary)
            {
                index = 0;
                insolubleConflict = ConflictLossReason.WasNotPrimary;
            }
            else if (!referenceArray[0].IsPrimary && !referenceArray[1].IsPrimary)
            {
                if (((extensionArray[0].Version != null) && (extensionArray[1].Version != null)) && (extensionArray[0].Version > extensionArray[1].Version))
                {
                    index = 1;
                    if (pfEquivalent)
                    {
                        insolubleConflict = ConflictLossReason.HadLowerVersion;
                    }
                }
                else if (((extensionArray[0].Version != null) && (extensionArray[1].Version != null)) && (extensionArray[0].Version < extensionArray[1].Version))
                {
                    index = 0;
                    if (pfEquivalent)
                    {
                        insolubleConflict = ConflictLossReason.HadLowerVersion;
                    }
                }
                else
                {
                    index = 0;
                    if (pfEquivalent)
                    {
                        insolubleConflict = ConflictLossReason.FusionEquivalentWithSameVersion;
                    }
                }
            }
            int num2 = 1 - index;
            referenceArray[index].ConflictVictorName = extensionArray[num2];
            referenceArray[index].ConflictLossExplanation = insolubleConflict;
            referenceArray[num2].AddConflictVictim(extensionArray[index]);
            return index;
        }

        internal void ResolveConflicts(out DependentAssembly[] idealRemappings, out AssemblyNameReference[] conflictingReferences)
        {
            idealRemappings = null;
            conflictingReferences = null;
            if (this.ResolveConflictsBetweenReferences() != 0)
            {
                Hashtable hashtable = new Hashtable(StringComparer.OrdinalIgnoreCase);
                Hashtable hashtable2 = new Hashtable(StringComparer.OrdinalIgnoreCase);
                foreach (AssemblyNameExtension extension in this.References.Keys)
                {
                    Reference reference = this.GetReference(extension);
                    if (reference.CheckForSpecificVersionMetadataOnParentsReference(true))
                    {
                        AssemblyEntry entry = null;
                        if (this.installedAssemblies != null)
                        {
                            entry = this.installedAssemblies.FindHighestVersionInRedistList(extension);
                        }
                        if (entry != null)
                        {
                            continue;
                        }
                    }
                    byte[] publicKeyToken = extension.GetPublicKeyToken();
                    if ((publicKeyToken != null) && (publicKeyToken.Length > 0))
                    {
                        AssemblyName name = (AssemblyName) extension.AssemblyName.Clone();
                        Version version = name.Version;
                        name.Version = null;
                        string key = name.ToString();
                        if (hashtable.ContainsKey(key))
                        {
                            hashtable[key] = ((int) hashtable[key]) + 1;
                            Version version2 = ((AssemblyNameReference) hashtable2[key]).assemblyName.Version;
                            if ((version2 == null) || (version2 < version))
                            {
                                hashtable2[key] = AssemblyNameReference.Create(extension, reference);
                            }
                        }
                        else
                        {
                            hashtable[key] = 1;
                            hashtable2[key] = AssemblyNameReference.Create(extension, reference);
                        }
                    }
                }
                List<AssemblyNameReference> list = new List<AssemblyNameReference>();
                foreach (string str2 in hashtable.Keys)
                {
                    if (((int) hashtable[str2]) > 1)
                    {
                        list.Add((AssemblyNameReference) hashtable2[str2]);
                    }
                }
                List<DependentAssembly> list2 = new List<DependentAssembly>();
                foreach (AssemblyNameReference reference2 in list)
                {
                    DependentAssembly item = new DependentAssembly {
                        PartialAssemblyName = reference2.assemblyName.AssemblyName
                    };
                    BindingRedirect redirect = new BindingRedirect {
                        OldVersionLow = new Version("0.0.0.0"),
                        OldVersionHigh = reference2.assemblyName.AssemblyName.Version,
                        NewVersion = reference2.assemblyName.AssemblyName.Version
                    };
                    item.BindingRedirects = new BindingRedirect[] { redirect };
                    list2.Add(item);
                }
                idealRemappings = list2.ToArray();
                conflictingReferences = list.ToArray();
            }
        }

        private int ResolveConflictsBetweenReferences()
        {
            int num = 0;
            Hashtable hashtable = this.BuildSimpleNameTable();
            foreach (string str in hashtable.Keys)
            {
                ArrayList list = (ArrayList) hashtable[str];
                list.Sort(AssemblyNameReferenceAscendingVersionComparer.comparer);
                while (list.Count > 1)
                {
                    int index = ResolveAssemblyNameConflict((AssemblyNameReference) list[0], (AssemblyNameReference) list[1]);
                    list.RemoveAt(index);
                    num++;
                }
            }
            return num;
        }

        private void ResolveReference(AssemblyNameExtension assemblyName, string rawFileNameCandidate, Reference reference)
        {
            string path = null;
            string resolvedSearchPath = string.Empty;
            bool userRequestedSpecificFile = false;
            ArrayList assembliesConsideredAndRejected = new ArrayList();
            Hashtable parentReferenceFolderHash = new Hashtable(StringComparer.OrdinalIgnoreCase);
            List<string> parentReferenceFolders = new List<string>();
            foreach (Reference reference2 in reference.GetDependees())
            {
                CalcuateParentAssemblyDirectories(parentReferenceFolderHash, parentReferenceFolders, reference2);
            }
            List<Microsoft.Build.Tasks.Resolver[]> jaggedResolvers = new List<Microsoft.Build.Tasks.Resolver[]> {
                AssemblyResolution.CompileDirectories(parentReferenceFolders, this.fileExists, this.getAssemblyName, this.getRuntimeVersion, this.targetedRuntimeVersion),
                this.compiledSearchPaths
            };
            try
            {
                path = AssemblyResolution.ResolveReference(jaggedResolvers, assemblyName, rawFileNameCandidate, reference.IsPrimary, reference.WantSpecificVersion, reference.GetExecutableExtensions(this.allowedAssemblyExtensions), reference.HintPath, reference.AssemblyFolderKey, assembliesConsideredAndRejected, out resolvedSearchPath, out userRequestedSpecificFile);
            }
            catch (BadImageFormatException exception)
            {
                reference.AddError(new DependencyResolutionException(exception.Message, exception));
            }
            reference.AddAssembliesConsideredAndRejected(assembliesConsideredAndRejected);
            if (path != null)
            {
                if (!Path.IsPathRooted(path))
                {
                    path = Path.GetFullPath(path);
                }
                reference.FullPath = path;
                reference.ResolvedSearchPath = resolvedSearchPath;
                reference.UserRequestedSpecificFile = userRequestedSpecificFile;
            }
            else if (assemblyName != null)
            {
                reference.AddError(new ReferenceResolutionException(Microsoft.Build.Shared.ResourceUtilities.FormatResourceString("General.CouldNotLocateAssembly", new object[] { assemblyName.FullName }), null));
            }
        }

        private static ITaskItem SetItemMetadata(ArrayList relatedItems, ArrayList satelliteItems, ArrayList serializationAssemblyItems, ArrayList scatterItems, string fusionName, Reference reference)
        {
            ITaskItem destinationItem = new TaskItem {
                ItemSpec = reference.FullPath
            };
            destinationItem.SetMetadata("ResolvedFrom", reference.ResolvedSearchPath);
            if (reference.IsCopyLocal)
            {
                destinationItem.SetMetadata("CopyLocal", "true");
            }
            else
            {
                destinationItem.SetMetadata("CopyLocal", "false");
            }
            destinationItem.SetMetadata("FusionName", fusionName);
            if (!string.IsNullOrEmpty(reference.RedistName))
            {
                destinationItem.SetMetadata("Redist", reference.RedistName);
            }
            if (reference.IsRedistRoot == true)
            {
                destinationItem.SetMetadata("IsRedistRoot", "true");
            }
            else if (reference.IsRedistRoot == false)
            {
                destinationItem.SetMetadata("IsRedistRoot", "false");
            }
            if (reference.PrimarySourceItem != null)
            {
                reference.PrimarySourceItem.CopyMetadataTo(destinationItem);
            }
            else
            {
                foreach (ITaskItem item2 in reference.GetSourceItems())
                {
                    item2.CopyMetadataTo(destinationItem);
                }
            }
            if (reference.ReferenceVersion != null)
            {
                destinationItem.SetMetadata("Version", reference.ReferenceVersion.ToString());
            }
            else
            {
                destinationItem.SetMetadata("Version", string.Empty);
            }
            foreach (string str in reference.GetRelatedFileExtensions())
            {
                ITaskItem item3 = new TaskItem(reference.FullPathWithoutExtension + str);
                destinationItem.CopyMetadataTo(item3);
                item3.SetMetadata("FusionName", "");
                relatedItems.Add(item3);
            }
            foreach (string str2 in reference.GetSatelliteFiles())
            {
                ITaskItem item4 = new TaskItem(Path.Combine(reference.DirectoryName, str2));
                destinationItem.CopyMetadataTo(item4);
                item4.SetMetadata("DestinationSubDirectory", Microsoft.Build.Shared.FileUtilities.EnsureTrailingSlash(Path.GetDirectoryName(str2)));
                item4.SetMetadata("FusionName", "");
                satelliteItems.Add(item4);
            }
            foreach (string str3 in reference.GetSerializationAssemblyFiles())
            {
                ITaskItem item5 = new TaskItem(Path.Combine(reference.DirectoryName, str3));
                destinationItem.CopyMetadataTo(item5);
                item5.SetMetadata("FusionName", "");
                serializationAssemblyItems.Add(item5);
            }
            foreach (string str4 in reference.GetScatterFiles())
            {
                ITaskItem item6 = new TaskItem(Path.Combine(reference.DirectoryName, str4));
                destinationItem.CopyMetadataTo(item6);
                item6.SetMetadata("FusionName", "");
                scatterItems.Add(item6);
            }
            return destinationItem;
        }

        private void SetPrimaryAssemblyReferenceItem(ITaskItem referenceAssemblyName)
        {
            string metadata = referenceAssemblyName.GetMetadata("ExecutableExtension");
            string itemSpec = referenceAssemblyName.ItemSpec;
            AssemblyNameExtension assemblyName = null;
            string str3 = referenceAssemblyName.ItemSpec;
            string fusionName = referenceAssemblyName.GetMetadata("FusionName");
            TryConvertToAssemblyName(str3, fusionName, ref assemblyName);
            bool metadataFound = false;
            bool wantSpecificVersionValue = MetadataConversionUtilities.TryConvertItemMetadataToBool(referenceAssemblyName, "SpecificVersion", out metadataFound);
            bool flag3 = (assemblyName != null) && assemblyName.IsSimpleName;
            Reference reference = new Reference();
            reference.MakePrimaryAssemblyReference(referenceAssemblyName, wantSpecificVersionValue, metadata);
            if ((assemblyName != null) && (flag3 || (metadataFound && !wantSpecificVersionValue)))
            {
                assemblyName = new AssemblyNameExtension(AssemblyNameExtension.EscapeDisplayNameCharacters(assemblyName.Name));
            }
            reference.HintPath = referenceAssemblyName.GetMetadata("HintPath");
            if ((this.projectTargetFramework != null) && (this.projectTargetFramework >= TargetFrameworkVersion_40))
            {
                reference.EmbedInteropTypes = MetadataConversionUtilities.TryConvertItemMetadataToBool(referenceAssemblyName, "EmbedInteropTypes");
            }
            reference.AssemblyFolderKey = referenceAssemblyName.GetMetadata("AssemblyFolderKey");
            try
            {
                this.ResolveReference(assemblyName, itemSpec, reference);
                if (reference.IsResolved)
                {
                    AssemblyNameExtension extension2 = null;
                    try
                    {
                        extension2 = this.getAssemblyName(reference.FullPath);
                    }
                    catch (ArgumentException)
                    {
                        extension2 = null;
                    }
                    if ((extension2 != null) && (extension2.Name != null))
                    {
                        assemblyName = extension2;
                    }
                }
            }
            catch (BadImageFormatException exception)
            {
                reference.AddError(new BadImageReferenceException(exception.Message, exception));
            }
            catch (FileNotFoundException exception2)
            {
                reference.AddError(new BadImageReferenceException(exception2.Message, exception2));
            }
            catch (FileLoadException exception3)
            {
                reference.AddError(new BadImageReferenceException(exception3.Message, exception3));
            }
            catch (Exception exception4)
            {
                if (Microsoft.Build.Shared.ExceptionHandling.NotExpectedException(exception4))
                {
                    throw;
                }
                reference.AddError(new BadImageReferenceException(exception4.Message, exception4));
            }
            if (assemblyName == null)
            {
                if (!reference.IsResolved)
                {
                    throw new InvalidReferenceAssemblyNameException(referenceAssemblyName.ItemSpec);
                }
                assemblyName = new AssemblyNameExtension(AssemblyNameExtension.EscapeDisplayNameCharacters(reference.FileNameWithoutExtension));
            }
            if (this.installedAssemblies == null)
            {
                reference.IsPrerequisite = false;
            }
            else
            {
                Version unifiedVersion = null;
                bool isPrerequisite = false;
                bool? isRedistRoot = null;
                string redistName = null;
                this.installedAssemblies.GetInfo(assemblyName, out unifiedVersion, out isPrerequisite, out isRedistRoot, out redistName);
                reference.IsPrerequisite = isPrerequisite;
                reference.IsRedistRoot = isRedistRoot;
                reference.RedistName = redistName;
            }
            this.AddReference(assemblyName, reference);
        }

        private void SetPrimaryFileItem(ITaskItem referenceAssemblyFile)
        {
            try
            {
                Reference reference = new Reference();
                bool wantSpecificVersionValue = MetadataConversionUtilities.TryConvertItemMetadataToBool(referenceAssemblyFile, "SpecificVersion");
                reference.MakePrimaryAssemblyReference(referenceAssemblyFile, wantSpecificVersionValue, Path.GetExtension(referenceAssemblyFile.ItemSpec));
                AssemblyNameExtension assemblyName = this.NameAssemblyFileReference(reference, referenceAssemblyFile.ItemSpec);
                if (this.projectTargetFramework >= TargetFrameworkVersion_40)
                {
                    reference.EmbedInteropTypes = MetadataConversionUtilities.TryConvertItemMetadataToBool(referenceAssemblyFile, "EmbedInteropTypes");
                }
                this.AddReference(assemblyName, reference);
            }
            catch (Exception exception)
            {
                if (Microsoft.Build.Shared.ExceptionHandling.NotExpectedException(exception))
                {
                    throw;
                }
                throw new InvalidParameterValueException("AssemblyFiles", referenceAssemblyFile.ItemSpec, exception.Message);
            }
        }

        private void SetPrimaryItems(ITaskItem[] referenceAssemblyFiles, ITaskItem[] referenceAssemblyNames, ArrayList exceptions)
        {
            if (referenceAssemblyFiles != null)
            {
                for (int i = 0; i < referenceAssemblyFiles.Length; i++)
                {
                    this.SetPrimaryFileItem(referenceAssemblyFiles[i]);
                }
            }
            if (referenceAssemblyNames != null)
            {
                for (int j = 0; j < referenceAssemblyNames.Length; j++)
                {
                    try
                    {
                        this.SetPrimaryAssemblyReferenceItem(referenceAssemblyNames[j]);
                    }
                    catch (InvalidReferenceAssemblyNameException exception)
                    {
                        exceptions.Add(exception);
                    }
                }
            }
        }

        private static void TryConvertToAssemblyName(string itemSpec, string fusionName, ref AssemblyNameExtension assemblyName)
        {
            string str = fusionName;
            if ((str == null) || (str.Length == 0))
            {
                str = itemSpec;
            }
            try
            {
                assemblyName = new AssemblyNameExtension(str, true);
            }
            catch (FileLoadException)
            {
                TryGatherAssemblyNameEssentials(str, ref assemblyName);
            }
        }

        private static void TryGatherAssemblyNameEssentials(string fusionName, ref AssemblyNameExtension assemblyName)
        {
            int index = fusionName.IndexOf(',');
            if (index != -1)
            {
                string str = fusionName.Substring(0, index);
                string str2 = null;
                string str3 = null;
                string str4 = null;
                TryGetAssemblyNameComponent(fusionName, "Version", ref str2);
                TryGetAssemblyNameComponent(fusionName, "PublicKeyToken", ref str3);
                TryGetAssemblyNameComponent(fusionName, "Culture", ref str4);
                if (((str2 != null) && (str3 != null)) && (str4 != null))
                {
                    string str5 = string.Format(CultureInfo.InvariantCulture, "{0}, Version={1}, Culture={2}, PublicKeyToken={3}", new object[] { str, str2, str4, str3 });
                    try
                    {
                        assemblyName = new AssemblyNameExtension(str5, true);
                    }
                    catch (FileLoadException)
                    {
                    }
                }
            }
        }

        private static void TryGetAssemblyNameComponent(string fusionName, string component, ref string value)
        {
            int index = fusionName.IndexOf(component + "=", StringComparison.Ordinal);
            if (index != -1)
            {
                index += component.Length + 1;
                int num2 = fusionName.IndexOfAny(new char[] { ',', ' ' }, index);
                if (num2 == -1)
                {
                    value = fusionName.Substring(index);
                }
                else
                {
                    value = fusionName.Substring(index, num2 - index);
                }
            }
        }

        private bool UnifyAssemblyNameVersions(AssemblyNameExtension assemblyName, out Version unifiedVersion, out UnificationReason unificationReason, out bool isPrerequisite, out bool? isRedistRoot, out string redistName)
        {
            unifiedVersion = assemblyName.Version;
            isPrerequisite = false;
            isRedistRoot = 0;
            redistName = null;
            unificationReason = UnificationReason.DidntUnify;
            if (assemblyName.Version != null)
            {
                if (this.remappedAssemblies != null)
                {
                    foreach (DependentAssembly assembly in this.remappedAssemblies)
                    {
                        AssemblyNameExtension that = new AssemblyNameExtension((AssemblyName) assembly.PartialAssemblyName.Clone());
                        if (assemblyName.CompareBaseNameTo(that) == 0)
                        {
                            that.ReplaceVersion(assemblyName.Version);
                            if (assemblyName.Equals(that))
                            {
                                foreach (BindingRedirect redirect in assembly.BindingRedirects)
                                {
                                    if (((assemblyName.Version >= redirect.OldVersionLow) && (assemblyName.Version <= redirect.OldVersionHigh)) && (assemblyName.Version != redirect.NewVersion))
                                    {
                                        unifiedVersion = redirect.NewVersion;
                                        unificationReason = UnificationReason.BecauseOfBindingRedirect;
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
                if (this.installedAssemblies != null)
                {
                    this.installedAssemblies.GetInfo(assemblyName, out unifiedVersion, out isPrerequisite, out isRedistRoot, out redistName);
                    if (unifiedVersion != assemblyName.Version)
                    {
                        unificationReason = UnificationReason.FrameworkRetarget;
                        return (assemblyName.Version != unifiedVersion);
                    }
                }
            }
            return false;
        }

        internal List<string> ListOfExcludedAssemblies
        {
            get
            {
                return this.listOfExcludedAssemblies;
            }
        }

        internal Dictionary<AssemblyNameExtension, Reference> References
        {
            get
            {
                return this.references;
            }
        }


        internal delegate void LogExclusionReason(bool displayPrimaryReferenceMessage, AssemblyNameExtension assemblyName, Reference reference, ITaskItem referenceItem, string targetedFramework);

        [StructLayout(LayoutKind.Sequential)]
        internal struct ReferenceAssemblyExtensionPair
        {
            private Reference assemblyKey;
            private AssemblyNameExtension assemblyValue;
            internal ReferenceAssemblyExtensionPair(Reference key, AssemblyNameExtension value)
            {
                this.assemblyKey = key;
                this.assemblyValue = value;
            }

            internal Reference Key
            {
                get
                {
                    return this.assemblyKey;
                }
            }
            internal AssemblyNameExtension Value
            {
                get
                {
                    return this.assemblyValue;
                }
            }
        }
    }
}

