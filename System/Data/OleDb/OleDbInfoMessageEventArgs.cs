namespace System.Data.OleDb
{
    using System;

    public sealed class OleDbInfoMessageEventArgs : EventArgs
    {
        private readonly OleDbException exception;

        internal OleDbInfoMessageEventArgs(OleDbException exception)
        {
            this.exception = exception;
        }

        internal bool ShouldSerializeErrors()
        {
            return this.exception.ShouldSerializeErrors();
        }

        public override string ToString()
        {
            return this.Message;
        }

        public int ErrorCode
        {
            get
            {
                return this.exception.ErrorCode;
            }
        }

        public OleDbErrorCollection Errors
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

