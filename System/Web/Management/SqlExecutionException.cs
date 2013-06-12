namespace System.Web.Management
{
    using System;
    using System.Data.SqlClient;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    public sealed class SqlExecutionException : SystemException
    {
        private string _commands;
        private string _database;
        private string _server;
        private SqlException _sqlException;
        private string _sqlFile;

        public SqlExecutionException()
        {
        }

        public SqlExecutionException(string message) : base(message)
        {
        }

        private SqlExecutionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this._server = info.GetString("_server");
            this._database = info.GetString("_database");
            this._sqlFile = info.GetString("_sqlFile");
            this._commands = info.GetString("_commands");
            this._sqlException = (SqlException) info.GetValue("_sqlException", typeof(SqlException));
        }

        public SqlExecutionException(string message, System.Exception innerException) : base(message, innerException)
        {
        }

        public SqlExecutionException(string message, string server, string database, string sqlFile, string commands, SqlException sqlException) : base(message)
        {
            this._server = server;
            this._database = database;
            this._sqlFile = sqlFile;
            this._commands = commands;
            this._sqlException = sqlException;
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("_server", this._server);
            info.AddValue("_database", this._database);
            info.AddValue("_sqlFile", this._sqlFile);
            info.AddValue("_commands", this._commands);
            info.AddValue("_sqlException", this._sqlException);
        }

        public string Commands
        {
            get
            {
                return this._commands;
            }
        }

        public string Database
        {
            get
            {
                return this._database;
            }
        }

        public SqlException Exception
        {
            get
            {
                return this._sqlException;
            }
        }

        public string Server
        {
            get
            {
                return this._server;
            }
        }

        public string SqlFile
        {
            get
            {
                return this._sqlFile;
            }
        }
    }
}

