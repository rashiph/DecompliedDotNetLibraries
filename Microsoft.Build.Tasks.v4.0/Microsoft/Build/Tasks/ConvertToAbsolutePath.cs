namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Shared;
    using System;
    using System.Collections.Generic;
    using System.IO;

    public class ConvertToAbsolutePath : TaskExtension
    {
        private ITaskItem[] absolutePaths;
        private ITaskItem[] paths;

        public override bool Execute()
        {
            List<ITaskItem> list = new List<ITaskItem>();
            foreach (ITaskItem item in this.Paths)
            {
                try
                {
                    if (!Path.IsPathRooted(item.ItemSpec))
                    {
                        if (item is ITaskItem2)
                        {
                            ((ITaskItem2) item).EvaluatedIncludeEscaped = ((ITaskItem2) item).GetMetadataValueEscaped("FullPath");
                        }
                        else
                        {
                            item.ItemSpec = Microsoft.Build.Shared.EscapingUtilities.Escape(item.GetMetadata("FullPath"));
                        }
                    }
                    list.Add(item);
                }
                catch (ArgumentException exception)
                {
                    base.Log.LogErrorWithCodeFromResources("General.InvalidArgument", new object[] { exception.Message });
                }
            }
            this.AbsolutePaths = list.ToArray();
            return !base.Log.HasLoggedErrors;
        }

        [Output]
        public ITaskItem[] AbsolutePaths
        {
            get
            {
                return this.absolutePaths;
            }
            set
            {
                this.absolutePaths = value;
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

