namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Framework;
    using System;
    using System.Collections;
    using System.IO;

    internal sealed class TaskItemSpecFilenameComparer : IComparer
    {
        internal static readonly IComparer comparer = new TaskItemSpecFilenameComparer();

        private TaskItemSpecFilenameComparer()
        {
        }

        public int Compare(object o1, object o2)
        {
            if (object.ReferenceEquals(o1, o2))
            {
                return 0;
            }
            ITaskItem item = (ITaskItem) o1;
            ITaskItem item2 = (ITaskItem) o2;
            string fileName = Path.GetFileName(item.ItemSpec);
            string strB = Path.GetFileName(item2.ItemSpec);
            int num = string.Compare(fileName, strB, StringComparison.OrdinalIgnoreCase);
            if (num != 0)
            {
                return num;
            }
            return string.Compare(item.ItemSpec, item2.ItemSpec, StringComparison.OrdinalIgnoreCase);
        }
    }
}

