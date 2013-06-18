namespace System.Windows.Forms.Design
{
    using System;
    using System.Windows.Forms;

    public interface IWindowsFormsEditorService
    {
        void CloseDropDown();
        void DropDownControl(Control control);
        DialogResult ShowDialog(Form dialog);
    }
}

