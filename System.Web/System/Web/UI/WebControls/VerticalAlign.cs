namespace System.Web.UI.WebControls
{
    using System;
    using System.ComponentModel;

    [TypeConverter(typeof(VerticalAlignConverter))]
    public enum VerticalAlign
    {
        NotSet,
        Top,
        Middle,
        Bottom
    }
}

