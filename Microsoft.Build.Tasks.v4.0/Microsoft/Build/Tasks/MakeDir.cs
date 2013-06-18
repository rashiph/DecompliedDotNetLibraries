namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Shared;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;

    public class MakeDir : TaskExtension
    {
        private ITaskItem[] directories;
        private ITaskItem[] directoriesCreated;

        public override bool Execute()
        {
            ArrayList list = new ArrayList();
            HashSet<string> set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (ITaskItem item in this.Directories)
            {
                if (item.ItemSpec.Length > 0)
                {
                    try
                    {
                        if (!set.Contains(item.ItemSpec))
                        {
                            if (!Directory.Exists(item.ItemSpec))
                            {
                                base.Log.LogMessageFromResources(MessageImportance.Normal, "MakeDir.Comment", new object[] { item.ItemSpec });
                                Directory.CreateDirectory(item.ItemSpec);
                            }
                            list.Add(item);
                        }
                    }
                    catch (Exception exception)
                    {
                        if (Microsoft.Build.Shared.ExceptionHandling.NotExpectedException(exception))
                        {
                            throw;
                        }
                        base.Log.LogErrorWithCodeFromResources("MakeDir.Error", new object[] { item.ItemSpec, exception.Message });
                    }
                    set.Add(item.ItemSpec);
                }
            }
            this.directoriesCreated = (ITaskItem[]) list.ToArray(typeof(ITaskItem));
            return !base.Log.HasLoggedErrors;
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
        public ITaskItem[] DirectoriesCreated
        {
            get
            {
                return this.directoriesCreated;
            }
        }
    }
}

