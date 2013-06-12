namespace System.Web.UI
{
    using System;

    public interface IDataItemContainer : INamingContainer
    {
        object DataItem { get; }

        int DataItemIndex { get; }

        int DisplayIndex { get; }
    }
}

