namespace System.Data.OracleClient
{
    using System;

    public sealed class OracleInfoMessageEventArgs : EventArgs
    {
        private OracleException exception;

        internal OracleInfoMessageEventArgs(OracleException exception)
        {
            this.exception = exception;
        }

        public override string ToString()
        {
            return this.Message;
        }

        public int Code
        {
            get
            {
                return this.exception.Code;
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

