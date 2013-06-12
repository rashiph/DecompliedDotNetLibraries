namespace System.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data.Common;
    using System.Globalization;
    using System.Threading;

    [TypeConverter(typeof(RelationshipConverter)), DefaultProperty("RelationName"), Editor("Microsoft.VSDesigner.Data.Design.DataRelationEditor, Microsoft.VSDesigner, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    public class DataRelation
    {
        private bool _checkMultipleNested;
        private readonly int _objectID;
        private static int _objectTypeCount;
        internal string[] childColumnNames;
        private DataKey childKey;
        private ForeignKeyConstraint childKeyConstraint;
        internal string childTableName;
        internal string childTableNamespace;
        internal bool createConstraints;
        private System.Data.DataSet dataSet;
        internal PropertyCollection extendedProperties;
        internal bool nested;
        internal string[] parentColumnNames;
        private DataKey parentKey;
        private UniqueConstraint parentKeyConstraint;
        internal string parentTableName;
        internal string parentTableNamespace;
        internal string relationName;

        internal event PropertyChangedEventHandler PropertyChanging;

        public DataRelation(string relationName, DataColumn parentColumn, DataColumn childColumn) : this(relationName, parentColumn, childColumn, true)
        {
        }

        public DataRelation(string relationName, DataColumn[] parentColumns, DataColumn[] childColumns) : this(relationName, parentColumns, childColumns, true)
        {
        }

        public DataRelation(string relationName, DataColumn parentColumn, DataColumn childColumn, bool createConstraints)
        {
            this.relationName = "";
            this._checkMultipleNested = true;
            this._objectID = Interlocked.Increment(ref _objectTypeCount);
            Bid.Trace("<ds.DataRelation.DataRelation|API> %d#, relationName='%ls', parentColumn=%d, childColumn=%d, createConstraints=%d{bool}\n", this.ObjectID, relationName, (parentColumn != null) ? parentColumn.ObjectID : 0, (childColumn != null) ? childColumn.ObjectID : 0, createConstraints);
            DataColumn[] parentColumns = new DataColumn[] { parentColumn };
            DataColumn[] childColumns = new DataColumn[] { childColumn };
            this.Create(relationName, parentColumns, childColumns, createConstraints);
        }

        public DataRelation(string relationName, DataColumn[] parentColumns, DataColumn[] childColumns, bool createConstraints)
        {
            this.relationName = "";
            this._checkMultipleNested = true;
            this._objectID = Interlocked.Increment(ref _objectTypeCount);
            this.Create(relationName, parentColumns, childColumns, createConstraints);
        }

        [Browsable(false)]
        public DataRelation(string relationName, string parentTableName, string childTableName, string[] parentColumnNames, string[] childColumnNames, bool nested)
        {
            this.relationName = "";
            this._checkMultipleNested = true;
            this._objectID = Interlocked.Increment(ref _objectTypeCount);
            this.relationName = relationName;
            this.parentColumnNames = parentColumnNames;
            this.childColumnNames = childColumnNames;
            this.parentTableName = parentTableName;
            this.childTableName = childTableName;
            this.nested = nested;
        }

        [Browsable(false)]
        public DataRelation(string relationName, string parentTableName, string parentTableNamespace, string childTableName, string childTableNamespace, string[] parentColumnNames, string[] childColumnNames, bool nested)
        {
            this.relationName = "";
            this._checkMultipleNested = true;
            this._objectID = Interlocked.Increment(ref _objectTypeCount);
            this.relationName = relationName;
            this.parentColumnNames = parentColumnNames;
            this.childColumnNames = childColumnNames;
            this.parentTableName = parentTableName;
            this.childTableName = childTableName;
            this.parentTableNamespace = parentTableNamespace;
            this.childTableNamespace = childTableNamespace;
            this.nested = nested;
        }

        internal void CheckNamespaceValidityForNestedRelations(string ns)
        {
            foreach (DataRelation relation in this.ChildTable.ParentRelations)
            {
                if (((relation == this) || relation.Nested) && (relation.ParentTable.Namespace != ns))
                {
                    throw ExceptionBuilder.InValidNestedRelation(this.ChildTable.TableName);
                }
            }
        }

        internal void CheckNestedRelations()
        {
            Bid.Trace("<ds.DataRelation.CheckNestedRelations|INFO> %d#\n", this.ObjectID);
            DataTable parentTable = this.ParentTable;
            if (this.ChildTable == this.ParentTable)
            {
                if (string.Compare(this.ChildTable.TableName, this.ChildTable.DataSet.DataSetName, true, this.ChildTable.DataSet.Locale) == 0)
                {
                    throw ExceptionBuilder.SelfnestedDatasetConflictingName(this.ChildTable.TableName);
                }
            }
            else
            {
                List<DataTable> list = new List<DataTable> {
                    this.ChildTable
                };
                for (int i = 0; i < list.Count; i++)
                {
                    foreach (DataRelation relation in list[i].NestedParentRelations)
                    {
                        if ((relation.ParentTable == this.ChildTable) && (relation.ChildTable != this.ChildTable))
                        {
                            throw ExceptionBuilder.LoopInNestedRelations(this.ChildTable.TableName);
                        }
                        if (!list.Contains(relation.ParentTable))
                        {
                            list.Add(relation.ParentTable);
                        }
                    }
                }
            }
        }

        internal void CheckState()
        {
            if (this.dataSet == null)
            {
                this.parentKey.CheckState();
                this.childKey.CheckState();
                if (this.parentKey.Table.DataSet != this.childKey.Table.DataSet)
                {
                    throw ExceptionBuilder.RelationDataSetMismatch();
                }
                if (this.childKey.ColumnsEqual(this.parentKey))
                {
                    throw ExceptionBuilder.KeyColumnsIdentical();
                }
                for (int i = 0; i < this.parentKey.ColumnsReference.Length; i++)
                {
                    if ((this.parentKey.ColumnsReference[i].DataType != this.childKey.ColumnsReference[i].DataType) || (((this.parentKey.ColumnsReference[i].DataType == typeof(DateTime)) && (this.parentKey.ColumnsReference[i].DateTimeMode != this.childKey.ColumnsReference[i].DateTimeMode)) && ((this.parentKey.ColumnsReference[i].DateTimeMode & this.childKey.ColumnsReference[i].DateTimeMode) != DataSetDateTime.Unspecified)))
                    {
                        throw ExceptionBuilder.ColumnsTypeMismatch();
                    }
                }
            }
        }

        protected void CheckStateForProperty()
        {
            try
            {
                this.CheckState();
            }
            catch (Exception exception)
            {
                if (ADP.IsCatchableExceptionType(exception))
                {
                    throw ExceptionBuilder.BadObjectPropertyAccess(exception.Message);
                }
                throw;
            }
        }

        internal DataRelation Clone(System.Data.DataSet destination)
        {
            Bid.Trace("<ds.DataRelation.Clone|INFO> %d#, destination=%d\n", this.ObjectID, (destination != null) ? destination.ObjectID : 0);
            DataTable table2 = destination.Tables[this.ParentTable.TableName, this.ParentTable.Namespace];
            DataTable table = destination.Tables[this.ChildTable.TableName, this.ChildTable.Namespace];
            int length = this.parentKey.ColumnsReference.Length;
            DataColumn[] parentColumns = new DataColumn[length];
            DataColumn[] childColumns = new DataColumn[length];
            for (int i = 0; i < length; i++)
            {
                parentColumns[i] = table2.Columns[this.ParentKey.ColumnsReference[i].ColumnName];
                childColumns[i] = table.Columns[this.ChildKey.ColumnsReference[i].ColumnName];
            }
            DataRelation relation = new DataRelation(this.relationName, parentColumns, childColumns, false) {
                CheckMultipleNested = false,
                Nested = this.Nested,
                CheckMultipleNested = true
            };
            if (this.extendedProperties != null)
            {
                foreach (object obj2 in this.extendedProperties.Keys)
                {
                    relation.ExtendedProperties[obj2] = this.extendedProperties[obj2];
                }
            }
            return relation;
        }

        private void Create(string relationName, DataColumn[] parentColumns, DataColumn[] childColumns, bool createConstraints)
        {
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<ds.DataRelation.Create|INFO> %d#, relationName='%ls', createConstraints=%d{bool}\n", this.ObjectID, relationName, createConstraints);
            try
            {
                this.parentKey = new DataKey(parentColumns, true);
                this.childKey = new DataKey(childColumns, true);
                if (parentColumns.Length != childColumns.Length)
                {
                    throw ExceptionBuilder.KeyLengthMismatch();
                }
                for (int i = 0; i < parentColumns.Length; i++)
                {
                    if ((parentColumns[i].Table.DataSet == null) || (childColumns[i].Table.DataSet == null))
                    {
                        throw ExceptionBuilder.ParentOrChildColumnsDoNotHaveDataSet();
                    }
                }
                this.CheckState();
                this.relationName = (relationName == null) ? "" : relationName;
                this.createConstraints = createConstraints;
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
        }

        internal static DataRow[] GetChildRows(DataKey parentKey, DataKey childKey, DataRow parentRow, DataRowVersion version)
        {
            object[] keyValues = parentRow.GetKeyValues(parentKey, version);
            if (IsKeyNull(keyValues))
            {
                return childKey.Table.NewRowArray(0);
            }
            return childKey.GetSortIndex((version == DataRowVersion.Original) ? DataViewRowState.OriginalRows : DataViewRowState.CurrentRows).GetRows(keyValues);
        }

        internal static DataRow GetParentRow(DataKey parentKey, DataKey childKey, DataRow childRow, DataRowVersion version)
        {
            if (!childRow.HasVersion((version == DataRowVersion.Original) ? DataRowVersion.Original : DataRowVersion.Current) && (childRow.tempRecord == -1))
            {
                return null;
            }
            object[] keyValues = childRow.GetKeyValues(childKey, version);
            if (IsKeyNull(keyValues))
            {
                return null;
            }
            Index sortIndex = parentKey.GetSortIndex((version == DataRowVersion.Original) ? DataViewRowState.OriginalRows : DataViewRowState.CurrentRows);
            Range range = sortIndex.FindRecords(keyValues);
            if (range.IsNull)
            {
                return null;
            }
            if (range.Count > 1)
            {
                throw ExceptionBuilder.MultipleParents();
            }
            return parentKey.Table.recordManager[sortIndex.GetRecord(range.Min)];
        }

        internal static DataRow[] GetParentRows(DataKey parentKey, DataKey childKey, DataRow childRow, DataRowVersion version)
        {
            object[] keyValues = childRow.GetKeyValues(childKey, version);
            if (IsKeyNull(keyValues))
            {
                return parentKey.Table.NewRowArray(0);
            }
            return parentKey.GetSortIndex((version == DataRowVersion.Original) ? DataViewRowState.OriginalRows : DataViewRowState.CurrentRows).GetRows(keyValues);
        }

        private bool IsAutoGenerated(DataColumn col)
        {
            if (col.ColumnMapping != MappingType.Hidden)
            {
                return false;
            }
            if (col.DataType != typeof(int))
            {
                return false;
            }
            string str = col.Table.TableName + "_Id";
            if ((col.ColumnName != str) && (col.ColumnName != (str + "_0")))
            {
                str = this.ParentColumnsReference[0].Table.TableName + "_Id";
                if (!(col.ColumnName == str) && !(col.ColumnName == (str + "_0")))
                {
                    return false;
                }
            }
            return true;
        }

        private static bool IsKeyNull(object[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                if (!DataStorage.IsObjectNull(values[i]))
                {
                    return false;
                }
            }
            return true;
        }

        protected internal void OnPropertyChanging(PropertyChangedEventArgs pcevent)
        {
            if (this.onPropertyChangingDelegate != null)
            {
                Bid.Trace("<ds.DataRelation.OnPropertyChanging|INFO> %d#\n", this.ObjectID);
                this.onPropertyChangingDelegate(this, pcevent);
            }
        }

        protected internal void RaisePropertyChanging(string name)
        {
            this.OnPropertyChanging(new PropertyChangedEventArgs(name));
        }

        internal void SetChildKeyConstraint(ForeignKeyConstraint value)
        {
            this.childKeyConstraint = value;
        }

        internal void SetDataSet(System.Data.DataSet dataSet)
        {
            if (this.dataSet != dataSet)
            {
                this.dataSet = dataSet;
            }
        }

        internal void SetParentKeyConstraint(UniqueConstraint value)
        {
            this.parentKeyConstraint = value;
        }

        internal void SetParentRowRecords(DataRow childRow, DataRow parentRow)
        {
            object[] keyValues = parentRow.GetKeyValues(this.ParentKey);
            if (childRow.tempRecord != -1)
            {
                this.ChildTable.recordManager.SetKeyValues(childRow.tempRecord, this.ChildKey, keyValues);
            }
            if (childRow.newRecord != -1)
            {
                this.ChildTable.recordManager.SetKeyValues(childRow.newRecord, this.ChildKey, keyValues);
            }
            if (childRow.oldRecord != -1)
            {
                this.ChildTable.recordManager.SetKeyValues(childRow.oldRecord, this.ChildKey, keyValues);
            }
        }

        public override string ToString()
        {
            return this.RelationName;
        }

        internal void ValidateMultipleNestedRelations()
        {
            if ((this.Nested && this.CheckMultipleNested) && (0 < this.ChildTable.NestedParentRelations.Length))
            {
                DataColumn[] childColumns = this.ChildColumns;
                if ((childColumns.Length != 1) || !this.IsAutoGenerated(childColumns[0]))
                {
                    throw ExceptionBuilder.TableCantBeNestedInTwoTables(this.ChildTable.TableName);
                }
                if (!XmlTreeGen.AutoGenerated(this))
                {
                    throw ExceptionBuilder.TableCantBeNestedInTwoTables(this.ChildTable.TableName);
                }
                foreach (Constraint constraint in this.ChildTable.Constraints)
                {
                    if (constraint is ForeignKeyConstraint)
                    {
                        ForeignKeyConstraint fk = (ForeignKeyConstraint) constraint;
                        if (!XmlTreeGen.AutoGenerated(fk, true))
                        {
                            throw ExceptionBuilder.TableCantBeNestedInTwoTables(this.ChildTable.TableName);
                        }
                    }
                    else
                    {
                        UniqueConstraint unique = (UniqueConstraint) constraint;
                        if (!XmlTreeGen.AutoGenerated(unique))
                        {
                            throw ExceptionBuilder.TableCantBeNestedInTwoTables(this.ChildTable.TableName);
                        }
                    }
                }
            }
        }

        internal bool CheckMultipleNested
        {
            get
            {
                return this._checkMultipleNested;
            }
            set
            {
                this._checkMultipleNested = value;
            }
        }

        internal string[] ChildColumnNames
        {
            get
            {
                return this.childKey.GetColumnNames();
            }
        }

        [ResCategory("DataCategory_Data"), ResDescription("DataRelationChildColumnsDescr")]
        public virtual DataColumn[] ChildColumns
        {
            get
            {
                this.CheckStateForProperty();
                return this.childKey.ToArray();
            }
        }

        internal DataColumn[] ChildColumnsReference
        {
            get
            {
                this.CheckStateForProperty();
                return this.childKey.ColumnsReference;
            }
        }

        internal DataKey ChildKey
        {
            get
            {
                this.CheckStateForProperty();
                return this.childKey;
            }
        }

        public virtual ForeignKeyConstraint ChildKeyConstraint
        {
            get
            {
                this.CheckStateForProperty();
                return this.childKeyConstraint;
            }
        }

        public virtual DataTable ChildTable
        {
            get
            {
                this.CheckStateForProperty();
                return this.childKey.Table;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual System.Data.DataSet DataSet
        {
            get
            {
                this.CheckStateForProperty();
                return this.dataSet;
            }
        }

        [ResCategory("DataCategory_Data"), Browsable(false), ResDescription("ExtendedPropertiesDescr")]
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

        [DefaultValue(false), ResCategory("DataCategory_Data"), ResDescription("DataRelationNested")]
        public virtual bool Nested
        {
            get
            {
                this.CheckStateForProperty();
                return this.nested;
            }
            set
            {
                IntPtr ptr;
                Bid.ScopeEnter(out ptr, "<ds.DataRelation.set_Nested|API> %d#, %d{bool}\n", this.ObjectID, value);
                try
                {
                    if (this.nested != value)
                    {
                        if ((this.dataSet != null) && value)
                        {
                            if (this.ChildTable.IsNamespaceInherited())
                            {
                                this.CheckNamespaceValidityForNestedRelations(this.ParentTable.Namespace);
                            }
                            ForeignKeyConstraint constraint = this.ChildTable.Constraints.FindForeignKeyConstraint(this.ChildKey.ColumnsReference, this.ParentKey.ColumnsReference);
                            if (constraint != null)
                            {
                                constraint.CheckConstraint();
                            }
                            this.ValidateMultipleNestedRelations();
                        }
                        if (!value && (this.parentKey.ColumnsReference[0].ColumnMapping == MappingType.Hidden))
                        {
                            throw ExceptionBuilder.RelationNestedReadOnly();
                        }
                        if (value)
                        {
                            this.ParentTable.Columns.RegisterColumnName(this.ChildTable.TableName, null, this.ChildTable);
                        }
                        else
                        {
                            this.ParentTable.Columns.UnregisterName(this.ChildTable.TableName);
                        }
                        this.RaisePropertyChanging("Nested");
                        if (value)
                        {
                            this.CheckNestedRelations();
                            if (this.DataSet != null)
                            {
                                if (this.ParentTable == this.ChildTable)
                                {
                                    foreach (DataRow row2 in this.ChildTable.Rows)
                                    {
                                        row2.CheckForLoops(this);
                                    }
                                    if ((this.ChildTable.DataSet != null) && (string.Compare(this.ChildTable.TableName, this.ChildTable.DataSet.DataSetName, true, this.ChildTable.DataSet.Locale) == 0))
                                    {
                                        throw ExceptionBuilder.DatasetConflictingName(this.dataSet.DataSetName);
                                    }
                                    this.ChildTable.fNestedInDataset = false;
                                }
                                else
                                {
                                    foreach (DataRow row in this.ChildTable.Rows)
                                    {
                                        row.GetParentRow(this);
                                    }
                                }
                            }
                            DataTable parentTable = this.ParentTable;
                            parentTable.ElementColumnCount++;
                        }
                        else
                        {
                            DataTable table2 = this.ParentTable;
                            table2.ElementColumnCount--;
                        }
                        this.nested = value;
                        this.ChildTable.CacheNestedParent();
                        if ((value && ADP.IsEmpty(this.ChildTable.Namespace)) && ((this.ChildTable.NestedParentsCount > 1) || ((this.ChildTable.NestedParentsCount > 0) && !this.ChildTable.DataSet.Relations.Contains(this.RelationName))))
                        {
                            string strA = null;
                            foreach (DataRelation relation in this.ChildTable.ParentRelations)
                            {
                                if (relation.Nested)
                                {
                                    if (strA == null)
                                    {
                                        strA = relation.ParentTable.Namespace;
                                    }
                                    else if (string.Compare(strA, relation.ParentTable.Namespace, StringComparison.Ordinal) != 0)
                                    {
                                        this.nested = false;
                                        throw ExceptionBuilder.InvalidParentNamespaceinNestedRelation(this.ChildTable.TableName);
                                    }
                                }
                            }
                            if ((this.CheckMultipleNested && (this.ChildTable.tableNamespace != null)) && (this.ChildTable.tableNamespace.Length == 0))
                            {
                                throw ExceptionBuilder.TableCantBeNestedInTwoTables(this.ChildTable.TableName);
                            }
                            this.ChildTable.tableNamespace = null;
                        }
                    }
                }
                finally
                {
                    Bid.ScopeLeave(ref ptr);
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

        internal string[] ParentColumnNames
        {
            get
            {
                return this.parentKey.GetColumnNames();
            }
        }

        [ResDescription("DataRelationParentColumnsDescr"), ResCategory("DataCategory_Data")]
        public virtual DataColumn[] ParentColumns
        {
            get
            {
                this.CheckStateForProperty();
                return this.parentKey.ToArray();
            }
        }

        internal DataColumn[] ParentColumnsReference
        {
            get
            {
                return this.parentKey.ColumnsReference;
            }
        }

        internal DataKey ParentKey
        {
            get
            {
                this.CheckStateForProperty();
                return this.parentKey;
            }
        }

        public virtual UniqueConstraint ParentKeyConstraint
        {
            get
            {
                this.CheckStateForProperty();
                return this.parentKeyConstraint;
            }
        }

        public virtual DataTable ParentTable
        {
            get
            {
                this.CheckStateForProperty();
                return this.parentKey.Table;
            }
        }

        [ResDescription("DataRelationRelationNameDescr"), ResCategory("DataCategory_Data"), DefaultValue("")]
        public virtual string RelationName
        {
            get
            {
                this.CheckStateForProperty();
                return this.relationName;
            }
            set
            {
                IntPtr ptr;
                Bid.ScopeEnter(out ptr, "<ds.DataRelation.set_RelationName|API> %d#, '%ls'\n", this.ObjectID, value);
                try
                {
                    if (value == null)
                    {
                        value = "";
                    }
                    CultureInfo culture = (this.dataSet != null) ? this.dataSet.Locale : CultureInfo.CurrentCulture;
                    if (string.Compare(this.relationName, value, true, culture) != 0)
                    {
                        if (this.dataSet != null)
                        {
                            if (value.Length == 0)
                            {
                                throw ExceptionBuilder.NoRelationName();
                            }
                            this.dataSet.Relations.RegisterName(value);
                            if (this.relationName.Length != 0)
                            {
                                this.dataSet.Relations.UnregisterName(this.relationName);
                            }
                        }
                        this.relationName = value;
                        ((DataRelationCollection.DataTableRelationCollection) this.ParentTable.ChildRelations).OnRelationPropertyChanged(new CollectionChangeEventArgs(CollectionChangeAction.Refresh, this));
                        ((DataRelationCollection.DataTableRelationCollection) this.ChildTable.ParentRelations).OnRelationPropertyChanged(new CollectionChangeEventArgs(CollectionChangeAction.Refresh, this));
                    }
                    else if (string.Compare(this.relationName, value, false, culture) != 0)
                    {
                        this.relationName = value;
                        ((DataRelationCollection.DataTableRelationCollection) this.ParentTable.ChildRelations).OnRelationPropertyChanged(new CollectionChangeEventArgs(CollectionChangeAction.Refresh, this));
                        ((DataRelationCollection.DataTableRelationCollection) this.ChildTable.ParentRelations).OnRelationPropertyChanged(new CollectionChangeEventArgs(CollectionChangeAction.Refresh, this));
                    }
                }
                finally
                {
                    Bid.ScopeLeave(ref ptr);
                }
            }
        }
    }
}

