namespace System.Data.SqlClient
{
    using System;

    public class SqlRowsCopiedEventArgs : EventArgs
    {
        private bool _abort;
        private long _rowsCopied;

        public SqlRowsCopiedEventArgs(long rowsCopied)
        {
            this._rowsCopied = rowsCopied;
        }

        public bool Abort
        {
            get
            {
                return this._abort;
            }
            set
            {
                this._abort = value;
            }
        }

        public long RowsCopied
        {
            get
            {
                return this._rowsCopied;
            }
        }
    }
}

