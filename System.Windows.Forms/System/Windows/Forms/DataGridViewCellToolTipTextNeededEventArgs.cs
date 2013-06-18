namespace System.Windows.Forms
{
    using System;

    public class DataGridViewCellToolTipTextNeededEventArgs : DataGridViewCellEventArgs
    {
        private string toolTipText;

        internal DataGridViewCellToolTipTextNeededEventArgs(int columnIndex, int rowIndex, string toolTipText) : base(columnIndex, rowIndex)
        {
            this.toolTipText = toolTipText;
        }

        public string ToolTipText
        {
            get
            {
                return this.toolTipText;
            }
            set
            {
                this.toolTipText = value;
            }
        }
    }
}

