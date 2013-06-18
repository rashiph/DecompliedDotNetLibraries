namespace System.Windows.Forms
{
    using System;

    public class ColumnWidthChangedEventArgs : EventArgs
    {
        private readonly int columnIndex;

        public ColumnWidthChangedEventArgs(int columnIndex)
        {
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

