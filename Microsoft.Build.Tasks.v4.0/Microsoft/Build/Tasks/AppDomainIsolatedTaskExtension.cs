namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Shared;
    using Microsoft.Build.Utilities;
    using System;

    [LoadInSeparateAppDomain]
    public abstract class AppDomainIsolatedTaskExtension : AppDomainIsolatedTask
    {
        private TaskLoggingHelperExtension logExtension;

        internal AppDomainIsolatedTaskExtension() : base(Microsoft.Build.Shared.AssemblyResources.PrimaryResources, "MSBuild.")
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

