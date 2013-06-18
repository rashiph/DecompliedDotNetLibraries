namespace System.Windows.Forms
{
    using System;
    using System.Collections;
    using System.Drawing;
    using System.Globalization;
    using System.Windows.Forms.Layout;

    internal class ToolStripPanelCell : ArrangedElement
    {
        private ToolStrip _wrappedToolStrip;
        private Rectangle cachedBounds;
        private bool currentlyDragging;
        private bool currentlySizing;
        private Size maxSize;
        private System.Windows.Forms.ToolStripPanelRow parent;
        private bool restoreOnVisibleChanged;

        public ToolStripPanelCell(System.Windows.Forms.Control control) : this(null, control)
        {
        }

        public ToolStripPanelCell(System.Windows.Forms.ToolStripPanelRow parent, System.Windows.Forms.Control control)
        {
            this.maxSize = LayoutUtils.MaxSize;
            this.cachedBounds = Rectangle.Empty;
            this.ToolStripPanelRow = parent;
            this._wrappedToolStrip = control as ToolStrip;
            if (control == null)
            {
                throw new ArgumentNullException("control");
            }
            if (this._wrappedToolStrip == null)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, System.Windows.Forms.SR.GetString("TypedControlCollectionShouldBeOfType", new object[] { typeof(ToolStrip).Name }), new object[0]), control.GetType().Name);
            }
            CommonProperties.SetAutoSize(this, true);
            this._wrappedToolStrip.LocationChanging += new ToolStripLocationCancelEventHandler(this.OnToolStripLocationChanging);
            this._wrappedToolStrip.VisibleChanged += new EventHandler(this.OnToolStripVisibleChanged);
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    if (this._wrappedToolStrip != null)
                    {
                        this._wrappedToolStrip.LocationChanging -= new ToolStripLocationCancelEventHandler(this.OnToolStripLocationChanging);
                        this._wrappedToolStrip.VisibleChanged -= new EventHandler(this.OnToolStripVisibleChanged);
                    }
                    this._wrappedToolStrip = null;
                    if (this.parent != null)
                    {
                        ((IList) this.parent.Cells).Remove(this);
                    }
                    this.parent = null;
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        protected override ArrangedElementCollection GetChildren()
        {
            return ArrangedElementCollection.Empty;
        }

        protected override IArrangedElement GetContainer()
        {
            return this.parent;
        }

        public override Size GetPreferredSize(Size constrainingSize)
        {
            ISupportToolStripPanel draggedControl = this.DraggedControl;
            Size empty = Size.Empty;
            if (draggedControl.Stretch)
            {
                if (this.ToolStripPanelRow.Orientation == Orientation.Horizontal)
                {
                    constrainingSize.Width = this.ToolStripPanelRow.Bounds.Width;
                    empty = this._wrappedToolStrip.GetPreferredSize(constrainingSize);
                    empty.Width = constrainingSize.Width;
                    return empty;
                }
                constrainingSize.Height = this.ToolStripPanelRow.Bounds.Height;
                empty = this._wrappedToolStrip.GetPreferredSize(constrainingSize);
                empty.Height = constrainingSize.Height;
                return empty;
            }
            return (!this._wrappedToolStrip.AutoSize ? this._wrappedToolStrip.Size : this._wrappedToolStrip.GetPreferredSize(constrainingSize));
        }

        public int Grow(int growBy)
        {
            if (this.ToolStripPanelRow.Orientation == Orientation.Vertical)
            {
                return this.GrowVertical(growBy);
            }
            return this.GrowHorizontal(growBy);
        }

        private int GrowHorizontal(int growBy)
        {
            if (this.MaximumSize.Width < this.Control.PreferredSize.Width)
            {
                if ((this.MaximumSize.Width + growBy) >= this.Control.PreferredSize.Width)
                {
                    int num = this.Control.PreferredSize.Width - this.MaximumSize.Width;
                    this.maxSize = LayoutUtils.MaxSize;
                    return num;
                }
                if ((this.MaximumSize.Width + growBy) < this.Control.PreferredSize.Width)
                {
                    this.maxSize.Width += growBy;
                    return growBy;
                }
            }
            return 0;
        }

        private int GrowVertical(int growBy)
        {
            if (this.MaximumSize.Height < this.Control.PreferredSize.Height)
            {
                if ((this.MaximumSize.Height + growBy) >= this.Control.PreferredSize.Height)
                {
                    int num = this.Control.PreferredSize.Height - this.MaximumSize.Height;
                    this.maxSize = LayoutUtils.MaxSize;
                    return num;
                }
                if ((this.MaximumSize.Height + growBy) < this.Control.PreferredSize.Height)
                {
                    this.maxSize.Height += growBy;
                    return growBy;
                }
            }
            return 0;
        }

        private void OnToolStripLocationChanging(object sender, ToolStripLocationCancelEventArgs e)
        {
            if ((this.ToolStripPanelRow != null) && (!this.currentlySizing && !this.currentlyDragging))
            {
                try
                {
                    this.currentlyDragging = true;
                    Point newLocation = e.NewLocation;
                    if ((this.ToolStripPanelRow != null) && (this.ToolStripPanelRow.Bounds == Rectangle.Empty))
                    {
                        this.ToolStripPanelRow.ToolStripPanel.PerformUpdate(true);
                    }
                    if (this._wrappedToolStrip != null)
                    {
                        this.ToolStripPanelRow.ToolStripPanel.Join(this._wrappedToolStrip, newLocation);
                    }
                }
                finally
                {
                    this.currentlyDragging = false;
                    e.Cancel = true;
                }
            }
        }

        private void OnToolStripVisibleChanged(object sender, EventArgs e)
        {
            if ((((this._wrappedToolStrip != null) && !this._wrappedToolStrip.IsInDesignMode) && (!this._wrappedToolStrip.IsCurrentlyDragging && !this._wrappedToolStrip.IsDisposed)) && !this._wrappedToolStrip.Disposing)
            {
                if (!this.Control.Visible)
                {
                    this.restoreOnVisibleChanged = (this.ToolStripPanelRow != null) && ((IList) this.ToolStripPanelRow.Cells).Contains(this);
                }
                else if (this.restoreOnVisibleChanged)
                {
                    try
                    {
                        if ((this.ToolStripPanelRow != null) && ((IList) this.ToolStripPanelRow.Cells).Contains(this))
                        {
                            this.ToolStripPanelRow.ToolStripPanel.Join(this._wrappedToolStrip, this._wrappedToolStrip.Location);
                        }
                    }
                    finally
                    {
                        this.restoreOnVisibleChanged = false;
                    }
                }
            }
        }

        protected override void SetBoundsCore(Rectangle bounds, BoundsSpecified specified)
        {
            this.currentlySizing = true;
            this.CachedBounds = bounds;
            try
            {
                if (this.DraggedControl.IsCurrentlyDragging)
                {
                    if (this.ToolStripPanelRow.Cells[this.ToolStripPanelRow.Cells.Count - 1] == this)
                    {
                        Rectangle displayRectangle = this.ToolStripPanelRow.DisplayRectangle;
                        if (this.ToolStripPanelRow.Orientation == Orientation.Horizontal)
                        {
                            int num = bounds.Right - displayRectangle.Right;
                            if ((num > 0) && (bounds.Width > num))
                            {
                                bounds.Width -= num;
                            }
                        }
                        else
                        {
                            int num2 = bounds.Bottom - displayRectangle.Bottom;
                            if ((num2 > 0) && (bounds.Height > num2))
                            {
                                bounds.Height -= num2;
                            }
                        }
                    }
                    base.SetBoundsCore(bounds, specified);
                    this.InnerElement.SetBounds(bounds, specified);
                }
                else if (!this.ToolStripPanelRow.CachedBoundsMode)
                {
                    base.SetBoundsCore(bounds, specified);
                    this.InnerElement.SetBounds(bounds, specified);
                }
            }
            finally
            {
                this.currentlySizing = false;
            }
        }

        public int Shrink(int shrinkBy)
        {
            if (this.ToolStripPanelRow.Orientation == Orientation.Vertical)
            {
                return this.ShrinkVertical(shrinkBy);
            }
            return this.ShrinkHorizontal(shrinkBy);
        }

        private int ShrinkHorizontal(int shrinkBy)
        {
            return 0;
        }

        private int ShrinkVertical(int shrinkBy)
        {
            return 0;
        }

        public Rectangle CachedBounds
        {
            get
            {
                return this.cachedBounds;
            }
            set
            {
                this.cachedBounds = value;
            }
        }

        public System.Windows.Forms.Control Control
        {
            get
            {
                return this._wrappedToolStrip;
            }
        }

        public bool ControlInDesignMode
        {
            get
            {
                return ((this._wrappedToolStrip != null) && this._wrappedToolStrip.IsInDesignMode);
            }
        }

        public ISupportToolStripPanel DraggedControl
        {
            get
            {
                return this._wrappedToolStrip;
            }
        }

        public IArrangedElement InnerElement
        {
            get
            {
                return this._wrappedToolStrip;
            }
        }

        public override System.Windows.Forms.Layout.LayoutEngine LayoutEngine
        {
            get
            {
                return DefaultLayout.Instance;
            }
        }

        public Size MaximumSize
        {
            get
            {
                return this.maxSize;
            }
        }

        public System.Windows.Forms.ToolStripPanelRow ToolStripPanelRow
        {
            get
            {
                return this.parent;
            }
            set
            {
                if (this.parent != value)
                {
                    if (this.parent != null)
                    {
                        ((IList) this.parent.Cells).Remove(this);
                    }
                    this.parent = value;
                    base.Margin = Padding.Empty;
                }
            }
        }

        public override bool Visible
        {
            get
            {
                return (((this.Control != null) && (this.Control.ParentInternal == this.ToolStripPanelRow.ToolStripPanel)) && this.InnerElement.ParticipatesInLayout);
            }
            set
            {
                this.Control.Visible = value;
            }
        }
    }
}

