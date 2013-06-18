namespace System.Web
{
    using System;

    [Flags]
    internal enum VirtualPathOptions
    {
        AllowAbsolutePath = 4,
        AllowAllPath = 0x1c,
        AllowAppRelativePath = 8,
        AllowNull = 1,
        AllowRelativePath = 0x10,
        EnsureTrailingSlash = 2,
        FailIfMalformed = 0x20
    }
}

