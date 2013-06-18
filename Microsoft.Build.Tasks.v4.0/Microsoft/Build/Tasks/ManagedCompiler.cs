namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;
    using System;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Security.Permissions;
    using System.Text;

    [StrongNameIdentityPermission(SecurityAction.InheritanceDemand, PublicKey="002400000480000094000000060200000024000052534131000400000100010007d1fa57c4aed9f0a32e84aa0faefd0de9e8fd6aec8f87fb03766c834c99921eb23be79ad9d5dcc1dd9ad236132102900b723cf980957fc4e177108fc607774f29e8320e92ea05ece4e821c0a5efe8f1645c4c0c93c1ab99285d622caa652c1dfad63d745d6f2de5f17e5eaf0fc4963d261c8a12436518206dc093344d5ad293")]
    public abstract class ManagedCompiler : ToolTaskExtension
    {
        private bool hostCompilerSupportsAllParameters;

        protected ManagedCompiler()
        {
        }

        protected internal override void AddCommandLineCommands(CommandLineBuilderExtension commandLine)
        {
            commandLine.AppendWhenTrue("/noconfig", base.Bag, "NoConfig");
        }

        protected internal override void AddResponseFileCommands(CommandLineBuilderExtension commandLine)
        {
            if (((this.OutputAssembly == null) && (this.Sources != null)) && ((this.Sources.Length > 0) && (this.ResponseFiles == null)))
            {
                try
                {
                    this.OutputAssembly = new TaskItem(Path.GetFileNameWithoutExtension(this.Sources[0].ItemSpec));
                }
                catch (ArgumentException exception)
                {
                    throw new ArgumentException(exception.Message, "Sources");
                }
                if (string.Compare(this.TargetType, "library", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    ITaskItem outputAssembly = this.OutputAssembly;
                    outputAssembly.ItemSpec = outputAssembly.ItemSpec + ".dll";
                }
                else if (string.Compare(this.TargetType, "module", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    ITaskItem item2 = this.OutputAssembly;
                    item2.ItemSpec = item2.ItemSpec + ".netmodule";
                }
                else
                {
                    ITaskItem item3 = this.OutputAssembly;
                    item3.ItemSpec = item3.ItemSpec + ".exe";
                }
            }
            commandLine.AppendSwitchIfNotNull("/addmodule:", this.AddModules, ",");
            commandLine.AppendSwitchWithInteger("/codepage:", base.Bag, "CodePage");
            this.ConfigureDebugProperties();
            commandLine.AppendPlusOrMinusSwitch("/debug", base.Bag, "EmitDebugInformation");
            commandLine.AppendSwitchIfNotNull("/debug:", this.DebugType);
            commandLine.AppendPlusOrMinusSwitch("/delaysign", base.Bag, "DelaySign");
            commandLine.AppendSwitchWithInteger("/filealign:", base.Bag, "FileAlignment");
            commandLine.AppendSwitchIfNotNull("/keycontainer:", this.KeyContainer);
            commandLine.AppendSwitchIfNotNull("/keyfile:", this.KeyFile);
            commandLine.AppendSwitchIfNotNull("/linkresource:", this.LinkResources, new string[] { "LogicalName", "Access" });
            commandLine.AppendWhenTrue("/nologo", base.Bag, "NoLogo");
            commandLine.AppendWhenTrue("/nowin32manifest", base.Bag, "NoWin32Manifest");
            commandLine.AppendPlusOrMinusSwitch("/optimize", base.Bag, "Optimize");
            commandLine.AppendSwitchIfNotNull("/out:", this.OutputAssembly);
            commandLine.AppendSwitchIfNotNull("/resource:", this.Resources, new string[] { "LogicalName", "Access" });
            commandLine.AppendSwitchIfNotNull("/target:", this.TargetType);
            commandLine.AppendPlusOrMinusSwitch("/warnaserror", base.Bag, "TreatWarningsAsErrors");
            commandLine.AppendWhenTrue("/utf8output", base.Bag, "Utf8Output");
            commandLine.AppendSwitchIfNotNull("/win32icon:", this.Win32Icon);
            commandLine.AppendSwitchIfNotNull("/win32manifest:", this.Win32Manifest);
            commandLine.AppendFileNamesIfNotNull(this.Sources, " ");
        }

        protected bool CheckAllReferencesExistOnDisk()
        {
            if (this.References == null)
            {
                return true;
            }
            bool flag = true;
            foreach (ITaskItem item in this.References)
            {
                if (!File.Exists(item.ItemSpec))
                {
                    flag = false;
                    base.Log.LogErrorWithCodeFromResources("General.ReferenceDoesNotExist", new object[] { item.ItemSpec });
                }
            }
            return flag;
        }

        protected void CheckHostObjectSupport(string parameterName, bool resultFromHostObjectSetOperation)
        {
            if (!resultFromHostObjectSetOperation)
            {
                base.Log.LogMessageFromResources(MessageImportance.Normal, "General.ParameterUnsupportedOnHostCompiler", new object[] { parameterName });
                this.hostCompilerSupportsAllParameters = false;
            }
        }

        private void ConfigureDebugProperties()
        {
            if ((base.Bag["DebugType"] != null) && (string.Compare((string) base.Bag["DebugType"], "none", StringComparison.OrdinalIgnoreCase) == 0))
            {
                base.Bag["DebugType"] = null;
                base.Bag["EmitDebugInformation"] = false;
            }
        }

        internal string GetWin32ManifestSwitch(bool noDefaultWin32Manifest, string win32Manifest)
        {
            if (((noDefaultWin32Manifest || !string.IsNullOrEmpty(win32Manifest)) || (!string.IsNullOrEmpty(this.Win32Resource) || string.Equals(this.TargetType, "library", StringComparison.OrdinalIgnoreCase))) || string.Equals(this.TargetType, "module", StringComparison.OrdinalIgnoreCase))
            {
                return win32Manifest;
            }
            string pathToDotNetFrameworkFile = ToolLocationHelper.GetPathToDotNetFrameworkFile("default.win32manifest", TargetDotNetFrameworkVersion.Version40);
            if (pathToDotNetFrameworkFile == null)
            {
                base.Log.LogMessageFromResources("General.ExpectedFileMissing", new object[] { "default.win32manifest" });
            }
            return pathToDotNetFrameworkFile;
        }

        protected override bool HandleTaskExecutionErrors()
        {
            if (!base.Log.HasLoggedErrors && this.UsedCommandLineTool)
            {
                base.HandleTaskExecutionErrors();
            }
            return false;
        }

        protected bool ListHasNoDuplicateItems(ITaskItem[] itemList, string parameterName)
        {
            return this.ListHasNoDuplicateItems(itemList, parameterName, null);
        }

        private bool ListHasNoDuplicateItems(ITaskItem[] itemList, string parameterName, string disambiguatingMetadataName)
        {
            if ((itemList != null) && (itemList.Length != 0))
            {
                Hashtable hashtable = new Hashtable(StringComparer.OrdinalIgnoreCase);
                foreach (ITaskItem item in itemList)
                {
                    string itemSpec;
                    string metadata = null;
                    if (disambiguatingMetadataName != null)
                    {
                        metadata = item.GetMetadata(disambiguatingMetadataName);
                    }
                    if ((disambiguatingMetadataName == null) || string.IsNullOrEmpty(metadata))
                    {
                        itemSpec = item.ItemSpec;
                    }
                    else
                    {
                        itemSpec = item.ItemSpec + ":" + metadata;
                    }
                    if (hashtable.ContainsKey(itemSpec))
                    {
                        if ((disambiguatingMetadataName == null) || string.IsNullOrEmpty(metadata))
                        {
                            base.Log.LogErrorWithCodeFromResources("General.DuplicateItemsNotSupported", new object[] { item.ItemSpec, parameterName });
                        }
                        else
                        {
                            base.Log.LogErrorWithCodeFromResources("General.DuplicateItemsNotSupportedWithMetadata", new object[] { item.ItemSpec, parameterName, metadata, disambiguatingMetadataName });
                        }
                        return false;
                    }
                    hashtable[itemSpec] = string.Empty;
                }
            }
            return true;
        }

        protected internal virtual bool UseAlternateCommandLineToolToExecute()
        {
            if (string.IsNullOrEmpty(base.ToolPath))
            {
                return !string.Equals(this.ToolName, this.ToolExe, StringComparison.OrdinalIgnoreCase);
            }
            return true;
        }

        protected override bool ValidateParameters()
        {
            return (this.ListHasNoDuplicateItems(this.Resources, "Resources", "LogicalName") && this.ListHasNoDuplicateItems(this.Sources, "Sources"));
        }

        public string[] AdditionalLibPaths
        {
            get
            {
                return (string[]) base.Bag["AdditionalLibPaths"];
            }
            set
            {
                base.Bag["AdditionalLibPaths"] = value;
            }
        }

        public string[] AddModules
        {
            get
            {
                return (string[]) base.Bag["AddModules"];
            }
            set
            {
                base.Bag["AddModules"] = value;
            }
        }

        public int CodePage
        {
            get
            {
                return base.GetIntParameterWithDefault("CodePage", 0);
            }
            set
            {
                base.Bag["CodePage"] = value;
            }
        }

        public string DebugType
        {
            get
            {
                return (string) base.Bag["DebugType"];
            }
            set
            {
                base.Bag["DebugType"] = value;
            }
        }

        public string DefineConstants
        {
            get
            {
                return (string) base.Bag["DefineConstants"];
            }
            set
            {
                base.Bag["DefineConstants"] = value;
            }
        }

        public bool DelaySign
        {
            get
            {
                return base.GetBoolParameterWithDefault("DelaySign", false);
            }
            set
            {
                base.Bag["DelaySign"] = value;
            }
        }

        public bool EmitDebugInformation
        {
            get
            {
                return base.GetBoolParameterWithDefault("EmitDebugInformation", false);
            }
            set
            {
                base.Bag["EmitDebugInformation"] = value;
            }
        }

        public int FileAlignment
        {
            get
            {
                return base.GetIntParameterWithDefault("FileAlignment", 0);
            }
            set
            {
                base.Bag["FileAlignment"] = value;
            }
        }

        protected bool HostCompilerSupportsAllParameters
        {
            get
            {
                return this.hostCompilerSupportsAllParameters;
            }
            set
            {
                this.hostCompilerSupportsAllParameters = value;
            }
        }

        public string KeyContainer
        {
            get
            {
                return (string) base.Bag["KeyContainer"];
            }
            set
            {
                base.Bag["KeyContainer"] = value;
            }
        }

        public string KeyFile
        {
            get
            {
                return (string) base.Bag["KeyFile"];
            }
            set
            {
                base.Bag["KeyFile"] = value;
            }
        }

        public ITaskItem[] LinkResources
        {
            get
            {
                return (ITaskItem[]) base.Bag["LinkResources"];
            }
            set
            {
                base.Bag["LinkResources"] = value;
            }
        }

        public string MainEntryPoint
        {
            get
            {
                return (string) base.Bag["MainEntryPoint"];
            }
            set
            {
                base.Bag["MainEntryPoint"] = value;
            }
        }

        public bool NoConfig
        {
            get
            {
                return base.GetBoolParameterWithDefault("NoConfig", false);
            }
            set
            {
                base.Bag["NoConfig"] = value;
            }
        }

        public bool NoLogo
        {
            get
            {
                return base.GetBoolParameterWithDefault("NoLogo", false);
            }
            set
            {
                base.Bag["NoLogo"] = value;
            }
        }

        public bool NoWin32Manifest
        {
            get
            {
                return base.GetBoolParameterWithDefault("NoWin32Manifest", false);
            }
            set
            {
                base.Bag["NoWin32Manifest"] = value;
            }
        }

        public bool Optimize
        {
            get
            {
                return base.GetBoolParameterWithDefault("Optimize", false);
            }
            set
            {
                base.Bag["Optimize"] = value;
            }
        }

        [Output]
        public ITaskItem OutputAssembly
        {
            get
            {
                return (ITaskItem) base.Bag["OutputAssembly"];
            }
            set
            {
                base.Bag["OutputAssembly"] = value;
            }
        }

        public ITaskItem[] References
        {
            get
            {
                return (ITaskItem[]) base.Bag["References"];
            }
            set
            {
                base.Bag["References"] = value;
            }
        }

        public ITaskItem[] Resources
        {
            get
            {
                return (ITaskItem[]) base.Bag["Resources"];
            }
            set
            {
                base.Bag["Resources"] = value;
            }
        }

        public ITaskItem[] ResponseFiles
        {
            get
            {
                return (ITaskItem[]) base.Bag["ResponseFiles"];
            }
            set
            {
                base.Bag["ResponseFiles"] = value;
            }
        }

        public ITaskItem[] Sources
        {
            get
            {
                return (ITaskItem[]) base.Bag["Sources"];
            }
            set
            {
                base.Bag["Sources"] = value;
            }
        }

        protected override Encoding StandardOutputEncoding
        {
            get
            {
                if (!this.Utf8Output)
                {
                    return base.StandardOutputEncoding;
                }
                return Encoding.UTF8;
            }
        }

        public string TargetType
        {
            get
            {
                return (string) base.Bag["TargetType"];
            }
            set
            {
                base.Bag["TargetType"] = value.ToLower(CultureInfo.InvariantCulture);
            }
        }

        public bool TreatWarningsAsErrors
        {
            get
            {
                return base.GetBoolParameterWithDefault("TreatWarningsAsErrors", false);
            }
            set
            {
                base.Bag["TreatWarningsAsErrors"] = value;
            }
        }

        protected bool UsedCommandLineTool { get; set; }

        public bool Utf8Output
        {
            get
            {
                return base.GetBoolParameterWithDefault("Utf8Output", false);
            }
            set
            {
                base.Bag["Utf8Output"] = value;
            }
        }

        public string Win32Icon
        {
            get
            {
                return (string) base.Bag["Win32Icon"];
            }
            set
            {
                base.Bag["Win32Icon"] = value;
            }
        }

        public string Win32Manifest
        {
            get
            {
                return (string) base.Bag["Win32Manifest"];
            }
            set
            {
                base.Bag["Win32Manifest"] = value;
            }
        }

        public string Win32Resource
        {
            get
            {
                return (string) base.Bag["Win32Resource"];
            }
            set
            {
                base.Bag["Win32Resource"] = value;
            }
        }
    }
}

