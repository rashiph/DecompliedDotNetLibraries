namespace System.Windows.Forms
{
    using System;

    public class DataGridViewCellValueEventArgs : EventArgs
    {
        private int columnIndex;
        private int rowIndex;
        private object val;

        internal DataGridViewCellValueEventArgs()
        {
            this.columnIndex = this.rowIndex = -1;
        }

        public DataGridViewCellValueEventArgs(int columnIndex, int rowIndex)
        {
            if (columnIndex < 0)
            {
                throw new ArgumentOutOfRangeException("columnIndex");
            }
            if (rowIndex < 0)
            {
                throw new ArgumentOutOfRangeException("rowIndex");
            }
            this.rowIndex = rowIndex;
            this.columnIndex = columnIndex;
        }

        internal void SetProperties(int columnIndex, int rowIndex, object value)
        {
            this.columnIndex = columnIndex;
            this.rowIndex = rowIndex;
            this.val = value;
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

        public object Value
        {
            get
            {
                return this.val;
            }
            set
            {
                this.val = value;
            }
        }
    }
}

