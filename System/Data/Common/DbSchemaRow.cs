namespace System.Data.Common
{
    using System;
    using System.Data;
    using System.Globalization;

    internal sealed class DbSchemaRow
    {
        private System.Data.DataRow dataRow;
        internal const string SchemaMappingUnsortedIndex = "SchemaMapping Unsorted Index";
        private DbSchemaTable schemaTable;

        internal DbSchemaRow(DbSchemaTable schemaTable, System.Data.DataRow dataRow)
        {
            this.schemaTable = schemaTable;
            this.dataRow = dataRow;
        }

        internal static DbSchemaRow[] GetSortedSchemaRows(DataTable dataTable, bool returnProviderSpecificTypes)
        {
            DataColumn column = dataTable.Columns["SchemaMapping Unsorted Index"];
            if (column == null)
            {
                column = new DataColumn("SchemaMapping Unsorted Index", typeof(int));
                dataTable.Columns.Add(column);
            }
            int count = dataTable.Rows.Count;
            for (int i = 0; i < count; i++)
            {
                dataTable.Rows[i][column] = i;
            }
            DbSchemaTable schemaTable = new DbSchemaTable(dataTable, returnProviderSpecificTypes);
            System.Data.DataRow[] rowArray = dataTable.Select(null, "ColumnOrdinal ASC", DataViewRowState.CurrentRows);
            DbSchemaRow[] rowArray2 = new DbSchemaRow[rowArray.Length];
            for (int j = 0; j < rowArray.Length; j++)
            {
                rowArray2[j] = new DbSchemaRow(schemaTable, rowArray[j]);
            }
            return rowArray2;
        }

        internal bool AllowDBNull
        {
            get
            {
                if (this.schemaTable.AllowDBNull != null)
                {
                    object obj2 = this.dataRow[this.schemaTable.AllowDBNull, DataRowVersion.Default];
                    if (!Convert.IsDBNull(obj2))
                    {
                        return Convert.ToBoolean(obj2, CultureInfo.InvariantCulture);
                    }
                }
                return true;
            }
        }

        internal string BaseCatalogName
        {
            get
            {
                if (this.schemaTable.BaseCatalogName != null)
                {
                    object obj2 = this.dataRow[this.schemaTable.BaseCatalogName, DataRowVersion.Default];
                    if (!Convert.IsDBNull(obj2))
                    {
                        return Convert.ToString(obj2, CultureInfo.InvariantCulture);
                    }
                }
                return "";
            }
        }

        internal string BaseColumnName
        {
            get
            {
                if (this.schemaTable.BaseColumnName != null)
                {
                    object obj2 = this.dataRow[this.schemaTable.BaseColumnName, DataRowVersion.Default];
                    if (!Convert.IsDBNull(obj2))
                    {
                        return Convert.ToString(obj2, CultureInfo.InvariantCulture);
                    }
                }
                return "";
            }
        }

        internal string BaseSchemaName
        {
            get
            {
                if (this.schemaTable.BaseSchemaName != null)
                {
                    object obj2 = this.dataRow[this.schemaTable.BaseSchemaName, DataRowVersion.Default];
                    if (!Convert.IsDBNull(obj2))
                    {
                        return Convert.ToString(obj2, CultureInfo.InvariantCulture);
                    }
                }
                return "";
            }
        }

        internal string BaseServerName
        {
            get
            {
                if (this.schemaTable.BaseServerName != null)
                {
                    object obj2 = this.dataRow[this.schemaTable.BaseServerName, DataRowVersion.Default];
                    if (!Convert.IsDBNull(obj2))
                    {
                        return Convert.ToString(obj2, CultureInfo.InvariantCulture);
                    }
                }
                return "";
            }
        }

        internal string BaseTableName
        {
            get
            {
                if (this.schemaTable.BaseTableName != null)
                {
                    object obj2 = this.dataRow[this.schemaTable.BaseTableName, DataRowVersion.Default];
                    if (!Convert.IsDBNull(obj2))
                    {
                        return Convert.ToString(obj2, CultureInfo.InvariantCulture);
                    }
                }
                return "";
            }
        }

        internal string ColumnName
        {
            get
            {
                object obj2 = this.dataRow[this.schemaTable.ColumnName, DataRowVersion.Default];
                if (!Convert.IsDBNull(obj2))
                {
                    return Convert.ToString(obj2, CultureInfo.InvariantCulture);
                }
                return "";
            }
        }

        internal System.Data.DataRow DataRow
        {
            get
            {
                return this.dataRow;
            }
        }

        internal Type DataType
        {
            get
            {
                if (this.schemaTable.DataType != null)
                {
                    object obj2 = this.dataRow[this.schemaTable.DataType, DataRowVersion.Default];
                    if (!Convert.IsDBNull(obj2))
                    {
                        return (Type) obj2;
                    }
                }
                return null;
            }
        }

        internal bool IsAutoIncrement
        {
            get
            {
                if (this.schemaTable.IsAutoIncrement != null)
                {
                    object obj2 = this.dataRow[this.schemaTable.IsAutoIncrement, DataRowVersion.Default];
                    if (!Convert.IsDBNull(obj2))
                    {
                        return Convert.ToBoolean(obj2, CultureInfo.InvariantCulture);
                    }
                }
                return false;
            }
        }

        internal bool IsExpression
        {
            get
            {
                if (this.schemaTable.IsExpression != null)
                {
                    object obj2 = this.dataRow[this.schemaTable.IsExpression, DataRowVersion.Default];
                    if (!Convert.IsDBNull(obj2))
                    {
                        return Convert.ToBoolean(obj2, CultureInfo.InvariantCulture);
                    }
                }
                return false;
            }
        }

        internal bool IsHidden
        {
            get
            {
                if (this.schemaTable.IsHidden != null)
                {
                    object obj2 = this.dataRow[this.schemaTable.IsHidden, DataRowVersion.Default];
                    if (!Convert.IsDBNull(obj2))
                    {
                        return Convert.ToBoolean(obj2, CultureInfo.InvariantCulture);
                    }
                }
                return false;
            }
        }

        internal bool IsKey
        {
            get
            {
                if (this.schemaTable.IsKey != null)
                {
                    object obj2 = this.dataRow[this.schemaTable.IsKey, DataRowVersion.Default];
                    if (!Convert.IsDBNull(obj2))
                    {
                        return Convert.ToBoolean(obj2, CultureInfo.InvariantCulture);
                    }
                }
                return false;
            }
        }

        internal bool IsLong
        {
            get
            {
                if (this.schemaTable.IsLong != null)
                {
                    object obj2 = this.dataRow[this.schemaTable.IsLong, DataRowVersion.Default];
                    if (!Convert.IsDBNull(obj2))
                    {
                        return Convert.ToBoolean(obj2, CultureInfo.InvariantCulture);
                    }
                }
                return false;
            }
        }

        internal bool IsReadOnly
        {
            get
            {
                if (this.schemaTable.IsReadOnly != null)
                {
                    object obj2 = this.dataRow[this.schemaTable.IsReadOnly, DataRowVersion.Default];
                    if (!Convert.IsDBNull(obj2))
                    {
                        return Convert.ToBoolean(obj2, CultureInfo.InvariantCulture);
                    }
                }
                return false;
            }
        }

        internal bool IsRowVersion
        {
            get
            {
                if (this.schemaTable.IsRowVersion != null)
                {
                    object obj2 = this.dataRow[this.schemaTable.IsRowVersion, DataRowVersion.Default];
                    if (!Convert.IsDBNull(obj2))
                    {
                        return Convert.ToBoolean(obj2, CultureInfo.InvariantCulture);
                    }
                }
                return false;
            }
        }

        internal bool IsUnique
        {
            get
            {
                if (this.schemaTable.IsUnique != null)
                {
                    object obj2 = this.dataRow[this.schemaTable.IsUnique, DataRowVersion.Default];
                    if (!Convert.IsDBNull(obj2))
                    {
                        return Convert.ToBoolean(obj2, CultureInfo.InvariantCulture);
                    }
                }
                return false;
            }
        }

        internal int Size
        {
            get
            {
                object obj2 = this.dataRow[this.schemaTable.Size, DataRowVersion.Default];
                if (!Convert.IsDBNull(obj2))
                {
                    return Convert.ToInt32(obj2, CultureInfo.InvariantCulture);
                }
                return 0;
            }
        }

        internal int UnsortedIndex
        {
            get
            {
                return (int) this.dataRow[this.schemaTable.UnsortedIndex, DataRowVersion.Default];
            }
        }
    }
}

