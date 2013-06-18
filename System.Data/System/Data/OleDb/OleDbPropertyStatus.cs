namespace System.Data.OleDb
{
    using System;

    internal enum OleDbPropertyStatus
    {
        Ok,
        NotSupported,
        BadValue,
        BadOption,
        BadColumn,
        NotAllSettable,
        NotSettable,
        NotSet,
        Conflicting,
        NotAvailable
    }
}

