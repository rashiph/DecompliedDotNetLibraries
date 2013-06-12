namespace System.Web.UI.WebControls
{
    using System;
    using System.Data.Common;
    using System.Web.UI;

    public class SqlDataSourceSelectingEventArgs : SqlDataSourceCommandEventArgs
    {
        private DataSourceSelectArguments _arguments;

        public SqlDataSourceSelectingEventArgs(DbCommand command, DataSourceSelectArguments arguments) : base(command)
        {
            this._arguments = arguments;
        }

        public DataSourceSelectArguments Arguments
        {
            get
            {
                return this._arguments;
            }
        }
    }
}

