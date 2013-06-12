namespace System.Windows.Forms
{
    using System;
    using System.Drawing;

    public class TableLayoutCellPaintEventArgs : PaintEventArgs
    {
        private Rectangle bounds;
        private int column;
        private int row;

        public TableLayoutCellPaintEventArgs(Graphics g, Rectangle clipRectangle, Rectangle cellBounds, int column, int row) : base(g, clipRectangle)
        {
            this.bounds = cellBounds;
            this.row = row;
            this.column = column;
        }

        public Rectangle CellBounds
        {
            get
            {
                return this.bounds;
            }
        }

        public int Column
        {
            get
            {
                return this.column;
            }
        }

        public int Row
        {
            get
            {
                return this.row;
            }
        }
    }
}

