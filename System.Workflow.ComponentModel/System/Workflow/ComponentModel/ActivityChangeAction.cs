namespace System.Workflow.ComponentModel
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Runtime;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.ComponentModel.Serialization;

    [DesignerSerializer(typeof(ActivityChangeActionMarkupSerializer), typeof(WorkflowMarkupSerializer))]
    public abstract class ActivityChangeAction : WorkflowChangeAction
    {
        private string ownerActivityDottedPath;

        protected ActivityChangeAction()
        {
            this.ownerActivityDottedPath = string.Empty;
        }

        protected ActivityChangeAction(CompositeActivity compositeActivity)
        {
            this.ownerActivityDottedPath = string.Empty;
            if (compositeActivity == null)
            {
                throw new ArgumentNullException("compositeActivity");
            }
            this.ownerActivityDottedPath = compositeActivity.DottedPath;
        }

        protected internal override ValidationErrorCollection ValidateChanges(Activity contextActivity)
        {
            if (contextActivity == null)
            {
                throw new ArgumentNullException("contextActivity");
            }
            ValidationErrorCollection errors = new ValidationErrorCollection();
            CompositeActivity activity = contextActivity.TraverseDottedPathFromRoot(this.OwnerActivityDottedPath) as CompositeActivity;
            if ((activity != null) && WorkflowChanges.IsActivityExecutable(activity))
            {
                foreach (Validator validator in ComponentDispenser.CreateComponents(activity.GetType(), typeof(ActivityValidatorAttribute)))
                {
                    ValidationError item = validator.ValidateActivityChange(activity, this);
                    if (item != null)
                    {
                        errors.Add(item);
                    }
                }
            }
            return errors;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string OwnerActivityDottedPath
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.ownerActivityDottedPath;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            internal set
            {
                this.ownerActivityDottedPath = value;
            }
        }
    }
}

