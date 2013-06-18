namespace System.Web.UI.Design
{
    using System;
    using System.Collections;

    public interface IDataSourceProvider
    {
        IEnumerable GetResolvedSelectedDataSource();
        object GetSelectedDataSource();
    }
}

