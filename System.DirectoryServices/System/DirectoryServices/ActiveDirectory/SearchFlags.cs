namespace System.DirectoryServices.ActiveDirectory
{
    using System;

    internal enum SearchFlags
    {
        IsInAnr = 4,
        IsIndexed = 1,
        IsIndexedOverContainer = 2,
        IsOnTombstonedObject = 8,
        IsTupleIndexed = 0x20,
        None = 0
    }
}

