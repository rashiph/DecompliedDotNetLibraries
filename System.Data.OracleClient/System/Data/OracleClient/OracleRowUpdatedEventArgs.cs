namespace System.Data.OracleClient
{
    using System;
    using System.Data;
    using System.Data.Common;

    public sealed class OracleRowUpdatedEventArgs : RowUpdatedEventArgs
    {
        public OracleRowUpdatedEventArgs(DataRow row, IDbCommand command, StatementType statementType, DataTableMapping tableMapping) : base(row, command, statementType, tableMapping)
        {
        }

        public OracleCommand Command
        {
            get
            {
                return (OracleCommand) base.Command;
            }
        }
    }
}

