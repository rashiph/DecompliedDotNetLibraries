namespace System.Data.Odbc
{
    using System;
    using System.Data;
    using System.Data.Common;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.Text;

    [Serializable]
    public sealed class OdbcException : DbException
    {
        private ODBC32.RETCODE _retcode;
        private OdbcErrorCollection odbcErrors;

        private OdbcException(SerializationInfo si, StreamingContext sc) : base(si, sc)
        {
            this.odbcErrors = new OdbcErrorCollection();
            this._retcode = (ODBC32.RETCODE) si.GetValue("odbcRetcode", typeof(ODBC32.RETCODE));
            this.odbcErrors = (OdbcErrorCollection) si.GetValue("odbcErrors", typeof(OdbcErrorCollection));
            base.HResult = -2146232009;
        }

        internal OdbcException(string message, OdbcErrorCollection errors) : base(message)
        {
            this.odbcErrors = new OdbcErrorCollection();
            this.odbcErrors = errors;
            base.HResult = -2146232009;
        }

        internal static OdbcException CreateException(OdbcErrorCollection errors, ODBC32.RetCode retcode)
        {
            StringBuilder builder = new StringBuilder();
            foreach (OdbcError error in errors)
            {
                if (builder.Length > 0)
                {
                    builder.Append(Environment.NewLine);
                }
                builder.Append(Res.GetString("Odbc_ExceptionMessage", new object[] { ODBC32.RetcodeToString(retcode), error.SQLState, error.Message }));
            }
            return new OdbcException(builder.ToString(), errors);
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.SerializationFormatter)]
        public override void GetObjectData(SerializationInfo si, StreamingContext context)
        {
            if (si == null)
            {
                throw new ArgumentNullException("si");
            }
            si.AddValue("odbcRetcode", this._retcode, typeof(ODBC32.RETCODE));
            si.AddValue("odbcErrors", this.odbcErrors, typeof(OdbcErrorCollection));
            base.GetObjectData(si, context);
        }

        public OdbcErrorCollection Errors
        {
            get
            {
                return this.odbcErrors;
            }
        }

        public override string Source
        {
            get
            {
                if (0 < this.Errors.Count)
                {
                    string source = this.Errors[0].Source;
                    if (!ADP.IsEmpty(source))
                    {
                        return source;
                    }
                }
                return "";
            }
        }
    }
}

