namespace System.Windows.Forms
{
    using System;

    public interface IDataGridColumnStyleEditingNotificationService
    {
        void ColumnStartedEditing(Control editingControl);
    }
}

