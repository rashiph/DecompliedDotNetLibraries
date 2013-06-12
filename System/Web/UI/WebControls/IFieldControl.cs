namespace System.Web.UI.WebControls
{
    using System;
    using System.Web.UI;

    public interface IFieldControl
    {
        IAutoFieldGenerator FieldsGenerator { get; set; }
    }
}

