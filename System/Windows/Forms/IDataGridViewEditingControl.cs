namespace System.Windows.Forms
{
    using System;

    public interface IDataGridViewEditingControl
    {
        void ApplyCellStyleToEditingControl(DataGridViewCellStyle dataGridViewCellStyle);
        bool EditingControlWantsInputKey(Keys keyData, bool dataGridViewWantsInputKey);
        object GetEditingControlFormattedValue(DataGridViewDataErrorContexts context);
        void PrepareEditingControlForEdit(bool selectAll);

        DataGridView EditingControlDataGridView { get; set; }

        object EditingControlFormattedValue { get; set; }

        int EditingControlRowIndex { get; set; }

        bool EditingControlValueChanged { get; set; }

        Cursor EditingPanelCursor { get; }

        bool RepositionEditingControlOnValueChange { get; }
    }
}

