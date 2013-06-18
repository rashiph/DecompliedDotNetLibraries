namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    public class WorkflowViewAccessibleObject : Control.ControlAccessibleObject
    {
        private WorkflowView workflowView;

        public WorkflowViewAccessibleObject(WorkflowView workflowView) : base(workflowView)
        {
            if (workflowView == null)
            {
                throw new ArgumentNullException("workflowView");
            }
            this.workflowView = workflowView;
        }

        public override AccessibleObject GetChild(int index)
        {
            if ((this.workflowView.RootDesigner != null) && (index == 0))
            {
                return this.workflowView.RootDesigner.AccessibilityObject;
            }
            return base.GetChild(index);
        }

        public override int GetChildCount()
        {
            if (this.workflowView.RootDesigner == null)
            {
                return -1;
            }
            return 1;
        }

        public override AccessibleObject Navigate(AccessibleNavigation navdir)
        {
            if ((navdir != AccessibleNavigation.FirstChild) && (navdir != AccessibleNavigation.LastChild))
            {
                return base.Navigate(navdir);
            }
            return this.GetChild(0);
        }

        public override Rectangle Bounds
        {
            get
            {
                return new Rectangle(this.workflowView.PointToScreen(Point.Empty), this.workflowView.ViewPortSize);
            }
        }

        public override string DefaultAction
        {
            get
            {
                return DR.GetString("AccessibleAction", new object[0]);
            }
        }

        public override string Description
        {
            get
            {
                return DR.GetString("WorkflowViewAccessibleDescription", new object[0]);
            }
        }

        public override string Help
        {
            get
            {
                return DR.GetString("WorkflowViewAccessibleHelp", new object[0]);
            }
        }

        public override string Name
        {
            get
            {
                return DR.GetString("WorkflowViewAccessibleName", new object[0]);
            }
            set
            {
            }
        }

        public override AccessibleRole Role
        {
            get
            {
                return AccessibleRole.Diagram;
            }
        }
    }
}

