namespace System.Data
{
    using System;

    [Flags]
    public enum CommandBehavior
    {
        CloseConnection = 0x20,
        Default = 0,
        KeyInfo = 4,
        SchemaOnly = 2,
        SequentialAccess = 0x10,
        SingleResult = 1,
        SingleRow = 8
    }
}

