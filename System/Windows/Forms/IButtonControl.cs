namespace System.Windows.Forms
{
    using System;

    public interface IButtonControl
    {
        void NotifyDefault(bool value);
        void PerformClick();

        System.Windows.Forms.DialogResult DialogResult { get; set; }
    }
}

