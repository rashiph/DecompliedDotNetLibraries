namespace System.Data
{
    using System;

    public sealed class DataTableNewRowEventArgs : EventArgs
    {
        private readonly DataRow dataRow;

        public DataTableNewRowEventArgs(DataRow dataRow)
        {
            this.dataRow = dataRow;
        }

        public DataRow Row
        {
            get
            {
                return this.dataRow;
            }
        }
    }
}

