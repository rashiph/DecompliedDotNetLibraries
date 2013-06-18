namespace System.Workflow.Activities
{
    using System;
    using System.Workflow.ComponentModel.Design;

    [ActivityDesignerTheme(typeof(ConditionedDesignerTheme))]
    internal sealed class IfElseBranchDesigner : SequentialActivityDesigner
    {
        public override bool CanBeParentedTo(CompositeActivityDesigner parentActivityDesigner)
        {
            if (parentActivityDesigner == null)
            {
                throw new ArgumentNullException("parentActivity");
            }
            return ((parentActivityDesigner.Activity is IfElseActivity) && base.CanBeParentedTo(parentActivityDesigner));
        }
    }
}

