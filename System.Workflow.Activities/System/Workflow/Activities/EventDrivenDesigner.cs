namespace System.Workflow.Activities
{
    using System;
    using System.Workflow.ComponentModel.Design;

    [ActivityDesignerTheme(typeof(EventDrivenDesignerTheme))]
    internal sealed class EventDrivenDesigner : System.Workflow.Activities.SequenceDesigner
    {
        public override bool CanBeParentedTo(CompositeActivityDesigner parentActivityDesigner)
        {
            if (parentActivityDesigner == null)
            {
                throw new ArgumentNullException("parentActivity");
            }
            if ((!Type.GetType("System.Workflow.Activities.ListenActivity,System.Workflow.Activities, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35").IsAssignableFrom(parentActivityDesigner.Activity.GetType()) && !(parentActivityDesigner.Activity is EventHandlersActivity)) && !Type.GetType("System.Workflow.Activities.StateActivity,System.Workflow.Activities, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35").IsAssignableFrom(parentActivityDesigner.Activity.GetType()))
            {
                return false;
            }
            return base.CanBeParentedTo(parentActivityDesigner);
        }

        protected override void DoDefaultAction()
        {
            base.DoDefaultAction();
            base.EnsureVisible();
        }

        public override bool CanExpandCollapse
        {
            get
            {
                if (base.ParentDesigner is StateDesigner)
                {
                    return false;
                }
                return base.CanExpandCollapse;
            }
        }
    }
}

