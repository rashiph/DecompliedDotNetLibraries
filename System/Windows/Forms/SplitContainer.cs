namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Windows.Forms.Layout;

    [DefaultEvent("SplitterMoved"), Designer("System.Windows.Forms.Design.SplitContainerDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), ComVisible(true), ClassInterface(ClassInterfaceType.AutoDispatch), System.Windows.Forms.SRDescription("DescriptionSplitContainer"), Docking(DockingBehavior.AutoDock)]
    public class SplitContainer : ContainerControl, ISupportInitialize
    {
        private Point anchor = Point.Empty;
        private int BORDERSIZE;
        private System.Windows.Forms.BorderStyle borderStyle;
        private bool callBaseVersion;
        private const int DRAW_END = 3;
        private const int DRAW_MOVE = 2;
        private const int DRAW_START = 1;
        private static readonly object EVENT_MOVED = new object();
        private static readonly object EVENT_MOVING = new object();
        private System.Windows.Forms.FixedPanel fixedPanel;
        private bool initializing;
        private int initialSplitterDistance;
        private Rectangle initialSplitterRectangle;
        private int lastDrawSplit = 1;
        private const int leftBorder = 2;
        private int newPanel1MinSize = 0x19;
        private int newPanel2MinSize = 0x19;
        private int newSplitterWidth = 4;
        private Control nextActiveControl;
        private System.Windows.Forms.Orientation orientation = System.Windows.Forms.Orientation.Vertical;
        private Cursor overrideCursor;
        private SplitterPanel panel1;
        private int panel1MinSize = 0x19;
        private SplitterPanel panel2;
        private int panel2MinSize = 0x19;
        private int panelSize;
        private double ratioHeight;
        private double ratioWidth;
        private bool resizeCalled;
        private const int rightBorder = 5;
        private bool selectNextControl;
        private bool setSplitterDistance;
        private bool splitBegin;
        private bool splitBreak;
        private SplitContainerMessageFilter splitContainerMessageFilter;
        private bool splitContainerScaling;
        private int splitDistance = 50;
        private bool splitMove;
        private bool splitterClick;
        private int splitterDistance = 50;
        private bool splitterDrag;
        private bool splitterFixed;
        private bool splitterFocused;
        private int splitterInc = 1;
        private Rectangle splitterRect;
        private int splitterWidth = 4;
        private bool tabStop = true;

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public event EventHandler AutoSizeChanged
        {
            add
            {
                base.AutoSizeChanged += value;
            }
            remove
            {
                base.AutoSizeChanged -= value;
            }
        }

        [Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
        public event EventHandler BackgroundImageChanged
        {
            add
            {
                base.BackgroundImageChanged += value;
            }
            remove
            {
                base.BackgroundImageChanged -= value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event EventHandler BackgroundImageLayoutChanged
        {
            add
            {
                base.BackgroundImageLayoutChanged += value;
            }
            remove
            {
                base.BackgroundImageLayoutChanged -= value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public event ControlEventHandler ControlAdded
        {
            add
            {
                base.ControlAdded += value;
            }
            remove
            {
                base.ControlAdded -= value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public event ControlEventHandler ControlRemoved
        {
            add
            {
                base.ControlRemoved += value;
            }
            remove
            {
                base.ControlRemoved -= value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public event EventHandler PaddingChanged
        {
            add
            {
                base.PaddingChanged += value;
            }
            remove
            {
                base.PaddingChanged -= value;
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("SplitterSplitterMovedDescr")]
        public event SplitterEventHandler SplitterMoved
        {
            add
            {
                base.Events.AddHandler(EVENT_MOVED, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_MOVED, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("SplitterSplitterMovingDescr")]
        public event SplitterCancelEventHandler SplitterMoving
        {
            add
            {
                base.Events.AddHandler(EVENT_MOVING, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_MOVING, value);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public event EventHandler TextChanged
        {
            add
            {
                base.TextChanged += value;
            }
            remove
            {
                base.TextChanged -= value;
            }
        }

        public SplitContainer()
        {
            this.panel1 = new SplitterPanel(this);
            this.panel2 = new SplitterPanel(this);
            this.splitterRect = new Rectangle();
            base.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            base.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            ((WindowsFormsUtils.TypedControlCollection) this.Controls).AddInternal(this.panel1);
            ((WindowsFormsUtils.TypedControlCollection) this.Controls).AddInternal(this.panel2);
            this.UpdateSplitter();
        }

        internal override void AfterControlRemoved(Control control, Control oldParent)
        {
            base.AfterControlRemoved(control, oldParent);
            if ((control is SplitContainer) && (control.Dock == DockStyle.Fill))
            {
                this.SetInnerMostBorder(this);
            }
        }

        private void ApplyPanel1MinSize(int value)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException("Panel1MinSize", System.Windows.Forms.SR.GetString("InvalidLowBoundArgument", new object[] { "Panel1MinSize", value.ToString(CultureInfo.CurrentCulture), "0" }));
            }
            if (this.Orientation == System.Windows.Forms.Orientation.Vertical)
            {
                if ((base.DesignMode && (base.Width != this.DefaultSize.Width)) && (((value + this.Panel2MinSize) + this.SplitterWidth) > base.Width))
                {
                    throw new ArgumentOutOfRangeException("Panel1MinSize", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "Panel1MinSize", value.ToString(CultureInfo.CurrentCulture) }));
                }
            }
            else if (((this.Orientation == System.Windows.Forms.Orientation.Horizontal) && base.DesignMode) && ((base.Height != this.DefaultSize.Height) && (((value + this.Panel2MinSize) + this.SplitterWidth) > base.Height)))
            {
                throw new ArgumentOutOfRangeException("Panel1MinSize", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "Panel1MinSize", value.ToString(CultureInfo.CurrentCulture) }));
            }
            this.panel1MinSize = value;
            if (value > this.SplitterDistanceInternal)
            {
                this.SplitterDistanceInternal = value;
            }
        }

        private void ApplyPanel2MinSize(int value)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException("Panel2MinSize", System.Windows.Forms.SR.GetString("InvalidLowBoundArgument", new object[] { "Panel2MinSize", value.ToString(CultureInfo.CurrentCulture), "0" }));
            }
            if (this.Orientation == System.Windows.Forms.Orientation.Vertical)
            {
                if ((base.DesignMode && (base.Width != this.DefaultSize.Width)) && (((value + this.Panel1MinSize) + this.SplitterWidth) > base.Width))
                {
                    throw new ArgumentOutOfRangeException("Panel2MinSize", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "Panel2MinSize", value.ToString(CultureInfo.CurrentCulture) }));
                }
            }
            else if (((this.Orientation == System.Windows.Forms.Orientation.Horizontal) && base.DesignMode) && ((base.Height != this.DefaultSize.Height) && (((value + this.Panel1MinSize) + this.SplitterWidth) > base.Height)))
            {
                throw new ArgumentOutOfRangeException("Panel2MinSize", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "Panel2MinSize", value.ToString(CultureInfo.CurrentCulture) }));
            }
            this.panel2MinSize = value;
            if (value > this.Panel2.Width)
            {
                this.SplitterDistanceInternal = this.Panel2.Width + this.SplitterWidthInternal;
            }
        }

        private void ApplySplitterDistance()
        {
            using (new LayoutTransaction(this, this, "SplitterDistance", false))
            {
                this.SplitterDistanceInternal = this.splitterDistance;
            }
            if (this.BackColor == Color.Transparent)
            {
                base.Invalidate();
            }
            if (this.Orientation == System.Windows.Forms.Orientation.Vertical)
            {
                if (this.RightToLeft == RightToLeft.No)
                {
                    this.splitterRect.X = base.Location.X + this.SplitterDistanceInternal;
                }
                else
                {
                    this.splitterRect.X = (base.Right - this.SplitterDistanceInternal) - this.SplitterWidthInternal;
                }
            }
            else
            {
                this.splitterRect.Y = base.Location.Y + this.SplitterDistanceInternal;
            }
        }

        private void ApplySplitterWidth(int value)
        {
            if (value < 1)
            {
                throw new ArgumentOutOfRangeException("SplitterWidth", System.Windows.Forms.SR.GetString("InvalidLowBoundArgumentEx", new object[] { "SplitterWidth", value.ToString(CultureInfo.CurrentCulture), "1" }));
            }
            if (this.Orientation == System.Windows.Forms.Orientation.Vertical)
            {
                if (base.DesignMode && (((value + this.Panel1MinSize) + this.Panel2MinSize) > base.Width))
                {
                    throw new ArgumentOutOfRangeException("SplitterWidth", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "SplitterWidth", value.ToString(CultureInfo.CurrentCulture) }));
                }
            }
            else if (((this.Orientation == System.Windows.Forms.Orientation.Horizontal) && base.DesignMode) && (((value + this.Panel1MinSize) + this.Panel2MinSize) > base.Height))
            {
                throw new ArgumentOutOfRangeException("SplitterWidth", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "SplitterWidth", value.ToString(CultureInfo.CurrentCulture) }));
            }
            this.splitterWidth = value;
            this.UpdateSplitter();
        }

        public void BeginInit()
        {
            this.initializing = true;
        }

        private Rectangle CalcSplitLine(int splitSize, int minWeight)
        {
            Rectangle rectangle = new Rectangle();
            switch (this.Orientation)
            {
                case System.Windows.Forms.Orientation.Horizontal:
                    rectangle.Width = base.Width;
                    rectangle.Height = this.SplitterWidthInternal;
                    if (rectangle.Width < minWeight)
                    {
                        rectangle.Width = minWeight;
                    }
                    rectangle.Y = this.panel1.Location.Y + splitSize;
                    return rectangle;

                case System.Windows.Forms.Orientation.Vertical:
                    rectangle.Width = this.SplitterWidthInternal;
                    rectangle.Height = base.Height;
                    if (rectangle.Width < minWeight)
                    {
                        rectangle.Width = minWeight;
                    }
                    if (this.RightToLeft == RightToLeft.No)
                    {
                        rectangle.X = this.panel1.Location.X + splitSize;
                        return rectangle;
                    }
                    rectangle.X = (base.Width - splitSize) - this.SplitterWidthInternal;
                    return rectangle;
            }
            return rectangle;
        }

        private void CollapsePanel(SplitterPanel p, bool collapsing)
        {
            p.Collapsed = collapsing;
            if (collapsing)
            {
                p.Visible = false;
            }
            else
            {
                p.Visible = true;
            }
            this.UpdateSplitter();
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected override Control.ControlCollection CreateControlsInstance()
        {
            return new SplitContainerTypedControlCollection(this, typeof(SplitterPanel), true);
        }

        private void DrawFocus(Graphics g, Rectangle r)
        {
            r.Inflate(-1, -1);
            ControlPaint.DrawFocusRectangle(g, r, this.ForeColor, this.BackColor);
        }

        private void DrawSplitBar(int mode)
        {
            if ((mode != 1) && (this.lastDrawSplit != -1))
            {
                this.DrawSplitHelper(this.lastDrawSplit);
                this.lastDrawSplit = -1;
            }
            else if ((mode != 1) && (this.lastDrawSplit == -1))
            {
                return;
            }
            if (mode != 3)
            {
                if (this.splitMove || this.splitBegin)
                {
                    this.DrawSplitHelper(this.splitterDistance);
                    this.lastDrawSplit = this.splitterDistance;
                }
                else
                {
                    this.DrawSplitHelper(this.splitterDistance);
                    this.lastDrawSplit = this.splitterDistance;
                }
            }
            else
            {
                if (this.lastDrawSplit != -1)
                {
                    this.DrawSplitHelper(this.lastDrawSplit);
                }
                this.lastDrawSplit = -1;
            }
        }

        private void DrawSplitHelper(int splitSize)
        {
            Rectangle rectangle = this.CalcSplitLine(splitSize, 3);
            IntPtr handle = base.Handle;
            IntPtr ptr2 = System.Windows.Forms.UnsafeNativeMethods.GetDCEx(new HandleRef(this, handle), System.Windows.Forms.NativeMethods.NullHandleRef, 0x402);
            IntPtr ptr3 = ControlPaint.CreateHalftoneHBRUSH();
            IntPtr ptr4 = System.Windows.Forms.SafeNativeMethods.SelectObject(new HandleRef(this, ptr2), new HandleRef(null, ptr3));
            System.Windows.Forms.SafeNativeMethods.PatBlt(new HandleRef(this, ptr2), rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, 0x5a0049);
            System.Windows.Forms.SafeNativeMethods.SelectObject(new HandleRef(this, ptr2), new HandleRef(null, ptr4));
            System.Windows.Forms.SafeNativeMethods.DeleteObject(new HandleRef(null, ptr3));
            System.Windows.Forms.UnsafeNativeMethods.ReleaseDC(new HandleRef(this, handle), new HandleRef(null, ptr2));
        }

        public void EndInit()
        {
            this.initializing = false;
            if (this.newPanel1MinSize != this.panel1MinSize)
            {
                this.ApplyPanel1MinSize(this.newPanel1MinSize);
            }
            if (this.newPanel2MinSize != this.panel2MinSize)
            {
                this.ApplyPanel2MinSize(this.newPanel2MinSize);
            }
            if (this.newSplitterWidth != this.splitterWidth)
            {
                this.ApplySplitterWidth(this.newSplitterWidth);
            }
        }

        private int GetSplitterDistance(int x, int y)
        {
            int num;
            if (this.Orientation == System.Windows.Forms.Orientation.Vertical)
            {
                num = x - this.anchor.X;
            }
            else
            {
                num = y - this.anchor.Y;
            }
            int num2 = 0;
            switch (this.Orientation)
            {
                case System.Windows.Forms.Orientation.Horizontal:
                    num2 = Math.Max(this.panel1.Height + num, this.BORDERSIZE);
                    break;

                case System.Windows.Forms.Orientation.Vertical:
                    if (this.RightToLeft != RightToLeft.No)
                    {
                        num2 = Math.Max(this.panel1.Width - num, this.BORDERSIZE);
                        break;
                    }
                    num2 = Math.Max(this.panel1.Width + num, this.BORDERSIZE);
                    break;
            }
            if (this.Orientation == System.Windows.Forms.Orientation.Vertical)
            {
                return Math.Max(Math.Min(num2, base.Width - this.Panel2MinSize), this.Panel1MinSize);
            }
            return Math.Max(Math.Min(num2, base.Height - this.Panel2MinSize), this.Panel1MinSize);
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            base.Invalidate();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (this.IsSplitterMovable && !this.IsSplitterFixed)
            {
                if ((e.KeyData == Keys.Escape) && this.splitBegin)
                {
                    this.splitBegin = false;
                    this.splitBreak = true;
                }
                else if (((e.KeyData == Keys.Right) || (e.KeyData == Keys.Down)) || ((e.KeyData == Keys.Left) || ((e.KeyData == Keys.Up) && this.splitterFocused)))
                {
                    if (this.splitBegin)
                    {
                        this.splitMove = true;
                    }
                    if ((e.KeyData == Keys.Left) || ((e.KeyData == Keys.Up) && this.splitterFocused))
                    {
                        this.splitterDistance -= this.SplitterIncrement;
                        this.splitterDistance = (this.splitterDistance < this.Panel1MinSize) ? (this.splitterDistance + this.SplitterIncrement) : Math.Max(this.splitterDistance, this.BORDERSIZE);
                    }
                    if ((e.KeyData == Keys.Right) || ((e.KeyData == Keys.Down) && this.splitterFocused))
                    {
                        this.splitterDistance += this.SplitterIncrement;
                        if (this.Orientation == System.Windows.Forms.Orientation.Vertical)
                        {
                            this.splitterDistance = ((this.splitterDistance + this.SplitterWidth) > ((base.Width - this.Panel2MinSize) - this.BORDERSIZE)) ? (this.splitterDistance - this.SplitterIncrement) : this.splitterDistance;
                        }
                        else
                        {
                            this.splitterDistance = ((this.splitterDistance + this.SplitterWidth) > ((base.Height - this.Panel2MinSize) - this.BORDERSIZE)) ? (this.splitterDistance - this.SplitterIncrement) : this.splitterDistance;
                        }
                    }
                    if (!this.splitBegin)
                    {
                        this.splitBegin = true;
                    }
                    if (this.splitBegin && !this.splitMove)
                    {
                        this.initialSplitterDistance = this.SplitterDistanceInternal;
                        this.DrawSplitBar(1);
                    }
                    else
                    {
                        this.DrawSplitBar(2);
                        Rectangle rectangle = this.CalcSplitLine(this.splitterDistance, 0);
                        int x = rectangle.X;
                        int y = rectangle.Y;
                        SplitterCancelEventArgs args = new SplitterCancelEventArgs((base.Left + this.SplitterRectangle.X) + (this.SplitterRectangle.Width / 2), (base.Top + this.SplitterRectangle.Y) + (this.SplitterRectangle.Height / 2), x, y);
                        this.OnSplitterMoving(args);
                        if (args.Cancel)
                        {
                            this.SplitEnd(false);
                        }
                    }
                }
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            if ((this.splitBegin && this.IsSplitterMovable) && (((e.KeyData == Keys.Right) || (e.KeyData == Keys.Down)) || ((e.KeyData == Keys.Left) || ((e.KeyData == Keys.Up) && this.splitterFocused))))
            {
                this.DrawSplitBar(3);
                this.ApplySplitterDistance();
                this.splitBegin = false;
                this.splitMove = false;
            }
            if (this.splitBreak)
            {
                this.splitBreak = false;
                this.SplitEnd(false);
            }
            using (Graphics graphics = base.CreateGraphicsInternal())
            {
                if (this.BackgroundImage == null)
                {
                    using (SolidBrush brush = new SolidBrush(this.BackColor))
                    {
                        graphics.FillRectangle(brush, this.SplitterRectangle);
                    }
                }
                this.DrawFocus(graphics, this.SplitterRectangle);
            }
        }

        protected override void OnLayout(LayoutEventArgs e)
        {
            this.SetInnerMostBorder(this);
            if (this.IsSplitterMovable && !this.setSplitterDistance)
            {
                this.ResizeSplitContainer();
            }
            base.OnLayout(e);
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            base.Invalidate();
        }

        protected override void OnMouseCaptureChanged(EventArgs e)
        {
            base.OnMouseCaptureChanged(e);
            if (this.splitContainerMessageFilter != null)
            {
                Application.RemoveMessageFilter(this.splitContainerMessageFilter);
                this.splitContainerMessageFilter = null;
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (((this.IsSplitterMovable && this.SplitterRectangle.Contains(e.Location)) && base.Enabled) && (((e.Button == MouseButtons.Left) && (e.Clicks == 1)) && !this.IsSplitterFixed))
            {
                this.splitterFocused = true;
                IContainerControl containerControlInternal = this.ParentInternal.GetContainerControlInternal();
                if (containerControlInternal != null)
                {
                    ContainerControl control2 = containerControlInternal as ContainerControl;
                    if (control2 == null)
                    {
                        containerControlInternal.ActiveControl = this;
                    }
                    else
                    {
                        control2.SetActiveControlInternal(this);
                    }
                }
                base.SetActiveControlInternal(null);
                this.nextActiveControl = this.panel2;
                this.SplitBegin(e.X, e.Y);
                this.splitterClick = true;
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (base.Enabled)
            {
                this.OverrideCursor = null;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (!this.IsSplitterFixed && this.IsSplitterMovable)
            {
                if ((this.Cursor == this.DefaultCursor) && this.SplitterRectangle.Contains(e.Location))
                {
                    if (this.Orientation == System.Windows.Forms.Orientation.Vertical)
                    {
                        this.OverrideCursor = Cursors.VSplit;
                    }
                    else
                    {
                        this.OverrideCursor = Cursors.HSplit;
                    }
                }
                else
                {
                    this.OverrideCursor = null;
                }
                if (this.splitterClick)
                {
                    int x = e.X;
                    int y = e.Y;
                    this.splitterDrag = true;
                    this.SplitMove(x, y);
                    if (this.Orientation == System.Windows.Forms.Orientation.Vertical)
                    {
                        x = Math.Max(Math.Min(x, base.Width - this.Panel2MinSize), this.Panel1MinSize);
                        y = Math.Max(y, 0);
                    }
                    else
                    {
                        y = Math.Max(Math.Min(y, base.Height - this.Panel2MinSize), this.Panel1MinSize);
                        x = Math.Max(x, 0);
                    }
                    Rectangle rectangle = this.CalcSplitLine(this.GetSplitterDistance(e.X, e.Y), 0);
                    int splitX = rectangle.X;
                    int splitY = rectangle.Y;
                    SplitterCancelEventArgs args = new SplitterCancelEventArgs(x, y, splitX, splitY);
                    this.OnSplitterMoving(args);
                    if (args.Cancel)
                    {
                        this.SplitEnd(false);
                    }
                }
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (base.Enabled && ((!this.IsSplitterFixed && this.IsSplitterMovable) && this.splitterClick))
            {
                base.CaptureInternal = false;
                if (this.splitterDrag)
                {
                    this.CalcSplitLine(this.GetSplitterDistance(e.X, e.Y), 0);
                    this.SplitEnd(true);
                }
                else
                {
                    this.SplitEnd(false);
                }
                this.splitterClick = false;
                this.splitterDrag = false;
            }
        }

        protected override void OnMove(EventArgs e)
        {
            base.OnMove(e);
            this.SetSplitterRect(this.Orientation == System.Windows.Forms.Orientation.Vertical);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (this.Focused)
            {
                this.DrawFocus(e.Graphics, this.SplitterRectangle);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected override void OnRightToLeftChanged(EventArgs e)
        {
            base.OnRightToLeftChanged(e);
            this.panel1.RightToLeft = this.RightToLeft;
            this.panel2.RightToLeft = this.RightToLeft;
            this.UpdateSplitter();
        }

        public void OnSplitterMoved(SplitterEventArgs e)
        {
            SplitterEventHandler handler = (SplitterEventHandler) base.Events[EVENT_MOVED];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public void OnSplitterMoving(SplitterCancelEventArgs e)
        {
            SplitterCancelEventHandler handler = (SplitterCancelEventHandler) base.Events[EVENT_MOVING];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private bool ProcessArrowKey(bool forward)
        {
            Control parentInternal = this;
            if (base.ActiveControl != null)
            {
                parentInternal = base.ActiveControl.ParentInternal;
            }
            return parentInternal.SelectNextControl(base.ActiveControl, forward, false, false, true);
        }

        [UIPermission(SecurityAction.LinkDemand, Window=UIPermissionWindow.AllWindows)]
        protected override bool ProcessDialogKey(Keys keyData)
        {
            if ((keyData & (Keys.Alt | Keys.Control)) == Keys.None)
            {
                Keys keys = keyData & Keys.KeyCode;
                switch (keys)
                {
                    case Keys.Left:
                    case Keys.Up:
                    case Keys.Right:
                    case Keys.Down:
                        if (this.splitterFocused)
                        {
                            return false;
                        }
                        if (!this.ProcessArrowKey((keys == Keys.Right) || (keys == Keys.Down)))
                        {
                            break;
                        }
                        return true;

                    case Keys.Tab:
                        if (this.ProcessTabKey((keyData & Keys.Shift) == Keys.None))
                        {
                            return true;
                        }
                        break;
                }
            }
            return base.ProcessDialogKey(keyData);
        }

        [UIPermission(SecurityAction.LinkDemand, Window=UIPermissionWindow.AllWindows)]
        protected override bool ProcessTabKey(bool forward)
        {
            if (!this.TabStop || this.IsSplitterFixed)
            {
                return base.ProcessTabKey(forward);
            }
            if (this.nextActiveControl != null)
            {
                base.SetActiveControlInternal(this.nextActiveControl);
                this.nextActiveControl = null;
            }
            if (this.SelectNextControlInPanel(base.ActiveControl, forward, true, true, true))
            {
                this.nextActiveControl = null;
                this.splitterFocused = false;
                return true;
            }
            if (this.callBaseVersion)
            {
                this.callBaseVersion = false;
                return base.ProcessTabKey(forward);
            }
            this.splitterFocused = true;
            IContainerControl containerControlInternal = this.ParentInternal.GetContainerControlInternal();
            if (containerControlInternal != null)
            {
                ContainerControl control2 = containerControlInternal as ContainerControl;
                if (control2 == null)
                {
                    containerControlInternal.ActiveControl = this;
                }
                else
                {
                    control2.SetActiveControlInternal(this);
                }
            }
            base.SetActiveControlInternal(null);
            return true;
        }

        private void RepaintSplitterRect()
        {
            if (!base.IsHandleCreated)
            {
                return;
            }
            Graphics graphics = base.CreateGraphicsInternal();
            if (this.BackgroundImage != null)
            {
                using (TextureBrush brush = new TextureBrush(this.BackgroundImage, WrapMode.Tile))
                {
                    graphics.FillRectangle(brush, base.ClientRectangle);
                    goto Label_0062;
                }
            }
            using (SolidBrush brush2 = new SolidBrush(this.BackColor))
            {
                graphics.FillRectangle(brush2, this.splitterRect);
            }
        Label_0062:
            graphics.Dispose();
        }

        private void ResizeSplitContainer()
        {
            if (!this.splitContainerScaling)
            {
                this.panel1.SuspendLayout();
                this.panel2.SuspendLayout();
                if (base.Width == 0)
                {
                    this.panel1.Size = new Size(0, this.panel1.Height);
                    this.panel2.Size = new Size(0, this.panel2.Height);
                }
                else if (base.Height == 0)
                {
                    this.panel1.Size = new Size(this.panel1.Width, 0);
                    this.panel2.Size = new Size(this.panel2.Width, 0);
                }
                else
                {
                    if (this.Orientation == System.Windows.Forms.Orientation.Vertical)
                    {
                        if (!this.CollapsedMode)
                        {
                            if (this.FixedPanel == System.Windows.Forms.FixedPanel.Panel1)
                            {
                                this.panel1.Size = new Size(this.panelSize, base.Height);
                                this.panel2.Size = new Size(Math.Max((base.Width - this.panelSize) - this.SplitterWidthInternal, this.Panel2MinSize), base.Height);
                            }
                            if (this.FixedPanel == System.Windows.Forms.FixedPanel.Panel2)
                            {
                                this.panel2.Size = new Size(this.panelSize, base.Height);
                                this.splitterDistance = Math.Max((base.Width - this.panelSize) - this.SplitterWidthInternal, this.Panel1MinSize);
                                this.panel1.WidthInternal = this.splitterDistance;
                                this.panel1.HeightInternal = base.Height;
                            }
                            if (this.FixedPanel == System.Windows.Forms.FixedPanel.None)
                            {
                                if (this.ratioWidth != 0.0)
                                {
                                    this.splitterDistance = Math.Max((int) Math.Floor((double) (((double) base.Width) / this.ratioWidth)), this.Panel1MinSize);
                                }
                                this.panel1.WidthInternal = this.splitterDistance;
                                this.panel1.HeightInternal = base.Height;
                                this.panel2.Size = new Size(Math.Max((base.Width - this.splitterDistance) - this.SplitterWidthInternal, this.Panel2MinSize), base.Height);
                            }
                            if (this.RightToLeft == RightToLeft.No)
                            {
                                this.panel2.Location = new Point(this.panel1.WidthInternal + this.SplitterWidthInternal, 0);
                            }
                            else
                            {
                                this.panel1.Location = new Point(base.Width - this.panel1.WidthInternal, 0);
                            }
                            this.RepaintSplitterRect();
                            this.SetSplitterRect(true);
                        }
                        else if (this.Panel1Collapsed)
                        {
                            this.panel2.Size = base.Size;
                            this.panel2.Location = new Point(0, 0);
                        }
                        else if (this.Panel2Collapsed)
                        {
                            this.panel1.Size = base.Size;
                            this.panel1.Location = new Point(0, 0);
                        }
                    }
                    else if (this.Orientation == System.Windows.Forms.Orientation.Horizontal)
                    {
                        if (!this.CollapsedMode)
                        {
                            if (this.FixedPanel == System.Windows.Forms.FixedPanel.Panel1)
                            {
                                this.panel1.Size = new Size(base.Width, this.panelSize);
                                int y = this.panelSize + this.SplitterWidthInternal;
                                this.panel2.Size = new Size(base.Width, Math.Max(base.Height - y, this.Panel2MinSize));
                                this.panel2.Location = new Point(0, y);
                            }
                            if (this.FixedPanel == System.Windows.Forms.FixedPanel.Panel2)
                            {
                                this.panel2.Size = new Size(base.Width, this.panelSize);
                                this.splitterDistance = Math.Max((base.Height - this.Panel2.Height) - this.SplitterWidthInternal, this.Panel1MinSize);
                                this.panel1.HeightInternal = this.splitterDistance;
                                this.panel1.WidthInternal = base.Width;
                                int num2 = this.splitterDistance + this.SplitterWidthInternal;
                                this.panel2.Location = new Point(0, num2);
                            }
                            if (this.FixedPanel == System.Windows.Forms.FixedPanel.None)
                            {
                                if (this.ratioHeight != 0.0)
                                {
                                    this.splitterDistance = Math.Max((int) Math.Floor((double) (((double) base.Height) / this.ratioHeight)), this.Panel1MinSize);
                                }
                                this.panel1.HeightInternal = this.splitterDistance;
                                this.panel1.WidthInternal = base.Width;
                                int num3 = this.splitterDistance + this.SplitterWidthInternal;
                                this.panel2.Size = new Size(base.Width, Math.Max(base.Height - num3, this.Panel2MinSize));
                                this.panel2.Location = new Point(0, num3);
                            }
                            this.RepaintSplitterRect();
                            this.SetSplitterRect(false);
                        }
                        else if (this.Panel1Collapsed)
                        {
                            this.panel2.Size = base.Size;
                            this.panel2.Location = new Point(0, 0);
                        }
                        else if (this.Panel2Collapsed)
                        {
                            this.panel1.Size = base.Size;
                            this.panel1.Location = new Point(0, 0);
                        }
                    }
                    try
                    {
                        this.resizeCalled = true;
                        this.ApplySplitterDistance();
                    }
                    finally
                    {
                        this.resizeCalled = false;
                    }
                }
                this.panel1.ResumeLayout();
                this.panel2.ResumeLayout();
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
        {
            try
            {
                float width;
                this.splitContainerScaling = true;
                base.ScaleControl(factor, specified);
                if (this.orientation == System.Windows.Forms.Orientation.Vertical)
                {
                    width = factor.Width;
                }
                else
                {
                    width = factor.Height;
                }
                this.SplitterWidth = (int) Math.Round((double) (this.SplitterWidth * width));
            }
            finally
            {
                this.splitContainerScaling = false;
            }
        }

        protected override void Select(bool directed, bool forward)
        {
            if (!this.selectNextControl)
            {
                if (((this.Panel1.Controls.Count > 0) || (this.Panel2.Controls.Count > 0)) || this.TabStop)
                {
                    this.SelectNextControlInContainer(this, forward, true, true, false);
                }
                else
                {
                    try
                    {
                        Control parentInternal = this.ParentInternal;
                        this.selectNextControl = true;
                        while (parentInternal != null)
                        {
                            if (parentInternal.SelectNextControl(this, forward, true, true, parentInternal.ParentInternal == null))
                            {
                                return;
                            }
                            parentInternal = parentInternal.ParentInternal;
                        }
                    }
                    finally
                    {
                        this.selectNextControl = false;
                    }
                }
            }
        }

        private static void SelectNextActiveControl(Control ctl, bool forward, bool tabStopOnly, bool nested, bool wrap)
        {
            ContainerControl control = ctl as ContainerControl;
            if (control != null)
            {
                bool flag = true;
                if (control.ParentInternal != null)
                {
                    IContainerControl containerControlInternal = control.ParentInternal.GetContainerControlInternal();
                    if (containerControlInternal != null)
                    {
                        containerControlInternal.ActiveControl = control;
                        flag = containerControlInternal.ActiveControl == control;
                    }
                }
                if (flag)
                {
                    ctl.SelectNextControl(null, forward, tabStopOnly, nested, wrap);
                }
            }
            else
            {
                ctl.Select();
            }
        }

        private bool SelectNextControlInContainer(Control ctl, bool forward, bool tabStopOnly, bool nested, bool wrap)
        {
            if (!base.Contains(ctl) || (!nested && (ctl.ParentInternal != this)))
            {
                ctl = null;
            }
            SplitterPanel panel = null;
            do
            {
                ctl = base.GetNextControl(ctl, forward);
                SplitterPanel panel2 = ctl as SplitterPanel;
                if ((panel2 != null) && panel2.Visible)
                {
                    if (panel != null)
                    {
                        break;
                    }
                    panel = panel2;
                }
                if ((!forward && (panel != null)) && (ctl.ParentInternal != panel))
                {
                    ctl = panel;
                    break;
                }
                if (ctl == null)
                {
                    break;
                }
                if (ctl.CanSelect && ctl.TabStop)
                {
                    if (ctl is SplitContainer)
                    {
                        ((SplitContainer) ctl).Select(forward, forward);
                    }
                    else
                    {
                        SelectNextActiveControl(ctl, forward, tabStopOnly, nested, wrap);
                    }
                    return true;
                }
            }
            while (ctl != null);
            if ((ctl != null) && this.TabStop)
            {
                this.splitterFocused = true;
                IContainerControl containerControlInternal = this.ParentInternal.GetContainerControlInternal();
                if (containerControlInternal != null)
                {
                    ContainerControl control2 = containerControlInternal as ContainerControl;
                    if (control2 == null)
                    {
                        containerControlInternal.ActiveControl = this;
                    }
                    else
                    {
                        System.Windows.Forms.IntSecurity.ModifyFocus.Demand();
                        control2.SetActiveControlInternal(this);
                    }
                }
                base.SetActiveControlInternal(null);
                this.nextActiveControl = ctl;
                return true;
            }
            if (!this.SelectNextControlInPanel(ctl, forward, tabStopOnly, nested, wrap))
            {
                Control parentInternal = this.ParentInternal;
                if (parentInternal != null)
                {
                    try
                    {
                        this.selectNextControl = true;
                        parentInternal.SelectNextControl(this, forward, true, true, true);
                    }
                    finally
                    {
                        this.selectNextControl = false;
                    }
                }
            }
            return false;
        }

        private bool SelectNextControlInPanel(Control ctl, bool forward, bool tabStopOnly, bool nested, bool wrap)
        {
            if (!base.Contains(ctl) || (!nested && (ctl.ParentInternal != this)))
            {
                ctl = null;
            }
            do
            {
                ctl = base.GetNextControl(ctl, forward);
                if ((ctl == null) || ((ctl is SplitterPanel) && ctl.Visible))
                {
                    break;
                }
                if (ctl.CanSelect && (!tabStopOnly || ctl.TabStop))
                {
                    if (ctl is SplitContainer)
                    {
                        ((SplitContainer) ctl).Select(forward, forward);
                    }
                    else
                    {
                        SelectNextActiveControl(ctl, forward, tabStopOnly, nested, wrap);
                    }
                    return true;
                }
            }
            while (ctl != null);
            if ((ctl == null) || ((ctl is SplitterPanel) && !ctl.Visible))
            {
                this.callBaseVersion = true;
            }
            else
            {
                ctl = base.GetNextControl(ctl, forward);
                if (forward)
                {
                    this.nextActiveControl = this.panel2;
                }
                else if ((ctl == null) || !ctl.ParentInternal.Visible)
                {
                    this.callBaseVersion = true;
                }
                else
                {
                    this.nextActiveControl = this.panel2;
                }
            }
            return false;
        }

        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            if ((((specified & BoundsSpecified.Height) != BoundsSpecified.None) && (this.Orientation == System.Windows.Forms.Orientation.Horizontal)) && (height < ((this.Panel1MinSize + this.SplitterWidthInternal) + this.Panel2MinSize)))
            {
                height = (this.Panel1MinSize + this.SplitterWidthInternal) + this.Panel2MinSize;
            }
            if ((((specified & BoundsSpecified.Width) != BoundsSpecified.None) && (this.Orientation == System.Windows.Forms.Orientation.Vertical)) && (width < ((this.Panel1MinSize + this.SplitterWidthInternal) + this.Panel2MinSize)))
            {
                width = (this.Panel1MinSize + this.SplitterWidthInternal) + this.Panel2MinSize;
            }
            base.SetBoundsCore(x, y, width, height, specified);
            this.SetSplitterRect(this.Orientation == System.Windows.Forms.Orientation.Vertical);
        }

        private void SetInnerMostBorder(SplitContainer sc)
        {
            foreach (Control control in sc.Controls)
            {
                bool flag = false;
                if (control is SplitterPanel)
                {
                    foreach (Control control2 in control.Controls)
                    {
                        SplitContainer container = control2 as SplitContainer;
                        if ((container != null) && (container.Dock == DockStyle.Fill))
                        {
                            if (container.BorderStyle != this.BorderStyle)
                            {
                                break;
                            }
                            ((SplitterPanel) control).BorderStyle = System.Windows.Forms.BorderStyle.None;
                            this.SetInnerMostBorder(container);
                            flag = true;
                        }
                    }
                    if (!flag)
                    {
                        ((SplitterPanel) control).BorderStyle = this.BorderStyle;
                    }
                }
            }
        }

        private void SetSplitterRect(bool vertical)
        {
            if (vertical)
            {
                this.splitterRect.X = (this.RightToLeft == RightToLeft.Yes) ? ((base.Width - this.splitterDistance) - this.SplitterWidthInternal) : (base.Location.X + this.splitterDistance);
                this.splitterRect.Y = base.Location.Y;
                this.splitterRect.Width = this.SplitterWidthInternal;
                this.splitterRect.Height = base.Height;
            }
            else
            {
                this.splitterRect.X = base.Location.X;
                this.splitterRect.Y = base.Location.Y + this.SplitterDistanceInternal;
                this.splitterRect.Width = base.Width;
                this.splitterRect.Height = this.SplitterWidthInternal;
            }
        }

        private void SplitBegin(int x, int y)
        {
            this.anchor = new Point(x, y);
            this.splitterDistance = this.GetSplitterDistance(x, y);
            this.initialSplitterDistance = this.splitterDistance;
            this.initialSplitterRectangle = this.SplitterRectangle;
            System.Windows.Forms.IntSecurity.UnmanagedCode.Assert();
            try
            {
                if (this.splitContainerMessageFilter == null)
                {
                    this.splitContainerMessageFilter = new SplitContainerMessageFilter(this);
                }
                Application.AddMessageFilter(this.splitContainerMessageFilter);
            }
            finally
            {
                CodeAccessPermission.RevertAssert();
            }
            base.CaptureInternal = true;
            this.DrawSplitBar(1);
        }

        private void SplitEnd(bool accept)
        {
            this.DrawSplitBar(3);
            if (this.splitContainerMessageFilter != null)
            {
                Application.RemoveMessageFilter(this.splitContainerMessageFilter);
                this.splitContainerMessageFilter = null;
            }
            if (accept)
            {
                this.ApplySplitterDistance();
            }
            else if (this.splitterDistance != this.initialSplitterDistance)
            {
                this.splitterClick = false;
                this.splitterDistance = this.SplitterDistanceInternal = this.initialSplitterDistance;
            }
            this.anchor = Point.Empty;
        }

        private void SplitMove(int x, int y)
        {
            int splitterDistance = this.GetSplitterDistance(x, y);
            int num2 = splitterDistance - this.initialSplitterDistance;
            int num3 = num2 % this.SplitterIncrement;
            if (this.splitterDistance != splitterDistance)
            {
                if (this.Orientation == System.Windows.Forms.Orientation.Vertical)
                {
                    if ((splitterDistance + this.SplitterWidthInternal) <= ((base.Width - this.Panel2MinSize) - this.BORDERSIZE))
                    {
                        this.splitterDistance = splitterDistance - num3;
                    }
                }
                else if ((splitterDistance + this.SplitterWidthInternal) <= ((base.Height - this.Panel2MinSize) - this.BORDERSIZE))
                {
                    this.splitterDistance = splitterDistance - num3;
                }
            }
            this.DrawSplitBar(2);
        }

        private void UpdateSplitter()
        {
            if (!this.splitContainerScaling)
            {
                this.panel1.SuspendLayout();
                this.panel2.SuspendLayout();
                if (this.Orientation == System.Windows.Forms.Orientation.Vertical)
                {
                    bool flag = this.RightToLeft == RightToLeft.Yes;
                    if (!this.CollapsedMode)
                    {
                        this.panel1.HeightInternal = base.Height;
                        this.panel1.WidthInternal = this.splitterDistance;
                        this.panel2.Size = new Size((base.Width - this.splitterDistance) - this.SplitterWidthInternal, base.Height);
                        if (!flag)
                        {
                            this.panel1.Location = new Point(0, 0);
                            this.panel2.Location = new Point(this.splitterDistance + this.SplitterWidthInternal, 0);
                        }
                        else
                        {
                            this.panel1.Location = new Point(base.Width - this.splitterDistance, 0);
                            this.panel2.Location = new Point(0, 0);
                        }
                        this.RepaintSplitterRect();
                        this.SetSplitterRect(true);
                        if (!this.resizeCalled)
                        {
                            this.ratioWidth = ((((double) base.Width) / ((double) this.panel1.Width)) > 0.0) ? (((double) base.Width) / ((double) this.panel1.Width)) : this.ratioWidth;
                        }
                    }
                    else
                    {
                        if (this.Panel1Collapsed)
                        {
                            this.panel2.Size = base.Size;
                            this.panel2.Location = new Point(0, 0);
                        }
                        else if (this.Panel2Collapsed)
                        {
                            this.panel1.Size = base.Size;
                            this.panel1.Location = new Point(0, 0);
                        }
                        if (!this.resizeCalled)
                        {
                            this.ratioWidth = ((((double) base.Width) / ((double) this.splitterDistance)) > 0.0) ? (((double) base.Width) / ((double) this.splitterDistance)) : this.ratioWidth;
                        }
                    }
                }
                else if (!this.CollapsedMode)
                {
                    this.panel1.Location = new Point(0, 0);
                    this.panel1.WidthInternal = base.Width;
                    this.panel1.HeightInternal = this.SplitterDistanceInternal;
                    int y = this.splitterDistance + this.SplitterWidthInternal;
                    this.panel2.Size = new Size(base.Width, base.Height - y);
                    this.panel2.Location = new Point(0, y);
                    this.RepaintSplitterRect();
                    this.SetSplitterRect(false);
                    if (!this.resizeCalled)
                    {
                        this.ratioHeight = ((((double) base.Height) / ((double) this.panel1.Height)) > 0.0) ? (((double) base.Height) / ((double) this.panel1.Height)) : this.ratioHeight;
                    }
                }
                else
                {
                    if (this.Panel1Collapsed)
                    {
                        this.panel2.Size = base.Size;
                        this.panel2.Location = new Point(0, 0);
                    }
                    else if (this.Panel2Collapsed)
                    {
                        this.panel1.Size = base.Size;
                        this.panel1.Location = new Point(0, 0);
                    }
                    if (!this.resizeCalled)
                    {
                        this.ratioHeight = ((((double) base.Height) / ((double) this.splitterDistance)) > 0.0) ? (((double) base.Height) / ((double) this.splitterDistance)) : this.ratioHeight;
                    }
                }
                this.panel1.ResumeLayout();
                this.panel2.ResumeLayout();
            }
        }

        private void WmSetCursor(ref Message m)
        {
            if ((m.WParam == base.InternalHandle) && ((((int) m.LParam) & 0xffff) == 1))
            {
                if (this.OverrideCursor != null)
                {
                    Cursor.CurrentInternal = this.OverrideCursor;
                }
                else
                {
                    Cursor.CurrentInternal = this.Cursor;
                }
            }
            else
            {
                this.DefWndProc(ref m);
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        protected override void WndProc(ref Message msg)
        {
            int num = msg.Msg;
            switch (num)
            {
                case 7:
                    this.splitterFocused = true;
                    base.WndProc(ref msg);
                    return;

                case 8:
                    this.splitterFocused = false;
                    base.WndProc(ref msg);
                    return;
            }
            if (num == 0x20)
            {
                this.WmSetCursor(ref msg);
            }
            else
            {
                base.WndProc(ref msg);
            }
        }

        [DefaultValue(false), EditorBrowsable(EditorBrowsableState.Never), System.Windows.Forms.SRDescription("FormAutoScrollDescr"), Browsable(false), System.Windows.Forms.SRCategory("CatLayout"), Localizable(true)]
        public override bool AutoScroll
        {
            get
            {
                return false;
            }
            set
            {
                base.AutoScroll = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public Size AutoScrollMargin
        {
            get
            {
                return base.AutoScrollMargin;
            }
            set
            {
                base.AutoScrollMargin = value;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never)]
        public Size AutoScrollMinSize
        {
            get
            {
                return base.AutoScrollMinSize;
            }
            set
            {
                base.AutoScrollMinSize = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), DefaultValue(typeof(Point), "0, 0"), Browsable(false)]
        public override Point AutoScrollOffset
        {
            get
            {
                return base.AutoScrollOffset;
            }
            set
            {
                base.AutoScrollOffset = value;
            }
        }

        [System.Windows.Forms.SRCategory("CatLayout"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), System.Windows.Forms.SRDescription("FormAutoScrollPositionDescr"), EditorBrowsable(EditorBrowsableState.Never)]
        public Point AutoScrollPosition
        {
            get
            {
                return base.AutoScrollPosition;
            }
            set
            {
                base.AutoScrollPosition = value;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never)]
        public override bool AutoSize
        {
            get
            {
                return base.AutoSize;
            }
            set
            {
                base.AutoSize = value;
            }
        }

        [Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
        public override Image BackgroundImage
        {
            get
            {
                return base.BackgroundImage;
            }
            set
            {
                base.BackgroundImage = value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public override ImageLayout BackgroundImageLayout
        {
            get
            {
                return base.BackgroundImageLayout;
            }
            set
            {
                base.BackgroundImageLayout = value;
            }
        }

        [Browsable(false), System.Windows.Forms.SRDescription("ContainerControlBindingContextDescr")]
        public override System.Windows.Forms.BindingContext BindingContext
        {
            get
            {
                return base.BindingContextInternal;
            }
            set
            {
                base.BindingContextInternal = value;
            }
        }

        [DispId(-504), System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("SplitterBorderStyleDescr"), DefaultValue(0)]
        public System.Windows.Forms.BorderStyle BorderStyle
        {
            get
            {
                return this.borderStyle;
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 2))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(System.Windows.Forms.BorderStyle));
                }
                if (this.borderStyle != value)
                {
                    this.borderStyle = value;
                    base.Invalidate();
                    this.SetInnerMostBorder(this);
                    if ((this.ParentInternal != null) && (this.ParentInternal is SplitterPanel))
                    {
                        SplitContainer owner = ((SplitterPanel) this.ParentInternal).Owner;
                        owner.SetInnerMostBorder(owner);
                    }
                }
                switch (this.BorderStyle)
                {
                    case System.Windows.Forms.BorderStyle.None:
                        this.BORDERSIZE = 0;
                        return;

                    case System.Windows.Forms.BorderStyle.FixedSingle:
                        this.BORDERSIZE = 1;
                        return;

                    case System.Windows.Forms.BorderStyle.Fixed3D:
                        this.BORDERSIZE = 4;
                        return;
                }
            }
        }

        private bool CollapsedMode
        {
            get
            {
                if (!this.Panel1Collapsed)
                {
                    return this.Panel2Collapsed;
                }
                return true;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never)]
        public Control.ControlCollection Controls
        {
            get
            {
                return base.Controls;
            }
        }

        protected override Size DefaultSize
        {
            get
            {
                return new Size(150, 100);
            }
        }

        public DockStyle Dock
        {
            get
            {
                return base.Dock;
            }
            set
            {
                base.Dock = value;
                if ((this.ParentInternal != null) && (this.ParentInternal is SplitterPanel))
                {
                    SplitContainer owner = ((SplitterPanel) this.ParentInternal).Owner;
                    owner.SetInnerMostBorder(owner);
                }
                this.ResizeSplitContainer();
            }
        }

        [System.Windows.Forms.SRDescription("SplitContainerFixedPanelDescr"), DefaultValue(0), System.Windows.Forms.SRCategory("CatLayout")]
        public System.Windows.Forms.FixedPanel FixedPanel
        {
            get
            {
                return this.fixedPanel;
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 2))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(System.Windows.Forms.FixedPanel));
                }
                if (this.fixedPanel != value)
                {
                    this.fixedPanel = value;
                    if (this.fixedPanel == System.Windows.Forms.FixedPanel.Panel2)
                    {
                        if (this.Orientation == System.Windows.Forms.Orientation.Vertical)
                        {
                            this.panelSize = (base.Width - this.SplitterDistanceInternal) - this.SplitterWidthInternal;
                        }
                        else
                        {
                            this.panelSize = (base.Height - this.SplitterDistanceInternal) - this.SplitterWidthInternal;
                        }
                    }
                    else
                    {
                        this.panelSize = this.SplitterDistanceInternal;
                    }
                }
            }
        }

        internal override bool IsContainerControl
        {
            get
            {
                return true;
            }
        }

        [System.Windows.Forms.SRCategory("CatLayout"), DefaultValue(false), Localizable(true), System.Windows.Forms.SRDescription("SplitContainerIsSplitterFixedDescr")]
        public bool IsSplitterFixed
        {
            get
            {
                return this.splitterFixed;
            }
            set
            {
                this.splitterFixed = value;
            }
        }

        private bool IsSplitterMovable
        {
            get
            {
                if (this.Orientation == System.Windows.Forms.Orientation.Vertical)
                {
                    return (base.Width >= ((this.Panel1MinSize + this.SplitterWidthInternal) + this.Panel2MinSize));
                }
                return (base.Height >= ((this.Panel1MinSize + this.SplitterWidthInternal) + this.Panel2MinSize));
            }
        }

        [System.Windows.Forms.SRDescription("SplitContainerOrientationDescr"), System.Windows.Forms.SRCategory("CatBehavior"), Localizable(true), DefaultValue(1)]
        public System.Windows.Forms.Orientation Orientation
        {
            get
            {
                return this.orientation;
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 1))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(System.Windows.Forms.Orientation));
                }
                if (this.orientation != value)
                {
                    this.orientation = value;
                    this.splitDistance = 0;
                    this.SplitterDistance = this.SplitterDistanceInternal;
                    this.UpdateSplitter();
                }
            }
        }

        private Cursor OverrideCursor
        {
            get
            {
                return this.overrideCursor;
            }
            set
            {
                if (this.overrideCursor != value)
                {
                    this.overrideCursor = value;
                    if (base.IsHandleCreated)
                    {
                        System.Windows.Forms.NativeMethods.POINT pt = new System.Windows.Forms.NativeMethods.POINT();
                        System.Windows.Forms.NativeMethods.RECT rect = new System.Windows.Forms.NativeMethods.RECT();
                        System.Windows.Forms.UnsafeNativeMethods.GetCursorPos(pt);
                        System.Windows.Forms.UnsafeNativeMethods.GetWindowRect(new HandleRef(this, base.Handle), ref rect);
                        if ((((rect.left <= pt.x) && (pt.x < rect.right)) && ((rect.top <= pt.y) && (pt.y < rect.bottom))) || (System.Windows.Forms.UnsafeNativeMethods.GetCapture() == base.Handle))
                        {
                            base.SendMessage(0x20, base.Handle, 1);
                        }
                    }
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public System.Windows.Forms.Padding Padding
        {
            get
            {
                return base.Padding;
            }
            set
            {
                base.Padding = value;
            }
        }

        [Localizable(false), System.Windows.Forms.SRCategory("CatAppearance"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), System.Windows.Forms.SRDescription("SplitContainerPanel1Descr")]
        public SplitterPanel Panel1
        {
            get
            {
                return this.panel1;
            }
        }

        [System.Windows.Forms.SRDescription("SplitContainerPanel1CollapsedDescr"), DefaultValue(false), System.Windows.Forms.SRCategory("CatLayout")]
        public bool Panel1Collapsed
        {
            get
            {
                return this.panel1.Collapsed;
            }
            set
            {
                if (value != this.panel1.Collapsed)
                {
                    if (value && this.panel2.Collapsed)
                    {
                        this.CollapsePanel(this.panel2, false);
                    }
                    this.CollapsePanel(this.panel1, value);
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatLayout"), RefreshProperties(RefreshProperties.All), DefaultValue(0x19), Localizable(true), System.Windows.Forms.SRDescription("SplitContainerPanel1MinSizeDescr")]
        public int Panel1MinSize
        {
            get
            {
                return this.panel1MinSize;
            }
            set
            {
                this.newPanel1MinSize = value;
                if ((value != this.Panel1MinSize) && !this.initializing)
                {
                    this.ApplyPanel1MinSize(value);
                }
            }
        }

        [System.Windows.Forms.SRDescription("SplitContainerPanel2Descr"), System.Windows.Forms.SRCategory("CatAppearance"), Localizable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public SplitterPanel Panel2
        {
            get
            {
                return this.panel2;
            }
        }

        [System.Windows.Forms.SRCategory("CatLayout"), System.Windows.Forms.SRDescription("SplitContainerPanel2CollapsedDescr"), DefaultValue(false)]
        public bool Panel2Collapsed
        {
            get
            {
                return this.panel2.Collapsed;
            }
            set
            {
                if (value != this.panel2.Collapsed)
                {
                    if (value && this.panel1.Collapsed)
                    {
                        this.CollapsePanel(this.panel1, false);
                    }
                    this.CollapsePanel(this.panel2, value);
                }
            }
        }

        [System.Windows.Forms.SRDescription("SplitContainerPanel2MinSizeDescr"), DefaultValue(0x19), Localizable(true), System.Windows.Forms.SRCategory("CatLayout"), RefreshProperties(RefreshProperties.All)]
        public int Panel2MinSize
        {
            get
            {
                return this.panel2MinSize;
            }
            set
            {
                this.newPanel2MinSize = value;
                if ((value != this.Panel2MinSize) && !this.initializing)
                {
                    this.ApplyPanel2MinSize(value);
                }
            }
        }

        [System.Windows.Forms.SRDescription("SplitContainerSplitterDistanceDescr"), Localizable(true), SettingsBindable(true), System.Windows.Forms.SRCategory("CatLayout"), DefaultValue(50)]
        public int SplitterDistance
        {
            get
            {
                return this.splitDistance;
            }
            set
            {
                if (value != this.SplitterDistance)
                {
                    if (value < 0)
                    {
                        throw new ArgumentOutOfRangeException("SplitterDistance", System.Windows.Forms.SR.GetString("InvalidLowBoundArgument", new object[] { "SplitterDistance", value.ToString(CultureInfo.CurrentCulture), "0" }));
                    }
                    try
                    {
                        this.setSplitterDistance = true;
                        if (this.Orientation == System.Windows.Forms.Orientation.Vertical)
                        {
                            if (value < this.Panel1MinSize)
                            {
                                value = this.Panel1MinSize;
                            }
                            if ((value + this.SplitterWidthInternal) > (base.Width - this.Panel2MinSize))
                            {
                                value = (base.Width - this.Panel2MinSize) - this.SplitterWidthInternal;
                            }
                            if (value < 0)
                            {
                                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("SplitterDistanceNotAllowed"));
                            }
                            this.splitDistance = value;
                            this.splitterDistance = value;
                            this.panel1.WidthInternal = this.SplitterDistance;
                        }
                        else
                        {
                            if (value < this.Panel1MinSize)
                            {
                                value = this.Panel1MinSize;
                            }
                            if ((value + this.SplitterWidthInternal) > (base.Height - this.Panel2MinSize))
                            {
                                value = (base.Height - this.Panel2MinSize) - this.SplitterWidthInternal;
                            }
                            if (value < 0)
                            {
                                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("SplitterDistanceNotAllowed"));
                            }
                            this.splitDistance = value;
                            this.splitterDistance = value;
                            this.panel1.HeightInternal = this.SplitterDistance;
                        }
                        switch (this.fixedPanel)
                        {
                            case System.Windows.Forms.FixedPanel.Panel1:
                                this.panelSize = this.SplitterDistance;
                                goto Label_01A6;

                            case System.Windows.Forms.FixedPanel.Panel2:
                                if (this.Orientation != System.Windows.Forms.Orientation.Vertical)
                                {
                                    break;
                                }
                                this.panelSize = (base.Width - this.SplitterDistance) - this.SplitterWidthInternal;
                                goto Label_01A6;

                            default:
                                goto Label_01A6;
                        }
                        this.panelSize = (base.Height - this.SplitterDistance) - this.SplitterWidthInternal;
                    Label_01A6:
                        this.UpdateSplitter();
                    }
                    finally
                    {
                        this.setSplitterDistance = false;
                    }
                    this.OnSplitterMoved(new SplitterEventArgs(this.SplitterRectangle.X + (this.SplitterRectangle.Width / 2), this.SplitterRectangle.Y + (this.SplitterRectangle.Height / 2), this.SplitterRectangle.X, this.SplitterRectangle.Y));
                }
            }
        }

        private int SplitterDistanceInternal
        {
            get
            {
                return this.splitterDistance;
            }
            set
            {
                this.SplitterDistance = value;
            }
        }

        [System.Windows.Forms.SRCategory("CatLayout"), DefaultValue(1), Localizable(true), System.Windows.Forms.SRDescription("SplitContainerSplitterIncrementDescr")]
        public int SplitterIncrement
        {
            get
            {
                return this.splitterInc;
            }
            set
            {
                if (value < 1)
                {
                    throw new ArgumentOutOfRangeException("SplitterIncrement", System.Windows.Forms.SR.GetString("InvalidLowBoundArgumentEx", new object[] { "SplitterIncrement", value.ToString(CultureInfo.CurrentCulture), "1" }));
                }
                this.splitterInc = value;
            }
        }

        [Browsable(false), System.Windows.Forms.SRCategory("CatLayout"), System.Windows.Forms.SRDescription("SplitContainerSplitterRectangleDescr")]
        public Rectangle SplitterRectangle
        {
            get
            {
                Rectangle splitterRect = this.splitterRect;
                splitterRect.X = this.splitterRect.X - base.Left;
                splitterRect.Y = this.splitterRect.Y - base.Top;
                return splitterRect;
            }
        }

        [DefaultValue(4), System.Windows.Forms.SRCategory("CatLayout"), System.Windows.Forms.SRDescription("SplitContainerSplitterWidthDescr"), Localizable(true)]
        public int SplitterWidth
        {
            get
            {
                return this.splitterWidth;
            }
            set
            {
                this.newSplitterWidth = value;
                if ((value != this.SplitterWidth) && !this.initializing)
                {
                    this.ApplySplitterWidth(value);
                }
            }
        }

        private int SplitterWidthInternal
        {
            get
            {
                if (!this.CollapsedMode)
                {
                    return this.splitterWidth;
                }
                return 0;
            }
        }

        [DefaultValue(true), System.Windows.Forms.SRDescription("ControlTabStopDescr"), System.Windows.Forms.SRCategory("CatBehavior"), DispId(-516)]
        public bool TabStop
        {
            get
            {
                return this.tabStop;
            }
            set
            {
                if (this.TabStop != value)
                {
                    this.tabStop = value;
                    this.OnTabStopChanged(EventArgs.Empty);
                }
            }
        }

        [Bindable(false), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public override string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                base.Text = value;
            }
        }

        private class SplitContainerMessageFilter : IMessageFilter
        {
            private SplitContainer owner;

            public SplitContainerMessageFilter(SplitContainer splitContainer)
            {
                this.owner = splitContainer;
            }

            [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            bool IMessageFilter.PreFilterMessage(ref Message m)
            {
                if ((m.Msg < 0x100) || (m.Msg > 0x108))
                {
                    return false;
                }
                if (((m.Msg == 0x100) && (((int) m.WParam) == 0x1b)) || (m.Msg == 260))
                {
                    this.owner.splitBegin = false;
                    this.owner.SplitEnd(false);
                    this.owner.splitterClick = false;
                    this.owner.splitterDrag = false;
                }
                return true;
            }
        }

        internal class SplitContainerTypedControlCollection : WindowsFormsUtils.TypedControlCollection
        {
            private SplitContainer owner;

            public SplitContainerTypedControlCollection(Control c, System.Type type, bool isReadOnly) : base(c, type, isReadOnly)
            {
                this.owner = c as SplitContainer;
            }

            public override void Remove(Control value)
            {
                if (((value is SplitterPanel) && !this.owner.DesignMode) && this.IsReadOnly)
                {
                    throw new NotSupportedException(System.Windows.Forms.SR.GetString("ReadonlyControlsCollection"));
                }
                base.Remove(value);
            }

            internal override void SetChildIndexInternal(Control child, int newIndex)
            {
                if (child is SplitterPanel)
                {
                    if (this.owner.DesignMode)
                    {
                        return;
                    }
                    if (this.IsReadOnly)
                    {
                        throw new NotSupportedException(System.Windows.Forms.SR.GetString("ReadonlyControlsCollection"));
                    }
                }
                base.SetChildIndexInternal(child, newIndex);
            }
        }
    }
}

