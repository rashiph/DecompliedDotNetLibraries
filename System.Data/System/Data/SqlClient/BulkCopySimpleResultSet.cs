namespace System.Data.SqlClient
{
    using System;
    using System.Collections;
    using System.Reflection;

    internal sealed class BulkCopySimpleResultSet
    {
        private ArrayList _results = new ArrayList();
        private int[] indexmap;
        private Result resultSet;

        internal BulkCopySimpleResultSet()
        {
        }

        internal int[] CreateIndexMap()
        {
            return this.indexmap;
        }

        internal object[] CreateRowBuffer()
        {
            Row row = new Row(this.resultSet.MetaData.Length);
            this.resultSet.AddRow(row);
            return row.DataFields;
        }

        internal void SetMetaData(_SqlMetaDataSet metadata)
        {
            this.resultSet = new Result(metadata);
            this._results.Add(this.resultSet);
            this.indexmap = new int[this.resultSet.MetaData.Length];
            for (int i = 0; i < this.indexmap.Length; i++)
            {
                this.indexmap[i] = i;
            }
        }

        internal Result this[int idx]
        {
            get
            {
                return (Result) this._results[idx];
            }
        }
    }
}

