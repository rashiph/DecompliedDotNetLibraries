namespace System.Web.UI.WebControls.WebParts
{
    using System;
    using System.ComponentModel;

    public interface IWebPartField
    {
        void GetFieldValue(FieldCallback callback);

        PropertyDescriptor Schema { get; }
    }
}

