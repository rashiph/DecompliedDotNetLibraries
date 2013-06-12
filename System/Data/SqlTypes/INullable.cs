namespace System.Data.SqlTypes
{
    using System;

    public interface INullable
    {
        bool IsNull { get; }
    }
}

