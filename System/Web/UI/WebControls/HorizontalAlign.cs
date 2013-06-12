namespace System.Web.UI.WebControls
{
    using System;
    using System.ComponentModel;

    [TypeConverter(typeof(HorizontalAlignConverter))]
    public enum HorizontalAlign
    {
        NotSet,
        Left,
        Center,
        Right,
        Justify
    }
}

