namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;
    using System;
    using System.IO;

    public class AL : ToolTaskExtension
    {
        protected internal override void AddResponseFileCommands(CommandLineBuilderExtension commandLine)
        {
            commandLine.AppendSwitchIfNotNull("/algid:", this.AlgorithmId);
            commandLine.AppendSwitchIfNotNull("/baseaddress:", this.BaseAddress);
            commandLine.AppendSwitchIfNotNull("/company:", this.CompanyName);
            commandLine.AppendSwitchIfNotNull("/configuration:", this.Configuration);
            commandLine.AppendSwitchIfNotNull("/copyright:", this.Copyright);
            commandLine.AppendSwitchIfNotNull("/culture:", this.Culture);
            commandLine.AppendPlusOrMinusSwitch("/delaysign", base.Bag, "DelaySign");
            commandLine.AppendSwitchIfNotNull("/description:", this.Description);
            commandLine.AppendSwitchIfNotNull("/evidence:", this.EvidenceFile);
            commandLine.AppendSwitchIfNotNull("/fileversion:", this.FileVersion);
            commandLine.AppendSwitchIfNotNull("/flags:", this.Flags);
            commandLine.AppendWhenTrue("/fullpaths", base.Bag, "GenerateFullPaths");
            commandLine.AppendSwitchIfNotNull("/keyfile:", this.KeyFile);
            commandLine.AppendSwitchIfNotNull("/keyname:", this.KeyContainer);
            commandLine.AppendSwitchIfNotNull("/main:", this.MainEntryPoint);
            commandLine.AppendSwitchIfNotNull("/out:", (this.OutputAssembly == null) ? null : this.OutputAssembly.ItemSpec);
            commandLine.AppendSwitchIfNotNull("/platform:", this.Platform);
            commandLine.AppendSwitchIfNotNull("/product:", this.ProductName);
            commandLine.AppendSwitchIfNotNull("/productversion:", this.ProductVersion);
            commandLine.AppendSwitchIfNotNull("/target:", this.TargetType);
            commandLine.AppendSwitchIfNotNull("/template:", this.TemplateFile);
            commandLine.AppendSwitchIfNotNull("/title:", this.Title);
            commandLine.AppendSwitchIfNotNull("/trademark:", this.Trademark);
            commandLine.AppendSwitchIfNotNull("/version:", this.Version);
            commandLine.AppendSwitchIfNotNull("/win32icon:", this.Win32Icon);
            commandLine.AppendSwitchIfNotNull("/win32res:", this.Win32Resource);
            commandLine.AppendSwitchIfNotNull("", this.SourceModules, new string[] { "TargetFile" });
            commandLine.AppendSwitchIfNotNull("/embed:", this.EmbedResources, new string[] { "LogicalName", "Access" });
            commandLine.AppendSwitchIfNotNull("/link:", this.LinkResources, new string[] { "LogicalName", "TargetFile", "Access" });
            if (this.ResponseFiles != null)
            {
                foreach (string str in this.ResponseFiles)
                {
                    commandLine.AppendSwitchIfNotNull("@", str);
                }
            }
        }

        public override bool Execute()
        {
            if ((this.Culture != null) && (this.OutputAssembly != null))
            {
                this.OutputAssembly.SetMetadata("Culture", this.Culture);
            }
            return base.Execute();
        }

        protected override string GenerateFullPathToTool()
        {
            string pathToDotNetFrameworkFile = null;
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("COMPLUS_InstallRoot")) || !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("COMPLUS_Version")))
            {
                pathToDotNetFrameworkFile = ToolLocationHelper.GetPathToDotNetFrameworkFile(this.ToolName, TargetDotNetFrameworkVersion.Version40);
            }
            if (!string.IsNullOrEmpty(pathToDotNetFrameworkFile) && File.Exists(pathToDotNetFrameworkFile))
            {
                return pathToDotNetFrameworkFile;
            }
            return SdkToolsPathUtility.GeneratePathToTool(SdkToolsPathUtility.FileInfoExists, ProcessorArchitecture.CurrentProcessArchitecture, this.SdkToolsPath, this.ToolName, base.Log, true);
        }

        public string AlgorithmId
        {
            get
            {
                return (string) base.Bag["AlgorithmId"];
            }
            set
            {
                base.Bag["AlgorithmId"] = value;
            }
        }

        public string BaseAddress
        {
            get
            {
                return (string) base.Bag["BaseAddress"];
            }
            set
            {
                base.Bag["BaseAddress"] = value;
            }
        }

        public string CompanyName
        {
            get
            {
                return (string) base.Bag["CompanyName"];
            }
            set
            {
                base.Bag["CompanyName"] = value;
            }
        }

        public string Configuration
        {
            get
            {
                return (string) base.Bag["Configuration"];
            }
            set
            {
                base.Bag["Configuration"] = value;
            }
        }

        public string Copyright
        {
            get
            {
                return (string) base.Bag["Copyright"];
            }
            set
            {
                base.Bag["Copyright"] = value;
            }
        }

        public string Culture
        {
            get
            {
                return (string) base.Bag["Culture"];
            }
            set
            {
                base.Bag["Culture"] = value;
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

        public string Description
        {
            get
            {
                return (string) base.Bag["Description"];
            }
            set
            {
                base.Bag["Description"] = value;
            }
        }

        public ITaskItem[] EmbedResources
        {
            get
            {
                return (ITaskItem[]) base.Bag["EmbedResources"];
            }
            set
            {
                base.Bag["EmbedResources"] = value;
            }
        }

        public string EvidenceFile
        {
            get
            {
                return (string) base.Bag["EvidenceFile"];
            }
            set
            {
                base.Bag["EvidenceFile"] = value;
            }
        }

        public string FileVersion
        {
            get
            {
                return (string) base.Bag["FileVersion"];
            }
            set
            {
                base.Bag["FileVersion"] = value;
            }
        }

        public string Flags
        {
            get
            {
                return (string) base.Bag["Flags"];
            }
            set
            {
                base.Bag["Flags"] = value;
            }
        }

        public bool GenerateFullPaths
        {
            get
            {
                return base.GetBoolParameterWithDefault("GenerateFullPaths", false);
            }
            set
            {
                base.Bag["GenerateFullPaths"] = value;
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

        [Output, Required]
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

        public string Platform
        {
            get
            {
                return (string) base.Bag["Platform"];
            }
            set
            {
                base.Bag["Platform"] = value;
            }
        }

        public string ProductName
        {
            get
            {
                return (string) base.Bag["ProductName"];
            }
            set
            {
                base.Bag["ProductName"] = value;
            }
        }

        public string ProductVersion
        {
            get
            {
                return (string) base.Bag["ProductVersion"];
            }
            set
            {
                base.Bag["ProductVersion"] = value;
            }
        }

        public string[] ResponseFiles
        {
            get
            {
                return (string[]) base.Bag["ResponseFiles"];
            }
            set
            {
                base.Bag["ResponseFiles"] = value;
            }
        }

        public string SdkToolsPath
        {
            get
            {
                return (string) base.Bag["SdkToolsPath"];
            }
            set
            {
                base.Bag["SdkToolsPath"] = value;
            }
        }

        public ITaskItem[] SourceModules
        {
            get
            {
                return (ITaskItem[]) base.Bag["SourceModules"];
            }
            set
            {
                base.Bag["SourceModules"] = value;
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
                base.Bag["TargetType"] = value;
            }
        }

        public string TemplateFile
        {
            get
            {
                return (string) base.Bag["TemplateFile"];
            }
            set
            {
                base.Bag["TemplateFile"] = value;
            }
        }

        public string Title
        {
            get
            {
                return (string) base.Bag["Title"];
            }
            set
            {
                base.Bag["Title"] = value;
            }
        }

        protected override string ToolName
        {
            get
            {
                return "AL.exe";
            }
        }

        public string Trademark
        {
            get
            {
                return (string) base.Bag["Trademark"];
            }
            set
            {
                base.Bag["Trademark"] = value;
            }
        }

        public string Version
        {
            get
            {
                return (string) base.Bag["Version"];
            }
            set
            {
                base.Bag["Version"] = value;
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

