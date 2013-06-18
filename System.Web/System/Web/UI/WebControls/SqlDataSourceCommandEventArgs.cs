namespace System.Web.UI.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Data.Common;

    public class SqlDataSourceCommandEventArgs : CancelEventArgs
    {
        private DbCommand _command;

        public SqlDataSourceCommandEventArgs(DbCommand command)
        {
            this._command = command;
        }

        public DbCommand Command
        {
            get
            {
                return this._command;
            }
        }
    }
}

