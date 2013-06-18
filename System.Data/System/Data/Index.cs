namespace System.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Threading;

    internal sealed class Index
    {
        private readonly Comparison<DataRow> _comparison;
        private readonly bool _hasRemoteAggregate;
        private Listeners<DataViewListener> _listeners;
        private readonly int _objectID;
        private static int _objectTypeCount;
        private const int DoNotReplaceCompareRecord = 0;
        internal readonly int[] IndexDesc;
        internal readonly IndexField[] IndexFields;
        private readonly bool isSharable;
        internal const int MaskBits = 0x7fffffff;
        private int recordCount;
        private IndexTree records;
        private readonly DataViewRowState recordStates;
        private int refCount;
        private const int ReplaceNewRecordForCompare = 1;
        private const int ReplaceOldRecordForCompare = 2;
        private WeakReference rowFilter;
        private bool suspendEvents;
        private readonly DataTable table;
        private static readonly object[] zeroObjects = new object[0];

        public Index(DataTable table, IndexField[] indexFields, DataViewRowState recordStates, IFilter rowFilter) : this(table, null, indexFields, recordStates, rowFilter)
        {
        }

        public Index(DataTable table, Comparison<DataRow> comparison, DataViewRowState recordStates, IFilter rowFilter) : this(table, null, GetAllFields(table.Columns), comparison, recordStates, rowFilter)
        {
        }

        public Index(DataTable table, int[] ndexDesc, IndexField[] indexFields, DataViewRowState recordStates, IFilter rowFilter) : this(table, ndexDesc, indexFields, null, recordStates, rowFilter)
        {
        }

        private Index(DataTable table, int[] ndexDesc, IndexField[] indexFields, Comparison<DataRow> comparison, DataViewRowState recordStates, IFilter rowFilter)
        {
            this._objectID = Interlocked.Increment(ref _objectTypeCount);
            Bid.Trace("<ds.Index.Index|API> %d#, table=%d, recordStates=%d{ds.DataViewRowState}\n", this.ObjectID, (table != null) ? table.ObjectID : 0, (int) recordStates);
            if ((recordStates & ~(DataViewRowState.OriginalRows | DataViewRowState.ModifiedCurrent | DataViewRowState.Added)) != DataViewRowState.None)
            {
                throw ExceptionBuilder.RecordStateRange();
            }
            this.table = table;
            this._listeners = new Listeners<DataViewListener>(this.ObjectID, listener => null != listener);
            this.IndexDesc = ndexDesc;
            this.IndexFields = indexFields;
            if (ndexDesc == null)
            {
                this.IndexDesc = Select.ConvertIndexFieldtoIndexDesc(indexFields);
            }
            this.recordStates = recordStates;
            this._comparison = comparison;
            DataColumnCollection columns = table.Columns;
            this.isSharable = (rowFilter == null) && (comparison == null);
            if (rowFilter != null)
            {
                this.rowFilter = new WeakReference(rowFilter);
                DataExpression expression = rowFilter as DataExpression;
                if (expression != null)
                {
                    this._hasRemoteAggregate = expression.HasRemoteAggregate();
                }
            }
            this.InitRecords(rowFilter);
        }

        private bool AcceptRecord(int record)
        {
            return this.AcceptRecord(record, this.RowFilter);
        }

        private bool AcceptRecord(int record, IFilter filter)
        {
            Bid.Trace("<ds.Index.AcceptRecord|API> %d#, record=%d\n", this.ObjectID, record);
            if (filter == null)
            {
                return true;
            }
            DataRow row = this.table.recordManager[record];
            if (row == null)
            {
                return true;
            }
            DataRowVersion original = DataRowVersion.Default;
            if (row.oldRecord == record)
            {
                original = DataRowVersion.Original;
            }
            else if (row.newRecord == record)
            {
                original = DataRowVersion.Current;
            }
            else if (row.tempRecord == record)
            {
                original = DataRowVersion.Proposed;
            }
            return filter.Invoke(row, original);
        }

        public void AddRef()
        {
            Bid.Trace("<ds.Index.AddRef|API> %d#\n", this.ObjectID);
            LockCookie lockCookie = this.table.indexesLock.UpgradeToWriterLock(-1);
            try
            {
                if (this.refCount == 0)
                {
                    this.table.ShadowIndexCopy();
                    this.table.indexes.Add(this);
                }
                this.refCount++;
            }
            finally
            {
                this.table.indexesLock.DowngradeFromWriterLock(ref lockCookie);
            }
        }

        private void ApplyChangeAction(int record, int action, int changeRecord)
        {
            if (action != 0)
            {
                if (action > 0)
                {
                    if (this.AcceptRecord(record))
                    {
                        this.InsertRecord(record, true);
                    }
                }
                else if ((this._comparison != null) && (-1 != record))
                {
                    this.DeleteRecord(this.GetIndex(record, changeRecord));
                }
                else
                {
                    this.DeleteRecord(this.GetIndex(record));
                }
            }
        }

        public bool CheckUnique()
        {
            return !this.HasDuplicates;
        }

        private int CompareDataRows(int record1, int record2)
        {
            return this._comparison(this.table.recordManager[record1], this.table.recordManager[record2]);
        }

        private int CompareDuplicateRecords(int record1, int record2)
        {
            if (this.table.recordManager[record1] == null)
            {
                if (this.table.recordManager[record2] != null)
                {
                    return -1;
                }
                return 0;
            }
            if (this.table.recordManager[record2] == null)
            {
                return 1;
            }
            int num = this.table.recordManager[record1].rowID.CompareTo(this.table.recordManager[record2].rowID);
            if ((num == 0) && (record1 != record2))
            {
                num = ((int) this.table.recordManager[record1].GetRecordState(record1)).CompareTo((int) this.table.recordManager[record2].GetRecordState(record2));
            }
            return num;
        }

        private int CompareRecords(int record1, int record2)
        {
            if (this._comparison != null)
            {
                return this.CompareDataRows(record1, record2);
            }
            if (0 >= this.IndexFields.Length)
            {
                return this.table.Rows.IndexOf(this.table.recordManager[record1]).CompareTo(this.table.Rows.IndexOf(this.table.recordManager[record2]));
            }
            for (int i = 0; i < this.IndexFields.Length; i++)
            {
                int num2 = this.IndexFields[i].Column.Compare(record1, record2);
                if (num2 != 0)
                {
                    if (!this.IndexFields[i].IsDescending)
                    {
                        return num2;
                    }
                    return -num2;
                }
            }
            return 0;
        }

        private int CompareRecordToKey(int record1, object[] vals)
        {
            for (int i = 0; i < this.IndexFields.Length; i++)
            {
                int num2 = this.IndexFields[i].Column.CompareValueTo(record1, vals[i]);
                if (num2 != 0)
                {
                    if (!this.IndexFields[i].IsDescending)
                    {
                        return num2;
                    }
                    return -num2;
                }
            }
            return 0;
        }

        internal static bool ContainsReference<T>(List<T> list, T item) where T: class
        {
            return (0 <= IndexOfReference<T>(list, item));
        }

        private void DeleteRecord(int recordIndex)
        {
            this.DeleteRecord(recordIndex, true);
        }

        private void DeleteRecord(int recordIndex, bool fireEvent)
        {
            Bid.Trace("<ds.Index.DeleteRecord|INFO> %d#, recordIndex=%d, fireEvent=%d{bool}\n", this.ObjectID, recordIndex, fireEvent);
            if (recordIndex >= 0)
            {
                this.recordCount--;
                int record = this.records.DeleteByIndex(recordIndex);
                this.MaintainDataView(ListChangedType.ItemDeleted, record, !fireEvent);
                if (fireEvent)
                {
                    this.OnListChanged(ListChangedType.ItemDeleted, recordIndex);
                }
            }
        }

        public void DeleteRecordFromIndex(int recordIndex)
        {
            this.DeleteRecord(recordIndex, false);
        }

        public bool Equal(IndexField[] indexDesc, DataViewRowState recordStates, IFilter rowFilter)
        {
            if ((!this.isSharable || (this.IndexFields.Length != indexDesc.Length)) || ((this.recordStates != recordStates) || (rowFilter != null)))
            {
                return false;
            }
            for (int i = 0; i < this.IndexFields.Length; i++)
            {
                if ((this.IndexFields[i].Column != indexDesc[i].Column) || (this.IndexFields[i].IsDescending != indexDesc[i].IsDescending))
                {
                    return false;
                }
            }
            return true;
        }

        private int FindNodeByKey(object originalKey)
        {
            if (this.IndexFields.Length != 1)
            {
                throw ExceptionBuilder.IndexKeyLength(this.IndexFields.Length, 1);
            }
            int root = this.records.root;
            if (root != 0)
            {
                int num2;
                DataColumn column = this.IndexFields[0].Column;
                object obj2 = column.ConvertValue(originalKey);
                root = this.records.root;
                if (!this.IndexFields[0].IsDescending)
                {
                    while (root != 0)
                    {
                        num2 = column.CompareValueTo(this.records.Key(root), obj2);
                        if (num2 == 0)
                        {
                            return root;
                        }
                        if (num2 > 0)
                        {
                            root = this.records.Left(root);
                        }
                        else
                        {
                            root = this.records.Right(root);
                        }
                    }
                    return root;
                }
                while (root != 0)
                {
                    num2 = column.CompareValueTo(this.records.Key(root), obj2);
                    if (num2 == 0)
                    {
                        return root;
                    }
                    if (num2 < 0)
                    {
                        root = this.records.Left(root);
                    }
                    else
                    {
                        root = this.records.Right(root);
                    }
                }
            }
            return root;
        }

        private int FindNodeByKeyRecord(int record)
        {
            int root = this.records.root;
            if (root != 0)
            {
                root = this.records.root;
                while (root != 0)
                {
                    int num2 = this.CompareRecords(this.records.Key(root), record);
                    if (num2 == 0)
                    {
                        return root;
                    }
                    if (num2 > 0)
                    {
                        root = this.records.Left(root);
                    }
                    else
                    {
                        root = this.records.Right(root);
                    }
                }
            }
            return root;
        }

        private int FindNodeByKeys(object[] originalKey)
        {
            int keyLength = (originalKey != null) ? originalKey.Length : 0;
            if ((keyLength == 0) || (this.IndexFields.Length != keyLength))
            {
                throw ExceptionBuilder.IndexKeyLength(this.IndexFields.Length, keyLength);
            }
            int root = this.records.root;
            if (root != 0)
            {
                object[] vals = new object[originalKey.Length];
                for (int i = 0; i < originalKey.Length; i++)
                {
                    vals[i] = this.IndexFields[i].Column.ConvertValue(originalKey[i]);
                }
                root = this.records.root;
                while (root != 0)
                {
                    keyLength = this.CompareRecordToKey(this.records.Key(root), vals);
                    if (keyLength == 0)
                    {
                        return root;
                    }
                    if (keyLength > 0)
                    {
                        root = this.records.Left(root);
                    }
                    else
                    {
                        root = this.records.Right(root);
                    }
                }
            }
            return root;
        }

        public int FindRecord(int record)
        {
            int node = this.records.Search(record);
            if (node != 0)
            {
                return this.records.GetIndexByNode(node);
            }
            return -1;
        }

        public int FindRecordByKey(object key)
        {
            int node = this.FindNodeByKey(key);
            if (node != 0)
            {
                return this.records.GetIndexByNode(node);
            }
            return -1;
        }

        public int FindRecordByKey(object[] key)
        {
            int node = this.FindNodeByKeys(key);
            if (node != 0)
            {
                return this.records.GetIndexByNode(node);
            }
            return -1;
        }

        public Range FindRecords(object key)
        {
            int nodeId = this.FindNodeByKey(key);
            return this.GetRangeFromNode(nodeId);
        }

        public Range FindRecords(object[] key)
        {
            int nodeId = this.FindNodeByKeys(key);
            return this.GetRangeFromNode(nodeId);
        }

        internal Range FindRecords<TKey, TRow>(ComparisonBySelector<TKey, TRow> comparison, TKey key) where TRow: DataRow
        {
            int root = this.records.root;
            while (root != 0)
            {
                int num2 = comparison(key, (TRow) this.table.recordManager[this.records.Key(root)]);
                if (num2 == 0)
                {
                    break;
                }
                if (num2 < 0)
                {
                    root = this.records.Left(root);
                }
                else
                {
                    root = this.records.Right(root);
                }
            }
            return this.GetRangeFromNode(root);
        }

        internal void FireResetEvent()
        {
            Bid.Trace("<ds.Index.FireResetEvent|API> %d#\n", this.ObjectID);
            if (this.DoListChanged)
            {
                this.OnListChanged(DataView.ResetEventArgs);
            }
        }

        private static IndexField[] GetAllFields(DataColumnCollection columns)
        {
            IndexField[] fieldArray = new IndexField[columns.Count];
            for (int i = 0; i < fieldArray.Length; i++)
            {
                fieldArray[i] = new IndexField(columns[i], false);
            }
            return fieldArray;
        }

        private int GetChangeAction(DataViewRowState oldState, DataViewRowState newState)
        {
            int num2 = ((this.recordStates & oldState) == DataViewRowState.None) ? 0 : 1;
            int num = ((this.recordStates & newState) == DataViewRowState.None) ? 0 : 1;
            return (num - num2);
        }

        public RBTree<int>.RBTreeEnumerator GetEnumerator(int startIndex)
        {
            return new RBTree<int>.RBTreeEnumerator(this.records, startIndex);
        }

        public int GetIndex(int record)
        {
            return this.records.GetIndexByKey(record);
        }

        private int GetIndex(int record, int changeRecord)
        {
            int indexByKey;
            DataRow row = this.table.recordManager[record];
            int newRecord = row.newRecord;
            int oldRecord = row.oldRecord;
            try
            {
                switch (changeRecord)
                {
                    case 1:
                        row.newRecord = record;
                        break;

                    case 2:
                        row.oldRecord = record;
                        break;
                }
                indexByKey = this.records.GetIndexByKey(record);
            }
            finally
            {
                switch (changeRecord)
                {
                    case 1:
                        row.newRecord = newRecord;
                        break;

                    case 2:
                        row.oldRecord = oldRecord;
                        break;
                }
            }
            return indexByKey;
        }

        private Range GetRangeFromNode(int nodeId)
        {
            if (nodeId == 0)
            {
                return new Range();
            }
            int indexByNode = this.records.GetIndexByNode(nodeId);
            if (this.records.Next(nodeId) == 0)
            {
                return new Range(indexByNode, indexByNode);
            }
            int num2 = this.records.SubTreeSize(this.records.Next(nodeId));
            return new Range(indexByNode, (indexByNode + num2) - 1);
        }

        public int GetRecord(int recordIndex)
        {
            return this.records[recordIndex];
        }

        private static int GetReplaceAction(DataViewRowState oldState)
        {
            if ((DataViewRowState.CurrentRows & oldState) != DataViewRowState.None)
            {
                return 1;
            }
            if ((DataViewRowState.OriginalRows & oldState) == DataViewRowState.None)
            {
                return 0;
            }
            return 2;
        }

        public DataRow GetRow(int i)
        {
            return this.table.recordManager[this.GetRecord(i)];
        }

        public DataRow[] GetRows(object[] values)
        {
            return this.GetRows(this.FindRecords(values));
        }

        public DataRow[] GetRows(Range range)
        {
            DataRow[] rowArray = this.table.NewRowArray(range.Count);
            if (0 < rowArray.Length)
            {
                RBTree<int>.RBTreeEnumerator enumerator = this.GetEnumerator(range.Min);
                for (int i = 0; (i < rowArray.Length) && enumerator.MoveNext(); i++)
                {
                    rowArray[i] = this.table.recordManager[enumerator.Current];
                }
            }
            return rowArray;
        }

        public object[] GetUniqueKeyValues()
        {
            if ((this.IndexFields == null) || (this.IndexFields.Length == 0))
            {
                return zeroObjects;
            }
            List<object[]> list = new List<object[]>();
            this.GetUniqueKeyValues(list, this.records.root);
            return list.ToArray();
        }

        private void GetUniqueKeyValues(List<object[]> list, int curNodeId)
        {
            if (curNodeId != 0)
            {
                this.GetUniqueKeyValues(list, this.records.Left(curNodeId));
                int num2 = this.records.Key(curNodeId);
                object[] item = new object[this.IndexFields.Length];
                for (int i = 0; i < item.Length; i++)
                {
                    item[i] = this.IndexFields[i].Column[num2];
                }
                list.Add(item);
                this.GetUniqueKeyValues(list, this.records.Right(curNodeId));
            }
        }

        internal static int IndexOfReference<T>(List<T> list, T item) where T: class
        {
            if (list != null)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (object.ReferenceEquals(list[i], item))
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        private void InitRecords(IFilter filter)
        {
            DataViewRowState recordStates = this.recordStates;
            bool append = 0 == this.IndexFields.Length;
            this.records = new IndexTree(this);
            this.recordCount = 0;
            foreach (DataRow row in this.table.Rows)
            {
                int record = -1;
                if (row.oldRecord == row.newRecord)
                {
                    if ((recordStates & DataViewRowState.Unchanged) != DataViewRowState.None)
                    {
                        record = row.oldRecord;
                    }
                }
                else if (row.oldRecord == -1)
                {
                    if ((recordStates & DataViewRowState.Added) != DataViewRowState.None)
                    {
                        record = row.newRecord;
                    }
                }
                else if (row.newRecord == -1)
                {
                    if ((recordStates & DataViewRowState.Deleted) != DataViewRowState.None)
                    {
                        record = row.oldRecord;
                    }
                }
                else if ((recordStates & DataViewRowState.ModifiedCurrent) != DataViewRowState.None)
                {
                    record = row.newRecord;
                }
                else if ((recordStates & DataViewRowState.ModifiedOriginal) != DataViewRowState.None)
                {
                    record = row.oldRecord;
                }
                if ((record != -1) && this.AcceptRecord(record, filter))
                {
                    this.records.InsertAt(-1, record, append);
                    this.recordCount++;
                }
            }
        }

        private int InsertRecord(int record, bool fireEvent)
        {
            Bid.Trace("<ds.Index.InsertRecord|INFO> %d#, record=%d, fireEvent=%d{bool}\n", this.ObjectID, record, fireEvent);
            bool append = false;
            if ((this.IndexFields.Length == 0) && (this.table != null))
            {
                DataRow row = this.table.recordManager[record];
                append = (this.table.Rows.IndexOf(row) + 1) == this.table.Rows.Count;
            }
            int node = this.records.InsertAt(-1, record, append);
            this.recordCount++;
            this.MaintainDataView(ListChangedType.ItemAdded, record, !fireEvent);
            if (!fireEvent)
            {
                return this.records.GetIndexByNode(node);
            }
            if (this.DoListChanged)
            {
                this.OnListChanged(ListChangedType.ItemAdded, this.records.GetIndexByNode(node));
            }
            return 0;
        }

        public int InsertRecordToIndex(int record)
        {
            int num = -1;
            if (this.AcceptRecord(record))
            {
                num = this.InsertRecord(record, false);
            }
            return num;
        }

        public bool IsKeyInIndex(object key)
        {
            int num = this.FindNodeByKey(key);
            return (0 != num);
        }

        public bool IsKeyInIndex(object[] key)
        {
            int num = this.FindNodeByKeys(key);
            return (0 != num);
        }

        public bool IsKeyRecordInIndex(int record)
        {
            int num = this.FindNodeByKeyRecord(record);
            return (0 != num);
        }

        internal void ListChangedAdd(DataViewListener listener)
        {
            this._listeners.Add(listener);
        }

        internal void ListChangedRemove(DataViewListener listener)
        {
            this._listeners.Remove(listener);
        }

        private void MaintainDataView(ListChangedType changedType, int record, bool trackAddRemove)
        {
            this._listeners.Notify<ListChangedType, DataRow, bool>(changedType, (0 <= record) ? this.table.recordManager[record] : null, trackAddRemove, (listener, type, row, track) => listener.MaintainDataView(changedType, row, track));
        }

        private void OnListChanged(ListChangedEventArgs e)
        {
            Bid.Trace("<ds.Index.OnListChanged|INFO> %d#\n", this.ObjectID);
            this._listeners.Notify<ListChangedEventArgs, bool, bool>(e, false, false, (listener, args, arg2, arg3) => listener.IndexListChanged(args));
        }

        private void OnListChanged(ListChangedType changedType, int index)
        {
            if (this.DoListChanged)
            {
                this.OnListChanged(new ListChangedEventArgs(changedType, index));
            }
        }

        private void OnListChanged(ListChangedType changedType, int newIndex, int oldIndex)
        {
            if (this.DoListChanged)
            {
                this.OnListChanged(new ListChangedEventArgs(changedType, newIndex, oldIndex));
            }
        }

        public void RecordChanged(int record)
        {
            Bid.Trace("<ds.Index.RecordChanged|API> %d#, record=%d\n", this.ObjectID, record);
            if (this.DoListChanged)
            {
                int index = this.GetIndex(record);
                if (index >= 0)
                {
                    this.OnListChanged(ListChangedType.ItemChanged, index);
                }
            }
        }

        public void RecordChanged(int oldIndex, int newIndex)
        {
            Bid.Trace("<ds.Index.RecordChanged|API> %d#, oldIndex=%d, newIndex=%d\n", this.ObjectID, oldIndex, newIndex);
            if ((oldIndex > -1) || (newIndex > -1))
            {
                if (oldIndex == newIndex)
                {
                    this.OnListChanged(ListChangedType.ItemChanged, newIndex, oldIndex);
                }
                else if (oldIndex == -1)
                {
                    this.OnListChanged(ListChangedType.ItemAdded, newIndex, oldIndex);
                }
                else if (newIndex == -1)
                {
                    this.OnListChanged(ListChangedType.ItemDeleted, oldIndex);
                }
                else
                {
                    this.OnListChanged(ListChangedType.ItemMoved, newIndex, oldIndex);
                }
            }
        }

        public void RecordStateChanged(int record, DataViewRowState oldState, DataViewRowState newState)
        {
            Bid.Trace("<ds.Index.RecordStateChanged|API> %d#, record=%d, oldState=%d{ds.DataViewRowState}, newState=%d{ds.DataViewRowState}\n", this.ObjectID, record, (int) oldState, (int) newState);
            int changeAction = this.GetChangeAction(oldState, newState);
            this.ApplyChangeAction(record, changeAction, GetReplaceAction(oldState));
        }

        public void RecordStateChanged(int oldRecord, DataViewRowState oldOldState, DataViewRowState oldNewState, int newRecord, DataViewRowState newOldState, DataViewRowState newNewState)
        {
            Bid.Trace("<ds.Index.RecordStateChanged|API> %d#, oldRecord=%d, oldOldState=%d{ds.DataViewRowState}, oldNewState=%d{ds.DataViewRowState}, newRecord=%d, newOldState=%d{ds.DataViewRowState}, newNewState=%d{ds.DataViewRowState}\n", this.ObjectID, oldRecord, (int) oldOldState, (int) oldNewState, newRecord, (int) newOldState, (int) newNewState);
            int changeAction = this.GetChangeAction(oldOldState, oldNewState);
            int action = this.GetChangeAction(newOldState, newNewState);
            if (((changeAction == -1) && (action == 1)) && this.AcceptRecord(newRecord))
            {
                int index;
                if ((this._comparison != null) && (changeAction < 0))
                {
                    index = this.GetIndex(oldRecord, GetReplaceAction(oldOldState));
                }
                else
                {
                    index = this.GetIndex(oldRecord);
                }
                if (((this._comparison == null) && (index != -1)) && (this.CompareRecords(oldRecord, newRecord) == 0))
                {
                    this.records.UpdateNodeKey(oldRecord, newRecord);
                    int newIndex = this.GetIndex(newRecord);
                    this.OnListChanged(ListChangedType.ItemChanged, newIndex, newIndex);
                }
                else
                {
                    this.suspendEvents = true;
                    if (index != -1)
                    {
                        this.records.DeleteByIndex(index);
                        this.recordCount--;
                    }
                    this.records.Insert(newRecord);
                    this.recordCount++;
                    this.suspendEvents = false;
                    int num2 = this.GetIndex(newRecord);
                    if (index == num2)
                    {
                        this.OnListChanged(ListChangedType.ItemChanged, num2, index);
                    }
                    else if (index == -1)
                    {
                        this.MaintainDataView(ListChangedType.ItemAdded, newRecord, false);
                        this.OnListChanged(ListChangedType.ItemAdded, this.GetIndex(newRecord));
                    }
                    else
                    {
                        this.OnListChanged(ListChangedType.ItemMoved, num2, index);
                    }
                }
            }
            else
            {
                this.ApplyChangeAction(oldRecord, changeAction, GetReplaceAction(oldOldState));
                this.ApplyChangeAction(newRecord, action, GetReplaceAction(newOldState));
            }
        }

        public int RemoveRef()
        {
            int num;
            Bid.Trace("<ds.Index.RemoveRef|API> %d#\n", this.ObjectID);
            LockCookie lockCookie = this.table.indexesLock.UpgradeToWriterLock(-1);
            try
            {
                num = --this.refCount;
                if (this.refCount <= 0)
                {
                    this.table.ShadowIndexCopy();
                    this.table.indexes.Remove(this);
                }
            }
            finally
            {
                this.table.indexesLock.DowngradeFromWriterLock(ref lockCookie);
            }
            return num;
        }

        public void Reset()
        {
            Bid.Trace("<ds.Index.Reset|API> %d#\n", this.ObjectID);
            this.InitRecords(this.RowFilter);
            this.MaintainDataView(ListChangedType.Reset, -1, false);
            this.FireResetEvent();
        }

        private bool DoListChanged
        {
            get
            {
                return ((!this.suspendEvents && this._listeners.HasListeners) && !this.table.AreIndexEventsSuspended);
            }
        }

        public bool HasDuplicates
        {
            get
            {
                return this.records.HasDuplicates;
            }
        }

        internal bool HasRemoteAggregate
        {
            get
            {
                return this._hasRemoteAggregate;
            }
        }

        public bool IsSharable
        {
            get
            {
                return this.isSharable;
            }
        }

        internal int ObjectID
        {
            get
            {
                return this._objectID;
            }
        }

        public int RecordCount
        {
            get
            {
                return this.recordCount;
            }
        }

        public DataViewRowState RecordStates
        {
            get
            {
                return this.recordStates;
            }
        }

        public int RefCount
        {
            get
            {
                return this.refCount;
            }
        }

        public IFilter RowFilter
        {
            get
            {
                return ((this.rowFilter != null) ? ((IFilter) this.rowFilter.Target) : null);
            }
        }

        internal DataTable Table
        {
            get
            {
                return this.table;
            }
        }

        internal delegate int ComparisonBySelector<TKey, TRow>(TKey key, TRow row) where TRow: DataRow;

        private sealed class IndexTree : RBTree<int>
        {
            private readonly Index _index;

            internal IndexTree(Index index) : base(TreeAccessMethod.KEY_SEARCH_AND_INDEX)
            {
                this._index = index;
            }

            protected override int CompareNode(int record1, int record2)
            {
                return this._index.CompareRecords(record1, record2);
            }

            protected override int CompareSateliteTreeNode(int record1, int record2)
            {
                return this._index.CompareDuplicateRecords(record1, record2);
            }
        }
    }
}

