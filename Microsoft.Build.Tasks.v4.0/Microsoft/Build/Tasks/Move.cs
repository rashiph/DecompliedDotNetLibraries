namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Shared;
    using Microsoft.Build.Utilities;
    using System;
    using System.Collections;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    public class Move : TaskExtension
    {
        private const Microsoft.Build.Tasks.NativeMethods.MoveFileFlags Flags = (Microsoft.Build.Tasks.NativeMethods.MoveFileFlags.MOVEFILE_WRITE_THROUGH | Microsoft.Build.Tasks.NativeMethods.MoveFileFlags.MOVEFILE_COPY_ALLOWED | Microsoft.Build.Tasks.NativeMethods.MoveFileFlags.MOVEFILE_REPLACE_EXISTING);
        private ITaskItem[] movedFiles;

        public override bool Execute()
        {
            bool flag = true;
            if ((this.SourceFiles == null) || (this.SourceFiles.Length == 0))
            {
                this.DestinationFiles = new TaskItem[0];
                this.movedFiles = new TaskItem[0];
                return true;
            }
            if ((this.DestinationFiles == null) && (this.DestinationFolder == null))
            {
                base.Log.LogErrorWithCodeFromResources("Move.NeedsDestination", new object[] { "DestinationFiles", "DestinationDirectory" });
                return false;
            }
            if ((this.DestinationFiles != null) && (this.DestinationFolder != null))
            {
                base.Log.LogErrorWithCodeFromResources("Move.ExactlyOneTypeOfDestination", new object[] { "DestinationFiles", "DestinationDirectory" });
                return false;
            }
            if ((this.DestinationFiles != null) && (this.DestinationFiles.Length != this.SourceFiles.Length))
            {
                base.Log.LogErrorWithCodeFromResources("General.TwoVectorsMustHaveSameLength", new object[] { this.DestinationFiles.Length, this.SourceFiles.Length, "DestinationFiles", "SourceFiles" });
                return false;
            }
            if (this.DestinationFiles == null)
            {
                this.DestinationFiles = new ITaskItem[this.SourceFiles.Length];
                for (int j = 0; j < this.SourceFiles.Length; j++)
                {
                    string str;
                    try
                    {
                        str = Path.Combine(this.DestinationFolder.ItemSpec, Path.GetFileName(this.SourceFiles[j].ItemSpec));
                    }
                    catch (ArgumentException exception)
                    {
                        base.Log.LogErrorWithCodeFromResources("Move.Error", new object[] { this.SourceFiles[j].ItemSpec, this.DestinationFolder.ItemSpec, exception.Message });
                        this.DestinationFiles = new ITaskItem[0];
                        return false;
                    }
                    this.DestinationFiles[j] = new TaskItem(str);
                }
            }
            ArrayList list = new ArrayList();
            for (int i = 0; i < this.SourceFiles.Length; i++)
            {
                string itemSpec = this.SourceFiles[i].ItemSpec;
                string destinationFile = this.DestinationFiles[i].ItemSpec;
                try
                {
                    if (this.MoveFileWithLogging(itemSpec, destinationFile))
                    {
                        this.SourceFiles[i].CopyMetadataTo(this.DestinationFiles[i]);
                        list.Add(this.DestinationFiles[i]);
                    }
                    else
                    {
                        flag = false;
                    }
                }
                catch (Exception exception2)
                {
                    if (Microsoft.Build.Shared.ExceptionHandling.NotExpectedException(exception2))
                    {
                        throw;
                    }
                    base.Log.LogErrorWithCodeFromResources("Move.Error", new object[] { itemSpec, destinationFile, exception2.Message });
                    flag = false;
                }
            }
            this.movedFiles = (ITaskItem[]) list.ToArray(typeof(ITaskItem));
            return flag;
        }

        private static void MakeWriteableIfReadOnly(string file)
        {
            FileInfo info = new FileInfo(file);
            if ((info.Attributes & FileAttributes.ReadOnly) != 0)
            {
                info.Attributes &= ~FileAttributes.ReadOnly;
            }
        }

        private bool MoveFileWithLogging(string sourceFile, string destinationFile)
        {
            if (Directory.Exists(destinationFile))
            {
                base.Log.LogErrorWithCodeFromResources("Move.DestinationIsDirectory", new object[] { sourceFile, destinationFile });
                return false;
            }
            if (Directory.Exists(sourceFile))
            {
                base.Log.LogErrorWithCodeFromResources("Move.SourceIsDirectory", new object[] { sourceFile });
                return false;
            }
            if (!File.Exists(sourceFile))
            {
                base.Log.LogErrorWithCodeFromResources("Move.SourceDoesNotExist", new object[] { sourceFile });
                return false;
            }
            if (this.OverwriteReadOnlyFiles && File.Exists(destinationFile))
            {
                MakeWriteableIfReadOnly(destinationFile);
            }
            string directoryName = Path.GetDirectoryName(destinationFile);
            if (((directoryName != null) && (directoryName.Length > 0)) && !Directory.Exists(directoryName))
            {
                base.Log.LogMessageFromResources(MessageImportance.Normal, "Move.CreatesDirectory", new object[] { directoryName });
                Directory.CreateDirectory(directoryName);
            }
            base.Log.LogMessageFromResources(MessageImportance.Normal, "Move.FileComment", new object[] { sourceFile, destinationFile });
            if (!Microsoft.Build.Tasks.NativeMethods.MoveFileEx(sourceFile, destinationFile, Microsoft.Build.Tasks.NativeMethods.MoveFileFlags.MOVEFILE_WRITE_THROUGH | Microsoft.Build.Tasks.NativeMethods.MoveFileFlags.MOVEFILE_COPY_ALLOWED | Microsoft.Build.Tasks.NativeMethods.MoveFileFlags.MOVEFILE_REPLACE_EXISTING))
            {
                File.Move(sourceFile, destinationFile);
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            }
            if (File.Exists(destinationFile))
            {
                MakeWriteableIfReadOnly(destinationFile);
            }
            return true;
        }

        [Output]
        public ITaskItem[] DestinationFiles { get; set; }

        public ITaskItem DestinationFolder { get; set; }

        [Output]
        public ITaskItem[] MovedFiles
        {
            get
            {
                return this.movedFiles;
            }
        }

        public bool OverwriteReadOnlyFiles { get; set; }

        [Required]
        public ITaskItem[] SourceFiles { get; set; }
    }
}

