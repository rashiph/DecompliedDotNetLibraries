namespace System.Data
{
    using System;

    public class DataRowChangeEventArgs : EventArgs
    {
        private DataRowAction action;
        private DataRow row;

        public DataRowChangeEventArgs(DataRow row, DataRowAction action)
        {
            this.row = row;
            this.action = action;
        }

        public DataRowAction Action
        {
            get
            {
                return this.action;
            }
        }

        public DataRow Row
        {
            get
            {
                return this.row;
            }
        }
    }
}

