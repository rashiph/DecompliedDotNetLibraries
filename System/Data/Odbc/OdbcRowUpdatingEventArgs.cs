namespace System.Data.Odbc
{
    using System;
    using System.Data;
    using System.Data.Common;

    public sealed class OdbcRowUpdatingEventArgs : RowUpdatingEventArgs
    {
        public OdbcRowUpdatingEventArgs(DataRow row, IDbCommand command, StatementType statementType, DataTableMapping tableMapping) : base(row, command, statementType, tableMapping)
        {
        }

        protected override IDbCommand BaseCommand
        {
            get
            {
                return base.BaseCommand;
            }
            set
            {
                base.BaseCommand = value as OdbcCommand;
            }
        }

        public OdbcCommand Command
        {
            get
            {
                return (base.Command as OdbcCommand);
            }
            set
            {
                base.Command = value;
            }
        }
    }
}

