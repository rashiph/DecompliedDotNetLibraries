namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Workflow.ComponentModel;

    [ActivityDesignerTheme(typeof(CancellationDesignerTheme))]
    internal sealed class CancellationHandlerActivityDesigner : SequentialActivityDesigner
    {
        public override bool CanInsertActivities(HitTestInfo insertLocation, ReadOnlyCollection<Activity> activitiesToInsert)
        {
            foreach (Activity activity in activitiesToInsert)
            {
                if (Helpers.IsFrameworkActivity(activity))
                {
                    return false;
                }
            }
            return base.CanInsertActivities(insertLocation, activitiesToInsert);
        }

        public override bool CanExpandCollapse
        {
            get
            {
                return false;
            }
        }

        public override ReadOnlyCollection<DesignerView> Views
        {
            get
            {
                List<DesignerView> list = new List<DesignerView>();
                foreach (DesignerView view in base.Views)
                {
                    if (((view.ViewId != 2) && (view.ViewId != 3)) && (view.ViewId != 4))
                    {
                        list.Add(view);
                    }
                }
                return new ReadOnlyCollection<DesignerView>(list);
            }
        }
    }
}

