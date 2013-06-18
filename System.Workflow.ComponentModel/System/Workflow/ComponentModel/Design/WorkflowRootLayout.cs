namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Drawing;
    using System.Runtime;

    internal sealed class WorkflowRootLayout : DefaultWorkflowLayout
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public WorkflowRootLayout(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public override Point MapInCoOrdToLayout(Point logicalPoint)
        {
            Size offset = this.Offset;
            logicalPoint.Offset(-offset.Width, -offset.Height);
            return logicalPoint;
        }

        public override Rectangle MapInRectangleToLayout(Rectangle logicalRectangle)
        {
            Size offset = this.Offset;
            logicalRectangle.X -= offset.Width;
            logicalRectangle.Y -= offset.Height;
            return logicalRectangle;
        }

        public override Point MapOutCoOrdFromLayout(Point logicalPoint)
        {
            Size offset = this.Offset;
            logicalPoint.Offset(offset.Width, offset.Height);
            return logicalPoint;
        }

        public override Rectangle MapOutRectangleFromLayout(Rectangle logicalRectangle)
        {
            Size offset = this.Offset;
            logicalRectangle.X += offset.Width;
            logicalRectangle.Y += offset.Height;
            return logicalRectangle;
        }

        private Size Offset
        {
            get
            {
                Size extent = this.Extent;
                Size size2 = base.parentView.ClientSizeToLogical(base.parentView.ViewPortSize);
                size2.Width = Math.Max(size2.Width, extent.Width);
                size2.Height = Math.Max(size2.Height, extent.Height);
                return new Size(Math.Max(0, (size2.Width - extent.Width) / 2), Math.Max(0, (size2.Height - extent.Height) / 2));
            }
        }
    }
}

