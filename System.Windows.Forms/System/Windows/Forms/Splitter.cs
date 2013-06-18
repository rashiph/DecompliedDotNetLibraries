namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;

    [ComVisible(true), Designer("System.Windows.Forms.Design.SplitterDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), DefaultEvent("SplitterMoved"), System.Windows.Forms.SRDescription("DescriptionSplitter"), ClassInterface(ClassInterfaceType.AutoDispatch), DefaultProperty("Dock")]
    public class Splitter : Control
    {
        private Point anchor = Point.Empty;
        private System.Windows.Forms.BorderStyle borderStyle;
        private const int defaultWidth = 3;
        private const int DRAW_END = 3;
        private const int DRAW_MOVE = 2;
        private const int DRAW_START = 1;
        private static readonly object EVENT_MOVED = new object();
        private static readonly object EVENT_MOVING = new object();
        private int initTargetSize;
        private int lastDrawSplit = -1;
        private int maxSize;
        private int minExtra = 0x19;
        private int minSize = 0x19;
        private int splitSize = -1;
        private Control splitTarget;
        private SplitterMessageFilter splitterMessageFilter;
        private int splitterThickness = 3;

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
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
        public event EventHandler Enter
        {
            add
            {
                base.Enter += value;
            }
            remove
            {
                base.Enter -= value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event EventHandler FontChanged
        {
            add
            {
                base.FontChanged += value;
            }
            remove
            {
                base.FontChanged -= value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event EventHandler ForeColorChanged
        {
            add
            {
                base.ForeColorChanged += value;
            }
            remove
            {
                base.ForeColorChanged -= value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event EventHandler ImeModeChanged
        {
            add
            {
                base.ImeModeChanged += value;
            }
            remove
            {
                base.ImeModeChanged -= value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event KeyEventHandler KeyDown
        {
            add
            {
                base.KeyDown += value;
            }
            remove
            {
                base.KeyDown -= value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public event KeyPressEventHandler KeyPress
        {
            add
            {
                base.KeyPress += value;
            }
            remove
            {
                base.KeyPress -= value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event KeyEventHandler KeyUp
        {
            add
            {
                base.KeyUp += value;
            }
            remove
            {
                base.KeyUp -= value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public event EventHandler Leave
        {
            add
            {
                base.Leave += value;
            }
            remove
            {
                base.Leave -= value;
            }
        }

        [System.Windows.Forms.SRDescription("SplitterSplitterMovedDescr"), System.Windows.Forms.SRCategory("CatBehavior")]
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
        public event SplitterEventHandler SplitterMoving
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

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event EventHandler TabStopChanged
        {
            add
            {
                base.TabStopChanged += value;
            }
            remove
            {
                base.TabStopChanged -= value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
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

        public Splitter()
        {
            base.SetStyle(ControlStyles.Selectable, false);
            this.TabStop = false;
            this.minSize = 0x19;
            this.minExtra = 0x19;
            this.Dock = DockStyle.Left;
        }

        private void ApplySplitPosition()
        {
            this.SplitPosition = this.splitSize;
        }

        private SplitData CalcSplitBounds()
        {
            SplitData data = new SplitData();
            Control control = this.FindTarget();
            data.target = control;
            if (control != null)
            {
                switch (control.Dock)
                {
                    case DockStyle.Top:
                    case DockStyle.Bottom:
                        this.initTargetSize = control.Bounds.Height;
                        break;

                    case DockStyle.Left:
                    case DockStyle.Right:
                        this.initTargetSize = control.Bounds.Width;
                        break;
                }
                Control parentInternal = this.ParentInternal;
                Control.ControlCollection controls = parentInternal.Controls;
                int count = controls.Count;
                int num2 = 0;
                int num3 = 0;
                for (int i = 0; i < count; i++)
                {
                    Control control3 = controls[i];
                    if (control3 != control)
                    {
                        switch (control3.Dock)
                        {
                            case DockStyle.Top:
                            case DockStyle.Bottom:
                                num3 += control3.Height;
                                break;

                            case DockStyle.Left:
                            case DockStyle.Right:
                                num2 += control3.Width;
                                break;
                        }
                    }
                }
                Size clientSize = parentInternal.ClientSize;
                if (this.Horizontal)
                {
                    this.maxSize = (clientSize.Width - num2) - this.minExtra;
                }
                else
                {
                    this.maxSize = (clientSize.Height - num3) - this.minExtra;
                }
                data.dockWidth = num2;
                data.dockHeight = num3;
            }
            return data;
        }

        private Rectangle CalcSplitLine(int splitSize, int minWeight)
        {
            Rectangle bounds = base.Bounds;
            Rectangle rectangle2 = this.splitTarget.Bounds;
            switch (this.Dock)
            {
                case DockStyle.Top:
                    if (bounds.Height < minWeight)
                    {
                        bounds.Height = minWeight;
                    }
                    bounds.Y = rectangle2.Y + splitSize;
                    return bounds;

                case DockStyle.Bottom:
                    if (bounds.Height < minWeight)
                    {
                        bounds.Height = minWeight;
                    }
                    bounds.Y = ((rectangle2.Y + rectangle2.Height) - splitSize) - bounds.Height;
                    return bounds;

                case DockStyle.Left:
                    if (bounds.Width < minWeight)
                    {
                        bounds.Width = minWeight;
                    }
                    bounds.X = rectangle2.X + splitSize;
                    return bounds;

                case DockStyle.Right:
                    if (bounds.Width < minWeight)
                    {
                        bounds.Width = minWeight;
                    }
                    bounds.X = ((rectangle2.X + rectangle2.Width) - splitSize) - bounds.Width;
                    return bounds;
            }
            return bounds;
        }

        private int CalcSplitSize()
        {
            Control control = this.FindTarget();
            if (control != null)
            {
                Rectangle bounds = control.Bounds;
                switch (this.Dock)
                {
                    case DockStyle.Top:
                    case DockStyle.Bottom:
                        return bounds.Height;

                    case DockStyle.Left:
                    case DockStyle.Right:
                        return bounds.Width;
                }
            }
            return -1;
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
                this.DrawSplitHelper(this.splitSize);
                this.lastDrawSplit = this.splitSize;
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
            if (this.splitTarget != null)
            {
                Rectangle rectangle = this.CalcSplitLine(splitSize, 3);
                IntPtr handle = this.ParentInternal.Handle;
                IntPtr ptr2 = System.Windows.Forms.UnsafeNativeMethods.GetDCEx(new HandleRef(this.ParentInternal, handle), System.Windows.Forms.NativeMethods.NullHandleRef, 0x402);
                IntPtr ptr3 = ControlPaint.CreateHalftoneHBRUSH();
                IntPtr ptr4 = System.Windows.Forms.SafeNativeMethods.SelectObject(new HandleRef(this.ParentInternal, ptr2), new HandleRef(null, ptr3));
                System.Windows.Forms.SafeNativeMethods.PatBlt(new HandleRef(this.ParentInternal, ptr2), rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, 0x5a0049);
                System.Windows.Forms.SafeNativeMethods.SelectObject(new HandleRef(this.ParentInternal, ptr2), new HandleRef(null, ptr4));
                System.Windows.Forms.SafeNativeMethods.DeleteObject(new HandleRef(null, ptr3));
                System.Windows.Forms.UnsafeNativeMethods.ReleaseDC(new HandleRef(this.ParentInternal, handle), new HandleRef(null, ptr2));
            }
        }

        private Control FindTarget()
        {
            Control parentInternal = this.ParentInternal;
            if (parentInternal != null)
            {
                Control.ControlCollection controls = parentInternal.Controls;
                int count = controls.Count;
                DockStyle dock = this.Dock;
                for (int i = 0; i < count; i++)
                {
                    Control control2 = controls[i];
                    if (control2 != this)
                    {
                        switch (dock)
                        {
                            case DockStyle.Top:
                                if (control2.Bottom != base.Top)
                                {
                                    break;
                                }
                                return control2;

                            case DockStyle.Bottom:
                                if (control2.Top != base.Bottom)
                                {
                                    break;
                                }
                                return control2;

                            case DockStyle.Left:
                                if (control2.Right != base.Left)
                                {
                                    break;
                                }
                                return control2;

                            case DockStyle.Right:
                                if (control2.Left != base.Right)
                                {
                                    break;
                                }
                                return control2;
                        }
                    }
                }
            }
            return null;
        }

        private int GetSplitSize(int x, int y)
        {
            int num;
            if (this.Horizontal)
            {
                num = x - this.anchor.X;
            }
            else
            {
                num = y - this.anchor.Y;
            }
            int num2 = 0;
            switch (this.Dock)
            {
                case DockStyle.Top:
                    num2 = this.splitTarget.Height + num;
                    break;

                case DockStyle.Bottom:
                    num2 = this.splitTarget.Height - num;
                    break;

                case DockStyle.Left:
                    num2 = this.splitTarget.Width + num;
                    break;

                case DockStyle.Right:
                    num2 = this.splitTarget.Width - num;
                    break;
            }
            return Math.Max(Math.Min(num2, this.maxSize), this.minSize);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if ((this.splitTarget != null) && (e.KeyCode == Keys.Escape))
            {
                this.SplitEnd(false);
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if ((e.Button == MouseButtons.Left) && (e.Clicks == 1))
            {
                this.SplitBegin(e.X, e.Y);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (this.splitTarget != null)
            {
                int x = e.X + base.Left;
                int y = e.Y + base.Top;
                Rectangle rectangle = this.CalcSplitLine(this.GetSplitSize(e.X, e.Y), 0);
                int splitX = rectangle.X;
                int splitY = rectangle.Y;
                this.OnSplitterMoving(new SplitterEventArgs(x, y, splitX, splitY));
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (this.splitTarget != null)
            {
                int x = e.X;
                int left = base.Left;
                int y = e.Y;
                int top = base.Top;
                Rectangle rectangle = this.CalcSplitLine(this.GetSplitSize(e.X, e.Y), 0);
                int num5 = rectangle.X;
                int num6 = rectangle.Y;
                this.SplitEnd(true);
            }
        }

        protected virtual void OnSplitterMoved(SplitterEventArgs sevent)
        {
            SplitterEventHandler handler = (SplitterEventHandler) base.Events[EVENT_MOVED];
            if (handler != null)
            {
                handler(this, sevent);
            }
            if (this.splitTarget != null)
            {
                this.SplitMove(sevent.SplitX, sevent.SplitY);
            }
        }

        protected virtual void OnSplitterMoving(SplitterEventArgs sevent)
        {
            SplitterEventHandler handler = (SplitterEventHandler) base.Events[EVENT_MOVING];
            if (handler != null)
            {
                handler(this, sevent);
            }
            if (this.splitTarget != null)
            {
                this.SplitMove(sevent.SplitX, sevent.SplitY);
            }
        }

        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            if (this.Horizontal)
            {
                if (width < 1)
                {
                    width = 3;
                }
                this.splitterThickness = width;
            }
            else
            {
                if (height < 1)
                {
                    height = 3;
                }
                this.splitterThickness = height;
            }
            base.SetBoundsCore(x, y, width, height, specified);
        }

        private void SplitBegin(int x, int y)
        {
            SplitData data = this.CalcSplitBounds();
            if ((data.target != null) && (this.minSize < this.maxSize))
            {
                this.anchor = new Point(x, y);
                this.splitTarget = data.target;
                this.splitSize = this.GetSplitSize(x, y);
                System.Windows.Forms.IntSecurity.UnmanagedCode.Assert();
                try
                {
                    if (this.splitterMessageFilter != null)
                    {
                        this.splitterMessageFilter = new SplitterMessageFilter(this);
                    }
                    Application.AddMessageFilter(this.splitterMessageFilter);
                }
                finally
                {
                    CodeAccessPermission.RevertAssert();
                }
                base.CaptureInternal = true;
                this.DrawSplitBar(1);
            }
        }

        private void SplitEnd(bool accept)
        {
            this.DrawSplitBar(3);
            this.splitTarget = null;
            base.CaptureInternal = false;
            if (this.splitterMessageFilter != null)
            {
                Application.RemoveMessageFilter(this.splitterMessageFilter);
                this.splitterMessageFilter = null;
            }
            if (accept)
            {
                this.ApplySplitPosition();
            }
            else if (this.splitSize != this.initTargetSize)
            {
                this.SplitPosition = this.initTargetSize;
            }
            this.anchor = Point.Empty;
        }

        private void SplitMove(int x, int y)
        {
            int splitSize = this.GetSplitSize((x - base.Left) + this.anchor.X, (y - base.Top) + this.anchor.Y);
            if (this.splitSize != splitSize)
            {
                this.splitSize = splitSize;
                this.DrawSplitBar(2);
            }
        }

        public override string ToString()
        {
            string str = base.ToString();
            return (str + ", MinExtra: " + this.MinExtra.ToString(CultureInfo.CurrentCulture) + ", MinSize: " + this.MinSize.ToString(CultureInfo.CurrentCulture));
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public override bool AllowDrop
        {
            get
            {
                return base.AllowDrop;
            }
            set
            {
                base.AllowDrop = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false), DefaultValue(0)]
        public override AnchorStyles Anchor
        {
            get
            {
                return AnchorStyles.None;
            }
            set
            {
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
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

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
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

        [System.Windows.Forms.SRDescription("SplitterBorderStyleDescr"), DispId(-504), DefaultValue(0), System.Windows.Forms.SRCategory("CatAppearance")]
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
                    base.UpdateStyles();
                }
            }
        }

        protected override System.Windows.Forms.CreateParams CreateParams
        {
            [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            get
            {
                System.Windows.Forms.CreateParams createParams = base.CreateParams;
                createParams.ExStyle &= -513;
                createParams.Style &= -8388609;
                switch (this.borderStyle)
                {
                    case System.Windows.Forms.BorderStyle.FixedSingle:
                        createParams.Style |= 0x800000;
                        return createParams;

                    case System.Windows.Forms.BorderStyle.Fixed3D:
                        createParams.ExStyle |= 0x200;
                        return createParams;
                }
                return createParams;
            }
        }

        protected override Cursor DefaultCursor
        {
            get
            {
                switch (this.Dock)
                {
                    case DockStyle.Top:
                    case DockStyle.Bottom:
                        return Cursors.HSplit;

                    case DockStyle.Left:
                    case DockStyle.Right:
                        return Cursors.VSplit;
                }
                return base.DefaultCursor;
            }
        }

        protected override System.Windows.Forms.ImeMode DefaultImeMode
        {
            get
            {
                return System.Windows.Forms.ImeMode.Disable;
            }
        }

        protected override Size DefaultSize
        {
            get
            {
                return new Size(3, 3);
            }
        }

        [DefaultValue(3), Localizable(true)]
        public override DockStyle Dock
        {
            get
            {
                return base.Dock;
            }
            set
            {
                if (((value != DockStyle.Top) && (value != DockStyle.Bottom)) && ((value != DockStyle.Left) && (value != DockStyle.Right)))
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("SplitterInvalidDockEnum"));
                }
                int splitterThickness = this.splitterThickness;
                base.Dock = value;
                switch (this.Dock)
                {
                    case DockStyle.Top:
                    case DockStyle.Bottom:
                        if (this.splitterThickness == -1)
                        {
                            break;
                        }
                        base.Height = splitterThickness;
                        return;

                    case DockStyle.Left:
                    case DockStyle.Right:
                        if (this.splitterThickness != -1)
                        {
                            base.Width = splitterThickness;
                        }
                        break;

                    default:
                        return;
                }
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public override System.Drawing.Font Font
        {
            get
            {
                return base.Font;
            }
            set
            {
                base.Font = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public override Color ForeColor
        {
            get
            {
                return base.ForeColor;
            }
            set
            {
                base.ForeColor = value;
            }
        }

        private bool Horizontal
        {
            get
            {
                DockStyle dock = this.Dock;
                if (dock != DockStyle.Left)
                {
                    return (dock == DockStyle.Right);
                }
                return true;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public System.Windows.Forms.ImeMode ImeMode
        {
            get
            {
                return base.ImeMode;
            }
            set
            {
                base.ImeMode = value;
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("SplitterMinExtraDescr"), Localizable(true), DefaultValue(0x19)]
        public int MinExtra
        {
            get
            {
                return this.minExtra;
            }
            set
            {
                if (value < 0)
                {
                    value = 0;
                }
                this.minExtra = value;
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("SplitterMinSizeDescr"), Localizable(true), DefaultValue(0x19)]
        public int MinSize
        {
            get
            {
                return this.minSize;
            }
            set
            {
                if (value < 0)
                {
                    value = 0;
                }
                this.minSize = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), System.Windows.Forms.SRCategory("CatLayout"), Browsable(false), System.Windows.Forms.SRDescription("SplitterSplitPositionDescr")]
        public int SplitPosition
        {
            get
            {
                if (this.splitSize == -1)
                {
                    this.splitSize = this.CalcSplitSize();
                }
                return this.splitSize;
            }
            set
            {
                SplitData data = this.CalcSplitBounds();
                if (value > this.maxSize)
                {
                    value = this.maxSize;
                }
                if (value < this.minSize)
                {
                    value = this.minSize;
                }
                this.splitSize = value;
                this.DrawSplitBar(3);
                if (data.target == null)
                {
                    this.splitSize = -1;
                }
                else
                {
                    Rectangle bounds = data.target.Bounds;
                    switch (this.Dock)
                    {
                        case DockStyle.Top:
                            bounds.Height = value;
                            break;

                        case DockStyle.Bottom:
                            bounds.Y += bounds.Height - this.splitSize;
                            bounds.Height = value;
                            break;

                        case DockStyle.Left:
                            bounds.Width = value;
                            break;

                        case DockStyle.Right:
                            bounds.X += bounds.Width - this.splitSize;
                            bounds.Width = value;
                            break;
                    }
                    data.target.Bounds = bounds;
                    Application.DoEvents();
                    this.OnSplitterMoved(new SplitterEventArgs(base.Left, base.Top, base.Left + (bounds.Width / 2), base.Top + (bounds.Height / 2)));
                }
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public bool TabStop
        {
            get
            {
                return base.TabStop;
            }
            set
            {
                base.TabStop = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false), EditorBrowsable(EditorBrowsableState.Never), Bindable(false)]
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

        private class SplitData
        {
            public int dockHeight = -1;
            public int dockWidth = -1;
            internal Control target;
        }

        private class SplitterMessageFilter : IMessageFilter
        {
            private Splitter owner;

            public SplitterMessageFilter(Splitter splitter)
            {
                this.owner = splitter;
            }

            [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            public bool PreFilterMessage(ref Message m)
            {
                if ((m.Msg < 0x100) || (m.Msg > 0x108))
                {
                    return false;
                }
                if ((m.Msg == 0x100) && (((int) ((long) m.WParam)) == 0x1b))
                {
                    this.owner.SplitEnd(false);
                }
                return true;
            }
        }
    }
}

