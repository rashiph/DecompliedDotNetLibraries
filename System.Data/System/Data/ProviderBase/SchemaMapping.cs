namespace System.Data.ProviderBase
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Data.SqlTypes;
    using System.Globalization;
    using System.IO;
    using System.Xml;

    internal sealed class SchemaMapping
    {
        private readonly DataAdapter _adapter;
        private bool[] _chapterMap;
        private readonly DataReaderContainer _dataReader;
        private readonly DataSet _dataSet;
        private System.Data.DataTable _dataTable;
        private readonly string[] _fieldNames;
        private int[] _indexMap;
        private readonly LoadOption _loadOption;
        private object[] _mappedDataValues;
        private int _mappedLength;
        private int _mappedMode;
        private readonly object[] _readerDataValues;
        private readonly System.Data.DataTable _schemaTable;
        private readonly DataTableMapping _tableMapping;
        private int[] _xmlMap;
        private const int MapChapters = 3;
        private const int MapChaptersReordered = 4;
        private const int MapDifferentSize = 1;
        private const int MapExactMatch = 0;
        private const int MapReorderedValues = 2;
        private const int SqlXml = 1;
        private const int XmlDocument = 2;

        internal SchemaMapping(DataAdapter adapter, DataSet dataset, System.Data.DataTable datatable, DataReaderContainer dataReader, bool keyInfo, SchemaType schemaType, string sourceTableName, bool gettingData, DataColumn parentChapterColumn, object parentChapterValue)
        {
            MissingMappingAction missingMappingAction;
            MissingSchemaAction missingSchemaAction;
            this._dataSet = dataset;
            this._dataTable = datatable;
            this._adapter = adapter;
            this._dataReader = dataReader;
            if (keyInfo)
            {
                this._schemaTable = dataReader.GetSchemaTable();
            }
            if (adapter.ShouldSerializeFillLoadOption())
            {
                this._loadOption = adapter.FillLoadOption;
            }
            else if (adapter.AcceptChangesDuringFill)
            {
                this._loadOption = (LoadOption) 4;
            }
            else
            {
                this._loadOption = (LoadOption) 5;
            }
            if (SchemaType.Mapped == schemaType)
            {
                missingMappingAction = this._adapter.MissingMappingAction;
                missingSchemaAction = this._adapter.MissingSchemaAction;
                if (ADP.IsEmpty(sourceTableName))
                {
                    if (this._dataTable != null)
                    {
                        int num2 = this._adapter.IndexOfDataSetTable(this._dataTable.TableName);
                        if (-1 == num2)
                        {
                            switch (missingMappingAction)
                            {
                                case MissingMappingAction.Passthrough:
                                    this._tableMapping = new DataTableMapping(this._dataTable.TableName, this._dataTable.TableName);
                                    goto Label_01DB;

                                case MissingMappingAction.Ignore:
                                    this._tableMapping = null;
                                    goto Label_01DB;

                                case MissingMappingAction.Error:
                                    throw ADP.MissingTableMappingDestination(this._dataTable.TableName);
                            }
                            throw ADP.InvalidMissingMappingAction(missingMappingAction);
                        }
                        this._tableMapping = this._adapter.TableMappings[num2];
                    }
                }
                else
                {
                    this._tableMapping = this._adapter.GetTableMappingBySchemaAction(sourceTableName, sourceTableName, missingMappingAction);
                }
            }
            else
            {
                if (SchemaType.Source != schemaType)
                {
                    throw ADP.InvalidSchemaType(schemaType);
                }
                missingMappingAction = MissingMappingAction.Passthrough;
                missingSchemaAction = MissingSchemaAction.Add;
                if (!ADP.IsEmpty(sourceTableName))
                {
                    this._tableMapping = DataTableMappingCollection.GetTableMappingBySchemaAction(null, sourceTableName, sourceTableName, missingMappingAction);
                }
                else if (this._dataTable != null)
                {
                    int num = this._adapter.IndexOfDataSetTable(this._dataTable.TableName);
                    if (-1 != num)
                    {
                        this._tableMapping = this._adapter.TableMappings[num];
                    }
                    else
                    {
                        this._tableMapping = new DataTableMapping(this._dataTable.TableName, this._dataTable.TableName);
                    }
                }
            }
        Label_01DB:
            if (this._tableMapping != null)
            {
                if (this._dataTable == null)
                {
                    this._dataTable = this._tableMapping.GetDataTableBySchemaAction(this._dataSet, missingSchemaAction);
                }
                if (this._dataTable != null)
                {
                    this._fieldNames = GenerateFieldNames(dataReader);
                    if (this._schemaTable == null)
                    {
                        this._readerDataValues = this.SetupSchemaWithoutKeyInfo(missingMappingAction, missingSchemaAction, gettingData, parentChapterColumn, parentChapterValue);
                        return;
                    }
                    this._readerDataValues = this.SetupSchemaWithKeyInfo(missingMappingAction, missingSchemaAction, gettingData, parentChapterColumn, parentChapterValue);
                }
            }
        }

        private void AddAdditionalProperties(DataColumn targetColumn, DataRow schemaRow)
        {
            DataColumnCollection columns = schemaRow.Table.Columns;
            DataColumn column = columns[SchemaTableOptionalColumn.DefaultValue];
            if (column != null)
            {
                targetColumn.DefaultValue = schemaRow[column];
            }
            column = columns[SchemaTableOptionalColumn.AutoIncrementSeed];
            if (column != null)
            {
                object obj6 = schemaRow[column];
                if (DBNull.Value != obj6)
                {
                    targetColumn.AutoIncrementSeed = ((IConvertible) obj6).ToInt64(CultureInfo.InvariantCulture);
                }
            }
            column = columns[SchemaTableOptionalColumn.AutoIncrementStep];
            if (column != null)
            {
                object obj5 = schemaRow[column];
                if (DBNull.Value != obj5)
                {
                    targetColumn.AutoIncrementStep = ((IConvertible) obj5).ToInt64(CultureInfo.InvariantCulture);
                }
            }
            column = columns[SchemaTableOptionalColumn.ColumnMapping];
            if (column != null)
            {
                object obj4 = schemaRow[column];
                if (DBNull.Value != obj4)
                {
                    targetColumn.ColumnMapping = (MappingType) ((IConvertible) obj4).ToInt32(CultureInfo.InvariantCulture);
                }
            }
            column = columns[SchemaTableOptionalColumn.BaseColumnNamespace];
            if (column != null)
            {
                object obj3 = schemaRow[column];
                if (DBNull.Value != obj3)
                {
                    targetColumn.Namespace = ((IConvertible) obj3).ToString(CultureInfo.InvariantCulture);
                }
            }
            column = columns[SchemaTableOptionalColumn.Expression];
            if (column != null)
            {
                object obj2 = schemaRow[column];
                if (DBNull.Value != obj2)
                {
                    targetColumn.Expression = ((IConvertible) obj2).ToString(CultureInfo.InvariantCulture);
                }
            }
        }

        private void AddItemToAllowRollback(ref List<object> items, object value)
        {
            if (items == null)
            {
                items = new List<object>();
            }
            items.Add(value);
        }

        private void AddRelation(DataColumn parentChapterColumn, DataColumn chapterColumn)
        {
            if (this._dataSet != null)
            {
                string columnName = chapterColumn.ColumnName;
                DataRelation relation = new DataRelation(columnName, new DataColumn[] { parentChapterColumn }, new DataColumn[] { chapterColumn }, false);
                int num = 1;
                string relationName = columnName;
                DataRelationCollection relations = this._dataSet.Relations;
                while (-1 != relations.IndexOf(relationName))
                {
                    relationName = columnName + num;
                    num++;
                }
                relation.RelationName = relationName;
                relations.Add(relation);
            }
        }

        internal void ApplyToDataRow(DataRow dataRow)
        {
            DataColumnCollection columns = dataRow.Table.Columns;
            this._dataReader.GetValues(this._readerDataValues);
            object[] mappedValues = this.GetMappedValues();
            bool[] flagArray = new bool[mappedValues.Length];
            for (int i = 0; i < flagArray.Length; i++)
            {
                flagArray[i] = columns[i].ReadOnly;
            }
            try
            {
                try
                {
                    for (int j = 0; j < flagArray.Length; j++)
                    {
                        if (columns[j].Expression.Length == 0)
                        {
                            columns[j].ReadOnly = false;
                        }
                    }
                    for (int k = 0; k < mappedValues.Length; k++)
                    {
                        if (mappedValues[k] != null)
                        {
                            dataRow[k] = mappedValues[k];
                        }
                    }
                }
                finally
                {
                    for (int m = 0; m < flagArray.Length; m++)
                    {
                        if (columns[m].Expression.Length == 0)
                        {
                            columns[m].ReadOnly = flagArray[m];
                        }
                    }
                }
            }
            finally
            {
                if (this._chapterMap != null)
                {
                    this.FreeDataRowChapters();
                }
            }
        }

        private int[] CreateIndexMap(int count, int index)
        {
            int[] numArray = new int[count];
            for (int i = 0; i < index; i++)
            {
                numArray[i] = i;
            }
            return numArray;
        }

        private void FreeDataRowChapters()
        {
            for (int i = 0; i < this._chapterMap.Length; i++)
            {
                if (this._chapterMap[i])
                {
                    IDisposable disposable = this._readerDataValues[i] as IDisposable;
                    if (disposable != null)
                    {
                        this._readerDataValues[i] = null;
                        disposable.Dispose();
                    }
                }
            }
        }

        private static string[] GenerateFieldNames(DataReaderContainer dataReader)
        {
            string[] columnNameArray = new string[dataReader.FieldCount];
            for (int i = 0; i < columnNameArray.Length; i++)
            {
                columnNameArray[i] = dataReader.GetName(i);
            }
            ADP.BuildSchemaTableInfoTableNames(columnNameArray);
            return columnNameArray;
        }

        private object[] GetMappedValues()
        {
            if (this._xmlMap != null)
            {
                for (int i = 0; i < this._xmlMap.Length; i++)
                {
                    if (this._xmlMap[i] != 0)
                    {
                        string s = this._readerDataValues[i] as string;
                        if ((s == null) && (this._readerDataValues[i] is SqlString))
                        {
                            SqlString str2 = (SqlString) this._readerDataValues[i];
                            if (!str2.IsNull)
                            {
                                s = str2.Value;
                            }
                            else
                            {
                                int num4 = this._xmlMap[i];
                                if (num4 == 1)
                                {
                                    this._readerDataValues[i] = System.Data.SqlTypes.SqlXml.Null;
                                }
                                else
                                {
                                    this._readerDataValues[i] = DBNull.Value;
                                }
                            }
                        }
                        if (s != null)
                        {
                            switch (this._xmlMap[i])
                            {
                                case 1:
                                {
                                    XmlReaderSettings settings = new XmlReaderSettings {
                                        ConformanceLevel = ConformanceLevel.Fragment
                                    };
                                    XmlReader reader = XmlReader.Create((TextReader) new StringReader(s), settings, (string) null);
                                    this._readerDataValues[i] = new System.Data.SqlTypes.SqlXml(reader);
                                    break;
                                }
                                case 2:
                                {
                                    System.Xml.XmlDocument document = new System.Xml.XmlDocument();
                                    document.LoadXml(s);
                                    this._readerDataValues[i] = document;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            switch (this._mappedMode)
            {
                case 1:
                    this.MappedValues();
                    break;

                case 2:
                    this.MappedIndex();
                    break;

                case 3:
                    this.MappedChapter();
                    break;

                case 4:
                    this.MappedChapterIndex();
                    break;

                default:
                    return this._readerDataValues;
            }
            return this._mappedDataValues;
        }

        internal void LoadDataRow()
        {
            try
            {
                DataRow row;
                this._dataReader.GetValues(this._readerDataValues);
                object[] mappedValues = this.GetMappedValues();
                switch (this._loadOption)
                {
                    case LoadOption.OverwriteChanges:
                    case LoadOption.PreserveChanges:
                    case LoadOption.Upsert:
                        row = this._dataTable.LoadDataRow(mappedValues, this._loadOption);
                        break;

                    case ((LoadOption) 4):
                        row = this._dataTable.LoadDataRow(mappedValues, true);
                        break;

                    case ((LoadOption) 5):
                        row = this._dataTable.LoadDataRow(mappedValues, false);
                        break;

                    default:
                        throw ADP.InvalidLoadOption(this._loadOption);
                }
                if ((this._chapterMap != null) && (this._dataSet != null))
                {
                    this.LoadDataRowChapters(row);
                }
            }
            finally
            {
                if (this._chapterMap != null)
                {
                    this.FreeDataRowChapters();
                }
            }
        }

        internal int LoadDataRowChapters(DataRow dataRow)
        {
            int num2 = 0;
            int length = this._chapterMap.Length;
            for (int i = 0; i < length; i++)
            {
                if (this._chapterMap[i])
                {
                    object obj2 = this._readerDataValues[i];
                    if ((obj2 != null) && !Convert.IsDBNull(obj2))
                    {
                        this._readerDataValues[i] = null;
                        using (IDataReader reader = (IDataReader) obj2)
                        {
                            if (!reader.IsClosed)
                            {
                                DataColumn column;
                                object obj3;
                                if (this._indexMap == null)
                                {
                                    column = this._dataTable.Columns[i];
                                    obj3 = dataRow[column];
                                }
                                else
                                {
                                    column = this._dataTable.Columns[this._indexMap[i]];
                                    obj3 = dataRow[column];
                                }
                                string srcTable = this._tableMapping.SourceTable + this._fieldNames[i];
                                DataReaderContainer dataReader = DataReaderContainer.Create(reader, this._dataReader.ReturnProviderSpecificTypes);
                                num2 += this._adapter.FillFromReader(this._dataSet, null, srcTable, dataReader, 0, 0, column, obj3);
                            }
                        }
                    }
                }
            }
            return num2;
        }

        internal void LoadDataRowWithClear()
        {
            for (int i = 0; i < this._readerDataValues.Length; i++)
            {
                this._readerDataValues[i] = null;
            }
            this.LoadDataRow();
        }

        private void MappedChapter()
        {
            int num2 = this._mappedLength;
            for (int i = 0; i < num2; i++)
            {
                this._mappedDataValues[i] = this._readerDataValues[i];
                if (this._chapterMap[i])
                {
                    this._mappedDataValues[i] = null;
                }
            }
        }

        private void MappedChapterIndex()
        {
            int num3 = this._mappedLength;
            for (int i = 0; i < num3; i++)
            {
                int index = this._indexMap[i];
                if (0 <= index)
                {
                    this._mappedDataValues[index] = this._readerDataValues[i];
                    if (this._chapterMap[i])
                    {
                        this._mappedDataValues[index] = null;
                    }
                }
            }
        }

        private void MappedIndex()
        {
            int num3 = this._mappedLength;
            for (int i = 0; i < num3; i++)
            {
                int index = this._indexMap[i];
                if (0 <= index)
                {
                    this._mappedDataValues[index] = this._readerDataValues[i];
                }
            }
        }

        private void MappedValues()
        {
            int num2 = this._mappedLength;
            for (int i = 0; i < num2; i++)
            {
                this._mappedDataValues[i] = this._readerDataValues[i];
            }
        }

        private DataColumn[] ResizeColumnArray(DataColumn[] rgcol, int len)
        {
            DataColumn[] destinationArray = new DataColumn[len];
            Array.Copy(rgcol, destinationArray, len);
            return destinationArray;
        }

        private void RollbackAddedItems(List<object> items)
        {
            if (items != null)
            {
                for (int i = items.Count - 1; 0 <= i; i--)
                {
                    if (items[i] != null)
                    {
                        DataColumn column = items[i] as DataColumn;
                        if (column != null)
                        {
                            if (column.Table != null)
                            {
                                column.Table.Columns.Remove(column);
                            }
                        }
                        else
                        {
                            System.Data.DataTable table = items[i] as System.Data.DataTable;
                            if ((table != null) && (table.DataSet != null))
                            {
                                table.DataSet.Tables.Remove(table);
                            }
                        }
                    }
                }
            }
        }

        private object[] SetupMapping(int count, DataColumnCollection columnCollection, DataColumn chapterColumn, object chapterValue)
        {
            object[] objArray = new object[count];
            if (this._indexMap == null)
            {
                int num = columnCollection.Count;
                bool flag = null != this._chapterMap;
                if ((count != num) || flag)
                {
                    this._mappedDataValues = new object[num];
                    if (flag)
                    {
                        this._mappedMode = 3;
                        this._mappedLength = count;
                    }
                    else
                    {
                        this._mappedMode = 1;
                        this._mappedLength = Math.Min(count, num);
                    }
                }
                else
                {
                    this._mappedMode = 0;
                }
            }
            else
            {
                this._mappedDataValues = new object[columnCollection.Count];
                this._mappedMode = (this._chapterMap == null) ? 2 : 4;
                this._mappedLength = count;
            }
            if (chapterColumn != null)
            {
                this._mappedDataValues[chapterColumn.Ordinal] = chapterValue;
            }
            return objArray;
        }

        private object[] SetupSchemaWithKeyInfo(MissingMappingAction mappingAction, MissingSchemaAction schemaAction, bool gettingData, DataColumn parentChapterColumn, object chapterValue)
        {
            DbSchemaRow[] sortedSchemaRows = DbSchemaRow.GetSortedSchemaRows(this._schemaTable, this._dataReader.ReturnProviderSpecificTypes);
            if (sortedSchemaRows.Length == 0)
            {
                this._dataTable = null;
                return null;
            }
            bool flag3 = ((this._dataTable.PrimaryKey.Length == 0) && ((((LoadOption) 4) <= this._loadOption) || (this._dataTable.Rows.Count == 0))) || (0 == this._dataTable.Columns.Count);
            DataColumn[] rgcol = null;
            int len = 0;
            bool flag2 = true;
            string str3 = null;
            string str2 = null;
            bool flag6 = false;
            bool flag = false;
            int[] numArray = null;
            bool[] flagArray = null;
            int num3 = 0;
            object[] objArray = null;
            List<object> items = null;
            DataColumnCollection columnCollection = this._dataTable.Columns;
            try
            {
                for (int i = 0; i < sortedSchemaRows.Length; i++)
                {
                    DbSchemaRow row = sortedSchemaRows[i];
                    int unsortedIndex = row.UnsortedIndex;
                    bool flag5 = false;
                    Type dataType = row.DataType;
                    if (null == dataType)
                    {
                        dataType = this._dataReader.GetFieldType(i);
                    }
                    if (null == dataType)
                    {
                        throw ADP.MissingDataReaderFieldType(i);
                    }
                    if (typeof(IDataReader).IsAssignableFrom(dataType))
                    {
                        if (flagArray == null)
                        {
                            flagArray = new bool[sortedSchemaRows.Length];
                        }
                        flagArray[unsortedIndex] = flag5 = true;
                        dataType = typeof(int);
                    }
                    else if (typeof(System.Data.SqlTypes.SqlXml).IsAssignableFrom(dataType))
                    {
                        if (this._xmlMap == null)
                        {
                            this._xmlMap = new int[sortedSchemaRows.Length];
                        }
                        this._xmlMap[i] = 1;
                    }
                    else if (typeof(XmlReader).IsAssignableFrom(dataType))
                    {
                        dataType = typeof(string);
                        if (this._xmlMap == null)
                        {
                            this._xmlMap = new int[sortedSchemaRows.Length];
                        }
                        this._xmlMap[i] = 2;
                    }
                    DataColumn targetColumn = null;
                    if (!row.IsHidden)
                    {
                        targetColumn = this._tableMapping.GetDataColumn(this._fieldNames[i], dataType, this._dataTable, mappingAction, schemaAction);
                    }
                    string baseTableName = row.BaseTableName;
                    if (targetColumn == null)
                    {
                        if (numArray == null)
                        {
                            numArray = this.CreateIndexMap(sortedSchemaRows.Length, unsortedIndex);
                        }
                        numArray[unsortedIndex] = -1;
                        if (row.IsKey && (flag6 || (row.BaseTableName == str3)))
                        {
                            flag3 = false;
                            rgcol = null;
                        }
                    }
                    else
                    {
                        if ((this._xmlMap != null) && (this._xmlMap[i] != 0))
                        {
                            if (typeof(System.Data.SqlTypes.SqlXml) == targetColumn.DataType)
                            {
                                this._xmlMap[i] = 1;
                            }
                            else if (typeof(System.Xml.XmlDocument) == targetColumn.DataType)
                            {
                                this._xmlMap[i] = 2;
                            }
                            else
                            {
                                this._xmlMap[i] = 0;
                                int num7 = 0;
                                for (int j = 0; j < this._xmlMap.Length; j++)
                                {
                                    num7 += this._xmlMap[j];
                                }
                                if (num7 == 0)
                                {
                                    this._xmlMap = null;
                                }
                            }
                        }
                        if (row.IsKey && (baseTableName != str3))
                        {
                            if (str3 == null)
                            {
                                str3 = baseTableName;
                            }
                            else
                            {
                                flag6 = true;
                            }
                        }
                        if (flag5)
                        {
                            if (targetColumn.Table != null)
                            {
                                if (!targetColumn.AutoIncrement)
                                {
                                    throw ADP.FillChapterAutoIncrement();
                                }
                            }
                            else
                            {
                                targetColumn.AllowDBNull = false;
                                targetColumn.AutoIncrement = true;
                                targetColumn.ReadOnly = true;
                            }
                        }
                        else
                        {
                            if ((!flag && (baseTableName != str2)) && !ADP.IsEmpty(baseTableName))
                            {
                                if (str2 == null)
                                {
                                    str2 = baseTableName;
                                }
                                else
                                {
                                    flag = true;
                                }
                            }
                            if (((LoadOption) 4) <= this._loadOption)
                            {
                                if (row.IsAutoIncrement && DataColumn.IsAutoIncrementType(dataType))
                                {
                                    targetColumn.AutoIncrement = true;
                                    if (!row.AllowDBNull)
                                    {
                                        targetColumn.AllowDBNull = false;
                                    }
                                }
                                if (dataType == typeof(string))
                                {
                                    targetColumn.MaxLength = (row.Size > 0) ? row.Size : -1;
                                }
                                if (row.IsReadOnly)
                                {
                                    targetColumn.ReadOnly = true;
                                }
                                if (!row.AllowDBNull && (!row.IsReadOnly || row.IsKey))
                                {
                                    targetColumn.AllowDBNull = false;
                                }
                                if ((row.IsUnique && !row.IsKey) && !dataType.IsArray)
                                {
                                    targetColumn.Unique = true;
                                    if (!row.AllowDBNull)
                                    {
                                        targetColumn.AllowDBNull = false;
                                    }
                                }
                            }
                            else if (targetColumn.Table == null)
                            {
                                targetColumn.AutoIncrement = row.IsAutoIncrement;
                                targetColumn.AllowDBNull = row.AllowDBNull;
                                targetColumn.ReadOnly = row.IsReadOnly;
                                targetColumn.Unique = row.IsUnique;
                                if ((dataType == typeof(string)) || (dataType == typeof(SqlString)))
                                {
                                    targetColumn.MaxLength = row.Size;
                                }
                            }
                        }
                        if (targetColumn.Table == null)
                        {
                            if (((LoadOption) 4) > this._loadOption)
                            {
                                this.AddAdditionalProperties(targetColumn, row.DataRow);
                            }
                            this.AddItemToAllowRollback(ref items, targetColumn);
                            columnCollection.Add(targetColumn);
                        }
                        if (flag3 && row.IsKey)
                        {
                            if (rgcol == null)
                            {
                                rgcol = new DataColumn[sortedSchemaRows.Length];
                            }
                            rgcol[len++] = targetColumn;
                            if (flag2 && targetColumn.AllowDBNull)
                            {
                                flag2 = false;
                            }
                        }
                        if (numArray != null)
                        {
                            numArray[unsortedIndex] = targetColumn.Ordinal;
                        }
                        else if (unsortedIndex != targetColumn.Ordinal)
                        {
                            numArray = this.CreateIndexMap(sortedSchemaRows.Length, unsortedIndex);
                            numArray[unsortedIndex] = targetColumn.Ordinal;
                        }
                        num3++;
                    }
                }
                bool flag4 = false;
                DataColumn column2 = null;
                if (chapterValue != null)
                {
                    Type type = chapterValue.GetType();
                    column2 = this._tableMapping.GetDataColumn(this._tableMapping.SourceTable, type, this._dataTable, mappingAction, schemaAction);
                    if (column2 != null)
                    {
                        if (column2.Table == null)
                        {
                            column2.ReadOnly = true;
                            column2.AllowDBNull = false;
                            this.AddItemToAllowRollback(ref items, column2);
                            columnCollection.Add(column2);
                            flag4 = null != parentChapterColumn;
                        }
                        num3++;
                    }
                }
                if (0 < num3)
                {
                    if ((this._dataSet != null) && (this._dataTable.DataSet == null))
                    {
                        this.AddItemToAllowRollback(ref items, this._dataTable);
                        this._dataSet.Tables.Add(this._dataTable);
                    }
                    if (flag3 && (rgcol != null))
                    {
                        if (len < rgcol.Length)
                        {
                            rgcol = this.ResizeColumnArray(rgcol, len);
                        }
                        if (flag2)
                        {
                            this._dataTable.PrimaryKey = rgcol;
                        }
                        else
                        {
                            UniqueConstraint constraint = new UniqueConstraint("", rgcol);
                            ConstraintCollection constraints = this._dataTable.Constraints;
                            int count = constraints.Count;
                            for (int k = 0; k < count; k++)
                            {
                                if (constraint.Equals(constraints[k]))
                                {
                                    constraint = null;
                                    break;
                                }
                            }
                            if (constraint != null)
                            {
                                constraints.Add(constraint);
                            }
                        }
                    }
                    if ((!flag && !ADP.IsEmpty(str2)) && ADP.IsEmpty(this._dataTable.TableName))
                    {
                        this._dataTable.TableName = str2;
                    }
                    if (gettingData)
                    {
                        this._indexMap = numArray;
                        this._chapterMap = flagArray;
                        objArray = this.SetupMapping(sortedSchemaRows.Length, columnCollection, column2, chapterValue);
                    }
                    else
                    {
                        this._mappedMode = -1;
                    }
                }
                else
                {
                    this._dataTable = null;
                }
                if (flag4)
                {
                    this.AddRelation(parentChapterColumn, column2);
                }
            }
            catch (Exception exception)
            {
                if (ADP.IsCatchableOrSecurityExceptionType(exception))
                {
                    this.RollbackAddedItems(items);
                }
                throw;
            }
            return objArray;
        }

        private object[] SetupSchemaWithoutKeyInfo(MissingMappingAction mappingAction, MissingSchemaAction schemaAction, bool gettingData, DataColumn parentChapterColumn, object chapterValue)
        {
            int[] numArray = null;
            bool[] flagArray = null;
            int num3 = 0;
            int fieldCount = this._dataReader.FieldCount;
            object[] objArray = null;
            List<object> items = null;
            try
            {
                DataColumnCollection columnCollection = this._dataTable.Columns;
                for (int i = 0; i < fieldCount; i++)
                {
                    bool flag = false;
                    Type fieldType = this._dataReader.GetFieldType(i);
                    if (null == fieldType)
                    {
                        throw ADP.MissingDataReaderFieldType(i);
                    }
                    if (typeof(IDataReader).IsAssignableFrom(fieldType))
                    {
                        if (flagArray == null)
                        {
                            flagArray = new bool[fieldCount];
                        }
                        flagArray[i] = flag = true;
                        fieldType = typeof(int);
                    }
                    else if (typeof(System.Data.SqlTypes.SqlXml).IsAssignableFrom(fieldType))
                    {
                        if (this._xmlMap == null)
                        {
                            this._xmlMap = new int[fieldCount];
                        }
                        this._xmlMap[i] = 1;
                    }
                    else if (typeof(XmlReader).IsAssignableFrom(fieldType))
                    {
                        fieldType = typeof(string);
                        if (this._xmlMap == null)
                        {
                            this._xmlMap = new int[fieldCount];
                        }
                        this._xmlMap[i] = 2;
                    }
                    DataColumn column = this._tableMapping.GetDataColumn(this._fieldNames[i], fieldType, this._dataTable, mappingAction, schemaAction);
                    if (column == null)
                    {
                        if (numArray == null)
                        {
                            numArray = this.CreateIndexMap(fieldCount, i);
                        }
                        numArray[i] = -1;
                    }
                    else
                    {
                        if ((this._xmlMap != null) && (this._xmlMap[i] != 0))
                        {
                            if (typeof(System.Data.SqlTypes.SqlXml) == column.DataType)
                            {
                                this._xmlMap[i] = 1;
                            }
                            else if (typeof(System.Xml.XmlDocument) == column.DataType)
                            {
                                this._xmlMap[i] = 2;
                            }
                            else
                            {
                                this._xmlMap[i] = 0;
                                int num5 = 0;
                                for (int j = 0; j < this._xmlMap.Length; j++)
                                {
                                    num5 += this._xmlMap[j];
                                }
                                if (num5 == 0)
                                {
                                    this._xmlMap = null;
                                }
                            }
                        }
                        if (column.Table == null)
                        {
                            if (flag)
                            {
                                column.AllowDBNull = false;
                                column.AutoIncrement = true;
                                column.ReadOnly = true;
                            }
                            this.AddItemToAllowRollback(ref items, column);
                            columnCollection.Add(column);
                        }
                        else if (flag && !column.AutoIncrement)
                        {
                            throw ADP.FillChapterAutoIncrement();
                        }
                        if (numArray != null)
                        {
                            numArray[i] = column.Ordinal;
                        }
                        else if (i != column.Ordinal)
                        {
                            numArray = this.CreateIndexMap(fieldCount, i);
                            numArray[i] = column.Ordinal;
                        }
                        num3++;
                    }
                }
                bool flag2 = false;
                DataColumn column2 = null;
                if (chapterValue != null)
                {
                    Type type = chapterValue.GetType();
                    column2 = this._tableMapping.GetDataColumn(this._tableMapping.SourceTable, type, this._dataTable, mappingAction, schemaAction);
                    if (column2 != null)
                    {
                        if (column2.Table == null)
                        {
                            this.AddItemToAllowRollback(ref items, column2);
                            columnCollection.Add(column2);
                            flag2 = null != parentChapterColumn;
                        }
                        num3++;
                    }
                }
                if (0 < num3)
                {
                    if ((this._dataSet != null) && (this._dataTable.DataSet == null))
                    {
                        this.AddItemToAllowRollback(ref items, this._dataTable);
                        this._dataSet.Tables.Add(this._dataTable);
                    }
                    if (gettingData)
                    {
                        if (columnCollection == null)
                        {
                            columnCollection = this._dataTable.Columns;
                        }
                        this._indexMap = numArray;
                        this._chapterMap = flagArray;
                        objArray = this.SetupMapping(fieldCount, columnCollection, column2, chapterValue);
                    }
                    else
                    {
                        this._mappedMode = -1;
                    }
                }
                else
                {
                    this._dataTable = null;
                }
                if (flag2)
                {
                    this.AddRelation(parentChapterColumn, column2);
                }
            }
            catch (Exception exception)
            {
                if (ADP.IsCatchableOrSecurityExceptionType(exception))
                {
                    this.RollbackAddedItems(items);
                }
                throw;
            }
            return objArray;
        }

        internal DataReaderContainer DataReader
        {
            get
            {
                return this._dataReader;
            }
        }

        internal System.Data.DataTable DataTable
        {
            get
            {
                return this._dataTable;
            }
        }

        internal object[] DataValues
        {
            get
            {
                return this._readerDataValues;
            }
        }
    }
}

