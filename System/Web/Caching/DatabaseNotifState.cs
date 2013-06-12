namespace System.Web.Caching
{
    using System;
    using System.Collections;
    using System.Data.SqlClient;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Web.DataAccess;

    internal class DatabaseNotifState : IDisposable
    {
        internal string _connectionString;
        internal string _database;
        internal bool _init;
        internal bool _notifEnabled;
        internal Exception _pollExpt;
        internal int _pollSqlError;
        internal bool _poolConn;
        internal int _refCount;
        internal int _rqInCallback;
        internal SqlCommand _sqlCmd;
        internal SqlConnection _sqlConn;
        internal Hashtable _tables;
        internal Timer _timer;
        internal DateTime _utcTablesUpdated;

        internal DatabaseNotifState(string database, string connection, int polltime)
        {
            this._database = database;
            this._connectionString = connection;
            this._timer = null;
            this._tables = new Hashtable();
            this._pollExpt = null;
            this._utcTablesUpdated = DateTime.MinValue;
            if (polltime <= 0x1388)
            {
                this._poolConn = true;
            }
        }

        public void Dispose()
        {
            if (this._sqlConn != null)
            {
                this._sqlConn.Close();
                this._sqlConn = null;
            }
            if (this._timer != null)
            {
                this._timer.Dispose();
                this._timer = null;
            }
        }

        internal void GetConnection(out SqlConnection sqlConn, out SqlCommand sqlCmd)
        {
            sqlConn = null;
            sqlCmd = null;
            if (this._sqlConn != null)
            {
                sqlConn = this._sqlConn;
                sqlCmd = this._sqlCmd;
                this._sqlConn = null;
                this._sqlCmd = null;
            }
            else
            {
                SqlConnectionHolder connection = null;
                try
                {
                    connection = SqlConnectionHelper.GetConnection(this._connectionString, true);
                    sqlCmd = new SqlCommand("dbo.AspNet_SqlCachePollingStoredProcedure", connection.Connection);
                    sqlConn = connection.Connection;
                }
                catch
                {
                    if (connection != null)
                    {
                        connection.Close();
                        connection = null;
                    }
                    sqlCmd = null;
                    throw;
                }
            }
        }

        internal void ReleaseConnection(ref SqlConnection sqlConn, ref SqlCommand sqlCmd, bool error)
        {
            if (sqlConn != null)
            {
                if (this._poolConn && !error)
                {
                    this._sqlConn = sqlConn;
                    this._sqlCmd = sqlCmd;
                }
                else
                {
                    sqlConn.Close();
                }
                sqlConn = null;
                sqlCmd = null;
            }
        }
    }
}

