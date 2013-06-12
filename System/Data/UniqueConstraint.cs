namespace System.Data
{
    using System;
    using System.ComponentModel;

    [DefaultProperty("ConstraintName"), Editor("Microsoft.VSDesigner.Data.Design.UniqueConstraintEditor, Microsoft.VSDesigner, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    public class UniqueConstraint : Constraint
    {
        private Index _constraintIndex;
        internal bool bPrimaryKey;
        internal string[] columnNames;
        internal string constraintName;
        private DataKey key;

        public UniqueConstraint(DataColumn column)
        {
            DataColumn[] columns = new DataColumn[] { column };
            this.Create(null, columns);
        }

        public UniqueConstraint(DataColumn[] columns)
        {
            this.Create(null, columns);
        }

        public UniqueConstraint(DataColumn column, bool isPrimaryKey)
        {
            DataColumn[] columns = new DataColumn[] { column };
            this.bPrimaryKey = isPrimaryKey;
            this.Create(null, columns);
        }

        public UniqueConstraint(string name, DataColumn column)
        {
            DataColumn[] columns = new DataColumn[] { column };
            this.Create(name, columns);
        }

        public UniqueConstraint(string name, DataColumn[] columns)
        {
            this.Create(name, columns);
        }

        public UniqueConstraint(DataColumn[] columns, bool isPrimaryKey)
        {
            this.bPrimaryKey = isPrimaryKey;
            this.Create(null, columns);
        }

        public UniqueConstraint(string name, DataColumn column, bool isPrimaryKey)
        {
            DataColumn[] columns = new DataColumn[] { column };
            this.bPrimaryKey = isPrimaryKey;
            this.Create(name, columns);
        }

        public UniqueConstraint(string name, DataColumn[] columns, bool isPrimaryKey)
        {
            this.bPrimaryKey = isPrimaryKey;
            this.Create(name, columns);
        }

        [Browsable(false)]
        public UniqueConstraint(string name, string[] columnNames, bool isPrimaryKey)
        {
            this.constraintName = name;
            this.columnNames = columnNames;
            this.bPrimaryKey = isPrimaryKey;
        }

        internal override bool CanBeRemovedFromCollection(ConstraintCollection constraints, bool fThrowException)
        {
            if (this.Equals(constraints.Table.primaryKey))
            {
                if (fThrowException)
                {
                    throw ExceptionBuilder.RemovePrimaryKey(constraints.Table);
                }
                return false;
            }
            ParentForeignKeyConstraintEnumerator enumerator = new ParentForeignKeyConstraintEnumerator(this.Table.DataSet, this.Table);
            while (enumerator.GetNext())
            {
                ForeignKeyConstraint foreignKeyConstraint = enumerator.GetForeignKeyConstraint();
                if (this.key.ColumnsEqual(foreignKeyConstraint.ParentKey))
                {
                    if (fThrowException)
                    {
                        throw ExceptionBuilder.NeededForForeignKeyConstraint(this, foreignKeyConstraint);
                    }
                    return false;
                }
            }
            return true;
        }

        internal override bool CanEnableConstraint()
        {
            if (this.Table.EnforceConstraints)
            {
                return this.ConstraintIndex.CheckUnique();
            }
            return true;
        }

        internal override void CheckCanAddToCollection(ConstraintCollection constraints)
        {
        }

        internal override void CheckConstraint(DataRow row, DataRowAction action)
        {
            if ((this.Table.EnforceConstraints && (((action == DataRowAction.Add) || (action == DataRowAction.Change)) || ((action == DataRowAction.Rollback) && (row.tempRecord != -1)))) && (row.HaveValuesChanged(this.ColumnsReference) && this.ConstraintIndex.IsKeyRecordInIndex(row.GetDefaultRecord())))
            {
                object[] columnValues = row.GetColumnValues(this.ColumnsReference);
                throw ExceptionBuilder.ConstraintViolation(this.ColumnsReference, columnValues);
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

        internal UniqueConstraint Clone(DataTable table)
        {
            int length = this.ColumnsReference.Length;
            DataColumn[] columns = new DataColumn[length];
            for (int i = 0; i < length; i++)
            {
                DataColumn column = this.ColumnsReference[i];
                int index = table.Columns.IndexOf(column.ColumnName);
                if (index < 0)
                {
                    return null;
                }
                columns[i] = table.Columns[index];
            }
            UniqueConstraint constraint = new UniqueConstraint(this.ConstraintName, columns);
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
            DataTable table = destination.Tables[index];
            int length = this.ColumnsReference.Length;
            DataColumn[] columns = new DataColumn[length];
            for (int i = 0; i < length; i++)
            {
                DataColumn column = this.ColumnsReference[i];
                index = table.Columns.IndexOf(column.ColumnName);
                if (index < 0)
                {
                    return null;
                }
                columns[i] = table.Columns[index];
            }
            UniqueConstraint constraint = new UniqueConstraint(this.ConstraintName, columns);
            foreach (object obj2 in base.ExtendedProperties.Keys)
            {
                constraint.ExtendedProperties[obj2] = base.ExtendedProperties[obj2];
            }
            return constraint;
        }

        internal void ConstraintIndexClear()
        {
            if (this._constraintIndex != null)
            {
                this._constraintIndex.RemoveRef();
                this._constraintIndex = null;
            }
        }

        internal void ConstraintIndexInitialize()
        {
            if (this._constraintIndex == null)
            {
                this._constraintIndex = this.key.GetSortIndex();
                this._constraintIndex.AddRef();
            }
        }

        internal override bool ContainsColumn(DataColumn column)
        {
            return this.key.ContainsColumn(column);
        }

        private void Create(string constraintName, DataColumn[] columns)
        {
            for (int i = 0; i < columns.Length; i++)
            {
                if (columns[i].Computed)
                {
                    throw ExceptionBuilder.ExpressionInConstraint(columns[i]);
                }
            }
            this.key = new DataKey(columns, true);
            this.ConstraintName = constraintName;
            this.NonVirtualCheckState();
        }

        public override bool Equals(object key2)
        {
            return ((key2 is UniqueConstraint) && this.Key.ColumnsEqual(((UniqueConstraint) key2).Key));
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        internal override bool IsConstraintViolated()
        {
            bool flag = false;
            Index constraintIndex = this.ConstraintIndex;
            if (constraintIndex.HasDuplicates)
            {
                object[] uniqueKeyValues = constraintIndex.GetUniqueKeyValues();
                for (int i = 0; i < uniqueKeyValues.Length; i++)
                {
                    Range range = constraintIndex.FindRecords((object[]) uniqueKeyValues[i]);
                    if (1 < range.Count)
                    {
                        DataRow[] rows = constraintIndex.GetRows(range);
                        string error = ExceptionBuilder.UniqueConstraintViolationText(this.key.ColumnsReference, (object[]) uniqueKeyValues[i]);
                        for (int j = 0; j < rows.Length; j++)
                        {
                            rows[j].RowError = error;
                            foreach (DataColumn column in this.key.ColumnsReference)
                            {
                                rows[j].SetColumnError(column, error);
                            }
                        }
                        flag = true;
                    }
                }
            }
            return flag;
        }

        private void NonVirtualCheckState()
        {
            this.key.CheckState();
        }

        internal string[] ColumnNames
        {
            get
            {
                return this.key.GetColumnNames();
            }
        }

        [ResCategory("DataCategory_Data"), ResDescription("KeyConstraintColumnsDescr"), ReadOnly(true)]
        public virtual DataColumn[] Columns
        {
            get
            {
                return this.key.ToArray();
            }
        }

        internal DataColumn[] ColumnsReference
        {
            get
            {
                return this.key.ColumnsReference;
            }
        }

        internal Index ConstraintIndex
        {
            get
            {
                return this._constraintIndex;
            }
        }

        internal override bool InCollection
        {
            set
            {
                base.InCollection = value;
                if (this.key.ColumnsReference.Length == 1)
                {
                    this.key.ColumnsReference[0].InternalUnique(value);
                }
            }
        }

        [ResDescription("KeyConstraintIsPrimaryKeyDescr"), ResCategory("DataCategory_Data")]
        public bool IsPrimaryKey
        {
            get
            {
                if (this.Table == null)
                {
                    return false;
                }
                return (this == this.Table.primaryKey);
            }
        }

        internal DataKey Key
        {
            get
            {
                return this.key;
            }
        }

        [ResCategory("DataCategory_Data"), ResDescription("ConstraintTableDescr"), ReadOnly(true)]
        public override DataTable Table
        {
            get
            {
                if (this.key.HasValue)
                {
                    return this.key.Table;
                }
                return null;
            }
        }
    }
}

