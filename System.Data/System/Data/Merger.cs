namespace System.Data
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Common;

    internal sealed class Merger
    {
        private bool _IgnoreNSforTableLookup;
        private DataSet dataSet;
        private DataTable dataTable;
        private bool isStandAlonetable;
        private MissingSchemaAction missingSchemaAction;
        private bool preserveChanges;

        internal Merger(DataSet dataSet, bool preserveChanges, MissingSchemaAction missingSchemaAction)
        {
            this.dataSet = dataSet;
            this.preserveChanges = preserveChanges;
            if (missingSchemaAction == MissingSchemaAction.AddWithKey)
            {
                this.missingSchemaAction = MissingSchemaAction.Add;
            }
            else
            {
                this.missingSchemaAction = missingSchemaAction;
            }
        }

        internal Merger(DataTable dataTable, bool preserveChanges, MissingSchemaAction missingSchemaAction)
        {
            this.isStandAlonetable = true;
            this.dataTable = dataTable;
            this.preserveChanges = preserveChanges;
            if (missingSchemaAction == MissingSchemaAction.AddWithKey)
            {
                this.missingSchemaAction = MissingSchemaAction.Add;
            }
            else
            {
                this.missingSchemaAction = missingSchemaAction;
            }
        }

        private DataKey GetSrcKey(DataTable src, DataTable dst)
        {
            if (src.primaryKey != null)
            {
                return src.primaryKey.Key;
            }
            DataKey key = new DataKey();
            if (dst.primaryKey == null)
            {
                return key;
            }
            DataColumn[] columnsReference = dst.primaryKey.Key.ColumnsReference;
            DataColumn[] columns = new DataColumn[columnsReference.Length];
            for (int i = 0; i < columnsReference.Length; i++)
            {
                columns[i] = src.Columns[columnsReference[i].ColumnName];
            }
            return new DataKey(columns, false);
        }

        private void MergeConstraints(DataSet source)
        {
            for (int i = 0; i < source.Tables.Count; i++)
            {
                this.MergeConstraints(source.Tables[i]);
            }
        }

        private void MergeConstraints(DataTable table)
        {
            for (int i = 0; i < table.Constraints.Count; i++)
            {
                Constraint constraint2 = table.Constraints[i];
                Constraint constraint = constraint2.Clone(this.dataSet, this._IgnoreNSforTableLookup);
                if (constraint == null)
                {
                    this.dataSet.RaiseMergeFailed(table, Res.GetString("DataMerge_MissingConstraint", new object[] { constraint2.GetType().FullName, constraint2.ConstraintName }), this.missingSchemaAction);
                }
                else
                {
                    Constraint constraint3 = constraint.Table.Constraints.FindConstraint(constraint);
                    if (constraint3 == null)
                    {
                        if (MissingSchemaAction.Add == this.missingSchemaAction)
                        {
                            try
                            {
                                constraint.Table.Constraints.Add(constraint);
                            }
                            catch (DuplicateNameException)
                            {
                                constraint.ConstraintName = "";
                                constraint.Table.Constraints.Add(constraint);
                            }
                        }
                        else if (MissingSchemaAction.Error == this.missingSchemaAction)
                        {
                            this.dataSet.RaiseMergeFailed(table, Res.GetString("DataMerge_MissingConstraint", new object[] { constraint2.GetType().FullName, constraint2.ConstraintName }), this.missingSchemaAction);
                        }
                    }
                    else
                    {
                        this.MergeExtendedProperties(constraint2.ExtendedProperties, constraint3.ExtendedProperties);
                    }
                }
            }
        }

        internal void MergeDataSet(DataSet source)
        {
            if (source != this.dataSet)
            {
                bool enforceConstraints = this.dataSet.EnforceConstraints;
                this.dataSet.EnforceConstraints = false;
                this._IgnoreNSforTableLookup = this.dataSet.namespaceURI != source.namespaceURI;
                List<DataColumn> list = null;
                if (MissingSchemaAction.Add == this.missingSchemaAction)
                {
                    list = new List<DataColumn>();
                    foreach (DataTable table4 in this.dataSet.Tables)
                    {
                        foreach (DataColumn column3 in table4.Columns)
                        {
                            list.Add(column3);
                        }
                    }
                }
                for (int i = 0; i < source.Tables.Count; i++)
                {
                    this.MergeTableData(source.Tables[i]);
                }
                if (MissingSchemaAction.Ignore != this.missingSchemaAction)
                {
                    this.MergeConstraints(source);
                    for (int j = 0; j < source.Relations.Count; j++)
                    {
                        this.MergeRelation(source.Relations[j]);
                    }
                }
                if (MissingSchemaAction.Add == this.missingSchemaAction)
                {
                    foreach (DataTable table in source.Tables)
                    {
                        DataTable table2;
                        if (this._IgnoreNSforTableLookup)
                        {
                            table2 = this.dataSet.Tables[table.TableName];
                        }
                        else
                        {
                            table2 = this.dataSet.Tables[table.TableName, table.Namespace];
                        }
                        foreach (DataColumn column in table.Columns)
                        {
                            if (column.Computed)
                            {
                                DataColumn item = table2.Columns[column.ColumnName];
                                if (!list.Contains(item))
                                {
                                    item.Expression = column.Expression;
                                }
                            }
                        }
                    }
                }
                this.MergeExtendedProperties(source.ExtendedProperties, this.dataSet.ExtendedProperties);
                foreach (DataTable table3 in this.dataSet.Tables)
                {
                    table3.EvaluateExpressions();
                }
                this.dataSet.EnforceConstraints = enforceConstraints;
            }
        }

        private void MergeExtendedProperties(PropertyCollection src, PropertyCollection dst)
        {
            if (MissingSchemaAction.Ignore != this.missingSchemaAction)
            {
                IDictionaryEnumerator enumerator = src.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    if (!this.preserveChanges || (dst[enumerator.Key] == null))
                    {
                        dst[enumerator.Key] = enumerator.Value;
                    }
                }
            }
        }

        private void MergeRelation(DataRelation relation)
        {
            DataRelation relation2 = null;
            int num3 = this.dataSet.Relations.InternalIndexOf(relation.RelationName);
            if (num3 >= 0)
            {
                relation2 = this.dataSet.Relations[num3];
                if (relation.ParentKey.ColumnsReference.Length != relation2.ParentKey.ColumnsReference.Length)
                {
                    this.dataSet.RaiseMergeFailed(null, Res.GetString("DataMerge_MissingDefinition", new object[] { relation.RelationName }), this.missingSchemaAction);
                }
                for (int i = 0; i < relation.ParentKey.ColumnsReference.Length; i++)
                {
                    DataColumn column = relation2.ParentKey.ColumnsReference[i];
                    DataColumn column2 = relation.ParentKey.ColumnsReference[i];
                    if (string.Compare(column.ColumnName, column2.ColumnName, false, column.Table.Locale) != 0)
                    {
                        this.dataSet.RaiseMergeFailed(null, Res.GetString("DataMerge_ReltionKeyColumnsMismatch", new object[] { relation.RelationName }), this.missingSchemaAction);
                    }
                    column = relation2.ChildKey.ColumnsReference[i];
                    column2 = relation.ChildKey.ColumnsReference[i];
                    if (string.Compare(column.ColumnName, column2.ColumnName, false, column.Table.Locale) != 0)
                    {
                        this.dataSet.RaiseMergeFailed(null, Res.GetString("DataMerge_ReltionKeyColumnsMismatch", new object[] { relation.RelationName }), this.missingSchemaAction);
                    }
                }
            }
            else
            {
                DataTable table;
                DataTable table2;
                if (MissingSchemaAction.Add != this.missingSchemaAction)
                {
                    throw ExceptionBuilder.MergeMissingDefinition(relation.RelationName);
                }
                if (this._IgnoreNSforTableLookup)
                {
                    table2 = this.dataSet.Tables[relation.ParentTable.TableName];
                }
                else
                {
                    table2 = this.dataSet.Tables[relation.ParentTable.TableName, relation.ParentTable.Namespace];
                }
                if (this._IgnoreNSforTableLookup)
                {
                    table = this.dataSet.Tables[relation.ChildTable.TableName];
                }
                else
                {
                    table = this.dataSet.Tables[relation.ChildTable.TableName, relation.ChildTable.Namespace];
                }
                DataColumn[] parentColumns = new DataColumn[relation.ParentKey.ColumnsReference.Length];
                DataColumn[] childColumns = new DataColumn[relation.ParentKey.ColumnsReference.Length];
                for (int j = 0; j < relation.ParentKey.ColumnsReference.Length; j++)
                {
                    parentColumns[j] = table2.Columns[relation.ParentKey.ColumnsReference[j].ColumnName];
                    childColumns[j] = table.Columns[relation.ChildKey.ColumnsReference[j].ColumnName];
                }
                try
                {
                    relation2 = new DataRelation(relation.RelationName, parentColumns, childColumns, relation.createConstraints) {
                        Nested = relation.Nested
                    };
                    this.dataSet.Relations.Add(relation2);
                }
                catch (Exception exception)
                {
                    if (!ADP.IsCatchableExceptionType(exception))
                    {
                        throw;
                    }
                    ExceptionBuilder.TraceExceptionForCapture(exception);
                    this.dataSet.RaiseMergeFailed(null, exception.Message, this.missingSchemaAction);
                }
            }
            this.MergeExtendedProperties(relation.ExtendedProperties, relation2.ExtendedProperties);
        }

        internal void MergeRows(DataRow[] rows)
        {
            DataTable dst = null;
            DataTable src = null;
            DataKey srcKey = new DataKey();
            Index ndx = null;
            bool enforceConstraints = this.dataSet.EnforceConstraints;
            this.dataSet.EnforceConstraints = false;
            for (int i = 0; i < rows.Length; i++)
            {
                DataRow row = rows[i];
                if (row == null)
                {
                    throw ExceptionBuilder.ArgumentNull("rows[" + i + "]");
                }
                if (row.Table == null)
                {
                    throw ExceptionBuilder.ArgumentNull("rows[" + i + "].Table");
                }
                if (row.Table.DataSet != this.dataSet)
                {
                    if (src != row.Table)
                    {
                        src = row.Table;
                        dst = this.MergeSchema(row.Table);
                        if (dst == null)
                        {
                            this.dataSet.EnforceConstraints = enforceConstraints;
                            return;
                        }
                        if (dst.primaryKey != null)
                        {
                            srcKey = this.GetSrcKey(src, dst);
                        }
                        if (srcKey.HasValue)
                        {
                            if (ndx != null)
                            {
                                ndx.RemoveRef();
                                ndx = null;
                            }
                            ndx = new Index(dst, dst.primaryKey.Key.GetIndexDesc(), DataViewRowState.OriginalRows | DataViewRowState.Added, null);
                            ndx.AddRef();
                            ndx.AddRef();
                        }
                    }
                    if ((row.newRecord != -1) || (row.oldRecord != -1))
                    {
                        DataRow targetRow = null;
                        if ((0 < dst.Rows.Count) && (ndx != null))
                        {
                            targetRow = dst.FindMergeTarget(row, srcKey, ndx);
                        }
                        targetRow = dst.MergeRow(row, targetRow, this.preserveChanges, ndx);
                        if ((targetRow.Table.dependentColumns != null) && (targetRow.Table.dependentColumns.Count > 0))
                        {
                            targetRow.Table.EvaluateExpressions(targetRow, DataRowAction.Change, null);
                        }
                    }
                }
            }
            if (ndx != null)
            {
                ndx.RemoveRef();
                ndx = null;
            }
            this.dataSet.EnforceConstraints = enforceConstraints;
        }

        private DataTable MergeSchema(DataTable table)
        {
            DataTable dataTable = null;
            if (!this.isStandAlonetable)
            {
                if (this.dataSet.Tables.Contains(table.TableName, true))
                {
                    if (this._IgnoreNSforTableLookup)
                    {
                        dataTable = this.dataSet.Tables[table.TableName];
                    }
                    else
                    {
                        dataTable = this.dataSet.Tables[table.TableName, table.Namespace];
                    }
                }
            }
            else
            {
                dataTable = this.dataTable;
            }
            if (dataTable == null)
            {
                if (MissingSchemaAction.Add == this.missingSchemaAction)
                {
                    dataTable = table.Clone(table.DataSet);
                    this.dataSet.Tables.Add(dataTable);
                    return dataTable;
                }
                if (MissingSchemaAction.Error == this.missingSchemaAction)
                {
                    throw ExceptionBuilder.MergeMissingDefinition(table.TableName);
                }
                return dataTable;
            }
            if (MissingSchemaAction.Ignore != this.missingSchemaAction)
            {
                int count = dataTable.Columns.Count;
                for (int i = 0; i < table.Columns.Count; i++)
                {
                    DataColumn column = table.Columns[i];
                    DataColumn column2 = dataTable.Columns.Contains(column.ColumnName, true) ? dataTable.Columns[column.ColumnName] : null;
                    if (column2 == null)
                    {
                        if (MissingSchemaAction.Add != this.missingSchemaAction)
                        {
                            if (this.isStandAlonetable)
                            {
                                throw ExceptionBuilder.MergeFailed(Res.GetString("DataMerge_MissingColumnDefinition", new object[] { table.TableName, column.ColumnName }));
                            }
                            this.dataSet.RaiseMergeFailed(dataTable, Res.GetString("DataMerge_MissingColumnDefinition", new object[] { table.TableName, column.ColumnName }), this.missingSchemaAction);
                        }
                        else
                        {
                            column2 = column.Clone();
                            dataTable.Columns.Add(column2);
                        }
                    }
                    else
                    {
                        if ((column2.DataType != column.DataType) || (((column2.DataType == typeof(DateTime)) && (column2.DateTimeMode != column.DateTimeMode)) && ((column2.DateTimeMode & column.DateTimeMode) != DataSetDateTime.Unspecified)))
                        {
                            if (this.isStandAlonetable)
                            {
                                throw ExceptionBuilder.MergeFailed(Res.GetString("DataMerge_DataTypeMismatch", new object[] { column.ColumnName }));
                            }
                            this.dataSet.RaiseMergeFailed(dataTable, Res.GetString("DataMerge_DataTypeMismatch", new object[] { column.ColumnName }), MissingSchemaAction.Error);
                        }
                        this.MergeExtendedProperties(column.ExtendedProperties, column2.ExtendedProperties);
                    }
                }
                if (this.isStandAlonetable)
                {
                    for (int j = count; j < dataTable.Columns.Count; j++)
                    {
                        dataTable.Columns[j].Expression = table.Columns[dataTable.Columns[j].ColumnName].Expression;
                    }
                }
                DataColumn[] primaryKey = dataTable.PrimaryKey;
                DataColumn[] columnArray = table.PrimaryKey;
                if (primaryKey.Length != columnArray.Length)
                {
                    if (primaryKey.Length == 0)
                    {
                        DataColumn[] columnArray3 = new DataColumn[columnArray.Length];
                        for (int k = 0; k < columnArray.Length; k++)
                        {
                            columnArray3[k] = dataTable.Columns[columnArray[k].ColumnName];
                        }
                        dataTable.PrimaryKey = columnArray3;
                    }
                    else if (columnArray.Length != 0)
                    {
                        this.dataSet.RaiseMergeFailed(dataTable, Res.GetString("DataMerge_PrimaryKeyMismatch"), this.missingSchemaAction);
                    }
                }
                else
                {
                    for (int m = 0; m < primaryKey.Length; m++)
                    {
                        if (string.Compare(primaryKey[m].ColumnName, columnArray[m].ColumnName, false, dataTable.Locale) != 0)
                        {
                            this.dataSet.RaiseMergeFailed(table, Res.GetString("DataMerge_PrimaryKeyColumnsMismatch", new object[] { primaryKey[m].ColumnName, columnArray[m].ColumnName }), this.missingSchemaAction);
                        }
                    }
                }
            }
            this.MergeExtendedProperties(table.ExtendedProperties, dataTable.ExtendedProperties);
            return dataTable;
        }

        internal void MergeTable(DataTable src)
        {
            bool enforceConstraints = false;
            if (!this.isStandAlonetable)
            {
                if (src.DataSet == this.dataSet)
                {
                    return;
                }
                enforceConstraints = this.dataSet.EnforceConstraints;
                this.dataSet.EnforceConstraints = false;
            }
            else
            {
                if (src == this.dataTable)
                {
                    return;
                }
                this.dataTable.SuspendEnforceConstraints = true;
            }
            if (this.dataSet != null)
            {
                if ((src.DataSet == null) || (src.DataSet.namespaceURI != this.dataSet.namespaceURI))
                {
                    this._IgnoreNSforTableLookup = true;
                }
            }
            else if (((this.dataTable.DataSet == null) || (src.DataSet == null)) || (src.DataSet.namespaceURI != this.dataTable.DataSet.namespaceURI))
            {
                this._IgnoreNSforTableLookup = true;
            }
            this.MergeTableData(src);
            DataTable dataTable = this.dataTable;
            if ((dataTable == null) && (this.dataSet != null))
            {
                if (this._IgnoreNSforTableLookup)
                {
                    dataTable = this.dataSet.Tables[src.TableName];
                }
                else
                {
                    dataTable = this.dataSet.Tables[src.TableName, src.Namespace];
                }
            }
            if (dataTable != null)
            {
                dataTable.EvaluateExpressions();
            }
            if (!this.isStandAlonetable)
            {
                this.dataSet.EnforceConstraints = enforceConstraints;
            }
            else
            {
                this.dataTable.SuspendEnforceConstraints = false;
                try
                {
                    if (this.dataTable.EnforceConstraints)
                    {
                        this.dataTable.EnableConstraints();
                    }
                }
                catch (ConstraintException)
                {
                    if (this.dataTable.DataSet != null)
                    {
                        this.dataTable.DataSet.EnforceConstraints = false;
                    }
                    throw;
                }
            }
        }

        private void MergeTable(DataTable src, DataTable dst)
        {
            int count = src.Rows.Count;
            bool flag = dst.Rows.Count == 0;
            if (0 < count)
            {
                Index ndx = null;
                DataKey srcKey = new DataKey();
                dst.SuspendIndexEvents();
                try
                {
                    if (!flag && (dst.primaryKey != null))
                    {
                        srcKey = this.GetSrcKey(src, dst);
                        if (srcKey.HasValue)
                        {
                            ndx = dst.primaryKey.Key.GetSortIndex(DataViewRowState.OriginalRows | DataViewRowState.Added);
                        }
                    }
                    foreach (DataRow row2 in src.Rows)
                    {
                        DataRow targetRow = null;
                        if (ndx != null)
                        {
                            targetRow = dst.FindMergeTarget(row2, srcKey, ndx);
                        }
                        dst.MergeRow(row2, targetRow, this.preserveChanges, ndx);
                    }
                }
                finally
                {
                    dst.RestoreIndexEvents(true);
                }
            }
            this.MergeExtendedProperties(src.ExtendedProperties, dst.ExtendedProperties);
        }

        private void MergeTableData(DataTable src)
        {
            DataTable dst = this.MergeSchema(src);
            if (dst != null)
            {
                dst.MergingData = true;
                try
                {
                    this.MergeTable(src, dst);
                }
                finally
                {
                    dst.MergingData = false;
                }
            }
        }
    }
}

