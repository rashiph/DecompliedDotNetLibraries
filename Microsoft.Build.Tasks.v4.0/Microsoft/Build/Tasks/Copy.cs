namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Shared;
    using Microsoft.Build.Utilities;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;

    public class Copy : TaskExtension
    {
        private ITaskItem[] copiedFiles;
        private ITaskItem[] destinationFiles;
        private ITaskItem destinationFolder;
        private bool overwriteReadOnlyFiles;
        private const int RetryDelayMillisecondsDefault = 0x3e8;
        private bool skipUnchangedFiles;
        private ITaskItem[] sourceFiles;

        public Copy()
        {
            this.RetryDelayMilliseconds = 0x3e8;
        }

        private bool CopyFileWithLogging(Microsoft.Build.Tasks.FileState sourceFileState, Microsoft.Build.Tasks.FileState destinationFileState)
        {
            bool exists = false;
            if (Directory.Exists(destinationFileState.Name))
            {
                base.Log.LogErrorWithCodeFromResources("Copy.DestinationIsDirectory", new object[] { sourceFileState.Name, destinationFileState.Name });
                return false;
            }
            if (Directory.Exists(sourceFileState.Name))
            {
                base.Log.LogErrorWithCodeFromResources("Copy.SourceIsDirectory", new object[] { sourceFileState.Name });
                return false;
            }
            string directoryName = Path.GetDirectoryName(destinationFileState.Name);
            if (((directoryName != null) && (directoryName.Length > 0)) && !Directory.Exists(directoryName))
            {
                base.Log.LogMessageFromResources(MessageImportance.Normal, "Copy.CreatesDirectory", new object[] { directoryName });
                Directory.CreateDirectory(directoryName);
            }
            if (this.overwriteReadOnlyFiles)
            {
                this.MakeFileWriteable(destinationFileState, true);
                exists = destinationFileState.Exists;
            }
            bool flag2 = false;
            if (this.UseHardlinksIfPossible)
            {
                base.Log.LogMessageFromResources(MessageImportance.Normal, "Copy.HardLinkComment", new object[] { sourceFileState.Name, destinationFileState.Name });
                if (!this.overwriteReadOnlyFiles)
                {
                    exists = destinationFileState.Exists;
                }
                if (exists && !IsMatchingSizeAndTimeStamp(sourceFileState, destinationFileState))
                {
                    Microsoft.Build.Shared.FileUtilities.DeleteNoThrow(destinationFileState.Name);
                }
                flag2 = Microsoft.Build.Tasks.NativeMethods.CreateHardLink(destinationFileState.Name, sourceFileState.Name, IntPtr.Zero);
                if (!flag2)
                {
                    Exception exceptionForHR = Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error());
                    base.Log.LogMessageFromResources(MessageImportance.Low, "Copy.RetryingAsFileCopy", new object[] { sourceFileState.Name, destinationFileState.Name, exceptionForHR.Message });
                }
            }
            if (!flag2)
            {
                base.Log.LogMessageFromResources(MessageImportance.Normal, "Copy.FileComment", new object[] { sourceFileState.Name, destinationFileState.Name });
                File.Copy(sourceFileState.Name, destinationFileState.Name, true);
            }
            destinationFileState.Reset();
            this.MakeFileWriteable(destinationFileState, false);
            return true;
        }

        private bool DoCopyIfNecessary(Microsoft.Build.Tasks.FileState sourceFileState, Microsoft.Build.Tasks.FileState destinationFileState, CopyFileWithState copyFile)
        {
            bool flag = true;
            try
            {
                if (this.skipUnchangedFiles && IsMatchingSizeAndTimeStamp(sourceFileState, destinationFileState))
                {
                    base.Log.LogMessageFromResources(MessageImportance.Low, "Copy.DidNotCopyBecauseOfFileMatch", new object[] { sourceFileState.Name, destinationFileState.Name, "SkipUnchangedFiles", "true" });
                    return flag;
                }
                if (string.Compare(sourceFileState.Name, destinationFileState.Name, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    flag = this.DoCopyWithRetries(sourceFileState, destinationFileState, copyFile);
                }
            }
            catch (PathTooLongException exception)
            {
                base.Log.LogErrorWithCodeFromResources("Copy.Error", new object[] { sourceFileState.Name, destinationFileState.Name, exception.Message });
                flag = false;
            }
            catch (IOException exception2)
            {
                if (this.PathsAreIdentical(sourceFileState.Name, destinationFileState.Name))
                {
                    return flag;
                }
                if (Microsoft.Build.Shared.ExceptionHandling.NotExpectedException(exception2))
                {
                    throw;
                }
                base.Log.LogErrorWithCodeFromResources("Copy.Error", new object[] { sourceFileState.Name, destinationFileState.Name, exception2.Message });
                flag = false;
            }
            catch (Exception exception3)
            {
                if (Microsoft.Build.Shared.ExceptionHandling.NotExpectedException(exception3))
                {
                    throw;
                }
                base.Log.LogErrorWithCodeFromResources("Copy.Error", new object[] { sourceFileState.Name, destinationFileState.Name, exception3.Message });
                flag = false;
            }
            return flag;
        }

        private bool DoCopyWithRetries(Microsoft.Build.Tasks.FileState sourceFileState, Microsoft.Build.Tasks.FileState destinationFileState, CopyFileWithState copyFile)
        {
            bool flag = false;
            int num = 0;
        Label_0004:
            try
            {
                flag = copyFile(sourceFileState, destinationFileState);
            }
            catch (Exception exception)
            {
                if (Microsoft.Build.Shared.ExceptionHandling.NotExpectedException(exception))
                {
                    throw;
                }
                if (num >= this.Retries)
                {
                    if (this.Retries > 0)
                    {
                        base.Log.LogErrorWithCodeFromResources("Copy.ExceededRetries", new object[] { sourceFileState.Name, destinationFileState.Name, this.Retries });
                        throw;
                    }
                    throw;
                }
                num++;
                base.Log.LogWarningWithCodeFromResources("Copy.Retrying", new object[] { sourceFileState.Name, destinationFileState.Name, num, this.RetryDelayMilliseconds, exception.Message });
                Thread.Sleep(this.RetryDelayMilliseconds);
                goto Label_0004;
            }
            if (flag)
            {
                return true;
            }
            if (num < this.Retries)
            {
                num++;
                base.Log.LogWarningWithCodeFromResources("Copy.Retrying", new object[] { sourceFileState.Name, destinationFileState.Name, num, this.RetryDelayMilliseconds, string.Empty });
                Thread.Sleep(this.RetryDelayMilliseconds);
                goto Label_0004;
            }
            if (this.Retries > 0)
            {
                base.Log.LogErrorWithCodeFromResources("Copy.ExceededRetries", new object[] { sourceFileState.Name, destinationFileState.Name, this.Retries });
                return false;
            }
            return false;
        }

        public override bool Execute()
        {
            return this.Execute(new CopyFileWithState(this.CopyFileWithLogging));
        }

        internal bool Execute(CopyFileWithState copyFile)
        {
            if ((this.sourceFiles == null) || (this.sourceFiles.Length == 0))
            {
                this.destinationFiles = new TaskItem[0];
                this.copiedFiles = new TaskItem[0];
                return true;
            }
            if (!this.ValidateInputs() || !this.InitializeDestinationFiles())
            {
                return false;
            }
            bool flag = true;
            List<ITaskItem> list = new List<ITaskItem>();
            Dictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < this.sourceFiles.Length; i++)
            {
                string str;
                bool flag2 = false;
                if (dictionary.TryGetValue(this.destinationFiles[i].ItemSpec, out str) && string.Equals(str, this.sourceFiles[i].ItemSpec, StringComparison.OrdinalIgnoreCase))
                {
                    flag2 = true;
                }
                if (!flag2)
                {
                    if (this.DoCopyIfNecessary(new Microsoft.Build.Tasks.FileState(this.sourceFiles[i].ItemSpec), new Microsoft.Build.Tasks.FileState(this.destinationFiles[i].ItemSpec), copyFile))
                    {
                        dictionary[this.destinationFiles[i].ItemSpec] = this.sourceFiles[i].ItemSpec;
                        flag2 = true;
                    }
                    else
                    {
                        flag = false;
                    }
                }
                if (flag2)
                {
                    this.sourceFiles[i].CopyMetadataTo(this.destinationFiles[i]);
                    list.Add(this.destinationFiles[i]);
                }
            }
            this.copiedFiles = list.ToArray();
            return flag;
        }

        private bool InitializeDestinationFiles()
        {
            if (this.destinationFiles == null)
            {
                this.destinationFiles = new ITaskItem[this.sourceFiles.Length];
                for (int i = 0; i < this.sourceFiles.Length; i++)
                {
                    string str;
                    try
                    {
                        str = Path.Combine(this.destinationFolder.ItemSpec, Path.GetFileName(this.sourceFiles[i].ItemSpec));
                    }
                    catch (ArgumentException exception)
                    {
                        base.Log.LogErrorWithCodeFromResources("Copy.Error", new object[] { this.sourceFiles[i].ItemSpec, this.destinationFolder.ItemSpec, exception.Message });
                        this.destinationFiles = new ITaskItem[0];
                        return false;
                    }
                    this.destinationFiles[i] = new TaskItem(Microsoft.Build.Shared.EscapingUtilities.Escape(str));
                    this.sourceFiles[i].CopyMetadataTo(this.destinationFiles[i]);
                }
            }
            return true;
        }

        private static bool IsMatchingSizeAndTimeStamp(Microsoft.Build.Tasks.FileState sourceFile, Microsoft.Build.Tasks.FileState destinationFile)
        {
            if (!destinationFile.Exists)
            {
                return false;
            }
            if (sourceFile.LastWriteTime != destinationFile.LastWriteTime)
            {
                return false;
            }
            if (sourceFile.Length != destinationFile.Length)
            {
                return false;
            }
            return true;
        }

        private void MakeFileWriteable(Microsoft.Build.Tasks.FileState file, bool logActivity)
        {
            if (file.Exists && file.IsReadOnly)
            {
                if (logActivity)
                {
                    base.Log.LogMessageFromResources(MessageImportance.Low, "Copy.RemovingReadOnlyAttribute", new object[] { file.Name });
                }
                File.SetAttributes(file.Name, FileAttributes.Normal);
                file.Reset();
            }
        }

        private bool PathsAreIdentical(string source, string destination)
        {
            string fullPath = Path.GetFullPath(source);
            string strB = Path.GetFullPath(destination);
            return (0 == string.Compare(fullPath, strB, StringComparison.OrdinalIgnoreCase));
        }

        private bool ValidateInputs()
        {
            if (this.Retries < 0)
            {
                base.Log.LogErrorWithCodeFromResources("Copy.InvalidRetryCount", new object[] { this.Retries });
                return false;
            }
            if (this.RetryDelayMilliseconds < 0)
            {
                base.Log.LogErrorWithCodeFromResources("Copy.InvalidRetryDelay", new object[] { this.RetryDelayMilliseconds });
                return false;
            }
            if ((this.destinationFiles == null) && (this.destinationFolder == null))
            {
                base.Log.LogErrorWithCodeFromResources("Copy.NeedsDestination", new object[] { "DestinationFiles", "DestinationFolder" });
                return false;
            }
            if ((this.destinationFiles != null) && (this.destinationFolder != null))
            {
                base.Log.LogErrorWithCodeFromResources("Copy.ExactlyOneTypeOfDestination", new object[] { "DestinationFiles", "DestinationFolder" });
                return false;
            }
            if ((this.destinationFiles != null) && (this.destinationFiles.Length != this.sourceFiles.Length))
            {
                base.Log.LogErrorWithCodeFromResources("General.TwoVectorsMustHaveSameLength", new object[] { this.destinationFiles.Length, this.sourceFiles.Length, "DestinationFiles", "SourceFiles" });
                return false;
            }
            return true;
        }

        [Output]
        public ITaskItem[] CopiedFiles
        {
            get
            {
                return this.copiedFiles;
            }
        }

        [Output]
        public ITaskItem[] DestinationFiles
        {
            get
            {
                return this.destinationFiles;
            }
            set
            {
                this.destinationFiles = value;
            }
        }

        public ITaskItem DestinationFolder
        {
            get
            {
                return this.destinationFolder;
            }
            set
            {
                this.destinationFolder = value;
            }
        }

        public bool OverwriteReadOnlyFiles
        {
            get
            {
                return this.overwriteReadOnlyFiles;
            }
            set
            {
                this.overwriteReadOnlyFiles = value;
            }
        }

        public int Retries { get; set; }

        public int RetryDelayMilliseconds { get; set; }

        public bool SkipUnchangedFiles
        {
            get
            {
                return this.skipUnchangedFiles;
            }
            set
            {
                this.skipUnchangedFiles = value;
            }
        }

        [Required]
        public ITaskItem[] SourceFiles
        {
            get
            {
                return this.sourceFiles;
            }
            set
            {
                this.sourceFiles = value;
            }
        }

        public bool UseHardlinksIfPossible { get; set; }
    }
}

