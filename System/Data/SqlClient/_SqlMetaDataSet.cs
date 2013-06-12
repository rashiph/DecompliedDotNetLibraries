namespace System.Data.SqlClient
{
    using System;
    using System.Data;
    using System.Reflection;

    internal sealed class _SqlMetaDataSet
    {
        internal ushort id;
        internal int[] indexMap;
        private readonly _SqlMetaData[] metaDataArray;
        internal DataTable schemaTable;
        internal int visibleColumns;

        internal _SqlMetaDataSet(int count)
        {
            this.metaDataArray = new _SqlMetaData[count];
            for (int i = 0; i < this.metaDataArray.Length; i++)
            {
                this.metaDataArray[i] = new _SqlMetaData(i);
            }
        }

        internal _SqlMetaData this[int index]
        {
            get
            {
                return this.metaDataArray[index];
            }
            set
            {
                this.metaDataArray[index] = value;
            }
        }

        internal int Length
        {
            get
            {
                return this.metaDataArray.Length;
            }
        }
    }
}

