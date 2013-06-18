namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Windows.Forms;

    public class ConfigErrorGlyph : DesignerGlyph
    {
        private static ConfigErrorGlyph defaultConfigErrorGlyph;

        public override Rectangle GetBounds(ActivityDesigner designer, bool activated)
        {
            if (designer == null)
            {
                throw new ArgumentNullException("designer");
            }
            Size glyphSize = WorkflowTheme.CurrentTheme.AmbientTheme.GlyphSize;
            Size margin = WorkflowTheme.CurrentTheme.AmbientTheme.Margin;
            Point location = new Point((designer.Bounds.Right - glyphSize.Width) - (margin.Width / 2), (designer.Bounds.Top - glyphSize.Height) + margin.Height);
            Rectangle rectangle = new Rectangle(location, glyphSize);
            if (activated)
            {
                rectangle.Width *= 2;
                AmbientTheme ambientTheme = WorkflowTheme.CurrentTheme.AmbientTheme;
                rectangle.Inflate(ambientTheme.Margin.Width / 2, ambientTheme.Margin.Height / 2);
            }
            return rectangle;
        }

        protected override void OnActivate(ActivityDesigner designer)
        {
            if ((designer != null) && (designer.DesignerActions.Count > 0))
            {
                Rectangle bounds = this.GetBounds(designer, false);
                Point location = designer.ParentView.LogicalPointToScreen(new Point(bounds.Left, bounds.Bottom));
                DesignerHelpers.ShowDesignerVerbs(designer, location, DesignerHelpers.GetDesignerActionVerbs(designer, designer.DesignerActions));
            }
        }

        protected override void OnPaint(Graphics graphics, bool activated, AmbientTheme ambientTheme, ActivityDesigner designer)
        {
            Rectangle bounds = this.GetBounds(designer, false);
            Rectangle rect = this.GetBounds(designer, activated);
            Region region = null;
            Region clip = graphics.Clip;
            try
            {
                if (clip != null)
                {
                    region = clip.Clone();
                    if (activated)
                    {
                        region.Union(rect);
                    }
                    graphics.Clip = region;
                }
                if (activated)
                {
                    graphics.FillRectangle(SystemBrushes.ButtonFace, rect);
                    graphics.DrawRectangle(SystemPens.ControlDarkDark, rect.Left, rect.Top, rect.Width - 1, rect.Height - 1);
                    rect.X += bounds.Width + ambientTheme.Margin.Width;
                    rect.Width -= bounds.Width + (2 * ambientTheme.Margin.Width);
                    using (GraphicsPath path = ActivityDesignerPaint.GetScrollIndicatorPath(rect, ScrollButton.Down))
                    {
                        graphics.FillPath(SystemBrushes.ControlText, path);
                        graphics.DrawPath(SystemPens.ControlText, path);
                    }
                }
                ActivityDesignerPaint.DrawImage(graphics, AmbientTheme.ConfigErrorImage, bounds, DesignerContentAlignment.Fill);
            }
            finally
            {
                if (region != null)
                {
                    graphics.Clip = clip;
                    region.Dispose();
                }
            }
        }

        public override bool CanBeActivated
        {
            get
            {
                return true;
            }
        }

        internal static ConfigErrorGlyph Default
        {
            get
            {
                if (defaultConfigErrorGlyph == null)
                {
                    defaultConfigErrorGlyph = new ConfigErrorGlyph();
                }
                return defaultConfigErrorGlyph;
            }
        }

        public override int Priority
        {
            get
            {
                return 2;
            }
        }
    }
}

