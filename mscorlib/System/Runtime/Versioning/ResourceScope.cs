namespace System.Runtime.Versioning
{
    using System;

    [Flags]
    public enum ResourceScope
    {
        AppDomain = 4,
        Assembly = 0x20,
        Library = 8,
        Machine = 1,
        None = 0,
        Private = 0x10,
        Process = 2
    }
}

