namespace System.Windows.Forms
{
    using System;

    public class ToolStripItemEventArgs : EventArgs
    {
        private ToolStripItem item;

        public ToolStripItemEventArgs(ToolStripItem item)
        {
            this.item = item;
        }

        public ToolStripItem Item
        {
            get
            {
                return this.item;
            }
        }
    }
}

