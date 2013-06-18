namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Tasks.Deployment.ManifestUtilities;
    using Microsoft.Build.Utilities;
    using System;
    using System.Globalization;
    using System.IO;

    public abstract class GenerateManifestBase : Task
    {
        private string assemblyName;
        private string assemblyVersion;
        private string description;
        private ITaskItem entryPoint;
        private ITaskItem inputManifest;
        private Manifest manifest;
        private int maxTargetPath;
        private ITaskItem outputManifest;
        private string platform;
        private string processorArchitecture;
        private int startTime;
        private string targetCulture;
        private string targetFrameworkMoniker;
        private string targetFrameworkVersion;

        protected GenerateManifestBase() : base(AssemblyResources.PrimaryResources, "MSBuild.")
        {
            this.targetFrameworkVersion = "v2.0";
        }

        protected internal AssemblyReference AddAssemblyFromItem(ITaskItem item)
        {
            AssemblyReferenceType managedAssembly;
            AssemblyReference reference;
            if (this.IsEmbedInteropEnabledForAssembly(item))
            {
                return null;
            }
            switch (this.GetItemAssemblyType(item))
            {
                case AssemblyType.Managed:
                    managedAssembly = AssemblyReferenceType.ManagedAssembly;
                    break;

                case AssemblyType.Native:
                    managedAssembly = AssemblyReferenceType.NativeAssembly;
                    break;

                case AssemblyType.Satellite:
                    managedAssembly = AssemblyReferenceType.ManagedAssembly;
                    break;

                default:
                    managedAssembly = AssemblyReferenceType.Unspecified;
                    break;
            }
            if (this.GetItemDependencyType(item) == DependencyType.Install)
            {
                reference = this.manifest.AssemblyReferences.Add(item.ItemSpec);
                this.SetItemAttributes(item, reference);
            }
            else
            {
                AssemblyIdentity identity = AssemblyIdentity.FromAssemblyName(item.ItemSpec);
                if (identity.IsStrongName)
                {
                    reference = new AssemblyReference {
                        AssemblyIdentity = identity
                    };
                }
                else
                {
                    reference = new AssemblyReference(item.ItemSpec);
                }
                this.manifest.AssemblyReferences.Add(reference);
                reference.IsPrerequisite = true;
            }
            reference.ReferenceType = managedAssembly;
            if (string.Equals(item.GetMetadata("IsPrimary"), "true", StringComparison.Ordinal))
            {
                reference.IsPrimary = true;
            }
            return reference;
        }

        protected internal AssemblyReference AddAssemblyNameFromItem(ITaskItem item, AssemblyReferenceType referenceType)
        {
            AssemblyReference assembly = new AssemblyReference {
                AssemblyIdentity = AssemblyIdentity.FromAssemblyName(item.ItemSpec),
                ReferenceType = referenceType
            };
            this.manifest.AssemblyReferences.Add(assembly);
            string metadata = item.GetMetadata("HintPath");
            if (!string.IsNullOrEmpty(metadata))
            {
                assembly.SourcePath = metadata;
            }
            this.SetItemAttributes(item, assembly);
            return assembly;
        }

        protected internal AssemblyReference AddEntryPointFromItem(ITaskItem item, AssemblyReferenceType referenceType)
        {
            AssemblyReference file = this.manifest.AssemblyReferences.Add(item.ItemSpec);
            file.ReferenceType = referenceType;
            this.SetItemAttributes(item, file);
            return file;
        }

        protected internal FileReference AddFileFromItem(ITaskItem item)
        {
            FileReference file = this.manifest.FileReferences.Add(item.ItemSpec);
            this.SetItemAttributes(item, file);
            file.IsDataFile = ConvertUtil.ToBoolean(item.GetMetadata("IsDataFile"));
            return file;
        }

        private bool BuildManifest()
        {
            if (!this.OnManifestLoaded(this.manifest))
            {
                return false;
            }
            if (!this.ResolveFiles())
            {
                return false;
            }
            if (!this.ResolveIdentity())
            {
                return false;
            }
            this.manifest.SourcePath = this.GetOutputPath();
            if (!this.OnManifestResolved(this.manifest))
            {
                return false;
            }
            return this.WriteManifest();
        }

        private AssemblyIdentity CreateAssemblyIdentity(AssemblyIdentity baseIdentity, AssemblyIdentity entryPointIdentity)
        {
            string assemblyName = this.assemblyName;
            string assemblyVersion = this.assemblyVersion;
            string publicKeyToken = "0000000000000000";
            string targetCulture = this.targetCulture;
            if (string.IsNullOrEmpty(assemblyName))
            {
                if ((baseIdentity != null) && !string.IsNullOrEmpty(baseIdentity.Name))
                {
                    assemblyName = baseIdentity.Name;
                }
                else if ((entryPointIdentity != null) && !string.IsNullOrEmpty(entryPointIdentity.Name))
                {
                    if (this.manifest is DeployManifest)
                    {
                        assemblyName = Path.GetFileNameWithoutExtension(entryPointIdentity.Name) + ".application";
                    }
                    else if (this.manifest is ApplicationManifest)
                    {
                        assemblyName = entryPointIdentity.Name + ".exe";
                    }
                }
            }
            if (string.IsNullOrEmpty(assemblyName))
            {
                base.Log.LogErrorWithCodeFromResources("GenerateManifest.NoIdentity", new object[0]);
                return null;
            }
            if (string.IsNullOrEmpty(assemblyVersion))
            {
                if ((baseIdentity != null) && !string.IsNullOrEmpty(baseIdentity.Version))
                {
                    assemblyVersion = baseIdentity.Version;
                }
                else if ((entryPointIdentity != null) && !string.IsNullOrEmpty(entryPointIdentity.Version))
                {
                    assemblyVersion = entryPointIdentity.Version;
                }
            }
            if (string.IsNullOrEmpty(assemblyVersion))
            {
                assemblyVersion = "1.0.0.0";
            }
            if (string.IsNullOrEmpty(targetCulture))
            {
                if ((baseIdentity != null) && !string.IsNullOrEmpty(baseIdentity.Culture))
                {
                    targetCulture = baseIdentity.Culture;
                }
                else if ((entryPointIdentity != null) && !string.IsNullOrEmpty(entryPointIdentity.Culture))
                {
                    targetCulture = entryPointIdentity.Culture;
                }
            }
            if ((string.IsNullOrEmpty(targetCulture) || string.Equals(targetCulture, "neutral", StringComparison.OrdinalIgnoreCase)) || string.Equals(targetCulture, "*", StringComparison.OrdinalIgnoreCase))
            {
                targetCulture = "neutral";
            }
            if (string.IsNullOrEmpty(this.processorArchitecture))
            {
                if ((baseIdentity != null) && !string.IsNullOrEmpty(baseIdentity.ProcessorArchitecture))
                {
                    this.processorArchitecture = baseIdentity.ProcessorArchitecture;
                }
                else if ((entryPointIdentity != null) && !string.IsNullOrEmpty(entryPointIdentity.ProcessorArchitecture))
                {
                    this.processorArchitecture = entryPointIdentity.ProcessorArchitecture;
                }
            }
            if (string.IsNullOrEmpty(this.processorArchitecture))
            {
                this.processorArchitecture = "msil";
            }
            if (this.manifest is ApplicationManifest)
            {
                ApplicationManifest manifest = this.manifest as ApplicationManifest;
                if (!manifest.IsClickOnceManifest)
                {
                    publicKeyToken = null;
                    if (string.Compare(targetCulture, "neutral", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        targetCulture = null;
                    }
                    if (string.Compare(this.processorArchitecture, "msil", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        this.processorArchitecture = null;
                    }
                }
            }
            return new AssemblyIdentity(assemblyName, assemblyVersion, publicKeyToken, targetCulture, this.processorArchitecture);
        }

        public override bool Execute()
        {
            bool flag = true;
            Type objectType = this.GetObjectType();
            if (!this.InitializeManifest(objectType))
            {
                flag = false;
            }
            if (flag && !this.BuildManifest())
            {
                flag = false;
            }
            if (this.manifest != null)
            {
                this.manifest.OutputMessages.LogTaskMessages(this);
                if (this.manifest.OutputMessages.ErrorCount > 0)
                {
                    flag = false;
                }
            }
            return flag;
        }

        protected internal FileReference FindFileFromItem(ITaskItem item)
        {
            string metadata = item.GetMetadata("TargetPath");
            if (string.IsNullOrEmpty(metadata))
            {
                metadata = BaseReference.GetDefaultTargetPath(item.ItemSpec);
            }
            foreach (FileReference reference in this.manifest.FileReferences)
            {
                if (string.Compare(metadata, reference.TargetPath, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return reference;
                }
            }
            return this.AddFileFromItem(item);
        }

        private string GetDefaultFileName()
        {
            if (this.manifest is DeployManifest)
            {
                return this.manifest.AssemblyIdentity.Name;
            }
            return (this.manifest.AssemblyIdentity.Name + ".manifest");
        }

        private AssemblyType GetItemAssemblyType(ITaskItem item)
        {
            string metadata = item.GetMetadata("AssemblyType");
            if (!string.IsNullOrEmpty(metadata))
            {
                try
                {
                    return (AssemblyType) Enum.Parse(typeof(AssemblyType), metadata, true);
                }
                catch (FormatException)
                {
                    base.Log.LogWarningWithCodeFromResources("GenerateManifest.InvalidItemValue", new object[] { "AssemblyType", item.ItemSpec });
                }
                catch (ArgumentException)
                {
                    base.Log.LogWarningWithCodeFromResources("GenerateManifest.InvalidItemValue", new object[] { "AssemblyType", item.ItemSpec });
                }
            }
            return AssemblyType.Unspecified;
        }

        private DependencyType GetItemDependencyType(ITaskItem item)
        {
            string metadata = item.GetMetadata("DependencyType");
            if (!string.IsNullOrEmpty(metadata))
            {
                try
                {
                    return (DependencyType) Enum.Parse(typeof(DependencyType), metadata, true);
                }
                catch (FormatException)
                {
                    base.Log.LogWarningWithCodeFromResources("GenerateManifest.InvalidItemValue", new object[] { "DependencyType", item.ItemSpec });
                }
                catch (ArgumentException)
                {
                    base.Log.LogWarningWithCodeFromResources("GenerateManifest.InvalidItemValue", new object[] { "DependencyType", item.ItemSpec });
                }
            }
            return DependencyType.Install;
        }

        protected abstract Type GetObjectType();
        private string GetOutputPath()
        {
            if (this.OutputManifest != null)
            {
                return this.OutputManifest.ItemSpec;
            }
            return this.GetDefaultFileName();
        }

        private bool InitializeManifest(Type manifestType)
        {
            this.startTime = Environment.TickCount;
            if (!this.ValidateInputs())
            {
                return false;
            }
            if (manifestType == null)
            {
                throw new ArgumentNullException("manifestType");
            }
            if ((this.InputManifest == null) || string.IsNullOrEmpty(this.InputManifest.ItemSpec))
            {
                if (manifestType != typeof(ApplicationManifest))
                {
                    if (manifestType != typeof(DeployManifest))
                    {
                        throw new ArgumentException(string.Empty, "manifestType");
                    }
                    this.manifest = new DeployManifest(this.TargetFrameworkMoniker);
                }
                else
                {
                    this.manifest = new ApplicationManifest(this.TargetFrameworkVersion);
                }
            }
            else
            {
                try
                {
                    this.manifest = ManifestReader.ReadManifest(manifestType.Name, this.InputManifest.ItemSpec, true);
                }
                catch (Exception exception)
                {
                    base.Log.LogErrorWithCodeFromResources("GenerateManifest.ReadInputManifestFailed", new object[] { this.InputManifest.ItemSpec, exception.Message });
                    return false;
                }
            }
            if (manifestType != this.manifest.GetType())
            {
                base.Log.LogErrorWithCodeFromResources("GenerateManifest.InvalidInputManifest", new object[0]);
                return false;
            }
            if (this.manifest is DeployManifest)
            {
                DeployManifest manifest = this.manifest as DeployManifest;
                if (string.IsNullOrEmpty(manifest.TargetFrameworkMoniker))
                {
                    manifest.TargetFrameworkMoniker = this.TargetFrameworkMoniker;
                }
            }
            else if (this.manifest is ApplicationManifest)
            {
                ApplicationManifest manifest2 = this.manifest as ApplicationManifest;
                if (string.IsNullOrEmpty(manifest2.TargetFrameworkVersion))
                {
                    manifest2.TargetFrameworkVersion = this.TargetFrameworkVersion;
                }
            }
            if ((this.EntryPoint != null) && !string.IsNullOrEmpty(this.EntryPoint.ItemSpec))
            {
                AssemblyReferenceType unspecified = AssemblyReferenceType.Unspecified;
                if (this.manifest is DeployManifest)
                {
                    unspecified = AssemblyReferenceType.ClickOnceManifest;
                }
                if (this.manifest is ApplicationManifest)
                {
                    unspecified = AssemblyReferenceType.ManagedAssembly;
                }
                this.manifest.EntryPoint = this.AddEntryPointFromItem(this.EntryPoint, unspecified);
            }
            if (this.Description != null)
            {
                this.manifest.Description = this.Description;
            }
            return true;
        }

        private bool IsEmbedInteropEnabledForAssembly(ITaskItem item)
        {
            bool flag;
            bool.TryParse(item.GetMetadata("EmbedInteropTypes"), out flag);
            return flag;
        }

        protected abstract bool OnManifestLoaded(Manifest manifest);
        protected abstract bool OnManifestResolved(Manifest manifest);
        private bool ResolveFiles()
        {
            int tickCount = Environment.TickCount;
            string[] searchPaths = new string[] { Environment.CurrentDirectory };
            this.manifest.ResolveFiles(searchPaths);
            this.manifest.UpdateFileInfo();
            if (this.manifest.OutputMessages.ErrorCount > 0)
            {
                return false;
            }
            Util.WriteLog(string.Format(CultureInfo.CurrentCulture, "GenerateManifestBase.ResolveFiles t={0}", new object[] { Environment.TickCount - tickCount }));
            return true;
        }

        private bool ResolveIdentity()
        {
            AssemblyIdentity entryPointIdentity = (this.manifest.EntryPoint != null) ? this.manifest.EntryPoint.AssemblyIdentity : null;
            this.manifest.AssemblyIdentity = this.CreateAssemblyIdentity(this.manifest.AssemblyIdentity, entryPointIdentity);
            return (this.manifest.AssemblyIdentity != null);
        }

        private void SetItemAttributes(ITaskItem item, BaseReference file)
        {
            string metadata = item.GetMetadata("TargetPath");
            if (!string.IsNullOrEmpty(metadata))
            {
                file.TargetPath = metadata;
            }
            else
            {
                file.TargetPath = (Path.IsPathRooted(file.SourcePath) || file.SourcePath.StartsWith("..", StringComparison.Ordinal)) ? Path.GetFileName(file.SourcePath) : file.SourcePath;
            }
            file.Group = item.GetMetadata("Group");
            file.IsOptional = !string.IsNullOrEmpty(file.Group);
            if (Util.CompareFrameworkVersions(this.TargetFrameworkVersion, "v3.5") >= 0)
            {
                file.IncludeHash = ConvertUtil.ToBoolean(item.GetMetadata("IncludeHash"), true);
            }
        }

        protected internal virtual bool ValidateInputs()
        {
            bool flag = true;
            if (!string.IsNullOrEmpty(this.assemblyName) && !Util.IsValidAssemblyName(this.assemblyName))
            {
                base.Log.LogErrorWithCodeFromResources("GenerateManifest.InvalidValue", new object[] { "AssemblyName" });
                flag = false;
            }
            if (!string.IsNullOrEmpty(this.assemblyVersion) && !Util.IsValidVersion(this.assemblyVersion, 4))
            {
                base.Log.LogErrorWithCodeFromResources("GenerateManifest.InvalidValue", new object[] { "AssemblyVersion" });
                flag = false;
            }
            if (!string.IsNullOrEmpty(this.targetCulture) && !Util.IsValidCulture(this.targetCulture))
            {
                base.Log.LogErrorWithCodeFromResources("GenerateManifest.InvalidValue", new object[] { "TargetCulture" });
                flag = false;
            }
            if (!string.IsNullOrEmpty(this.platform))
            {
                this.processorArchitecture = Util.PlatformToProcessorArchitecture(this.platform);
                if (string.IsNullOrEmpty(this.processorArchitecture))
                {
                    base.Log.LogErrorWithCodeFromResources("GenerateManifest.InvalidValue", new object[] { "Platform" });
                    flag = false;
                }
            }
            return flag;
        }

        protected internal virtual bool ValidateOutput()
        {
            this.manifest.Validate();
            if (this.manifest.OutputMessages.ErrorCount > 0)
            {
                return false;
            }
            if (this.MaxTargetPath > 0)
            {
                string fileName = Path.GetFileName(this.OutputManifest.ItemSpec);
                if (fileName.Length > this.MaxTargetPath)
                {
                    base.Log.LogWarningWithCodeFromResources("GenerateManifest.TargetPathTooLong", new object[] { fileName, this.MaxTargetPath });
                }
            }
            return true;
        }

        private bool WriteManifest()
        {
            if (this.OutputManifest == null)
            {
                this.OutputManifest = new TaskItem(this.GetDefaultFileName());
            }
            if (!this.ValidateOutput())
            {
                return false;
            }
            int tickCount = Environment.TickCount;
            try
            {
                ManifestWriter.WriteManifest(this.manifest, this.OutputManifest.ItemSpec);
            }
            catch (Exception exception)
            {
                base.Log.LogErrorWithCodeFromResources("GenerateManifest.WriteOutputManifestFailed", new object[] { this.OutputManifest.ItemSpec, exception.Message });
                return false;
            }
            Util.WriteLog(string.Format(CultureInfo.CurrentCulture, "GenerateManifestBase.WriteManifest t={0}", new object[] { Environment.TickCount - tickCount }));
            Util.WriteLog(string.Format(CultureInfo.CurrentCulture, "Total time to generate manifest '{1}': t={0}", new object[] { Environment.TickCount - this.startTime, Path.GetFileName(this.OutputManifest.ItemSpec) }));
            return true;
        }

        public string AssemblyName
        {
            get
            {
                return this.assemblyName;
            }
            set
            {
                this.assemblyName = value;
            }
        }

        public string AssemblyVersion
        {
            get
            {
                return this.assemblyVersion;
            }
            set
            {
                this.assemblyVersion = value;
            }
        }

        public string Description
        {
            get
            {
                return this.description;
            }
            set
            {
                this.description = value;
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

        public ITaskItem InputManifest
        {
            get
            {
                return this.inputManifest;
            }
            set
            {
                this.inputManifest = value;
            }
        }

        public int MaxTargetPath
        {
            get
            {
                return this.maxTargetPath;
            }
            set
            {
                this.maxTargetPath = value;
            }
        }

        [Output]
        public ITaskItem OutputManifest
        {
            get
            {
                return this.outputManifest;
            }
            set
            {
                this.outputManifest = value;
            }
        }

        public string Platform
        {
            get
            {
                return this.platform;
            }
            set
            {
                this.platform = value;
            }
        }

        public string TargetCulture
        {
            get
            {
                return this.targetCulture;
            }
            set
            {
                this.targetCulture = value;
            }
        }

        public string TargetFrameworkMoniker
        {
            get
            {
                return this.targetFrameworkMoniker;
            }
            set
            {
                this.targetFrameworkMoniker = value;
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

        private enum AssemblyType
        {
            Unspecified,
            Managed,
            Native,
            Satellite
        }

        private enum DependencyType
        {
            Install,
            Prerequisite
        }
    }
}

