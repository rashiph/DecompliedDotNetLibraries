namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Windows.Forms.Layout;

    [ComVisible(true), System.Windows.Forms.SRDescription("DescriptionPanel"), ClassInterface(ClassInterfaceType.AutoDispatch), DefaultProperty("BorderStyle"), DefaultEvent("Paint"), Docking(DockingBehavior.Ask), Designer("System.Windows.Forms.Design.PanelDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    public class Panel : ScrollableControl
    {
        private System.Windows.Forms.BorderStyle borderStyle;

        [System.Windows.Forms.SRDescription("ControlOnAutoSizeChangedDescr"), System.Windows.Forms.SRCategory("CatPropertyChanged"), Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
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

        public Panel()
        {
            base.SetState2(0x800, true);
            this.TabStop = false;
            base.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.Selectable, false);
            base.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
        }

        internal override Size GetPreferredSizeCore(Size proposedSize)
        {
            Size size2 = this.SizeFromClientSize(Size.Empty) + base.Padding.Size;
            return (this.LayoutEngine.GetPreferredSize(this, proposedSize - size2) + size2);
        }

        protected override void OnResize(EventArgs eventargs)
        {
            if (base.DesignMode && (this.borderStyle == System.Windows.Forms.BorderStyle.None))
            {
                base.Invalidate();
            }
            base.OnResize(eventargs);
        }

        internal override void PrintToMetaFileRecursive(HandleRef hDC, IntPtr lParam, Rectangle bounds)
        {
            base.PrintToMetaFileRecursive(hDC, lParam, bounds);
            using (new WindowsFormsUtils.DCMapping(hDC, bounds))
            {
                using (Graphics graphics = Graphics.FromHdcInternal(hDC.Handle))
                {
                    ControlPaint.PrintBorder(graphics, new Rectangle(Point.Empty, base.Size), this.BorderStyle, Border3DStyle.Sunken);
                }
            }
        }

        private static string StringFromBorderStyle(System.Windows.Forms.BorderStyle value)
        {
            System.Type type = typeof(System.Windows.Forms.BorderStyle);
            if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 2))
            {
                return "[Invalid BorderStyle]";
            }
            return (type.ToString() + "." + value.ToString());
        }

        public override string ToString()
        {
            return (base.ToString() + ", BorderStyle: " + StringFromBorderStyle(this.borderStyle));
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
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

        [System.Windows.Forms.SRCategory("CatLayout"), DefaultValue(1), System.Windows.Forms.SRDescription("ControlAutoSizeModeDescr"), Browsable(true), Localizable(true)]
        public virtual System.Windows.Forms.AutoSizeMode AutoSizeMode
        {
            get
            {
                return base.GetAutoSizeMode();
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 1))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(System.Windows.Forms.AutoSizeMode));
                }
                if (base.GetAutoSizeMode() != value)
                {
                    base.SetAutoSizeMode(value);
                    if (this.ParentInternal != null)
                    {
                        if (this.ParentInternal.LayoutEngine == DefaultLayout.Instance)
                        {
                            this.ParentInternal.LayoutEngine.InitLayout(this, BoundsSpecified.Size);
                        }
                        LayoutTransaction.DoLayout(this.ParentInternal, this, PropertyNames.AutoSize);
                    }
                }
            }
        }

        [System.Windows.Forms.SRDescription("PanelBorderStyleDescr"), DispId(-504), System.Windows.Forms.SRCategory("CatAppearance"), DefaultValue(0)]
        public System.Windows.Forms.BorderStyle BorderStyle
        {
            get
            {
                return this.borderStyle;
            }
            set
            {
                if (this.borderStyle != value)
                {
                    if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 2))
                    {
                        throw new InvalidEnumArgumentException("value", (int) value, typeof(System.Windows.Forms.BorderStyle));
                    }
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
                createParams.ExStyle |= 0x10000;
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

        protected override Size DefaultSize
        {
            get
            {
                return new Size(200, 100);
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

        [Browsable(false), Bindable(false), EditorBrowsable(EditorBrowsableState.Never)]
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
    }
}

