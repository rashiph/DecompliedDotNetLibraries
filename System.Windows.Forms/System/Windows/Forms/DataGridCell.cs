namespace System.Windows.Forms
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct DataGridCell
    {
        private int rowNumber;
        private int columnNumber;
        public int ColumnNumber
        {
            get
            {
                return this.columnNumber;
            }
            set
            {
                this.columnNumber = value;
            }
        }
        public int RowNumber
        {
            get
            {
                return this.rowNumber;
            }
            set
            {
                this.rowNumber = value;
            }
        }
        public DataGridCell(int r, int c)
        {
            this.rowNumber = r;
            this.columnNumber = c;
        }

        public override bool Equals(object o)
        {
            if (!(o is DataGridCell))
            {
                return false;
            }
            DataGridCell cell = (DataGridCell) o;
            return ((cell.RowNumber == this.RowNumber) && (cell.ColumnNumber == this.ColumnNumber));
        }

        public override int GetHashCode()
        {
            return (((~this.rowNumber * (this.columnNumber + 1)) & 0xffff00) >> 8);
        }

        public override string ToString()
        {
            return ("DataGridCell {RowNumber = " + this.RowNumber.ToString(CultureInfo.CurrentCulture) + ", ColumnNumber = " + this.ColumnNumber.ToString(CultureInfo.CurrentCulture) + "}");
        }
    }
}

