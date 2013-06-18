namespace System.Data
{
    using System;
    using System.Collections;
    using System.Reflection;

    public sealed class DataRowCollection : InternalDataCollectionBase
    {
        private readonly DataRowTree list = new DataRowTree();
        internal int nullInList;
        private readonly DataTable table;

        internal DataRowCollection(DataTable table)
        {
            this.table = table;
        }

        public DataRow Add(params object[] values)
        {
            int record = this.table.NewRecordFromArray(values);
            DataRow row = this.table.NewRow(record);
            this.table.AddRow(row, -1);
            return row;
        }

        public void Add(DataRow row)
        {
            this.table.AddRow(row, -1);
        }

        internal DataRow AddWithColumnEvents(params object[] values)
        {
            DataRow row = this.table.NewRow(-1);
            row.ItemArray = values;
            this.table.AddRow(row, -1);
            return row;
        }

        internal void ArrayAdd(DataRow row)
        {
            row.RBTreeNodeId = this.list.Add(row);
        }

        internal void ArrayClear()
        {
            this.list.Clear();
        }

        internal void ArrayInsert(DataRow row, int pos)
        {
            row.RBTreeNodeId = this.list.Insert(pos, row);
        }

        internal void ArrayRemove(DataRow row)
        {
            if (row.RBTreeNodeId == 0)
            {
                throw ExceptionBuilder.InternalRBTreeError(RBTreeError.AttachedNodeWithZerorbTreeNodeId);
            }
            this.list.RBDelete(row.RBTreeNodeId);
            row.RBTreeNodeId = 0;
        }

        public void Clear()
        {
            this.table.Clear(false);
        }

        public bool Contains(object key)
        {
            return (this.table.FindByPrimaryKey(key) != null);
        }

        public bool Contains(object[] keys)
        {
            return (this.table.FindByPrimaryKey(keys) != null);
        }

        public override void CopyTo(Array ar, int index)
        {
            this.list.CopyTo(ar, index);
        }

        public void CopyTo(DataRow[] array, int index)
        {
            this.list.CopyTo(array, index);
        }

        internal void DiffInsertAt(DataRow row, int pos)
        {
            if ((pos < 0) || (pos == this.list.Count))
            {
                this.table.AddRow(row, (pos > -1) ? (pos + 1) : -1);
            }
            else if (this.table.NestedParentRelations.Length <= 0)
            {
                this.table.InsertRow(row, pos + 1, (pos > this.list.Count) ? -1 : pos);
            }
            else if (pos >= this.list.Count)
            {
                while (pos > this.list.Count)
                {
                    this.list.Add(null);
                    this.nullInList++;
                }
                this.table.AddRow(row, pos + 1);
            }
            else
            {
                if (this.list[pos] != null)
                {
                    throw ExceptionBuilder.RowInsertTwice(pos, this.table.TableName);
                }
                this.list.RemoveAt(pos);
                this.nullInList--;
                this.table.InsertRow(row, pos + 1, pos);
            }
        }

        public DataRow Find(object key)
        {
            return this.table.FindByPrimaryKey(key);
        }

        public DataRow Find(object[] keys)
        {
            return this.table.FindByPrimaryKey(keys);
        }

        public override IEnumerator GetEnumerator()
        {
            return this.list.GetEnumerator();
        }

        public int IndexOf(DataRow row)
        {
            if (((row != null) && (row.Table == this.table)) && ((row.RBTreeNodeId != 0) || (row.RowState != DataRowState.Detached)))
            {
                return this.list.IndexOf(row.RBTreeNodeId, row);
            }
            return -1;
        }

        public void InsertAt(DataRow row, int pos)
        {
            if (pos < 0)
            {
                throw ExceptionBuilder.RowInsertOutOfRange(pos);
            }
            if (pos >= this.list.Count)
            {
                this.table.AddRow(row, -1);
            }
            else
            {
                this.table.InsertRow(row, -1, pos);
            }
        }

        public void Remove(DataRow row)
        {
            if (((row == null) || (row.Table != this.table)) || (-1L == row.rowID))
            {
                throw ExceptionBuilder.RowOutOfRange();
            }
            if ((row.RowState != DataRowState.Deleted) && (row.RowState != DataRowState.Detached))
            {
                row.Delete();
            }
            if (row.RowState != DataRowState.Detached)
            {
                row.AcceptChanges();
            }
        }

        public void RemoveAt(int index)
        {
            this.Remove(this[index]);
        }

        public override int Count
        {
            get
            {
                return this.list.Count;
            }
        }

        public DataRow this[int index]
        {
            get
            {
                return this.list[index];
            }
        }

        private sealed class DataRowTree : RBTree<DataRow>
        {
            internal DataRowTree() : base(TreeAccessMethod.INDEX_ONLY)
            {
            }

            protected override int CompareNode(DataRow record1, DataRow record2)
            {
                throw ExceptionBuilder.InternalRBTreeError(RBTreeError.CompareNodeInDataRowTree);
            }

            protected override int CompareSateliteTreeNode(DataRow record1, DataRow record2)
            {
                throw ExceptionBuilder.InternalRBTreeError(RBTreeError.CompareSateliteTreeNodeInDataRowTree);
            }
        }
    }
}

