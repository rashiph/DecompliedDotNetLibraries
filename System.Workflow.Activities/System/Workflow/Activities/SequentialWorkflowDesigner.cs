namespace System.Workflow.Activities
{
    using System;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;

    [ActivityDesignerTheme(typeof(SequentialWorkflowDesignerTheme))]
    internal class SequentialWorkflowDesigner : SequentialWorkflowRootDesigner
    {
        protected override void Initialize(Activity activity)
        {
            base.Initialize(activity);
            this.HelpText = System.Workflow.Activities.DR.GetString("SequentialWorkflowHelpText");
            this.Header.Text = System.Workflow.Activities.DR.GetString("StartSequentialWorkflow");
        }

        protected override bool IsSupportedActivityType(Type activityType)
        {
            return (((!typeof(SetStateActivity).IsAssignableFrom(activityType) && !typeof(StateActivity).IsAssignableFrom(activityType)) && (!typeof(StateInitializationActivity).IsAssignableFrom(activityType) && !typeof(StateFinalizationActivity).IsAssignableFrom(activityType))) && base.IsSupportedActivityType(activityType));
        }

        protected override void OnViewChanged(DesignerView view)
        {
            base.OnViewChanged(view);
            ActivityDesigner designer = (base.ActiveView != null) ? base.ActiveView.AssociatedDesigner : null;
            if (designer.Activity is FaultHandlersActivity)
            {
                this.Header.Text = System.Workflow.Activities.DR.GetString("WorkflowExceptions");
                this.HelpText = string.Empty;
            }
            else if (designer.Activity is EventHandlersActivity)
            {
                this.Header.Text = System.Workflow.Activities.DR.GetString("WorkflowEvents");
                this.HelpText = string.Empty;
            }
            else if (designer.Activity is CompensationHandlerActivity)
            {
                this.Header.Text = System.Workflow.Activities.DR.GetString("WorkflowCompensation");
                this.HelpText = string.Empty;
            }
            else if (designer.Activity is CancellationHandlerActivity)
            {
                this.Header.Text = System.Workflow.Activities.DR.GetString("WorkflowCancellation");
                this.HelpText = string.Empty;
            }
            else
            {
                this.Header.Text = System.Workflow.Activities.DR.GetString("StartSequentialWorkflow");
                this.HelpText = System.Workflow.Activities.DR.GetString("SequentialWorkflowHelpText");
            }
        }
    }
}

