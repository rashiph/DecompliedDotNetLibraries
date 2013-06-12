namespace System.Windows.Forms
{
    using Microsoft.Win32;
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Windows.Forms.VisualStyles;

    [ClassInterface(ClassInterfaceType.AutoDispatch), ComVisible(true), Designer("System.Windows.Forms.Design.UpDownBaseDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    public abstract class UpDownBase : ContainerControl
    {
        private System.Windows.Forms.BorderStyle borderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
        private bool changingText;
        private const System.Windows.Forms.BorderStyle DefaultBorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
        private const int DefaultButtonsWidth = 0x10;
        private const int DefaultControlWidth = 120;
        private static readonly bool DefaultInterceptArrowKeys = true;
        private const int DefaultTimerInterval = 500;
        private const LeftRightAlignment DefaultUpDownAlign = LeftRightAlignment.Right;
        private const int DefaultWheelScrollLinesPerPage = 1;
        private bool doubleClickFired;
        private bool interceptArrowKeys = DefaultInterceptArrowKeys;
        private const int ThemedBorderWidth = 1;
        private LeftRightAlignment upDownAlign = LeftRightAlignment.Right;
        internal UpDownButtons upDownButtons;
        internal UpDownEdit upDownEdit;
        private bool userEdit;
        private int wheelDelta;

        [System.Windows.Forms.SRDescription("ControlOnAutoSizeChangedDescr"), Browsable(true), EditorBrowsable(EditorBrowsableState.Always), System.Windows.Forms.SRCategory("CatPropertyChanged")]
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

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
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

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event EventHandler MouseHover
        {
            add
            {
                base.MouseHover += value;
            }
            remove
            {
                base.MouseHover -= value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
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

        public UpDownBase()
        {
            this.upDownButtons = new UpDownButtons(this);
            this.upDownEdit = new UpDownEdit(this);
            this.upDownEdit.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.upDownEdit.AutoSize = false;
            this.upDownEdit.KeyDown += new KeyEventHandler(this.OnTextBoxKeyDown);
            this.upDownEdit.KeyPress += new KeyPressEventHandler(this.OnTextBoxKeyPress);
            this.upDownEdit.TextChanged += new EventHandler(this.OnTextBoxTextChanged);
            this.upDownEdit.LostFocus += new EventHandler(this.OnTextBoxLostFocus);
            this.upDownEdit.Resize += new EventHandler(this.OnTextBoxResize);
            this.upDownButtons.TabStop = false;
            this.upDownButtons.Size = new Size(0x10, this.PreferredHeight);
            this.upDownButtons.UpDown += new UpDownEventHandler(this.OnUpDown);
            base.Controls.AddRange(new Control[] { this.upDownButtons, this.upDownEdit });
            base.SetStyle(ControlStyles.FixedHeight | ControlStyles.ResizeRedraw | ControlStyles.Opaque, true);
            base.SetStyle(ControlStyles.StandardClick, false);
            base.SetStyle(ControlStyles.UseTextForAccessibility, false);
        }

        internal override Rectangle ApplyBoundsConstraints(int suggestedX, int suggestedY, int proposedWidth, int proposedHeight)
        {
            return base.ApplyBoundsConstraints(suggestedX, suggestedY, proposedWidth, this.PreferredHeight);
        }

        public abstract void DownButton();
        protected virtual void OnChanged(object source, EventArgs e)
        {
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.FontHeight = -1;
            base.Height = this.PreferredHeight;
            this.PositionControls();
            base.OnFontChanged(e);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            this.PositionControls();
            SystemEvents.UserPreferenceChanged += new UserPreferenceChangedEventHandler(this.UserPreferenceChanged);
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            SystemEvents.UserPreferenceChanged -= new UserPreferenceChangedEventHandler(this.UserPreferenceChanged);
            base.OnHandleDestroyed(e);
        }

        protected override void OnLayout(LayoutEventArgs e)
        {
            this.PositionControls();
            base.OnLayout(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if ((e.Clicks == 2) && (e.Button == MouseButtons.Left))
            {
                this.doubleClickFired = true;
            }
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs mevent)
        {
            if (mevent.Button == MouseButtons.Left)
            {
                Point point = base.PointToScreen(new Point(mevent.X, mevent.Y));
                if ((System.Windows.Forms.UnsafeNativeMethods.WindowFromPoint(point.X, point.Y) == base.Handle) && !base.ValidationCancelled)
                {
                    if (!this.doubleClickFired)
                    {
                        this.OnClick(mevent);
                        this.OnMouseClick(mevent);
                    }
                    else
                    {
                        this.doubleClickFired = false;
                        this.OnDoubleClick(mevent);
                        this.OnMouseDoubleClick(mevent);
                    }
                }
                this.doubleClickFired = false;
            }
            base.OnMouseUp(mevent);
        }

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
                    this.wheelDelta += e.Delta;
                    float num2 = ((float) this.wheelDelta) / 120f;
                    if (mouseWheelScrollLines == -1)
                    {
                        mouseWheelScrollLines = 1;
                    }
                    int num3 = (int) (mouseWheelScrollLines * num2);
                    if (num3 != 0)
                    {
                        int num4;
                        if (num3 > 0)
                        {
                            for (num4 = num3; num4 > 0; num4--)
                            {
                                this.UpButton();
                            }
                            this.wheelDelta -= (int) (num3 * (120f / ((float) mouseWheelScrollLines)));
                        }
                        else
                        {
                            for (num4 = -num3; num4 > 0; num4--)
                            {
                                this.DownButton();
                            }
                            this.wheelDelta -= (int) (num3 * (120f / ((float) mouseWheelScrollLines)));
                        }
                    }
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Rectangle bounds = this.upDownEdit.Bounds;
            if (Application.RenderWithVisualStyles)
            {
                if (this.borderStyle == System.Windows.Forms.BorderStyle.None)
                {
                    goto Label_0211;
                }
                Rectangle clientRectangle = base.ClientRectangle;
                Rectangle clipRectangle = e.ClipRectangle;
                VisualStyleRenderer renderer = new VisualStyleRenderer(System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox.TextEdit.Normal);
                int width = 1;
                Rectangle rectangle4 = new Rectangle(clientRectangle.Left, clientRectangle.Top, width, clientRectangle.Height);
                Rectangle rectangle5 = new Rectangle(clientRectangle.Left, clientRectangle.Top, clientRectangle.Width, width);
                Rectangle rectangle6 = new Rectangle(clientRectangle.Right - width, clientRectangle.Top, width, clientRectangle.Height);
                Rectangle rectangle7 = new Rectangle(clientRectangle.Left, clientRectangle.Bottom - width, clientRectangle.Width, width);
                rectangle4.Intersect(clipRectangle);
                rectangle5.Intersect(clipRectangle);
                rectangle6.Intersect(clipRectangle);
                rectangle7.Intersect(clipRectangle);
                renderer.DrawBackground(e.Graphics, clientRectangle, rectangle4);
                renderer.DrawBackground(e.Graphics, clientRectangle, rectangle5);
                renderer.DrawBackground(e.Graphics, clientRectangle, rectangle6);
                renderer.DrawBackground(e.Graphics, clientRectangle, rectangle7);
                using (Pen pen = new Pen(this.BackColor))
                {
                    Rectangle rect = bounds;
                    rect.X--;
                    rect.Y--;
                    rect.Width++;
                    rect.Height++;
                    e.Graphics.DrawRectangle(pen, rect);
                    goto Label_0211;
                }
            }
            using (Pen pen2 = new Pen(this.BackColor, base.Enabled ? ((float) 2) : ((float) 1)))
            {
                Rectangle rectangle9 = bounds;
                rectangle9.Inflate(1, 1);
                if (!base.Enabled)
                {
                    rectangle9.X--;
                    rectangle9.Y--;
                    rectangle9.Width++;
                    rectangle9.Height++;
                }
                e.Graphics.DrawRectangle(pen2, rectangle9);
            }
        Label_0211:
            if ((!base.Enabled && (this.BorderStyle != System.Windows.Forms.BorderStyle.None)) && !this.upDownEdit.ShouldSerializeBackColor())
            {
                bounds.Inflate(1, 1);
                ControlPaint.DrawBorder(e.Graphics, bounds, SystemColors.Control, ButtonBorderStyle.Solid);
            }
        }

        internal virtual void OnStartTimer()
        {
        }

        internal virtual void OnStopTimer()
        {
        }

        protected virtual void OnTextBoxKeyDown(object source, KeyEventArgs e)
        {
            this.OnKeyDown(e);
            if (this.interceptArrowKeys)
            {
                if (e.KeyData == Keys.Up)
                {
                    this.UpButton();
                    e.Handled = true;
                }
                else if (e.KeyData == Keys.Down)
                {
                    this.DownButton();
                    e.Handled = true;
                }
            }
            if ((e.KeyCode == Keys.Enter) && this.UserEdit)
            {
                this.ValidateEditText();
            }
        }

        protected virtual void OnTextBoxKeyPress(object source, KeyPressEventArgs e)
        {
            this.OnKeyPress(e);
        }

        protected virtual void OnTextBoxLostFocus(object source, EventArgs e)
        {
            if (this.UserEdit)
            {
                this.ValidateEditText();
            }
        }

        protected virtual void OnTextBoxResize(object source, EventArgs e)
        {
            base.Height = this.PreferredHeight;
            this.PositionControls();
        }

        protected virtual void OnTextBoxTextChanged(object source, EventArgs e)
        {
            if (this.changingText)
            {
                this.ChangingText = false;
            }
            else
            {
                this.UserEdit = true;
            }
            this.OnTextChanged(e);
            this.OnChanged(source, new EventArgs());
        }

        private void OnUpDown(object source, UpDownEventArgs e)
        {
            if (e.ButtonID == 1)
            {
                this.UpButton();
            }
            else if (e.ButtonID == 2)
            {
                this.DownButton();
            }
        }

        private void PositionControls()
        {
            Rectangle empty = Rectangle.Empty;
            Rectangle rectangle2 = Rectangle.Empty;
            Rectangle rectangle3 = new Rectangle(Point.Empty, base.ClientSize);
            int width = rectangle3.Width;
            bool renderWithVisualStyles = Application.RenderWithVisualStyles;
            System.Windows.Forms.BorderStyle borderStyle = this.BorderStyle;
            int num2 = (borderStyle == System.Windows.Forms.BorderStyle.None) ? 0 : 2;
            rectangle3.Inflate(-num2, -num2);
            if (this.upDownEdit != null)
            {
                empty = rectangle3;
                empty.Size = new Size(rectangle3.Width - 0x10, rectangle3.Height);
            }
            if (this.upDownButtons != null)
            {
                int num3 = renderWithVisualStyles ? 1 : 2;
                if (borderStyle == System.Windows.Forms.BorderStyle.None)
                {
                    num3 = 0;
                }
                rectangle2 = new Rectangle((rectangle3.Right - 0x10) + num3, rectangle3.Top - num3, 0x10, rectangle3.Height + (num3 * 2));
            }
            LeftRightAlignment upDownAlign = this.UpDownAlign;
            if (base.RtlTranslateLeftRight(upDownAlign) == LeftRightAlignment.Left)
            {
                rectangle2.X = width - rectangle2.Right;
                empty.X = width - empty.Right;
            }
            if (this.upDownEdit != null)
            {
                this.upDownEdit.Bounds = empty;
            }
            if (this.upDownButtons != null)
            {
                this.upDownButtons.Bounds = rectangle2;
                this.upDownButtons.Invalidate();
            }
        }

        public void Select(int start, int length)
        {
            this.upDownEdit.Select(start, length);
        }

        internal void SetToolTip(System.Windows.Forms.ToolTip toolTip, string caption)
        {
            toolTip.SetToolTip(this.upDownEdit, caption);
            toolTip.SetToolTip(this.upDownButtons, caption);
        }

        private MouseEventArgs TranslateMouseEvent(Control child, MouseEventArgs e)
        {
            if ((child != null) && base.IsHandleCreated)
            {
                System.Windows.Forms.NativeMethods.POINT pt = new System.Windows.Forms.NativeMethods.POINT(e.X, e.Y);
                System.Windows.Forms.UnsafeNativeMethods.MapWindowPoints(new HandleRef(child, child.Handle), new HandleRef(this, base.Handle), pt, 1);
                return new MouseEventArgs(e.Button, e.Clicks, pt.x, pt.y, e.Delta);
            }
            return e;
        }

        public abstract void UpButton();
        protected abstract void UpdateEditText();
        private void UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs pref)
        {
            if (pref.Category == UserPreferenceCategory.Locale)
            {
                this.UpdateEditText();
            }
        }

        protected virtual void ValidateEditText()
        {
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case 7:
                    if (base.HostedInWin32DialogManager)
                    {
                        if (this.TextBox.CanFocus)
                        {
                            System.Windows.Forms.UnsafeNativeMethods.SetFocus(new HandleRef(this.TextBox, this.TextBox.Handle));
                        }
                        base.WndProc(ref m);
                        return;
                    }
                    if (base.ActiveControl != null)
                    {
                        base.FocusActiveControlInternal();
                        return;
                    }
                    base.SetActiveControlInternal(this.TextBox);
                    return;

                case 8:
                    this.DefWndProc(ref m);
                    return;
            }
            base.WndProc(ref m);
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public override bool AutoScroll
        {
            get
            {
                return false;
            }
            set
            {
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
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

        [EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
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

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible), EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
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

        public override System.Drawing.Color BackColor
        {
            get
            {
                return this.upDownEdit.BackColor;
            }
            set
            {
                base.BackColor = value;
                this.upDownEdit.BackColor = value;
                base.Invalidate();
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never)]
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

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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

        [System.Windows.Forms.SRDescription("UpDownBaseBorderStyleDescr"), System.Windows.Forms.SRCategory("CatAppearance"), DefaultValue(2), DispId(-504)]
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
                    base.RecreateHandle();
                }
            }
        }

        protected bool ChangingText
        {
            get
            {
                return this.changingText;
            }
            set
            {
                this.changingText = value;
            }
        }

        public override System.Windows.Forms.ContextMenu ContextMenu
        {
            get
            {
                return base.ContextMenu;
            }
            set
            {
                base.ContextMenu = value;
                this.upDownEdit.ContextMenu = value;
            }
        }

        public override System.Windows.Forms.ContextMenuStrip ContextMenuStrip
        {
            get
            {
                return base.ContextMenuStrip;
            }
            set
            {
                base.ContextMenuStrip = value;
                this.upDownEdit.ContextMenuStrip = value;
            }
        }

        protected override System.Windows.Forms.CreateParams CreateParams
        {
            [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            get
            {
                System.Windows.Forms.CreateParams createParams = base.CreateParams;
                createParams.Style &= -8388609;
                if (!Application.RenderWithVisualStyles)
                {
                    switch (this.borderStyle)
                    {
                        case System.Windows.Forms.BorderStyle.FixedSingle:
                            createParams.Style |= 0x800000;
                            return createParams;

                        case System.Windows.Forms.BorderStyle.Fixed3D:
                            createParams.ExStyle |= 0x200;
                            return createParams;
                    }
                }
                return createParams;
            }
        }

        protected override Size DefaultSize
        {
            get
            {
                return new Size(120, this.PreferredHeight);
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never)]
        public ScrollableControl.DockPaddingEdges DockPadding
        {
            get
            {
                return base.DockPadding;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), System.Windows.Forms.SRDescription("ControlFocusedDescr")]
        public override bool Focused
        {
            get
            {
                return this.upDownEdit.Focused;
            }
        }

        public override System.Drawing.Color ForeColor
        {
            get
            {
                return this.upDownEdit.ForeColor;
            }
            set
            {
                base.ForeColor = value;
                this.upDownEdit.ForeColor = value;
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("UpDownBaseInterceptArrowKeysDescr"), DefaultValue(true)]
        public bool InterceptArrowKeys
        {
            get
            {
                return this.interceptArrowKeys;
            }
            set
            {
                this.interceptArrowKeys = value;
            }
        }

        public override Size MaximumSize
        {
            get
            {
                return base.MaximumSize;
            }
            set
            {
                base.MaximumSize = new Size(value.Width, 0);
            }
        }

        public override Size MinimumSize
        {
            get
            {
                return base.MinimumSize;
            }
            set
            {
                base.MinimumSize = new Size(value.Width, 0);
            }
        }

        [System.Windows.Forms.SRDescription("UpDownBasePreferredHeightDescr"), System.Windows.Forms.SRCategory("CatLayout"), EditorBrowsable(EditorBrowsableState.Advanced), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int PreferredHeight
        {
            get
            {
                int fontHeight = base.FontHeight;
                if (this.borderStyle != System.Windows.Forms.BorderStyle.None)
                {
                    return (fontHeight + ((SystemInformation.BorderSize.Height * 4) + 3));
                }
                return (fontHeight + 3);
            }
        }

        [System.Windows.Forms.SRDescription("UpDownBaseReadOnlyDescr"), System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue(false)]
        public bool ReadOnly
        {
            get
            {
                return this.upDownEdit.ReadOnly;
            }
            set
            {
                this.upDownEdit.ReadOnly = value;
            }
        }

        [Localizable(true)]
        public override string Text
        {
            get
            {
                return this.upDownEdit.Text;
            }
            set
            {
                this.upDownEdit.Text = value;
                this.ChangingText = false;
                if (this.UserEdit)
                {
                    this.ValidateEditText();
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), DefaultValue(0), Localizable(true), System.Windows.Forms.SRDescription("UpDownBaseTextAlignDescr")]
        public HorizontalAlignment TextAlign
        {
            get
            {
                return this.upDownEdit.TextAlign;
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 2))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(HorizontalAlignment));
                }
                this.upDownEdit.TextAlign = value;
            }
        }

        internal System.Windows.Forms.TextBox TextBox
        {
            get
            {
                return this.upDownEdit;
            }
        }

        [System.Windows.Forms.SRDescription("UpDownBaseAlignmentDescr"), DefaultValue(1), Localizable(true), System.Windows.Forms.SRCategory("CatAppearance")]
        public LeftRightAlignment UpDownAlign
        {
            get
            {
                return this.upDownAlign;
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 1))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(LeftRightAlignment));
                }
                if (this.upDownAlign != value)
                {
                    this.upDownAlign = value;
                    this.PositionControls();
                    base.Invalidate();
                }
            }
        }

        internal UpDownButtons UpDownButtonsInternal
        {
            get
            {
                return this.upDownButtons;
            }
        }

        protected bool UserEdit
        {
            get
            {
                return this.userEdit;
            }
            set
            {
                this.userEdit = value;
            }
        }

        internal enum ButtonID
        {
            None,
            Up,
            Down
        }

        internal class UpDownButtons : Control
        {
            private UpDownBase.ButtonID captured;
            private bool doubleClickFired;
            private UpDownBase.ButtonID mouseOver;
            private UpDownBase parent;
            private UpDownBase.ButtonID pushed;
            private Timer timer;
            private int timerInterval;

            public event UpDownEventHandler UpDown;

            internal UpDownButtons(UpDownBase parent)
            {
                base.SetStyle(ControlStyles.FixedHeight | ControlStyles.FixedWidth | ControlStyles.Opaque, true);
                base.SetStyle(ControlStyles.Selectable, false);
                this.parent = parent;
            }

            private void BeginButtonPress(MouseEventArgs e)
            {
                int num = base.Size.Height / 2;
                if (e.Y < num)
                {
                    this.pushed = this.captured = UpDownBase.ButtonID.Up;
                    base.Invalidate();
                }
                else
                {
                    this.pushed = this.captured = UpDownBase.ButtonID.Down;
                    base.Invalidate();
                }
                base.CaptureInternal = true;
                this.OnUpDown(new UpDownEventArgs((int) this.pushed));
                this.StartTimer();
            }

            protected override AccessibleObject CreateAccessibilityInstance()
            {
                return new UpDownButtonsAccessibleObject(this);
            }

            private void EndButtonPress()
            {
                this.pushed = UpDownBase.ButtonID.None;
                this.captured = UpDownBase.ButtonID.None;
                this.StopTimer();
                base.CaptureInternal = false;
                base.Invalidate();
            }

            protected override void OnMouseDown(MouseEventArgs e)
            {
                this.parent.FocusInternal();
                if (!this.parent.ValidationCancelled && (e.Button == MouseButtons.Left))
                {
                    this.BeginButtonPress(e);
                }
                if ((e.Clicks == 2) && (e.Button == MouseButtons.Left))
                {
                    this.doubleClickFired = true;
                }
                this.parent.OnMouseDown(this.parent.TranslateMouseEvent(this, e));
            }

            protected override void OnMouseLeave(EventArgs e)
            {
                this.mouseOver = UpDownBase.ButtonID.None;
                base.Invalidate();
                this.parent.OnMouseLeave(e);
            }

            protected override void OnMouseMove(MouseEventArgs e)
            {
                if (base.Capture)
                {
                    Rectangle rectangle = base.ClientRectangle;
                    rectangle.Height /= 2;
                    if (this.captured == UpDownBase.ButtonID.Down)
                    {
                        rectangle.Y += rectangle.Height;
                    }
                    if (rectangle.Contains(e.X, e.Y))
                    {
                        if (this.pushed != this.captured)
                        {
                            this.StartTimer();
                            this.pushed = this.captured;
                            base.Invalidate();
                        }
                    }
                    else if (this.pushed != UpDownBase.ButtonID.None)
                    {
                        this.StopTimer();
                        this.pushed = UpDownBase.ButtonID.None;
                        base.Invalidate();
                    }
                }
                Rectangle clientRectangle = base.ClientRectangle;
                Rectangle rectangle3 = base.ClientRectangle;
                clientRectangle.Height /= 2;
                rectangle3.Y += rectangle3.Height / 2;
                if (clientRectangle.Contains(e.X, e.Y))
                {
                    this.mouseOver = UpDownBase.ButtonID.Up;
                    base.Invalidate();
                }
                else if (rectangle3.Contains(e.X, e.Y))
                {
                    this.mouseOver = UpDownBase.ButtonID.Down;
                    base.Invalidate();
                }
                this.parent.OnMouseMove(this.parent.TranslateMouseEvent(this, e));
            }

            protected override void OnMouseUp(MouseEventArgs e)
            {
                if (!this.parent.ValidationCancelled && (e.Button == MouseButtons.Left))
                {
                    this.EndButtonPress();
                }
                Point p = new Point(e.X, e.Y);
                p = base.PointToScreen(p);
                MouseEventArgs args = this.parent.TranslateMouseEvent(this, e);
                if (e.Button == MouseButtons.Left)
                {
                    if (!this.parent.ValidationCancelled && (System.Windows.Forms.UnsafeNativeMethods.WindowFromPoint(p.X, p.Y) == base.Handle))
                    {
                        if (!this.doubleClickFired)
                        {
                            this.parent.OnClick(args);
                        }
                        else
                        {
                            this.doubleClickFired = false;
                            this.parent.OnDoubleClick(args);
                            this.parent.OnMouseDoubleClick(args);
                        }
                    }
                    this.doubleClickFired = false;
                }
                this.parent.OnMouseUp(args);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                int height = base.ClientSize.Height / 2;
                if (Application.RenderWithVisualStyles)
                {
                    VisualStyleRenderer renderer = new VisualStyleRenderer((this.mouseOver == UpDownBase.ButtonID.Up) ? VisualStyleElement.Spin.Up.Hot : VisualStyleElement.Spin.Up.Normal);
                    if (!base.Enabled)
                    {
                        renderer.SetParameters(VisualStyleElement.Spin.Up.Disabled);
                    }
                    else if (this.pushed == UpDownBase.ButtonID.Up)
                    {
                        renderer.SetParameters(VisualStyleElement.Spin.Up.Pressed);
                    }
                    renderer.DrawBackground(e.Graphics, new Rectangle(0, 0, 0x10, height));
                    if (!base.Enabled)
                    {
                        renderer.SetParameters(VisualStyleElement.Spin.Down.Disabled);
                    }
                    else if (this.pushed == UpDownBase.ButtonID.Down)
                    {
                        renderer.SetParameters(VisualStyleElement.Spin.Down.Pressed);
                    }
                    else
                    {
                        renderer.SetParameters((this.mouseOver == UpDownBase.ButtonID.Down) ? VisualStyleElement.Spin.Down.Hot : VisualStyleElement.Spin.Down.Normal);
                    }
                    renderer.DrawBackground(e.Graphics, new Rectangle(0, height, 0x10, height));
                }
                else
                {
                    ControlPaint.DrawScrollButton(e.Graphics, new Rectangle(0, 0, 0x10, height), ScrollButton.Up, (this.pushed == UpDownBase.ButtonID.Up) ? ButtonState.Pushed : (base.Enabled ? ButtonState.Normal : ButtonState.Inactive));
                    ControlPaint.DrawScrollButton(e.Graphics, new Rectangle(0, height, 0x10, height), ScrollButton.Down, (this.pushed == UpDownBase.ButtonID.Down) ? ButtonState.Pushed : (base.Enabled ? ButtonState.Normal : ButtonState.Inactive));
                }
                if (height != ((base.ClientSize.Height + 1) / 2))
                {
                    using (Pen pen = new Pen(this.parent.BackColor))
                    {
                        Rectangle clientRectangle = base.ClientRectangle;
                        e.Graphics.DrawLine(pen, clientRectangle.Left, clientRectangle.Bottom - 1, clientRectangle.Right - 1, clientRectangle.Bottom - 1);
                    }
                }
                base.OnPaint(e);
            }

            protected virtual void OnUpDown(UpDownEventArgs upevent)
            {
                if (this.upDownEventHandler != null)
                {
                    this.upDownEventHandler(this, upevent);
                }
            }

            protected void StartTimer()
            {
                this.parent.OnStartTimer();
                if (this.timer == null)
                {
                    this.timer = new Timer();
                    this.timer.Tick += new EventHandler(this.TimerHandler);
                }
                this.timerInterval = 500;
                this.timer.Interval = this.timerInterval;
                this.timer.Start();
            }

            protected void StopTimer()
            {
                if (this.timer != null)
                {
                    this.timer.Stop();
                    this.timer.Dispose();
                    this.timer = null;
                }
                this.parent.OnStopTimer();
            }

            private void TimerHandler(object source, EventArgs args)
            {
                if (!base.Capture)
                {
                    this.EndButtonPress();
                }
                else
                {
                    this.OnUpDown(new UpDownEventArgs((int) this.pushed));
                    this.timerInterval *= 7;
                    this.timerInterval /= 10;
                    if (this.timerInterval < 1)
                    {
                        this.timerInterval = 1;
                    }
                    this.timer.Interval = this.timerInterval;
                }
            }

            internal class UpDownButtonsAccessibleObject : Control.ControlAccessibleObject
            {
                private DirectionButtonAccessibleObject downButton;
                private DirectionButtonAccessibleObject upButton;

                public UpDownButtonsAccessibleObject(UpDownBase.UpDownButtons owner) : base(owner)
                {
                }

                public override AccessibleObject GetChild(int index)
                {
                    if (index == 0)
                    {
                        return this.UpButton;
                    }
                    if (index == 1)
                    {
                        return this.DownButton;
                    }
                    return null;
                }

                public override int GetChildCount()
                {
                    return 2;
                }

                private DirectionButtonAccessibleObject DownButton
                {
                    get
                    {
                        if (this.downButton == null)
                        {
                            this.downButton = new DirectionButtonAccessibleObject(this, false);
                        }
                        return this.downButton;
                    }
                }

                public override string Name
                {
                    get
                    {
                        string name = base.Name;
                        if ((name != null) && (name.Length != 0))
                        {
                            return name;
                        }
                        return "Spinner";
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
                        return AccessibleRole.SpinButton;
                    }
                }

                private DirectionButtonAccessibleObject UpButton
                {
                    get
                    {
                        if (this.upButton == null)
                        {
                            this.upButton = new DirectionButtonAccessibleObject(this, true);
                        }
                        return this.upButton;
                    }
                }

                internal class DirectionButtonAccessibleObject : AccessibleObject
                {
                    private UpDownBase.UpDownButtons.UpDownButtonsAccessibleObject parent;
                    private bool up;

                    public DirectionButtonAccessibleObject(UpDownBase.UpDownButtons.UpDownButtonsAccessibleObject parent, bool up)
                    {
                        this.parent = parent;
                        this.up = up;
                    }

                    public override Rectangle Bounds
                    {
                        get
                        {
                            Rectangle bounds = ((UpDownBase.UpDownButtons) this.parent.Owner).Bounds;
                            bounds.Height /= 2;
                            if (!this.up)
                            {
                                bounds.Y += bounds.Height;
                            }
                            return ((UpDownBase.UpDownButtons) this.parent.Owner).ParentInternal.RectangleToScreen(bounds);
                        }
                    }

                    public override string Name
                    {
                        get
                        {
                            if (this.up)
                            {
                                return System.Windows.Forms.SR.GetString("UpDownBaseUpButtonAccName");
                            }
                            return System.Windows.Forms.SR.GetString("UpDownBaseDownButtonAccName");
                        }
                        set
                        {
                        }
                    }

                    public override AccessibleObject Parent
                    {
                        [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
                        get
                        {
                            return this.parent;
                        }
                    }

                    public override AccessibleRole Role
                    {
                        get
                        {
                            return AccessibleRole.PushButton;
                        }
                    }
                }
            }
        }

        internal class UpDownEdit : System.Windows.Forms.TextBox
        {
            private bool doubleClickFired;
            private UpDownBase parent;

            internal UpDownEdit(UpDownBase parent)
            {
                base.SetStyle(ControlStyles.FixedHeight | ControlStyles.FixedWidth, true);
                base.SetStyle(ControlStyles.Selectable, false);
                this.parent = parent;
            }

            protected override AccessibleObject CreateAccessibilityInstance()
            {
                return new UpDownEditAccessibleObject(this, this.parent);
            }

            protected override void OnGotFocus(EventArgs e)
            {
                this.parent.SetActiveControlInternal(this);
                this.parent.OnGotFocus(e);
            }

            protected override void OnKeyUp(KeyEventArgs e)
            {
                this.parent.OnKeyUp(e);
            }

            protected override void OnLostFocus(EventArgs e)
            {
                this.parent.OnLostFocus(e);
            }

            protected override void OnMouseDown(MouseEventArgs e)
            {
                if ((e.Clicks == 2) && (e.Button == MouseButtons.Left))
                {
                    this.doubleClickFired = true;
                }
                this.parent.OnMouseDown(this.parent.TranslateMouseEvent(this, e));
            }

            protected override void OnMouseUp(MouseEventArgs e)
            {
                Point p = new Point(e.X, e.Y);
                p = base.PointToScreen(p);
                MouseEventArgs args = this.parent.TranslateMouseEvent(this, e);
                if (e.Button == MouseButtons.Left)
                {
                    if (!this.parent.ValidationCancelled && (System.Windows.Forms.UnsafeNativeMethods.WindowFromPoint(p.X, p.Y) == base.Handle))
                    {
                        if (!this.doubleClickFired)
                        {
                            this.parent.OnClick(args);
                            this.parent.OnMouseClick(args);
                        }
                        else
                        {
                            this.doubleClickFired = false;
                            this.parent.OnDoubleClick(args);
                            this.parent.OnMouseDoubleClick(args);
                        }
                    }
                    this.doubleClickFired = false;
                }
                this.parent.OnMouseUp(args);
            }

            internal override void WmContextMenu(ref Message m)
            {
                if ((this.ContextMenu == null) && (this.ContextMenuStrip != null))
                {
                    base.WmContextMenu(ref m, this.parent);
                }
                else
                {
                    base.WmContextMenu(ref m, this);
                }
            }

            internal class UpDownEditAccessibleObject : Control.ControlAccessibleObject
            {
                private UpDownBase parent;

                public UpDownEditAccessibleObject(UpDownBase.UpDownEdit owner, UpDownBase parent) : base(owner)
                {
                    this.parent = parent;
                }

                public override string KeyboardShortcut
                {
                    get
                    {
                        return this.parent.AccessibilityObject.KeyboardShortcut;
                    }
                }

                public override string Name
                {
                    get
                    {
                        return this.parent.AccessibilityObject.Name;
                    }
                    set
                    {
                        this.parent.AccessibilityObject.Name = value;
                    }
                }
            }
        }
    }
}

