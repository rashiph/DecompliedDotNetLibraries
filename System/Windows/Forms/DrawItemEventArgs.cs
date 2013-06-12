namespace System.Windows.Forms
{
    using System;
    using System.Drawing;

    public class DrawItemEventArgs : EventArgs
    {
        private Color backColor;
        private System.Drawing.Font font;
        private Color foreColor;
        private readonly System.Drawing.Graphics graphics;
        private readonly int index;
        private readonly Rectangle rect;
        private readonly DrawItemState state;

        public DrawItemEventArgs(System.Drawing.Graphics graphics, System.Drawing.Font font, Rectangle rect, int index, DrawItemState state)
        {
            this.graphics = graphics;
            this.font = font;
            this.rect = rect;
            this.index = index;
            this.state = state;
            this.foreColor = SystemColors.WindowText;
            this.backColor = SystemColors.Window;
        }

        public DrawItemEventArgs(System.Drawing.Graphics graphics, System.Drawing.Font font, Rectangle rect, int index, DrawItemState state, Color foreColor, Color backColor)
        {
            this.graphics = graphics;
            this.font = font;
            this.rect = rect;
            this.index = index;
            this.state = state;
            this.foreColor = foreColor;
            this.backColor = backColor;
        }

        public virtual void DrawBackground()
        {
            Brush brush = new SolidBrush(this.BackColor);
            this.Graphics.FillRectangle(brush, this.rect);
            brush.Dispose();
        }

        public virtual void DrawFocusRectangle()
        {
            if (((this.state & DrawItemState.Focus) == DrawItemState.Focus) && ((this.state & DrawItemState.NoFocusRect) != DrawItemState.NoFocusRect))
            {
                ControlPaint.DrawFocusRectangle(this.Graphics, this.rect, this.ForeColor, this.BackColor);
            }
        }

        public Color BackColor
        {
            get
            {
                if ((this.state & DrawItemState.Selected) == DrawItemState.Selected)
                {
                    return SystemColors.Highlight;
                }
                return this.backColor;
            }
        }

        public Rectangle Bounds
        {
            get
            {
                return this.rect;
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
                if ((this.state & DrawItemState.Selected) == DrawItemState.Selected)
                {
                    return SystemColors.HighlightText;
                }
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

        public int Index
        {
            get
            {
                return this.index;
            }
        }

        public DrawItemState State
        {
            get
            {
                return this.state;
            }
        }
    }
}

