namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Runtime;
    using System.Windows.Forms;
    using System.Workflow.ComponentModel.Serialization;

    [DesignerSerializer(typeof(ConnectorLayoutSerializer), typeof(WorkflowMarkupSerializer))]
    public class Connector : IDisposable
    {
        private AccessibleObject accessibilityObject;
        private bool connectorModified;
        private FreeformActivityDesigner parentDesigner;
        private List<Point> segments = new List<Point>();
        private ConnectionPoint source;
        private ConnectionPoint target;

        public Connector(ConnectionPoint source, ConnectionPoint target)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }
            if (ConnectionManager.GetConnectorContainer(source.AssociatedDesigner) != ConnectionManager.GetConnectorContainer(target.AssociatedDesigner))
            {
                throw new ArgumentException(DR.GetString("Error_Connector1", new object[0]));
            }
            this.source = source;
            this.target = target;
        }

        public override bool Equals(object obj)
        {
            Connector connector = obj as Connector;
            if (connector == null)
            {
                return false;
            }
            return ((connector.Source == this.source) && (connector.target == this.target));
        }

        internal static Connector GetConnectorFromSelectedObject(object selectedObject)
        {
            Connector connector = null;
            ConnectorHitTestInfo info = selectedObject as ConnectorHitTestInfo;
            if (info != null)
            {
                FreeformActivityDesigner associatedDesigner = info.AssociatedDesigner as FreeformActivityDesigner;
                int num = info.MapToIndex();
                if (((associatedDesigner != null) && (num >= 0)) && (num < associatedDesigner.Connectors.Count))
                {
                    connector = associatedDesigner.Connectors[num];
                }
            }
            return connector;
        }

        public override int GetHashCode()
        {
            if ((this.source != null) && (this.target != null))
            {
                return (this.source.GetHashCode() ^ this.target.GetHashCode());
            }
            return base.GetHashCode();
        }

        protected virtual object GetService(System.Type serviceType)
        {
            object service = null;
            if (((this.parentDesigner != null) && (this.parentDesigner.Activity != null)) && (this.parentDesigner.Activity.Site != null))
            {
                service = this.parentDesigner.Activity.Site.GetService(serviceType);
            }
            return service;
        }

        public virtual bool HitTest(Point point)
        {
            Size selectionSize = WorkflowTheme.CurrentTheme.AmbientTheme.SelectionSize;
            ReadOnlyCollection<Point> connectorSegments = this.ConnectorSegments;
            for (int i = 1; i < connectorSegments.Count; i++)
            {
                Point[] line = new Point[] { connectorSegments[i - 1], connectorSegments[i] };
                if (DesignerGeometryHelper.PointOnLineSegment(point, line, selectionSize))
                {
                    return true;
                }
            }
            return false;
        }

        public void Invalidate()
        {
            WorkflowView parentView = this.ParentView;
            if (parentView != null)
            {
                parentView.InvalidateLogicalRectangle(this.Bounds);
            }
        }

        public virtual void Offset(Size size)
        {
            for (int i = 0; i < this.segments.Count; i++)
            {
                Point point = this.segments[i];
                Point point2 = this.segments[i];
                this.segments[i] = new Point(point.X + size.Width, point2.Y + size.Height);
            }
        }

        protected internal virtual void OnLayout(ActivityDesignerLayoutEventArgs e)
        {
            if ((this.segments.Count > 0) && ((this.segments[0] != this.Source.Location) || (this.segments[this.segments.Count - 1] != this.Target.Location)))
            {
                this.connectorModified = false;
            }
            if (!this.connectorModified && (this.ParentDesigner != null))
            {
                Point[] collection = ActivityDesignerConnectorRouter.Route(this.Source.AssociatedDesigner.Activity.Site, this.Source, this.Target, this.ExcludedRoutingRectangles);
                this.segments.Clear();
                this.segments.AddRange(collection);
            }
        }

        protected internal virtual void OnPaint(ActivityDesignerPaintEventArgs e)
        {
            CompositeDesignerTheme designerTheme = e.DesignerTheme as CompositeDesignerTheme;
            if (designerTheme != null)
            {
                Size connectorCapSize = new Size(designerTheme.ConnectorSize.Width / 5, designerTheme.ConnectorSize.Height / 5);
                Size connectorSize = designerTheme.ConnectorSize;
                ActivityDesignerPaint.DrawConnectors(e.Graphics, e.DesignerTheme.ForegroundPen, new List<Point>(this.ConnectorSegments).ToArray(), connectorCapSize, connectorSize, designerTheme.ConnectorStartCap, designerTheme.ConnectorEndCap);
            }
        }

        protected internal virtual void OnPaintEdited(ActivityDesignerPaintEventArgs e, Point[] segments, Point[] segmentEditPoints)
        {
            CompositeDesignerTheme designerTheme = e.DesignerTheme as CompositeDesignerTheme;
            if (designerTheme != null)
            {
                using (Pen pen = new Pen(e.AmbientTheme.SelectionForegroundPen.Color, e.AmbientTheme.SelectionForegroundPen.Width))
                {
                    pen.DashStyle = DashStyle.Dash;
                    Size connectorCapSize = new Size(designerTheme.ConnectorSize.Width / 5, designerTheme.ConnectorSize.Height / 5);
                    Size connectorSize = designerTheme.ConnectorSize;
                    ActivityDesignerPaint.DrawConnectors(e.Graphics, pen, segments, connectorCapSize, connectorSize, designerTheme.ConnectorStartCap, designerTheme.ConnectorEndCap);
                }
                if (this.source != null)
                {
                    this.source.OnPaint(e, false);
                }
                for (int i = 1; i < (segments.Length - 1); i++)
                {
                    this.PaintEditPoints(e, segments[i], false);
                }
                for (int j = 0; j < segmentEditPoints.Length; j++)
                {
                    this.PaintEditPoints(e, segmentEditPoints[j], true);
                }
                if (this.target != null)
                {
                    this.target.OnPaint(e, false);
                }
            }
        }

        protected internal virtual void OnPaintSelected(ActivityDesignerPaintEventArgs e, bool primarySelection, Point[] segmentEditPoints)
        {
            CompositeDesignerTheme designerTheme = e.DesignerTheme as CompositeDesignerTheme;
            if (designerTheme != null)
            {
                using (Pen pen = new Pen(WorkflowTheme.CurrentTheme.AmbientTheme.SelectionForeColor, 1f))
                {
                    Size connectorCapSize = new Size(designerTheme.ConnectorSize.Width / 5, designerTheme.ConnectorSize.Height / 5);
                    Size connectorSize = designerTheme.ConnectorSize;
                    ActivityDesignerPaint.DrawConnectors(e.Graphics, pen, new List<Point>(this.ConnectorSegments).ToArray(), connectorCapSize, connectorSize, designerTheme.ConnectorStartCap, designerTheme.ConnectorEndCap);
                }
                if (this.source != null)
                {
                    this.source.OnPaint(e, false);
                }
                ReadOnlyCollection<Point> connectorSegments = this.ConnectorSegments;
                for (int i = 1; i < (connectorSegments.Count - 1); i++)
                {
                    this.PaintEditPoints(e, connectorSegments[i], false);
                }
                for (int j = 0; j < segmentEditPoints.Length; j++)
                {
                    this.PaintEditPoints(e, segmentEditPoints[j], true);
                }
                if (this.target != null)
                {
                    this.target.OnPaint(e, false);
                }
            }
        }

        private void PaintEditPoints(ActivityDesignerPaintEventArgs e, Point point, bool drawMidSegmentEditPoint)
        {
            Size size = (this.source != null) ? this.source.Bounds.Size : Size.Empty;
            if (!size.IsEmpty)
            {
                Rectangle rect = new Rectangle(point.X - (size.Width / 2), point.Y - (size.Height / 2), size.Width, size.Height);
                if (drawMidSegmentEditPoint)
                {
                    using (GraphicsPath path = new GraphicsPath())
                    {
                        path.AddLine(new Point(rect.Left + (rect.Width / 2), rect.Top), new Point(rect.Right, rect.Top + (rect.Height / 2)));
                        path.AddLine(new Point(rect.Right, rect.Top + (rect.Height / 2)), new Point(rect.Left + (rect.Width / 2), rect.Bottom));
                        path.AddLine(new Point(rect.Left + (rect.Width / 2), rect.Bottom), new Point(rect.Left, rect.Top + (rect.Height / 2)));
                        path.AddLine(new Point(rect.Left, rect.Top + (rect.Height / 2)), new Point(rect.Left + (rect.Width / 2), rect.Top));
                        e.Graphics.FillPath(Brushes.White, path);
                        e.Graphics.DrawPath(e.AmbientTheme.SelectionForegroundPen, path);
                        return;
                    }
                }
                rect.Inflate(-1, -1);
                e.Graphics.FillEllipse(e.AmbientTheme.SelectionForegroundBrush, rect);
            }
        }

        protected void PerformLayout()
        {
            WorkflowView parentView = this.ParentView;
            if (parentView != null)
            {
                parentView.PerformLayout(false);
            }
        }

        internal void SetConnectorModified(bool modified)
        {
            this.connectorModified = modified;
        }

        protected internal void SetConnectorSegments(ICollection<Point> segments)
        {
            if (segments == null)
            {
                throw new ArgumentNullException("segments");
            }
            this.connectorModified = (this.parentDesigner != null) && (segments.Count > 0);
            if (this.connectorModified)
            {
                this.Invalidate();
            }
            this.segments.Clear();
            this.segments.AddRange(segments);
            if (this.connectorModified)
            {
                this.Invalidate();
            }
        }

        internal void SetParent(FreeformActivityDesigner parentDesigner)
        {
            WorkflowView parentView = this.ParentView;
            if ((this.parentDesigner != null) && (parentView != null))
            {
                parentView.InvalidateLogicalRectangle(this.parentDesigner.Bounds);
            }
            this.parentDesigner = parentDesigner;
            if ((this.parentDesigner != null) && (parentView != null))
            {
                parentView.InvalidateLogicalRectangle(this.parentDesigner.Bounds);
            }
        }

        void IDisposable.Dispose()
        {
        }

        public virtual AccessibleObject AccessibilityObject
        {
            get
            {
                if (this.accessibilityObject == null)
                {
                    this.accessibilityObject = new ConnectorAccessibleObject(this);
                }
                return this.accessibilityObject;
            }
        }

        public Rectangle Bounds
        {
            get
            {
                Rectangle rectangle = DesignerGeometryHelper.RectangleFromLineSegments(new List<Point>(this.ConnectorSegments).ToArray());
                rectangle.Inflate(1, 1);
                return rectangle;
            }
        }

        public bool ConnectorModified
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.connectorModified;
            }
        }

        public virtual ReadOnlyCollection<Point> ConnectorSegments
        {
            get
            {
                List<Point> list = new List<Point>();
                if ((this.source != null) && (this.target != null))
                {
                    if ((this.segments.Count == 0) || (this.segments[0] != this.source.Location))
                    {
                        list.Add(this.source.Location);
                    }
                    list.AddRange(this.segments);
                    if ((this.segments.Count == 0) || (this.segments[this.segments.Count - 1] != this.target.Location))
                    {
                        list.Add(this.target.Location);
                    }
                }
                return list.AsReadOnly();
            }
        }

        protected internal virtual ICollection<Rectangle> ExcludedRoutingRectangles
        {
            get
            {
                return new Rectangle[0];
            }
        }

        public FreeformActivityDesigner ParentDesigner
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.parentDesigner;
            }
        }

        protected WorkflowView ParentView
        {
            get
            {
                return (this.GetService(typeof(WorkflowView)) as WorkflowView);
            }
        }

        internal FreeformActivityDesigner RenderingOwner
        {
            get
            {
                ActivityDesigner associatedDesigner;
                if ((this.source == null) || (this.target == null))
                {
                    return null;
                }
                List<FreeformActivityDesigner> list = new List<FreeformActivityDesigner>();
                for (associatedDesigner = this.target.AssociatedDesigner; associatedDesigner != null; associatedDesigner = associatedDesigner.ParentDesigner)
                {
                    FreeformActivityDesigner item = associatedDesigner as FreeformActivityDesigner;
                    if (item != null)
                    {
                        list.Add(item);
                    }
                }
                associatedDesigner = this.source.AssociatedDesigner;
                while (associatedDesigner != null)
                {
                    FreeformActivityDesigner designer3 = associatedDesigner as FreeformActivityDesigner;
                    if ((designer3 != null) && list.Contains(designer3))
                    {
                        break;
                    }
                    associatedDesigner = associatedDesigner.ParentDesigner;
                }
                return (associatedDesigner as FreeformActivityDesigner);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content), EditorBrowsable(EditorBrowsableState.Never)]
        internal List<Point> Segments
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.segments;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ConnectionPoint Source
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.source;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                if (!this.source.Equals(value))
                {
                    if (ConnectionManager.GetConnectorContainer(value.AssociatedDesigner) != ConnectionManager.GetConnectorContainer(this.target.AssociatedDesigner))
                    {
                        throw new ArgumentException(SR.GetString("Error_InvalidConnectorSource"), "value");
                    }
                    this.source = value;
                    this.PerformLayout();
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content), EditorBrowsable(EditorBrowsableState.Never)]
        internal string SourceActivity
        {
            get
            {
                string qualifiedName = string.Empty;
                if (this.source != null)
                {
                    qualifiedName = this.source.AssociatedDesigner.Activity.QualifiedName;
                }
                return qualifiedName;
            }
            set
            {
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        internal DesignerEdges SourceConnectionEdge
        {
            get
            {
                DesignerEdges none = DesignerEdges.None;
                if (this.source != null)
                {
                    none = this.source.ConnectionEdge;
                }
                return none;
            }
            set
            {
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content), EditorBrowsable(EditorBrowsableState.Never)]
        internal int SourceConnectionIndex
        {
            get
            {
                int connectionIndex = 0;
                if (this.source != null)
                {
                    connectionIndex = this.source.ConnectionIndex;
                }
                return connectionIndex;
            }
            set
            {
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ConnectionPoint Target
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.target;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                if (!this.target.Equals(value))
                {
                    if (ConnectionManager.GetConnectorContainer(value.AssociatedDesigner) != ConnectionManager.GetConnectorContainer(this.source.AssociatedDesigner))
                    {
                        throw new ArgumentException(SR.GetString("Error_InvalidConnectorSource"), "value");
                    }
                    this.target = value;
                    this.PerformLayout();
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content), EditorBrowsable(EditorBrowsableState.Never)]
        internal string TargetActivity
        {
            get
            {
                string qualifiedName = string.Empty;
                if (this.target != null)
                {
                    qualifiedName = this.target.AssociatedDesigner.Activity.QualifiedName;
                }
                return qualifiedName;
            }
            set
            {
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        internal DesignerEdges TargetConnectionEdge
        {
            get
            {
                DesignerEdges none = DesignerEdges.None;
                if (this.target != null)
                {
                    none = this.target.ConnectionEdge;
                }
                return none;
            }
            set
            {
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        internal int TargetConnectionIndex
        {
            get
            {
                int connectionIndex = 0;
                if (this.target != null)
                {
                    connectionIndex = this.target.ConnectionIndex;
                }
                return connectionIndex;
            }
            set
            {
            }
        }
    }
}

