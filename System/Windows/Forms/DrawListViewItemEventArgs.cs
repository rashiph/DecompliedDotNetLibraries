namespace System.Windows.Forms
{
    using System;
    using System.Drawing;

    public class DrawListViewItemEventArgs : EventArgs
    {
        private readonly Rectangle bounds;
        private bool drawDefault;
        private readonly System.Drawing.Graphics graphics;
        private readonly ListViewItem item;
        private readonly int itemIndex;
        private readonly ListViewItemStates state;

        public DrawListViewItemEventArgs(System.Drawing.Graphics graphics, ListViewItem item, Rectangle bounds, int itemIndex, ListViewItemStates state)
        {
            this.graphics = graphics;
            this.item = item;
            this.bounds = bounds;
            this.itemIndex = itemIndex;
            this.state = state;
            this.drawDefault = false;
        }

        public void DrawBackground()
        {
            Brush brush = new SolidBrush(this.item.BackColor);
            this.Graphics.FillRectangle(brush, this.bounds);
            brush.Dispose();
        }

        public void DrawFocusRectangle()
        {
            if ((this.state & ListViewItemStates.Focused) == ListViewItemStates.Focused)
            {
                Rectangle bounds = this.bounds;
                ControlPaint.DrawFocusRectangle(this.graphics, this.UpdateBounds(bounds, false), this.item.ForeColor, this.item.BackColor);
            }
        }

        public void DrawText()
        {
            this.DrawText(TextFormatFlags.Default);
        }

        public void DrawText(TextFormatFlags flags)
        {
            TextRenderer.DrawText(this.graphics, this.item.Text, this.item.Font, this.UpdateBounds(this.bounds, true), this.item.ForeColor, flags);
        }

        private Rectangle UpdateBounds(Rectangle originalBounds, bool drawText)
        {
            Rectangle rectangle = originalBounds;
            if (this.item.ListView.View == View.Details)
            {
                if (!this.item.ListView.FullRowSelect && (this.item.SubItems.Count > 0))
                {
                    ListViewItem.ListViewSubItem item = this.item.SubItems[0];
                    Size size = TextRenderer.MeasureText(item.Text, item.Font);
                    rectangle = new Rectangle(originalBounds.X, originalBounds.Y, size.Width, size.Height) {
                        X = rectangle.X + 4
                    };
                    rectangle.Width++;
                }
                else
                {
                    rectangle.X += 4;
                    rectangle.Width -= 4;
                }
                if (drawText)
                {
                    rectangle.X--;
                }
            }
            return rectangle;
        }

        public Rectangle Bounds
        {
            get
            {
                return this.bounds;
            }
        }

        public bool DrawDefault
        {
            get
            {
                return this.drawDefault;
            }
            set
            {
                this.drawDefault = value;
            }
        }

        public System.Drawing.Graphics Graphics
        {
            get
            {
                return this.graphics;
            }
        }

        public ListViewItem Item
        {
            get
            {
                return this.item;
            }
        }

        public int ItemIndex
        {
            get
            {
                return this.itemIndex;
            }
        }

        public ListViewItemStates State
        {
            get
            {
                return this.state;
            }
        }
    }
}

