namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;

    public class DataGridViewCellValidatingEventArgs : CancelEventArgs
    {
        private int columnIndex;
        private object formattedValue;
        private int rowIndex;

        internal DataGridViewCellValidatingEventArgs(int columnIndex, int rowIndex, object formattedValue)
        {
            this.rowIndex = rowIndex;
            this.columnIndex = columnIndex;
            this.formattedValue = formattedValue;
        }

        public int ColumnIndex
        {
            get
            {
                return this.columnIndex;
            }
        }

        public object FormattedValue
        {
            get
            {
                return this.formattedValue;
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

