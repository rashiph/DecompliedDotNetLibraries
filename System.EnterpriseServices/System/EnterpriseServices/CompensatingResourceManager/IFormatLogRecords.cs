namespace System.EnterpriseServices.CompensatingResourceManager
{
    using System;

    internal interface IFormatLogRecords
    {
        string[] Format(LogRecord r);

        int ColumnCount { get; }

        string[] ColumnHeaders { get; }
    }
}

