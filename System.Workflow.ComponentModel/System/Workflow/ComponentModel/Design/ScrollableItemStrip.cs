namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Runtime;
    using System.Windows.Forms;

    internal abstract class ScrollableItemStrip
    {
        private ScrollButton activeButton;
        private Rectangle bounds = Rectangle.Empty;
        private System.Drawing.Size buttonSize;
        protected ItemStrip itemStrip;
        private System.Drawing.Size margin;
        private System.Windows.Forms.Orientation orientation;
        protected IServiceProvider serviceProvider;

        public event SelectionChangeEventHandler<SelectionChangeEventArgs> SelectionChanged
        {
            add
            {
                this.itemStrip.SelectionChanged += value;
            }
            remove
            {
                this.itemStrip.SelectionChanged -= value;
            }
        }

        public ScrollableItemStrip(IServiceProvider serviceProvider, System.Windows.Forms.Orientation orientation, System.Drawing.Size itemSize, System.Drawing.Size margin)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException("serviceProvider");
            }
            this.serviceProvider = serviceProvider;
            this.orientation = orientation;
            this.margin = margin;
            if (orientation == System.Windows.Forms.Orientation.Horizontal)
            {
                this.buttonSize = new System.Drawing.Size((itemSize.Width * 2) / 3, itemSize.Height);
            }
            else
            {
                this.buttonSize = new System.Drawing.Size(itemSize.Width, (itemSize.Height * 2) / 3);
            }
            this.itemStrip = this.CreateItemStrip(serviceProvider, orientation, itemSize, margin);
            this.itemStrip.ScrollPositionChanged += new EventHandler(this.OnScrollPositionChanged);
        }

        protected abstract ItemStrip CreateItemStrip(IServiceProvider serviceProvider, System.Windows.Forms.Orientation orientation, System.Drawing.Size itemSize, System.Drawing.Size margin);
        public abstract void Draw(Graphics graphics);
        protected Rectangle GetButtonBounds(ScrollButton scrollButton)
        {
            Rectangle empty = Rectangle.Empty;
            empty.Size = this.buttonSize;
            if ((scrollButton == ScrollButton.Left) || (scrollButton == ScrollButton.Up))
            {
                empty.X = this.bounds.X + this.margin.Width;
                empty.Y = this.bounds.Y + this.margin.Height;
                return empty;
            }
            if ((scrollButton == ScrollButton.Right) || (scrollButton == ScrollButton.Down))
            {
                if (this.orientation == System.Windows.Forms.Orientation.Horizontal)
                {
                    empty.X = ((this.bounds.X + this.margin.Width) + empty.Size.Width) + this.itemStrip.Size.Width;
                    if (empty.X >= this.bounds.Right)
                    {
                        empty.X = this.bounds.Right - empty.Size.Width;
                    }
                    empty.Y = this.bounds.Y + this.margin.Height;
                    return empty;
                }
                empty.X = this.bounds.X + this.margin.Width;
                empty.Y = ((this.bounds.Y + this.margin.Height) + empty.Size.Height) + this.itemStrip.Size.Height;
                if (empty.Y >= this.bounds.Bottom)
                {
                    empty.Y = this.bounds.Bottom - empty.Size.Height;
                }
            }
            return empty;
        }

        protected ScrollButton HitTest(Point mousePoint)
        {
            if (this.itemStrip.ScrollPosition > 0)
            {
                ScrollButton scrollButton = (this.orientation == System.Windows.Forms.Orientation.Horizontal) ? ScrollButton.Left : ScrollButton.Up;
                if (this.GetButtonBounds(scrollButton).Contains(mousePoint))
                {
                    return scrollButton;
                }
            }
            if ((this.itemStrip.ScrollPosition + this.itemStrip.MaxVisibleItems) < this.itemStrip.Items.Count)
            {
                ScrollButton button2 = (this.orientation == System.Windows.Forms.Orientation.Horizontal) ? ScrollButton.Right : ScrollButton.Down;
                if (this.GetButtonBounds(button2).Contains(mousePoint))
                {
                    return button2;
                }
            }
            return ScrollButton.Up;
        }

        protected void Invalidate()
        {
            WorkflowView service = this.serviceProvider.GetService(typeof(WorkflowView)) as WorkflowView;
            if (service != null)
            {
                service.InvalidateLogicalRectangle(this.bounds);
            }
        }

        public virtual void OnMouseDown(MouseEventArgs e)
        {
            Point pt = new Point(e.X, e.Y);
            if (this.itemStrip.Bounds.Contains(pt))
            {
                this.itemStrip.OnMouseDown(e);
            }
            else
            {
                ScrollButton button = this.HitTest(pt);
                if (button != ScrollButton.Up)
                {
                    int num = ((button == ScrollButton.Left) || (button == ScrollButton.Up)) ? -1 : 1;
                    this.itemStrip.ScrollPosition += num;
                }
                if (e.Button == MouseButtons.Left)
                {
                    this.ActiveButton = button;
                }
            }
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
            this.itemStrip.OnMouseEnter(e);
        }

        public virtual void OnMouseLeave()
        {
            this.itemStrip.OnMouseLeave();
            this.ActiveButton = ScrollButton.Up;
        }

        public virtual void OnMouseMove(MouseEventArgs e)
        {
            this.itemStrip.OnMouseMove(e);
            if (e.Button == MouseButtons.Left)
            {
                this.ActiveButton = this.HitTest(new Point(e.X, e.Y));
            }
        }

        public virtual void OnMouseUp(MouseEventArgs e)
        {
            Point pt = new Point(e.X, e.Y);
            if (this.itemStrip.Bounds.Contains(pt))
            {
                this.itemStrip.OnMouseUp(e);
            }
            this.ActiveButton = ScrollButton.Up;
        }

        private void OnScrollPositionChanged(object sender, EventArgs e)
        {
            this.Invalidate();
        }

        protected ScrollButton ActiveButton
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.activeButton;
            }
            private set
            {
                if (this.activeButton != value)
                {
                    this.activeButton = value;
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

        public IList<System.Workflow.ComponentModel.Design.ItemInfo> Items
        {
            get
            {
                return this.itemStrip.Items;
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
                    Rectangle buttonBounds = this.GetButtonBounds(ScrollButton.Left);
                    if (this.orientation == System.Windows.Forms.Orientation.Horizontal)
                    {
                        this.itemStrip.Location = new Point(buttonBounds.Right, buttonBounds.Top);
                    }
                    else
                    {
                        this.itemStrip.Location = new Point(buttonBounds.Left, buttonBounds.Bottom);
                    }
                    this.Invalidate();
                }
            }
        }

        public System.Windows.Forms.Orientation Orientation
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.orientation;
            }
        }

        public System.Workflow.ComponentModel.Design.ItemInfo SelectedItem
        {
            get
            {
                return this.itemStrip.SelectedItem;
            }
            set
            {
                this.itemStrip.SelectedItem = value;
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
                    System.Drawing.Size requiredSize = this.itemStrip.RequiredSize;
                    int num = 0;
                    if (this.orientation == System.Windows.Forms.Orientation.Horizontal)
                    {
                        num = this.bounds.Width - (2 * ((2 * this.margin.Width) + this.buttonSize.Width));
                        num -= this.margin.Width;
                        if ((this.margin.Width + this.itemStrip.ItemSize.Width) > 0)
                        {
                            num -= num % (this.margin.Width + this.itemStrip.ItemSize.Width);
                        }
                        this.itemStrip.Size = new System.Drawing.Size(Math.Min(num, requiredSize.Width), Math.Min(this.bounds.Height, requiredSize.Height));
                    }
                    else
                    {
                        num = this.bounds.Height - (2 * ((2 * this.margin.Height) + this.buttonSize.Height));
                        num -= this.margin.Height;
                        if ((this.margin.Height + this.itemStrip.ItemSize.Height) > 0)
                        {
                            num -= num % (this.margin.Height + this.itemStrip.ItemSize.Height);
                        }
                        this.itemStrip.Size = new System.Drawing.Size(Math.Min(this.bounds.Width, requiredSize.Width), Math.Min(num, requiredSize.Height));
                    }
                    this.Invalidate();
                }
            }
        }
    }
}

