namespace System.Data.Common
{
    using System;
    using System.Data;

    internal sealed class DbSchemaTable
    {
        private bool _returnProviderSpecificTypes;
        private DataColumn[] columnCache = new DataColumn[DBCOLUMN_NAME.Length];
        private DataColumnCollection columns;
        internal DataTable dataTable;
        private static readonly string[] DBCOLUMN_NAME = new string[] { 
            SchemaTableColumn.ColumnName, SchemaTableColumn.ColumnOrdinal, SchemaTableColumn.ColumnSize, SchemaTableOptionalColumn.BaseServerName, SchemaTableOptionalColumn.BaseCatalogName, SchemaTableColumn.BaseColumnName, SchemaTableColumn.BaseSchemaName, SchemaTableColumn.BaseTableName, SchemaTableOptionalColumn.IsAutoIncrement, SchemaTableColumn.IsUnique, SchemaTableColumn.IsKey, SchemaTableOptionalColumn.IsRowVersion, SchemaTableColumn.DataType, SchemaTableOptionalColumn.ProviderSpecificDataType, SchemaTableColumn.AllowDBNull, SchemaTableColumn.ProviderType, 
            SchemaTableColumn.IsExpression, SchemaTableOptionalColumn.IsHidden, SchemaTableColumn.IsLong, SchemaTableOptionalColumn.IsReadOnly, "SchemaMapping Unsorted Index"
         };

        internal DbSchemaTable(DataTable dataTable, bool returnProviderSpecificTypes)
        {
            this.dataTable = dataTable;
            this.columns = dataTable.Columns;
            this._returnProviderSpecificTypes = returnProviderSpecificTypes;
        }

        private DataColumn CachedDataColumn(ColumnEnum column)
        {
            return this.CachedDataColumn(column, column);
        }

        private DataColumn CachedDataColumn(ColumnEnum column, ColumnEnum column2)
        {
            DataColumn column3 = this.columnCache[(int) column];
            if (column3 == null)
            {
                int index = this.columns.IndexOf(DBCOLUMN_NAME[(int) column]);
                if ((-1 == index) && (column != column2))
                {
                    index = this.columns.IndexOf(DBCOLUMN_NAME[(int) column2]);
                }
                if (-1 != index)
                {
                    column3 = this.columns[index];
                    this.columnCache[(int) column] = column3;
                }
            }
            return column3;
        }

        internal DataColumn AllowDBNull
        {
            get
            {
                return this.CachedDataColumn(ColumnEnum.AllowDBNull);
            }
        }

        internal DataColumn BaseCatalogName
        {
            get
            {
                return this.CachedDataColumn(ColumnEnum.BaseCatalogName);
            }
        }

        internal DataColumn BaseColumnName
        {
            get
            {
                return this.CachedDataColumn(ColumnEnum.BaseColumnName);
            }
        }

        internal DataColumn BaseSchemaName
        {
            get
            {
                return this.CachedDataColumn(ColumnEnum.BaseSchemaName);
            }
        }

        internal DataColumn BaseServerName
        {
            get
            {
                return this.CachedDataColumn(ColumnEnum.BaseServerName);
            }
        }

        internal DataColumn BaseTableName
        {
            get
            {
                return this.CachedDataColumn(ColumnEnum.BaseTableName);
            }
        }

        internal DataColumn ColumnName
        {
            get
            {
                return this.CachedDataColumn(ColumnEnum.ColumnName);
            }
        }

        internal DataColumn DataType
        {
            get
            {
                if (this._returnProviderSpecificTypes)
                {
                    return this.CachedDataColumn(ColumnEnum.ProviderSpecificDataType, ColumnEnum.DataType);
                }
                return this.CachedDataColumn(ColumnEnum.DataType);
            }
        }

        internal DataColumn IsAutoIncrement
        {
            get
            {
                return this.CachedDataColumn(ColumnEnum.IsAutoIncrement);
            }
        }

        internal DataColumn IsExpression
        {
            get
            {
                return this.CachedDataColumn(ColumnEnum.IsExpression);
            }
        }

        internal DataColumn IsHidden
        {
            get
            {
                return this.CachedDataColumn(ColumnEnum.IsHidden);
            }
        }

        internal DataColumn IsKey
        {
            get
            {
                return this.CachedDataColumn(ColumnEnum.IsKey);
            }
        }

        internal DataColumn IsLong
        {
            get
            {
                return this.CachedDataColumn(ColumnEnum.IsLong);
            }
        }

        internal DataColumn IsReadOnly
        {
            get
            {
                return this.CachedDataColumn(ColumnEnum.IsReadOnly);
            }
        }

        internal DataColumn IsRowVersion
        {
            get
            {
                return this.CachedDataColumn(ColumnEnum.IsRowVersion);
            }
        }

        internal DataColumn IsUnique
        {
            get
            {
                return this.CachedDataColumn(ColumnEnum.IsUnique);
            }
        }

        internal DataColumn Size
        {
            get
            {
                return this.CachedDataColumn(ColumnEnum.ColumnSize);
            }
        }

        internal DataColumn UnsortedIndex
        {
            get
            {
                return this.CachedDataColumn(ColumnEnum.SchemaMappingUnsortedIndex);
            }
        }

        private enum ColumnEnum
        {
            ColumnName,
            ColumnOrdinal,
            ColumnSize,
            BaseServerName,
            BaseCatalogName,
            BaseColumnName,
            BaseSchemaName,
            BaseTableName,
            IsAutoIncrement,
            IsUnique,
            IsKey,
            IsRowVersion,
            DataType,
            ProviderSpecificDataType,
            AllowDBNull,
            ProviderType,
            IsExpression,
            IsHidden,
            IsLong,
            IsReadOnly,
            SchemaMappingUnsortedIndex
        }
    }
}

