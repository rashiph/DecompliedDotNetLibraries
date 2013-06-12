namespace System.Data.OleDb
{
    using System;
    using System.Data;
    using System.Data.Common;

    public sealed class OleDbRowUpdatingEventArgs : RowUpdatingEventArgs
    {
        public OleDbRowUpdatingEventArgs(DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping) : base(dataRow, command, statementType, tableMapping)
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
                base.BaseCommand = value as OleDbCommand;
            }
        }

        public OleDbCommand Command
        {
            get
            {
                return (base.Command as OleDbCommand);
            }
            set
            {
                base.Command = value;
            }
        }
    }
}

