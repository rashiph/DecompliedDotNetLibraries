namespace System.Windows.Forms
{
    using System;

    public class DataGridViewColumnDividerDoubleClickEventArgs : HandledMouseEventArgs
    {
        private int columnIndex;

        public DataGridViewColumnDividerDoubleClickEventArgs(int columnIndex, HandledMouseEventArgs e) : base(e.Button, e.Clicks, e.X, e.Y, e.Delta, e.Handled)
        {
            if (columnIndex < -1)
            {
                throw new ArgumentOutOfRangeException("columnIndex");
            }
            this.columnIndex = columnIndex;
        }

        public int ColumnIndex
        {
            get
            {
                return this.columnIndex;
            }
        }
    }
}

