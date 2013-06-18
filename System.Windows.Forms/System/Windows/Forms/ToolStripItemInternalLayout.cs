namespace System.Windows.Forms
{
    using System;
    using System.Drawing;
    using System.Runtime;
    using System.Windows.Forms.ButtonInternal;
    using System.Windows.Forms.Layout;

    internal class ToolStripItemInternalLayout
    {
        private const int BORDER_HEIGHT = 3;
        private const int BORDER_WIDTH = 2;
        private ToolStripItemLayoutOptions currentLayoutOptions;
        private static readonly Size INVALID_SIZE = new Size(-2147483648, -2147483648);
        private Size lastPreferredSize = INVALID_SIZE;
        private System.Windows.Forms.ButtonInternal.ButtonBaseAdapter.LayoutData layoutData;
        private ToolStripItem ownerItem;
        private ToolStripLayoutData parentLayoutData;

        public ToolStripItemInternalLayout(ToolStripItem ownerItem)
        {
            if (ownerItem == null)
            {
                throw new ArgumentNullException("ownerItem");
            }
            this.ownerItem = ownerItem;
        }

        protected virtual ToolStripItemLayoutOptions CommonLayoutOptions()
        {
            ToolStripItemLayoutOptions options = new ToolStripItemLayoutOptions();
            Rectangle rectangle = new Rectangle(Point.Empty, this.ownerItem.Size);
            options.client = rectangle;
            options.growBorderBy1PxWhenDefault = false;
            options.borderSize = 2;
            options.paddingSize = 0;
            options.maxFocus = true;
            options.focusOddEvenFixup = false;
            options.font = this.ownerItem.Font;
            options.text = ((this.Owner.DisplayStyle & ToolStripItemDisplayStyle.Text) == ToolStripItemDisplayStyle.Text) ? this.Owner.Text : string.Empty;
            options.imageSize = this.PreferredImageSize;
            options.checkSize = 0;
            options.checkPaddingSize = 0;
            options.checkAlign = ContentAlignment.TopLeft;
            options.imageAlign = this.Owner.ImageAlign;
            options.textAlign = this.Owner.TextAlign;
            options.hintTextUp = false;
            options.shadowedText = !this.ownerItem.Enabled;
            options.layoutRTL = RightToLeft.Yes == this.Owner.RightToLeft;
            options.textImageRelation = this.Owner.TextImageRelation;
            options.textImageInset = 0;
            options.everettButtonCompat = false;
            options.gdiTextFormatFlags = ContentAlignToTextFormat(this.Owner.TextAlign, this.Owner.RightToLeft == RightToLeft.Yes);
            options.gdiTextFormatFlags = this.Owner.ShowKeyboardCues ? options.gdiTextFormatFlags : (options.gdiTextFormatFlags | TextFormatFlags.HidePrefix);
            return options;
        }

        internal static TextFormatFlags ContentAlignToTextFormat(ContentAlignment alignment, bool rightToLeft)
        {
            TextFormatFlags flags = TextFormatFlags.Default;
            if (rightToLeft)
            {
                flags |= TextFormatFlags.RightToLeft;
            }
            flags |= ControlPaint.TranslateAlignmentForGDI(alignment);
            return (flags | ControlPaint.TranslateLineAlignmentForGDI(alignment));
        }

        private bool EnsureLayout()
        {
            if (((this.layoutData != null) && (this.parentLayoutData != null)) && this.parentLayoutData.IsCurrent(this.ParentInternal))
            {
                return false;
            }
            this.PerformLayout();
            return true;
        }

        private System.Windows.Forms.ButtonInternal.ButtonBaseAdapter.LayoutData GetLayoutData()
        {
            this.currentLayoutOptions = this.CommonLayoutOptions();
            if (this.Owner.TextDirection != ToolStripTextDirection.Horizontal)
            {
                this.currentLayoutOptions.verticalText = true;
            }
            return this.currentLayoutOptions.Layout();
        }

        public virtual Size GetPreferredSize(Size constrainingSize)
        {
            this.EnsureLayout();
            if (this.ownerItem != null)
            {
                this.lastPreferredSize = this.currentLayoutOptions.GetPreferredSizeCore(constrainingSize);
                return this.lastPreferredSize;
            }
            return Size.Empty;
        }

        internal void PerformLayout()
        {
            this.layoutData = this.GetLayoutData();
            ToolStrip parentInternal = this.ParentInternal;
            if (parentInternal != null)
            {
                this.parentLayoutData = new ToolStripLayoutData(parentInternal);
            }
            else
            {
                this.parentLayoutData = null;
            }
        }

        public virtual Rectangle ContentRectangle
        {
            get
            {
                return this.LayoutData.field;
            }
        }

        public virtual Rectangle ImageRectangle
        {
            get
            {
                Rectangle imageBounds = this.LayoutData.imageBounds;
                imageBounds.Intersect(this.layoutData.field);
                return imageBounds;
            }
        }

        internal System.Windows.Forms.ButtonInternal.ButtonBaseAdapter.LayoutData LayoutData
        {
            get
            {
                this.EnsureLayout();
                return this.layoutData;
            }
        }

        protected virtual ToolStripItem Owner
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return this.ownerItem;
            }
        }

        protected virtual ToolStrip ParentInternal
        {
            get
            {
                if (this.ownerItem == null)
                {
                    return null;
                }
                return this.ownerItem.ParentInternal;
            }
        }

        public Size PreferredImageSize
        {
            get
            {
                return this.Owner.PreferredImageSize;
            }
        }

        public virtual TextFormatFlags TextFormat
        {
            get
            {
                if (this.currentLayoutOptions != null)
                {
                    return this.currentLayoutOptions.gdiTextFormatFlags;
                }
                return this.CommonLayoutOptions().gdiTextFormatFlags;
            }
        }

        public virtual Rectangle TextRectangle
        {
            get
            {
                Rectangle textBounds = this.LayoutData.textBounds;
                textBounds.Intersect(this.layoutData.field);
                return textBounds;
            }
        }

        internal class ToolStripItemLayoutOptions : ButtonBaseAdapter.LayoutOptions
        {
            private Size cachedProposedConstraints = LayoutUtils.InvalidSize;
            private Size cachedSize = LayoutUtils.InvalidSize;

            protected override Size GetTextSize(Size proposedConstraints)
            {
                if ((this.cachedSize == LayoutUtils.InvalidSize) || ((this.cachedProposedConstraints != proposedConstraints) && (this.cachedSize.Width > proposedConstraints.Width)))
                {
                    this.cachedSize = base.GetTextSize(proposedConstraints);
                    this.cachedProposedConstraints = proposedConstraints;
                }
                return this.cachedSize;
            }
        }

        private class ToolStripLayoutData
        {
            private bool autoSize;
            private ToolStripLayoutStyle layoutStyle;
            private Size size;

            public ToolStripLayoutData(ToolStrip toolStrip)
            {
                this.layoutStyle = toolStrip.LayoutStyle;
                this.autoSize = toolStrip.AutoSize;
                this.size = toolStrip.Size;
            }

            public bool IsCurrent(ToolStrip toolStrip)
            {
                if (toolStrip == null)
                {
                    return false;
                }
                return (((toolStrip.Size == this.size) && (toolStrip.LayoutStyle == this.layoutStyle)) && (toolStrip.AutoSize == this.autoSize));
            }
        }
    }
}

