namespace System.Data
{
    using System;
    using System.ComponentModel;
    using System.Data.Common;

    [Editor("Microsoft.VSDesigner.Data.Design.ForeignKeyConstraintEditor, Microsoft.VSDesigner, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), DefaultProperty("ConstraintName")]
    public class ForeignKeyConstraint : Constraint
    {
        internal System.Data.AcceptRejectRule acceptRejectRule;
        internal const System.Data.AcceptRejectRule AcceptRejectRule_Default = System.Data.AcceptRejectRule.None;
        internal string[] childColumnNames;
        private DataKey childKey;
        internal string constraintName;
        internal Rule deleteRule;
        internal string[] parentColumnNames;
        private DataKey parentKey;
        internal string parentTableName;
        internal string parentTableNamespace;
        internal const Rule Rule_Default = Rule.Cascade;
        internal Rule updateRule;

        public ForeignKeyConstraint(DataColumn parentColumn, DataColumn childColumn) : this(null, parentColumn, childColumn)
        {
        }

        public ForeignKeyConstraint(DataColumn[] parentColumns, DataColumn[] childColumns) : this(null, parentColumns, childColumns)
        {
        }

        public ForeignKeyConstraint(string constraintName, DataColumn parentColumn, DataColumn childColumn)
        {
            this.deleteRule = Rule.Cascade;
            this.updateRule = Rule.Cascade;
            DataColumn[] parentColumns = new DataColumn[] { parentColumn };
            DataColumn[] childColumns = new DataColumn[] { childColumn };
            this.Create(constraintName, parentColumns, childColumns);
        }

        public ForeignKeyConstraint(string constraintName, DataColumn[] parentColumns, DataColumn[] childColumns)
        {
            this.deleteRule = Rule.Cascade;
            this.updateRule = Rule.Cascade;
            this.Create(constraintName, parentColumns, childColumns);
        }

        [Browsable(false)]
        public ForeignKeyConstraint(string constraintName, string parentTableName, string[] parentColumnNames, string[] childColumnNames, System.Data.AcceptRejectRule acceptRejectRule, Rule deleteRule, Rule updateRule)
        {
            this.deleteRule = Rule.Cascade;
            this.updateRule = Rule.Cascade;
            this.constraintName = constraintName;
            this.parentColumnNames = parentColumnNames;
            this.childColumnNames = childColumnNames;
            this.parentTableName = parentTableName;
            this.acceptRejectRule = acceptRejectRule;
            this.deleteRule = deleteRule;
            this.updateRule = updateRule;
        }

        [Browsable(false)]
        public ForeignKeyConstraint(string constraintName, string parentTableName, string parentTableNamespace, string[] parentColumnNames, string[] childColumnNames, System.Data.AcceptRejectRule acceptRejectRule, Rule deleteRule, Rule updateRule)
        {
            this.deleteRule = Rule.Cascade;
            this.updateRule = Rule.Cascade;
            this.constraintName = constraintName;
            this.parentColumnNames = parentColumnNames;
            this.childColumnNames = childColumnNames;
            this.parentTableName = parentTableName;
            this.parentTableNamespace = parentTableNamespace;
            this.acceptRejectRule = acceptRejectRule;
            this.deleteRule = deleteRule;
            this.updateRule = updateRule;
        }

        internal override bool CanBeRemovedFromCollection(ConstraintCollection constraints, bool fThrowException)
        {
            return true;
        }

        internal override bool CanEnableConstraint()
        {
            if ((this.Table.DataSet != null) && this.Table.DataSet.EnforceConstraints)
            {
                object[] uniqueKeyValues = this.childKey.GetSortIndex().GetUniqueKeyValues();
                Index sortIndex = this.parentKey.GetSortIndex();
                for (int i = 0; i < uniqueKeyValues.Length; i++)
                {
                    object[] values = (object[]) uniqueKeyValues[i];
                    if (!this.IsKeyNull(values) && !sortIndex.IsKeyInIndex(values))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        internal void CascadeCommit(DataRow row)
        {
            if ((row.RowState != DataRowState.Detached) && (this.acceptRejectRule == System.Data.AcceptRejectRule.Cascade))
            {
                Index sortIndex = this.childKey.GetSortIndex((row.RowState == DataRowState.Deleted) ? DataViewRowState.Deleted : DataViewRowState.CurrentRows);
                object[] keyValues = row.GetKeyValues(this.parentKey, (row.RowState == DataRowState.Deleted) ? DataRowVersion.Original : DataRowVersion.Default);
                if (!this.IsKeyNull(keyValues))
                {
                    Range range = sortIndex.FindRecords(keyValues);
                    if (!range.IsNull)
                    {
                        foreach (DataRow row2 in sortIndex.GetRows(range))
                        {
                            if ((DataRowState.Detached != row2.RowState) && !row2.inCascade)
                            {
                                row2.AcceptChanges();
                            }
                        }
                    }
                }
            }
        }

        internal void CascadeDelete(DataRow row)
        {
            if (-1 != row.newRecord)
            {
                object[] keyValues = row.GetKeyValues(this.parentKey, DataRowVersion.Current);
                if (!this.IsKeyNull(keyValues))
                {
                    Index sortIndex = this.childKey.GetSortIndex();
                    switch (this.DeleteRule)
                    {
                        case Rule.None:
                            if (row.Table.DataSet.EnforceConstraints)
                            {
                                Range range4 = sortIndex.FindRecords(keyValues);
                                if (range4.IsNull)
                                {
                                    return;
                                }
                                if ((range4.Count != 1) || (sortIndex.GetRow(range4.Min) != row))
                                {
                                    throw ExceptionBuilder.FailedCascadeDelete(this.ConstraintName);
                                }
                            }
                            return;

                        case Rule.Cascade:
                        {
                            object[] key = row.GetKeyValues(this.parentKey, DataRowVersion.Default);
                            Range range3 = sortIndex.FindRecords(key);
                            if (!range3.IsNull)
                            {
                                foreach (DataRow row2 in sortIndex.GetRows(range3))
                                {
                                    if (!row2.inCascade)
                                    {
                                        row2.Table.DeleteRow(row2);
                                    }
                                }
                            }
                            return;
                        }
                        case Rule.SetNull:
                        {
                            object[] objArray3 = new object[this.childKey.ColumnsReference.Length];
                            for (int i = 0; i < this.childKey.ColumnsReference.Length; i++)
                            {
                                objArray3[i] = DBNull.Value;
                            }
                            Range range2 = sortIndex.FindRecords(keyValues);
                            if (!range2.IsNull)
                            {
                                DataRow[] rows = sortIndex.GetRows(range2);
                                for (int j = 0; j < rows.Length; j++)
                                {
                                    if (row != rows[j])
                                    {
                                        rows[j].SetKeyValues(this.childKey, objArray3);
                                    }
                                }
                            }
                            return;
                        }
                        case Rule.SetDefault:
                        {
                            object[] objArray2 = new object[this.childKey.ColumnsReference.Length];
                            for (int k = 0; k < this.childKey.ColumnsReference.Length; k++)
                            {
                                objArray2[k] = this.childKey.ColumnsReference[k].DefaultValue;
                            }
                            Range range = sortIndex.FindRecords(keyValues);
                            if (!range.IsNull)
                            {
                                DataRow[] rowArray = sortIndex.GetRows(range);
                                for (int m = 0; m < rowArray.Length; m++)
                                {
                                    if (row != rowArray[m])
                                    {
                                        rowArray[m].SetKeyValues(this.childKey, objArray2);
                                    }
                                }
                            }
                            return;
                        }
                    }
                }
            }
        }

        internal void CascadeRollback(DataRow row)
        {
            Index sortIndex = this.childKey.GetSortIndex((row.RowState == DataRowState.Deleted) ? DataViewRowState.OriginalRows : DataViewRowState.CurrentRows);
            object[] keyValues = row.GetKeyValues(this.parentKey, (row.RowState == DataRowState.Modified) ? DataRowVersion.Current : DataRowVersion.Default);
            if (!this.IsKeyNull(keyValues))
            {
                Range range = sortIndex.FindRecords(keyValues);
                if (this.acceptRejectRule == System.Data.AcceptRejectRule.Cascade)
                {
                    if (!range.IsNull)
                    {
                        DataRow[] rows = sortIndex.GetRows(range);
                        for (int i = 0; i < rows.Length; i++)
                        {
                            if (!rows[i].inCascade)
                            {
                                rows[i].RejectChanges();
                            }
                        }
                    }
                }
                else if (((((row.RowState != DataRowState.Deleted) && row.Table.DataSet.EnforceConstraints) && !range.IsNull) && ((range.Count != 1) || (sortIndex.GetRow(range.Min) != row))) && row.HasKeyChanged(this.parentKey))
                {
                    throw ExceptionBuilder.FailedCascadeUpdate(this.ConstraintName);
                }
            }
        }

        internal void CascadeUpdate(DataRow row)
        {
            if (-1 != row.newRecord)
            {
                object[] keyValues = row.GetKeyValues(this.parentKey, DataRowVersion.Current);
                if (this.Table.DataSet.fInReadXml || !this.IsKeyNull(keyValues))
                {
                    Index sortIndex = this.childKey.GetSortIndex();
                    switch (this.UpdateRule)
                    {
                        case Rule.None:
                            if (row.Table.DataSet.EnforceConstraints && !sortIndex.FindRecords(keyValues).IsNull)
                            {
                                throw ExceptionBuilder.FailedCascadeUpdate(this.ConstraintName);
                            }
                            return;

                        case Rule.Cascade:
                        {
                            Range range3 = sortIndex.FindRecords(keyValues);
                            if (!range3.IsNull)
                            {
                                object[] objArray4 = row.GetKeyValues(this.parentKey, DataRowVersion.Proposed);
                                DataRow[] rows = sortIndex.GetRows(range3);
                                for (int i = 0; i < rows.Length; i++)
                                {
                                    rows[i].SetKeyValues(this.childKey, objArray4);
                                }
                            }
                            return;
                        }
                        case Rule.SetNull:
                        {
                            object[] objArray3 = new object[this.childKey.ColumnsReference.Length];
                            for (int j = 0; j < this.childKey.ColumnsReference.Length; j++)
                            {
                                objArray3[j] = DBNull.Value;
                            }
                            Range range2 = sortIndex.FindRecords(keyValues);
                            if (!range2.IsNull)
                            {
                                DataRow[] rowArray2 = sortIndex.GetRows(range2);
                                for (int k = 0; k < rowArray2.Length; k++)
                                {
                                    rowArray2[k].SetKeyValues(this.childKey, objArray3);
                                }
                            }
                            return;
                        }
                        case Rule.SetDefault:
                        {
                            object[] objArray2 = new object[this.childKey.ColumnsReference.Length];
                            for (int m = 0; m < this.childKey.ColumnsReference.Length; m++)
                            {
                                objArray2[m] = this.childKey.ColumnsReference[m].DefaultValue;
                            }
                            Range range = sortIndex.FindRecords(keyValues);
                            if (!range.IsNull)
                            {
                                DataRow[] rowArray = sortIndex.GetRows(range);
                                for (int n = 0; n < rowArray.Length; n++)
                                {
                                    rowArray[n].SetKeyValues(this.childKey, objArray2);
                                }
                            }
                            return;
                        }
                    }
                }
            }
        }

        internal override void CheckCanAddToCollection(ConstraintCollection constraints)
        {
            if (this.Table != constraints.Table)
            {
                throw ExceptionBuilder.ConstraintAddFailed(constraints.Table);
            }
            if ((this.Table.Locale.LCID != this.RelatedTable.Locale.LCID) || (this.Table.CaseSensitive != this.RelatedTable.CaseSensitive))
            {
                throw ExceptionBuilder.CaseLocaleMismatch();
            }
        }

        internal void CheckCanClearParentTable(DataTable table)
        {
            if (this.Table.DataSet.EnforceConstraints && (this.Table.Rows.Count > 0))
            {
                throw ExceptionBuilder.FailedClearParentTable(table.TableName, this.ConstraintName, this.Table.TableName);
            }
        }

        internal void CheckCanRemoveParentRow(DataRow row)
        {
            if (this.Table.DataSet.EnforceConstraints && (DataRelation.GetChildRows(this.ParentKey, this.ChildKey, row, DataRowVersion.Default).Length > 0))
            {
                throw ExceptionBuilder.RemoveParentRow(this);
            }
        }

        internal void CheckCascade(DataRow row, DataRowAction action)
        {
            if (!row.inCascade)
            {
                row.inCascade = true;
                try
                {
                    if (action == DataRowAction.Change)
                    {
                        if (row.HasKeyChanged(this.parentKey))
                        {
                            this.CascadeUpdate(row);
                        }
                    }
                    else if (action == DataRowAction.Delete)
                    {
                        this.CascadeDelete(row);
                    }
                    else if (action == DataRowAction.Commit)
                    {
                        this.CascadeCommit(row);
                    }
                    else if (action == DataRowAction.Rollback)
                    {
                        this.CascadeRollback(row);
                    }
                }
                finally
                {
                    row.inCascade = false;
                }
            }
        }

        internal override void CheckConstraint(DataRow childRow, DataRowAction action)
        {
            if (((((action == DataRowAction.Change) || (action == DataRowAction.Add)) || (action == DataRowAction.Rollback)) && ((this.Table.DataSet != null) && this.Table.DataSet.EnforceConstraints)) && childRow.HasKeyChanged(this.childKey))
            {
                DataRowVersion version = (action == DataRowAction.Rollback) ? DataRowVersion.Original : DataRowVersion.Current;
                object[] keyValues = childRow.GetKeyValues(this.childKey);
                if (childRow.HasVersion(version))
                {
                    DataRow row = DataRelation.GetParentRow(this.ParentKey, this.ChildKey, childRow, version);
                    if ((row != null) && row.inCascade)
                    {
                        object[] objArray2 = row.GetKeyValues(this.parentKey, (action == DataRowAction.Rollback) ? version : DataRowVersion.Default);
                        int record = childRow.Table.NewRecord();
                        childRow.Table.SetKeyValues(this.childKey, objArray2, record);
                        if (this.childKey.RecordsEqual(childRow.tempRecord, record))
                        {
                            return;
                        }
                    }
                }
                object[] values = childRow.GetKeyValues(this.childKey);
                if (!this.IsKeyNull(values) && !this.parentKey.GetSortIndex().IsKeyInIndex(values))
                {
                    if ((this.childKey.Table == this.parentKey.Table) && (childRow.tempRecord != -1))
                    {
                        int index = 0;
                        index = 0;
                        while (index < values.Length)
                        {
                            DataColumn column = this.parentKey.ColumnsReference[index];
                            object obj2 = column.ConvertValue(values[index]);
                            if (column.CompareValueTo(childRow.tempRecord, obj2) != 0)
                            {
                                break;
                            }
                            index++;
                        }
                        if (index == values.Length)
                        {
                            return;
                        }
                    }
                    throw ExceptionBuilder.ForeignKeyViolation(this.ConstraintName, keyValues);
                }
            }
        }

        internal override void CheckState()
        {
            this.NonVirtualCheckState();
        }

        internal override Constraint Clone(DataSet destination)
        {
            return this.Clone(destination, false);
        }

        internal ForeignKeyConstraint Clone(DataTable destination)
        {
            int length = this.Columns.Length;
            DataColumn[] childColumns = new DataColumn[length];
            DataColumn[] parentColumns = new DataColumn[length];
            int index = 0;
            for (int i = 0; i < length; i++)
            {
                DataColumn column = this.Columns[i];
                index = destination.Columns.IndexOf(column.ColumnName);
                if (index < 0)
                {
                    return null;
                }
                childColumns[i] = destination.Columns[index];
                column = this.RelatedColumnsReference[i];
                index = destination.Columns.IndexOf(column.ColumnName);
                if (index < 0)
                {
                    return null;
                }
                parentColumns[i] = destination.Columns[index];
            }
            ForeignKeyConstraint constraint = new ForeignKeyConstraint(this.ConstraintName, parentColumns, childColumns) {
                UpdateRule = this.UpdateRule,
                DeleteRule = this.DeleteRule,
                AcceptRejectRule = this.AcceptRejectRule
            };
            foreach (object obj2 in base.ExtendedProperties.Keys)
            {
                constraint.ExtendedProperties[obj2] = base.ExtendedProperties[obj2];
            }
            return constraint;
        }

        internal override Constraint Clone(DataSet destination, bool ignorNSforTableLookup)
        {
            int index;
            if (ignorNSforTableLookup)
            {
                index = destination.Tables.IndexOf(this.Table.TableName);
            }
            else
            {
                index = destination.Tables.IndexOf(this.Table.TableName, this.Table.Namespace, false);
            }
            if (index < 0)
            {
                return null;
            }
            DataTable table2 = destination.Tables[index];
            if (ignorNSforTableLookup)
            {
                index = destination.Tables.IndexOf(this.RelatedTable.TableName);
            }
            else
            {
                index = destination.Tables.IndexOf(this.RelatedTable.TableName, this.RelatedTable.Namespace, false);
            }
            if (index < 0)
            {
                return null;
            }
            DataTable table = destination.Tables[index];
            int length = this.Columns.Length;
            DataColumn[] childColumns = new DataColumn[length];
            DataColumn[] parentColumns = new DataColumn[length];
            for (int i = 0; i < length; i++)
            {
                DataColumn column = this.Columns[i];
                index = table2.Columns.IndexOf(column.ColumnName);
                if (index < 0)
                {
                    return null;
                }
                childColumns[i] = table2.Columns[index];
                column = this.RelatedColumnsReference[i];
                index = table.Columns.IndexOf(column.ColumnName);
                if (index < 0)
                {
                    return null;
                }
                parentColumns[i] = table.Columns[index];
            }
            ForeignKeyConstraint constraint = new ForeignKeyConstraint(this.ConstraintName, parentColumns, childColumns) {
                UpdateRule = this.UpdateRule,
                DeleteRule = this.DeleteRule,
                AcceptRejectRule = this.AcceptRejectRule
            };
            foreach (object obj2 in base.ExtendedProperties.Keys)
            {
                constraint.ExtendedProperties[obj2] = base.ExtendedProperties[obj2];
            }
            return constraint;
        }

        internal override bool ContainsColumn(DataColumn column)
        {
            if (!this.parentKey.ContainsColumn(column))
            {
                return this.childKey.ContainsColumn(column);
            }
            return true;
        }

        private void Create(string relationName, DataColumn[] parentColumns, DataColumn[] childColumns)
        {
            if ((parentColumns.Length == 0) || (childColumns.Length == 0))
            {
                throw ExceptionBuilder.KeyLengthZero();
            }
            if (parentColumns.Length != childColumns.Length)
            {
                throw ExceptionBuilder.KeyLengthMismatch();
            }
            for (int i = 0; i < parentColumns.Length; i++)
            {
                if (parentColumns[i].Computed)
                {
                    throw ExceptionBuilder.ExpressionInConstraint(parentColumns[i]);
                }
                if (childColumns[i].Computed)
                {
                    throw ExceptionBuilder.ExpressionInConstraint(childColumns[i]);
                }
            }
            this.parentKey = new DataKey(parentColumns, true);
            this.childKey = new DataKey(childColumns, true);
            this.ConstraintName = relationName;
            this.NonVirtualCheckState();
        }

        public override bool Equals(object key)
        {
            if (!(key is ForeignKeyConstraint))
            {
                return false;
            }
            ForeignKeyConstraint constraint = (ForeignKeyConstraint) key;
            return (this.ParentKey.ColumnsEqual(constraint.ParentKey) && this.ChildKey.ColumnsEqual(constraint.ChildKey));
        }

        internal DataRelation FindParentRelation()
        {
            DataRelationCollection parentRelations = this.Table.ParentRelations;
            for (int i = 0; i < parentRelations.Count; i++)
            {
                if (parentRelations[i].ChildKeyConstraint == this)
                {
                    return parentRelations[i];
                }
            }
            return null;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        internal override bool IsConstraintViolated()
        {
            Index sortIndex = this.childKey.GetSortIndex();
            object[] uniqueKeyValues = sortIndex.GetUniqueKeyValues();
            bool flag = false;
            Index index2 = this.parentKey.GetSortIndex();
            for (int i = 0; i < uniqueKeyValues.Length; i++)
            {
                object[] values = (object[]) uniqueKeyValues[i];
                if (!this.IsKeyNull(values) && !index2.IsKeyInIndex(values))
                {
                    DataRow[] rows = sortIndex.GetRows(sortIndex.FindRecords(values));
                    string str = Res.GetString("DataConstraint_ForeignKeyViolation", new object[] { this.ConstraintName, ExceptionBuilder.KeysToString(values) });
                    for (int j = 0; j < rows.Length; j++)
                    {
                        rows[j].RowError = str;
                    }
                    flag = true;
                }
            }
            return flag;
        }

        internal bool IsKeyNull(object[] values)
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

        private void NonVirtualCheckState()
        {
            if (this._DataSet == null)
            {
                this.parentKey.CheckState();
                this.childKey.CheckState();
                if (this.parentKey.Table.DataSet != this.childKey.Table.DataSet)
                {
                    throw ExceptionBuilder.TablesInDifferentSets();
                }
                for (int i = 0; i < this.parentKey.ColumnsReference.Length; i++)
                {
                    if ((this.parentKey.ColumnsReference[i].DataType != this.childKey.ColumnsReference[i].DataType) || (((this.parentKey.ColumnsReference[i].DataType == typeof(DateTime)) && (this.parentKey.ColumnsReference[i].DateTimeMode != this.childKey.ColumnsReference[i].DateTimeMode)) && ((this.parentKey.ColumnsReference[i].DateTimeMode & this.childKey.ColumnsReference[i].DateTimeMode) != DataSetDateTime.Unspecified)))
                    {
                        throw ExceptionBuilder.ColumnsTypeMismatch();
                    }
                }
                if (this.childKey.ColumnsEqual(this.parentKey))
                {
                    throw ExceptionBuilder.KeyColumnsIdentical();
                }
            }
        }

        [ResDescription("ForeignKeyConstraintAcceptRejectRuleDescr"), ResCategory("DataCategory_Data"), DefaultValue(0)]
        public virtual System.Data.AcceptRejectRule AcceptRejectRule
        {
            get
            {
                base.CheckStateForProperty();
                return this.acceptRejectRule;
            }
            set
            {
                switch (value)
                {
                    case System.Data.AcceptRejectRule.None:
                    case System.Data.AcceptRejectRule.Cascade:
                        this.acceptRejectRule = value;
                        return;
                }
                throw ADP.InvalidAcceptRejectRule(value);
            }
        }

        internal string[] ChildColumnNames
        {
            get
            {
                return this.childKey.GetColumnNames();
            }
        }

        internal DataKey ChildKey
        {
            get
            {
                base.CheckStateForProperty();
                return this.childKey;
            }
        }

        [ReadOnly(true), ResCategory("DataCategory_Data"), ResDescription("ForeignKeyConstraintChildColumnsDescr")]
        public virtual DataColumn[] Columns
        {
            get
            {
                base.CheckStateForProperty();
                return this.childKey.ToArray();
            }
        }

        [ResDescription("ForeignKeyConstraintDeleteRuleDescr"), DefaultValue(1), ResCategory("DataCategory_Data")]
        public virtual Rule DeleteRule
        {
            get
            {
                base.CheckStateForProperty();
                return this.deleteRule;
            }
            set
            {
                switch (value)
                {
                    case Rule.None:
                    case Rule.Cascade:
                    case Rule.SetNull:
                    case Rule.SetDefault:
                        this.deleteRule = value;
                        return;
                }
                throw ADP.InvalidRule(value);
            }
        }

        internal string[] ParentColumnNames
        {
            get
            {
                return this.parentKey.GetColumnNames();
            }
        }

        internal DataKey ParentKey
        {
            get
            {
                base.CheckStateForProperty();
                return this.parentKey;
            }
        }

        [ResCategory("DataCategory_Data"), ResDescription("ForeignKeyConstraintParentColumnsDescr"), ReadOnly(true)]
        public virtual DataColumn[] RelatedColumns
        {
            get
            {
                base.CheckStateForProperty();
                return this.parentKey.ToArray();
            }
        }

        internal DataColumn[] RelatedColumnsReference
        {
            get
            {
                base.CheckStateForProperty();
                return this.parentKey.ColumnsReference;
            }
        }

        [ResCategory("DataCategory_Data"), ResDescription("ForeignKeyRelatedTableDescr"), ReadOnly(true)]
        public virtual DataTable RelatedTable
        {
            get
            {
                base.CheckStateForProperty();
                return this.parentKey.Table;
            }
        }

        [ResCategory("DataCategory_Data"), ResDescription("ConstraintTableDescr"), ReadOnly(true)]
        public override DataTable Table
        {
            get
            {
                base.CheckStateForProperty();
                return this.childKey.Table;
            }
        }

        [DefaultValue(1), ResCategory("DataCategory_Data"), ResDescription("ForeignKeyConstraintUpdateRuleDescr")]
        public virtual Rule UpdateRule
        {
            get
            {
                base.CheckStateForProperty();
                return this.updateRule;
            }
            set
            {
                switch (value)
                {
                    case Rule.None:
                    case Rule.Cascade:
                    case Rule.SetNull:
                    case Rule.SetDefault:
                        this.updateRule = value;
                        return;
                }
                throw ADP.InvalidRule(value);
            }
        }
    }
}

