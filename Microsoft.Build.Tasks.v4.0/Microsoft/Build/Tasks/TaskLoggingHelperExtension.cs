namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Shared;
    using Microsoft.Build.Utilities;
    using System;
    using System.Globalization;
    using System.Resources;

    public class TaskLoggingHelperExtension : TaskLoggingHelper
    {
        private ResourceManager taskSharedResources;

        private TaskLoggingHelperExtension() : base(null)
        {
        }

        public TaskLoggingHelperExtension(ITask taskInstance, ResourceManager primaryResources, ResourceManager sharedResources, string helpKeywordPrefix) : base(taskInstance)
        {
            base.TaskResources = primaryResources;
            this.TaskSharedResources = sharedResources;
            base.HelpKeywordPrefix = helpKeywordPrefix;
        }

        public override string FormatResourceString(string resourceName, params object[] args)
        {
            Microsoft.Build.Shared.ErrorUtilities.VerifyThrowArgumentNull(resourceName, "resourceName");
            Microsoft.Build.Shared.ErrorUtilities.VerifyThrowInvalidOperation(base.TaskResources != null, "Shared.TaskResourcesNotRegistered", base.TaskName);
            Microsoft.Build.Shared.ErrorUtilities.VerifyThrowInvalidOperation(this.TaskSharedResources != null, "Shared.TaskResourcesNotRegistered", base.TaskName);
            string unformatted = base.TaskResources.GetString(resourceName, CultureInfo.CurrentUICulture);
            if (unformatted == null)
            {
                unformatted = this.TaskSharedResources.GetString(resourceName, CultureInfo.CurrentUICulture);
            }
            Microsoft.Build.Shared.ErrorUtilities.VerifyThrowArgument(unformatted != null, "Shared.TaskResourceNotFound", resourceName, base.TaskName);
            return this.FormatString(unformatted, args);
        }

        public ResourceManager TaskSharedResources
        {
            get
            {
                return this.taskSharedResources;
            }
            set
            {
                this.taskSharedResources = value;
            }
        }
    }
}

