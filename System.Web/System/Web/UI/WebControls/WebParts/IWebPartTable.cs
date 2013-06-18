namespace System.Web.UI.WebControls.WebParts
{
    using System;
    using System.ComponentModel;

    public interface IWebPartTable
    {
        void GetTableData(TableCallback callback);

        PropertyDescriptorCollection Schema { get; }
    }
}

