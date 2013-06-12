namespace System.Web.UI
{
    using System;

    public interface ICheckBoxControl
    {
        event EventHandler CheckedChanged;

        bool Checked { get; set; }
    }
}

