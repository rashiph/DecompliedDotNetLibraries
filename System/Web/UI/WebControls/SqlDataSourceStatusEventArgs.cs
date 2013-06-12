namespace System.Web.UI.WebControls
{
    using System;
    using System.Data.Common;

    public class SqlDataSourceStatusEventArgs : EventArgs
    {
        private int _affectedRows;
        private DbCommand _command;
        private System.Exception _exception;
        private bool _exceptionHandled;

        public SqlDataSourceStatusEventArgs(DbCommand command, int affectedRows, System.Exception exception)
        {
            this._command = command;
            this._affectedRows = affectedRows;
            this._exception = exception;
        }

        public int AffectedRows
        {
            get
            {
                return this._affectedRows;
            }
        }

        public DbCommand Command
        {
            get
            {
                return this._command;
            }
        }

        public System.Exception Exception
        {
            get
            {
                return this._exception;
            }
        }

        public bool ExceptionHandled
        {
            get
            {
                return this._exceptionHandled;
            }
            set
            {
                this._exceptionHandled = value;
            }
        }
    }
}

