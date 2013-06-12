namespace System.Data.SqlTypes
{
    using System;

    [Serializable]
    internal enum SqlBytesCharsState
    {
        Buffer = 1,
        Null = 0,
        Stream = 3
    }
}

