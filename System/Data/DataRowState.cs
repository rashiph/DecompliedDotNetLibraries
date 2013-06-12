namespace System.Data
{
    using System;

    [Flags]
    public enum DataRowState
    {
        Added = 4,
        Deleted = 8,
        Detached = 1,
        Modified = 0x10,
        Unchanged = 2
    }
}

