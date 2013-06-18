namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Windows.Forms.Layout;

    [ComVisible(true), DefaultProperty("Value"), DefaultEvent("Scroll"), System.Windows.Forms.SRDescription("DescriptionTrackBar"), ClassInterface(ClassInterfaceType.AutoDispatch), DefaultBindingProperty("Value"), Designer("System.Windows.Forms.Design.TrackBarDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    public class TrackBar : Control, ISupportInitialize
    {
        private bool autoSize = true;
        private int cumulativeWheelData;
        private static readonly object EVENT_RIGHTTOLEFTLAYOUTCHANGED = new object();
        private static readonly object EVENT_SCROLL = new object();
        private static readonly object EVENT_VALUECHANGED = new object();
        private bool initializing;
        private int largeChange = 5;
        private int maximum = 10;
        private int minimum;
        private System.Windows.Forms.Orientation orientation;
        private int requestedDim;
        private bool rightToLeftLayout;
        private int smallChange = 1;
        private int tickFrequency = 1;
        private System.Windows.Forms.TickStyle tickStyle = System.Windows.Forms.TickStyle.BottomRight;
        private int value;

        [System.Windows.Forms.SRCategory("CatPropertyChanged"), EditorBrowsable(EditorBrowsableState.Always), System.Windows.Forms.SRDescription("ControlOnAutoSizeChangedDescr"), Browsable(true)]
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

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
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

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
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

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
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

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
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

        [System.Windows.Forms.SRCategory("CatPropertyChanged"), System.Windows.Forms.SRDescription("ControlOnRightToLeftLayoutChangedDescr")]
        public event EventHandler RightToLeftLayoutChanged
        {
            add
            {
                base.Events.AddHandler(EVENT_RIGHTTOLEFTLAYOUTCHANGED, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_RIGHTTOLEFTLAYOUTCHANGED, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("TrackBarOnScrollDescr")]
        public event EventHandler Scroll
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

        public TrackBar()
        {
            base.SetStyle(ControlStyles.UserPaint, false);
            base.SetStyle(ControlStyles.UseTextForAccessibility, false);
            this.requestedDim = this.PreferredDimension;
        }

        private void AdjustSize()
        {
            if (base.IsHandleCreated)
            {
                int requestedDim = this.requestedDim;
                try
                {
                    if (this.orientation == System.Windows.Forms.Orientation.Horizontal)
                    {
                        base.Height = this.autoSize ? this.PreferredDimension : requestedDim;
                    }
                    else
                    {
                        base.Width = this.autoSize ? this.PreferredDimension : requestedDim;
                    }
                }
                finally
                {
                    this.requestedDim = requestedDim;
                }
            }
        }

        public void BeginInit()
        {
            this.initializing = true;
        }

        private void ConstrainValue()
        {
            if (!this.initializing)
            {
                if (this.Value < this.minimum)
                {
                    this.Value = this.minimum;
                }
                if (this.Value > this.maximum)
                {
                    this.Value = this.maximum;
                }
            }
        }

        protected override void CreateHandle()
        {
            if (!base.RecreatingHandle)
            {
                IntPtr userCookie = System.Windows.Forms.UnsafeNativeMethods.ThemingScope.Activate();
                try
                {
                    System.Windows.Forms.NativeMethods.INITCOMMONCONTROLSEX icc = new System.Windows.Forms.NativeMethods.INITCOMMONCONTROLSEX {
                        dwICC = 4
                    };
                    System.Windows.Forms.SafeNativeMethods.InitCommonControlsEx(icc);
                }
                finally
                {
                    System.Windows.Forms.UnsafeNativeMethods.ThemingScope.Deactivate(userCookie);
                }
            }
            base.CreateHandle();
        }

        public void EndInit()
        {
            this.initializing = false;
            this.ConstrainValue();
        }

        private void GetTrackBarValue()
        {
            if (base.IsHandleCreated)
            {
                this.value = (int) ((long) base.SendMessage(0x400, 0, 0));
                if (this.orientation == System.Windows.Forms.Orientation.Vertical)
                {
                    this.value = (this.Minimum + this.Maximum) - this.value;
                }
                if (((this.orientation == System.Windows.Forms.Orientation.Horizontal) && (this.RightToLeft == RightToLeft.Yes)) && !base.IsMirrored)
                {
                    this.value = (this.Minimum + this.Maximum) - this.value;
                }
            }
        }

        protected override bool IsInputKey(Keys keyData)
        {
            if ((keyData & Keys.Alt) == Keys.Alt)
            {
                return false;
            }
            switch ((keyData & Keys.KeyCode))
            {
                case Keys.PageUp:
                case Keys.Next:
                case Keys.End:
                case Keys.Home:
                    return true;
            }
            return base.IsInputKey(keyData);
        }

        protected override void OnBackColorChanged(EventArgs e)
        {
            base.OnBackColorChanged(e);
            this.RedrawControl();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            base.SendMessage(0x407, 0, this.minimum);
            base.SendMessage(0x408, 0, this.maximum);
            base.SendMessage(0x414, this.tickFrequency, 0);
            base.SendMessage(0x415, 0, this.largeChange);
            base.SendMessage(0x417, 0, this.smallChange);
            this.SetTrackBarPosition();
            this.AdjustSize();
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            HandledMouseEventArgs args = e as HandledMouseEventArgs;
            if (args != null)
            {
                if (args.Handled)
                {
                    return;
                }
                args.Handled = true;
            }
            if (((Control.ModifierKeys & (Keys.Alt | Keys.Shift)) == Keys.None) && (Control.MouseButtons == MouseButtons.None))
            {
                int mouseWheelScrollLines = SystemInformation.MouseWheelScrollLines;
                if (mouseWheelScrollLines != 0)
                {
                    this.cumulativeWheelData += e.Delta;
                    float num2 = ((float) this.cumulativeWheelData) / 120f;
                    if (mouseWheelScrollLines == -1)
                    {
                        mouseWheelScrollLines = this.TickFrequency;
                    }
                    int num3 = (int) (mouseWheelScrollLines * num2);
                    if (num3 != 0)
                    {
                        int num4;
                        if (num3 > 0)
                        {
                            num4 = num3;
                            this.Value = Math.Min(num4 + this.Value, this.Maximum);
                            this.cumulativeWheelData -= (int) (num3 * (120f / ((float) mouseWheelScrollLines)));
                        }
                        else
                        {
                            num4 = -num3;
                            this.Value = Math.Max(this.Value - num4, this.Minimum);
                            this.cumulativeWheelData -= (int) (num3 * (120f / ((float) mouseWheelScrollLines)));
                        }
                    }
                    if (e.Delta != this.Value)
                    {
                        this.OnScroll(EventArgs.Empty);
                        this.OnValueChanged(EventArgs.Empty);
                    }
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnRightToLeftLayoutChanged(EventArgs e)
        {
            if (!base.GetAnyDisposingInHierarchy())
            {
                if (this.RightToLeft == RightToLeft.Yes)
                {
                    base.RecreateHandle();
                }
                EventHandler handler = base.Events[EVENT_RIGHTTOLEFTLAYOUTCHANGED] as EventHandler;
                if (handler != null)
                {
                    handler(this, e);
                }
            }
        }

        protected virtual void OnScroll(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EVENT_SCROLL];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected override void OnSystemColorsChanged(EventArgs e)
        {
            base.OnSystemColorsChanged(e);
            this.RedrawControl();
        }

        protected virtual void OnValueChanged(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EVENT_VALUECHANGED];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void RedrawControl()
        {
            if (base.IsHandleCreated)
            {
                base.SendMessage(0x408, 1, this.maximum);
                base.Invalidate();
            }
        }

        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            this.requestedDim = (this.orientation == System.Windows.Forms.Orientation.Horizontal) ? height : width;
            if (this.autoSize)
            {
                if (this.orientation == System.Windows.Forms.Orientation.Horizontal)
                {
                    if ((specified & BoundsSpecified.Height) != BoundsSpecified.None)
                    {
                        height = this.PreferredDimension;
                    }
                }
                else if ((specified & BoundsSpecified.Width) != BoundsSpecified.None)
                {
                    width = this.PreferredDimension;
                }
            }
            base.SetBoundsCore(x, y, width, height, specified);
        }

        public void SetRange(int minValue, int maxValue)
        {
            if ((this.minimum != minValue) || (this.maximum != maxValue))
            {
                if (minValue > maxValue)
                {
                    maxValue = minValue;
                }
                this.minimum = minValue;
                this.maximum = maxValue;
                if (base.IsHandleCreated)
                {
                    base.SendMessage(0x407, 0, this.minimum);
                    base.SendMessage(0x408, 1, this.maximum);
                    base.Invalidate();
                }
                if (this.value < this.minimum)
                {
                    this.value = this.minimum;
                }
                if (this.value > this.maximum)
                {
                    this.value = this.maximum;
                }
                this.SetTrackBarPosition();
            }
        }

        private void SetTrackBarPosition()
        {
            if (base.IsHandleCreated)
            {
                int lparam = this.value;
                if (this.orientation == System.Windows.Forms.Orientation.Vertical)
                {
                    lparam = (this.Minimum + this.Maximum) - this.value;
                }
                if (((this.orientation == System.Windows.Forms.Orientation.Horizontal) && (this.RightToLeft == RightToLeft.Yes)) && !base.IsMirrored)
                {
                    lparam = (this.Minimum + this.Maximum) - this.value;
                }
                base.SendMessage(0x405, 1, lparam);
            }
        }

        public override string ToString()
        {
            string str = base.ToString();
            return (str + ", Minimum: " + this.Minimum.ToString(CultureInfo.CurrentCulture) + ", Maximum: " + this.Maximum.ToString(CultureInfo.CurrentCulture) + ", Value: " + this.Value.ToString(CultureInfo.CurrentCulture));
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case 0x2114:
                case 0x2115:
                    switch (System.Windows.Forms.NativeMethods.Util.LOWORD(m.WParam))
                    {
                        case 0:
                        case 1:
                        case 2:
                        case 3:
                        case 5:
                        case 6:
                        case 7:
                        case 8:
                            if (this.value == this.Value)
                            {
                                break;
                            }
                            this.OnScroll(EventArgs.Empty);
                            this.OnValueChanged(EventArgs.Empty);
                            return;
                    }
                    return;

                default:
                    base.WndProc(ref m);
                    break;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible), Browsable(true), System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("TrackBarAutoSizeDescr"), DefaultValue(true), EditorBrowsable(EditorBrowsableState.Always)]
        public override bool AutoSize
        {
            get
            {
                return this.autoSize;
            }
            set
            {
                if (this.autoSize != value)
                {
                    this.autoSize = value;
                    if (this.orientation == System.Windows.Forms.Orientation.Horizontal)
                    {
                        base.SetStyle(ControlStyles.FixedHeight, this.autoSize);
                        base.SetStyle(ControlStyles.FixedWidth, false);
                    }
                    else
                    {
                        base.SetStyle(ControlStyles.FixedWidth, this.autoSize);
                        base.SetStyle(ControlStyles.FixedHeight, false);
                    }
                    this.AdjustSize();
                    this.OnAutoSizeChanged(EventArgs.Empty);
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
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
                createParams.ClassName = "msctls_trackbar32";
                switch (this.tickStyle)
                {
                    case System.Windows.Forms.TickStyle.None:
                        createParams.Style |= 0x10;
                        break;

                    case System.Windows.Forms.TickStyle.TopLeft:
                        createParams.Style |= 5;
                        break;

                    case System.Windows.Forms.TickStyle.BottomRight:
                        createParams.Style |= 1;
                        break;

                    case System.Windows.Forms.TickStyle.Both:
                        createParams.Style |= 9;
                        break;
                }
                if (this.orientation == System.Windows.Forms.Orientation.Vertical)
                {
                    createParams.Style |= 2;
                }
                if ((this.RightToLeft == RightToLeft.Yes) && this.RightToLeftLayout)
                {
                    createParams.ExStyle |= 0x500000;
                    createParams.ExStyle &= -28673;
                }
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

        protected override Size DefaultSize
        {
            get
            {
                return new Size(0x68, this.PreferredDimension);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        protected override bool DoubleBuffered
        {
            get
            {
                return base.DoubleBuffered;
            }
            set
            {
                base.DoubleBuffered = value;
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
                return SystemColors.WindowText;
            }
            set
            {
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
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

        [DefaultValue(5), System.Windows.Forms.SRDescription("TrackBarLargeChangeDescr"), System.Windows.Forms.SRCategory("CatBehavior")]
        public int LargeChange
        {
            get
            {
                return this.largeChange;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("LargeChange", System.Windows.Forms.SR.GetString("TrackBarLargeChangeError", new object[] { value }));
                }
                if (this.largeChange != value)
                {
                    this.largeChange = value;
                    if (base.IsHandleCreated)
                    {
                        base.SendMessage(0x415, 0, value);
                    }
                }
            }
        }

        [System.Windows.Forms.SRDescription("TrackBarMaximumDescr"), System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue(10), RefreshProperties(RefreshProperties.All)]
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
                    if (value < this.minimum)
                    {
                        this.minimum = value;
                    }
                    this.SetRange(this.minimum, value);
                }
            }
        }

        [System.Windows.Forms.SRDescription("TrackBarMinimumDescr"), DefaultValue(0), System.Windows.Forms.SRCategory("CatBehavior"), RefreshProperties(RefreshProperties.All)]
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
                    if (value > this.maximum)
                    {
                        this.maximum = value;
                    }
                    this.SetRange(value, this.maximum);
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("TrackBarOrientationDescr"), DefaultValue(0), Localizable(true)]
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
                    if (this.orientation == System.Windows.Forms.Orientation.Horizontal)
                    {
                        base.SetStyle(ControlStyles.FixedHeight, this.autoSize);
                        base.SetStyle(ControlStyles.FixedWidth, false);
                        base.Width = this.requestedDim;
                    }
                    else
                    {
                        base.SetStyle(ControlStyles.FixedHeight, false);
                        base.SetStyle(ControlStyles.FixedWidth, this.autoSize);
                        base.Height = this.requestedDim;
                    }
                    if (base.IsHandleCreated)
                    {
                        Rectangle bounds = base.Bounds;
                        base.RecreateHandle();
                        base.SetBounds(bounds.X, bounds.Y, bounds.Height, bounds.Width, BoundsSpecified.All);
                        this.AdjustSize();
                    }
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
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

        private int PreferredDimension
        {
            get
            {
                return ((System.Windows.Forms.UnsafeNativeMethods.GetSystemMetrics(3) * 8) / 3);
            }
        }

        [Localizable(true), System.Windows.Forms.SRDescription("ControlRightToLeftLayoutDescr"), System.Windows.Forms.SRCategory("CatAppearance"), DefaultValue(false)]
        public virtual bool RightToLeftLayout
        {
            get
            {
                return this.rightToLeftLayout;
            }
            set
            {
                if (value != this.rightToLeftLayout)
                {
                    this.rightToLeftLayout = value;
                    using (new LayoutTransaction(this, this, PropertyNames.RightToLeftLayout))
                    {
                        this.OnRightToLeftLayoutChanged(EventArgs.Empty);
                    }
                }
            }
        }

        [System.Windows.Forms.SRDescription("TrackBarSmallChangeDescr"), DefaultValue(1), System.Windows.Forms.SRCategory("CatBehavior")]
        public int SmallChange
        {
            get
            {
                return this.smallChange;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("SmallChange", System.Windows.Forms.SR.GetString("TrackBarSmallChangeError", new object[] { value }));
                }
                if (this.smallChange != value)
                {
                    this.smallChange = value;
                    if (base.IsHandleCreated)
                    {
                        base.SendMessage(0x417, 0, value);
                    }
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

        [DefaultValue(1), System.Windows.Forms.SRDescription("TrackBarTickFrequencyDescr"), System.Windows.Forms.SRCategory("CatAppearance")]
        public int TickFrequency
        {
            get
            {
                return this.tickFrequency;
            }
            set
            {
                if (this.tickFrequency != value)
                {
                    this.tickFrequency = value;
                    if (base.IsHandleCreated)
                    {
                        base.SendMessage(0x414, value, 0);
                        base.Invalidate();
                    }
                }
            }
        }

        [System.Windows.Forms.SRDescription("TrackBarTickStyleDescr"), DefaultValue(2), System.Windows.Forms.SRCategory("CatAppearance")]
        public System.Windows.Forms.TickStyle TickStyle
        {
            get
            {
                return this.tickStyle;
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 3))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(System.Windows.Forms.TickStyle));
                }
                if (this.tickStyle != value)
                {
                    this.tickStyle = value;
                    base.RecreateHandle();
                }
            }
        }

        [DefaultValue(0), Bindable(true), System.Windows.Forms.SRDescription("TrackBarValueDescr"), System.Windows.Forms.SRCategory("CatBehavior")]
        public int Value
        {
            get
            {
                this.GetTrackBarValue();
                return this.value;
            }
            set
            {
                if (this.value != value)
                {
                    if (!this.initializing && ((value < this.minimum) || (value > this.maximum)))
                    {
                        throw new ArgumentOutOfRangeException("Value", System.Windows.Forms.SR.GetString("InvalidBoundArgument", new object[] { "Value", value.ToString(CultureInfo.CurrentCulture), "'Minimum'", "'Maximum'" }));
                    }
                    this.value = value;
                    this.SetTrackBarPosition();
                    this.OnValueChanged(EventArgs.Empty);
                }
            }
        }
    }
}

