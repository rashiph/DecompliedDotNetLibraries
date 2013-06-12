namespace System.Data
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Data.Common;
    using System.Globalization;
    using System.Reflection;

    [DefaultEvent("CollectionChanged"), Editor("Microsoft.VSDesigner.Data.Design.DataRelationCollectionEditor, Microsoft.VSDesigner, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), DefaultProperty("Table")]
    public abstract class DataRelationCollection : InternalDataCollectionBase
    {
        private readonly int _objectID = Interlocked.Increment(ref _objectTypeCount);
        private static int _objectTypeCount;
        private int defaultNameIndex = 1;
        private DataRelation inTransition;
        private CollectionChangeEventHandler onCollectionChangedDelegate;
        private CollectionChangeEventHandler onCollectionChangingDelegate;

        [ResDescription("collectionChangedEventDescr")]
        public event CollectionChangeEventHandler CollectionChanged
        {
            add
            {
                Bid.Trace("<ds.DataRelationCollection.add_CollectionChanged|API> %d#\n", this.ObjectID);
                this.onCollectionChangedDelegate = (CollectionChangeEventHandler) Delegate.Combine(this.onCollectionChangedDelegate, value);
            }
            remove
            {
                Bid.Trace("<ds.DataRelationCollection.remove_CollectionChanged|API> %d#\n", this.ObjectID);
                this.onCollectionChangedDelegate = (CollectionChangeEventHandler) Delegate.Remove(this.onCollectionChangedDelegate, value);
            }
        }

        internal event CollectionChangeEventHandler CollectionChanging
        {
            add
            {
                Bid.Trace("<ds.DataRelationCollection.add_CollectionChanging|INFO> %d#\n", this.ObjectID);
                this.onCollectionChangingDelegate = (CollectionChangeEventHandler) Delegate.Combine(this.onCollectionChangingDelegate, value);
            }
            remove
            {
                Bid.Trace("<ds.DataRelationCollection.remove_CollectionChanging|INFO> %d#\n", this.ObjectID);
                this.onCollectionChangingDelegate = (CollectionChangeEventHandler) Delegate.Remove(this.onCollectionChangingDelegate, value);
            }
        }

        protected DataRelationCollection()
        {
        }

        public void Add(DataRelation relation)
        {
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<ds.DataRelationCollection.Add|API> %d#, relation=%d\n", this.ObjectID, (relation != null) ? relation.ObjectID : 0);
            try
            {
                if (this.inTransition != relation)
                {
                    this.inTransition = relation;
                    try
                    {
                        this.OnCollectionChanging(new CollectionChangeEventArgs(CollectionChangeAction.Add, relation));
                        this.AddCore(relation);
                        this.OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Add, relation));
                    }
                    finally
                    {
                        this.inTransition = null;
                    }
                }
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
        }

        public virtual DataRelation Add(DataColumn[] parentColumns, DataColumn[] childColumns)
        {
            DataRelation relation = new DataRelation(null, parentColumns, childColumns);
            this.Add(relation);
            return relation;
        }

        public virtual DataRelation Add(DataColumn parentColumn, DataColumn childColumn)
        {
            DataRelation relation = new DataRelation(null, parentColumn, childColumn);
            this.Add(relation);
            return relation;
        }

        public virtual DataRelation Add(string name, DataColumn[] parentColumns, DataColumn[] childColumns)
        {
            DataRelation relation = new DataRelation(name, parentColumns, childColumns);
            this.Add(relation);
            return relation;
        }

        public virtual DataRelation Add(string name, DataColumn parentColumn, DataColumn childColumn)
        {
            DataRelation relation = new DataRelation(name, parentColumn, childColumn);
            this.Add(relation);
            return relation;
        }

        public virtual DataRelation Add(string name, DataColumn[] parentColumns, DataColumn[] childColumns, bool createConstraints)
        {
            DataRelation relation = new DataRelation(name, parentColumns, childColumns, createConstraints);
            this.Add(relation);
            return relation;
        }

        public virtual DataRelation Add(string name, DataColumn parentColumn, DataColumn childColumn, bool createConstraints)
        {
            DataRelation relation = new DataRelation(name, parentColumn, childColumn, createConstraints);
            this.Add(relation);
            return relation;
        }

        protected virtual void AddCore(DataRelation relation)
        {
            Bid.Trace("<ds.DataRelationCollection.AddCore|INFO> %d#, relation=%d\n", this.ObjectID, (relation != null) ? relation.ObjectID : 0);
            if (relation == null)
            {
                throw ExceptionBuilder.ArgumentNull("relation");
            }
            relation.CheckState();
            DataSet dataSet = this.GetDataSet();
            if (relation.DataSet == dataSet)
            {
                throw ExceptionBuilder.RelationAlreadyInTheDataSet();
            }
            if (relation.DataSet != null)
            {
                throw ExceptionBuilder.RelationAlreadyInOtherDataSet();
            }
            if ((relation.ChildTable.Locale.LCID != relation.ParentTable.Locale.LCID) || (relation.ChildTable.CaseSensitive != relation.ParentTable.CaseSensitive))
            {
                throw ExceptionBuilder.CaseLocaleMismatch();
            }
            if (relation.Nested)
            {
                relation.CheckNamespaceValidityForNestedRelations(relation.ParentTable.Namespace);
                relation.ValidateMultipleNestedRelations();
                DataTable parentTable = relation.ParentTable;
                parentTable.ElementColumnCount++;
            }
        }

        public virtual void AddRange(DataRelation[] relations)
        {
            if (relations != null)
            {
                foreach (DataRelation relation in relations)
                {
                    if (relation != null)
                    {
                        this.Add(relation);
                    }
                }
            }
        }

        internal string AssignName()
        {
            string str = this.MakeName(this.defaultNameIndex);
            this.defaultNameIndex++;
            return str;
        }

        public virtual bool CanRemove(DataRelation relation)
        {
            if (relation == null)
            {
                return false;
            }
            if (relation.DataSet != this.GetDataSet())
            {
                return false;
            }
            return true;
        }

        public virtual void Clear()
        {
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<ds.DataRelationCollection.Clear|API> %d#\n", this.ObjectID);
            try
            {
                int count = this.Count;
                this.OnCollectionChanging(InternalDataCollectionBase.RefreshEventArgs);
                for (int i = count - 1; i >= 0; i--)
                {
                    this.inTransition = this[i];
                    this.RemoveCore(this.inTransition);
                }
                this.OnCollectionChanged(InternalDataCollectionBase.RefreshEventArgs);
                this.inTransition = null;
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
        }

        public virtual bool Contains(string name)
        {
            return (this.InternalIndexOf(name) >= 0);
        }

        public void CopyTo(DataRelation[] array, int index)
        {
            if (array == null)
            {
                throw ExceptionBuilder.ArgumentNull("array");
            }
            if (index < 0)
            {
                throw ExceptionBuilder.ArgumentOutOfRange("index");
            }
            ArrayList list = this.List;
            if ((array.Length - index) < list.Count)
            {
                throw ExceptionBuilder.InvalidOffsetLength();
            }
            for (int i = 0; i < list.Count; i++)
            {
                array[index + i] = (DataRelation) list[i];
            }
        }

        protected abstract DataSet GetDataSet();
        public virtual int IndexOf(DataRelation relation)
        {
            int count = this.List.Count;
            for (int i = 0; i < count; i++)
            {
                if (relation == ((DataRelation) this.List[i]))
                {
                    return i;
                }
            }
            return -1;
        }

        public virtual int IndexOf(string relationName)
        {
            int num = this.InternalIndexOf(relationName);
            if (num >= 0)
            {
                return num;
            }
            return -1;
        }

        internal int InternalIndexOf(string name)
        {
            int num3 = -1;
            if ((name != null) && (0 < name.Length))
            {
                int count = this.List.Count;
                for (int i = 0; i < count; i++)
                {
                    DataRelation relation = (DataRelation) this.List[i];
                    switch (base.NamesEqual(relation.RelationName, name, false, this.GetDataSet().Locale))
                    {
                        case 1:
                            return i;

                        case -1:
                            num3 = (num3 == -1) ? i : -2;
                            break;
                    }
                }
            }
            return num3;
        }

        private string MakeName(int index)
        {
            if (1 == index)
            {
                return "Relation1";
            }
            return ("Relation" + index.ToString(CultureInfo.InvariantCulture));
        }

        protected virtual void OnCollectionChanged(CollectionChangeEventArgs ccevent)
        {
            if (this.onCollectionChangedDelegate != null)
            {
                Bid.Trace("<ds.DataRelationCollection.OnCollectionChanged|INFO> %d#\n", this.ObjectID);
                this.onCollectionChangedDelegate(this, ccevent);
            }
        }

        protected virtual void OnCollectionChanging(CollectionChangeEventArgs ccevent)
        {
            if (this.onCollectionChangingDelegate != null)
            {
                Bid.Trace("<ds.DataRelationCollection.OnCollectionChanging|INFO> %d#\n", this.ObjectID);
                this.onCollectionChangingDelegate(this, ccevent);
            }
        }

        internal void RegisterName(string name)
        {
            Bid.Trace("<ds.DataRelationCollection.RegisterName|INFO> %d#, name='%ls'\n", this.ObjectID, name);
            CultureInfo locale = this.GetDataSet().Locale;
            int count = this.Count;
            for (int i = 0; i < count; i++)
            {
                if (base.NamesEqual(name, this[i].RelationName, true, locale) != 0)
                {
                    throw ExceptionBuilder.DuplicateRelation(this[i].RelationName);
                }
            }
            if (base.NamesEqual(name, this.MakeName(this.defaultNameIndex), true, locale) != 0)
            {
                this.defaultNameIndex++;
            }
        }

        public void Remove(DataRelation relation)
        {
            Bid.Trace("<ds.DataRelationCollection.Remove|API> %d#, relation=%d\n", this.ObjectID, (relation != null) ? relation.ObjectID : 0);
            if (this.inTransition != relation)
            {
                this.inTransition = relation;
                try
                {
                    this.OnCollectionChanging(new CollectionChangeEventArgs(CollectionChangeAction.Remove, relation));
                    this.RemoveCore(relation);
                    this.OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Remove, relation));
                }
                finally
                {
                    this.inTransition = null;
                }
            }
        }

        public void Remove(string name)
        {
            DataRelation relation = this[name];
            if (relation == null)
            {
                throw ExceptionBuilder.RelationNotInTheDataSet(name);
            }
            this.Remove(relation);
        }

        public void RemoveAt(int index)
        {
            DataRelation relation = this[index];
            if (relation == null)
            {
                throw ExceptionBuilder.RelationOutOfRange(index);
            }
            this.Remove(relation);
        }

        protected virtual void RemoveCore(DataRelation relation)
        {
            Bid.Trace("<ds.DataRelationCollection.RemoveCore|INFO> %d#, relation=%d\n", this.ObjectID, (relation != null) ? relation.ObjectID : 0);
            if (relation == null)
            {
                throw ExceptionBuilder.ArgumentNull("relation");
            }
            DataSet dataSet = this.GetDataSet();
            if (relation.DataSet != dataSet)
            {
                throw ExceptionBuilder.RelationNotInTheDataSet(relation.RelationName);
            }
            if (relation.Nested)
            {
                DataTable parentTable = relation.ParentTable;
                parentTable.ElementColumnCount--;
                relation.ParentTable.Columns.UnregisterName(relation.ChildTable.TableName);
            }
        }

        internal void UnregisterName(string name)
        {
            Bid.Trace("<ds.DataRelationCollection.UnregisterName|INFO> %d#, name='%ls'\n", this.ObjectID, name);
            if (base.NamesEqual(name, this.MakeName(this.defaultNameIndex - 1), true, this.GetDataSet().Locale) != 0)
            {
                do
                {
                    this.defaultNameIndex--;
                }
                while ((this.defaultNameIndex > 1) && !this.Contains(this.MakeName(this.defaultNameIndex - 1)));
            }
        }

        public abstract DataRelation this[int index] { get; }

        public abstract DataRelation this[string name] { get; }

        internal int ObjectID
        {
            get
            {
                return this._objectID;
            }
        }

        internal sealed class DataSetRelationCollection : DataRelationCollection
        {
            private readonly DataSet dataSet;
            private DataRelation[] delayLoadingRelations;
            private readonly ArrayList relations;

            internal DataSetRelationCollection(DataSet dataSet)
            {
                if (dataSet == null)
                {
                    throw ExceptionBuilder.RelationDataSetNull();
                }
                this.dataSet = dataSet;
                this.relations = new ArrayList();
            }

            protected override void AddCore(DataRelation relation)
            {
                base.AddCore(relation);
                if ((relation.ChildTable.DataSet != this.dataSet) || (relation.ParentTable.DataSet != this.dataSet))
                {
                    throw ExceptionBuilder.ForeignRelation();
                }
                relation.CheckState();
                if (relation.Nested)
                {
                    relation.CheckNestedRelations();
                }
                if (relation.relationName.Length == 0)
                {
                    relation.relationName = base.AssignName();
                }
                else
                {
                    base.RegisterName(relation.relationName);
                }
                DataKey childKey = relation.ChildKey;
                for (int i = 0; i < this.relations.Count; i++)
                {
                    if (childKey.ColumnsEqual(((DataRelation) this.relations[i]).ChildKey) && relation.ParentKey.ColumnsEqual(((DataRelation) this.relations[i]).ParentKey))
                    {
                        throw ExceptionBuilder.RelationAlreadyExists();
                    }
                }
                this.relations.Add(relation);
                ((DataRelationCollection.DataTableRelationCollection) relation.ParentTable.ChildRelations).Add(relation);
                ((DataRelationCollection.DataTableRelationCollection) relation.ChildTable.ParentRelations).Add(relation);
                relation.SetDataSet(this.dataSet);
                relation.ChildKey.GetSortIndex().AddRef();
                if (relation.Nested)
                {
                    relation.ChildTable.CacheNestedParent();
                }
                ForeignKeyConstraint constraint = relation.ChildTable.Constraints.FindForeignKeyConstraint(relation.ParentColumnsReference, relation.ChildColumnsReference);
                if (relation.createConstraints && (constraint == null))
                {
                    relation.ChildTable.Constraints.Add(constraint = new ForeignKeyConstraint(relation.ParentColumnsReference, relation.ChildColumnsReference));
                    try
                    {
                        constraint.ConstraintName = relation.RelationName;
                    }
                    catch (Exception exception)
                    {
                        if (!ADP.IsCatchableExceptionType(exception))
                        {
                            throw;
                        }
                        ExceptionBuilder.TraceExceptionWithoutRethrow(exception);
                    }
                }
                UniqueConstraint constraint2 = relation.ParentTable.Constraints.FindKeyConstraint(relation.ParentColumnsReference);
                relation.SetParentKeyConstraint(constraint2);
                relation.SetChildKeyConstraint(constraint);
            }

            public override void AddRange(DataRelation[] relations)
            {
                if (this.dataSet.fInitInProgress)
                {
                    this.delayLoadingRelations = relations;
                }
                else if (relations != null)
                {
                    foreach (DataRelation relation in relations)
                    {
                        if (relation != null)
                        {
                            base.Add(relation);
                        }
                    }
                }
            }

            public override void Clear()
            {
                base.Clear();
                if (this.dataSet.fInitInProgress && (this.delayLoadingRelations != null))
                {
                    this.delayLoadingRelations = null;
                }
            }

            internal void FinishInitRelations()
            {
                if (this.delayLoadingRelations != null)
                {
                    for (int i = 0; i < this.delayLoadingRelations.Length; i++)
                    {
                        DataRelation relation = this.delayLoadingRelations[i];
                        if ((relation.parentColumnNames == null) || (relation.childColumnNames == null))
                        {
                            base.Add(relation);
                        }
                        else
                        {
                            int length = relation.parentColumnNames.Length;
                            DataColumn[] parentColumns = new DataColumn[length];
                            DataColumn[] childColumns = new DataColumn[length];
                            for (int j = 0; j < length; j++)
                            {
                                if (relation.parentTableNamespace == null)
                                {
                                    parentColumns[j] = this.dataSet.Tables[relation.parentTableName].Columns[relation.parentColumnNames[j]];
                                }
                                else
                                {
                                    parentColumns[j] = this.dataSet.Tables[relation.parentTableName, relation.parentTableNamespace].Columns[relation.parentColumnNames[j]];
                                }
                                if (relation.childTableNamespace == null)
                                {
                                    childColumns[j] = this.dataSet.Tables[relation.childTableName].Columns[relation.childColumnNames[j]];
                                }
                                else
                                {
                                    childColumns[j] = this.dataSet.Tables[relation.childTableName, relation.childTableNamespace].Columns[relation.childColumnNames[j]];
                                }
                            }
                            DataRelation relation2 = new DataRelation(relation.relationName, parentColumns, childColumns, false) {
                                Nested = relation.nested
                            };
                            base.Add(relation2);
                        }
                    }
                    this.delayLoadingRelations = null;
                }
            }

            protected override DataSet GetDataSet()
            {
                return this.dataSet;
            }

            protected override void RemoveCore(DataRelation relation)
            {
                base.RemoveCore(relation);
                this.dataSet.OnRemoveRelationHack(relation);
                relation.SetDataSet(null);
                relation.ChildKey.GetSortIndex().RemoveRef();
                if (relation.Nested)
                {
                    relation.ChildTable.CacheNestedParent();
                }
                for (int i = 0; i < this.relations.Count; i++)
                {
                    if (relation == this.relations[i])
                    {
                        this.relations.RemoveAt(i);
                        ((DataRelationCollection.DataTableRelationCollection) relation.ParentTable.ChildRelations).Remove(relation);
                        ((DataRelationCollection.DataTableRelationCollection) relation.ChildTable.ParentRelations).Remove(relation);
                        if (relation.Nested)
                        {
                            relation.ChildTable.CacheNestedParent();
                        }
                        base.UnregisterName(relation.RelationName);
                        relation.SetParentKeyConstraint(null);
                        relation.SetChildKeyConstraint(null);
                        return;
                    }
                }
                throw ExceptionBuilder.RelationDoesNotExist();
            }

            public override DataRelation this[int index]
            {
                get
                {
                    if ((index < 0) || (index >= this.relations.Count))
                    {
                        throw ExceptionBuilder.RelationOutOfRange(index);
                    }
                    return (DataRelation) this.relations[index];
                }
            }

            public override DataRelation this[string name]
            {
                get
                {
                    int num = base.InternalIndexOf(name);
                    if (num == -2)
                    {
                        throw ExceptionBuilder.CaseInsensitiveNameConflict(name);
                    }
                    if (num >= 0)
                    {
                        return (DataRelation) this.List[num];
                    }
                    return null;
                }
            }

            protected override ArrayList List
            {
                get
                {
                    return this.relations;
                }
            }
        }

        internal sealed class DataTableRelationCollection : DataRelationCollection
        {
            private readonly bool fParentCollection;
            private readonly ArrayList relations;
            private readonly DataTable table;

            internal event CollectionChangeEventHandler RelationPropertyChanged;

            internal DataTableRelationCollection(DataTable table, bool fParentCollection)
            {
                if (table == null)
                {
                    throw ExceptionBuilder.RelationTableNull();
                }
                this.table = table;
                this.fParentCollection = fParentCollection;
                this.relations = new ArrayList();
            }

            private void AddCache(DataRelation relation)
            {
                this.relations.Add(relation);
                if (!this.fParentCollection)
                {
                    this.table.UpdatePropertyDescriptorCollectionCache();
                }
            }

            protected override void AddCore(DataRelation relation)
            {
                if (this.fParentCollection)
                {
                    if (relation.ChildTable != this.table)
                    {
                        throw ExceptionBuilder.ChildTableMismatch();
                    }
                }
                else if (relation.ParentTable != this.table)
                {
                    throw ExceptionBuilder.ParentTableMismatch();
                }
                this.GetDataSet().Relations.Add(relation);
                this.AddCache(relation);
            }

            public override bool CanRemove(DataRelation relation)
            {
                if (!base.CanRemove(relation))
                {
                    return false;
                }
                if (this.fParentCollection)
                {
                    if (relation.ChildTable != this.table)
                    {
                        return false;
                    }
                }
                else if (relation.ParentTable != this.table)
                {
                    return false;
                }
                return true;
            }

            private void EnsureDataSet()
            {
                if (this.table.DataSet == null)
                {
                    throw ExceptionBuilder.RelationTableWasRemoved();
                }
            }

            protected override DataSet GetDataSet()
            {
                this.EnsureDataSet();
                return this.table.DataSet;
            }

            internal void OnRelationPropertyChanged(CollectionChangeEventArgs ccevent)
            {
                if (!this.fParentCollection)
                {
                    this.table.UpdatePropertyDescriptorCollectionCache();
                }
                if (this.onRelationPropertyChangedDelegate != null)
                {
                    this.onRelationPropertyChangedDelegate(this, ccevent);
                }
            }

            private void RemoveCache(DataRelation relation)
            {
                for (int i = 0; i < this.relations.Count; i++)
                {
                    if (relation == this.relations[i])
                    {
                        this.relations.RemoveAt(i);
                        if (!this.fParentCollection)
                        {
                            this.table.UpdatePropertyDescriptorCollectionCache();
                        }
                        return;
                    }
                }
                throw ExceptionBuilder.RelationDoesNotExist();
            }

            protected override void RemoveCore(DataRelation relation)
            {
                if (this.fParentCollection)
                {
                    if (relation.ChildTable != this.table)
                    {
                        throw ExceptionBuilder.ChildTableMismatch();
                    }
                }
                else if (relation.ParentTable != this.table)
                {
                    throw ExceptionBuilder.ParentTableMismatch();
                }
                this.GetDataSet().Relations.Remove(relation);
                this.RemoveCache(relation);
            }

            public override DataRelation this[int index]
            {
                get
                {
                    if ((index < 0) || (index >= this.relations.Count))
                    {
                        throw ExceptionBuilder.RelationOutOfRange(index);
                    }
                    return (DataRelation) this.relations[index];
                }
            }

            public override DataRelation this[string name]
            {
                get
                {
                    int num = base.InternalIndexOf(name);
                    if (num == -2)
                    {
                        throw ExceptionBuilder.CaseInsensitiveNameConflict(name);
                    }
                    if (num >= 0)
                    {
                        return (DataRelation) this.List[num];
                    }
                    return null;
                }
            }

            protected override ArrayList List
            {
                get
                {
                    return this.relations;
                }
            }
        }
    }
}

