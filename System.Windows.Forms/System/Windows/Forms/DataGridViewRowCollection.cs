namespace System.Windows.Forms
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Drawing;
    using System.Globalization;
    using System.Reflection;

    [ListBindable(false), DesignerSerializer("System.Windows.Forms.Design.DataGridViewRowCollectionCodeDomSerializer, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.ComponentModel.Design.Serialization.CodeDomSerializer, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    public class DataGridViewRowCollection : IList, ICollection, IEnumerable
    {
        private System.Windows.Forms.DataGridView dataGridView;
        private RowArrayList items;
        private int rowCountsVisible;
        private int rowCountsVisibleFrozen;
        private int rowCountsVisibleSelected;
        private int rowsHeightVisible;
        private int rowsHeightVisibleFrozen;
        private List<DataGridViewElementStates> rowStates;

        public event CollectionChangeEventHandler CollectionChanged;

        public DataGridViewRowCollection(System.Windows.Forms.DataGridView dataGridView)
        {
            this.InvalidateCachedRowCounts();
            this.InvalidateCachedRowsHeights();
            this.dataGridView = dataGridView;
            this.rowStates = new List<DataGridViewElementStates>();
            this.items = new RowArrayList(this);
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual int Add()
        {
            if (this.DataGridView.DataSource != null)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewRowCollection_AddUnboundRow"));
            }
            if (this.DataGridView.NoDimensionChangeAllowed)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridView_ForbiddenOperationInEventHandler"));
            }
            return this.AddInternal(false, null);
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual int Add(params object[] values)
        {
            if (values == null)
            {
                throw new ArgumentNullException("values");
            }
            if (this.DataGridView.VirtualMode)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridView_InvalidOperationInVirtualMode"));
            }
            if (this.DataGridView.DataSource != null)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewRowCollection_AddUnboundRow"));
            }
            if (this.DataGridView.NoDimensionChangeAllowed)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridView_ForbiddenOperationInEventHandler"));
            }
            return this.AddInternal(false, values);
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual int Add(int count)
        {
            if (count <= 0)
            {
                throw new ArgumentOutOfRangeException("count", System.Windows.Forms.SR.GetString("DataGridViewRowCollection_CountOutOfRange"));
            }
            if (this.DataGridView.Columns.Count == 0)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewRowCollection_NoColumns"));
            }
            if (this.DataGridView.DataSource != null)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewRowCollection_AddUnboundRow"));
            }
            if (this.DataGridView.NoDimensionChangeAllowed)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridView_ForbiddenOperationInEventHandler"));
            }
            if (this.DataGridView.RowTemplate.Cells.Count > this.DataGridView.Columns.Count)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewRowCollection_RowTemplateTooManyCells"));
            }
            DataGridViewRow rowTemplateClone = this.DataGridView.RowTemplateClone;
            DataGridViewElementStates state = rowTemplateClone.State;
            rowTemplateClone.DataGridViewInternal = this.dataGridView;
            int num = 0;
            foreach (DataGridViewCell cell in rowTemplateClone.Cells)
            {
                cell.DataGridViewInternal = this.dataGridView;
                cell.OwningColumnInternal = this.DataGridView.Columns[num];
                num++;
            }
            if (rowTemplateClone.HasHeaderCell)
            {
                rowTemplateClone.HeaderCell.DataGridViewInternal = this.dataGridView;
                rowTemplateClone.HeaderCell.OwningRowInternal = rowTemplateClone;
            }
            if (this.DataGridView.NewRowIndex != -1)
            {
                int indexDestination = this.Count - 1;
                this.InsertCopiesPrivate(rowTemplateClone, state, indexDestination, count);
                return ((indexDestination + count) - 1);
            }
            return this.AddCopiesPrivate(rowTemplateClone, state, count);
        }

        public virtual int Add(DataGridViewRow dataGridViewRow)
        {
            if (this.DataGridView.Columns.Count == 0)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewRowCollection_NoColumns"));
            }
            if (this.DataGridView.DataSource != null)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewRowCollection_AddUnboundRow"));
            }
            if (this.DataGridView.NoDimensionChangeAllowed)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridView_ForbiddenOperationInEventHandler"));
            }
            return this.AddInternal(dataGridViewRow);
        }

        public virtual int AddCopies(int indexSource, int count)
        {
            if (this.DataGridView.DataSource != null)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewRowCollection_AddUnboundRow"));
            }
            if (this.DataGridView.NoDimensionChangeAllowed)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridView_ForbiddenOperationInEventHandler"));
            }
            return this.AddCopiesInternal(indexSource, count);
        }

        internal int AddCopiesInternal(int indexSource, int count)
        {
            if (this.DataGridView.NewRowIndex != -1)
            {
                int indexDestination = this.Count - 1;
                this.InsertCopiesPrivate(indexSource, indexDestination, count);
                return ((indexDestination + count) - 1);
            }
            return this.AddCopiesInternal(indexSource, count, DataGridViewElementStates.None, DataGridViewElementStates.Selected | DataGridViewElementStates.Displayed);
        }

        internal int AddCopiesInternal(int indexSource, int count, DataGridViewElementStates dgvesAdd, DataGridViewElementStates dgvesRemove)
        {
            if ((indexSource < 0) || (this.Count <= indexSource))
            {
                throw new ArgumentOutOfRangeException("indexSource", System.Windows.Forms.SR.GetString("DataGridViewRowCollection_IndexSourceOutOfRange"));
            }
            if (count <= 0)
            {
                throw new ArgumentOutOfRangeException("count", System.Windows.Forms.SR.GetString("DataGridViewRowCollection_CountOutOfRange"));
            }
            DataGridViewElementStates rowTemplateState = ((DataGridViewElementStates) this.rowStates[indexSource]) & ~dgvesRemove;
            rowTemplateState |= dgvesAdd;
            return this.AddCopiesPrivate(this.SharedRow(indexSource), rowTemplateState, count);
        }

        private int AddCopiesPrivate(DataGridViewRow rowTemplate, DataGridViewElementStates rowTemplateState, int count)
        {
            int num;
            int rowIndex = this.items.Count;
            if (rowTemplate.Index == -1)
            {
                this.DataGridView.OnAddingRow(rowTemplate, rowTemplateState, true);
                for (int i = 0; i < (count - 1); i++)
                {
                    this.SharedList.Add(rowTemplate);
                    this.rowStates.Add(rowTemplateState);
                }
                num = this.SharedList.Add(rowTemplate);
                this.rowStates.Add(rowTemplateState);
                this.DataGridView.OnAddedRow_PreNotification(num);
                this.OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Refresh, null), rowIndex, count);
                for (int j = 0; j < count; j++)
                {
                    this.DataGridView.OnAddedRow_PostNotification((num - (count - 1)) + j);
                }
                return num;
            }
            num = this.AddDuplicateRow(rowTemplate, false);
            if (count > 1)
            {
                this.DataGridView.OnAddedRow_PreNotification(num);
                if (this.RowIsSharable(num))
                {
                    DataGridViewRow dataGridViewRow = this.SharedRow(num);
                    this.DataGridView.OnAddingRow(dataGridViewRow, rowTemplateState, true);
                    for (int m = 1; m < (count - 1); m++)
                    {
                        this.SharedList.Add(dataGridViewRow);
                        this.rowStates.Add(rowTemplateState);
                    }
                    num = this.SharedList.Add(dataGridViewRow);
                    this.rowStates.Add(rowTemplateState);
                    this.DataGridView.OnAddedRow_PreNotification(num);
                }
                else
                {
                    this.UnshareRow(num);
                    for (int n = 1; n < count; n++)
                    {
                        num = this.AddDuplicateRow(rowTemplate, false);
                        this.UnshareRow(num);
                        this.DataGridView.OnAddedRow_PreNotification(num);
                    }
                }
                this.OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Refresh, null), rowIndex, count);
                for (int k = 0; k < count; k++)
                {
                    this.DataGridView.OnAddedRow_PostNotification((num - (count - 1)) + k);
                }
                return num;
            }
            if (this.IsCollectionChangedListenedTo)
            {
                this.UnshareRow(num);
            }
            this.OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Add, this.SharedRow(num)), num, 1);
            return num;
        }

        public virtual int AddCopy(int indexSource)
        {
            if (this.DataGridView.DataSource != null)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewRowCollection_AddUnboundRow"));
            }
            if (this.DataGridView.NoDimensionChangeAllowed)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridView_ForbiddenOperationInEventHandler"));
            }
            return this.AddCopyInternal(indexSource, DataGridViewElementStates.None, DataGridViewElementStates.Selected | DataGridViewElementStates.Displayed, false);
        }

        internal int AddCopyInternal(int indexSource, DataGridViewElementStates dgvesAdd, DataGridViewElementStates dgvesRemove, bool newRow)
        {
            int num2;
            if (this.DataGridView.NewRowIndex != -1)
            {
                int indexDestination = this.Count - 1;
                this.InsertCopy(indexSource, indexDestination);
                return indexDestination;
            }
            if ((indexSource < 0) || (indexSource >= this.Count))
            {
                throw new ArgumentOutOfRangeException("indexSource", System.Windows.Forms.SR.GetString("DataGridViewRowCollection_IndexSourceOutOfRange"));
            }
            DataGridViewRow dataGridViewRow = this.SharedRow(indexSource);
            if (((dataGridViewRow.Index == -1) && !this.IsCollectionChangedListenedTo) && !newRow)
            {
                DataGridViewElementStates rowState = ((DataGridViewElementStates) this.rowStates[indexSource]) & ~dgvesRemove;
                rowState |= dgvesAdd;
                this.DataGridView.OnAddingRow(dataGridViewRow, rowState, true);
                num2 = this.SharedList.Add(dataGridViewRow);
                this.rowStates.Add(rowState);
                this.OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Add, dataGridViewRow), num2, 1);
                return num2;
            }
            num2 = this.AddDuplicateRow(dataGridViewRow, newRow);
            if ((!this.RowIsSharable(num2) || RowHasValueOrToolTipText(this.SharedRow(num2))) || this.IsCollectionChangedListenedTo)
            {
                this.UnshareRow(num2);
            }
            this.OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Add, this.SharedRow(num2)), num2, 1);
            return num2;
        }

        private int AddDuplicateRow(DataGridViewRow rowTemplate, bool newRow)
        {
            DataGridViewRow dataGridViewRow = (DataGridViewRow) rowTemplate.Clone();
            dataGridViewRow.StateInternal = DataGridViewElementStates.None;
            dataGridViewRow.DataGridViewInternal = this.dataGridView;
            DataGridViewCellCollection cells = dataGridViewRow.Cells;
            int num = 0;
            foreach (DataGridViewCell cell in cells)
            {
                if (newRow)
                {
                    cell.Value = cell.DefaultNewRowValue;
                }
                cell.DataGridViewInternal = this.dataGridView;
                cell.OwningColumnInternal = this.DataGridView.Columns[num];
                num++;
            }
            DataGridViewElementStates rowState = rowTemplate.State & ~(DataGridViewElementStates.Selected | DataGridViewElementStates.Displayed);
            if (dataGridViewRow.HasHeaderCell)
            {
                dataGridViewRow.HeaderCell.DataGridViewInternal = this.dataGridView;
                dataGridViewRow.HeaderCell.OwningRowInternal = dataGridViewRow;
            }
            this.DataGridView.OnAddingRow(dataGridViewRow, rowState, true);
            this.rowStates.Add(rowState);
            return this.SharedList.Add(dataGridViewRow);
        }

        internal int AddInternal(DataGridViewRow dataGridViewRow)
        {
            if (dataGridViewRow == null)
            {
                throw new ArgumentNullException("dataGridViewRow");
            }
            if (dataGridViewRow.DataGridView != null)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridView_RowAlreadyBelongsToDataGridView"));
            }
            if (this.DataGridView.Columns.Count == 0)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewRowCollection_NoColumns"));
            }
            if (dataGridViewRow.Cells.Count > this.DataGridView.Columns.Count)
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridViewRowCollection_TooManyCells"), "dataGridViewRow");
            }
            if (dataGridViewRow.Selected)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewRowCollection_CannotAddOrInsertSelectedRow"));
            }
            if (this.DataGridView.NewRowIndex != -1)
            {
                int rowIndex = this.Count - 1;
                this.InsertInternal(rowIndex, dataGridViewRow);
                return rowIndex;
            }
            this.DataGridView.CompleteCellsCollection(dataGridViewRow);
            this.DataGridView.OnAddingRow(dataGridViewRow, dataGridViewRow.State, true);
            int num2 = 0;
            foreach (DataGridViewCell cell in dataGridViewRow.Cells)
            {
                cell.DataGridViewInternal = this.dataGridView;
                if (cell.ColumnIndex == -1)
                {
                    cell.OwningColumnInternal = this.DataGridView.Columns[num2];
                }
                num2++;
            }
            if (dataGridViewRow.HasHeaderCell)
            {
                dataGridViewRow.HeaderCell.DataGridViewInternal = this.DataGridView;
                dataGridViewRow.HeaderCell.OwningRowInternal = dataGridViewRow;
            }
            int index = this.SharedList.Add(dataGridViewRow);
            this.rowStates.Add(dataGridViewRow.State);
            dataGridViewRow.DataGridViewInternal = this.dataGridView;
            if ((!this.RowIsSharable(index) || RowHasValueOrToolTipText(dataGridViewRow)) || this.IsCollectionChangedListenedTo)
            {
                dataGridViewRow.IndexInternal = index;
            }
            this.OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Add, dataGridViewRow), index, 1);
            return index;
        }

        internal int AddInternal(bool newRow, object[] values)
        {
            if (this.DataGridView.Columns.Count == 0)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewRowCollection_NoColumns"));
            }
            if (this.DataGridView.RowTemplate.Cells.Count > this.DataGridView.Columns.Count)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewRowCollection_RowTemplateTooManyCells"));
            }
            DataGridViewRow rowTemplateClone = this.DataGridView.RowTemplateClone;
            if (newRow)
            {
                rowTemplateClone.StateInternal = rowTemplateClone.State | DataGridViewElementStates.Visible;
                foreach (DataGridViewCell cell in rowTemplateClone.Cells)
                {
                    cell.Value = cell.DefaultNewRowValue;
                }
            }
            if (values != null)
            {
                rowTemplateClone.SetValuesInternal(values);
            }
            if (this.DataGridView.NewRowIndex != -1)
            {
                int rowIndex = this.Count - 1;
                this.Insert(rowIndex, rowTemplateClone);
                return rowIndex;
            }
            DataGridViewElementStates state = rowTemplateClone.State;
            this.DataGridView.OnAddingRow(rowTemplateClone, state, true);
            rowTemplateClone.DataGridViewInternal = this.dataGridView;
            int num2 = 0;
            foreach (DataGridViewCell cell2 in rowTemplateClone.Cells)
            {
                cell2.DataGridViewInternal = this.dataGridView;
                cell2.OwningColumnInternal = this.DataGridView.Columns[num2];
                num2++;
            }
            if (rowTemplateClone.HasHeaderCell)
            {
                rowTemplateClone.HeaderCell.DataGridViewInternal = this.DataGridView;
                rowTemplateClone.HeaderCell.OwningRowInternal = rowTemplateClone;
            }
            int index = this.SharedList.Add(rowTemplateClone);
            this.rowStates.Add(state);
            if (((values != null) || !this.RowIsSharable(index)) || (RowHasValueOrToolTipText(rowTemplateClone) || this.IsCollectionChangedListenedTo))
            {
                rowTemplateClone.IndexInternal = index;
            }
            this.OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Add, rowTemplateClone), index, 1);
            return index;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual void AddRange(params DataGridViewRow[] dataGridViewRows)
        {
            if (dataGridViewRows == null)
            {
                throw new ArgumentNullException("dataGridViewRows");
            }
            if (this.DataGridView.NoDimensionChangeAllowed)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridView_ForbiddenOperationInEventHandler"));
            }
            if (this.DataGridView.DataSource != null)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewRowCollection_AddUnboundRow"));
            }
            if (this.DataGridView.NewRowIndex != -1)
            {
                this.InsertRange(this.Count - 1, dataGridViewRows);
            }
            else
            {
                if (this.DataGridView.Columns.Count == 0)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewRowCollection_NoColumns"));
                }
                int count = this.items.Count;
                this.DataGridView.OnAddingRows(dataGridViewRows, true);
                foreach (DataGridViewRow row in dataGridViewRows)
                {
                    int num2 = 0;
                    foreach (DataGridViewCell cell in row.Cells)
                    {
                        cell.DataGridViewInternal = this.dataGridView;
                        cell.OwningColumnInternal = this.DataGridView.Columns[num2];
                        num2++;
                    }
                    if (row.HasHeaderCell)
                    {
                        row.HeaderCell.DataGridViewInternal = this.dataGridView;
                        row.HeaderCell.OwningRowInternal = row;
                    }
                    int num3 = this.SharedList.Add(row);
                    this.rowStates.Add(row.State);
                    row.IndexInternal = num3;
                    row.DataGridViewInternal = this.dataGridView;
                }
                this.DataGridView.OnAddedRows_PreNotification(dataGridViewRows);
                this.OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Refresh, null), count, dataGridViewRows.Length);
                this.DataGridView.OnAddedRows_PostNotification(dataGridViewRows);
            }
        }

        public virtual void Clear()
        {
            if (this.DataGridView.NoDimensionChangeAllowed)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridView_ForbiddenOperationInEventHandler"));
            }
            if (this.DataGridView.DataSource != null)
            {
                IBindingList list = this.DataGridView.DataConnection.List as IBindingList;
                if (((list == null) || !list.AllowRemove) || !list.SupportsChangeNotification)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewRowCollection_CantClearRowCollectionWithWrongSource"));
                }
                list.Clear();
            }
            else
            {
                this.ClearInternal(true);
            }
        }

        internal void ClearInternal(bool recreateNewRow)
        {
            int count = this.items.Count;
            if (count > 0)
            {
                this.DataGridView.OnClearingRows();
                for (int i = 0; i < count; i++)
                {
                    this.SharedRow(i).DetachFromDataGridView();
                }
                this.SharedList.Clear();
                this.rowStates.Clear();
                this.OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Refresh, null), 0, count, true, false, recreateNewRow, new Point(-1, -1));
            }
            else if ((recreateNewRow && (this.DataGridView.Columns.Count != 0)) && (this.DataGridView.AllowUserToAddRowsInternal && (this.items.Count == 0)))
            {
                this.DataGridView.AddNewRow(false);
            }
        }

        public virtual bool Contains(DataGridViewRow dataGridViewRow)
        {
            return (this.items.IndexOf(dataGridViewRow) != -1);
        }

        public void CopyTo(DataGridViewRow[] array, int index)
        {
            this.items.CopyTo(array, index);
        }

        internal int DisplayIndexToRowIndex(int visibleRowIndex)
        {
            int num = -1;
            for (int i = 0; i < this.Count; i++)
            {
                if ((this.GetRowState(i) & DataGridViewElementStates.Visible) == DataGridViewElementStates.Visible)
                {
                    num++;
                }
                if (num == visibleRowIndex)
                {
                    return i;
                }
            }
            return -1;
        }

        public int GetFirstRow(DataGridViewElementStates includeFilter)
        {
            if ((includeFilter & ~(DataGridViewElementStates.Visible | DataGridViewElementStates.Selected | DataGridViewElementStates.Resizable | DataGridViewElementStates.ReadOnly | DataGridViewElementStates.Frozen | DataGridViewElementStates.Displayed)) != DataGridViewElementStates.None)
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridView_InvalidDataGridViewElementStateCombination", new object[] { "includeFilter" }));
            }
            switch (includeFilter)
            {
                case DataGridViewElementStates.Visible:
                    if (this.rowCountsVisible != 0)
                    {
                        break;
                    }
                    return -1;

                case (DataGridViewElementStates.Visible | DataGridViewElementStates.Frozen):
                    if (this.rowCountsVisibleFrozen != 0)
                    {
                        break;
                    }
                    return -1;

                case (DataGridViewElementStates.Visible | DataGridViewElementStates.Selected):
                    if (this.rowCountsVisibleSelected == 0)
                    {
                        return -1;
                    }
                    break;
            }
            int rowIndex = 0;
            while ((rowIndex < this.items.Count) && ((this.GetRowState(rowIndex) & includeFilter) != includeFilter))
            {
                rowIndex++;
            }
            if (rowIndex >= this.items.Count)
            {
                return -1;
            }
            return rowIndex;
        }

        public int GetFirstRow(DataGridViewElementStates includeFilter, DataGridViewElementStates excludeFilter)
        {
            if (excludeFilter == DataGridViewElementStates.None)
            {
                return this.GetFirstRow(includeFilter);
            }
            if ((includeFilter & ~(DataGridViewElementStates.Visible | DataGridViewElementStates.Selected | DataGridViewElementStates.Resizable | DataGridViewElementStates.ReadOnly | DataGridViewElementStates.Frozen | DataGridViewElementStates.Displayed)) != DataGridViewElementStates.None)
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridView_InvalidDataGridViewElementStateCombination", new object[] { "includeFilter" }));
            }
            if ((excludeFilter & ~(DataGridViewElementStates.Visible | DataGridViewElementStates.Selected | DataGridViewElementStates.Resizable | DataGridViewElementStates.ReadOnly | DataGridViewElementStates.Frozen | DataGridViewElementStates.Displayed)) != DataGridViewElementStates.None)
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridView_InvalidDataGridViewElementStateCombination", new object[] { "excludeFilter" }));
            }
            switch (includeFilter)
            {
                case DataGridViewElementStates.Visible:
                    if (this.rowCountsVisible != 0)
                    {
                        break;
                    }
                    return -1;

                case (DataGridViewElementStates.Visible | DataGridViewElementStates.Frozen):
                    if (this.rowCountsVisibleFrozen != 0)
                    {
                        break;
                    }
                    return -1;

                case (DataGridViewElementStates.Visible | DataGridViewElementStates.Selected):
                    if (this.rowCountsVisibleSelected == 0)
                    {
                        return -1;
                    }
                    break;
            }
            int rowIndex = 0;
            while ((rowIndex < this.items.Count) && (((this.GetRowState(rowIndex) & includeFilter) != includeFilter) || ((this.GetRowState(rowIndex) & excludeFilter) != DataGridViewElementStates.None)))
            {
                rowIndex++;
            }
            if (rowIndex >= this.items.Count)
            {
                return -1;
            }
            return rowIndex;
        }

        public int GetLastRow(DataGridViewElementStates includeFilter)
        {
            if ((includeFilter & ~(DataGridViewElementStates.Visible | DataGridViewElementStates.Selected | DataGridViewElementStates.Resizable | DataGridViewElementStates.ReadOnly | DataGridViewElementStates.Frozen | DataGridViewElementStates.Displayed)) != DataGridViewElementStates.None)
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridView_InvalidDataGridViewElementStateCombination", new object[] { "includeFilter" }));
            }
            switch (includeFilter)
            {
                case DataGridViewElementStates.Visible:
                    if (this.rowCountsVisible != 0)
                    {
                        break;
                    }
                    return -1;

                case (DataGridViewElementStates.Visible | DataGridViewElementStates.Frozen):
                    if (this.rowCountsVisibleFrozen != 0)
                    {
                        break;
                    }
                    return -1;

                case (DataGridViewElementStates.Visible | DataGridViewElementStates.Selected):
                    if (this.rowCountsVisibleSelected == 0)
                    {
                        return -1;
                    }
                    break;
            }
            int rowIndex = this.items.Count - 1;
            while ((rowIndex >= 0) && ((this.GetRowState(rowIndex) & includeFilter) != includeFilter))
            {
                rowIndex--;
            }
            if (rowIndex < 0)
            {
                return -1;
            }
            return rowIndex;
        }

        public int GetNextRow(int indexStart, DataGridViewElementStates includeFilter)
        {
            if ((includeFilter & ~(DataGridViewElementStates.Visible | DataGridViewElementStates.Selected | DataGridViewElementStates.Resizable | DataGridViewElementStates.ReadOnly | DataGridViewElementStates.Frozen | DataGridViewElementStates.Displayed)) != DataGridViewElementStates.None)
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridView_InvalidDataGridViewElementStateCombination", new object[] { "includeFilter" }));
            }
            if (indexStart < -1)
            {
                object[] args = new object[] { "indexStart", indexStart.ToString(CultureInfo.CurrentCulture), -1.ToString(CultureInfo.CurrentCulture) };
                throw new ArgumentOutOfRangeException("indexStart", System.Windows.Forms.SR.GetString("InvalidLowBoundArgumentEx", args));
            }
            int rowIndex = indexStart + 1;
            while ((rowIndex < this.items.Count) && ((this.GetRowState(rowIndex) & includeFilter) != includeFilter))
            {
                rowIndex++;
            }
            if (rowIndex >= this.items.Count)
            {
                return -1;
            }
            return rowIndex;
        }

        internal int GetNextRow(int indexStart, DataGridViewElementStates includeFilter, int skipRows)
        {
            int nextRow = indexStart;
            do
            {
                nextRow = this.GetNextRow(nextRow, includeFilter);
                skipRows--;
            }
            while ((skipRows >= 0) && (nextRow != -1));
            return nextRow;
        }

        public int GetNextRow(int indexStart, DataGridViewElementStates includeFilter, DataGridViewElementStates excludeFilter)
        {
            if (excludeFilter == DataGridViewElementStates.None)
            {
                return this.GetNextRow(indexStart, includeFilter);
            }
            if ((includeFilter & ~(DataGridViewElementStates.Visible | DataGridViewElementStates.Selected | DataGridViewElementStates.Resizable | DataGridViewElementStates.ReadOnly | DataGridViewElementStates.Frozen | DataGridViewElementStates.Displayed)) != DataGridViewElementStates.None)
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridView_InvalidDataGridViewElementStateCombination", new object[] { "includeFilter" }));
            }
            if ((excludeFilter & ~(DataGridViewElementStates.Visible | DataGridViewElementStates.Selected | DataGridViewElementStates.Resizable | DataGridViewElementStates.ReadOnly | DataGridViewElementStates.Frozen | DataGridViewElementStates.Displayed)) != DataGridViewElementStates.None)
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridView_InvalidDataGridViewElementStateCombination", new object[] { "excludeFilter" }));
            }
            if (indexStart < -1)
            {
                object[] args = new object[] { "indexStart", indexStart.ToString(CultureInfo.CurrentCulture), -1.ToString(CultureInfo.CurrentCulture) };
                throw new ArgumentOutOfRangeException("indexStart", System.Windows.Forms.SR.GetString("InvalidLowBoundArgumentEx", args));
            }
            int rowIndex = indexStart + 1;
            while ((rowIndex < this.items.Count) && (((this.GetRowState(rowIndex) & includeFilter) != includeFilter) || ((this.GetRowState(rowIndex) & excludeFilter) != DataGridViewElementStates.None)))
            {
                rowIndex++;
            }
            if (rowIndex >= this.items.Count)
            {
                return -1;
            }
            return rowIndex;
        }

        public int GetPreviousRow(int indexStart, DataGridViewElementStates includeFilter)
        {
            if ((includeFilter & ~(DataGridViewElementStates.Visible | DataGridViewElementStates.Selected | DataGridViewElementStates.Resizable | DataGridViewElementStates.ReadOnly | DataGridViewElementStates.Frozen | DataGridViewElementStates.Displayed)) != DataGridViewElementStates.None)
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridView_InvalidDataGridViewElementStateCombination", new object[] { "includeFilter" }));
            }
            if (indexStart > this.items.Count)
            {
                throw new ArgumentOutOfRangeException("indexStart", System.Windows.Forms.SR.GetString("InvalidHighBoundArgumentEx", new object[] { "indexStart", indexStart.ToString(CultureInfo.CurrentCulture), this.items.Count.ToString(CultureInfo.CurrentCulture) }));
            }
            int rowIndex = indexStart - 1;
            while ((rowIndex >= 0) && ((this.GetRowState(rowIndex) & includeFilter) != includeFilter))
            {
                rowIndex--;
            }
            if (rowIndex < 0)
            {
                return -1;
            }
            return rowIndex;
        }

        public int GetPreviousRow(int indexStart, DataGridViewElementStates includeFilter, DataGridViewElementStates excludeFilter)
        {
            if (excludeFilter == DataGridViewElementStates.None)
            {
                return this.GetPreviousRow(indexStart, includeFilter);
            }
            if ((includeFilter & ~(DataGridViewElementStates.Visible | DataGridViewElementStates.Selected | DataGridViewElementStates.Resizable | DataGridViewElementStates.ReadOnly | DataGridViewElementStates.Frozen | DataGridViewElementStates.Displayed)) != DataGridViewElementStates.None)
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridView_InvalidDataGridViewElementStateCombination", new object[] { "includeFilter" }));
            }
            if ((excludeFilter & ~(DataGridViewElementStates.Visible | DataGridViewElementStates.Selected | DataGridViewElementStates.Resizable | DataGridViewElementStates.ReadOnly | DataGridViewElementStates.Frozen | DataGridViewElementStates.Displayed)) != DataGridViewElementStates.None)
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridView_InvalidDataGridViewElementStateCombination", new object[] { "excludeFilter" }));
            }
            if (indexStart > this.items.Count)
            {
                throw new ArgumentOutOfRangeException("indexStart", System.Windows.Forms.SR.GetString("InvalidHighBoundArgumentEx", new object[] { "indexStart", indexStart.ToString(CultureInfo.CurrentCulture), this.items.Count.ToString(CultureInfo.CurrentCulture) }));
            }
            int rowIndex = indexStart - 1;
            while ((rowIndex >= 0) && (((this.GetRowState(rowIndex) & includeFilter) != includeFilter) || ((this.GetRowState(rowIndex) & excludeFilter) != DataGridViewElementStates.None)))
            {
                rowIndex--;
            }
            if (rowIndex < 0)
            {
                return -1;
            }
            return rowIndex;
        }

        public int GetRowCount(DataGridViewElementStates includeFilter)
        {
            if ((includeFilter & ~(DataGridViewElementStates.Visible | DataGridViewElementStates.Selected | DataGridViewElementStates.Resizable | DataGridViewElementStates.ReadOnly | DataGridViewElementStates.Frozen | DataGridViewElementStates.Displayed)) != DataGridViewElementStates.None)
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridView_InvalidDataGridViewElementStateCombination", new object[] { "includeFilter" }));
            }
            switch (includeFilter)
            {
                case DataGridViewElementStates.Visible:
                    if (this.rowCountsVisible == -1)
                    {
                        break;
                    }
                    return this.rowCountsVisible;

                case (DataGridViewElementStates.Visible | DataGridViewElementStates.Frozen):
                    if (this.rowCountsVisibleFrozen == -1)
                    {
                        break;
                    }
                    return this.rowCountsVisibleFrozen;

                case (DataGridViewElementStates.Visible | DataGridViewElementStates.Selected):
                    if (this.rowCountsVisibleSelected != -1)
                    {
                        return this.rowCountsVisibleSelected;
                    }
                    break;
            }
            int num = 0;
            for (int i = 0; i < this.items.Count; i++)
            {
                if ((this.GetRowState(i) & includeFilter) == includeFilter)
                {
                    num++;
                }
            }
            switch (includeFilter)
            {
                case DataGridViewElementStates.Visible:
                    this.rowCountsVisible = num;
                    return num;

                case (DataGridViewElementStates.Visible | DataGridViewElementStates.Displayed):
                    return num;

                case (DataGridViewElementStates.Visible | DataGridViewElementStates.Frozen):
                    this.rowCountsVisibleFrozen = num;
                    return num;

                case (DataGridViewElementStates.Visible | DataGridViewElementStates.Selected):
                    this.rowCountsVisibleSelected = num;
                    return num;
            }
            return num;
        }

        internal int GetRowCount(DataGridViewElementStates includeFilter, int fromRowIndex, int toRowIndex)
        {
            int num = 0;
            for (int i = fromRowIndex + 1; i <= toRowIndex; i++)
            {
                if ((this.GetRowState(i) & includeFilter) == includeFilter)
                {
                    num++;
                }
            }
            return num;
        }

        public int GetRowsHeight(DataGridViewElementStates includeFilter)
        {
            if ((includeFilter & ~(DataGridViewElementStates.Visible | DataGridViewElementStates.Selected | DataGridViewElementStates.Resizable | DataGridViewElementStates.ReadOnly | DataGridViewElementStates.Frozen | DataGridViewElementStates.Displayed)) != DataGridViewElementStates.None)
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridView_InvalidDataGridViewElementStateCombination", new object[] { "includeFilter" }));
            }
            switch (includeFilter)
            {
                case DataGridViewElementStates.Visible:
                    if (this.rowsHeightVisible == -1)
                    {
                        break;
                    }
                    return this.rowsHeightVisible;

                case (DataGridViewElementStates.Visible | DataGridViewElementStates.Frozen):
                    if (this.rowsHeightVisibleFrozen == -1)
                    {
                        break;
                    }
                    return this.rowsHeightVisibleFrozen;
            }
            int num = 0;
            for (int i = 0; i < this.items.Count; i++)
            {
                if ((this.GetRowState(i) & includeFilter) == includeFilter)
                {
                    num += ((DataGridViewRow) this.items[i]).GetHeight(i);
                }
            }
            switch (includeFilter)
            {
                case DataGridViewElementStates.Visible:
                    this.rowsHeightVisible = num;
                    return num;

                case (DataGridViewElementStates.Visible | DataGridViewElementStates.Displayed):
                    return num;

                case (DataGridViewElementStates.Visible | DataGridViewElementStates.Frozen):
                    this.rowsHeightVisibleFrozen = num;
                    return num;
            }
            return num;
        }

        internal int GetRowsHeight(DataGridViewElementStates includeFilter, int fromRowIndex, int toRowIndex)
        {
            int num = 0;
            for (int i = fromRowIndex; i < toRowIndex; i++)
            {
                if ((this.GetRowState(i) & includeFilter) == includeFilter)
                {
                    num += ((DataGridViewRow) this.items[i]).GetHeight(i);
                }
            }
            return num;
        }

        private bool GetRowsHeightExceedLimit(DataGridViewElementStates includeFilter, int fromRowIndex, int toRowIndex, int heightLimit)
        {
            int num = 0;
            for (int i = fromRowIndex; i < toRowIndex; i++)
            {
                if ((this.GetRowState(i) & includeFilter) == includeFilter)
                {
                    num += ((DataGridViewRow) this.items[i]).GetHeight(i);
                    if (num > heightLimit)
                    {
                        return true;
                    }
                }
            }
            return (num > heightLimit);
        }

        public virtual DataGridViewElementStates GetRowState(int rowIndex)
        {
            if ((rowIndex < 0) || (rowIndex >= this.items.Count))
            {
                throw new ArgumentOutOfRangeException("rowIndex", System.Windows.Forms.SR.GetString("DataGridViewRowCollection_RowIndexOutOfRange"));
            }
            DataGridViewRow row = this.SharedRow(rowIndex);
            if (row.Index == -1)
            {
                return this.SharedRowState(rowIndex);
            }
            return row.GetState(rowIndex);
        }

        public int IndexOf(DataGridViewRow dataGridViewRow)
        {
            return this.items.IndexOf(dataGridViewRow);
        }

        public virtual void Insert(int rowIndex, params object[] values)
        {
            if (values == null)
            {
                throw new ArgumentNullException("values");
            }
            if (this.DataGridView.VirtualMode)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridView_InvalidOperationInVirtualMode"));
            }
            if (this.DataGridView.DataSource != null)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewRowCollection_AddUnboundRow"));
            }
            DataGridViewRow rowTemplateClone = this.DataGridView.RowTemplateClone;
            rowTemplateClone.SetValuesInternal(values);
            this.Insert(rowIndex, rowTemplateClone);
        }

        public virtual void Insert(int rowIndex, int count)
        {
            if (this.DataGridView.DataSource != null)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewRowCollection_AddUnboundRow"));
            }
            if ((rowIndex < 0) || (this.Count < rowIndex))
            {
                throw new ArgumentOutOfRangeException("rowIndex", System.Windows.Forms.SR.GetString("DataGridViewRowCollection_IndexDestinationOutOfRange"));
            }
            if (count <= 0)
            {
                throw new ArgumentOutOfRangeException("count", System.Windows.Forms.SR.GetString("DataGridViewRowCollection_CountOutOfRange"));
            }
            if (this.DataGridView.NoDimensionChangeAllowed)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridView_ForbiddenOperationInEventHandler"));
            }
            if (this.DataGridView.Columns.Count == 0)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewRowCollection_NoColumns"));
            }
            if (this.DataGridView.RowTemplate.Cells.Count > this.DataGridView.Columns.Count)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewRowCollection_RowTemplateTooManyCells"));
            }
            if ((this.DataGridView.NewRowIndex != -1) && (rowIndex == this.Count))
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewRowCollection_NoInsertionAfterNewRow"));
            }
            DataGridViewRow rowTemplateClone = this.DataGridView.RowTemplateClone;
            DataGridViewElementStates state = rowTemplateClone.State;
            rowTemplateClone.DataGridViewInternal = this.dataGridView;
            int num = 0;
            foreach (DataGridViewCell cell in rowTemplateClone.Cells)
            {
                cell.DataGridViewInternal = this.dataGridView;
                cell.OwningColumnInternal = this.DataGridView.Columns[num];
                num++;
            }
            if (rowTemplateClone.HasHeaderCell)
            {
                rowTemplateClone.HeaderCell.DataGridViewInternal = this.dataGridView;
                rowTemplateClone.HeaderCell.OwningRowInternal = rowTemplateClone;
            }
            this.InsertCopiesPrivate(rowTemplateClone, state, rowIndex, count);
        }

        public virtual void Insert(int rowIndex, DataGridViewRow dataGridViewRow)
        {
            if (this.DataGridView.DataSource != null)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewRowCollection_AddUnboundRow"));
            }
            if (this.DataGridView.NoDimensionChangeAllowed)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridView_ForbiddenOperationInEventHandler"));
            }
            this.InsertInternal(rowIndex, dataGridViewRow);
        }

        public virtual void InsertCopies(int indexSource, int indexDestination, int count)
        {
            if (this.DataGridView.DataSource != null)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewRowCollection_AddUnboundRow"));
            }
            if (this.DataGridView.NoDimensionChangeAllowed)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridView_ForbiddenOperationInEventHandler"));
            }
            this.InsertCopiesPrivate(indexSource, indexDestination, count);
        }

        private void InsertCopiesPrivate(int indexSource, int indexDestination, int count)
        {
            if ((indexSource < 0) || (this.Count <= indexSource))
            {
                throw new ArgumentOutOfRangeException("indexSource", System.Windows.Forms.SR.GetString("DataGridViewRowCollection_IndexSourceOutOfRange"));
            }
            if ((indexDestination < 0) || (this.Count < indexDestination))
            {
                throw new ArgumentOutOfRangeException("indexDestination", System.Windows.Forms.SR.GetString("DataGridViewRowCollection_IndexDestinationOutOfRange"));
            }
            if (count <= 0)
            {
                throw new ArgumentOutOfRangeException("count", System.Windows.Forms.SR.GetString("DataGridViewRowCollection_CountOutOfRange"));
            }
            if ((this.DataGridView.NewRowIndex != -1) && (indexDestination == this.Count))
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewRowCollection_NoInsertionAfterNewRow"));
            }
            DataGridViewElementStates rowTemplateState = this.GetRowState(indexSource) & ~(DataGridViewElementStates.Selected | DataGridViewElementStates.Displayed);
            this.InsertCopiesPrivate(this.SharedRow(indexSource), rowTemplateState, indexDestination, count);
        }

        private void InsertCopiesPrivate(DataGridViewRow rowTemplate, DataGridViewElementStates rowTemplateState, int indexDestination, int count)
        {
            Point newCurrentCell = new Point(-1, -1);
            if (rowTemplate.Index == -1)
            {
                if (count > 1)
                {
                    this.DataGridView.OnInsertingRow(indexDestination, rowTemplate, rowTemplateState, ref newCurrentCell, true, count, false);
                    for (int i = 0; i < count; i++)
                    {
                        this.SharedList.Insert(indexDestination + i, rowTemplate);
                        this.rowStates.Insert(indexDestination + i, rowTemplateState);
                    }
                    this.DataGridView.OnInsertedRow_PreNotification(indexDestination, count);
                    this.OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Refresh, null), indexDestination, count, false, true, false, newCurrentCell);
                    for (int j = 0; j < count; j++)
                    {
                        this.DataGridView.OnInsertedRow_PostNotification(indexDestination + j, newCurrentCell, j == (count - 1));
                    }
                }
                else
                {
                    this.DataGridView.OnInsertingRow(indexDestination, rowTemplate, rowTemplateState, ref newCurrentCell, true, 1, false);
                    this.SharedList.Insert(indexDestination, rowTemplate);
                    this.rowStates.Insert(indexDestination, rowTemplateState);
                    this.OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Add, this.SharedRow(indexDestination)), indexDestination, count, false, true, false, newCurrentCell);
                }
            }
            else
            {
                this.InsertDuplicateRow(indexDestination, rowTemplate, true, ref newCurrentCell);
                if (count > 1)
                {
                    this.DataGridView.OnInsertedRow_PreNotification(indexDestination, 1);
                    if (this.RowIsSharable(indexDestination))
                    {
                        DataGridViewRow dataGridViewRow = this.SharedRow(indexDestination);
                        this.DataGridView.OnInsertingRow(indexDestination + 1, dataGridViewRow, rowTemplateState, ref newCurrentCell, false, count - 1, false);
                        for (int m = 1; m < count; m++)
                        {
                            this.SharedList.Insert(indexDestination + m, dataGridViewRow);
                            this.rowStates.Insert(indexDestination + m, rowTemplateState);
                        }
                        this.DataGridView.OnInsertedRow_PreNotification(indexDestination + 1, count - 1);
                        this.OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Refresh, null), indexDestination, count, false, true, false, newCurrentCell);
                    }
                    else
                    {
                        this.UnshareRow(indexDestination);
                        for (int n = 1; n < count; n++)
                        {
                            this.InsertDuplicateRow(indexDestination + n, rowTemplate, false, ref newCurrentCell);
                            this.UnshareRow(indexDestination + n);
                            this.DataGridView.OnInsertedRow_PreNotification(indexDestination + n, 1);
                        }
                        this.OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Refresh, null), indexDestination, count, false, true, false, newCurrentCell);
                    }
                    for (int k = 0; k < count; k++)
                    {
                        this.DataGridView.OnInsertedRow_PostNotification(indexDestination + k, newCurrentCell, k == (count - 1));
                    }
                }
                else
                {
                    if (this.IsCollectionChangedListenedTo)
                    {
                        this.UnshareRow(indexDestination);
                    }
                    this.OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Add, this.SharedRow(indexDestination)), indexDestination, 1, false, true, false, newCurrentCell);
                }
            }
        }

        public virtual void InsertCopy(int indexSource, int indexDestination)
        {
            this.InsertCopies(indexSource, indexDestination, 1);
        }

        private void InsertDuplicateRow(int indexDestination, DataGridViewRow rowTemplate, bool firstInsertion, ref Point newCurrentCell)
        {
            DataGridViewRow dataGridViewRow = (DataGridViewRow) rowTemplate.Clone();
            dataGridViewRow.StateInternal = DataGridViewElementStates.None;
            dataGridViewRow.DataGridViewInternal = this.dataGridView;
            DataGridViewCellCollection cells = dataGridViewRow.Cells;
            int num = 0;
            foreach (DataGridViewCell cell in cells)
            {
                cell.DataGridViewInternal = this.dataGridView;
                cell.OwningColumnInternal = this.DataGridView.Columns[num];
                num++;
            }
            DataGridViewElementStates rowState = rowTemplate.State & ~(DataGridViewElementStates.Selected | DataGridViewElementStates.Displayed);
            if (dataGridViewRow.HasHeaderCell)
            {
                dataGridViewRow.HeaderCell.DataGridViewInternal = this.dataGridView;
                dataGridViewRow.HeaderCell.OwningRowInternal = dataGridViewRow;
            }
            this.DataGridView.OnInsertingRow(indexDestination, dataGridViewRow, rowState, ref newCurrentCell, firstInsertion, 1, false);
            this.SharedList.Insert(indexDestination, dataGridViewRow);
            this.rowStates.Insert(indexDestination, rowState);
        }

        internal void InsertInternal(int rowIndex, DataGridViewRow dataGridViewRow)
        {
            if ((rowIndex < 0) || (this.Count < rowIndex))
            {
                throw new ArgumentOutOfRangeException("rowIndex", System.Windows.Forms.SR.GetString("DataGridViewRowCollection_RowIndexOutOfRange"));
            }
            if (dataGridViewRow == null)
            {
                throw new ArgumentNullException("dataGridViewRow");
            }
            if (dataGridViewRow.DataGridView != null)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridView_RowAlreadyBelongsToDataGridView"));
            }
            if ((this.DataGridView.NewRowIndex != -1) && (rowIndex == this.Count))
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewRowCollection_NoInsertionAfterNewRow"));
            }
            if (this.DataGridView.Columns.Count == 0)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewRowCollection_NoColumns"));
            }
            if (dataGridViewRow.Cells.Count > this.DataGridView.Columns.Count)
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridViewRowCollection_TooManyCells"), "dataGridViewRow");
            }
            if (dataGridViewRow.Selected)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewRowCollection_CannotAddOrInsertSelectedRow"));
            }
            this.InsertInternal(rowIndex, dataGridViewRow, false);
        }

        internal void InsertInternal(int rowIndex, DataGridViewRow dataGridViewRow, bool force)
        {
            Point newCurrentCell = new Point(-1, -1);
            if (force)
            {
                if (this.DataGridView.Columns.Count == 0)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewRowCollection_NoColumns"));
                }
                if (dataGridViewRow.Cells.Count > this.DataGridView.Columns.Count)
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridViewRowCollection_TooManyCells"), "dataGridViewRow");
                }
            }
            this.DataGridView.CompleteCellsCollection(dataGridViewRow);
            this.DataGridView.OnInsertingRow(rowIndex, dataGridViewRow, dataGridViewRow.State, ref newCurrentCell, true, 1, force);
            int num = 0;
            foreach (DataGridViewCell cell in dataGridViewRow.Cells)
            {
                cell.DataGridViewInternal = this.dataGridView;
                if (cell.ColumnIndex == -1)
                {
                    cell.OwningColumnInternal = this.DataGridView.Columns[num];
                }
                num++;
            }
            if (dataGridViewRow.HasHeaderCell)
            {
                dataGridViewRow.HeaderCell.DataGridViewInternal = this.DataGridView;
                dataGridViewRow.HeaderCell.OwningRowInternal = dataGridViewRow;
            }
            this.SharedList.Insert(rowIndex, dataGridViewRow);
            this.rowStates.Insert(rowIndex, dataGridViewRow.State);
            dataGridViewRow.DataGridViewInternal = this.dataGridView;
            if ((!this.RowIsSharable(rowIndex) || RowHasValueOrToolTipText(dataGridViewRow)) || this.IsCollectionChangedListenedTo)
            {
                dataGridViewRow.IndexInternal = rowIndex;
            }
            this.OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Add, dataGridViewRow), rowIndex, 1, false, true, false, newCurrentCell);
        }

        public virtual void InsertRange(int rowIndex, params DataGridViewRow[] dataGridViewRows)
        {
            if (dataGridViewRows == null)
            {
                throw new ArgumentNullException("dataGridViewRows");
            }
            if (dataGridViewRows.Length == 1)
            {
                this.Insert(rowIndex, dataGridViewRows[0]);
            }
            else
            {
                if ((rowIndex < 0) || (rowIndex > this.Count))
                {
                    throw new ArgumentOutOfRangeException("rowIndex", System.Windows.Forms.SR.GetString("DataGridViewRowCollection_IndexDestinationOutOfRange"));
                }
                if (this.DataGridView.NoDimensionChangeAllowed)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridView_ForbiddenOperationInEventHandler"));
                }
                if ((this.DataGridView.NewRowIndex != -1) && (rowIndex == this.Count))
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewRowCollection_NoInsertionAfterNewRow"));
                }
                if (this.DataGridView.DataSource != null)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewRowCollection_AddUnboundRow"));
                }
                if (this.DataGridView.Columns.Count == 0)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewRowCollection_NoColumns"));
                }
                Point newCurrentCell = new Point(-1, -1);
                this.DataGridView.OnInsertingRows(rowIndex, dataGridViewRows, ref newCurrentCell);
                int index = rowIndex;
                foreach (DataGridViewRow row in dataGridViewRows)
                {
                    int num2 = 0;
                    foreach (DataGridViewCell cell in row.Cells)
                    {
                        cell.DataGridViewInternal = this.dataGridView;
                        if (cell.ColumnIndex == -1)
                        {
                            cell.OwningColumnInternal = this.DataGridView.Columns[num2];
                        }
                        num2++;
                    }
                    if (row.HasHeaderCell)
                    {
                        row.HeaderCell.DataGridViewInternal = this.DataGridView;
                        row.HeaderCell.OwningRowInternal = row;
                    }
                    this.SharedList.Insert(index, row);
                    this.rowStates.Insert(index, row.State);
                    row.IndexInternal = index;
                    row.DataGridViewInternal = this.dataGridView;
                    index++;
                }
                this.DataGridView.OnInsertedRows_PreNotification(rowIndex, dataGridViewRows);
                this.OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Refresh, null), rowIndex, dataGridViewRows.Length, false, true, false, newCurrentCell);
                this.DataGridView.OnInsertedRows_PostNotification(dataGridViewRows, newCurrentCell);
            }
        }

        internal void InvalidateCachedRowCount(DataGridViewElementStates includeFilter)
        {
            if (includeFilter == DataGridViewElementStates.Visible)
            {
                this.InvalidateCachedRowCounts();
            }
            else if (includeFilter == DataGridViewElementStates.Frozen)
            {
                this.rowCountsVisibleFrozen = -1;
            }
            else if (includeFilter == DataGridViewElementStates.Selected)
            {
                this.rowCountsVisibleSelected = -1;
            }
        }

        internal void InvalidateCachedRowCounts()
        {
            this.rowCountsVisible = this.rowCountsVisibleFrozen = this.rowCountsVisibleSelected = -1;
        }

        internal void InvalidateCachedRowsHeight(DataGridViewElementStates includeFilter)
        {
            if (includeFilter == DataGridViewElementStates.Visible)
            {
                this.InvalidateCachedRowsHeights();
            }
            else if (includeFilter == DataGridViewElementStates.Frozen)
            {
                this.rowsHeightVisibleFrozen = -1;
            }
        }

        internal void InvalidateCachedRowsHeights()
        {
            this.rowsHeightVisible = this.rowsHeightVisibleFrozen = -1;
        }

        protected virtual void OnCollectionChanged(CollectionChangeEventArgs e)
        {
            if (this.onCollectionChanged != null)
            {
                this.onCollectionChanged(this, e);
            }
        }

        private void OnCollectionChanged(CollectionChangeEventArgs e, int rowIndex, int rowCount)
        {
            Point newCurrentCell = new Point(-1, -1);
            DataGridViewRow element = (DataGridViewRow) e.Element;
            int index = 0;
            if ((element != null) && (e.Action == CollectionChangeAction.Add))
            {
                index = this.SharedRow(rowIndex).Index;
            }
            this.OnCollectionChanged_PreNotification(e.Action, rowIndex, rowCount, ref element, false);
            if ((index == -1) && (this.SharedRow(rowIndex).Index != -1))
            {
                e = new CollectionChangeEventArgs(e.Action, element);
            }
            this.OnCollectionChanged(e);
            this.OnCollectionChanged_PostNotification(e.Action, rowIndex, rowCount, element, false, false, false, newCurrentCell);
        }

        private void OnCollectionChanged(CollectionChangeEventArgs e, int rowIndex, int rowCount, bool changeIsDeletion, bool changeIsInsertion, bool recreateNewRow, Point newCurrentCell)
        {
            DataGridViewRow element = (DataGridViewRow) e.Element;
            int index = 0;
            if ((element != null) && (e.Action == CollectionChangeAction.Add))
            {
                index = this.SharedRow(rowIndex).Index;
            }
            this.OnCollectionChanged_PreNotification(e.Action, rowIndex, rowCount, ref element, changeIsInsertion);
            if ((index == -1) && (this.SharedRow(rowIndex).Index != -1))
            {
                e = new CollectionChangeEventArgs(e.Action, element);
            }
            this.OnCollectionChanged(e);
            this.OnCollectionChanged_PostNotification(e.Action, rowIndex, rowCount, element, changeIsDeletion, changeIsInsertion, recreateNewRow, newCurrentCell);
        }

        private void OnCollectionChanged_PostNotification(CollectionChangeAction cca, int rowIndex, int rowCount, DataGridViewRow dataGridViewRow, bool changeIsDeletion, bool changeIsInsertion, bool recreateNewRow, Point newCurrentCell)
        {
            if (changeIsDeletion)
            {
                this.DataGridView.OnRowsRemovedInternal(rowIndex, rowCount);
            }
            else
            {
                this.DataGridView.OnRowsAddedInternal(rowIndex, rowCount);
            }
            switch (cca)
            {
                case CollectionChangeAction.Add:
                    if (!changeIsInsertion)
                    {
                        this.DataGridView.OnAddedRow_PostNotification(rowIndex);
                        break;
                    }
                    this.DataGridView.OnInsertedRow_PostNotification(rowIndex, newCurrentCell, true);
                    break;

                case CollectionChangeAction.Remove:
                    this.DataGridView.OnRemovedRow_PostNotification(dataGridViewRow, newCurrentCell);
                    break;

                case CollectionChangeAction.Refresh:
                    if (changeIsDeletion)
                    {
                        this.DataGridView.OnClearedRows();
                    }
                    break;
            }
            this.DataGridView.OnRowCollectionChanged_PostNotification(recreateNewRow, newCurrentCell.X == -1, cca, dataGridViewRow, rowIndex);
        }

        private void OnCollectionChanged_PreNotification(CollectionChangeAction cca, int rowIndex, int rowCount, ref DataGridViewRow dataGridViewRow, bool changeIsInsertion)
        {
            int height;
            bool useRowShortcut = false;
            bool computeVisibleRows = false;
            switch (cca)
            {
                case CollectionChangeAction.Add:
                    height = 0;
                    this.UpdateRowCaches(rowIndex, ref dataGridViewRow, true);
                    if ((this.GetRowState(rowIndex) & DataGridViewElementStates.Visible) != DataGridViewElementStates.None)
                    {
                        int firstDisplayedRowIndex = this.DataGridView.FirstDisplayedRowIndex;
                        if (firstDisplayedRowIndex != -1)
                        {
                            height = this.SharedRow(firstDisplayedRowIndex).GetHeight(firstDisplayedRowIndex);
                        }
                        break;
                    }
                    useRowShortcut = true;
                    computeVisibleRows = changeIsInsertion;
                    break;

                case CollectionChangeAction.Remove:
                {
                    DataGridViewElementStates rowState = this.GetRowState(rowIndex);
                    bool flag3 = (rowState & DataGridViewElementStates.Visible) != DataGridViewElementStates.None;
                    bool flag4 = (rowState & DataGridViewElementStates.Frozen) != DataGridViewElementStates.None;
                    this.rowStates.RemoveAt(rowIndex);
                    this.SharedList.RemoveAt(rowIndex);
                    this.DataGridView.OnRemovedRow_PreNotification(rowIndex);
                    if (!flag3)
                    {
                        useRowShortcut = true;
                    }
                    else if (!flag4)
                    {
                        if ((this.DataGridView.FirstDisplayedScrollingRowIndex != -1) && (rowIndex > this.DataGridView.FirstDisplayedScrollingRowIndex))
                        {
                            int num4 = 0;
                            int num5 = this.DataGridView.FirstDisplayedRowIndex;
                            if (num5 != -1)
                            {
                                num4 = this.SharedRow(num5).GetHeight(num5);
                            }
                            useRowShortcut = this.GetRowsHeightExceedLimit(DataGridViewElementStates.Visible, 0, rowIndex, (this.DataGridView.LayoutInfo.Data.Height + this.DataGridView.VerticalScrollingOffset) + SystemInformation.HorizontalScrollBarHeight) && (num4 <= this.DataGridView.LayoutInfo.Data.Height);
                        }
                    }
                    else
                    {
                        useRowShortcut = (this.DataGridView.FirstDisplayedScrollingRowIndex == -1) && this.GetRowsHeightExceedLimit(DataGridViewElementStates.Visible, 0, rowIndex, this.DataGridView.LayoutInfo.Data.Height + SystemInformation.HorizontalScrollBarHeight);
                    }
                    goto Label_02DF;
                }
                case CollectionChangeAction.Refresh:
                    this.InvalidateCachedRowCounts();
                    this.InvalidateCachedRowsHeights();
                    goto Label_02DF;

                default:
                    goto Label_02DF;
            }
            if (changeIsInsertion)
            {
                this.DataGridView.OnInsertedRow_PreNotification(rowIndex, 1);
                if (!useRowShortcut)
                {
                    if ((this.GetRowState(rowIndex) & DataGridViewElementStates.Frozen) != DataGridViewElementStates.None)
                    {
                        useRowShortcut = (this.DataGridView.FirstDisplayedScrollingRowIndex == -1) && this.GetRowsHeightExceedLimit(DataGridViewElementStates.Visible, 0, rowIndex, this.DataGridView.LayoutInfo.Data.Height);
                    }
                    else if ((this.DataGridView.FirstDisplayedScrollingRowIndex != -1) && (rowIndex > this.DataGridView.FirstDisplayedScrollingRowIndex))
                    {
                        useRowShortcut = this.GetRowsHeightExceedLimit(DataGridViewElementStates.Visible, 0, rowIndex, this.DataGridView.LayoutInfo.Data.Height + this.DataGridView.VerticalScrollingOffset) && (height <= this.DataGridView.LayoutInfo.Data.Height);
                    }
                }
            }
            else
            {
                this.DataGridView.OnAddedRow_PreNotification(rowIndex);
                if (!useRowShortcut)
                {
                    int num3 = (this.GetRowsHeight(DataGridViewElementStates.Visible) - this.DataGridView.VerticalScrollingOffset) - dataGridViewRow.GetHeight(rowIndex);
                    dataGridViewRow = this.SharedRow(rowIndex);
                    useRowShortcut = (this.DataGridView.LayoutInfo.Data.Height < num3) && (height <= this.DataGridView.LayoutInfo.Data.Height);
                }
            }
        Label_02DF:
            this.DataGridView.ResetUIState(useRowShortcut, computeVisibleRows);
        }

        public virtual void Remove(DataGridViewRow dataGridViewRow)
        {
            if (dataGridViewRow == null)
            {
                throw new ArgumentNullException("dataGridViewRow");
            }
            if (dataGridViewRow.DataGridView != this.DataGridView)
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridView_RowDoesNotBelongToDataGridView"), "dataGridViewRow");
            }
            if (dataGridViewRow.Index == -1)
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridView_RowMustBeUnshared"), "dataGridViewRow");
            }
            this.RemoveAt(dataGridViewRow.Index);
        }

        public virtual void RemoveAt(int index)
        {
            if ((index < 0) || (index >= this.Count))
            {
                throw new ArgumentOutOfRangeException("index", System.Windows.Forms.SR.GetString("DataGridViewRowCollection_RowIndexOutOfRange"));
            }
            if (this.DataGridView.NewRowIndex == index)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewRowCollection_CannotDeleteNewRow"));
            }
            if (this.DataGridView.NoDimensionChangeAllowed)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridView_ForbiddenOperationInEventHandler"));
            }
            if (this.DataGridView.DataSource != null)
            {
                IBindingList list = this.DataGridView.DataConnection.List as IBindingList;
                if (((list == null) || !list.AllowRemove) || !list.SupportsChangeNotification)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewRowCollection_CantRemoveRowsWithWrongSource"));
                }
                list.RemoveAt(index);
            }
            else
            {
                this.RemoveAtInternal(index, false);
            }
        }

        internal void RemoveAtInternal(int index, bool force)
        {
            DataGridViewRow dataGridViewRow = this.SharedRow(index);
            Point newCurrentCell = new Point(-1, -1);
            if (this.IsCollectionChangedListenedTo || dataGridViewRow.GetDisplayed(index))
            {
                dataGridViewRow = this[index];
            }
            dataGridViewRow = this.SharedRow(index);
            this.DataGridView.OnRemovingRow(index, out newCurrentCell, force);
            this.UpdateRowCaches(index, ref dataGridViewRow, false);
            if (dataGridViewRow.Index != -1)
            {
                this.rowStates[index] = dataGridViewRow.State;
                dataGridViewRow.DetachFromDataGridView();
            }
            this.OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Remove, dataGridViewRow), index, 1, true, false, false, newCurrentCell);
        }

        private static bool RowHasValueOrToolTipText(DataGridViewRow dataGridViewRow)
        {
            foreach (DataGridViewCell cell in dataGridViewRow.Cells)
            {
                if (cell.HasValue || cell.HasToolTipText)
                {
                    return true;
                }
            }
            return false;
        }

        internal bool RowIsSharable(int index)
        {
            DataGridViewRow row = this.SharedRow(index);
            if (row.Index != -1)
            {
                return false;
            }
            foreach (DataGridViewCell cell in row.Cells)
            {
                if ((cell.State & ~cell.CellStateFromColumnRowStates(this.rowStates[index])) != DataGridViewElementStates.None)
                {
                    return false;
                }
            }
            return true;
        }

        internal void SetRowState(int rowIndex, DataGridViewElementStates state, bool value)
        {
            DataGridViewRow row = this.SharedRow(rowIndex);
            if (row.Index == -1)
            {
                if (((((DataGridViewElementStates) this.rowStates[rowIndex]) & state) != DataGridViewElementStates.None) != value)
                {
                    if (((state == DataGridViewElementStates.Frozen) || (state == DataGridViewElementStates.Visible)) || (state == DataGridViewElementStates.ReadOnly))
                    {
                        row.OnSharedStateChanging(rowIndex, state);
                    }
                    if (value)
                    {
                        this.rowStates[rowIndex] = ((DataGridViewElementStates) this.rowStates[rowIndex]) | state;
                    }
                    else
                    {
                        this.rowStates[rowIndex] = ((DataGridViewElementStates) this.rowStates[rowIndex]) & ~state;
                    }
                    row.OnSharedStateChanged(rowIndex, state);
                }
            }
            else
            {
                DataGridViewElementStates states = state;
                if (states <= DataGridViewElementStates.Resizable)
                {
                    switch (states)
                    {
                        case DataGridViewElementStates.Displayed:
                            row.DisplayedInternal = value;
                            return;

                        case DataGridViewElementStates.Frozen:
                            row.Frozen = value;
                            return;

                        case (DataGridViewElementStates.Frozen | DataGridViewElementStates.Displayed):
                            return;

                        case DataGridViewElementStates.ReadOnly:
                            row.ReadOnlyInternal = value;
                            return;

                        case DataGridViewElementStates.Resizable:
                            row.Resizable = value ? DataGridViewTriState.True : DataGridViewTriState.False;
                            return;
                    }
                }
                else
                {
                    if (states != DataGridViewElementStates.Selected)
                    {
                        if (states != DataGridViewElementStates.Visible)
                        {
                            return;
                        }
                    }
                    else
                    {
                        row.SelectedInternal = value;
                        return;
                    }
                    row.Visible = value;
                }
            }
        }

        public DataGridViewRow SharedRow(int rowIndex)
        {
            return (DataGridViewRow) this.SharedList[rowIndex];
        }

        internal DataGridViewElementStates SharedRowState(int rowIndex)
        {
            return this.rowStates[rowIndex];
        }

        internal void Sort(IComparer customComparer, bool ascending)
        {
            if (this.items.Count > 0)
            {
                RowComparer rowComparer = new RowComparer(this, customComparer, ascending);
                this.items.CustomSort(rowComparer);
            }
        }

        internal void SwapSortedRows(int rowIndex1, int rowIndex2)
        {
            this.DataGridView.SwapSortedRows(rowIndex1, rowIndex2);
            DataGridViewRow row = this.SharedRow(rowIndex1);
            DataGridViewRow row2 = this.SharedRow(rowIndex2);
            if (row.Index != -1)
            {
                row.IndexInternal = rowIndex2;
            }
            if (row2.Index != -1)
            {
                row2.IndexInternal = rowIndex1;
            }
            if (this.DataGridView.VirtualMode)
            {
                int count = this.DataGridView.Columns.Count;
                for (int i = 0; i < count; i++)
                {
                    DataGridViewCell cell = row.Cells[i];
                    DataGridViewCell cell2 = row2.Cells[i];
                    object valueInternal = cell.GetValueInternal(rowIndex1);
                    object obj3 = cell2.GetValueInternal(rowIndex2);
                    cell.SetValueInternal(rowIndex1, obj3);
                    cell2.SetValueInternal(rowIndex2, valueInternal);
                }
            }
            object obj4 = this.items[rowIndex1];
            this.items[rowIndex1] = this.items[rowIndex2];
            this.items[rowIndex2] = obj4;
            DataGridViewElementStates states = this.rowStates[rowIndex1];
            this.rowStates[rowIndex1] = this.rowStates[rowIndex2];
            this.rowStates[rowIndex2] = states;
        }

        void ICollection.CopyTo(Array array, int index)
        {
            this.items.CopyTo(array, index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new UnsharingRowEnumerator(this);
        }

        int IList.Add(object value)
        {
            return this.Add((DataGridViewRow) value);
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
            this.Insert(index, (DataGridViewRow) value);
        }

        void IList.Remove(object value)
        {
            this.Remove((DataGridViewRow) value);
        }

        void IList.RemoveAt(int index)
        {
            this.RemoveAt(index);
        }

        private void UnshareRow(int rowIndex)
        {
            this.SharedRow(rowIndex).IndexInternal = rowIndex;
            this.SharedRow(rowIndex).StateInternal = this.SharedRowState(rowIndex);
        }

        private void UpdateRowCaches(int rowIndex, ref DataGridViewRow dataGridViewRow, bool adding)
        {
            if (((this.rowCountsVisible != -1) || (this.rowCountsVisibleFrozen != -1)) || (((this.rowCountsVisibleSelected != -1) || (this.rowsHeightVisible != -1)) || (this.rowsHeightVisibleFrozen != -1)))
            {
                DataGridViewElementStates rowState = this.GetRowState(rowIndex);
                if ((rowState & DataGridViewElementStates.Visible) != DataGridViewElementStates.None)
                {
                    int num = adding ? 1 : -1;
                    int num2 = 0;
                    if ((this.rowsHeightVisible != -1) || ((this.rowsHeightVisibleFrozen != -1) && ((rowState & (DataGridViewElementStates.Visible | DataGridViewElementStates.Frozen)) == (DataGridViewElementStates.Visible | DataGridViewElementStates.Frozen))))
                    {
                        num2 = adding ? dataGridViewRow.GetHeight(rowIndex) : -dataGridViewRow.GetHeight(rowIndex);
                        dataGridViewRow = this.SharedRow(rowIndex);
                    }
                    if (this.rowCountsVisible != -1)
                    {
                        this.rowCountsVisible += num;
                    }
                    if (this.rowsHeightVisible != -1)
                    {
                        this.rowsHeightVisible += num2;
                    }
                    if ((rowState & (DataGridViewElementStates.Visible | DataGridViewElementStates.Frozen)) == (DataGridViewElementStates.Visible | DataGridViewElementStates.Frozen))
                    {
                        if (this.rowCountsVisibleFrozen != -1)
                        {
                            this.rowCountsVisibleFrozen += num;
                        }
                        if (this.rowsHeightVisibleFrozen != -1)
                        {
                            this.rowsHeightVisibleFrozen += num2;
                        }
                    }
                    if (((rowState & (DataGridViewElementStates.Visible | DataGridViewElementStates.Selected)) == (DataGridViewElementStates.Visible | DataGridViewElementStates.Selected)) && (this.rowCountsVisibleSelected != -1))
                    {
                        this.rowCountsVisibleSelected += num;
                    }
                }
            }
        }

        public int Count
        {
            get
            {
                return this.items.Count;
            }
        }

        protected System.Windows.Forms.DataGridView DataGridView
        {
            get
            {
                return this.dataGridView;
            }
        }

        internal bool IsCollectionChangedListenedTo
        {
            get
            {
                return (this.onCollectionChanged != null);
            }
        }

        public DataGridViewRow this[int index]
        {
            get
            {
                DataGridViewRow dataGridViewRow = this.SharedRow(index);
                if (dataGridViewRow.Index != -1)
                {
                    return dataGridViewRow;
                }
                if ((index == 0) && (this.items.Count == 1))
                {
                    dataGridViewRow.IndexInternal = 0;
                    dataGridViewRow.StateInternal = this.SharedRowState(0);
                    if (this.DataGridView != null)
                    {
                        this.DataGridView.OnRowUnshared(dataGridViewRow);
                    }
                    return dataGridViewRow;
                }
                DataGridViewRow row2 = (DataGridViewRow) dataGridViewRow.Clone();
                row2.IndexInternal = index;
                row2.DataGridViewInternal = dataGridViewRow.DataGridView;
                row2.StateInternal = this.SharedRowState(index);
                this.SharedList[index] = row2;
                int num = 0;
                foreach (DataGridViewCell cell in row2.Cells)
                {
                    cell.DataGridViewInternal = dataGridViewRow.DataGridView;
                    cell.OwningRowInternal = row2;
                    cell.OwningColumnInternal = this.DataGridView.Columns[num];
                    num++;
                }
                if (row2.HasHeaderCell)
                {
                    row2.HeaderCell.DataGridViewInternal = dataGridViewRow.DataGridView;
                    row2.HeaderCell.OwningRowInternal = row2;
                }
                if (this.DataGridView != null)
                {
                    this.DataGridView.OnRowUnshared(row2);
                }
                return row2;
            }
        }

        protected ArrayList List
        {
            get
            {
                int count = this.Count;
                for (int i = 0; i < count; i++)
                {
                    DataGridViewRow row1 = this[i];
                }
                return this.items;
            }
        }

        internal ArrayList SharedList
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
                return this.Count;
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
                throw new NotSupportedException();
            }
        }

        private class RowArrayList : ArrayList
        {
            private DataGridViewRowCollection owner;
            private DataGridViewRowCollection.RowComparer rowComparer;

            public RowArrayList(DataGridViewRowCollection owner)
            {
                this.owner = owner;
            }

            private void CustomQuickSort(int left, int right)
            {
            Label_0000:
                if ((right - left) < 2)
                {
                    if (((right - left) > 0) && (this.rowComparer.CompareObjects(this.rowComparer.GetComparedObject(left), this.rowComparer.GetComparedObject(right), left, right) > 0))
                    {
                        this.owner.SwapSortedRows(left, right);
                    }
                }
                else
                {
                    int center = (left + right) >> 1;
                    object obj2 = this.Pivot(left, center, right);
                    int num2 = left + 1;
                    int num3 = right - 1;
                    do
                    {
                        while ((center != num2) && (this.rowComparer.CompareObjects(this.rowComparer.GetComparedObject(num2), obj2, num2, center) < 0))
                        {
                            num2++;
                        }
                        while ((center != num3) && (this.rowComparer.CompareObjects(obj2, this.rowComparer.GetComparedObject(num3), center, num3) < 0))
                        {
                            num3--;
                        }
                        if (num2 > num3)
                        {
                            break;
                        }
                        if (num2 < num3)
                        {
                            this.owner.SwapSortedRows(num2, num3);
                            if (num2 == center)
                            {
                                center = num3;
                            }
                            else if (num3 == center)
                            {
                                center = num2;
                            }
                        }
                        num2++;
                        num3--;
                    }
                    while (num2 <= num3);
                    if ((num3 - left) <= (right - num2))
                    {
                        if (left < num3)
                        {
                            this.CustomQuickSort(left, num3);
                        }
                        left = num2;
                    }
                    else
                    {
                        if (num2 < right)
                        {
                            this.CustomQuickSort(num2, right);
                        }
                        right = num3;
                    }
                    if (left < right)
                    {
                        goto Label_0000;
                    }
                }
            }

            public void CustomSort(DataGridViewRowCollection.RowComparer rowComparer)
            {
                this.rowComparer = rowComparer;
                this.CustomQuickSort(0, this.Count - 1);
            }

            private object Pivot(int left, int center, int right)
            {
                if (this.rowComparer.CompareObjects(this.rowComparer.GetComparedObject(left), this.rowComparer.GetComparedObject(center), left, center) > 0)
                {
                    this.owner.SwapSortedRows(left, center);
                }
                if (this.rowComparer.CompareObjects(this.rowComparer.GetComparedObject(left), this.rowComparer.GetComparedObject(right), left, right) > 0)
                {
                    this.owner.SwapSortedRows(left, right);
                }
                if (this.rowComparer.CompareObjects(this.rowComparer.GetComparedObject(center), this.rowComparer.GetComparedObject(right), center, right) > 0)
                {
                    this.owner.SwapSortedRows(center, right);
                }
                return this.rowComparer.GetComparedObject(center);
            }
        }

        private class RowComparer
        {
            private bool ascending;
            private IComparer customComparer;
            private DataGridView dataGridView;
            private DataGridViewRowCollection dataGridViewRows;
            private DataGridViewColumn dataGridViewSortedColumn;
            private static ComparedObjectMax max = new ComparedObjectMax();
            private int sortedColumnIndex;

            public RowComparer(DataGridViewRowCollection dataGridViewRows, IComparer customComparer, bool ascending)
            {
                this.dataGridView = dataGridViewRows.DataGridView;
                this.dataGridViewRows = dataGridViewRows;
                this.dataGridViewSortedColumn = this.dataGridView.SortedColumn;
                if (this.dataGridViewSortedColumn == null)
                {
                    this.sortedColumnIndex = -1;
                }
                else
                {
                    this.sortedColumnIndex = this.dataGridViewSortedColumn.Index;
                }
                this.customComparer = customComparer;
                this.ascending = ascending;
            }

            internal int CompareObjects(object value1, object value2, int rowIndex1, int rowIndex2)
            {
                if (value1 is ComparedObjectMax)
                {
                    return 1;
                }
                if (value2 is ComparedObjectMax)
                {
                    return -1;
                }
                int sortResult = 0;
                if (this.customComparer == null)
                {
                    if (!this.dataGridView.OnSortCompare(this.dataGridViewSortedColumn, value1, value2, rowIndex1, rowIndex2, out sortResult))
                    {
                        if ((value1 is IComparable) || (value2 is IComparable))
                        {
                            sortResult = Comparer.Default.Compare(value1, value2);
                        }
                        else if (value1 == null)
                        {
                            if (value2 == null)
                            {
                                sortResult = 0;
                            }
                            else
                            {
                                sortResult = 1;
                            }
                        }
                        else if (value2 == null)
                        {
                            sortResult = -1;
                        }
                        else
                        {
                            sortResult = Comparer.Default.Compare(value1.ToString(), value2.ToString());
                        }
                        if (sortResult == 0)
                        {
                            if (this.ascending)
                            {
                                sortResult = rowIndex1 - rowIndex2;
                            }
                            else
                            {
                                sortResult = rowIndex2 - rowIndex1;
                            }
                        }
                    }
                }
                else
                {
                    sortResult = this.customComparer.Compare(value1, value2);
                }
                if (this.ascending)
                {
                    return sortResult;
                }
                return -sortResult;
            }

            internal object GetComparedObject(int rowIndex)
            {
                if ((this.dataGridView.NewRowIndex != -1) && (rowIndex == this.dataGridView.NewRowIndex))
                {
                    return max;
                }
                if (this.customComparer == null)
                {
                    return this.dataGridViewRows.SharedRow(rowIndex).Cells[this.sortedColumnIndex].GetValueInternal(rowIndex);
                }
                return this.dataGridViewRows[rowIndex];
            }

            private class ComparedObjectMax
            {
            }
        }

        private class UnsharingRowEnumerator : IEnumerator
        {
            private int current;
            private DataGridViewRowCollection owner;

            public UnsharingRowEnumerator(DataGridViewRowCollection owner)
            {
                this.owner = owner;
                this.current = -1;
            }

            bool IEnumerator.MoveNext()
            {
                if (this.current < (this.owner.Count - 1))
                {
                    this.current++;
                    return true;
                }
                this.current = this.owner.Count;
                return false;
            }

            void IEnumerator.Reset()
            {
                this.current = -1;
            }

            object IEnumerator.Current
            {
                get
                {
                    if (this.current == -1)
                    {
                        throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewRowCollection_EnumNotStarted"));
                    }
                    if (this.current == this.owner.Count)
                    {
                        throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewRowCollection_EnumFinished"));
                    }
                    return this.owner[this.current];
                }
            }
        }
    }
}

