namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Shared;
    using Microsoft.Build.Utilities;
    using System;
    using System.Collections;
    using System.IO;
    using System.Runtime.InteropServices;

    public class RemoveDir : TaskExtension
    {
        private ITaskItem[] directories;
        private ITaskItem[] removedDirectories;

        public override bool Execute()
        {
            bool flag = true;
            ArrayList list = new ArrayList();
            foreach (ITaskItem item in this.Directories)
            {
                if (Directory.Exists(item.ItemSpec))
                {
                    bool unauthorizedAccess = false;
                    base.Log.LogMessageFromResources(MessageImportance.Normal, "RemoveDir.Removing", new object[] { item.ItemSpec });
                    bool flag3 = this.RemoveDirectory(item, false, out unauthorizedAccess);
                    if (!flag3 && unauthorizedAccess)
                    {
                        flag3 = this.RemoveReadOnlyAttributeRecursively(new DirectoryInfo(item.ItemSpec));
                        if (flag3)
                        {
                            flag3 = this.RemoveDirectory(item, true, out unauthorizedAccess);
                        }
                    }
                    if (!flag3)
                    {
                        flag = false;
                    }
                    if (flag3)
                    {
                        list.Add(new TaskItem(item));
                    }
                }
                else
                {
                    base.Log.LogMessageFromResources(MessageImportance.Normal, "RemoveDir.SkippingNonexistentDirectory", new object[] { item.ItemSpec });
                    list.Add(new TaskItem(item));
                }
            }
            this.RemovedDirectories = (ITaskItem[]) list.ToArray(typeof(ITaskItem));
            return flag;
        }

        private bool RemoveDirectory(ITaskItem directory, bool logUnauthorizedError, out bool unauthorizedAccess)
        {
            bool flag = true;
            unauthorizedAccess = false;
            try
            {
                Directory.Delete(directory.ItemSpec, true);
            }
            catch (UnauthorizedAccessException exception)
            {
                flag = false;
                if (logUnauthorizedError)
                {
                    base.Log.LogErrorWithCodeFromResources("RemoveDir.Error", new object[] { directory, exception.Message });
                }
                unauthorizedAccess = true;
            }
            catch (Exception exception2)
            {
                if (Microsoft.Build.Shared.ExceptionHandling.NotExpectedException(exception2))
                {
                    throw;
                }
                base.Log.LogErrorWithCodeFromResources("RemoveDir.Error", new object[] { directory.ItemSpec, exception2.Message });
                flag = false;
            }
            return flag;
        }

        private bool RemoveReadOnlyAttributeRecursively(DirectoryInfo directory)
        {
            bool flag = true;
            try
            {
                if ((directory.Attributes & FileAttributes.ReadOnly) != 0)
                {
                    FileAttributes attributes = directory.Attributes & ~FileAttributes.ReadOnly;
                    directory.Attributes = attributes;
                }
                foreach (FileSystemInfo info in directory.GetFileSystemInfos())
                {
                    if ((info.Attributes & FileAttributes.ReadOnly) != 0)
                    {
                        FileAttributes attributes2 = info.Attributes & ~FileAttributes.ReadOnly;
                        info.Attributes = attributes2;
                    }
                }
                foreach (DirectoryInfo info2 in directory.GetDirectories())
                {
                    flag = this.RemoveReadOnlyAttributeRecursively(info2);
                }
            }
            catch (Exception exception)
            {
                if (Microsoft.Build.Shared.ExceptionHandling.NotExpectedException(exception))
                {
                    throw;
                }
                base.Log.LogErrorWithCodeFromResources("RemoveDir.Error", new object[] { directory, exception.Message });
                flag = false;
            }
            return flag;
        }

        [Required]
        public ITaskItem[] Directories
        {
            get
            {
                Microsoft.Build.Shared.ErrorUtilities.VerifyThrowArgumentNull(this.directories, "directories");
                return this.directories;
            }
            set
            {
                this.directories = value;
            }
        }

        [Output]
        public ITaskItem[] RemovedDirectories
        {
            get
            {
                return this.removedDirectories;
            }
            set
            {
                this.removedDirectories = value;
            }
        }
    }
}

