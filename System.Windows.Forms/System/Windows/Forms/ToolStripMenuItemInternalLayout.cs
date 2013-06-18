namespace System.Windows.Forms
{
    using System;
    using System.Drawing;
    using System.Windows.Forms.Layout;

    internal class ToolStripMenuItemInternalLayout : ToolStripItemInternalLayout
    {
        private ToolStripMenuItem ownerItem;

        public ToolStripMenuItemInternalLayout(ToolStripMenuItem ownerItem) : base(ownerItem)
        {
            this.ownerItem = ownerItem;
        }

        public override Size GetPreferredSize(Size constrainingSize)
        {
            if (this.UseMenuLayout)
            {
                ToolStripDropDownMenu owner = this.ownerItem.Owner as ToolStripDropDownMenu;
                if (owner != null)
                {
                    return owner.MaxItemSize;
                }
            }
            return base.GetPreferredSize(constrainingSize);
        }

        public Rectangle ArrowRectangle
        {
            get
            {
                if (this.UseMenuLayout)
                {
                    ToolStripDropDownMenu owner = this.ownerItem.Owner as ToolStripDropDownMenu;
                    if (owner != null)
                    {
                        Rectangle arrowRectangle = owner.ArrowRectangle;
                        arrowRectangle.Y = LayoutUtils.VAlign(arrowRectangle.Size, this.ownerItem.ClientBounds, ContentAlignment.MiddleCenter).Y;
                        return arrowRectangle;
                    }
                }
                return Rectangle.Empty;
            }
        }

        public Rectangle CheckRectangle
        {
            get
            {
                if (this.UseMenuLayout)
                {
                    ToolStripDropDownMenu owner = this.ownerItem.Owner as ToolStripDropDownMenu;
                    if (owner != null)
                    {
                        Rectangle checkRectangle = owner.CheckRectangle;
                        if (this.ownerItem.CheckedImage != null)
                        {
                            int height = this.ownerItem.CheckedImage.Height;
                            checkRectangle.Y += (checkRectangle.Height - height) / 2;
                            checkRectangle.Height = height;
                            return checkRectangle;
                        }
                    }
                }
                return Rectangle.Empty;
            }
        }

        public override Rectangle ImageRectangle
        {
            get
            {
                if (this.UseMenuLayout)
                {
                    ToolStripDropDownMenu owner = this.ownerItem.Owner as ToolStripDropDownMenu;
                    if (owner != null)
                    {
                        Rectangle imageRectangle = owner.ImageRectangle;
                        if (this.ownerItem.ImageScaling == ToolStripItemImageScaling.SizeToFit)
                        {
                            imageRectangle.Size = owner.ImageScalingSize;
                        }
                        else
                        {
                            Image image = this.ownerItem.Image ?? this.ownerItem.CheckedImage;
                            imageRectangle.Size = image.Size;
                        }
                        imageRectangle.Y = LayoutUtils.VAlign(imageRectangle.Size, this.ownerItem.ClientBounds, ContentAlignment.MiddleCenter).Y;
                        return imageRectangle;
                    }
                }
                return base.ImageRectangle;
            }
        }

        public bool PaintCheck
        {
            get
            {
                if (!this.ShowCheckMargin)
                {
                    return this.ShowImageMargin;
                }
                return true;
            }
        }

        public bool PaintImage
        {
            get
            {
                return this.ShowImageMargin;
            }
        }

        public bool ShowCheckMargin
        {
            get
            {
                ToolStripDropDownMenu owner = this.ownerItem.Owner as ToolStripDropDownMenu;
                return ((owner != null) && owner.ShowCheckMargin);
            }
        }

        public bool ShowImageMargin
        {
            get
            {
                ToolStripDropDownMenu owner = this.ownerItem.Owner as ToolStripDropDownMenu;
                return ((owner != null) && owner.ShowImageMargin);
            }
        }

        public override Rectangle TextRectangle
        {
            get
            {
                if (this.UseMenuLayout)
                {
                    ToolStripDropDownMenu owner = this.ownerItem.Owner as ToolStripDropDownMenu;
                    if (owner != null)
                    {
                        return owner.TextRectangle;
                    }
                }
                return base.TextRectangle;
            }
        }

        public bool UseMenuLayout
        {
            get
            {
                return (this.ownerItem.Owner is ToolStripDropDownMenu);
            }
        }
    }
}

