namespace System.Web.DataAccess
{
    using System;
    using System.Data.SqlClient;
    using System.Web;

    internal sealed class SqlConnectionHolder
    {
        internal SqlConnection _Connection;
        private bool _Opened;

        internal SqlConnectionHolder(string connectionString)
        {
            try
            {
                this._Connection = new SqlConnection(connectionString);
            }
            catch (ArgumentException exception)
            {
                throw new ArgumentException(System.Web.SR.GetString("SqlError_Connection_String"), "connectionString", exception);
            }
        }

        internal void Close()
        {
            if (this._Opened)
            {
                this.Connection.Close();
                this._Opened = false;
            }
        }

        internal void Open(HttpContext context, bool revertImpersonate)
        {
            if (this._Opened)
            {
                return;
            }
            if (revertImpersonate)
            {
                using (new ApplicationImpersonationContext())
                {
                    this.Connection.Open();
                    goto Label_0034;
                }
            }
            this.Connection.Open();
        Label_0034:
            this._Opened = true;
        }

        internal SqlConnection Connection
        {
            get
            {
                return this._Connection;
            }
        }
    }
}

