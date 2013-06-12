namespace System.Data
{
    using System;

    public enum DataRowVersion
    {
        Current = 0x200,
        Default = 0x600,
        Original = 0x100,
        Proposed = 0x400
    }
}

