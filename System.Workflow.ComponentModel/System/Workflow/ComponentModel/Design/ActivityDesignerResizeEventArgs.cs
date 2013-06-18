namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Drawing;
    using System.Runtime;

    public class ActivityDesignerResizeEventArgs : EventArgs
    {
        private Rectangle newBounds;
        private DesignerEdges sizingEdge;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ActivityDesignerResizeEventArgs(DesignerEdges sizingEdge, Rectangle newBounds)
        {
            this.sizingEdge = sizingEdge;
            this.newBounds = newBounds;
        }

        public Rectangle Bounds
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.newBounds;
            }
        }

        public DesignerEdges SizingEdge
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.sizingEdge;
            }
        }
    }
}

