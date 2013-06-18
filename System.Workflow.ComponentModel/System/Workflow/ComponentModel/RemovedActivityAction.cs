namespace System.Workflow.ComponentModel
{
    using System;
    using System.ComponentModel;
    using System.Runtime;
    using System.Workflow.ComponentModel.Compiler;

    public sealed class RemovedActivityAction : ActivityChangeAction
    {
        private Activity originalRemovedActivity;
        private int removedActivityIndex;

        public RemovedActivityAction()
        {
            this.removedActivityIndex = -1;
        }

        public RemovedActivityAction(int removedActivityIndex, Activity originalActivity, CompositeActivity clonedParentActivity) : base(clonedParentActivity)
        {
            this.removedActivityIndex = -1;
            if (originalActivity == null)
            {
                throw new ArgumentNullException("originalActivity");
            }
            if (clonedParentActivity == null)
            {
                throw new ArgumentNullException("clonedParentActivity");
            }
            this.originalRemovedActivity = originalActivity;
            this.removedActivityIndex = removedActivityIndex;
        }

        protected internal override bool ApplyTo(Activity rootActivity)
        {
            if (rootActivity == null)
            {
                throw new ArgumentNullException("rootActivity");
            }
            if (!(rootActivity is CompositeActivity))
            {
                throw new ArgumentException(SR.GetString("Error_RootActivityTypeInvalid"), "rootActivity");
            }
            CompositeActivity activity = rootActivity.TraverseDottedPathFromRoot(base.OwnerActivityDottedPath) as CompositeActivity;
            if (activity == null)
            {
                return false;
            }
            if (this.removedActivityIndex >= activity.Activities.Count)
            {
                return false;
            }
            activity.DynamicUpdateMode = true;
            try
            {
                this.originalRemovedActivity = activity.Activities[this.removedActivityIndex];
                activity.Activities.RemoveAt(this.removedActivityIndex);
            }
            finally
            {
                activity.DynamicUpdateMode = false;
            }
            return true;
        }

        protected internal override ValidationErrorCollection ValidateChanges(Activity contextActivity)
        {
            ValidationErrorCollection errors = base.ValidateChanges(contextActivity);
            Activity activity = contextActivity.TraverseDottedPathFromRoot(this.originalRemovedActivity.DottedPath);
            if (WorkflowChanges.IsActivityExecutable(activity) && (activity.ExecutionStatus == ActivityExecutionStatus.Executing))
            {
                errors.Add(new ValidationError(SR.GetString("Error_RemoveExecutingActivity", new object[] { this.originalRemovedActivity.QualifiedName }), 0x11d));
            }
            return errors;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Activity OriginalRemovedActivity
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.originalRemovedActivity;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            internal set
            {
                this.originalRemovedActivity = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public int RemovedActivityIndex
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.removedActivityIndex;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            internal set
            {
                this.removedActivityIndex = value;
            }
        }
    }
}

