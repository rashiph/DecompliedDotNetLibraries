namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Drawing;
    using System.Runtime;
    using System.Windows.Forms;

    internal sealed class AutoExpandingMessageFilter : WorkflowDesignerMessageFilter
    {
        private CompositeActivityDesigner autoExpandableDesigner;
        private EventHandler autoExpandEventHandler;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal AutoExpandingMessageFilter()
        {
        }

        private void OnAutoExpand(object sender, EventArgs eventArgs)
        {
            if (this.autoExpandableDesigner != null)
            {
                this.autoExpandableDesigner.Expanded = true;
                base.ParentView.PerformLayout(true);
            }
            this.SetAutoExpandableDesigner(null);
        }

        protected override bool OnDragDrop(DragEventArgs eventArgs)
        {
            this.SetAutoExpandableDesigner(null);
            return false;
        }

        protected override bool OnDragEnter(DragEventArgs eventArgs)
        {
            WorkflowView parentView = base.ParentView;
            if (parentView.IsClientPointInActiveLayout(parentView.PointToClient(new Point(eventArgs.X, eventArgs.Y))))
            {
                this.SetAutoExpandableDesigner(parentView.MessageHitTestContext.AssociatedDesigner as CompositeActivityDesigner);
            }
            else
            {
                this.SetAutoExpandableDesigner(null);
            }
            return false;
        }

        protected override bool OnDragLeave()
        {
            this.SetAutoExpandableDesigner(null);
            return false;
        }

        protected override bool OnDragOver(DragEventArgs eventArgs)
        {
            WorkflowView parentView = base.ParentView;
            if (parentView.IsClientPointInActiveLayout(parentView.PointToClient(new Point(eventArgs.X, eventArgs.Y))))
            {
                this.SetAutoExpandableDesigner(parentView.MessageHitTestContext.AssociatedDesigner as CompositeActivityDesigner);
            }
            else
            {
                this.SetAutoExpandableDesigner(null);
            }
            return false;
        }

        private void SetAutoExpandableDesigner(CompositeActivityDesigner value)
        {
            if (this.autoExpandableDesigner != value)
            {
                if (((value == null) || value.Expanded) || !value.CanExpandCollapse)
                {
                    this.autoExpandableDesigner = null;
                    if (this.autoExpandEventHandler != null)
                    {
                        WorkflowTimer.Default.Unsubscribe(this.autoExpandEventHandler);
                        this.autoExpandEventHandler = null;
                    }
                }
                else
                {
                    this.autoExpandableDesigner = value;
                    if (this.autoExpandEventHandler == null)
                    {
                        this.autoExpandEventHandler = new EventHandler(this.OnAutoExpand);
                        WorkflowTimer.Default.Subscribe(500, this.autoExpandEventHandler);
                    }
                }
            }
        }
    }
}

