namespace System.Data.SqlClient
{
    using System;

    public sealed class SqlInfoMessageEventArgs : EventArgs
    {
        private SqlException exception;

        internal SqlInfoMessageEventArgs(SqlException exception)
        {
            this.exception = exception;
        }

        private bool ShouldSerializeErrors()
        {
            return ((this.exception != null) && (0 < this.exception.Errors.Count));
        }

        public override string ToString()
        {
            return this.Message;
        }

        public SqlErrorCollection Errors
        {
            get
            {
                return this.exception.Errors;
            }
        }

        public string Message
        {
            get
            {
                return this.exception.Message;
            }
        }

        public string Source
        {
            get
            {
                return this.exception.Source;
            }
        }
    }
}

