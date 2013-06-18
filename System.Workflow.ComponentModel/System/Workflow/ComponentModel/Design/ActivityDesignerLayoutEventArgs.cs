namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Drawing;
    using System.Runtime;

    public class ActivityDesignerLayoutEventArgs : EventArgs
    {
        private ActivityDesignerTheme designerTheme;
        private System.Drawing.Graphics graphics;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ActivityDesignerLayoutEventArgs(System.Drawing.Graphics graphics, ActivityDesignerTheme designerTheme)
        {
            this.graphics = graphics;
            this.designerTheme = designerTheme;
        }

        public System.Workflow.ComponentModel.Design.AmbientTheme AmbientTheme
        {
            get
            {
                return WorkflowTheme.CurrentTheme.AmbientTheme;
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
    }
}

