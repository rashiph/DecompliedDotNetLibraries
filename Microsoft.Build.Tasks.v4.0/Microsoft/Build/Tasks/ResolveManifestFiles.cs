namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Tasks.Deployment.ManifestUtilities;
    using Microsoft.Build.Utilities;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;

    public sealed class ResolveManifestFiles : TaskExtension
    {
        private bool canPublish;
        private ITaskItem deploymentManifestEntryPoint;
        private ITaskItem entryPoint;
        private ITaskItem[] extraFiles;
        private ITaskItem[] files;
        private bool includeAllSatellites;
        private ITaskItem[] managedAssemblies;
        private ITaskItem[] nativeAssemblies;
        private ITaskItem[] outputAssemblies;
        private ITaskItem outputDeploymentManifestEntryPoint;
        private ITaskItem outputEntryPoint;
        private ITaskItem[] outputFiles;
        private ITaskItem[] publishFiles;
        private ITaskItem[] satelliteAssemblies;
        private bool signingManifests;
        private string specifiedTargetCulture;
        private CultureInfo targetCulture;
        private string targetFrameworkVersion;

        private int CompareFrameworkVersions(string versionA, string versionB)
        {
            Version version = this.ConvertFrameworkVersionToString(versionA);
            Version version2 = this.ConvertFrameworkVersionToString(versionB);
            return version.CompareTo(version2);
        }

        private Version ConvertFrameworkVersionToString(string version)
        {
            if (version.StartsWith("v", StringComparison.OrdinalIgnoreCase))
            {
                return new Version(version.Substring(1));
            }
            return new Version(version);
        }

        private static ITaskItem CreateAssemblyItem(ITaskItem item, string group, string targetPath, string includeHash)
        {
            ITaskItem destinationItem = new TaskItem(item.ItemSpec);
            item.CopyMetadataTo(destinationItem);
            destinationItem.SetMetadata("DependencyType", "Install");
            if (string.IsNullOrEmpty(targetPath))
            {
                targetPath = GetItemTargetPath(destinationItem);
            }
            destinationItem.SetMetadata("TargetPath", targetPath);
            if (!string.IsNullOrEmpty(group))
            {
                destinationItem.SetMetadata("Group", group);
            }
            if (!string.IsNullOrEmpty(includeHash))
            {
                destinationItem.SetMetadata("IncludeHash", includeHash);
            }
            return destinationItem;
        }

        private static ITaskItem CreateFileItem(ITaskItem item, string group, string targetPath, string includeHash, bool isDataFile)
        {
            ITaskItem destinationItem = new TaskItem(item.ItemSpec);
            item.CopyMetadataTo(destinationItem);
            if (string.IsNullOrEmpty(targetPath))
            {
                targetPath = GetItemTargetPath(destinationItem);
            }
            destinationItem.SetMetadata("TargetPath", targetPath);
            if (!string.IsNullOrEmpty(group) && !isDataFile)
            {
                destinationItem.SetMetadata("Group", group);
            }
            if (!string.IsNullOrEmpty(includeHash))
            {
                destinationItem.SetMetadata("IncludeHash", includeHash);
            }
            destinationItem.SetMetadata("IsDataFile", isDataFile.ToString().ToLowerInvariant());
            return destinationItem;
        }

        private static ITaskItem CreatePrerequisiteItem(ITaskItem item)
        {
            ITaskItem destinationItem = new TaskItem(item.ItemSpec);
            item.CopyMetadataTo(destinationItem);
            destinationItem.SetMetadata("DependencyType", "Prerequisite");
            return destinationItem;
        }

        public override bool Execute()
        {
            PublishInfo[] infoArray;
            PublishInfo[] infoArray2;
            PublishInfo[] infoArray3;
            PublishInfo[] infoArray4;
            if (!this.ValidateInputs())
            {
                return false;
            }
            this.canPublish = true;
            bool flag = this.CompareFrameworkVersions(this.TargetFrameworkVersion, "v3.5") >= 0;
            this.GetPublishInfo(out infoArray, out infoArray2, out infoArray3, out infoArray4);
            this.outputAssemblies = this.GetOutputAssembliesAndSatellites(infoArray, infoArray3);
            if (!this.canPublish && flag)
            {
                base.Log.LogErrorWithCodeFromResources("GenerateManifest.ManifestsSignedHashExcluded", new object[0]);
                return false;
            }
            this.outputFiles = this.GetOutputFiles(infoArray2);
            if (!this.canPublish && flag)
            {
                base.Log.LogErrorWithCodeFromResources("GenerateManifest.ManifestsSignedHashExcluded", new object[0]);
                return false;
            }
            this.outputEntryPoint = this.GetOutputEntryPoint(this.entryPoint, infoArray4);
            if (!this.canPublish && flag)
            {
                base.Log.LogErrorWithCodeFromResources("GenerateManifest.ManifestsSignedHashExcluded", new object[0]);
                return false;
            }
            this.outputDeploymentManifestEntryPoint = this.GetOutputEntryPoint(this.deploymentManifestEntryPoint, infoArray4);
            if (!this.canPublish && flag)
            {
                base.Log.LogErrorWithCodeFromResources("GenerateManifest.ManifestsSignedHashExcluded", new object[0]);
                return false;
            }
            return true;
        }

        private static bool GetItemCopyLocal(ITaskItem item)
        {
            string metadata = item.GetMetadata("CopyLocal");
            if (!string.IsNullOrEmpty(metadata))
            {
                return ConvertUtil.ToBoolean(metadata);
            }
            return true;
        }

        private static CultureInfo GetItemCulture(ITaskItem item)
        {
            string metadata = item.GetMetadata("Culture");
            if (string.IsNullOrEmpty(metadata))
            {
                string[] pathSegments = PathUtil.GetPathSegments(item.ItemSpec);
                metadata = (pathSegments.Length > 1) ? pathSegments[pathSegments.Length - 2] : null;
                item.SetMetadata("Culture", metadata);
            }
            return new CultureInfo(metadata);
        }

        private static string GetItemTargetPath(ITaskItem item)
        {
            string metadata = item.GetMetadata("TargetPath");
            if (string.IsNullOrEmpty(metadata))
            {
                metadata = Path.GetFileName(item.ItemSpec);
                if (string.Equals(item.GetMetadata("AssemblyType"), "Satellite", StringComparison.Ordinal))
                {
                    CultureInfo itemCulture = GetItemCulture(item);
                    if (itemCulture != null)
                    {
                        metadata = Path.Combine(itemCulture.ToString(), metadata);
                    }
                }
            }
            return metadata;
        }

        private void GetOutputAssemblies(PublishInfo[] publishInfos, ref List<ITaskItem> assemblyList)
        {
            AssemblyMap map = new AssemblyMap();
            if (this.managedAssemblies != null)
            {
                foreach (ITaskItem item in this.managedAssemblies)
                {
                    if (!IsFiltered(item))
                    {
                        item.SetMetadata("AssemblyType", "Managed");
                        map.Add(item);
                    }
                }
            }
            if (this.nativeAssemblies != null)
            {
                foreach (ITaskItem item2 in this.nativeAssemblies)
                {
                    if (!IsFiltered(item2))
                    {
                        item2.SetMetadata("AssemblyType", "Native");
                        map.Add(item2);
                    }
                }
            }
            foreach (PublishInfo info in publishInfos)
            {
                MapEntry entry = map[info.key];
                if (entry != null)
                {
                    entry.publishInfo = info;
                }
                else
                {
                    base.Log.LogWarningWithCodeFromResources("ResolveManifestFiles.PublishFileNotFound", new object[] { info.key });
                }
            }
            foreach (MapEntry entry2 in (IEnumerable) map)
            {
                if (entry2.publishInfo == null)
                {
                    entry2.publishInfo = new PublishInfo();
                }
                if (entry2.publishInfo.state == PublishState.Auto)
                {
                    string metadata = entry2.item.GetMetadata("DependencyType");
                    if (string.Equals(metadata, "Prerequisite", StringComparison.Ordinal))
                    {
                        entry2.publishInfo.state = PublishState.Prerequisite;
                    }
                    else if (string.Equals(metadata, "Install", StringComparison.Ordinal))
                    {
                        entry2.publishInfo.state = PublishState.Include;
                    }
                }
                bool itemCopyLocal = GetItemCopyLocal(entry2.item);
                PublishFlags assemblyFlags = PublishFlags.GetAssemblyFlags(entry2.publishInfo.state, itemCopyLocal);
                if ((assemblyFlags.IsPublished && string.Equals(entry2.publishInfo.includeHash, "false", StringComparison.OrdinalIgnoreCase)) && this.SigningManifests)
                {
                    this.canPublish = false;
                }
                if (assemblyFlags.IsPublished)
                {
                    assemblyList.Add(CreateAssemblyItem(entry2.item, entry2.publishInfo.group, entry2.publishInfo.targetPath, entry2.publishInfo.includeHash));
                }
                else if (assemblyFlags.IsPrerequisite)
                {
                    assemblyList.Add(CreatePrerequisiteItem(entry2.item));
                }
            }
        }

        private ITaskItem[] GetOutputAssembliesAndSatellites(PublishInfo[] assemblyPublishInfos, PublishInfo[] satellitePublishInfos)
        {
            List<ITaskItem> assemblyList = new List<ITaskItem>();
            this.GetOutputAssemblies(assemblyPublishInfos, ref assemblyList);
            this.GetOutputSatellites(satellitePublishInfos, ref assemblyList);
            return assemblyList.ToArray();
        }

        private ITaskItem GetOutputEntryPoint(ITaskItem entryPoint, PublishInfo[] manifestEntryPointList)
        {
            if (entryPoint == null)
            {
                return null;
            }
            TaskItem destinationItem = new TaskItem(entryPoint.ItemSpec);
            entryPoint.CopyMetadataTo(destinationItem);
            string metadata = entryPoint.GetMetadata("TargetPath");
            if (!string.IsNullOrEmpty(metadata))
            {
                for (int i = 0; i < manifestEntryPointList.Length; i++)
                {
                    if (string.Equals(metadata, manifestEntryPointList[i].key, StringComparison.OrdinalIgnoreCase))
                    {
                        if (!string.IsNullOrEmpty(manifestEntryPointList[i].includeHash))
                        {
                            if (((manifestEntryPointList[i].state != PublishState.Exclude) && string.Equals(manifestEntryPointList[i].includeHash, "false", StringComparison.OrdinalIgnoreCase)) && this.SigningManifests)
                            {
                                this.canPublish = false;
                            }
                            destinationItem.SetMetadata("IncludeHash", manifestEntryPointList[i].includeHash);
                        }
                        return destinationItem;
                    }
                }
            }
            return destinationItem;
        }

        private ITaskItem[] GetOutputFiles(PublishInfo[] publishInfos)
        {
            List<ITaskItem> list = new List<ITaskItem>();
            FileMap map = new FileMap();
            if (this.Files != null)
            {
                foreach (ITaskItem item in this.Files)
                {
                    map.Add(item, true);
                }
            }
            if (this.ExtraFiles != null)
            {
                foreach (ITaskItem item2 in this.ExtraFiles)
                {
                    map.Add(item2, false);
                }
            }
            foreach (PublishInfo info in publishInfos)
            {
                MapEntry entry = map[info.key];
                if (entry != null)
                {
                    entry.publishInfo = info;
                }
                else
                {
                    base.Log.LogWarningWithCodeFromResources("ResolveManifestFiles.PublishFileNotFound", new object[] { info.key });
                }
            }
            foreach (MapEntry entry2 in (IEnumerable) map)
            {
                if (entry2.publishInfo == null)
                {
                    entry2.publishInfo = new PublishInfo();
                }
                string extension = Path.GetExtension(entry2.item.ItemSpec);
                PublishFlags flags = PublishFlags.GetFileFlags(entry2.publishInfo.state, extension, entry2.includedByDefault);
                if ((flags.IsPublished && string.Equals(entry2.publishInfo.includeHash, "false", StringComparison.OrdinalIgnoreCase)) && this.SigningManifests)
                {
                    this.canPublish = false;
                }
                if (flags.IsPublished)
                {
                    list.Add(CreateFileItem(entry2.item, entry2.publishInfo.group, entry2.publishInfo.targetPath, entry2.publishInfo.includeHash, flags.IsDataFile));
                }
            }
            return list.ToArray();
        }

        private void GetOutputSatellites(PublishInfo[] publishInfos, ref List<ITaskItem> assemblyList)
        {
            FileMap map = new FileMap();
            if (this.satelliteAssemblies != null)
            {
                foreach (ITaskItem item in this.satelliteAssemblies)
                {
                    item.SetMetadata("AssemblyType", "Satellite");
                    map.Add(item, true);
                }
            }
            foreach (PublishInfo info in publishInfos)
            {
                string str = info.key + ".dll";
                MapEntry entry = map[str];
                if (entry != null)
                {
                    entry.publishInfo = info;
                }
                else
                {
                    base.Log.LogWarningWithCodeFromResources("ResolveManifestFiles.PublishFileNotFound", new object[] { info.key });
                }
            }
            foreach (MapEntry entry2 in (IEnumerable) map)
            {
                if (entry2.publishInfo == null)
                {
                    entry2.publishInfo = new PublishInfo();
                }
                CultureInfo itemCulture = GetItemCulture(entry2.item);
                PublishFlags flags = PublishFlags.GetSatelliteFlags(entry2.publishInfo.state, itemCulture, this.targetCulture, this.includeAllSatellites);
                if ((flags.IsPublished && string.Equals(entry2.publishInfo.includeHash, "false", StringComparison.OrdinalIgnoreCase)) && this.SigningManifests)
                {
                    this.canPublish = false;
                }
                if (flags.IsPublished)
                {
                    assemblyList.Add(CreateAssemblyItem(entry2.item, entry2.publishInfo.group, entry2.publishInfo.targetPath, entry2.publishInfo.includeHash));
                }
                else if (flags.IsPrerequisite)
                {
                    assemblyList.Add(CreatePrerequisiteItem(entry2.item));
                }
            }
        }

        private void GetPublishInfo(out PublishInfo[] assemblyPublishInfos, out PublishInfo[] filePublishInfos, out PublishInfo[] satellitePublishInfos, out PublishInfo[] manifestEntryPointPublishInfos)
        {
            List<PublishInfo> list = new List<PublishInfo>();
            List<PublishInfo> list2 = new List<PublishInfo>();
            List<PublishInfo> list3 = new List<PublishInfo>();
            List<PublishInfo> list4 = new List<PublishInfo>();
            if (this.PublishFiles != null)
            {
                foreach (ITaskItem item in this.PublishFiles)
                {
                    PublishInfo info = new PublishInfo(item);
                    string metadata = item.GetMetadata("FileType");
                    if (metadata == null)
                    {
                        goto Label_00BA;
                    }
                    if (!(metadata == "Assembly"))
                    {
                        if (metadata == "File")
                        {
                            goto Label_009C;
                        }
                        if (metadata == "Satellite")
                        {
                            goto Label_00A6;
                        }
                        if (metadata == "ManifestEntryPoint")
                        {
                            goto Label_00B0;
                        }
                        goto Label_00BA;
                    }
                    list.Add(info);
                    continue;
                Label_009C:
                    list2.Add(info);
                    continue;
                Label_00A6:
                    list3.Add(info);
                    continue;
                Label_00B0:
                    list4.Add(info);
                    continue;
                Label_00BA:;
                    base.Log.LogWarningWithCodeFromResources("GenerateManifest.InvalidItemValue", new object[] { "FileType", item.ItemSpec });
                }
            }
            assemblyPublishInfos = list.ToArray();
            filePublishInfos = list2.ToArray();
            satellitePublishInfos = list3.ToArray();
            manifestEntryPointPublishInfos = list4.ToArray();
        }

        private static bool IsFiltered(ITaskItem item)
        {
            bool flag;
            AssemblyIdentity identity = AssemblyIdentity.FromManagedAssembly(item.ItemSpec);
            if ((identity != null) && identity.IsFrameworkAssembly)
            {
                return true;
            }
            string metadata = item.GetMetadata("IsRedistRoot");
            return ((!string.IsNullOrEmpty(metadata) && bool.TryParse(metadata, out flag)) && !flag);
        }

        private static PublishState StringToPublishState(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                try
                {
                    return (PublishState) Enum.Parse(typeof(PublishState), value, false);
                }
                catch (FormatException)
                {
                }
                catch (ArgumentException)
                {
                }
            }
            return PublishState.Auto;
        }

        private bool ValidateInputs()
        {
            if (!string.IsNullOrEmpty(this.specifiedTargetCulture))
            {
                if (string.Equals(this.specifiedTargetCulture, "*", StringComparison.Ordinal))
                {
                    this.includeAllSatellites = true;
                }
                else if (!string.Equals(this.specifiedTargetCulture, "neutral", StringComparison.Ordinal))
                {
                    try
                    {
                        this.targetCulture = new CultureInfo(this.specifiedTargetCulture);
                    }
                    catch (ArgumentException)
                    {
                        base.Log.LogErrorWithCodeFromResources("General.InvalidValue", new object[] { "TargetCulture", "ResolveManifestFiles" });
                        return false;
                    }
                }
            }
            return true;
        }

        public ITaskItem DeploymentManifestEntryPoint
        {
            get
            {
                return this.deploymentManifestEntryPoint;
            }
            set
            {
                this.deploymentManifestEntryPoint = value;
            }
        }

        public ITaskItem EntryPoint
        {
            get
            {
                return this.entryPoint;
            }
            set
            {
                this.entryPoint = value;
            }
        }

        public ITaskItem[] ExtraFiles
        {
            get
            {
                return this.extraFiles;
            }
            set
            {
                this.extraFiles = Util.SortItems(value);
            }
        }

        public ITaskItem[] Files
        {
            get
            {
                return this.files;
            }
            set
            {
                this.files = Util.SortItems(value);
            }
        }

        public ITaskItem[] ManagedAssemblies
        {
            get
            {
                return this.managedAssemblies;
            }
            set
            {
                this.managedAssemblies = Util.SortItems(value);
            }
        }

        public ITaskItem[] NativeAssemblies
        {
            get
            {
                return this.nativeAssemblies;
            }
            set
            {
                this.nativeAssemblies = Util.SortItems(value);
            }
        }

        [Output]
        public ITaskItem[] OutputAssemblies
        {
            get
            {
                return this.outputAssemblies;
            }
            set
            {
                this.outputAssemblies = value;
            }
        }

        [Output]
        public ITaskItem OutputDeploymentManifestEntryPoint
        {
            get
            {
                return this.outputDeploymentManifestEntryPoint;
            }
            set
            {
                this.outputDeploymentManifestEntryPoint = value;
            }
        }

        [Output]
        public ITaskItem OutputEntryPoint
        {
            get
            {
                return this.outputEntryPoint;
            }
            set
            {
                this.OutputEntryPoint = value;
            }
        }

        [Output]
        public ITaskItem[] OutputFiles
        {
            get
            {
                return this.outputFiles;
            }
            set
            {
                this.outputFiles = value;
            }
        }

        public ITaskItem[] PublishFiles
        {
            get
            {
                return this.publishFiles;
            }
            set
            {
                this.publishFiles = Util.SortItems(value);
            }
        }

        public ITaskItem[] SatelliteAssemblies
        {
            get
            {
                return this.satelliteAssemblies;
            }
            set
            {
                this.satelliteAssemblies = Util.SortItems(value);
            }
        }

        public bool SigningManifests
        {
            get
            {
                return this.signingManifests;
            }
            set
            {
                this.signingManifests = value;
            }
        }

        public string TargetCulture
        {
            get
            {
                return this.specifiedTargetCulture;
            }
            set
            {
                this.specifiedTargetCulture = value;
            }
        }

        public string TargetFrameworkVersion
        {
            get
            {
                if (string.IsNullOrEmpty(this.targetFrameworkVersion))
                {
                    return "v3.5";
                }
                return this.targetFrameworkVersion;
            }
            set
            {
                this.targetFrameworkVersion = value;
            }
        }

        private class AssemblyMap : IEnumerable
        {
            private readonly Dictionary<string, ResolveManifestFiles.MapEntry> dictionary = new Dictionary<string, ResolveManifestFiles.MapEntry>();
            private readonly Dictionary<string, ResolveManifestFiles.MapEntry> simpleNameDictionary = new Dictionary<string, ResolveManifestFiles.MapEntry>();

            public void Add(ITaskItem item)
            {
                ResolveManifestFiles.MapEntry entry = new ResolveManifestFiles.MapEntry(item, true);
                string metadata = item.GetMetadata("FusionName");
                if (string.IsNullOrEmpty(metadata))
                {
                    metadata = Path.GetFileNameWithoutExtension(item.ItemSpec);
                }
                string key = metadata.ToLowerInvariant();
                if (!this.dictionary.ContainsKey(key))
                {
                    this.dictionary.Add(key, entry);
                }
                int index = metadata.IndexOf(',');
                if (index > 0)
                {
                    key = metadata.Substring(0, index).ToLowerInvariant();
                    if (!this.simpleNameDictionary.ContainsKey(key))
                    {
                        this.simpleNameDictionary.Add(key, entry);
                    }
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.dictionary.Values.GetEnumerator();
            }

            public ResolveManifestFiles.MapEntry this[string fusionName]
            {
                get
                {
                    ResolveManifestFiles.MapEntry entry = null;
                    string key = fusionName.ToLowerInvariant();
                    if (!this.dictionary.TryGetValue(key, out entry))
                    {
                        this.simpleNameDictionary.TryGetValue(key, out entry);
                    }
                    return entry;
                }
            }
        }

        private class FileMap : IEnumerable
        {
            private readonly Dictionary<string, ResolveManifestFiles.MapEntry> dictionary = new Dictionary<string, ResolveManifestFiles.MapEntry>();

            public void Add(ITaskItem item, bool includedByDefault)
            {
                string itemTargetPath = ResolveManifestFiles.GetItemTargetPath(item);
                if (!string.IsNullOrEmpty(itemTargetPath))
                {
                    string key = itemTargetPath.ToLowerInvariant();
                    ResolveManifestFiles.MapEntry entry = new ResolveManifestFiles.MapEntry(item, includedByDefault);
                    if (!this.dictionary.ContainsKey(key))
                    {
                        this.dictionary.Add(key, entry);
                    }
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.dictionary.Values.GetEnumerator();
            }

            public ResolveManifestFiles.MapEntry this[string targetPath]
            {
                get
                {
                    ResolveManifestFiles.MapEntry entry = null;
                    string key = targetPath.ToLowerInvariant();
                    this.dictionary.TryGetValue(key, out entry);
                    return entry;
                }
            }
        }

        private class MapEntry
        {
            public readonly bool includedByDefault;
            public readonly ITaskItem item;
            public ResolveManifestFiles.PublishInfo publishInfo;

            public MapEntry(ITaskItem item, bool includedByDefault)
            {
                this.item = item;
                this.includedByDefault = includedByDefault;
            }
        }

        private class PublishFlags
        {
            private bool isDataFile;
            private bool isPrerequisite;
            private bool isPublished;

            private PublishFlags(bool isDataFile, bool isPrerequisite, bool isPublished)
            {
                this.isDataFile = isDataFile;
                this.isPrerequisite = isPrerequisite;
                this.isPublished = isPublished;
            }

            public static ResolveManifestFiles.PublishFlags GetAssemblyFlags(ResolveManifestFiles.PublishState state, bool copyLocal)
            {
                bool isDataFile = false;
                bool isPrerequisite = false;
                bool isPublished = false;
                switch (state)
                {
                    case ResolveManifestFiles.PublishState.Auto:
                        isPrerequisite = !copyLocal;
                        isPublished = copyLocal;
                        break;

                    case ResolveManifestFiles.PublishState.Include:
                        isPrerequisite = false;
                        isPublished = true;
                        break;

                    case ResolveManifestFiles.PublishState.Exclude:
                        isPrerequisite = false;
                        isPublished = false;
                        break;

                    case ResolveManifestFiles.PublishState.Prerequisite:
                        isPrerequisite = true;
                        isPublished = false;
                        break;
                }
                return new ResolveManifestFiles.PublishFlags(isDataFile, isPrerequisite, isPublished);
            }

            public static ResolveManifestFiles.PublishFlags GetFileFlags(ResolveManifestFiles.PublishState state, string fileExtension, bool includedByDefault)
            {
                bool isDataFile = false;
                bool isPrerequisite = false;
                bool isPublished = false;
                switch (state)
                {
                    case ResolveManifestFiles.PublishState.Auto:
                        isDataFile = includedByDefault && PathUtil.IsDataFile(fileExtension);
                        isPublished = includedByDefault;
                        break;

                    case ResolveManifestFiles.PublishState.Include:
                        isDataFile = false;
                        isPublished = true;
                        break;

                    case ResolveManifestFiles.PublishState.Exclude:
                        isDataFile = false;
                        isPublished = false;
                        break;

                    case ResolveManifestFiles.PublishState.DataFile:
                        isDataFile = true;
                        isPublished = true;
                        break;
                }
                return new ResolveManifestFiles.PublishFlags(isDataFile, isPrerequisite, isPublished);
            }

            public static ResolveManifestFiles.PublishFlags GetSatelliteFlags(ResolveManifestFiles.PublishState state, CultureInfo satelliteCulture, CultureInfo targetCulture, bool includeAllSatellites)
            {
                bool flag = IsSatelliteIncludedByDefault(satelliteCulture, targetCulture, includeAllSatellites);
                bool isDataFile = false;
                bool isPrerequisite = false;
                bool isPublished = false;
                switch (state)
                {
                    case ResolveManifestFiles.PublishState.Auto:
                        isPrerequisite = false;
                        isPublished = flag;
                        break;

                    case ResolveManifestFiles.PublishState.Include:
                        isPrerequisite = false;
                        isPublished = true;
                        break;

                    case ResolveManifestFiles.PublishState.Exclude:
                        isPrerequisite = false;
                        isPublished = false;
                        break;

                    case ResolveManifestFiles.PublishState.Prerequisite:
                        isPrerequisite = true;
                        isPublished = false;
                        break;
                }
                return new ResolveManifestFiles.PublishFlags(isDataFile, isPrerequisite, isPublished);
            }

            private static bool IsSatelliteIncludedByDefault(CultureInfo satelliteCulture, CultureInfo targetCulture, bool includeAllSatellites)
            {
                if (targetCulture == null)
                {
                    return includeAllSatellites;
                }
                return (targetCulture.Equals(satelliteCulture) || ((!targetCulture.IsNeutralCulture && targetCulture.Parent.Equals(satelliteCulture)) || includeAllSatellites));
            }

            public bool IsDataFile
            {
                get
                {
                    return this.isDataFile;
                }
            }

            public bool IsPrerequisite
            {
                get
                {
                    return this.isPrerequisite;
                }
            }

            public bool IsPublished
            {
                get
                {
                    return this.isPublished;
                }
            }
        }

        private class PublishInfo
        {
            public readonly string group;
            public readonly string includeHash;
            public readonly string key;
            public ResolveManifestFiles.PublishState state;
            public readonly string targetPath;

            public PublishInfo()
            {
            }

            public PublishInfo(ITaskItem item)
            {
                this.key = (item.ItemSpec != null) ? item.ItemSpec.ToLowerInvariant() : null;
                this.group = item.GetMetadata("Group");
                this.state = ResolveManifestFiles.StringToPublishState(item.GetMetadata("PublishState"));
                this.includeHash = item.GetMetadata("IncludeHash");
                this.targetPath = item.GetMetadata("TargetPath");
            }
        }

        private enum PublishState
        {
            Auto,
            Include,
            Exclude,
            DataFile,
            Prerequisite
        }
    }
}

