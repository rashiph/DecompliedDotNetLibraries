namespace System.Data
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data.Common;
    using System.Data.SqlTypes;
    using System.Diagnostics;
    using System.Globalization;
    using System.Numerics;
    using System.Reflection;
    using System.Threading;
    using System.Xml;
    using System.Xml.Serialization;

    [DefaultProperty("ColumnName"), ToolboxItem(false), DesignTimeVisible(false), Editor("Microsoft.VSDesigner.Data.Design.DataColumnEditor, Microsoft.VSDesigner, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    public class DataColumn : MarshalByValueComponent
    {
        private string _columnName;
        private string _columnPrefix;
        internal string _columnUri;
        private DataSetDateTime _dateTimeMode;
        internal int _hashCode;
        private readonly int _objectID;
        private static int _objectTypeCount;
        private int _ordinal;
        private DataStorage _storage;
        private bool allowNull;
        private AutoIncrementValue autoInc;
        private string caption;
        internal MappingType columnMapping;
        private Type dataType;
        internal object defaultValue;
        private bool defaultValueIsNull;
        internal List<DataColumn> dependentColumns;
        internal string description;
        internal string dttype;
        internal string encodedColumnName;
        internal int errors;
        private System.Data.DataExpression expression;
        internal PropertyCollection extendedProperties;
        private bool implementsIChangeTracking;
        private bool implementsINullable;
        private bool implementsIRevertibleChangeTracking;
        private bool implementsIXMLSerializable;
        private bool isSqlType;
        private int maxLength;
        private bool readOnly;
        internal System.Data.SimpleType simpleType;
        internal Index sortIndex;
        internal DataTable table;
        private bool unique;

        internal event PropertyChangedEventHandler PropertyChanging;

        public DataColumn() : this(null, typeof(string), null, MappingType.Element)
        {
        }

        public DataColumn(string columnName) : this(columnName, typeof(string), null, MappingType.Element)
        {
        }

        public DataColumn(string columnName, Type dataType) : this(columnName, dataType, null, MappingType.Element)
        {
        }

        public DataColumn(string columnName, Type dataType, string expr) : this(columnName, dataType, expr, MappingType.Element)
        {
        }

        public DataColumn(string columnName, Type dataType, string expr, MappingType type)
        {
            this.allowNull = true;
            this.defaultValue = DBNull.Value;
            this._dateTimeMode = DataSetDateTime.UnspecifiedLocal;
            this.maxLength = -1;
            this._ordinal = -1;
            this.columnMapping = MappingType.Element;
            this.defaultValueIsNull = true;
            this._columnPrefix = "";
            this.description = "";
            this.dttype = "";
            this._objectID = Interlocked.Increment(ref _objectTypeCount);
            GC.SuppressFinalize(this);
            Bid.Trace("<ds.DataColumn.DataColumn|API> %d#, columnName='%ls', expr='%ls', type=%d{ds.MappingType}\n", this.ObjectID, columnName, expr, (int) type);
            if (dataType == null)
            {
                throw ExceptionBuilder.ArgumentNull("dataType");
            }
            StorageType storageType = DataStorage.GetStorageType(dataType);
            if (DataStorage.ImplementsINullableValue(storageType, dataType))
            {
                throw ExceptionBuilder.ColumnTypeNotSupported();
            }
            this._columnName = (columnName == null) ? "" : columnName;
            System.Data.SimpleType type2 = System.Data.SimpleType.CreateSimpleType(dataType);
            if (type2 != null)
            {
                this.SimpleType = type2;
            }
            this.UpdateColumnType(dataType, storageType);
            if ((expr != null) && (0 < expr.Length))
            {
                this.Expression = expr;
            }
            this.columnMapping = type;
        }

        internal void AddDependentColumn(DataColumn expressionColumn)
        {
            if (this.dependentColumns == null)
            {
                this.dependentColumns = new List<DataColumn>();
            }
            this.dependentColumns.Add(expressionColumn);
            this.table.AddDependentColumn(expressionColumn);
        }

        internal void BindExpression()
        {
            this.DataExpression.Bind(this.table);
        }

        internal void CheckColumnConstraint(DataRow row, DataRowAction action)
        {
            if (this.table.UpdatingCurrent(row, action))
            {
                this.CheckNullable(row);
                this.CheckMaxLength(row);
            }
        }

        internal bool CheckMaxLength()
        {
            if (((0 <= this.maxLength) && (this.Table != null)) && (0 < this.Table.Rows.Count))
            {
                foreach (DataRow row in this.Table.Rows)
                {
                    if (row.HasVersion(DataRowVersion.Current) && (this.maxLength < this.GetStringLength(row.GetCurrentRecordNo())))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        internal void CheckMaxLength(DataRow dr)
        {
            if ((0 <= this.maxLength) && (this.maxLength < this.GetStringLength(dr.GetDefaultRecord())))
            {
                throw ExceptionBuilder.LongerThanMaxLength(this);
            }
        }

        protected internal void CheckNotAllowNull()
        {
            if (this._storage != null)
            {
                if (this.sortIndex != null)
                {
                    if (this.sortIndex.IsKeyInIndex(this._storage.NullValue))
                    {
                        throw ExceptionBuilder.NullKeyValues(this.ColumnName);
                    }
                }
                else
                {
                    foreach (DataRow row in this.table.Rows)
                    {
                        if (row.RowState != DataRowState.Deleted)
                        {
                            if (!this.implementsINullable)
                            {
                                if (row[this] == DBNull.Value)
                                {
                                    throw ExceptionBuilder.NullKeyValues(this.ColumnName);
                                }
                            }
                            else if (DataStorage.IsObjectNull(row[this]))
                            {
                                throw ExceptionBuilder.NullKeyValues(this.ColumnName);
                            }
                        }
                    }
                }
            }
        }

        internal void CheckNullable(DataRow row)
        {
            if (!this.AllowDBNull && this._storage.IsNull(row.GetDefaultRecord()))
            {
                throw ExceptionBuilder.NullValues(this.ColumnName);
            }
        }

        protected void CheckUnique()
        {
            if (!this.SortIndex.CheckUnique())
            {
                throw ExceptionBuilder.NonUniqueValues(this.ColumnName);
            }
        }

        internal DataColumn Clone()
        {
            DataColumn column = (DataColumn) Activator.CreateInstance(base.GetType());
            column.SimpleType = this.SimpleType;
            column.allowNull = this.allowNull;
            if (this.autoInc != null)
            {
                column.autoInc = this.autoInc.Clone();
            }
            column.caption = this.caption;
            column.ColumnName = this.ColumnName;
            column._columnUri = this._columnUri;
            column._columnPrefix = this._columnPrefix;
            column.DataType = this.DataType;
            column.defaultValue = this.defaultValue;
            column.defaultValueIsNull = (this.defaultValue == DBNull.Value) || (column.ImplementsINullable && DataStorage.IsObjectSqlNull(this.defaultValue));
            column.columnMapping = this.columnMapping;
            column.readOnly = this.readOnly;
            column.MaxLength = this.MaxLength;
            column.dttype = this.dttype;
            column._dateTimeMode = this._dateTimeMode;
            if (this.extendedProperties != null)
            {
                foreach (object obj2 in this.extendedProperties.Keys)
                {
                    column.ExtendedProperties[obj2] = this.extendedProperties[obj2];
                }
            }
            return column;
        }

        internal int Compare(int record1, int record2)
        {
            return this._storage.Compare(record1, record2);
        }

        internal int CompareValueTo(int record1, object value)
        {
            return this._storage.CompareValueTo(record1, value);
        }

        internal bool CompareValueTo(int record1, object value, bool checkType)
        {
            if (this.CompareValueTo(record1, value) == 0)
            {
                Type type2 = value.GetType();
                Type type = this._storage.Get(record1).GetType();
                if ((type2 == typeof(string)) && (type == typeof(string)))
                {
                    if (string.CompareOrdinal((string) this._storage.Get(record1), (string) value) != 0)
                    {
                        return false;
                    }
                    return true;
                }
                if (type2 == type)
                {
                    return true;
                }
            }
            return false;
        }

        internal string ConvertObjectToXml(object value)
        {
            this.InsureStorage();
            return this._storage.ConvertObjectToXml(value);
        }

        internal void ConvertObjectToXml(object value, XmlWriter xmlWriter, XmlRootAttribute xmlAttrib)
        {
            this.InsureStorage();
            this._storage.ConvertObjectToXml(value, xmlWriter, xmlAttrib);
        }

        internal object ConvertValue(object value)
        {
            return this._storage.ConvertValue(value);
        }

        internal object ConvertXmlToObject(string s)
        {
            this.InsureStorage();
            return this._storage.ConvertXmlToObject(s);
        }

        internal object ConvertXmlToObject(XmlReader xmlReader, XmlRootAttribute xmlAttrib)
        {
            this.InsureStorage();
            return this._storage.ConvertXmlToObject(xmlReader, xmlAttrib);
        }

        internal void Copy(int srcRecordNo, int dstRecordNo)
        {
            this._storage.Copy(srcRecordNo, dstRecordNo);
        }

        internal void CopyValueIntoStore(int record, object store, BitArray nullbits, int storeIndex)
        {
            this._storage.CopyValueInternal(record, store, nullbits, storeIndex);
        }

        internal void Description(string value)
        {
            if (value == null)
            {
                value = "";
            }
            this.description = value;
        }

        internal DataRelation FindParentRelation()
        {
            DataRelation[] array = new DataRelation[this.Table.ParentRelations.Count];
            this.Table.ParentRelations.CopyTo(array, 0);
            for (int i = 0; i < array.Length; i++)
            {
                DataRelation relation = array[i];
                DataKey childKey = relation.ChildKey;
                if ((childKey.ColumnsReference.Length == 1) && (childKey.ColumnsReference[0] == this))
                {
                    return relation;
                }
            }
            return null;
        }

        internal void FinishInitInProgress()
        {
            if (this.Computed)
            {
                this.BindExpression();
            }
        }

        internal void FreeRecord(int record)
        {
            this._storage.Set(record, this._storage.NullValue);
        }

        internal object GetAggregateValue(int[] records, AggregateType kind)
        {
            if (this._storage != null)
            {
                return this._storage.Aggregate(records, kind);
            }
            if (kind == AggregateType.Count)
            {
                return 0;
            }
            return DBNull.Value;
        }

        internal string GetColumnValueAsString(DataRow row, DataRowVersion version)
        {
            object obj2 = this[row.GetRecordFromVersion(version)];
            if (DataStorage.IsObjectNull(obj2))
            {
                return null;
            }
            return this.ConvertObjectToXml(obj2);
        }

        private DataRow GetDataRow(int index)
        {
            return this.table.recordManager[index];
        }

        internal object GetEmptyColumnStore(int recordCount)
        {
            this.InsureStorage();
            return this._storage.GetEmptyStorageInternal(recordCount);
        }

        private int GetStringLength(int record)
        {
            return this._storage.GetStringLength(record);
        }

        internal void HandleDependentColumnList(System.Data.DataExpression oldExpression, System.Data.DataExpression newExpression)
        {
            if (oldExpression != null)
            {
                foreach (DataColumn column2 in oldExpression.GetDependency())
                {
                    column2.RemoveDependentColumn(this);
                    if (column2.table != this.table)
                    {
                        this.table.RemoveDependentColumn(this);
                    }
                }
                this.table.RemoveDependentColumn(this);
            }
            if (newExpression != null)
            {
                foreach (DataColumn column in newExpression.GetDependency())
                {
                    column.AddDependentColumn(this);
                    if (column.table != this.table)
                    {
                        this.table.AddDependentColumn(this);
                    }
                }
                this.table.AddDependentColumn(this);
            }
        }

        internal void Init(int record)
        {
            if (this.AutoIncrement)
            {
                object current = this.autoInc.Current;
                this.autoInc.MoveAfter();
                this._storage.Set(record, current);
            }
            else
            {
                this[record] = this.defaultValue;
            }
        }

        internal void InitializeRecord(int record)
        {
            this._storage.Set(record, this.DefaultValue);
        }

        private void InsureStorage()
        {
            if (this._storage == null)
            {
                this._storage = DataStorage.CreateStorage(this, this.dataType);
            }
        }

        internal void InternalUnique(bool value)
        {
            this.unique = value;
        }

        internal static bool IsAutoIncrementType(Type dataType)
        {
            if (((!(dataType == typeof(int)) && !(dataType == typeof(long))) && (!(dataType == typeof(short)) && !(dataType == typeof(decimal)))) && ((!(dataType == typeof(BigInteger)) && !(dataType == typeof(SqlInt32))) && (!(dataType == typeof(SqlInt64)) && !(dataType == typeof(SqlInt16)))))
            {
                return (dataType == typeof(SqlDecimal));
            }
            return true;
        }

        private bool IsColumnMappingValid(StorageType typeCode, MappingType mapping)
        {
            if ((mapping != MappingType.Element) && DataStorage.IsTypeCustomType(typeCode))
            {
                return false;
            }
            return true;
        }

        internal bool IsInRelation()
        {
            DataRelationCollection parentRelations = this.table.ParentRelations;
            for (int i = 0; i < parentRelations.Count; i++)
            {
                if (parentRelations[i].ChildKey.ContainsColumn(this))
                {
                    return true;
                }
            }
            parentRelations = this.table.ChildRelations;
            for (int j = 0; j < parentRelations.Count; j++)
            {
                if (parentRelations[j].ParentKey.ContainsColumn(this))
                {
                    return true;
                }
            }
            return false;
        }

        internal bool IsMaxLengthViolated()
        {
            if (this.MaxLength < 0)
            {
                return true;
            }
            bool flag = false;
            string error = null;
            foreach (DataRow row in this.Table.Rows)
            {
                if (row.HasVersion(DataRowVersion.Current))
                {
                    object obj2 = row[this];
                    if (!this.isSqlType)
                    {
                        if (((obj2 != null) && (obj2 != DBNull.Value)) && (((string) obj2).Length > this.MaxLength))
                        {
                            if (error == null)
                            {
                                error = ExceptionBuilder.MaxLengthViolationText(this.ColumnName);
                            }
                            row.RowError = error;
                            row.SetColumnError(this, error);
                            flag = true;
                        }
                    }
                    else if (!DataStorage.IsObjectNull(obj2))
                    {
                        SqlString str2 = (SqlString) obj2;
                        if (str2.Value.Length > this.MaxLength)
                        {
                            if (error == null)
                            {
                                error = ExceptionBuilder.MaxLengthViolationText(this.ColumnName);
                            }
                            row.RowError = error;
                            row.SetColumnError(this, error);
                            flag = true;
                        }
                    }
                }
            }
            return flag;
        }

        internal bool IsNotAllowDBNullViolated()
        {
            Index sortIndex = this.SortIndex;
            DataRow[] rows = sortIndex.GetRows(sortIndex.FindRecords(DBNull.Value));
            for (int i = 0; i < rows.Length; i++)
            {
                string error = ExceptionBuilder.NotAllowDBNullViolationText(this.ColumnName);
                rows[i].RowError = error;
                rows[i].SetColumnError(this, error);
            }
            return (rows.Length > 0);
        }

        internal bool IsNull(int record)
        {
            return this._storage.IsNull(record);
        }

        internal bool IsValueCustomTypeInstance(object value)
        {
            return (DataStorage.IsTypeCustomType(value.GetType()) && !(value is Type));
        }

        protected virtual void OnPropertyChanging(PropertyChangedEventArgs pcevent)
        {
            if (this.onPropertyChangingDelegate != null)
            {
                this.onPropertyChangingDelegate(this, pcevent);
            }
        }

        internal void OnSetDataSet()
        {
        }

        protected internal void RaisePropertyChanging(string name)
        {
            this.OnPropertyChanging(new PropertyChangedEventArgs(name));
        }

        internal void RemoveDependentColumn(DataColumn expressionColumn)
        {
            if ((this.dependentColumns != null) && this.dependentColumns.Contains(expressionColumn))
            {
                this.dependentColumns.Remove(expressionColumn);
            }
            this.table.RemoveDependentColumn(expressionColumn);
        }

        private void ResetCaption()
        {
            if (this.caption != null)
            {
                this.caption = null;
            }
        }

        private void ResetNamespace()
        {
            this.Namespace = null;
        }

        internal void SetCapacity(int capacity)
        {
            this.InsureStorage();
            this._storage.SetCapacity(capacity);
        }

        private void SetMaxLengthSimpleType()
        {
            if (this.simpleType != null)
            {
                this.simpleType.MaxLength = this.maxLength;
                if (this.simpleType.IsPlainString())
                {
                    this.simpleType = null;
                }
                else if ((this.simpleType.Name != null) && (this.dttype != null))
                {
                    this.simpleType.ConvertToAnnonymousSimpleType();
                    this.dttype = null;
                }
            }
            else if (-1 < this.maxLength)
            {
                this.SimpleType = System.Data.SimpleType.CreateLimitedStringType(this.maxLength);
            }
        }

        public void SetOrdinal(int ordinal)
        {
            if (this._ordinal == -1)
            {
                throw ExceptionBuilder.ColumnNotInAnyTable();
            }
            if (this._ordinal != ordinal)
            {
                this.table.Columns.MoveTo(this, ordinal);
            }
        }

        internal void SetOrdinalInternal(int ordinal)
        {
            if (this._ordinal != ordinal)
            {
                if ((this.Unique && (this._ordinal != -1)) && (ordinal == -1))
                {
                    UniqueConstraint constraint = this.table.Constraints.FindKeyConstraint(this);
                    if (constraint != null)
                    {
                        this.table.Constraints.Remove(constraint);
                    }
                }
                if ((this.sortIndex != null) && (-1 == ordinal))
                {
                    this.sortIndex.RemoveRef();
                    this.sortIndex.RemoveRef();
                    this.sortIndex = null;
                }
                int num = this._ordinal;
                this._ordinal = ordinal;
                if (((num == -1) && (this._ordinal != -1)) && this.Unique)
                {
                    UniqueConstraint constraint2 = new UniqueConstraint(this);
                    this.table.Constraints.Add(constraint2);
                }
            }
        }

        internal void SetStorage(object store, BitArray nullbits)
        {
            this.InsureStorage();
            this._storage.SetStorageInternal(store, nullbits);
        }

        internal void SetTable(DataTable table)
        {
            if (this.table != table)
            {
                if (this.Computed && ((table == null) || (!table.fInitInProgress && ((table.DataSet == null) || (!table.DataSet.fIsSchemaLoading && !table.DataSet.fInitInProgress)))))
                {
                    this.DataExpression.Bind(table);
                }
                if (this.Unique && (this.table != null))
                {
                    UniqueConstraint constraint = table.Constraints.FindKeyConstraint(this);
                    if (constraint != null)
                    {
                        table.Constraints.CanRemove(constraint, true);
                    }
                }
                this.table = table;
                this._storage = null;
            }
        }

        internal void SetValue(int record, object value)
        {
            try
            {
                this._storage.Set(record, value);
            }
            catch (Exception exception)
            {
                ExceptionBuilder.TraceExceptionForCapture(exception);
                throw ExceptionBuilder.SetFailed(value, this, this.DataType, exception);
            }
            DataRow dataRow = this.GetDataRow(record);
            if (dataRow != null)
            {
                dataRow.LastChangedColumn = this;
            }
        }

        private bool ShouldSerializeCaption()
        {
            return (this.caption != null);
        }

        private bool ShouldSerializeDefaultValue()
        {
            return !this.DefaultValueIsNull;
        }

        private bool ShouldSerializeNamespace()
        {
            return (this._columnUri != null);
        }

        public override string ToString()
        {
            if (this.expression == null)
            {
                return this.ColumnName;
            }
            return (this.ColumnName + " + " + this.Expression);
        }

        private void UpdateColumnType(Type type, StorageType typeCode)
        {
            this.dataType = type;
            if (StorageType.DateTime != typeCode)
            {
                this._dateTimeMode = DataSetDateTime.UnspecifiedLocal;
            }
            DataStorage.ImplementsInterfaces(typeCode, type, out this.isSqlType, out this.implementsINullable, out this.implementsIXMLSerializable, out this.implementsIChangeTracking, out this.implementsIRevertibleChangeTracking);
            if (!this.isSqlType && this.implementsINullable)
            {
                SqlUdtStorage.GetStaticNullForUdtType(type);
            }
        }

        [System.Data.ResDescription("DataColumnAllowNullDescr"), DefaultValue(true), System.Data.ResCategory("DataCategory_Data")]
        public bool AllowDBNull
        {
            get
            {
                return this.allowNull;
            }
            set
            {
                IntPtr ptr;
                Bid.ScopeEnter(out ptr, "<ds.DataColumn.set_AllowDBNull|API> %d#, %d{bool}\n", this.ObjectID, value);
                try
                {
                    if (this.allowNull != value)
                    {
                        if (((this.table != null) && !value) && this.table.EnforceConstraints)
                        {
                            this.CheckNotAllowNull();
                        }
                        this.allowNull = value;
                    }
                }
                finally
                {
                    Bid.ScopeLeave(ref ptr);
                }
            }
        }

        internal AutoIncrementValue AutoInc
        {
            get
            {
                return (this.autoInc ?? (this.autoInc = (this.DataType == typeof(BigInteger)) ? ((AutoIncrementValue) new AutoIncrementBigInteger()) : ((AutoIncrementValue) new AutoIncrementInt64())));
            }
        }

        [RefreshProperties(RefreshProperties.All), System.Data.ResCategory("DataCategory_Data"), System.Data.ResDescription("DataColumnAutoIncrementDescr"), DefaultValue(false)]
        public bool AutoIncrement
        {
            get
            {
                return ((this.autoInc != null) && this.autoInc.Auto);
            }
            set
            {
                Bid.Trace("<ds.DataColumn.set_AutoIncrement|API> %d#, %d{bool}\n", this.ObjectID, value);
                if (this.AutoIncrement != value)
                {
                    if (value)
                    {
                        if (this.expression != null)
                        {
                            throw ExceptionBuilder.AutoIncrementAndExpression();
                        }
                        if (!this.DefaultValueIsNull)
                        {
                            throw ExceptionBuilder.AutoIncrementAndDefaultValue();
                        }
                        if (!IsAutoIncrementType(this.DataType))
                        {
                            if (this.HasData)
                            {
                                throw ExceptionBuilder.AutoIncrementCannotSetIfHasData(this.DataType.Name);
                            }
                            this.DataType = typeof(int);
                        }
                    }
                    this.AutoInc.Auto = value;
                }
            }
        }

        internal object AutoIncrementCurrent
        {
            get
            {
                if (this.autoInc == null)
                {
                    return this.AutoIncrementSeed;
                }
                return this.autoInc.Current;
            }
            set
            {
                if (this.AutoIncrementSeed != BigIntegerStorage.ConvertToBigInteger(value, this.FormatProvider))
                {
                    this.AutoInc.SetCurrent(value, this.FormatProvider);
                }
            }
        }

        [System.Data.ResDescription("DataColumnAutoIncrementSeedDescr"), System.Data.ResCategory("DataCategory_Data"), DefaultValue((long) 0L)]
        public long AutoIncrementSeed
        {
            get
            {
                if (this.autoInc == null)
                {
                    return 0L;
                }
                return this.autoInc.Seed;
            }
            set
            {
                Bid.Trace("<ds.DataColumn.set_AutoIncrementSeed|API> %d#, %I64d\n", this.ObjectID, value);
                if (this.AutoIncrementSeed != value)
                {
                    this.AutoInc.Seed = value;
                }
            }
        }

        [System.Data.ResCategory("DataCategory_Data"), System.Data.ResDescription("DataColumnAutoIncrementStepDescr"), DefaultValue((long) 1L)]
        public long AutoIncrementStep
        {
            get
            {
                if (this.autoInc == null)
                {
                    return 1L;
                }
                return this.autoInc.Step;
            }
            set
            {
                Bid.Trace("<ds.DataColumn.set_AutoIncrementStep|API> %d#, %I64d\n", this.ObjectID, value);
                if (this.AutoIncrementStep != value)
                {
                    this.AutoInc.Step = value;
                }
            }
        }

        [System.Data.ResCategory("DataCategory_Data"), System.Data.ResDescription("DataColumnCaptionDescr")]
        public string Caption
        {
            get
            {
                if (this.caption == null)
                {
                    return this._columnName;
                }
                return this.caption;
            }
            set
            {
                if (value == null)
                {
                    value = "";
                }
                if ((this.caption == null) || (string.Compare(this.caption, value, true, this.Locale) != 0))
                {
                    this.caption = value;
                }
            }
        }

        [System.Data.ResDescription("DataColumnMappingDescr"), DefaultValue(1)]
        public virtual MappingType ColumnMapping
        {
            get
            {
                return this.columnMapping;
            }
            set
            {
                Bid.Trace("<ds.DataColumn.set_ColumnMapping|API> %d#, %d{ds.MappingType}\n", this.ObjectID, (int) value);
                if (value != this.columnMapping)
                {
                    if ((value == MappingType.SimpleContent) && (this.table != null))
                    {
                        int num = 0;
                        if (this.columnMapping == MappingType.Element)
                        {
                            num = 1;
                        }
                        if (this.dataType == typeof(char))
                        {
                            throw ExceptionBuilder.CannotSetSimpleContent(this.ColumnName, this.dataType);
                        }
                        if ((this.table.XmlText != null) && (this.table.XmlText != this))
                        {
                            throw ExceptionBuilder.CannotAddColumn3();
                        }
                        if (this.table.ElementColumnCount > num)
                        {
                            throw ExceptionBuilder.CannotAddColumn4(this.ColumnName);
                        }
                    }
                    this.RaisePropertyChanging("ColumnMapping");
                    if (this.table != null)
                    {
                        if (this.columnMapping == MappingType.SimpleContent)
                        {
                            this.table.xmlText = null;
                        }
                        if (value == MappingType.Element)
                        {
                            this.table.ElementColumnCount++;
                        }
                        else if (this.columnMapping == MappingType.Element)
                        {
                            this.table.ElementColumnCount--;
                        }
                    }
                    this.columnMapping = value;
                    if (value == MappingType.SimpleContent)
                    {
                        this._columnUri = null;
                        if (this.table != null)
                        {
                            this.table.XmlText = this;
                        }
                        this.SimpleType = null;
                    }
                }
            }
        }

        [DefaultValue(""), System.Data.ResDescription("DataColumnColumnNameDescr"), RefreshProperties(RefreshProperties.All), System.Data.ResCategory("DataCategory_Data")]
        public string ColumnName
        {
            get
            {
                return this._columnName;
            }
            set
            {
                IntPtr ptr;
                Bid.ScopeEnter(out ptr, "<ds.DataColumn.set_ColumnName|API> %d#, '%ls'\n", this.ObjectID, value);
                try
                {
                    if (value == null)
                    {
                        value = "";
                    }
                    if (string.Compare(this._columnName, value, true, this.Locale) != 0)
                    {
                        if (this.table != null)
                        {
                            if (value.Length == 0)
                            {
                                throw ExceptionBuilder.ColumnNameRequired();
                            }
                            this.table.Columns.RegisterColumnName(value, this, null);
                            if (this._columnName.Length != 0)
                            {
                                this.table.Columns.UnregisterName(this._columnName);
                            }
                        }
                        this.RaisePropertyChanging("ColumnName");
                        this._columnName = value;
                        this.encodedColumnName = null;
                        if (this.table != null)
                        {
                            this.table.Columns.OnColumnPropertyChanged(new CollectionChangeEventArgs(CollectionChangeAction.Refresh, this));
                        }
                    }
                    else if (this._columnName != value)
                    {
                        this.RaisePropertyChanging("ColumnName");
                        this._columnName = value;
                        this.encodedColumnName = null;
                        if (this.table != null)
                        {
                            this.table.Columns.OnColumnPropertyChanged(new CollectionChangeEventArgs(CollectionChangeAction.Refresh, this));
                        }
                    }
                }
                finally
                {
                    Bid.ScopeLeave(ref ptr);
                }
            }
        }

        internal bool Computed
        {
            get
            {
                return (this.expression != null);
            }
        }

        internal System.Data.DataExpression DataExpression
        {
            get
            {
                return this.expression;
            }
        }

        [TypeConverter(typeof(ColumnTypeConverter)), System.Data.ResCategory("DataCategory_Data"), System.Data.ResDescription("DataColumnDataTypeDescr"), DefaultValue(typeof(string)), RefreshProperties(RefreshProperties.All)]
        public Type DataType
        {
            get
            {
                return this.dataType;
            }
            set
            {
                if (this.dataType != value)
                {
                    if (this.HasData)
                    {
                        throw ExceptionBuilder.CantChangeDataType();
                    }
                    if (value == null)
                    {
                        throw ExceptionBuilder.NullDataType();
                    }
                    StorageType storageType = DataStorage.GetStorageType(value);
                    if (DataStorage.ImplementsINullableValue(storageType, value))
                    {
                        throw ExceptionBuilder.ColumnTypeNotSupported();
                    }
                    if ((this.table != null) && this.IsInRelation())
                    {
                        throw ExceptionBuilder.ColumnsTypeMismatch();
                    }
                    if ((storageType == StorageType.BigInteger) && (this.expression != null))
                    {
                        throw ExprException.UnsupportedDataType(value);
                    }
                    if (!this.DefaultValueIsNull)
                    {
                        try
                        {
                            if (this.defaultValue is BigInteger)
                            {
                                this.defaultValue = BigIntegerStorage.ConvertFromBigInteger((BigInteger) this.defaultValue, value, this.FormatProvider);
                            }
                            else if (typeof(BigInteger) == value)
                            {
                                this.defaultValue = BigIntegerStorage.ConvertToBigInteger(this.defaultValue, this.FormatProvider);
                            }
                            else if (typeof(string) == value)
                            {
                                this.defaultValue = this.DefaultValue.ToString();
                            }
                            else if (typeof(SqlString) == value)
                            {
                                this.defaultValue = SqlConvert.ConvertToSqlString(this.DefaultValue);
                            }
                            else if (typeof(object) != value)
                            {
                                this.DefaultValue = SqlConvert.ChangeTypeForDefaultValue(this.DefaultValue, value, this.FormatProvider);
                            }
                        }
                        catch (InvalidCastException exception2)
                        {
                            throw ExceptionBuilder.DefaultValueDataType(this.ColumnName, this.DefaultValue.GetType(), value, exception2);
                        }
                        catch (FormatException exception)
                        {
                            throw ExceptionBuilder.DefaultValueDataType(this.ColumnName, this.DefaultValue.GetType(), value, exception);
                        }
                    }
                    if ((this.ColumnMapping == MappingType.SimpleContent) && (value == typeof(char)))
                    {
                        throw ExceptionBuilder.CannotSetSimpleContentType(this.ColumnName, value);
                    }
                    this.SimpleType = System.Data.SimpleType.CreateSimpleType(value);
                    if (StorageType.String == storageType)
                    {
                        this.maxLength = -1;
                    }
                    this.UpdateColumnType(value, storageType);
                    this.XmlDataType = null;
                    if (this.AutoIncrement)
                    {
                        if (!IsAutoIncrementType(value))
                        {
                            this.AutoIncrement = false;
                        }
                        if (this.autoInc != null)
                        {
                            AutoIncrementValue autoInc = this.autoInc;
                            this.autoInc = null;
                            this.AutoInc.Auto = autoInc.Auto;
                            this.AutoInc.Seed = autoInc.Seed;
                            this.AutoInc.Step = autoInc.Step;
                            if (this.autoInc.DataType == autoInc.DataType)
                            {
                                this.autoInc.Current = autoInc.Current;
                            }
                            else if (autoInc.DataType == typeof(long))
                            {
                                this.AutoInc.Current = (long) autoInc.Current;
                            }
                            else
                            {
                                this.AutoInc.Current = (long) ((BigInteger) autoInc.Current);
                            }
                        }
                    }
                }
            }
        }

        [System.Data.ResDescription("DataColumnDateTimeModeDescr"), DefaultValue(3), System.Data.ResCategory("DataCategory_Data"), RefreshProperties(RefreshProperties.All)]
        public DataSetDateTime DateTimeMode
        {
            get
            {
                return this._dateTimeMode;
            }
            set
            {
                if (this._dateTimeMode != value)
                {
                    if ((this.DataType != typeof(DateTime)) && (value != DataSetDateTime.UnspecifiedLocal))
                    {
                        throw ExceptionBuilder.CannotSetDateTimeModeForNonDateTimeColumns();
                    }
                    switch (value)
                    {
                        case DataSetDateTime.Local:
                        case DataSetDateTime.Utc:
                            if (this.HasData)
                            {
                                throw ExceptionBuilder.CantChangeDateTimeMode(this._dateTimeMode, value);
                            }
                            break;

                        case DataSetDateTime.Unspecified:
                        case DataSetDateTime.UnspecifiedLocal:
                            if (((this._dateTimeMode != DataSetDateTime.Unspecified) && (this._dateTimeMode != DataSetDateTime.UnspecifiedLocal)) && this.HasData)
                            {
                                throw ExceptionBuilder.CantChangeDateTimeMode(this._dateTimeMode, value);
                            }
                            break;

                        default:
                            throw ExceptionBuilder.InvalidDateTimeMode(value);
                    }
                    this._dateTimeMode = value;
                }
            }
        }

        [System.Data.ResCategory("DataCategory_Data"), TypeConverter(typeof(DefaultValueTypeConverter)), System.Data.ResDescription("DataColumnDefaultValueDescr")]
        public object DefaultValue
        {
            get
            {
                if ((this.defaultValue == DBNull.Value) && this.implementsINullable)
                {
                    if (this._storage != null)
                    {
                        this.defaultValue = this._storage.NullValue;
                    }
                    else if (this.isSqlType)
                    {
                        this.defaultValue = SqlConvert.ChangeTypeForDefaultValue(this.defaultValue, this.dataType, this.FormatProvider);
                    }
                    else if (this.implementsINullable)
                    {
                        PropertyInfo property = this.dataType.GetProperty("Null", BindingFlags.Public | BindingFlags.Static);
                        if (property != null)
                        {
                            this.defaultValue = property.GetValue(null, null);
                        }
                    }
                }
                return this.defaultValue;
            }
            set
            {
                Bid.Trace("<ds.DataColumn.set_DefaultValue|API> %d#\n", this.ObjectID);
                if ((this.defaultValue == null) || !this.DefaultValue.Equals(value))
                {
                    if (this.AutoIncrement)
                    {
                        throw ExceptionBuilder.DefaultValueAndAutoIncrement();
                    }
                    object obj2 = (value == null) ? DBNull.Value : value;
                    if ((obj2 != DBNull.Value) && (this.DataType != typeof(object)))
                    {
                        try
                        {
                            obj2 = SqlConvert.ChangeTypeForDefaultValue(obj2, this.DataType, this.FormatProvider);
                        }
                        catch (InvalidCastException exception)
                        {
                            throw ExceptionBuilder.DefaultValueColumnDataType(this.ColumnName, obj2.GetType(), this.DataType, exception);
                        }
                    }
                    this.defaultValue = obj2;
                    this.defaultValueIsNull = (obj2 == DBNull.Value) || (this.ImplementsINullable && DataStorage.IsObjectSqlNull(obj2));
                }
            }
        }

        internal bool DefaultValueIsNull
        {
            get
            {
                return this.defaultValueIsNull;
            }
        }

        internal string EncodedColumnName
        {
            get
            {
                if (this.encodedColumnName == null)
                {
                    this.encodedColumnName = XmlConvert.EncodeLocalName(this.ColumnName);
                }
                return this.encodedColumnName;
            }
        }

        [DefaultValue(""), System.Data.ResCategory("DataCategory_Data"), RefreshProperties(RefreshProperties.All), System.Data.ResDescription("DataColumnExpressionDescr")]
        public string Expression
        {
            get
            {
                if (this.expression != null)
                {
                    return this.expression.Expression;
                }
                return "";
            }
            set
            {
                IntPtr ptr;
                Bid.ScopeEnter(out ptr, "<ds.DataColumn.set_Expression|API> %d#, '%ls'\n", this.ObjectID, value);
                if (value == null)
                {
                    value = "";
                }
                try
                {
                    System.Data.DataExpression newExpression = null;
                    if (value.Length > 0)
                    {
                        System.Data.DataExpression expression3 = new System.Data.DataExpression(this.table, value, this.dataType);
                        if (expression3.HasValue)
                        {
                            newExpression = expression3;
                        }
                    }
                    if ((this.expression == null) && (newExpression != null))
                    {
                        if (this.AutoIncrement || this.Unique)
                        {
                            throw ExceptionBuilder.ExpressionAndUnique();
                        }
                        if (this.table != null)
                        {
                            for (int i = 0; i < this.table.Constraints.Count; i++)
                            {
                                if (this.table.Constraints[i].ContainsColumn(this))
                                {
                                    throw ExceptionBuilder.ExpressionAndConstraint(this, this.table.Constraints[i]);
                                }
                            }
                        }
                        bool readOnly = this.ReadOnly;
                        try
                        {
                            this.ReadOnly = true;
                        }
                        catch (ReadOnlyException exception3)
                        {
                            ExceptionBuilder.TraceExceptionForCapture(exception3);
                            this.ReadOnly = readOnly;
                            throw ExceptionBuilder.ExpressionAndReadOnly();
                        }
                    }
                    if (this.table != null)
                    {
                        if ((newExpression != null) && newExpression.DependsOn(this))
                        {
                            throw ExceptionBuilder.ExpressionCircular();
                        }
                        this.HandleDependentColumnList(this.expression, newExpression);
                        System.Data.DataExpression expression = this.expression;
                        this.expression = newExpression;
                        try
                        {
                            if (newExpression == null)
                            {
                                for (int j = 0; j < this.table.RecordCapacity; j++)
                                {
                                    this.InitializeRecord(j);
                                }
                            }
                            else
                            {
                                this.table.EvaluateExpressions(this);
                            }
                            this.table.ResetInternalIndexes(this);
                            this.table.EvaluateDependentExpressions(this);
                            return;
                        }
                        catch (Exception exception2)
                        {
                            if (!ADP.IsCatchableExceptionType(exception2))
                            {
                                throw;
                            }
                            ExceptionBuilder.TraceExceptionForCapture(exception2);
                            try
                            {
                                this.expression = expression;
                                this.HandleDependentColumnList(newExpression, this.expression);
                                if (expression == null)
                                {
                                    for (int k = 0; k < this.table.RecordCapacity; k++)
                                    {
                                        this.InitializeRecord(k);
                                    }
                                }
                                else
                                {
                                    this.table.EvaluateExpressions(this);
                                }
                                this.table.ResetInternalIndexes(this);
                                this.table.EvaluateDependentExpressions(this);
                            }
                            catch (Exception exception)
                            {
                                if (!ADP.IsCatchableExceptionType(exception))
                                {
                                    throw;
                                }
                                ExceptionBuilder.TraceExceptionWithoutRethrow(exception);
                            }
                            throw;
                        }
                    }
                    this.expression = newExpression;
                }
                finally
                {
                    Bid.ScopeLeave(ref ptr);
                }
            }
        }

        [System.Data.ResCategory("DataCategory_Data"), System.Data.ResDescription("ExtendedPropertiesDescr"), Browsable(false)]
        public PropertyCollection ExtendedProperties
        {
            get
            {
                if (this.extendedProperties == null)
                {
                    this.extendedProperties = new PropertyCollection();
                }
                return this.extendedProperties;
            }
        }

        internal IFormatProvider FormatProvider
        {
            get
            {
                if (this.table == null)
                {
                    return CultureInfo.CurrentCulture;
                }
                return this.table.FormatProvider;
            }
        }

        internal bool HasData
        {
            get
            {
                return (this._storage != null);
            }
        }

        internal bool ImplementsIChangeTracking
        {
            get
            {
                return this.implementsIChangeTracking;
            }
        }

        internal bool ImplementsINullable
        {
            get
            {
                return this.implementsINullable;
            }
        }

        internal bool ImplementsIRevertibleChangeTracking
        {
            get
            {
                return this.implementsIRevertibleChangeTracking;
            }
        }

        internal bool ImplementsIXMLSerializable
        {
            get
            {
                return this.implementsIXMLSerializable;
            }
        }

        internal bool IsCloneable
        {
            get
            {
                return this._storage.IsCloneable;
            }
        }

        internal bool IsCustomType
        {
            get
            {
                if (this._storage != null)
                {
                    return this._storage.IsCustomDefinedType;
                }
                return DataStorage.IsTypeCustomType(this.DataType);
            }
        }

        internal bool IsSqlType
        {
            get
            {
                return this.isSqlType;
            }
        }

        internal bool IsStringType
        {
            get
            {
                return this._storage.IsStringType;
            }
        }

        internal bool IsValueType
        {
            get
            {
                return this._storage.IsValueType;
            }
        }

        internal object this[int record]
        {
            get
            {
                return this._storage.Get(record);
            }
            set
            {
                try
                {
                    this._storage.Set(record, value);
                }
                catch (Exception exception)
                {
                    ExceptionBuilder.TraceExceptionForCapture(exception);
                    throw ExceptionBuilder.SetFailed(value, this, this.DataType, exception);
                }
                if (this.AutoIncrement && !this._storage.IsNull(record))
                {
                    this.AutoInc.SetCurrentAndIncrement(this._storage.Get(record));
                }
                if (this.Computed)
                {
                    DataRow dataRow = this.GetDataRow(record);
                    if (dataRow != null)
                    {
                        dataRow.LastChangedColumn = this;
                    }
                }
            }
        }

        internal CultureInfo Locale
        {
            get
            {
                if (this.table == null)
                {
                    return CultureInfo.CurrentCulture;
                }
                return this.table.Locale;
            }
        }

        [DefaultValue(-1), System.Data.ResCategory("DataCategory_Data"), System.Data.ResDescription("DataColumnMaxLengthDescr")]
        public int MaxLength
        {
            get
            {
                return this.maxLength;
            }
            set
            {
                IntPtr ptr;
                Bid.ScopeEnter(out ptr, "<ds.DataColumn.set_MaxLength|API> %d#, %d\n", this.ObjectID, value);
                try
                {
                    if (this.maxLength != value)
                    {
                        if (this.ColumnMapping == MappingType.SimpleContent)
                        {
                            throw ExceptionBuilder.CannotSetMaxLength2(this);
                        }
                        if ((this.DataType != typeof(string)) && (this.DataType != typeof(SqlString)))
                        {
                            throw ExceptionBuilder.HasToBeStringType(this);
                        }
                        int maxLength = this.maxLength;
                        this.maxLength = Math.Max(value, -1);
                        if (((maxLength < 0) || (value < maxLength)) && (((this.table != null) && this.table.EnforceConstraints) && !this.CheckMaxLength()))
                        {
                            this.maxLength = maxLength;
                            throw ExceptionBuilder.CannotSetMaxLength(this, value);
                        }
                        this.SetMaxLengthSimpleType();
                    }
                }
                finally
                {
                    Bid.ScopeLeave(ref ptr);
                }
            }
        }

        [System.Data.ResCategory("DataCategory_Data"), System.Data.ResDescription("DataColumnNamespaceDescr")]
        public string Namespace
        {
            get
            {
                if (this._columnUri != null)
                {
                    return this._columnUri;
                }
                if ((this.Table != null) && (this.columnMapping != MappingType.Attribute))
                {
                    return this.Table.Namespace;
                }
                return "";
            }
            set
            {
                Bid.Trace("<ds.DataColumn.set_Namespace|API> %d#, '%ls'\n", this.ObjectID, value);
                if (this._columnUri != value)
                {
                    if (this.columnMapping != MappingType.SimpleContent)
                    {
                        this.RaisePropertyChanging("Namespace");
                        this._columnUri = value;
                    }
                    else if (value != this.Namespace)
                    {
                        throw ExceptionBuilder.CannotChangeNamespace(this.ColumnName);
                    }
                }
            }
        }

        internal int ObjectID
        {
            get
            {
                return this._objectID;
            }
        }

        [System.Data.ResDescription("DataColumnOrdinalDescr"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), System.Data.ResCategory("DataCategory_Data")]
        public int Ordinal
        {
            get
            {
                return this._ordinal;
            }
        }

        [System.Data.ResCategory("DataCategory_Data"), System.Data.ResDescription("DataColumnPrefixDescr"), DefaultValue("")]
        public string Prefix
        {
            get
            {
                return this._columnPrefix;
            }
            set
            {
                if (value == null)
                {
                    value = "";
                }
                Bid.Trace("<ds.DataColumn.set_Prefix|API> %d#, '%ls'\n", this.ObjectID, value);
                if ((XmlConvert.DecodeName(value) == value) && (XmlConvert.EncodeName(value) != value))
                {
                    throw ExceptionBuilder.InvalidPrefix(value);
                }
                this._columnPrefix = value;
            }
        }

        [System.Data.ResDescription("DataColumnReadOnlyDescr"), System.Data.ResCategory("DataCategory_Data"), DefaultValue(false)]
        public bool ReadOnly
        {
            get
            {
                return this.readOnly;
            }
            set
            {
                Bid.Trace("<ds.DataColumn.set_ReadOnly|API> %d#, %d{bool}\n", this.ObjectID, value);
                if (this.readOnly != value)
                {
                    if (!value && (this.expression != null))
                    {
                        throw ExceptionBuilder.ReadOnlyAndExpression();
                    }
                    this.readOnly = value;
                }
            }
        }

        internal System.Data.SimpleType SimpleType
        {
            get
            {
                return this.simpleType;
            }
            set
            {
                this.simpleType = value;
                if ((value != null) && value.CanHaveMaxLength())
                {
                    this.maxLength = this.simpleType.MaxLength;
                }
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Index SortIndex
        {
            get
            {
                if (this.sortIndex == null)
                {
                    IndexField[] indexDesc = new IndexField[] { new IndexField(this, false) };
                    this.sortIndex = this.table.GetIndex(indexDesc, DataViewRowState.CurrentRows, null);
                    this.sortIndex.AddRef();
                }
                return this.sortIndex;
            }
        }

        [System.Data.ResDescription("DataColumnDataTableDescr"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), System.Data.ResCategory("DataCategory_Data")]
        public DataTable Table
        {
            get
            {
                return this.table;
            }
        }

        [System.Data.ResCategory("DataCategory_Data"), DefaultValue(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), System.Data.ResDescription("DataColumnUniqueDescr")]
        public bool Unique
        {
            get
            {
                return this.unique;
            }
            set
            {
                IntPtr ptr;
                Bid.ScopeEnter(out ptr, "<ds.DataColumn.set_Unique|API> %d#, %d{bool}\n", this.ObjectID, value);
                try
                {
                    if (this.unique != value)
                    {
                        if (value && (this.expression != null))
                        {
                            throw ExceptionBuilder.UniqueAndExpression();
                        }
                        UniqueConstraint constraint2 = null;
                        if (this.table != null)
                        {
                            if (value)
                            {
                                this.CheckUnique();
                            }
                            else
                            {
                                IEnumerator enumerator = this.Table.Constraints.GetEnumerator();
                                while (enumerator.MoveNext())
                                {
                                    UniqueConstraint current = enumerator.Current as UniqueConstraint;
                                    if (((current != null) && (current.ColumnsReference.Length == 1)) && (current.ColumnsReference[0] == this))
                                    {
                                        constraint2 = current;
                                    }
                                }
                                this.table.Constraints.CanRemove(constraint2, true);
                            }
                        }
                        this.unique = value;
                        if (this.table != null)
                        {
                            if (value)
                            {
                                UniqueConstraint constraint = new UniqueConstraint(this);
                                this.table.Constraints.Add(constraint);
                            }
                            else
                            {
                                this.table.Constraints.Remove(constraint2);
                            }
                        }
                    }
                }
                finally
                {
                    Bid.ScopeLeave(ref ptr);
                }
            }
        }

        internal string XmlDataType
        {
            get
            {
                return this.dttype;
            }
            set
            {
                this.dttype = value;
            }
        }
    }
}

