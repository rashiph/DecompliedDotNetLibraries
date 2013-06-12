namespace System.Web.UI.WebControls
{
    using System;
    using System.Web;
    using System.Web.UI;

    internal sealed class ReadOnlyHierarchicalDataSource : IHierarchicalDataSource
    {
        private object _dataSource;

        event EventHandler IHierarchicalDataSource.DataSourceChanged
        {
            add
            {
            }
            remove
            {
            }
        }

        public ReadOnlyHierarchicalDataSource(object dataSource)
        {
            this._dataSource = dataSource;
        }

        HierarchicalDataSourceView IHierarchicalDataSource.GetHierarchicalView(string viewPath)
        {
            IHierarchicalDataSource source = this._dataSource as IHierarchicalDataSource;
            if (source != null)
            {
                return source.GetHierarchicalView(viewPath);
            }
            IHierarchicalEnumerable dataSource = this._dataSource as IHierarchicalEnumerable;
            if (((dataSource != null) && (viewPath != null)) && (viewPath.Length != 0))
            {
                throw new InvalidOperationException(System.Web.SR.GetString("ReadOnlyHierarchicalDataSourceView_CantAccessPathInEnumerable"));
            }
            return new ReadOnlyHierarchicalDataSourceView(dataSource);
        }
    }
}

