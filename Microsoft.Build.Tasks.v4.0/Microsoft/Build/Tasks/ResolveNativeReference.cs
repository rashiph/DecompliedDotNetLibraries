namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Shared;
    using Microsoft.Build.Tasks.Deployment.ManifestUtilities;
    using Microsoft.Build.Utilities;
    using System;
    using System.Collections;
    using System.IO;
    using System.Xml;

    public class ResolveNativeReference : TaskExtension
    {
        private string[] additionalSearchPaths = new string[0];
        private ITaskItem[] containedComComponents;
        private ITaskItem[] containedLooseEtcFiles;
        private ITaskItem[] containedLooseTlbFiles;
        private ITaskItem[] containedPrerequisiteAssemblies;
        private ITaskItem[] containedTypeLibraries;
        private ITaskItem[] containingReferenceFiles;
        private ITaskItem[] nativeReferences;

        public override bool Execute()
        {
            bool flag = true;
            int index = 0;
            Hashtable containingReferenceFilesTable = new Hashtable(StringComparer.OrdinalIgnoreCase);
            Hashtable containedPrerequisiteAssembliesTable = new Hashtable(StringComparer.OrdinalIgnoreCase);
            Hashtable containedComComponentsTable = new Hashtable(StringComparer.OrdinalIgnoreCase);
            Hashtable containedTypeLibrariesTable = new Hashtable(StringComparer.OrdinalIgnoreCase);
            Hashtable containedLooseTlbFilesTable = new Hashtable(StringComparer.OrdinalIgnoreCase);
            Hashtable containedLooseEtcFilesTable = new Hashtable(StringComparer.OrdinalIgnoreCase);
            for (index = 0; index < this.NativeReferences.GetLength(0); index++)
            {
                ITaskItem item = this.NativeReferences[index];
                string metadata = item.GetMetadata("HintPath");
                if (string.IsNullOrEmpty(metadata) || !File.Exists(metadata))
                {
                    AssemblyIdentity identity = AssemblyIdentity.FromAssemblyName(item.ItemSpec);
                    if (identity != null)
                    {
                        base.Log.LogMessageFromResources(MessageImportance.Low, "ResolveNativeReference.ResolveReference", new object[] { item.ItemSpec });
                        foreach (string str2 in this.AdditionalSearchPaths)
                        {
                            base.Log.LogMessageFromResources(MessageImportance.Low, "ResolveAssemblyReference.FourSpaceIndent", new object[] { str2 });
                        }
                        metadata = identity.Resolve(this.AdditionalSearchPaths);
                    }
                }
                else
                {
                    base.Log.LogMessageFromResources(MessageImportance.Low, "ResolveNativeReference.ResolveReference", new object[] { item.ItemSpec });
                    base.Log.LogMessageFromResources(MessageImportance.Low, "ResolveAssemblyReference.FourSpaceIndent", new object[] { metadata });
                }
                if (!string.IsNullOrEmpty(metadata))
                {
                    if (!this.ExtractFromManifest(this.NativeReferences[index], metadata, containingReferenceFilesTable, containedPrerequisiteAssembliesTable, containedComComponentsTable, containedTypeLibrariesTable, containedLooseTlbFilesTable, containedLooseEtcFilesTable))
                    {
                        flag = false;
                    }
                }
                else
                {
                    base.Log.LogWarningWithCodeFromResources("ResolveNativeReference.FailedToResolveReference", new object[] { item.ItemSpec });
                }
            }
            IComparer comparer = new ItemSpecComparerClass();
            this.containingReferenceFiles = new ITaskItem[containingReferenceFilesTable.Count];
            containingReferenceFilesTable.Values.CopyTo(this.containingReferenceFiles, 0);
            Array.Sort(this.containingReferenceFiles, comparer);
            this.containedPrerequisiteAssemblies = new ITaskItem[containedPrerequisiteAssembliesTable.Count];
            containedPrerequisiteAssembliesTable.Values.CopyTo(this.containedPrerequisiteAssemblies, 0);
            Array.Sort(this.containedPrerequisiteAssemblies, comparer);
            this.containedComComponents = new ITaskItem[containedComComponentsTable.Count];
            containedComComponentsTable.Values.CopyTo(this.containedComComponents, 0);
            Array.Sort(this.containedComComponents, comparer);
            this.containedTypeLibraries = new ITaskItem[containedTypeLibrariesTable.Count];
            containedTypeLibrariesTable.Values.CopyTo(this.containedTypeLibraries, 0);
            Array.Sort(this.containedTypeLibraries, comparer);
            this.containedLooseTlbFiles = new ITaskItem[containedLooseTlbFilesTable.Count];
            containedLooseTlbFilesTable.Values.CopyTo(this.containedLooseTlbFiles, 0);
            Array.Sort(this.containedLooseTlbFiles, comparer);
            this.containedLooseEtcFiles = new ITaskItem[containedLooseEtcFilesTable.Count];
            containedLooseEtcFilesTable.Values.CopyTo(this.containedLooseEtcFiles, 0);
            Array.Sort(this.containedLooseEtcFiles, comparer);
            return flag;
        }

        internal bool ExtractFromManifest(ITaskItem taskItem, string path, Hashtable containingReferenceFilesTable, Hashtable containedPrerequisiteAssembliesTable, Hashtable containedComComponentsTable, Hashtable containedTypeLibrariesTable, Hashtable containedLooseTlbFilesTable, Hashtable containedLooseEtcFilesTable)
        {
            base.Log.LogMessageFromResources(MessageImportance.Low, "ResolveNativeReference.Comment", new object[] { path });
            Manifest manifest = null;
            try
            {
                manifest = ManifestReader.ReadManifest(path, false);
            }
            catch (XmlException exception)
            {
                base.Log.LogErrorWithCodeFromResources("GenerateManifest.ReadInputManifestFailed", new object[] { path, exception.Message });
                return false;
            }
            if (manifest != null)
            {
                manifest.TreatUnfoundNativeAssembliesAsPrerequisites = true;
                manifest.ReadOnly = true;
                manifest.ResolveFiles();
                if (!manifest.OutputMessages.LogTaskMessages(this))
                {
                    return false;
                }
                ApplicationManifest manifest2 = manifest as ApplicationManifest;
                bool flag = (manifest2 != null) && manifest2.IsClickOnceManifest;
                if (!containingReferenceFilesTable.ContainsKey(path) && !flag)
                {
                    ITaskItem destinationItem = new TaskItem {
                        ItemSpec = path
                    };
                    if (manifest.AssemblyIdentity.Name != null)
                    {
                        destinationItem.SetMetadata("FusionName", manifest.AssemblyIdentity.Name);
                    }
                    if (taskItem != null)
                    {
                        taskItem.CopyMetadataTo(destinationItem);
                    }
                    containingReferenceFilesTable.Add(path, destinationItem);
                }
                if (manifest.AssemblyReferences != null)
                {
                    foreach (AssemblyReference reference in manifest.AssemblyReferences)
                    {
                        if (!reference.IsVirtual)
                        {
                            if (!reference.IsPrerequisite)
                            {
                                this.ExtractFromManifest(null, reference.ResolvedPath, containingReferenceFilesTable, containedPrerequisiteAssembliesTable, containedComComponentsTable, containedTypeLibrariesTable, containedLooseTlbFilesTable, containedLooseEtcFilesTable);
                            }
                            else
                            {
                                string fullName = reference.AssemblyIdentity.GetFullName(AssemblyIdentity.FullNameFlags.All);
                                if (!containedPrerequisiteAssembliesTable.ContainsKey(fullName))
                                {
                                    ITaskItem item2 = new TaskItem {
                                        ItemSpec = fullName
                                    };
                                    item2.SetMetadata("DependencyType", "Prerequisite");
                                    containedPrerequisiteAssembliesTable.Add(fullName, item2);
                                }
                            }
                        }
                    }
                }
                if (manifest.FileReferences != null)
                {
                    foreach (FileReference reference2 in manifest.FileReferences)
                    {
                        if (reference2.ResolvedPath != null)
                        {
                            if (!containedLooseEtcFilesTable.ContainsKey(reference2.ResolvedPath))
                            {
                                ITaskItem item3 = new TaskItem {
                                    ItemSpec = reference2.ResolvedPath
                                };
                                item3.SetMetadata("ParentFile", Path.GetFileName(path));
                                containedLooseEtcFilesTable.Add(reference2.ResolvedPath, item3);
                            }
                            if (reference2.ComClasses != null)
                            {
                                foreach (ComClass class2 in reference2.ComClasses)
                                {
                                    if (!containedComComponentsTable.ContainsKey(class2.ClsId))
                                    {
                                        ITaskItem item4 = new TaskItem {
                                            ItemSpec = class2.ClsId
                                        };
                                        containedComComponentsTable.Add(class2.ClsId, item4);
                                    }
                                }
                            }
                            if (reference2.TypeLibs != null)
                            {
                                foreach (TypeLib lib in reference2.TypeLibs)
                                {
                                    if (!containedTypeLibrariesTable.ContainsKey(lib.TlbId))
                                    {
                                        ITaskItem item5 = new TaskItem {
                                            ItemSpec = lib.TlbId
                                        };
                                        item5.SetMetadata("WrapperTool", "tlbimp");
                                        item5.SetMetadata("Guid", lib.TlbId);
                                        item5.SetMetadata("Lcid", "0");
                                        char[] separator = ".".ToCharArray();
                                        string[] strArray = null;
                                        strArray = lib.Version.Split(separator);
                                        item5.SetMetadata("VersionMajor", strArray[0]);
                                        item5.SetMetadata("VersionMinor", strArray[1]);
                                        containedTypeLibrariesTable.Add(lib.TlbId, item5);
                                    }
                                }
                                if (!containedLooseTlbFilesTable.Contains(reference2.ResolvedPath))
                                {
                                    ITaskItem item6 = new TaskItem {
                                        ItemSpec = reference2.ResolvedPath
                                    };
                                    containedLooseTlbFilesTable.Add(reference2.ResolvedPath, item6);
                                }
                            }
                        }
                    }
                }
            }
            return true;
        }

        [Required]
        public string[] AdditionalSearchPaths
        {
            get
            {
                Microsoft.Build.Shared.ErrorUtilities.VerifyThrowArgumentNull(this.additionalSearchPaths, "additionalSearchPaths");
                return this.additionalSearchPaths;
            }
            set
            {
                this.additionalSearchPaths = value;
            }
        }

        [Output]
        public ITaskItem[] ContainedComComponents
        {
            get
            {
                return this.containedComComponents;
            }
            set
            {
                this.containedComComponents = value;
            }
        }

        [Output]
        public ITaskItem[] ContainedLooseEtcFiles
        {
            get
            {
                return this.containedLooseEtcFiles;
            }
            set
            {
                this.containedLooseEtcFiles = value;
            }
        }

        [Output]
        public ITaskItem[] ContainedLooseTlbFiles
        {
            get
            {
                return this.containedLooseTlbFiles;
            }
            set
            {
                this.containedLooseTlbFiles = value;
            }
        }

        [Output]
        public ITaskItem[] ContainedPrerequisiteAssemblies
        {
            get
            {
                return this.containedPrerequisiteAssemblies;
            }
            set
            {
                this.containedPrerequisiteAssemblies = value;
            }
        }

        [Output]
        public ITaskItem[] ContainedTypeLibraries
        {
            get
            {
                return this.containedTypeLibraries;
            }
            set
            {
                this.containedTypeLibraries = value;
            }
        }

        [Output]
        public ITaskItem[] ContainingReferenceFiles
        {
            get
            {
                return this.containingReferenceFiles;
            }
            set
            {
                this.containingReferenceFiles = value;
            }
        }

        [Required]
        public ITaskItem[] NativeReferences
        {
            get
            {
                Microsoft.Build.Shared.ErrorUtilities.VerifyThrowArgumentNull(this.nativeReferences, "nativeReferences");
                return this.nativeReferences;
            }
            set
            {
                this.nativeReferences = value;
            }
        }

        private class ItemSpecComparerClass : IComparer
        {
            int IComparer.Compare(object taskItem1, object taskItem2)
            {
                return string.Compare(((ITaskItem) taskItem1).ItemSpec, ((ITaskItem) taskItem2).ItemSpec, StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}

