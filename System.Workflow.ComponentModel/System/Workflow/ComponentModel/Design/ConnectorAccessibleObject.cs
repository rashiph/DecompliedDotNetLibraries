namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    public class ConnectorAccessibleObject : AccessibleObject
    {
        private Connector connector;

        public ConnectorAccessibleObject(Connector connector)
        {
            if (connector == null)
            {
                throw new ArgumentNullException("connector");
            }
            this.connector = connector;
        }

        public override AccessibleObject HitTest(int x, int y)
        {
            WorkflowView parentView = this.connector.ParentDesigner.ParentView;
            if (this.connector.HitTest(parentView.ScreenPointToLogical(new Point(x, y))))
            {
                return this;
            }
            return null;
        }

        public override Rectangle Bounds
        {
            get
            {
                WorkflowView parentView = this.connector.ParentDesigner.ParentView;
                Rectangle bounds = this.connector.Bounds;
                return new Rectangle(parentView.LogicalPointToScreen(bounds.Location), parentView.LogicalSizeToClient(bounds.Size));
            }
        }

        public override string Name
        {
            get
            {
                return this.connector.GetType().Name;
            }
            set
            {
            }
        }

        public override AccessibleObject Parent
        {
            get
            {
                return this.connector.ParentDesigner.AccessibilityObject;
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

