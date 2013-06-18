namespace System.Workflow.Activities
{
    using System;
    using System.ComponentModel;
    using System.Workflow.ComponentModel.Design;

    [ActivityDesignerTheme(typeof(WebServiceFaultDesignerTheme))]
    internal sealed class WebServiceFaultDesigner : ActivityDesigner
    {
        protected override void OnActivityChanged(ActivityChangedEventArgs e)
        {
            base.OnActivityChanged(e);
            if ((e.Member != null) && (e.Member.Name == "InputActivityName"))
            {
                TypeDescriptor.Refresh(e.Activity);
            }
        }
    }
}

