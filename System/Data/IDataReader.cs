namespace System.Data
{
    using System;

    public interface IDataReader : IDisposable, IDataRecord
    {
        void Close();
        DataTable GetSchemaTable();
        bool NextResult();
        bool Read();

        int Depth { get; }

        bool IsClosed { get; }

        int RecordsAffected { get; }
    }
}

