namespace System.Data.OleDb
{
    using System;

    internal enum DBBindStatus
    {
        OK,
        BADORDINAL,
        UNSUPPORTEDCONVERSION,
        BADBINDINFO,
        BADSTORAGEFLAGS,
        NOINTERFACE,
        MULTIPLESTORAGE
    }
}

