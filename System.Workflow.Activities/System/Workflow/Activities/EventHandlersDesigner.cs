namespace System.Workflow.Activities
{
    using System;
    using System.Collections.ObjectModel;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;

    [ActivityDesignerTheme(typeof(EventHandlersDesignerTheme))]
    internal sealed class EventHandlersDesigner : ActivityPreviewDesigner
    {
        public override bool CanBeParentedTo(CompositeActivityDesigner parentActivityDesigner)
        {
            if (parentActivityDesigner == null)
            {
                throw new ArgumentNullException("parentActivity");
            }
            if ((parentActivityDesigner.Activity != null) && !(parentActivityDesigner.Activity is EventHandlingScopeActivity))
            {
                return false;
            }
            return base.CanBeParentedTo(parentActivityDesigner);
        }

        public override bool CanInsertActivities(HitTestInfo insertLocation, ReadOnlyCollection<Activity> activitiesToInsert)
        {
            foreach (Activity activity in activitiesToInsert)
            {
                if (!(activity is EventDrivenActivity))
                {
                    return false;
                }
            }
            return base.CanInsertActivities(insertLocation, activitiesToInsert);
        }

        protected override void Initialize(Activity activity)
        {
            base.Initialize(activity);
            this.HelpText = System.Workflow.Activities.DR.GetString("DropEventsHere");
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
    }
}

