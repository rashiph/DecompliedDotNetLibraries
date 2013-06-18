namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Runtime.InteropServices;

    public abstract class ScrollProperties
    {
        private bool enabled = true;
        internal int largeChange = 10;
        internal bool largeChangeSetExternally;
        internal int maximum = 100;
        internal bool maximumSetExternally;
        internal int minimum;
        private ScrollableControl parent;
        private const int SCROLL_LINE = 5;
        internal int smallChange = 1;
        internal bool smallChangeSetExternally;
        internal int value;
        internal bool visible;

        protected ScrollProperties(ScrollableControl container)
        {
            this.parent = container;
        }

        private void EnableScroll(bool enable)
        {
            if (enable)
            {
                UnsafeNativeMethods.EnableScrollBar(new HandleRef(this.parent, this.parent.Handle), this.Orientation, 0);
            }
            else
            {
                UnsafeNativeMethods.EnableScrollBar(new HandleRef(this.parent, this.parent.Handle), this.Orientation, 3);
            }
        }

        internal void UpdateScrollInfo()
        {
            if (this.parent.IsHandleCreated && this.visible)
            {
                System.Windows.Forms.NativeMethods.SCROLLINFO si = new System.Windows.Forms.NativeMethods.SCROLLINFO {
                    cbSize = Marshal.SizeOf(typeof(System.Windows.Forms.NativeMethods.SCROLLINFO)),
                    fMask = 0x17,
                    nMin = this.minimum,
                    nMax = this.maximum,
                    nPage = this.parent.AutoScroll ? this.PageSize : this.LargeChange,
                    nPos = this.value,
                    nTrackPos = 0
                };
                UnsafeNativeMethods.SetScrollInfo(new HandleRef(this.parent, this.parent.Handle), this.Orientation, si, true);
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue(true), System.Windows.Forms.SRDescription("ScrollBarEnableDescr")]
        public bool Enabled
        {
            get
            {
                return this.enabled;
            }
            set
            {
                if (!this.parent.AutoScroll && (value != this.enabled))
                {
                    this.enabled = value;
                    this.EnableScroll(value);
                }
            }
        }

        internal abstract int HorizontalDisplayPosition { get; }

        [RefreshProperties(RefreshProperties.Repaint), System.Windows.Forms.SRDescription("ScrollBarLargeChangeDescr"), DefaultValue(10), System.Windows.Forms.SRCategory("CatBehavior")]
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
                    this.largeChangeSetExternally = true;
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
                if (!this.parent.AutoScroll && (this.maximum != value))
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
                    this.maximumSetExternally = true;
                    this.UpdateScrollInfo();
                }
            }
        }

        [RefreshProperties(RefreshProperties.Repaint), DefaultValue(0), System.Windows.Forms.SRDescription("ScrollBarMinimumDescr"), System.Windows.Forms.SRCategory("CatBehavior")]
        public int Minimum
        {
            get
            {
                return this.minimum;
            }
            set
            {
                if (!this.parent.AutoScroll && (this.minimum != value))
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
                    if (value > this.value)
                    {
                        this.value = value;
                    }
                    this.minimum = value;
                    this.UpdateScrollInfo();
                }
            }
        }

        internal abstract int Orientation { get; }

        internal abstract int PageSize { get; }

        protected ScrollableControl ParentControl
        {
            get
            {
                return this.parent;
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
                    this.smallChangeSetExternally = true;
                    this.UpdateScrollInfo();
                }
            }
        }

        [DefaultValue(0), System.Windows.Forms.SRCategory("CatBehavior"), Bindable(true), System.Windows.Forms.SRDescription("ScrollBarValueDescr")]
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
                    this.parent.SetDisplayFromScrollProps(this.HorizontalDisplayPosition, this.VerticalDisplayPosition);
                }
            }
        }

        internal abstract int VerticalDisplayPosition { get; }

        [System.Windows.Forms.SRDescription("ScrollBarVisibleDescr"), System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue(false)]
        public bool Visible
        {
            get
            {
                return this.visible;
            }
            set
            {
                if (!this.parent.AutoScroll && (value != this.visible))
                {
                    this.visible = value;
                    this.parent.UpdateStylesCore();
                    this.UpdateScrollInfo();
                    this.parent.SetDisplayFromScrollProps(this.HorizontalDisplayPosition, this.VerticalDisplayPosition);
                }
            }
        }
    }
}

