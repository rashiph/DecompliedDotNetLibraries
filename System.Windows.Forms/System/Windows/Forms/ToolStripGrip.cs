namespace System.Windows.Forms
{
    using System;
    using System.Drawing;
    using System.Windows.Forms.Layout;

    internal class ToolStripGrip : ToolStripButton
    {
        private static Size DragSize = LayoutUtils.MaxSize;
        private int gripThickness = (ToolStripManager.VisualStylesEnabled ? 5 : 3);
        private Point lastEndLocation = ToolStrip.InvalidMouseEnter;
        private bool movingToolStrip;
        private Cursor oldCursor;
        private Point startLocation = Point.Empty;

        internal ToolStripGrip()
        {
            base.SupportsItemClick = false;
        }

        protected override AccessibleObject CreateAccessibilityInstance()
        {
            return new ToolStripGripAccessibleObject(this);
        }

        public override Size GetPreferredSize(Size constrainingSize)
        {
            Size empty = Size.Empty;
            if (base.ParentInternal != null)
            {
                if (base.ParentInternal.LayoutStyle == ToolStripLayoutStyle.VerticalStackWithOverflow)
                {
                    empty = new Size(base.ParentInternal.Width, this.gripThickness);
                }
                else
                {
                    empty = new Size(this.gripThickness, base.ParentInternal.Height);
                }
            }
            if (empty.Width > constrainingSize.Width)
            {
                empty.Width = constrainingSize.Width;
            }
            if (empty.Height > constrainingSize.Height)
            {
                empty.Height = constrainingSize.Height;
            }
            return empty;
        }

        private bool LeftMouseButtonIsDown()
        {
            return ((Control.MouseButtons == MouseButtons.Left) && (Control.ModifierKeys == Keys.None));
        }

        protected override void OnMouseDown(MouseEventArgs mea)
        {
            this.startLocation = base.TranslatePoint(new Point(mea.X, mea.Y), ToolStripPointType.ToolStripItemCoords, ToolStripPointType.ScreenCoords);
            base.OnMouseDown(mea);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            if (((base.ParentInternal != null) && (this.ToolStripPanelRow != null)) && !base.ParentInternal.IsInDesignMode)
            {
                this.oldCursor = base.ParentInternal.Cursor;
                SetCursor(base.ParentInternal, Cursors.SizeAll);
            }
            else
            {
                this.oldCursor = null;
            }
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            if ((this.oldCursor != null) && !base.ParentInternal.IsInDesignMode)
            {
                SetCursor(base.ParentInternal, this.oldCursor);
            }
            if (!this.MovingToolStrip && this.LeftMouseButtonIsDown())
            {
                this.MovingToolStrip = true;
            }
            base.OnMouseLeave(e);
        }

        protected override void OnMouseMove(MouseEventArgs mea)
        {
            bool flag = this.LeftMouseButtonIsDown();
            if (!this.MovingToolStrip && flag)
            {
                Point point = base.TranslatePoint(mea.Location, ToolStripPointType.ToolStripItemCoords, ToolStripPointType.ScreenCoords);
                int num = point.X - this.startLocation.X;
                num = (num < 0) ? (num * -1) : num;
                if (DragSize == LayoutUtils.MaxSize)
                {
                    DragSize = SystemInformation.DragSize;
                }
                if (num >= DragSize.Width)
                {
                    this.MovingToolStrip = true;
                }
                else
                {
                    int num2 = point.Y - this.startLocation.Y;
                    num2 = (num2 < 0) ? (num2 * -1) : num2;
                    if (num2 >= DragSize.Height)
                    {
                        this.MovingToolStrip = true;
                    }
                }
            }
            if (this.MovingToolStrip)
            {
                if (flag)
                {
                    Point screenLocation = base.TranslatePoint(new Point(mea.X, mea.Y), ToolStripPointType.ToolStripItemCoords, ToolStripPointType.ScreenCoords);
                    if (screenLocation != this.lastEndLocation)
                    {
                        this.ToolStripPanelRow.ToolStripPanel.MoveControl(base.ParentInternal, screenLocation);
                        this.lastEndLocation = screenLocation;
                    }
                    this.startLocation = screenLocation;
                }
                else
                {
                    this.MovingToolStrip = false;
                }
            }
            base.OnMouseMove(mea);
        }

        protected override void OnMouseUp(MouseEventArgs mea)
        {
            if (this.MovingToolStrip)
            {
                Point screenLocation = base.TranslatePoint(new Point(mea.X, mea.Y), ToolStripPointType.ToolStripItemCoords, ToolStripPointType.ScreenCoords);
                this.ToolStripPanelRow.ToolStripPanel.MoveControl(base.ParentInternal, screenLocation);
            }
            if (!base.ParentInternal.IsInDesignMode)
            {
                SetCursor(base.ParentInternal, this.oldCursor);
            }
            ToolStripPanel.ClearDragFeedback();
            this.MovingToolStrip = false;
            base.OnMouseUp(mea);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (base.ParentInternal != null)
            {
                base.ParentInternal.OnPaintGrip(e);
            }
        }

        private static void SetCursor(Control control, Cursor cursor)
        {
            System.Windows.Forms.IntSecurity.ModifyCursor.Assert();
            control.Cursor = cursor;
        }

        public override bool CanSelect
        {
            get
            {
                return false;
            }
        }

        protected internal override Padding DefaultMargin
        {
            get
            {
                return new Padding(2);
            }
        }

        internal int GripThickness
        {
            get
            {
                return this.gripThickness;
            }
        }

        internal bool MovingToolStrip
        {
            get
            {
                return ((this.ToolStripPanelRow != null) && this.movingToolStrip);
            }
            set
            {
                if (((this.movingToolStrip != value) && (base.ParentInternal != null)) && (!value || (base.ParentInternal.ToolStripPanelRow != null)))
                {
                    this.movingToolStrip = value;
                    this.lastEndLocation = ToolStrip.InvalidMouseEnter;
                    if (this.movingToolStrip)
                    {
                        ((ISupportToolStripPanel) base.ParentInternal).BeginDrag();
                    }
                    else
                    {
                        ((ISupportToolStripPanel) base.ParentInternal).EndDrag();
                    }
                }
            }
        }

        private System.Windows.Forms.ToolStripPanelRow ToolStripPanelRow
        {
            get
            {
                if (base.ParentInternal != null)
                {
                    return ((ISupportToolStripPanel) base.ParentInternal).ToolStripPanelRow;
                }
                return null;
            }
        }

        internal class ToolStripGripAccessibleObject : ToolStripButton.ToolStripButtonAccessibleObject
        {
            private string stockName;

            public ToolStripGripAccessibleObject(ToolStripGrip owner) : base(owner)
            {
            }

            public override string Name
            {
                get
                {
                    string accessibleName = base.Owner.AccessibleName;
                    if (accessibleName != null)
                    {
                        return accessibleName;
                    }
                    if (string.IsNullOrEmpty(this.stockName))
                    {
                        this.stockName = System.Windows.Forms.SR.GetString("ToolStripGripAccessibleName");
                    }
                    return this.stockName;
                }
                set
                {
                    base.Name = value;
                }
            }

            public override AccessibleRole Role
            {
                get
                {
                    AccessibleRole accessibleRole = base.Owner.AccessibleRole;
                    if (accessibleRole != AccessibleRole.Default)
                    {
                        return accessibleRole;
                    }
                    return AccessibleRole.Grip;
                }
            }
        }
    }
}

