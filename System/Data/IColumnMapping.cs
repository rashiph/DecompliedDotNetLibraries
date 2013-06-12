namespace System.Data
{
    using System;

    public interface IColumnMapping
    {
        string DataSetColumn { get; set; }

        string SourceColumn { get; set; }
    }
}

