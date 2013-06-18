namespace System.Activities.Validation
{
    using System;
    using System.Activities;
    using System.Collections.Generic;

    public sealed class ValidationContext
    {
        private LocationReferenceEnvironment environment;
        private IList<ValidationError> getChildrenErrors;
        private ProcessActivityTreeOptions options;
        private ActivityUtilities.ChildActivity owner;
        private ActivityUtilities.ActivityCallStack parentChain;

        internal ValidationContext(ActivityUtilities.ChildActivity owner, ActivityUtilities.ActivityCallStack parentChain, ProcessActivityTreeOptions options, LocationReferenceEnvironment environment)
        {
            this.owner = owner;
            this.parentChain = parentChain;
            this.options = options;
            this.environment = environment;
        }

        internal void AddGetChildrenErrors(ref IList<ValidationError> validationErrors)
        {
            if ((this.getChildrenErrors != null) && (this.getChildrenErrors.Count > 0))
            {
                if (validationErrors == null)
                {
                    validationErrors = new List<ValidationError>();
                }
                for (int i = 0; i < this.getChildrenErrors.Count; i++)
                {
                    validationErrors.Add(this.getChildrenErrors[i]);
                }
                this.getChildrenErrors = null;
            }
        }

        internal IEnumerable<Activity> GetChildren()
        {
            if (!this.owner.Equals(ActivityUtilities.ChildActivity.Empty))
            {
                return ActivityValidationServices.GetChildren(this.owner, this.parentChain, this.options);
            }
            return ActivityValidationServices.EmptyChildren;
        }

        internal IEnumerable<Activity> GetParents()
        {
            List<Activity> list = new List<Activity>();
            for (int i = 0; i < this.parentChain.Count; i++)
            {
                ActivityUtilities.ChildActivity activity = this.parentChain[i];
                list.Add(activity.Activity);
            }
            return list;
        }

        internal IEnumerable<Activity> GetWorkflowTree()
        {
            Activity parent = this.owner.Activity;
            if (parent == null)
            {
                return ActivityValidationServices.EmptyChildren;
            }
            while (parent.Parent != null)
            {
                parent = parent.Parent;
            }
            List<Activity> list = ActivityValidationServices.GetChildren(new ActivityUtilities.ChildActivity(parent, true), new ActivityUtilities.ActivityCallStack(), this.options);
            list.Add(parent);
            return list;
        }

        internal LocationReferenceEnvironment Environment
        {
            get
            {
                return this.environment;
            }
        }
    }
}

