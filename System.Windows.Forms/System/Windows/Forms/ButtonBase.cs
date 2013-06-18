namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Windows.Forms.ButtonInternal;
    using System.Windows.Forms.Layout;

    [ComVisible(true), Designer("System.Windows.Forms.Design.ButtonBaseDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), ClassInterface(ClassInterfaceType.AutoDispatch)]
    public abstract class ButtonBase : Control
    {
        private ButtonBaseAdapter _adapter;
        private System.Windows.Forms.FlatStyle _cachedAdapterType;
        private bool enableVisualStyleBackground = true;
        private const int FlagAutoEllipsis = 0x20;
        private const int FlagCurrentlyAnimating = 0x10;
        private const int FlagInButtonUp = 8;
        private const int FlagIsDefault = 0x40;
        private const int FlagMouseDown = 2;
        private const int FlagMouseOver = 1;
        private const int FlagMousePressed = 4;
        private const int FlagShowToolTip = 0x100;
        private const int FlagUseMnemonic = 0x80;
        private FlatButtonAppearance flatAppearance;
        private System.Windows.Forms.FlatStyle flatStyle = System.Windows.Forms.FlatStyle.Standard;
        private System.Drawing.Image image;
        private ContentAlignment imageAlign = ContentAlignment.MiddleCenter;
        private System.Windows.Forms.ImageList.Indexer imageIndex = new System.Windows.Forms.ImageList.Indexer();
        private System.Windows.Forms.ImageList imageList;
        private bool isEnableVisualStyleBackgroundSet;
        private int state;
        private ContentAlignment textAlign = ContentAlignment.MiddleCenter;
        private System.Windows.Forms.TextImageRelation textImageRelation;
        private ToolTip textToolTip;

        [Browsable(true), System.Windows.Forms.SRDescription("ControlOnAutoSizeChangedDescr"), EditorBrowsable(EditorBrowsableState.Always), System.Windows.Forms.SRCategory("CatPropertyChanged")]
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

        protected ButtonBase()
        {
            base.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.CacheText | ControlStyles.SupportsTransparentBackColor | ControlStyles.StandardClick | ControlStyles.ResizeRedraw | ControlStyles.Opaque, true);
            base.SetState2(0x800, true);
            base.SetStyle(ControlStyles.UserMouse | ControlStyles.UserPaint, this.OwnerDraw);
            this.SetFlag(0x80, true);
            this.SetFlag(0x100, false);
        }

        private void Animate()
        {
            this.Animate(((!base.DesignMode && base.Visible) && base.Enabled) && (this.ParentInternal != null));
        }

        private void Animate(bool animate)
        {
            if (animate != this.GetFlag(0x10))
            {
                if (animate)
                {
                    if (this.image != null)
                    {
                        ImageAnimator.Animate(this.image, new EventHandler(this.OnFrameChanged));
                        this.SetFlag(0x10, animate);
                    }
                }
                else if (this.image != null)
                {
                    ImageAnimator.StopAnimate(this.image, new EventHandler(this.OnFrameChanged));
                    this.SetFlag(0x10, animate);
                }
            }
        }

        protected override AccessibleObject CreateAccessibilityInstance()
        {
            return new ButtonBaseAccessibleObject(this);
        }

        internal virtual ButtonBaseAdapter CreateFlatAdapter()
        {
            return null;
        }

        internal virtual ButtonBaseAdapter CreatePopupAdapter()
        {
            return null;
        }

        internal virtual ButtonBaseAdapter CreateStandardAdapter()
        {
            return null;
        }

        internal virtual StringFormat CreateStringFormat()
        {
            if (this.Adapter == null)
            {
                return new StringFormat();
            }
            return this.Adapter.CreateStringFormat();
        }

        internal virtual TextFormatFlags CreateTextFormatFlags()
        {
            if (this.Adapter == null)
            {
                return TextFormatFlags.Default;
            }
            return this.Adapter.CreateTextFormatFlags();
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
                if (this.imageList != null)
                {
                    this.imageList.Disposed -= new EventHandler(this.DetachImageList);
                }
                if (this.textToolTip != null)
                {
                    this.textToolTip.Dispose();
                    this.textToolTip = null;
                }
            }
            base.Dispose(disposing);
        }

        private bool GetFlag(int flag)
        {
            return ((this.state & flag) == flag);
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
            return LayoutUtils.UnionSizes(this.Adapter.GetPreferredSizeCore(proposedConstraints) + base.Padding.Size, this.MinimumSize);
        }

        private void ImageListRecreateHandle(object sender, EventArgs e)
        {
            if (base.IsHandleCreated)
            {
                base.Invalidate();
            }
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            this.Animate();
            if (!base.Enabled)
            {
                this.SetFlag(2, false);
                this.SetFlag(1, false);
                base.Invalidate();
            }
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

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            base.Invalidate();
        }

        protected override void OnKeyDown(KeyEventArgs kevent)
        {
            if (kevent.KeyData == Keys.Space)
            {
                if (!this.GetFlag(2))
                {
                    this.SetFlag(2, true);
                    if (!this.OwnerDraw)
                    {
                        base.SendMessage(0xf3, 1, 0);
                    }
                    base.Invalidate(this.DownChangeRectangle);
                }
                kevent.Handled = true;
            }
            base.OnKeyDown(kevent);
        }

        protected override void OnKeyUp(KeyEventArgs kevent)
        {
            if (this.GetFlag(2) && !base.ValidationCancelled)
            {
                if (this.OwnerDraw)
                {
                    this.ResetFlagsandPaint();
                }
                else
                {
                    this.SetFlag(4, false);
                    this.SetFlag(2, false);
                    base.SendMessage(0xf3, 0, 0);
                }
                if ((kevent.KeyCode == Keys.Enter) || (kevent.KeyCode == Keys.Space))
                {
                    this.OnClick(EventArgs.Empty);
                }
                kevent.Handled = true;
            }
            base.OnKeyUp(kevent);
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            this.SetFlag(2, false);
            base.CaptureInternal = false;
            base.Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs mevent)
        {
            if (mevent.Button == MouseButtons.Left)
            {
                this.SetFlag(2, true);
                this.SetFlag(4, true);
                base.Invalidate(this.DownChangeRectangle);
            }
            base.OnMouseDown(mevent);
        }

        protected override void OnMouseEnter(EventArgs eventargs)
        {
            this.SetFlag(1, true);
            base.Invalidate();
            if ((!base.DesignMode && this.AutoEllipsis) && (this.ShowToolTip && (this.textToolTip != null)))
            {
                System.Windows.Forms.IntSecurity.AllWindows.Assert();
                try
                {
                    this.textToolTip.Show(WindowsFormsUtils.TextWithoutMnemonics(this.Text), this);
                }
                finally
                {
                    CodeAccessPermission.RevertAssert();
                }
            }
            base.OnMouseEnter(eventargs);
        }

        protected override void OnMouseLeave(EventArgs eventargs)
        {
            this.SetFlag(1, false);
            if (this.textToolTip != null)
            {
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
            base.Invalidate();
            base.OnMouseLeave(eventargs);
        }

        protected override void OnMouseMove(MouseEventArgs mevent)
        {
            if ((mevent.Button != MouseButtons.None) && this.GetFlag(4))
            {
                if (!base.ClientRectangle.Contains(mevent.X, mevent.Y))
                {
                    if (this.GetFlag(2))
                    {
                        this.SetFlag(2, false);
                        base.Invalidate(this.DownChangeRectangle);
                    }
                }
                else if (!this.GetFlag(2))
                {
                    this.SetFlag(2, true);
                    base.Invalidate(this.DownChangeRectangle);
                }
            }
            base.OnMouseMove(mevent);
        }

        protected override void OnMouseUp(MouseEventArgs mevent)
        {
            base.OnMouseUp(mevent);
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            if (this.AutoEllipsis)
            {
                Size preferredSize = base.PreferredSize;
                this.ShowToolTip = (base.ClientRectangle.Width < preferredSize.Width) || (base.ClientRectangle.Height < preferredSize.Height);
            }
            else
            {
                this.ShowToolTip = false;
            }
            if (base.GetStyle(ControlStyles.UserPaint))
            {
                this.Animate();
                ImageAnimator.UpdateFrames();
                this.PaintControl(pevent);
            }
            base.OnPaint(pevent);
        }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            this.Animate();
        }

        protected override void OnTextChanged(EventArgs e)
        {
            using (LayoutTransaction.CreateTransactionIf(this.AutoSize, this.ParentInternal, this, PropertyNames.Text))
            {
                base.OnTextChanged(e);
                base.Invalidate();
            }
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            this.Animate();
        }

        private void PaintControl(PaintEventArgs pevent)
        {
            this.Adapter.Paint(pevent);
        }

        protected void ResetFlagsandPaint()
        {
            this.SetFlag(4, false);
            this.SetFlag(2, false);
            base.Invalidate(this.DownChangeRectangle);
            base.Update();
        }

        private void ResetImage()
        {
            this.Image = null;
        }

        private void ResetUseVisualStyleBackColor()
        {
            this.isEnableVisualStyleBackgroundSet = false;
            this.enableVisualStyleBackground = true;
            base.Invalidate();
        }

        private void SetFlag(int flag, bool value)
        {
            bool flag2 = (this.state & flag) != 0;
            if (value)
            {
                this.state |= flag;
            }
            else
            {
                this.state &= ~flag;
            }
            if ((this.OwnerDraw && ((flag & 2) != 0)) && (value != flag2))
            {
                base.AccessibilityNotifyClients(AccessibleEvents.StateChange, -1);
            }
        }

        private bool ShouldSerializeImage()
        {
            return (this.image != null);
        }

        private bool ShouldSerializeUseVisualStyleBackColor()
        {
            return this.isEnableVisualStyleBackgroundSet;
        }

        private void StopAnimate()
        {
            this.Animate(false);
        }

        private void UpdateOwnerDraw()
        {
            if (this.OwnerDraw != base.GetStyle(ControlStyles.UserPaint))
            {
                base.SetStyle(ControlStyles.UserMouse | ControlStyles.UserPaint, this.OwnerDraw);
                base.RecreateHandle();
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0xf5)
            {
                if (this is IButtonControl)
                {
                    ((IButtonControl) this).PerformClick();
                    return;
                }
                this.OnClick(EventArgs.Empty);
                return;
            }
            if (!this.OwnerDraw)
            {
                if (m.Msg == 0x2111)
                {
                    if ((System.Windows.Forms.NativeMethods.Util.HIWORD(m.WParam) == 0) && !base.ValidationCancelled)
                    {
                        this.OnClick(EventArgs.Empty);
                        return;
                    }
                }
                else
                {
                    base.WndProc(ref m);
                }
                return;
            }
            int msg = m.Msg;
            if (msg <= 0xf3)
            {
                switch (msg)
                {
                    case 8:
                    case 0x1f:
                        goto Label_008C;

                    case 0xf3:
                        return;
                }
                goto Label_00E5;
            }
            if (msg <= 0x205)
            {
                switch (msg)
                {
                    case 0x202:
                    case 0x205:
                        goto Label_00CB;
                }
                goto Label_00E5;
            }
            if (msg == 520)
            {
                goto Label_00CB;
            }
            if (msg != 0x215)
            {
                goto Label_00E5;
            }
        Label_008C:
            if (!this.GetFlag(8) && this.GetFlag(4))
            {
                this.SetFlag(4, false);
                if (this.GetFlag(2))
                {
                    this.SetFlag(2, false);
                    base.Invalidate(this.DownChangeRectangle);
                }
            }
            base.WndProc(ref m);
            return;
        Label_00CB:
            try
            {
                this.SetFlag(8, true);
                base.WndProc(ref m);
                return;
            }
            finally
            {
                this.SetFlag(8, false);
            }
        Label_00E5:
            base.WndProc(ref m);
        }

        internal ButtonBaseAdapter Adapter
        {
            get
            {
                if ((this._adapter == null) || (this.FlatStyle != this._cachedAdapterType))
                {
                    switch (this.FlatStyle)
                    {
                        case System.Windows.Forms.FlatStyle.Flat:
                            this._adapter = this.CreateFlatAdapter();
                            break;

                        case System.Windows.Forms.FlatStyle.Popup:
                            this._adapter = this.CreatePopupAdapter();
                            break;

                        case System.Windows.Forms.FlatStyle.Standard:
                            this._adapter = this.CreateStandardAdapter();
                            break;
                    }
                    this._cachedAdapterType = this.FlatStyle;
                }
                return this._adapter;
            }
        }

        [DefaultValue(false), Browsable(true), EditorBrowsable(EditorBrowsableState.Always), System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("ButtonAutoEllipsisDescr")]
        public bool AutoEllipsis
        {
            get
            {
                return this.GetFlag(0x20);
            }
            set
            {
                if (this.AutoEllipsis != value)
                {
                    this.SetFlag(0x20, value);
                    if (value && (this.textToolTip == null))
                    {
                        this.textToolTip = new ToolTip();
                    }
                    base.Invalidate();
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible), Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
        public override bool AutoSize
        {
            get
            {
                return base.AutoSize;
            }
            set
            {
                base.AutoSize = value;
                if (value)
                {
                    this.AutoEllipsis = false;
                }
            }
        }

        [System.Windows.Forms.SRDescription("ControlBackColorDescr"), System.Windows.Forms.SRCategory("CatAppearance")]
        public override Color BackColor
        {
            get
            {
                return base.BackColor;
            }
            set
            {
                if (base.DesignMode)
                {
                    if (value != Color.Empty)
                    {
                        PropertyDescriptor descriptor = TypeDescriptor.GetProperties(this)["UseVisualStyleBackColor"];
                        if (descriptor != null)
                        {
                            descriptor.SetValue(this, false);
                        }
                    }
                }
                else
                {
                    this.UseVisualStyleBackColor = false;
                }
                base.BackColor = value;
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
                    createParams.ExStyle &= -4097;
                    createParams.Style |= 0x2000;
                    if (this.IsDefault)
                    {
                        createParams.Style |= 1;
                    }
                    ContentAlignment alignment = base.RtlTranslateContent(this.TextAlign);
                    if ((alignment & WindowsFormsUtils.AnyLeftAlign) != ((ContentAlignment) 0))
                    {
                        createParams.Style |= 0x100;
                    }
                    else if ((alignment & WindowsFormsUtils.AnyRightAlign) != ((ContentAlignment) 0))
                    {
                        createParams.Style |= 0x200;
                    }
                    else
                    {
                        createParams.Style |= 0x300;
                    }
                    if ((alignment & WindowsFormsUtils.AnyTopAlign) != ((ContentAlignment) 0))
                    {
                        createParams.Style |= 0x400;
                        return createParams;
                    }
                    if ((alignment & WindowsFormsUtils.AnyBottomAlign) != ((ContentAlignment) 0))
                    {
                        createParams.Style |= 0x800;
                        return createParams;
                    }
                    createParams.Style |= 0xc00;
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
                return new Size(0x4b, 0x17);
            }
        }

        internal virtual Rectangle DownChangeRectangle
        {
            get
            {
                return base.ClientRectangle;
            }
        }

        [Browsable(true), System.Windows.Forms.SRDescription("ButtonFlatAppearance"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), System.Windows.Forms.SRCategory("CatAppearance")]
        public FlatButtonAppearance FlatAppearance
        {
            get
            {
                if (this.flatAppearance == null)
                {
                    this.flatAppearance = new FlatButtonAppearance(this);
                }
                return this.flatAppearance;
            }
        }

        [DefaultValue(2), System.Windows.Forms.SRDescription("ButtonFlatStyleDescr"), Localizable(true), System.Windows.Forms.SRCategory("CatAppearance")]
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
                this.flatStyle = value;
                LayoutTransaction.DoLayoutIf(this.AutoSize, this.ParentInternal, this, PropertyNames.FlatStyle);
                base.Invalidate();
                this.UpdateOwnerDraw();
            }
        }

        [Localizable(true), System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("ButtonImageDescr")]
        public System.Drawing.Image Image
        {
            get
            {
                if ((this.image == null) && (this.imageList != null))
                {
                    int actualIndex = this.imageIndex.ActualIndex;
                    if (actualIndex >= this.imageList.Images.Count)
                    {
                        actualIndex = this.imageList.Images.Count - 1;
                    }
                    if (actualIndex >= 0)
                    {
                        return this.imageList.Images[actualIndex];
                    }
                }
                return this.image;
            }
            set
            {
                if (this.Image != value)
                {
                    this.StopAnimate();
                    this.image = value;
                    if (this.image != null)
                    {
                        this.ImageIndex = -1;
                        this.ImageList = null;
                    }
                    LayoutTransaction.DoLayoutIf(this.AutoSize, this.ParentInternal, this, PropertyNames.Image);
                    this.Animate();
                    base.Invalidate();
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), DefaultValue(0x20), Localizable(true), System.Windows.Forms.SRDescription("ButtonImageAlignDescr")]
        public ContentAlignment ImageAlign
        {
            get
            {
                return this.imageAlign;
            }
            set
            {
                if (!WindowsFormsUtils.EnumValidator.IsValidContentAlignment(value))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(ContentAlignment));
                }
                if (value != this.imageAlign)
                {
                    this.imageAlign = value;
                    LayoutTransaction.DoLayoutIf(this.AutoSize, this.ParentInternal, this, PropertyNames.ImageAlign);
                    base.Invalidate();
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), Editor("System.Windows.Forms.Design.ImageIndexEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), Localizable(true), System.Windows.Forms.SRDescription("ButtonImageIndexDescr"), TypeConverter(typeof(ImageIndexConverter)), DefaultValue(-1), RefreshProperties(RefreshProperties.Repaint)]
        public int ImageIndex
        {
            get
            {
                if (((this.imageIndex.Index != -1) && (this.imageList != null)) && (this.imageIndex.Index >= this.imageList.Images.Count))
                {
                    return (this.imageList.Images.Count - 1);
                }
                return this.imageIndex.Index;
            }
            set
            {
                if (value < -1)
                {
                    object[] args = new object[] { "ImageIndex", value.ToString(CultureInfo.CurrentCulture), -1.ToString(CultureInfo.CurrentCulture) };
                    throw new ArgumentOutOfRangeException("ImageIndex", System.Windows.Forms.SR.GetString("InvalidLowBoundArgumentEx", args));
                }
                if (this.imageIndex.Index != value)
                {
                    if (value != -1)
                    {
                        this.image = null;
                    }
                    this.imageIndex.Index = value;
                    base.Invalidate();
                }
            }
        }

        [TypeConverter(typeof(ImageKeyConverter)), DefaultValue(""), Editor("System.Windows.Forms.Design.ImageIndexEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), Localizable(true), System.Windows.Forms.SRDescription("ButtonImageIndexDescr"), System.Windows.Forms.SRCategory("CatAppearance"), RefreshProperties(RefreshProperties.Repaint)]
        public string ImageKey
        {
            get
            {
                return this.imageIndex.Key;
            }
            set
            {
                if (this.imageIndex.Key != value)
                {
                    if (value != null)
                    {
                        this.image = null;
                    }
                    this.imageIndex.Key = value;
                    base.Invalidate();
                }
            }
        }

        [DefaultValue((string) null), System.Windows.Forms.SRCategory("CatAppearance"), RefreshProperties(RefreshProperties.Repaint), System.Windows.Forms.SRDescription("ButtonImageListDescr")]
        public System.Windows.Forms.ImageList ImageList
        {
            get
            {
                return this.imageList;
            }
            set
            {
                if (this.imageList != value)
                {
                    EventHandler handler = new EventHandler(this.ImageListRecreateHandle);
                    EventHandler handler2 = new EventHandler(this.DetachImageList);
                    if (this.imageList != null)
                    {
                        this.imageList.RecreateHandle -= handler;
                        this.imageList.Disposed -= handler2;
                    }
                    if (value != null)
                    {
                        this.image = null;
                    }
                    this.imageList = value;
                    this.imageIndex.ImageList = value;
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

        protected internal bool IsDefault
        {
            get
            {
                return this.GetFlag(0x40);
            }
            set
            {
                if (this.GetFlag(0x40) != value)
                {
                    this.SetFlag(0x40, value);
                    if (base.IsHandleCreated)
                    {
                        if (this.OwnerDraw)
                        {
                            base.Invalidate();
                        }
                        else
                        {
                            base.UpdateStyles();
                        }
                    }
                }
            }
        }

        internal override bool IsMnemonicsListenerAxSourced
        {
            get
            {
                return true;
            }
        }

        internal bool MouseIsDown
        {
            get
            {
                return this.GetFlag(2);
            }
        }

        internal bool MouseIsOver
        {
            get
            {
                return this.GetFlag(1);
            }
        }

        internal bool MouseIsPressed
        {
            get
            {
                return this.GetFlag(4);
            }
        }

        internal virtual Rectangle OverChangeRectangle
        {
            get
            {
                if (this.FlatStyle == System.Windows.Forms.FlatStyle.Standard)
                {
                    return new Rectangle(-1, -1, 1, 1);
                }
                return base.ClientRectangle;
            }
        }

        internal bool OwnerDraw
        {
            get
            {
                return (this.FlatStyle != System.Windows.Forms.FlatStyle.System);
            }
        }

        internal bool ShowToolTip
        {
            get
            {
                return this.GetFlag(0x100);
            }
            set
            {
                this.SetFlag(0x100, value);
            }
        }

        internal override bool SupportsUseCompatibleTextRendering
        {
            get
            {
                return true;
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

        [Localizable(true), DefaultValue(0x20), System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("ButtonTextAlignDescr")]
        public virtual ContentAlignment TextAlign
        {
            get
            {
                return this.textAlign;
            }
            set
            {
                if (!WindowsFormsUtils.EnumValidator.IsValidContentAlignment(value))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(ContentAlignment));
                }
                if (value != this.textAlign)
                {
                    this.textAlign = value;
                    LayoutTransaction.DoLayoutIf(this.AutoSize, this.ParentInternal, this, PropertyNames.TextAlign);
                    if (this.OwnerDraw)
                    {
                        base.Invalidate();
                    }
                    else
                    {
                        base.UpdateStyles();
                    }
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("ButtonTextImageRelationDescr"), DefaultValue(0), Localizable(true)]
        public System.Windows.Forms.TextImageRelation TextImageRelation
        {
            get
            {
                return this.textImageRelation;
            }
            set
            {
                if (!WindowsFormsUtils.EnumValidator.IsValidTextImageRelation(value))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(System.Windows.Forms.TextImageRelation));
                }
                if (value != this.TextImageRelation)
                {
                    this.textImageRelation = value;
                    LayoutTransaction.DoLayoutIf(this.AutoSize, this.ParentInternal, this, PropertyNames.TextImageRelation);
                    base.Invalidate();
                }
            }
        }

        [System.Windows.Forms.SRDescription("UseCompatibleTextRenderingDescr"), DefaultValue(false), System.Windows.Forms.SRCategory("CatBehavior")]
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

        [DefaultValue(true), System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("ButtonUseMnemonicDescr")]
        public bool UseMnemonic
        {
            get
            {
                return this.GetFlag(0x80);
            }
            set
            {
                this.SetFlag(0x80, value);
                LayoutTransaction.DoLayoutIf(this.AutoSize, this.ParentInternal, this, PropertyNames.Text);
                base.Invalidate();
            }
        }

        [System.Windows.Forms.SRDescription("ButtonUseVisualStyleBackColorDescr"), System.Windows.Forms.SRCategory("CatAppearance")]
        public bool UseVisualStyleBackColor
        {
            get
            {
                if (!this.isEnableVisualStyleBackgroundSet && (!base.RawBackColor.IsEmpty || !(this.BackColor == SystemColors.Control)))
                {
                    return false;
                }
                return this.enableVisualStyleBackground;
            }
            set
            {
                this.isEnableVisualStyleBackgroundSet = true;
                this.enableVisualStyleBackground = value;
                base.Invalidate();
            }
        }

        [ComVisible(true)]
        public class ButtonBaseAccessibleObject : Control.ControlAccessibleObject
        {
            public ButtonBaseAccessibleObject(Control owner) : base(owner)
            {
            }

            [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            public override void DoDefaultAction()
            {
                ((ButtonBase) base.Owner).OnClick(EventArgs.Empty);
            }

            public override AccessibleStates State
            {
                get
                {
                    AccessibleStates state = base.State;
                    ButtonBase owner = (ButtonBase) base.Owner;
                    if (owner.OwnerDraw && owner.MouseIsDown)
                    {
                        state |= AccessibleStates.Pressed;
                    }
                    return state;
                }
            }
        }
    }
}

