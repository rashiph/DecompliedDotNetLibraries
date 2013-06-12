namespace System.Windows.Forms
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Reflection;

    [ListBindable(false)]
    public class DataGridViewCellCollection : BaseCollection, IList, ICollection, IEnumerable
    {
        private ArrayList items = new ArrayList();
        private DataGridViewRow owner;

        public event CollectionChangeEventHandler CollectionChanged;

        public DataGridViewCellCollection(DataGridViewRow dataGridViewRow)
        {
            this.owner = dataGridViewRow;
        }

        public virtual int Add(DataGridViewCell dataGridViewCell)
        {
            if (this.owner.DataGridView != null)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewCellCollection_OwningRowAlreadyBelongsToDataGridView"));
            }
            if (dataGridViewCell.OwningRow != null)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewCellCollection_CellAlreadyBelongsToDataGridViewRow"));
            }
            return this.AddInternal(dataGridViewCell);
        }

        internal int AddInternal(DataGridViewCell dataGridViewCell)
        {
            int num = this.items.Add(dataGridViewCell);
            dataGridViewCell.OwningRowInternal = this.owner;
            DataGridView dataGridView = this.owner.DataGridView;
            if ((dataGridView != null) && (dataGridView.Columns.Count > num))
            {
                dataGridViewCell.OwningColumnInternal = dataGridView.Columns[num];
            }
            this.OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Add, dataGridViewCell));
            return num;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual void AddRange(params DataGridViewCell[] dataGridViewCells)
        {
            if (dataGridViewCells == null)
            {
                throw new ArgumentNullException("dataGridViewCells");
            }
            if (this.owner.DataGridView != null)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewCellCollection_OwningRowAlreadyBelongsToDataGridView"));
            }
            foreach (DataGridViewCell cell in dataGridViewCells)
            {
                if (cell == null)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewCellCollection_AtLeastOneCellIsNull"));
                }
                if (cell.OwningRow != null)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewCellCollection_CellAlreadyBelongsToDataGridViewRow"));
                }
            }
            int length = dataGridViewCells.Length;
            for (int i = 0; i < (length - 1); i++)
            {
                for (int j = i + 1; j < length; j++)
                {
                    if (dataGridViewCells[i] == dataGridViewCells[j])
                    {
                        throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewCellCollection_CannotAddIdenticalCells"));
                    }
                }
            }
            this.items.AddRange(dataGridViewCells);
            foreach (DataGridViewCell cell2 in dataGridViewCells)
            {
                cell2.OwningRowInternal = this.owner;
            }
            this.OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Refresh, null));
        }

        public virtual void Clear()
        {
            if (this.owner.DataGridView != null)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewCellCollection_OwningRowAlreadyBelongsToDataGridView"));
            }
            foreach (DataGridViewCell cell in this.items)
            {
                cell.OwningRowInternal = null;
            }
            this.items.Clear();
            this.OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Refresh, null));
        }

        public virtual bool Contains(DataGridViewCell dataGridViewCell)
        {
            return (this.items.IndexOf(dataGridViewCell) != -1);
        }

        public void CopyTo(DataGridViewCell[] array, int index)
        {
            this.items.CopyTo(array, index);
        }

        public int IndexOf(DataGridViewCell dataGridViewCell)
        {
            return this.items.IndexOf(dataGridViewCell);
        }

        public virtual void Insert(int index, DataGridViewCell dataGridViewCell)
        {
            if (this.owner.DataGridView != null)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewCellCollection_OwningRowAlreadyBelongsToDataGridView"));
            }
            if (dataGridViewCell.OwningRow != null)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewCellCollection_CellAlreadyBelongsToDataGridViewRow"));
            }
            this.items.Insert(index, dataGridViewCell);
            dataGridViewCell.OwningRowInternal = this.owner;
            this.OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Add, dataGridViewCell));
        }

        internal void InsertInternal(int index, DataGridViewCell dataGridViewCell)
        {
            this.items.Insert(index, dataGridViewCell);
            dataGridViewCell.OwningRowInternal = this.owner;
            DataGridView dataGridView = this.owner.DataGridView;
            if ((dataGridView != null) && (dataGridView.Columns.Count > index))
            {
                dataGridViewCell.OwningColumnInternal = dataGridView.Columns[index];
            }
            this.OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Add, dataGridViewCell));
        }

        protected void OnCollectionChanged(CollectionChangeEventArgs e)
        {
            if (this.onCollectionChanged != null)
            {
                this.onCollectionChanged(this, e);
            }
        }

        public virtual void Remove(DataGridViewCell cell)
        {
            if (this.owner.DataGridView != null)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewCellCollection_OwningRowAlreadyBelongsToDataGridView"));
            }
            int index = -1;
            int count = this.items.Count;
            for (int i = 0; i < count; i++)
            {
                if (this.items[i] == cell)
                {
                    index = i;
                    break;
                }
            }
            if (index == -1)
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridViewCellCollection_CellNotFound"));
            }
            this.RemoveAt(index);
        }

        public virtual void RemoveAt(int index)
        {
            if (this.owner.DataGridView != null)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewCellCollection_OwningRowAlreadyBelongsToDataGridView"));
            }
            this.RemoveAtInternal(index);
        }

        internal void RemoveAtInternal(int index)
        {
            DataGridViewCell element = (DataGridViewCell) this.items[index];
            this.items.RemoveAt(index);
            element.DataGridViewInternal = null;
            element.OwningRowInternal = null;
            if (element.ReadOnly)
            {
                element.ReadOnlyInternal = false;
            }
            if (element.Selected)
            {
                element.SelectedInternal = false;
            }
            this.OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Remove, element));
        }

        void ICollection.CopyTo(Array array, int index)
        {
            this.items.CopyTo(array, index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.items.GetEnumerator();
        }

        int IList.Add(object value)
        {
            return this.Add((DataGridViewCell) value);
        }

        void IList.Clear()
        {
            this.Clear();
        }

        bool IList.Contains(object value)
        {
            return this.items.Contains(value);
        }

        int IList.IndexOf(object value)
        {
            return this.items.IndexOf(value);
        }

        void IList.Insert(int index, object value)
        {
            this.Insert(index, (DataGridViewCell) value);
        }

        void IList.Remove(object value)
        {
            this.Remove((DataGridViewCell) value);
        }

        void IList.RemoveAt(int index)
        {
            this.RemoveAt(index);
        }

        public DataGridViewCell this[int index]
        {
            get
            {
                return (DataGridViewCell) this.items[index];
            }
            set
            {
                DataGridViewCell cell = value;
                if (cell == null)
                {
                    throw new ArgumentNullException("value");
                }
                if (cell.DataGridView != null)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewCellCollection_CellAlreadyBelongsToDataGridView"));
                }
                if (cell.OwningRow != null)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewCellCollection_CellAlreadyBelongsToDataGridViewRow"));
                }
                if (this.owner.DataGridView != null)
                {
                    this.owner.DataGridView.OnReplacingCell(this.owner, index);
                }
                DataGridViewCell cell2 = (DataGridViewCell) this.items[index];
                this.items[index] = cell;
                cell.OwningRowInternal = this.owner;
                cell.StateInternal = cell2.State;
                if (this.owner.DataGridView != null)
                {
                    cell.DataGridViewInternal = this.owner.DataGridView;
                    cell.OwningColumnInternal = this.owner.DataGridView.Columns[index];
                    this.owner.DataGridView.OnReplacedCell(this.owner, index);
                }
                cell2.DataGridViewInternal = null;
                cell2.OwningRowInternal = null;
                cell2.OwningColumnInternal = null;
                if (cell2.ReadOnly)
                {
                    cell2.ReadOnlyInternal = false;
                }
                if (cell2.Selected)
                {
                    cell2.SelectedInternal = false;
                }
            }
        }

        public DataGridViewCell this[string columnName]
        {
            get
            {
                DataGridViewColumn column = null;
                if (this.owner.DataGridView != null)
                {
                    column = this.owner.DataGridView.Columns[columnName];
                }
                if (column == null)
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridViewColumnCollection_ColumnNotFound", new object[] { columnName }), "columnName");
                }
                return (DataGridViewCell) this.items[column.Index];
            }
            set
            {
                DataGridViewColumn column = null;
                if (this.owner.DataGridView != null)
                {
                    column = this.owner.DataGridView.Columns[columnName];
                }
                if (column == null)
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridViewColumnCollection_ColumnNotFound", new object[] { columnName }), "columnName");
                }
                this[column.Index] = value;
            }
        }

        protected override ArrayList List
        {
            get
            {
                return this.items;
            }
        }

        int ICollection.Count
        {
            get
            {
                return this.items.Count;
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

        object IList.this[int index]
        {
            get
            {
                return this[index];
            }
            set
            {
                this[index] = (DataGridViewCell) value;
            }
        }
    }
}

