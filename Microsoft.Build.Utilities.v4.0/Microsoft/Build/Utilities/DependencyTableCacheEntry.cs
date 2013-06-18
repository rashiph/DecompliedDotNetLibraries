namespace Microsoft.Build.Utilities
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Shared;
    using System;
    using System.Collections;
    using System.Runtime;

    internal class DependencyTableCacheEntry
    {
        private IDictionary dependencyTable;
        private DateTime tableTime;
        private ITaskItem[] tlogFiles;

        internal DependencyTableCacheEntry(ITaskItem[] tlogFiles, IDictionary dependencyTable)
        {
            this.tlogFiles = new ITaskItem[tlogFiles.Length];
            this.tableTime = DateTime.MinValue;
            for (int i = 0; i < tlogFiles.Length; i++)
            {
                string itemSpec = FileUtilities.NormalizePath(tlogFiles[i].ItemSpec);
                this.tlogFiles[i] = new TaskItem(itemSpec);
                DateTime lastWriteTimeUtc = NativeMethods.GetLastWriteTimeUtc(itemSpec);
                if (lastWriteTimeUtc > this.tableTime)
                {
                    this.tableTime = lastWriteTimeUtc;
                }
            }
            this.dependencyTable = dependencyTable;
        }

        public IDictionary DependencyTable
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.dependencyTable;
            }
        }

        public DateTime TableTime
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.tableTime;
            }
        }

        public ITaskItem[] TlogFiles
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.tlogFiles;
            }
        }
    }
}

