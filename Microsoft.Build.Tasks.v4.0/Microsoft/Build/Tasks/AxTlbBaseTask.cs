namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Utilities;
    using System;
    using System.IO;
    using System.Reflection;

    internal abstract class AxTlbBaseTask : ToolTaskExtension
    {
        protected AxTlbBaseTask()
        {
        }

        protected internal override void AddCommandLineCommands(CommandLineBuilderExtension commandLine)
        {
            this.AddStrongNameOptions(commandLine);
            base.AddCommandLineCommands(commandLine);
        }

        private void AddStrongNameOptions(CommandLineBuilderExtension commandLine)
        {
            commandLine.AppendWhenTrue("/delaysign", base.Bag, "DelaySign");
            commandLine.AppendSwitchIfNotNull("/keyfile:", this.KeyFile);
            commandLine.AppendSwitchIfNotNull("/keycontainer:", this.KeyContainer);
        }

        public override bool Execute()
        {
            if (string.IsNullOrEmpty(this.ToolName))
            {
                base.Log.LogErrorWithCodeFromResources("AxTlbBaseTask.ToolNameMustBeSet", new object[0]);
                return false;
            }
            return base.Execute();
        }

        protected override string GenerateFullPathToTool()
        {
            return SdkToolsPathUtility.GeneratePathToTool(SdkToolsPathUtility.FileInfoExists, Microsoft.Build.Utilities.ProcessorArchitecture.CurrentProcessArchitecture, this.SdkToolsPath, this.ToolName, base.Log, true);
        }

        protected override bool ValidateParameters()
        {
            if ((string.IsNullOrEmpty(base.ToolPath) || !Directory.Exists(base.ToolPath)) && (string.IsNullOrEmpty(this.SdkToolsPath) || !Directory.Exists(this.SdkToolsPath)))
            {
                object[] messageArgs = new object[] { this.SdkToolsPath ?? "", base.ToolPath ?? "" };
                base.Log.LogErrorWithCodeFromResources("AxTlbBaseTask.SdkOrToolPathNotSpecifiedOrInvalid", messageArgs);
                return false;
            }
            return (this.ValidateStrongNameParameters() && base.ValidateParameters());
        }

        private bool ValidateStrongNameParameters()
        {
            bool flag = false;
            bool flag2 = false;
            if (!string.IsNullOrEmpty(this.KeyFile))
            {
                if (!File.Exists(this.KeyFile))
                {
                    base.Log.LogErrorWithCodeFromResources("AxTlbBaseTask.InvalidKeyFileSpecified", new object[] { this.KeyFile });
                    return false;
                }
                flag = true;
            }
            if (!string.IsNullOrEmpty(this.KeyContainer))
            {
                if (!File.Exists(this.KeyContainer))
                {
                    base.Log.LogErrorWithCodeFromResources("AxTlbBaseTask.InvalidKeyContainerSpecified", new object[] { this.KeyContainer });
                    return false;
                }
                flag2 = true;
            }
            if (flag && flag2)
            {
                base.Log.LogErrorWithCodeFromResources("AxTlbBaseTask.CannotSpecifyBothKeyFileAndKeyContainer", new object[0]);
                return false;
            }
            if (this.DelaySign)
            {
                if (!flag && !flag2)
                {
                    base.Log.LogErrorWithCodeFromResources("AxTlbBaseTask.CannotSpecifyDelaySignWithoutEitherKeyFileOrKeyContainer", new object[0]);
                    return false;
                }
            }
            else if (flag || flag2)
            {
                StrongNameKeyPair keyPair = null;
                byte[] publicKey = null;
                try
                {
                    StrongNameUtils.GetStrongNameKey(base.Log, this.KeyFile, this.KeyContainer, out keyPair, out publicKey);
                }
                catch (StrongNameException exception)
                {
                    base.Log.LogErrorFromException(exception);
                    keyPair = null;
                }
                if (keyPair == null)
                {
                    if (!string.IsNullOrEmpty(this.KeyContainer))
                    {
                        base.Log.LogErrorWithCodeFromResources("AxTlbBaseTask.StrongNameUtils.NoKeyPairInContainer", new object[] { this.KeyContainer });
                        return false;
                    }
                    if (!string.IsNullOrEmpty(this.KeyFile))
                    {
                        base.Log.LogErrorWithCodeFromResources("AxTlbBaseTask.StrongNameUtils.NoKeyPairInFile", new object[] { this.KeyFile });
                        return false;
                    }
                }
            }
            return true;
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

        protected override string ToolName
        {
            get
            {
                return null;
            }
        }
    }
}

