namespace System.Data.SqlClient
{
    using System;
    using System.Collections.Generic;

    internal sealed class _SqlMetaDataSetCollection
    {
        private readonly List<_SqlMetaDataSet> altMetaDataSetArray = new List<_SqlMetaDataSet>();
        internal _SqlMetaDataSet metaDataSet;

        internal _SqlMetaDataSetCollection()
        {
        }

        internal _SqlMetaDataSet GetAltMetaData(int id)
        {
            foreach (_SqlMetaDataSet set in this.altMetaDataSetArray)
            {
                if (set.id == id)
                {
                    return set;
                }
            }
            return null;
        }

        internal void SetAltMetaData(_SqlMetaDataSet altMetaDataSet)
        {
            int id = altMetaDataSet.id;
            for (int i = 0; i < this.altMetaDataSetArray.Count; i++)
            {
                if (this.altMetaDataSetArray[i].id == id)
                {
                    this.altMetaDataSetArray[i] = altMetaDataSet;
                    return;
                }
            }
            this.altMetaDataSetArray.Add(altMetaDataSet);
        }
    }
}

