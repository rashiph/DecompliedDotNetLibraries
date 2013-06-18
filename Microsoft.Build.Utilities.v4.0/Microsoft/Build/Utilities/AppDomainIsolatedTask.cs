namespace Microsoft.Build.Utilities
{
    using Microsoft.Build.Framework;
    using System;
    using System.Resources;
    using System.Runtime;
    using System.Security.Permissions;

    [LoadInSeparateAppDomain]
    public abstract class AppDomainIsolatedTask : MarshalByRefObject, ITask
    {
        private IBuildEngine buildEngine;
        private ITaskHost hostObject;
        private TaskLoggingHelper log;

        protected AppDomainIsolatedTask()
        {
            this.log = new TaskLoggingHelper(this);
        }

        protected AppDomainIsolatedTask(ResourceManager taskResources) : this()
        {
            this.log.TaskResources = taskResources;
        }

        protected AppDomainIsolatedTask(ResourceManager taskResources, string helpKeywordPrefix) : this(taskResources)
        {
            this.log.HelpKeywordPrefix = helpKeywordPrefix;
        }

        public abstract bool Execute();
        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.Infrastructure)]
        public override object InitializeLifetimeService()
        {
            return null;
        }

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

