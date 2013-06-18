namespace System.Data.OracleClient
{
    using System;

    internal sealed class DbSqlParserTable
    {
        private DbSqlParserColumnCollection _columns;
        private string _correlationName;
        private string _databaseName;
        private string _schemaName;
        private string _tableName;

        internal DbSqlParserTable(string databaseName, string schemaName, string tableName, string correlationName)
        {
            this._databaseName = databaseName;
            this._schemaName = schemaName;
            this._tableName = tableName;
            this._correlationName = correlationName;
        }

        internal DbSqlParserColumnCollection Columns
        {
            get
            {
                if (this._columns == null)
                {
                    this._columns = new DbSqlParserColumnCollection();
                }
                return this._columns;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                if (!typeof(DbSqlParserColumnCollection).IsInstanceOfType(value))
                {
                    throw new InvalidCastException("value");
                }
                this._columns = value;
            }
        }

        internal string CorrelationName
        {
            get
            {
                if (this._correlationName != null)
                {
                    return this._correlationName;
                }
                return string.Empty;
            }
        }

        internal string DatabaseName
        {
            get
            {
                if (this._databaseName != null)
                {
                    return this._databaseName;
                }
                return string.Empty;
            }
        }

        internal string SchemaName
        {
            get
            {
                if (this._schemaName != null)
                {
                    return this._schemaName;
                }
                return string.Empty;
            }
        }

        internal string TableName
        {
            get
            {
                if (this._tableName != null)
                {
                    return this._tableName;
                }
                return string.Empty;
            }
        }
    }
}

