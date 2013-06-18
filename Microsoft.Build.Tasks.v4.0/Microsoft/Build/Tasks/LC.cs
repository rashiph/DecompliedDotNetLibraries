namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;
    using System;
    using System.IO;

    public class LC : ToolTaskExtension
    {
        protected internal override void AddCommandLineCommands(CommandLineBuilderExtension commandLine)
        {
            commandLine.AppendSwitchIfNotNull("/target:", this.LicenseTarget.ItemSpec);
            foreach (ITaskItem item in this.Sources)
            {
                commandLine.AppendSwitchIfNotNull("/complist:", item.ItemSpec);
            }
            commandLine.AppendSwitchIfNotNull("/outdir:", this.OutputDirectory);
            if (this.ReferencedAssemblies != null)
            {
                foreach (ITaskItem item2 in this.ReferencedAssemblies)
                {
                    commandLine.AppendSwitchIfNotNull("/i:", item2.ItemSpec);
                }
            }
            commandLine.AppendWhenTrue("/nologo", base.Bag, "NoLogo");
            string str = this.LicenseTarget.ItemSpec + ".licenses";
            if (this.OutputDirectory != null)
            {
                str = Path.Combine(this.OutputDirectory, str);
            }
            this.OutputLicense = new TaskItem(str);
        }

        protected override string GenerateFullPathToTool()
        {
            return SdkToolsPathUtility.GeneratePathToTool(SdkToolsPathUtility.FileInfoExists, ProcessorArchitecture.CurrentProcessArchitecture, this.SdkToolsPath, this.ToolName, base.Log, true);
        }

        protected override bool ValidateParameters()
        {
            return true;
        }

        [Required]
        public ITaskItem LicenseTarget
        {
            get
            {
                return (ITaskItem) base.Bag["LicenseTarget"];
            }
            set
            {
                base.Bag["LicenseTarget"] = value;
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

        public string OutputDirectory
        {
            get
            {
                return (string) base.Bag["OutputDirectory"];
            }
            set
            {
                base.Bag["OutputDirectory"] = value;
            }
        }

        [Output]
        public ITaskItem OutputLicense
        {
            get
            {
                return (ITaskItem) base.Bag["OutputLicense"];
            }
            set
            {
                base.Bag["OutputLicense"] = value;
            }
        }

        public ITaskItem[] ReferencedAssemblies
        {
            get
            {
                return (ITaskItem[]) base.Bag["ReferencedAssemblies"];
            }
            set
            {
                base.Bag["ReferencedAssemblies"] = value;
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

        [Required]
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

        protected override string ToolName
        {
            get
            {
                return "LC.exe";
            }
        }
    }
}

