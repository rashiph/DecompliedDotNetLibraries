namespace System.Data
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Reflection;

    [Editor("Microsoft.VSDesigner.Data.Design.TablesCollectionEditor, Microsoft.VSDesigner, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), DefaultEvent("CollectionChanged"), ListBindable(false)]
    public sealed class DataTableCollection : InternalDataCollectionBase
    {
        private readonly ArrayList _list = new ArrayList();
        private readonly int _objectID = Interlocked.Increment(ref _objectTypeCount);
        private static int _objectTypeCount;
        private readonly DataSet dataSet;
        private int defaultNameIndex = 1;
        private DataTable[] delayedAddRangeTables;
        private CollectionChangeEventHandler onCollectionChangedDelegate;
        private CollectionChangeEventHandler onCollectionChangingDelegate;

        [ResDescription("collectionChangedEventDescr")]
        public event CollectionChangeEventHandler CollectionChanged
        {
            add
            {
                Bid.Trace("<ds.DataTableCollection.add_CollectionChanged|API> %d#\n", this.ObjectID);
                this.onCollectionChangedDelegate = (CollectionChangeEventHandler) Delegate.Combine(this.onCollectionChangedDelegate, value);
            }
            remove
            {
                Bid.Trace("<ds.DataTableCollection.remove_CollectionChanged|API> %d#\n", this.ObjectID);
                this.onCollectionChangedDelegate = (CollectionChangeEventHandler) Delegate.Remove(this.onCollectionChangedDelegate, value);
            }
        }

        public event CollectionChangeEventHandler CollectionChanging
        {
            add
            {
                Bid.Trace("<ds.DataTableCollection.add_CollectionChanging|API> %d#\n", this.ObjectID);
                this.onCollectionChangingDelegate = (CollectionChangeEventHandler) Delegate.Combine(this.onCollectionChangingDelegate, value);
            }
            remove
            {
                Bid.Trace("<ds.DataTableCollection.remove_CollectionChanging|API> %d#\n", this.ObjectID);
                this.onCollectionChangingDelegate = (CollectionChangeEventHandler) Delegate.Remove(this.onCollectionChangingDelegate, value);
            }
        }

        internal DataTableCollection(DataSet dataSet)
        {
            Bid.Trace("<ds.DataTableCollection.DataTableCollection|INFO> %d#, dataSet=%d\n", this.ObjectID, (dataSet != null) ? dataSet.ObjectID : 0);
            this.dataSet = dataSet;
        }

        public DataTable Add()
        {
            DataTable table = new DataTable();
            this.Add(table);
            return table;
        }

        public void Add(DataTable table)
        {
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<ds.DataTableCollection.Add|API> %d#, table=%d\n", this.ObjectID, (table != null) ? table.ObjectID : 0);
            try
            {
                this.OnCollectionChanging(new CollectionChangeEventArgs(CollectionChangeAction.Add, table));
                this.BaseAdd(table);
                this.ArrayAdd(table);
                if (table.SetLocaleValue(this.dataSet.Locale, false, false) || table.SetCaseSensitiveValue(this.dataSet.CaseSensitive, false, false))
                {
                    table.ResetIndexes();
                }
                this.OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Add, table));
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
        }

        public DataTable Add(string name)
        {
            DataTable table = new DataTable(name);
            this.Add(table);
            return table;
        }

        public DataTable Add(string name, string tableNamespace)
        {
            DataTable table = new DataTable(name, tableNamespace);
            this.Add(table);
            return table;
        }

        public void AddRange(DataTable[] tables)
        {
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<ds.DataTableCollection.AddRange|API> %d#\n", this.ObjectID);
            try
            {
                if (this.dataSet.fInitInProgress)
                {
                    this.delayedAddRangeTables = tables;
                }
                else if (tables != null)
                {
                    foreach (DataTable table in tables)
                    {
                        if (table != null)
                        {
                            this.Add(table);
                        }
                    }
                }
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
        }

        private void ArrayAdd(DataTable table)
        {
            this._list.Add(table);
        }

        internal string AssignName()
        {
            string str = null;
            while (this.Contains(str = this.MakeName(this.defaultNameIndex)))
            {
                this.defaultNameIndex++;
            }
            return str;
        }

        private void BaseAdd(DataTable table)
        {
            if (table == null)
            {
                throw ExceptionBuilder.ArgumentNull("table");
            }
            if (table.DataSet == this.dataSet)
            {
                throw ExceptionBuilder.TableAlreadyInTheDataSet();
            }
            if (table.DataSet != null)
            {
                throw ExceptionBuilder.TableAlreadyInOtherDataSet();
            }
            if (table.TableName.Length == 0)
            {
                table.TableName = this.AssignName();
            }
            else
            {
                if ((base.NamesEqual(table.TableName, this.dataSet.DataSetName, false, this.dataSet.Locale) != 0) && !table.fNestedInDataset)
                {
                    throw ExceptionBuilder.DatasetConflictingName(this.dataSet.DataSetName);
                }
                this.RegisterName(table.TableName, table.Namespace);
            }
            table.SetDataSet(this.dataSet);
        }

        private void BaseGroupSwitch(DataTable[] oldArray, int oldLength, DataTable[] newArray, int newLength)
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
                if (!flag && (oldArray[i].DataSet == this.dataSet))
                {
                    this.BaseRemove(oldArray[i]);
                }
            }
            for (int j = 0; j < newLength; j++)
            {
                if (newArray[j].DataSet != this.dataSet)
                {
                    this.BaseAdd(newArray[j]);
                    this._list.Add(newArray[j]);
                }
            }
        }

        private void BaseRemove(DataTable table)
        {
            if (this.CanRemove(table, true))
            {
                this.UnregisterName(table.TableName);
                table.SetDataSet(null);
            }
            this._list.Remove(table);
            this.dataSet.OnRemovedTable(table);
        }

        public bool CanRemove(DataTable table)
        {
            return this.CanRemove(table, false);
        }

        internal bool CanRemove(DataTable table, bool fThrowException)
        {
            bool flag;
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<ds.DataTableCollection.CanRemove|INFO> %d#, table=%d, fThrowException=%d{bool}\n", this.ObjectID, (table != null) ? table.ObjectID : 0, fThrowException);
            try
            {
                if (table == null)
                {
                    if (fThrowException)
                    {
                        throw ExceptionBuilder.ArgumentNull("table");
                    }
                    return false;
                }
                if (table.DataSet != this.dataSet)
                {
                    if (fThrowException)
                    {
                        throw ExceptionBuilder.TableNotInTheDataSet(table.TableName);
                    }
                    return false;
                }
                this.dataSet.OnRemoveTable(table);
                if ((table.ChildRelations.Count != 0) || (table.ParentRelations.Count != 0))
                {
                    if (fThrowException)
                    {
                        throw ExceptionBuilder.TableInRelation();
                    }
                    return false;
                }
                ParentForeignKeyConstraintEnumerator enumerator2 = new ParentForeignKeyConstraintEnumerator(this.dataSet, table);
                while (enumerator2.GetNext())
                {
                    ForeignKeyConstraint foreignKeyConstraint = enumerator2.GetForeignKeyConstraint();
                    if ((foreignKeyConstraint.Table != table) || (foreignKeyConstraint.RelatedTable != table))
                    {
                        if (fThrowException)
                        {
                            throw ExceptionBuilder.TableInConstraint(table, foreignKeyConstraint);
                        }
                        return false;
                    }
                }
                ChildForeignKeyConstraintEnumerator enumerator = new ChildForeignKeyConstraintEnumerator(this.dataSet, table);
                while (enumerator.GetNext())
                {
                    ForeignKeyConstraint constraint = enumerator.GetForeignKeyConstraint();
                    if ((constraint.Table != table) || (constraint.RelatedTable != table))
                    {
                        if (fThrowException)
                        {
                            throw ExceptionBuilder.TableInConstraint(table, constraint);
                        }
                        return false;
                    }
                }
                flag = true;
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
            return flag;
        }

        public void Clear()
        {
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<ds.DataTableCollection.Clear|API> %d#\n", this.ObjectID);
            try
            {
                int count = this._list.Count;
                DataTable[] array = new DataTable[this._list.Count];
                this._list.CopyTo(array, 0);
                this.OnCollectionChanging(InternalDataCollectionBase.RefreshEventArgs);
                if (this.dataSet.fInitInProgress && (this.delayedAddRangeTables != null))
                {
                    this.delayedAddRangeTables = null;
                }
                this.BaseGroupSwitch(array, count, null, 0);
                this._list.Clear();
                this.OnCollectionChanged(InternalDataCollectionBase.RefreshEventArgs);
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
        }

        public bool Contains(string name)
        {
            return (this.InternalIndexOf(name) >= 0);
        }

        internal bool Contains(string name, bool caseSensitive)
        {
            if (!caseSensitive)
            {
                return (this.InternalIndexOf(name) >= 0);
            }
            int count = this._list.Count;
            for (int i = 0; i < count; i++)
            {
                DataTable table = (DataTable) this._list[i];
                if (base.NamesEqual(table.TableName, name, true, this.dataSet.Locale) == 1)
                {
                    return true;
                }
            }
            return false;
        }

        public bool Contains(string name, string tableNamespace)
        {
            if (name == null)
            {
                throw ExceptionBuilder.ArgumentNull("name");
            }
            if (tableNamespace == null)
            {
                throw ExceptionBuilder.ArgumentNull("tableNamespace");
            }
            return (this.InternalIndexOf(name, tableNamespace) >= 0);
        }

        internal bool Contains(string name, string tableNamespace, bool checkProperty, bool caseSensitive)
        {
            if (!caseSensitive)
            {
                return (this.InternalIndexOf(name) >= 0);
            }
            int count = this._list.Count;
            for (int i = 0; i < count; i++)
            {
                DataTable table = (DataTable) this._list[i];
                string str = checkProperty ? table.Namespace : table.tableNamespace;
                if ((base.NamesEqual(table.TableName, name, true, this.dataSet.Locale) == 1) && (str == tableNamespace))
                {
                    return true;
                }
            }
            return false;
        }

        public void CopyTo(DataTable[] array, int index)
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
                array[index + i] = (DataTable) this._list[i];
            }
        }

        internal void FinishInitCollection()
        {
            if (this.delayedAddRangeTables != null)
            {
                foreach (DataTable table in this.delayedAddRangeTables)
                {
                    if (table != null)
                    {
                        this.Add(table);
                    }
                }
                this.delayedAddRangeTables = null;
            }
        }

        internal DataTable GetTable(string name, string ns)
        {
            for (int i = 0; i < this._list.Count; i++)
            {
                DataTable table = (DataTable) this._list[i];
                if ((table.TableName == name) && (table.Namespace == ns))
                {
                    return table;
                }
            }
            return null;
        }

        internal DataTable GetTableSmart(string name, string ns)
        {
            int num2 = 0;
            DataTable table2 = null;
            for (int i = 0; i < this._list.Count; i++)
            {
                DataTable table = (DataTable) this._list[i];
                if (table.TableName == name)
                {
                    if (table.Namespace == ns)
                    {
                        return table;
                    }
                    num2++;
                    table2 = table;
                }
            }
            if (num2 != 1)
            {
                return null;
            }
            return table2;
        }

        public int IndexOf(DataTable table)
        {
            int count = this._list.Count;
            for (int i = 0; i < count; i++)
            {
                if (table == ((DataTable) this._list[i]))
                {
                    return i;
                }
            }
            return -1;
        }

        public int IndexOf(string tableName)
        {
            int num = this.InternalIndexOf(tableName);
            if (num >= 0)
            {
                return num;
            }
            return -1;
        }

        public int IndexOf(string tableName, string tableNamespace)
        {
            return this.IndexOf(tableName, tableNamespace, true);
        }

        internal int IndexOf(string tableName, string tableNamespace, bool chekforNull)
        {
            if (chekforNull)
            {
                if (tableName == null)
                {
                    throw ExceptionBuilder.ArgumentNull("tableName");
                }
                if (tableNamespace == null)
                {
                    throw ExceptionBuilder.ArgumentNull("tableNamespace");
                }
            }
            int num = this.InternalIndexOf(tableName, tableNamespace);
            if (num >= 0)
            {
                return num;
            }
            return -1;
        }

        internal int InternalIndexOf(string tableName)
        {
            int num4 = -1;
            if ((tableName != null) && (0 < tableName.Length))
            {
                int count = this._list.Count;
                for (int i = 0; i < count; i++)
                {
                    DataTable table2 = (DataTable) this._list[i];
                    switch (base.NamesEqual(table2.TableName, tableName, false, this.dataSet.Locale))
                    {
                        case 1:
                            for (int j = i + 1; j < count; j++)
                            {
                                DataTable table = (DataTable) this._list[j];
                                if (base.NamesEqual(table.TableName, tableName, false, this.dataSet.Locale) == 1)
                                {
                                    return -3;
                                }
                            }
                            return i;

                        case -1:
                            num4 = (num4 == -1) ? i : -2;
                            break;
                    }
                }
            }
            return num4;
        }

        internal int InternalIndexOf(string tableName, string tableNamespace)
        {
            int num3 = -1;
            if ((tableName != null) && (0 < tableName.Length))
            {
                int count = this._list.Count;
                int num2 = 0;
                for (int i = 0; i < count; i++)
                {
                    DataTable table = (DataTable) this._list[i];
                    num2 = base.NamesEqual(table.TableName, tableName, false, this.dataSet.Locale);
                    if ((num2 == 1) && (table.Namespace == tableNamespace))
                    {
                        return i;
                    }
                    if ((num2 == -1) && (table.Namespace == tableNamespace))
                    {
                        num3 = (num3 == -1) ? i : -2;
                    }
                }
            }
            return num3;
        }

        private string MakeName(int index)
        {
            if (1 == index)
            {
                return "Table1";
            }
            return ("Table" + index.ToString(CultureInfo.InvariantCulture));
        }

        private void OnCollectionChanged(CollectionChangeEventArgs ccevent)
        {
            if (this.onCollectionChangedDelegate != null)
            {
                Bid.Trace("<ds.DataTableCollection.OnCollectionChanged|INFO> %d#\n", this.ObjectID);
                this.onCollectionChangedDelegate(this, ccevent);
            }
        }

        private void OnCollectionChanging(CollectionChangeEventArgs ccevent)
        {
            if (this.onCollectionChangingDelegate != null)
            {
                Bid.Trace("<ds.DataTableCollection.OnCollectionChanging|INFO> %d#\n", this.ObjectID);
                this.onCollectionChangingDelegate(this, ccevent);
            }
        }

        internal void RegisterName(string name, string tbNamespace)
        {
            Bid.Trace("<ds.DataTableCollection.RegisterName|INFO> %d#, name='%ls', tbNamespace='%ls'\n", this.ObjectID, name, tbNamespace);
            CultureInfo locale = this.dataSet.Locale;
            int count = this._list.Count;
            for (int i = 0; i < count; i++)
            {
                DataTable table = (DataTable) this._list[i];
                if ((base.NamesEqual(name, table.TableName, true, locale) != 0) && (tbNamespace == table.Namespace))
                {
                    throw ExceptionBuilder.DuplicateTableName(((DataTable) this._list[i]).TableName);
                }
            }
            if (base.NamesEqual(name, this.MakeName(this.defaultNameIndex), true, locale) != 0)
            {
                this.defaultNameIndex++;
            }
        }

        public void Remove(DataTable table)
        {
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<ds.DataTableCollection.Remove|API> %d#, table=%d\n", this.ObjectID, (table != null) ? table.ObjectID : 0);
            try
            {
                this.OnCollectionChanging(new CollectionChangeEventArgs(CollectionChangeAction.Remove, table));
                this.BaseRemove(table);
                this.OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Remove, table));
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
        }

        public void Remove(string name)
        {
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<ds.DataTableCollection.Remove|API> %d#, name='%ls'\n", this.ObjectID, name);
            try
            {
                DataTable table = this[name];
                if (table == null)
                {
                    throw ExceptionBuilder.TableNotInTheDataSet(name);
                }
                this.Remove(table);
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
        }

        public void Remove(string name, string tableNamespace)
        {
            if (name == null)
            {
                throw ExceptionBuilder.ArgumentNull("name");
            }
            if (tableNamespace == null)
            {
                throw ExceptionBuilder.ArgumentNull("tableNamespace");
            }
            DataTable table = this[name, tableNamespace];
            if (table == null)
            {
                throw ExceptionBuilder.TableNotInTheDataSet(name);
            }
            this.Remove(table);
        }

        public void RemoveAt(int index)
        {
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<ds.DataTableCollection.RemoveAt|API> %d#, index=%d\n", this.ObjectID, index);
            try
            {
                DataTable table = this[index];
                if (table == null)
                {
                    throw ExceptionBuilder.TableOutOfRange(index);
                }
                this.Remove(table);
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
        }

        internal void ReplaceFromInference(List<DataTable> tableList)
        {
            this._list.Clear();
            this._list.AddRange(tableList);
        }

        internal void UnregisterName(string name)
        {
            Bid.Trace("<ds.DataTableCollection.UnregisterName|INFO> %d#, name='%ls'\n", this.ObjectID, name);
            if (base.NamesEqual(name, this.MakeName(this.defaultNameIndex - 1), true, this.dataSet.Locale) != 0)
            {
                do
                {
                    this.defaultNameIndex--;
                }
                while ((this.defaultNameIndex > 1) && !this.Contains(this.MakeName(this.defaultNameIndex - 1)));
            }
        }

        public DataTable this[int index]
        {
            get
            {
                DataTable table;
                try
                {
                    table = (DataTable) this._list[index];
                }
                catch (ArgumentOutOfRangeException)
                {
                    throw ExceptionBuilder.TableOutOfRange(index);
                }
                return table;
            }
        }

        public DataTable this[string name]
        {
            get
            {
                int num = this.InternalIndexOf(name);
                switch (num)
                {
                    case -2:
                        throw ExceptionBuilder.CaseInsensitiveNameConflict(name);

                    case -3:
                        throw ExceptionBuilder.NamespaceNameConflict(name);
                }
                if (num >= 0)
                {
                    return (DataTable) this._list[num];
                }
                return null;
            }
        }

        public DataTable this[string name, string tableNamespace]
        {
            get
            {
                if (tableNamespace == null)
                {
                    throw ExceptionBuilder.ArgumentNull("tableNamespace");
                }
                int num = this.InternalIndexOf(name, tableNamespace);
                if (num == -2)
                {
                    throw ExceptionBuilder.CaseInsensitiveNameConflict(name);
                }
                if (num >= 0)
                {
                    return (DataTable) this._list[num];
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

        internal int ObjectID
        {
            get
            {
                return this._objectID;
            }
        }
    }
}

