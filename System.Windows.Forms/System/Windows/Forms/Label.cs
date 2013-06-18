namespace System.Windows.Forms
{
    using System;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Windows.Forms.Internal;
    using System.Windows.Forms.Layout;

    [ToolboxItem("System.Windows.Forms.Design.AutoSizeToolboxItem,System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), Designer("System.Windows.Forms.Design.LabelDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), DefaultProperty("Text"), ComVisible(true), ClassInterface(ClassInterfaceType.AutoDispatch), System.Windows.Forms.SRDescription("DescriptionLabel"), DefaultBindingProperty("Text")]
    public class Label : Control
    {
        private bool controlToolTip;
        private static readonly object EVENT_TEXTALIGNCHANGED = new object();
        private BitVector32 labelState = new BitVector32();
        private static readonly int PropImage = PropertyStore.CreateKey();
        private static readonly int PropImageAlign = PropertyStore.CreateKey();
        private static readonly int PropImageIndex = PropertyStore.CreateKey();
        private static readonly int PropImageList = PropertyStore.CreateKey();
        private static readonly int PropTextAlign = PropertyStore.CreateKey();
        private int requestedHeight;
        private int requestedWidth;
        internal bool showToolTip;
        private static readonly BitVector32.Section StateAnimating = BitVector32.CreateSection(1, StateAutoSize);
        private static readonly BitVector32.Section StateAutoEllipsis = BitVector32.CreateSection(1, StateBorderStyle);
        private static readonly BitVector32.Section StateAutoSize = BitVector32.CreateSection(1, StateUseMnemonic);
        private static readonly BitVector32.Section StateBorderStyle = BitVector32.CreateSection(2, StateFlatStyle);
        private static readonly BitVector32.Section StateFlatStyle = BitVector32.CreateSection(3, StateAnimating);
        private static readonly BitVector32.Section StateUseMnemonic = BitVector32.CreateSection(1);
        private System.Windows.Forms.Layout.LayoutUtils.MeasureTextCache textMeasurementCache;
        private ToolTip textToolTip;

        [EditorBrowsable(EditorBrowsableState.Always), System.Windows.Forms.SRCategory("CatPropertyChanged"), System.Windows.Forms.SRDescription("ControlOnAutoSizeChangedDescr"), Browsable(true)]
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

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
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

        [System.Windows.Forms.SRDescription("LabelOnTextAlignChangedDescr"), System.Windows.Forms.SRCategory("CatPropertyChanged")]
        public event EventHandler TextAlignChanged
        {
            add
            {
                base.Events.AddHandler(EVENT_TEXTALIGNCHANGED, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_TEXTALIGNCHANGED, value);
            }
        }

        public Label()
        {
            base.SetState2(0x800, true);
            base.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor | ControlStyles.UserPaint, this.IsOwnerDraw());
            base.SetStyle(ControlStyles.Selectable | ControlStyles.FixedHeight, false);
            base.SetStyle(ControlStyles.ResizeRedraw, true);
            CommonProperties.SetSelfAutoSizeInDefaultLayout(this, true);
            this.labelState[StateFlatStyle] = 2;
            this.labelState[StateUseMnemonic] = 1;
            this.labelState[StateBorderStyle] = 0;
            this.TabStop = false;
            this.requestedHeight = base.Height;
            this.requestedWidth = base.Width;
        }

        internal void AdjustSize()
        {
            if (this.SelfSizing && (this.AutoSize || (((this.Anchor & (AnchorStyles.Right | AnchorStyles.Left)) != (AnchorStyles.Right | AnchorStyles.Left)) && ((this.Anchor & (AnchorStyles.Bottom | AnchorStyles.Top)) != (AnchorStyles.Bottom | AnchorStyles.Top)))))
            {
                int requestedHeight = this.requestedHeight;
                int requestedWidth = this.requestedWidth;
                try
                {
                    Size size = this.AutoSize ? base.PreferredSize : new Size(requestedWidth, requestedHeight);
                    base.Size = size;
                }
                finally
                {
                    this.requestedHeight = requestedHeight;
                    this.requestedWidth = requestedWidth;
                }
            }
        }

        internal void Animate()
        {
            this.Animate(((!base.DesignMode && base.Visible) && base.Enabled) && (this.ParentInternal != null));
        }

        private void Animate(bool animate)
        {
            bool flag = this.labelState[StateAnimating] != 0;
            if (animate != flag)
            {
                System.Drawing.Image image = (System.Drawing.Image) base.Properties.GetObject(PropImage);
                if (animate)
                {
                    if (image != null)
                    {
                        ImageAnimator.Animate(image, new EventHandler(this.OnFrameChanged));
                        this.labelState[StateAnimating] = animate ? 1 : 0;
                    }
                }
                else if (image != null)
                {
                    ImageAnimator.StopAnimate(image, new EventHandler(this.OnFrameChanged));
                    this.labelState[StateAnimating] = animate ? 1 : 0;
                }
            }
        }

        protected Rectangle CalcImageRenderBounds(System.Drawing.Image image, Rectangle r, ContentAlignment align)
        {
            Size size = image.Size;
            int x = r.X + 2;
            int y = r.Y + 2;
            if ((align & WindowsFormsUtils.AnyRightAlign) != ((ContentAlignment) 0))
            {
                x = ((r.X + r.Width) - 4) - size.Width;
            }
            else if ((align & WindowsFormsUtils.AnyCenterAlign) != ((ContentAlignment) 0))
            {
                x = r.X + ((r.Width - size.Width) / 2);
            }
            if ((align & WindowsFormsUtils.AnyBottomAlign) != ((ContentAlignment) 0))
            {
                y = ((r.Y + r.Height) - 4) - size.Height;
            }
            else if ((align & WindowsFormsUtils.AnyTopAlign) != ((ContentAlignment) 0))
            {
                y = r.Y + 2;
            }
            else
            {
                y = r.Y + ((r.Height - size.Height) / 2);
            }
            return new Rectangle(x, y, size.Width, size.Height);
        }

        protected override AccessibleObject CreateAccessibilityInstance()
        {
            return new LabelAccessibleObject(this);
        }

        internal virtual StringFormat CreateStringFormat()
        {
            return ControlPaint.CreateStringFormat(this, this.TextAlign, this.AutoEllipsis, this.UseMnemonic);
        }

        private TextFormatFlags CreateTextFormatFlags()
        {
            return this.CreateTextFormatFlags(base.Size - this.GetBordersAndPadding());
        }

        internal virtual TextFormatFlags CreateTextFormatFlags(Size constrainingSize)
        {
            TextFormatFlags flags = ControlPaint.CreateTextFormatFlags(this, this.TextAlign, this.AutoEllipsis, this.UseMnemonic);
            if (!this.MeasureTextCache.TextRequiresWordBreak(this.Text, this.Font, constrainingSize, flags))
            {
                flags &= ~(TextFormatFlags.TextBoxControl | TextFormatFlags.WordBreak);
            }
            return flags;
        }

        private void DetachImageList(object sender, EventArgs e)
        {
            this.ImageList = null;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.StopAnimate();
                if (this.ImageList != null)
                {
                    this.ImageList.Disposed -= new EventHandler(this.DetachImageList);
                    this.ImageList.RecreateHandle -= new EventHandler(this.ImageListRecreateHandle);
                    base.Properties.SetObject(PropImageList, null);
                }
                if (this.Image != null)
                {
                    base.Properties.SetObject(PropImage, null);
                }
                if (this.textToolTip != null)
                {
                    this.textToolTip.Dispose();
                    this.textToolTip = null;
                }
                this.controlToolTip = false;
            }
            base.Dispose(disposing);
        }

        protected void DrawImage(Graphics g, System.Drawing.Image image, Rectangle r, ContentAlignment align)
        {
            Rectangle rectangle = this.CalcImageRenderBounds(image, r, align);
            if (!base.Enabled)
            {
                ControlPaint.DrawImageDisabled(g, image, rectangle.X, rectangle.Y, this.BackColor);
            }
            else
            {
                g.DrawImage(image, rectangle.X, rectangle.Y, image.Width, image.Height);
            }
        }

        private Size GetBordersAndPadding()
        {
            Size size = base.Padding.Size;
            if (this.UseCompatibleTextRendering)
            {
                if (this.BorderStyle != System.Windows.Forms.BorderStyle.None)
                {
                    size.Height += 6;
                    size.Width += 2;
                    return size;
                }
                size.Height += 3;
                return size;
            }
            size += this.SizeFromClientSize(Size.Empty);
            if (this.BorderStyle == System.Windows.Forms.BorderStyle.Fixed3D)
            {
                size += new Size(2, 2);
            }
            return size;
        }

        private int GetLeadingTextPaddingFromTextFormatFlags()
        {
            int iLeftMargin;
            if (!base.IsHandleCreated)
            {
                return 0;
            }
            if (this.UseCompatibleTextRendering && (this.FlatStyle != System.Windows.Forms.FlatStyle.System))
            {
                return 0;
            }
            using (WindowsGraphics graphics = WindowsGraphics.FromHwnd(base.Handle))
            {
                TextFormatFlags flags = this.CreateTextFormatFlags();
                if ((flags & TextFormatFlags.NoPadding) == TextFormatFlags.NoPadding)
                {
                    graphics.TextPadding = TextPaddingOptions.NoPadding;
                }
                else if ((flags & TextFormatFlags.LeftAndRightPadding) == TextFormatFlags.LeftAndRightPadding)
                {
                    graphics.TextPadding = TextPaddingOptions.LeftAndRightPadding;
                }
                using (WindowsFont font = WindowsGraphicsCacheManager.GetWindowsFont(this.Font))
                {
                    iLeftMargin = graphics.GetTextMargins(font).iLeftMargin;
                }
            }
            return iLeftMargin;
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            if (proposedSize.Width == 1)
            {
                proposedSize.Width = 0;
            }
            if (proposedSize.Height == 1)
            {
                proposedSize.Height = 0;
            }
            return base.GetPreferredSize(proposedSize);
        }

        internal override Size GetPreferredSizeCore(Size proposedConstraints)
        {
            Size textExtent;
            Size bordersAndPadding = this.GetBordersAndPadding();
            proposedConstraints -= bordersAndPadding;
            proposedConstraints = LayoutUtils.UnionSizes(proposedConstraints, Size.Empty);
            if (string.IsNullOrEmpty(this.Text))
            {
                using (WindowsFont font = WindowsFont.FromFont(this.Font))
                {
                    textExtent = WindowsGraphicsCacheManager.MeasurementGraphics.GetTextExtent("0", font);
                    textExtent.Width = 0;
                    goto Label_0113;
                }
            }
            if (this.UseGDIMeasuring())
            {
                TextFormatFlags flags = (this.FlatStyle == System.Windows.Forms.FlatStyle.System) ? TextFormatFlags.Default : this.CreateTextFormatFlags(proposedConstraints);
                textExtent = this.MeasureTextCache.GetTextSize(this.Text, this.Font, proposedConstraints, flags);
            }
            else
            {
                using (Graphics graphics = WindowsFormsUtils.CreateMeasurementGraphics())
                {
                    using (StringFormat format = this.CreateStringFormat())
                    {
                        SizeF layoutArea = (proposedConstraints.Width == 1) ? new SizeF(0f, (float) proposedConstraints.Height) : new SizeF((float) proposedConstraints.Width, (float) proposedConstraints.Height);
                        textExtent = Size.Ceiling(graphics.MeasureString(this.Text, this.Font, layoutArea, format));
                    }
                }
            }
        Label_0113:
            return (textExtent + bordersAndPadding);
        }

        private void ImageListRecreateHandle(object sender, EventArgs e)
        {
            if (base.IsHandleCreated)
            {
                base.Invalidate();
            }
        }

        internal bool IsOwnerDraw()
        {
            return (this.FlatStyle != System.Windows.Forms.FlatStyle.System);
        }

        internal virtual void OnAutoEllipsisChanged()
        {
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            this.Animate();
        }

        protected override void OnFontChanged(EventArgs e)
        {
            this.MeasureTextCache.InvalidateCache();
            base.OnFontChanged(e);
            this.AdjustSize();
            base.Invalidate();
        }

        private void OnFrameChanged(object o, EventArgs e)
        {
            if (!base.Disposing && !base.IsDisposed)
            {
                if (base.IsHandleCreated && base.InvokeRequired)
                {
                    base.BeginInvoke(new EventHandler(this.OnFrameChanged), new object[] { o, e });
                }
                else
                {
                    base.Invalidate();
                }
            }
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            base.OnHandleDestroyed(e);
            if ((this.textToolTip != null) && this.textToolTip.GetHandleCreated())
            {
                this.textToolTip.DestroyHandle();
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            if (((!this.controlToolTip && !base.DesignMode) && (this.AutoEllipsis && this.showToolTip)) && (this.textToolTip != null))
            {
                System.Windows.Forms.IntSecurity.AllWindows.Assert();
                try
                {
                    this.controlToolTip = true;
                    this.textToolTip.Show(WindowsFormsUtils.TextWithoutMnemonics(this.Text), this);
                }
                finally
                {
                    CodeAccessPermission.RevertAssert();
                    this.controlToolTip = false;
                }
            }
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            if ((!this.controlToolTip && (this.textToolTip != null)) && this.textToolTip.GetHandleCreated())
            {
                this.textToolTip.RemoveAll();
                System.Windows.Forms.IntSecurity.AllWindows.Assert();
                try
                {
                    this.textToolTip.Hide(this);
                }
                finally
                {
                    CodeAccessPermission.RevertAssert();
                }
            }
            base.OnMouseLeave(e);
        }

        protected override void OnPaddingChanged(EventArgs e)
        {
            base.OnPaddingChanged(e);
            this.AdjustSize();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Color nearestColor;
            this.Animate();
            Rectangle r = LayoutUtils.DeflateRect(base.ClientRectangle, base.Padding);
            ImageAnimator.UpdateFrames();
            System.Drawing.Image image = this.Image;
            if (image != null)
            {
                this.DrawImage(e.Graphics, image, r, base.RtlTranslateAlignment(this.ImageAlign));
            }
            IntPtr hdc = e.Graphics.GetHdc();
            try
            {
                using (WindowsGraphics graphics = WindowsGraphics.FromHdc(hdc))
                {
                    nearestColor = graphics.GetNearestColor(base.Enabled ? this.ForeColor : base.DisabledColor);
                }
            }
            finally
            {
                e.Graphics.ReleaseHdc();
            }
            if (this.AutoEllipsis)
            {
                Rectangle clientRectangle = base.ClientRectangle;
                Size preferredSize = this.GetPreferredSize(new Size(clientRectangle.Width, clientRectangle.Height));
                this.showToolTip = (clientRectangle.Width < preferredSize.Width) || (clientRectangle.Height < preferredSize.Height);
            }
            else
            {
                this.showToolTip = false;
            }
            if (this.UseCompatibleTextRendering)
            {
                using (StringFormat format = this.CreateStringFormat())
                {
                    if (base.Enabled)
                    {
                        using (Brush brush = new SolidBrush(nearestColor))
                        {
                            e.Graphics.DrawString(this.Text, this.Font, brush, r, format);
                            goto Label_01BF;
                        }
                    }
                    ControlPaint.DrawStringDisabled(e.Graphics, this.Text, this.Font, nearestColor, r, format);
                    goto Label_01BF;
                }
            }
            TextFormatFlags flags = this.CreateTextFormatFlags();
            if (base.Enabled)
            {
                TextRenderer.DrawText(e.Graphics, this.Text, this.Font, r, nearestColor, flags);
            }
            else
            {
                Color foreColor = TextRenderer.DisabledTextColor(this.BackColor);
                TextRenderer.DrawText(e.Graphics, this.Text, this.Font, r, foreColor, flags);
            }
        Label_01BF:
            base.OnPaint(e);
        }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            if (this.SelfSizing)
            {
                this.AdjustSize();
            }
            this.Animate();
        }

        protected override void OnRightToLeftChanged(EventArgs e)
        {
            this.MeasureTextCache.InvalidateCache();
            base.OnRightToLeftChanged(e);
        }

        protected virtual void OnTextAlignChanged(EventArgs e)
        {
            EventHandler handler = base.Events[EVENT_TEXTALIGNCHANGED] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected override void OnTextChanged(EventArgs e)
        {
            using (LayoutTransaction.CreateTransactionIf(this.AutoSize, this.ParentInternal, this, PropertyNames.Text))
            {
                this.MeasureTextCache.InvalidateCache();
                base.OnTextChanged(e);
                this.AdjustSize();
                base.Invalidate();
            }
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            this.Animate();
        }

        internal override void PrintToMetaFileRecursive(HandleRef hDC, IntPtr lParam, Rectangle bounds)
        {
            base.PrintToMetaFileRecursive(hDC, lParam, bounds);
            using (new WindowsFormsUtils.DCMapping(hDC, bounds))
            {
                using (Graphics graphics = Graphics.FromHdcInternal(hDC.Handle))
                {
                    ControlPaint.PrintBorder(graphics, new Rectangle(Point.Empty, base.Size), this.BorderStyle, Border3DStyle.SunkenOuter);
                }
            }
        }

        [UIPermission(SecurityAction.LinkDemand, Window=UIPermissionWindow.AllWindows)]
        protected internal override bool ProcessMnemonic(char charCode)
        {
            if ((!this.UseMnemonic || !Control.IsMnemonic(charCode, this.Text)) || !this.CanProcessMnemonic())
            {
                return false;
            }
            Control parentInternal = this.ParentInternal;
            if (parentInternal != null)
            {
                System.Windows.Forms.IntSecurity.ModifyFocus.Assert();
                try
                {
                    if (parentInternal.SelectNextControl(this, true, false, true, false) && !parentInternal.ContainsFocus)
                    {
                        parentInternal.Focus();
                    }
                }
                finally
                {
                    CodeAccessPermission.RevertAssert();
                }
            }
            return true;
        }

        private void ResetImage()
        {
            this.Image = null;
        }

        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            if ((specified & BoundsSpecified.Height) != BoundsSpecified.None)
            {
                this.requestedHeight = height;
            }
            if ((specified & BoundsSpecified.Width) != BoundsSpecified.None)
            {
                this.requestedWidth = width;
            }
            if (this.AutoSize && this.SelfSizing)
            {
                Size preferredSize = base.PreferredSize;
                width = preferredSize.Width;
                height = preferredSize.Height;
            }
            base.SetBoundsCore(x, y, width, height, specified);
        }

        internal void SetToolTip(ToolTip toolTip)
        {
            if ((toolTip != null) && !this.controlToolTip)
            {
                this.controlToolTip = true;
            }
        }

        private bool ShouldSerializeImage()
        {
            return (base.Properties.GetObject(PropImage) != null);
        }

        internal void StopAnimate()
        {
            this.Animate(false);
        }

        public override string ToString()
        {
            return (base.ToString() + ", Text: " + this.Text);
        }

        internal virtual bool UseGDIMeasuring()
        {
            if (this.FlatStyle != System.Windows.Forms.FlatStyle.System)
            {
                return !this.UseCompatibleTextRendering;
            }
            return true;
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x84)
            {
                Rectangle rectangle = base.RectangleToScreen(new Rectangle(0, 0, base.Width, base.Height));
                Point pt = new Point((int) ((long) m.LParam));
                m.Result = rectangle.Contains(pt) ? ((IntPtr) 1) : IntPtr.Zero;
            }
            else
            {
                base.WndProc(ref m);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Always), DefaultValue(false), System.Windows.Forms.SRCategory("CatBehavior"), Browsable(true), System.Windows.Forms.SRDescription("LabelAutoEllipsisDescr")]
        public bool AutoEllipsis
        {
            get
            {
                return (this.labelState[StateAutoEllipsis] != 0);
            }
            set
            {
                if (this.AutoEllipsis != value)
                {
                    this.labelState[StateAutoEllipsis] = value ? 1 : 0;
                    this.MeasureTextCache.InvalidateCache();
                    this.OnAutoEllipsisChanged();
                    if (value && (this.textToolTip == null))
                    {
                        this.textToolTip = new ToolTip();
                    }
                    if (this.ParentInternal != null)
                    {
                        LayoutTransaction.DoLayoutIf(this.AutoSize, this.ParentInternal, this, PropertyNames.AutoEllipsis);
                    }
                    base.Invalidate();
                }
            }
        }

        [System.Windows.Forms.SRDescription("LabelAutoSizeDescr"), RefreshProperties(RefreshProperties.All), EditorBrowsable(EditorBrowsableState.Always), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible), Localizable(true), Browsable(true), DefaultValue(false), System.Windows.Forms.SRCategory("CatLayout")]
        public override bool AutoSize
        {
            get
            {
                return base.AutoSize;
            }
            set
            {
                if (this.AutoSize != value)
                {
                    base.AutoSize = value;
                    this.AdjustSize();
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("LabelBackgroundImageDescr"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public override System.Drawing.Image BackgroundImage
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

        [System.Windows.Forms.SRCategory("CatAppearance"), DispId(-504), System.Windows.Forms.SRDescription("LabelBorderDescr"), DefaultValue(0)]
        public virtual System.Windows.Forms.BorderStyle BorderStyle
        {
            get
            {
                return (System.Windows.Forms.BorderStyle) this.labelState[StateBorderStyle];
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 2))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(System.Windows.Forms.BorderStyle));
                }
                if (this.BorderStyle != value)
                {
                    this.labelState[StateBorderStyle] = (int) value;
                    if (this.ParentInternal != null)
                    {
                        LayoutTransaction.DoLayoutIf(this.AutoSize, this.ParentInternal, this, PropertyNames.BorderStyle);
                    }
                    if (this.AutoSize)
                    {
                        this.AdjustSize();
                    }
                    base.RecreateHandle();
                }
            }
        }

        internal virtual bool CanUseTextRenderer
        {
            get
            {
                return true;
            }
        }

        protected override System.Windows.Forms.CreateParams CreateParams
        {
            [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            get
            {
                System.Windows.Forms.CreateParams createParams = base.CreateParams;
                createParams.ClassName = "STATIC";
                if (this.OwnerDraw)
                {
                    createParams.Style |= 13;
                    createParams.ExStyle &= -4097;
                }
                if (!this.OwnerDraw)
                {
                    switch (this.TextAlign)
                    {
                        case ContentAlignment.TopLeft:
                        case ContentAlignment.MiddleLeft:
                        case ContentAlignment.BottomLeft:
                            createParams.Style = createParams.Style;
                            break;

                        case ContentAlignment.TopCenter:
                        case ContentAlignment.MiddleCenter:
                        case ContentAlignment.BottomCenter:
                            createParams.Style |= 1;
                            break;

                        case ContentAlignment.TopRight:
                        case ContentAlignment.BottomRight:
                        case ContentAlignment.MiddleRight:
                            createParams.Style |= 2;
                            break;
                    }
                }
                else
                {
                    createParams.Style = createParams.Style;
                }
                switch (this.BorderStyle)
                {
                    case System.Windows.Forms.BorderStyle.FixedSingle:
                        createParams.Style |= 0x800000;
                        break;

                    case System.Windows.Forms.BorderStyle.Fixed3D:
                        createParams.Style |= 0x1000;
                        break;
                }
                if (!this.UseMnemonic)
                {
                    createParams.Style |= 0x80;
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

        protected override Padding DefaultMargin
        {
            get
            {
                return new Padding(3, 0, 3, 0);
            }
        }

        protected override Size DefaultSize
        {
            get
            {
                return new Size(100, this.AutoSize ? this.PreferredHeight : 0x17);
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), DefaultValue(2), System.Windows.Forms.SRDescription("ButtonFlatStyleDescr")]
        public System.Windows.Forms.FlatStyle FlatStyle
        {
            get
            {
                return (System.Windows.Forms.FlatStyle) this.labelState[StateFlatStyle];
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 3))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(System.Windows.Forms.FlatStyle));
                }
                if (this.labelState[StateFlatStyle] != value)
                {
                    bool flag = (this.labelState[StateFlatStyle] == 3) || (value == System.Windows.Forms.FlatStyle.System);
                    this.labelState[StateFlatStyle] = (int) value;
                    base.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor | ControlStyles.UserPaint, this.OwnerDraw);
                    if (flag)
                    {
                        LayoutTransaction.DoLayoutIf(this.AutoSize, this.ParentInternal, this, PropertyNames.BorderStyle);
                        if (this.AutoSize)
                        {
                            this.AdjustSize();
                        }
                        base.RecreateHandle();
                    }
                    else
                    {
                        this.Refresh();
                    }
                }
            }
        }

        [Localizable(true), System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("ButtonImageDescr")]
        public System.Drawing.Image Image
        {
            get
            {
                System.Drawing.Image image = (System.Drawing.Image) base.Properties.GetObject(PropImage);
                if (((image == null) && (this.ImageList != null)) && (this.ImageIndexer.ActualIndex >= 0))
                {
                    return this.ImageList.Images[this.ImageIndexer.ActualIndex];
                }
                return image;
            }
            set
            {
                if (this.Image != value)
                {
                    this.StopAnimate();
                    base.Properties.SetObject(PropImage, value);
                    if (value != null)
                    {
                        this.ImageIndex = -1;
                        this.ImageList = null;
                    }
                    this.Animate();
                    base.Invalidate();
                }
            }
        }

        [DefaultValue(0x20), System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("ButtonImageAlignDescr"), Localizable(true)]
        public ContentAlignment ImageAlign
        {
            get
            {
                bool flag;
                int integer = base.Properties.GetInteger(PropImageAlign, out flag);
                if (flag)
                {
                    return (ContentAlignment) integer;
                }
                return ContentAlignment.MiddleCenter;
            }
            set
            {
                if (!WindowsFormsUtils.EnumValidator.IsValidContentAlignment(value))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(ContentAlignment));
                }
                if (value != this.ImageAlign)
                {
                    base.Properties.SetInteger(PropImageAlign, (int) value);
                    LayoutTransaction.DoLayoutIf(this.AutoSize, this.ParentInternal, this, PropertyNames.ImageAlign);
                    base.Invalidate();
                }
            }
        }

        [TypeConverter(typeof(ImageIndexConverter)), Localizable(true), RefreshProperties(RefreshProperties.Repaint), System.Windows.Forms.SRDescription("ButtonImageIndexDescr"), System.Windows.Forms.SRCategory("CatAppearance"), Editor("System.Windows.Forms.Design.ImageIndexEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), DefaultValue(-1)]
        public int ImageIndex
        {
            get
            {
                if (this.ImageIndexer == null)
                {
                    return -1;
                }
                int index = this.ImageIndexer.Index;
                if ((this.ImageList != null) && (index >= this.ImageList.Images.Count))
                {
                    return (this.ImageList.Images.Count - 1);
                }
                return index;
            }
            set
            {
                if (value < -1)
                {
                    object[] args = new object[] { "ImageIndex", value.ToString(CultureInfo.CurrentCulture), -1.ToString(CultureInfo.CurrentCulture) };
                    throw new ArgumentOutOfRangeException("ImageIndex", System.Windows.Forms.SR.GetString("InvalidLowBoundArgumentEx", args));
                }
                if (this.ImageIndex != value)
                {
                    if (value != -1)
                    {
                        base.Properties.SetObject(PropImage, null);
                    }
                    this.ImageIndexer.Index = value;
                    base.Invalidate();
                }
            }
        }

        internal LabelImageIndexer ImageIndexer
        {
            get
            {
                bool flag;
                LabelImageIndexer indexer = base.Properties.GetObject(PropImageIndex, out flag) as LabelImageIndexer;
                if ((indexer == null) || !flag)
                {
                    indexer = new LabelImageIndexer(this);
                    this.ImageIndexer = indexer;
                }
                return indexer;
            }
            set
            {
                base.Properties.SetObject(PropImageIndex, value);
            }
        }

        [RefreshProperties(RefreshProperties.Repaint), TypeConverter(typeof(ImageKeyConverter)), Editor("System.Windows.Forms.Design.ImageIndexEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), DefaultValue(""), Localizable(true), System.Windows.Forms.SRDescription("ButtonImageIndexDescr"), System.Windows.Forms.SRCategory("CatAppearance")]
        public string ImageKey
        {
            get
            {
                if (this.ImageIndexer != null)
                {
                    return this.ImageIndexer.Key;
                }
                return null;
            }
            set
            {
                if (this.ImageKey != value)
                {
                    base.Properties.SetObject(PropImage, null);
                    this.ImageIndexer.Key = value;
                    base.Invalidate();
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("ButtonImageListDescr"), DefaultValue((string) null), RefreshProperties(RefreshProperties.Repaint)]
        public System.Windows.Forms.ImageList ImageList
        {
            get
            {
                return (System.Windows.Forms.ImageList) base.Properties.GetObject(PropImageList);
            }
            set
            {
                if (this.ImageList != value)
                {
                    EventHandler handler = new EventHandler(this.ImageListRecreateHandle);
                    EventHandler handler2 = new EventHandler(this.DetachImageList);
                    System.Windows.Forms.ImageList imageList = this.ImageList;
                    if (imageList != null)
                    {
                        imageList.RecreateHandle -= handler;
                        imageList.Disposed -= handler2;
                    }
                    if (value != null)
                    {
                        base.Properties.SetObject(PropImage, null);
                    }
                    base.Properties.SetObject(PropImageList, value);
                    if (value != null)
                    {
                        value.RecreateHandle += handler;
                        value.Disposed += handler2;
                    }
                    base.Invalidate();
                }
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

        internal override bool IsMnemonicsListenerAxSourced
        {
            get
            {
                return true;
            }
        }

        internal System.Windows.Forms.Layout.LayoutUtils.MeasureTextCache MeasureTextCache
        {
            get
            {
                if (this.textMeasurementCache == null)
                {
                    this.textMeasurementCache = new System.Windows.Forms.Layout.LayoutUtils.MeasureTextCache();
                }
                return this.textMeasurementCache;
            }
        }

        internal virtual bool OwnerDraw
        {
            get
            {
                return this.IsOwnerDraw();
            }
        }

        [System.Windows.Forms.SRCategory("CatLayout"), System.Windows.Forms.SRDescription("LabelPreferredHeightDescr"), Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual int PreferredHeight
        {
            get
            {
                return base.PreferredSize.Height;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), Browsable(false), System.Windows.Forms.SRCategory("CatLayout"), System.Windows.Forms.SRDescription("LabelPreferredWidthDescr"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual int PreferredWidth
        {
            get
            {
                return base.PreferredSize.Width;
            }
        }

        [Obsolete("This property has been deprecated. Use BackColor instead.  http://go.microsoft.com/fwlink/?linkid=14202")]
        protected virtual bool RenderTransparent
        {
            get
            {
                return base.RenderTransparent;
            }
            set
            {
            }
        }

        private bool SelfSizing
        {
            get
            {
                return CommonProperties.ShouldSelfSize(this);
            }
        }

        internal override bool SupportsUseCompatibleTextRendering
        {
            get
            {
                return true;
            }
        }

        [Browsable(false), DefaultValue(false), EditorBrowsable(EditorBrowsableState.Never)]
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

        [SettingsBindable(true), Editor("System.ComponentModel.Design.MultilineStringEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
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

        [System.Windows.Forms.SRCategory("CatAppearance"), DefaultValue(1), System.Windows.Forms.SRDescription("LabelTextAlignDescr"), Localizable(true)]
        public virtual ContentAlignment TextAlign
        {
            get
            {
                bool flag;
                int integer = base.Properties.GetInteger(PropTextAlign, out flag);
                if (flag)
                {
                    return (ContentAlignment) integer;
                }
                return ContentAlignment.TopLeft;
            }
            set
            {
                if (!WindowsFormsUtils.EnumValidator.IsValidContentAlignment(value))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(ContentAlignment));
                }
                if (this.TextAlign != value)
                {
                    base.Properties.SetInteger(PropTextAlign, (int) value);
                    base.Invalidate();
                    if (!this.OwnerDraw)
                    {
                        base.RecreateHandle();
                    }
                    this.OnTextAlignChanged(EventArgs.Empty);
                }
            }
        }

        [DefaultValue(false), System.Windows.Forms.SRDescription("UseCompatibleTextRenderingDescr"), System.Windows.Forms.SRCategory("CatBehavior")]
        public bool UseCompatibleTextRendering
        {
            get
            {
                if (this.CanUseTextRenderer)
                {
                    return base.UseCompatibleTextRenderingInt;
                }
                return true;
            }
            set
            {
                if (base.UseCompatibleTextRenderingInt != value)
                {
                    base.UseCompatibleTextRenderingInt = value;
                    this.AdjustSize();
                }
            }
        }

        [System.Windows.Forms.SRDescription("LabelUseMnemonicDescr"), DefaultValue(true), System.Windows.Forms.SRCategory("CatAppearance")]
        public bool UseMnemonic
        {
            get
            {
                return (this.labelState[StateUseMnemonic] != 0);
            }
            set
            {
                if (this.UseMnemonic != value)
                {
                    this.labelState[StateUseMnemonic] = value ? 1 : 0;
                    this.MeasureTextCache.InvalidateCache();
                    using (LayoutTransaction.CreateTransactionIf(this.AutoSize, this.ParentInternal, this, PropertyNames.Text))
                    {
                        this.AdjustSize();
                        base.Invalidate();
                    }
                    if (base.IsHandleCreated)
                    {
                        int windowStyle = base.WindowStyle;
                        if (!this.UseMnemonic)
                        {
                            windowStyle |= 0x80;
                        }
                        else
                        {
                            windowStyle &= -129;
                        }
                        base.WindowStyle = windowStyle;
                    }
                }
            }
        }

        [ComVisible(true)]
        internal class LabelAccessibleObject : Control.ControlAccessibleObject
        {
            public LabelAccessibleObject(Label owner) : base(owner)
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
                    return AccessibleRole.StaticText;
                }
            }
        }
    }
}

