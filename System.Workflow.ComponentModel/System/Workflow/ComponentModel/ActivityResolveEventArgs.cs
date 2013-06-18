namespace System.Workflow.ComponentModel
{
    using System;
    using System.Runtime;

    internal sealed class ActivityResolveEventArgs : EventArgs
    {
        private string activityDefinition;
        private System.Type activityType;
        private bool createNew;
        private bool initForRuntime = true;
        private string rulesDefinition;
        private IServiceProvider serviceProvider;

        internal ActivityResolveEventArgs(System.Type activityType, string workflowMarkup, string rulesMarkup, bool createNew, bool initForRuntime, IServiceProvider serviceProvider)
        {
            if (!(string.IsNullOrEmpty(workflowMarkup) ^ (activityType == null)))
            {
                throw new ArgumentException(SR.GetString("Error_WrongParamForActivityResolveEventArgs"));
            }
            this.activityType = activityType;
            this.activityDefinition = workflowMarkup;
            this.rulesDefinition = rulesMarkup;
            this.createNew = createNew;
            this.initForRuntime = initForRuntime;
            this.serviceProvider = serviceProvider;
        }

        public bool CreateNewDefinition
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.createNew;
            }
        }

        public bool InitializeForRuntime
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.initForRuntime;
            }
        }

        public string RulesMarkup
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.rulesDefinition;
            }
        }

        public IServiceProvider ServiceProvider
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.serviceProvider;
            }
        }

        public System.Type Type
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.activityType;
            }
        }

        public string WorkflowMarkup
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.activityDefinition;
            }
        }
    }
}

