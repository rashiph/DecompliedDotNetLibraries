namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Shared;
    using Microsoft.Build.Utilities;
    using System;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;

    public class Exec : ToolTaskExtension
    {
        private string batchFile;
        private string command = string.Empty;
        private string customErrorRegex;
        private string customWarningRegex;
        private bool encodingParametersValid = true;
        private bool ignoreExitCode;
        private bool ignoreStandardErrorWarningFormat;
        private ITaskItem[] outputs;
        private Encoding standardErrorEncoding = Microsoft.Build.Shared.EncodingUtilities.CurrentSystemOemEncoding;
        private Encoding standardOutputEncoding = Microsoft.Build.Shared.EncodingUtilities.CurrentSystemOemEncoding;
        private string userSpecifiedWorkingDirectory;
        private string workingDirectory;
        internal bool workingDirectoryIsUNC;

        protected internal override void AddCommandLineCommands(CommandLineBuilderExtension commandLine)
        {
            this.CreateTemporaryBatchFile();
            string batchFile = this.batchFile;
            commandLine.AppendSwitch("/Q");
            commandLine.AppendSwitch("/C");
            if (batchFile.Contains("&") && !batchFile.Contains("^&"))
            {
                batchFile = Microsoft.Build.Shared.NativeMethodsShared.GetShortFilePath(batchFile).Replace("&", "^&");
            }
            commandLine.AppendFileNameIfNotNull(batchFile);
        }

        private void CreateTemporaryBatchFile()
        {
            this.batchFile = Microsoft.Build.Shared.FileUtilities.GetTemporaryFile(".exec.cmd");
            using (StreamWriter writer = new StreamWriter(this.batchFile, false, Microsoft.Build.Shared.EncodingUtilities.CurrentSystemOemEncoding))
            {
                writer.WriteLine("setlocal");
                writer.WriteLine("set errorlevel=dummy");
                writer.WriteLine("set errorlevel=");
                if (this.workingDirectoryIsUNC)
                {
                    writer.WriteLine("pushd " + this.workingDirectory);
                }
                writer.WriteLine(this.Command);
                if (this.workingDirectoryIsUNC)
                {
                    writer.WriteLine("popd");
                }
                writer.WriteLine("exit %errorlevel%");
            }
        }

        protected override int ExecuteTool(string pathToTool, string responseFileCommands, string commandLineCommands)
        {
            int num;
            try
            {
                num = base.ExecuteTool(pathToTool, responseFileCommands, commandLineCommands);
            }
            finally
            {
                base.DeleteTempFile(this.batchFile);
            }
            return num;
        }

        protected override string GenerateFullPathToTool()
        {
            return ToolLocationHelper.GetPathToSystemFile("cmd.exe");
        }

        protected override string GetWorkingDirectory()
        {
            if (!Directory.Exists(this.workingDirectory))
            {
                throw new DirectoryNotFoundException(Microsoft.Build.Shared.ResourceUtilities.FormatResourceString("Exec.InvalidWorkingDirectory", new object[] { this.workingDirectory }));
            }
            if (this.workingDirectoryIsUNC)
            {
                return ToolLocationHelper.PathToSystem;
            }
            return this.workingDirectory;
        }

        internal string GetWorkingDirectoryAccessor()
        {
            return this.GetWorkingDirectory();
        }

        protected override bool HandleTaskExecutionErrors()
        {
            if (this.ignoreExitCode)
            {
                base.Log.LogMessageFromResources(MessageImportance.Normal, "Exec.CommandFailedNoErrorCode", new object[] { this.Command, base.ExitCode });
                return true;
            }
            if (base.ExitCode == 5)
            {
                base.Log.LogErrorWithCodeFromResources("Exec.CommandFailedAccessDenied", new object[] { this.Command, base.ExitCode });
            }
            else
            {
                base.Log.LogErrorWithCodeFromResources("Exec.CommandFailed", new object[] { this.Command, base.ExitCode });
            }
            return false;
        }

        protected override void LogEventsFromTextOutput(string singleLine, MessageImportance messageImportance)
        {
            if (this.OutputMatchesRegex(singleLine, ref this.customErrorRegex))
            {
                base.Log.LogError(singleLine, new object[0]);
            }
            else if (this.OutputMatchesRegex(singleLine, ref this.customWarningRegex))
            {
                base.Log.LogWarning(singleLine, new object[0]);
            }
            else if (this.ignoreStandardErrorWarningFormat)
            {
                base.Log.LogMessage(messageImportance, singleLine, null);
            }
            else
            {
                base.Log.LogMessageFromText(singleLine, messageImportance);
            }
        }

        protected override void LogPathToTool(string toolName, string pathToTool)
        {
        }

        protected override void LogToolCommand(string message)
        {
            base.LogToolCommand(this.Command);
        }

        private bool OutputMatchesRegex(string singleLine, ref string regularExpression)
        {
            if (regularExpression == null)
            {
                return false;
            }
            bool flag = false;
            try
            {
                flag = Regex.IsMatch(singleLine, regularExpression);
            }
            catch (ArgumentException exception)
            {
                base.Log.LogErrorWithCodeFromResources("Exec.InvalidRegex", new object[] { regularExpression, exception.Message });
                regularExpression = null;
            }
            return flag;
        }

        protected override bool ValidateParameters()
        {
            if (!this.encodingParametersValid)
            {
                return false;
            }
            if (this.Command.Trim().Length == 0)
            {
                base.Log.LogErrorWithCodeFromResources("Exec.MissingCommandError", new object[0]);
                return false;
            }
            this.workingDirectory = ((this.userSpecifiedWorkingDirectory != null) && (this.userSpecifiedWorkingDirectory.Length > 0)) ? this.userSpecifiedWorkingDirectory : Directory.GetCurrentDirectory();
            this.workingDirectoryIsUNC = Microsoft.Build.Shared.FileUtilitiesRegex.UNCPattern.IsMatch(this.workingDirectory);
            if (this.workingDirectoryIsUNC && (DriveInfo.GetDrives().Length == 0x1a))
            {
                base.Log.LogErrorWithCodeFromResources("Exec.AllDriveLettersMappedError", new object[] { this.workingDirectory });
                return false;
            }
            return true;
        }

        internal bool ValidateParametersAccessor()
        {
            return this.ValidateParameters();
        }

        [Required]
        public string Command
        {
            get
            {
                return this.command;
            }
            set
            {
                this.command = value;
            }
        }

        public string CustomErrorRegularExpression
        {
            get
            {
                return this.customErrorRegex;
            }
            set
            {
                this.customErrorRegex = value;
            }
        }

        public string CustomWarningRegularExpression
        {
            get
            {
                return this.customWarningRegex;
            }
            set
            {
                this.customWarningRegex = value;
            }
        }

        public bool IgnoreExitCode
        {
            get
            {
                return this.ignoreExitCode;
            }
            set
            {
                this.ignoreExitCode = value;
            }
        }

        public bool IgnoreStandardErrorWarningFormat
        {
            get
            {
                return this.ignoreStandardErrorWarningFormat;
            }
            set
            {
                this.ignoreStandardErrorWarningFormat = value;
            }
        }

        [Output]
        public ITaskItem[] Outputs
        {
            get
            {
                if (this.outputs == null)
                {
                    return new ITaskItem[0];
                }
                return this.outputs;
            }
            set
            {
                this.outputs = value;
            }
        }

        protected override Encoding StandardErrorEncoding
        {
            get
            {
                return this.standardErrorEncoding;
            }
        }

        protected override MessageImportance StandardErrorLoggingImportance
        {
            get
            {
                return MessageImportance.High;
            }
        }

        protected override Encoding StandardOutputEncoding
        {
            get
            {
                return this.standardOutputEncoding;
            }
        }

        protected override MessageImportance StandardOutputLoggingImportance
        {
            get
            {
                return MessageImportance.High;
            }
        }

        [Output]
        public string StdErrEncoding
        {
            get
            {
                return this.StandardErrorEncoding.EncodingName;
            }
            set
            {
                try
                {
                    this.standardErrorEncoding = Encoding.GetEncoding(value);
                }
                catch (ArgumentException)
                {
                    base.Log.LogErrorWithCodeFromResources("General.InvalidValue", new object[] { "StdErrEncoding", "Exec" });
                    this.encodingParametersValid = false;
                }
            }
        }

        [Output]
        public string StdOutEncoding
        {
            get
            {
                return this.StandardOutputEncoding.EncodingName;
            }
            set
            {
                try
                {
                    this.standardOutputEncoding = Encoding.GetEncoding(value);
                }
                catch (ArgumentException)
                {
                    base.Log.LogErrorWithCodeFromResources("General.InvalidValue", new object[] { "StdOutEncoding", "Exec" });
                    this.encodingParametersValid = false;
                }
            }
        }

        protected override string ToolName
        {
            get
            {
                return "cmd.exe";
            }
        }

        public string WorkingDirectory
        {
            get
            {
                return this.userSpecifiedWorkingDirectory;
            }
            set
            {
                this.userSpecifiedWorkingDirectory = value;
            }
        }
    }
}

