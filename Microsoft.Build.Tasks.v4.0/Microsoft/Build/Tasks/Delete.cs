namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Shared;
    using Microsoft.Build.Utilities;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;

    public sealed class Delete : TaskExtension
    {
        private ITaskItem[] deletedFiles;
        private ITaskItem[] files;
        private bool treatErrorsAsWarnings;

        public override bool Execute()
        {
            ArrayList list = new ArrayList();
            HashSet<string> set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (ITaskItem item in this.Files)
            {
                try
                {
                    if (!set.Contains(item.ItemSpec))
                    {
                        if (File.Exists(item.ItemSpec))
                        {
                            base.Log.LogMessageFromResources(MessageImportance.Normal, "Delete.DeletingFile", new object[] { item.ItemSpec });
                            File.Delete(item.ItemSpec);
                        }
                        else
                        {
                            base.Log.LogMessageFromResources(MessageImportance.Low, "Delete.SkippingNonexistentFile", new object[] { item.ItemSpec });
                        }
                        ITaskItem item2 = new TaskItem(item);
                        list.Add(item2);
                    }
                }
                catch (Exception exception)
                {
                    if (Microsoft.Build.Shared.ExceptionHandling.NotExpectedException(exception))
                    {
                        throw;
                    }
                    this.LogError(item, exception);
                }
                set.Add(item.ItemSpec);
            }
            this.DeletedFiles = (ITaskItem[]) list.ToArray(typeof(ITaskItem));
            return !base.Log.HasLoggedErrors;
        }

        private void LogError(ITaskItem file, Exception e)
        {
            if (this.TreatErrorsAsWarnings)
            {
                base.Log.LogWarningWithCodeFromResources("Delete.Error", new object[] { file.ItemSpec, e.Message });
            }
            else
            {
                base.Log.LogErrorWithCodeFromResources("Delete.Error", new object[] { file.ItemSpec, e.Message });
            }
        }

        [Output]
        public ITaskItem[] DeletedFiles
        {
            get
            {
                return this.deletedFiles;
            }
            set
            {
                this.deletedFiles = value;
            }
        }

        [Required]
        public ITaskItem[] Files
        {
            get
            {
                Microsoft.Build.Shared.ErrorUtilities.VerifyThrowArgumentNull(this.files, "files");
                return this.files;
            }
            set
            {
                this.files = value;
            }
        }

        public bool TreatErrorsAsWarnings
        {
            get
            {
                return this.treatErrorsAsWarnings;
            }
            set
            {
                this.treatErrorsAsWarnings = value;
            }
        }
    }
}

