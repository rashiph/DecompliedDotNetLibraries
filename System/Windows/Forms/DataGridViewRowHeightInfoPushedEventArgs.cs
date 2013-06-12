namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;

    public class DataGridViewRowHeightInfoPushedEventArgs : HandledEventArgs
    {
        private int height;
        private int minimumHeight;
        private int rowIndex;

        internal DataGridViewRowHeightInfoPushedEventArgs(int rowIndex, int height, int minimumHeight) : base(false)
        {
            this.rowIndex = rowIndex;
            this.height = height;
            this.minimumHeight = minimumHeight;
        }

        public int Height
        {
            get
            {
                return this.height;
            }
        }

        public int MinimumHeight
        {
            get
            {
                return this.minimumHeight;
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

