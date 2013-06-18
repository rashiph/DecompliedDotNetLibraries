namespace System.Data.OracleClient
{
    using System;
    using System.Data;
    using System.Data.Common;

    public sealed class OracleRowUpdatingEventArgs : RowUpdatingEventArgs
    {
        public OracleRowUpdatingEventArgs(DataRow row, IDbCommand command, StatementType statementType, DataTableMapping tableMapping) : base(row, command, statementType, tableMapping)
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
                base.BaseCommand = value as OracleCommand;
            }
        }

        public OracleCommand Command
        {
            get
            {
                return (base.Command as OracleCommand);
            }
            set
            {
                base.Command = value;
            }
        }
    }
}

