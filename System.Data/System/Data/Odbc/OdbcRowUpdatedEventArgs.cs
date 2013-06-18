namespace System.Data.Odbc
{
    using System;
    using System.Data;
    using System.Data.Common;

    public sealed class OdbcRowUpdatedEventArgs : RowUpdatedEventArgs
    {
        public OdbcRowUpdatedEventArgs(DataRow row, IDbCommand command, StatementType statementType, DataTableMapping tableMapping) : base(row, command, statementType, tableMapping)
        {
        }

        public OdbcCommand Command
        {
            get
            {
                return (OdbcCommand) base.Command;
            }
        }
    }
}

