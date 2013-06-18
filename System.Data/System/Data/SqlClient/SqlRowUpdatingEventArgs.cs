namespace System.Data.SqlClient
{
    using System;
    using System.Data;
    using System.Data.Common;

    public sealed class SqlRowUpdatingEventArgs : RowUpdatingEventArgs
    {
        public SqlRowUpdatingEventArgs(DataRow row, IDbCommand command, StatementType statementType, DataTableMapping tableMapping) : base(row, command, statementType, tableMapping)
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
                base.BaseCommand = value as SqlCommand;
            }
        }

        public SqlCommand Command
        {
            get
            {
                return (base.Command as SqlCommand);
            }
            set
            {
                base.Command = value;
            }
        }
    }
}

