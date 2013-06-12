namespace System.Data
{
    using System;

    [Flags]
    public enum DataRowAction
    {
        Add = 0x10,
        Change = 2,
        ChangeCurrentAndOriginal = 0x40,
        ChangeOriginal = 0x20,
        Commit = 8,
        Delete = 1,
        Nothing = 0,
        Rollback = 4
    }
}

