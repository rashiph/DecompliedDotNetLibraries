namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.IO;
    using System.Runtime;
    using System.Windows.Forms;
    using System.Workflow.ComponentModel;

    public abstract class StructuredCompositeActivityDesigner : CompositeActivityDesigner
    {
        private DesignerView activeView;
        private int currentDropTarget = -1;
        private ItemPalette itemPalette;
        private List<DesignerView> views;

        protected StructuredCompositeActivityDesigner()
        {
        }

        private int CanDrop(ActivityDragEventArgs e)
        {
            if (e.Activities.Count == 0)
            {
                return -1;
            }
            Point dropPoint = new Point(e.X, e.Y);
            int connector = -1;
            Rectangle[] dropTargets = this.GetDropTargets(dropPoint);
            for (int i = 0; i < dropTargets.Length; i++)
            {
                if (dropTargets[i].Contains(dropPoint))
                {
                    connector = i;
                    break;
                }
            }
            if ((connector >= 0) && !this.CanInsertActivities(new ConnectorHitTestInfo(this, HitTestLocations.Designer, connector), e.Activities))
            {
                connector = -1;
            }
            bool flag = (e.KeyState & 8) == 8;
            if (((connector >= 0) && !flag) && ((e.AllowedEffect & DragDropEffects.Move) == DragDropEffects.Move))
            {
                ConnectorHitTestInfo moveLocation = new ConnectorHitTestInfo(this, HitTestLocations.Designer, connector);
                foreach (Activity activity in e.Activities)
                {
                    if (activity.Site != null)
                    {
                        ActivityDesigner designer = ActivityDesigner.GetDesigner(activity);
                        if (((designer == null) || (designer.ParentDesigner == null)) || !designer.ParentDesigner.CanMoveActivities(moveLocation, new List<Activity>(new Activity[] { activity }).AsReadOnly()))
                        {
                            return -1;
                        }
                    }
                }
            }
            return connector;
        }

        public override bool CanInsertActivities(System.Workflow.ComponentModel.Design.HitTestInfo insertLocation, ReadOnlyCollection<Activity> activitiesToInsert)
        {
            if (insertLocation == null)
            {
                throw new ArgumentNullException("insertLocation");
            }
            if (activitiesToInsert == null)
            {
                throw new ArgumentNullException("activitiesToInsert");
            }
            ActivityDesigner designer = (this.ActiveView != null) ? this.ActiveView.AssociatedDesigner : null;
            if (designer != this)
            {
                return false;
            }
            IList<System.Type> activityTypes = SecondaryViewProvider.GetActivityTypes(this);
            foreach (Activity activity in activitiesToInsert)
            {
                if (activity == null)
                {
                    throw new ArgumentException("activitiesToInsert", SR.GetString("Error_CollectionHasNullEntry"));
                }
                if (activityTypes.Contains(activity.GetType()))
                {
                    return false;
                }
            }
            return base.CanInsertActivities(this.GetUpdatedLocation(insertLocation), activitiesToInsert);
        }

        public override bool CanRemoveActivities(ReadOnlyCollection<Activity> activitiesToRemove)
        {
            if (activitiesToRemove == null)
            {
                throw new ArgumentNullException("activitiesToRemove");
            }
            return base.CanRemoveActivities(activitiesToRemove);
        }

        private DragDropEffects CheckDragEffect(ActivityDragEventArgs e)
        {
            if (e.Activities.Count == 0)
            {
                return DragDropEffects.None;
            }
            if (this.CurrentDropTarget >= 0)
            {
                if (((e.KeyState & 8) == 8) && ((e.AllowedEffect & DragDropEffects.Copy) == DragDropEffects.Copy))
                {
                    return DragDropEffects.Copy;
                }
                if ((e.AllowedEffect & DragDropEffects.Move) == DragDropEffects.Move)
                {
                    return DragDropEffects.Move;
                }
            }
            return e.Effect;
        }

        protected void DrawConnectors(Graphics graphics, Pen pen, Point[] points, LineAnchor startCap, LineAnchor endCap)
        {
            Size empty = Size.Empty;
            Size maxCapSize = Size.Empty;
            CompositeDesignerTheme designerTheme = base.DesignerTheme as CompositeDesignerTheme;
            if (designerTheme != null)
            {
                empty = new Size(designerTheme.ConnectorSize.Width / 3, designerTheme.ConnectorSize.Height / 3);
                maxCapSize = designerTheme.ConnectorSize;
            }
            ActivityDesignerPaint.DrawConnectors(graphics, pen, points, empty, maxCapSize, startCap, endCap);
        }

        public override void EnsureVisibleContainedDesigner(ActivityDesigner containedDesigner)
        {
            if (containedDesigner == null)
            {
                throw new ArgumentNullException("containedDesigner");
            }
            this.Expanded = true;
            ActivityDesigner activeDesigner = this.ActiveDesigner;
            if ((containedDesigner != activeDesigner) && (containedDesigner != this))
            {
                DesignerView view = null;
                ReadOnlyCollection<DesignerView> validatedViews = this.ValidatedViews;
                foreach (DesignerView view2 in validatedViews)
                {
                    if (containedDesigner == view2.AssociatedDesigner)
                    {
                        view = view2;
                        break;
                    }
                }
                if (view == null)
                {
                    view = validatedViews[0];
                }
                this.ActiveView = view;
                CompositeActivityDesigner designer2 = this.ActiveDesigner as CompositeActivityDesigner;
                if (designer2 != null)
                {
                    if (designer2 != this)
                    {
                        designer2.EnsureVisibleContainedDesigner(containedDesigner);
                    }
                    else
                    {
                        base.EnsureVisibleContainedDesigner(containedDesigner);
                    }
                }
            }
        }

        protected virtual Rectangle[] GetDropTargets(Point dropPoint)
        {
            return new Rectangle[] { base.Bounds };
        }

        protected virtual ReadOnlyCollection<Point> GetInnerConnections(DesignerEdges edges)
        {
            List<Point> list = new List<Point>(this.GetConnections(edges));
            if ((list.Count > 0) && ((edges & DesignerEdges.Top) > DesignerEdges.None))
            {
                Point point = list[0];
                Point point2 = list[0];
                list[0] = new Point(point.X, point2.Y + this.TitleHeight);
            }
            return list.AsReadOnly();
        }

        public override object GetNextSelectableObject(object current, DesignerNavigationDirection direction)
        {
            object nextSelectableObject = null;
            ActivityDesigner activeDesigner = this.ActiveDesigner;
            if (activeDesigner == null)
            {
                return nextSelectableObject;
            }
            if (activeDesigner != this)
            {
                if ((current != activeDesigner.Activity) && (activeDesigner is CompositeActivityDesigner))
                {
                    nextSelectableObject = ((CompositeActivityDesigner) activeDesigner).GetNextSelectableObject(current, direction);
                }
                return nextSelectableObject;
            }
            return base.GetNextSelectableObject(current, direction);
        }

        private System.Workflow.ComponentModel.Design.HitTestInfo GetUpdatedLocation(System.Workflow.ComponentModel.Design.HitTestInfo location)
        {
            int num = 0;
            foreach (DesignerView view in this.Views)
            {
                if (((view.AssociatedDesigner != null) && (this != view.AssociatedDesigner)) && Helpers.IsActivityLocked(view.AssociatedDesigner.Activity))
                {
                    num++;
                }
            }
            return new ConnectorHitTestInfo(this, location.HitLocation, num + location.MapToIndex());
        }

        protected override void Initialize(Activity activity)
        {
            base.Initialize(activity);
            this.ActiveView = this.ValidatedViews[0];
        }

        public override void InsertActivities(System.Workflow.ComponentModel.Design.HitTestInfo insertLocation, ReadOnlyCollection<Activity> activitiesToInsert)
        {
            if (insertLocation == null)
            {
                throw new ArgumentNullException("insertLocation");
            }
            if (activitiesToInsert == null)
            {
                throw new ArgumentNullException("activitiesToInsert");
            }
            base.InsertActivities(this.GetUpdatedLocation(insertLocation), activitiesToInsert);
        }

        protected override void LoadViewState(BinaryReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
            string str = reader.ReadString();
            if ((str != null) && str.Equals("ActiveView", StringComparison.Ordinal))
            {
                int num = reader.ReadInt32();
                ReadOnlyCollection<DesignerView> validatedViews = this.ValidatedViews;
                if ((num != -1) && (num < validatedViews.Count))
                {
                    this.ActiveView = validatedViews[num];
                }
            }
            base.LoadViewState(reader);
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
            base.MoveActivities(this.GetUpdatedLocation(moveLocation), activitiesToMove);
        }

        protected override void OnActivityChanged(ActivityChangedEventArgs e)
        {
            ReadOnlyCollection<DesignerView> views = SecondaryViewProvider.GetViews(this);
            ReadOnlyCollection<DesignerView> onlys2 = this.Views;
            if (views.Count != onlys2.Count)
            {
                this.views = null;
                base.PerformLayout();
            }
            base.OnActivityChanged(e);
        }

        protected override void OnContainedActivitiesChanging(ActivityCollectionChangeEventArgs listChangeArgs)
        {
            base.OnContainedActivitiesChanging(listChangeArgs);
            if ((listChangeArgs.Action == ActivityCollectionChangeAction.Remove) && (listChangeArgs.RemovedItems[0] != null))
            {
                ActivityDesigner activeDesigner = this.ActiveDesigner;
                if ((activeDesigner != null) && (listChangeArgs.RemovedItems[0] == activeDesigner.Activity))
                {
                    this.ActiveView = this.ValidatedViews[0];
                }
                SecondaryViewProvider.OnViewRemoved(this, listChangeArgs.RemovedItems[0].GetType());
            }
        }

        protected override void OnDragDrop(ActivityDragEventArgs e)
        {
            base.OnDragDrop(e);
            if (((e.KeyState & 8) == 8) && ((e.AllowedEffect & DragDropEffects.Copy) == DragDropEffects.Copy))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else if ((e.AllowedEffect & DragDropEffects.Move) == DragDropEffects.Move)
            {
                e.Effect = DragDropEffects.Move;
            }
            try
            {
                CompositeActivityDesigner.InsertActivities(this, new ConnectorHitTestInfo(this, HitTestLocations.Designer, this.CurrentDropTarget), e.Activities, SR.GetString("DragDropActivities"));
            }
            finally
            {
                this.CurrentDropTarget = -1;
            }
        }

        protected override void OnDragEnter(ActivityDragEventArgs e)
        {
            base.OnDragEnter(e);
            this.CurrentDropTarget = this.CanDrop(e);
            e.Effect = this.CheckDragEffect(e);
            e.DragImageSnapPoint = this.SnapInToDropTarget(e);
        }

        protected override void OnDragLeave()
        {
            base.OnDragLeave();
            this.CurrentDropTarget = -1;
        }

        protected override void OnDragOver(ActivityDragEventArgs e)
        {
            base.OnDragOver(e);
            this.CurrentDropTarget = this.CanDrop(e);
            e.Effect = this.CheckDragEffect(e);
            e.DragImageSnapPoint = this.SnapInToDropTarget(e);
        }

        protected override void OnLayoutPosition(ActivityDesignerLayoutEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }
            base.OnLayoutPosition(e);
            if (this.Expanded)
            {
                ActivityDesigner activeDesigner = this.ActiveDesigner;
                if ((activeDesigner != null) && (activeDesigner != this))
                {
                    Point location = this.Location;
                    location.X += (this.Size.Width - activeDesigner.Size.Width) / 2;
                    location.Y += e.AmbientTheme.SelectionSize.Height;
                    activeDesigner.Location = location;
                }
                int titleHeight = this.TitleHeight;
                foreach (ActivityDesigner designer2 in this.ContainedDesigners)
                {
                    designer2.Location = new Point(designer2.Location.X, designer2.Location.Y + titleHeight);
                }
            }
        }

        protected override Size OnLayoutSize(ActivityDesignerLayoutEventArgs e)
        {
            Size size = base.OnLayoutSize(e);
            if (this.Expanded)
            {
                ActivityDesigner activeDesigner = this.ActiveDesigner;
                if ((activeDesigner != null) && (activeDesigner != this))
                {
                    size.Width = Math.Max(size.Width, activeDesigner.Size.Width);
                    size.Height += activeDesigner.Size.Height;
                    size.Width += 2 * e.AmbientTheme.SelectionSize.Width;
                    size.Width += 3 * e.AmbientTheme.Margin.Width;
                    size.Height += e.AmbientTheme.Margin.Height;
                    size.Height += 2 * e.AmbientTheme.SelectionSize.Height;
                }
            }
            return size;
        }

        internal override void OnPaintContainedDesigners(ActivityDesignerPaintEventArgs e)
        {
            bool flag = false;
            foreach (ActivityDesigner designer in this.ContainedDesigners)
            {
                Rectangle bounds = designer.Bounds;
                if (e.ViewPort.IntersectsWith(bounds))
                {
                    flag = true;
                    using (PaintEventArgs args = new PaintEventArgs(e.Graphics, e.ViewPort))
                    {
                        ((IWorkflowDesignerMessageSink) designer).OnPaint(args, e.ViewPort);
                        continue;
                    }
                }
                if (flag)
                {
                    break;
                }
            }
        }

        private void OnPaletteClosed(object sender, EventArgs e)
        {
            base.Invalidate(base.DesignerSmartTag.GetBounds(this, true));
        }

        protected override void OnShowSmartTagVerbs(Point smartTagPoint)
        {
            if (this.itemPalette == null)
            {
                this.itemPalette = new ItemPalette();
                this.itemPalette.Closed += new EventHandler(this.OnPaletteClosed);
                this.itemPalette.SelectionChanged += new SelectionChangeEventHandler<SelectionChangeEventArgs>(this.OnSmartAction);
            }
            this.itemPalette.SetFont(WorkflowTheme.CurrentTheme.AmbientTheme.Font);
            this.itemPalette.Items.Clear();
            foreach (ActivityDesignerVerb verb in this.SmartTagVerbs)
            {
                Image image = verb.Properties[DesignerUserDataKeys.Image] as Image;
                System.Workflow.ComponentModel.Design.ItemInfo item = new System.Workflow.ComponentModel.Design.ItemInfo(verb.Id, image, verb.Text);
                item.UserData[DesignerUserDataKeys.DesignerVerb] = verb;
                this.itemPalette.Items.Add(item);
            }
            Point location = base.PointToScreen(smartTagPoint);
            this.itemPalette.Show(location);
        }

        private void OnSmartAction(object sender, SelectionChangeEventArgs e)
        {
            System.Workflow.ComponentModel.Design.ItemInfo currentItem = e.CurrentItem;
            if (currentItem != null)
            {
                ActivityDesignerVerb verb = currentItem.UserData[DesignerUserDataKeys.DesignerVerb] as ActivityDesignerVerb;
                if (verb != null)
                {
                    verb.Invoke();
                }
            }
        }

        private void OnSmartTagVerb(object sender, EventArgs e)
        {
            ActivityDesignerVerb verb = sender as ActivityDesignerVerb;
            DesignerView view = verb.Properties[DesignerUserDataKeys.DesignerView] as DesignerView;
            if (view != null)
            {
                this.ActiveView = view;
                if (this.Expanded && (view.AssociatedDesigner != null))
                {
                    ISelectionService service = base.GetService(typeof(ISelectionService)) as ISelectionService;
                    if (service != null)
                    {
                        service.SetSelectedComponents(new object[] { view.AssociatedDesigner.Activity }, SelectionTypes.Replace);
                    }
                }
            }
        }

        private void OnSmartTagVerbStatus(object sender, EventArgs e)
        {
            ActivityDesignerVerb verb = sender as ActivityDesignerVerb;
            DesignerView view = verb.Properties[DesignerUserDataKeys.DesignerView] as DesignerView;
            if (view != null)
            {
                verb.Checked = view == this.ActiveView;
            }
        }

        protected virtual void OnViewChanged(DesignerView view)
        {
            base.PerformLayout();
        }

        protected override void SaveViewState(BinaryWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }
            List<DesignerView> list = new List<DesignerView>(this.ValidatedViews);
            writer.Write("ActiveView");
            writer.Write(list.IndexOf(this.activeView));
            base.SaveViewState(writer);
        }

        private Point SnapInToDropTarget(ActivityDragEventArgs e)
        {
            if (this.CurrentDropTarget >= 0)
            {
                Rectangle[] dropTargets = this.GetDropTargets(new Point(e.X, e.Y));
                if (this.CurrentDropTarget < dropTargets.Length)
                {
                    Rectangle rectangle = dropTargets[this.CurrentDropTarget];
                    return new Point(rectangle.Left + (rectangle.Width / 2), rectangle.Top + (rectangle.Height / 2));
                }
            }
            return Point.Empty;
        }

        internal ActivityDesigner ActiveDesigner
        {
            get
            {
                if (this.ActiveView != null)
                {
                    return this.ActiveView.AssociatedDesigner;
                }
                return null;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DesignerView ActiveView
        {
            get
            {
                if (this.activeView == null)
                {
                    this.activeView = this.ValidatedViews[0];
                }
                return this.activeView;
            }
            set
            {
                if ((this.activeView != value) && (value != null))
                {
                    DesignerView activeView = this.activeView;
                    this.activeView = value;
                    value.OnActivate();
                    if (value.AssociatedDesigner == null)
                    {
                        value.OnDeactivate();
                        this.activeView = activeView;
                    }
                    else
                    {
                        if (activeView != null)
                        {
                            activeView.OnDeactivate();
                        }
                        this.OnViewChanged(this.activeView);
                        DesignerHelpers.RefreshDesignerActions(base.Activity.Site);
                        base.RefreshDesignerVerbs();
                    }
                }
            }
        }

        public override ReadOnlyCollection<ActivityDesigner> ContainedDesigners
        {
            get
            {
                List<ActivityDesigner> list = new List<ActivityDesigner>();
                ActivityDesigner activeDesigner = this.ActiveDesigner;
                if (activeDesigner != null)
                {
                    if (activeDesigner == this)
                    {
                        list.AddRange(base.ContainedDesigners);
                        List<ActivityDesigner> list2 = new List<ActivityDesigner>();
                        IList<ActivityDesigner> designersFromSupportedViews = this.DesignersFromSupportedViews;
                        foreach (ActivityDesigner designer2 in list)
                        {
                            bool flag = Helpers.IsAlternateFlowActivity(designer2.Activity);
                            if (designersFromSupportedViews.Contains(designer2) || flag)
                            {
                                list2.Add(designer2);
                            }
                        }
                        foreach (ActivityDesigner designer3 in list2)
                        {
                            list.Remove(designer3);
                        }
                    }
                    else
                    {
                        list.Add(activeDesigner);
                    }
                }
                return list.AsReadOnly();
            }
        }

        protected virtual int CurrentDropTarget
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.currentDropTarget;
            }
            set
            {
                this.currentDropTarget = value;
                base.Invalidate();
            }
        }

        private IList<ActivityDesigner> DesignersFromSupportedViews
        {
            get
            {
                List<ActivityDesigner> list = new List<ActivityDesigner>();
                foreach (DesignerView view in this.ValidatedViews)
                {
                    ActivityDesigner associatedDesigner = view.AssociatedDesigner;
                    if (associatedDesigner != null)
                    {
                        list.Add(associatedDesigner);
                    }
                }
                return list.AsReadOnly();
            }
        }

        public override object FirstSelectableObject
        {
            get
            {
                ActivityDesigner activeDesigner = this.ActiveDesigner;
                if ((activeDesigner != null) && (activeDesigner != this))
                {
                    return activeDesigner.Activity;
                }
                return base.FirstSelectableObject;
            }
        }

        public override object LastSelectableObject
        {
            get
            {
                ActivityDesigner activeDesigner = this.ActiveDesigner;
                if (((activeDesigner != null) && (activeDesigner != this)) && (activeDesigner is CompositeActivityDesigner))
                {
                    return ((CompositeActivityDesigner) activeDesigner).LastSelectableObject;
                }
                return base.LastSelectableObject;
            }
        }

        public override Size MinimumSize
        {
            get
            {
                Size minimumSize = base.MinimumSize;
                ActivityDesigner activeDesigner = this.ActiveDesigner;
                if (((activeDesigner != null) && (activeDesigner != this)) && this.Expanded)
                {
                    minimumSize.Width = Math.Max(minimumSize.Width, 160);
                    minimumSize.Height = Math.Max(minimumSize.Height, 160);
                }
                return minimumSize;
            }
        }

        protected override bool ShowSmartTag
        {
            get
            {
                return ((!string.IsNullOrEmpty(this.Text) && !this.TextRectangle.Size.IsEmpty) && (this.Views.Count > 1));
            }
        }

        protected override ReadOnlyCollection<ActivityDesignerVerb> SmartTagVerbs
        {
            get
            {
                List<ActivityDesignerVerb> list = new List<ActivityDesignerVerb>(base.SmartTagVerbs);
                if (this.Views.Count > 1)
                {
                    for (int i = 0; i < this.Views.Count; i++)
                    {
                        DesignerView view = this.Views[i];
                        ActivityDesignerVerb item = new ActivityDesignerVerb(this, DesignerVerbGroup.Actions, view.Text, new EventHandler(this.OnSmartTagVerb), new EventHandler(this.OnSmartTagVerbStatus));
                        item.Properties[DesignerUserDataKeys.DesignerView] = view;
                        item.Properties[DesignerUserDataKeys.Image] = view.Image;
                        list.Add(item);
                    }
                }
                return list.AsReadOnly();
            }
        }

        internal override bool SmartTagVisible
        {
            get
            {
                return (((this.itemPalette != null) && this.itemPalette.IsVisible) || base.SmartTagVisible);
            }
            set
            {
                base.SmartTagVisible = value;
            }
        }

        private ReadOnlyCollection<DesignerView> ValidatedViews
        {
            get
            {
                ReadOnlyCollection<DesignerView> views = this.Views;
                if (views.Count == 0)
                {
                    throw new InvalidOperationException(DR.GetString("Error_MultiviewSequentialActivityDesigner", new object[0]));
                }
                return views;
            }
        }

        public virtual ReadOnlyCollection<DesignerView> Views
        {
            get
            {
                if (this.views == null)
                {
                    this.views = new List<DesignerView>();
                    this.views.AddRange(SecondaryViewProvider.GetViews(this));
                }
                return this.views.AsReadOnly();
            }
        }
    }
}

