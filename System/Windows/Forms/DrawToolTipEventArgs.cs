namespace System.Windows.Forms
{
    using System;
    using System.Drawing;

    public class DrawToolTipEventArgs : EventArgs
    {
        private readonly Control associatedControl;
        private readonly IWin32Window associatedWindow;
        private readonly Color backColor;
        private readonly Rectangle bounds;
        private readonly System.Drawing.Font font;
        private readonly Color foreColor;
        private readonly System.Drawing.Graphics graphics;
        private readonly string toolTipText;

        public DrawToolTipEventArgs(System.Drawing.Graphics graphics, IWin32Window associatedWindow, Control associatedControl, Rectangle bounds, string toolTipText, Color backColor, Color foreColor, System.Drawing.Font font)
        {
            this.graphics = graphics;
            this.associatedWindow = associatedWindow;
            this.associatedControl = associatedControl;
            this.bounds = bounds;
            this.toolTipText = toolTipText;
            this.backColor = backColor;
            this.foreColor = foreColor;
            this.font = font;
        }

        public void DrawBackground()
        {
            Brush brush = new SolidBrush(this.backColor);
            this.Graphics.FillRectangle(brush, this.bounds);
            brush.Dispose();
        }

        public void DrawBorder()
        {
            ControlPaint.DrawBorder(this.graphics, this.bounds, SystemColors.WindowFrame, ButtonBorderStyle.Solid);
        }

        public void DrawText()
        {
            this.DrawText(TextFormatFlags.HidePrefix | TextFormatFlags.SingleLine | TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
        }

        public void DrawText(TextFormatFlags flags)
        {
            TextRenderer.DrawText(this.graphics, this.toolTipText, this.font, this.bounds, this.foreColor, flags);
        }

        public Control AssociatedControl
        {
            get
            {
                return this.associatedControl;
            }
        }

        public IWin32Window AssociatedWindow
        {
            get
            {
                return this.associatedWindow;
            }
        }

        public Rectangle Bounds
        {
            get
            {
                return this.bounds;
            }
        }

        public System.Drawing.Font Font
        {
            get
            {
                return this.font;
            }
        }

        public System.Drawing.Graphics Graphics
        {
            get
            {
                return this.graphics;
            }
        }

        public string ToolTipText
        {
            get
            {
                return this.toolTipText;
            }
        }
    }
}

