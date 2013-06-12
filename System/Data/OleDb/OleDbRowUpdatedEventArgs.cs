namespace System.Data.OleDb
{
    using System;
    using System.Data;
    using System.Data.Common;

    public sealed class OleDbRowUpdatedEventArgs : RowUpdatedEventArgs
    {
        public OleDbRowUpdatedEventArgs(DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping) : base(dataRow, command, statementType, tableMapping)
        {
        }

        public OleDbCommand Command
        {
            get
            {
                return (OleDbCommand) base.Command;
            }
        }
    }
}

