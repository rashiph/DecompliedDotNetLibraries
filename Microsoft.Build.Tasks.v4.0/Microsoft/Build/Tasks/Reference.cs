namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Shared;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    internal sealed class Reference
    {
        private ArrayList assembliesConsideredAndRejected = new ArrayList();
        private string assemblyFolderKey = string.Empty;
        private ConflictLossReason conflictLossReason;
        private ArrayList conflictVictims = new ArrayList();
        private AssemblyNameExtension conflictVictorName;
        private CopyLocalState copyLocalState;
        private Hashtable dependees = new Hashtable();
        private HashSet<Reference> dependencies = new HashSet<Reference>();
        private bool dependenciesFound;
        private string directoryName = string.Empty;
        private bool embedInteropTypes;
        private ArrayList errors = new ArrayList();
        private ExclusionListProperties exclusionListProperties = new ExclusionListProperties();
        private ArrayList expectedExtensions;
        private string fileNameWithoutExtension = string.Empty;
        private string fullPath = string.Empty;
        private string fullPathWithoutExtension = string.Empty;
        private string hintPath = "";
        private bool isBadImage;
        private bool isPrerequisite;
        private bool isPrimary;
        private bool? isRedistRoot = null;
        private Dictionary<string, UnificationVersion> preUnificationVersions = new Dictionary<string, UnificationVersion>(StringComparer.OrdinalIgnoreCase);
        private ITaskItem primarySourceItem;
        private string redistName;
        private Version referenceVersion;
        private ArrayList relatedFileExtensions = new ArrayList();
        private string resolvedSearchPath = string.Empty;
        private ArrayList satelliteFiles = new ArrayList();
        private string[] scatterFiles = new string[0];
        private ArrayList serializationAssemblyFiles = new ArrayList();
        private Hashtable sourceItems = new Hashtable(StringComparer.OrdinalIgnoreCase);
        private bool userRequestedSpecificFile;
        private bool wantSpecificVersion = true;

        internal void AddAssembliesConsideredAndRejected(ArrayList assembliesConsideredAndRejectedToAdd)
        {
            this.assembliesConsideredAndRejected.AddRange(assembliesConsideredAndRejectedToAdd);
        }

        internal void AddConflictVictim(AssemblyNameExtension victim)
        {
            this.conflictVictims.Add(victim);
        }

        internal void AddDependee(Reference dependee)
        {
            dependee.AddDependency(this);
            if (this.dependees[dependee] == null)
            {
                this.dependees[dependee] = string.Empty;
                if (this.IsUnresolvable)
                {
                    this.errors = new ArrayList();
                    this.assembliesConsideredAndRejected = new ArrayList();
                }
            }
        }

        internal void AddDependency(Reference dependency)
        {
            if (!this.dependencies.Contains(dependency))
            {
                this.dependencies.Add(dependency);
            }
        }

        internal void AddError(Exception e)
        {
            if (e is BadImageReferenceException)
            {
                this.isBadImage = true;
            }
            this.errors.Add(e);
        }

        internal void AddPreUnificationVersion(string referencePath, Version version, UnificationReason reason)
        {
            UnificationVersion version2;
            string key = referencePath + version.ToString() + reason.ToString();
            if (!this.preUnificationVersions.TryGetValue(key, out version2))
            {
                version2 = new UnificationVersion {
                    referenceFullPath = referencePath,
                    version = version,
                    reason = reason
                };
                this.preUnificationVersions[key] = version2;
            }
        }

        internal void AddRelatedFileExtension(string filenameExtension)
        {
            this.relatedFileExtensions.Add(filenameExtension);
        }

        internal void AddSatelliteFile(string filename)
        {
            this.satelliteFiles.Add(filename);
        }

        internal void AddSerializationAssemblyFile(string filename)
        {
            this.serializationAssemblyFiles.Add(filename);
        }

        internal void AddSourceItem(ITaskItem sourceItem)
        {
            if (!this.sourceItems.Contains(sourceItem.ItemSpec))
            {
                this.sourceItems[sourceItem.ItemSpec] = sourceItem;
                this.PropagateSourceItems(sourceItem);
            }
        }

        internal void AddSourceItems(IEnumerable sourceItemsToAdd)
        {
            foreach (ITaskItem item in sourceItemsToAdd)
            {
                this.AddSourceItem(item);
            }
        }

        internal void AttachScatterFiles(string[] scatterFilesToAttach)
        {
            if ((scatterFilesToAttach == null) || (scatterFilesToAttach.Length == 0))
            {
                this.scatterFiles = new string[0];
            }
            else
            {
                this.scatterFiles = scatterFilesToAttach;
            }
        }

        internal bool CheckForSpecificVersionMetadataOnParentsReference(bool anyParentHasMetadata)
        {
            bool flag = false;
            if (this.IsPrimary)
            {
                return this.wantSpecificVersion;
            }
            foreach (ITaskItem item in this.GetSourceItems())
            {
                flag = MetadataConversionUtilities.TryConvertItemMetadataToBool(item, "SpecificVersion");
                if (anyParentHasMetadata == flag)
                {
                    return flag;
                }
            }
            return flag;
        }

        internal AssemblyNameExtension[] GetConflictVictims()
        {
            return (AssemblyNameExtension[]) this.conflictVictims.ToArray(typeof(AssemblyNameExtension));
        }

        internal ICollection GetDependees()
        {
            return this.dependees.Keys;
        }

        internal ICollection GetErrors()
        {
            return this.errors;
        }

        internal string[] GetExecutableExtensions(string[] allowedAssemblyExtensions)
        {
            if (this.expectedExtensions == null)
            {
                return allowedAssemblyExtensions;
            }
            return (string[]) this.expectedExtensions.ToArray(typeof(string));
        }

        internal List<UnificationVersion> GetPreUnificationVersions()
        {
            return new List<UnificationVersion>(this.preUnificationVersions.Values);
        }

        internal ICollection GetRelatedFileExtensions()
        {
            return this.relatedFileExtensions;
        }

        internal ICollection GetSatelliteFiles()
        {
            return this.satelliteFiles;
        }

        internal string[] GetScatterFiles()
        {
            return this.scatterFiles;
        }

        internal ICollection GetSerializationAssemblyFiles()
        {
            return this.serializationAssemblyFiles;
        }

        internal ICollection GetSourceItems()
        {
            return this.sourceItems.Values;
        }

        private static bool IsFrameworkFile(string fullPath, string[] frameworkPaths)
        {
            if (frameworkPaths != null)
            {
                foreach (string str in frameworkPaths)
                {
                    if (string.Compare(str, 0, fullPath, 0, str.Length, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        internal void MakeDependentAssemblyReference(Reference sourceReference)
        {
            this.copyLocalState = CopyLocalState.Undecided;
            this.isPrimary = false;
            this.DependenciesFound = false;
            this.wantSpecificVersion = true;
            this.AddSourceItems(sourceReference.GetSourceItems());
            this.AddDependee(sourceReference);
        }

        internal void MakePrimaryAssemblyReference(ITaskItem sourceItem, bool wantSpecificVersionValue, string executableExtension)
        {
            this.copyLocalState = CopyLocalState.Undecided;
            this.isPrimary = true;
            this.primarySourceItem = sourceItem;
            if ((executableExtension != null) && (executableExtension.Length > 0))
            {
                this.SetExecutableExtension(executableExtension);
            }
            this.wantSpecificVersion = wantSpecificVersionValue;
            this.DependenciesFound = false;
            this.AddSourceItem(sourceItem);
        }

        internal void PropagateSourceItems(ITaskItem sourceItem)
        {
            if (this.dependencies != null)
            {
                foreach (Reference reference in this.dependencies)
                {
                    reference.AddSourceItem(sourceItem);
                }
            }
        }

        internal void RemoveDependee(Reference dependeeToRemove)
        {
            this.dependees.Remove(dependeeToRemove);
        }

        internal void RemoveDependency(Reference dependencyToRemove)
        {
            this.dependencies.Remove(dependencyToRemove);
        }

        internal void SetExecutableExtension(string extension)
        {
            if (this.expectedExtensions == null)
            {
                this.expectedExtensions = new ArrayList();
            }
            else
            {
                this.expectedExtensions.Clear();
            }
            if ((extension.Length > 0) && (extension[0] != '.'))
            {
                extension = '.' + extension;
            }
            this.expectedExtensions.Add(extension);
        }

        internal void SetFinalCopyLocalState(AssemblyNameExtension assemblyName, string[] frameworkPaths, ProcessorArchitecture targetProcessorArchitecture, GetAssemblyRuntimeVersion getRuntimeVersion, Version targetedRuntimeVersion, Microsoft.Build.Shared.FileExists fileExists, bool copyLocalDependenciesWhenParentReferenceInGac, ReferenceTable referenceTable, CheckIfAssemblyInGac checkIfAssemblyInGac)
        {
            if (this.IsUnresolvable)
            {
                this.copyLocalState = CopyLocalState.NoBecauseUnresolved;
            }
            else if (this.EmbedInteropTypes)
            {
                this.copyLocalState = CopyLocalState.NoBecauseEmbedded;
            }
            else if (this.IsConflictVictim)
            {
                this.copyLocalState = CopyLocalState.NoBecauseConflictVictim;
            }
            else
            {
                if (this.IsPrimary)
                {
                    bool flag;
                    bool flag2 = MetadataConversionUtilities.TryConvertItemMetadataToBool(this.PrimarySourceItem, "Private", out flag);
                    if (flag)
                    {
                        if (flag2)
                        {
                            this.copyLocalState = CopyLocalState.YesBecauseReferenceItemHadMetadata;
                            return;
                        }
                        this.copyLocalState = CopyLocalState.NoBecauseReferenceItemHadMetadata;
                        return;
                    }
                }
                else
                {
                    bool flag3 = false;
                    bool flag4 = false;
                    foreach (DictionaryEntry entry in this.sourceItems)
                    {
                        bool flag5;
                        bool flag6 = MetadataConversionUtilities.TryConvertItemMetadataToBool((ITaskItem) entry.Value, "Private", out flag5);
                        if (flag5)
                        {
                            if (flag6)
                            {
                                flag3 = true;
                                break;
                            }
                            flag4 = true;
                        }
                    }
                    if (flag4 && !flag3)
                    {
                        this.copyLocalState = CopyLocalState.NoBecauseReferenceItemHadMetadata;
                        return;
                    }
                }
                if (this.IsPrerequisite && !this.UserRequestedSpecificFile)
                {
                    this.copyLocalState = CopyLocalState.NoBecausePrerequisite;
                }
                else if (IsFrameworkFile(this.fullPath, frameworkPaths))
                {
                    this.copyLocalState = CopyLocalState.NoBecauseFrameworkFile;
                }
                else
                {
                    if (!this.FoundInGac.HasValue)
                    {
                        bool flag7 = checkIfAssemblyInGac(assemblyName, targetProcessorArchitecture, getRuntimeVersion, targetedRuntimeVersion, fileExists);
                        this.FoundInGac = new bool?(flag7);
                    }
                    if (this.FoundInGac.Value)
                    {
                        this.copyLocalState = CopyLocalState.NoBecauseReferenceFoundInGAC;
                    }
                    else
                    {
                        if (!this.IsPrimary && !copyLocalDependenciesWhenParentReferenceInGac)
                        {
                            bool flag8 = false;
                            foreach (DictionaryEntry entry2 in this.sourceItems)
                            {
                                AssemblyNameExtension referenceFromItemSpec = referenceTable.GetReferenceFromItemSpec((string) entry2.Key);
                                Reference reference = referenceTable.GetReference(referenceFromItemSpec);
                                bool flag9 = false;
                                if (!reference.FoundInGac.HasValue)
                                {
                                    flag9 = checkIfAssemblyInGac(referenceFromItemSpec, targetProcessorArchitecture, getRuntimeVersion, targetedRuntimeVersion, fileExists);
                                    reference.FoundInGac = new bool?(flag9);
                                }
                                else
                                {
                                    flag9 = reference.FoundInGac.Value;
                                }
                                if (!flag9)
                                {
                                    flag8 = true;
                                    break;
                                }
                            }
                            if (!flag8)
                            {
                                this.copyLocalState = CopyLocalState.NoBecauseParentReferencesFoundInGAC;
                                return;
                            }
                        }
                        this.copyLocalState = CopyLocalState.YesBecauseOfHeuristic;
                    }
                }
            }
        }

        public override string ToString()
        {
            if (this.IsResolved)
            {
                return this.FullPath;
            }
            return "*Unresolved*";
        }

        internal ArrayList AssembliesConsideredAndRejected
        {
            get
            {
                return this.assembliesConsideredAndRejected;
            }
        }

        internal string AssemblyFolderKey
        {
            get
            {
                return this.assemblyFolderKey;
            }
            set
            {
                this.assemblyFolderKey = value;
            }
        }

        internal ConflictLossReason ConflictLossExplanation
        {
            get
            {
                return this.conflictLossReason;
            }
            set
            {
                this.conflictLossReason = value;
            }
        }

        internal AssemblyNameExtension ConflictVictorName
        {
            get
            {
                return this.conflictVictorName;
            }
            set
            {
                this.conflictVictorName = value;
            }
        }

        internal CopyLocalState CopyLocal
        {
            get
            {
                return this.copyLocalState;
            }
        }

        internal bool DependenciesFound
        {
            get
            {
                return this.dependenciesFound;
            }
            set
            {
                this.dependenciesFound = value;
            }
        }

        internal string DirectoryName
        {
            get
            {
                if (((this.directoryName == null) || (this.directoryName.Length == 0)) && ((this.fullPath != null) && (this.fullPath.Length != 0)))
                {
                    this.directoryName = Path.GetDirectoryName(this.fullPath);
                    if (this.directoryName.Length == 0)
                    {
                        this.directoryName = ".";
                    }
                }
                return this.directoryName;
            }
        }

        internal bool EmbedInteropTypes
        {
            get
            {
                return this.embedInteropTypes;
            }
            set
            {
                this.embedInteropTypes = value;
            }
        }

        internal ExclusionListProperties ExclusionListLoggingProperties
        {
            get
            {
                return this.exclusionListProperties;
            }
        }

        internal string FileNameWithoutExtension
        {
            get
            {
                if (((this.fileNameWithoutExtension == null) || (this.fileNameWithoutExtension.Length == 0)) && ((this.fullPath != null) && (this.fullPath.Length != 0)))
                {
                    this.fileNameWithoutExtension = Path.GetFileNameWithoutExtension(this.fullPath);
                }
                return this.fileNameWithoutExtension;
            }
        }

        internal bool? FoundInGac { get; set; }

        internal string FullPath
        {
            get
            {
                return this.fullPath;
            }
            set
            {
                if (this.fullPath != value)
                {
                    this.fullPath = value;
                    this.fullPathWithoutExtension = null;
                    this.fileNameWithoutExtension = null;
                    this.directoryName = null;
                    if ((this.fullPath == null) || (this.fullPath.Length == 0))
                    {
                        this.scatterFiles = new string[0];
                        this.satelliteFiles = new ArrayList();
                        this.serializationAssemblyFiles = new ArrayList();
                        this.assembliesConsideredAndRejected = new ArrayList();
                        this.resolvedSearchPath = string.Empty;
                        this.preUnificationVersions = new Dictionary<string, UnificationVersion>(StringComparer.OrdinalIgnoreCase);
                        this.isBadImage = false;
                        this.dependenciesFound = false;
                        this.userRequestedSpecificFile = false;
                    }
                }
            }
        }

        internal string FullPathWithoutExtension
        {
            get
            {
                if (((this.fullPathWithoutExtension == null) || (this.fullPathWithoutExtension.Length == 0)) && ((this.fullPath != null) && (this.fullPath.Length != 0)))
                {
                    this.fullPathWithoutExtension = Path.Combine(this.DirectoryName, this.FileNameWithoutExtension);
                }
                return this.fullPathWithoutExtension;
            }
        }

        internal string HintPath
        {
            get
            {
                return this.hintPath;
            }
            set
            {
                this.hintPath = value;
            }
        }

        internal bool IsBadImage
        {
            get
            {
                return this.isBadImage;
            }
        }

        internal bool IsConflictVictim
        {
            get
            {
                return (this.ConflictVictorName != null);
            }
        }

        internal bool IsCopyLocal
        {
            get
            {
                return CopyLocalStateUtility.IsCopyLocal(this.copyLocalState);
            }
        }

        internal bool IsPrerequisite
        {
            get
            {
                return this.isPrerequisite;
            }
            set
            {
                this.isPrerequisite = value;
            }
        }

        internal bool IsPrimary
        {
            get
            {
                return this.isPrimary;
            }
        }

        internal bool? IsRedistRoot
        {
            get
            {
                return this.isRedistRoot;
            }
            set
            {
                this.isRedistRoot = value;
            }
        }

        internal bool IsResolved
        {
            get
            {
                return (this.fullPath.Length > 0);
            }
        }

        internal bool IsUnified
        {
            get
            {
                return (this.preUnificationVersions.Count != 0);
            }
        }

        internal bool IsUnresolvable
        {
            get
            {
                return (!this.IsResolved && (this.errors.Count > 0));
            }
        }

        internal ITaskItem PrimarySourceItem
        {
            get
            {
                Microsoft.Build.Shared.ErrorUtilities.VerifyThrow(!this.isPrimary || (this.primarySourceItem != null), "A primary reference must have a primary source item.");
                Microsoft.Build.Shared.ErrorUtilities.VerifyThrow(this.isPrimary || (this.primarySourceItem == null), "Only a primary reference can have a primary source item.");
                return this.primarySourceItem;
            }
        }

        internal string RedistName
        {
            get
            {
                return this.redistName;
            }
            set
            {
                this.redistName = value;
            }
        }

        internal Version ReferenceVersion
        {
            get
            {
                return this.referenceVersion;
            }
            set
            {
                this.referenceVersion = value;
            }
        }

        internal string ResolvedSearchPath
        {
            get
            {
                return this.resolvedSearchPath;
            }
            set
            {
                this.resolvedSearchPath = value;
            }
        }

        internal bool UserRequestedSpecificFile
        {
            get
            {
                return this.userRequestedSpecificFile;
            }
            set
            {
                this.userRequestedSpecificFile = value;
            }
        }

        internal bool WantSpecificVersion
        {
            get
            {
                return this.wantSpecificVersion;
            }
        }

        internal class ExclusionListProperties
        {
            private ReferenceTable.LogExclusionReason exclusionReasonLogDelegate;
            private string highestRedistListMonkier;
            private Version highestVersionInRedist;
            private bool isInExclusionList;

            internal ReferenceTable.LogExclusionReason ExclusionReasonLogDelegate
            {
                get
                {
                    return this.exclusionReasonLogDelegate;
                }
                set
                {
                    this.exclusionReasonLogDelegate = value;
                }
            }

            internal string HighestRedistListMonkier
            {
                get
                {
                    return this.highestRedistListMonkier;
                }
                set
                {
                    this.highestRedistListMonkier = value;
                }
            }

            internal Version HighestVersionInRedist
            {
                get
                {
                    return this.highestVersionInRedist;
                }
                set
                {
                    this.highestVersionInRedist = value;
                }
            }

            internal bool IsInExclusionList
            {
                get
                {
                    return this.isInExclusionList;
                }
                set
                {
                    this.isInExclusionList = value;
                }
            }
        }
    }
}

