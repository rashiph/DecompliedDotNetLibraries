namespace System.Data
{
    using System;

    public sealed class DataRowBuilder
    {
        internal int _record;
        internal readonly DataTable _table;

        internal DataRowBuilder(DataTable table, int record)
        {
            this._table = table;
            this._record = record;
        }
    }
}

