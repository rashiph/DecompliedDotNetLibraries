namespace System.Web.UI
{
    using System;

    [Flags]
    public enum DataSourceCapabilities
    {
        None = 0,
        Page = 2,
        RetrieveTotalRowCount = 4,
        Sort = 1
    }
}

