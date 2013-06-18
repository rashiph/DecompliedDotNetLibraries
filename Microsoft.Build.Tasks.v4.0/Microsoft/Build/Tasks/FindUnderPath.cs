namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Shared;
    using System;
    using System.Collections;
    using System.IO;

    public class FindUnderPath : TaskExtension
    {
        private ITaskItem[] files = new TaskItem[0];
        private ITaskItem[] inPath;
        private ITaskItem[] outOfPath;
        private ITaskItem path;
        private bool updateToAbsolutePaths;

        public override bool Execute()
        {
            string str;
            ArrayList list = new ArrayList();
            ArrayList list2 = new ArrayList();
            try
            {
                str = Microsoft.Build.Shared.FileUtilities.EnsureTrailingSlash(System.IO.Path.GetFullPath(this.path.ItemSpec));
            }
            catch (Exception exception)
            {
                if (Microsoft.Build.Shared.ExceptionHandling.NotExpectedException(exception))
                {
                    throw;
                }
                base.Log.LogErrorWithCodeFromResources(null, "", 0, 0, 0, 0, "FindUnderPath.InvalidParameter", new object[] { "Path", this.path.ItemSpec, exception.Message });
                return false;
            }
            int length = str.Length;
            base.Log.LogMessageFromResources(MessageImportance.Low, "FindUnderPath.ComparisonPath", new object[] { this.Path.ItemSpec });
            foreach (ITaskItem item in this.Files)
            {
                string fullPath;
                try
                {
                    fullPath = System.IO.Path.GetFullPath(item.ItemSpec);
                }
                catch (Exception exception2)
                {
                    if (Microsoft.Build.Shared.ExceptionHandling.NotExpectedException(exception2))
                    {
                        throw;
                    }
                    base.Log.LogErrorWithCodeFromResources(null, "", 0, 0, 0, 0, "FindUnderPath.InvalidParameter", new object[] { "Files", item.ItemSpec, exception2.Message });
                    return false;
                }
                if (string.Compare(str, 0, fullPath, 0, length, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    if (this.updateToAbsolutePaths)
                    {
                        item.ItemSpec = Microsoft.Build.Shared.EscapingUtilities.Escape(fullPath);
                    }
                    list.Add(item);
                }
                else
                {
                    list2.Add(item);
                }
            }
            this.InPath = (ITaskItem[]) list.ToArray(typeof(ITaskItem));
            this.OutOfPath = (ITaskItem[]) list2.ToArray(typeof(ITaskItem));
            return true;
        }

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

        [Output]
        public ITaskItem[] InPath
        {
            get
            {
                return this.inPath;
            }
            set
            {
                this.inPath = value;
            }
        }

        [Output]
        public ITaskItem[] OutOfPath
        {
            get
            {
                return this.outOfPath;
            }
            set
            {
                this.outOfPath = value;
            }
        }

        [Required]
        public ITaskItem Path
        {
            get
            {
                return this.path;
            }
            set
            {
                this.path = value;
            }
        }

        public bool UpdateToAbsolutePaths
        {
            get
            {
                return this.updateToAbsolutePaths;
            }
            set
            {
                this.updateToAbsolutePaths = value;
            }
        }
    }
}

