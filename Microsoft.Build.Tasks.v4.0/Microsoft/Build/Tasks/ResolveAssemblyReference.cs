namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Shared;
    using Microsoft.Build.Utilities;
    using Microsoft.Internal.Performance;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;

    public class ResolveAssemblyReference : TaskExtension
    {
        private string[] allowedAssemblyExtensions = new string[] { ".dll", ".exe" };
        private string appConfigFile;
        private ITaskItem[] assemblyFiles = new TaskItem[0];
        private ITaskItem[] assemblyNames = new TaskItem[0];
        private bool autoUnify;
        private SystemState cache;
        private string[] candidateAssemblyFiles = new string[0];
        private bool copyLocalDependenciesWhenParentReferenceInGac = true;
        private ITaskItem[] copyLocalFiles = new TaskItem[0];
        private ArrayList filesWritten = new ArrayList();
        private bool findDependencies = true;
        private bool findRelatedFiles = true;
        private bool findSatellites = true;
        private bool findSerializationAssemblies = true;
        private ITaskItem[] fullFrameworkAssemblyTables = new TaskItem[0];
        private string[] fullFrameworkFolders = new string[0];
        private string[] fullTargetFrameworkSubsetNames = new string[0];
        private bool ignoreDefaultInstalledAssemblySubsetTables;
        private bool ignoreDefaultInstalledAssemblyTables;
        private ITaskItem[] installedAssemblySubsetTables = new TaskItem[0];
        private ITaskItem[] installedAssemblyTables = new TaskItem[0];
        private string[] latestTargetFrameworkDirectories = new string[0];
        private string profileName = string.Empty;
        private Version projectTargetFramework;
        private string projectTargetFrameworkAsString = string.Empty;
        private string[] relatedFileExtensions = new string[] { ".pdb", ".xml" };
        private ITaskItem[] relatedFiles = new TaskItem[0];
        private ITaskItem[] resolvedDependencyFiles = new TaskItem[0];
        private ITaskItem[] resolvedFiles = new TaskItem[0];
        private ITaskItem[] satelliteFiles = new TaskItem[0];
        private ITaskItem[] scatterFiles = new TaskItem[0];
        private string[] searchPaths = new string[0];
        private ITaskItem[] serializationAssemblyFiles = new TaskItem[0];
        private bool silent;
        private string stateFile;
        private ITaskItem[] suggestedRedirects = new TaskItem[0];
        private string targetedFrameworkMoniker = string.Empty;
        private string targetedRuntimeVersionRawValue = string.Empty;
        private string[] targetFrameworkDirectories = new string[0];
        private string[] targetFrameworkSubsets = new string[0];
        private string targetProcessorArchitecture;

        private bool CheckForAssemblyInGac(AssemblyNameExtension assemblyName, System.Reflection.ProcessorArchitecture targetProcessorArchitecture, GetAssemblyRuntimeVersion getRuntimeVersion, Version targetedRuntimeVersion, Microsoft.Build.Shared.FileExists fileExists)
        {
            string str = null;
            if (assemblyName.Version != null)
            {
                str = GlobalAssemblyCache.GetLocation(assemblyName, targetProcessorArchitecture, getRuntimeVersion, targetedRuntimeVersion, true, fileExists, null, null, false);
            }
            return (str != null);
        }

        private MessageImportance ChooseReferenceLoggingImportance(Reference reference)
        {
            MessageImportance low = MessageImportance.Low;
            if ((reference.GetErrors().Count <= 0) || (!reference.IsPrimary && !reference.IsCopyLocal))
            {
                return low;
            }
            return MessageImportance.Normal;
        }

        private void DumpTargetProfileLists(AssemblyTableInfo[] installedAssemblyTableInfo, AssemblyTableInfo[] whiteListSubsetTableInfo, ReferenceTable referenceTable)
        {
            if ((installedAssemblyTableInfo != null) && (Environment.GetEnvironmentVariable("MSBUILDDUMPFRAMEWORKSUBSETLIST") != null))
            {
                base.Log.LogMessageFromResources(MessageImportance.Low, "ResolveAssemblyReference.TargetFrameworkSubsetLogHeader", new object[0]);
                base.Log.LogMessageFromResources(MessageImportance.Low, "ResolveAssemblyReference.TargetFrameworkRedistLogHeader", new object[0]);
                if (installedAssemblyTableInfo != null)
                {
                    foreach (AssemblyTableInfo info in installedAssemblyTableInfo)
                    {
                        if (info != null)
                        {
                            base.Log.LogMessageFromResources(MessageImportance.Low, "ResolveAssemblyReference.FourSpaceIndent", new object[] { base.Log.FormatResourceString("ResolveAssemblyReference.FormattedAssemblyInfo", new object[] { info.Path }) });
                        }
                    }
                }
                base.Log.LogMessageFromResources(MessageImportance.Low, "ResolveAssemblyReference.TargetFrameworkWhiteListLogHeader", new object[0]);
                if (whiteListSubsetTableInfo != null)
                {
                    foreach (AssemblyTableInfo info2 in whiteListSubsetTableInfo)
                    {
                        if (info2 != null)
                        {
                            base.Log.LogMessageFromResources(MessageImportance.Low, "ResolveAssemblyReference.FourSpaceIndent", new object[] { base.Log.FormatResourceString("ResolveAssemblyReference.FormattedAssemblyInfo", new object[] { info2.Path }) });
                        }
                    }
                }
                if (referenceTable.ListOfExcludedAssemblies != null)
                {
                    base.Log.LogMessageFromResources(MessageImportance.Low, "ResolveAssemblyReference.TargetFrameworkExclusionListLogHeader", new object[0]);
                    foreach (string str2 in referenceTable.ListOfExcludedAssemblies)
                    {
                        base.Log.LogMessageFromResources(MessageImportance.Low, "ResolveAssemblyReference.FourSpaceIndent", new object[] { str2 });
                    }
                }
            }
        }

        public override bool Execute()
        {
            return this.Execute(new Microsoft.Build.Shared.FileExists(Microsoft.Build.Shared.FileUtilities.FileExistsNoThrow), new Microsoft.Build.Shared.DirectoryExists(Microsoft.Build.Shared.FileUtilities.DirectoryExistsNoThrow), new Microsoft.Build.Tasks.GetDirectories(Directory.GetDirectories), new GetAssemblyName(AssemblyNameExtension.GetAssemblyNameEx), new GetAssemblyMetadata(AssemblyInformation.GetAssemblyMetadata), new GetRegistrySubKeyNames(RegistryHelper.GetSubKeyNames), new GetRegistrySubKeyDefaultValue(RegistryHelper.GetDefaultValue), new GetLastWriteTime(Microsoft.Build.Shared.NativeMethodsShared.GetLastWriteFileUtcTime), new GetAssemblyRuntimeVersion(AssemblyInformation.GetRuntimeVersion), new OpenBaseKey(RegistryHelper.OpenBaseKey), new CheckIfAssemblyInGac(this.CheckForAssemblyInGac));
        }

        internal bool Execute(Microsoft.Build.Shared.FileExists fileExists, Microsoft.Build.Shared.DirectoryExists directoryExists, Microsoft.Build.Tasks.GetDirectories getDirectories, GetAssemblyName getAssemblyName, GetAssemblyMetadata getAssemblyMetadata, GetRegistrySubKeyNames getRegistrySubKeyNames, GetRegistrySubKeyDefaultValue getRegistrySubKeyDefaultValue, GetLastWriteTime getLastWriteTime, GetAssemblyRuntimeVersion getRuntimeVersion, OpenBaseKey openBaseKey, CheckIfAssemblyInGac checkIfAssemblyIsInGac)
        {
            bool flag = true;
            CodeMarkerStartEnd end = new CodeMarkerStartEnd(CodeMarkerEvent.perfMSBuildResolveAssemblyReferenceBegin, CodeMarkerEvent.perfMSBuildResolveAssemblyReferenceEnd);
            try
            {
                FrameworkName targetFrameworkMoniker = null;
                if (!string.IsNullOrEmpty(this.targetedFrameworkMoniker))
                {
                    targetFrameworkMoniker = new FrameworkName(this.targetedFrameworkMoniker);
                }
                Version targetedRuntimeVersion = this.SetTargetedRuntimeVersion(getRuntimeVersion);
                this.LogInputs();
                if (!this.VerifyInputConditions())
                {
                    return false;
                }
                if (this.targetFrameworkDirectories != null)
                {
                    for (int i = 0; i < this.targetFrameworkDirectories.Length; i++)
                    {
                        this.targetFrameworkDirectories[i] = Microsoft.Build.Shared.FileUtilities.EnsureTrailingSlash(this.targetFrameworkDirectories[i]);
                    }
                }
                AssemblyTableInfo[] assemblyTables = this.GetInstalledAssemblyTableInfo(this.ignoreDefaultInstalledAssemblyTables, this.installedAssemblyTables, new GetListPath(RedistList.GetRedistListPathsFromDisk), this.TargetFrameworkDirectories);
                AssemblyTableInfo[] whiteListAssemblyTableInfo = null;
                InstalledAssemblies installedAssemblies = null;
                RedistList redistList = null;
                if ((assemblyTables != null) && (assemblyTables.Length > 0))
                {
                    redistList = RedistList.GetRedistList(assemblyTables);
                }
                Hashtable blackList = null;
                string subsetName = null;
                bool flag2 = !string.IsNullOrEmpty(this.ProfileName) && ((this.FullFrameworkFolders.Length > 0) || (this.FullFrameworkAssemblyTables.Length > 0));
                bool flag3 = false;
                if (((redistList != null) && (redistList.Count > 0)) || (flag2 || this.ShouldUseSubsetBlackList()))
                {
                    if (!flag2 && this.ShouldUseSubsetBlackList())
                    {
                        SubsetListFinder finder = new SubsetListFinder(this.targetFrameworkSubsets);
                        whiteListAssemblyTableInfo = this.GetInstalledAssemblyTableInfo(this.IgnoreDefaultInstalledAssemblySubsetTables, this.InstalledAssemblySubsetTables, new GetListPath(finder.GetSubsetListPathsFromDisk), this.TargetFrameworkDirectories);
                        if (((whiteListAssemblyTableInfo.Length > 0) && (redistList != null)) && (redistList.Count > 0))
                        {
                            blackList = redistList.GenerateBlackList(whiteListAssemblyTableInfo);
                        }
                        else
                        {
                            base.Log.LogWarningWithCodeFromResources("ResolveAssemblyReference.NoSubsetsFound", new object[0]);
                        }
                        if (blackList == null)
                        {
                            base.Log.LogWarningWithCodeFromResources("ResolveAssemblyReference.NoRedistAssembliesToGenerateExclusionList", new object[0]);
                        }
                        subsetName = GenerateSubSetName(this.targetFrameworkSubsets, this.installedAssemblySubsetTables);
                        flag3 = true;
                    }
                    else if (flag2)
                    {
                        AssemblyTableInfo[] fullRedistAssemblyTableInfo = null;
                        RedistList fullFrameworkRedistList = null;
                        this.HandleProfile(assemblyTables, out fullRedistAssemblyTableInfo, out blackList, out fullFrameworkRedistList);
                        redistList = fullFrameworkRedistList;
                        whiteListAssemblyTableInfo = assemblyTables;
                        assemblyTables = fullRedistAssemblyTableInfo;
                        subsetName = this.profileName;
                    }
                    if ((redistList != null) && (redistList.Count > 0))
                    {
                        installedAssemblies = new InstalledAssemblies(redistList);
                    }
                }
                if (redistList != null)
                {
                    for (int j = 0; j < redistList.Errors.Length; j++)
                    {
                        Exception exception = redistList.Errors[j];
                        string str2 = redistList.ErrorFileNames[j];
                        base.Log.LogWarningWithCodeFromResources("ResolveAssemblyReference.InvalidInstalledAssemblyTablesFile", new object[] { str2, RedistList.RedistListFolder, exception.Message });
                    }
                    for (int k = 0; k < redistList.WhiteListErrors.Length; k++)
                    {
                        Exception exception2 = redistList.WhiteListErrors[k];
                        string str3 = redistList.WhiteListErrorFileNames[k];
                        base.Log.LogWarningWithCodeFromResources("ResolveAssemblyReference.InvalidInstalledAssemblySubsetTablesFile", new object[] { str3, SubsetListFinder.SubsetListFolder, exception2.Message });
                    }
                }
                this.ReadStateFile();
                this.cache.SetGetLastWriteTime(getLastWriteTime);
                this.cache.SetInstalledAssemblyInformation(assemblyTables);
                getAssemblyName = this.cache.CacheDelegate(getAssemblyName);
                getAssemblyMetadata = this.cache.CacheDelegate(getAssemblyMetadata);
                fileExists = this.cache.CacheDelegate(fileExists);
                getDirectories = this.cache.CacheDelegate(getDirectories);
                getRuntimeVersion = this.cache.CacheDelegate(getRuntimeVersion);
                this.projectTargetFramework = this.FrameworkVersionFromString(this.projectTargetFrameworkAsString);
                this.FilterBySubtypeAndTargetFramework();
                DependentAssembly[] idealRemappings = null;
                if (this.FindDependencies)
                {
                    try
                    {
                        idealRemappings = this.GetAssemblyRemappingsFromAppConfig();
                    }
                    catch (AppConfigException exception3)
                    {
                        base.Log.LogErrorWithCodeFromResources(null, exception3.FileName, exception3.Line, exception3.Column, 0, 0, "ResolveAssemblyReference.InvalidAppConfig", new object[] { this.AppConfigFile, exception3.Message });
                        return false;
                    }
                }
                System.Reflection.ProcessorArchitecture targetProcessorArchitecture = TargetProcessorArchitectureToEnumeration(this.targetProcessorArchitecture);
                if (checkIfAssemblyIsInGac == null)
                {
                    checkIfAssemblyIsInGac = new CheckIfAssemblyInGac(this.CheckForAssemblyInGac);
                }
                ReferenceTable dependencyTable = new ReferenceTable(this.findDependencies, this.findSatellites, this.findSerializationAssemblies, this.findRelatedFiles, this.searchPaths, this.allowedAssemblyExtensions, this.relatedFileExtensions, this.candidateAssemblyFiles, this.targetFrameworkDirectories, installedAssemblies, targetProcessorArchitecture, fileExists, directoryExists, getDirectories, getAssemblyName, getAssemblyMetadata, getRegistrySubKeyNames, getRegistrySubKeyDefaultValue, openBaseKey, getRuntimeVersion, targetedRuntimeVersion, this.projectTargetFramework, targetFrameworkMoniker, base.Log, this.latestTargetFrameworkDirectories, this.copyLocalDependenciesWhenParentReferenceInGac, checkIfAssemblyIsInGac);
                ArrayList exceptions = new ArrayList();
                subsetName = flag3 ? subsetName : this.targetedFrameworkMoniker;
                bool flag4 = false;
                if (this.AutoUnify && this.FindDependencies)
                {
                    dependencyTable.ComputeClosure(null, this.assemblyFiles, this.assemblyNames, exceptions);
                    try
                    {
                        flag4 = false;
                        if ((redistList != null) && (redistList.Count > 0))
                        {
                            flag4 = dependencyTable.MarkReferencesForExclusion(blackList);
                        }
                    }
                    catch (InvalidOperationException exception4)
                    {
                        base.Log.LogErrorWithCodeFromResources("ResolveAssemblyReference.ProblemDeterminingFrameworkMembership", new object[] { exception4.Message });
                        return false;
                    }
                    if (flag4)
                    {
                        dependencyTable.RemoveReferencesMarkedForExclusion(true, subsetName);
                    }
                    AssemblyNameReference[] referenceArray = null;
                    dependencyTable.ResolveConflicts(out idealRemappings, out referenceArray);
                }
                dependencyTable.ComputeClosure(idealRemappings, this.assemblyFiles, this.assemblyNames, exceptions);
                try
                {
                    flag4 = false;
                    if ((redistList != null) && (redistList.Count > 0))
                    {
                        flag4 = dependencyTable.MarkReferencesForExclusion(blackList);
                    }
                }
                catch (InvalidOperationException exception5)
                {
                    base.Log.LogErrorWithCodeFromResources("ResolveAssemblyReference.ProblemDeterminingFrameworkMembership", new object[] { exception5.Message });
                    return false;
                }
                if (flag4)
                {
                    dependencyTable.RemoveReferencesMarkedForExclusion(false, subsetName);
                }
                DependentAssembly[] assemblyArray2 = null;
                AssemblyNameReference[] conflictingReferences = null;
                dependencyTable.ResolveConflicts(out assemblyArray2, out conflictingReferences);
                dependencyTable.GetReferenceItems(out this.resolvedFiles, out this.resolvedDependencyFiles, out this.relatedFiles, out this.satelliteFiles, out this.serializationAssemblyFiles, out this.scatterFiles, out this.copyLocalFiles);
                if (this.FindDependencies)
                {
                    this.PopulateSuggestedRedirects(assemblyArray2);
                }
                if (this.stateFile != null)
                {
                    this.filesWritten.Add(new TaskItem(this.stateFile));
                }
                this.WriteStateFile();
                flag = this.LogResults(dependencyTable, assemblyArray2, conflictingReferences, exceptions);
                this.DumpTargetProfileLists(assemblyTables, whiteListAssemblyTableInfo, dependencyTable);
                return (flag && !base.Log.HasLoggedErrors);
            }
            catch (ArgumentException exception6)
            {
                base.Log.LogErrorWithCodeFromResources("General.InvalidArgument", new object[] { exception6.Message });
            }
            catch (InvalidParameterValueException exception7)
            {
                base.Log.LogErrorWithCodeFromResources(null, "", 0, 0, 0, 0, "ResolveAssemblyReference.InvalidParameter", new object[] { exception7.ParamName, exception7.ActualValue, exception7.Message });
            }
            finally
            {
                if (end != null)
                {
                    end.Dispose();
                }
            }
            return (flag && !base.Log.HasLoggedErrors);
        }

        private void FilterBySubtypeAndTargetFramework()
        {
            ArrayList list = new ArrayList();
            foreach (ITaskItem item in this.Assemblies)
            {
                string metadata = item.GetMetadata("SubType");
                if ((metadata != null) && (metadata.Length > 0))
                {
                    base.Log.LogMessageFromResources(MessageImportance.Normal, "ResolveAssemblyReference.IgnoringBecauseNonEmptySubtype", new object[] { item.ItemSpec, metadata });
                }
                else if (!this.IsAvailableForTargetFramework(item.GetMetadata("RequiredTargetFramework")))
                {
                    base.Log.LogWarningWithCodeFromResources("ResolveAssemblyReference.FailedToResolveReferenceBecauseHigherTargetFramework", new object[] { item.ItemSpec, item.GetMetadata("RequiredTargetFramework") });
                }
                else
                {
                    list.Add(item);
                }
            }
            this.assemblyNames = (ITaskItem[]) list.ToArray(typeof(ITaskItem));
        }

        private Version FrameworkVersionFromString(string version)
        {
            Version version2 = null;
            if (!string.IsNullOrEmpty(version))
            {
                version2 = Microsoft.Build.Tasks.VersionUtilities.ConvertToVersion(version);
                if (version2 == null)
                {
                    base.Log.LogMessageFromResources(MessageImportance.Normal, "ResolveAssemblyReference.BadTargetFrameworkFormat", new object[] { version });
                }
            }
            return version2;
        }

        internal static string GenerateSubSetName(string[] frameworkSubSetNames, ITaskItem[] installedSubSetNames)
        {
            List<string> list = new List<string>();
            if (frameworkSubSetNames != null)
            {
                foreach (string str in frameworkSubSetNames)
                {
                    if (!string.IsNullOrEmpty(str))
                    {
                        list.Add(str);
                    }
                }
            }
            if (installedSubSetNames != null)
            {
                foreach (ITaskItem item in installedSubSetNames)
                {
                    string itemSpec = item.ItemSpec;
                    if (!string.IsNullOrEmpty(itemSpec))
                    {
                        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(itemSpec);
                        if (!string.IsNullOrEmpty(fileNameWithoutExtension))
                        {
                            list.Add(fileNameWithoutExtension);
                        }
                    }
                }
            }
            return string.Join(", ", list.ToArray());
        }

        private DependentAssembly[] GetAssemblyRemappingsFromAppConfig()
        {
            if (this.appConfigFile != null)
            {
                AppConfig config = new AppConfig();
                config.Load(this.appConfigFile);
                return config.Runtime.DependentAssemblies;
            }
            return null;
        }

        private AssemblyTableInfo[] GetInstalledAssemblyTableInfo(bool ignoreInstalledAssemblyTables, ITaskItem[] assemblyTables, GetListPath GetAssemblyListPaths, string[] targetFrameworkDirectories)
        {
            Dictionary<string, AssemblyTableInfo> dictionary = new Dictionary<string, AssemblyTableInfo>(StringComparer.OrdinalIgnoreCase);
            if (!ignoreInstalledAssemblyTables)
            {
                foreach (string str in targetFrameworkDirectories)
                {
                    foreach (string str2 in GetAssemblyListPaths(str))
                    {
                        dictionary[str2] = new AssemblyTableInfo(str2, str);
                    }
                }
            }
            foreach (ITaskItem item in assemblyTables)
            {
                string metadata = item.GetMetadata("FrameworkDirectory");
                if (string.IsNullOrEmpty(metadata))
                {
                    if ((this.TargetFrameworkDirectories != null) && (this.TargetFrameworkDirectories.Length == 1))
                    {
                        metadata = this.TargetFrameworkDirectories[0];
                    }
                }
                else
                {
                    metadata = Microsoft.Build.Shared.FileUtilities.EnsureTrailingSlash(metadata);
                }
                dictionary[item.ItemSpec] = new AssemblyTableInfo(item.ItemSpec, metadata);
            }
            AssemblyTableInfo[] array = new AssemblyTableInfo[dictionary.Count];
            dictionary.Values.CopyTo(array, 0);
            return array;
        }

        private void HandleProfile(AssemblyTableInfo[] installedAssemblyTableInfo, out AssemblyTableInfo[] fullRedistAssemblyTableInfo, out Hashtable blackList, out RedistList fullFrameworkRedistList)
        {
            fullFrameworkRedistList = null;
            blackList = null;
            fullRedistAssemblyTableInfo = null;
            foreach (ITaskItem item in this.FullFrameworkAssemblyTables)
            {
                if (string.IsNullOrEmpty(item.GetMetadata("FrameworkDirectory")))
                {
                    base.Log.LogErrorWithCodeFromResources("ResolveAssemblyReference.FrameworkDirectoryOnProfiles", new object[] { item.ItemSpec });
                    return;
                }
            }
            fullRedistAssemblyTableInfo = this.GetInstalledAssemblyTableInfo(false, this.FullFrameworkAssemblyTables, new GetListPath(RedistList.GetRedistListPathsFromDisk), this.FullFrameworkFolders);
            if (fullRedistAssemblyTableInfo.Length > 0)
            {
                fullFrameworkRedistList = RedistList.GetRedistList(fullRedistAssemblyTableInfo);
                if (fullFrameworkRedistList != null)
                {
                    base.Log.LogMessageFromResources("ResolveAssemblyReference.ProfileExclusionListWillBeGenerated", new object[0]);
                    blackList = fullFrameworkRedistList.GenerateBlackList(installedAssemblyTableInfo);
                }
                if (blackList == null)
                {
                    base.Log.LogWarningWithCodeFromResources("ResolveAssemblyReference.NoRedistAssembliesToGenerateExclusionList", new object[0]);
                }
            }
            else
            {
                base.Log.LogWarningWithCodeFromResources("ResolveAssemblyReference.NoProfilesFound", new object[0]);
            }
            if (fullFrameworkRedistList != null)
            {
                for (int i = 0; i < fullFrameworkRedistList.Errors.Length; i++)
                {
                    Exception exception = fullFrameworkRedistList.Errors[i];
                    string str = fullFrameworkRedistList.ErrorFileNames[i];
                    base.Log.LogWarningWithCodeFromResources("ResolveAssemblyReference.InvalidProfileRedistLocation", new object[] { str, RedistList.RedistListFolder, exception.Message });
                }
            }
        }

        private bool IsAvailableForTargetFramework(string assemblyFXVersionAsString)
        {
            Version version = this.FrameworkVersionFromString(assemblyFXVersionAsString);
            if ((version != null) && (this.projectTargetFramework != null))
            {
                return (this.projectTargetFramework >= version);
            }
            return true;
        }

        private void LogAssembliesConsideredAndRejected(Reference reference, MessageImportance importance)
        {
            if (reference.AssembliesConsideredAndRejected != null)
            {
                ArrayList assembliesConsideredAndRejected = reference.AssembliesConsideredAndRejected;
                string searchPath = null;
                foreach (ResolutionSearchLocation location in reference.AssembliesConsideredAndRejected)
                {
                    if (searchPath != location.SearchPath)
                    {
                        searchPath = location.SearchPath;
                        base.Log.LogMessageFromResources(importance, "ResolveAssemblyReference.EightSpaceIndent", new object[] { base.Log.FormatResourceString("ResolveAssemblyReference.SearchPath", new object[] { searchPath }) });
                    }
                    switch (location.Reason)
                    {
                        case NoMatchReason.FileNotFound:
                            base.Log.LogMessageFromResources(importance, "ResolveAssemblyReference.EightSpaceIndent", new object[] { base.Log.FormatResourceString("ResolveAssemblyReference.ConsideredAndRejectedBecauseNoFile", new object[] { location.FileNameAttempted }) });
                            break;

                        case NoMatchReason.FusionNamesDidNotMatch:
                            base.Log.LogMessageFromResources(importance, "ResolveAssemblyReference.EightSpaceIndent", new object[] { base.Log.FormatResourceString("ResolveAssemblyReference.ConsideredAndRejectedBecauseFusionNamesDidntMatch", new object[] { location.FileNameAttempted, location.AssemblyName.FullName }) });
                            break;

                        case NoMatchReason.TargetHadNoFusionName:
                            base.Log.LogMessageFromResources(importance, "ResolveAssemblyReference.EightSpaceIndent", new object[] { base.Log.FormatResourceString("ResolveAssemblyReference.ConsideredAndRejectedBecauseTargetDidntHaveFusionName", new object[] { location.FileNameAttempted }) });
                            break;

                        case NoMatchReason.NotInGac:
                            base.Log.LogMessageFromResources(importance, "ResolveAssemblyReference.EightSpaceIndent", new object[] { base.Log.FormatResourceString("ResolveAssemblyReference.ConsideredAndRejectedBecauseNotInGac", new object[] { location.FileNameAttempted }) });
                            break;

                        case NoMatchReason.NotAFileNameOnDisk:
                            base.Log.LogMessageFromResources(importance, "ResolveAssemblyReference.EightSpaceIndent", new object[] { base.Log.FormatResourceString("ResolveAssemblyReference.ConsideredAndRejectedBecauseNotAFileNameOnDisk", new object[] { location.FileNameAttempted }) });
                            break;

                        case NoMatchReason.ProcessorArchitectureDoesNotMatch:
                            base.Log.LogMessageFromResources(importance, "ResolveAssemblyReference.EightSpaceIndent", new object[] { base.Log.FormatResourceString("ResolveAssemblyReference.TargetedProcessorArchitectureDoesNotMatch", new object[] { location.FileNameAttempted, location.AssemblyName.AssemblyName.ProcessorArchitecture.ToString(), this.targetProcessorArchitecture }) });
                            break;
                    }
                }
            }
        }

        private void LogAttribute(ITaskItem item, string metadataName)
        {
            string metadata = item.GetMetadata(metadataName);
            if ((metadata != null) && (metadata.Length > 0))
            {
                base.Log.LogMessageFromResources(MessageImportance.Low, "ResolveAssemblyReference.EightSpaceIndent", new object[] { base.Log.FormatResourceString("ResolveAssemblyReference.LogAttributeFormat", new object[] { metadataName, metadata }) });
            }
        }

        private void LogConflict(Reference reference, string fusionName)
        {
            MessageImportance importance = this.ChooseReferenceLoggingImportance(reference);
            base.Log.LogMessageFromResources(importance, "ResolveAssemblyReference.ConflictFound", new object[] { reference.ConflictVictorName, fusionName });
            switch (reference.ConflictLossExplanation)
            {
                case ConflictLossReason.HadLowerVersion:
                {
                    string str = base.Log.FormatResourceString("ResolveAssemblyReference.ConflictHigherVersionChosen", new object[] { reference.ConflictVictorName });
                    base.Log.LogMessageFromResources(importance, "ResolveAssemblyReference.FourSpaceIndent", new object[] { str });
                    return;
                }
                case ConflictLossReason.InsolubleConflict:
                    if (!reference.IsPrimary)
                    {
                        string str3;
                        base.Log.ExtractMessageCode(base.Log.FormatResourceString("ResolveAssemblyReference.ConflictUnsolvable", new object[] { reference.ConflictVictorName, fusionName }), out str3);
                        base.Log.LogMessage(MessageImportance.High, str3, new object[0]);
                        return;
                    }
                    base.Log.LogWarningWithCodeFromResources("ResolveAssemblyReference.ConflictUnsolvable", new object[] { reference.ConflictVictorName, fusionName });
                    return;

                case ConflictLossReason.WasNotPrimary:
                {
                    string str2 = base.Log.FormatResourceString("ResolveAssemblyReference.ConflictPrimaryChosen", new object[] { reference.ConflictVictorName, fusionName });
                    base.Log.LogMessageFromResources(importance, "ResolveAssemblyReference.FourSpaceIndent", new object[] { str2 });
                    return;
                }
            }
        }

        private void LogCopyLocalState(Reference reference, MessageImportance importance)
        {
            if (!reference.IsUnresolvable && !reference.IsBadImage)
            {
                switch (reference.CopyLocal)
                {
                    case CopyLocalState.YesBecauseOfHeuristic:
                    case CopyLocalState.YesBecauseReferenceItemHadMetadata:
                    case CopyLocalState.NoBecauseUnresolved:
                        return;

                    case CopyLocalState.NoBecauseFrameworkFile:
                        base.Log.LogMessageFromResources(importance, "ResolveAssemblyReference.FourSpaceIndent", new object[] { base.Log.FormatResourceString("ResolveAssemblyReference.NotCopyLocalBecauseFrameworksFiles", new object[0]) });
                        return;

                    case CopyLocalState.NoBecausePrerequisite:
                        base.Log.LogMessageFromResources(importance, "ResolveAssemblyReference.FourSpaceIndent", new object[] { base.Log.FormatResourceString("ResolveAssemblyReference.NotCopyLocalBecausePrerequisite", new object[0]) });
                        return;

                    case CopyLocalState.NoBecauseReferenceItemHadMetadata:
                        base.Log.LogMessageFromResources(importance, "ResolveAssemblyReference.FourSpaceIndent", new object[] { base.Log.FormatResourceString("ResolveAssemblyReference.NotCopyLocalBecauseIncomingItemAttributeOverrode", new object[0]) });
                        return;

                    case CopyLocalState.NoBecauseReferenceFoundInGAC:
                        base.Log.LogMessageFromResources(importance, "ResolveAssemblyReference.FourSpaceIndent", new object[] { base.Log.FormatResourceString("ResolveAssemblyReference.NotCopyLocalBecauseReferenceFoundInGAC", new object[0]) });
                        return;

                    case CopyLocalState.NoBecauseConflictVictim:
                        base.Log.LogMessageFromResources(importance, "ResolveAssemblyReference.FourSpaceIndent", new object[] { base.Log.FormatResourceString("ResolveAssemblyReference.NotCopyLocalBecauseConflictVictim", new object[0]) });
                        return;

                    case CopyLocalState.NoBecauseEmbedded:
                        base.Log.LogMessageFromResources(importance, "ResolveAssemblyReference.FourSpaceIndent", new object[] { base.Log.FormatResourceString("ResolveAssemblyReference.NotCopyLocalBecauseEmbedded", new object[0]) });
                        return;

                    case CopyLocalState.NoBecauseParentReferencesFoundInGAC:
                        base.Log.LogMessageFromResources(importance, "ResolveAssemblyReference.FourSpaceIndent", new object[] { base.Log.FormatResourceString("ResolveAssemblyReference.NoBecauseParentReferencesFoundInGac", new object[0]) });
                        return;
                }
            }
        }

        private void LogDependeeReference(Reference dependeeReference)
        {
            base.Log.LogMessageFromResources(MessageImportance.Low, "ResolveAssemblyReference.EightSpaceIndent", new object[] { dependeeReference.FullPath });
            base.Log.LogMessageFromResources(MessageImportance.Low, "ResolveAssemblyReference.TenSpaceIndent", new object[] { Microsoft.Build.Shared.ResourceUtilities.FormatResourceString("ResolveAssemblyReference.PrimarySourceItemsForReference", new object[] { dependeeReference.FullPath }) });
            foreach (ITaskItem item in dependeeReference.GetSourceItems())
            {
                base.Log.LogMessageFromResources(MessageImportance.Low, "ResolveAssemblyReference.TwelveSpaceIndent", new object[] { item.ItemSpec });
            }
        }

        private void LogDependees(Reference reference, MessageImportance importance)
        {
            if (!reference.IsPrimary)
            {
                foreach (ITaskItem item in reference.GetSourceItems())
                {
                    base.Log.LogMessageFromResources(importance, "ResolveAssemblyReference.FourSpaceIndent", new object[] { base.Log.FormatResourceString("ResolveAssemblyReference.RequiredBy", new object[] { item.ItemSpec }) });
                }
            }
        }

        private void LogFullName(Reference reference, MessageImportance importance)
        {
            Microsoft.Build.Shared.ErrorUtilities.VerifyThrowArgumentNull(reference, "reference");
            if (reference.IsResolved)
            {
                base.Log.LogMessageFromResources(importance, "ResolveAssemblyReference.FourSpaceIndent", new object[] { base.Log.FormatResourceString("ResolveAssemblyReference.Resolved", new object[] { reference.FullPath }) });
                base.Log.LogMessageFromResources(importance, "ResolveAssemblyReference.FourSpaceIndent", new object[] { base.Log.FormatResourceString("ResolveAssemblyReference.ResolvedFrom", new object[] { reference.ResolvedSearchPath }) });
            }
        }

        private void LogInputs()
        {
            if (!this.Silent)
            {
                base.Log.LogMessageFromResources(MessageImportance.Low, "ResolveAssemblyReference.LogTaskPropertyFormat", new object[] { "TargetFrameworkMoniker" });
                base.Log.LogMessageFromResources(MessageImportance.Low, "ResolveAssemblyReference.FourSpaceIndent", new object[] { this.targetedFrameworkMoniker });
                base.Log.LogMessageFromResources(MessageImportance.Low, "ResolveAssemblyReference.LogTaskPropertyFormat", new object[] { "TargetFrameworkMonikerDisplayName" });
                base.Log.LogMessageFromResources(MessageImportance.Low, "ResolveAssemblyReference.FourSpaceIndent", new object[] { this.TargetFrameworkMonikerDisplayName });
                base.Log.LogMessageFromResources(MessageImportance.Low, "ResolveAssemblyReference.LogTaskPropertyFormat", new object[] { "TargetedRuntimeVersion" });
                base.Log.LogMessageFromResources(MessageImportance.Low, "ResolveAssemblyReference.FourSpaceIndent", new object[] { this.targetedRuntimeVersionRawValue });
                base.Log.LogMessageFromResources(MessageImportance.Low, "ResolveAssemblyReference.LogTaskPropertyFormat", new object[] { "Assemblies" });
                foreach (ITaskItem item in this.Assemblies)
                {
                    base.Log.LogMessageFromResources(MessageImportance.Low, "ResolveAssemblyReference.FourSpaceIndent", new object[] { item.ItemSpec });
                    this.LogAttribute(item, "Private");
                    this.LogAttribute(item, "HintPath");
                    this.LogAttribute(item, "SpecificVersion");
                    this.LogAttribute(item, "EmbedInteropTypes");
                    this.LogAttribute(item, "ExecutableExtension");
                    this.LogAttribute(item, "SubType");
                }
                base.Log.LogMessageFromResources(MessageImportance.Low, "ResolveAssemblyReference.LogTaskPropertyFormat", new object[] { "AssemblyFiles" });
                foreach (ITaskItem item2 in this.AssemblyFiles)
                {
                    base.Log.LogMessageFromResources(MessageImportance.Low, "ResolveAssemblyReference.FourSpaceIndent", new object[] { item2.ItemSpec });
                    this.LogAttribute(item2, "Private");
                    this.LogAttribute(item2, "FusionName");
                }
                base.Log.LogMessageFromResources(MessageImportance.Low, "ResolveAssemblyReference.LogTaskPropertyFormat", new object[] { "CandidateAssemblyFiles" });
                foreach (string str in this.CandidateAssemblyFiles)
                {
                    try
                    {
                        if (Microsoft.Build.Shared.FileUtilities.HasExtension(str, this.allowedAssemblyExtensions))
                        {
                            base.Log.LogMessageFromResources(MessageImportance.Low, "ResolveAssemblyReference.FourSpaceIndent", new object[] { str });
                        }
                    }
                    catch (Exception exception)
                    {
                        if (Microsoft.Build.Shared.ExceptionHandling.NotExpectedException(exception))
                        {
                            throw;
                        }
                        throw new InvalidParameterValueException("CandidateAssemblyFiles", str, exception.Message);
                    }
                }
                base.Log.LogMessageFromResources(MessageImportance.Low, "ResolveAssemblyReference.LogTaskPropertyFormat", new object[] { "TargetFrameworkDirectories" });
                base.Log.LogMessageFromResources(MessageImportance.Low, "ResolveAssemblyReference.FourSpaceIndent", new object[] { string.Join(",", this.TargetFrameworkDirectories) });
                base.Log.LogMessageFromResources(MessageImportance.Low, "ResolveAssemblyReference.LogTaskPropertyFormat", new object[] { "InstalledAssemblyTables" });
                foreach (ITaskItem item3 in this.InstalledAssemblyTables)
                {
                    base.Log.LogMessageFromResources(MessageImportance.Low, "ResolveAssemblyReference.FourSpaceIndent", new object[] { item3 });
                    this.LogAttribute(item3, "FrameworkDirectory");
                }
                base.Log.LogMessageFromResources(MessageImportance.Low, "ResolveAssemblyReference.LogTaskPropertyFormat", new object[] { "IgnoreInstalledAssemblyTable" });
                base.Log.LogMessageFromResources(MessageImportance.Low, "ResolveAssemblyReference.FourSpaceIndent", new object[] { this.ignoreDefaultInstalledAssemblyTables });
                base.Log.LogMessageFromResources(MessageImportance.Low, "ResolveAssemblyReference.LogTaskPropertyFormat", new object[] { "SearchPaths" });
                foreach (string str2 in this.SearchPaths)
                {
                    base.Log.LogMessageFromResources(MessageImportance.Low, "ResolveAssemblyReference.FourSpaceIndent", new object[] { str2 });
                }
                base.Log.LogMessageFromResources(MessageImportance.Low, "ResolveAssemblyReference.LogTaskPropertyFormat", new object[] { "AllowedAssemblyExtensions" });
                foreach (string str3 in this.allowedAssemblyExtensions)
                {
                    base.Log.LogMessageFromResources(MessageImportance.Low, "ResolveAssemblyReference.FourSpaceIndent", new object[] { str3 });
                }
                base.Log.LogMessageFromResources(MessageImportance.Low, "ResolveAssemblyReference.LogTaskPropertyFormat", new object[] { "AllowedRelatedFileExtensions" });
                foreach (string str4 in this.relatedFileExtensions)
                {
                    base.Log.LogMessageFromResources(MessageImportance.Low, "ResolveAssemblyReference.FourSpaceIndent", new object[] { str4 });
                }
                base.Log.LogMessageFromResources(MessageImportance.Low, "ResolveAssemblyReference.LogTaskPropertyFormat", new object[] { "AppConfigFile" });
                base.Log.LogMessageFromResources(MessageImportance.Low, "ResolveAssemblyReference.FourSpaceIndent", new object[] { this.AppConfigFile });
                base.Log.LogMessageFromResources(MessageImportance.Low, "ResolveAssemblyReference.LogTaskPropertyFormat", new object[] { "AutoUnify" });
                base.Log.LogMessageFromResources(MessageImportance.Low, "ResolveAssemblyReference.FourSpaceIndent", new object[] { this.AutoUnify.ToString() });
                base.Log.LogMessageFromResources(MessageImportance.Low, "ResolveAssemblyReference.LogTaskPropertyFormat", new object[] { "CopyLocalDependenciesWhenParentReferenceInGac" });
                base.Log.LogMessageFromResources(MessageImportance.Low, "ResolveAssemblyReference.FourSpaceIndent", new object[] { this.copyLocalDependenciesWhenParentReferenceInGac });
                base.Log.LogMessageFromResources(MessageImportance.Low, "ResolveAssemblyReference.LogTaskPropertyFormat", new object[] { "FindDependencies" });
                base.Log.LogMessageFromResources(MessageImportance.Low, "ResolveAssemblyReference.FourSpaceIndent", new object[] { this.findDependencies });
                base.Log.LogMessageFromResources(MessageImportance.Low, "ResolveAssemblyReference.LogTaskPropertyFormat", new object[] { "TargetProcessorArchitecture" });
                base.Log.LogMessageFromResources(MessageImportance.Low, "ResolveAssemblyReference.FourSpaceIndent", new object[] { this.TargetProcessorArchitecture });
                base.Log.LogMessageFromResources(MessageImportance.Low, "ResolveAssemblyReference.LogTaskPropertyFormat", new object[] { "StateFile" });
                base.Log.LogMessageFromResources(MessageImportance.Low, "ResolveAssemblyReference.FourSpaceIndent", new object[] { this.StateFile });
                base.Log.LogMessageFromResources(MessageImportance.Low, "ResolveAssemblyReference.LogTaskPropertyFormat", new object[] { "InstalledAssemblySubsetTables" });
                foreach (ITaskItem item4 in this.InstalledAssemblySubsetTables)
                {
                    base.Log.LogMessageFromResources(MessageImportance.Low, "ResolveAssemblyReference.FourSpaceIndent", new object[] { item4 });
                    this.LogAttribute(item4, "FrameworkDirectory");
                }
                base.Log.LogMessageFromResources(MessageImportance.Low, "ResolveAssemblyReference.LogTaskPropertyFormat", new object[] { "IgnoreInstalledAssemblySubsetTable" });
                base.Log.LogMessageFromResources(MessageImportance.Low, "ResolveAssemblyReference.FourSpaceIndent", new object[] { this.ignoreDefaultInstalledAssemblySubsetTables });
                base.Log.LogMessageFromResources(MessageImportance.Low, "ResolveAssemblyReference.LogTaskPropertyFormat", new object[] { "TargetFrameworkSubsets" });
                foreach (string str5 in this.targetFrameworkSubsets)
                {
                    base.Log.LogMessageFromResources(MessageImportance.Low, "ResolveAssemblyReference.FourSpaceIndent", new object[] { str5 });
                }
                base.Log.LogMessageFromResources(MessageImportance.Low, "ResolveAssemblyReference.LogTaskPropertyFormat", new object[] { "FullTargetFrameworkSubsetNames" });
                foreach (string str6 in this.FullTargetFrameworkSubsetNames)
                {
                    base.Log.LogMessageFromResources(MessageImportance.Low, "ResolveAssemblyReference.FourSpaceIndent", new object[] { str6 });
                }
                base.Log.LogMessageFromResources(MessageImportance.Low, "ResolveAssemblyReference.LogTaskPropertyFormat", new object[] { "ProfileName" });
                base.Log.LogMessageFromResources(MessageImportance.Low, "ResolveAssemblyReference.FourSpaceIndent", new object[] { this.ProfileName });
                base.Log.LogMessageFromResources(MessageImportance.Low, "ResolveAssemblyReference.LogTaskPropertyFormat", new object[] { "ProfileFullFrameworkFolders" });
                foreach (string str7 in this.FullFrameworkFolders)
                {
                    base.Log.LogMessageFromResources(MessageImportance.Low, "ResolveAssemblyReference.FourSpaceIndent", new object[] { str7 });
                }
                base.Log.LogMessageFromResources(MessageImportance.Low, "ResolveAssemblyReference.LogTaskPropertyFormat", new object[] { "LatestTargetFrameworkDirectories" });
                foreach (string str8 in this.latestTargetFrameworkDirectories)
                {
                    base.Log.LogMessageFromResources(MessageImportance.Low, "ResolveAssemblyReference.FourSpaceIndent", new object[] { str8 });
                }
                base.Log.LogMessageFromResources(MessageImportance.Low, "ResolveAssemblyReference.LogTaskPropertyFormat", new object[] { "ProfileTablesLocation" });
                foreach (ITaskItem item5 in this.FullFrameworkAssemblyTables)
                {
                    base.Log.LogMessageFromResources(MessageImportance.Low, "ResolveAssemblyReference.FourSpaceIndent", new object[] { item5 });
                    this.LogAttribute(item5, "FrameworkDirectory");
                }
            }
        }

        private void LogPrimaryOrDependency(Reference reference, string fusionName, MessageImportance importance)
        {
            if (reference.IsPrimary)
            {
                if (reference.IsUnified)
                {
                    base.Log.LogMessageFromResources(importance, "ResolveAssemblyReference.UnifiedPrimaryReference", new object[] { fusionName });
                }
                else
                {
                    base.Log.LogMessageFromResources(importance, "ResolveAssemblyReference.PrimaryReference", new object[] { fusionName });
                }
            }
            else if (reference.IsUnified)
            {
                base.Log.LogMessageFromResources(importance, "ResolveAssemblyReference.UnifiedDependency", new object[] { fusionName });
            }
            else
            {
                base.Log.LogMessageFromResources(importance, "ResolveAssemblyReference.Dependency", new object[] { fusionName });
            }
            foreach (UnificationVersion version in reference.GetPreUnificationVersions())
            {
                switch (version.reason)
                {
                    case UnificationReason.DidntUnify:
                    {
                        continue;
                    }
                    case UnificationReason.FrameworkRetarget:
                    {
                        base.Log.LogMessageFromResources(importance, "ResolveAssemblyReference.FourSpaceIndent", new object[] { base.Log.FormatResourceString("ResolveAssemblyReference.UnificationByFrameworkRetarget", new object[] { version.version, version.referenceFullPath }) });
                        continue;
                    }
                    case UnificationReason.BecauseOfBindingRedirect:
                    {
                        if (!this.AutoUnify)
                        {
                            break;
                        }
                        base.Log.LogMessageFromResources(importance, "ResolveAssemblyReference.FourSpaceIndent", new object[] { base.Log.FormatResourceString("ResolveAssemblyReference.UnificationByAutoUnify", new object[] { version.version, version.referenceFullPath }) });
                        continue;
                    }
                    default:
                    {
                        continue;
                    }
                }
                base.Log.LogMessageFromResources(importance, "ResolveAssemblyReference.FourSpaceIndent", new object[] { base.Log.FormatResourceString("ResolveAssemblyReference.UnificationByAppConfig", new object[] { version.version, this.appConfigFile, version.referenceFullPath }) });
            }
        }

        private void LogReference(Reference reference, string fusionName)
        {
            MessageImportance importance = this.ChooseReferenceLoggingImportance(reference);
            this.LogPrimaryOrDependency(reference, fusionName, importance);
            this.LogReferenceErrors(reference, importance);
            this.LogFullName(reference, importance);
            this.LogAssembliesConsideredAndRejected(reference, importance);
            if (!reference.IsBadImage)
            {
                this.LogDependees(reference, importance);
                this.LogRelatedFiles(reference, importance);
                this.LogSatellites(reference, importance);
                this.LogScatterFiles(reference, importance);
                this.LogCopyLocalState(reference, importance);
            }
        }

        private void LogReferenceDependenciesAndSourceItems(string fusionName, Reference conflictCandidate)
        {
            Microsoft.Build.Shared.ErrorUtilities.VerifyThrowInternalNull(conflictCandidate, "ConflictCandidate");
            base.Log.LogMessageFromResources(MessageImportance.Low, "ResolveAssemblyReference.FourSpaceIndent", new object[] { Microsoft.Build.Shared.ResourceUtilities.FormatResourceString("ResolveAssemblyReference.ReferenceDependsOn", new object[] { fusionName, conflictCandidate.FullPath }) });
            if (conflictCandidate.IsPrimary)
            {
                if (conflictCandidate.IsResolved)
                {
                    this.LogDependeeReference(conflictCandidate);
                }
                else
                {
                    base.Log.LogMessageFromResources(MessageImportance.Low, "ResolveAssemblyReference.EightSpaceIndent", new object[] { Microsoft.Build.Shared.ResourceUtilities.FormatResourceString("ResolveAssemblyReference.UnResolvedPrimaryItemSpec", new object[] { conflictCandidate.PrimarySourceItem }) });
                }
            }
            foreach (Reference reference in conflictCandidate.GetDependees())
            {
                this.LogDependeeReference(reference);
            }
        }

        private void LogReferenceErrors(Reference reference, MessageImportance importance)
        {
            foreach (Exception exception in reference.GetErrors())
            {
                string str3;
                string message = string.Empty;
                string helpKeyword = null;
                bool flag = false;
                if (exception is ReferenceResolutionException)
                {
                    message = base.Log.FormatResourceString("ResolveAssemblyReference.FailedToResolveReference", new object[] { exception.Message });
                    helpKeyword = "MSBuild.ResolveAssemblyReference.FailedToResolveReference";
                    flag = false;
                }
                else if (exception is DependencyResolutionException)
                {
                    message = base.Log.FormatResourceString("ResolveAssemblyReference.FailedToFindDependentFiles", new object[] { exception.Message });
                    helpKeyword = "MSBuild.ResolveAssemblyReference.FailedToFindDependentFiles";
                    flag = true;
                }
                else if (exception is BadImageReferenceException)
                {
                    message = base.Log.FormatResourceString("ResolveAssemblyReference.FailedWithException", new object[] { exception.Message });
                    helpKeyword = "MSBuild.ResolveAssemblyReference.FailedWithException";
                    flag = false;
                }
                string warningCode = base.Log.ExtractMessageCode(message, out str3);
                if (reference.IsPrimary && !flag)
                {
                    base.Log.LogWarning(null, warningCode, helpKeyword, null, 0, 0, 0, 0, str3, new object[0]);
                }
                else
                {
                    base.Log.LogMessageFromResources(importance, "ResolveAssemblyReference.FourSpaceIndent", new object[] { str3 });
                }
            }
        }

        private void LogRelatedFiles(Reference reference, MessageImportance importance)
        {
            if (reference.IsResolved && (reference.FullPath.Length > 0))
            {
                foreach (string str in reference.GetRelatedFileExtensions())
                {
                    base.Log.LogMessageFromResources(importance, "ResolveAssemblyReference.FourSpaceIndent", new object[] { base.Log.FormatResourceString("ResolveAssemblyReference.FoundRelatedFile", new object[] { reference.FullPathWithoutExtension + str }) });
                }
            }
        }

        private bool LogResults(ReferenceTable dependencyTable, DependentAssembly[] idealAssemblyRemappings, AssemblyNameReference[] idealAssemblyRemappingsIdentities, ArrayList generalResolutionExceptions)
        {
            bool flag = true;
            using (new CodeMarkerStartEnd(CodeMarkerEvent.perfMSBuildRARLogResultsBegin, CodeMarkerEvent.perfMSBuildRARLogResultsEnd))
            {
                if (this.Silent)
                {
                    return flag;
                }
                foreach (AssemblyNameExtension extension in dependencyTable.References.Keys)
                {
                    string fullName = extension.FullName;
                    Reference reference = dependencyTable.GetReference(extension);
                    if (reference.IsPrimary && (!reference.IsConflictVictim || !reference.IsCopyLocal))
                    {
                        this.LogReference(reference, fullName);
                    }
                }
                foreach (AssemblyNameExtension extension2 in dependencyTable.References.Keys)
                {
                    string fusionName = extension2.FullName;
                    Reference reference2 = dependencyTable.GetReference(extension2);
                    if (!reference2.IsPrimary && (!reference2.IsConflictVictim || !reference2.IsCopyLocal))
                    {
                        this.LogReference(reference2, fusionName);
                    }
                }
                foreach (AssemblyNameExtension extension3 in dependencyTable.References.Keys)
                {
                    string str3 = extension3.FullName;
                    Reference reference3 = dependencyTable.GetReference(extension3);
                    if (reference3.IsConflictVictim)
                    {
                        this.LogConflict(reference3, str3);
                        Reference conflictCandidate = dependencyTable.GetReference(reference3.ConflictVictorName);
                        this.LogReferenceDependenciesAndSourceItems(reference3.ConflictVictorName.FullName, conflictCandidate);
                        this.LogReferenceDependenciesAndSourceItems(str3, reference3);
                    }
                }
                if (this.suggestedRedirects.Length > 0)
                {
                    for (int i = 0; i < idealAssemblyRemappings.Length; i++)
                    {
                        DependentAssembly assembly = idealAssemblyRemappings[i];
                        AssemblyName partialAssemblyName = assembly.PartialAssemblyName;
                        Reference reference5 = idealAssemblyRemappingsIdentities[i].reference;
                        for (int j = 0; j < assembly.BindingRedirects.Length; j++)
                        {
                            foreach (AssemblyNameExtension extension4 in reference5.GetConflictVictims())
                            {
                                Reference reference6 = dependencyTable.GetReference(extension4);
                                base.Log.LogMessageFromResources(MessageImportance.High, "ResolveAssemblyReference.ConflictRedirectSuggestion", new object[] { partialAssemblyName, extension4.Version, reference6.FullPath, assembly.BindingRedirects[j].NewVersion, reference5.FullPath });
                            }
                        }
                    }
                    base.Log.LogWarningWithCodeFromResources("ResolveAssemblyReference.SuggestedRedirects", new object[0]);
                }
                foreach (Exception exception in generalResolutionExceptions)
                {
                    if (!(exception is InvalidReferenceAssemblyNameException))
                    {
                        throw exception;
                    }
                    InvalidReferenceAssemblyNameException exception2 = (InvalidReferenceAssemblyNameException) exception;
                    base.Log.LogWarningWithCodeFromResources("General.MalformedAssemblyName", new object[] { exception2.SourceItemSpec });
                }
            }
            return flag;
        }

        private void LogSatellites(Reference reference, MessageImportance importance)
        {
            foreach (string str in reference.GetSatelliteFiles())
            {
                base.Log.LogMessageFromResources(importance, "ResolveAssemblyReference.FourSpaceIndent", new object[] { base.Log.FormatResourceString("ResolveAssemblyReference.FoundSatelliteFile", new object[] { str }) });
            }
        }

        private void LogScatterFiles(Reference reference, MessageImportance importance)
        {
            foreach (string str in reference.GetScatterFiles())
            {
                base.Log.LogMessageFromResources(importance, "ResolveAssemblyReference.FourSpaceIndent", new object[] { base.Log.FormatResourceString("ResolveAssemblyReference.FoundScatterFile", new object[] { str }) });
            }
        }

        private void PopulateSuggestedRedirects(DependentAssembly[] idealAssemblyRemappings)
        {
            ArrayList list = new ArrayList();
            if (idealAssemblyRemappings != null)
            {
                for (int i = 0; i < idealAssemblyRemappings.Length; i++)
                {
                    DependentAssembly assembly = idealAssemblyRemappings[i];
                    string str = assembly.PartialAssemblyName.ToString();
                    for (int j = 0; j < assembly.BindingRedirects.Length; j++)
                    {
                        ITaskItem item = new TaskItem {
                            ItemSpec = str
                        };
                        item.SetMetadata("MaxVersion", assembly.BindingRedirects[j].NewVersion.ToString());
                        list.Add(item);
                    }
                }
            }
            this.suggestedRedirects = (ITaskItem[]) list.ToArray(typeof(ITaskItem));
        }

        internal static string ProcessorArchitectureToString(System.Reflection.ProcessorArchitecture processorArchitecture)
        {
            if (System.Reflection.ProcessorArchitecture.Amd64 == processorArchitecture)
            {
                return "AMD64";
            }
            if (System.Reflection.ProcessorArchitecture.IA64 == processorArchitecture)
            {
                return "IA64";
            }
            if (System.Reflection.ProcessorArchitecture.MSIL == processorArchitecture)
            {
                return "MSIL";
            }
            if (System.Reflection.ProcessorArchitecture.X86 == processorArchitecture)
            {
                return "x86";
            }
            return string.Empty;
        }

        private void ReadStateFile()
        {
            this.cache = (SystemState) StateFileBase.DeserializeCache(this.stateFile, base.Log, typeof(SystemState));
            if (this.cache == null)
            {
                this.cache = new SystemState();
            }
        }

        private Version SetTargetedRuntimeVersion(GetAssemblyRuntimeVersion getRuntimeVersion)
        {
            Version version = null;
            if (this.targetedRuntimeVersionRawValue != null)
            {
                version = Microsoft.Build.Tasks.VersionUtilities.ConvertToVersion(this.targetedRuntimeVersionRawValue);
            }
            if (version == null)
            {
                this.targetedRuntimeVersionRawValue = typeof(object).Assembly.ImageRuntimeVersion;
                version = Microsoft.Build.Tasks.VersionUtilities.ConvertToVersion(this.targetedRuntimeVersionRawValue);
            }
            return version;
        }

        private bool ShouldUseSubsetBlackList()
        {
            foreach (string str in this.fullTargetFrameworkSubsetNames)
            {
                foreach (string str2 in this.targetFrameworkSubsets)
                {
                    if (string.Equals(str, str2, StringComparison.OrdinalIgnoreCase))
                    {
                        if (!this.silent)
                        {
                            base.Log.LogMessageFromResources(MessageImportance.Low, "ResolveAssemblyReference.NoExclusionListBecauseofFullClientName", new object[] { str2 });
                        }
                        return false;
                    }
                }
            }
            if (this.IgnoreDefaultInstalledAssemblySubsetTables && (this.installedAssemblySubsetTables.Length == 0))
            {
                return false;
            }
            if ((this.targetFrameworkSubsets.Length == 0) && (this.installedAssemblySubsetTables.Length == 0))
            {
                return false;
            }
            if (!this.silent)
            {
                base.Log.LogMessageFromResources(MessageImportance.Low, "ResolveAssemblyReference.UsingExclusionList", new object[0]);
            }
            return true;
        }

        internal static System.Reflection.ProcessorArchitecture TargetProcessorArchitectureToEnumeration(string targetedProcessorArchitecture)
        {
            if (targetedProcessorArchitecture != null)
            {
                if (targetedProcessorArchitecture.Equals("AMD64", StringComparison.OrdinalIgnoreCase))
                {
                    return System.Reflection.ProcessorArchitecture.Amd64;
                }
                if (targetedProcessorArchitecture.Equals("IA64", StringComparison.OrdinalIgnoreCase))
                {
                    return System.Reflection.ProcessorArchitecture.IA64;
                }
                if (targetedProcessorArchitecture.Equals("MSIL", StringComparison.OrdinalIgnoreCase))
                {
                    return System.Reflection.ProcessorArchitecture.MSIL;
                }
                if (targetedProcessorArchitecture.Equals("x86", StringComparison.OrdinalIgnoreCase))
                {
                    return System.Reflection.ProcessorArchitecture.X86;
                }
            }
            return System.Reflection.ProcessorArchitecture.None;
        }

        private bool VerifyInputConditions()
        {
            if ((this.AutoUnify && (this.AppConfigFile != null)) && (this.AppConfigFile.Length > 0))
            {
                base.Log.LogErrorWithCodeFromResources("ResolveAssemblyReference.ConflictBetweenAppConfigAndAutoUnify", new object[0]);
                return false;
            }
            bool flag = (this.TargetFrameworkSubsets.Length != 0) || (this.InstalledAssemblySubsetTables.Length != 0);
            bool flag2 = !string.IsNullOrEmpty(this.ProfileName);
            bool flag3 = this.FullFrameworkFolders.Length > 0;
            bool flag4 = this.FullFrameworkAssemblyTables.Length > 0;
            bool flag5 = flag2 && (flag3 || flag4);
            if (flag && flag5)
            {
                base.Log.LogErrorWithCodeFromResources("ResolveAssemblyReference.CannotSetProfileAndSubSet", new object[0]);
                return false;
            }
            if ((flag2 && !flag3) && !flag4)
            {
                base.Log.LogErrorWithCodeFromResources("ResolveAssemblyReference.MustSetProfileNameAndFolderLocations", new object[0]);
                return false;
            }
            return true;
        }

        private void WriteStateFile()
        {
            if (((this.stateFile != null) && (this.stateFile.Length > 0)) && this.cache.IsDirty)
            {
                this.cache.SerializeCache(this.stateFile, base.Log);
            }
        }

        public string[] AllowedAssemblyExtensions
        {
            get
            {
                return this.allowedAssemblyExtensions;
            }
            set
            {
                this.allowedAssemblyExtensions = value;
            }
        }

        public string[] AllowedRelatedFileExtensions
        {
            get
            {
                return this.relatedFileExtensions;
            }
            set
            {
                this.relatedFileExtensions = value;
            }
        }

        public string AppConfigFile
        {
            get
            {
                return this.appConfigFile;
            }
            set
            {
                this.appConfigFile = value;
            }
        }

        public ITaskItem[] Assemblies
        {
            get
            {
                return this.assemblyNames;
            }
            set
            {
                this.assemblyNames = value;
            }
        }

        public ITaskItem[] AssemblyFiles
        {
            get
            {
                return this.assemblyFiles;
            }
            set
            {
                this.assemblyFiles = value;
            }
        }

        public bool AutoUnify
        {
            get
            {
                return this.autoUnify;
            }
            set
            {
                this.autoUnify = value;
            }
        }

        public string[] CandidateAssemblyFiles
        {
            get
            {
                return this.candidateAssemblyFiles;
            }
            set
            {
                this.candidateAssemblyFiles = value;
            }
        }

        public bool CopyLocalDependenciesWhenParentReferenceInGac
        {
            get
            {
                return this.copyLocalDependenciesWhenParentReferenceInGac;
            }
            set
            {
                this.copyLocalDependenciesWhenParentReferenceInGac = value;
            }
        }

        [Output]
        public ITaskItem[] CopyLocalFiles
        {
            get
            {
                return this.copyLocalFiles;
            }
        }

        [Output]
        public ITaskItem[] FilesWritten
        {
            get
            {
                return (ITaskItem[]) this.filesWritten.ToArray(typeof(ITaskItem));
            }
            set
            {
            }
        }

        public bool FindDependencies
        {
            get
            {
                return this.findDependencies;
            }
            set
            {
                this.findDependencies = value;
            }
        }

        public bool FindRelatedFiles
        {
            get
            {
                return this.findRelatedFiles;
            }
            set
            {
                this.findRelatedFiles = value;
            }
        }

        public bool FindSatellites
        {
            get
            {
                return this.findSatellites;
            }
            set
            {
                this.findSatellites = value;
            }
        }

        public bool FindSerializationAssemblies
        {
            get
            {
                return this.findSerializationAssemblies;
            }
            set
            {
                this.findSerializationAssemblies = value;
            }
        }

        public ITaskItem[] FullFrameworkAssemblyTables
        {
            get
            {
                return this.fullFrameworkAssemblyTables;
            }
            set
            {
                Microsoft.Build.Shared.ErrorUtilities.VerifyThrowArgumentNull(value, "FullFrameworkAssemblyTables");
                this.fullFrameworkAssemblyTables = value;
            }
        }

        public string[] FullFrameworkFolders
        {
            get
            {
                return this.fullFrameworkFolders;
            }
            set
            {
                Microsoft.Build.Shared.ErrorUtilities.VerifyThrowArgumentNull(value, "FullFrameworkFolders");
                this.fullFrameworkFolders = value;
            }
        }

        public string[] FullTargetFrameworkSubsetNames
        {
            get
            {
                return this.fullTargetFrameworkSubsetNames;
            }
            set
            {
                Microsoft.Build.Shared.ErrorUtilities.VerifyThrowArgumentNull(value, "FullTargetFrameworkSubsetNames");
                this.fullTargetFrameworkSubsetNames = value;
            }
        }

        public bool IgnoreDefaultInstalledAssemblySubsetTables
        {
            get
            {
                return this.ignoreDefaultInstalledAssemblySubsetTables;
            }
            set
            {
                this.ignoreDefaultInstalledAssemblySubsetTables = value;
            }
        }

        public bool IgnoreDefaultInstalledAssemblyTables
        {
            get
            {
                return this.ignoreDefaultInstalledAssemblyTables;
            }
            set
            {
                this.ignoreDefaultInstalledAssemblyTables = value;
            }
        }

        public ITaskItem[] InstalledAssemblySubsetTables
        {
            get
            {
                return this.installedAssemblySubsetTables;
            }
            set
            {
                Microsoft.Build.Shared.ErrorUtilities.VerifyThrowArgumentNull(value, "InstalledAssemblySubsetTables");
                this.installedAssemblySubsetTables = value;
            }
        }

        public ITaskItem[] InstalledAssemblyTables
        {
            get
            {
                return this.installedAssemblyTables;
            }
            set
            {
                this.installedAssemblyTables = value;
            }
        }

        public string[] LatestTargetFrameworkDirectories
        {
            get
            {
                return this.latestTargetFrameworkDirectories;
            }
            set
            {
                this.latestTargetFrameworkDirectories = value;
            }
        }

        public string ProfileName
        {
            get
            {
                return this.profileName;
            }
            set
            {
                Microsoft.Build.Shared.ErrorUtilities.VerifyThrowArgumentNull(value, "profileName");
                this.profileName = value;
            }
        }

        [Output]
        public ITaskItem[] RelatedFiles
        {
            get
            {
                return this.relatedFiles;
            }
        }

        [Output]
        public ITaskItem[] ResolvedDependencyFiles
        {
            get
            {
                return this.resolvedDependencyFiles;
            }
        }

        [Output]
        public ITaskItem[] ResolvedFiles
        {
            get
            {
                return this.resolvedFiles;
            }
        }

        [Output]
        public ITaskItem[] SatelliteFiles
        {
            get
            {
                return this.satelliteFiles;
            }
        }

        [Output]
        public ITaskItem[] ScatterFiles
        {
            get
            {
                return this.scatterFiles;
            }
        }

        [Required]
        public string[] SearchPaths
        {
            get
            {
                return this.searchPaths;
            }
            set
            {
                this.searchPaths = value;
            }
        }

        [Output]
        public ITaskItem[] SerializationAssemblyFiles
        {
            get
            {
                return this.serializationAssemblyFiles;
            }
        }

        public bool Silent
        {
            get
            {
                return this.silent;
            }
            set
            {
                this.silent = value;
            }
        }

        public string StateFile
        {
            get
            {
                return this.stateFile;
            }
            set
            {
                this.stateFile = value;
            }
        }

        [Output]
        public ITaskItem[] SuggestedRedirects
        {
            get
            {
                return this.suggestedRedirects;
            }
        }

        public string TargetedRuntimeVersion
        {
            get
            {
                return this.targetedRuntimeVersionRawValue;
            }
            set
            {
                this.targetedRuntimeVersionRawValue = value;
            }
        }

        public string[] TargetFrameworkDirectories
        {
            get
            {
                return this.targetFrameworkDirectories;
            }
            set
            {
                this.targetFrameworkDirectories = value;
            }
        }

        public string TargetFrameworkMoniker
        {
            get
            {
                return this.targetedFrameworkMoniker;
            }
            set
            {
                this.targetedFrameworkMoniker = value;
            }
        }

        public string TargetFrameworkMonikerDisplayName { get; set; }

        public string[] TargetFrameworkSubsets
        {
            get
            {
                return this.targetFrameworkSubsets;
            }
            set
            {
                Microsoft.Build.Shared.ErrorUtilities.VerifyThrowArgumentNull(value, "TargetFrameworkSubsets");
                this.targetFrameworkSubsets = value;
            }
        }

        public string TargetFrameworkVersion
        {
            get
            {
                return this.projectTargetFrameworkAsString;
            }
            set
            {
                this.projectTargetFrameworkAsString = value;
            }
        }

        public string TargetProcessorArchitecture
        {
            get
            {
                return this.targetProcessorArchitecture;
            }
            set
            {
                this.targetProcessorArchitecture = value;
            }
        }

        private delegate string[] GetListPath(string targetFrameworkDirectory);
    }
}

