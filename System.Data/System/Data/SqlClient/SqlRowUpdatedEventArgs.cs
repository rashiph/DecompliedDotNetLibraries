namespace System.Data.SqlClient
{
    using System;
    using System.Data;
    using System.Data.Common;

    public sealed class SqlRowUpdatedEventArgs : RowUpdatedEventArgs
    {
        public SqlRowUpdatedEventArgs(DataRow row, IDbCommand command, StatementType statementType, DataTableMapping tableMapping) : base(row, command, statementType, tableMapping)
        {
        }

        public SqlCommand Command
        {
            get
            {
                return (SqlCommand) base.Command;
            }
        }
    }
}

