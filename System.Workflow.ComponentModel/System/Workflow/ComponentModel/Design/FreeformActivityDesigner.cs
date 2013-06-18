namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.ComponentModel.Design.Serialization;
    using System.Drawing;
    using System.Runtime;
    using System.Threading;
    using System.Windows.Forms;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Serialization;

    [DesignerSerializer(typeof(FreeformActivityDesignerLayoutSerializer), typeof(WorkflowMarkupSerializer))]
    public class FreeformActivityDesigner : CompositeActivityDesigner
    {
        private FreeformDesignerAccessibleObject accessibilityObject;
        private bool autoSize = true;
        private Size autoSizeMargin = DefaultAutoSizeMargin;
        private System.Windows.Forms.AutoSizeMode autoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
        private List<Connector> connectors = new List<Connector>();
        internal static Size DefaultAutoSizeMargin = new Size(40, 40);
        private bool enableUserDrawnConnectors = true;
        private List<ActivityDesigner> movedActivities;
        private bool retainContainedDesignerLocations;

        public event ConnectorEventHandler ConnectorAdded;

        public event ConnectorEventHandler ConnectorChanged;

        public event ConnectorEventHandler ConnectorRemoved;

        public Connector AddConnector(ConnectionPoint source, ConnectionPoint target)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (source.AssociatedDesigner == null)
            {
                throw new ArgumentException("source", SR.GetString("Error_AssociatedDesignerMissing"));
            }
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }
            if (target.AssociatedDesigner == null)
            {
                throw new ArgumentException("target", SR.GetString("Error_AssociatedDesignerMissing"));
            }
            FreeformActivityDesigner connectorContainer = ConnectionManager.GetConnectorContainer(source.AssociatedDesigner);
            if (this != connectorContainer)
            {
                throw new InvalidOperationException(DR.GetString("Error_AddConnector1", new object[0]));
            }
            if (((base.Activity != source.AssociatedDesigner.Activity) && !Helpers.IsChildActivity(base.Activity as CompositeActivity, source.AssociatedDesigner.Activity)) || ((base.Activity != target.AssociatedDesigner.Activity) && !Helpers.IsChildActivity(base.Activity as CompositeActivity, target.AssociatedDesigner.Activity)))
            {
                throw new ArgumentException(DR.GetString("Error_AddConnector2", new object[0]));
            }
            Connector item = this.CreateConnector(source, target);
            if (item != null)
            {
                if (this.connectors.Contains(item))
                {
                    throw new InvalidOperationException(DR.GetString("Error_AddConnector3", new object[0]));
                }
                this.connectors.Add(item);
                item.SetParent(this);
                this.OnConnectorAdded(new ConnectorEventArgs(item));
            }
            base.PerformLayout();
            return item;
        }

        public void BringToFront(ActivityDesigner containedDesigner)
        {
            if (containedDesigner == null)
            {
                throw new ArgumentNullException("containedDesigner");
            }
            if (!this.ContainedDesigners.Contains(containedDesigner))
            {
                throw new ArgumentException(DR.GetString("InvalidDesignerSpecified", new object[] { "containedDesigner" }));
            }
            this.UpdateZOrder(containedDesigner, ZOrder.Foreground);
        }

        protected internal virtual bool CanConnectContainedDesigners(ConnectionPoint source, ConnectionPoint target)
        {
            return (((IConnectableDesigner) source.AssociatedDesigner).CanConnect(source, target) && ((IConnectableDesigner) target.AssociatedDesigner).CanConnect(source, target));
        }

        protected internal virtual bool CanResizeContainedDesigner(ActivityDesigner containedDesigner)
        {
            return (containedDesigner is FreeformActivityDesigner);
        }

        internal bool CanUpdateZOrder(ActivityDesigner activityDesigner, ZOrder zorder)
        {
            CompositeActivityDesigner parentDesigner = this;
            ActivityDesigner designer2 = activityDesigner;
            while ((parentDesigner != null) && (designer2 != null))
            {
                if (parentDesigner is FreeformActivityDesigner)
                {
                    ReadOnlyCollection<ActivityDesigner> containedDesigners = parentDesigner.ContainedDesigners;
                    if ((containedDesigners.Count > 1) && (containedDesigners[(zorder == ZOrder.Background) ? 0 : (containedDesigners.Count - 1)] != designer2))
                    {
                        return true;
                    }
                }
                designer2 = parentDesigner;
                parentDesigner = parentDesigner.ParentDesigner;
            }
            return false;
        }

        protected internal virtual Connector CreateConnector(ConnectionPoint source, ConnectionPoint target)
        {
            return new Connector(source, target);
        }

        protected override void Dispose(bool disposing)
        {
            for (int i = 0; i < this.connectors.Count; i++)
            {
                this.connectors[i].Dispose();
            }
            this.connectors.Clear();
            base.Dispose(disposing);
        }

        private void EnsureDesignerExtender()
        {
            bool flag = true;
            IExtenderListService service = base.GetService(typeof(IExtenderListService)) as IExtenderListService;
            if (service != null)
            {
                foreach (IExtenderProvider provider in service.GetExtenderProviders())
                {
                    if (provider.GetType() == typeof(FreeFormDesignerPropertyExtender))
                    {
                        flag = false;
                        break;
                    }
                }
            }
            if (flag)
            {
                IExtenderProviderService service2 = base.GetService(typeof(IExtenderProviderService)) as IExtenderProviderService;
                if (service2 != null)
                {
                    service2.AddExtenderProvider(new FreeFormDesignerPropertyExtender());
                    TypeDescriptor.Refresh(base.Activity);
                }
            }
        }

        private Rectangle GetEnclosingRectangle()
        {
            Point point = new Point(0x7fffffff, 0x7fffffff);
            Point point2 = new Point(-2147483648, -2147483648);
            foreach (ActivityDesigner designer in this.ContainedDesigners)
            {
                if (designer.IsVisible)
                {
                    point.X = (designer.Bounds.Left < point.X) ? designer.Bounds.Left : point.X;
                    point.Y = (designer.Bounds.Top < point.Y) ? designer.Bounds.Top : point.Y;
                    point2.X = (point2.X < designer.Bounds.Right) ? designer.Bounds.Right : point2.X;
                    point2.Y = (point2.Y < designer.Bounds.Bottom) ? designer.Bounds.Bottom : point2.Y;
                }
            }
            if (this.InvokingDesigner == null)
            {
                foreach (Connector connector in this.Connectors)
                {
                    point.X = (connector.Bounds.Left < point.X) ? connector.Bounds.Left : point.X;
                    point.Y = (connector.Bounds.Top < point.Y) ? connector.Bounds.Top : point.Y;
                    point2.X = (point2.X < connector.Bounds.Right) ? connector.Bounds.Right : point2.X;
                    point2.Y = (point2.Y < connector.Bounds.Bottom) ? connector.Bounds.Bottom : point2.Y;
                }
            }
            Rectangle empty = Rectangle.Empty;
            if ((point.X != 0x7fffffff) && (point2.X != -2147483648))
            {
                empty.X = point.X;
                empty.Width = point2.X - point.X;
            }
            if ((point.Y != 0x7fffffff) && (point2.Y != -2147483648))
            {
                empty.Y = point.Y;
                empty.Height = point2.Y - point.Y;
            }
            if (!empty.IsEmpty)
            {
                empty.Inflate(this.AutoSizeMargin);
            }
            return empty;
        }

        public override object GetNextSelectableObject(object current, DesignerNavigationDirection navigate)
        {
            object activity = null;
            ArrayList list = new ArrayList(this.ContainedDesigners);
            ActivityDesigner designer = (current is Activity) ? ActivityDesigner.GetDesigner(current as Activity) : ActivityDesigner.GetParentDesigner(current);
            int num = (designer != null) ? list.IndexOf(designer) : -1;
            if (((navigate == DesignerNavigationDirection.Left) || (navigate == DesignerNavigationDirection.Up)) && ((num >= 0) && (num < list.Count)))
            {
                return ((ActivityDesigner) list[(num > 0) ? (num - 1) : (list.Count - 1)]).Activity;
            }
            if (((navigate == DesignerNavigationDirection.Right) || (navigate == DesignerNavigationDirection.Down)) && (num <= (list.Count - 1)))
            {
                activity = ((ActivityDesigner) list[(num < (list.Count - 1)) ? (num + 1) : 0]).Activity;
            }
            return activity;
        }

        public override System.Workflow.ComponentModel.Design.HitTestInfo HitTest(Point point)
        {
            System.Workflow.ComponentModel.Design.HitTestInfo info = base.HitTest(point);
            ReadOnlyCollection<ActivityDesigner> containedDesigners = this.ContainedDesigners;
            WorkflowView parentView = base.ParentView;
            DragDropManager service = base.GetService(typeof(DragDropManager)) as DragDropManager;
            if ((((parentView != null) && (service != null)) && (parentView.DragDropInProgress && (info.AssociatedDesigner != null))) && (service.DraggedActivities.Contains(info.AssociatedDesigner.Activity) && info.AssociatedDesigner.Bounds.Contains(point)))
            {
                if (base.Activity == info.AssociatedDesigner.Activity)
                {
                    return System.Workflow.ComponentModel.Design.HitTestInfo.Nowhere;
                }
                if (containedDesigners.Contains(info.AssociatedDesigner))
                {
                    return new System.Workflow.ComponentModel.Design.HitTestInfo(this, HitTestLocations.Designer);
                }
            }
            if (!(info is ConnectionPointHitTestInfo) && (((info.HitLocation == HitTestLocations.None) || (info.AssociatedDesigner == this)) || this.ShowConnectorsInForeground))
            {
                for (int i = 0; i < this.connectors.Count; i++)
                {
                    if (this.connectors[i].HitTest(point))
                    {
                        return new ConnectorHitTestInfo(this, HitTestLocations.Connector | HitTestLocations.Designer, i);
                    }
                }
            }
            return info;
        }

        protected override void Initialize(Activity activity)
        {
            base.Initialize(activity);
            this.EnsureDesignerExtender();
        }

        public override void InsertActivities(System.Workflow.ComponentModel.Design.HitTestInfo insertLocation, ReadOnlyCollection<Activity> activitiesToInsert)
        {
            base.InsertActivities(insertLocation, activitiesToInsert);
            if (this.AutoSize)
            {
                Size autoSizeMargin = this.AutoSizeMargin;
                Point location = this.Location;
                foreach (Activity activity in activitiesToInsert)
                {
                    ActivityDesigner designer = ActivityDesigner.GetDesigner(activity);
                    if (designer.Location.IsEmpty)
                    {
                        designer.Location = new Point(location.X + autoSizeMargin.Width, location.Y + autoSizeMargin.Height);
                    }
                }
            }
        }

        public override void MoveActivities(System.Workflow.ComponentModel.Design.HitTestInfo moveLocation, ReadOnlyCollection<Activity> activitiesToMove)
        {
            if (moveLocation == null)
            {
                throw new ArgumentNullException("moveLocation");
            }
            if (activitiesToMove == null)
            {
                throw new ArgumentNullException("activitiesToMove");
            }
            FreeformActivityDesigner connectorContainer = ConnectionManager.GetConnectorContainer(this);
            try
            {
                connectorContainer.MovingActivities.Clear();
                if ((connectorContainer != null) && (connectorContainer.Connectors.Count > 0))
                {
                    foreach (Activity activity in activitiesToMove)
                    {
                        ActivityDesigner designer = ActivityDesigner.GetDesigner(activity);
                        if (ConnectionManager.GetConnectorContainer(designer) == connectorContainer)
                        {
                            connectorContainer.MovingActivities.Add(designer);
                        }
                    }
                }
                base.MoveActivities(moveLocation, activitiesToMove);
            }
            finally
            {
                connectorContainer.MovingActivities.Clear();
            }
        }

        public void MoveContainedDesigner(ActivityDesigner containedDesigner, Point newLocation)
        {
            if (containedDesigner == null)
            {
                throw new ArgumentNullException("containedDesigner");
            }
            if (!this.ContainedDesigners.Contains(containedDesigner))
            {
                throw new ArgumentException(DR.GetString("InvalidDesignerSpecified", new object[] { "containedDesigner" }));
            }
            SetDesignerBounds(containedDesigner, new Rectangle(newLocation, containedDesigner.Size));
            base.PerformLayout();
            base.Invalidate();
        }

        protected virtual void OnConnectorAdded(ConnectorEventArgs e)
        {
            if (this.ConnectorAdded != null)
            {
                this.ConnectorAdded(this, e);
            }
        }

        protected internal virtual void OnConnectorChanged(ConnectorEventArgs e)
        {
            if (this.ConnectorChanged != null)
            {
                this.ConnectorChanged(this, e);
            }
        }

        protected virtual void OnConnectorRemoved(ConnectorEventArgs e)
        {
            if (this.ConnectorRemoved != null)
            {
                this.ConnectorRemoved(this, e);
            }
        }

        protected override void OnContainedActivitiesChanging(ActivityCollectionChangeEventArgs listChangeArgs)
        {
            base.OnContainedActivitiesChanging(listChangeArgs);
            if (listChangeArgs.Action == ActivityCollectionChangeAction.Remove)
            {
                FreeformActivityDesigner connectorContainer = ConnectionManager.GetConnectorContainer(this);
                List<Connector> list = new List<Connector>();
                for (ActivityDesigner designer2 = this; designer2 != null; designer2 = designer2.ParentDesigner)
                {
                    FreeformActivityDesigner designer3 = designer2 as FreeformActivityDesigner;
                    if ((designer3 != null) && (designer3.Connectors.Count > 0))
                    {
                        foreach (Activity activity in listChangeArgs.RemovedItems)
                        {
                            ActivityDesigner designer = ActivityDesigner.GetDesigner(activity);
                            if (!connectorContainer.MovingActivities.Contains(designer))
                            {
                                foreach (Connector connector in designer3.Connectors)
                                {
                                    if ((designer == connector.Source.AssociatedDesigner) || (designer == connector.Target.AssociatedDesigner))
                                    {
                                        list.Add(connector);
                                    }
                                }
                            }
                        }
                    }
                }
                foreach (Connector connector2 in list)
                {
                    connector2.ParentDesigner.RemoveConnector(connector2);
                    ((IDisposable) connector2).Dispose();
                }
            }
        }

        protected internal virtual void OnContainedDesignersConnected(ConnectionPoint source, ConnectionPoint target)
        {
            ((IConnectableDesigner) source.AssociatedDesigner).OnConnected(source, target);
            ((IConnectableDesigner) target.AssociatedDesigner).OnConnected(source, target);
        }

        protected override void OnDragDrop(ActivityDragEventArgs e)
        {
            if (((e.KeyState & 8) == 8) && ((e.AllowedEffect & DragDropEffects.Copy) == DragDropEffects.Copy))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else if ((e.AllowedEffect & DragDropEffects.Move) == DragDropEffects.Move)
            {
                e.Effect = DragDropEffects.Move;
            }
            List<Activity> list = new List<Activity>();
            List<Activity> list2 = new List<Activity>();
            foreach (Activity activity in e.Activities)
            {
                if ((activity.Site == null) || (activity.Parent != base.Activity))
                {
                    list.Add(activity);
                }
                if (activity.Site == null)
                {
                    list2.Add(activity);
                }
            }
            if (list.Count > 0)
            {
                CompositeActivityDesigner.InsertActivities(this, new ConnectorHitTestInfo(this, HitTestLocations.Designer, ((CompositeActivity) base.Activity).Activities.Count), list.AsReadOnly(), SR.GetString("DragDropActivities"));
            }
            Point endPoint = new Point(e.X, e.Y);
            Point[] pointArray = FreeFormDragDropManager.GetDesignerLocations(e.DragInitiationPoint, endPoint, e.Activities);
            if (pointArray.Length == e.Activities.Count)
            {
                for (int i = 0; i < e.Activities.Count; i++)
                {
                    ActivityDesigner containedDesigner = ActivityDesigner.GetDesigner(e.Activities[i]);
                    if (containedDesigner != null)
                    {
                        Point newLocation = list2.Contains(containedDesigner.Activity) ? endPoint : pointArray[i];
                        this.MoveContainedDesigner(containedDesigner, newLocation);
                    }
                }
            }
        }

        protected override void OnDragOver(ActivityDragEventArgs e)
        {
            if (((e.KeyState & 8) == 8) && ((e.AllowedEffect & DragDropEffects.Copy) == DragDropEffects.Copy))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else if ((e.AllowedEffect & DragDropEffects.Move) == DragDropEffects.Move)
            {
                e.Effect = DragDropEffects.Move;
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }
            ISelectionService service = base.GetService(typeof(ISelectionService)) as ISelectionService;
            if (((service != null) ? service.PrimarySelection : null) != null)
            {
                List<Activity> list = new List<Activity>(Helpers.GetTopLevelActivities(service.GetSelectedComponents()));
                if (((e.KeyCode == Keys.Left) || (e.KeyCode == Keys.Right)) || ((e.KeyCode == Keys.Up) || (e.KeyCode == Keys.Down)))
                {
                    Size empty = Size.Empty;
                    if (e.KeyCode == Keys.Left)
                    {
                        empty = new Size(-5, 0);
                    }
                    else if (e.KeyCode == Keys.Right)
                    {
                        empty = new Size(5, 0);
                    }
                    else if (e.KeyCode == Keys.Up)
                    {
                        empty = new Size(0, -5);
                    }
                    else if (e.KeyCode == Keys.Down)
                    {
                        empty = new Size(0, 5);
                    }
                    foreach (Activity activity in list)
                    {
                        ActivityDesigner designer = ActivityDesigner.GetDesigner(activity);
                        if (designer != null)
                        {
                            base.ParentView.InvalidateClientRectangle(new Rectangle(designer.Location, designer.Size));
                            designer.Location += empty;
                            base.ParentView.InvalidateClientRectangle(new Rectangle(designer.Location, designer.Size));
                        }
                    }
                    base.PerformLayout();
                    e.Handled = true;
                }
                else if (e.KeyCode == Keys.Delete)
                {
                    foreach (object obj3 in service.GetSelectedComponents())
                    {
                        ConnectorHitTestInfo info = obj3 as ConnectorHitTestInfo;
                        if (info != null)
                        {
                            FreeformActivityDesigner associatedDesigner = info.AssociatedDesigner as FreeformActivityDesigner;
                            if (associatedDesigner != null)
                            {
                                ReadOnlyCollection<Connector> connectors = associatedDesigner.Connectors;
                                int num = info.MapToIndex();
                                if (num < connectors.Count)
                                {
                                    service.SetSelectedComponents(new object[] { info }, SelectionTypes.Remove);
                                    associatedDesigner.RemoveConnector(connectors[num]);
                                    object obj4 = associatedDesigner;
                                    if (connectors.Count > 0)
                                    {
                                        obj4 = new ConnectorHitTestInfo(associatedDesigner, HitTestLocations.Connector | HitTestLocations.Designer, (num > 0) ? (num - 1) : num);
                                    }
                                    service.SetSelectedComponents(new object[] { obj4 }, SelectionTypes.Replace);
                                }
                            }
                        }
                    }
                    e.Handled = true;
                }
                if (!e.Handled)
                {
                    base.OnKeyDown(e);
                }
            }
        }

        protected override void OnLayoutPosition(ActivityDesignerLayoutEventArgs e)
        {
            base.OnLayoutPosition(e);
            if (this.AutoSize)
            {
                Point location = this.Location;
                Rectangle enclosingRectangle = this.GetEnclosingRectangle();
                if (!enclosingRectangle.IsEmpty)
                {
                    if (this.AutoSizeMode == System.Windows.Forms.AutoSizeMode.GrowOnly)
                    {
                        location.X = Math.Min(location.X, enclosingRectangle.Left);
                        location.Y = Math.Min(location.Y, enclosingRectangle.Top);
                    }
                    else
                    {
                        location = enclosingRectangle.Location;
                    }
                }
                this.retainContainedDesignerLocations = true;
                this.Location = location;
                this.retainContainedDesignerLocations = false;
            }
            foreach (Connector connector in this.connectors)
            {
                connector.OnLayout(e);
            }
        }

        protected override Size OnLayoutSize(ActivityDesignerLayoutEventArgs e)
        {
            Rectangle bounds = base.Bounds;
            Size size = bounds.Size;
            base.OnLayoutSize(e);
            if (!this.AutoSize)
            {
                return size;
            }
            if (this.AutoSizeMode == System.Windows.Forms.AutoSizeMode.GrowOnly)
            {
                Rectangle enclosingRectangle = this.GetEnclosingRectangle();
                if (!enclosingRectangle.IsEmpty)
                {
                    size.Width += Math.Max(bounds.Left - enclosingRectangle.Left, 0);
                    size.Width += Math.Max(enclosingRectangle.Right - bounds.Right, 0);
                    size.Height += Math.Max(bounds.Top - enclosingRectangle.Top, 0);
                    size.Height += Math.Max(enclosingRectangle.Bottom - bounds.Bottom, 0);
                }
                return size;
            }
            return this.MinimumSize;
        }

        internal override void OnPaintContainedDesigners(ActivityDesignerPaintEventArgs e)
        {
            if (this.ShowConnectorsInForeground)
            {
                base.OnPaintContainedDesigners(e);
            }
            FreeformActivityDesigner connectorContainer = ConnectionManager.GetConnectorContainer(this);
            if (((connectorContainer != null) && (base.Activity != null)) && (base.Activity.Site != null))
            {
                Region region = null;
                Region clip = e.Graphics.Clip;
                try
                {
                    if (clip != null)
                    {
                        region = new Region(connectorContainer.Bounds);
                        region.Intersect(e.ViewPort);
                        e.Graphics.Clip = region;
                    }
                    foreach (Connector connector in connectorContainer.Connectors)
                    {
                        if (this == connector.RenderingOwner)
                        {
                            connector.OnPaint(e);
                        }
                    }
                }
                finally
                {
                    if (clip != null)
                    {
                        e.Graphics.Clip = clip;
                        region.Dispose();
                    }
                }
            }
            if (!this.ShowConnectorsInForeground)
            {
                base.OnPaintContainedDesigners(e);
            }
        }

        protected override void OnResizing(ActivityDesignerResizeEventArgs e)
        {
            if (this.AutoSize)
            {
                if (this.AutoSizeMode == System.Windows.Forms.AutoSizeMode.GrowOnly)
                {
                    Rectangle enclosingRectangle = this.GetEnclosingRectangle();
                    if (!enclosingRectangle.IsEmpty)
                    {
                        Rectangle empty = Rectangle.Empty;
                        empty.X = Math.Min(enclosingRectangle.Left, e.Bounds.Left);
                        empty.Y = Math.Min(enclosingRectangle.Top, e.Bounds.Top);
                        empty.Width = Math.Max((int) (enclosingRectangle.Right - empty.Left), (int) (e.Bounds.Right - empty.Left));
                        empty.Height = Math.Max((int) (enclosingRectangle.Bottom - empty.Top), (int) (e.Bounds.Bottom - empty.Top));
                        if (empty != e.Bounds)
                        {
                            e = new ActivityDesignerResizeEventArgs(e.SizingEdge, empty);
                        }
                    }
                }
                else
                {
                    base.PerformLayout();
                }
            }
            this.retainContainedDesignerLocations = true;
            base.OnResizing(e);
            this.retainContainedDesignerLocations = false;
        }

        protected override void OnThemeChange(ActivityDesignerTheme newTheme)
        {
            base.OnThemeChange(newTheme);
            if (WorkflowTheme.CurrentTheme.AmbientTheme.ShowGrid)
            {
                foreach (ActivityDesigner designer in this.ContainedDesigners)
                {
                    designer.Location = DesignerHelpers.SnapToGrid(designer.Location);
                }
                base.PerformLayout();
            }
        }

        public void RemoveConnector(Connector connector)
        {
            if (connector == null)
            {
                throw new ArgumentNullException("connector");
            }
            if (this.connectors.Contains(connector))
            {
                this.OnConnectorRemoved(new ConnectorEventArgs(connector));
                connector.SetParent(null);
                this.connectors.Remove(connector);
            }
        }

        public void ResizeContainedDesigner(ActivityDesigner containedDesigner, Size newSize)
        {
            if (containedDesigner == null)
            {
                throw new ArgumentNullException("containedDesigner");
            }
            if (!this.ContainedDesigners.Contains(containedDesigner))
            {
                throw new ArgumentException(DR.GetString("InvalidDesignerSpecified", new object[] { "containedDesigner" }));
            }
            SetDesignerBounds(containedDesigner, new Rectangle(containedDesigner.Location, newSize));
            base.PerformLayout();
        }

        public void SendToBack(ActivityDesigner containedDesigner)
        {
            if (containedDesigner == null)
            {
                throw new ArgumentNullException("containedDesigner");
            }
            if (!this.ContainedDesigners.Contains(containedDesigner))
            {
                throw new ArgumentException(DR.GetString("InvalidDesignerSpecified", new object[] { "containedDesigner" }));
            }
            this.UpdateZOrder(containedDesigner, ZOrder.Background);
        }

        internal static void SetDesignerBounds(ActivityDesigner designer, Rectangle bounds)
        {
            if (((designer != null) && (designer.Activity != null)) && (designer.Activity.Site != null))
            {
                PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(designer);
                PropertyDescriptor descriptor = (properties != null) ? properties["Size"] : null;
                if (descriptor != null)
                {
                    descriptor.SetValue(designer.Activity, bounds.Size);
                }
                else
                {
                    designer.Size = bounds.Size;
                }
                PropertyDescriptor descriptor2 = (properties != null) ? properties["Location"] : null;
                if (descriptor2 != null)
                {
                    descriptor2.SetValue(designer.Activity, bounds.Location);
                }
                else
                {
                    designer.Location = bounds.Location;
                }
                WorkflowView service = designer.Activity.Site.GetService(typeof(WorkflowView)) as WorkflowView;
                if (service != null)
                {
                    if (designer.ParentDesigner != null)
                    {
                        service.InvalidateLogicalRectangle(designer.ParentDesigner.Bounds);
                    }
                    else
                    {
                        service.Invalidate();
                    }
                }
            }
        }

        private void UpdateZOrder(ActivityDesigner activityDesigner, ZOrder zorder)
        {
            IDesignerHost service = base.GetService(typeof(IDesignerHost)) as IDesignerHost;
            DesignerTransaction transaction = null;
            if (service != null)
            {
                transaction = service.CreateTransaction(DR.GetString("ZOrderUndoDescription", new object[] { activityDesigner.Text }));
            }
            try
            {
                bool flag = false;
                CompositeActivityDesigner parentDesigner = this;
                ActivityDesigner designer2 = activityDesigner;
                while ((parentDesigner != null) && (designer2 != null))
                {
                    if (parentDesigner is FreeformActivityDesigner)
                    {
                        ReadOnlyCollection<ActivityDesigner> containedDesigners = parentDesigner.ContainedDesigners;
                        if ((containedDesigners.Count > 1) && (containedDesigners[(zorder == ZOrder.Background) ? 0 : (containedDesigners.Count - 1)] != designer2))
                        {
                            int connector = (zorder == ZOrder.Background) ? 0 : containedDesigners.Count;
                            parentDesigner.MoveActivities(new ConnectorHitTestInfo(this, HitTestLocations.Designer, connector), new List<Activity>(new Activity[] { designer2.Activity }).AsReadOnly());
                            flag = true;
                        }
                    }
                    designer2 = parentDesigner;
                    parentDesigner = parentDesigner.ParentDesigner;
                }
                if (flag)
                {
                    base.Invalidate();
                }
                if (transaction != null)
                {
                    transaction.Commit();
                }
            }
            catch (Exception exception)
            {
                if (transaction != null)
                {
                    transaction.Cancel();
                }
                throw exception;
            }
        }

        public override AccessibleObject AccessibilityObject
        {
            get
            {
                if (this.accessibilityObject == null)
                {
                    this.accessibilityObject = new FreeformDesignerAccessibleObject(this);
                }
                return this.accessibilityObject;
            }
        }

        [DefaultValue(true)]
        public bool AutoSize
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.autoSize;
            }
            set
            {
                if (this.autoSize != value)
                {
                    this.autoSize = value;
                    base.PerformLayout();
                }
            }
        }

        public Size AutoSizeMargin
        {
            get
            {
                Size autoSizeMargin = this.autoSizeMargin;
                if (WorkflowTheme.CurrentTheme.AmbientTheme.ShowGrid)
                {
                    Size gridSize = WorkflowTheme.CurrentTheme.AmbientTheme.GridSize;
                    autoSizeMargin.Width += gridSize.Width / 2;
                    autoSizeMargin.Height += gridSize.Height / 2;
                }
                return autoSizeMargin;
            }
            set
            {
                if (this.autoSizeMargin != value)
                {
                    this.autoSizeMargin = value;
                    base.PerformLayout();
                }
            }
        }

        [DefaultValue(1)]
        public System.Windows.Forms.AutoSizeMode AutoSizeMode
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.autoSizeMode;
            }
            set
            {
                if (this.autoSizeMode != value)
                {
                    this.autoSizeMode = value;
                    base.PerformLayout();
                }
            }
        }

        public override bool CanExpandCollapse
        {
            get
            {
                return false;
            }
        }

        public ReadOnlyCollection<Connector> Connectors
        {
            get
            {
                return this.connectors.AsReadOnly();
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content), EditorBrowsable(EditorBrowsableState.Never)]
        internal List<Connector> DesignerConnectors
        {
            get
            {
                return new List<Connector>(this.connectors);
            }
        }

        [DefaultValue(true)]
        public bool EnableUserDrawnConnectors
        {
            get
            {
                return (this.enableUserDrawnConnectors && base.IsEditable);
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.enableUserDrawnConnectors = value;
            }
        }

        protected internal override bool EnableVisualResizing
        {
            get
            {
                if (this.AutoSize && (this.AutoSizeMode == System.Windows.Forms.AutoSizeMode.GrowAndShrink))
                {
                    return false;
                }
                return true;
            }
        }

        public override object FirstSelectableObject
        {
            get
            {
                IList<ActivityDesigner> containedDesigners = this.ContainedDesigners;
                if (containedDesigners.Count <= 0)
                {
                    return null;
                }
                return containedDesigners[0].Activity;
            }
        }

        protected internal override ActivityDesignerGlyphCollection Glyphs
        {
            get
            {
                ActivityDesignerGlyphCollection glyphs = new ActivityDesignerGlyphCollection();
                glyphs.AddRange(base.Glyphs);
                ISelectionService service = base.GetService(typeof(ISelectionService)) as ISelectionService;
                if (service != null)
                {
                    foreach (object obj2 in service.GetSelectedComponents())
                    {
                        ConnectorHitTestInfo info = obj2 as ConnectorHitTestInfo;
                        if ((info != null) && (info.AssociatedDesigner == this))
                        {
                            glyphs.Add(new FreeFormConnectorSelectionGlyph(info.MapToIndex(), info == service.PrimarySelection));
                        }
                    }
                }
                return glyphs;
            }
        }

        public override object LastSelectableObject
        {
            get
            {
                IList<ActivityDesigner> containedDesigners = this.ContainedDesigners;
                if (containedDesigners.Count <= 0)
                {
                    return null;
                }
                return containedDesigners[containedDesigners.Count - 1].Activity;
            }
        }

        public override Point Location
        {
            get
            {
                return base.Location;
            }
            set
            {
                if (this.Location != value)
                {
                    ReadOnlyCollection<ActivityDesigner> containedDesigners = this.ContainedDesigners;
                    List<Point> list = new List<Point>();
                    if (this.retainContainedDesignerLocations)
                    {
                        foreach (ActivityDesigner designer in containedDesigners)
                        {
                            list.Add(designer.Location);
                        }
                    }
                    else
                    {
                        Size size = new Size(value.X - base.Location.X, value.Y - base.Location.Y);
                        FreeformActivityDesigner parentDesigner = this;
                        Collection<Connector> collection = new Collection<Connector>();
                        while (parentDesigner != null)
                        {
                            foreach (Connector connector in parentDesigner.Connectors)
                            {
                                if (connector.RenderingOwner == this)
                                {
                                    collection.Add(connector);
                                }
                            }
                            parentDesigner = parentDesigner.ParentDesigner as FreeformActivityDesigner;
                        }
                        foreach (Connector connector2 in collection)
                        {
                            connector2.Offset(size);
                        }
                    }
                    base.Location = value;
                    if (this.retainContainedDesignerLocations && (containedDesigners.Count == list.Count))
                    {
                        for (int i = 0; i < containedDesigners.Count; i++)
                        {
                            containedDesigners[i].Location = list[i];
                        }
                    }
                    base.Invalidate();
                }
            }
        }

        public override Size MinimumSize
        {
            get
            {
                Size minimumSize = base.MinimumSize;
                if (((base.Activity != null) && (base.Activity.Site != null)) && !(base.ParentDesigner is FreeformActivityDesigner))
                {
                    minimumSize.Width *= 4;
                    minimumSize.Height *= 4;
                }
                if (base.IsRootDesigner && (this.InvokingDesigner == null))
                {
                    WorkflowView parentView = base.ParentView;
                    minimumSize.Width = Math.Max(minimumSize.Width, parentView.ViewPortSize.Width - (2 * DefaultWorkflowLayout.Separator.Width));
                    minimumSize.Height = Math.Max(minimumSize.Height, parentView.ViewPortSize.Height - (2 * DefaultWorkflowLayout.Separator.Height));
                }
                if (this.AutoSize)
                {
                    Rectangle enclosingRectangle = this.GetEnclosingRectangle();
                    if (!enclosingRectangle.IsEmpty)
                    {
                        minimumSize.Width = Math.Max(minimumSize.Width, enclosingRectangle.Width);
                        minimumSize.Height = Math.Max(minimumSize.Height, enclosingRectangle.Height);
                    }
                }
                return minimumSize;
            }
        }

        private List<ActivityDesigner> MovingActivities
        {
            get
            {
                if (this.movedActivities == null)
                {
                    this.movedActivities = new List<ActivityDesigner>();
                }
                return this.movedActivities;
            }
        }

        protected virtual bool ShowConnectorsInForeground
        {
            get
            {
                return false;
            }
        }

        internal override WorkflowLayout SupportedLayout
        {
            get
            {
                return new WorkflowRootLayout(base.Activity.Site);
            }
        }

        private sealed class FreeFormConnectorSelectionGlyph : ConnectorSelectionGlyph
        {
            internal FreeFormConnectorSelectionGlyph(int connectorIndex, bool isPrimarySelectionGlyph) : base(connectorIndex, isPrimarySelectionGlyph)
            {
            }

            public override Rectangle GetBounds(ActivityDesigner designer, bool activated)
            {
                return Rectangle.Empty;
            }

            protected override void OnPaint(Graphics graphics, bool activated, AmbientTheme ambientTheme, ActivityDesigner designer)
            {
            }

            public override bool IsPrimarySelection
            {
                get
                {
                    return base.isPrimarySelectionGlyph;
                }
            }
        }

        internal class FreeformDesignerAccessibleObject : CompositeDesignerAccessibleObject
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            public FreeformDesignerAccessibleObject(FreeformActivityDesigner activityDesigner) : base(activityDesigner)
            {
            }

            public override AccessibleObject GetChild(int index)
            {
                FreeformActivityDesigner activityDesigner = (FreeformActivityDesigner) base.ActivityDesigner;
                if (activityDesigner.ShowConnectorsInForeground)
                {
                    int num = activityDesigner.Connectors.Count;
                    if (index < num)
                    {
                        return activityDesigner.Connectors[index].AccessibilityObject;
                    }
                    return activityDesigner.ContainedDesigners[index - num].AccessibilityObject;
                }
                int count = activityDesigner.ContainedDesigners.Count;
                if (index < count)
                {
                    return activityDesigner.ContainedDesigners[index].AccessibilityObject;
                }
                return activityDesigner.Connectors[index - count].AccessibilityObject;
            }

            public override int GetChildCount()
            {
                FreeformActivityDesigner activityDesigner = (FreeformActivityDesigner) base.ActivityDesigner;
                return (base.GetChildCount() + activityDesigner.Connectors.Count);
            }
        }

        [ProvideProperty("Location", typeof(Activity)), ProvideProperty("Size", typeof(Activity))]
        private sealed class FreeFormDesignerPropertyExtender : IExtenderProvider
        {
            [MergableProperty(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false), DesignOnly(true)]
            public Point GetLocation(Activity activity)
            {
                Point empty = Point.Empty;
                ActivityDesigner designer = ActivityDesigner.GetDesigner(activity);
                if (designer != null)
                {
                    empty = designer.Location;
                }
                return empty;
            }

            [MergableProperty(false), Browsable(false), DesignOnly(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public Size GetSize(Activity activity)
            {
                Size empty = Size.Empty;
                ActivityDesigner designer = ActivityDesigner.GetDesigner(activity);
                if (designer != null)
                {
                    empty = designer.Size;
                }
                return empty;
            }

            public void SetLocation(Activity activity, Point location)
            {
                ActivityDesigner designer = ActivityDesigner.GetDesigner(activity);
                if (designer != null)
                {
                    FreeformActivityDesigner designer2 = (designer.ParentDesigner != null) ? (designer.ParentDesigner as FreeformActivityDesigner) : (designer as FreeformActivityDesigner);
                    if (designer2 != null)
                    {
                        designer.Location = location;
                        if (designer2.AutoSize)
                        {
                            designer2.PerformLayout();
                        }
                    }
                }
            }

            public void SetSize(Activity activity, Size size)
            {
                ActivityDesigner designer = ActivityDesigner.GetDesigner(activity);
                if (designer != null)
                {
                    FreeformActivityDesigner designer2 = (designer.ParentDesigner != null) ? (designer.ParentDesigner as FreeformActivityDesigner) : (designer as FreeformActivityDesigner);
                    if (designer2 != null)
                    {
                        designer.Size = size;
                        if (designer2.AutoSize)
                        {
                            designer2.PerformLayout();
                        }
                    }
                }
            }

            bool IExtenderProvider.CanExtend(object extendee)
            {
                bool flag = false;
                Activity activity = extendee as Activity;
                if (activity != null)
                {
                    ActivityDesigner designer = ActivityDesigner.GetDesigner(activity);
                    if ((designer != null) && (((designer.ParentDesigner != null) ? (designer.ParentDesigner as FreeformActivityDesigner) : (designer as FreeformActivityDesigner)) != null))
                    {
                        flag = true;
                    }
                }
                return flag;
            }
        }
    }
}

