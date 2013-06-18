namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Drawing;

    public class CommentGlyph : DesignerGlyph
    {
        private static CommentGlyph defaultCommentGlyph;

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
            graphics.FillRectangle(AmbientTheme.FadeBrush, bounds);
            graphics.FillRectangle(ambientTheme.CommentIndicatorBrush, bounds);
            graphics.DrawRectangle(ambientTheme.CommentIndicatorPen, bounds);
        }

        internal static CommentGlyph Default
        {
            get
            {
                if (defaultCommentGlyph == null)
                {
                    defaultCommentGlyph = new CommentGlyph();
                }
                return defaultCommentGlyph;
            }
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

