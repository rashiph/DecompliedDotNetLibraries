namespace System.Web.UI.WebControls.WebParts
{
    using System;
    using System.ComponentModel;

    public interface IWebPartParameters
    {
        void GetParametersData(ParametersCallback callback);
        void SetConsumerSchema(PropertyDescriptorCollection schema);

        PropertyDescriptorCollection Schema { get; }
    }
}

