namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Drawing;
    using System.Runtime;

    public class ActivityDesignerPaintEventArgs : EventArgs
    {
        private Rectangle clipRectangle;
        private ActivityDesignerTheme designerTheme;
        private System.Drawing.Graphics graphics;
        private Rectangle viewPort;

        public ActivityDesignerPaintEventArgs(System.Drawing.Graphics graphics, Rectangle clipRectangle, Rectangle viewPort, ActivityDesignerTheme designerTheme)
        {
            this.graphics = graphics;
            this.clipRectangle = Rectangle.Inflate(clipRectangle, 1, 1);
            this.viewPort = viewPort;
            this.designerTheme = designerTheme;
        }

        public System.Workflow.ComponentModel.Design.AmbientTheme AmbientTheme
        {
            get
            {
                return WorkflowTheme.CurrentTheme.AmbientTheme;
            }
        }

        public Rectangle ClipRectangle
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.clipRectangle;
            }
        }

        public ActivityDesignerTheme DesignerTheme
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.designerTheme;
            }
        }

        public System.Drawing.Graphics Graphics
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.graphics;
            }
        }

        internal Rectangle ViewPort
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.viewPort;
            }
        }
    }
}

