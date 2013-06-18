namespace System.Windows.Forms
{
    using System;

    public class DataGridViewRowDividerDoubleClickEventArgs : HandledMouseEventArgs
    {
        private int rowIndex;

        public DataGridViewRowDividerDoubleClickEventArgs(int rowIndex, HandledMouseEventArgs e) : base(e.Button, e.Clicks, e.X, e.Y, e.Delta, e.Handled)
        {
            if (rowIndex < -1)
            {
                throw new ArgumentOutOfRangeException("rowIndex");
            }
            this.rowIndex = rowIndex;
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

