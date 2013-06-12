namespace System.Data
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data.Common;
    using System.Globalization;
    using System.Reflection;

    [DefaultEvent("CollectionChanged"), Editor("Microsoft.VSDesigner.Data.Design.ColumnsCollectionEditor, Microsoft.VSDesigner, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    public sealed class DataColumnCollection : InternalDataCollectionBase
    {
        private readonly ArrayList _list = new ArrayList();
        private readonly Hashtable columnFromName;
        private DataColumn[] columnsImplementingIChangeTracking = DataTable.zeroColumns;
        private int defaultNameIndex = 1;
        private DataColumn[] delayedAddRangeColumns;
        private bool fInClear;
        private int nColumnsImplementingIChangeTracking;
        private int nColumnsImplementingIRevertibleChangeTracking;
        private readonly DataTable table;

        [ResDescription("collectionChangedEventDescr")]
        public event CollectionChangeEventHandler CollectionChanged;

        internal event CollectionChangeEventHandler CollectionChanging;

        internal event CollectionChangeEventHandler ColumnPropertyChanged;

        internal DataColumnCollection(DataTable table)
        {
            this.table = table;
            this.columnFromName = new Hashtable();
        }

        public DataColumn Add()
        {
            DataColumn column = new DataColumn();
            this.Add(column);
            return column;
        }

        public void Add(DataColumn column)
        {
            this.AddAt(-1, column);
        }

        public DataColumn Add(string columnName)
        {
            DataColumn column = new DataColumn(columnName);
            this.Add(column);
            return column;
        }

        public DataColumn Add(string columnName, Type type)
        {
            DataColumn column = new DataColumn(columnName, type);
            this.Add(column);
            return column;
        }

        public DataColumn Add(string columnName, Type type, string expression)
        {
            DataColumn column = new DataColumn(columnName, type, expression);
            this.Add(column);
            return column;
        }

        internal void AddAt(int index, DataColumn column)
        {
            if ((column != null) && (column.ColumnMapping == MappingType.SimpleContent))
            {
                if ((this.table.XmlText != null) && (this.table.XmlText != column))
                {
                    throw ExceptionBuilder.CannotAddColumn3();
                }
                if (this.table.ElementColumnCount > 0)
                {
                    throw ExceptionBuilder.CannotAddColumn4(column.ColumnName);
                }
                this.OnCollectionChanging(new CollectionChangeEventArgs(CollectionChangeAction.Add, column));
                this.BaseAdd(column);
                if (index != -1)
                {
                    this.ArrayAdd(index, column);
                }
                else
                {
                    this.ArrayAdd(column);
                }
                this.table.XmlText = column;
            }
            else
            {
                this.OnCollectionChanging(new CollectionChangeEventArgs(CollectionChangeAction.Add, column));
                this.BaseAdd(column);
                if (index != -1)
                {
                    this.ArrayAdd(index, column);
                }
                else
                {
                    this.ArrayAdd(column);
                }
                if (column.ColumnMapping == MappingType.Element)
                {
                    this.table.ElementColumnCount++;
                }
            }
            if ((!this.table.fInitInProgress && (column != null)) && column.Computed)
            {
                column.Expression = column.Expression;
            }
            this.OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Add, column));
        }

        private void AddColumnsImplementingIChangeTrackingList(DataColumn dataColumn)
        {
            DataColumn[] columnsImplementingIChangeTracking = this.columnsImplementingIChangeTracking;
            DataColumn[] array = new DataColumn[columnsImplementingIChangeTracking.Length + 1];
            columnsImplementingIChangeTracking.CopyTo(array, 0);
            array[columnsImplementingIChangeTracking.Length] = dataColumn;
            this.columnsImplementingIChangeTracking = array;
        }

        public void AddRange(DataColumn[] columns)
        {
            if (this.table.fInitInProgress)
            {
                this.delayedAddRangeColumns = columns;
            }
            else if (columns != null)
            {
                foreach (DataColumn column in columns)
                {
                    if (column != null)
                    {
                        this.Add(column);
                    }
                }
            }
        }

        private void ArrayAdd(DataColumn column)
        {
            this._list.Add(column);
            column.SetOrdinalInternal(this._list.Count - 1);
            this.CheckIChangeTracking(column);
        }

        private void ArrayAdd(int index, DataColumn column)
        {
            this._list.Insert(index, column);
            this.CheckIChangeTracking(column);
        }

        private void ArrayRemove(DataColumn column)
        {
            column.SetOrdinalInternal(-1);
            this._list.Remove(column);
            int count = this._list.Count;
            for (int i = 0; i < count; i++)
            {
                ((DataColumn) this._list[i]).SetOrdinalInternal(i);
            }
            if (column.ImplementsIChangeTracking)
            {
                this.RemoveColumnsImplementingIChangeTrackingList(column);
            }
        }

        internal string AssignName()
        {
            string str = this.MakeName(this.defaultNameIndex++);
            while (this.columnFromName[str] != null)
            {
                str = this.MakeName(this.defaultNameIndex++);
            }
            return str;
        }

        private void BaseAdd(DataColumn column)
        {
            if (column == null)
            {
                throw ExceptionBuilder.ArgumentNull("column");
            }
            if (column.table == this.table)
            {
                throw ExceptionBuilder.CannotAddColumn1(column.ColumnName);
            }
            if (column.table != null)
            {
                throw ExceptionBuilder.CannotAddColumn2(column.ColumnName);
            }
            if (column.ColumnName.Length == 0)
            {
                column.ColumnName = this.AssignName();
            }
            this.RegisterColumnName(column.ColumnName, column, null);
            try
            {
                column.SetTable(this.table);
                if ((!this.table.fInitInProgress && column.Computed) && column.DataExpression.DependsOn(column))
                {
                    throw ExceptionBuilder.ExpressionCircular();
                }
                if (0 < this.table.RecordCapacity)
                {
                    column.SetCapacity(this.table.RecordCapacity);
                }
                for (int i = 0; i < this.table.RecordCapacity; i++)
                {
                    column.InitializeRecord(i);
                }
                if (this.table.DataSet != null)
                {
                    column.OnSetDataSet();
                }
            }
            catch (Exception exception)
            {
                if (ADP.IsCatchableOrSecurityExceptionType(exception))
                {
                    this.UnregisterName(column.ColumnName);
                }
                throw;
            }
        }

        private void BaseGroupSwitch(DataColumn[] oldArray, int oldLength, DataColumn[] newArray, int newLength)
        {
            int num4 = 0;
            for (int i = 0; i < oldLength; i++)
            {
                bool flag = false;
                for (int k = num4; k < newLength; k++)
                {
                    if (oldArray[i] == newArray[k])
                    {
                        if (num4 == k)
                        {
                            num4++;
                        }
                        flag = true;
                        break;
                    }
                }
                if (!flag && (oldArray[i].Table == this.table))
                {
                    this.BaseRemove(oldArray[i]);
                    this._list.Remove(oldArray[i]);
                    oldArray[i].SetOrdinalInternal(-1);
                }
            }
            for (int j = 0; j < newLength; j++)
            {
                if (newArray[j].Table != this.table)
                {
                    this.BaseAdd(newArray[j]);
                    this._list.Add(newArray[j]);
                }
                newArray[j].SetOrdinalInternal(j);
            }
        }

        private void BaseRemove(DataColumn column)
        {
            if (this.CanRemove(column, true))
            {
                if (column.errors > 0)
                {
                    for (int i = 0; i < this.table.Rows.Count; i++)
                    {
                        this.table.Rows[i].ClearError(column);
                    }
                }
                this.UnregisterName(column.ColumnName);
                column.SetTable(null);
            }
        }

        internal bool CanRegisterName(string name)
        {
            return (null == this.columnFromName[name]);
        }

        public bool CanRemove(DataColumn column)
        {
            return this.CanRemove(column, false);
        }

        internal bool CanRemove(DataColumn column, bool fThrowException)
        {
            if (column == null)
            {
                if (fThrowException)
                {
                    throw ExceptionBuilder.ArgumentNull("column");
                }
                return false;
            }
            if (column.table != this.table)
            {
                if (fThrowException)
                {
                    throw ExceptionBuilder.CannotRemoveColumn();
                }
                return false;
            }
            this.table.OnRemoveColumnInternal(column);
            if ((this.table.primaryKey != null) && this.table.primaryKey.Key.ContainsColumn(column))
            {
                if (fThrowException)
                {
                    throw ExceptionBuilder.CannotRemovePrimaryKey();
                }
                return false;
            }
            for (int i = 0; i < this.table.ParentRelations.Count; i++)
            {
                if (this.table.ParentRelations[i].ChildKey.ContainsColumn(column))
                {
                    if (fThrowException)
                    {
                        throw ExceptionBuilder.CannotRemoveChildKey(this.table.ParentRelations[i].RelationName);
                    }
                    return false;
                }
            }
            for (int j = 0; j < this.table.ChildRelations.Count; j++)
            {
                if (this.table.ChildRelations[j].ParentKey.ContainsColumn(column))
                {
                    if (fThrowException)
                    {
                        throw ExceptionBuilder.CannotRemoveChildKey(this.table.ChildRelations[j].RelationName);
                    }
                    return false;
                }
            }
            for (int k = 0; k < this.table.Constraints.Count; k++)
            {
                if (this.table.Constraints[k].ContainsColumn(column))
                {
                    if (fThrowException)
                    {
                        throw ExceptionBuilder.CannotRemoveConstraint(this.table.Constraints[k].ConstraintName, this.table.Constraints[k].Table.TableName);
                    }
                    return false;
                }
            }
            if (this.table.DataSet != null)
            {
                ParentForeignKeyConstraintEnumerator enumerator = new ParentForeignKeyConstraintEnumerator(this.table.DataSet, this.table);
                while (enumerator.GetNext())
                {
                    Constraint constraint = enumerator.GetConstraint();
                    if (((ForeignKeyConstraint) constraint).ParentKey.ContainsColumn(column))
                    {
                        if (fThrowException)
                        {
                            throw ExceptionBuilder.CannotRemoveConstraint(constraint.ConstraintName, constraint.Table.TableName);
                        }
                        return false;
                    }
                }
            }
            if (column.dependentColumns != null)
            {
                for (int m = 0; m < column.dependentColumns.Count; m++)
                {
                    DataColumn column2 = column.dependentColumns[m];
                    if ((!this.fInClear || ((column2.Table != this.table) && (column2.Table != null))) && (column2.Table != null))
                    {
                        DataExpression dataExpression = column2.DataExpression;
                        if ((dataExpression != null) && dataExpression.DependsOn(column))
                        {
                            if (fThrowException)
                            {
                                throw ExceptionBuilder.CannotRemoveExpression(column2.ColumnName, column2.Expression);
                            }
                            return false;
                        }
                    }
                }
            }
            using (List<Index>.Enumerator enumerator2 = this.table.LiveIndexes.GetEnumerator())
            {
                while (enumerator2.MoveNext())
                {
                    Index current = enumerator2.Current;
                }
            }
            return true;
        }

        private void CheckIChangeTracking(DataColumn column)
        {
            if (column.ImplementsIRevertibleChangeTracking)
            {
                this.nColumnsImplementingIRevertibleChangeTracking++;
                this.nColumnsImplementingIChangeTracking++;
                this.AddColumnsImplementingIChangeTrackingList(column);
            }
            else if (column.ImplementsIChangeTracking)
            {
                this.nColumnsImplementingIChangeTracking++;
                this.AddColumnsImplementingIChangeTrackingList(column);
            }
        }

        public void Clear()
        {
            int count = this._list.Count;
            DataColumn[] array = new DataColumn[this._list.Count];
            this._list.CopyTo(array, 0);
            this.OnCollectionChanging(InternalDataCollectionBase.RefreshEventArgs);
            if (this.table.fInitInProgress && (this.delayedAddRangeColumns != null))
            {
                this.delayedAddRangeColumns = null;
            }
            try
            {
                this.fInClear = true;
                this.BaseGroupSwitch(array, count, null, 0);
                this.fInClear = false;
            }
            catch (Exception exception)
            {
                if (ADP.IsCatchableOrSecurityExceptionType(exception))
                {
                    this.fInClear = false;
                    this.BaseGroupSwitch(null, 0, array, count);
                    this._list.Clear();
                    for (int i = 0; i < count; i++)
                    {
                        this._list.Add(array[i]);
                    }
                }
                throw;
            }
            this._list.Clear();
            this.table.ElementColumnCount = 0;
            this.OnCollectionChanged(InternalDataCollectionBase.RefreshEventArgs);
        }

        public bool Contains(string name)
        {
            return ((this.columnFromName[name] is DataColumn) || (this.IndexOfCaseInsensitive(name) >= 0));
        }

        internal bool Contains(string name, bool caseSensitive)
        {
            if (this.columnFromName[name] is DataColumn)
            {
                return true;
            }
            if (caseSensitive)
            {
                return false;
            }
            return (this.IndexOfCaseInsensitive(name) >= 0);
        }

        public void CopyTo(DataColumn[] array, int index)
        {
            if (array == null)
            {
                throw ExceptionBuilder.ArgumentNull("array");
            }
            if (index < 0)
            {
                throw ExceptionBuilder.ArgumentOutOfRange("index");
            }
            if ((array.Length - index) < this._list.Count)
            {
                throw ExceptionBuilder.InvalidOffsetLength();
            }
            for (int i = 0; i < this._list.Count; i++)
            {
                array[index + i] = (DataColumn) this._list[i];
            }
        }

        internal void FinishInitCollection()
        {
            if (this.delayedAddRangeColumns != null)
            {
                foreach (DataColumn column2 in this.delayedAddRangeColumns)
                {
                    if (column2 != null)
                    {
                        this.Add(column2);
                    }
                }
                foreach (DataColumn column in this.delayedAddRangeColumns)
                {
                    if (column != null)
                    {
                        column.FinishInitInProgress();
                    }
                }
                this.delayedAddRangeColumns = null;
            }
        }

        public int IndexOf(DataColumn column)
        {
            int count = this._list.Count;
            for (int i = 0; i < count; i++)
            {
                if (column == ((DataColumn) this._list[i]))
                {
                    return i;
                }
            }
            return -1;
        }

        public int IndexOf(string columnName)
        {
            if ((columnName != null) && (0 < columnName.Length))
            {
                int count = this.Count;
                DataColumn column = this.columnFromName[columnName] as DataColumn;
                if (column == null)
                {
                    int num2 = this.IndexOfCaseInsensitive(columnName);
                    if (num2 >= 0)
                    {
                        return num2;
                    }
                    return -1;
                }
                for (int i = 0; i < count; i++)
                {
                    if (column == this._list[i])
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        internal int IndexOfCaseInsensitive(string name)
        {
            int specialHashCode = this.table.GetSpecialHashCode(name);
            int num2 = -1;
            DataColumn column = null;
            for (int i = 0; i < this.Count; i++)
            {
                column = (DataColumn) this._list[i];
                if ((((specialHashCode == 0) || (column._hashCode == 0)) || (column._hashCode == specialHashCode)) && (base.NamesEqual(column.ColumnName, name, false, this.table.Locale) != 0))
                {
                    if (num2 != -1)
                    {
                        return -2;
                    }
                    num2 = i;
                }
            }
            return num2;
        }

        private string MakeName(int index)
        {
            if (1 == index)
            {
                return "Column1";
            }
            return ("Column" + index.ToString(CultureInfo.InvariantCulture));
        }

        internal void MoveTo(DataColumn column, int newPosition)
        {
            if ((0 > newPosition) || (newPosition > (this.Count - 1)))
            {
                throw ExceptionBuilder.InvalidOrdinal("ordinal", newPosition);
            }
            if (column.ImplementsIChangeTracking)
            {
                this.RemoveColumnsImplementingIChangeTrackingList(column);
            }
            this._list.Remove(column);
            this._list.Insert(newPosition, column);
            int count = this._list.Count;
            for (int i = 0; i < count; i++)
            {
                ((DataColumn) this._list[i]).SetOrdinalInternal(i);
            }
            this.CheckIChangeTracking(column);
            this.OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Refresh, column));
        }

        private void OnCollectionChanged(CollectionChangeEventArgs ccevent)
        {
            this.table.UpdatePropertyDescriptorCollectionCache();
            if (((ccevent != null) && !this.table.SchemaLoading) && !this.table.fInitInProgress)
            {
                DataColumn element = (DataColumn) ccevent.Element;
            }
            if (this.onCollectionChangedDelegate != null)
            {
                this.onCollectionChangedDelegate(this, ccevent);
            }
        }

        private void OnCollectionChanging(CollectionChangeEventArgs ccevent)
        {
            if (this.onCollectionChangingDelegate != null)
            {
                this.onCollectionChangingDelegate(this, ccevent);
            }
        }

        internal void OnColumnPropertyChanged(CollectionChangeEventArgs ccevent)
        {
            this.table.UpdatePropertyDescriptorCollectionCache();
            if (this.onColumnPropertyChangedDelegate != null)
            {
                this.onColumnPropertyChangedDelegate(this, ccevent);
            }
        }

        internal void RegisterColumnName(string name, DataColumn column, DataTable table)
        {
            object obj2 = this.columnFromName[name];
            if (obj2 != null)
            {
                if (!(obj2 is DataColumn))
                {
                    throw ExceptionBuilder.CannotAddDuplicate2(name);
                }
                if (column != null)
                {
                    throw ExceptionBuilder.CannotAddDuplicate(name);
                }
                throw ExceptionBuilder.CannotAddDuplicate3(name);
            }
            if ((table != null) && (base.NamesEqual(name, this.MakeName(this.defaultNameIndex), true, this.table.Locale) != 0))
            {
                do
                {
                    this.defaultNameIndex++;
                }
                while (this.Contains(this.MakeName(this.defaultNameIndex)));
            }
            if (column != null)
            {
                column._hashCode = this.table.GetSpecialHashCode(name);
                this.columnFromName.Add(name, column);
            }
            else
            {
                this.columnFromName.Add(name, table);
            }
        }

        public void Remove(DataColumn column)
        {
            this.OnCollectionChanging(new CollectionChangeEventArgs(CollectionChangeAction.Remove, column));
            this.BaseRemove(column);
            this.ArrayRemove(column);
            this.OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Remove, column));
            if (column.ColumnMapping == MappingType.Element)
            {
                this.table.ElementColumnCount--;
            }
        }

        public void Remove(string name)
        {
            DataColumn column = this[name];
            if (column == null)
            {
                throw ExceptionBuilder.ColumnNotInTheTable(name, this.table.TableName);
            }
            this.Remove(column);
        }

        public void RemoveAt(int index)
        {
            DataColumn column = this[index];
            if (column == null)
            {
                throw ExceptionBuilder.ColumnOutOfRange(index);
            }
            this.Remove(column);
        }

        private void RemoveColumnsImplementingIChangeTrackingList(DataColumn dataColumn)
        {
            DataColumn[] columnsImplementingIChangeTracking = this.columnsImplementingIChangeTracking;
            DataColumn[] columnArray2 = new DataColumn[columnsImplementingIChangeTracking.Length - 1];
            int index = 0;
            int num2 = 0;
            while (index < columnsImplementingIChangeTracking.Length)
            {
                if (columnsImplementingIChangeTracking[index] != dataColumn)
                {
                    columnArray2[num2++] = columnsImplementingIChangeTracking[index];
                }
                index++;
            }
            this.columnsImplementingIChangeTracking = columnArray2;
        }

        internal void UnregisterName(string name)
        {
            if (this.columnFromName[name] != null)
            {
                this.columnFromName.Remove(name);
            }
            if (base.NamesEqual(name, this.MakeName(this.defaultNameIndex - 1), true, this.table.Locale) != 0)
            {
                do
                {
                    this.defaultNameIndex--;
                }
                while ((this.defaultNameIndex > 1) && !this.Contains(this.MakeName(this.defaultNameIndex - 1)));
            }
        }

        internal DataColumn[] ColumnsImplementingIChangeTracking
        {
            get
            {
                return this.columnsImplementingIChangeTracking;
            }
        }

        internal int ColumnsImplementingIChangeTrackingCount
        {
            get
            {
                return this.nColumnsImplementingIChangeTracking;
            }
        }

        internal int ColumnsImplementingIRevertibleChangeTrackingCount
        {
            get
            {
                return this.nColumnsImplementingIRevertibleChangeTracking;
            }
        }

        public DataColumn this[int index]
        {
            get
            {
                DataColumn column;
                try
                {
                    column = (DataColumn) this._list[index];
                }
                catch (ArgumentOutOfRangeException)
                {
                    throw ExceptionBuilder.ColumnOutOfRange(index);
                }
                return column;
            }
        }

        public DataColumn this[string name]
        {
            get
            {
                if (name == null)
                {
                    throw ExceptionBuilder.ArgumentNull("name");
                }
                DataColumn column = this.columnFromName[name] as DataColumn;
                if (column == null)
                {
                    int num = this.IndexOfCaseInsensitive(name);
                    if (0 <= num)
                    {
                        return (DataColumn) this._list[num];
                    }
                    if (-2 == num)
                    {
                        throw ExceptionBuilder.CaseInsensitiveNameConflict(name);
                    }
                }
                return column;
            }
        }

        internal DataColumn this[string name, string ns]
        {
            get
            {
                DataColumn column = this.columnFromName[name] as DataColumn;
                if ((column != null) && (column.Namespace == ns))
                {
                    return column;
                }
                return null;
            }
        }

        protected override ArrayList List
        {
            get
            {
                return this._list;
            }
        }
    }
}

