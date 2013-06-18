namespace System.Workflow.Activities
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.ComponentModel.Design.Serialization;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Windows.Forms;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;
    using System.Workflow.ComponentModel.Serialization;

    [ComVisible(false), ActivityDesignerTheme(typeof(StateDesignerTheme)), DesignerSerializer(typeof(StateDesignerLayoutSerializer), typeof(WorkflowMarkupSerializer))]
    internal class StateDesigner : FreeformActivityDesigner
    {
        private ActivityDesigner _activeDesigner;
        private bool _addingSetState = true;
        private Dictionary<Activity, DesignerLayout> _designerLayouts;
        private DesignerLinkLayout _designerLinkLayout;
        private ContainedDesignersParser _designersParser;
        private bool _dragDropActive;
        internal bool _ensuringVisible;
        private EventDrivenLayout _eventDrivenLayout;
        private EventHandlersLayout _eventHandlersLayout;
        private string _helpText;
        private System.Drawing.Size _minimumSize = System.Drawing.Size.Empty;
        private bool _needsAutoLayout;
        private bool _performingLayout;
        private bool _removingSetState = true;
        private Layout _rootDesignerLayout;
        private StateDesigner _rootStateDesigner;
        private ISelectionService _selectionService;
        private Point _stateLocation;
        private System.Drawing.Size _stateMinimumSize;
        private System.Drawing.Size _stateSize;
        private StatesLayout _statesLayout;
        private TitleBarLayout _titleBarLayout;
        private ActivityDesignerVerbCollection _verbs;
        private const string ActiveDesignerNamePropertyName = "ActiveDesignerName";
        internal static readonly System.Drawing.Image CompletedState = System.Workflow.Activities.DR.GetImage("CompletedState");
        private const int DefaultStateDesignerAutoLayoutDistance = 0x10;
        internal static readonly System.Drawing.Image InitialState = System.Workflow.Activities.DR.GetImage("InitialState");
        internal static System.Drawing.Size Separator = new System.Drawing.Size(30, 30);

        internal void AddChild(Activity child)
        {
            if ((base.Activity is CompositeActivity) && (child != null))
            {
                int count = this.ContainedDesigners.Count;
                System.Workflow.ComponentModel.Design.HitTestInfo insertLocation = new System.Workflow.ComponentModel.Design.HitTestInfo(this, HitTestLocations.Designer);
                CompositeActivityDesigner.InsertActivities(this, insertLocation, new List<Activity>(new Activity[] { child }).AsReadOnly(), string.Format(CultureInfo.InvariantCulture, System.Workflow.Activities.DR.GetString("AddingChild"), new object[] { child.GetType().Name }));
                if ((this.ContainedDesigners.Count > count) && (this.ContainedDesigners.Count > 0))
                {
                    this.ContainedDesigners[this.ContainedDesigners.Count - 1].EnsureVisible();
                }
                this.SelectionService.SetSelectedComponents(new object[] { child }, SelectionTypes.Click);
            }
        }

        public override bool CanBeParentedTo(CompositeActivityDesigner parentActivityDesigner)
        {
            if (parentActivityDesigner == null)
            {
                throw new ArgumentNullException("parentActivityDesigner");
            }
            CompositeActivity activity = parentActivityDesigner.Activity as CompositeActivity;
            return (((activity != null) && (activity is StateActivity)) && base.CanBeParentedTo(parentActivityDesigner));
        }

        protected override bool CanConnect(ConnectionPoint source, ConnectionPoint target)
        {
            DesignerLayoutConnectionPoint point = source as DesignerLayoutConnectionPoint;
            DesignerLayoutConnectionPoint point2 = target as DesignerLayoutConnectionPoint;
            if (point == null)
            {
                if (!this.IsValidTargetConnectionPoint(source))
                {
                    return false;
                }
            }
            else if (point.DesignerLayout.ActivityDesigner is StateFinalizationDesigner)
            {
                return false;
            }
            if (point2 == null)
            {
                if (!this.IsValidTargetConnectionPoint(target))
                {
                    return false;
                }
            }
            else if (point2.DesignerLayout.ActivityDesigner is StateFinalizationDesigner)
            {
                return false;
            }
            return (((point == null) && (point2 != null)) || ((point != null) && (point2 == null)));
        }

        private bool CanDrop(ActivityDragEventArgs e)
        {
            if (e.Activities.Count == 0)
            {
                return false;
            }
            if (this.HasActiveDesigner)
            {
                return false;
            }
            if (!this.CanInsertActivities(new System.Workflow.ComponentModel.Design.HitTestInfo(this, HitTestLocations.Designer), e.Activities))
            {
                return false;
            }
            if (((e.KeyState & 8) != 8) && ((e.AllowedEffect & DragDropEffects.Move) == DragDropEffects.Move))
            {
                System.Workflow.ComponentModel.Design.HitTestInfo moveLocation = new System.Workflow.ComponentModel.Design.HitTestInfo(this, HitTestLocations.Designer);
                foreach (Activity activity in e.Activities)
                {
                    if (activity.Site != null)
                    {
                        ActivityDesigner designer = GetDesigner(activity);
                        if (((designer == null) || (designer.ParentDesigner == null)) || !designer.ParentDesigner.CanMoveActivities(moveLocation, new List<Activity>(new Activity[] { activity }).AsReadOnly()))
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        public override bool CanInsertActivities(System.Workflow.ComponentModel.Design.HitTestInfo insertLocation, ReadOnlyCollection<Activity> activitiesToInsert)
        {
            if (this.HasActiveDesigner || this.IsStateCustomActivity)
            {
                return false;
            }
            StateActivity state = (StateActivity) base.Activity;
            if (StateMachineHelpers.IsLeafState(state) && StateMachineHelpers.IsCompletedState(state))
            {
                return false;
            }
            ReadOnlyCollection<System.Type> validChildTypes = this.ValidChildTypes;
            foreach (Activity activity2 in activitiesToInsert)
            {
                bool flag = false;
                foreach (System.Type type in validChildTypes)
                {
                    if (type.IsInstanceOfType(activity2))
                    {
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                {
                    return false;
                }
            }
            return base.CanInsertActivities(insertLocation, activitiesToInsert);
        }

        protected override bool CanResizeContainedDesigner(ActivityDesigner containedDesigner)
        {
            if (this.HasActiveDesigner)
            {
                return false;
            }
            return base.CanResizeContainedDesigner(containedDesigner);
        }

        private DragDropEffects CheckDragEffect(ActivityDragEventArgs e)
        {
            if ((e.Activities.Count == 0) || !this.DragDropActive)
            {
                return DragDropEffects.None;
            }
            if (((e.KeyState & 8) == 8) && ((e.AllowedEffect & DragDropEffects.Copy) == DragDropEffects.Copy))
            {
                return DragDropEffects.Copy;
            }
            if ((e.AllowedEffect & DragDropEffects.Move) == DragDropEffects.Move)
            {
                return DragDropEffects.Move;
            }
            return e.Effect;
        }

        protected override Connector CreateConnector(ConnectionPoint source, ConnectionPoint target)
        {
            return new StateDesignerConnector(source, target);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this._designerLinkLayout.MouseDown -= new MouseEventHandler(this.StateDesignerLinkMouseDown);
            }
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
                    if (provider.GetType() == typeof(StateDesignerPropertyExtender))
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
                    service2.AddExtenderProvider(new StateDesignerPropertyExtender());
                    TypeDescriptor.Refresh(base.Activity);
                }
            }
        }

        public override void EnsureVisibleContainedDesigner(ActivityDesigner containedDesigner)
        {
            if (containedDesigner == null)
            {
                throw new ArgumentNullException("containedDesigner");
            }
            base.EnsureVisibleContainedDesigner(containedDesigner);
            if (!this._ensuringVisible)
            {
                if (containedDesigner is StateDesigner)
                {
                    this.SetActiveDesigner(null);
                }
                else
                {
                    this.SetActiveDesigner(containedDesigner);
                }
                this.SetParentTreeEnsuringVisible(true);
            }
            this._ensuringVisible = false;
        }

        private static Activity FindActivityByQualifiedName(Activity activity, string qualifiedName)
        {
            Queue<Activity> queue = new Queue<Activity>();
            queue.Enqueue(activity);
            while (queue.Count > 0)
            {
                activity = queue.Dequeue();
                if (activity.QualifiedName.Equals(qualifiedName))
                {
                    return activity;
                }
                CompositeActivity activity2 = activity as CompositeActivity;
                if (activity2 != null)
                {
                    foreach (Activity activity3 in activity2.Activities)
                    {
                        queue.Enqueue(activity3);
                    }
                }
            }
            return null;
        }

        internal StateDesignerConnector FindConnector(TransitionInfo transitionInfo)
        {
            foreach (Connector connector in base.Connectors)
            {
                StateDesignerConnector stateDesignerConnector = connector as StateDesignerConnector;
                if ((stateDesignerConnector != null) && transitionInfo.Matches(stateDesignerConnector))
                {
                    return stateDesignerConnector;
                }
            }
            return null;
        }

        internal static System.Drawing.Image GetCompletedStateDesignerImage(StateDesigner stateDesigner)
        {
            StateMachineTheme designerTheme = stateDesigner.DesignerTheme as StateMachineTheme;
            if ((designerTheme != null) && (designerTheme.CompletedStateDesignerImage != null))
            {
                return designerTheme.CompletedStateDesignerImage;
            }
            return CompletedState;
        }

        public override ReadOnlyCollection<ConnectionPoint> GetConnectionPoints(DesignerEdges edges)
        {
            List<ConnectionPoint> list = new List<ConnectionPoint>();
            if (!this.HasActiveDesigner)
            {
                if (!this.IsRootStateDesigner)
                {
                    if ((edges & DesignerEdges.Top) > DesignerEdges.None)
                    {
                        list.Add(new ConnectionPoint(this, DesignerEdges.Top, 0));
                    }
                    if ((edges & DesignerEdges.Bottom) > DesignerEdges.None)
                    {
                        list.Add(new ConnectionPoint(this, DesignerEdges.Bottom, 0));
                    }
                }
                int connectionIndex = 0;
                int num2 = 0;
                foreach (DesignerLayout layout in this.DesignerLayouts.Values)
                {
                    if ((!this.IsRootStateDesigner && ((edges & DesignerEdges.Left) > DesignerEdges.None)) && (layout.LeftConnectionPoint != Point.Empty))
                    {
                        list.Add(new DesignerLayoutConnectionPoint(this, connectionIndex, (CompositeActivity) layout.ActivityDesigner.Activity, DesignerEdges.Left));
                        connectionIndex++;
                    }
                    if (((edges & DesignerEdges.Right) > DesignerEdges.None) && (layout.RightConnectionPoint != Point.Empty))
                    {
                        list.Add(new DesignerLayoutConnectionPoint(this, num2, (CompositeActivity) layout.ActivityDesigner.Activity, DesignerEdges.Right));
                        num2++;
                    }
                }
            }
            return list.AsReadOnly();
        }

        protected override ReadOnlyCollection<Point> GetConnections(DesignerEdges edges)
        {
            List<Point> list = new List<Point>();
            if ((edges & DesignerEdges.Top) > DesignerEdges.None)
            {
                list.Add(this.TopConnectionPoint);
            }
            if ((edges & DesignerEdges.Bottom) > DesignerEdges.None)
            {
                list.Add(this.BottomConnectionPoint);
            }
            foreach (DesignerLayout layout in this.DesignerLayouts.Values)
            {
                if ((!this.IsRootStateDesigner && ((edges & DesignerEdges.Left) > DesignerEdges.None)) && (layout.LeftConnectionPoint != Point.Empty))
                {
                    list.Add(layout.LeftConnectionPoint);
                }
                if (((edges & DesignerEdges.Right) > DesignerEdges.None) && (layout.RightConnectionPoint != Point.Empty))
                {
                    list.Add(layout.RightConnectionPoint);
                }
            }
            return list.AsReadOnly();
        }

        internal static ActivityDesigner GetDesigner(Activity activity)
        {
            ActivityDesigner designer = null;
            if ((activity != null) && (activity.Site != null))
            {
                IDesignerHost service = activity.Site.GetService(typeof(IDesignerHost)) as IDesignerHost;
                if (service != null)
                {
                    designer = service.GetDesigner(activity) as ActivityDesigner;
                }
            }
            return designer;
        }

        internal static System.Drawing.Image GetDesignerImage(ActivityDesigner designer)
        {
            if ((designer.DesignerTheme != null) && (designer.DesignerTheme.DesignerImage != null))
            {
                return designer.DesignerTheme.DesignerImage;
            }
            if (designer.Image == null)
            {
                return ActivityToolboxItem.GetToolboxImage(designer.Activity.GetType());
            }
            return designer.Image;
        }

        private Point GetDragImageSnapPoint(ActivityDragEventArgs e)
        {
            Point point = new Point(e.Y, e.Y);
            if (!this.HasActiveDesigner)
            {
                int bottom = this._statesLayout.EventHandlersLayout.Bounds.Bottom;
                if (point.Y <= bottom)
                {
                    point.Y = bottom + 1;
                }
            }
            return point;
        }

        private DesignerLayoutConnectionPoint GetEventHandlerConnectionPoint(CompositeActivity eventHandler)
        {
            DesignerLayout layout;
            StateDesigner associatedDesigner = (StateDesigner) GetDesigner(eventHandler.Parent);
            if (!associatedDesigner.DesignerLayouts.TryGetValue(eventHandler, out layout))
            {
                return null;
            }
            int connectionIndex = 0;
            foreach (DesignerLayout layout2 in associatedDesigner.DesignerLayouts.Values)
            {
                if (layout2 == layout)
                {
                    break;
                }
                connectionIndex++;
            }
            return new DesignerLayoutConnectionPoint(associatedDesigner, connectionIndex, eventHandler, DesignerEdges.Right);
        }

        internal static System.Drawing.Image GetInitialStateDesignerImage(StateDesigner stateDesigner)
        {
            StateMachineTheme designerTheme = stateDesigner.DesignerTheme as StateMachineTheme;
            if ((designerTheme != null) && (designerTheme.InitialStateDesignerImage != null))
            {
                return designerTheme.InitialStateDesignerImage;
            }
            return InitialState;
        }

        private bool GetIsEditable()
        {
            return true;
        }

        public override object GetNextSelectableObject(object current, DesignerNavigationDirection direction)
        {
            Activity activity = current as Activity;
            if (activity != null)
            {
                this.SetParentTreeEnsuringVisible(false);
                ActivityDesigner item = GetDesigner(activity);
                List<ActivityDesigner> ordered = this.DesignersParser.Ordered;
                int index = ordered.IndexOf(item);
                if (index < 0)
                {
                    return null;
                }
                if (((current is EventDrivenActivity) || (current is StateInitializationActivity)) || (current is StateFinalizationActivity))
                {
                    if ((direction != DesignerNavigationDirection.Left) && (direction != DesignerNavigationDirection.Right))
                    {
                        if (direction == DesignerNavigationDirection.Down)
                        {
                            if (index < (ordered.Count - 1))
                            {
                                return ordered[index + 1].Activity;
                            }
                            return null;
                        }
                        if (index > 0)
                        {
                            return ordered[index - 1].Activity;
                        }
                    }
                    return null;
                }
                StateActivity state = current as StateActivity;
                if (StateMachineHelpers.IsLeafState(state))
                {
                    if (direction != DesignerNavigationDirection.Right)
                    {
                        if (direction != DesignerNavigationDirection.Up)
                        {
                            if ((direction == DesignerNavigationDirection.Down) && (index < (ordered.Count - 1)))
                            {
                                return ordered[index + 1].Activity;
                            }
                        }
                        else if (index > 0)
                        {
                            return ordered[index - 1].Activity;
                        }
                    }
                    else if (this.DesignersParser.StateDesigners.Count > 0)
                    {
                        return this.DesignersParser.StateDesigners[0].Activity;
                    }
                }
                else if ((direction == DesignerNavigationDirection.Left) || (direction == DesignerNavigationDirection.Up))
                {
                    if (index > 0)
                    {
                        return ordered[index - 1].Activity;
                    }
                }
                else if (index < (ordered.Count - 1))
                {
                    return ordered[index + 1].Activity;
                }
            }
            return null;
        }

        private static PropertyDescriptor GetPropertyDescriptor(Activity activity, string propertyName)
        {
            return TypeDescriptor.GetProperties(activity).Find(propertyName, false);
        }

        private static object GetService(ActivityDesigner designer, System.Type serviceType)
        {
            if (designer == null)
            {
                throw new ArgumentNullException("designer");
            }
            if (serviceType == null)
            {
                throw new ArgumentNullException("serviceType");
            }
            Activity activity = designer.Activity;
            object service = null;
            if ((activity != null) && (activity.Site != null))
            {
                service = activity.Site.GetService(serviceType);
            }
            return service;
        }

        private ConnectionPoint GetTargetStateConnectionPoint(StateActivity targetState)
        {
            return new ConnectionPoint((StateDesigner) GetDesigner(targetState), DesignerEdges.Top, 0);
        }

        public override System.Workflow.ComponentModel.Design.HitTestInfo HitTest(Point point)
        {
            System.Workflow.ComponentModel.Design.HitTestInfo info = base.HitTest(point);
            System.Workflow.ComponentModel.Design.HitTestInfo info2 = this.RootDesignerLayout.HitTest(point);
            if (!info2.Equals(System.Workflow.ComponentModel.Design.HitTestInfo.Nowhere))
            {
                return info2;
            }
            return info;
        }

        protected override void Initialize(Activity activity)
        {
            base.Initialize(activity);
            this.EnsureDesignerExtender();
            this._titleBarLayout = new TitleBarLayout(this);
            this._eventDrivenLayout = new EventDrivenLayout(this, this._titleBarLayout);
            this._eventHandlersLayout = new EventHandlersLayout(this);
            this._statesLayout = new StatesLayout(this, this._titleBarLayout, this._eventHandlersLayout);
            this._designerLinkLayout = new DesignerLinkLayout(this);
            this._designerLinkLayout.MouseDown += new MouseEventHandler(this.StateDesignerLinkMouseDown);
            base.AutoSizeMargin = new System.Drawing.Size(0x10, 0x18);
            base.AutoSize = true;
        }

        public override bool IsContainedDesignerVisible(ActivityDesigner containedDesigner)
        {
            if (this.HasActiveDesigner)
            {
                if (containedDesigner == this.ActiveDesigner)
                {
                    return true;
                }
            }
            else if (containedDesigner is StateDesigner)
            {
                return true;
            }
            return false;
        }

        private bool IsValidTargetConnectionPoint(ConnectionPoint target)
        {
            StateDesigner associatedDesigner = target.AssociatedDesigner as StateDesigner;
            if (associatedDesigner == null)
            {
                return false;
            }
            StateActivity state = (StateActivity) associatedDesigner.Activity;
            return StateMachineHelpers.IsLeafState(state);
        }

        internal void OnAddEventDriven(object sender, EventArgs e)
        {
            EventDrivenActivity child = new EventDrivenActivity();
            this.AddChild(child);
        }

        internal void OnAddState(object sender, EventArgs e)
        {
            StateActivity child = new StateActivity();
            this.AddChild(child);
        }

        internal void OnAddStateFinalization(object sender, EventArgs e)
        {
            StateFinalizationActivity child = new StateFinalizationActivity();
            this.AddChild(child);
        }

        internal void OnAddStateInitialization(object sender, EventArgs e)
        {
            StateInitializationActivity child = new StateInitializationActivity();
            this.AddChild(child);
        }

        protected override void OnConnectorAdded(ConnectorEventArgs e)
        {
            base.OnConnectorAdded(e);
            StateDesignerConnector connector = e.Connector as StateDesignerConnector;
            if (connector != null)
            {
                DesignerLayoutConnectionPoint source = connector.Source as DesignerLayoutConnectionPoint;
                ConnectionPoint target = connector.Target;
                if (source == null)
                {
                    ConnectionPoint point2 = connector.Source;
                    connector.Source = connector.Target;
                    connector.Target = point2;
                }
                ConnectionPoint point3 = connector.Target;
                source = (DesignerLayoutConnectionPoint) connector.Source;
                if (this.RootStateDesigner.AddingSetState)
                {
                    SetStateActivity activity = new SetStateActivity {
                        TargetStateName = point3.AssociatedDesigner.Activity.QualifiedName
                    };
                    CompositeActivityDesigner designer = (CompositeActivityDesigner) GetDesigner(source.EventHandler);
                    designer.InsertActivities(new System.Workflow.ComponentModel.Design.HitTestInfo(designer, HitTestLocations.Designer), new List<Activity> { activity }.AsReadOnly());
                    connector.SetStateName = activity.QualifiedName;
                }
                connector.TargetStateName = point3.AssociatedDesigner.Activity.QualifiedName;
                connector.SourceStateName = source.EventHandler.Parent.QualifiedName;
                connector.EventHandlerName = source.EventHandler.QualifiedName;
            }
        }

        protected override void OnConnectorChanged(ConnectorEventArgs e)
        {
            base.OnConnectorChanged(e);
            StateDesignerConnector connector = e.Connector as StateDesignerConnector;
            if (connector != null)
            {
                if (!connector.Target.AssociatedDesigner.Activity.QualifiedName.Equals(connector.TargetStateName))
                {
                    StateActivity activity = (StateActivity) this.RootStateDesigner.Activity;
                    SetStateActivity component = FindActivityByQualifiedName(activity, connector.SetStateName) as SetStateActivity;
                    if (component != null)
                    {
                        StateActivity activity3 = (StateActivity) connector.Target.AssociatedDesigner.Activity;
                        GetPropertyDescriptor(component, "TargetStateName").SetValue(component, activity3.QualifiedName);
                        connector.TargetStateName = activity3.QualifiedName;
                    }
                }
                DesignerLayoutConnectionPoint source = (DesignerLayoutConnectionPoint) connector.Source;
                if (!source.EventHandler.QualifiedName.Equals(connector.EventHandlerName))
                {
                    StateActivity activity4 = (StateActivity) this.RootStateDesigner.Activity;
                    SetStateActivity activity5 = FindActivityByQualifiedName(activity4, connector.SetStateName) as SetStateActivity;
                    if (activity5 != null)
                    {
                        IDesignerHost service = base.GetService(typeof(IDesignerHost)) as IDesignerHost;
                        DesignerTransaction transaction = null;
                        if (service != null)
                        {
                            transaction = service.CreateTransaction(SR.GetMoveSetState());
                        }
                        try
                        {
                            ((CompositeActivityDesigner) GetDesigner(activity5.Parent)).RemoveActivities(new List<Activity> { activity5 }.AsReadOnly());
                            DesignerLayoutConnectionPoint point2 = (DesignerLayoutConnectionPoint) connector.Source;
                            CompositeActivityDesigner designer = (CompositeActivityDesigner) GetDesigner(point2.EventHandler);
                            designer.InsertActivities(new System.Workflow.ComponentModel.Design.HitTestInfo(designer, HitTestLocations.Designer), new List<Activity> { activity5 }.AsReadOnly());
                            connector.EventHandlerName = point2.EventHandler.QualifiedName;
                            connector.SourceStateName = point2.EventHandler.Parent.QualifiedName;
                            if (transaction != null)
                            {
                                transaction.Commit();
                            }
                        }
                        catch
                        {
                            if (transaction != null)
                            {
                                transaction.Cancel();
                            }
                            throw;
                        }
                    }
                }
            }
        }

        protected override void OnConnectorRemoved(ConnectorEventArgs e)
        {
            base.OnConnectorRemoved(e);
            StateDesignerConnector connector = e.Connector as StateDesignerConnector;
            if (((connector != null) && !string.IsNullOrEmpty(connector.SetStateName)) && this.RootStateDesigner.RemovingSetState)
            {
                DesignerLayoutConnectionPoint source = connector.Source as DesignerLayoutConnectionPoint;
                if (((source != null) && (GetDesigner(source.EventHandler) is CompositeActivityDesigner)) && (source.EventHandler != null))
                {
                    Activity activity = FindActivityByQualifiedName(source.EventHandler, connector.SetStateName);
                    if (activity != null)
                    {
                        List<Activity> list = new List<Activity> {
                            activity
                        };
                        CompositeActivityDesigner designer = GetDesigner(activity.Parent) as CompositeActivityDesigner;
                        if (designer != null)
                        {
                            designer.RemoveActivities(list.AsReadOnly());
                        }
                    }
                }
            }
        }

        protected override void OnContainedActivitiesChanged(ActivityCollectionChangeEventArgs listChangeArgs)
        {
            base.OnContainedActivitiesChanged(listChangeArgs);
            if ((this.ActiveDesigner != null) && listChangeArgs.RemovedItems.Contains(this.ActiveDesigner.Activity))
            {
                this.SetActiveDesigner(null);
            }
            this.DesignersParser = null;
        }

        protected override void OnDragDrop(ActivityDragEventArgs e)
        {
            if (this.DragDropActive)
            {
                base.OnDragDrop(e);
                this.DragDropActive = false;
            }
        }

        protected override void OnDragEnter(ActivityDragEventArgs e)
        {
            base.OnDragEnter(e);
            this.DragDropActive = this.CanDrop(e);
            e.Effect = this.CheckDragEffect(e);
            e.DragImageSnapPoint = this.GetDragImageSnapPoint(e);
        }

        protected override void OnDragLeave()
        {
            base.OnDragLeave();
            this.DragDropActive = false;
        }

        protected override void OnDragOver(ActivityDragEventArgs e)
        {
            base.OnDragOver(e);
            this.DragDropActive = this.CanDrop(e);
            e.Effect = this.CheckDragEffect(e);
            e.DragImageSnapPoint = this.GetDragImageSnapPoint(e);
        }

        internal object OnGetPropertyValue(System.Workflow.Activities.ExtendedPropertyInfo extendedProperty, object extendee)
        {
            object obj2 = null;
            if (extendedProperty.Name.Equals("Location", StringComparison.Ordinal))
            {
                return ((this.ActiveDesigner == null) ? this.Location : this._stateLocation);
            }
            if (extendedProperty.Name.Equals("Size", StringComparison.Ordinal))
            {
                obj2 = (this.ActiveDesigner == null) ? this.Size : this._stateSize;
            }
            return obj2;
        }

        protected override void OnLayoutPosition(ActivityDesignerLayoutEventArgs e)
        {
            try
            {
                if (this.IsRootStateDesigner && (this.ActiveDesigner == null))
                {
                    this.UpdateConnectors();
                }
                Graphics graphics = e.Graphics;
                ActivityDesignerTheme designerTheme = e.DesignerTheme;
                AmbientTheme ambientTheme = e.AmbientTheme;
                this.RootDesignerLayout.Location = this.Location;
                this.RootDesignerLayout.OnLayoutPosition(graphics, designerTheme, ambientTheme);
                if (!this.HasActiveDesigner)
                {
                    this.RelocateStates();
                }
                base.OnLayoutPosition(e);
                if (!this.HasActiveDesigner && this.NeedsAutoLayout)
                {
                    this.RepositionStates();
                }
                if (base.IsRootDesigner && (this.InvokingDesigner == null))
                {
                    this.RecalculateRootDesignerSize();
                }
            }
            finally
            {
                if (this.IsRootStateDesigner)
                {
                    this.PerformingLayout = false;
                }
            }
        }

        protected override System.Drawing.Size OnLayoutSize(ActivityDesignerLayoutEventArgs e)
        {
            if (this.IsRootStateDesigner)
            {
                this.PerformingLayout = true;
            }
            if (this.HasActiveDesigner)
            {
                this._minimumSize = System.Drawing.Size.Empty;
                this.Size = System.Drawing.Size.Empty;
            }
            else
            {
                this.NeedsAutoLayout = this.Size.IsEmpty;
            }
            System.Drawing.Size containerSize = base.OnLayoutSize(e);
            Graphics graphics = e.Graphics;
            ActivityDesignerTheme designerTheme = e.DesignerTheme;
            AmbientTheme ambientTheme = e.AmbientTheme;
            this.RefreshRootDesignerLayout();
            this.RootDesignerLayout.OnLayoutSize(graphics, designerTheme, ambientTheme, containerSize);
            this._minimumSize = this.RootDesignerLayout.MinimumSize;
            return this.RootDesignerLayout.Size;
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            this.RootDesignerLayout.OnMouseDoubleClick(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            this.RootDesignerLayout.OnMouseDown(e);
        }

        protected override void OnMouseLeave()
        {
            base.OnMouseLeave();
            this.RootDesignerLayout.OnMouseLeave();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            this.RootDesignerLayout.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            this.RootDesignerLayout.OnMouseUp(e);
        }

        protected override void OnPaint(ActivityDesignerPaintEventArgs e)
        {
            Graphics graphics = e.Graphics;
            ActivityDesignerTheme designerTheme = e.DesignerTheme;
            AmbientTheme ambientTheme = e.AmbientTheme;
            this.RootDesignerLayout.OnPaint(graphics, designerTheme, ambientTheme);
            base.PaintContainedDesigners(e);
        }

        internal void OnSetAsCompletedState(object sender, EventArgs e)
        {
            IDesignerHost service = base.GetService(typeof(IDesignerHost)) as IDesignerHost;
            DesignerTransaction transaction = null;
            StateActivity state = (StateActivity) base.Activity;
            if (service != null)
            {
                transaction = service.CreateTransaction(SR.GetUndoSetAsCompletedState(state.Name));
            }
            try
            {
                StateActivity rootState = StateMachineHelpers.GetRootState(state);
                GetPropertyDescriptor(rootState, "CompletedStateName").SetValue(rootState, state.Name);
                if (StateMachineHelpers.GetInitialStateName(rootState) == state.Name)
                {
                    GetPropertyDescriptor(rootState, "InitialStateName").SetValue(rootState, "");
                }
                if (transaction != null)
                {
                    transaction.Commit();
                }
            }
            catch
            {
                if (transaction != null)
                {
                    transaction.Cancel();
                }
                throw;
            }
        }

        internal void OnSetAsInitialState(object sender, EventArgs e)
        {
            IDesignerHost service = base.GetService(typeof(IDesignerHost)) as IDesignerHost;
            DesignerTransaction transaction = null;
            StateActivity state = (StateActivity) base.Activity;
            if (service != null)
            {
                transaction = service.CreateTransaction(SR.GetUndoSetAsInitialState(state.Name));
            }
            try
            {
                StateActivity rootState = StateMachineHelpers.GetRootState(state);
                GetPropertyDescriptor(rootState, "InitialStateName").SetValue(rootState, state.Name);
                if (StateMachineHelpers.GetCompletedStateName(rootState) == state.Name)
                {
                    GetPropertyDescriptor(rootState, "CompletedStateName").SetValue(rootState, "");
                }
                if (transaction != null)
                {
                    transaction.Commit();
                }
            }
            catch
            {
                if (transaction != null)
                {
                    transaction.Cancel();
                }
                throw;
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal void OnStateMachineView(object sender, EventArgs e)
        {
            this.SetLeafActiveDesigner(this, null);
        }

        internal void OnStatusAddEventDriven(object sender, EventArgs e)
        {
            ActivityDesignerVerb verb = sender as ActivityDesignerVerb;
            if (verb != null)
            {
                StateActivity state = (StateActivity) base.Activity;
                bool flag2 = StateMachineHelpers.IsLeafState(state) && StateMachineHelpers.IsCompletedState(state);
                bool flag = ((this.GetIsEditable() && !this.HasActiveDesigner) && (!flag2 && !base.IsLocked)) && !this.IsStateCustomActivity;
                verb.Visible = flag;
                verb.Enabled = flag;
            }
        }

        internal void OnStatusAddState(object sender, EventArgs e)
        {
            ActivityDesignerVerb verb = sender as ActivityDesignerVerb;
            if (verb != null)
            {
                StateActivity state = (StateActivity) base.Activity;
                bool flag2 = false;
                bool flag3 = false;
                if (StateMachineHelpers.IsLeafState(state))
                {
                    flag2 = StateMachineHelpers.IsInitialState(state);
                    flag3 = StateMachineHelpers.IsCompletedState(state);
                }
                bool flag = (((this.GetIsEditable() && !this.HasActiveDesigner) && (!flag2 && !flag3)) && !base.IsLocked) && !this.IsStateCustomActivity;
                verb.Visible = flag;
                verb.Enabled = flag;
            }
        }

        internal void OnStatusAddStateFinalization(object sender, EventArgs e)
        {
            ActivityDesignerVerb verb = sender as ActivityDesignerVerb;
            if (verb != null)
            {
                StateActivity state = (StateActivity) base.Activity;
                bool flag2 = StateMachineHelpers.IsLeafState(state);
                bool flag3 = flag2 && StateMachineHelpers.IsCompletedState(state);
                bool flag4 = this.DesignersParser.StateFinalizationDesigners.Count > 0;
                bool flag = (((this.GetIsEditable() && !this.HasActiveDesigner) && (flag2 && !flag3)) && (!flag4 && !base.IsLocked)) && !this.IsStateCustomActivity;
                verb.Visible = flag;
                verb.Enabled = flag;
            }
        }

        internal void OnStatusAddStateInitialization(object sender, EventArgs e)
        {
            ActivityDesignerVerb verb = sender as ActivityDesignerVerb;
            if (verb != null)
            {
                StateActivity state = (StateActivity) base.Activity;
                bool flag2 = StateMachineHelpers.IsLeafState(state);
                bool flag3 = flag2 && StateMachineHelpers.IsCompletedState(state);
                bool flag4 = this.DesignersParser.StateInitializationDesigners.Count > 0;
                bool flag = (((this.GetIsEditable() && !this.HasActiveDesigner) && (flag2 && !flag3)) && (!flag4 && !base.IsLocked)) && !this.IsStateCustomActivity;
                verb.Visible = flag;
                verb.Enabled = flag;
            }
        }

        internal void OnStatusSetAsCompletedState(object sender, EventArgs e)
        {
            ActivityDesignerVerb verb = sender as ActivityDesignerVerb;
            if (verb != null)
            {
                bool flag = false;
                if (!this.HasActiveDesigner)
                {
                    StateActivity state = (StateActivity) base.Activity;
                    StateActivity rootState = StateMachineHelpers.GetRootState(state);
                    flag = ((StateMachineHelpers.IsLeafState(state) && StateMachineHelpers.IsStateMachine(rootState)) && !StateMachineHelpers.IsCompletedState(state)) && (state.Activities.Count == 0);
                }
                verb.Visible = flag;
                verb.Enabled = flag;
            }
        }

        internal void OnStatusSetAsInitialState(object sender, EventArgs e)
        {
            ActivityDesignerVerb verb = sender as ActivityDesignerVerb;
            if (verb != null)
            {
                bool flag = false;
                if (!this.HasActiveDesigner)
                {
                    StateActivity state = (StateActivity) base.Activity;
                    StateActivity rootState = StateMachineHelpers.GetRootState(state);
                    flag = (StateMachineHelpers.IsLeafState(state) && StateMachineHelpers.IsStateMachine(rootState)) && !StateMachineHelpers.IsInitialState(state);
                }
                verb.Visible = flag;
                verb.Enabled = flag;
            }
        }

        internal void OnStatusStateMachineView(object sender, EventArgs e)
        {
            ActivityDesignerVerb verb = sender as ActivityDesignerVerb;
            if (verb != null)
            {
                bool hasActiveDesigner = this.HasActiveDesigner;
                verb.Visible = hasActiveDesigner;
                verb.Enabled = hasActiveDesigner;
            }
        }

        protected override void OnThemeChange(ActivityDesignerTheme newTheme)
        {
            base.OnThemeChange(newTheme);
            this.Image = GetDesignerImage(this);
        }

        private void RecalculateRootDesignerSize()
        {
            System.Drawing.Size empty = System.Drawing.Size.Empty;
            foreach (ActivityDesigner designer in this.ContainedDesigners)
            {
                if (this.IsContainedDesignerVisible(designer))
                {
                    Rectangle bounds = designer.Bounds;
                    bounds.Offset(Separator.Width - this.Location.X, Separator.Height - this.Location.Y);
                    empty.Width = Math.Max(empty.Width, bounds.Right);
                    empty.Height = Math.Max(empty.Height, bounds.Bottom);
                }
            }
            empty.Width = Math.Max(empty.Width, this.MinimumSize.Width);
            empty.Height = Math.Max(empty.Height, this.MinimumSize.Height);
            this.Size = empty;
        }

        private void RefreshRootDesignerLayout()
        {
            if (!this.HasActiveDesigner)
            {
                this._eventHandlersLayout.Layouts.Clear();
                this.DesignerLayouts.Clear();
                this._designersParser = new ContainedDesignersParser(this.ContainedDesigners);
                foreach (StateInitializationDesigner designer in this.DesignersParser.StateInitializationDesigners)
                {
                    DesignerLayout item = new DesignerLayout(designer);
                    this.DesignerLayouts[designer.Activity] = item;
                    this._eventHandlersLayout.Layouts.Add(item);
                }
                foreach (EventDrivenDesigner designer2 in this.DesignersParser.EventDrivenDesigners)
                {
                    DesignerLayout layout2 = new DesignerLayout(designer2);
                    this.DesignerLayouts[designer2.Activity] = layout2;
                    this._eventHandlersLayout.Layouts.Add(layout2);
                }
                foreach (StateFinalizationDesigner designer3 in this.DesignersParser.StateFinalizationDesigners)
                {
                    DesignerLayout layout3 = new DesignerLayout(designer3);
                    this.DesignerLayouts[designer3.Activity] = layout3;
                    this._eventHandlersLayout.Layouts.Add(layout3);
                }
                this.RootDesignerLayout = this._statesLayout;
            }
            else
            {
                this.RootDesignerLayout = this._eventDrivenLayout;
            }
        }

        private void RelocateStates()
        {
            int num = this._eventHandlersLayout.Bounds.Bottom + 0x10;
            int num2 = 0;
            Rectangle empty = Rectangle.Empty;
            int num3 = 0x7fffffff;
            foreach (ActivityDesigner designer in this.ContainedDesigners)
            {
                if (this.IsContainedDesignerVisible(designer))
                {
                    StateDesigner designer2 = designer as StateDesigner;
                    if (designer2 != null)
                    {
                        if (designer2.Location.Y < num)
                        {
                            num2 = Math.Max(num2, num - designer2.Location.Y);
                            if (empty.IsEmpty)
                            {
                                empty = designer2.Bounds;
                            }
                            else
                            {
                                empty = Rectangle.Union(empty, designer2.Bounds);
                            }
                        }
                        else
                        {
                            num3 = Math.Min(num3, designer2.Location.Y - num);
                        }
                    }
                }
            }
            if (num3 == 0x7fffffff)
            {
                num3 = 0;
            }
            if (num2 > 0)
            {
                int num4 = -2147483648;
                foreach (ActivityDesigner designer3 in this.ContainedDesigners)
                {
                    if (this.IsContainedDesignerVisible(designer3))
                    {
                        StateDesigner designer4 = designer3 as StateDesigner;
                        if (designer4 != null)
                        {
                            Point location = designer4.Location;
                            if (designer4.Location.Y < num)
                            {
                                designer4.Location = new Point(location.X, location.Y + num2);
                            }
                            else
                            {
                                designer4.Location = new Point(location.X, ((location.Y + empty.Height) + 0x10) - num3);
                            }
                            num4 = Math.Max(num4, designer4.Bounds.Bottom);
                        }
                    }
                }
                if (num4 > base.Bounds.Bottom)
                {
                    System.Drawing.Size size = new System.Drawing.Size(this.Size.Width, this.Size.Height + ((num4 + 0x10) - base.Bounds.Bottom));
                    this.Size = size;
                }
            }
        }

        private void RepositionStates()
        {
            int num = this._eventHandlersLayout.Bounds.Bottom + 0x10;
            foreach (ActivityDesigner designer in this.ContainedDesigners)
            {
                if (this.IsContainedDesignerVisible(designer))
                {
                    StateDesigner designer2 = designer as StateDesigner;
                    if (designer2 != null)
                    {
                        int x = this.Location.X + 0x10;
                        int y = num;
                        designer2.Location = new Point(x, y);
                        num = designer2.Bounds.Bottom + 0x10;
                    }
                }
            }
            this.NeedsAutoLayout = false;
        }

        private void SelectionChanged(object sender, EventArgs e)
        {
            if (this.HasActiveDesigner)
            {
                StateActivity activity = (StateActivity) base.Activity;
                Activity primarySelection = this.SelectionService.PrimarySelection as Activity;
                if (((primarySelection != null) && activity.Activities.Contains(primarySelection)) && (this.ActiveDesigner.Activity != primarySelection))
                {
                    ActivityDesigner designer = GetDesigner(primarySelection);
                    if (!(designer is StateDesigner))
                    {
                        this.SetActiveDesigner(designer);
                    }
                }
            }
            else if (base.Activity == this.SelectionService.PrimarySelection)
            {
                base.RefreshDesignerVerbs();
            }
        }

        private void SetActiveDesigner(ActivityDesigner designer)
        {
            string qualifiedName = null;
            if (designer == null)
            {
                if (!this.HasActiveDesigner)
                {
                    return;
                }
            }
            else
            {
                qualifiedName = designer.Activity.QualifiedName;
                if (this.HasActiveDesigner && (this.ActiveDesigner.Activity.QualifiedName == qualifiedName))
                {
                    return;
                }
            }
            IDesignerHost service = base.GetService(typeof(IDesignerHost)) as IDesignerHost;
            DesignerTransaction transaction = null;
            if (service != null)
            {
                transaction = service.CreateTransaction(SR.GetString("UndoSwitchViews"));
            }
            try
            {
                StateDesigner rootStateDesigner = this.RootStateDesigner;
                this.SetLeafActiveDesigner(rootStateDesigner, null);
                this.SetActiveDesignerHelper(this, designer);
                if (transaction != null)
                {
                    transaction.Commit();
                }
            }
            catch
            {
                if (transaction != null)
                {
                    transaction.Cancel();
                }
                throw;
            }
        }

        private void SetActiveDesignerByName(string activeDesignerName)
        {
            ActivityDesigner designer = null;
            if (!string.IsNullOrEmpty(activeDesignerName))
            {
                foreach (ActivityDesigner designer2 in this.ContainedDesigners)
                {
                    if (designer2.Activity.QualifiedName == activeDesignerName)
                    {
                        designer = designer2;
                        break;
                    }
                }
            }
            this.ActiveDesigner = designer;
        }

        private void SetActiveDesignerHelper(StateDesigner stateDesigner, ActivityDesigner activeDesigner)
        {
            WorkflowDesignerLoader service = base.GetService(typeof(WorkflowDesignerLoader)) as WorkflowDesignerLoader;
            if ((service != null) && service.InDebugMode)
            {
                stateDesigner.ActiveDesigner = activeDesigner;
            }
            else
            {
                PropertyDescriptor propertyDescriptor = GetPropertyDescriptor(stateDesigner.Activity, "ActiveDesignerName");
                if (activeDesigner == null)
                {
                    propertyDescriptor.SetValue(stateDesigner.Activity, null);
                }
                else
                {
                    propertyDescriptor.SetValue(stateDesigner.Activity, activeDesigner.Activity.QualifiedName);
                }
            }
        }

        private void SetLeafActiveDesigner(StateDesigner parentDesigner, ActivityDesigner activityDesigner)
        {
            StateDesigner stateDesigner = parentDesigner;
            while (true)
            {
                StateDesigner activeDesigner = stateDesigner.ActiveDesigner as StateDesigner;
                if (activeDesigner == null)
                {
                    break;
                }
                stateDesigner = activeDesigner;
            }
            this.SetActiveDesignerHelper(stateDesigner, activityDesigner);
        }

        private void SetParentTreeEnsuringVisible(bool value)
        {
            this._ensuringVisible = value;
            for (StateDesigner designer = base.ParentDesigner as StateDesigner; designer != null; designer = designer.ParentDesigner as StateDesigner)
            {
                designer._ensuringVisible = value;
            }
        }

        internal void StateDesignerLinkMouseDown(object sender, MouseEventArgs e)
        {
            IDesignerHost host = base.GetService(typeof(IDesignerHost)) as IDesignerHost;
            DesignerTransaction transaction = null;
            if (host != null)
            {
                transaction = host.CreateTransaction(SR.GetString("UndoSwitchViews"));
            }
            try
            {
                ISelectionService service = base.GetService(typeof(ISelectionService)) as ISelectionService;
                if (service != null)
                {
                    service.SetSelectedComponents(new object[] { base.Activity }, SelectionTypes.Click);
                }
                this.SetLeafActiveDesigner(this, null);
                if (transaction != null)
                {
                    transaction.Commit();
                }
            }
            catch
            {
                if (transaction != null)
                {
                    transaction.Cancel();
                }
                throw;
            }
        }

        private void UpdateConnectors()
        {
            try
            {
                this.RootStateDesigner.RemovingSetState = false;
                StateActivity rootState = (StateActivity) base.Activity;
                ReadOnlyCollection<TransitionInfo> onlys = TransitionInfo.ParseStateMachine(rootState);
                Connector[] array = new Connector[base.Connectors.Count];
                base.Connectors.CopyTo(array, 0);
                foreach (Connector connector in array)
                {
                    StateDesignerConnector stateDesignerConnector = connector as StateDesignerConnector;
                    if (stateDesignerConnector == null)
                    {
                        base.RemoveConnector(connector);
                        continue;
                    }
                    bool flag = false;
                    foreach (TransitionInfo info in onlys)
                    {
                        if (info.Matches(stateDesignerConnector))
                        {
                            info.Connector = stateDesignerConnector;
                            flag = true;
                            break;
                        }
                    }
                    if (!flag)
                    {
                        base.RemoveConnector(connector);
                    }
                }
                foreach (TransitionInfo info2 in onlys)
                {
                    if ((info2.Connector == null) && (info2.TargetState != null))
                    {
                        DesignerLayoutConnectionPoint eventHandlerConnectionPoint = this.GetEventHandlerConnectionPoint(info2.EventHandler);
                        ConnectionPoint targetStateConnectionPoint = this.GetTargetStateConnectionPoint(info2.TargetState);
                        if ((eventHandlerConnectionPoint != null) && (targetStateConnectionPoint != null))
                        {
                            this.RootStateDesigner.AddingSetState = false;
                            try
                            {
                                StateDesignerConnector connector3 = (StateDesignerConnector) base.AddConnector(eventHandlerConnectionPoint, targetStateConnectionPoint);
                                connector3.SetStateName = info2.SetState.QualifiedName;
                                connector3.TargetStateName = info2.SetState.TargetStateName;
                                if (info2.EventHandler != null)
                                {
                                    connector3.EventHandlerName = info2.EventHandler.QualifiedName;
                                }
                            }
                            finally
                            {
                                this.RootStateDesigner.AddingSetState = true;
                            }
                        }
                    }
                }
            }
            finally
            {
                this.RemovingSetState = true;
            }
        }

        internal ActivityDesigner ActiveDesigner
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._activeDesigner;
            }
            set
            {
                if (this._activeDesigner != value)
                {
                    this._activeDesigner = value;
                    base.AutoSize = value == null;
                    base.RefreshDesignerVerbs();
                    if (this.IsRootStateDesigner)
                    {
                        base.PerformLayout();
                    }
                    else
                    {
                        StateDesigner parentDesigner = base.ParentDesigner as StateDesigner;
                        if (value == null)
                        {
                            this.SetActiveDesignerHelper(parentDesigner, null);
                        }
                        else
                        {
                            this.SetActiveDesignerHelper(parentDesigner, this);
                        }
                    }
                    if (this._activeDesigner == null)
                    {
                        this._minimumSize = this._stateMinimumSize;
                        this.Location = this._stateLocation;
                        this.Size = this._stateSize;
                    }
                    else
                    {
                        this._stateLocation = this.Location;
                        this._stateSize = this.Size;
                        this._stateMinimumSize = this._minimumSize;
                        this._minimumSize = System.Drawing.Size.Empty;
                    }
                }
            }
        }

        internal bool AddingSetState
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._addingSetState;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this._addingSetState = value;
            }
        }

        private Point BottomConnectionPoint
        {
            get
            {
                Rectangle bounds = base.Bounds;
                return new Point(bounds.X + (bounds.Width / 2), bounds.Bottom);
            }
        }

        public override bool CanExpandCollapse
        {
            get
            {
                return false;
            }
        }

        internal System.Windows.Forms.Cursor Cursor
        {
            get
            {
                return base.ParentView.Cursor;
            }
            set
            {
                base.ParentView.Cursor = value;
            }
        }

        internal Dictionary<Activity, DesignerLayout> DesignerLayouts
        {
            get
            {
                if (this._designerLayouts == null)
                {
                    this._designerLayouts = new Dictionary<Activity, DesignerLayout>();
                }
                return this._designerLayouts;
            }
        }

        private ContainedDesignersParser DesignersParser
        {
            get
            {
                if (this._designersParser == null)
                {
                    this._designersParser = new ContainedDesignersParser(this.ContainedDesigners);
                }
                return this._designersParser;
            }
            set
            {
                this._designersParser = value;
            }
        }

        private bool DragDropActive
        {
            get
            {
                return this._dragDropActive;
            }
            set
            {
                if (value != this._dragDropActive)
                {
                    this._dragDropActive = value;
                    base.Invalidate();
                }
            }
        }

        internal ReadOnlyCollection<Rectangle> EventHandlersBounds
        {
            get
            {
                List<Rectangle> list = new List<Rectangle>();
                foreach (DesignerLayout layout in this.DesignerLayouts.Values)
                {
                    Rectangle bounds = layout.Bounds;
                    bounds.Inflate(0, 4);
                    list.Add(bounds);
                }
                return list.AsReadOnly();
            }
        }

        protected override Rectangle ExpandButtonRectangle
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return Rectangle.Empty;
            }
        }

        public override object FirstSelectableObject
        {
            get
            {
                if (this.IsVisible)
                {
                    if (this.HasActiveDesigner)
                    {
                        return this.ActiveDesigner.Activity;
                    }
                    if (this.DesignersParser.Ordered.Count > 0)
                    {
                        return this.DesignersParser.Ordered[0].Activity;
                    }
                }
                return null;
            }
        }

        protected override ActivityDesignerGlyphCollection Glyphs
        {
            get
            {
                ActivityDesignerGlyphCollection glyphs = new ActivityDesignerGlyphCollection();
                glyphs.AddRange(base.Glyphs);
                if (!this.HasActiveDesigner)
                {
                    foreach (EventDrivenDesigner designer in this.DesignersParser.EventDrivenDesigners)
                    {
                        Layout layout = this.RootDesignerLayout.GetLayout(designer);
                        if (layout != null)
                        {
                            if (designer.IsSelected)
                            {
                                LayoutSelectionGlyph item = new LayoutSelectionGlyph(layout);
                                glyphs.Add(item);
                            }
                            if (!designer.Activity.Enabled)
                            {
                                CommentLayoutGlyph glyph2 = new CommentLayoutGlyph(layout);
                                glyphs.Add(glyph2);
                            }
                        }
                    }
                    foreach (StateInitializationDesigner designer2 in this.DesignersParser.StateInitializationDesigners)
                    {
                        Layout layout2 = this.RootDesignerLayout.GetLayout(designer2);
                        if (layout2 != null)
                        {
                            if (designer2.IsSelected)
                            {
                                LayoutSelectionGlyph glyph3 = new LayoutSelectionGlyph(layout2);
                                glyphs.Add(glyph3);
                            }
                            if (!designer2.Activity.Enabled)
                            {
                                CommentLayoutGlyph glyph4 = new CommentLayoutGlyph(layout2);
                                glyphs.Add(glyph4);
                            }
                        }
                    }
                    foreach (StateFinalizationDesigner designer3 in this.DesignersParser.StateFinalizationDesigners)
                    {
                        Layout layout3 = this.RootDesignerLayout.GetLayout(designer3);
                        if (layout3 != null)
                        {
                            if (designer3.IsSelected)
                            {
                                LayoutSelectionGlyph glyph5 = new LayoutSelectionGlyph(layout3);
                                glyphs.Add(glyph5);
                            }
                            if (!designer3.Activity.Enabled)
                            {
                                CommentLayoutGlyph glyph6 = new CommentLayoutGlyph(layout3);
                                glyphs.Add(glyph6);
                            }
                        }
                    }
                }
                return glyphs;
            }
        }

        internal bool HasActiveDesigner
        {
            get
            {
                return (this.ActiveDesigner != null);
            }
        }

        internal virtual string HelpText
        {
            get
            {
                if (this._helpText == null)
                {
                    this._helpText = System.Workflow.Activities.DR.GetString("StateHelpText");
                }
                return this._helpText;
            }
        }

        public override System.Drawing.Image Image
        {
            get
            {
                StateActivity state = base.Activity as StateActivity;
                if ((state != null) && StateMachineHelpers.IsLeafState(state))
                {
                    if (StateMachineHelpers.IsInitialState(state))
                    {
                        if (!StateMachineHelpers.IsCompletedState(state))
                        {
                            return GetInitialStateDesignerImage(this);
                        }
                    }
                    else if (StateMachineHelpers.IsCompletedState(state))
                    {
                        return GetCompletedStateDesignerImage(this);
                    }
                }
                return base.Image;
            }
            protected set
            {
                base.Image = value;
            }
        }

        protected override Rectangle ImageRectangle
        {
            get
            {
                if (this.HasActiveDesigner && !this.IsRootStateDesigner)
                {
                    return new Rectangle(-1, -1, 1, 1);
                }
                return this._titleBarLayout.ImageLayout.Bounds;
            }
        }

        private DesignerLinkLayout InlineLayout
        {
            get
            {
                return this._designerLinkLayout;
            }
        }

        internal bool IsRootStateDesigner
        {
            get
            {
                return ((base.Activity.Site != null) && StateMachineHelpers.IsRootState((StateActivity) base.Activity));
            }
        }

        private bool IsStateCustomActivity
        {
            get
            {
                StateActivity state = (StateActivity) base.Activity;
                return (!StateMachineHelpers.IsStateMachine(state) && (state.Parent == null));
            }
        }

        public override object LastSelectableObject
        {
            get
            {
                if (this.IsVisible)
                {
                    if (this.HasActiveDesigner)
                    {
                        return this.ActiveDesigner.Activity;
                    }
                    if (this.DesignersParser.Ordered.Count > 0)
                    {
                        return this.DesignersParser.Ordered[this.DesignersParser.Ordered.Count - 1].Activity;
                    }
                }
                return null;
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
                if (base.Location != value)
                {
                    if ((this.HasActiveDesigner && !this.PerformingLayout) && !this.IsRootStateDesigner)
                    {
                        this._stateLocation = value;
                    }
                    else
                    {
                        if (this.IsRootStateDesigner)
                        {
                            bool performingLayout = this.PerformingLayout;
                            this.PerformingLayout = true;
                            try
                            {
                                base.Location = value;
                            }
                            finally
                            {
                                this.PerformingLayout = performingLayout;
                            }
                        }
                        else
                        {
                            base.Location = value;
                        }
                        this.RootDesignerLayout.MoveLayout(base.Location);
                        base.Invalidate();
                    }
                }
            }
        }

        public override System.Drawing.Size MinimumSize
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._minimumSize;
            }
        }

        private bool NeedsAutoLayout
        {
            get
            {
                return this._needsAutoLayout;
            }
            set
            {
                this._needsAutoLayout = value;
            }
        }

        internal bool PerformingLayout
        {
            get
            {
                return this.RootStateDesigner._performingLayout;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this._performingLayout = value;
            }
        }

        internal bool RemovingSetState
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._removingSetState;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this._removingSetState = value;
            }
        }

        private Layout RootDesignerLayout
        {
            get
            {
                if (this._rootDesignerLayout == null)
                {
                    this.RefreshRootDesignerLayout();
                }
                return this._rootDesignerLayout;
            }
            set
            {
                this._rootDesignerLayout = value;
            }
        }

        internal StateDesigner RootStateDesigner
        {
            get
            {
                if (this._rootStateDesigner == null)
                {
                    StateActivity rootState = StateMachineHelpers.GetRootState((StateActivity) base.Activity);
                    this._rootStateDesigner = GetDesigner(rootState) as StateDesigner;
                }
                return this._rootStateDesigner;
            }
        }

        internal ISelectionService SelectionService
        {
            get
            {
                if (this._selectionService == null)
                {
                    this._selectionService = (ISelectionService) base.GetService(typeof(ISelectionService));
                    this._selectionService.SelectionChanged += new EventHandler(this.SelectionChanged);
                }
                return this._selectionService;
            }
        }

        protected override bool ShowConnectorsInForeground
        {
            get
            {
                return true;
            }
        }

        public override System.Drawing.Size Size
        {
            get
            {
                return base.Size;
            }
            set
            {
                if ((this.HasActiveDesigner && !this.PerformingLayout) && !this.IsRootStateDesigner)
                {
                    this._stateSize = value;
                }
                else
                {
                    if (this.IsRootStateDesigner)
                    {
                        bool performingLayout = this.PerformingLayout;
                        this.PerformingLayout = true;
                        try
                        {
                            base.Size = value;
                        }
                        finally
                        {
                            this.PerformingLayout = performingLayout;
                        }
                    }
                    else
                    {
                        base.Size = value;
                    }
                    System.Drawing.Size newSize = base.Size;
                    this.RootDesignerLayout.ResizeLayout(newSize);
                }
            }
        }

        public override string Text
        {
            get
            {
                string text = base.Text;
                if (string.IsNullOrEmpty(text))
                {
                    text = base.Activity.GetType().Name;
                }
                return text;
            }
        }

        protected override Rectangle TextRectangle
        {
            get
            {
                if (this.HasActiveDesigner && !this.IsRootStateDesigner)
                {
                    return Rectangle.Empty;
                }
                return this._titleBarLayout.TextLayout.Bounds;
            }
        }

        private Point TopConnectionPoint
        {
            get
            {
                Rectangle bounds = base.Bounds;
                return new Point(bounds.X + (bounds.Width / 2), bounds.Top);
            }
        }

        internal virtual ReadOnlyCollection<System.Type> ValidChildTypes
        {
            get
            {
                List<System.Type> list = new List<System.Type> {
                    typeof(StateActivity),
                    typeof(EventDrivenActivity)
                };
                StateActivity state = (StateActivity) base.Activity;
                if (StateMachineHelpers.IsLeafState(state))
                {
                    if (this.DesignersParser.StateInitializationDesigners.Count == 0)
                    {
                        list.Add(typeof(StateInitializationActivity));
                    }
                    if (this.DesignersParser.StateFinalizationDesigners.Count == 0)
                    {
                        list.Add(typeof(StateFinalizationActivity));
                    }
                }
                return list.AsReadOnly();
            }
        }

        protected override ActivityDesignerVerbCollection Verbs
        {
            get
            {
                ActivityDesignerVerbCollection verbs = new ActivityDesignerVerbCollection();
                verbs.AddRange(base.Verbs);
                if (this._verbs == null)
                {
                    this._verbs = new ActivityDesignerVerbCollection();
                    ActivityDesignerVerb verb = new ActivityDesignerVerb(this, DesignerVerbGroup.General, System.Workflow.Activities.DR.GetString("StateMachineView"), new EventHandler(this.OnStateMachineView), new EventHandler(this.OnStatusStateMachineView));
                    this._verbs.Add(verb);
                    ActivityDesignerVerb verb2 = new ActivityDesignerVerb(this, DesignerVerbGroup.General, System.Workflow.Activities.DR.GetString("SetAsInitialState"), new EventHandler(this.OnSetAsInitialState), new EventHandler(this.OnStatusSetAsInitialState));
                    this._verbs.Add(verb2);
                    ActivityDesignerVerb verb3 = new ActivityDesignerVerb(this, DesignerVerbGroup.General, System.Workflow.Activities.DR.GetString("SetAsCompletedState"), new EventHandler(this.OnSetAsCompletedState), new EventHandler(this.OnStatusSetAsCompletedState));
                    this._verbs.Add(verb3);
                    ActivityDesignerVerb verb4 = new ActivityDesignerVerb(this, DesignerVerbGroup.General, System.Workflow.Activities.DR.GetString("AddState"), new EventHandler(this.OnAddState), new EventHandler(this.OnStatusAddState));
                    this._verbs.Add(verb4);
                    ActivityDesignerVerb verb5 = new ActivityDesignerVerb(this, DesignerVerbGroup.General, System.Workflow.Activities.DR.GetString("AddEventDriven"), new EventHandler(this.OnAddEventDriven), new EventHandler(this.OnStatusAddEventDriven));
                    this._verbs.Add(verb5);
                    ActivityDesignerVerb verb6 = new ActivityDesignerVerb(this, DesignerVerbGroup.General, System.Workflow.Activities.DR.GetString("AddStateInitialization"), new EventHandler(this.OnAddStateInitialization), new EventHandler(this.OnStatusAddStateInitialization));
                    this._verbs.Add(verb6);
                    ActivityDesignerVerb verb7 = new ActivityDesignerVerb(this, DesignerVerbGroup.General, System.Workflow.Activities.DR.GetString("AddStateFinalization"), new EventHandler(this.OnAddStateFinalization), new EventHandler(this.OnStatusAddStateFinalization));
                    this._verbs.Add(verb7);
                }
                verbs.AddRange(this._verbs);
                return verbs;
            }
        }

        private class BreadCrumbBarLayout : StateDesigner.Layout
        {
            private Size _breadCrumbSeparatorSize;
            private const string BreadCrumbSeparator = " : ";

            public BreadCrumbBarLayout(ActivityDesigner activityDesigner) : base(activityDesigner)
            {
            }

            private void InitializeLayouts()
            {
                base.Layouts.Clear();
                StateDesigner activityDesigner = (StateDesigner) base.ActivityDesigner;
                for (StateDesigner designer2 = activityDesigner; designer2 != null; designer2 = designer2.ActiveDesigner as StateDesigner)
                {
                    designer2.InlineLayout.ParentStateDesigner = activityDesigner;
                    base.Layouts.Add(designer2.InlineLayout);
                }
            }

            public override void OnLayoutPosition(Graphics graphics, ActivityDesignerTheme designerTheme, AmbientTheme ambientTheme)
            {
                Point location = base.Location;
                foreach (StateDesigner.Layout layout in base.Layouts)
                {
                    layout.Location = location;
                    layout.OnLayoutPosition(graphics, designerTheme, ambientTheme);
                    location.X += layout.Size.Width + this._breadCrumbSeparatorSize.Width;
                }
            }

            public override void OnLayoutSize(Graphics graphics, ActivityDesignerTheme designerTheme, AmbientTheme ambientTheme, Size containerSize)
            {
                base.OnLayoutSize(graphics, designerTheme, ambientTheme, containerSize);
                this.InitializeLayouts();
                CompositeDesignerTheme theme = designerTheme as CompositeDesignerTheme;
                if (theme != null)
                {
                    Font boldFont = designerTheme.BoldFont;
                    this._breadCrumbSeparatorSize = StateMachineDesignerPaint.MeasureString(graphics, boldFont, " : ", StringAlignment.Near, Size.Empty);
                    Size empty = Size.Empty;
                    foreach (StateDesigner.Layout layout in base.Layouts)
                    {
                        layout.OnLayoutSize(graphics, theme, ambientTheme, empty);
                        empty.Width += layout.Size.Width + this._breadCrumbSeparatorSize.Width;
                        empty.Height = Math.Max(empty.Height, layout.Size.Height);
                    }
                    this.MinimumSize = empty;
                    base.Size = empty;
                }
            }

            public override void OnPaint(Graphics graphics, ActivityDesignerTheme designerTheme, AmbientTheme ambientTheme)
            {
                if (base.Layouts.Count != 0)
                {
                    Font boldFont = designerTheme.BoldFont;
                    TextQuality textQuality = ambientTheme.TextQuality;
                    Brush foregroundBrush = designerTheme.ForegroundBrush;
                    List<StateDesigner.Layout> layouts = base.Layouts;
                    for (int i = 0; i < (layouts.Count - 1); i++)
                    {
                        StateDesigner.Layout layout = layouts[i];
                        layout.OnPaint(graphics, designerTheme, ambientTheme);
                        Rectangle boundingRect = new Rectangle(layout.Bounds.Right, layout.Location.Y, this._breadCrumbSeparatorSize.Width, this._breadCrumbSeparatorSize.Height);
                        ActivityDesignerPaint.DrawText(graphics, boldFont, " : ", boundingRect, StringAlignment.Near, textQuality, foregroundBrush);
                    }
                    layouts[layouts.Count - 1].OnPaint(graphics, designerTheme, ambientTheme);
                }
            }
        }

        private class CommentLayoutGlyph : DesignerGlyph
        {
            private StateDesigner.Layout _layout;

            public CommentLayoutGlyph(StateDesigner.Layout layout)
            {
                if (layout == null)
                {
                    throw new ArgumentNullException("layout");
                }
                this._layout = layout;
            }

            public override Rectangle GetBounds(ActivityDesigner designer, bool activated)
            {
                if (designer == null)
                {
                    throw new ArgumentNullException("designer");
                }
                return this._layout.Bounds;
            }

            protected override void OnPaint(Graphics graphics, bool activated, AmbientTheme ambientTheme, ActivityDesigner designer)
            {
                if (designer == null)
                {
                    throw new ArgumentNullException("designer");
                }
                if (graphics == null)
                {
                    throw new ArgumentNullException("graphics");
                }
                Rectangle bounds = this.GetBounds(designer, false);
                graphics.FillRectangle(StateMachineDesignerPaint.FadeBrush, bounds);
                graphics.FillRectangle(ambientTheme.CommentIndicatorBrush, bounds);
                graphics.DrawRectangle(ambientTheme.CommentIndicatorPen, bounds);
            }

            public override int Priority
            {
                get
                {
                    return 0x2710;
                }
            }
        }

        private class ContainedDesignersParser
        {
            private List<EventDrivenDesigner> _eventDrivenDesigners = new List<EventDrivenDesigner>();
            private List<StateDesigner> _leafStateDesigners = new List<StateDesigner>();
            private List<ActivityDesigner> _ordered;
            private List<StateDesigner> _stateDesigners = new List<StateDesigner>();
            private List<StateFinalizationDesigner> _stateFinalizationDesigners = new List<StateFinalizationDesigner>();
            private List<StateInitializationDesigner> _stateInitializationDesigners = new List<StateInitializationDesigner>();

            internal ContainedDesignersParser(ReadOnlyCollection<ActivityDesigner> containedDesigners)
            {
                foreach (ActivityDesigner designer in containedDesigners)
                {
                    StateInitializationDesigner item = designer as StateInitializationDesigner;
                    if (item != null)
                    {
                        this._stateInitializationDesigners.Add(item);
                    }
                    else
                    {
                        StateFinalizationDesigner designer3 = designer as StateFinalizationDesigner;
                        if (designer3 != null)
                        {
                            this._stateFinalizationDesigners.Add(designer3);
                        }
                        else
                        {
                            EventDrivenDesigner designer4 = designer as EventDrivenDesigner;
                            if (designer4 != null)
                            {
                                this._eventDrivenDesigners.Add(designer4);
                            }
                            else
                            {
                                StateDesigner designer5 = designer as StateDesigner;
                                if (designer5 != null)
                                {
                                    if (StateMachineHelpers.IsLeafState((StateActivity) designer.Activity))
                                    {
                                        this._leafStateDesigners.Add(designer5);
                                    }
                                    else
                                    {
                                        this._stateDesigners.Add(designer5);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            public List<EventDrivenDesigner> EventDrivenDesigners
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this._eventDrivenDesigners;
                }
            }

            public List<StateDesigner> LeafStateDesigners
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this._leafStateDesigners;
                }
            }

            public List<ActivityDesigner> Ordered
            {
                get
                {
                    if (this._ordered == null)
                    {
                        this._ordered = new List<ActivityDesigner>();
                        this._ordered.AddRange(this._stateInitializationDesigners.ToArray());
                        this._ordered.AddRange(this._eventDrivenDesigners.ToArray());
                        this._ordered.AddRange(this._stateFinalizationDesigners.ToArray());
                        this._ordered.AddRange(this._leafStateDesigners.ToArray());
                        this._ordered.AddRange(this._stateDesigners.ToArray());
                    }
                    return this._ordered;
                }
            }

            public List<StateDesigner> StateDesigners
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this._stateDesigners;
                }
            }

            public List<StateFinalizationDesigner> StateFinalizationDesigners
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this._stateFinalizationDesigners;
                }
            }

            public List<StateInitializationDesigner> StateInitializationDesigners
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this._stateInitializationDesigners;
                }
            }
        }

        internal class DesignerLayout : StateDesigner.DesignerLayoutBase
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            public DesignerLayout(ActivityDesigner activityDesigner) : base(activityDesigner)
            {
            }

            public override System.Workflow.ComponentModel.Design.HitTestInfo HitTest(Point point)
            {
                System.Workflow.ComponentModel.Design.HitTestInfo nowhere = System.Workflow.ComponentModel.Design.HitTestInfo.Nowhere;
                if (base.Bounds.Contains(point))
                {
                    nowhere = new System.Workflow.ComponentModel.Design.HitTestInfo(base.ActivityDesigner, HitTestLocations.ActionArea | HitTestLocations.Designer);
                }
                return nowhere;
            }

            public virtual Point LeftConnectionPoint
            {
                get
                {
                    Rectangle bounds = base.Bounds;
                    return new Point(bounds.Left - 4, bounds.Y + (bounds.Height / 2));
                }
            }

            public virtual Point RightConnectionPoint
            {
                get
                {
                    Rectangle bounds = base.Bounds;
                    return new Point(bounds.Right + 4, bounds.Y + (bounds.Height / 2));
                }
            }
        }

        internal class DesignerLayoutBase : StateDesigner.Layout
        {
            private Point _imageLocation;
            private Size _imageSize;
            private Point _textLocation;
            private Size _textSize;
            public const int ImagePadding = 4;

            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            public DesignerLayoutBase(ActivityDesigner activityDesigner) : base(activityDesigner)
            {
            }

            public override void MoveLayout(Point newLocation)
            {
                Point point = new Point(base.Location.X - newLocation.X, base.Location.Y - newLocation.Y);
                this._textLocation = new Point(this._textLocation.X - point.X, this._textLocation.Y - point.Y);
                this._imageLocation = new Point(this._imageLocation.X - point.X, this._imageLocation.Y - point.Y);
                base.MoveLayout(newLocation);
            }

            public override void OnLayoutPosition(Graphics graphics, ActivityDesignerTheme designerTheme, AmbientTheme ambientTheme)
            {
                base.OnLayoutPosition(graphics, designerTheme, ambientTheme);
                Point location = base.Location;
                location.X += ambientTheme.Margin.Width;
                location.Y += ambientTheme.Margin.Height / 2;
                this._imageLocation = location;
                location.X += this._imageSize.Width + 4;
                this._textLocation = location;
            }

            public override void OnLayoutSize(Graphics graphics, ActivityDesignerTheme designerTheme, AmbientTheme ambientTheme, Size containerSize)
            {
                base.OnLayoutSize(graphics, designerTheme, ambientTheme, containerSize);
                this._imageSize = designerTheme.ImageSize;
                string text = this.Text;
                Font font = designerTheme.Font;
                this._textSize = StateMachineDesignerPaint.MeasureString(graphics, font, text, StringAlignment.Near, Size.Empty);
                int width = (this._imageSize.Width + 4) + this._textSize.Width;
                width += ambientTheme.Margin.Width * 2;
                int height = Math.Max(this._imageSize.Height, this._textSize.Height) + ambientTheme.Margin.Height;
                Size size = new Size(width, height);
                this.MinimumSize = size;
                base.Size = size;
            }

            public override void OnPaint(Graphics graphics, ActivityDesignerTheme designerTheme, AmbientTheme ambientTheme)
            {
                ActivityDesigner activityDesigner = base.ActivityDesigner;
                Font font = designerTheme.Font;
                Image designerImage = StateDesigner.GetDesignerImage(activityDesigner);
                if (designerImage != null)
                {
                    ActivityDesignerPaint.DrawImage(graphics, designerImage, this.ImageRectangle, DesignerContentAlignment.Fill);
                }
                ActivityDesignerPaint.DrawText(graphics, font, this.Text, this.TextRectangle, StringAlignment.Near, ambientTheme.TextQuality, designerTheme.ForegroundBrush);
            }

            public Rectangle ImageRectangle
            {
                get
                {
                    Rectangle rectangle = new Rectangle(this._imageLocation, this._imageSize);
                    return StateMachineDesignerPaint.TrimRectangle(rectangle, base.Bounds);
                }
            }

            public string Text
            {
                get
                {
                    return base.ActivityDesigner.Activity.Name;
                }
            }

            public Rectangle TextRectangle
            {
                get
                {
                    Rectangle rectangle = new Rectangle(this._textLocation, this._textSize);
                    return StateMachineDesignerPaint.TrimRectangle(rectangle, base.Bounds);
                }
            }
        }

        internal class DesignerLayoutConnectionPoint : ConnectionPoint
        {
            private System.Workflow.ComponentModel.Design.DesignerEdges _designerEdges;
            private CompositeActivity _eventHandler;

            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            public DesignerLayoutConnectionPoint(ActivityDesigner associatedDesigner, int connectionIndex, CompositeActivity eventHandler, System.Workflow.ComponentModel.Design.DesignerEdges designerEdges) : base(associatedDesigner, designerEdges, connectionIndex)
            {
                this._eventHandler = eventHandler;
                this._designerEdges = designerEdges;
            }

            public System.Workflow.ComponentModel.Design.DesignerEdges DesignerEdges
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this._designerEdges;
                }
            }

            public System.Workflow.Activities.StateDesigner.DesignerLayout DesignerLayout
            {
                get
                {
                    System.Workflow.Activities.StateDesigner.DesignerLayout layout;
                    ((StateDesigner) base.AssociatedDesigner).DesignerLayouts.TryGetValue(this._eventHandler, out layout);
                    return layout;
                }
            }

            public CompositeActivity EventHandler
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this._eventHandler;
                }
            }

            public override Point Location
            {
                get
                {
                    System.Workflow.Activities.StateDesigner.DesignerLayout designerLayout = this.DesignerLayout;
                    if (designerLayout == null)
                    {
                        return Point.Empty;
                    }
                    if (this.DesignerEdges == System.Workflow.ComponentModel.Design.DesignerEdges.Left)
                    {
                        return designerLayout.LeftConnectionPoint;
                    }
                    return designerLayout.RightConnectionPoint;
                }
            }
        }

        private class DesignerLinkLayout : StateDesigner.DesignerLayoutBase
        {
            private bool _mouseOver;
            private StateDesigner _parentStateDesigner;
            private Cursor _previousCursor;

            public DesignerLinkLayout(ActivityDesigner activityDesigner) : base(activityDesigner)
            {
            }

            public override void OnMouseEnter()
            {
                base.OnMouseEnter();
                if (this.ParentStateDesigner != null)
                {
                    this._previousCursor = this.ParentStateDesigner.Cursor;
                    this.ParentStateDesigner.Cursor = Cursors.Hand;
                }
                this.MouseOver = true;
            }

            public override void OnMouseLeave()
            {
                base.OnMouseLeave();
                if (this.ParentStateDesigner != null)
                {
                    this.ParentStateDesigner.Cursor = this._previousCursor;
                }
                this.MouseOver = false;
                this.Invalidate();
            }

            public override void OnPaint(Graphics graphics, ActivityDesignerTheme designerTheme, AmbientTheme ambientTheme)
            {
                ActivityDesigner activityDesigner = base.ActivityDesigner;
                if (this.MouseOver)
                {
                    using (Font font = new Font(designerTheme.Font, FontStyle.Underline | designerTheme.Font.Style))
                    {
                        Image designerImage = StateDesigner.GetDesignerImage(activityDesigner);
                        if (designerImage != null)
                        {
                            ActivityDesignerPaint.DrawImage(graphics, designerImage, base.ImageRectangle, DesignerContentAlignment.Fill);
                        }
                        ActivityDesignerPaint.DrawText(graphics, font, base.Text, base.TextRectangle, StringAlignment.Near, ambientTheme.TextQuality, designerTheme.ForegroundBrush);
                        return;
                    }
                }
                base.OnPaint(graphics, designerTheme, ambientTheme);
            }

            public bool MouseOver
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this._mouseOver;
                }
                set
                {
                    if (value != this._mouseOver)
                    {
                        this._mouseOver = value;
                        this.Invalidate();
                    }
                }
            }

            public StateDesigner ParentStateDesigner
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this._parentStateDesigner;
                }
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                set
                {
                    this._parentStateDesigner = value;
                }
            }
        }

        private class EventDrivenLayout : System.Workflow.Activities.StateDesigner.Layout
        {
            private System.Workflow.Activities.StateDesigner.BreadCrumbBarLayout _breadCrumbBarLayout;
            private System.Workflow.Activities.StateDesigner.DesignerLinkLayout _designerLayout;
            private System.Workflow.Activities.StateDesigner.TitleBarLayout _titleBarLayout;
            private const int ActiveDesignerPadding = 0x10;

            public EventDrivenLayout(ActivityDesigner activityDesigner, System.Workflow.Activities.StateDesigner.TitleBarLayout titleBarLayout) : base(activityDesigner)
            {
                this._breadCrumbBarLayout = new System.Workflow.Activities.StateDesigner.BreadCrumbBarLayout(activityDesigner);
                this._designerLayout = new System.Workflow.Activities.StateDesigner.DesignerLinkLayout(activityDesigner);
                System.Workflow.Activities.StateDesigner designer = activityDesigner as System.Workflow.Activities.StateDesigner;
                if (designer != null)
                {
                    this._designerLayout.ParentStateDesigner = designer;
                    this._designerLayout.MouseDown += new MouseEventHandler(designer.StateDesignerLinkMouseDown);
                }
                this._titleBarLayout = titleBarLayout;
            }

            private void InitializeLayout()
            {
                base.Layouts.Clear();
                if (this.StateDesigner.IsRootStateDesigner)
                {
                    base.Layouts.Add(this._titleBarLayout);
                    base.Layouts.Add(this._breadCrumbBarLayout);
                }
                else
                {
                    base.Layouts.Add(this._designerLayout);
                }
            }

            public override void OnLayoutPosition(Graphics graphics, ActivityDesignerTheme designerTheme, AmbientTheme ambientTheme)
            {
                if (designerTheme is CompositeDesignerTheme)
                {
                    Rectangle bounds = base.Bounds;
                    Point location = bounds.Location;
                    if (this.StateDesigner.IsRootStateDesigner)
                    {
                        this._titleBarLayout.Location = location;
                        this._titleBarLayout.OnLayoutPosition(graphics, designerTheme, ambientTheme);
                        location.X += 0x10;
                        location.Y += this._titleBarLayout.Size.Height + 0x10;
                        this._breadCrumbBarLayout.Location = location;
                        this._breadCrumbBarLayout.OnLayoutPosition(graphics, designerTheme, ambientTheme);
                        location.Y += this._breadCrumbBarLayout.Size.Height + 0x10;
                    }
                    else
                    {
                        Point point2 = new Point(bounds.Left + ((bounds.Width - this._designerLayout.Size.Width) / 2), bounds.Top + ambientTheme.SelectionSize.Height);
                        this._designerLayout.Location = point2;
                        this._designerLayout.OnLayoutPosition(graphics, designerTheme, ambientTheme);
                        location.Y = (this._designerLayout.Bounds.Bottom + ambientTheme.SelectionSize.Height) + 0x10;
                    }
                    Size size = this.StateDesigner.ActiveDesigner.Size;
                    location.X = bounds.Left + ((bounds.Width - size.Width) / 2);
                    this.StateDesigner.ActiveDesigner.Location = location;
                }
            }

            public override void OnLayoutSize(Graphics graphics, ActivityDesignerTheme designerTheme, AmbientTheme ambientTheme, Size containerSize)
            {
                base.OnLayoutSize(graphics, designerTheme, ambientTheme, containerSize);
                this.InitializeLayout();
                Size size = containerSize;
                Size minimumSize = this.StateDesigner.MinimumSize;
                size.Width = Math.Max(size.Width, minimumSize.Width);
                size.Height = Math.Max(size.Height, minimumSize.Height);
                ActivityDesigner activeDesigner = this.StateDesigner.ActiveDesigner;
                Size size3 = activeDesigner.Size;
                if (this.StateDesigner.IsRootStateDesigner)
                {
                    this._titleBarLayout.OnLayoutSize(graphics, designerTheme, ambientTheme, size);
                    this._breadCrumbBarLayout.OnLayoutSize(graphics, designerTheme, ambientTheme, size);
                    size.Width = Math.Max(size.Width, size3.Width + 0x20);
                    size.Width = Math.Max(size.Width, this._titleBarLayout.Size.Width);
                    size.Width = Math.Max(size.Width, this._breadCrumbBarLayout.Size.Width);
                    int num = (((size3.Height + this._titleBarLayout.Size.Height) + this._breadCrumbBarLayout.Size.Height) + 0x30) + (ambientTheme.SelectionSize.Height * 2);
                    size.Height = Math.Max(size.Height, num);
                    this._titleBarLayout.ResizeLayout(new Size(size.Width, this._titleBarLayout.Size.Height));
                }
                else
                {
                    this._designerLayout.OnLayoutSize(graphics, designerTheme, ambientTheme, size);
                    size.Width = Math.Max(size.Width, activeDesigner.Size.Width + 0x20);
                    size.Width = Math.Max(size.Width, this._designerLayout.Size.Width);
                    size.Height = Math.Max(size.Height, ((activeDesigner.Size.Height + 0x20) + this._designerLayout.Size.Height) + (ambientTheme.SelectionSize.Height * 2));
                }
                this.MinimumSize = size;
                base.Size = size;
            }

            public override void OnPaint(Graphics graphics, ActivityDesignerTheme designerTheme, AmbientTheme ambientTheme)
            {
                GraphicsPath path = StateMachineDesignerPaint.GetDesignerPath(base.ActivityDesigner, base.Bounds, designerTheme);
                Brush backgroundBrush = designerTheme.GetBackgroundBrush(base.Bounds);
                graphics.FillPath(backgroundBrush, path);
                base.OnPaint(graphics, designerTheme, ambientTheme);
                if (ambientTheme.ShowDesignerBorder)
                {
                    graphics.DrawPath(designerTheme.BorderPen, path);
                }
            }

            public override void ResizeLayout(Size newSize)
            {
                if (this.StateDesigner.IsRootStateDesigner)
                {
                    this._titleBarLayout.ResizeLayout(new Size(newSize.Width, this._titleBarLayout.Size.Height));
                }
                base.ResizeLayout(newSize);
            }

            private System.Workflow.Activities.StateDesigner StateDesigner
            {
                get
                {
                    return (System.Workflow.Activities.StateDesigner) base.ActivityDesigner;
                }
            }
        }

        private class EventHandlersLayout : StateDesigner.Layout
        {
            internal const int EventDrivenPadding = 8;

            public EventHandlersLayout(ActivityDesigner activityDesigner) : base(activityDesigner)
            {
            }

            public override void OnLayoutPosition(Graphics graphics, ActivityDesignerTheme designerTheme, AmbientTheme ambientTheme)
            {
                Size selectionSize = ambientTheme.SelectionSize;
                int x = base.Location.X + 8;
                int y = base.Location.Y + 8;
                foreach (StateDesigner.Layout layout in base.Layouts)
                {
                    layout.Location = new Point(x, y);
                    StateDesigner.DesignerLayoutBase base2 = layout as StateDesigner.DesignerLayoutBase;
                    if (base2 != null)
                    {
                        base2.ActivityDesigner.Location = layout.Location;
                    }
                    layout.OnLayoutPosition(graphics, designerTheme, ambientTheme);
                    y += layout.Size.Height + selectionSize.Height;
                }
            }

            public override void OnLayoutSize(Graphics graphics, ActivityDesignerTheme designerTheme, AmbientTheme ambientTheme, Size containerSize)
            {
                Size selectionSize = ambientTheme.SelectionSize;
                Size size2 = new Size();
                foreach (StateDesigner.Layout layout in base.Layouts)
                {
                    layout.OnLayoutSize(graphics, designerTheme, ambientTheme, size2);
                    size2.Height += layout.Size.Height;
                    size2.Height += selectionSize.Height;
                    int num = layout.Size.Width + (2 * (selectionSize.Width + ambientTheme.Margin.Width));
                    size2.Width = Math.Max(size2.Width, num);
                }
                if (base.Layouts.Count > 0)
                {
                    size2.Height += 8;
                }
                this.MinimumSize = size2;
                Size size3 = new Size {
                    Width = Math.Max(containerSize.Width, size2.Height),
                    Height = size2.Height
                };
                base.Size = size3;
            }

            public override void OnPaint(Graphics graphics, ActivityDesignerTheme designerTheme, AmbientTheme ambientTheme)
            {
                base.OnPaint(graphics, designerTheme, ambientTheme);
                StateDesigner activityDesigner = (StateDesigner) base.ActivityDesigner;
                StateDesigner.ContainedDesignersParser parser = activityDesigner._designersParser;
                if ((((parser.EventDrivenDesigners.Count > 0) || (parser.StateInitializationDesigners.Count > 0)) || (parser.StateFinalizationDesigners.Count > 0)) && ((parser.StateDesigners.Count > 0) || (parser.LeafStateDesigners.Count > 0)))
                {
                    Rectangle bounds = base.Bounds;
                    graphics.DrawLine(designerTheme.BorderPen, bounds.Left, bounds.Bottom, bounds.Right, bounds.Bottom);
                }
            }

            public override void ResizeLayout(Size newSize)
            {
                base.ResizeLayout(newSize);
                int width = newSize.Width - 0x10;
                foreach (StateDesigner.Layout layout in base.Layouts)
                {
                    Size size = layout.Size;
                    if (size.Width > width)
                    {
                        layout.ResizeLayout(new Size(width, size.Height));
                    }
                }
            }
        }

        private class ImageLayout : StateDesigner.Layout
        {
            public ImageLayout(ActivityDesigner activityDesigner) : base(activityDesigner)
            {
            }

            public override void OnLayoutSize(Graphics graphics, ActivityDesignerTheme designerTheme, AmbientTheme ambientTheme, Size containerSize)
            {
                base.OnLayoutSize(graphics, designerTheme, ambientTheme, containerSize);
                CompositeDesignerTheme theme = designerTheme as CompositeDesignerTheme;
                Size empty = Size.Empty;
                if ((base.ActivityDesigner.Image != null) && (theme != null))
                {
                    empty = designerTheme.ImageSize;
                }
                this.MinimumSize = empty;
                base.Size = empty;
            }

            public override void OnPaint(Graphics graphics, ActivityDesignerTheme designerTheme, AmbientTheme ambientTheme)
            {
                Image image = base.ActivityDesigner.Image;
                if (image != null)
                {
                    ActivityDesignerPaint.DrawImage(graphics, image, base.Bounds, DesignerContentAlignment.Fill);
                }
            }
        }

        internal abstract class Layout
        {
            private System.Workflow.ComponentModel.Design.ActivityDesigner _activityDesigner;
            private List<StateDesigner.Layout> _layouts = new List<StateDesigner.Layout>();
            private Point _location;
            private System.Drawing.Size _minimumSize;
            private StateDesigner.Layout _mouseOverLayout;
            private System.Drawing.Size _size;

            public event MouseEventHandler MouseDoubleClick;

            public event MouseEventHandler MouseDown;

            public event EventHandler MouseEnter;

            public event EventHandler MouseLeave;

            public event MouseEventHandler MouseMove;

            public event MouseEventHandler MouseUp;

            public Layout(System.Workflow.ComponentModel.Design.ActivityDesigner activityDesigner)
            {
                this._activityDesigner = activityDesigner;
            }

            public StateDesigner.Layout GetLayout(System.Workflow.ComponentModel.Design.ActivityDesigner designer)
            {
                if (this.ActivityDesigner == designer)
                {
                    return this;
                }
                foreach (StateDesigner.Layout layout in this.Layouts)
                {
                    StateDesigner.Layout layout2 = layout.GetLayout(designer);
                    if (layout2 != null)
                    {
                        return layout2;
                    }
                }
                return null;
            }

            private StateDesigner.Layout GetLayoutAt(int x, int y)
            {
                foreach (StateDesigner.Layout layout in this._layouts)
                {
                    if (layout.Bounds.Contains(x, y))
                    {
                        return layout;
                    }
                }
                if (this.Bounds.Contains(x, y))
                {
                    return this;
                }
                return null;
            }

            public virtual System.Workflow.ComponentModel.Design.HitTestInfo HitTest(Point point)
            {
                System.Workflow.ComponentModel.Design.HitTestInfo nowhere = System.Workflow.ComponentModel.Design.HitTestInfo.Nowhere;
                if (this.Bounds.Contains(point))
                {
                    StateDesigner.Layout layoutAt = this.GetLayoutAt(point.X, point.Y);
                    if (layoutAt != this)
                    {
                        nowhere = layoutAt.HitTest(point);
                    }
                }
                return nowhere;
            }

            public virtual void Invalidate()
            {
                WorkflowView service = StateDesigner.GetService(this._activityDesigner, typeof(WorkflowView)) as WorkflowView;
                if (service != null)
                {
                    service.InvalidateLogicalRectangle(this.Bounds);
                }
            }

            public virtual void MoveLayout(Point newLocation)
            {
                if (newLocation != this._location)
                {
                    Point point = new Point(this._location.X - newLocation.X, this._location.Y - newLocation.Y);
                    foreach (StateDesigner.Layout layout in this._layouts)
                    {
                        Point location = layout.Location;
                        Point point3 = new Point(location.X - point.X, location.Y - point.Y);
                        layout.MoveLayout(point3);
                    }
                    this._location = newLocation;
                }
            }

            public virtual void OnLayoutPosition(Graphics graphics, ActivityDesignerTheme designerTheme, AmbientTheme ambientTheme)
            {
            }

            public virtual void OnLayoutSize(Graphics graphics, ActivityDesignerTheme designerTheme, AmbientTheme ambientTheme, System.Drawing.Size containerSize)
            {
            }

            public virtual void OnMouseDoubleClick(MouseEventArgs e)
            {
                if (this.MouseOverLayout == this)
                {
                    if (this.MouseDoubleClick != null)
                    {
                        this.MouseDoubleClick(this, e);
                    }
                }
                else if (this.MouseOverLayout != null)
                {
                    this.MouseOverLayout.OnMouseDoubleClick(e);
                }
                StateDesigner.Layout layoutAt = this.GetLayoutAt(e.X, e.Y);
                this.MouseOverLayout = layoutAt;
            }

            public virtual void OnMouseDown(MouseEventArgs e)
            {
                if (this.MouseOverLayout == this)
                {
                    if (this.MouseDown != null)
                    {
                        this.MouseDown(this, e);
                    }
                }
                else if (this.MouseOverLayout != null)
                {
                    this.MouseOverLayout.OnMouseDown(e);
                }
            }

            public virtual void OnMouseEnter()
            {
                if (this.MouseEnter != null)
                {
                    this.MouseEnter(this, EventArgs.Empty);
                }
            }

            public virtual void OnMouseLeave()
            {
                if (this.MouseOverLayout != this)
                {
                    this.MouseOverLayout = null;
                }
                if (this.MouseLeave != null)
                {
                    this.MouseLeave(this, EventArgs.Empty);
                }
            }

            public virtual void OnMouseMove(MouseEventArgs e)
            {
                StateDesigner.Layout layoutAt = this.GetLayoutAt(e.X, e.Y);
                if (layoutAt != this.MouseOverLayout)
                {
                    this.MouseOverLayout = layoutAt;
                }
                if (this.MouseOverLayout == this)
                {
                    if (this.MouseMove != null)
                    {
                        this.MouseMove(this, e);
                    }
                }
                else if (this.MouseOverLayout != null)
                {
                    this.MouseOverLayout.OnMouseMove(e);
                }
            }

            public virtual void OnMouseUp(MouseEventArgs e)
            {
                if (this.MouseOverLayout == this)
                {
                    if (this.MouseUp != null)
                    {
                        this.MouseUp(this, e);
                    }
                }
                else if (this.MouseOverLayout != null)
                {
                    this.MouseOverLayout.OnMouseUp(e);
                }
                StateDesigner.Layout layoutAt = this.GetLayoutAt(e.X, e.Y);
                this.MouseOverLayout = layoutAt;
            }

            public virtual void OnPaint(Graphics graphics, ActivityDesignerTheme designerTheme, AmbientTheme ambientTheme)
            {
                foreach (StateDesigner.Layout layout in this._layouts)
                {
                    layout.OnPaint(graphics, designerTheme, ambientTheme);
                }
            }

            public virtual void ResizeLayout(System.Drawing.Size newSize)
            {
                this._size = newSize;
            }

            public System.Workflow.ComponentModel.Design.ActivityDesigner ActivityDesigner
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this._activityDesigner;
                }
            }

            public Rectangle Bounds
            {
                get
                {
                    return new Rectangle(this._location, this._size);
                }
            }

            public List<StateDesigner.Layout> Layouts
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this._layouts;
                }
            }

            public Point Location
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this._location;
                }
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                set
                {
                    this._location = value;
                }
            }

            public virtual System.Drawing.Size MinimumSize
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this._minimumSize;
                }
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                set
                {
                    this._minimumSize = value;
                }
            }

            public StateDesigner.Layout MouseOverLayout
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this._mouseOverLayout;
                }
                set
                {
                    if (value != this._mouseOverLayout)
                    {
                        if (this._mouseOverLayout != null)
                        {
                            this._mouseOverLayout.OnMouseLeave();
                        }
                        this._mouseOverLayout = value;
                        if (value != null)
                        {
                            value.OnMouseEnter();
                        }
                    }
                }
            }

            public System.Drawing.Size Size
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this._size;
                }
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                set
                {
                    this._size = value;
                }
            }
        }

        private class LayoutSelectionGlyph : SelectionGlyph
        {
            private StateDesigner.Layout _layout;

            public LayoutSelectionGlyph(StateDesigner.Layout layout)
            {
                if (layout == null)
                {
                    throw new ArgumentNullException("layout");
                }
                this._layout = layout;
            }

            public override Rectangle GetBounds(ActivityDesigner designer, bool activated)
            {
                if (designer == null)
                {
                    throw new ArgumentNullException("designer");
                }
                return this._layout.Bounds;
            }

            public override bool IsPrimarySelection
            {
                get
                {
                    return true;
                }
            }

            public override int Priority
            {
                get
                {
                    return 0;
                }
            }
        }

        [ProvideProperty("ActiveDesignerName", typeof(Activity))]
        private sealed class StateDesignerPropertyExtender : IExtenderProvider
        {
            [DesignOnly(true), MergableProperty(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
            public string GetActiveDesignerName(Activity activity)
            {
                string qualifiedName = null;
                StateDesigner designer = (StateDesigner) StateDesigner.GetDesigner(activity);
                if (designer != null)
                {
                    ActivityDesigner activeDesigner = designer.ActiveDesigner;
                    if (activeDesigner != null)
                    {
                        qualifiedName = activeDesigner.Activity.QualifiedName;
                    }
                }
                return qualifiedName;
            }

            public void SetActiveDesignerName(Activity activity, string activeDesignerName)
            {
                StateDesigner designer = (StateDesigner) StateDesigner.GetDesigner(activity);
                if (designer != null)
                {
                    designer.SetActiveDesignerByName(activeDesignerName);
                }
            }

            bool IExtenderProvider.CanExtend(object extendee)
            {
                bool flag = false;
                StateActivity activity = extendee as StateActivity;
                if ((activity != null) && (StateDesigner.GetDesigner(activity) is StateDesigner))
                {
                    flag = true;
                }
                return flag;
            }
        }

        private class StatesLayout : System.Workflow.Activities.StateDesigner.Layout
        {
            private System.Workflow.Activities.StateDesigner.EventHandlersLayout _eventHandlersLayout;
            private System.Workflow.Activities.StateDesigner.TitleBarLayout _titleBarLayout;
            private static readonly Size RealMinimumSize = new Size(160, 80);
            private const int StatePadding = 0x10;

            public StatesLayout(ActivityDesigner activityDesigner, System.Workflow.Activities.StateDesigner.TitleBarLayout titleBarLayout, System.Workflow.Activities.StateDesigner.EventHandlersLayout eventHandlersLayout) : base(activityDesigner)
            {
                this._titleBarLayout = titleBarLayout;
                base.Layouts.Add(titleBarLayout);
                this._eventHandlersLayout = eventHandlersLayout;
                base.Layouts.Add(eventHandlersLayout);
            }

            public override void OnLayoutPosition(Graphics graphics, ActivityDesignerTheme designerTheme, AmbientTheme ambientTheme)
            {
                this._titleBarLayout.Location = base.Location;
                this._titleBarLayout.OnLayoutPosition(graphics, designerTheme, ambientTheme);
                int x = base.Location.X;
                int y = this._titleBarLayout.Bounds.Bottom + 1;
                this._eventHandlersLayout.Location = new Point(x, y);
                this._eventHandlersLayout.OnLayoutPosition(graphics, designerTheme, ambientTheme);
            }

            public override void OnLayoutSize(Graphics graphics, ActivityDesignerTheme designerTheme, AmbientTheme ambientTheme, Size containerSize)
            {
                base.OnLayoutSize(graphics, designerTheme, ambientTheme, containerSize);
                CompositeDesignerTheme theme = (CompositeDesignerTheme) designerTheme;
                Size size = containerSize;
                this._titleBarLayout.OnLayoutSize(graphics, theme, ambientTheme, size);
                this._eventHandlersLayout.OnLayoutSize(graphics, designerTheme, ambientTheme, size);
                int width = Math.Max(this._titleBarLayout.MinimumSize.Width, this._eventHandlersLayout.MinimumSize.Width);
                int height = this._titleBarLayout.MinimumSize.Height + this._eventHandlersLayout.MinimumSize.Height;
                this.MinimumSize = new Size(width, height);
                size.Width = Math.Max(width, size.Width);
                size.Height = Math.Max(height, size.Height);
                if (this.StateDesigner.NeedsAutoLayout)
                {
                    int num3 = size.Width;
                    int num4 = (this._titleBarLayout.Size.Height + this._eventHandlersLayout.Size.Height) + 0x10;
                    bool flag = false;
                    foreach (ActivityDesigner designer in this.StateDesigner.ContainedDesigners)
                    {
                        if (this.StateDesigner.IsContainedDesignerVisible(designer) && (designer is System.Workflow.Activities.StateDesigner))
                        {
                            flag = true;
                            num3 = Math.Max(num3, designer.Size.Width);
                            num4 += designer.Size.Height + 0x10;
                        }
                    }
                    if (flag)
                    {
                        num4 += 0x20;
                    }
                    size = new Size(num3, num4);
                }
                this._titleBarLayout.ResizeLayout(new Size(size.Width, this._titleBarLayout.Size.Height));
                base.Size = size;
            }

            public override void OnPaint(Graphics graphics, ActivityDesignerTheme designerTheme, AmbientTheme ambientTheme)
            {
                GraphicsPath path = StateMachineDesignerPaint.GetDesignerPath(base.ActivityDesigner, base.Bounds, designerTheme);
                Brush backgroundBrush = designerTheme.GetBackgroundBrush(base.Bounds);
                graphics.FillPath(backgroundBrush, path);
                base.OnPaint(graphics, designerTheme, ambientTheme);
                if (ambientTheme.ShowDesignerBorder)
                {
                    graphics.DrawPath(designerTheme.BorderPen, path);
                }
                if ((this.StateDesigner.ContainedDesigners.Count == 0) && !this.StateDesigner.IsStateCustomActivity)
                {
                    Point location = new Point(base.Location.X, this._titleBarLayout.Bounds.Bottom);
                    Size size = new Size(base.Size.Width, base.Size.Height - this._titleBarLayout.Bounds.Height);
                    Rectangle boundingRect = new Rectangle(location, size);
                    boundingRect.Inflate(-1, -1);
                    StateActivity state = (StateActivity) this.StateDesigner.Activity;
                    if (!StateMachineHelpers.IsLeafState(state) || !StateMachineHelpers.IsCompletedState(state))
                    {
                        if (this.StateDesigner.DragDropActive)
                        {
                            using (SolidBrush brush2 = new SolidBrush(Color.FromArgb(0x40, designerTheme.ForeColor)))
                            {
                                ActivityDesignerPaint.DrawText(graphics, designerTheme.Font, this.StateDesigner.HelpText, boundingRect, StringAlignment.Center, ambientTheme.TextQuality, brush2);
                                return;
                            }
                        }
                        ActivityDesignerPaint.DrawText(graphics, designerTheme.Font, this.StateDesigner.HelpText, boundingRect, StringAlignment.Center, ambientTheme.TextQuality, designerTheme.ForegroundBrush);
                    }
                }
            }

            public override void ResizeLayout(Size newSize)
            {
                this._eventHandlersLayout.ResizeLayout(new Size(newSize.Width, this._eventHandlersLayout.Size.Height));
                this._titleBarLayout.ResizeLayout(new Size(newSize.Width, this._titleBarLayout.Size.Height));
                base.ResizeLayout(newSize);
            }

            public System.Workflow.Activities.StateDesigner.EventHandlersLayout EventHandlersLayout
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this._eventHandlersLayout;
                }
            }

            public override Size MinimumSize
            {
                get
                {
                    Size minimumSize = base.MinimumSize;
                    minimumSize.Width = Math.Max(minimumSize.Width, RealMinimumSize.Width);
                    minimumSize.Height = Math.Max(minimumSize.Height, RealMinimumSize.Height);
                    return minimumSize;
                }
            }

            private System.Workflow.Activities.StateDesigner StateDesigner
            {
                get
                {
                    return (System.Workflow.Activities.StateDesigner) base.ActivityDesigner;
                }
            }
        }

        private class TextLayout : StateDesigner.Layout
        {
            public TextLayout(ActivityDesigner activityDesigner) : base(activityDesigner)
            {
            }

            public override void OnLayoutSize(Graphics graphics, ActivityDesignerTheme designerTheme, AmbientTheme ambientTheme, Size containerSize)
            {
                base.OnLayoutSize(graphics, designerTheme, ambientTheme, containerSize);
                CompositeDesignerTheme theme = designerTheme as CompositeDesignerTheme;
                string text = base.ActivityDesigner.Text;
                Size empty = Size.Empty;
                if ((theme != null) && !string.IsNullOrEmpty(text))
                {
                    empty = StateMachineDesignerPaint.MeasureString(graphics, theme.Font, text, StringAlignment.Center, Size.Empty);
                }
                this.MinimumSize = empty;
                base.Size = empty;
            }

            public override void OnPaint(Graphics graphics, ActivityDesignerTheme designerTheme, AmbientTheme ambientTheme)
            {
                string text = base.ActivityDesigner.Text;
                if (!string.IsNullOrEmpty(text))
                {
                    Font font = designerTheme.Font;
                    ActivityDesignerPaint.DrawText(graphics, font, text, base.Bounds, StringAlignment.Near, ambientTheme.TextQuality, designerTheme.ForegroundBrush);
                }
            }
        }

        private class TitleBarLayout : StateDesigner.Layout
        {
            private System.Workflow.Activities.StateDesigner.ImageLayout _imageLayout;
            private System.Workflow.Activities.StateDesigner.TextLayout _textLayout;
            private const int Padding = 4;

            public TitleBarLayout(ActivityDesigner activityDesigner) : base(activityDesigner)
            {
                this._textLayout = new System.Workflow.Activities.StateDesigner.TextLayout(activityDesigner);
                base.Layouts.Add(this._textLayout);
                this._imageLayout = new System.Workflow.Activities.StateDesigner.ImageLayout(activityDesigner);
                base.Layouts.Add(this._imageLayout);
            }

            private void CalculateTextLayout()
            {
                Size margin = WorkflowTheme.CurrentTheme.AmbientTheme.Margin;
                int num = this._imageLayout.Bounds.Right + 4;
                int width = this._textLayout.Size.Width;
                int x = (base.Location.X + (base.Size.Width / 2)) - (width / 2);
                if (x < num)
                {
                    x = num;
                }
                if ((x + width) > (base.Bounds.Right - margin.Width))
                {
                    width = (base.Bounds.Right - margin.Width) - x;
                }
                this._textLayout.Location = new Point(x, base.Location.Y + margin.Height);
                this._textLayout.Size = new Size(width, this._textLayout.Size.Height);
            }

            public override void OnLayoutPosition(Graphics graphics, ActivityDesignerTheme designerTheme, AmbientTheme ambientTheme)
            {
                Size margin = WorkflowTheme.CurrentTheme.AmbientTheme.Margin;
                Point location = base.Location;
                location.X += margin.Width;
                location.Y += 2;
                this._imageLayout.Location = location;
                this.CalculateTextLayout();
            }

            public override void OnLayoutSize(Graphics graphics, ActivityDesignerTheme designerTheme, AmbientTheme ambientTheme, Size containerSize)
            {
                base.OnLayoutSize(graphics, designerTheme, ambientTheme, containerSize);
                Size size = new Size();
                this._textLayout.OnLayoutSize(graphics, designerTheme, ambientTheme, Size.Empty);
                this._imageLayout.OnLayoutSize(graphics, designerTheme, ambientTheme, Size.Empty);
                size.Width = (((designerTheme.BorderWidth * 2) + 10) + this._textLayout.Size.Width) + this._imageLayout.Size.Width;
                size.Height = Math.Max(this._textLayout.Size.Height, this._imageLayout.Size.Height);
                size.Height += (designerTheme.BorderWidth * 2) + 4;
                this.MinimumSize = size;
                Size size2 = size;
                size2.Width = Math.Max(size.Width, containerSize.Width);
                base.Size = size2;
            }

            public override void OnPaint(Graphics graphics, ActivityDesignerTheme designerTheme, AmbientTheme ambientTheme)
            {
                Color empty;
                Color color2;
                Rectangle bounds = base.Bounds;
                Brush backgroundBrush = designerTheme.GetBackgroundBrush(base.Bounds);
                graphics.FillRectangle(backgroundBrush, bounds);
                StateActivity state = (StateActivity) base.ActivityDesigner.Activity;
                if (StateMachineHelpers.IsLeafState(state))
                {
                    empty = Color.FromArgb(0x20, designerTheme.BorderColor);
                    color2 = Color.FromArgb(160, designerTheme.BorderColor);
                }
                else if (StateMachineHelpers.IsRootState(state))
                {
                    empty = Color.Empty;
                    color2 = Color.FromArgb(0x80, designerTheme.BorderColor);
                }
                else
                {
                    empty = Color.FromArgb(0x10, designerTheme.BorderColor);
                    color2 = Color.FromArgb(0x10, designerTheme.BorderColor);
                }
                if ((bounds.Width > 0) && (bounds.Height > 0))
                {
                    using (Brush brush2 = new LinearGradientBrush(bounds, empty, color2, LinearGradientMode.Vertical))
                    {
                        graphics.FillRectangle(brush2, bounds);
                        graphics.DrawLine(designerTheme.BorderPen, bounds.Left, bounds.Bottom, bounds.Right, bounds.Bottom);
                    }
                }
                base.OnPaint(graphics, designerTheme, ambientTheme);
            }

            public override void ResizeLayout(Size newSize)
            {
                base.ResizeLayout(newSize);
                this.CalculateTextLayout();
            }

            public System.Workflow.Activities.StateDesigner.ImageLayout ImageLayout
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this._imageLayout;
                }
            }

            public System.Workflow.Activities.StateDesigner.TextLayout TextLayout
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this._textLayout;
                }
            }
        }

        internal class TransitionInfo
        {
            private StateDesignerConnector _connector;
            private CompositeActivity _eventHandler;
            private SetStateActivity _setState;
            private StateActivity _targetState;

            internal TransitionInfo(SetStateActivity setState, CompositeActivity eventHandler)
            {
                if (setState == null)
                {
                    throw new ArgumentNullException("setState");
                }
                if (eventHandler == null)
                {
                    throw new ArgumentNullException("eventHandler");
                }
                this._setState = setState;
                this._eventHandler = eventHandler;
            }

            internal bool Matches(StateDesignerConnector stateDesignerConnector)
            {
                if (stateDesignerConnector == null)
                {
                    throw new ArgumentNullException("stateDesignerConnector");
                }
                if ((this.Connector == null) || (this.Connector != stateDesignerConnector))
                {
                    if (((this.SetState == null) || (this.SourceState == null)) || ((this.TargetState == null) || (this.EventHandler == null)))
                    {
                        return false;
                    }
                    if (this.SetState.QualifiedName != stateDesignerConnector.SetStateName)
                    {
                        return false;
                    }
                    if (this.SourceState.QualifiedName != stateDesignerConnector.SourceStateName)
                    {
                        return false;
                    }
                    if ((this.TargetState.QualifiedName != stateDesignerConnector.TargetStateName) || (stateDesignerConnector.Target.AssociatedDesigner.Activity.QualifiedName != stateDesignerConnector.TargetStateName))
                    {
                        return false;
                    }
                    if (this.EventHandler.QualifiedName != stateDesignerConnector.EventHandlerName)
                    {
                        return false;
                    }
                }
                return true;
            }

            private static void ParseEventHandler(CompositeActivity eventHandler, List<StateDesigner.TransitionInfo> transitions)
            {
                Queue<Activity> queue = new Queue<Activity>();
                queue.Enqueue(eventHandler);
                while (queue.Count > 0)
                {
                    Activity activity = queue.Dequeue();
                    SetStateActivity setState = activity as SetStateActivity;
                    if (setState != null)
                    {
                        StateDesigner.TransitionInfo item = new StateDesigner.TransitionInfo(setState, eventHandler);
                        transitions.Add(item);
                    }
                    else
                    {
                        CompositeActivity activity3 = activity as CompositeActivity;
                        if (activity3 != null)
                        {
                            foreach (Activity activity4 in activity3.Activities)
                            {
                                queue.Enqueue(activity4);
                            }
                            continue;
                        }
                    }
                }
            }

            internal static ReadOnlyCollection<StateDesigner.TransitionInfo> ParseStateMachine(StateActivity rootState)
            {
                List<StateDesigner.TransitionInfo> transitions = new List<StateDesigner.TransitionInfo>();
                Dictionary<string, StateActivity> dictionary = new Dictionary<string, StateActivity>();
                Queue<StateActivity> queue = new Queue<StateActivity>();
                queue.Enqueue(rootState);
                while (queue.Count > 0)
                {
                    StateActivity activity = queue.Dequeue();
                    dictionary[activity.QualifiedName] = activity;
                    foreach (Activity activity2 in activity.Activities)
                    {
                        StateActivity item = activity2 as StateActivity;
                        if (item == null)
                        {
                            CompositeActivity eventHandler = activity2 as CompositeActivity;
                            if (eventHandler != null)
                            {
                                ParseEventHandler(eventHandler, transitions);
                            }
                        }
                        else
                        {
                            queue.Enqueue(item);
                        }
                    }
                }
                foreach (StateDesigner.TransitionInfo info in transitions)
                {
                    if (!string.IsNullOrEmpty(info.SetState.TargetStateName))
                    {
                        StateActivity activity5;
                        dictionary.TryGetValue(info.SetState.TargetStateName, out activity5);
                        info.TargetState = activity5;
                    }
                }
                return transitions.AsReadOnly();
            }

            internal StateDesignerConnector Connector
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this._connector;
                }
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                set
                {
                    this._connector = value;
                }
            }

            internal CompositeActivity EventHandler
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this._eventHandler;
                }
            }

            internal SetStateActivity SetState
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this._setState;
                }
            }

            internal StateActivity SourceState
            {
                get
                {
                    if (this._eventHandler == null)
                    {
                        return null;
                    }
                    return (this._eventHandler.Parent as StateActivity);
                }
            }

            internal StateActivity TargetState
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this._targetState;
                }
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                set
                {
                    this._targetState = value;
                }
            }
        }
    }
}

