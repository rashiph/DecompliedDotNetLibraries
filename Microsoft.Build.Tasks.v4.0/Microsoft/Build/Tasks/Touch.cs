namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Shared;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;

    public class Touch : TaskExtension
    {
        private bool alwaysCreate = false;
        private ITaskItem[] files;
        private bool forceTouch = false;
        private string specificTime;
        private ITaskItem[] touchedFiles;

        private bool CreateFile(string file, Microsoft.Build.Shared.FileCreate fileCreate)
        {
            try
            {
                using (fileCreate(file))
                {
                }
            }
            catch (Exception exception)
            {
                if (Microsoft.Build.Shared.ExceptionHandling.NotExpectedException(exception))
                {
                    throw;
                }
                base.Log.LogErrorWithCodeFromResources("Touch.CannotCreateFile", new object[] { file, exception.Message });
                return false;
            }
            return true;
        }

        public override bool Execute()
        {
            return this.ExecuteImpl(new Microsoft.Build.Shared.FileExists(File.Exists), new Microsoft.Build.Shared.FileCreate(File.Create), new GetAttributes(File.GetAttributes), new SetAttributes(File.SetAttributes), new SetLastAccessTime(File.SetLastAccessTime), new SetLastWriteTime(File.SetLastWriteTime));
        }

        internal bool ExecuteImpl(Microsoft.Build.Shared.FileExists fileExists, Microsoft.Build.Shared.FileCreate fileCreate, GetAttributes fileGetAttributes, SetAttributes fileSetAttributes, SetLastAccessTime fileSetLastAccessTime, SetLastWriteTime fileSetLastWriteTime)
        {
            DateTime touchDateTime;
            try
            {
                touchDateTime = this.GetTouchDateTime();
            }
            catch (FormatException exception)
            {
                base.Log.LogErrorWithCodeFromResources("Touch.TimeSyntaxIncorrect", new object[] { exception.Message });
                return false;
            }
            bool flag = true;
            ArrayList list = new ArrayList();
            HashSet<string> set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (ITaskItem item in this.Files)
            {
                if (!set.Contains(item.ItemSpec))
                {
                    if (this.TouchFile(item.ItemSpec, touchDateTime, fileExists, fileCreate, fileGetAttributes, fileSetAttributes, fileSetLastAccessTime, fileSetLastWriteTime))
                    {
                        list.Add(item);
                    }
                    else
                    {
                        flag = false;
                    }
                    set.Add(item.ItemSpec);
                }
            }
            this.TouchedFiles = (ITaskItem[]) list.ToArray(typeof(ITaskItem));
            return flag;
        }

        private DateTime GetTouchDateTime()
        {
            if ((this.Time != null) && (this.Time.Length != 0))
            {
                return DateTime.Parse(this.Time, DateTimeFormatInfo.InvariantInfo);
            }
            return DateTime.Now;
        }

        private bool TouchFile(string file, DateTime dt, Microsoft.Build.Shared.FileExists fileExists, Microsoft.Build.Shared.FileCreate fileCreate, GetAttributes fileGetAttributes, SetAttributes fileSetAttributes, SetLastAccessTime fileSetLastAccessTime, SetLastWriteTime fileSetLastWriteTime)
        {
            if (!fileExists(file))
            {
                if (!this.AlwaysCreate)
                {
                    base.Log.LogErrorWithCodeFromResources("Touch.FileDoesNotExist", new object[] { file });
                    return false;
                }
                base.Log.LogMessageFromResources(MessageImportance.Normal, "Touch.CreatingFile", new object[] { file, "AlwaysCreate" });
                if (!this.CreateFile(file, fileCreate))
                {
                    return false;
                }
            }
            else
            {
                base.Log.LogMessageFromResources(MessageImportance.Normal, "Touch.Touching", new object[] { file });
            }
            bool flag = false;
            FileAttributes attributes = fileGetAttributes(file);
            if (((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly) && this.ForceTouch)
            {
                try
                {
                    FileAttributes attributes2 = attributes & ~FileAttributes.ReadOnly;
                    fileSetAttributes(file, attributes2);
                    flag = true;
                }
                catch (Exception exception)
                {
                    if (Microsoft.Build.Shared.ExceptionHandling.NotExpectedException(exception))
                    {
                        throw;
                    }
                    base.Log.LogErrorWithCodeFromResources("Touch.CannotMakeFileWritable", new object[] { file, exception.Message });
                    return false;
                }
            }
            bool flag2 = true;
            try
            {
                fileSetLastAccessTime(file, dt);
                fileSetLastWriteTime(file, dt);
            }
            catch (Exception exception2)
            {
                if (Microsoft.Build.Shared.ExceptionHandling.NotExpectedException(exception2))
                {
                    throw;
                }
                base.Log.LogErrorWithCodeFromResources("Touch.CannotTouch", new object[] { file, exception2.Message });
                return false;
            }
            finally
            {
                if (flag)
                {
                    try
                    {
                        fileSetAttributes(file, attributes);
                    }
                    catch (Exception exception3)
                    {
                        if (Microsoft.Build.Shared.ExceptionHandling.NotExpectedException(exception3))
                        {
                            throw;
                        }
                        base.Log.LogErrorWithCodeFromResources("Touch.CannotRestoreAttributes", new object[] { file, exception3.Message });
                        flag2 = false;
                    }
                }
            }
            return flag2;
        }

        public bool AlwaysCreate
        {
            get
            {
                return this.alwaysCreate;
            }
            set
            {
                this.alwaysCreate = value;
            }
        }

        [Required]
        public ITaskItem[] Files
        {
            get
            {
                return this.files;
            }
            set
            {
                this.files = value;
            }
        }

        public bool ForceTouch
        {
            get
            {
                return this.forceTouch;
            }
            set
            {
                this.forceTouch = value;
            }
        }

        public string Time
        {
            get
            {
                return this.specificTime;
            }
            set
            {
                this.specificTime = value;
            }
        }

        [Output]
        public ITaskItem[] TouchedFiles
        {
            get
            {
                return this.touchedFiles;
            }
            set
            {
                this.touchedFiles = value;
            }
        }
    }
}

