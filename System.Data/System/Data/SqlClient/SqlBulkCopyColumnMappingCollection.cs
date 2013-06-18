namespace System.Data.SqlClient
{
    using System;
    using System.Collections;
    using System.Data.Common;
    using System.Reflection;

    public sealed class SqlBulkCopyColumnMappingCollection : CollectionBase
    {
        private MappingSchema _mappingSchema;
        private bool _readOnly;

        internal SqlBulkCopyColumnMappingCollection()
        {
        }

        public SqlBulkCopyColumnMapping Add(SqlBulkCopyColumnMapping bulkCopyColumnMapping)
        {
            this.AssertWriteAccess();
            if ((ADP.IsEmpty(bulkCopyColumnMapping.SourceColumn) && (bulkCopyColumnMapping.SourceOrdinal == -1)) || (ADP.IsEmpty(bulkCopyColumnMapping.DestinationColumn) && (bulkCopyColumnMapping.DestinationOrdinal == -1)))
            {
                throw SQL.BulkLoadNonMatchingColumnMapping();
            }
            base.InnerList.Add(bulkCopyColumnMapping);
            return bulkCopyColumnMapping;
        }

        public SqlBulkCopyColumnMapping Add(int sourceColumnIndex, int destinationColumnIndex)
        {
            this.AssertWriteAccess();
            SqlBulkCopyColumnMapping bulkCopyColumnMapping = new SqlBulkCopyColumnMapping(sourceColumnIndex, destinationColumnIndex);
            return this.Add(bulkCopyColumnMapping);
        }

        public SqlBulkCopyColumnMapping Add(int sourceColumnIndex, string destinationColumn)
        {
            this.AssertWriteAccess();
            SqlBulkCopyColumnMapping bulkCopyColumnMapping = new SqlBulkCopyColumnMapping(sourceColumnIndex, destinationColumn);
            return this.Add(bulkCopyColumnMapping);
        }

        public SqlBulkCopyColumnMapping Add(string sourceColumn, int destinationColumnIndex)
        {
            this.AssertWriteAccess();
            SqlBulkCopyColumnMapping bulkCopyColumnMapping = new SqlBulkCopyColumnMapping(sourceColumn, destinationColumnIndex);
            return this.Add(bulkCopyColumnMapping);
        }

        public SqlBulkCopyColumnMapping Add(string sourceColumn, string destinationColumn)
        {
            this.AssertWriteAccess();
            SqlBulkCopyColumnMapping bulkCopyColumnMapping = new SqlBulkCopyColumnMapping(sourceColumn, destinationColumn);
            return this.Add(bulkCopyColumnMapping);
        }

        private void AssertWriteAccess()
        {
            if (this.ReadOnly)
            {
                throw SQL.BulkLoadMappingInaccessible();
            }
        }

        public void Clear()
        {
            this.AssertWriteAccess();
            base.Clear();
        }

        public bool Contains(SqlBulkCopyColumnMapping value)
        {
            return (-1 != base.InnerList.IndexOf(value));
        }

        public void CopyTo(SqlBulkCopyColumnMapping[] array, int index)
        {
            base.InnerList.CopyTo(array, index);
        }

        internal void CreateDefaultMapping(int columnCount)
        {
            for (int i = 0; i < columnCount; i++)
            {
                base.InnerList.Add(new SqlBulkCopyColumnMapping(i, i));
            }
        }

        public int IndexOf(SqlBulkCopyColumnMapping value)
        {
            return base.InnerList.IndexOf(value);
        }

        public void Insert(int index, SqlBulkCopyColumnMapping value)
        {
            this.AssertWriteAccess();
            base.InnerList.Insert(index, value);
        }

        public void Remove(SqlBulkCopyColumnMapping value)
        {
            this.AssertWriteAccess();
            base.InnerList.Remove(value);
        }

        public void RemoveAt(int index)
        {
            this.AssertWriteAccess();
            base.RemoveAt(index);
        }

        internal void ValidateCollection()
        {
            foreach (SqlBulkCopyColumnMapping mapping in this)
            {
                MappingSchema ordinalsOrdinals;
                if (mapping.SourceOrdinal != -1)
                {
                    if (mapping.DestinationOrdinal != -1)
                    {
                        ordinalsOrdinals = MappingSchema.OrdinalsOrdinals;
                    }
                    else
                    {
                        ordinalsOrdinals = MappingSchema.OrdinalsNames;
                    }
                }
                else if (mapping.DestinationOrdinal != -1)
                {
                    ordinalsOrdinals = MappingSchema.NemesOrdinals;
                }
                else
                {
                    ordinalsOrdinals = MappingSchema.NamesNames;
                }
                if (this._mappingSchema == MappingSchema.Undefined)
                {
                    this._mappingSchema = ordinalsOrdinals;
                }
                else if (this._mappingSchema != ordinalsOrdinals)
                {
                    throw SQL.BulkLoadMappingsNamesOrOrdinalsOnly();
                }
            }
        }

        public SqlBulkCopyColumnMapping this[int index]
        {
            get
            {
                return (SqlBulkCopyColumnMapping) base.List[index];
            }
        }

        internal bool ReadOnly
        {
            get
            {
                return this._readOnly;
            }
            set
            {
                this._readOnly = value;
            }
        }

        private enum MappingSchema
        {
            Undefined,
            NamesNames,
            NemesOrdinals,
            OrdinalsNames,
            OrdinalsOrdinals
        }
    }
}

