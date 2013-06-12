namespace System.Windows.Forms
{
    using System;
    using System.Globalization;

    public class DataGridViewRowsRemovedEventArgs : EventArgs
    {
        private int rowCount;
        private int rowIndex;

        public DataGridViewRowsRemovedEventArgs(int rowIndex, int rowCount)
        {
            if (rowIndex < 0)
            {
                object[] args = new object[] { "rowIndex", rowIndex.ToString(CultureInfo.CurrentCulture), 0.ToString(CultureInfo.CurrentCulture) };
                throw new ArgumentOutOfRangeException("rowIndex", System.Windows.Forms.SR.GetString("InvalidLowBoundArgumentEx", args));
            }
            if (rowCount < 1)
            {
                object[] objArray2 = new object[] { "rowCount", rowCount.ToString(CultureInfo.CurrentCulture), 1.ToString(CultureInfo.CurrentCulture) };
                throw new ArgumentOutOfRangeException("rowCount", System.Windows.Forms.SR.GetString("InvalidLowBoundArgumentEx", objArray2));
            }
            this.rowIndex = rowIndex;
            this.rowCount = rowCount;
        }

        public int RowCount
        {
            get
            {
                return this.rowCount;
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

