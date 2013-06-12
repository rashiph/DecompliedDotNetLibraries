namespace System.Data.SqlClient
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Data.Common;
    using System.Globalization;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.Text;

    [Serializable]
    public sealed class SqlException : DbException
    {
        internal bool _doNotReconnect;
        private SqlErrorCollection _errors;

        private SqlException(SerializationInfo si, StreamingContext sc) : base(si, sc)
        {
            this._errors = (SqlErrorCollection) si.GetValue("Errors", typeof(SqlErrorCollection));
            base.HResult = -2146232060;
        }

        private SqlException(string message, SqlErrorCollection errorCollection) : base(message)
        {
            base.HResult = -2146232060;
            this._errors = errorCollection;
        }

        internal static SqlException CreateException(SqlErrorCollection errorCollection, string serverVersion)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < errorCollection.Count; i++)
            {
                if (i > 0)
                {
                    builder.Append(Environment.NewLine);
                }
                builder.Append(errorCollection[i].Message);
            }
            SqlException exception = new SqlException(builder.ToString(), errorCollection);
            exception.Data.Add("HelpLink.ProdName", "Microsoft SQL Server");
            if (!ADP.IsEmpty(serverVersion))
            {
                exception.Data.Add("HelpLink.ProdVer", serverVersion);
            }
            exception.Data.Add("HelpLink.EvtSrc", "MSSQLServer");
            exception.Data.Add("HelpLink.EvtID", errorCollection[0].Number.ToString(CultureInfo.InvariantCulture));
            exception.Data.Add("HelpLink.BaseHelpUrl", "http://go.microsoft.com/fwlink");
            exception.Data.Add("HelpLink.LinkId", "20476");
            return exception;
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.SerializationFormatter)]
        public override void GetObjectData(SerializationInfo si, StreamingContext context)
        {
            if (si == null)
            {
                throw new ArgumentNullException("si");
            }
            si.AddValue("Errors", this._errors, typeof(SqlErrorCollection));
            base.GetObjectData(si, context);
        }

        internal SqlException InternalClone()
        {
            SqlException exception = new SqlException(this.Message, this._errors);
            if (this.Data != null)
            {
                foreach (DictionaryEntry entry in this.Data)
                {
                    exception.Data.Add(entry.Key, entry.Value);
                }
            }
            exception._doNotReconnect = this._doNotReconnect;
            return exception;
        }

        private bool ShouldSerializeErrors()
        {
            return ((this._errors != null) && (0 < this._errors.Count));
        }

        public byte Class
        {
            get
            {
                return this.Errors[0].Class;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public SqlErrorCollection Errors
        {
            get
            {
                if (this._errors == null)
                {
                    this._errors = new SqlErrorCollection();
                }
                return this._errors;
            }
        }

        public int LineNumber
        {
            get
            {
                return this.Errors[0].LineNumber;
            }
        }

        public int Number
        {
            get
            {
                return this.Errors[0].Number;
            }
        }

        public string Procedure
        {
            get
            {
                return this.Errors[0].Procedure;
            }
        }

        public string Server
        {
            get
            {
                return this.Errors[0].Server;
            }
        }

        public override string Source
        {
            get
            {
                return this.Errors[0].Source;
            }
        }

        public byte State
        {
            get
            {
                return this.Errors[0].State;
            }
        }
    }
}

