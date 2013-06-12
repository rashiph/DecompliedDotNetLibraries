namespace System.Windows.Forms
{
    using System;

    public class ColumnClickEventArgs : EventArgs
    {
        private readonly int column;

        public ColumnClickEventArgs(int column)
        {
            this.column = column;
        }

        public int Column
        {
            get
            {
                return this.column;
            }
        }
    }
}

