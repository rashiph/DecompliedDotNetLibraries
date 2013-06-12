namespace System.Windows.Forms
{
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;

    internal class ToolStripProfessionalLowResolutionRenderer : ToolStripProfessionalRenderer
    {
        protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
        {
            if (e.ToolStrip is ToolStripDropDown)
            {
                base.OnRenderToolStripBackground(e);
            }
        }

        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
        {
            if (!(e.ToolStrip is MenuStrip) && !(e.ToolStrip is StatusStrip))
            {
                if (e.ToolStrip is ToolStripDropDown)
                {
                    base.OnRenderToolStripBorder(e);
                }
                else
                {
                    this.RenderToolStripBorderInternal(e);
                }
            }
        }

        private void RenderToolStripBorderInternal(ToolStripRenderEventArgs e)
        {
            Rectangle rectangle = new Rectangle(Point.Empty, e.ToolStrip.Size);
            Graphics graphics = e.Graphics;
            using (Pen pen = new Pen(SystemColors.ButtonShadow))
            {
                pen.DashStyle = DashStyle.Dot;
                bool flag = (rectangle.Width & 1) == 1;
                bool flag2 = (rectangle.Height & 1) == 1;
                int num = 2;
                graphics.DrawLine(pen, rectangle.X + num, rectangle.Y, rectangle.Width - 1, rectangle.Y);
                graphics.DrawLine(pen, (int) (rectangle.X + num), (int) (rectangle.Height - 1), (int) (rectangle.Width - 1), (int) (rectangle.Height - 1));
                graphics.DrawLine(pen, rectangle.X, rectangle.Y + num, rectangle.X, rectangle.Height - 1);
                graphics.DrawLine(pen, (int) (rectangle.Width - 1), (int) (rectangle.Y + num), (int) (rectangle.Width - 1), (int) (rectangle.Height - 1));
                graphics.FillRectangle(SystemBrushes.ButtonShadow, new Rectangle(1, 1, 1, 1));
                if (flag)
                {
                    graphics.FillRectangle(SystemBrushes.ButtonShadow, new Rectangle(rectangle.Width - 2, 1, 1, 1));
                }
                if (flag2)
                {
                    graphics.FillRectangle(SystemBrushes.ButtonShadow, new Rectangle(1, rectangle.Height - 2, 1, 1));
                }
                if (flag2 && flag)
                {
                    graphics.FillRectangle(SystemBrushes.ButtonShadow, new Rectangle(rectangle.Width - 2, rectangle.Height - 2, 1, 1));
                }
            }
        }

        internal override ToolStripRenderer RendererOverride
        {
            get
            {
                return null;
            }
        }
    }
}

