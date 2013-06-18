namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [ComVisible(true), ClassInterface(ClassInterfaceType.AutoDispatch), DefaultProperty("Value"), DefaultEvent("Scroll")]
    public abstract class ScrollBar : Control
    {
        private static readonly object EVENT_SCROLL = new object();
        private static readonly object EVENT_VALUECHANGED = new object();
        private int largeChange = 10;
        private int maximum = 100;
        private int minimum;
        private ScrollOrientation scrollOrientation;
        private int smallChange = 1;
        private int value;
        private int wheelDelta;

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
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

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public event EventHandler BackColorChanged
        {
            add
            {
                base.BackColorChanged += value;
            }
            remove
            {
                base.BackColorChanged -= value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
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
        public event EventHandler Click
        {
            add
            {
                base.Click += value;
            }
            remove
            {
                base.Click -= value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event EventHandler DoubleClick
        {
            add
            {
                base.DoubleClick += value;
            }
            remove
            {
                base.DoubleClick -= value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
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

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
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
        public event MouseEventHandler MouseClick
        {
            add
            {
                base.MouseClick += value;
            }
            remove
            {
                base.MouseClick -= value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public event MouseEventHandler MouseDoubleClick
        {
            add
            {
                base.MouseDoubleClick += value;
            }
            remove
            {
                base.MouseDoubleClick -= value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event MouseEventHandler MouseDown
        {
            add
            {
                base.MouseDown += value;
            }
            remove
            {
                base.MouseDown -= value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public event MouseEventHandler MouseMove
        {
            add
            {
                base.MouseMove += value;
            }
            remove
            {
                base.MouseMove -= value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event MouseEventHandler MouseUp
        {
            add
            {
                base.MouseUp += value;
            }
            remove
            {
                base.MouseUp -= value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event PaintEventHandler Paint
        {
            add
            {
                base.Paint += value;
            }
            remove
            {
                base.Paint -= value;
            }
        }

        [System.Windows.Forms.SRDescription("ScrollBarOnScrollDescr"), System.Windows.Forms.SRCategory("CatAction")]
        public event ScrollEventHandler Scroll
        {
            add
            {
                base.Events.AddHandler(EVENT_SCROLL, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_SCROLL, value);
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

        [System.Windows.Forms.SRCategory("CatAction"), System.Windows.Forms.SRDescription("valueChangedEventDescr")]
        public event EventHandler ValueChanged
        {
            add
            {
                base.Events.AddHandler(EVENT_VALUECHANGED, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_VALUECHANGED, value);
            }
        }

        public ScrollBar()
        {
            base.SetStyle(ControlStyles.UserPaint, false);
            base.SetStyle(ControlStyles.StandardClick, false);
            base.SetStyle(ControlStyles.UseTextForAccessibility, false);
            this.TabStop = false;
            if ((this.CreateParams.Style & 1) != 0)
            {
                this.scrollOrientation = ScrollOrientation.VerticalScroll;
            }
            else
            {
                this.scrollOrientation = ScrollOrientation.HorizontalScroll;
            }
        }

        private void DoScroll(ScrollEventType type)
        {
            if (this.RightToLeft == RightToLeft.Yes)
            {
                switch (type)
                {
                    case ScrollEventType.SmallDecrement:
                        type = ScrollEventType.SmallIncrement;
                        break;

                    case ScrollEventType.SmallIncrement:
                        type = ScrollEventType.SmallDecrement;
                        break;

                    case ScrollEventType.LargeDecrement:
                        type = ScrollEventType.LargeIncrement;
                        break;

                    case ScrollEventType.LargeIncrement:
                        type = ScrollEventType.LargeDecrement;
                        break;

                    case ScrollEventType.First:
                        type = ScrollEventType.Last;
                        break;

                    case ScrollEventType.Last:
                        type = ScrollEventType.First;
                        break;
                }
            }
            int newValue = this.value;
            int oldValue = this.value;
            switch (type)
            {
                case ScrollEventType.SmallDecrement:
                    newValue = Math.Max(this.value - this.SmallChange, this.minimum);
                    break;

                case ScrollEventType.SmallIncrement:
                    newValue = Math.Min((int) (this.value + this.SmallChange), (int) ((this.maximum - this.LargeChange) + 1));
                    break;

                case ScrollEventType.LargeDecrement:
                    newValue = Math.Max(this.value - this.LargeChange, this.minimum);
                    break;

                case ScrollEventType.LargeIncrement:
                    newValue = Math.Min((int) (this.value + this.LargeChange), (int) ((this.maximum - this.LargeChange) + 1));
                    break;

                case ScrollEventType.ThumbPosition:
                case ScrollEventType.ThumbTrack:
                {
                    System.Windows.Forms.NativeMethods.SCROLLINFO si = new System.Windows.Forms.NativeMethods.SCROLLINFO {
                        fMask = 0x10
                    };
                    System.Windows.Forms.SafeNativeMethods.GetScrollInfo(new HandleRef(this, base.Handle), 2, si);
                    if (this.RightToLeft != RightToLeft.Yes)
                    {
                        newValue = si.nTrackPos;
                        break;
                    }
                    newValue = this.ReflectPosition(si.nTrackPos);
                    break;
                }
                case ScrollEventType.First:
                    newValue = this.minimum;
                    break;

                case ScrollEventType.Last:
                    newValue = (this.maximum - this.LargeChange) + 1;
                    break;
            }
            ScrollEventArgs se = new ScrollEventArgs(type, oldValue, newValue, this.scrollOrientation);
            this.OnScroll(se);
            this.Value = se.NewValue;
        }

        protected override Rectangle GetScaledBounds(Rectangle bounds, SizeF factor, BoundsSpecified specified)
        {
            if (this.scrollOrientation == ScrollOrientation.VerticalScroll)
            {
                specified &= ~BoundsSpecified.Width;
            }
            else
            {
                specified &= ~BoundsSpecified.Height;
            }
            return base.GetScaledBounds(bounds, factor, specified);
        }

        internal override IntPtr InitializeDCForWmCtlColor(IntPtr dc, int msg)
        {
            return IntPtr.Zero;
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            if (base.Enabled)
            {
                this.UpdateScrollInfo();
            }
            base.OnEnabledChanged(e);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            this.UpdateScrollInfo();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            this.wheelDelta += e.Delta;
            bool flag = false;
            while (Math.Abs(this.wheelDelta) >= 120)
            {
                if (this.wheelDelta > 0)
                {
                    this.wheelDelta -= 120;
                    this.DoScroll(ScrollEventType.SmallDecrement);
                    flag = true;
                }
                else
                {
                    this.wheelDelta += 120;
                    this.DoScroll(ScrollEventType.SmallIncrement);
                    flag = true;
                }
            }
            if (flag)
            {
                this.DoScroll(ScrollEventType.EndScroll);
            }
            if (e is HandledMouseEventArgs)
            {
                ((HandledMouseEventArgs) e).Handled = true;
            }
            base.OnMouseWheel(e);
        }

        protected virtual void OnScroll(ScrollEventArgs se)
        {
            ScrollEventHandler handler = (ScrollEventHandler) base.Events[EVENT_SCROLL];
            if (handler != null)
            {
                handler(this, se);
            }
        }

        protected virtual void OnValueChanged(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EVENT_VALUECHANGED];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private int ReflectPosition(int position)
        {
            if (this is HScrollBar)
            {
                return ((this.minimum + ((this.maximum - this.LargeChange) + 1)) - position);
            }
            return position;
        }

        public override string ToString()
        {
            string str = base.ToString();
            return (str + ", Minimum: " + this.Minimum.ToString(CultureInfo.CurrentCulture) + ", Maximum: " + this.Maximum.ToString(CultureInfo.CurrentCulture) + ", Value: " + this.Value.ToString(CultureInfo.CurrentCulture));
        }

        protected void UpdateScrollInfo()
        {
            if (base.IsHandleCreated && base.Enabled)
            {
                System.Windows.Forms.NativeMethods.SCROLLINFO si = new System.Windows.Forms.NativeMethods.SCROLLINFO {
                    cbSize = Marshal.SizeOf(typeof(System.Windows.Forms.NativeMethods.SCROLLINFO)),
                    fMask = 0x17,
                    nMin = this.minimum,
                    nMax = this.maximum,
                    nPage = this.LargeChange
                };
                if (this.RightToLeft == RightToLeft.Yes)
                {
                    si.nPos = this.ReflectPosition(this.value);
                }
                else
                {
                    si.nPos = this.value;
                }
                si.nTrackPos = 0;
                System.Windows.Forms.UnsafeNativeMethods.SetScrollInfo(new HandleRef(this, base.Handle), 2, si, true);
            }
        }

        private void WmReflectScroll(ref Message m)
        {
            ScrollEventType type = (ScrollEventType) System.Windows.Forms.NativeMethods.Util.LOWORD(m.WParam);
            this.DoScroll(type);
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case 5:
                    if (!(System.Windows.Forms.UnsafeNativeMethods.GetFocus() == base.Handle))
                    {
                        break;
                    }
                    this.DefWndProc(ref m);
                    base.SendMessage(8, 0, 0);
                    base.SendMessage(7, 0, 0);
                    return;

                case 0x2114:
                case 0x2115:
                    this.WmReflectScroll(ref m);
                    return;

                case 20:
                    break;

                default:
                    base.WndProc(ref m);
                    break;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public override Color BackColor
        {
            get
            {
                return base.BackColor;
            }
            set
            {
                base.BackColor = value;
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

        protected override System.Windows.Forms.CreateParams CreateParams
        {
            [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            get
            {
                System.Windows.Forms.CreateParams createParams = base.CreateParams;
                createParams.ClassName = "SCROLLBAR";
                createParams.Style &= -8388609;
                return createParams;
            }
        }

        protected override System.Windows.Forms.ImeMode DefaultImeMode
        {
            get
            {
                return System.Windows.Forms.ImeMode.Disable;
            }
        }

        protected override Padding DefaultMargin
        {
            get
            {
                return Padding.Empty;
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

        [System.Windows.Forms.SRDescription("ScrollBarLargeChangeDescr"), System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue(10), RefreshProperties(RefreshProperties.Repaint)]
        public int LargeChange
        {
            get
            {
                return Math.Min(this.largeChange, (this.maximum - this.minimum) + 1);
            }
            set
            {
                if (this.largeChange != value)
                {
                    if (value < 0)
                    {
                        object[] args = new object[] { "LargeChange", value.ToString(CultureInfo.CurrentCulture), 0.ToString(CultureInfo.CurrentCulture) };
                        throw new ArgumentOutOfRangeException("LargeChange", System.Windows.Forms.SR.GetString("InvalidLowBoundArgumentEx", args));
                    }
                    this.largeChange = value;
                    this.UpdateScrollInfo();
                }
            }
        }

        [System.Windows.Forms.SRDescription("ScrollBarMaximumDescr"), RefreshProperties(RefreshProperties.Repaint), System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue(100)]
        public int Maximum
        {
            get
            {
                return this.maximum;
            }
            set
            {
                if (this.maximum != value)
                {
                    if (this.minimum > value)
                    {
                        this.minimum = value;
                    }
                    if (value < this.value)
                    {
                        this.Value = value;
                    }
                    this.maximum = value;
                    this.UpdateScrollInfo();
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), RefreshProperties(RefreshProperties.Repaint), DefaultValue(0), System.Windows.Forms.SRDescription("ScrollBarMinimumDescr")]
        public int Minimum
        {
            get
            {
                return this.minimum;
            }
            set
            {
                if (this.minimum != value)
                {
                    if (this.maximum < value)
                    {
                        this.maximum = value;
                    }
                    if (value > this.value)
                    {
                        this.value = value;
                    }
                    this.minimum = value;
                    this.UpdateScrollInfo();
                }
            }
        }

        [System.Windows.Forms.SRDescription("ScrollBarSmallChangeDescr"), System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue(1)]
        public int SmallChange
        {
            get
            {
                return Math.Min(this.smallChange, this.LargeChange);
            }
            set
            {
                if (this.smallChange != value)
                {
                    if (value < 0)
                    {
                        object[] args = new object[] { "SmallChange", value.ToString(CultureInfo.CurrentCulture), 0.ToString(CultureInfo.CurrentCulture) };
                        throw new ArgumentOutOfRangeException("SmallChange", System.Windows.Forms.SR.GetString("InvalidLowBoundArgumentEx", args));
                    }
                    this.smallChange = value;
                    this.UpdateScrollInfo();
                }
            }
        }

        [DefaultValue(false)]
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

        [Bindable(true), System.Windows.Forms.SRDescription("ScrollBarValueDescr"), System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue(0)]
        public int Value
        {
            get
            {
                return this.value;
            }
            set
            {
                if (this.value != value)
                {
                    if ((value < this.minimum) || (value > this.maximum))
                    {
                        throw new ArgumentOutOfRangeException("Value", System.Windows.Forms.SR.GetString("InvalidBoundArgument", new object[] { "Value", value.ToString(CultureInfo.CurrentCulture), "'minimum'", "'maximum'" }));
                    }
                    this.value = value;
                    this.UpdateScrollInfo();
                    this.OnValueChanged(EventArgs.Empty);
                }
            }
        }
    }
}

