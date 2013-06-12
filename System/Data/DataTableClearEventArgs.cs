namespace System.Data
{
    using System;

    public sealed class DataTableClearEventArgs : EventArgs
    {
        private readonly DataTable dataTable;

        public DataTableClearEventArgs(DataTable dataTable)
        {
            this.dataTable = dataTable;
        }

        public DataTable Table
        {
            get
            {
                return this.dataTable;
            }
        }

        public string TableName
        {
            get
            {
                return this.dataTable.TableName;
            }
        }

        public string TableNamespace
        {
            get
            {
                return this.dataTable.Namespace;
            }
        }
    }
}

