namespace System.Data.SqlClient
{
    using System;

    internal sealed class _ColumnMapping
    {
        internal _SqlMetaData _metadata;
        internal int _sourceColumnOrdinal;

        internal _ColumnMapping(int columnId, _SqlMetaData metadata)
        {
            this._sourceColumnOrdinal = columnId;
            this._metadata = metadata;
        }
    }
}

