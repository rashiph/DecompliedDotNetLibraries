namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Framework;
    using System;
    using System.Collections;

    public class RemoveDuplicates : TaskExtension
    {
        private ITaskItem[] filtered;
        private ITaskItem[] inputs = new TaskItem[0];

        public override bool Execute()
        {
            Hashtable hashtable = new Hashtable(this.Inputs.Length, StringComparer.OrdinalIgnoreCase);
            ArrayList list = new ArrayList();
            foreach (ITaskItem item in this.Inputs)
            {
                if (!hashtable.ContainsKey(item.ItemSpec))
                {
                    hashtable[item.ItemSpec] = string.Empty;
                    list.Add(item);
                }
            }
            this.Filtered = (ITaskItem[]) list.ToArray(typeof(ITaskItem));
            return true;
        }

        [Output]
        public ITaskItem[] Filtered
        {
            get
            {
                return this.filtered;
            }
            set
            {
                this.filtered = value;
            }
        }

        public ITaskItem[] Inputs
        {
            get
            {
                return this.inputs;
            }
            set
            {
                this.inputs = value;
            }
        }
    }
}

