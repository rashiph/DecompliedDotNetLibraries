namespace System.Windows.Forms
{
    using System;

    public class DataGridViewCellEventArgs : EventArgs
    {
        private int columnIndex;
        private int rowIndex;

        internal DataGridViewCellEventArgs(DataGridViewCell dataGridViewCell) : this(dataGridViewCell.ColumnIndex, dataGridViewCell.RowIndex)
        {
        }

        public DataGridViewCellEventArgs(int columnIndex, int rowIndex)
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

