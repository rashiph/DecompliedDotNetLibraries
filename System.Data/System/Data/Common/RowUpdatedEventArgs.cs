namespace System.Data.Common
{
    using System;
    using System.Data;

    public class RowUpdatedEventArgs : EventArgs
    {
        private IDbCommand _command;
        private DataRow _dataRow;
        private DataRow[] _dataRows;
        private Exception _errors;
        private int _recordsAffected;
        private System.Data.StatementType _statementType;
        private UpdateStatus _status;
        private DataTableMapping _tableMapping;

        public RowUpdatedEventArgs(DataRow dataRow, IDbCommand command, System.Data.StatementType statementType, DataTableMapping tableMapping)
        {
            switch (statementType)
            {
                case System.Data.StatementType.Select:
                case System.Data.StatementType.Insert:
                case System.Data.StatementType.Update:
                case System.Data.StatementType.Delete:
                case System.Data.StatementType.Batch:
                    this._dataRow = dataRow;
                    this._command = command;
                    this._statementType = statementType;
                    this._tableMapping = tableMapping;
                    return;
            }
            throw ADP.InvalidStatementType(statementType);
        }

        internal void AdapterInit(DataRow[] dataRows)
        {
            this._statementType = System.Data.StatementType.Batch;
            this._dataRows = dataRows;
            if ((dataRows != null) && (1 == dataRows.Length))
            {
                this._dataRow = dataRows[0];
            }
        }

        internal void AdapterInit(int recordsAffected)
        {
            this._recordsAffected = recordsAffected;
        }

        public void CopyToRows(DataRow[] array)
        {
            this.CopyToRows(array, 0);
        }

        public void CopyToRows(DataRow[] array, int arrayIndex)
        {
            DataRow[] rowArray = this._dataRows;
            if (rowArray != null)
            {
                rowArray.CopyTo(array, arrayIndex);
            }
            else
            {
                if (array == null)
                {
                    throw ADP.ArgumentNull("array");
                }
                array[arrayIndex] = this.Row;
            }
        }

        public IDbCommand Command
        {
            get
            {
                return this._command;
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

        public int RecordsAffected
        {
            get
            {
                return this._recordsAffected;
            }
        }

        public DataRow Row
        {
            get
            {
                return this._dataRow;
            }
        }

        public int RowCount
        {
            get
            {
                DataRow[] rowArray = this._dataRows;
                if (rowArray != null)
                {
                    return rowArray.Length;
                }
                if (this._dataRow == null)
                {
                    return 0;
                }
                return 1;
            }
        }

        internal DataRow[] Rows
        {
            get
            {
                return this._dataRows;
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

