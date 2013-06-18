namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Runtime;
    using System.Threading;
    using System.Windows.Forms;
    using System.Workflow.ComponentModel;

    internal sealed class PreviewItemStrip
    {
        private List<ItemStripAccessibleObject> accessibilityObjects;
        private int activeDropTarget = -1;
        private System.Workflow.ComponentModel.Design.ItemInfo activeItem;
        private ScrollButton activeScrollButton;
        private Rectangle bounds = Rectangle.Empty;
        private string helpText = string.Empty;
        private ItemList<System.Workflow.ComponentModel.Design.ItemInfo> items;
        private ActivityPreviewDesigner parentDesigner;
        private int scrollMarker;

        public event SelectionChangeEventHandler<SelectionChangeEventArgs> SelectionChanged;

        public PreviewItemStrip(ActivityPreviewDesigner parentDesigner)
        {
            if (parentDesigner == null)
            {
                throw new ArgumentNullException("parentDesigner");
            }
            this.parentDesigner = parentDesigner;
            this.items = new ItemList<System.Workflow.ComponentModel.Design.ItemInfo>(this);
            this.items.ListChanging += new ItemListChangeEventHandler<System.Workflow.ComponentModel.Design.ItemInfo>(this.OnItemsChanging);
            this.items.ListChanged += new ItemListChangeEventHandler<System.Workflow.ComponentModel.Design.ItemInfo>(this.OnItemsChanged);
        }

        public void Draw(Graphics graphics)
        {
            AmbientTheme ambientTheme = WorkflowTheme.CurrentTheme.AmbientTheme;
            ActivityPreviewDesignerTheme designerTheme = this.parentDesigner.DesignerTheme as ActivityPreviewDesignerTheme;
            if (designerTheme != null)
            {
                Rectangle stripRectangle = this.StripRectangle;
                GraphicsPath path = new GraphicsPath();
                if (designerTheme.DesignerGeometry == DesignerGeometry.RoundedRectangle)
                {
                    path.AddPath(ActivityDesignerPaint.GetRoundedRectanglePath(stripRectangle, 4), false);
                }
                else
                {
                    path.AddRectangle(stripRectangle);
                }
                path.CloseFigure();
                graphics.FillPath(designerTheme.PreviewBackgroundBrush, path);
                graphics.DrawPath(designerTheme.PreviewBorderPen, path);
                path.Dispose();
                Image leftScrollImageUp = ActivityPreviewDesignerTheme.LeftScrollImageUp;
                Rectangle buttonBounds = this.GetButtonBounds(ScrollButton.Left);
                if (this.ActiveScrollButton == ScrollButton.Left)
                {
                    leftScrollImageUp = ActivityPreviewDesignerTheme.LeftScrollImage;
                    buttonBounds.Offset(1, 1);
                }
                if (leftScrollImageUp != null)
                {
                    ActivityDesignerPaint.DrawImage(graphics, leftScrollImageUp, buttonBounds, DesignerContentAlignment.Center);
                }
                leftScrollImageUp = ActivityPreviewDesignerTheme.RightScrollImageUp;
                buttonBounds = this.GetButtonBounds(ScrollButton.Right);
                if (this.ActiveScrollButton == ScrollButton.Right)
                {
                    leftScrollImageUp = ActivityPreviewDesignerTheme.RightScrollImage;
                    buttonBounds.Offset(1, 1);
                }
                if (leftScrollImageUp != null)
                {
                    ActivityDesignerPaint.DrawImage(graphics, leftScrollImageUp, buttonBounds, DesignerContentAlignment.Center);
                }
                System.Drawing.Size itemMargin = this.ItemMargin;
                int width = Math.Max(Math.Min((int) (itemMargin.Width / 4), (int) (itemMargin.Height / 2)), 1);
                for (int i = this.scrollMarker; (i < this.items.Count) && (i < (this.scrollMarker + this.VisibleItemCount)); i++)
                {
                    Rectangle itemBounds = this.GetItemBounds(this.items[i]);
                    if (!itemBounds.IsEmpty)
                    {
                        GraphicsPath path2 = new GraphicsPath();
                        if (designerTheme.DesignerGeometry == DesignerGeometry.RoundedRectangle)
                        {
                            path2.AddPath(ActivityDesignerPaint.GetRoundedRectanglePath(itemBounds, 4), true);
                        }
                        else
                        {
                            path2.AddRectangle(itemBounds);
                        }
                        graphics.FillPath(designerTheme.PreviewForegroundBrush, path2);
                        graphics.DrawPath(designerTheme.PreviewBorderPen, path2);
                        path2.Dispose();
                        Image image = this.items[i].Image;
                        if (image == null)
                        {
                            Activity activity = this.items[i].UserData[DesignerUserDataKeys.Activity] as Activity;
                            ActivityDesigner designer = ActivityDesigner.GetDesigner(activity);
                            if (designer != null)
                            {
                                image = designer.Image;
                            }
                        }
                        if (image != null)
                        {
                            Rectangle empty = Rectangle.Empty;
                            empty.X = itemBounds.Left + 2;
                            empty.Y = itemBounds.Top + 2;
                            empty.Size = new System.Drawing.Size(itemBounds.Width - 4, itemBounds.Height - 4);
                            ActivityDesignerPaint.DrawImage(graphics, image, empty, DesignerContentAlignment.Center);
                        }
                        if (i == this.items.IndexOf(this.ActiveItem))
                        {
                            itemBounds.Inflate(width, width);
                            graphics.DrawRectangle(ambientTheme.SelectionForegroundPen, itemBounds);
                        }
                    }
                }
                Rectangle[] dropTargets = this.DropTargets;
                int activeDropTarget = this.ActiveDropTarget;
                if ((activeDropTarget >= 0) && (activeDropTarget < dropTargets.GetLength(0)))
                {
                    dropTargets[activeDropTarget].Width = itemMargin.Width;
                    graphics.DrawLine(ambientTheme.DropIndicatorPen, dropTargets[activeDropTarget].Left + (dropTargets[activeDropTarget].Width / 2), dropTargets[activeDropTarget].Top, dropTargets[activeDropTarget].Left + (dropTargets[activeDropTarget].Width / 2), dropTargets[activeDropTarget].Bottom);
                }
                else if ((this.items.Count == 0) && (this.helpText.Length > 0))
                {
                    stripRectangle.Inflate(-2, -2);
                    Brush textBrush = (this.ActiveDropTarget != -1) ? ambientTheme.DropIndicatorBrush : designerTheme.ForegroundBrush;
                    ActivityDesignerPaint.DrawText(graphics, designerTheme.Font, this.helpText, stripRectangle, StringAlignment.Center, WorkflowTheme.CurrentTheme.AmbientTheme.TextQuality, textBrush);
                }
            }
        }

        private void EnsureScrollMarker()
        {
            if ((this.ActiveItem != null) && (this.VisibleItemCount != 0))
            {
                int num = -1;
                int index = this.items.IndexOf(this.ActiveItem);
                if (index >= 0)
                {
                    num = (index < this.scrollMarker) ? index : ((index >= (this.scrollMarker + this.VisibleItemCount)) ? ((index - this.VisibleItemCount) + 1) : num);
                }
                if ((this.items.Count >= this.VisibleItemCount) && ((this.items.Count - this.scrollMarker) < this.VisibleItemCount))
                {
                    num = this.items.Count - this.VisibleItemCount;
                }
                if ((num >= 0) && (num <= Math.Max((this.items.Count - this.VisibleItemCount) + 1, 0)))
                {
                    this.scrollMarker = num;
                }
                this.Invalidate();
            }
        }

        private Rectangle GetButtonBounds(ScrollButton scrollButton)
        {
            Image leftScrollImage = ActivityPreviewDesignerTheme.LeftScrollImage;
            if ((scrollButton == ScrollButton.Up) || (leftScrollImage == null))
            {
                return Rectangle.Empty;
            }
            System.Drawing.Size size = leftScrollImage.Size;
            size.Height = Math.Min(size.Width, Math.Min(size.Height, this.ItemSize.Height));
            size.Width = Math.Min(size.Width, size.Height);
            int num = (scrollButton == ScrollButton.Left) ? this.bounds.X : (this.bounds.Right - size.Width);
            Rectangle empty = Rectangle.Empty;
            empty.X = num;
            empty.Y = (this.bounds.Y + (this.bounds.Size.Height / 2)) - (size.Height / 2);
            empty.Size = size;
            return empty;
        }

        public Rectangle GetItemBounds(System.Workflow.ComponentModel.Design.ItemInfo itemInfo)
        {
            int index = this.items.IndexOf(itemInfo);
            if (index < 0)
            {
                return Rectangle.Empty;
            }
            if ((index < this.scrollMarker) || (index >= (this.scrollMarker + this.VisibleItemCount)))
            {
                return Rectangle.Empty;
            }
            Rectangle stripRectangle = this.StripRectangle;
            Rectangle empty = Rectangle.Empty;
            System.Drawing.Size itemMargin = this.ItemMargin;
            System.Drawing.Size itemSize = this.ItemSize;
            index -= this.scrollMarker;
            empty.X = (stripRectangle.Left + (index * itemSize.Width)) + ((index + 1) * itemMargin.Width);
            empty.Y = stripRectangle.Top + itemMargin.Height;
            empty.Size = itemSize;
            return empty;
        }

        public System.Workflow.ComponentModel.Design.ItemInfo HitTest(Point point)
        {
            for (int i = this.scrollMarker; i < this.items.Count; i++)
            {
                if (this.GetItemBounds(this.items[i]).Contains(point))
                {
                    return this.items[i];
                }
            }
            return null;
        }

        private void Invalidate()
        {
            if ((this.parentDesigner != null) && (this.parentDesigner.Activity.Site != null))
            {
                WorkflowView service = this.parentDesigner.Activity.Site.GetService(typeof(WorkflowView)) as WorkflowView;
                if (service != null)
                {
                    service.InvalidateLogicalRectangle(this.bounds);
                }
            }
        }

        private void OnItemsChanged(object sender, ItemListChangeEventArgs<System.Workflow.ComponentModel.Design.ItemInfo> e)
        {
            if ((e.Action == ItemListChangeAction.Add) && (e.AddedItems.Count > 0))
            {
                this.ActiveItem = e.AddedItems[0];
            }
            if (e.Action == ItemListChangeAction.Remove)
            {
                this.EnsureScrollMarker();
            }
            this.accessibilityObjects = null;
            this.Invalidate();
        }

        private void OnItemsChanging(object sender, ItemListChangeEventArgs<System.Workflow.ComponentModel.Design.ItemInfo> e)
        {
            if (((e.Action == ItemListChangeAction.Remove) && (e.RemovedItems.Count > 0)) && (this.ActiveItem == e.RemovedItems[0]))
            {
                int index = this.items.IndexOf(e.RemovedItems[0]);
                index += (index < (this.items.Count - 1)) ? 1 : -1;
                this.ActiveItem = ((index >= 0) && (index < this.items.Count)) ? this.items[index] : null;
            }
        }

        public void OnLayoutSize(Graphics graphics)
        {
            ActivityPreviewDesignerTheme designerTheme = this.parentDesigner.DesignerTheme as ActivityPreviewDesignerTheme;
            System.Drawing.Size itemMargin = this.ItemMargin;
            System.Drawing.Size itemSize = this.ItemSize;
            this.bounds.Width = 2 * WorkflowTheme.CurrentTheme.AmbientTheme.Margin.Width;
            this.bounds.Width += itemSize.Width * ((designerTheme != null) ? designerTheme.PreviewItemCount : 0);
            this.bounds.Width += itemMargin.Width * (((designerTheme != null) ? designerTheme.PreviewItemCount : 0) + 1);
            this.bounds.Width += this.GetButtonBounds(ScrollButton.Left).Size.Width;
            this.bounds.Width += this.GetButtonBounds(ScrollButton.Right).Size.Width;
            this.bounds.Height = itemSize.Height + (2 * itemMargin.Height);
            this.EnsureScrollMarker();
        }

        public void OnMouseDown(MouseEventArgs e)
        {
            Point pt = new Point(e.X, e.Y);
            int num = 0;
            if (this.GetButtonBounds(ScrollButton.Left).Contains(pt))
            {
                this.ActiveScrollButton = ScrollButton.Left;
                num = -1;
            }
            else if (this.GetButtonBounds(ScrollButton.Right).Contains(pt))
            {
                this.ActiveScrollButton = ScrollButton.Right;
                num = 1;
            }
            if ((num != 0) && (this.ActiveItem != null))
            {
                int num2 = this.items.IndexOf(this.ActiveItem) + num;
                num2 = (num2 >= this.items.Count) ? 0 : ((num2 < 0) ? (this.items.Count - 1) : num2);
                this.ActiveItem = this.items[num2];
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void OnMouseLeave()
        {
            this.ActiveScrollButton = ScrollButton.Up;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void OnMouseUp(MouseEventArgs e)
        {
            this.ActiveScrollButton = ScrollButton.Up;
        }

        public AccessibleObject[] AccessibilityObjects
        {
            get
            {
                if (this.accessibilityObjects == null)
                {
                    this.accessibilityObjects = new List<ItemStripAccessibleObject>();
                    this.accessibilityObjects.Add(new ItemStripAccessibleObject(ItemStripAccessibleObject.AccessibleObjectType.LeftScroll, this));
                    for (int i = 0; (i < this.VisibleItemCount) && ((this.scrollMarker + i) < this.Items.Count); i++)
                    {
                        this.accessibilityObjects.Add(new ItemStripAccessibleObject(ItemStripAccessibleObject.AccessibleObjectType.Item, this, i));
                    }
                    this.accessibilityObjects.Add(new ItemStripAccessibleObject(ItemStripAccessibleObject.AccessibleObjectType.RightScroll, this));
                }
                return this.accessibilityObjects.ToArray();
            }
        }

        public int ActiveDropTarget
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.activeDropTarget;
            }
            set
            {
                if (this.activeDropTarget != value)
                {
                    this.activeDropTarget = value;
                    this.Invalidate();
                }
            }
        }

        public System.Workflow.ComponentModel.Design.ItemInfo ActiveItem
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.activeItem;
            }
            set
            {
                if (this.activeItem != value)
                {
                    System.Workflow.ComponentModel.Design.ItemInfo activeItem = this.activeItem;
                    this.activeItem = value;
                    this.EnsureScrollMarker();
                    if (this.SelectionChanged != null)
                    {
                        this.SelectionChanged(this, new SelectionChangeEventArgs(activeItem, this.activeItem));
                    }
                }
            }
        }

        private ScrollButton ActiveScrollButton
        {
            get
            {
                return this.activeScrollButton;
            }
            set
            {
                if (this.activeScrollButton != value)
                {
                    this.activeScrollButton = value;
                    this.Invalidate();
                }
            }
        }

        public Rectangle Bounds
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.bounds;
            }
        }

        public Rectangle[] DropTargets
        {
            get
            {
                System.Drawing.Size itemMargin = this.ItemMargin;
                System.Drawing.Size itemSize = this.ItemSize;
                Rectangle stripRectangle = this.StripRectangle;
                Rectangle[] rectangleArray = new Rectangle[this.items.Count + 1];
                int index = 0;
                int num2 = Math.Min(this.items.Count - this.scrollMarker, this.VisibleItemCount) + 1;
                for (int i = 0; i < num2; i++)
                {
                    index = i + this.scrollMarker;
                    rectangleArray[index].X = stripRectangle.Left + (i * (itemSize.Width + itemMargin.Width));
                    rectangleArray[index].Y = stripRectangle.Top + (itemMargin.Height / 2);
                    rectangleArray[index].Size = new System.Drawing.Size(itemMargin.Width, itemSize.Height + itemMargin.Height);
                }
                rectangleArray[index] = new Rectangle(rectangleArray[index].Left, rectangleArray[index].Top, stripRectangle.Right - rectangleArray[index].Left, rectangleArray[index].Height);
                return rectangleArray;
            }
        }

        public string HelpText
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.helpText;
            }
            set
            {
                this.helpText = value;
                if ((this.items.Count == 0) && (this.parentDesigner.Activity != null))
                {
                    this.Invalidate();
                }
            }
        }

        private System.Drawing.Size ItemMargin
        {
            get
            {
                System.Drawing.Size itemSize = this.ItemSize;
                return new System.Drawing.Size(itemSize.Width / 2, itemSize.Height / 4);
            }
        }

        public IList<System.Workflow.ComponentModel.Design.ItemInfo> Items
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.items;
            }
        }

        private System.Drawing.Size ItemSize
        {
            get
            {
                ActivityPreviewDesignerTheme designerTheme = this.parentDesigner.DesignerTheme as ActivityPreviewDesignerTheme;
                if (designerTheme == null)
                {
                    return System.Drawing.Size.Empty;
                }
                return designerTheme.PreviewItemSize;
            }
        }

        public Point Location
        {
            get
            {
                return this.bounds.Location;
            }
            set
            {
                if (this.bounds.Location != value)
                {
                    this.bounds.Location = value;
                }
            }
        }

        public System.Drawing.Size Size
        {
            get
            {
                return this.bounds.Size;
            }
        }

        private Rectangle StripRectangle
        {
            get
            {
                Rectangle empty = Rectangle.Empty;
                Rectangle buttonBounds = this.GetButtonBounds(ScrollButton.Left);
                Rectangle rectangle3 = this.GetButtonBounds(ScrollButton.Right);
                System.Drawing.Size margin = WorkflowTheme.CurrentTheme.AmbientTheme.Margin;
                empty.X = buttonBounds.Right + margin.Width;
                empty.Y = this.bounds.Y;
                empty.Width = (rectangle3.Left - margin.Width) - (buttonBounds.Right + margin.Width);
                empty.Height = this.bounds.Height;
                return empty;
            }
        }

        private int VisibleItemCount
        {
            get
            {
                ActivityPreviewDesignerTheme designerTheme = this.parentDesigner.DesignerTheme as ActivityPreviewDesignerTheme;
                if (designerTheme == null)
                {
                    return 1;
                }
                return designerTheme.PreviewItemCount;
            }
        }

        private sealed class ItemStripAccessibleObject : AccessibleObject
        {
            private AccessibleObjectType accessibleObjectType;
            private int itemIndex;
            private PreviewItemStrip itemStrip;

            internal ItemStripAccessibleObject(AccessibleObjectType type, PreviewItemStrip itemStrip)
            {
                this.itemIndex = -1;
                this.accessibleObjectType = type;
                this.itemStrip = itemStrip;
            }

            internal ItemStripAccessibleObject(AccessibleObjectType type, PreviewItemStrip itemStrip, int itemIndex)
            {
                this.itemIndex = -1;
                this.accessibleObjectType = type;
                this.itemStrip = itemStrip;
                this.itemIndex = itemIndex;
            }

            public override void DoDefaultAction()
            {
                if (this.accessibleObjectType == AccessibleObjectType.Item)
                {
                    ActivityDesigner associatedDesigner = this.AssociatedDesigner;
                    if (associatedDesigner != null)
                    {
                        ISelectionService service = this.GetService(typeof(ISelectionService)) as ISelectionService;
                        if (service != null)
                        {
                            service.SetSelectedComponents(new object[] { associatedDesigner.Activity }, SelectionTypes.Replace);
                        }
                    }
                }
            }

            private object GetService(System.Type serviceType)
            {
                if ((this.itemStrip.parentDesigner.Activity == null) && (this.itemStrip.parentDesigner.Activity.Site == null))
                {
                    return null;
                }
                return this.itemStrip.parentDesigner.Activity.Site.GetService(serviceType);
            }

            public override AccessibleObject Navigate(AccessibleNavigation navdir)
            {
                if ((navdir == AccessibleNavigation.Left) || (navdir == AccessibleNavigation.Right))
                {
                    AccessibleObject[] accessibilityObjects = this.itemStrip.AccessibilityObjects;
                    int num = -1;
                    if (this.accessibleObjectType == AccessibleObjectType.LeftScroll)
                    {
                        num = 0;
                    }
                    else if (this.accessibleObjectType == AccessibleObjectType.RightScroll)
                    {
                        num = accessibilityObjects.Length - 1;
                    }
                    else if (this.accessibleObjectType == AccessibleObjectType.Item)
                    {
                        num = this.itemIndex + ((navdir == AccessibleNavigation.Left) ? -1 : 1);
                    }
                    num = Math.Max(Math.Min(accessibilityObjects.Length - 1, num), 0);
                    return accessibilityObjects[num];
                }
                if (navdir == AccessibleNavigation.Previous)
                {
                    return this.itemStrip.parentDesigner.AccessibilityObject;
                }
                if (navdir != AccessibleNavigation.Next)
                {
                    return base.Navigate(navdir);
                }
                int length = this.itemStrip.AccessibilityObjects.Length;
                if (this.itemStrip.parentDesigner.AccessibilityObject.GetChildCount() > length)
                {
                    return this.itemStrip.parentDesigner.AccessibilityObject.GetChild(length);
                }
                return this.itemStrip.parentDesigner.AccessibilityObject.Navigate(navdir);
            }

            public override void Select(AccessibleSelection flags)
            {
                if (this.accessibleObjectType == AccessibleObjectType.Item)
                {
                    ISelectionService service = this.GetService(typeof(ISelectionService)) as ISelectionService;
                    ActivityDesigner associatedDesigner = this.AssociatedDesigner;
                    if ((service != null) && (associatedDesigner != null))
                    {
                        if (((flags & AccessibleSelection.TakeFocus) > AccessibleSelection.None) || ((flags & AccessibleSelection.TakeSelection) > AccessibleSelection.None))
                        {
                            service.SetSelectedComponents(new object[] { associatedDesigner.Activity }, SelectionTypes.Replace);
                        }
                        else if ((flags & AccessibleSelection.AddSelection) > AccessibleSelection.None)
                        {
                            service.SetSelectedComponents(new object[] { associatedDesigner.Activity }, SelectionTypes.Add);
                        }
                        else if ((flags & AccessibleSelection.RemoveSelection) > AccessibleSelection.None)
                        {
                            service.SetSelectedComponents(new object[] { associatedDesigner.Activity }, SelectionTypes.Remove);
                        }
                    }
                }
                else
                {
                    base.Select(flags);
                }
            }

            private ActivityDesigner AssociatedDesigner
            {
                get
                {
                    if (this.accessibleObjectType == AccessibleObjectType.Item)
                    {
                        int num = this.itemStrip.scrollMarker + this.itemIndex;
                        System.Workflow.ComponentModel.Design.ItemInfo info = ((num >= 0) && (num < this.itemStrip.Items.Count)) ? this.itemStrip.Items[num] : null;
                        if (info != null)
                        {
                            return ActivityDesigner.GetDesigner(info.UserData[DesignerUserDataKeys.Activity] as Activity);
                        }
                    }
                    return null;
                }
            }

            public override Rectangle Bounds
            {
                get
                {
                    Rectangle empty = Rectangle.Empty;
                    if (this.accessibleObjectType == AccessibleObjectType.LeftScroll)
                    {
                        empty = this.itemStrip.GetButtonBounds(ScrollButton.Left);
                    }
                    else if (this.accessibleObjectType == AccessibleObjectType.RightScroll)
                    {
                        empty = this.itemStrip.GetButtonBounds(ScrollButton.Right);
                    }
                    else if (this.accessibleObjectType == AccessibleObjectType.Item)
                    {
                        int num = this.itemStrip.scrollMarker + this.itemIndex;
                        empty = ((num >= 0) && (num < this.itemStrip.Items.Count)) ? this.itemStrip.GetItemBounds(this.itemStrip.Items[num]) : Rectangle.Empty;
                    }
                    if (!empty.IsEmpty)
                    {
                        WorkflowView service = this.itemStrip.parentDesigner.Activity.Site.GetService(typeof(WorkflowView)) as WorkflowView;
                        if (service != null)
                        {
                            empty = new Rectangle(service.LogicalPointToScreen(empty.Location), service.LogicalSizeToClient(empty.Size));
                        }
                    }
                    return empty;
                }
            }

            public override string DefaultAction
            {
                get
                {
                    return DR.GetString("AccessibleAction", new object[0]);
                }
            }

            public override string Description
            {
                get
                {
                    string str = string.Empty;
                    if (this.accessibleObjectType == AccessibleObjectType.LeftScroll)
                    {
                        return DR.GetString("LeftScrollButtonAccessibleDescription", new object[0]);
                    }
                    if (this.accessibleObjectType == AccessibleObjectType.RightScroll)
                    {
                        return DR.GetString("RightScrollButtonAccessibleDescription", new object[0]);
                    }
                    if (this.accessibleObjectType == AccessibleObjectType.Item)
                    {
                        ActivityDesigner associatedDesigner = this.AssociatedDesigner;
                        if (associatedDesigner != null)
                        {
                            str = DR.GetString("ActivityDesignerAccessibleDescription", new object[] { associatedDesigner.Activity.GetType().Name });
                        }
                    }
                    return str;
                }
            }

            public override string Help
            {
                get
                {
                    string str = string.Empty;
                    if (this.accessibleObjectType == AccessibleObjectType.LeftScroll)
                    {
                        return DR.GetString("LeftScrollButtonAccessibleHelp", new object[0]);
                    }
                    if (this.accessibleObjectType == AccessibleObjectType.RightScroll)
                    {
                        return DR.GetString("RightScrollButtonAccessibleHelp", new object[0]);
                    }
                    if (this.accessibleObjectType == AccessibleObjectType.Item)
                    {
                        ActivityDesigner associatedDesigner = this.AssociatedDesigner;
                        if (associatedDesigner != null)
                        {
                            str = DR.GetString("ActivityDesignerAccessibleHelp", new object[] { associatedDesigner.Activity.GetType().Name });
                        }
                    }
                    return str;
                }
            }

            public override string Name
            {
                get
                {
                    string str = string.Empty;
                    if (this.accessibleObjectType == AccessibleObjectType.LeftScroll)
                    {
                        return DR.GetString("LeftScrollButtonName", new object[0]);
                    }
                    if (this.accessibleObjectType == AccessibleObjectType.RightScroll)
                    {
                        return DR.GetString("RightScrollButtonName", new object[0]);
                    }
                    if (this.accessibleObjectType == AccessibleObjectType.Item)
                    {
                        ActivityDesigner associatedDesigner = this.AssociatedDesigner;
                        if (associatedDesigner != null)
                        {
                            Activity activity = associatedDesigner.Activity;
                            str = (activity != null) ? activity.QualifiedName : base.Name;
                        }
                    }
                    return str;
                }
                set
                {
                }
            }

            public override AccessibleObject Parent
            {
                get
                {
                    return this.itemStrip.parentDesigner.AccessibilityObject;
                }
            }

            public override AccessibleRole Role
            {
                get
                {
                    return AccessibleRole.Diagram;
                }
            }

            public override AccessibleStates State
            {
                get
                {
                    AccessibleStates none = AccessibleStates.None;
                    if (this.accessibleObjectType == AccessibleObjectType.Item)
                    {
                        ActivityDesigner associatedDesigner = this.AssociatedDesigner;
                        if ((this.GetService(typeof(ISelectionService)) is ISelectionService) && (associatedDesigner != null))
                        {
                            none = associatedDesigner.IsSelected ? AccessibleStates.Selected : AccessibleStates.Selectable;
                            none |= AccessibleStates.MultiSelectable;
                            none |= associatedDesigner.IsLocked ? AccessibleStates.ReadOnly : AccessibleStates.Moveable;
                            none |= associatedDesigner.IsPrimarySelection ? AccessibleStates.Focused : AccessibleStates.Focusable;
                        }
                    }
                    return none;
                }
            }

            internal enum AccessibleObjectType
            {
                Item = 2,
                LeftScroll = 1,
                RightScroll = 3
            }
        }
    }
}

