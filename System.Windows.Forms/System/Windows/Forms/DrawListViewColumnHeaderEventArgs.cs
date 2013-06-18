namespace System.Windows.Forms
{
    using System;
    using System.Drawing;
    using System.Windows.Forms.VisualStyles;

    public class DrawListViewColumnHeaderEventArgs : EventArgs
    {
        private readonly Color backColor;
        private readonly Rectangle bounds;
        private readonly int columnIndex;
        private bool drawDefault;
        private readonly System.Drawing.Font font;
        private readonly Color foreColor;
        private readonly System.Drawing.Graphics graphics;
        private readonly ColumnHeader header;
        private readonly ListViewItemStates state;

        public DrawListViewColumnHeaderEventArgs(System.Drawing.Graphics graphics, Rectangle bounds, int columnIndex, ColumnHeader header, ListViewItemStates state, Color foreColor, Color backColor, System.Drawing.Font font)
        {
            this.graphics = graphics;
            this.bounds = bounds;
            this.columnIndex = columnIndex;
            this.header = header;
            this.state = state;
            this.foreColor = foreColor;
            this.backColor = backColor;
            this.font = font;
        }

        public void DrawBackground()
        {
            if (Application.RenderWithVisualStyles)
            {
                new VisualStyleRenderer(System.Windows.Forms.VisualStyles.VisualStyleElement.Header.Item.Normal).DrawBackground(this.graphics, this.bounds);
            }
            else
            {
                using (Brush brush = new SolidBrush(this.backColor))
                {
                    this.graphics.FillRectangle(brush, this.bounds);
                }
                Rectangle bounds = this.bounds;
                bounds.Width--;
                bounds.Height--;
                this.graphics.DrawRectangle(SystemPens.ControlDarkDark, bounds);
                bounds.Width--;
                bounds.Height--;
                this.graphics.DrawLine(SystemPens.ControlLightLight, bounds.X, bounds.Y, bounds.Right, bounds.Y);
                this.graphics.DrawLine(SystemPens.ControlLightLight, bounds.X, bounds.Y, bounds.X, bounds.Bottom);
                this.graphics.DrawLine(SystemPens.ControlDark, bounds.X + 1, bounds.Bottom, bounds.Right, bounds.Bottom);
                this.graphics.DrawLine(SystemPens.ControlDark, bounds.Right, bounds.Y + 1, bounds.Right, bounds.Bottom);
            }
        }

        public void DrawText()
        {
            HorizontalAlignment textAlign = this.header.TextAlign;
            TextFormatFlags flags = (textAlign == HorizontalAlignment.Left) ? TextFormatFlags.Default : ((textAlign == HorizontalAlignment.Center) ? TextFormatFlags.HorizontalCenter : TextFormatFlags.Right);
            flags |= TextFormatFlags.WordEllipsis;
            this.DrawText(flags);
        }

        public void DrawText(TextFormatFlags flags)
        {
            string text = this.header.Text;
            int width = TextRenderer.MeasureText(" ", this.font).Width;
            Rectangle bounds = Rectangle.Inflate(this.bounds, -width, 0);
            TextRenderer.DrawText(this.graphics, text, this.font, bounds, this.foreColor, flags);
        }

        public Color BackColor
        {
            get
            {
                return this.backColor;
            }
        }

        public Rectangle Bounds
        {
            get
            {
                return this.bounds;
            }
        }

        public int ColumnIndex
        {
            get
            {
                return this.columnIndex;
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

        public System.Drawing.Font Font
        {
            get
            {
                return this.font;
            }
        }

        public Color ForeColor
        {
            get
            {
                return this.foreColor;
            }
        }

        public System.Drawing.Graphics Graphics
        {
            get
            {
                return this.graphics;
            }
        }

        public ColumnHeader Header
        {
            get
            {
                return this.header;
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

