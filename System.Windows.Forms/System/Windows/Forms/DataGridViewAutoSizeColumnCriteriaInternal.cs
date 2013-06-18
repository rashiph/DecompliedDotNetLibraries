namespace System.Windows.Forms
{
    using System;

    [Flags]
    internal enum DataGridViewAutoSizeColumnCriteriaInternal
    {
        AllRows = 4,
        DisplayedRows = 8,
        Fill = 0x10,
        Header = 2,
        None = 1,
        NotSet = 0
    }
}

