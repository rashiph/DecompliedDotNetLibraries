namespace System.Web.UI.WebControls
{
    using System;
    using System.Collections;
    using System.Web.UI;

    internal sealed class ReadOnlyDataSource : IDataSource
    {
        private string _dataMember;
        private object _dataSource;
        private static string[] ViewNames = new string[0];

        event EventHandler IDataSource.DataSourceChanged
        {
            add
            {
            }
            remove
            {
            }
        }

        public ReadOnlyDataSource(object dataSource, string dataMember)
        {
            this._dataSource = dataSource;
            this._dataMember = dataMember;
        }

        DataSourceView IDataSource.GetView(string viewName)
        {
            IDataSource source = this._dataSource as IDataSource;
            if (source != null)
            {
                return source.GetView(viewName);
            }
            return new ReadOnlyDataSourceView(this, this._dataMember, DataSourceHelper.GetResolvedDataSource(this._dataSource, this._dataMember));
        }

        ICollection IDataSource.GetViewNames()
        {
            return ViewNames;
        }
    }
}

