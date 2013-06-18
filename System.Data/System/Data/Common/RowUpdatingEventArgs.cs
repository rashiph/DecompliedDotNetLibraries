namespace System.Data.Common
{
    using System;
    using System.Data;

    public class RowUpdatingEventArgs : EventArgs
    {
        private IDbCommand _command;
        private DataRow _dataRow;
        private Exception _errors;
        private System.Data.StatementType _statementType;
        private UpdateStatus _status;
        private DataTableMapping _tableMapping;

        public RowUpdatingEventArgs(DataRow dataRow, IDbCommand command, System.Data.StatementType statementType, DataTableMapping tableMapping)
        {
            ADP.CheckArgumentNull(dataRow, "dataRow");
            ADP.CheckArgumentNull(tableMapping, "tableMapping");
            switch (statementType)
            {
                case System.Data.StatementType.Select:
                case System.Data.StatementType.Insert:
                case System.Data.StatementType.Update:
                case System.Data.StatementType.Delete:
                    this._dataRow = dataRow;
                    this._command = command;
                    this._statementType = statementType;
                    this._tableMapping = tableMapping;
                    return;

                case System.Data.StatementType.Batch:
                    throw ADP.NotSupportedStatementType(statementType, "RowUpdatingEventArgs");
            }
            throw ADP.InvalidStatementType(statementType);
        }

        protected virtual IDbCommand BaseCommand
        {
            get
            {
                return this._command;
            }
            set
            {
                this._command = value;
            }
        }

        public IDbCommand Command
        {
            get
            {
                return this.BaseCommand;
            }
            set
            {
                this.BaseCommand = value;
            }
        }

        public Exception Errors
        {
            get
            {
                return this._errors;
            }
            set
            {
                this._errors = value;
            }
        }

        public DataRow Row
        {
            get
            {
                return this._dataRow;
            }
        }

        public System.Data.StatementType StatementType
        {
            get
            {
                return this._statementType;
            }
        }

        public UpdateStatus Status
        {
            get
            {
                return this._status;
            }
            set
            {
                switch (value)
                {
                    case UpdateStatus.Continue:
                    case UpdateStatus.ErrorsOccurred:
                    case UpdateStatus.SkipCurrentRow:
                    case UpdateStatus.SkipAllRemainingRows:
                        this._status = value;
                        return;
                }
                throw ADP.InvalidUpdateStatus(value);
            }
        }

        public DataTableMapping TableMapping
        {
            get
            {
                return this._tableMapping;
            }
        }
    }
}

