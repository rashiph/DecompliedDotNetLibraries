namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Globalization;
    using System.Security.Permissions;
    using System.Windows.Forms.Design;
    using System.Windows.Forms.Layout;

    [DefaultEvent("ButtonClick"), ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.StatusStrip | ToolStripItemDesignerAvailability.ToolStrip)]
    public class ToolStripSplitButton : ToolStripDropDownItem
    {
        private const int DEFAULT_DROPDOWN_WIDTH = 11;
        private ToolStripItem defaultItem;
        private Rectangle dropDownButtonBounds;
        private int dropDownButtonWidth;
        private static readonly object EventButtonClick = new object();
        private static readonly object EventButtonDoubleClick = new object();
        private static readonly object EventDefaultItemChanged = new object();
        private static readonly object EventDropDownClosed = new object();
        private static readonly object EventDropDownOpened = new object();
        private long lastClickTime;
        private byte openMouseId;
        private ToolStripSplitButtonButton splitButtonButton;
        private ToolStripSplitButtonButtonLayout splitButtonButtonLayout;
        private Rectangle splitterBounds;
        private int splitterWidth;

        [System.Windows.Forms.SRCategory("CatAction"), System.Windows.Forms.SRDescription("ToolStripSplitButtonOnButtonClickDescr")]
        public event EventHandler ButtonClick
        {
            add
            {
                base.Events.AddHandler(EventButtonClick, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventButtonClick, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatAction"), System.Windows.Forms.SRDescription("ToolStripSplitButtonOnButtonDoubleClickDescr")]
        public event EventHandler ButtonDoubleClick
        {
            add
            {
                base.Events.AddHandler(EventButtonDoubleClick, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventButtonDoubleClick, value);
            }
        }

        [System.Windows.Forms.SRDescription("ToolStripSplitButtonOnDefaultItemChangedDescr"), System.Windows.Forms.SRCategory("CatAction")]
        public event EventHandler DefaultItemChanged
        {
            add
            {
                base.Events.AddHandler(EventDefaultItemChanged, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventDefaultItemChanged, value);
            }
        }

        public ToolStripSplitButton()
        {
            this.dropDownButtonBounds = Rectangle.Empty;
            this.splitterWidth = 1;
            this.splitterBounds = Rectangle.Empty;
            this.Initialize();
        }

        public ToolStripSplitButton(Image image) : base(null, image, (EventHandler) null)
        {
            this.dropDownButtonBounds = Rectangle.Empty;
            this.splitterWidth = 1;
            this.splitterBounds = Rectangle.Empty;
            this.Initialize();
        }

        public ToolStripSplitButton(string text) : base(text, null, (EventHandler) null)
        {
            this.dropDownButtonBounds = Rectangle.Empty;
            this.splitterWidth = 1;
            this.splitterBounds = Rectangle.Empty;
            this.Initialize();
        }

        public ToolStripSplitButton(string text, Image image) : base(text, image, (EventHandler) null)
        {
            this.dropDownButtonBounds = Rectangle.Empty;
            this.splitterWidth = 1;
            this.splitterBounds = Rectangle.Empty;
            this.Initialize();
        }

        public ToolStripSplitButton(string text, Image image, EventHandler onClick) : base(text, image, onClick)
        {
            this.dropDownButtonBounds = Rectangle.Empty;
            this.splitterWidth = 1;
            this.splitterBounds = Rectangle.Empty;
            this.Initialize();
        }

        public ToolStripSplitButton(string text, Image image, params ToolStripItem[] dropDownItems) : base(text, image, dropDownItems)
        {
            this.dropDownButtonBounds = Rectangle.Empty;
            this.splitterWidth = 1;
            this.splitterBounds = Rectangle.Empty;
            this.Initialize();
        }

        public ToolStripSplitButton(string text, Image image, EventHandler onClick, string name) : base(text, image, onClick, name)
        {
            this.dropDownButtonBounds = Rectangle.Empty;
            this.splitterWidth = 1;
            this.splitterBounds = Rectangle.Empty;
            this.Initialize();
        }

        private void CalculateLayout()
        {
            Rectangle rect = new Rectangle(Point.Empty, this.Size);
            Rectangle empty = Rectangle.Empty;
            rect = new Rectangle(Point.Empty, new Size(Math.Min(base.Width, this.DropDownButtonWidth), base.Height));
            int width = Math.Max(0, base.Width - rect.Width);
            int height = Math.Max(0, base.Height);
            empty = new Rectangle(Point.Empty, new Size(width, height)) {
                Width = empty.Width - this.splitterWidth
            };
            if (this.RightToLeft == RightToLeft.No)
            {
                rect.Offset(empty.Right + this.splitterWidth, 0);
                this.splitterBounds = new Rectangle(empty.Right, empty.Top, this.splitterWidth, empty.Height);
            }
            else
            {
                empty.Offset(this.DropDownButtonWidth + this.splitterWidth, 0);
                this.splitterBounds = new Rectangle(rect.Right, rect.Top, this.splitterWidth, rect.Height);
            }
            this.SplitButtonButton.SetBounds(empty);
            this.SetDropDownButtonBounds(rect);
        }

        protected override AccessibleObject CreateAccessibilityInstance()
        {
            return new ToolStripSplitButtonAccessibleObject(this);
        }

        protected override ToolStripDropDown CreateDefaultDropDown()
        {
            return new ToolStripDropDownMenu(this, true);
        }

        internal override ToolStripItemInternalLayout CreateInternalLayout()
        {
            this.splitButtonButtonLayout = null;
            return new ToolStripItemInternalLayout(this);
        }

        public override Size GetPreferredSize(Size constrainingSize)
        {
            Size preferredSize = this.SplitButtonButtonLayout.GetPreferredSize(constrainingSize);
            preferredSize.Width += (this.DropDownButtonWidth + this.SplitterWidth) + this.Padding.Horizontal;
            return preferredSize;
        }

        private void Initialize()
        {
            this.dropDownButtonWidth = this.DefaultDropDownButtonWidth;
            base.SupportsSpaceKey = true;
        }

        private void InvalidateSplitButtonLayout()
        {
            this.splitButtonButtonLayout = null;
            this.CalculateLayout();
        }

        protected virtual void OnButtonClick(EventArgs e)
        {
            if (this.DefaultItem != null)
            {
                this.DefaultItem.FireEvent(ToolStripItemEventType.Click);
            }
            EventHandler handler = (EventHandler) base.Events[EventButtonClick];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public virtual void OnButtonDoubleClick(EventArgs e)
        {
            if (this.DefaultItem != null)
            {
                this.DefaultItem.FireEvent(ToolStripItemEventType.DoubleClick);
            }
            EventHandler handler = (EventHandler) base.Events[EventButtonDoubleClick];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnDefaultItemChanged(EventArgs e)
        {
            this.InvalidateSplitButtonLayout();
            if (this.CanRaiseEvents)
            {
                EventHandler handler = base.Events[EventDefaultItemChanged] as EventHandler;
                if (handler != null)
                {
                    handler(this, e);
                }
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (this.DropDownButtonBounds.Contains(e.Location))
            {
                if ((e.Button == MouseButtons.Left) && !base.DropDown.Visible)
                {
                    this.openMouseId = (base.ParentInternal == null) ? ((byte) 0) : base.ParentInternal.GetMouseId();
                    base.ShowDropDown(true);
                }
            }
            else
            {
                this.SplitButtonButton.Push(true);
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            this.openMouseId = 0;
            this.SplitButtonButton.Push(false);
            base.OnMouseLeave(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (this.Enabled)
            {
                this.SplitButtonButton.Push(false);
                if ((this.DropDownButtonBounds.Contains(e.Location) && (e.Button == MouseButtons.Left)) && base.DropDown.Visible)
                {
                    byte num = (base.ParentInternal == null) ? ((byte) 0) : base.ParentInternal.GetMouseId();
                    if (num != this.openMouseId)
                    {
                        this.openMouseId = 0;
                        ToolStripManager.ModalMenuFilter.CloseActiveDropDown(base.DropDown, ToolStripDropDownCloseReason.AppClicked);
                        base.Select();
                    }
                }
                Point pt = new Point(e.X, e.Y);
                if ((e.Button == MouseButtons.Left) && this.SplitButtonButton.Bounds.Contains(pt))
                {
                    bool flag = false;
                    if (base.DoubleClickEnabled)
                    {
                        long ticks = DateTime.Now.Ticks;
                        long num3 = ticks - this.lastClickTime;
                        this.lastClickTime = ticks;
                        if ((num3 >= 0L) && (num3 < ToolStripItem.DoubleClickTicks))
                        {
                            flag = true;
                        }
                    }
                    if (flag)
                    {
                        this.OnButtonDoubleClick(new EventArgs());
                        this.lastClickTime = 0L;
                    }
                    else
                    {
                        this.OnButtonClick(new EventArgs());
                    }
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            ToolStripRenderer renderer = base.Renderer;
            if (renderer != null)
            {
                this.InvalidateSplitButtonLayout();
                Graphics g = e.Graphics;
                renderer.DrawSplitButton(new ToolStripItemRenderEventArgs(g, this));
                if ((this.DisplayStyle & ToolStripItemDisplayStyle.Image) != ToolStripItemDisplayStyle.None)
                {
                    renderer.DrawItemImage(new ToolStripItemImageRenderEventArgs(g, this, this.SplitButtonButtonLayout.ImageRectangle));
                }
                if ((this.DisplayStyle & ToolStripItemDisplayStyle.Text) != ToolStripItemDisplayStyle.None)
                {
                    renderer.DrawItemText(new ToolStripItemTextRenderEventArgs(g, this, this.SplitButtonButton.Text, this.SplitButtonButtonLayout.TextRectangle, this.ForeColor, this.Font, this.SplitButtonButtonLayout.TextFormat));
                }
            }
        }

        protected override void OnRightToLeftChanged(EventArgs e)
        {
            base.OnRightToLeftChanged(e);
            this.InvalidateSplitButtonLayout();
        }

        public void PerformButtonClick()
        {
            if (this.Enabled && base.Available)
            {
                base.PerformClick();
                this.OnButtonClick(EventArgs.Empty);
            }
        }

        [UIPermission(SecurityAction.LinkDemand, Window=UIPermissionWindow.AllWindows)]
        protected internal override bool ProcessDialogKey(Keys keyData)
        {
            if (!this.Enabled || ((keyData != Keys.Enter) && (!base.SupportsSpaceKey || (keyData != Keys.Space))))
            {
                return base.ProcessDialogKey(keyData);
            }
            this.PerformButtonClick();
            return true;
        }

        [UIPermission(SecurityAction.LinkDemand, Window=UIPermissionWindow.AllWindows)]
        protected internal override bool ProcessMnemonic(char charCode)
        {
            this.PerformButtonClick();
            return true;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual void ResetDropDownButtonWidth()
        {
            this.DropDownButtonWidth = this.DefaultDropDownButtonWidth;
        }

        private void SetDropDownButtonBounds(Rectangle rect)
        {
            this.dropDownButtonBounds = rect;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal virtual bool ShouldSerializeDropDownButtonWidth()
        {
            return (this.DropDownButtonWidth != this.DefaultDropDownButtonWidth);
        }

        [DefaultValue(true)]
        public bool AutoToolTip
        {
            get
            {
                return base.AutoToolTip;
            }
            set
            {
                base.AutoToolTip = value;
            }
        }

        [Browsable(false)]
        public Rectangle ButtonBounds
        {
            get
            {
                return this.SplitButtonButton.Bounds;
            }
        }

        [Browsable(false)]
        public bool ButtonPressed
        {
            get
            {
                return this.SplitButtonButton.Pressed;
            }
        }

        [Browsable(false)]
        public bool ButtonSelected
        {
            get
            {
                if (!this.SplitButtonButton.Selected)
                {
                    return this.DropDownButtonPressed;
                }
                return true;
            }
        }

        protected override bool DefaultAutoToolTip
        {
            get
            {
                return true;
            }
        }

        private int DefaultDropDownButtonWidth
        {
            get
            {
                return 11;
            }
        }

        [DefaultValue((string) null), Browsable(false)]
        public ToolStripItem DefaultItem
        {
            get
            {
                return this.defaultItem;
            }
            set
            {
                if (this.defaultItem != value)
                {
                    this.OnDefaultItemChanged(new EventArgs());
                    this.defaultItem = value;
                }
            }
        }

        protected internal override bool DismissWhenClicked
        {
            get
            {
                return !base.DropDown.Visible;
            }
        }

        internal override Rectangle DropDownButtonArea
        {
            get
            {
                return this.DropDownButtonBounds;
            }
        }

        [Browsable(false)]
        public Rectangle DropDownButtonBounds
        {
            get
            {
                return this.dropDownButtonBounds;
            }
        }

        [Browsable(false)]
        public bool DropDownButtonPressed
        {
            get
            {
                return base.DropDown.Visible;
            }
        }

        [Browsable(false)]
        public bool DropDownButtonSelected
        {
            get
            {
                return this.Selected;
            }
        }

        [System.Windows.Forms.SRDescription("ToolStripSplitButtonDropDownButtonWidthDescr"), System.Windows.Forms.SRCategory("CatLayout")]
        public int DropDownButtonWidth
        {
            get
            {
                return this.dropDownButtonWidth;
            }
            set
            {
                if (value < 0)
                {
                    object[] args = new object[] { "DropDownButtonWidth", value.ToString(CultureInfo.CurrentCulture), 0.ToString(CultureInfo.CurrentCulture) };
                    throw new ArgumentOutOfRangeException("DropDownButtonWidth", System.Windows.Forms.SR.GetString("InvalidLowBoundArgumentEx", args));
                }
                if (this.dropDownButtonWidth != value)
                {
                    this.dropDownButtonWidth = value;
                    this.InvalidateSplitButtonLayout();
                    base.InvalidateItemLayout(PropertyNames.DropDownButtonWidth, true);
                }
            }
        }

        private ToolStripSplitButtonButton SplitButtonButton
        {
            get
            {
                if (this.splitButtonButton == null)
                {
                    this.splitButtonButton = new ToolStripSplitButtonButton(this);
                }
                this.splitButtonButton.Image = this.Image;
                this.splitButtonButton.Text = this.Text;
                this.splitButtonButton.BackColor = this.BackColor;
                this.splitButtonButton.ForeColor = this.ForeColor;
                this.splitButtonButton.Font = this.Font;
                this.splitButtonButton.ImageAlign = base.ImageAlign;
                this.splitButtonButton.TextAlign = this.TextAlign;
                this.splitButtonButton.TextImageRelation = base.TextImageRelation;
                return this.splitButtonButton;
            }
        }

        internal ToolStripItemInternalLayout SplitButtonButtonLayout
        {
            get
            {
                if ((base.InternalLayout != null) && (this.splitButtonButtonLayout == null))
                {
                    this.splitButtonButtonLayout = new ToolStripSplitButtonButtonLayout(this);
                }
                return this.splitButtonButtonLayout;
            }
        }

        [Browsable(false)]
        public Rectangle SplitterBounds
        {
            get
            {
                return this.splitterBounds;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced), System.Windows.Forms.SRCategory("CatLayout"), System.Windows.Forms.SRDescription("ToolStripSplitButtonSplitterWidthDescr")]
        internal int SplitterWidth
        {
            get
            {
                return this.splitterWidth;
            }
            set
            {
                if (value < 0)
                {
                    this.splitterWidth = 0;
                }
                else
                {
                    this.splitterWidth = value;
                }
                this.InvalidateSplitButtonLayout();
            }
        }

        public class ToolStripSplitButtonAccessibleObject : ToolStripItem.ToolStripItemAccessibleObject
        {
            private ToolStripSplitButton owner;

            public ToolStripSplitButtonAccessibleObject(ToolStripSplitButton item) : base(item)
            {
                this.owner = item;
            }

            [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            public override void DoDefaultAction()
            {
                this.owner.PerformButtonClick();
            }
        }

        private class ToolStripSplitButtonButton : ToolStripButton
        {
            private ToolStripSplitButton owner;

            public ToolStripSplitButtonButton(ToolStripSplitButton owner)
            {
                this.owner = owner;
            }

            public override ToolStripItemDisplayStyle DisplayStyle
            {
                get
                {
                    return this.owner.DisplayStyle;
                }
                set
                {
                }
            }

            public override bool Enabled
            {
                get
                {
                    return this.owner.Enabled;
                }
                set
                {
                }
            }

            public override System.Drawing.Image Image
            {
                get
                {
                    if ((this.owner.DisplayStyle & ToolStripItemDisplayStyle.Image) == ToolStripItemDisplayStyle.Image)
                    {
                        return this.owner.Image;
                    }
                    return null;
                }
                set
                {
                }
            }

            public override System.Windows.Forms.Padding Padding
            {
                get
                {
                    return this.owner.Padding;
                }
                set
                {
                }
            }

            public override bool Selected
            {
                get
                {
                    if (this.owner != null)
                    {
                        return this.owner.Selected;
                    }
                    return base.Selected;
                }
            }

            public override string Text
            {
                get
                {
                    if ((this.owner.DisplayStyle & ToolStripItemDisplayStyle.Text) == ToolStripItemDisplayStyle.Text)
                    {
                        return this.owner.Text;
                    }
                    return null;
                }
                set
                {
                }
            }

            public override ToolStripTextDirection TextDirection
            {
                get
                {
                    return this.owner.TextDirection;
                }
            }
        }

        private class ToolStripSplitButtonButtonLayout : ToolStripItemInternalLayout
        {
            private ToolStripSplitButton owner;

            public ToolStripSplitButtonButtonLayout(ToolStripSplitButton owner) : base(owner.SplitButtonButton)
            {
                this.owner = owner;
            }

            public override Rectangle ImageRectangle
            {
                get
                {
                    Rectangle imageRectangle = base.ImageRectangle;
                    imageRectangle.Offset(this.owner.SplitButtonButton.Bounds.Location);
                    return imageRectangle;
                }
            }

            protected override ToolStripItem Owner
            {
                get
                {
                    return this.owner;
                }
            }

            protected override ToolStrip ParentInternal
            {
                get
                {
                    return this.owner.ParentInternal;
                }
            }

            public override Rectangle TextRectangle
            {
                get
                {
                    Rectangle textRectangle = base.TextRectangle;
                    textRectangle.Offset(this.owner.SplitButtonButton.Bounds.Location);
                    return textRectangle;
                }
            }
        }
    }
}

