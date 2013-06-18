namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Runtime;
    using System.Threading;
    using System.Windows.Forms;
    using System.Workflow.ComponentModel;

    internal abstract class ItemStrip
    {
        private Rectangle bounds = Rectangle.Empty;
        private System.Workflow.ComponentModel.Design.ItemInfo highlitedItem;
        private ItemList<System.Workflow.ComponentModel.Design.ItemInfo> items;
        private System.Drawing.Size itemSize = new System.Drawing.Size(0x10, 0x10);
        private System.Drawing.Size margin = System.Drawing.Size.Empty;
        private Orientation orientation;
        private int scrollPosition;
        private System.Workflow.ComponentModel.Design.ItemInfo selectedItem;
        protected IServiceProvider serviceProvider;

        public event EventHandler ScrollPositionChanged;

        public event SelectionChangeEventHandler<SelectionChangeEventArgs> SelectionChanged;

        public ItemStrip(IServiceProvider serviceProvider, Orientation orientation, System.Drawing.Size itemSize, System.Drawing.Size margin)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException("serviceProvider");
            }
            this.serviceProvider = serviceProvider;
            this.orientation = orientation;
            this.itemSize = itemSize;
            this.margin = margin;
            this.items = new ItemList<System.Workflow.ComponentModel.Design.ItemInfo>(this);
            this.items.ListChanging += new ItemListChangeEventHandler<System.Workflow.ComponentModel.Design.ItemInfo>(this.OnItemsChanging);
            this.items.ListChanged += new ItemListChangeEventHandler<System.Workflow.ComponentModel.Design.ItemInfo>(this.OnItemsChanged);
        }

        public abstract void Draw(Graphics graphics);
        private void EnsureScrollPositionAndSelection()
        {
            int scrollPosition = this.scrollPosition;
            if (this.selectedItem != null)
            {
                int index = this.items.IndexOf(this.selectedItem);
                if (index >= 0)
                {
                    if (index <= this.scrollPosition)
                    {
                        scrollPosition = Math.Max(index - 1, 0);
                    }
                    int maxVisibleItems = this.MaxVisibleItems;
                    if (index >= ((this.scrollPosition + maxVisibleItems) - 1))
                    {
                        scrollPosition = (index - maxVisibleItems) + 2;
                    }
                }
            }
            this.ScrollPosition = scrollPosition;
        }

        public Rectangle GetItemBounds(System.Workflow.ComponentModel.Design.ItemInfo itemInfo)
        {
            int index = this.items.IndexOf(itemInfo);
            if (((index < 0) || (index < this.scrollPosition)) || (index >= (this.scrollPosition + this.MaxVisibleItems)))
            {
                return Rectangle.Empty;
            }
            Rectangle empty = Rectangle.Empty;
            index -= this.scrollPosition;
            if (this.orientation == Orientation.Horizontal)
            {
                empty.X = (this.bounds.Left + (index * this.itemSize.Width)) + ((index + 1) * this.margin.Width);
                empty.Y = this.bounds.Top + this.margin.Height;
            }
            else
            {
                empty.X = this.bounds.Left + this.margin.Width;
                empty.Y = (this.bounds.Top + (index * this.itemSize.Height)) + ((index + 1) * this.margin.Height);
            }
            empty.Size = this.itemSize;
            return empty;
        }

        public System.Workflow.ComponentModel.Design.ItemInfo HitTest(Point point)
        {
            foreach (System.Workflow.ComponentModel.Design.ItemInfo info2 in this.items)
            {
                if (this.GetItemBounds(info2).Contains(point))
                {
                    return info2;
                }
            }
            return null;
        }

        protected void Invalidate()
        {
            WorkflowView service = this.serviceProvider.GetService(typeof(WorkflowView)) as WorkflowView;
            if (service != null)
            {
                service.InvalidateLogicalRectangle(this.bounds);
            }
        }

        private void OnItemsChanged(object sender, ItemListChangeEventArgs<System.Workflow.ComponentModel.Design.ItemInfo> e)
        {
            if (e.Action == ItemListChangeAction.Add)
            {
                if (e.AddedItems.Count > 0)
                {
                    this.SelectedItem = e.AddedItems[0];
                }
            }
            else if (e.Action == ItemListChangeAction.Remove)
            {
                this.EnsureScrollPositionAndSelection();
            }
            this.Invalidate();
        }

        private void OnItemsChanging(object sender, ItemListChangeEventArgs<System.Workflow.ComponentModel.Design.ItemInfo> e)
        {
            if (((e.Action == ItemListChangeAction.Remove) && (e.RemovedItems.Count > 0)) && (this.selectedItem == e.RemovedItems[0]))
            {
                int index = this.items.IndexOf(e.RemovedItems[0]);
                index += (index < (this.items.Count - 1)) ? 1 : -1;
                this.SelectedItem = ((index >= 0) && (index < this.items.Count)) ? this.items[index] : null;
            }
        }

        public virtual void OnMouseDown(MouseEventArgs e)
        {
            System.Workflow.ComponentModel.Design.ItemInfo info = this.HitTest(new Point(e.X, e.Y));
            if (info != null)
            {
                this.SelectedItem = info;
                if ((info.Text != null) && (info.Text.Length > 0))
                {
                    this.ShowInfoTip(info.Text);
                }
            }
            this.HighlitedItem = info;
        }

        public virtual void OnMouseDragBegin(Point initialDragPoint, MouseEventArgs e)
        {
        }

        public virtual void OnMouseDragEnd()
        {
        }

        public virtual void OnMouseDragMove(MouseEventArgs e)
        {
        }

        public virtual void OnMouseEnter(MouseEventArgs e)
        {
            System.Workflow.ComponentModel.Design.ItemInfo info = this.HitTest(new Point(e.X, e.Y));
            if (((info != null) && (info.Text != null)) && (info.Text.Length > 0))
            {
                this.ShowInfoTip(info.Text);
            }
            this.HighlitedItem = info;
        }

        public virtual void OnMouseLeave()
        {
            this.ShowInfoTip(string.Empty);
            this.HighlitedItem = null;
        }

        public virtual void OnMouseMove(MouseEventArgs e)
        {
            System.Workflow.ComponentModel.Design.ItemInfo info = this.HitTest(new Point(e.X, e.Y));
            if (((info != null) && (info.Text != null)) && (info.Text.Length > 0))
            {
                this.ShowInfoTip(info.Text);
            }
            this.HighlitedItem = info;
        }

        public virtual void OnMouseUp(MouseEventArgs e)
        {
        }

        private void ShowInfoTip(string infoTip)
        {
            WorkflowView service = this.serviceProvider.GetService(typeof(WorkflowView)) as WorkflowView;
            if (service != null)
            {
                service.ShowInfoTip(string.Empty, infoTip);
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

        protected System.Workflow.ComponentModel.Design.ItemInfo HighlitedItem
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.highlitedItem;
            }
            private set
            {
                if (this.highlitedItem != value)
                {
                    this.highlitedItem = value;
                    this.Invalidate();
                }
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

        public System.Drawing.Size ItemSize
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.itemSize;
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
                    this.Invalidate();
                    this.bounds.Location = value;
                    this.Invalidate();
                }
            }
        }

        protected internal int MaxVisibleItems
        {
            get
            {
                int num = 0;
                if (this.orientation == Orientation.Horizontal)
                {
                    int num2 = this.bounds.Width - this.margin.Width;
                    num = num2 / Math.Max(this.itemSize.Width + this.margin.Width, 1);
                }
                else
                {
                    int num3 = this.bounds.Height - this.margin.Height;
                    num = num3 / Math.Max(this.itemSize.Height + this.margin.Height, 1);
                }
                return Math.Max(num, 1);
            }
        }

        public System.Drawing.Size RequiredSize
        {
            get
            {
                System.Drawing.Size empty = System.Drawing.Size.Empty;
                if (this.orientation == Orientation.Horizontal)
                {
                    empty.Width = (this.items.Count * this.itemSize.Width) + ((this.items.Count + 1) * this.margin.Width);
                    empty.Height = this.itemSize.Height + (2 * this.margin.Height);
                    return empty;
                }
                empty.Width = this.itemSize.Width + (2 * this.margin.Width);
                empty.Height = (this.items.Count * this.itemSize.Height) + ((this.items.Count + 1) * this.margin.Height);
                return empty;
            }
        }

        public int ScrollPosition
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.scrollPosition;
            }
            set
            {
                if (value >= 0)
                {
                    int num = value;
                    int maxVisibleItems = this.MaxVisibleItems;
                    if ((this.items.Count >= maxVisibleItems) && ((this.items.Count - num) < maxVisibleItems))
                    {
                        num = this.items.Count - maxVisibleItems;
                    }
                    if ((num >= 0) && (num <= Math.Max((this.items.Count - maxVisibleItems) + 1, 0)))
                    {
                        this.scrollPosition = num;
                        this.Invalidate();
                        if (this.ScrollPositionChanged != null)
                        {
                            this.ScrollPositionChanged(this, EventArgs.Empty);
                        }
                    }
                }
            }
        }

        public System.Workflow.ComponentModel.Design.ItemInfo SelectedItem
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.selectedItem;
            }
            set
            {
                if (this.selectedItem != value)
                {
                    System.Workflow.ComponentModel.Design.ItemInfo selectedItem = this.selectedItem;
                    this.selectedItem = value;
                    this.EnsureScrollPositionAndSelection();
                    this.Invalidate();
                    if (this.SelectionChanged != null)
                    {
                        this.SelectionChanged(this, new SelectionChangeEventArgs(selectedItem, this.selectedItem));
                    }
                }
            }
        }

        public System.Drawing.Size Size
        {
            get
            {
                return this.bounds.Size;
            }
            set
            {
                if (this.bounds.Size != value)
                {
                    this.Invalidate();
                    this.bounds.Size = value;
                    this.EnsureScrollPositionAndSelection();
                    this.Invalidate();
                }
            }
        }
    }
}

