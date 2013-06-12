namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms.Design;
    using System.Windows.Forms.Layout;

    [ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.StatusStrip)]
    public class ToolStripStatusLabel : ToolStripLabel
    {
        private ToolStripStatusLabelBorderSides borderSides;
        private Border3DStyle borderStyle;
        private bool spring;

        public ToolStripStatusLabel()
        {
            this.borderStyle = Border3DStyle.Flat;
        }

        public ToolStripStatusLabel(Image image) : base(null, image, false, null)
        {
            this.borderStyle = Border3DStyle.Flat;
        }

        public ToolStripStatusLabel(string text) : base(text, null, false, null)
        {
            this.borderStyle = Border3DStyle.Flat;
        }

        public ToolStripStatusLabel(string text, Image image) : base(text, image, false, null)
        {
            this.borderStyle = Border3DStyle.Flat;
        }

        public ToolStripStatusLabel(string text, Image image, EventHandler onClick) : base(text, image, false, onClick, null)
        {
            this.borderStyle = Border3DStyle.Flat;
        }

        public ToolStripStatusLabel(string text, Image image, EventHandler onClick, string name) : base(text, image, false, onClick, name)
        {
            this.borderStyle = Border3DStyle.Flat;
        }

        internal override ToolStripItemInternalLayout CreateInternalLayout()
        {
            return new ToolStripStatusLabelLayout(this);
        }

        public override Size GetPreferredSize(Size constrainingSize)
        {
            if (this.BorderSides != ToolStripStatusLabelBorderSides.None)
            {
                return (base.GetPreferredSize(constrainingSize) + new Size(4, 4));
            }
            return base.GetPreferredSize(constrainingSize);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (base.Owner != null)
            {
                ToolStripRenderer renderer = base.Renderer;
                renderer.DrawToolStripStatusLabelBackground(new ToolStripItemRenderEventArgs(e.Graphics, this));
                if ((this.DisplayStyle & ToolStripItemDisplayStyle.Image) == ToolStripItemDisplayStyle.Image)
                {
                    renderer.DrawItemImage(new ToolStripItemImageRenderEventArgs(e.Graphics, this, base.InternalLayout.ImageRectangle));
                }
                base.PaintText(e.Graphics);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), Browsable(false)]
        public ToolStripItemAlignment Alignment
        {
            get
            {
                return base.Alignment;
            }
            set
            {
                base.Alignment = value;
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("ToolStripStatusLabelBorderSidesDescr"), DefaultValue(0)]
        public ToolStripStatusLabelBorderSides BorderSides
        {
            get
            {
                return this.borderSides;
            }
            set
            {
                if (this.borderSides != value)
                {
                    this.borderSides = value;
                    LayoutTransaction.DoLayout(base.Owner, this, PropertyNames.BorderStyle);
                    base.Invalidate();
                }
            }
        }

        [DefaultValue(0x400a), System.Windows.Forms.SRDescription("ToolStripStatusLabelBorderStyleDescr"), System.Windows.Forms.SRCategory("CatAppearance")]
        public Border3DStyle BorderStyle
        {
            get
            {
                return this.borderStyle;
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid_NotSequential(value, (int) value, new int[] { 0x2000, 9, 6, 0x400a, 5, 4, 1, 10, 8, 2 }))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(Border3DStyle));
                }
                if (this.borderStyle != value)
                {
                    this.borderStyle = value;
                    base.Invalidate();
                }
            }
        }

        protected internal override Padding DefaultMargin
        {
            get
            {
                return new Padding(0, 3, 0, 2);
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), DefaultValue(false), System.Windows.Forms.SRDescription("ToolStripStatusLabelSpringDescr")]
        public bool Spring
        {
            get
            {
                return this.spring;
            }
            set
            {
                if (this.spring != value)
                {
                    this.spring = value;
                    if (base.ParentInternal != null)
                    {
                        LayoutTransaction.DoLayout(base.ParentInternal, this, PropertyNames.Spring);
                    }
                }
            }
        }

        private class ToolStripStatusLabelLayout : ToolStripItemInternalLayout
        {
            private ToolStripStatusLabel owner;

            public ToolStripStatusLabelLayout(ToolStripStatusLabel owner) : base(owner)
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

