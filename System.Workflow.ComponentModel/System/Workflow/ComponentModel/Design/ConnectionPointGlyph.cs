namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Drawing;
    using System.Runtime;

    internal sealed class ConnectionPointGlyph : DesignerGlyph
    {
        private ConnectionPoint connectionPoint;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal ConnectionPointGlyph(ConnectionPoint connectionPoint)
        {
            this.connectionPoint = connectionPoint;
        }

        protected override void OnPaint(Graphics graphics, bool activated, AmbientTheme ambientTheme, ActivityDesigner designer)
        {
            if (((designer.Activity != null) && (designer.Activity.Site != null)) && (this.connectionPoint != null))
            {
                WorkflowView service = designer.Activity.Site.GetService(typeof(WorkflowView)) as WorkflowView;
                Rectangle viewPort = (service != null) ? service.ViewPortRectangle : Rectangle.Empty;
                Rectangle clipRectangle = (designer.ParentDesigner != null) ? designer.ParentDesigner.Bounds : designer.Bounds;
                ConnectionManager manager = designer.Activity.Site.GetService(typeof(ConnectionManager)) as ConnectionManager;
                ActivityDesignerPaintEventArgs e = new ActivityDesignerPaintEventArgs(graphics, clipRectangle, viewPort, designer.DesignerTheme);
                bool drawHighlighted = (manager != null) && this.connectionPoint.Equals(manager.SnappedConnectionPoint);
                this.connectionPoint.OnPaint(e, drawHighlighted);
            }
        }

        public override int Priority
        {
            get
            {
                return 1;
            }
        }
    }
}

