namespace System.Workflow.ComponentModel.Compiler
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;
    using System;
    using System.IO;
    using System.Runtime;

    public sealed class CompileWorkflowCleanupTask : Task, ITask
    {
        private ITaskItem[] temporaryFiles;

        public CompileWorkflowCleanupTask() : base(new ResourceManager("System.Workflow.ComponentModel.BuildTasksStrings", Assembly.GetExecutingAssembly()))
        {
        }

        public override bool Execute()
        {
            if (this.temporaryFiles != null)
            {
                foreach (ITaskItem item in this.temporaryFiles)
                {
                    string itemSpec = item.ItemSpec;
                    if (File.Exists(itemSpec))
                    {
                        File.Open(itemSpec, FileMode.Truncate).Close();
                    }
                }
            }
            return true;
        }

        public ITaskItem[] TemporaryFiles
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.temporaryFiles;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.temporaryFiles = value;
            }
        }
    }
}

