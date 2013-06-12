namespace System.Windows.Forms
{
    using System;
    using System.Drawing;

    public class ToolStripItemImageRenderEventArgs : ToolStripItemRenderEventArgs
    {
        private System.Drawing.Image image;
        private Rectangle imageRectangle;
        private bool shiftOnPress;

        public ToolStripItemImageRenderEventArgs(Graphics g, ToolStripItem item, Rectangle imageRectangle) : base(g, item)
        {
            this.imageRectangle = Rectangle.Empty;
            this.image = (item.RightToLeftAutoMirrorImage && (item.RightToLeft == RightToLeft.Yes)) ? item.MirroredImage : item.Image;
            this.imageRectangle = imageRectangle;
        }

        public ToolStripItemImageRenderEventArgs(Graphics g, ToolStripItem item, System.Drawing.Image image, Rectangle imageRectangle) : base(g, item)
        {
            this.imageRectangle = Rectangle.Empty;
            this.image = image;
            this.imageRectangle = imageRectangle;
        }

        public System.Drawing.Image Image
        {
            get
            {
                return this.image;
            }
        }

        public Rectangle ImageRectangle
        {
            get
            {
                return this.imageRectangle;
            }
        }

        internal bool ShiftOnPress
        {
            get
            {
                return this.shiftOnPress;
            }
            set
            {
                this.shiftOnPress = value;
            }
        }
    }
}

