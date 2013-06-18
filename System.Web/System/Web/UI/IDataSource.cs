namespace System.Web.UI
{
    using System;
    using System.Collections;

    public interface IDataSource
    {
        event EventHandler DataSourceChanged;

        DataSourceView GetView(string viewName);
        ICollection GetViewNames();
    }
}

