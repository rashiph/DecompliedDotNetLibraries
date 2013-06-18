namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Shared;
    using Microsoft.Build.Utilities;
    using System;

    public abstract class TaskExtension : Task
    {
        private TaskLoggingHelperExtension logExtension;

        internal TaskExtension() : base(Microsoft.Build.Shared.AssemblyResources.PrimaryResources, "MSBuild.")
        {
            this.logExtension = new TaskLoggingHelperExtension(this, Microsoft.Build.Shared.AssemblyResources.PrimaryResources, Microsoft.Build.Shared.AssemblyResources.SharedResources, "MSBuild.");
        }

        public TaskLoggingHelper Log
        {
            get
            {
                return this.logExtension;
            }
        }
    }
}

