namespace System.Data.Common
{
    using System;
    using System.Data;

    internal sealed class LoadAdapter : DataAdapter
    {
        internal LoadAdapter()
        {
        }

        internal int FillFromReader(DataTable[] dataTables, IDataReader dataReader, int startRecord, int maxRecords)
        {
            return this.Fill(dataTables, dataReader, startRecord, maxRecords);
        }
    }
}

