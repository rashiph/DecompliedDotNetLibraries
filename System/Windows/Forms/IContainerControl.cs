namespace System.Windows.Forms
{
    using System;

    public interface IContainerControl
    {
        bool ActivateControl(Control active);

        Control ActiveControl { get; set; }
    }
}

