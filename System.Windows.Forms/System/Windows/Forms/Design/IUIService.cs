namespace System.Windows.Forms.Design
{
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    [Guid("06A9C74B-5E32-4561-BE73-381B37869F4F")]
    public interface IUIService
    {
        IDictionary Styles { get; }
        bool CanShowComponentEditor(object component);
        IWin32Window GetDialogOwnerWindow();
        void SetUIDirty();
        bool ShowComponentEditor(object component, IWin32Window parent);
        DialogResult ShowDialog(Form form);
        void ShowError(string message);
        void ShowError(Exception ex);
        void ShowError(Exception ex, string message);
        void ShowMessage(string message);
        void ShowMessage(string message, string caption);
        DialogResult ShowMessage(string message, string caption, MessageBoxButtons buttons);
        bool ShowToolWindow(Guid toolWindow);
    }
}

