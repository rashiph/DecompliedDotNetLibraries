namespace System.Data.OracleClient
{
    using System;

    internal sealed class DbSqlParserColumn
    {
        private string _alias;
        private string _columnName;
        private string _databaseName;
        private bool _isKey;
        private bool _isUnique;
        private string _schemaName;
        private string _tableName;

        internal DbSqlParserColumn(string databaseName, string schemaName, string tableName, string columnName, string alias)
        {
            this._databaseName = databaseName;
            this._schemaName = schemaName;
            this._tableName = tableName;
            this._columnName = columnName;
            this._alias = alias;
        }

        internal void CopySchemaInfoFrom(DbSqlParserColumn completedColumn)
        {
            this._databaseName = completedColumn.DatabaseName;
            this._schemaName = completedColumn.SchemaName;
            this._tableName = completedColumn.TableName;
            this._columnName = completedColumn.ColumnName;
            this._isKey = completedColumn.IsKey;
            this._isUnique = completedColumn.IsUnique;
        }

        internal void CopySchemaInfoFrom(DbSqlParserTable table)
        {
            this._databaseName = table.DatabaseName;
            this._schemaName = table.SchemaName;
            this._tableName = table.TableName;
            this._isKey = false;
            this._isUnique = false;
        }

        internal void SetConstraint(ConstraintType constraintType)
        {
            switch (constraintType)
            {
                case ConstraintType.PrimaryKey:
                    this._isKey = true;
                    return;

                case ConstraintType.UniqueKey:
                case ConstraintType.UniqueConstraint:
                    this._isUnique = this._isKey = true;
                    return;
            }
        }

        internal string ColumnName
        {
            get
            {
                if (this._columnName != null)
                {
                    return this._columnName;
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

        internal bool IsAliased
        {
            get
            {
                return (this._alias != null);
            }
        }

        internal bool IsExpression
        {
            get
            {
                return (this._columnName == null);
            }
        }

        internal bool IsKey
        {
            get
            {
                return this._isKey;
            }
        }

        internal bool IsUnique
        {
            get
            {
                return this._isUnique;
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

        internal enum ConstraintType
        {
            PrimaryKey = 1,
            UniqueConstraint = 3,
            UniqueKey = 2
        }
    }
}

