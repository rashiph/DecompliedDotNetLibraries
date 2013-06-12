namespace System.Web.UI
{
    using System;

    public interface IHierarchicalDataSource
    {
        event EventHandler DataSourceChanged;

        HierarchicalDataSourceView GetHierarchicalView(string viewPath);
    }
}

