namespace System.Workflow.ComponentModel
{
    using System;
    using System.Workflow.ComponentModel.Design;

    [ActivityDesignerTheme(typeof(CompensateDesignerTheme))]
    internal sealed class CompensateDesigner : ActivityDesigner
    {
        public override bool CanBeParentedTo(CompositeActivityDesigner parentActivityDesigner)
        {
            for (Activity activity = parentActivityDesigner.Activity; activity != null; activity = activity.Parent)
            {
                if (((activity is CancellationHandlerActivity) || (activity is CompensationHandlerActivity)) || (activity is FaultHandlerActivity))
                {
                    return true;
                }
            }
            return false;
        }
    }
}

