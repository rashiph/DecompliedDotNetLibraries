namespace System.Windows.Forms
{
    using System;
    using System.Drawing;

    public class StatusBarDrawItemEventArgs : DrawItemEventArgs
    {
        private readonly StatusBarPanel panel;

        public StatusBarDrawItemEventArgs(Graphics g, Font font, Rectangle r, int itemId, DrawItemState itemState, StatusBarPanel panel) : base(g, font, r, itemId, itemState)
        {
            this.panel = panel;
        }

        public StatusBarDrawItemEventArgs(Graphics g, Font font, Rectangle r, int itemId, DrawItemState itemState, StatusBarPanel panel, Color foreColor, Color backColor) : base(g, font, r, itemId, itemState, foreColor, backColor)
        {
            this.panel = panel;
        }

        public StatusBarPanel Panel
        {
            get
            {
                return this.panel;
            }
        }
    }
}

