namespace System.Windows.Forms
{
    using System;

    public interface IDataGridViewEditingCell
    {
        object GetEditingCellFormattedValue(DataGridViewDataErrorContexts context);
        void PrepareEditingCellForEdit(bool selectAll);

        object EditingCellFormattedValue { get; set; }

        bool EditingCellValueChanged { get; set; }
    }
}

