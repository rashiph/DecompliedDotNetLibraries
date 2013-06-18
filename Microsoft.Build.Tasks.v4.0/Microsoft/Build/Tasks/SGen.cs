namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Shared;
    using Microsoft.Build.Utilities;
    using System;
    using System.IO;

    public class SGen : ToolTaskExtension
    {
        private string buildAssemblyPath;

        protected override string GenerateCommandLineCommands()
        {
            CommandLineBuilderExtension extension = new CommandLineBuilderExtension();
            bool flag = false;
            try
            {
                if (this.SerializationAssembly == null)
                {
                    this.SerializationAssembly = new TaskItem[] { new TaskItem(this.SerializationAssemblyPath) };
                }
                extension.AppendSwitchIfNotNull("/assembly:", this.AssemblyFullPath);
                extension.AppendWhenTrue("/proxytypes", base.Bag, "UseProxyTypes");
                if (this.References != null)
                {
                    foreach (string str in this.References)
                    {
                        extension.AppendSwitchIfNotNull("/reference:", str);
                    }
                }
                if (this.Types != null)
                {
                    foreach (string str2 in this.Types)
                    {
                        extension.AppendSwitchIfNotNull("/type:", str2);
                    }
                }
                if (this.KeyFile != null)
                {
                    extension.AppendNestedSwitch("/compiler:", "/keyfile:", this.KeyFile);
                }
                else if (this.KeyContainer != null)
                {
                    extension.AppendNestedSwitch("/compiler:", "/keycontainer:", this.KeyContainer);
                }
                extension.AppendPlusOrMinusSwitch("/compiler:/delaysign", base.Bag, "DelaySign");
                if (this.Platform != null)
                {
                    extension.AppendNestedSwitch("/compiler:", "/platform:", this.Platform);
                }
                flag = File.Exists(this.SerializationAssemblyPath);
            }
            catch (Exception exception)
            {
                if (Microsoft.Build.Shared.ExceptionHandling.NotExpectedException(exception))
                {
                    throw;
                }
            }
            if (flag)
            {
                try
                {
                    File.Delete(this.SerializationAssemblyPath);
                }
                catch (UnauthorizedAccessException exception2)
                {
                    base.Log.LogErrorWithCodeFromResources("SGen.CouldNotDeleteSerializer", new object[] { this.SerializationAssemblyPath, exception2.Message });
                }
                catch (IOException exception3)
                {
                    base.Log.LogErrorWithCodeFromResources("SGen.CouldNotDeleteSerializer", new object[] { this.SerializationAssemblyPath, exception3.Message });
                }
            }
            return extension.ToString();
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

        protected override bool SkipTaskExecution()
        {
            return ((this.SerializationAssembly == null) && !this.ShouldGenerateSerializer);
        }

        protected override bool ValidateParameters()
        {
            if (this.References != null)
            {
                foreach (string str in this.References)
                {
                    if (!File.Exists(str))
                    {
                        base.Log.LogErrorWithCodeFromResources("SGen.ResourceNotFound", new object[] { str });
                        return false;
                    }
                }
            }
            return true;
        }

        private string AssemblyFullPath
        {
            get
            {
                return Path.Combine(this.BuildAssemblyPath, this.BuildAssemblyName);
            }
        }

        [Required]
        public string BuildAssemblyName
        {
            get
            {
                return (string) base.Bag["BuildAssemblyName"];
            }
            set
            {
                Microsoft.Build.Shared.ErrorUtilities.VerifyThrowArgumentNull(value, "BuildAssemblyName");
                base.Bag["BuildAssemblyName"] = value;
            }
        }

        [Required]
        public string BuildAssemblyPath
        {
            get
            {
                string fullPath = null;
                try
                {
                    fullPath = Path.GetFullPath(this.buildAssemblyPath);
                }
                catch (Exception exception)
                {
                    if (!Microsoft.Build.Shared.ExceptionHandling.NotExpectedException(exception))
                    {
                        base.Log.LogErrorWithCodeFromResources("SGen.InvalidPath", new object[] { "BuildAssemblyPath", exception.Message });
                    }
                    throw;
                }
                return fullPath;
            }
            set
            {
                Microsoft.Build.Shared.ErrorUtilities.VerifyThrowArgumentNull(value, "BuildAssemblyPath");
                this.buildAssemblyPath = value;
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

        public string[] References
        {
            get
            {
                return (string[]) base.Bag["References"];
            }
            set
            {
                base.Bag["References"] = value;
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

        [Output]
        public ITaskItem[] SerializationAssembly
        {
            get
            {
                return (ITaskItem[]) base.Bag["SerializationAssembly"];
            }
            set
            {
                base.Bag["SerializationAssembly"] = value;
            }
        }

        public string SerializationAssemblyName
        {
            get
            {
                string fileNameWithoutExtension = null;
                try
                {
                    fileNameWithoutExtension = Path.GetFileNameWithoutExtension(this.BuildAssemblyName);
                }
                catch (ArgumentException exception)
                {
                    base.Log.LogErrorWithCodeFromResources("SGen.InvalidPath", new object[] { "BuildAssemblyName", exception.Message });
                    throw;
                }
                return (fileNameWithoutExtension + ".XmlSerializers.dll");
            }
        }

        private string SerializationAssemblyPath
        {
            get
            {
                return Path.Combine(this.BuildAssemblyPath, this.SerializationAssemblyName);
            }
        }

        [Required]
        public bool ShouldGenerateSerializer
        {
            get
            {
                return base.GetBoolParameterWithDefault("ShouldGenerateSerializer", false);
            }
            set
            {
                base.Bag["ShouldGenerateSerializer"] = value;
            }
        }

        protected override string ToolName
        {
            get
            {
                return "sgen.exe";
            }
        }

        public string[] Types
        {
            get
            {
                return (string[]) base.Bag["Types"];
            }
            set
            {
                base.Bag["Types"] = value;
            }
        }

        [Required]
        public bool UseProxyTypes
        {
            get
            {
                return base.GetBoolParameterWithDefault("UseProxyTypes", false);
            }
            set
            {
                base.Bag["UseProxyTypes"] = value;
            }
        }
    }
}

