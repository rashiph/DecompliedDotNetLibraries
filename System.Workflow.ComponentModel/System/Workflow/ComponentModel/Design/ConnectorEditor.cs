namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Runtime;
    using System.Windows.Forms;

    internal sealed class ConnectorEditor
    {
        private EditPoint activeEditPoint;
        private Connector editedConnector;
        private List<EditPoint> editPoints = new List<EditPoint>();
        private IServiceProvider serviceProvider;

        public ConnectorEditor(Connector connectorEdited)
        {
            this.editedConnector = connectorEdited;
            this.serviceProvider = this.editedConnector.Source.AssociatedDesigner.Activity.Site;
            this.CreateEditPoints();
        }

        private void AddEditPoints(EditPoint.EditPointTypes editPointType)
        {
            if (editPointType == EditPoint.EditPointTypes.ConnectionEditPoint)
            {
                if ((this.editPoints.Count == 0) || !this.editPoints[0].EditedConnectionPoint.Equals(this.Source))
                {
                    this.editPoints.Insert(0, new EditPoint(this, this.Source));
                }
                if ((this.editPoints.Count < 2) || !this.editPoints[this.editPoints.Count - 1].EditedConnectionPoint.Equals(this.Target))
                {
                    this.editPoints.Add(new EditPoint(this, this.Target));
                }
            }
            else if (editPointType == EditPoint.EditPointTypes.MidSegmentEditPoint)
            {
                int num = this.Source.Bounds.Width * 4;
                for (int i = 0; i < (this.editPoints.Count - 1); i++)
                {
                    if ((this.editPoints[i].Type != EditPoint.EditPointTypes.MidSegmentEditPoint) && (this.editPoints[i + 1].Type != EditPoint.EditPointTypes.MidSegmentEditPoint))
                    {
                        Point[] segments = new Point[] { this.editPoints[i].Location, this.editPoints[i + 1].Location };
                        if (DesignerGeometryHelper.DistanceOfLineSegments(segments) > num)
                        {
                            Point point = DesignerGeometryHelper.MidPointOfLineSegment(this.editPoints[i].Location, this.editPoints[i + 1].Location);
                            this.editPoints.Insert(i + 1, new EditPoint(this, EditPoint.EditPointTypes.MidSegmentEditPoint, point));
                        }
                    }
                }
            }
            else if ((editPointType == EditPoint.EditPointTypes.MultiSegmentEditPoint) && (this.editPoints.Count == 2))
            {
                List<Point> list = new List<Point>(this.editedConnector.ConnectorSegments);
                if ((list.Count > 0) && (list[0] == this.Source.Location))
                {
                    list.RemoveAt(0);
                }
                if ((list.Count > 0) && (list[list.Count - 1] == this.Target.Location))
                {
                    list.RemoveAt(list.Count - 1);
                }
                List<EditPoint> list2 = new List<EditPoint>();
                for (int j = 0; j < list.Count; j++)
                {
                    list2.Add(new EditPoint(this, EditPoint.EditPointTypes.MultiSegmentEditPoint, list[j]));
                }
                this.editPoints.InsertRange(this.editPoints.Count - 1, list2.ToArray());
            }
        }

        private void CreateEditPoints()
        {
            this.editPoints.Clear();
            this.AddEditPoints(EditPoint.EditPointTypes.ConnectionEditPoint);
            this.AddEditPoints(EditPoint.EditPointTypes.MultiSegmentEditPoint);
            this.AddEditPoints(EditPoint.EditPointTypes.MidSegmentEditPoint);
            this.ValidateEditPoints();
        }

        public Cursor GetCursor(Point cursorPoint)
        {
            Cursor cursor = Cursors.Default;
            if (this.activeEditPoint != null)
            {
                return ConnectionManager.NewConnectorCursor;
            }
            foreach (EditPoint point in this.editPoints)
            {
                if (point.Bounds.Contains(cursorPoint))
                {
                    return ConnectionManager.SnappedConnectionCursor;
                }
            }
            return cursor;
        }

        private List<Point> GetPointsFromEditPoints(List<EditPoint> editPoints)
        {
            List<Point> list = new List<Point>();
            for (int i = 0; i < editPoints.Count; i++)
            {
                EditPoint point = editPoints[i];
                if ((point.Type == EditPoint.EditPointTypes.ConnectionEditPoint) || (point.Type == EditPoint.EditPointTypes.MultiSegmentEditPoint))
                {
                    list.Add(point.Location);
                }
            }
            return list;
        }

        private object GetService(System.Type serviceType)
        {
            object service = null;
            if (this.serviceProvider != null)
            {
                service = this.serviceProvider.GetService(serviceType);
            }
            return service;
        }

        public bool HitTest(Point point)
        {
            for (int i = 0; i < this.editPoints.Count; i++)
            {
                EditPoint point2 = this.editPoints[i];
                if (point2.Bounds.Contains(point))
                {
                    return true;
                }
            }
            return false;
        }

        private void Invalidate()
        {
            WorkflowView service = this.GetService(typeof(WorkflowView)) as WorkflowView;
            if (service != null)
            {
                Rectangle logicalRectangle = DesignerGeometryHelper.RectangleFromLineSegments(this.GetPointsFromEditPoints(this.editPoints).ToArray());
                logicalRectangle.Inflate(1, 1);
                service.InvalidateLogicalRectangle(logicalRectangle);
            }
        }

        public bool OnBeginEditing(Point point)
        {
            this.CreateEditPoints();
            EditPoint point2 = null;
            for (int i = this.editPoints.Count - 1; i >= 0; i--)
            {
                if (this.editPoints[i].Bounds.Contains(point))
                {
                    point2 = this.editPoints[i];
                    break;
                }
            }
            if ((point2 != null) && ((point2.EditedConnectionPoint == null) || (ConnectionManager.GetConnectorContainer(point2.EditedConnectionPoint.AssociatedDesigner) != null)))
            {
                point2.Location = point;
                this.activeEditPoint = point2;
            }
            this.Invalidate();
            return (this.activeEditPoint != null);
        }

        public void OnContinueEditing(Point point)
        {
            if (this.activeEditPoint != null)
            {
                this.Invalidate();
                this.UpdateEditPoints(point);
                this.Invalidate();
            }
        }

        public void OnEndEditing(Point point, bool commitChanges)
        {
            if (this.activeEditPoint != null)
            {
                this.Invalidate();
                if (commitChanges)
                {
                    this.UpdateEditPoints(point);
                    EditPoint activeEditPoint = this.activeEditPoint;
                    this.activeEditPoint = null;
                    this.UpdateEditPoints(point);
                    bool flag = false;
                    if (activeEditPoint.Type == EditPoint.EditPointTypes.ConnectionEditPoint)
                    {
                        ConnectionManager service = this.GetService(typeof(ConnectionManager)) as ConnectionManager;
                        FreeformActivityDesigner connectorContainer = ConnectionManager.GetConnectorContainer(activeEditPoint.EditedConnectionPoint.AssociatedDesigner);
                        if (((service != null) && (service.SnappedConnectionPoint != null)) && (connectorContainer != null))
                        {
                            ConnectionPoint source = this.editedConnector.Source;
                            ConnectionPoint target = this.editedConnector.Target;
                            if (target.Equals(activeEditPoint.EditedConnectionPoint))
                            {
                                target = service.SnappedConnectionPoint;
                            }
                            else if (source.Equals(activeEditPoint.EditedConnectionPoint))
                            {
                                source = service.SnappedConnectionPoint;
                            }
                            if ((connectorContainer == ConnectionManager.GetConnectorContainer(target.AssociatedDesigner)) && connectorContainer.CanConnectContainedDesigners(source, target))
                            {
                                this.editedConnector.Source = source;
                                this.editedConnector.Target = target;
                                if (this.editedConnector.ParentDesigner == null)
                                {
                                    this.editedConnector = connectorContainer.AddConnector(source, target);
                                    WorkflowDesignerLoader loader = this.GetService(typeof(WorkflowDesignerLoader)) as WorkflowDesignerLoader;
                                    if (loader != null)
                                    {
                                        loader.SetModified(true);
                                    }
                                }
                                connectorContainer.OnContainedDesignersConnected(source, target);
                            }
                            flag = true;
                        }
                    }
                    else
                    {
                        flag = true;
                    }
                    if (flag)
                    {
                        this.editedConnector.SetConnectorSegments(this.GetPointsFromEditPoints(this.editPoints));
                        if (this.editedConnector.ParentDesigner != null)
                        {
                            this.editedConnector.ParentDesigner.OnConnectorChanged(new ConnectorEventArgs(this.editedConnector));
                            WorkflowDesignerLoader loader2 = this.GetService(typeof(WorkflowDesignerLoader)) as WorkflowDesignerLoader;
                            if (loader2 != null)
                            {
                                loader2.SetModified(true);
                            }
                        }
                    }
                    this.PerformLayout();
                }
                this.Invalidate();
            }
        }

        public void OnPaint(ActivityDesignerPaintEventArgs e, bool drawSelected, bool drawPrimarySelection)
        {
            List<Point> list = new List<Point>();
            List<Point> list2 = new List<Point>();
            for (int i = 0; i < this.editPoints.Count; i++)
            {
                EditPoint point = this.editPoints[i];
                if ((point.Type == EditPoint.EditPointTypes.ConnectionEditPoint) || (point.Type == EditPoint.EditPointTypes.MultiSegmentEditPoint))
                {
                    list.Add(point.Location);
                }
                else
                {
                    list2.Add(point.Location);
                }
            }
            if (drawSelected)
            {
                this.editedConnector.OnPaintSelected(e, drawPrimarySelection, list2.ToArray());
            }
            if (this.activeEditPoint != null)
            {
                this.editedConnector.OnPaintEdited(e, list.ToArray(), list2.ToArray());
            }
        }

        private void PerformLayout()
        {
            WorkflowView service = this.GetService(typeof(WorkflowView)) as WorkflowView;
            if (service != null)
            {
                service.PerformLayout(false);
            }
        }

        private void RemoveCoincidingEditPoints()
        {
            if (((this.editPoints.Count >= 2) && (this.editPoints[0].Type == EditPoint.EditPointTypes.ConnectionEditPoint)) && ((this.editPoints[this.editPoints.Count - 1].Type == EditPoint.EditPointTypes.ConnectionEditPoint) && ((this.activeEditPoint == null) || (this.activeEditPoint.Type != EditPoint.EditPointTypes.ConnectionEditPoint))))
            {
                this.RemoveEditPoints(EditPoint.EditPointTypes.MidSegmentEditPoint);
                List<EditPoint> list = new List<EditPoint>();
                for (int i = 0; i < this.editPoints.Count; i++)
                {
                    if ((((this.editPoints[i].Type != EditPoint.EditPointTypes.MultiSegmentEditPoint) || (this.editPoints[i] == this.activeEditPoint)) || ((i > 0) && (this.editPoints[i - 1].Type == EditPoint.EditPointTypes.MidSegmentEditPoint))) || ((i < (this.editPoints.Count - 1)) && (this.editPoints[i + 1].Type == EditPoint.EditPointTypes.MidSegmentEditPoint)))
                    {
                        list.Add(this.editPoints[i]);
                    }
                }
                for (int j = 1; j < (this.editPoints.Count - 1); j++)
                {
                    EditPoint point = this.editPoints[j - 1];
                    EditPoint item = this.editPoints[j];
                    EditPoint point3 = this.editPoints[j + 1];
                    if (!list.Contains(item))
                    {
                        Point[] segments = new Point[] { point.Location, item.Location };
                        double num3 = DesignerGeometryHelper.DistanceOfLineSegments(segments);
                        if (((num3 < item.Bounds.Width) || (num3 < item.Bounds.Height)) && (point3.Type == EditPoint.EditPointTypes.MultiSegmentEditPoint))
                        {
                            float num4 = DesignerGeometryHelper.SlopeOfLineSegment(item.Location, point3.Location);
                            point3.Location = (num4 < 1f) ? new Point(point3.Location.X, point.Location.Y) : new Point(point.Location.X, point3.Location.Y);
                            this.editPoints.Remove(item);
                            j--;
                        }
                        else
                        {
                            Point[] pointArray2 = new Point[] { item.Location, point3.Location };
                            num3 = DesignerGeometryHelper.DistanceOfLineSegments(pointArray2);
                            if (((num3 < item.Bounds.Width) || (num3 < item.Bounds.Height)) && (point.Type == EditPoint.EditPointTypes.MultiSegmentEditPoint))
                            {
                                float num5 = DesignerGeometryHelper.SlopeOfLineSegment(point.Location, item.Location);
                                point.Location = (num5 < 1f) ? new Point(point.Location.X, point3.Location.Y) : new Point(point3.Location.X, point.Location.Y);
                                this.editPoints.Remove(item);
                                j--;
                            }
                        }
                    }
                }
                for (int k = 1; k < (this.editPoints.Count - 1); k++)
                {
                    EditPoint point4 = this.editPoints[k];
                    EditPoint point5 = this.editPoints[k - 1];
                    EditPoint point6 = this.editPoints[k + 1];
                    if (!list.Contains(point4))
                    {
                        float num7 = DesignerGeometryHelper.SlopeOfLineSegment(point5.Location, point4.Location);
                        float num8 = DesignerGeometryHelper.SlopeOfLineSegment(point4.Location, point6.Location);
                        if (Math.Abs(num7) == Math.Abs(num8))
                        {
                            this.editPoints.Remove(point4);
                            k--;
                        }
                    }
                }
                for (int m = 0; m < (this.editPoints.Count - 1); m++)
                {
                    EditPoint point7 = this.editPoints[m];
                    EditPoint point8 = this.editPoints[m + 1];
                    float num10 = DesignerGeometryHelper.SlopeOfLineSegment(point7.Location, point8.Location);
                    if ((num10 != 0f) && (num10 != float.MaxValue))
                    {
                        Point point9 = (num10 < 1f) ? new Point(point8.Location.X, point7.Location.Y) : new Point(point7.Location.X, point8.Location.Y);
                        this.editPoints.Insert(m + 1, new EditPoint(this, EditPoint.EditPointTypes.MultiSegmentEditPoint, point9));
                    }
                }
            }
        }

        private void RemoveEditPoints(EditPoint.EditPointTypes editPointType)
        {
            List<EditPoint> list = new List<EditPoint>();
            for (int i = 0; i < this.editPoints.Count; i++)
            {
                EditPoint item = this.editPoints[i];
                if (item.Type == editPointType)
                {
                    list.Add(item);
                }
            }
            for (int j = 0; j < list.Count; j++)
            {
                EditPoint point2 = list[j];
                if (point2 != this.activeEditPoint)
                {
                    this.editPoints.Remove(point2);
                }
            }
        }

        private void UpdateEditPoints(Point newPoint)
        {
            if (((this.editPoints.Count >= 2) && (this.editPoints[0].Type == EditPoint.EditPointTypes.ConnectionEditPoint)) && (this.editPoints[this.editPoints.Count - 1].Type == EditPoint.EditPointTypes.ConnectionEditPoint))
            {
                this.RemoveEditPoints(EditPoint.EditPointTypes.MidSegmentEditPoint);
                if (this.activeEditPoint != null)
                {
                    int index = this.editPoints.IndexOf(this.activeEditPoint);
                    EditPoint point = (index > 0) ? this.editPoints[index - 1] : null;
                    EditPoint point2 = (index < (this.editPoints.Count - 1)) ? this.editPoints[index + 1] : null;
                    if ((point != null) && (point.Type == EditPoint.EditPointTypes.ConnectionEditPoint))
                    {
                        Orientation orientation = (Math.Abs(DesignerGeometryHelper.SlopeOfLineSegment(point.Location, this.activeEditPoint.Location)) < 1f) ? Orientation.Horizontal : Orientation.Vertical;
                        int num3 = Convert.ToInt32(DesignerGeometryHelper.DistanceBetweenPoints(point.Location, (point2 != null) ? point2.Location : this.activeEditPoint.Location)) / 4;
                        if (orientation == Orientation.Horizontal)
                        {
                            num3 *= (point.Location.X < this.activeEditPoint.Location.X) ? 1 : -1;
                        }
                        else
                        {
                            num3 *= (point.Location.Y < this.activeEditPoint.Location.X) ? 1 : -1;
                        }
                        index = this.editPoints.IndexOf(this.activeEditPoint);
                        Point point3 = (orientation == Orientation.Horizontal) ? new Point(point.Location.X + num3, point.Location.Y) : new Point(point.Location.X, point.Location.Y + num3);
                        point = new EditPoint(this, EditPoint.EditPointTypes.MultiSegmentEditPoint, point3);
                        this.editPoints.InsertRange(index, new EditPoint[] { new EditPoint(this, EditPoint.EditPointTypes.MultiSegmentEditPoint, point3), point });
                    }
                    if ((point2 != null) && (point2.Type == EditPoint.EditPointTypes.ConnectionEditPoint))
                    {
                        Orientation orientation2 = (Math.Abs(DesignerGeometryHelper.SlopeOfLineSegment(this.activeEditPoint.Location, point2.Location)) < 1f) ? Orientation.Horizontal : Orientation.Vertical;
                        int num5 = Convert.ToInt32(DesignerGeometryHelper.DistanceBetweenPoints((point != null) ? point.Location : this.activeEditPoint.Location, point2.Location)) / 4;
                        if (orientation2 == Orientation.Horizontal)
                        {
                            num5 *= (this.activeEditPoint.Location.X < point2.Location.X) ? -1 : 1;
                        }
                        else
                        {
                            num5 *= (this.activeEditPoint.Location.Y < point2.Location.Y) ? -1 : 1;
                        }
                        index = this.editPoints.IndexOf(this.activeEditPoint);
                        Point point4 = (orientation2 == Orientation.Horizontal) ? new Point(point2.Location.X + num5, point2.Location.Y) : new Point(point2.Location.X, point2.Location.Y + num5);
                        point2 = new EditPoint(this, EditPoint.EditPointTypes.MultiSegmentEditPoint, point4);
                        this.editPoints.InsertRange(index + 1, new EditPoint[] { point2, new EditPoint(this, EditPoint.EditPointTypes.MultiSegmentEditPoint, point4) });
                    }
                    if (this.activeEditPoint.Type == EditPoint.EditPointTypes.ConnectionEditPoint)
                    {
                        this.activeEditPoint.Location = newPoint;
                        this.RemoveEditPoints(EditPoint.EditPointTypes.MultiSegmentEditPoint);
                        object source = null;
                        object target = null;
                        if (this.activeEditPoint.EditedConnectionPoint.Equals(this.Target))
                        {
                            target = newPoint;
                            source = this.Source;
                        }
                        else
                        {
                            source = newPoint;
                            target = this.Target;
                        }
                        int num6 = (this.editPoints.Count == 2) ? 1 : 0;
                        List<EditPoint> list = new List<EditPoint>();
                        Point[] pointArray = ActivityDesignerConnectorRouter.Route(this.serviceProvider, source, target, this.editedConnector.ExcludedRoutingRectangles);
                        for (int i = num6; i < (pointArray.Length - num6); i++)
                        {
                            list.Add(new EditPoint(this, EditPoint.EditPointTypes.MultiSegmentEditPoint, pointArray[i]));
                        }
                        this.editPoints.InsertRange(1, list.ToArray());
                    }
                    else if (this.activeEditPoint.Type == EditPoint.EditPointTypes.MultiSegmentEditPoint)
                    {
                        if (((point != null) && (point.Type != EditPoint.EditPointTypes.ConnectionEditPoint)) && ((point2 != null) && (point2.Type != EditPoint.EditPointTypes.ConnectionEditPoint)))
                        {
                            Orientation orientation3 = (Math.Abs(DesignerGeometryHelper.SlopeOfLineSegment(point.Location, this.activeEditPoint.Location)) < 1f) ? Orientation.Horizontal : Orientation.Vertical;
                            point.Location = (orientation3 == Orientation.Horizontal) ? new Point(point.Location.X, newPoint.Y) : new Point(newPoint.X, point.Location.Y);
                            orientation3 = (Math.Abs(DesignerGeometryHelper.SlopeOfLineSegment(this.activeEditPoint.Location, point2.Location)) < 1f) ? Orientation.Horizontal : Orientation.Vertical;
                            point2.Location = (orientation3 == Orientation.Horizontal) ? new Point(point2.Location.X, newPoint.Y) : new Point(newPoint.X, point2.Location.Y);
                            this.activeEditPoint.Location = newPoint;
                        }
                    }
                    else if ((((this.activeEditPoint.Type == EditPoint.EditPointTypes.MidSegmentEditPoint) && (point != null)) && ((point.Type != EditPoint.EditPointTypes.ConnectionEditPoint) && (point2 != null))) && (point2.Type != EditPoint.EditPointTypes.ConnectionEditPoint))
                    {
                        if (((Math.Abs(DesignerGeometryHelper.SlopeOfLineSegment(point.Location, point2.Location)) < 1f) ? 0 : 1) == 0)
                        {
                            point.Location = new Point(point.Location.X, newPoint.Y);
                            point2.Location = new Point(point2.Location.X, newPoint.Y);
                            this.activeEditPoint.Location = new Point(this.activeEditPoint.Location.X, newPoint.Y);
                        }
                        else
                        {
                            point.Location = new Point(newPoint.X, point.Location.Y);
                            point2.Location = new Point(newPoint.X, point2.Location.Y);
                            this.activeEditPoint.Location = new Point(newPoint.X, this.activeEditPoint.Location.Y);
                        }
                    }
                }
                this.RemoveCoincidingEditPoints();
                this.AddEditPoints(EditPoint.EditPointTypes.MidSegmentEditPoint);
                this.ValidateEditPoints();
            }
        }

        private bool ValidateEditPoints()
        {
            if (this.editPoints.Count < 2)
            {
                return false;
            }
            ConnectionPoint editedConnectionPoint = this.editPoints[0].EditedConnectionPoint;
            if ((editedConnectionPoint == null) || !editedConnectionPoint.Equals(this.Source))
            {
                return false;
            }
            ConnectionPoint point2 = this.editPoints[this.editPoints.Count - 1].EditedConnectionPoint;
            if ((point2 == null) || !point2.Equals(this.Target))
            {
                return false;
            }
            for (int i = 0; i < (this.editPoints.Count - 1); i++)
            {
                if ((this.editPoints[i].Type == EditPoint.EditPointTypes.MidSegmentEditPoint) && (this.editPoints[i + 1].Type == EditPoint.EditPointTypes.MidSegmentEditPoint))
                {
                    return false;
                }
            }
            return true;
        }

        public ConnectionPoint EditedConectionPoint
        {
            get
            {
                if (this.activeEditPoint != null)
                {
                    return this.activeEditPoint.EditedConnectionPoint;
                }
                return null;
            }
        }

        public Connector EditedConnector
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.editedConnector;
            }
        }

        private ConnectionPoint Source
        {
            get
            {
                return this.editedConnector.Source;
            }
        }

        private ConnectionPoint Target
        {
            get
            {
                return this.editedConnector.Target;
            }
        }

        private sealed class EditPoint
        {
            private ConnectionPoint connectionPoint;
            private EditPointTypes editPointType;
            private ConnectorEditor owner;
            private Point point;

            public EditPoint(ConnectorEditor owner, ConnectionPoint connectionPoint)
            {
                this.owner = owner;
                this.editPointType = EditPointTypes.ConnectionEditPoint;
                this.connectionPoint = connectionPoint;
                this.point = connectionPoint.Location;
            }

            public EditPoint(ConnectorEditor owner, EditPointTypes editPointType, Point point)
            {
                this.owner = owner;
                this.editPointType = editPointType;
                this.point = point;
            }

            public Rectangle Bounds
            {
                get
                {
                    Size size = this.owner.Source.Bounds.Size;
                    return new Rectangle(this.point.X - (size.Width / 2), this.point.Y - (size.Height / 2), size.Width, size.Height);
                }
            }

            public ConnectionPoint EditedConnectionPoint
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.connectionPoint;
                }
            }

            public Point Location
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.point;
                }
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                set
                {
                    this.point = value;
                }
            }

            public EditPointTypes Type
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.editPointType;
                }
            }

            public enum EditPointTypes
            {
                ConnectionEditPoint = 1,
                MidSegmentEditPoint = 3,
                MultiSegmentEditPoint = 2
            }
        }
    }
}

