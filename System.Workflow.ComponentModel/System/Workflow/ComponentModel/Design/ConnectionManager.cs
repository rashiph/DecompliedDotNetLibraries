namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    internal sealed class ConnectionManager : WorkflowDesignerMessageFilter, IDesignerGlyphProvider
    {
        private ConnectionPoint[] connectablePoints;
        private ConnectorEditor connectorEditor;
        private System.Workflow.ComponentModel.Design.HitTestInfo dragPointHitInfo;
        private const int HighlightDistance = 20;
        private Point? initialDragPoint = null;
        internal static Cursor NewConnectorCursor = new Cursor(typeof(WorkflowView), "Resources.ConnectorDraw.cur");
        private const int SnapHighlightDistance = 20;
        internal static Cursor SnappedConnectionCursor = new Cursor(typeof(WorkflowView), "Resources.Connector.cur");
        private ConnectionPoint snappedConnectionPoint;

        private void BeginEditing(ConnectorEditor editableConnector, Point editPoint)
        {
            WorkflowView parentView = base.ParentView;
            if ((parentView != null) && (editableConnector != null))
            {
                this.connectorEditor = editableConnector;
                parentView.Capture = true;
                this.connectorEditor.OnBeginEditing(editPoint);
            }
        }

        private bool CanBeginEditing(Point editPoint, System.Workflow.ComponentModel.Design.HitTestInfo messageContext)
        {
            ISelectionService service = base.GetService(typeof(ISelectionService)) as ISelectionService;
            if (service != null)
            {
                Connector connectorFromSelectedObject = Connector.GetConnectorFromSelectedObject(service.PrimarySelection);
                if (((connectorFromSelectedObject != null) && connectorFromSelectedObject.ParentDesigner.EnableUserDrawnConnectors) && new ConnectorEditor(connectorFromSelectedObject).HitTest(editPoint))
                {
                    return true;
                }
            }
            ConnectionPointHitTestInfo info = messageContext as ConnectionPointHitTestInfo;
            if ((info != null) && (info.ConnectionPoint != null))
            {
                FreeformActivityDesigner connectorContainer = GetConnectorContainer(info.AssociatedDesigner);
                if ((connectorContainer != null) && connectorContainer.EnableUserDrawnConnectors)
                {
                    return true;
                }
            }
            return false;
        }

        private void ContinueEditing(Point editPoint)
        {
            if (this.EditingInProgress)
            {
                ConnectionPoint[] pointArray = null;
                if (this.connectorEditor.EditedConectionPoint != null)
                {
                    ConnectionPoint sourceConnectionPoint = (this.connectorEditor.EditedConnector.Source == this.connectorEditor.EditedConectionPoint) ? this.connectorEditor.EditedConnector.Target : this.connectorEditor.EditedConnector.Source;
                    pointArray = GetSnappableConnectionPoints(editPoint, sourceConnectionPoint, this.connectorEditor.EditedConectionPoint, base.MessageHitTestContext.AssociatedDesigner, out this.snappedConnectionPoint);
                }
                this.ConnectablePoints = pointArray;
                if (this.SnappedConnectionPoint != null)
                {
                    editPoint = this.SnappedConnectionPoint.Location;
                }
                this.connectorEditor.OnContinueEditing(editPoint);
            }
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    IServiceContainer container = base.GetService(typeof(IServiceContainer)) as IServiceContainer;
                    if (container != null)
                    {
                        container.RemoveService(typeof(ConnectionManager));
                    }
                    IDesignerGlyphProviderService service = base.GetService(typeof(IDesignerGlyphProviderService)) as IDesignerGlyphProviderService;
                    if (service != null)
                    {
                        service.RemoveGlyphProvider(this);
                    }
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        private void EndEditing(Point? editPoint)
        {
            WorkflowView parentView = base.ParentView;
            if (parentView != null)
            {
                if (this.EditingInProgress)
                {
                    if (editPoint.HasValue)
                    {
                        if (this.connectorEditor.EditedConectionPoint != null)
                        {
                            ConnectionPoint sourceConnectionPoint = (this.connectorEditor.EditedConnector.Source == this.connectorEditor.EditedConectionPoint) ? this.connectorEditor.EditedConnector.Target : this.connectorEditor.EditedConnector.Source;
                            GetSnappableConnectionPoints(editPoint.Value, sourceConnectionPoint, this.connectorEditor.EditedConectionPoint, base.MessageHitTestContext.AssociatedDesigner, out this.snappedConnectionPoint);
                        }
                        if (this.SnappedConnectionPoint != null)
                        {
                            editPoint = new Point?(this.SnappedConnectionPoint.Location);
                        }
                    }
                    this.connectorEditor.OnEndEditing(editPoint.HasValue ? editPoint.Value : Point.Empty, editPoint.HasValue);
                }
                this.initialDragPoint = null;
                this.dragPointHitInfo = null;
                this.snappedConnectionPoint = null;
                this.ConnectablePoints = null;
                parentView.Capture = false;
                this.connectorEditor = null;
            }
        }

        internal static FreeformActivityDesigner GetConnectorContainer(ActivityDesigner associatedDesigner)
        {
            FreeformActivityDesigner designer = null;
            if (associatedDesigner != null)
            {
                for (ActivityDesigner designer2 = associatedDesigner; designer2 != null; designer2 = designer2.ParentDesigner)
                {
                    if (designer2 is FreeformActivityDesigner)
                    {
                        designer = designer2 as FreeformActivityDesigner;
                    }
                    else if (designer2 is InvokeWorkflowDesigner)
                    {
                        return designer;
                    }
                }
            }
            return designer;
        }

        private ConnectorEditor GetConnectorEditor(Point editPoint, System.Workflow.ComponentModel.Design.HitTestInfo messageContext)
        {
            Connector connectorEdited = null;
            ISelectionService service = base.GetService(typeof(ISelectionService)) as ISelectionService;
            if (service != null)
            {
                Connector connectorFromSelectedObject = Connector.GetConnectorFromSelectedObject(service.PrimarySelection);
                if (((connectorFromSelectedObject != null) && connectorFromSelectedObject.ParentDesigner.EnableUserDrawnConnectors) && new ConnectorEditor(connectorFromSelectedObject).HitTest(editPoint))
                {
                    connectorEdited = connectorFromSelectedObject;
                }
            }
            if (connectorEdited == null)
            {
                ConnectionPointHitTestInfo info = messageContext as ConnectionPointHitTestInfo;
                if ((info != null) && (info.ConnectionPoint != null))
                {
                    FreeformActivityDesigner connectorContainer = GetConnectorContainer(info.AssociatedDesigner);
                    if ((connectorContainer != null) && connectorContainer.EnableUserDrawnConnectors)
                    {
                        connectorEdited = connectorContainer.CreateConnector(info.ConnectionPoint, info.ConnectionPoint);
                    }
                }
            }
            if (connectorEdited == null)
            {
                return null;
            }
            return new ConnectorEditor(connectorEdited);
        }

        private static ConnectionPoint[] GetHighlightableConnectionPoints(Point currentPoint, ActivityDesigner activityDesigner)
        {
            List<ConnectionPoint> list = new List<ConnectionPoint>();
            List<ActivityDesigner> list2 = new List<ActivityDesigner>();
            FreeformActivityDesigner designer = activityDesigner as FreeformActivityDesigner;
            if (designer != null)
            {
                list2.AddRange(designer.ContainedDesigners);
            }
            list2.Add(activityDesigner);
            foreach (ActivityDesigner designer2 in list2)
            {
                bool flag = designer2.Bounds.Contains(currentPoint);
                ReadOnlyCollection<ConnectionPoint> connectionPoints = designer2.GetConnectionPoints(DesignerEdges.All);
                if (!flag)
                {
                    foreach (ConnectionPoint point in connectionPoints)
                    {
                        if (point.Bounds.Contains(currentPoint))
                        {
                            flag = true;
                            break;
                        }
                    }
                }
                if (flag)
                {
                    list.AddRange(connectionPoints);
                }
            }
            return list.ToArray();
        }

        private static ConnectionPoint[] GetSnappableConnectionPoints(Point currentPoint, ConnectionPoint sourceConnectionPoint, ConnectionPoint activeConnectionPoint, ActivityDesigner activityDesigner, out ConnectionPoint snappedConnectionPoint)
        {
            snappedConnectionPoint = null;
            List<ConnectionPoint> list = new List<ConnectionPoint>();
            FreeformActivityDesigner connectorContainer = GetConnectorContainer(activeConnectionPoint.AssociatedDesigner);
            if (connectorContainer != null)
            {
                FreeformActivityDesigner designer2 = activityDesigner as FreeformActivityDesigner;
                List<ActivityDesigner> list2 = new List<ActivityDesigner> {
                    activityDesigner
                };
                if (designer2 != null)
                {
                    list2.AddRange(designer2.ContainedDesigners);
                }
                double num = 20.0;
                foreach (ActivityDesigner designer3 in list2)
                {
                    if (GetConnectorContainer(designer3) == connectorContainer)
                    {
                        bool flag = false;
                        List<ConnectionPoint> collection = new List<ConnectionPoint>();
                        foreach (ConnectionPoint point in designer3.GetConnectionPoints(DesignerEdges.All))
                        {
                            if (!point.Equals(activeConnectionPoint) && connectorContainer.CanConnectContainedDesigners(sourceConnectionPoint, point))
                            {
                                collection.Add(point);
                                double num2 = DesignerGeometryHelper.DistanceFromPointToRectangle(currentPoint, point.Bounds);
                                if (num2 <= 20.0)
                                {
                                    flag = true;
                                    if (num2 < num)
                                    {
                                        snappedConnectionPoint = point;
                                        num = num2;
                                    }
                                }
                            }
                        }
                        if (flag)
                        {
                            list.AddRange(collection);
                        }
                    }
                }
                if (snappedConnectionPoint != null)
                {
                    foreach (ConnectionPoint point2 in snappedConnectionPoint.AssociatedDesigner.GetConnectionPoints(DesignerEdges.All))
                    {
                        if (!list.Contains(point2))
                        {
                            list.Add(point2);
                        }
                    }
                }
            }
            return list.ToArray();
        }

        protected override void Initialize(WorkflowView parentView)
        {
            base.Initialize(parentView);
            IServiceContainer container = base.GetService(typeof(IServiceContainer)) as IServiceContainer;
            if (container != null)
            {
                container.RemoveService(typeof(ConnectionManager));
                container.AddService(typeof(ConnectionManager), this);
            }
            IDesignerGlyphProviderService service = base.GetService(typeof(IDesignerGlyphProviderService)) as IDesignerGlyphProviderService;
            if (service != null)
            {
                service.AddGlyphProvider(this);
            }
        }

        protected override bool OnKeyDown(KeyEventArgs eventArgs)
        {
            if (this.EditingInProgress && (eventArgs.KeyValue == 0x1b))
            {
                this.EndEditing(null);
                eventArgs.Handled = true;
            }
            return eventArgs.Handled;
        }

        protected override bool OnMouseCaptureChanged()
        {
            this.EndEditing(null);
            this.UpdateCursor(null);
            return false;
        }

        protected override bool OnMouseDown(MouseEventArgs eventArgs)
        {
            Point empty = Point.Empty;
            if ((eventArgs.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                WorkflowView parentView = base.ParentView;
                Point clientPoint = new Point(eventArgs.X, eventArgs.Y);
                if ((parentView != null) && parentView.IsClientPointInActiveLayout(clientPoint))
                {
                    Point editPoint = parentView.ClientPointToLogical(clientPoint);
                    if (this.CanBeginEditing(editPoint, base.MessageHitTestContext))
                    {
                        this.initialDragPoint = new Point?(editPoint);
                        this.dragPointHitInfo = base.MessageHitTestContext;
                    }
                    empty = editPoint;
                }
            }
            else
            {
                this.EndEditing(null);
            }
            return (this.initialDragPoint.HasValue | this.UpdateCursor(new Point?(empty)));
        }

        protected override bool OnMouseEnter(MouseEventArgs eventArgs)
        {
            Point empty = Point.Empty;
            Point clientPoint = new Point(eventArgs.X, eventArgs.Y);
            WorkflowView parentView = base.ParentView;
            if (((parentView != null) && parentView.IsClientPointInActiveLayout(clientPoint)) && !this.EditingInProgress)
            {
                FreeformActivityDesigner connectorContainer = GetConnectorContainer(base.MessageHitTestContext.AssociatedDesigner);
                if ((connectorContainer != null) && connectorContainer.EnableUserDrawnConnectors)
                {
                    Point currentPoint = parentView.ClientPointToLogical(clientPoint);
                    this.ConnectablePoints = GetHighlightableConnectionPoints(currentPoint, base.MessageHitTestContext.AssociatedDesigner);
                    empty = currentPoint;
                }
            }
            return this.UpdateCursor(new Point?(empty));
        }

        protected override bool OnMouseLeave()
        {
            this.EndEditing(null);
            this.UpdateCursor(null);
            return false;
        }

        protected override bool OnMouseMove(MouseEventArgs eventArgs)
        {
            Point empty = Point.Empty;
            WorkflowView parentView = base.ParentView;
            Point clientPoint = new Point(eventArgs.X, eventArgs.Y);
            if ((parentView != null) && parentView.IsClientPointInActiveLayout(clientPoint))
            {
                Point editPoint = parentView.ClientPointToLogical(clientPoint);
                if ((eventArgs.Button & MouseButtons.Left) == MouseButtons.Left)
                {
                    if ((!this.EditingInProgress && this.initialDragPoint.HasValue) && ((Math.Abs((int) (this.initialDragPoint.Value.X - editPoint.X)) > SystemInformation.DragSize.Width) || (Math.Abs((int) (this.initialDragPoint.Value.Y - editPoint.Y)) > SystemInformation.DragSize.Height)))
                    {
                        ConnectorEditor connectorEditor = this.GetConnectorEditor(this.initialDragPoint.Value, this.dragPointHitInfo);
                        this.BeginEditing(connectorEditor, this.initialDragPoint.Value);
                    }
                    if (this.EditingInProgress)
                    {
                        this.ContinueEditing(editPoint);
                        if (this.SnappedConnectionPoint != null)
                        {
                            editPoint = this.SnappedConnectionPoint.Location;
                        }
                    }
                }
                else
                {
                    FreeformActivityDesigner connectorContainer = GetConnectorContainer(base.MessageHitTestContext.AssociatedDesigner);
                    this.ConnectablePoints = ((connectorContainer != null) && connectorContainer.EnableUserDrawnConnectors) ? GetHighlightableConnectionPoints(editPoint, base.MessageHitTestContext.AssociatedDesigner) : null;
                }
                empty = editPoint;
            }
            return (this.EditingInProgress | this.UpdateCursor(new Point?(empty)));
        }

        protected override bool OnMouseUp(MouseEventArgs eventArgs)
        {
            Point empty = Point.Empty;
            bool editingInProgress = this.EditingInProgress;
            if ((eventArgs.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                WorkflowView parentView = base.ParentView;
                Point clientPoint = new Point(eventArgs.X, eventArgs.Y);
                if ((parentView != null) && parentView.IsClientPointInActiveLayout(clientPoint))
                {
                    Point point3 = parentView.ClientPointToLogical(clientPoint);
                    if (this.EditingInProgress)
                    {
                        this.EndEditing(new Point?(point3));
                    }
                    empty = point3;
                }
            }
            this.EndEditing(null);
            return (editingInProgress | this.UpdateCursor(new Point?(empty)));
        }

        protected override bool OnPaint(PaintEventArgs e, Rectangle viewPort, AmbientTheme ambientTheme)
        {
            Connector connectorEdited = null;
            ISelectionService service = base.GetService(typeof(ISelectionService)) as ISelectionService;
            foreach (object obj2 in service.GetSelectedComponents())
            {
                Connector connectorFromSelectedObject = Connector.GetConnectorFromSelectedObject(obj2);
                if (connectorFromSelectedObject != null)
                {
                    connectorFromSelectedObject.OnPaintSelected(new ActivityDesignerPaintEventArgs(e.Graphics, connectorFromSelectedObject.ParentDesigner.Bounds, viewPort, connectorFromSelectedObject.ParentDesigner.DesignerTheme), obj2 == service.PrimarySelection, new Point[0]);
                    if (obj2 == service.PrimarySelection)
                    {
                        connectorEdited = connectorFromSelectedObject;
                    }
                }
            }
            if (connectorEdited != null)
            {
                new ConnectorEditor(connectorEdited).OnPaint(new ActivityDesignerPaintEventArgs(e.Graphics, connectorEdited.ParentDesigner.Bounds, viewPort, connectorEdited.ParentDesigner.DesignerTheme), true, true);
            }
            if (this.EditingInProgress)
            {
                FreeformActivityDesigner designer = (this.connectorEditor.EditedConnector.ParentDesigner != null) ? this.connectorEditor.EditedConnector.ParentDesigner : GetConnectorContainer(this.connectorEditor.EditedConnector.Source.AssociatedDesigner);
                this.connectorEditor.OnPaint(new ActivityDesignerPaintEventArgs(e.Graphics, designer.Bounds, viewPort, designer.DesignerTheme), false, false);
            }
            return false;
        }

        ActivityDesignerGlyphCollection IDesignerGlyphProvider.GetGlyphs(ActivityDesigner activityDesigner)
        {
            ActivityDesignerGlyphCollection glyphs = new ActivityDesignerGlyphCollection();
            ConnectionPoint[] connectablePoints = this.ConnectablePoints;
            if (connectablePoints != null)
            {
                foreach (ConnectionPoint point in connectablePoints)
                {
                    if (activityDesigner == point.AssociatedDesigner)
                    {
                        glyphs.Add(new ConnectionPointGlyph(point));
                    }
                }
            }
            return glyphs;
        }

        private bool UpdateCursor(Point? cursorPoint)
        {
            Cursor snappedConnectionCursor = Cursors.Default;
            if (cursorPoint.HasValue)
            {
                if (this.EditingInProgress)
                {
                    snappedConnectionCursor = this.connectorEditor.GetCursor(cursorPoint.Value);
                }
                if (this.SnappedConnectionPoint != null)
                {
                    snappedConnectionCursor = SnappedConnectionCursor;
                }
                else if (this.ConnectablePoints != null)
                {
                    foreach (ConnectionPoint point in this.ConnectablePoints)
                    {
                        if (point.Bounds.Contains(cursorPoint.Value))
                        {
                            snappedConnectionCursor = SnappedConnectionCursor;
                            break;
                        }
                    }
                    if (snappedConnectionCursor == Cursors.Default)
                    {
                        ISelectionService service = base.GetService(typeof(ISelectionService)) as ISelectionService;
                        if (service != null)
                        {
                            Connector connectorFromSelectedObject = Connector.GetConnectorFromSelectedObject(service.PrimarySelection);
                            if ((connectorFromSelectedObject != null) && connectorFromSelectedObject.ParentDesigner.EnableUserDrawnConnectors)
                            {
                                snappedConnectionCursor = new ConnectorEditor(connectorFromSelectedObject).GetCursor(cursorPoint.Value);
                            }
                        }
                    }
                }
            }
            WorkflowView parentView = base.ParentView;
            if ((parentView != null) && (((snappedConnectionCursor != Cursors.Default) || (parentView.Cursor == SnappedConnectionCursor)) || (parentView.Cursor == NewConnectorCursor)))
            {
                parentView.Cursor = snappedConnectionCursor;
            }
            return (snappedConnectionCursor != Cursors.Default);
        }

        private ConnectionPoint[] ConnectablePoints
        {
            get
            {
                return this.connectablePoints;
            }
            set
            {
                WorkflowView parentView = base.ParentView;
                if (parentView != null)
                {
                    if (this.connectablePoints != null)
                    {
                        foreach (ConnectionPoint point in this.connectablePoints)
                        {
                            parentView.InvalidateLogicalRectangle(point.Bounds);
                        }
                    }
                    this.connectablePoints = value;
                    if (this.connectablePoints != null)
                    {
                        foreach (ConnectionPoint point2 in this.connectablePoints)
                        {
                            parentView.InvalidateLogicalRectangle(point2.Bounds);
                        }
                    }
                }
            }
        }

        private bool EditingInProgress
        {
            get
            {
                return (this.connectorEditor != null);
            }
        }

        internal ConnectionPoint SnappedConnectionPoint
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.snappedConnectionPoint;
            }
        }
    }
}

