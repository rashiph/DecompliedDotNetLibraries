namespace System.Windows.Forms
{
    using System;
    using System.Drawing;

    public sealed class AmbientProperties
    {
        private Color backColor;
        private System.Windows.Forms.Cursor cursor;
        private System.Drawing.Font font;
        private Color foreColor;

        public Color BackColor
        {
            get
            {
                return this.backColor;
            }
            set
            {
                this.backColor = value;
            }
        }

        public System.Windows.Forms.Cursor Cursor
        {
            get
            {
                return this.cursor;
            }
            set
            {
                this.cursor = value;
            }
        }

        public System.Drawing.Font Font
        {
            get
            {
                return this.font;
            }
            set
            {
                this.font = value;
            }
        }

        public Color ForeColor
        {
            get
            {
                return this.foreColor;
            }
            set
            {
                this.foreColor = value;
            }
        }
    }
}

