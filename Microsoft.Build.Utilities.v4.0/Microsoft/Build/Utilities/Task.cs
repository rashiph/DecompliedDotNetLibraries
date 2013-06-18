namespace Microsoft.Build.Utilities
{
    using Microsoft.Build.Framework;
    using System;
    using System.Resources;
    using System.Runtime;

    public abstract class Task : ITask
    {
        private IBuildEngine buildEngine;
        private ITaskHost hostObject;
        private TaskLoggingHelper log;

        protected Task()
        {
            this.log = new TaskLoggingHelper(this);
        }

        protected Task(ResourceManager taskResources) : this()
        {
            this.log.TaskResources = taskResources;
        }

        protected Task(ResourceManager taskResources, string helpKeywordPrefix) : this(taskResources)
        {
            this.log.HelpKeywordPrefix = helpKeywordPrefix;
        }

        public abstract bool Execute();

        public IBuildEngine BuildEngine
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.buildEngine;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.buildEngine = value;
            }
        }

        public IBuildEngine2 BuildEngine2
        {
            get
            {
                return (IBuildEngine2) this.buildEngine;
            }
        }

        public IBuildEngine3 BuildEngine3
        {
            get
            {
                return (IBuildEngine3) this.buildEngine;
            }
        }

        protected string HelpKeywordPrefix
        {
            get
            {
                return this.Log.HelpKeywordPrefix;
            }
            set
            {
                this.Log.HelpKeywordPrefix = value;
            }
        }

        public ITaskHost HostObject
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.hostObject;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.hostObject = value;
            }
        }

        public TaskLoggingHelper Log
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.log;
            }
        }

        protected ResourceManager TaskResources
        {
            get
            {
                return this.Log.TaskResources;
            }
            set
            {
                this.Log.TaskResources = value;
            }
        }
    }
}

