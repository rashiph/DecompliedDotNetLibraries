namespace System.Windows.Forms
{
    using System;
    using System.Drawing;

    public class ToolStripItemTextRenderEventArgs : ToolStripItemRenderEventArgs
    {
        private Color defaultTextColor;
        private string text;
        private ContentAlignment textAlignment;
        private Color textColor;
        private bool textColorChanged;
        private ToolStripTextDirection textDirection;
        private Font textFont;
        private TextFormatFlags textFormat;
        private Rectangle textRectangle;

        public ToolStripItemTextRenderEventArgs(Graphics g, ToolStripItem item, string text, Rectangle textRectangle, Color textColor, Font textFont, ContentAlignment textAlign) : base(g, item)
        {
            this.textRectangle = Rectangle.Empty;
            this.textColor = SystemColors.ControlText;
            this.textDirection = ToolStripTextDirection.Horizontal;
            this.defaultTextColor = SystemColors.ControlText;
            this.text = text;
            this.textRectangle = textRectangle;
            this.defaultTextColor = textColor;
            this.textFont = textFont;
            this.textFormat = ToolStripItemInternalLayout.ContentAlignToTextFormat(textAlign, item.RightToLeft == RightToLeft.Yes);
            this.textFormat = item.ShowKeyboardCues ? this.textFormat : (this.textFormat | TextFormatFlags.HidePrefix);
            this.textDirection = item.TextDirection;
        }

        public ToolStripItemTextRenderEventArgs(Graphics g, ToolStripItem item, string text, Rectangle textRectangle, Color textColor, Font textFont, TextFormatFlags format) : base(g, item)
        {
            this.textRectangle = Rectangle.Empty;
            this.textColor = SystemColors.ControlText;
            this.textDirection = ToolStripTextDirection.Horizontal;
            this.defaultTextColor = SystemColors.ControlText;
            this.text = text;
            this.textRectangle = textRectangle;
            this.defaultTextColor = textColor;
            this.textFont = textFont;
            this.textAlignment = item.TextAlign;
            this.textFormat = format;
            this.textDirection = item.TextDirection;
        }

        internal Color DefaultTextColor
        {
            get
            {
                return this.defaultTextColor;
            }
            set
            {
                this.defaultTextColor = value;
            }
        }

        public string Text
        {
            get
            {
                return this.text;
            }
            set
            {
                this.text = value;
            }
        }

        public Color TextColor
        {
            get
            {
                if (this.textColorChanged)
                {
                    return this.textColor;
                }
                return this.DefaultTextColor;
            }
            set
            {
                this.textColor = value;
                this.textColorChanged = true;
            }
        }

        public ToolStripTextDirection TextDirection
        {
            get
            {
                return this.textDirection;
            }
            set
            {
                this.textDirection = value;
            }
        }

        public Font TextFont
        {
            get
            {
                return this.textFont;
            }
            set
            {
                this.textFont = value;
            }
        }

        public TextFormatFlags TextFormat
        {
            get
            {
                return this.textFormat;
            }
            set
            {
                this.textFormat = value;
            }
        }

        public Rectangle TextRectangle
        {
            get
            {
                return this.textRectangle;
            }
            set
            {
                this.textRectangle = value;
            }
        }
    }
}

