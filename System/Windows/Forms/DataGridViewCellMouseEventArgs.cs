namespace System.Windows.Forms
{
    using System;

    public class DataGridViewCellMouseEventArgs : MouseEventArgs
    {
        private int columnIndex;
        private int rowIndex;

        public DataGridViewCellMouseEventArgs(int columnIndex, int rowIndex, int localX, int localY, MouseEventArgs e) : base(e.Button, e.Clicks, localX, localY, e.Delta)
        {
            if (columnIndex < -1)
            {
                throw new ArgumentOutOfRangeException("columnIndex");
            }
            if (rowIndex < -1)
            {
                throw new ArgumentOutOfRangeException("rowIndex");
            }
            this.columnIndex = columnIndex;
            this.rowIndex = rowIndex;
        }

        public int ColumnIndex
        {
            get
            {
                return this.columnIndex;
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

