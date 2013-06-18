namespace System.Windows.Forms
{
    using System;

    public class ToolStripItemClickedEventArgs : EventArgs
    {
        private ToolStripItem clickedItem;

        public ToolStripItemClickedEventArgs(ToolStripItem clickedItem)
        {
            this.clickedItem = clickedItem;
        }

        public ToolStripItem ClickedItem
        {
            get
            {
                return this.clickedItem;
            }
        }
    }
}

