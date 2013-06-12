namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Security.Permissions;
    using System.Windows.Forms.Design;
    using System.Windows.Forms.Layout;

    [ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.StatusStrip | ToolStripItemDesignerAvailability.ToolStrip)]
    public class ToolStripDropDownButton : ToolStripDropDownItem
    {
        private byte openMouseId;
        private bool showDropDownArrow;

        public ToolStripDropDownButton()
        {
            this.showDropDownArrow = true;
            this.Initialize();
        }

        public ToolStripDropDownButton(Image image) : base(null, image, (EventHandler) null)
        {
            this.showDropDownArrow = true;
            this.Initialize();
        }

        public ToolStripDropDownButton(string text) : base(text, null, (EventHandler) null)
        {
            this.showDropDownArrow = true;
            this.Initialize();
        }

        public ToolStripDropDownButton(string text, Image image) : base(text, image, (EventHandler) null)
        {
            this.showDropDownArrow = true;
            this.Initialize();
        }

        public ToolStripDropDownButton(string text, Image image, EventHandler onClick) : base(text, image, onClick)
        {
            this.showDropDownArrow = true;
            this.Initialize();
        }

        public ToolStripDropDownButton(string text, Image image, params ToolStripItem[] dropDownItems) : base(text, image, dropDownItems)
        {
            this.showDropDownArrow = true;
            this.Initialize();
        }

        public ToolStripDropDownButton(string text, Image image, EventHandler onClick, string name) : base(text, image, onClick, name)
        {
            this.showDropDownArrow = true;
            this.Initialize();
        }

        protected override ToolStripDropDown CreateDefaultDropDown()
        {
            return new ToolStripDropDownMenu(this, true);
        }

        internal override ToolStripItemInternalLayout CreateInternalLayout()
        {
            return new ToolStripDropDownButtonInternalLayout(this);
        }

        private void Initialize()
        {
            base.SupportsSpaceKey = true;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if ((Control.ModifierKeys != Keys.Alt) && (e.Button == MouseButtons.Left))
            {
                if (base.DropDown.Visible)
                {
                    ToolStripManager.ModalMenuFilter.CloseActiveDropDown(base.DropDown, ToolStripDropDownCloseReason.AppClicked);
                }
                else
                {
                    this.openMouseId = (base.ParentInternal == null) ? ((byte) 0) : base.ParentInternal.GetMouseId();
                    base.ShowDropDown(true);
                }
            }
            base.OnMouseDown(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            this.openMouseId = 0;
            base.OnMouseLeave(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if ((Control.ModifierKeys != Keys.Alt) && (e.Button == MouseButtons.Left))
            {
                byte num = (base.ParentInternal == null) ? ((byte) 0) : base.ParentInternal.GetMouseId();
                if (num != this.openMouseId)
                {
                    this.openMouseId = 0;
                    ToolStripManager.ModalMenuFilter.CloseActiveDropDown(base.DropDown, ToolStripDropDownCloseReason.AppClicked);
                    base.Select();
                }
            }
            base.OnMouseUp(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (base.Owner != null)
            {
                ToolStripRenderer renderer = base.Renderer;
                Graphics g = e.Graphics;
                renderer.DrawDropDownButtonBackground(new ToolStripItemRenderEventArgs(e.Graphics, this));
                if ((this.DisplayStyle & ToolStripItemDisplayStyle.Image) == ToolStripItemDisplayStyle.Image)
                {
                    renderer.DrawItemImage(new ToolStripItemImageRenderEventArgs(g, this, base.InternalLayout.ImageRectangle));
                }
                if ((this.DisplayStyle & ToolStripItemDisplayStyle.Text) == ToolStripItemDisplayStyle.Text)
                {
                    renderer.DrawItemText(new ToolStripItemTextRenderEventArgs(g, this, this.Text, base.InternalLayout.TextRectangle, this.ForeColor, this.Font, base.InternalLayout.TextFormat));
                }
                if (this.ShowDropDownArrow)
                {
                    ToolStripDropDownButtonInternalLayout internalLayout = base.InternalLayout as ToolStripDropDownButtonInternalLayout;
                    Rectangle arrowRectangle = (internalLayout != null) ? internalLayout.DropDownArrowRect : Rectangle.Empty;
                    Color arrowColor = this.Enabled ? SystemColors.ControlText : SystemColors.ControlDark;
                    renderer.DrawArrow(new ToolStripArrowRenderEventArgs(g, this, arrowRectangle, arrowColor, ArrowDirection.Down));
                }
            }
        }

        [UIPermission(SecurityAction.LinkDemand, Window=UIPermissionWindow.AllWindows)]
        protected internal override bool ProcessMnemonic(char charCode)
        {
            if (this.HasDropDownItems)
            {
                base.Select();
                base.ShowDropDown();
                return true;
            }
            return false;
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

        protected override bool DefaultAutoToolTip
        {
            get
            {
                return true;
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("ToolStripDropDownButtonShowDropDownArrowDescr"), DefaultValue(true)]
        public bool ShowDropDownArrow
        {
            get
            {
                return this.showDropDownArrow;
            }
            set
            {
                if (this.showDropDownArrow != value)
                {
                    this.showDropDownArrow = value;
                    base.InvalidateItemLayout(PropertyNames.ShowDropDownArrow);
                }
            }
        }

        internal class ToolStripDropDownButtonInternalLayout : ToolStripItemInternalLayout
        {
            private static Padding dropDownArrowPadding = new Padding(2);
            private Rectangle dropDownArrowRect;
            private static Size dropDownArrowSize = new Size(5, 3);
            private ToolStripDropDownButton ownerItem;

            public ToolStripDropDownButtonInternalLayout(ToolStripDropDownButton ownerItem) : base(ownerItem)
            {
                this.dropDownArrowRect = Rectangle.Empty;
                this.ownerItem = ownerItem;
            }

            protected override ToolStripItemInternalLayout.ToolStripItemLayoutOptions CommonLayoutOptions()
            {
                ToolStripItemInternalLayout.ToolStripItemLayoutOptions options = base.CommonLayoutOptions();
                if (this.ownerItem.ShowDropDownArrow)
                {
                    if (this.ownerItem.TextDirection == ToolStripTextDirection.Horizontal)
                    {
                        int x = dropDownArrowSize.Width + dropDownArrowPadding.Horizontal;
                        options.client.Width -= x;
                        if (this.ownerItem.RightToLeft == RightToLeft.Yes)
                        {
                            options.client.Offset(x, 0);
                            this.dropDownArrowRect = new Rectangle(dropDownArrowPadding.Left, 0, dropDownArrowSize.Width, this.ownerItem.Bounds.Height);
                            return options;
                        }
                        this.dropDownArrowRect = new Rectangle(options.client.Right, 0, dropDownArrowSize.Width, this.ownerItem.Bounds.Height);
                        return options;
                    }
                    int num2 = dropDownArrowSize.Height + dropDownArrowPadding.Vertical;
                    options.client.Height -= num2;
                    this.dropDownArrowRect = new Rectangle(0, options.client.Bottom + dropDownArrowPadding.Top, this.ownerItem.Bounds.Width - 1, dropDownArrowSize.Height);
                }
                return options;
            }

            public override Size GetPreferredSize(Size constrainingSize)
            {
                Size preferredSize = base.GetPreferredSize(constrainingSize);
                if (this.ownerItem.ShowDropDownArrow)
                {
                    if (this.ownerItem.TextDirection == ToolStripTextDirection.Horizontal)
                    {
                        preferredSize.Width += this.DropDownArrowRect.Width + dropDownArrowPadding.Horizontal;
                        return preferredSize;
                    }
                    preferredSize.Height += this.DropDownArrowRect.Height + dropDownArrowPadding.Vertical;
                }
                return preferredSize;
            }

            public Rectangle DropDownArrowRect
            {
                get
                {
                    return this.dropDownArrowRect;
                }
            }
        }
    }
}

