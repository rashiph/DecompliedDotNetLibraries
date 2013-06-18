namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Runtime;
    using System.Windows.Forms;

    internal abstract class DefaultWorkflowLayout : WorkflowLayout
    {
        public static Size Separator = new Size(30, 30);

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public DefaultWorkflowLayout(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public override bool IsCoOrdInLayout(Point logicalCoOrd)
        {
            return true;
        }

        public override Point MapInCoOrdToLayout(Point logicalPoint)
        {
            return logicalPoint;
        }

        public override Rectangle MapInRectangleToLayout(Rectangle logicalRectangle)
        {
            return logicalRectangle;
        }

        public override Point MapOutCoOrdFromLayout(Point logicalPoint)
        {
            return logicalPoint;
        }

        public override Rectangle MapOutRectangleFromLayout(Rectangle logicalRectangle)
        {
            return logicalRectangle;
        }

        public override void OnPaint(PaintEventArgs e, ViewPortData viewPortData)
        {
            Graphics graphics = e.Graphics;
            Bitmap memoryBitmap = viewPortData.MemoryBitmap;
            Rectangle rect = new Rectangle(Point.Empty, memoryBitmap.Size);
            graphics.FillRectangle(AmbientTheme.WorkspaceBackgroundBrush, rect);
            if (((base.parentView.RootDesigner != null) && (base.parentView.RootDesigner.Bounds.Width >= 0)) && (base.parentView.RootDesigner.Bounds.Height >= 0))
            {
                GraphicsContainer container = graphics.BeginContainer();
                Matrix matrix = new Matrix();
                matrix.Scale(viewPortData.Scaling.Width, viewPortData.Scaling.Height, MatrixOrder.Prepend);
                Point[] pts = new Point[] { viewPortData.LogicalViewPort.Location };
                matrix.TransformPoints(pts);
                matrix.Translate((float) (-pts[0].X + viewPortData.ShadowDepth.Width), (float) (-pts[0].Y + viewPortData.ShadowDepth.Height), MatrixOrder.Append);
                graphics.Transform = matrix;
                using (Region region = new Region(ActivityDesignerPaint.GetDesignerPath(base.parentView.RootDesigner, false)))
                {
                    Region clip = graphics.Clip;
                    graphics.Clip = region;
                    AmbientTheme ambientTheme = WorkflowTheme.CurrentTheme.AmbientTheme;
                    graphics.FillRectangle(Brushes.White, base.parentView.RootDesigner.Bounds);
                    if (ambientTheme.WorkflowWatermarkImage != null)
                    {
                        ActivityDesignerPaint.DrawImage(graphics, ambientTheme.WorkflowWatermarkImage, base.parentView.RootDesigner.Bounds, new Rectangle(Point.Empty, ambientTheme.WorkflowWatermarkImage.Size), ambientTheme.WatermarkAlignment, 0.25f, false);
                    }
                    graphics.Clip = clip;
                }
                graphics.EndContainer(container);
            }
        }

        public override void OnPaintWorkflow(PaintEventArgs e, ViewPortData viewPortData)
        {
            Graphics graphics = e.Graphics;
            Bitmap memoryBitmap = viewPortData.MemoryBitmap;
            Rectangle destination = new Rectangle(Point.Empty, memoryBitmap.Size);
            ActivityDesignerPaint.DrawImage(graphics, memoryBitmap, destination, destination, DesignerContentAlignment.Fill, 1f, WorkflowTheme.CurrentTheme.AmbientTheme.DrawGrayscale);
        }

        public override void Update(Graphics graphics, WorkflowLayout.LayoutUpdateReason reason)
        {
        }

        public override Size Extent
        {
            get
            {
                Size size = (base.parentView.RootDesigner != null) ? base.parentView.RootDesigner.Size : Size.Empty;
                Size size2 = new Size(size.Width + (Separator.Width * 2), size.Height + (Separator.Height * 2));
                Size viewPortSize = base.parentView.ViewPortSize;
                int width = Math.Max(size2.Width, viewPortSize.Width);
                return new Size(width, Math.Max(size2.Height, viewPortSize.Height));
            }
        }

        public override Point RootDesignerAlignment
        {
            get
            {
                return new Point(Separator);
            }
        }

        public override float Scaling
        {
            get
            {
                return 1f;
            }
        }
    }
}

