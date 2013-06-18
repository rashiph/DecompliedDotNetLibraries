namespace System.DirectoryServices
{
    using System;

    [Flags]
    public enum SecurityMasks
    {
        Dacl = 4,
        Group = 2,
        None = 0,
        Owner = 1,
        Sacl = 8
    }
}

