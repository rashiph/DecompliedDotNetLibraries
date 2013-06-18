namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    internal abstract class WorkflowLayout : IDisposable
    {
        protected WorkflowView parentView;
        protected IServiceProvider serviceProvider;

        public WorkflowLayout(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException("serviceProvider");
            }
            this.serviceProvider = serviceProvider;
            this.parentView = this.serviceProvider.GetService(typeof(WorkflowView)) as WorkflowView;
            if (this.parentView == null)
            {
                throw new InvalidOperationException(SR.GetString("General_MissingService", new object[] { typeof(WorkflowView).FullName }));
            }
        }

        public virtual void Dispose()
        {
        }

        public abstract bool IsCoOrdInLayout(Point logicalCoOrd);
        public abstract Point MapInCoOrdToLayout(Point logicalPoint);
        public abstract Rectangle MapInRectangleToLayout(Rectangle logicalRectangle);
        public abstract Point MapOutCoOrdFromLayout(Point logicalPoint);
        public abstract Rectangle MapOutRectangleFromLayout(Rectangle logicalRectangle);
        public abstract void OnPaint(PaintEventArgs e, ViewPortData viewPortData);
        public abstract void OnPaintWorkflow(PaintEventArgs e, ViewPortData viewPortData);
        public abstract void Update(Graphics graphics, LayoutUpdateReason reason);

        public abstract Size Extent { get; }

        public abstract Point RootDesignerAlignment { get; }

        public abstract float Scaling { get; }

        public enum LayoutUpdateReason
        {
            LayoutChanged,
            ZoomChanged
        }
    }
}

