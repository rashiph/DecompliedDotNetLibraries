namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel.Design;
    using System.Runtime;
    using System.Windows.Forms;

    internal sealed class DragSelectionMessageFilter : DragRectangleMessageFilter
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal DragSelectionMessageFilter()
        {
        }

        protected override bool OnKeyUp(KeyEventArgs eventArgs)
        {
            base.OnKeyUp(eventArgs);
            if ((Control.ModifierKeys & Keys.Shift) == Keys.None)
            {
                base.DragStarted = false;
            }
            return false;
        }

        protected override bool OnMouseDown(MouseEventArgs eventArgs)
        {
            if ((Control.ModifierKeys & Keys.Shift) > Keys.None)
            {
                base.OnMouseDown(eventArgs);
                return true;
            }
            return false;
        }

        protected override bool OnMouseMove(MouseEventArgs eventArgs)
        {
            if ((Control.ModifierKeys & Keys.Shift) > Keys.None)
            {
                base.OnMouseMove(eventArgs);
                return true;
            }
            return false;
        }

        protected override bool OnMouseUp(MouseEventArgs eventArgs)
        {
            if ((Control.ModifierKeys & Keys.Shift) <= Keys.None)
            {
                return false;
            }
            base.OnMouseUp(eventArgs);
            WorkflowView parentView = base.ParentView;
            if (!base.DragRectangle.IsEmpty && (parentView.RootDesigner != null))
            {
                ActivityDesigner[] intersectingDesigners = CompositeActivityDesigner.GetIntersectingDesigners(parentView.RootDesigner, base.DragRectangle);
                ArrayList list = new ArrayList();
                foreach (ActivityDesigner designer in intersectingDesigners)
                {
                    list.Add(designer.Activity);
                }
                ISelectionService service = base.GetService(typeof(ISelectionService)) as ISelectionService;
                if ((list.Count > 0) && (service != null))
                {
                    service.SetSelectedComponents((object[]) list.ToArray(typeof(object)), SelectionTypes.Replace);
                }
            }
            return true;
        }
    }
}

