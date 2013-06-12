namespace System.Data.Odbc
{
    using System;
    using System.Text;

    public sealed class OdbcInfoMessageEventArgs : EventArgs
    {
        private OdbcErrorCollection _errors;

        internal OdbcInfoMessageEventArgs(OdbcErrorCollection errors)
        {
            this._errors = errors;
        }

        public override string ToString()
        {
            return this.Message;
        }

        public OdbcErrorCollection Errors
        {
            get
            {
                return this._errors;
            }
        }

        public string Message
        {
            get
            {
                StringBuilder builder = new StringBuilder();
                foreach (OdbcError error in this.Errors)
                {
                    if (0 < builder.Length)
                    {
                        builder.Append(Environment.NewLine);
                    }
                    builder.Append(error.Message);
                }
                return builder.ToString();
            }
        }
    }
}

