namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Windows.Forms.Design;

    [ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.ToolStrip)]
    public class ToolStripLabel : ToolStripItem
    {
        private Color activeLinkColor;
        private Font hoverLinkFont;
        private bool isLink;
        private Cursor lastCursor;
        private System.Windows.Forms.LinkBehavior linkBehavior;
        private Color linkColor;
        private Font linkFont;
        private bool linkVisited;
        private Color visitedLinkColor;

        public ToolStripLabel()
        {
            this.linkColor = Color.Empty;
            this.activeLinkColor = Color.Empty;
            this.visitedLinkColor = Color.Empty;
        }

        public ToolStripLabel(Image image) : base(null, image, null)
        {
            this.linkColor = Color.Empty;
            this.activeLinkColor = Color.Empty;
            this.visitedLinkColor = Color.Empty;
        }

        public ToolStripLabel(string text) : base(text, null, null)
        {
            this.linkColor = Color.Empty;
            this.activeLinkColor = Color.Empty;
            this.visitedLinkColor = Color.Empty;
        }

        public ToolStripLabel(string text, Image image) : base(text, image, null)
        {
            this.linkColor = Color.Empty;
            this.activeLinkColor = Color.Empty;
            this.visitedLinkColor = Color.Empty;
        }

        public ToolStripLabel(string text, Image image, bool isLink) : this(text, image, isLink, null)
        {
        }

        public ToolStripLabel(string text, Image image, bool isLink, EventHandler onClick) : this(text, image, isLink, onClick, null)
        {
        }

        public ToolStripLabel(string text, Image image, bool isLink, EventHandler onClick, string name) : base(text, image, onClick, name)
        {
            this.linkColor = Color.Empty;
            this.activeLinkColor = Color.Empty;
            this.visitedLinkColor = Color.Empty;
            this.IsLink = isLink;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected override AccessibleObject CreateAccessibilityInstance()
        {
            return new ToolStripLabelAccessibleObject(this);
        }

        internal override ToolStripItemInternalLayout CreateInternalLayout()
        {
            return new ToolStripLabelLayout(this);
        }

        private void InvalidateLinkFonts()
        {
            if (this.linkFont != null)
            {
                this.linkFont.Dispose();
            }
            if ((this.hoverLinkFont != null) && (this.hoverLinkFont != this.linkFont))
            {
                this.hoverLinkFont.Dispose();
            }
            this.linkFont = null;
            this.hoverLinkFont = null;
        }

        protected override void OnFontChanged(EventArgs e)
        {
            this.InvalidateLinkFonts();
            base.OnFontChanged(e);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            if (this.IsLink)
            {
                ToolStrip parent = base.Parent;
                if (parent != null)
                {
                    this.lastCursor = parent.Cursor;
                    parent.Cursor = Cursors.Hand;
                }
            }
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            if (this.IsLink)
            {
                ToolStrip parent = base.Parent;
                if (parent != null)
                {
                    parent.Cursor = this.lastCursor;
                }
            }
            base.OnMouseLeave(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (base.Owner != null)
            {
                ToolStripRenderer renderer = base.Renderer;
                renderer.DrawLabelBackground(new ToolStripItemRenderEventArgs(e.Graphics, this));
                if ((this.DisplayStyle & ToolStripItemDisplayStyle.Image) == ToolStripItemDisplayStyle.Image)
                {
                    renderer.DrawItemImage(new ToolStripItemImageRenderEventArgs(e.Graphics, this, base.InternalLayout.ImageRectangle));
                }
                this.PaintText(e.Graphics);
            }
        }

        internal void PaintText(Graphics g)
        {
            ToolStripRenderer renderer = base.Renderer;
            if ((this.DisplayStyle & ToolStripItemDisplayStyle.Text) == ToolStripItemDisplayStyle.Text)
            {
                Font baseFont = this.Font;
                Color foreColor = this.ForeColor;
                if (this.IsLink)
                {
                    LinkUtilities.EnsureLinkFonts(baseFont, this.LinkBehavior, ref this.linkFont, ref this.hoverLinkFont);
                    if (this.Pressed)
                    {
                        baseFont = this.hoverLinkFont;
                        foreColor = this.ActiveLinkColor;
                    }
                    else if (this.Selected)
                    {
                        baseFont = this.hoverLinkFont;
                        foreColor = this.LinkVisited ? this.VisitedLinkColor : this.LinkColor;
                    }
                    else
                    {
                        baseFont = this.linkFont;
                        foreColor = this.LinkVisited ? this.VisitedLinkColor : this.LinkColor;
                    }
                }
                Rectangle textRectangle = base.InternalLayout.TextRectangle;
                renderer.DrawItemText(new ToolStripItemTextRenderEventArgs(g, this, this.Text, textRectangle, foreColor, baseFont, base.InternalLayout.TextFormat));
            }
        }

        [UIPermission(SecurityAction.LinkDemand, Window=UIPermissionWindow.AllWindows)]
        protected internal override bool ProcessMnemonic(char charCode)
        {
            if (base.ParentInternal == null)
            {
                return false;
            }
            if (!this.CanSelect)
            {
                base.ParentInternal.SetFocusUnsafe();
                base.ParentInternal.SelectNextToolStripItem(this, true);
            }
            else
            {
                base.FireEvent(ToolStripItemEventType.Click);
            }
            return true;
        }

        private void ResetActiveLinkColor()
        {
            this.ActiveLinkColor = this.IEActiveLinkColor;
        }

        private void ResetLinkColor()
        {
            this.LinkColor = this.IELinkColor;
        }

        private void ResetVisitedLinkColor()
        {
            this.VisitedLinkColor = this.IEVisitedLinkColor;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        private bool ShouldSerializeActiveLinkColor()
        {
            return !this.activeLinkColor.IsEmpty;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        private bool ShouldSerializeLinkColor()
        {
            return !this.linkColor.IsEmpty;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        private bool ShouldSerializeVisitedLinkColor()
        {
            return !this.visitedLinkColor.IsEmpty;
        }

        [System.Windows.Forms.SRDescription("ToolStripLabelActiveLinkColorDescr"), System.Windows.Forms.SRCategory("CatAppearance")]
        public Color ActiveLinkColor
        {
            get
            {
                if (this.activeLinkColor.IsEmpty)
                {
                    return this.IEActiveLinkColor;
                }
                return this.activeLinkColor;
            }
            set
            {
                if (this.activeLinkColor != value)
                {
                    this.activeLinkColor = value;
                    base.Invalidate();
                }
            }
        }

        public override bool CanSelect
        {
            get
            {
                if (!this.IsLink)
                {
                    return base.DesignMode;
                }
                return true;
            }
        }

        private Color IEActiveLinkColor
        {
            get
            {
                return LinkUtilities.IEActiveLinkColor;
            }
        }

        private Color IELinkColor
        {
            get
            {
                return LinkUtilities.IELinkColor;
            }
        }

        private Color IEVisitedLinkColor
        {
            get
            {
                return LinkUtilities.IEVisitedLinkColor;
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("ToolStripLabelIsLinkDescr"), DefaultValue(false)]
        public bool IsLink
        {
            get
            {
                return this.isLink;
            }
            set
            {
                if (this.isLink != value)
                {
                    this.isLink = value;
                    base.Invalidate();
                }
            }
        }

        [DefaultValue(0), System.Windows.Forms.SRDescription("ToolStripLabelLinkBehaviorDescr"), System.Windows.Forms.SRCategory("CatBehavior")]
        public System.Windows.Forms.LinkBehavior LinkBehavior
        {
            get
            {
                return this.linkBehavior;
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 3))
                {
                    throw new InvalidEnumArgumentException("LinkBehavior", (int) value, typeof(System.Windows.Forms.LinkBehavior));
                }
                if (this.linkBehavior != value)
                {
                    this.linkBehavior = value;
                    this.InvalidateLinkFonts();
                    base.Invalidate();
                }
            }
        }

        [System.Windows.Forms.SRDescription("ToolStripLabelLinkColorDescr"), System.Windows.Forms.SRCategory("CatAppearance")]
        public Color LinkColor
        {
            get
            {
                if (this.linkColor.IsEmpty)
                {
                    return this.IELinkColor;
                }
                return this.linkColor;
            }
            set
            {
                if (this.linkColor != value)
                {
                    this.linkColor = value;
                    base.Invalidate();
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("ToolStripLabelLinkVisitedDescr"), DefaultValue(false)]
        public bool LinkVisited
        {
            get
            {
                return this.linkVisited;
            }
            set
            {
                if (this.linkVisited != value)
                {
                    this.linkVisited = value;
                    base.Invalidate();
                }
            }
        }

        [System.Windows.Forms.SRDescription("ToolStripLabelVisitedLinkColorDescr"), System.Windows.Forms.SRCategory("CatAppearance")]
        public Color VisitedLinkColor
        {
            get
            {
                if (this.visitedLinkColor.IsEmpty)
                {
                    return this.IEVisitedLinkColor;
                }
                return this.visitedLinkColor;
            }
            set
            {
                if (this.visitedLinkColor != value)
                {
                    this.visitedLinkColor = value;
                    base.Invalidate();
                }
            }
        }

        [ComVisible(true)]
        internal class ToolStripLabelAccessibleObject : ToolStripItem.ToolStripItemAccessibleObject
        {
            private ToolStripLabel ownerItem;

            public ToolStripLabelAccessibleObject(ToolStripLabel ownerItem) : base(ownerItem)
            {
                this.ownerItem = ownerItem;
            }

            public override void DoDefaultAction()
            {
                if (this.ownerItem.IsLink)
                {
                    base.DoDefaultAction();
                }
            }

            public override string DefaultAction
            {
                get
                {
                    if (this.ownerItem.IsLink)
                    {
                        return System.Windows.Forms.SR.GetString("AccessibleActionClick");
                    }
                    return string.Empty;
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
                    if (!this.ownerItem.IsLink)
                    {
                        return AccessibleRole.StaticText;
                    }
                    return AccessibleRole.Link;
                }
            }

            public override AccessibleStates State
            {
                get
                {
                    return (base.State | AccessibleStates.ReadOnly);
                }
            }
        }

        private class ToolStripLabelLayout : ToolStripItemInternalLayout
        {
            private ToolStripLabel owner;

            public ToolStripLabelLayout(ToolStripLabel owner) : base(owner)
            {
                this.owner = owner;
            }

            protected override ToolStripItemInternalLayout.ToolStripItemLayoutOptions CommonLayoutOptions()
            {
                ToolStripItemInternalLayout.ToolStripItemLayoutOptions options = base.CommonLayoutOptions();
                options.borderSize = 0;
                return options;
            }
        }
    }
}

