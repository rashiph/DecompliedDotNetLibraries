namespace System.Web.UI.WebControls.WebParts
{
    using System;
    using System.ComponentModel;

    public interface IWebPartRow
    {
        void GetRowData(RowCallback callback);

        PropertyDescriptorCollection Schema { get; }
    }
}

