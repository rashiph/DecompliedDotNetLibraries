namespace System.Windows.Forms
{
    using System;
    using System.Drawing;

    public class ToolStripSeparatorRenderEventArgs : ToolStripItemRenderEventArgs
    {
        private bool vertical;

        public ToolStripSeparatorRenderEventArgs(Graphics g, ToolStripSeparator separator, bool vertical) : base(g, separator)
        {
            this.vertical = vertical;
        }

        public bool Vertical
        {
            get
            {
                return this.vertical;
            }
        }
    }
}

