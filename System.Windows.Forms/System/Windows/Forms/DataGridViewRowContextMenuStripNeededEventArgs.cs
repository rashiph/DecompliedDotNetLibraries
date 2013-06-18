namespace System.Windows.Forms
{
    using System;

    public class DataGridViewRowContextMenuStripNeededEventArgs : EventArgs
    {
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private int rowIndex;

        public DataGridViewRowContextMenuStripNeededEventArgs(int rowIndex)
        {
            if (rowIndex < -1)
            {
                throw new ArgumentOutOfRangeException("rowIndex");
            }
            this.rowIndex = rowIndex;
        }

        internal DataGridViewRowContextMenuStripNeededEventArgs(int rowIndex, System.Windows.Forms.ContextMenuStrip contextMenuStrip) : this(rowIndex)
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

        public int RowIndex
        {
            get
            {
                return this.rowIndex;
            }
        }
    }
}

