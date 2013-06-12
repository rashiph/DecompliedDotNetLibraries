namespace System.Data.SqlClient
{
    using System;
    using System.Data.Common;

    public sealed class SqlBulkCopyColumnMapping
    {
        internal string _destinationColumnName;
        internal int _destinationColumnOrdinal;
        internal int _internalDestinationColumnOrdinal;
        internal int _internalSourceColumnOrdinal;
        internal string _sourceColumnName;
        internal int _sourceColumnOrdinal;

        public SqlBulkCopyColumnMapping()
        {
            this._internalSourceColumnOrdinal = -1;
        }

        public SqlBulkCopyColumnMapping(int sourceColumnOrdinal, int destinationOrdinal)
        {
            this.SourceOrdinal = sourceColumnOrdinal;
            this.DestinationOrdinal = destinationOrdinal;
        }

        public SqlBulkCopyColumnMapping(int sourceColumnOrdinal, string destinationColumn)
        {
            this.SourceOrdinal = sourceColumnOrdinal;
            this.DestinationColumn = destinationColumn;
        }

        public SqlBulkCopyColumnMapping(string sourceColumn, int destinationOrdinal)
        {
            this.SourceColumn = sourceColumn;
            this.DestinationOrdinal = destinationOrdinal;
        }

        public SqlBulkCopyColumnMapping(string sourceColumn, string destinationColumn)
        {
            this.SourceColumn = sourceColumn;
            this.DestinationColumn = destinationColumn;
        }

        public string DestinationColumn
        {
            get
            {
                if (this._destinationColumnName != null)
                {
                    return this._destinationColumnName;
                }
                return string.Empty;
            }
            set
            {
                this._destinationColumnOrdinal = this._internalDestinationColumnOrdinal = -1;
                this._destinationColumnName = value;
            }
        }

        public int DestinationOrdinal
        {
            get
            {
                return this._destinationColumnOrdinal;
            }
            set
            {
                if (value < 0)
                {
                    throw ADP.IndexOutOfRange(value);
                }
                this._destinationColumnName = null;
                this._destinationColumnOrdinal = this._internalDestinationColumnOrdinal = value;
            }
        }

        public string SourceColumn
        {
            get
            {
                if (this._sourceColumnName != null)
                {
                    return this._sourceColumnName;
                }
                return string.Empty;
            }
            set
            {
                this._sourceColumnOrdinal = this._internalSourceColumnOrdinal = -1;
                this._sourceColumnName = value;
            }
        }

        public int SourceOrdinal
        {
            get
            {
                return this._sourceColumnOrdinal;
            }
            set
            {
                if (value < 0)
                {
                    throw ADP.IndexOutOfRange(value);
                }
                this._sourceColumnName = null;
                this._sourceColumnOrdinal = this._internalSourceColumnOrdinal = value;
            }
        }
    }
}

