namespace System.DirectoryServices
{
    using System;

    [Flags]
    public enum DirectorySynchronizationOptions : long
    {
        IncrementalValues = 0x80000000L,
        None = 0L,
        ObjectSecurity = 1L,
        ParentsFirst = 0x800L,
        PublicDataOnly = 0x2000L
    }
}

