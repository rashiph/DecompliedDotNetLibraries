namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Runtime;

    public class ConnectionPoint
    {
        private ActivityDesigner associatedDesigner;
        private int connectionIndex;
        private DesignerEdges designerEdge;

        public ConnectionPoint(ActivityDesigner associatedDesigner, DesignerEdges designerEdge, int connectionIndex)
        {
            if (associatedDesigner == null)
            {
                throw new ArgumentNullException("associatedDesigner");
            }
            if ((connectionIndex < 0) || (connectionIndex >= associatedDesigner.GetConnections(designerEdge).Count))
            {
                throw new ArgumentException(DR.GetString("Error_ConnectionPoint", new object[0]), "connectionIndex");
            }
            this.associatedDesigner = associatedDesigner;
            this.designerEdge = designerEdge;
            this.connectionIndex = connectionIndex;
        }

        internal static void Draw(ActivityDesignerPaintEventArgs e, Rectangle bounds)
        {
            bounds.Inflate(-1, -1);
            e.Graphics.FillEllipse(Brushes.White, bounds);
            e.Graphics.DrawEllipse(e.AmbientTheme.SelectionForegroundPen, bounds);
            bounds.Inflate(-1, -1);
            e.Graphics.FillEllipse(e.AmbientTheme.SelectionForegroundBrush, bounds);
        }

        public override bool Equals(object obj)
        {
            ConnectionPoint point = obj as ConnectionPoint;
            if (point == null)
            {
                return false;
            }
            return (((point.AssociatedDesigner == this.associatedDesigner) && (point.designerEdge == this.designerEdge)) && (point.ConnectionIndex == this.connectionIndex));
        }

        public override int GetHashCode()
        {
            return ((this.associatedDesigner.GetHashCode() ^ this.designerEdge.GetHashCode()) ^ this.connectionIndex.GetHashCode());
        }

        public void OnPaint(ActivityDesignerPaintEventArgs e, bool drawHighlighted)
        {
            Draw(e, this.Bounds);
        }

        public ActivityDesigner AssociatedDesigner
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.associatedDesigner;
            }
        }

        public virtual Rectangle Bounds
        {
            get
            {
                if (this.associatedDesigner.GetConnections(DesignerEdges.All).Count > 0)
                {
                    Point location = this.Location;
                    Size defaultSize = this.DefaultSize;
                    return new Rectangle(new Point(location.X - (defaultSize.Width / 2), location.Y - (defaultSize.Height / 2)), defaultSize);
                }
                return Rectangle.Empty;
            }
        }

        public DesignerEdges ConnectionEdge
        {
            get
            {
                DesignerEdges designerEdge = this.designerEdge;
                if (((designerEdge != DesignerEdges.Left) && (designerEdge != DesignerEdges.Right)) && ((designerEdge != DesignerEdges.Top) && (designerEdge != DesignerEdges.Bottom)))
                {
                    designerEdge = DesignerGeometryHelper.ClosestEdgeToPoint(this.Location, this.associatedDesigner.Bounds, designerEdge);
                }
                return designerEdge;
            }
        }

        public int ConnectionIndex
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.connectionIndex;
            }
        }

        private Size DefaultSize
        {
            get
            {
                Size margin = WorkflowTheme.CurrentTheme.AmbientTheme.Margin;
                margin.Width += margin.Width / 2;
                margin.Height += margin.Height / 2;
                if (this.associatedDesigner != null)
                {
                    int width = Math.Max(margin.Width, ((int) this.associatedDesigner.DesignerTheme.ForegroundPen.Width) * 4);
                    margin = new Size(width, Math.Max(margin.Height, ((int) this.associatedDesigner.DesignerTheme.ForegroundPen.Width) * 4));
                }
                return margin;
            }
        }

        public virtual Point Location
        {
            get
            {
                IList<Point> connections = this.associatedDesigner.GetConnections(this.designerEdge);
                if (this.connectionIndex < connections.Count)
                {
                    return connections[this.connectionIndex];
                }
                return Point.Empty;
            }
        }
    }
}

