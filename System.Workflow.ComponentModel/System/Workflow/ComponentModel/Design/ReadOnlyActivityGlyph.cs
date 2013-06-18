namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Drawing;

    public class ReadOnlyActivityGlyph : DesignerGlyph
    {
        public override Rectangle GetBounds(ActivityDesigner designer, bool activated)
        {
            if (designer == null)
            {
                throw new ArgumentNullException("designer");
            }
            Rectangle bounds = designer.Bounds;
            bounds.Inflate(WorkflowTheme.CurrentTheme.AmbientTheme.Margin);
            return bounds;
        }

        protected override void OnPaint(Graphics graphics, bool activated, AmbientTheme ambientTheme, ActivityDesigner designer)
        {
            Rectangle bounds = this.GetBounds(designer, activated);
            bounds.Inflate(WorkflowTheme.CurrentTheme.AmbientTheme.Margin);
            ActivityDesignerPaint.DrawImage(graphics, AmbientTheme.ReadOnlyImage, bounds, DesignerContentAlignment.TopLeft);
        }

        public override int Priority
        {
            get
            {
                return 3;
            }
        }
    }
}

