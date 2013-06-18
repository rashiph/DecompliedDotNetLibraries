namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;

    public class DataGridViewSortCompareEventArgs : HandledEventArgs
    {
        private object cellValue1;
        private object cellValue2;
        private DataGridViewColumn dataGridViewColumn;
        private int rowIndex1;
        private int rowIndex2;
        private int sortResult;

        public DataGridViewSortCompareEventArgs(DataGridViewColumn dataGridViewColumn, object cellValue1, object cellValue2, int rowIndex1, int rowIndex2)
        {
            this.dataGridViewColumn = dataGridViewColumn;
            this.cellValue1 = cellValue1;
            this.cellValue2 = cellValue2;
            this.rowIndex1 = rowIndex1;
            this.rowIndex2 = rowIndex2;
        }

        public object CellValue1
        {
            get
            {
                return this.cellValue1;
            }
        }

        public object CellValue2
        {
            get
            {
                return this.cellValue2;
            }
        }

        public DataGridViewColumn Column
        {
            get
            {
                return this.dataGridViewColumn;
            }
        }

        public int RowIndex1
        {
            get
            {
                return this.rowIndex1;
            }
        }

        public int RowIndex2
        {
            get
            {
                return this.rowIndex2;
            }
        }

        public int SortResult
        {
            get
            {
                return this.sortResult;
            }
            set
            {
                this.sortResult = value;
            }
        }
    }
}

