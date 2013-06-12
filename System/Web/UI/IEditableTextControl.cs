namespace System.Web.UI
{
    using System;

    public interface IEditableTextControl : ITextControl
    {
        event EventHandler TextChanged;
    }
}

