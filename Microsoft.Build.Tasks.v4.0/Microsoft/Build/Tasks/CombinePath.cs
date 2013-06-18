namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Shared;
    using Microsoft.Build.Utilities;
    using System;
    using System.Collections.Generic;
    using System.IO;

    public class CombinePath : TaskExtension
    {
        private string basePath;
        private ITaskItem[] combinedPaths;
        private ITaskItem[] paths;

        public override bool Execute()
        {
            if (this.BasePath == null)
            {
                this.BasePath = string.Empty;
            }
            List<ITaskItem> list = new List<ITaskItem>();
            foreach (ITaskItem item in this.Paths)
            {
                TaskItem item2 = new TaskItem(item);
                try
                {
                    item2.ItemSpec = Path.Combine(this.basePath, item.ItemSpec);
                    list.Add(item2);
                }
                catch (ArgumentException exception)
                {
                    base.Log.LogErrorWithCodeFromResources("General.InvalidArgument", new object[] { exception.Message });
                }
            }
            this.CombinedPaths = list.ToArray();
            return !base.Log.HasLoggedErrors;
        }

        public string BasePath
        {
            get
            {
                return this.basePath;
            }
            set
            {
                this.basePath = value;
            }
        }

        [Output]
        public ITaskItem[] CombinedPaths
        {
            get
            {
                return this.combinedPaths;
            }
            set
            {
                this.combinedPaths = value;
            }
        }

        [Required]
        public ITaskItem[] Paths
        {
            get
            {
                Microsoft.Build.Shared.ErrorUtilities.VerifyThrowArgumentNull(this.paths, "paths");
                return this.paths;
            }
            set
            {
                this.paths = value;
            }
        }
    }
}

