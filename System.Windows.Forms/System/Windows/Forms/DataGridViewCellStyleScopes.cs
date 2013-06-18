namespace System.Windows.Forms
{
    using System;

    [Flags]
    public enum DataGridViewCellStyleScopes
    {
        AlternatingRows = 0x80,
        Cell = 1,
        Column = 2,
        ColumnHeaders = 0x10,
        DataGridView = 8,
        None = 0,
        Row = 4,
        RowHeaders = 0x20,
        Rows = 0x40
    }
}

