namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Text;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Windows.Forms.Internal;
    using System.Windows.Forms.Layout;
    using System.Windows.Forms.VisualStyles;

    [ComVisible(true), ClassInterface(ClassInterfaceType.AutoDispatch), DefaultEvent("Enter"), DefaultProperty("Text"), Designer("System.Windows.Forms.Design.GroupBoxDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), System.Windows.Forms.SRDescription("DescriptionGroupBox")]
    public class GroupBox : Control
    {
        private Font cachedFont;
        private System.Windows.Forms.FlatStyle flatStyle = System.Windows.Forms.FlatStyle.Standard;
        private int fontHeight = -1;

        [System.Windows.Forms.SRDescription("ControlOnAutoSizeChangedDescr"), System.Windows.Forms.SRCategory("CatPropertyChanged"), EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
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

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced)]
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

        [EditorBrowsable(EditorBrowsableState.Advanced), Browsable(false)]
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

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced)]
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

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced)]
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

        [EditorBrowsable(EditorBrowsableState.Advanced), Browsable(false)]
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

        [EditorBrowsable(EditorBrowsableState.Advanced), Browsable(false)]
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

        [EditorBrowsable(EditorBrowsableState.Advanced), Browsable(false)]
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

        [EditorBrowsable(EditorBrowsableState.Advanced), Browsable(false)]
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

        [EditorBrowsable(EditorBrowsableState.Advanced), Browsable(false)]
        public event EventHandler MouseEnter
        {
            add
            {
                base.MouseEnter += value;
            }
            remove
            {
                base.MouseEnter -= value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), Browsable(false)]
        public event EventHandler MouseLeave
        {
            add
            {
                base.MouseLeave += value;
            }
            remove
            {
                base.MouseLeave -= value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), Browsable(false)]
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

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced)]
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

        [EditorBrowsable(EditorBrowsableState.Advanced), Browsable(false)]
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

        public GroupBox()
        {
            base.SetState2(0x800, true);
            base.SetStyle(ControlStyles.ContainerControl, true);
            base.SetStyle(ControlStyles.SupportsTransparentBackColor | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, this.OwnerDraw);
            base.SetStyle(ControlStyles.Selectable, false);
            this.TabStop = false;
        }

        protected override AccessibleObject CreateAccessibilityInstance()
        {
            return new GroupBoxAccessibleObject(this);
        }

        private void DrawGroupBox(PaintEventArgs e)
        {
            Graphics graphics = e.Graphics;
            Rectangle clientRectangle = base.ClientRectangle;
            int num = 8;
            Color disabledColor = base.DisabledColor;
            Pen pen = new Pen(ControlPaint.Light(disabledColor, 1f));
            Pen pen2 = new Pen(ControlPaint.Dark(disabledColor, 0f));
            clientRectangle.X += num;
            clientRectangle.Width -= 2 * num;
            try
            {
                Size size;
                int num2;
                if (this.UseCompatibleTextRendering)
                {
                    using (Brush brush = new SolidBrush(this.ForeColor))
                    {
                        using (StringFormat format = new StringFormat())
                        {
                            format.HotkeyPrefix = this.ShowKeyboardCues ? HotkeyPrefix.Show : HotkeyPrefix.Hide;
                            if (this.RightToLeft == RightToLeft.Yes)
                            {
                                format.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
                            }
                            size = Size.Ceiling(graphics.MeasureString(this.Text, this.Font, clientRectangle.Width, format));
                            if (base.Enabled)
                            {
                                graphics.DrawString(this.Text, this.Font, brush, clientRectangle, format);
                            }
                            else
                            {
                                ControlPaint.DrawStringDisabled(graphics, this.Text, this.Font, disabledColor, clientRectangle, format);
                            }
                        }
                        goto Label_01E7;
                    }
                }
                using (WindowsGraphics graphics2 = WindowsGraphics.FromGraphics(graphics))
                {
                    IntTextFormatFlags flags = IntTextFormatFlags.TextBoxControl | IntTextFormatFlags.WordBreak;
                    if (!this.ShowKeyboardCues)
                    {
                        flags |= IntTextFormatFlags.HidePrefix;
                    }
                    if (this.RightToLeft == RightToLeft.Yes)
                    {
                        flags |= IntTextFormatFlags.RightToLeft;
                        flags |= IntTextFormatFlags.Right;
                    }
                    using (WindowsFont font = WindowsGraphicsCacheManager.GetWindowsFont(this.Font))
                    {
                        size = graphics2.MeasureText(this.Text, font, new Size(clientRectangle.Width, 0x7fffffff), flags);
                        if (base.Enabled)
                        {
                            graphics2.DrawText(this.Text, font, clientRectangle, this.ForeColor, flags);
                        }
                        else
                        {
                            ControlPaint.DrawStringDisabled(graphics2, this.Text, this.Font, disabledColor, clientRectangle, (TextFormatFlags) flags);
                        }
                    }
                }
            Label_01E7:
                num2 = num;
                if (this.RightToLeft == RightToLeft.Yes)
                {
                    num2 += clientRectangle.Width - size.Width;
                }
                int num3 = Math.Min((int) (num2 + size.Width), (int) (base.Width - 6));
                int num4 = base.FontHeight / 2;
                graphics.DrawLine(pen, 1, num4, 1, base.Height - 1);
                graphics.DrawLine(pen2, 0, num4, 0, base.Height - 2);
                graphics.DrawLine(pen, 0, base.Height - 1, base.Width, base.Height - 1);
                graphics.DrawLine(pen2, 0, base.Height - 2, base.Width - 1, base.Height - 2);
                graphics.DrawLine(pen2, 0, num4 - 1, num2, num4 - 1);
                graphics.DrawLine(pen, 1, num4, num2, num4);
                graphics.DrawLine(pen2, num3, num4 - 1, base.Width - 2, num4 - 1);
                graphics.DrawLine(pen, num3, num4, base.Width - 1, num4);
                graphics.DrawLine(pen, (int) (base.Width - 1), (int) (num4 - 1), (int) (base.Width - 1), (int) (base.Height - 1));
                graphics.DrawLine(pen2, base.Width - 2, num4, base.Width - 2, base.Height - 2);
            }
            finally
            {
                pen.Dispose();
                pen2.Dispose();
            }
        }

        internal override Size GetPreferredSizeCore(Size proposedSize)
        {
            Size size2 = (this.SizeFromClientSize(Size.Empty) + new Size(0, this.fontHeight)) + base.Padding.Size;
            return (this.LayoutEngine.GetPreferredSize(this, proposedSize - size2) + size2);
        }

        protected override void OnFontChanged(EventArgs e)
        {
            this.fontHeight = -1;
            this.cachedFont = null;
            base.Invalidate();
            base.OnFontChanged(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if ((Application.RenderWithVisualStyles && (base.Width >= 10)) && (base.Height >= 10))
            {
                GroupBoxState state = base.Enabled ? GroupBoxState.Normal : GroupBoxState.Disabled;
                TextFormatFlags flags = TextFormatFlags.PreserveGraphicsTranslateTransform | TextFormatFlags.PreserveGraphicsClipping | TextFormatFlags.TextBoxControl | TextFormatFlags.WordBreak;
                if (!this.ShowKeyboardCues)
                {
                    flags |= TextFormatFlags.HidePrefix;
                }
                if (this.RightToLeft == RightToLeft.Yes)
                {
                    flags |= TextFormatFlags.RightToLeft | TextFormatFlags.Right;
                }
                if (this.ShouldSerializeForeColor() || !base.Enabled)
                {
                    Color textColor = base.Enabled ? this.ForeColor : TextRenderer.DisabledTextColor(this.BackColor);
                    GroupBoxRenderer.DrawGroupBox(e.Graphics, new Rectangle(0, 0, base.Width, base.Height), this.Text, this.Font, textColor, flags, state);
                }
                else
                {
                    GroupBoxRenderer.DrawGroupBox(e.Graphics, new Rectangle(0, 0, base.Width, base.Height), this.Text, this.Font, flags, state);
                }
            }
            else
            {
                this.DrawGroupBox(e);
            }
            base.OnPaint(e);
        }

        [UIPermission(SecurityAction.LinkDemand, Window=UIPermissionWindow.AllWindows)]
        protected internal override bool ProcessMnemonic(char charCode)
        {
            if (!Control.IsMnemonic(charCode, this.Text) || !this.CanProcessMnemonic())
            {
                return false;
            }
            System.Windows.Forms.IntSecurity.ModifyFocus.Assert();
            try
            {
                base.SelectNextControl(null, true, true, true, false);
            }
            finally
            {
                CodeAccessPermission.RevertAssert();
            }
            return true;
        }

        protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
        {
            if ((factor.Width != 1f) && (factor.Height != 1f))
            {
                this.fontHeight = -1;
                this.cachedFont = null;
            }
            base.ScaleControl(factor, specified);
        }

        public override string ToString()
        {
            return (base.ToString() + ", Text: " + this.Text);
        }

        private void WmEraseBkgnd(ref Message m)
        {
            System.Windows.Forms.NativeMethods.RECT rect = new System.Windows.Forms.NativeMethods.RECT();
            System.Windows.Forms.SafeNativeMethods.GetClientRect(new HandleRef(this, base.Handle), ref rect);
            using (Graphics graphics = Graphics.FromHdcInternal(m.WParam))
            {
                using (Brush brush = new SolidBrush(this.BackColor))
                {
                    graphics.FillRectangle(brush, rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top);
                }
            }
            m.Result = (IntPtr) 1;
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        protected override void WndProc(ref Message m)
        {
            if (this.OwnerDraw)
            {
                base.WndProc(ref m);
            }
            else
            {
                int msg = m.Msg;
                if (msg != 20)
                {
                    if (msg == 0x3d)
                    {
                        base.WndProc(ref m);
                        if (((int) ((long) m.LParam)) == -12)
                        {
                            m.Result = IntPtr.Zero;
                        }
                        return;
                    }
                    if (msg != 0x318)
                    {
                        base.WndProc(ref m);
                        return;
                    }
                }
                this.WmEraseBkgnd(ref m);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), Browsable(false)]
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

        [Browsable(true), EditorBrowsable(EditorBrowsableState.Always), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
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

        [System.Windows.Forms.SRDescription("ControlAutoSizeModeDescr"), System.Windows.Forms.SRCategory("CatLayout"), Browsable(true), Localizable(true), DefaultValue(1)]
        public System.Windows.Forms.AutoSizeMode AutoSizeMode
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

        protected override System.Windows.Forms.CreateParams CreateParams
        {
            [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            get
            {
                System.Windows.Forms.CreateParams createParams = base.CreateParams;
                if (!this.OwnerDraw)
                {
                    createParams.ClassName = "BUTTON";
                    createParams.Style |= 7;
                }
                else
                {
                    createParams.ClassName = null;
                    createParams.Style &= -8;
                }
                createParams.ExStyle |= 0x10000;
                return createParams;
            }
        }

        protected override Padding DefaultPadding
        {
            get
            {
                return new Padding(3);
            }
        }

        protected override Size DefaultSize
        {
            get
            {
                return new Size(200, 100);
            }
        }

        public override Rectangle DisplayRectangle
        {
            get
            {
                Size clientSize = base.ClientSize;
                if (this.fontHeight == -1)
                {
                    this.fontHeight = this.Font.Height;
                    this.cachedFont = this.Font;
                }
                else if (!object.ReferenceEquals(this.cachedFont, this.Font))
                {
                    this.fontHeight = this.Font.Height;
                    this.cachedFont = this.Font;
                }
                Padding padding = base.Padding;
                int width = Math.Max(clientSize.Width - padding.Horizontal, 0);
                return new Rectangle(padding.Left, this.fontHeight + padding.Top, width, Math.Max((clientSize.Height - this.fontHeight) - padding.Vertical, 0));
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("ButtonFlatStyleDescr"), DefaultValue(2)]
        public System.Windows.Forms.FlatStyle FlatStyle
        {
            get
            {
                return this.flatStyle;
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 3))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(System.Windows.Forms.FlatStyle));
                }
                if (this.flatStyle != value)
                {
                    bool ownerDraw = this.OwnerDraw;
                    this.flatStyle = value;
                    bool flag2 = this.OwnerDraw != ownerDraw;
                    base.SetStyle(ControlStyles.ContainerControl, true);
                    base.SetStyle(ControlStyles.SupportsTransparentBackColor | ControlStyles.UserMouse | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, this.OwnerDraw);
                    if (flag2)
                    {
                        base.RecreateHandle();
                    }
                    else
                    {
                        this.Refresh();
                    }
                }
            }
        }

        private bool OwnerDraw
        {
            get
            {
                return (this.FlatStyle != System.Windows.Forms.FlatStyle.System);
            }
        }

        internal override bool SupportsUseCompatibleTextRendering
        {
            get
            {
                return true;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced)]
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

        [Localizable(true)]
        public override string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                bool visible = base.Visible;
                try
                {
                    if (visible && base.IsHandleCreated)
                    {
                        base.SendMessage(11, 0, 0);
                    }
                    base.Text = value;
                }
                finally
                {
                    if (visible && base.IsHandleCreated)
                    {
                        base.SendMessage(11, 1, 0);
                    }
                }
                base.Invalidate(true);
            }
        }

        [DefaultValue(false), System.Windows.Forms.SRDescription("UseCompatibleTextRenderingDescr"), System.Windows.Forms.SRCategory("CatBehavior")]
        public bool UseCompatibleTextRendering
        {
            get
            {
                return base.UseCompatibleTextRenderingInt;
            }
            set
            {
                base.UseCompatibleTextRenderingInt = value;
            }
        }

        [ComVisible(true)]
        internal class GroupBoxAccessibleObject : Control.ControlAccessibleObject
        {
            internal GroupBoxAccessibleObject(System.Windows.Forms.GroupBox owner) : base(owner)
            {
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
                    return AccessibleRole.Grouping;
                }
            }
        }
    }
}

