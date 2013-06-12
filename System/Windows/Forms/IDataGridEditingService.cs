namespace System.Windows.Forms
{
    using System;

    public interface IDataGridEditingService
    {
        bool BeginEdit(DataGridColumnStyle gridColumn, int rowNumber);
        bool EndEdit(DataGridColumnStyle gridColumn, int rowNumber, bool shouldAbort);
    }
}

