namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms.Design;

    [ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.None)]
    public class ToolStripOverflowButton : ToolStripDropDownButton
    {
        private ToolStrip parentToolStrip;

        internal ToolStripOverflowButton(ToolStrip parentToolStrip)
        {
            base.SupportsItemClick = false;
            this.parentToolStrip = parentToolStrip;
        }

        protected override AccessibleObject CreateAccessibilityInstance()
        {
            return new ToolStripOverflowButtonAccessibleObject(this);
        }

        protected override ToolStripDropDown CreateDefaultDropDown()
        {
            return new ToolStripOverflow(this);
        }

        public override Size GetPreferredSize(Size constrainingSize)
        {
            Size size = constrainingSize;
            if (base.ParentInternal != null)
            {
                if (base.ParentInternal.Orientation == Orientation.Horizontal)
                {
                    size.Width = Math.Min(constrainingSize.Width, 0x10);
                }
                else
                {
                    size.Height = Math.Min(constrainingSize.Height, 0x10);
                }
            }
            return (size + this.Padding.Size);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (base.ParentInternal != null)
            {
                base.ParentInternal.Renderer.DrawOverflowButtonBackground(new ToolStripItemRenderEventArgs(e.Graphics, this));
            }
        }

        protected internal override void SetBounds(Rectangle bounds)
        {
            if ((base.ParentInternal != null) && (base.ParentInternal.LayoutEngine is ToolStripSplitStackLayout))
            {
                if (base.ParentInternal.Orientation == Orientation.Horizontal)
                {
                    bounds.Height = base.ParentInternal.Height;
                    bounds.Y = 0;
                }
                else
                {
                    bounds.Width = base.ParentInternal.Width;
                    bounds.X = 0;
                }
            }
            base.SetBounds(bounds);
        }

        protected internal override Padding DefaultMargin
        {
            get
            {
                return Padding.Empty;
            }
        }

        public override bool HasDropDownItems
        {
            get
            {
                return (base.ParentInternal.OverflowItems.Count > 0);
            }
        }

        internal override bool OppositeDropDownAlign
        {
            get
            {
                return true;
            }
        }

        internal ToolStrip ParentToolStrip
        {
            get
            {
                return this.parentToolStrip;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public bool RightToLeftAutoMirrorImage
        {
            get
            {
                return base.RightToLeftAutoMirrorImage;
            }
            set
            {
                base.RightToLeftAutoMirrorImage = value;
            }
        }

        internal class ToolStripOverflowButtonAccessibleObject : ToolStripDropDownItemAccessibleObject
        {
            private string stockName;

            public ToolStripOverflowButtonAccessibleObject(ToolStripOverflowButton owner) : base(owner)
            {
            }

            public override string Name
            {
                get
                {
                    string accessibleName = base.Owner.AccessibleName;
                    if (accessibleName != null)
                    {
                        return accessibleName;
                    }
                    if (string.IsNullOrEmpty(this.stockName))
                    {
                        this.stockName = System.Windows.Forms.SR.GetString("ToolStripOptions");
                    }
                    return this.stockName;
                }
                set
                {
                    base.Name = value;
                }
            }
        }
    }
}

