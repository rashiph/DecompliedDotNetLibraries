namespace System.Data
{
    using System;

    public class DataColumnChangeEventArgs : EventArgs
    {
        private DataColumn _column;
        private object _proposedValue;
        private readonly DataRow _row;

        internal DataColumnChangeEventArgs(DataRow row)
        {
            this._row = row;
        }

        public DataColumnChangeEventArgs(DataRow row, DataColumn column, object value)
        {
            this._row = row;
            this._column = column;
            this._proposedValue = value;
        }

        internal void InitializeColumnChangeEvent(DataColumn column, object value)
        {
            this._column = column;
            this._proposedValue = value;
        }

        public DataColumn Column
        {
            get
            {
                return this._column;
            }
        }

        public object ProposedValue
        {
            get
            {
                return this._proposedValue;
            }
            set
            {
                this._proposedValue = value;
            }
        }

        public DataRow Row
        {
            get
            {
                return this._row;
            }
        }
    }
}

