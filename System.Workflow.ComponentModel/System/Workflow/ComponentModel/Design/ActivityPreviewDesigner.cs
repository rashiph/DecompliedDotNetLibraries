namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.IO;
    using System.Windows.Forms;
    using System.Workflow.ComponentModel;

    [ActivityDesignerTheme(typeof(ActivityPreviewDesignerTheme)), SRCategory("ActivityPreviewDesigners", "System.Workflow.ComponentModel.Design.DesignerResources")]
    public class ActivityPreviewDesigner : SequentialActivityDesigner
    {
        private ActivityCollectionAccessibleObject accessibilityObject;
        private ActivityDesignerVerbCollection designerVerbs;
        private PreviewItemStrip previewStrip;
        private PreviewWindow previewWindow;
        private bool removePreviewedDesigner;
        private Point[] separatorLine = new Point[2];

        public ActivityPreviewDesigner()
        {
            this.previewStrip = new PreviewItemStrip(this);
            this.previewStrip.SelectionChanged += new SelectionChangeEventHandler<SelectionChangeEventArgs>(this.OnPreviewChanged);
            this.previewStrip.HelpText = DR.GetString("DropActivitiesHere", new object[0]);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ISelectionService service = base.GetService(typeof(ISelectionService)) as ISelectionService;
                if (service != null)
                {
                    service.SelectionChanged -= new EventHandler(this.OnSelectionChanged);
                }
            }
            base.Dispose(disposing);
        }

        public override void EnsureVisibleContainedDesigner(ActivityDesigner containedDesigner)
        {
            if (containedDesigner == null)
            {
                throw new ArgumentNullException("containedDesigner");
            }
            base.EnsureVisibleContainedDesigner(containedDesigner);
            if (base.ActiveDesigner == this)
            {
                foreach (System.Workflow.ComponentModel.Design.ItemInfo info in this.previewStrip.Items)
                {
                    if (info.UserData[DesignerUserDataKeys.Activity] == containedDesigner.Activity)
                    {
                        this.previewStrip.ActiveItem = info;
                        break;
                    }
                }
            }
        }

        protected internal override Rectangle[] GetConnectors()
        {
            if ((!this.Expanded || (this.ContainedDesigners.Count > 0)) || (this.ShowPreview || (base.ActiveDesigner != this)))
            {
                return new Rectangle[0];
            }
            AmbientTheme ambientTheme = WorkflowTheme.CurrentTheme.AmbientTheme;
            int num = ambientTheme.Margin.Height + this.previewStrip.Size.Height;
            num += ambientTheme.Margin.Height;
            Rectangle[] connectors = base.GetConnectors();
            if (connectors.Length > 0)
            {
                connectors[0].Y += num;
                connectors[0].Height -= num;
            }
            return connectors;
        }

        protected override Rectangle[] GetDropTargets(Point dropPoint)
        {
            if ((base.ActiveDesigner == this) && this.previewStrip.Bounds.Contains(dropPoint))
            {
                return this.previewStrip.DropTargets;
            }
            return base.GetDropTargets(dropPoint);
        }

        public override object GetNextSelectableObject(object obj, DesignerNavigationDirection direction)
        {
            if (base.ActiveDesigner != this)
            {
                return base.GetNextSelectableObject(obj, direction);
            }
            if ((direction != DesignerNavigationDirection.Left) && (direction != DesignerNavigationDirection.Right))
            {
                return null;
            }
            object obj2 = null;
            int num = this.StripItemIndexFromActivity(obj as Activity);
            if ((direction == DesignerNavigationDirection.Left) && (num >= 0))
            {
                return this.previewStrip.Items[(num > 0) ? (num - 1) : (this.previewStrip.Items.Count - 1)].UserData[DesignerUserDataKeys.Activity];
            }
            if ((direction == DesignerNavigationDirection.Right) && (num <= (this.previewStrip.Items.Count - 1)))
            {
                obj2 = this.previewStrip.Items[(num < (this.previewStrip.Items.Count - 1)) ? (num + 1) : 0].UserData[DesignerUserDataKeys.Activity];
            }
            return obj2;
        }

        public override System.Workflow.ComponentModel.Design.HitTestInfo HitTest(Point point)
        {
            System.Workflow.ComponentModel.Design.HitTestInfo nowhere = System.Workflow.ComponentModel.Design.HitTestInfo.Nowhere;
            if (this.Expanded && (base.ActiveDesigner == this))
            {
                if ((this.ContainedDesigners.Count == 0) && this.HelpTextRectangle.Contains(point))
                {
                    return new ConnectorHitTestInfo(this, HitTestLocations.Designer, 0);
                }
                if (this.previewStrip.Bounds.Contains(point))
                {
                    System.Workflow.ComponentModel.Design.ItemInfo info2 = this.previewStrip.HitTest(point);
                    ActivityDesigner designer = (info2 != null) ? ActivityDesigner.GetDesigner(info2.UserData[DesignerUserDataKeys.Activity] as Activity) : null;
                    if (designer != null)
                    {
                        return new System.Workflow.ComponentModel.Design.HitTestInfo(designer, HitTestLocations.Designer);
                    }
                    return new System.Workflow.ComponentModel.Design.HitTestInfo(this, HitTestLocations.ActionArea | HitTestLocations.Designer);
                }
                if ((this.ShowPreview && this.previewWindow.Bounds.Contains(point)) && ((this.previewWindow.PreviewMode || (this.PreviewedDesigner == null)) || !this.PreviewedDesigner.Bounds.Contains(point)))
                {
                    return new System.Workflow.ComponentModel.Design.HitTestInfo(this, HitTestLocations.ActionArea | HitTestLocations.Designer);
                }
                nowhere = base.HitTest(point);
                if ((this.ShowPreview && this.previewWindow.PreviewMode) && (nowhere.AssociatedDesigner != this))
                {
                    nowhere = System.Workflow.ComponentModel.Design.HitTestInfo.Nowhere;
                }
                return nowhere;
            }
            return base.HitTest(point);
        }

        protected override void Initialize(Activity activity)
        {
            base.Initialize(activity);
            this.ShowPreview = true;
            CompositeActivity activity2 = base.Activity as CompositeActivity;
            if (activity2 != null)
            {
                foreach (Activity activity3 in activity2.Activities)
                {
                    if (!Helpers.IsAlternateFlowActivity(activity3))
                    {
                        System.Workflow.ComponentModel.Design.ItemInfo item = new System.Workflow.ComponentModel.Design.ItemInfo(activity3.GetHashCode());
                        item.UserData[DesignerUserDataKeys.Activity] = activity3;
                        this.previewStrip.Items.Add(item);
                    }
                }
            }
            ISelectionService service = base.GetService(typeof(ISelectionService)) as ISelectionService;
            if (service != null)
            {
                service.SelectionChanged += new EventHandler(this.OnSelectionChanged);
            }
        }

        public override bool IsContainedDesignerVisible(ActivityDesigner containedDesigner)
        {
            if (containedDesigner == null)
            {
                throw new ArgumentNullException("containedDesigner");
            }
            if (base.ActiveDesigner != this)
            {
                return base.IsContainedDesignerVisible(containedDesigner);
            }
            if (this.ShowPreview && this.previewWindow.PreviewMode)
            {
                return false;
            }
            return ((this.previewStrip.ActiveItem != null) && (this.previewStrip.ActiveItem.UserData[DesignerUserDataKeys.Activity] == containedDesigner.Activity));
        }

        protected override void LoadViewState(BinaryReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
            int num = reader.ReadInt32();
            if ((num != -1) && (num < this.previewStrip.Items.Count))
            {
                System.Workflow.ComponentModel.Design.ItemInfo info = this.previewStrip.Items[num];
                IDesignerHost service = base.GetService(typeof(IDesignerHost)) as IDesignerHost;
                if (service == null)
                {
                    throw new Exception(SR.GetString("General_MissingService", new object[] { typeof(IDesignerHost).FullName }));
                }
                this.EnsureVisibleContainedDesigner(service.GetDesigner((Activity) info.UserData[DesignerUserDataKeys.Activity]) as ActivityDesigner);
            }
            bool flag = reader.ReadBoolean();
            if (this.ShowPreview)
            {
                this.previewWindow.PreviewMode = flag;
            }
            base.LoadViewState(reader);
        }

        private void OnChangePreviewMode(object sender, EventArgs args)
        {
            DesignerVerb verb = sender as DesignerVerb;
            if ((this.ShowPreview && (verb != null)) && verb.Properties.Contains(DesignerUserDataKeys.PreviewActivity))
            {
                this.previewWindow.PreviewMode = (bool) verb.Properties[DesignerUserDataKeys.PreviewActivity];
            }
        }

        protected override void OnContainedActivitiesChanged(ActivityCollectionChangeEventArgs listChangeArgs)
        {
            if (base.ActiveDesigner == this)
            {
                if (((listChangeArgs.Action == ActivityCollectionChangeAction.Add) && (listChangeArgs.AddedItems.Count > 0)) && !Helpers.IsAlternateFlowActivity(listChangeArgs.AddedItems[0]))
                {
                    System.Workflow.ComponentModel.Design.ItemInfo item = new System.Workflow.ComponentModel.Design.ItemInfo(listChangeArgs.AddedItems[0].GetHashCode());
                    item.UserData[DesignerUserDataKeys.Activity] = listChangeArgs.AddedItems[0];
                    if (listChangeArgs.Index < this.previewStrip.Items.Count)
                    {
                        this.previewStrip.Items.Insert(listChangeArgs.Index, item);
                    }
                    else
                    {
                        this.previewStrip.Items.Add(item);
                    }
                }
                else if ((listChangeArgs.Action == ActivityCollectionChangeAction.Remove) && (listChangeArgs.RemovedItems.Count > 0))
                {
                    int index = this.previewStrip.Items.IndexOf(new System.Workflow.ComponentModel.Design.ItemInfo(listChangeArgs.RemovedItems[0].GetHashCode()));
                    if (index >= 0)
                    {
                        this.previewStrip.Items.RemoveAt(index);
                    }
                }
            }
            base.OnContainedActivitiesChanged(listChangeArgs);
        }

        protected override void OnDragEnter(ActivityDragEventArgs e)
        {
            base.OnDragEnter(e);
            e.DragImageSnapPoint = this.SnapInToPreviewStripDropTarget(e);
        }

        protected override void OnDragOver(ActivityDragEventArgs e)
        {
            base.OnDragOver(e);
            e.DragImageSnapPoint = this.SnapInToPreviewStripDropTarget(e);
        }

        protected override void OnLayoutPosition(ActivityDesignerLayoutEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }
            if ((base.ActiveDesigner == this) && this.Expanded)
            {
                Rectangle bounds = base.Bounds;
                this.previewStrip.Location = new Point((bounds.Left + (bounds.Width / 2)) - (this.previewStrip.Size.Width / 2), (this.Location.Y + this.TitleHeight) + e.AmbientTheme.Margin.Height);
                base.OnLayoutPosition(e);
                if (this.ShowPreview)
                {
                    Rectangle rectangle2 = this.previewStrip.Bounds;
                    this.previewWindow.Location = new Point((bounds.Left + (bounds.Width / 2)) - (this.previewWindow.Size.Width / 2), rectangle2.Bottom + (3 * e.AmbientTheme.Margin.Height));
                    this.separatorLine[0].X = bounds.Left + e.AmbientTheme.Margin.Width;
                    this.separatorLine[0].Y = rectangle2.Bottom;
                    this.separatorLine[0].Y += e.AmbientTheme.Margin.Height + (e.AmbientTheme.Margin.Height / 2);
                    this.separatorLine[1].X = bounds.Right - e.AmbientTheme.Margin.Width;
                    this.separatorLine[1].Y = rectangle2.Bottom;
                    this.separatorLine[1].Y += e.AmbientTheme.Margin.Height + (e.AmbientTheme.Margin.Height / 2);
                }
                else
                {
                    int num = this.previewStrip.Bounds.Bottom - this.Location.Y;
                    if (this.PreviewedDesigner != null)
                    {
                        this.PreviewedDesigner.Location = new Point(this.Location.X + ((this.Size.Width - this.PreviewedDesigner.Size.Width) / 2), (this.Location.Y + num) + (2 * e.AmbientTheme.Margin.Height));
                    }
                }
            }
            else
            {
                base.OnLayoutPosition(e);
            }
        }

        protected override Size OnLayoutSize(ActivityDesignerLayoutEventArgs e)
        {
            Size size = base.OnLayoutSize(e);
            if (!this.Expanded || (base.ActiveDesigner != this))
            {
                return size;
            }
            this.previewStrip.OnLayoutSize(e.Graphics);
            Size empty = Size.Empty;
            empty.Width = Math.Max(empty.Width, this.previewStrip.Size.Width);
            empty.Height += this.previewStrip.Size.Height;
            empty.Height += e.AmbientTheme.Margin.Height;
            if (this.previewWindow != null)
            {
                this.previewWindow.Refresh();
                this.previewWindow.OnLayoutSize(e.Graphics, empty.Width);
                empty.Width = Math.Max(empty.Width, this.previewWindow.Size.Width);
                empty.Width += 2 * e.AmbientTheme.Margin.Width;
                empty.Height += this.TitleHeight;
                empty.Height += 4 * e.AmbientTheme.Margin.Height;
                empty.Height += this.previewWindow.Size.Height;
                empty.Height += e.AmbientTheme.Margin.Height;
            }
            else
            {
                empty.Width = Math.Max(empty.Width, size.Width);
                empty.Width += 3 * e.AmbientTheme.Margin.Width;
                empty.Width += 2 * e.AmbientTheme.SelectionSize.Width;
                empty.Height += size.Height;
            }
            empty.Width = Math.Max(empty.Width, this.MinimumSize.Width);
            empty.Height = Math.Max(empty.Height, this.MinimumSize.Height);
            if (!this.ShowPreview && (this.PreviewedDesigner != null))
            {
                ActivityPreviewDesignerTheme designerTheme = e.DesignerTheme as ActivityPreviewDesignerTheme;
                if (designerTheme != null)
                {
                    empty.Height -= designerTheme.ConnectorSize.Height;
                    empty.Height -= 2 * e.AmbientTheme.Margin.Height;
                    empty.Height -= 2 * e.AmbientTheme.SelectionSize.Height;
                }
                Size size3 = new Size((2 * e.AmbientTheme.Margin.Width) + (2 * e.AmbientTheme.SelectionSize.Width), (2 * e.AmbientTheme.Margin.Height) + (2 * e.AmbientTheme.SelectionSize.Height));
                this.PreviewedDesigner.Size = new Size(empty.Width - size3.Width, empty.Height - ((this.TitleHeight + this.previewStrip.Size.Height) + size3.Height));
            }
            return empty;
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            if (base.ActiveDesigner == this)
            {
                Point pt = new Point(e.X, e.Y);
                if (((this.PreviewedDesigner != null) && this.ShowPreview) && (this.previewWindow.PreviewMode && this.previewWindow.Bounds.Contains(pt)))
                {
                    this.previewWindow.PreviewMode = false;
                }
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (base.ActiveDesigner == this)
            {
                Point pt = new Point(e.X, e.Y);
                if (this.previewStrip.Bounds.Contains(pt))
                {
                    this.previewStrip.OnMouseDown(e);
                }
                else if (this.ShowPreview && this.previewWindow.Bounds.Contains(pt))
                {
                    this.previewWindow.OnMouseDown(e);
                }
            }
        }

        protected override void OnMouseLeave()
        {
            base.OnMouseLeave();
            if (base.ActiveDesigner == this)
            {
                this.previewStrip.OnMouseLeave();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (base.ActiveDesigner == this)
            {
                this.previewStrip.OnMouseUp(e);
            }
        }

        protected override void OnPaint(ActivityDesignerPaintEventArgs e)
        {
            if ((this.ShowPreview && !this.previewWindow.PreviewMode) && (this.PreviewedDesigner != null))
            {
                this.removePreviewedDesigner = true;
            }
            base.OnPaint(e);
            if ((this.ShowPreview && !this.previewWindow.PreviewMode) && (this.PreviewedDesigner != null))
            {
                this.removePreviewedDesigner = false;
            }
            if (this.Expanded && (base.ActiveDesigner == this))
            {
                this.previewStrip.Draw(e.Graphics);
                if (this.ShowPreview)
                {
                    e.Graphics.DrawLine(e.DesignerTheme.ForegroundPen, this.separatorLine[0], this.separatorLine[1]);
                    this.previewWindow.Draw(e.Graphics, e.ViewPort);
                }
            }
        }

        private void OnPreviewChanged(object sender, SelectionChangeEventArgs e)
        {
            bool flag = !this.ShowPreview;
            if (this.ShowPreview)
            {
                this.previewWindow.PreviewedActivity = (e.CurrentItem != null) ? (e.CurrentItem.UserData[DesignerUserDataKeys.Activity] as Activity) : null;
                flag = !this.previewWindow.PreviewMode;
            }
            if (flag)
            {
                base.PerformLayout();
            }
        }

        private void OnPreviewModeChanged(object sender, EventArgs e)
        {
            base.RefreshDesignerVerbs();
        }

        private void OnPreviewModeStatusUpdate(object sender, EventArgs args)
        {
            DesignerVerb verb = sender as DesignerVerb;
            if ((this.ShowPreview && (verb != null)) && verb.Properties.Contains(DesignerUserDataKeys.PreviewActivity))
            {
                verb.Enabled = this.previewWindow.PreviewMode != ((bool) verb.Properties[DesignerUserDataKeys.PreviewActivity]);
            }
        }

        private void OnSelectionChanged(object sender, EventArgs e)
        {
            ISelectionService service = base.GetService(typeof(ISelectionService)) as ISelectionService;
            if ((service != null) && (base.ActiveDesigner == this))
            {
                foreach (System.Workflow.ComponentModel.Design.ItemInfo info in this.previewStrip.Items)
                {
                    if (info.UserData[DesignerUserDataKeys.Activity] == service.PrimarySelection)
                    {
                        this.previewStrip.ActiveItem = info;
                        break;
                    }
                }
                if (service.SelectionCount == 1)
                {
                    ActivityDesigner designer = ActivityDesigner.GetDesigner(service.PrimarySelection as Activity);
                    if (((designer != null) && !designer.IsVisible) && (this != designer.ParentDesigner))
                    {
                        ActivityDesigner designer2 = designer;
                        while (designer2 != null)
                        {
                            CompositeActivityDesigner parentDesigner = designer2.ParentDesigner;
                            if (this == parentDesigner)
                            {
                                break;
                            }
                            designer2 = parentDesigner;
                        }
                        if (designer2 != null)
                        {
                            if ((this.previewWindow != null) && this.previewWindow.PreviewMode)
                            {
                                designer2.EnsureVisible();
                            }
                            else
                            {
                                designer.EnsureVisible();
                            }
                        }
                    }
                }
            }
        }

        protected override void OnThemeChange(ActivityDesignerTheme newTheme)
        {
            base.OnThemeChange(newTheme);
            this.RefreshPreview();
        }

        private void OnViewActivity(object sender, EventArgs args)
        {
            DesignerVerb verb = sender as DesignerVerb;
            if ((verb != null) && verb.Properties.Contains(DesignerUserDataKeys.ViewActivity))
            {
                System.Workflow.ComponentModel.Design.ItemInfo activeItem = this.previewStrip.ActiveItem;
                if (activeItem != null)
                {
                    bool flag = (bool) verb.Properties[DesignerUserDataKeys.ViewActivity];
                    int num = this.previewStrip.Items.IndexOf(activeItem) + (flag ? 1 : -1);
                    num = (num >= this.previewStrip.Items.Count) ? 0 : ((num < 0) ? (this.previewStrip.Items.Count - 1) : num);
                    this.previewStrip.ActiveItem = this.previewStrip.Items[num];
                }
            }
        }

        private void OnViewActivityStatusUpdate(object sender, EventArgs args)
        {
            DesignerVerb verb = sender as DesignerVerb;
            if (verb != null)
            {
                verb.Enabled = ((this.previewStrip.ActiveItem != null) && (this.previewStrip.Items.Count > 1)) && (this.previewStrip.Items.IndexOf(this.previewStrip.ActiveItem) >= 0);
            }
        }

        protected internal override void RefreshDesignerActions()
        {
            base.RefreshDesignerActions();
            this.RefreshPreview();
        }

        public void RefreshPreview()
        {
            if (this.ShowPreview)
            {
                this.previewWindow.Refresh();
            }
        }

        protected override void SaveViewState(BinaryWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }
            int index = -1;
            bool previewMode = false;
            if (this.previewStrip.ActiveItem != null)
            {
                index = this.previewStrip.Items.IndexOf(this.previewStrip.ActiveItem);
            }
            writer.Write(index);
            if (this.ShowPreview)
            {
                previewMode = this.previewWindow.PreviewMode;
            }
            writer.Write(previewMode);
            base.SaveViewState(writer);
        }

        private Point SnapInToPreviewStripDropTarget(ActivityDragEventArgs e)
        {
            int activeDropTarget = this.previewStrip.ActiveDropTarget;
            Rectangle[] dropTargets = this.previewStrip.DropTargets;
            if ((activeDropTarget < 0) || (activeDropTarget >= dropTargets.Length))
            {
                return Point.Empty;
            }
            Rectangle rectangle = dropTargets[activeDropTarget];
            ActivityPreviewDesignerTheme designerTheme = base.DesignerTheme as ActivityPreviewDesignerTheme;
            rectangle.Width = (rectangle.Width > ((designerTheme != null) ? designerTheme.ConnectorSize.Width : 0)) ? ((designerTheme != null) ? designerTheme.ConnectorSize.Width : 0) : rectangle.Width;
            return new Point(rectangle.Left + (rectangle.Width / 2), rectangle.Top + (rectangle.Height / 2));
        }

        private int StripItemIndexFromActivity(Activity activity)
        {
            int num = 0;
            foreach (System.Workflow.ComponentModel.Design.ItemInfo info in this.previewStrip.Items)
            {
                if (info.UserData[DesignerUserDataKeys.Activity] == activity)
                {
                    return num;
                }
                num++;
            }
            return -1;
        }

        public override AccessibleObject AccessibilityObject
        {
            get
            {
                if (this.accessibilityObject == null)
                {
                    this.accessibilityObject = new ActivityCollectionAccessibleObject(this);
                }
                return this.accessibilityObject;
            }
        }

        public override ReadOnlyCollection<ActivityDesigner> ContainedDesigners
        {
            get
            {
                if (base.ActiveDesigner != this)
                {
                    return base.ContainedDesigners;
                }
                List<ActivityDesigner> list = new List<ActivityDesigner>();
                if (this.PreviewedDesigner != null)
                {
                    if (this.ShowPreview)
                    {
                        list.AddRange(base.ContainedDesigners);
                        if (this.removePreviewedDesigner)
                        {
                            list.Remove(this.PreviewedDesigner);
                        }
                    }
                    else
                    {
                        list.Add(this.PreviewedDesigner);
                    }
                }
                return list.AsReadOnly();
            }
        }

        protected override int CurrentDropTarget
        {
            get
            {
                return base.CurrentDropTarget;
            }
            set
            {
                base.CurrentDropTarget = value;
                this.previewStrip.ActiveDropTarget = value;
            }
        }

        public override object FirstSelectableObject
        {
            get
            {
                if (this.Expanded && this.IsVisible)
                {
                    if (this.PreviewedDesigner != null)
                    {
                        return this.PreviewedDesigner.Activity;
                    }
                    if (this.ContainedDesigners.Count > 0)
                    {
                        return this.ContainedDesigners[0].Activity;
                    }
                }
                return null;
            }
        }

        protected internal override ActivityDesignerGlyphCollection Glyphs
        {
            get
            {
                ActivityDesignerGlyphCollection glyphs = new ActivityDesignerGlyphCollection();
                if (this.Expanded && (base.ActiveDesigner == this))
                {
                    ISelectionService service = base.GetService(typeof(ISelectionService)) as ISelectionService;
                    WorkflowDesignerLoader loader = base.GetService(typeof(WorkflowDesignerLoader)) as WorkflowDesignerLoader;
                    bool flag = (loader != null) && !loader.InDebugMode;
                    foreach (System.Workflow.ComponentModel.Design.ItemInfo info in this.previewStrip.Items)
                    {
                        Rectangle itemBounds = this.previewStrip.GetItemBounds(info);
                        Activity component = info.UserData[DesignerUserDataKeys.Activity] as Activity;
                        if ((component != null) && !itemBounds.IsEmpty)
                        {
                            if ((service != null) && service.GetComponentSelected(component))
                            {
                                glyphs.Add(new StripItemSelectionGlyph(this, info));
                            }
                            if (!component.Enabled && !ActivityDesigner.IsCommentedActivity(component))
                            {
                                glyphs.Add(new StripItemCommentGlyph(this, info));
                            }
                            if ((this.ShowPreview && flag) && this.Expanded)
                            {
                                ActivityDesigner designer = ActivityDesigner.GetDesigner(component);
                                if (((designer != null) && (designer.DesignerActions.Count > 0)) && flag)
                                {
                                    glyphs.Add(new StripItemConfigErrorGlyph(this, info));
                                }
                            }
                        }
                    }
                }
                glyphs.AddRange(base.Glyphs);
                return glyphs;
            }
        }

        protected override string HelpText
        {
            get
            {
                return base.HelpText;
            }
            set
            {
                base.HelpText = value;
                this.previewStrip.HelpText = value;
            }
        }

        public override object LastSelectableObject
        {
            get
            {
                if (this.Expanded && this.IsVisible)
                {
                    if (this.PreviewedDesigner != null)
                    {
                        CompositeActivityDesigner previewedDesigner = this.PreviewedDesigner as CompositeActivityDesigner;
                        if (((this.previewWindow != null) && !this.previewWindow.PreviewMode) && (previewedDesigner != null))
                        {
                            return previewedDesigner.LastSelectableObject;
                        }
                        return this.PreviewedDesigner.Activity;
                    }
                    if (this.ContainedDesigners.Count > 0)
                    {
                        return this.ContainedDesigners[this.ContainedDesigners.Count - 1].Activity;
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
                    Size size = new Size(value.X - base.Location.X, value.Y - base.Location.Y);
                    base.Location = value;
                    this.previewStrip.Location = new Point(this.previewStrip.Location.X + size.Width, this.previewStrip.Location.Y + size.Height);
                    if (this.ShowPreview)
                    {
                        this.previewWindow.Location = new Point(this.previewWindow.Location.X + size.Width, this.previewWindow.Location.Y + size.Height);
                        this.separatorLine[0] = new Point(this.separatorLine[0].X + size.Width, this.separatorLine[0].Y + size.Height);
                        this.separatorLine[1] = new Point(this.separatorLine[1].X + size.Width, this.separatorLine[1].Y + size.Height);
                    }
                    else
                    {
                        int num = this.previewStrip.Bounds.Bottom - this.Location.Y;
                        if (this.PreviewedDesigner != null)
                        {
                            this.PreviewedDesigner.Location = new Point(this.Location.X + ((this.Size.Width - this.PreviewedDesigner.Size.Width) / 2), (this.Location.Y + num) + (2 * WorkflowTheme.CurrentTheme.AmbientTheme.Margin.Height));
                        }
                    }
                }
            }
        }

        public ActivityDesigner PreviewedDesigner
        {
            get
            {
                System.Workflow.ComponentModel.Design.ItemInfo activeItem = this.previewStrip.ActiveItem;
                if (activeItem == null)
                {
                    return null;
                }
                return ActivityDesigner.GetDesigner(activeItem.UserData[DesignerUserDataKeys.Activity] as Activity);
            }
        }

        public bool ShowPreview
        {
            get
            {
                return (this.previewWindow != null);
            }
            set
            {
                if (this.ShowPreview != value)
                {
                    if (this.previewWindow != null)
                    {
                        this.previewWindow.PreviewModeChanged -= new EventHandler(this.OnPreviewModeChanged);
                        this.previewWindow = null;
                    }
                    else
                    {
                        this.previewWindow = new PreviewWindow(this);
                        this.previewWindow.PreviewModeChanged += new EventHandler(this.OnPreviewModeChanged);
                    }
                    this.designerVerbs = null;
                    TypeDescriptor.Refresh(base.Activity);
                    base.RefreshDesignerVerbs();
                }
            }
        }

        protected override ActivityDesignerVerbCollection Verbs
        {
            get
            {
                ActivityDesignerVerbCollection verbs = new ActivityDesignerVerbCollection();
                verbs.AddRange(base.Verbs);
                if (this.designerVerbs == null)
                {
                    this.designerVerbs = new ActivityDesignerVerbCollection();
                    if (base.ActiveDesigner == this)
                    {
                        DesignerVerb verb = new ActivityDesignerVerb(this, DesignerVerbGroup.View, DR.GetString("ViewPreviousActivity", new object[0]), new EventHandler(this.OnViewActivity), new EventHandler(this.OnViewActivityStatusUpdate));
                        verb.Properties[DesignerUserDataKeys.ViewActivity] = false;
                        this.designerVerbs.Add(verb);
                        verb = new ActivityDesignerVerb(this, DesignerVerbGroup.View, DR.GetString("ViewNextActivity", new object[0]), new EventHandler(this.OnViewActivity), new EventHandler(this.OnViewActivityStatusUpdate));
                        verb.Properties[DesignerUserDataKeys.ViewActivity] = true;
                        this.designerVerbs.Add(verb);
                        if (this.ShowPreview)
                        {
                            verb = new ActivityDesignerVerb(this, DesignerVerbGroup.Edit, DR.GetString("PreviewActivity", new object[0]), new EventHandler(this.OnChangePreviewMode), new EventHandler(this.OnPreviewModeStatusUpdate));
                            verb.Properties[DesignerUserDataKeys.PreviewActivity] = true;
                            this.designerVerbs.Add(verb);
                            verb = new ActivityDesignerVerb(this, DesignerVerbGroup.Edit, DR.GetString("EditActivity", new object[0]), new EventHandler(this.OnChangePreviewMode), new EventHandler(this.OnPreviewModeStatusUpdate));
                            verb.Properties[DesignerUserDataKeys.PreviewActivity] = false;
                            this.designerVerbs.Add(verb);
                        }
                    }
                }
                verbs.AddRange(this.designerVerbs);
                return verbs;
            }
        }

        private sealed class ActivityCollectionAccessibleObject : SequenceDesignerAccessibleObject
        {
            public ActivityCollectionAccessibleObject(ActivityPreviewDesigner activityDesigner) : base(activityDesigner)
            {
            }

            public override AccessibleObject GetChild(int index)
            {
                ActivityPreviewDesigner activityDesigner = base.ActivityDesigner as ActivityPreviewDesigner;
                if ((activityDesigner == null) || (activityDesigner.ActiveDesigner != activityDesigner))
                {
                    return base.GetChild(index);
                }
                if (index < activityDesigner.previewStrip.AccessibilityObjects.Length)
                {
                    return activityDesigner.previewStrip.AccessibilityObjects[index];
                }
                index -= activityDesigner.previewStrip.AccessibilityObjects.Length;
                if (activityDesigner.ShowPreview && (index == 0))
                {
                    return activityDesigner.previewWindow.AccessibilityObject;
                }
                AccessibleObject accessibilityObject = activityDesigner.PreviewedDesigner.AccessibilityObject;
                while (accessibilityObject.Bounds.Size.IsEmpty && (accessibilityObject.GetChildCount() > 0))
                {
                    accessibilityObject = accessibilityObject.GetChild(0);
                }
                return accessibilityObject;
            }

            public override int GetChildCount()
            {
                int num = 0;
                ActivityPreviewDesigner activityDesigner = base.ActivityDesigner as ActivityPreviewDesigner;
                if ((activityDesigner != null) && (activityDesigner.ActiveDesigner == activityDesigner))
                {
                    num += activityDesigner.previewStrip.AccessibilityObjects.Length;
                    if (activityDesigner.ShowPreview)
                    {
                        num++;
                    }
                    if ((!activityDesigner.ShowPreview || activityDesigner.previewWindow.PreviewMode) && activityDesigner.ShowPreview)
                    {
                        return num;
                    }
                    num++;
                }
                return num;
            }
        }

        private sealed class StripItemCommentGlyph : CommentGlyph
        {
            private System.Workflow.ComponentModel.Design.ItemInfo item;
            private ActivityPreviewDesigner parentDesigner;

            internal StripItemCommentGlyph(ActivityPreviewDesigner parentDesigner, System.Workflow.ComponentModel.Design.ItemInfo item)
            {
                this.parentDesigner = parentDesigner;
                this.item = item;
            }

            public override Rectangle GetBounds(ActivityDesigner designer, bool activated)
            {
                return this.parentDesigner.previewStrip.GetItemBounds(this.item);
            }
        }

        private sealed class StripItemConfigErrorGlyph : ConfigErrorGlyph
        {
            private System.Workflow.ComponentModel.Design.ItemInfo item;
            private ActivityPreviewDesigner parentDesigner;

            internal StripItemConfigErrorGlyph(ActivityPreviewDesigner parentDesigner, System.Workflow.ComponentModel.Design.ItemInfo item)
            {
                this.parentDesigner = parentDesigner;
                this.item = item;
            }

            public override Rectangle GetBounds(ActivityDesigner designer, bool activated)
            {
                Rectangle itemBounds = this.parentDesigner.previewStrip.GetItemBounds(this.item);
                Size glyphSize = WorkflowTheme.CurrentTheme.AmbientTheme.GlyphSize;
                glyphSize.Width = (glyphSize.Width * 3) / 4;
                glyphSize.Height = (glyphSize.Height * 3) / 4;
                Point location = new Point(itemBounds.Right - (glyphSize.Width / 2), itemBounds.Top - (glyphSize.Height / 2));
                itemBounds = new Rectangle(location, glyphSize);
                if (activated)
                {
                    itemBounds.Width *= 2;
                    AmbientTheme ambientTheme = WorkflowTheme.CurrentTheme.AmbientTheme;
                    itemBounds.Inflate(ambientTheme.Margin.Width / 2, ambientTheme.Margin.Height / 2);
                }
                return itemBounds;
            }

            protected override void OnActivate(ActivityDesigner designer)
            {
                ActivityDesigner designer2 = ActivityDesigner.GetDesigner(this.item.UserData[DesignerUserDataKeys.Activity] as Activity);
                if (designer2 != null)
                {
                    base.OnActivate(designer2);
                }
            }
        }

        private sealed class StripItemSelectionGlyph : SelectionGlyph
        {
            private System.Workflow.ComponentModel.Design.ItemInfo item;
            private ActivityPreviewDesigner parentDesigner;

            internal StripItemSelectionGlyph(ActivityPreviewDesigner parentDesigner, System.Workflow.ComponentModel.Design.ItemInfo item)
            {
                this.parentDesigner = parentDesigner;
                this.item = item;
            }

            public override Rectangle GetBounds(ActivityDesigner designer, bool activated)
            {
                Rectangle itemBounds = this.parentDesigner.previewStrip.GetItemBounds(this.item);
                int width = Math.Max(itemBounds.Width / 6, 1);
                Size size = new Size(width, Math.Max(itemBounds.Height / 6, 1));
                itemBounds.Inflate(size);
                return itemBounds;
            }

            public override bool IsPrimarySelection
            {
                get
                {
                    ISelectionService service = this.parentDesigner.GetService(typeof(ISelectionService)) as ISelectionService;
                    return ((service != null) && (service.PrimarySelection == this.item.UserData[DesignerUserDataKeys.Activity]));
                }
            }
        }
    }
}

