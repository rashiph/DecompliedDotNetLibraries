namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Workflow.ComponentModel;

    [ActivityDesignerTheme(typeof(FaultHandlersActivityDesignerTheme))]
    internal sealed class FaultHandlersActivityDesigner : ActivityPreviewDesigner
    {
        public override bool CanInsertActivities(HitTestInfo insertLocation, ReadOnlyCollection<Activity> activitiesToInsert)
        {
            foreach (Activity activity in activitiesToInsert)
            {
                if (!(activity is FaultHandlerActivity))
                {
                    return false;
                }
            }
            return base.CanInsertActivities(insertLocation, activitiesToInsert);
        }

        protected override void Initialize(Activity activity)
        {
            base.Initialize(activity);
            this.HelpText = DR.GetString("DropExceptionsHere", new object[0]);
            base.ShowPreview = false;
        }

        public override bool CanExpandCollapse
        {
            get
            {
                return false;
            }
        }

        public override object FirstSelectableObject
        {
            get
            {
                if (this.Expanded && this.IsVisible)
                {
                    if ((base.PreviewedDesigner != null) || (this.ContainedDesigners.Count > 0))
                    {
                        return base.FirstSelectableObject;
                    }
                    if (this.ContainedDesigners.Count == 0)
                    {
                        return new ConnectorHitTestInfo(this, HitTestLocations.Designer, 0).SelectableObject;
                    }
                }
                return null;
            }
        }

        public override object LastSelectableObject
        {
            get
            {
                if (this.Expanded && this.IsVisible)
                {
                    if ((base.PreviewedDesigner != null) || (this.ContainedDesigners.Count > 0))
                    {
                        return base.LastSelectableObject;
                    }
                    if (this.ContainedDesigners.Count == 0)
                    {
                        return new ConnectorHitTestInfo(this, HitTestLocations.Designer, this.GetConnectors().GetLength(0) - 1).SelectableObject;
                    }
                }
                return null;
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

