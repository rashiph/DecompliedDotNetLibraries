namespace System.Workflow.Activities
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel.Design.Serialization;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Runtime;
    using System.Workflow.ComponentModel.Design;
    using System.Workflow.ComponentModel.Serialization;

    [DesignerSerializer(typeof(StateDesignerConnectorLayoutSerializer), typeof(WorkflowMarkupSerializer))]
    internal class StateDesignerConnector : Connector
    {
        private string _eventHandlerName;
        private string _setStateName;
        private string _sourceStateName;
        private string _targetStateName;
        internal const int ConnectorPadding = 10;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal StateDesignerConnector(ConnectionPoint source, ConnectionPoint target) : base(source, target)
        {
        }

        internal void ClearConnectorSegments()
        {
            base.SetConnectorSegments(new Collection<Point>());
        }

        public override bool HitTest(Point point)
        {
            if ((this.RootStateDesigner != null) && (this.RootStateDesigner.ActiveDesigner != null))
            {
                return false;
            }
            return base.HitTest(point);
        }

        protected override void OnLayout(ActivityDesignerLayoutEventArgs e)
        {
            if (!this.RootStateDesigner.HasActiveDesigner)
            {
                base.OnLayout(e);
            }
        }

        protected override void OnPaint(ActivityDesignerPaintEventArgs e)
        {
            if ((this.RootStateDesigner == null) || (this.RootStateDesigner.ActiveDesigner == null))
            {
                StateMachineTheme designerTheme = e.DesignerTheme as StateMachineTheme;
                if (designerTheme != null)
                {
                    Size connectorCapSize = new Size(designerTheme.ConnectorSize.Width / 5, designerTheme.ConnectorSize.Height / 5);
                    Size connectorSize = designerTheme.ConnectorSize;
                    StateMachineDesignerPaint.DrawConnector(e.Graphics, designerTheme.ConnectorPen, new List<Point>(this.ConnectorSegments).ToArray(), connectorCapSize, connectorSize, designerTheme.ConnectorStartCap, designerTheme.ConnectorEndCap);
                }
            }
        }

        protected override void OnPaintEdited(ActivityDesignerPaintEventArgs e, Point[] segments, Point[] segmentEditPoints)
        {
            if (((this.RootStateDesigner == null) || (this.RootStateDesigner.ActiveDesigner == null)) && (e.DesignerTheme is StateMachineTheme))
            {
                using (Pen pen = new Pen(WorkflowTheme.CurrentTheme.AmbientTheme.SelectionForeColor, 1f))
                {
                    pen.DashStyle = DashStyle.Dash;
                    e.Graphics.DrawLines(pen, segments);
                    if (base.Source != null)
                    {
                        base.Source.OnPaint(e, false);
                    }
                    for (int i = 1; i < (segments.Length - 1); i++)
                    {
                        this.PaintEditPoints(e, segments[i], false);
                    }
                    for (int j = 0; j < segmentEditPoints.Length; j++)
                    {
                        this.PaintEditPoints(e, segmentEditPoints[j], true);
                    }
                    if (base.Target != null)
                    {
                        base.Target.OnPaint(e, false);
                    }
                }
            }
        }

        protected override void OnPaintSelected(ActivityDesignerPaintEventArgs e, bool primarySelection, Point[] segmentEditPoints)
        {
            if ((this.RootStateDesigner == null) || (this.RootStateDesigner.ActiveDesigner == null))
            {
                StateMachineTheme designerTheme = e.DesignerTheme as StateMachineTheme;
                if (designerTheme != null)
                {
                    Size connectorCapSize = new Size(designerTheme.ConnectorSize.Width / 5, designerTheme.ConnectorSize.Height / 5);
                    Size connectorSize = designerTheme.ConnectorSize;
                    using (Pen pen = new Pen(WorkflowTheme.CurrentTheme.AmbientTheme.SelectionForeColor, 1f))
                    {
                        StateMachineDesignerPaint.DrawConnector(e.Graphics, pen, new List<Point>(this.ConnectorSegments).ToArray(), connectorCapSize, connectorSize, designerTheme.ConnectorStartCap, designerTheme.ConnectorEndCap);
                    }
                    if (base.Source != null)
                    {
                        base.Source.OnPaint(e, false);
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
                    if (base.Target != null)
                    {
                        base.Target.OnPaint(e, false);
                    }
                }
            }
        }

        private void PaintEditPoints(ActivityDesignerPaintEventArgs e, Point point, bool drawMidSegmentEditPoint)
        {
            Size size = (base.Source != null) ? base.Source.Bounds.Size : Size.Empty;
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

        internal string EventHandlerName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._eventHandlerName;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this._eventHandlerName = value;
            }
        }

        protected override ICollection<Rectangle> ExcludedRoutingRectangles
        {
            get
            {
                StateDesigner associatedDesigner = (StateDesigner) base.Source.AssociatedDesigner;
                List<Rectangle> list = new List<Rectangle>(base.ExcludedRoutingRectangles);
                if (associatedDesigner.IsRootDesigner)
                {
                    list.AddRange(associatedDesigner.EventHandlersBounds);
                }
                return list.AsReadOnly();
            }
        }

        private StateDesigner RootStateDesigner
        {
            get
            {
                StateDesigner parentDesigner = (StateDesigner) base.ParentDesigner;
                while (parentDesigner != null)
                {
                    StateDesigner designer2 = parentDesigner.ParentDesigner as StateDesigner;
                    if (designer2 == null)
                    {
                        return parentDesigner;
                    }
                    parentDesigner = designer2;
                }
                return parentDesigner;
            }
        }

        internal string SetStateName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._setStateName;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this._setStateName = value;
            }
        }

        internal string SourceStateName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._sourceStateName;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this._sourceStateName = value;
            }
        }

        internal string TargetStateName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._targetStateName;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this._targetStateName = value;
            }
        }
    }
}

