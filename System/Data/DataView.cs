namespace System.Data
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data.Common;
    using System.Globalization;
    using System.Reflection;
    using System.Text;
    using System.Threading;

    [Editor("Microsoft.VSDesigner.Data.Design.DataSourceEditor, Microsoft.VSDesigner, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), Designer("Microsoft.VSDesigner.Data.VS.DataViewDesigner, Microsoft.VSDesigner, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), DefaultEvent("PositionChanged"), DefaultProperty("Table")]
    public class DataView : MarshalByValueComponent, IBindingListView, IBindingList, IList, ICollection, IEnumerable, ITypedList, ISupportInitializeNotification, ISupportInitialize
    {
        private Comparison<DataRow> _comparison;
        private readonly int _objectID;
        private static int _objectTypeCount;
        private ListChangedEventArgs addNewMoved;
        internal DataRow addNewRow;
        private bool allowDelete;
        private bool allowEdit;
        private bool allowNew;
        private bool applyDefaultSort;
        private System.Data.DataViewManager dataViewManager;
        private DataViewRowState delayedRecordStates;
        private string delayedRowFilter;
        private string delayedSort;
        private DataTable delayedTable;
        private DataViewListener dvListener;
        private bool fEndInitInProgress;
        private Dictionary<string, Index> findIndexes;
        private bool fInitInProgress;
        private Index index;
        private bool locked;
        private ListChangedEventHandler onListChanged;
        private bool open;
        private DataViewRowState recordStates;
        internal static ListChangedEventArgs ResetEventArgs = new ListChangedEventArgs(ListChangedType.Reset, -1);
        private IFilter rowFilter;
        private readonly Dictionary<DataRow, DataRowView> rowViewBuffer;
        private Dictionary<DataRow, DataRowView> rowViewCache;
        private bool shouldOpen;
        private string sort;
        private DataTable table;

        [ResCategory("DataCategory_Action"), ResDescription("DataSetInitializedDescr")]
        public event EventHandler Initialized;

        [ResCategory("DataCategory_Data"), ResDescription("DataViewListChangedDescr")]
        public event ListChangedEventHandler ListChanged
        {
            add
            {
                Bid.Trace("<ds.DataView.add_ListChanged|API> %d#\n", this.ObjectID);
                this.onListChanged = (ListChangedEventHandler) Delegate.Combine(this.onListChanged, value);
            }
            remove
            {
                Bid.Trace("<ds.DataView.remove_ListChanged|API> %d#\n", this.ObjectID);
                this.onListChanged = (ListChangedEventHandler) Delegate.Remove(this.onListChanged, value);
            }
        }

        public DataView() : this(null)
        {
            this.SetIndex2("", DataViewRowState.CurrentRows, null, true);
        }

        public DataView(DataTable table) : this(table, false)
        {
            this.SetIndex2("", DataViewRowState.CurrentRows, null, true);
        }

        internal DataView(DataTable table, bool locked)
        {
            this.sort = "";
            this.recordStates = DataViewRowState.CurrentRows;
            this.shouldOpen = true;
            this.allowNew = true;
            this.allowEdit = true;
            this.allowDelete = true;
            this.delayedRecordStates = ~DataViewRowState.None;
            this.rowViewCache = new Dictionary<DataRow, DataRowView>(DataRowReferenceComparer.Default);
            this.rowViewBuffer = new Dictionary<DataRow, DataRowView>(DataRowReferenceComparer.Default);
            this._objectID = Interlocked.Increment(ref _objectTypeCount);
            GC.SuppressFinalize(this);
            Bid.Trace("<ds.DataView.DataView|INFO> %d#, table=%d, locked=%d{bool}\n", this.ObjectID, (table != null) ? table.ObjectID : 0, locked);
            this.dvListener = new DataViewListener(this);
            this.locked = locked;
            this.table = table;
            this.dvListener.RegisterMetaDataEvents(this.table);
        }

        internal DataView(DataTable table, Predicate<DataRow> predicate, Comparison<DataRow> comparison, DataViewRowState RowState)
        {
            this.sort = "";
            this.recordStates = DataViewRowState.CurrentRows;
            this.shouldOpen = true;
            this.allowNew = true;
            this.allowEdit = true;
            this.allowDelete = true;
            this.delayedRecordStates = ~DataViewRowState.None;
            this.rowViewCache = new Dictionary<DataRow, DataRowView>(DataRowReferenceComparer.Default);
            this.rowViewBuffer = new Dictionary<DataRow, DataRowView>(DataRowReferenceComparer.Default);
            this._objectID = Interlocked.Increment(ref _objectTypeCount);
            GC.SuppressFinalize(this);
            Bid.Trace("<ds.DataView.DataView|API> %d#, table=%d, RowState=%d{ds.DataViewRowState}\n", this.ObjectID, (table != null) ? table.ObjectID : 0, (int) RowState);
            if (table == null)
            {
                throw ExceptionBuilder.CanNotUse();
            }
            this.dvListener = new DataViewListener(this);
            this.locked = false;
            this.table = table;
            this.dvListener.RegisterMetaDataEvents(this.table);
            if ((RowState & ~(DataViewRowState.OriginalRows | DataViewRowState.ModifiedCurrent | DataViewRowState.Added)) != DataViewRowState.None)
            {
                throw ExceptionBuilder.RecordStateRange();
            }
            if (((RowState & DataViewRowState.ModifiedOriginal) != DataViewRowState.None) && ((RowState & DataViewRowState.ModifiedCurrent) != DataViewRowState.None))
            {
                throw ExceptionBuilder.SetRowStateFilter();
            }
            this._comparison = comparison;
            this.SetIndex2("", RowState, (predicate != null) ? new RowPredicateFilter(predicate) : null, true);
        }

        public DataView(DataTable table, string RowFilter, string Sort, DataViewRowState RowState)
        {
            this.sort = "";
            this.recordStates = DataViewRowState.CurrentRows;
            this.shouldOpen = true;
            this.allowNew = true;
            this.allowEdit = true;
            this.allowDelete = true;
            this.delayedRecordStates = ~DataViewRowState.None;
            this.rowViewCache = new Dictionary<DataRow, DataRowView>(DataRowReferenceComparer.Default);
            this.rowViewBuffer = new Dictionary<DataRow, DataRowView>(DataRowReferenceComparer.Default);
            this._objectID = Interlocked.Increment(ref _objectTypeCount);
            GC.SuppressFinalize(this);
            Bid.Trace("<ds.DataView.DataView|API> %d#, table=%d, RowFilter='%ls', Sort='%ls', RowState=%d{ds.DataViewRowState}\n", this.ObjectID, (table != null) ? table.ObjectID : 0, RowFilter, Sort, (int) RowState);
            if (table == null)
            {
                throw ExceptionBuilder.CanNotUse();
            }
            this.dvListener = new DataViewListener(this);
            this.locked = false;
            this.table = table;
            this.dvListener.RegisterMetaDataEvents(this.table);
            if ((RowState & ~(DataViewRowState.OriginalRows | DataViewRowState.ModifiedCurrent | DataViewRowState.Added)) != DataViewRowState.None)
            {
                throw ExceptionBuilder.RecordStateRange();
            }
            if (((RowState & DataViewRowState.ModifiedOriginal) != DataViewRowState.None) && ((RowState & DataViewRowState.ModifiedCurrent) != DataViewRowState.None))
            {
                throw ExceptionBuilder.SetRowStateFilter();
            }
            if (Sort == null)
            {
                Sort = "";
            }
            if (RowFilter == null)
            {
                RowFilter = "";
            }
            DataExpression newRowFilter = new DataExpression(table, RowFilter);
            this.SetIndex(Sort, RowState, newRowFilter);
        }

        public virtual DataRowView AddNew()
        {
            DataRowView view2;
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<ds.DataView.AddNew|API> %d#\n", this.ObjectID);
            try
            {
                this.CheckOpen();
                if (!this.AllowNew)
                {
                    throw ExceptionBuilder.AddNewNotAllowNull();
                }
                if (this.addNewRow != null)
                {
                    this.rowViewCache[this.addNewRow].EndEdit();
                }
                this.addNewRow = this.table.NewRow();
                DataRowView view = new DataRowView(this, this.addNewRow);
                this.rowViewCache.Add(this.addNewRow, view);
                this.OnListChanged(new ListChangedEventArgs(ListChangedType.ItemAdded, this.IndexOf(view)));
                view2 = view;
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
            return view2;
        }

        public void BeginInit()
        {
            this.fInitInProgress = true;
        }

        private void CheckOpen()
        {
            if (!this.IsOpen)
            {
                throw ExceptionBuilder.NotOpen();
            }
        }

        private void CheckSort(string sort)
        {
            if (this.table == null)
            {
                throw ExceptionBuilder.CanNotUse();
            }
            if (sort.Length != 0)
            {
                this.table.ParseSortString(sort);
            }
        }

        internal void ChildRelationCollectionChanged(object sender, CollectionChangeEventArgs e)
        {
            DataRelationPropertyDescriptor propDesc = null;
            this.OnListChanged((e.Action == CollectionChangeAction.Add) ? new ListChangedEventArgs(ListChangedType.PropertyDescriptorAdded, new DataRelationPropertyDescriptor((DataRelation) e.Element)) : ((e.Action == CollectionChangeAction.Refresh) ? new ListChangedEventArgs(ListChangedType.PropertyDescriptorChanged, propDesc) : ((e.Action == CollectionChangeAction.Remove) ? new ListChangedEventArgs(ListChangedType.PropertyDescriptorDeleted, new DataRelationPropertyDescriptor((DataRelation) e.Element)) : null)));
        }

        protected void Close()
        {
            this.shouldOpen = false;
            this.UpdateIndex();
            this.dvListener.UnregisterMetaDataEvents();
        }

        protected virtual void ColumnCollectionChanged(object sender, CollectionChangeEventArgs e)
        {
            DataColumnPropertyDescriptor propDesc = null;
            this.OnListChanged((e.Action == CollectionChangeAction.Add) ? new ListChangedEventArgs(ListChangedType.PropertyDescriptorAdded, new DataColumnPropertyDescriptor((DataColumn) e.Element)) : ((e.Action == CollectionChangeAction.Refresh) ? new ListChangedEventArgs(ListChangedType.PropertyDescriptorChanged, propDesc) : ((e.Action == CollectionChangeAction.Remove) ? new ListChangedEventArgs(ListChangedType.PropertyDescriptorDeleted, new DataColumnPropertyDescriptor((DataColumn) e.Element)) : null)));
        }

        internal void ColumnCollectionChangedInternal(object sender, CollectionChangeEventArgs e)
        {
            this.ColumnCollectionChanged(sender, e);
        }

        public void CopyTo(Array array, int index)
        {
            if (this.index != null)
            {
                RBTree<int>.RBTreeEnumerator enumerator = this.index.GetEnumerator(0);
                while (enumerator.MoveNext())
                {
                    array.SetValue(this.GetRowView(enumerator.Current), index);
                    index++;
                }
            }
            if (this.addNewRow != null)
            {
                array.SetValue(this.rowViewCache[this.addNewRow], index);
            }
        }

        private void CopyTo(DataRowView[] array, int index)
        {
            if (this.index != null)
            {
                RBTree<int>.RBTreeEnumerator enumerator = this.index.GetEnumerator(0);
                while (enumerator.MoveNext())
                {
                    array[index] = this.GetRowView(enumerator.Current);
                    index++;
                }
            }
            if (this.addNewRow != null)
            {
                array[index] = this.rowViewCache[this.addNewRow];
            }
        }

        private string CreateSortString(PropertyDescriptor property, ListSortDirection direction)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append('[');
            builder.Append(property.Name);
            builder.Append(']');
            if (ListSortDirection.Descending == direction)
            {
                builder.Append(" DESC");
            }
            return builder.ToString();
        }

        internal void Delete(DataRow row)
        {
            if (row != null)
            {
                IntPtr ptr;
                Bid.ScopeEnter(out ptr, "<ds.DataView.Delete|API> %d#, row=%d#", this.ObjectID, row.ObjectID);
                try
                {
                    this.CheckOpen();
                    if (row == this.addNewRow)
                    {
                        this.FinishAddNew(false);
                    }
                    else
                    {
                        if (!this.AllowDelete)
                        {
                            throw ExceptionBuilder.CanNotDelete();
                        }
                        row.Delete();
                    }
                }
                finally
                {
                    Bid.ScopeLeave(ref ptr);
                }
            }
        }

        public void Delete(int index)
        {
            this.Delete(this.GetRow(index));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Close();
            }
            base.Dispose(disposing);
        }

        public void EndInit()
        {
            if ((this.delayedTable != null) && this.delayedTable.fInitInProgress)
            {
                this.delayedTable.delayedViews.Add(this);
            }
            else
            {
                this.fInitInProgress = false;
                this.fEndInitInProgress = true;
                if (this.delayedTable != null)
                {
                    this.Table = this.delayedTable;
                    this.delayedTable = null;
                }
                if (this.delayedSort != null)
                {
                    this.Sort = this.delayedSort;
                    this.delayedSort = null;
                }
                if (this.delayedRowFilter != null)
                {
                    this.RowFilter = this.delayedRowFilter;
                    this.delayedRowFilter = null;
                }
                if (this.delayedRecordStates != ~DataViewRowState.None)
                {
                    this.RowStateFilter = this.delayedRecordStates;
                    this.delayedRecordStates = ~DataViewRowState.None;
                }
                this.fEndInitInProgress = false;
                this.SetIndex(this.Sort, this.RowStateFilter, this.rowFilter);
                this.OnInitialized();
            }
        }

        public virtual bool Equals(DataView view)
        {
            return (((((view != null) && (this.Table == view.Table)) && ((this.Count == view.Count) && (string.Compare(this.RowFilter, view.RowFilter, StringComparison.OrdinalIgnoreCase) == 0))) && (((string.Compare(this.Sort, view.Sort, StringComparison.OrdinalIgnoreCase) == 0) && object.ReferenceEquals(this.SortComparison, view.SortComparison)) && (object.ReferenceEquals(this.RowPredicate, view.RowPredicate) && (this.RowStateFilter == view.RowStateFilter)))) && (((this.DataViewManager == view.DataViewManager) && (this.AllowDelete == view.AllowDelete)) && ((this.AllowNew == view.AllowNew) && (this.AllowEdit == view.AllowEdit))));
        }

        public int Find(object key)
        {
            return this.FindByKey(key);
        }

        public int Find(object[] key)
        {
            return this.FindByKey(key);
        }

        internal virtual int FindByKey(object key)
        {
            return this.index.FindRecordByKey(key);
        }

        internal virtual int FindByKey(object[] key)
        {
            return this.index.FindRecordByKey(key);
        }

        internal System.Data.Range FindRecords<TKey, TRow>(Index.ComparisonBySelector<TKey, TRow> comparison, TKey key) where TRow: DataRow
        {
            return this.index.FindRecords<TKey, TRow>(comparison, key);
        }

        public DataRowView[] FindRows(object key)
        {
            return this.FindRowsByKey(new object[] { key });
        }

        public DataRowView[] FindRows(object[] key)
        {
            return this.FindRowsByKey(key);
        }

        internal virtual DataRowView[] FindRowsByKey(object[] key)
        {
            DataRowView[] dataRowViewFromRange;
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<ds.DataView.FindRows|API> %d#\n", this.ObjectID);
            try
            {
                System.Data.Range range = this.index.FindRecords(key);
                dataRowViewFromRange = this.GetDataRowViewFromRange(range);
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
            return dataRowViewFromRange;
        }

        internal void FinishAddNew(bool success)
        {
            Bid.Trace("<ds.DataView.FinishAddNew|INFO> %d#, success=%d{bool}\n", this.ObjectID, success);
            DataRow addNewRow = this.addNewRow;
            if (success)
            {
                if (DataRowState.Detached == addNewRow.RowState)
                {
                    this.table.Rows.Add(addNewRow);
                }
                else
                {
                    addNewRow.EndEdit();
                }
            }
            if (addNewRow == this.addNewRow)
            {
                this.rowViewCache.Remove(this.addNewRow);
                this.addNewRow = null;
                if (!success)
                {
                    addNewRow.CancelEdit();
                }
                this.OnListChanged(new ListChangedEventArgs(ListChangedType.ItemDeleted, this.Count));
            }
        }

        internal DataRowView[] GetDataRowViewFromRange(System.Data.Range range)
        {
            if (range.IsNull)
            {
                return new DataRowView[0];
            }
            DataRowView[] viewArray = new DataRowView[range.Count];
            for (int i = 0; i < viewArray.Length; i++)
            {
                viewArray[i] = this[i + range.Min];
            }
            return viewArray;
        }

        public IEnumerator GetEnumerator()
        {
            DataRowView[] array = new DataRowView[this.Count];
            this.CopyTo(array, 0);
            return array.GetEnumerator();
        }

        internal virtual IFilter GetFilter()
        {
            return this.rowFilter;
        }

        internal Index GetFindIndex(string column, bool keepIndex)
        {
            Index index;
            if (this.findIndexes == null)
            {
                this.findIndexes = new Dictionary<string, Index>();
            }
            if (this.findIndexes.TryGetValue(column, out index))
            {
                if (!keepIndex)
                {
                    this.findIndexes.Remove(column);
                    index.RemoveRef();
                    if (index.RefCount == 1)
                    {
                        index.RemoveRef();
                    }
                }
                return index;
            }
            if (keepIndex)
            {
                index = this.table.GetIndex(column, this.recordStates, this.GetFilter());
                this.findIndexes[column] = index;
                index.AddRef();
            }
            return index;
        }

        private int GetRecord(int recordIndex)
        {
            if (this.Count <= recordIndex)
            {
                throw ExceptionBuilder.RowOutOfRange(recordIndex);
            }
            if (recordIndex == this.index.RecordCount)
            {
                return this.addNewRow.GetDefaultRecord();
            }
            return this.index.GetRecord(recordIndex);
        }

        internal DataRow GetRow(int index)
        {
            int count = this.Count;
            if (count <= index)
            {
                throw ExceptionBuilder.GetElementIndex(index);
            }
            if ((index == (count - 1)) && (this.addNewRow != null))
            {
                return this.addNewRow;
            }
            return this.table.recordManager[this.GetRecord(index)];
        }

        private DataRowView GetRowView(DataRow dr)
        {
            return this.rowViewCache[dr];
        }

        private DataRowView GetRowView(int record)
        {
            return this.GetRowView(this.table.recordManager[record]);
        }

        internal ListSortDescriptionCollection GetSortDescriptions()
        {
            ListSortDescription[] sorts = new ListSortDescription[0];
            if (((this.table != null) && (this.index != null)) && (this.index.IndexFields.Length > 0))
            {
                sorts = new ListSortDescription[this.index.IndexFields.Length];
                for (int i = 0; i < this.index.IndexFields.Length; i++)
                {
                    DataColumnPropertyDescriptor property = new DataColumnPropertyDescriptor(this.index.IndexFields[i].Column);
                    if (this.index.IndexFields[i].IsDescending)
                    {
                        sorts[i] = new ListSortDescription(property, ListSortDirection.Descending);
                    }
                    else
                    {
                        sorts[i] = new ListSortDescription(property, ListSortDirection.Ascending);
                    }
                }
            }
            return new ListSortDescriptionCollection(sorts);
        }

        internal PropertyDescriptor GetSortProperty()
        {
            if (((this.table != null) && (this.index != null)) && (this.index.IndexFields.Length == 1))
            {
                return new DataColumnPropertyDescriptor(this.index.IndexFields[0].Column);
            }
            return null;
        }

        protected virtual void IndexListChanged(object sender, ListChangedEventArgs e)
        {
            if (e.ListChangedType != ListChangedType.Reset)
            {
                this.OnListChanged(e);
            }
            if ((this.addNewRow != null) && (this.index.RecordCount == 0))
            {
                this.FinishAddNew(false);
            }
            if (e.ListChangedType == ListChangedType.Reset)
            {
                this.OnListChanged(e);
            }
        }

        internal void IndexListChangedInternal(ListChangedEventArgs e)
        {
            this.rowViewBuffer.Clear();
            if (((ListChangedType.ItemAdded == e.ListChangedType) && (this.addNewMoved != null)) && (this.addNewMoved.NewIndex != this.addNewMoved.OldIndex))
            {
                ListChangedEventArgs addNewMoved = this.addNewMoved;
                this.addNewMoved = null;
                this.IndexListChanged(this, addNewMoved);
            }
            this.IndexListChanged(this, e);
        }

        internal int IndexOf(DataRowView rowview)
        {
            if (rowview != null)
            {
                DataRowView view;
                if (object.ReferenceEquals(this.addNewRow, rowview.Row))
                {
                    return (this.Count - 1);
                }
                if (((this.index != null) && (DataRowState.Detached != rowview.Row.RowState)) && (this.rowViewCache.TryGetValue(rowview.Row, out view) && (view == rowview)))
                {
                    return this.IndexOfDataRowView(rowview);
                }
            }
            return -1;
        }

        private int IndexOfDataRowView(DataRowView rowview)
        {
            return this.index.GetIndex(rowview.Row.GetRecordFromVersion(rowview.Row.GetDefaultRowVersion(this.RowStateFilter) & ~DataRowVersion.Proposed));
        }

        internal void MaintainDataView(ListChangedType changedType, DataRow row, bool trackAddRemove)
        {
            DataRowView view = null;
            switch (changedType)
            {
                case ListChangedType.Reset:
                    this.ResetRowViewCache();
                    break;

                case ListChangedType.ItemAdded:
                    if (trackAddRemove && this.rowViewBuffer.TryGetValue(row, out view))
                    {
                        this.rowViewBuffer.Remove(row);
                    }
                    if (row == this.addNewRow)
                    {
                        int newIndex = this.IndexOfDataRowView(this.rowViewCache[this.addNewRow]);
                        this.addNewRow = null;
                        this.addNewMoved = new ListChangedEventArgs(ListChangedType.ItemMoved, newIndex, this.Count - 1);
                        return;
                    }
                    if (this.rowViewCache.ContainsKey(row))
                    {
                        break;
                    }
                    this.rowViewCache.Add(row, view ?? new DataRowView(this, row));
                    return;

                case ListChangedType.ItemDeleted:
                    if (trackAddRemove)
                    {
                        this.rowViewCache.TryGetValue(row, out view);
                        if (view != null)
                        {
                            this.rowViewBuffer.Add(row, view);
                        }
                    }
                    if (!this.rowViewCache.Remove(row))
                    {
                        return;
                    }
                    break;

                case ListChangedType.ItemMoved:
                case ListChangedType.ItemChanged:
                case ListChangedType.PropertyDescriptorAdded:
                case ListChangedType.PropertyDescriptorDeleted:
                case ListChangedType.PropertyDescriptorChanged:
                    break;

                default:
                    return;
            }
        }

        private void OnInitialized()
        {
            if (this.onInitialized != null)
            {
                this.onInitialized(this, EventArgs.Empty);
            }
        }

        protected virtual void OnListChanged(ListChangedEventArgs e)
        {
            Bid.Trace("<ds.DataView.OnListChanged|INFO> %d#, ListChangedType=%d{ListChangedType}\n", this.ObjectID, (int) e.ListChangedType);
            try
            {
                DataColumn dataColumn = null;
                string propName = null;
                switch (e.ListChangedType)
                {
                    case ListChangedType.ItemMoved:
                    case ListChangedType.ItemChanged:
                        if (0 <= e.NewIndex)
                        {
                            DataRow row = this.GetRow(e.NewIndex);
                            if (row.HasPropertyChanged)
                            {
                                dataColumn = row.LastChangedColumn;
                                propName = (dataColumn != null) ? dataColumn.ColumnName : string.Empty;
                            }
                        }
                        break;
                }
                if (this.onListChanged != null)
                {
                    if ((dataColumn != null) && (e.NewIndex == e.OldIndex))
                    {
                        ListChangedEventArgs args = new ListChangedEventArgs(e.ListChangedType, e.NewIndex, new DataColumnPropertyDescriptor(dataColumn));
                        this.onListChanged(this, args);
                    }
                    else
                    {
                        this.onListChanged(this, e);
                    }
                }
                if (propName != null)
                {
                    this[e.NewIndex].RaisePropertyChangedEvent(propName);
                }
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

        protected void Open()
        {
            this.shouldOpen = true;
            this.UpdateIndex();
            this.dvListener.RegisterMetaDataEvents(this.table);
        }

        internal void ParentRelationCollectionChanged(object sender, CollectionChangeEventArgs e)
        {
            DataRelationPropertyDescriptor propDesc = null;
            this.OnListChanged((e.Action == CollectionChangeAction.Add) ? new ListChangedEventArgs(ListChangedType.PropertyDescriptorAdded, new DataRelationPropertyDescriptor((DataRelation) e.Element)) : ((e.Action == CollectionChangeAction.Refresh) ? new ListChangedEventArgs(ListChangedType.PropertyDescriptorChanged, propDesc) : ((e.Action == CollectionChangeAction.Remove) ? new ListChangedEventArgs(ListChangedType.PropertyDescriptorDeleted, new DataRelationPropertyDescriptor((DataRelation) e.Element)) : null)));
        }

        protected void Reset()
        {
            if (this.IsOpen)
            {
                this.index.Reset();
            }
        }

        internal void ResetRowViewCache()
        {
            DataRowView view;
            Dictionary<DataRow, DataRowView> dictionary = new Dictionary<DataRow, DataRowView>(this.CountFromIndex, DataRowReferenceComparer.Default);
            if (this.index != null)
            {
                RBTree<int>.RBTreeEnumerator enumerator = this.index.GetEnumerator(0);
                while (enumerator.MoveNext())
                {
                    DataRow key = this.table.recordManager[enumerator.Current];
                    if (!this.rowViewCache.TryGetValue(key, out view))
                    {
                        view = new DataRowView(this, key);
                    }
                    dictionary.Add(key, view);
                }
            }
            if (this.addNewRow != null)
            {
                this.rowViewCache.TryGetValue(this.addNewRow, out view);
                dictionary.Add(this.addNewRow, view);
            }
            this.rowViewCache = dictionary;
        }

        private void ResetSort()
        {
            this.sort = "";
            this.SetIndex(this.sort, this.recordStates, this.rowFilter);
        }

        private bool RowExist(List<object[]> arraylist, object[] objectArray)
        {
            for (int i = 0; i < arraylist.Count; i++)
            {
                object[] objArray = arraylist[i];
                bool flag = true;
                for (int j = 0; j < objectArray.Length; j++)
                {
                    flag &= objArray[j].Equals(objectArray[j]);
                }
                if (flag)
                {
                    return true;
                }
            }
            return false;
        }

        internal void SetDataViewManager(System.Data.DataViewManager dataViewManager)
        {
            if (this.table == null)
            {
                throw ExceptionBuilder.CanNotUse();
            }
            if (this.dataViewManager != dataViewManager)
            {
                if (dataViewManager != null)
                {
                    dataViewManager.nViews--;
                }
                this.dataViewManager = dataViewManager;
                if (dataViewManager != null)
                {
                    dataViewManager.nViews++;
                    DataViewSetting setting = dataViewManager.DataViewSettings[this.table];
                    try
                    {
                        this.applyDefaultSort = setting.ApplyDefaultSort;
                        DataExpression newRowFilter = new DataExpression(this.table, setting.RowFilter);
                        this.SetIndex(setting.Sort, setting.RowStateFilter, newRowFilter);
                    }
                    catch (Exception exception)
                    {
                        if (!ADP.IsCatchableExceptionType(exception))
                        {
                            throw;
                        }
                        ExceptionBuilder.TraceExceptionWithoutRethrow(exception);
                    }
                    this.locked = true;
                }
                else
                {
                    this.SetIndex("", DataViewRowState.CurrentRows, null);
                }
            }
        }

        internal virtual void SetIndex(string newSort, DataViewRowState newRowStates, IFilter newRowFilter)
        {
            this.SetIndex2(newSort, newRowStates, newRowFilter, true);
        }

        internal void SetIndex2(string newSort, DataViewRowState newRowStates, IFilter newRowFilter, bool fireEvent)
        {
            Bid.Trace("<ds.DataView.SetIndex|INFO> %d#, newSort='%ls', newRowStates=%d{ds.DataViewRowState}\n", this.ObjectID, newSort, (int) newRowStates);
            this.sort = newSort;
            this.recordStates = newRowStates;
            this.rowFilter = newRowFilter;
            if (!this.fEndInitInProgress)
            {
                if (fireEvent)
                {
                    this.UpdateIndex(true);
                }
                else
                {
                    this.UpdateIndex(true, false);
                }
                if (this.findIndexes != null)
                {
                    Dictionary<string, Index> findIndexes = this.findIndexes;
                    this.findIndexes = null;
                    foreach (KeyValuePair<string, Index> pair in findIndexes)
                    {
                        pair.Value.RemoveRef();
                    }
                }
            }
        }

        private bool ShouldSerializeSort()
        {
            return (this.sort != null);
        }

        int IList.Add(object value)
        {
            if (value != null)
            {
                throw ExceptionBuilder.AddExternalObject();
            }
            this.AddNew();
            return (this.Count - 1);
        }

        void IList.Clear()
        {
            throw ExceptionBuilder.CanNotClear();
        }

        bool IList.Contains(object value)
        {
            return (0 <= this.IndexOf(value as DataRowView));
        }

        int IList.IndexOf(object value)
        {
            return this.IndexOf(value as DataRowView);
        }

        void IList.Insert(int index, object value)
        {
            throw ExceptionBuilder.InsertExternalObject();
        }

        void IList.Remove(object value)
        {
            int index = this.IndexOf(value as DataRowView);
            if (0 > index)
            {
                throw ExceptionBuilder.RemoveExternalObject();
            }
            ((IList) this).RemoveAt(index);
        }

        void IList.RemoveAt(int index)
        {
            this.Delete(index);
        }

        void IBindingList.AddIndex(PropertyDescriptor property)
        {
            this.GetFindIndex(property.Name, true);
        }

        object IBindingList.AddNew()
        {
            return this.AddNew();
        }

        void IBindingList.ApplySort(PropertyDescriptor property, ListSortDirection direction)
        {
            this.Sort = this.CreateSortString(property, direction);
        }

        int IBindingList.Find(PropertyDescriptor property, object key)
        {
            if (property != null)
            {
                bool flag = false;
                Index index = null;
                try
                {
                    if ((this.findIndexes == null) || !this.findIndexes.TryGetValue(property.Name, out index))
                    {
                        flag = true;
                        index = this.table.GetIndex(property.Name, this.recordStates, this.GetFilter());
                        index.AddRef();
                    }
                    System.Data.Range range = index.FindRecords(key);
                    if (!range.IsNull)
                    {
                        return this.index.GetIndex(index.GetRecord(range.Min));
                    }
                }
                finally
                {
                    if (flag && (index != null))
                    {
                        index.RemoveRef();
                        if (index.RefCount == 1)
                        {
                            index.RemoveRef();
                        }
                    }
                }
            }
            return -1;
        }

        void IBindingList.RemoveIndex(PropertyDescriptor property)
        {
            this.GetFindIndex(property.Name, false);
        }

        void IBindingList.RemoveSort()
        {
            Bid.Trace("<ds.DataView.RemoveSort|API> %d#\n", this.ObjectID);
            this.Sort = string.Empty;
        }

        void IBindingListView.ApplySort(ListSortDescriptionCollection sorts)
        {
            if (sorts == null)
            {
                throw ExceptionBuilder.ArgumentNull("sorts");
            }
            StringBuilder builder = new StringBuilder();
            bool flag = false;
            foreach (ListSortDescription description in (IEnumerable) sorts)
            {
                if (description == null)
                {
                    throw ExceptionBuilder.ArgumentContainsNull("sorts");
                }
                PropertyDescriptor propertyDescriptor = description.PropertyDescriptor;
                if (propertyDescriptor == null)
                {
                    throw ExceptionBuilder.ArgumentNull("PropertyDescriptor");
                }
                if (!this.table.Columns.Contains(propertyDescriptor.Name))
                {
                    throw ExceptionBuilder.ColumnToSortIsOutOfRange(propertyDescriptor.Name);
                }
                ListSortDirection sortDirection = description.SortDirection;
                if (flag)
                {
                    builder.Append(',');
                }
                builder.Append(this.CreateSortString(propertyDescriptor, sortDirection));
                if (!flag)
                {
                    flag = true;
                }
            }
            this.Sort = builder.ToString();
        }

        void IBindingListView.RemoveFilter()
        {
            Bid.Trace("<ds.DataView.RemoveFilter|API> %d#\n", this.ObjectID);
            this.RowFilter = "";
        }

        PropertyDescriptorCollection ITypedList.GetItemProperties(PropertyDescriptor[] listAccessors)
        {
            if (this.table != null)
            {
                if ((listAccessors == null) || (listAccessors.Length == 0))
                {
                    return this.table.GetPropertyDescriptorCollection(null);
                }
                DataSet dataSet = this.table.DataSet;
                if (dataSet == null)
                {
                    return new PropertyDescriptorCollection(null);
                }
                DataTable table = dataSet.FindTable(this.table, listAccessors, 0);
                if (table != null)
                {
                    return table.GetPropertyDescriptorCollection(null);
                }
            }
            return new PropertyDescriptorCollection(null);
        }

        string ITypedList.GetListName(PropertyDescriptor[] listAccessors)
        {
            if (this.table != null)
            {
                if ((listAccessors == null) || (listAccessors.Length == 0))
                {
                    return this.table.TableName;
                }
                DataSet dataSet = this.table.DataSet;
                if (dataSet != null)
                {
                    DataTable table = dataSet.FindTable(this.table, listAccessors, 0);
                    if (table != null)
                    {
                        return table.TableName;
                    }
                }
            }
            return string.Empty;
        }

        public DataTable ToTable()
        {
            return this.ToTable(null, false, new string[0]);
        }

        public DataTable ToTable(string tableName)
        {
            return this.ToTable(tableName, false, new string[0]);
        }

        public DataTable ToTable(bool distinct, params string[] columnNames)
        {
            return this.ToTable(null, distinct, columnNames);
        }

        public DataTable ToTable(string tableName, bool distinct, params string[] columnNames)
        {
            Bid.Trace("<ds.DataView.ToTable|API> %d#, TableName='%ls', distinct=%d{bool}\n", this.ObjectID, tableName, distinct);
            if (columnNames == null)
            {
                throw ExceptionBuilder.ArgumentNull("columnNames");
            }
            DataTable table = new DataTable {
                Locale = this.table.Locale,
                CaseSensitive = this.table.CaseSensitive,
                TableName = (tableName != null) ? tableName : this.table.TableName,
                Namespace = this.table.Namespace,
                Prefix = this.table.Prefix
            };
            if (columnNames.Length == 0)
            {
                columnNames = new string[this.Table.Columns.Count];
                for (int j = 0; j < columnNames.Length; j++)
                {
                    columnNames[j] = this.Table.Columns[j].ColumnName;
                }
            }
            int[] numArray = new int[columnNames.Length];
            List<object[]> arraylist = new List<object[]>();
            for (int i = 0; i < columnNames.Length; i++)
            {
                DataColumn column = this.Table.Columns[columnNames[i]];
                if (column == null)
                {
                    throw ExceptionBuilder.ColumnNotInTheUnderlyingTable(columnNames[i], this.Table.TableName);
                }
                table.Columns.Add(column.Clone());
                numArray[i] = this.Table.Columns.IndexOf(column);
            }
            foreach (DataRowView view in this)
            {
                object[] objectArray = new object[columnNames.Length];
                for (int k = 0; k < numArray.Length; k++)
                {
                    objectArray[k] = view[numArray[k]];
                }
                if (!distinct || !this.RowExist(arraylist, objectArray))
                {
                    table.Rows.Add(objectArray);
                    arraylist.Add(objectArray);
                }
            }
            return table;
        }

        protected void UpdateIndex()
        {
            this.UpdateIndex(false);
        }

        protected virtual void UpdateIndex(bool force)
        {
            this.UpdateIndex(force, true);
        }

        internal void UpdateIndex(bool force, bool fireEvent)
        {
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<ds.DataView.UpdateIndex|INFO> %d#, force=%d{bool}\n", this.ObjectID, force);
            try
            {
                if ((this.open != this.shouldOpen) || force)
                {
                    this.open = this.shouldOpen;
                    Index index = null;
                    if (this.open && (this.table != null))
                    {
                        if (this.SortComparison != null)
                        {
                            index = new Index(this.table, this.SortComparison, this.recordStates, this.GetFilter());
                            index.AddRef();
                        }
                        else
                        {
                            index = this.table.GetIndex(this.Sort, this.recordStates, this.GetFilter());
                        }
                    }
                    if (this.index != index)
                    {
                        if (this.index == null)
                        {
                            DataTable table = index.Table;
                        }
                        else
                        {
                            DataTable table2 = this.index.Table;
                        }
                        if (this.index != null)
                        {
                            this.dvListener.UnregisterListChangedEvent();
                        }
                        this.index = index;
                        if (this.index != null)
                        {
                            this.dvListener.RegisterListChangedEvent(this.index);
                        }
                        this.ResetRowViewCache();
                        if (fireEvent)
                        {
                            this.OnListChanged(ResetEventArgs);
                        }
                    }
                }
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
        }

        [ResDescription("DataViewAllowDeleteDescr"), ResCategory("DataCategory_Data"), DefaultValue(true)]
        public bool AllowDelete
        {
            get
            {
                return this.allowDelete;
            }
            set
            {
                if (this.allowDelete != value)
                {
                    this.allowDelete = value;
                    this.OnListChanged(ResetEventArgs);
                }
            }
        }

        [ResDescription("DataViewAllowEditDescr"), ResCategory("DataCategory_Data"), DefaultValue(true)]
        public bool AllowEdit
        {
            get
            {
                return this.allowEdit;
            }
            set
            {
                if (this.allowEdit != value)
                {
                    this.allowEdit = value;
                    this.OnListChanged(ResetEventArgs);
                }
            }
        }

        [DefaultValue(true), ResDescription("DataViewAllowNewDescr"), ResCategory("DataCategory_Data")]
        public bool AllowNew
        {
            get
            {
                return this.allowNew;
            }
            set
            {
                if (this.allowNew != value)
                {
                    this.allowNew = value;
                    this.OnListChanged(ResetEventArgs);
                }
            }
        }

        [RefreshProperties(RefreshProperties.All), ResDescription("DataViewApplyDefaultSortDescr"), DefaultValue(false), ResCategory("DataCategory_Data")]
        public bool ApplyDefaultSort
        {
            get
            {
                return this.applyDefaultSort;
            }
            set
            {
                Bid.Trace("<ds.DataView.set_ApplyDefaultSort|API> %d#, %d{bool}\n", this.ObjectID, value);
                if (this.applyDefaultSort != value)
                {
                    this._comparison = null;
                    this.applyDefaultSort = value;
                    this.UpdateIndex(true);
                    this.OnListChanged(ResetEventArgs);
                }
            }
        }

        [ResDescription("DataViewCountDescr"), Browsable(false)]
        public int Count
        {
            get
            {
                return this.rowViewCache.Count;
            }
        }

        private int CountFromIndex
        {
            get
            {
                return (((this.index != null) ? this.index.RecordCount : 0) + ((this.addNewRow != null) ? 1 : 0));
            }
        }

        [Browsable(false), ResDescription("DataViewDataViewManagerDescr")]
        public System.Data.DataViewManager DataViewManager
        {
            get
            {
                return this.dataViewManager;
            }
        }

        [Browsable(false)]
        public bool IsInitialized
        {
            get
            {
                return !this.fInitInProgress;
            }
        }

        [Browsable(false), ResDescription("DataViewIsOpenDescr")]
        protected bool IsOpen
        {
            get
            {
                return this.open;
            }
        }

        public DataRowView this[int recordIndex]
        {
            get
            {
                return this.GetRowView(this.GetRow(recordIndex));
            }
        }

        internal int ObjectID
        {
            get
            {
                return this._objectID;
            }
        }

        [ResDescription("DataViewRowFilterDescr"), DefaultValue(""), ResCategory("DataCategory_Data")]
        public virtual string RowFilter
        {
            get
            {
                DataExpression rowFilter = this.rowFilter as DataExpression;
                if (rowFilter != null)
                {
                    return rowFilter.Expression;
                }
                return "";
            }
            set
            {
                if (value == null)
                {
                    value = "";
                }
                Bid.Trace("<ds.DataView.set_RowFilter|API> %d#, '%ls'\n", this.ObjectID, value);
                if (this.fInitInProgress)
                {
                    this.delayedRowFilter = value;
                }
                else
                {
                    CultureInfo culture = (this.table != null) ? this.table.Locale : CultureInfo.CurrentCulture;
                    if ((this.rowFilter == null) || (string.Compare(this.RowFilter, value, false, culture) != 0))
                    {
                        DataExpression newRowFilter = new DataExpression(this.table, value);
                        this.SetIndex(this.sort, this.recordStates, newRowFilter);
                    }
                }
            }
        }

        internal Predicate<DataRow> RowPredicate
        {
            get
            {
                RowPredicateFilter filter = this.GetFilter() as RowPredicateFilter;
                if (filter == null)
                {
                    return null;
                }
                return filter.PredicateFilter;
            }
            set
            {
                if (!object.ReferenceEquals(this.RowPredicate, value))
                {
                    this.SetIndex(this.Sort, this.RowStateFilter, (value != null) ? new RowPredicateFilter(value) : null);
                }
            }
        }

        [DefaultValue(0x16), ResDescription("DataViewRowStateFilterDescr"), ResCategory("DataCategory_Data")]
        public DataViewRowState RowStateFilter
        {
            get
            {
                return this.recordStates;
            }
            set
            {
                Bid.Trace("<ds.DataView.set_RowStateFilter|API> %d#, %d{ds.DataViewRowState}\n", this.ObjectID, (int) value);
                if (this.fInitInProgress)
                {
                    this.delayedRecordStates = value;
                }
                else
                {
                    if ((value & ~(DataViewRowState.OriginalRows | DataViewRowState.ModifiedCurrent | DataViewRowState.Added)) != DataViewRowState.None)
                    {
                        throw ExceptionBuilder.RecordStateRange();
                    }
                    if (((value & DataViewRowState.ModifiedOriginal) != DataViewRowState.None) && ((value & DataViewRowState.ModifiedCurrent) != DataViewRowState.None))
                    {
                        throw ExceptionBuilder.SetRowStateFilter();
                    }
                    if (this.recordStates != value)
                    {
                        this.SetIndex(this.sort, value, this.rowFilter);
                    }
                }
            }
        }

        [ResDescription("DataViewSortDescr"), DefaultValue(""), ResCategory("DataCategory_Data")]
        public string Sort
        {
            get
            {
                if (((this.sort.Length == 0) && this.applyDefaultSort) && ((this.table != null) && (this.table._primaryIndex.Length > 0)))
                {
                    return this.table.FormatSortString(this.table._primaryIndex);
                }
                return this.sort;
            }
            set
            {
                if (value == null)
                {
                    value = "";
                }
                Bid.Trace("<ds.DataView.set_Sort|API> %d#, '%ls'\n", this.ObjectID, value);
                if (this.fInitInProgress)
                {
                    this.delayedSort = value;
                }
                else
                {
                    CultureInfo culture = (this.table != null) ? this.table.Locale : CultureInfo.CurrentCulture;
                    if ((string.Compare(this.sort, value, false, culture) != 0) || (this._comparison != null))
                    {
                        this.CheckSort(value);
                        this._comparison = null;
                        this.SetIndex(value, this.recordStates, this.rowFilter);
                    }
                }
            }
        }

        internal Comparison<DataRow> SortComparison
        {
            get
            {
                return this._comparison;
            }
            set
            {
                Bid.Trace("<ds.DataView.set_SortComparison|API> %d#\n", this.ObjectID);
                if (!object.ReferenceEquals(this._comparison, value))
                {
                    this._comparison = value;
                    this.SetIndex("", this.recordStates, this.rowFilter);
                }
            }
        }

        bool ICollection.IsSynchronized
        {
            get
            {
                return false;
            }
        }

        object ICollection.SyncRoot
        {
            get
            {
                return this;
            }
        }

        bool IList.IsFixedSize
        {
            get
            {
                return false;
            }
        }

        bool IList.IsReadOnly
        {
            get
            {
                return false;
            }
        }

        object IList.this[int recordIndex]
        {
            get
            {
                return this[recordIndex];
            }
            set
            {
                throw ExceptionBuilder.SetIListObject();
            }
        }

        bool IBindingList.AllowEdit
        {
            get
            {
                return this.AllowEdit;
            }
        }

        bool IBindingList.AllowNew
        {
            get
            {
                return this.AllowNew;
            }
        }

        bool IBindingList.AllowRemove
        {
            get
            {
                return this.AllowDelete;
            }
        }

        bool IBindingList.IsSorted
        {
            get
            {
                return (this.Sort.Length != 0);
            }
        }

        ListSortDirection IBindingList.SortDirection
        {
            get
            {
                if ((this.index.IndexFields.Length == 1) && this.index.IndexFields[0].IsDescending)
                {
                    return ListSortDirection.Descending;
                }
                return ListSortDirection.Ascending;
            }
        }

        PropertyDescriptor IBindingList.SortProperty
        {
            get
            {
                return this.GetSortProperty();
            }
        }

        bool IBindingList.SupportsChangeNotification
        {
            get
            {
                return true;
            }
        }

        bool IBindingList.SupportsSearching
        {
            get
            {
                return true;
            }
        }

        bool IBindingList.SupportsSorting
        {
            get
            {
                return true;
            }
        }

        string IBindingListView.Filter
        {
            get
            {
                return this.RowFilter;
            }
            set
            {
                this.RowFilter = value;
            }
        }

        ListSortDescriptionCollection IBindingListView.SortDescriptions
        {
            get
            {
                return this.GetSortDescriptions();
            }
        }

        bool IBindingListView.SupportsAdvancedSorting
        {
            get
            {
                return true;
            }
        }

        bool IBindingListView.SupportsFiltering
        {
            get
            {
                return true;
            }
        }

        [TypeConverter(typeof(DataTableTypeConverter)), DefaultValue((string) null), RefreshProperties(RefreshProperties.All), ResCategory("DataCategory_Data"), ResDescription("DataViewTableDescr")]
        public DataTable Table
        {
            get
            {
                return this.table;
            }
            set
            {
                Bid.Trace("<ds.DataView.set_Table|API> %d#, %d\n", this.ObjectID, (value != null) ? value.ObjectID : 0);
                if (this.fInitInProgress && (value != null))
                {
                    this.delayedTable = value;
                }
                else
                {
                    if (this.locked)
                    {
                        throw ExceptionBuilder.SetTable();
                    }
                    if (this.dataViewManager != null)
                    {
                        throw ExceptionBuilder.CanNotSetTable();
                    }
                    if ((value != null) && (value.TableName.Length == 0))
                    {
                        throw ExceptionBuilder.CanNotBindTable();
                    }
                    if (this.table != value)
                    {
                        this.dvListener.UnregisterMetaDataEvents();
                        this.table = value;
                        if (this.table != null)
                        {
                            this.dvListener.RegisterMetaDataEvents(this.table);
                        }
                        this.SetIndex2("", DataViewRowState.CurrentRows, null, false);
                        if (this.table != null)
                        {
                            this.OnListChanged(new ListChangedEventArgs(ListChangedType.PropertyDescriptorChanged, new DataTablePropertyDescriptor(this.table)));
                        }
                        this.OnListChanged(ResetEventArgs);
                    }
                }
            }
        }

        private sealed class DataRowReferenceComparer : IEqualityComparer<DataRow>
        {
            internal static readonly DataView.DataRowReferenceComparer Default = new DataView.DataRowReferenceComparer();

            private DataRowReferenceComparer()
            {
            }

            public bool Equals(DataRow x, DataRow y)
            {
                return (x == y);
            }

            public int GetHashCode(DataRow obj)
            {
                return obj.ObjectID;
            }
        }

        private sealed class RowPredicateFilter : IFilter
        {
            internal readonly Predicate<DataRow> PredicateFilter;

            internal RowPredicateFilter(Predicate<DataRow> predicate)
            {
                this.PredicateFilter = predicate;
            }

            bool IFilter.Invoke(DataRow row, DataRowVersion version)
            {
                return this.PredicateFilter(row);
            }
        }
    }
}

