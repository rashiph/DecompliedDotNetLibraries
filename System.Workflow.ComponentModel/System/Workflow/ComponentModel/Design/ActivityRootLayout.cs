namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Runtime;
    using System.Windows.Forms;

    internal sealed class ActivityRootLayout : DefaultWorkflowLayout
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal ActivityRootLayout(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public override void OnPaint(PaintEventArgs e, ViewPortData viewPortData)
        {
            base.OnPaint(e, viewPortData);
            Graphics graphics = e.Graphics;
            if (((base.parentView.RootDesigner != null) && (base.parentView.RootDesigner.Bounds.Width >= 0)) && (base.parentView.RootDesigner.Bounds.Height >= 0))
            {
                GraphicsContainer container = graphics.BeginContainer();
                Matrix matrix = new Matrix();
                matrix.Scale(viewPortData.Scaling.Width, viewPortData.Scaling.Height, MatrixOrder.Prepend);
                Point[] pts = new Point[] { viewPortData.LogicalViewPort.Location };
                matrix.TransformPoints(pts);
                matrix.Translate((float) (-pts[0].X + viewPortData.ShadowDepth.Width), (float) (-pts[0].Y + viewPortData.ShadowDepth.Height), MatrixOrder.Append);
                graphics.Transform = matrix;
                Rectangle bounds = base.parentView.RootDesigner.Bounds;
                graphics.ExcludeClip(bounds);
                bounds.Inflate(DefaultWorkflowLayout.Separator.Width / 2, DefaultWorkflowLayout.Separator.Height / 2);
                ActivityDesignerPaint.DrawDropShadow(graphics, bounds, AmbientTheme.WorkflowBorderPen.Color, 4, LightSourcePosition.Top | LightSourcePosition.Left, 0.2f, false);
                graphics.FillRectangle(WorkflowTheme.CurrentTheme.AmbientTheme.BackgroundBrush, bounds);
                graphics.DrawRectangle(AmbientTheme.WorkflowBorderPen, bounds);
                graphics.EndContainer(container);
            }
        }

        public override Size Extent
        {
            get
            {
                Size size = (base.parentView.RootDesigner != null) ? base.parentView.RootDesigner.Size : Size.Empty;
                Size size2 = new Size(size.Width + (DefaultWorkflowLayout.Separator.Width * 2), size.Height + (DefaultWorkflowLayout.Separator.Height * 2));
                Size viewPortSize = base.parentView.ViewPortSize;
                viewPortSize.Width = (int) (((float) viewPortSize.Width) / (((float) base.parentView.Zoom) / 100f));
                viewPortSize.Height = (int) (((float) viewPortSize.Height) / (((float) base.parentView.Zoom) / 100f));
                int width = Math.Max(size2.Width, viewPortSize.Width);
                return new Size(width, Math.Max(size2.Height, viewPortSize.Height));
            }
        }
    }
}

