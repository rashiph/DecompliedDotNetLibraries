namespace System.Windows.Forms
{
    using Microsoft.Win32;
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Windows.Forms.Layout;

    [DefaultBindingProperty("Value"), ComVisible(true), ClassInterface(ClassInterfaceType.AutoDispatch), DefaultProperty("Value"), System.Windows.Forms.SRDescription("DescriptionProgressBar")]
    public class ProgressBar : Control
    {
        private System.Drawing.Color defaultForeColor = SystemColors.Highlight;
        private int marqueeSpeed = 100;
        private int maximum = 100;
        private int minimum;
        private bool rightToLeftLayout;
        private int step = 10;
        private ProgressBarStyle style;
        private int value;

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

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
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
        public event EventHandler CausesValidationChanged
        {
            add
            {
                base.CausesValidationChanged += value;
            }
            remove
            {
                base.CausesValidationChanged -= value;
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

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
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

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
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

        [System.Windows.Forms.SRDescription("ControlOnRightToLeftLayoutChangedDescr"), System.Windows.Forms.SRCategory("CatPropertyChanged")]
        public event EventHandler RightToLeftLayoutChanged;

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

        public ProgressBar()
        {
            base.SetStyle(ControlStyles.UseTextForAccessibility | ControlStyles.Selectable | ControlStyles.UserPaint, false);
            this.ForeColor = this.defaultForeColor;
        }

        protected override void CreateHandle()
        {
            if (!base.RecreatingHandle)
            {
                IntPtr userCookie = System.Windows.Forms.UnsafeNativeMethods.ThemingScope.Activate();
                try
                {
                    System.Windows.Forms.NativeMethods.INITCOMMONCONTROLSEX icc = new System.Windows.Forms.NativeMethods.INITCOMMONCONTROLSEX {
                        dwICC = 0x20
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

        public void Increment(int value)
        {
            if (this.Style == ProgressBarStyle.Marquee)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("ProgressBarIncrementMarqueeException"));
            }
            this.value += value;
            if (this.value < this.minimum)
            {
                this.value = this.minimum;
            }
            if (this.value > this.maximum)
            {
                this.value = this.maximum;
            }
            this.UpdatePos();
        }

        protected override void OnBackColorChanged(EventArgs e)
        {
            base.OnBackColorChanged(e);
            if (base.IsHandleCreated)
            {
                System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x2001, 0, ColorTranslator.ToWin32(this.BackColor));
            }
        }

        protected override void OnForeColorChanged(EventArgs e)
        {
            base.OnForeColorChanged(e);
            if (base.IsHandleCreated)
            {
                System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x409, 0, ColorTranslator.ToWin32(this.ForeColor));
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            base.SendMessage(0x406, this.minimum, this.maximum);
            base.SendMessage(0x404, this.step, 0);
            base.SendMessage(0x402, this.value, 0);
            System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x2001, 0, ColorTranslator.ToWin32(this.BackColor));
            System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x409, 0, ColorTranslator.ToWin32(this.ForeColor));
            this.StartMarquee();
            SystemEvents.UserPreferenceChanged += new UserPreferenceChangedEventHandler(this.UserPreferenceChangedHandler);
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            SystemEvents.UserPreferenceChanged -= new UserPreferenceChangedEventHandler(this.UserPreferenceChangedHandler);
            base.OnHandleDestroyed(e);
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
                if (this.onRightToLeftLayoutChanged != null)
                {
                    this.onRightToLeftLayoutChanged(this, e);
                }
            }
        }

        public void PerformStep()
        {
            if (this.Style == ProgressBarStyle.Marquee)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("ProgressBarPerformStepMarqueeException"));
            }
            this.Increment(this.step);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override void ResetForeColor()
        {
            this.ForeColor = this.defaultForeColor;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal override bool ShouldSerializeForeColor()
        {
            return (this.ForeColor != this.defaultForeColor);
        }

        private void StartMarquee()
        {
            if (base.IsHandleCreated && (this.style == ProgressBarStyle.Marquee))
            {
                if (this.marqueeSpeed == 0)
                {
                    base.SendMessage(0x40a, 0, this.marqueeSpeed);
                }
                else
                {
                    base.SendMessage(0x40a, 1, this.marqueeSpeed);
                }
            }
        }

        public override string ToString()
        {
            string str = base.ToString();
            return (str + ", Minimum: " + this.Minimum.ToString(CultureInfo.CurrentCulture) + ", Maximum: " + this.Maximum.ToString(CultureInfo.CurrentCulture) + ", Value: " + this.Value.ToString(CultureInfo.CurrentCulture));
        }

        private void UpdatePos()
        {
            if (base.IsHandleCreated)
            {
                base.SendMessage(0x402, this.value, 0);
            }
        }

        private void UserPreferenceChangedHandler(object o, UserPreferenceChangedEventArgs e)
        {
            if (base.IsHandleCreated)
            {
                System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x409, 0, ColorTranslator.ToWin32(this.ForeColor));
                System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x2001, 0, ColorTranslator.ToWin32(this.BackColor));
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
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

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public bool CausesValidation
        {
            get
            {
                return base.CausesValidation;
            }
            set
            {
                base.CausesValidation = value;
            }
        }

        protected override System.Windows.Forms.CreateParams CreateParams
        {
            [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            get
            {
                System.Windows.Forms.CreateParams createParams = base.CreateParams;
                createParams.ClassName = "msctls_progress32";
                if (this.Style == ProgressBarStyle.Continuous)
                {
                    createParams.Style |= 1;
                }
                else if ((this.Style == ProgressBarStyle.Marquee) && !base.DesignMode)
                {
                    createParams.Style |= 8;
                }
                if ((this.RightToLeft == RightToLeft.Yes) && this.RightToLeftLayout)
                {
                    createParams.ExStyle |= 0x400000;
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
                return new Size(100, 0x17);
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

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
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

        [System.Windows.Forms.SRDescription("ProgressBarMarqueeAnimationSpeed"), DefaultValue(100), System.Windows.Forms.SRCategory("CatBehavior")]
        public int MarqueeAnimationSpeed
        {
            get
            {
                return this.marqueeSpeed;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("MarqueeAnimationSpeed must be non-negative");
                }
                this.marqueeSpeed = value;
                if (!base.DesignMode)
                {
                    this.StartMarquee();
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("ProgressBarMaximumDescr"), DefaultValue(100), RefreshProperties(RefreshProperties.Repaint)]
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
                    if (value < 0)
                    {
                        object[] args = new object[] { "Maximum", value.ToString(CultureInfo.CurrentCulture), 0.ToString(CultureInfo.CurrentCulture) };
                        throw new ArgumentOutOfRangeException("Maximum", System.Windows.Forms.SR.GetString("InvalidLowBoundArgumentEx", args));
                    }
                    if (this.minimum > value)
                    {
                        this.minimum = value;
                    }
                    this.maximum = value;
                    if (this.value > this.maximum)
                    {
                        this.value = this.maximum;
                    }
                    if (base.IsHandleCreated)
                    {
                        base.SendMessage(0x406, this.minimum, this.maximum);
                        this.UpdatePos();
                    }
                }
            }
        }

        [DefaultValue(0), RefreshProperties(RefreshProperties.Repaint), System.Windows.Forms.SRDescription("ProgressBarMinimumDescr"), System.Windows.Forms.SRCategory("CatBehavior")]
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
                    if (value < 0)
                    {
                        object[] args = new object[] { "Minimum", value.ToString(CultureInfo.CurrentCulture), 0.ToString(CultureInfo.CurrentCulture) };
                        throw new ArgumentOutOfRangeException("Minimum", System.Windows.Forms.SR.GetString("InvalidLowBoundArgumentEx", args));
                    }
                    if (this.maximum < value)
                    {
                        this.maximum = value;
                    }
                    this.minimum = value;
                    if (this.value < this.minimum)
                    {
                        this.value = this.minimum;
                    }
                    if (base.IsHandleCreated)
                    {
                        base.SendMessage(0x406, this.minimum, this.maximum);
                        this.UpdatePos();
                    }
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
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

        [System.Windows.Forms.SRCategory("CatAppearance"), DefaultValue(false), System.Windows.Forms.SRDescription("ControlRightToLeftLayoutDescr"), Localizable(true)]
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

        [DefaultValue(10), System.Windows.Forms.SRDescription("ProgressBarStepDescr"), System.Windows.Forms.SRCategory("CatBehavior")]
        public int Step
        {
            get
            {
                return this.step;
            }
            set
            {
                this.step = value;
                if (base.IsHandleCreated)
                {
                    base.SendMessage(0x404, this.step, 0);
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("ProgressBarStyleDescr"), Browsable(true), EditorBrowsable(EditorBrowsableState.Always), DefaultValue(0)]
        public ProgressBarStyle Style
        {
            get
            {
                return this.style;
            }
            set
            {
                if (this.style != value)
                {
                    if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 2))
                    {
                        throw new InvalidEnumArgumentException("value", (int) value, typeof(ProgressBarStyle));
                    }
                    this.style = value;
                    if (base.IsHandleCreated)
                    {
                        base.RecreateHandle();
                    }
                    if (this.style == ProgressBarStyle.Marquee)
                    {
                        this.StartMarquee();
                    }
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
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

        [EditorBrowsable(EditorBrowsableState.Never), Bindable(false), Browsable(false)]
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

        [System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue(0), System.Windows.Forms.SRDescription("ProgressBarValueDescr"), Bindable(true)]
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
                    this.UpdatePos();
                }
            }
        }
    }
}

