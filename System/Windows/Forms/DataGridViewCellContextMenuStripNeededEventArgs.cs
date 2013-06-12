namespace System.Windows.Forms
{
    using System;

    public class DataGridViewCellContextMenuStripNeededEventArgs : DataGridViewCellEventArgs
    {
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;

        public DataGridViewCellContextMenuStripNeededEventArgs(int columnIndex, int rowIndex) : base(columnIndex, rowIndex)
        {
        }

        internal DataGridViewCellContextMenuStripNeededEventArgs(int columnIndex, int rowIndex, System.Windows.Forms.ContextMenuStrip contextMenuStrip) : base(columnIndex, rowIndex)
        {
            this.contextMenuStrip = contextMenuStrip;
        }

        public System.Windows.Forms.ContextMenuStrip ContextMenuStrip
        {
            get
            {
                return this.contextMenuStrip;
            }
            set
            {
                this.contextMenuStrip = value;
            }
        }
    }
}

