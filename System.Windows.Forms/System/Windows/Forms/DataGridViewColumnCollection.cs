namespace System.Windows.Forms
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Drawing;
    using System.Globalization;
    using System.Reflection;

    [ListBindable(false)]
    public class DataGridViewColumnCollection : BaseCollection, IList, ICollection, IEnumerable
    {
        private int columnCountsVisible;
        private int columnCountsVisibleSelected;
        private static ColumnOrderComparer columnOrderComparer = new ColumnOrderComparer();
        private int columnsWidthVisible;
        private int columnsWidthVisibleFrozen;
        private System.Windows.Forms.DataGridView dataGridView;
        private ArrayList items = new ArrayList();
        private ArrayList itemsSorted;
        private int lastAccessedSortedIndex = -1;

        public event CollectionChangeEventHandler CollectionChanged;

        public DataGridViewColumnCollection(System.Windows.Forms.DataGridView dataGridView)
        {
            this.InvalidateCachedColumnCounts();
            this.InvalidateCachedColumnsWidths();
            this.dataGridView = dataGridView;
        }

        internal int ActualDisplayIndexToColumnIndex(int actualDisplayIndex, DataGridViewElementStates includeFilter)
        {
            DataGridViewColumn firstColumn = this.GetFirstColumn(includeFilter);
            for (int i = 0; i < actualDisplayIndex; i++)
            {
                firstColumn = this.GetNextColumn(firstColumn, includeFilter, DataGridViewElementStates.None);
            }
            return firstColumn.Index;
        }

        public virtual int Add(DataGridViewColumn dataGridViewColumn)
        {
            if (this.DataGridView.NoDimensionChangeAllowed)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridView_ForbiddenOperationInEventHandler"));
            }
            if (this.DataGridView.InDisplayIndexAdjustments)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridView_CannotAlterDisplayIndexWithinAdjustments"));
            }
            this.DataGridView.OnAddingColumn(dataGridViewColumn);
            this.InvalidateCachedColumnsOrder();
            int num = this.items.Add(dataGridViewColumn);
            dataGridViewColumn.IndexInternal = num;
            dataGridViewColumn.DataGridViewInternal = this.dataGridView;
            this.UpdateColumnCaches(dataGridViewColumn, true);
            this.DataGridView.OnAddedColumn(dataGridViewColumn);
            this.OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Add, dataGridViewColumn), false, new Point(-1, -1));
            return num;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual int Add(string columnName, string headerText)
        {
            DataGridViewTextBoxColumn dataGridViewColumn = new DataGridViewTextBoxColumn {
                Name = columnName,
                HeaderText = headerText
            };
            return this.Add(dataGridViewColumn);
        }

        public virtual void AddRange(params DataGridViewColumn[] dataGridViewColumns)
        {
            int num3;
            if (dataGridViewColumns == null)
            {
                throw new ArgumentNullException("dataGridViewColumns");
            }
            if (this.DataGridView.NoDimensionChangeAllowed)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridView_ForbiddenOperationInEventHandler"));
            }
            if (this.DataGridView.InDisplayIndexAdjustments)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridView_CannotAlterDisplayIndexWithinAdjustments"));
            }
            ArrayList list = new ArrayList(dataGridViewColumns.Length);
            ArrayList list2 = new ArrayList(dataGridViewColumns.Length);
            foreach (DataGridViewColumn column in dataGridViewColumns)
            {
                if (column.DisplayIndex != -1)
                {
                    list.Add(column);
                }
            }
            while (list.Count > 0)
            {
                int displayIndex = 0x7fffffff;
                int index = -1;
                for (num3 = 0; num3 < list.Count; num3++)
                {
                    DataGridViewColumn column2 = (DataGridViewColumn) list[num3];
                    if (column2.DisplayIndex < displayIndex)
                    {
                        displayIndex = column2.DisplayIndex;
                        index = num3;
                    }
                }
                list2.Add(list[index]);
                list.RemoveAt(index);
            }
            foreach (DataGridViewColumn column3 in dataGridViewColumns)
            {
                if (column3.DisplayIndex == -1)
                {
                    list2.Add(column3);
                }
            }
            num3 = 0;
            foreach (DataGridViewColumn column4 in list2)
            {
                dataGridViewColumns[num3] = column4;
                num3++;
            }
            this.DataGridView.OnAddingColumns(dataGridViewColumns);
            foreach (DataGridViewColumn column5 in dataGridViewColumns)
            {
                this.InvalidateCachedColumnsOrder();
                num3 = this.items.Add(column5);
                column5.IndexInternal = num3;
                column5.DataGridViewInternal = this.dataGridView;
                this.UpdateColumnCaches(column5, true);
                this.DataGridView.OnAddedColumn(column5);
            }
            this.OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Refresh, null), false, new Point(-1, -1));
        }

        public virtual void Clear()
        {
            if (this.Count > 0)
            {
                if (this.DataGridView.NoDimensionChangeAllowed)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridView_ForbiddenOperationInEventHandler"));
                }
                if (this.DataGridView.InDisplayIndexAdjustments)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridView_CannotAlterDisplayIndexWithinAdjustments"));
                }
                for (int i = 0; i < this.Count; i++)
                {
                    DataGridViewColumn column = this[i];
                    column.DataGridViewInternal = null;
                    if (column.HasHeaderCell)
                    {
                        column.HeaderCell.DataGridViewInternal = null;
                    }
                }
                DataGridViewColumn[] array = new DataGridViewColumn[this.items.Count];
                this.CopyTo(array, 0);
                this.DataGridView.OnClearingColumns();
                this.InvalidateCachedColumnsOrder();
                this.items.Clear();
                this.InvalidateCachedColumnCounts();
                this.InvalidateCachedColumnsWidths();
                foreach (DataGridViewColumn column2 in array)
                {
                    this.DataGridView.OnColumnRemoved(column2);
                    this.DataGridView.OnColumnHidden(column2);
                }
                this.OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Refresh, null), false, new Point(-1, -1));
            }
        }

        internal int ColumnIndexToActualDisplayIndex(int columnIndex, DataGridViewElementStates includeFilter)
        {
            DataGridViewColumn firstColumn = this.GetFirstColumn(includeFilter);
            int num = 0;
            while ((firstColumn != null) && (firstColumn.Index != columnIndex))
            {
                firstColumn = this.GetNextColumn(firstColumn, includeFilter, DataGridViewElementStates.None);
                num++;
            }
            return num;
        }

        public virtual bool Contains(string columnName)
        {
            if (columnName == null)
            {
                throw new ArgumentNullException("columnName");
            }
            int count = this.items.Count;
            for (int i = 0; i < count; i++)
            {
                DataGridViewColumn column = (DataGridViewColumn) this.items[i];
                if (string.Compare(column.Name, columnName, true, CultureInfo.InvariantCulture) == 0)
                {
                    return true;
                }
            }
            return false;
        }

        public virtual bool Contains(DataGridViewColumn dataGridViewColumn)
        {
            return (this.items.IndexOf(dataGridViewColumn) != -1);
        }

        public void CopyTo(DataGridViewColumn[] array, int index)
        {
            this.items.CopyTo(array, index);
        }

        internal bool DisplayInOrder(int columnIndex1, int columnIndex2)
        {
            int displayIndex = ((DataGridViewColumn) this.items[columnIndex1]).DisplayIndex;
            int num2 = ((DataGridViewColumn) this.items[columnIndex2]).DisplayIndex;
            return (displayIndex < num2);
        }

        internal DataGridViewColumn GetColumnAtDisplayIndex(int displayIndex)
        {
            if ((displayIndex >= 0) && (displayIndex < this.items.Count))
            {
                DataGridViewColumn column = (DataGridViewColumn) this.items[displayIndex];
                if (column.DisplayIndex == displayIndex)
                {
                    return column;
                }
                for (int i = 0; i < this.items.Count; i++)
                {
                    column = (DataGridViewColumn) this.items[i];
                    if (column.DisplayIndex == displayIndex)
                    {
                        return column;
                    }
                }
            }
            return null;
        }

        public int GetColumnCount(DataGridViewElementStates includeFilter)
        {
            if ((includeFilter & ~(DataGridViewElementStates.Visible | DataGridViewElementStates.Selected | DataGridViewElementStates.Resizable | DataGridViewElementStates.ReadOnly | DataGridViewElementStates.Frozen | DataGridViewElementStates.Displayed)) != DataGridViewElementStates.None)
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridView_InvalidDataGridViewElementStateCombination", new object[] { "includeFilter" }));
            }
            DataGridViewElementStates states2 = includeFilter;
            if (states2 != DataGridViewElementStates.Visible)
            {
                if ((states2 == (DataGridViewElementStates.Visible | DataGridViewElementStates.Selected)) && (this.columnCountsVisibleSelected != -1))
                {
                    return this.columnCountsVisibleSelected;
                }
            }
            else if (this.columnCountsVisible != -1)
            {
                return this.columnCountsVisible;
            }
            int num = 0;
            if ((includeFilter & DataGridViewElementStates.Resizable) == DataGridViewElementStates.None)
            {
                for (int j = 0; j < this.items.Count; j++)
                {
                    if (((DataGridViewColumn) this.items[j]).StateIncludes(includeFilter))
                    {
                        num++;
                    }
                }
                DataGridViewElementStates states3 = includeFilter;
                if (states3 != DataGridViewElementStates.Visible)
                {
                    if (states3 != (DataGridViewElementStates.Visible | DataGridViewElementStates.Selected))
                    {
                        return num;
                    }
                }
                else
                {
                    this.columnCountsVisible = num;
                    return num;
                }
                this.columnCountsVisibleSelected = num;
                return num;
            }
            DataGridViewElementStates elementState = includeFilter & ~DataGridViewElementStates.Resizable;
            for (int i = 0; i < this.items.Count; i++)
            {
                if (((DataGridViewColumn) this.items[i]).StateIncludes(elementState) && (((DataGridViewColumn) this.items[i]).Resizable == DataGridViewTriState.True))
                {
                    num++;
                }
            }
            return num;
        }

        internal int GetColumnCount(DataGridViewElementStates includeFilter, int fromColumnIndex, int toColumnIndex)
        {
            int num = 0;
            DataGridViewColumn dataGridViewColumnStart = (DataGridViewColumn) this.items[fromColumnIndex];
            while (dataGridViewColumnStart != ((DataGridViewColumn) this.items[toColumnIndex]))
            {
                dataGridViewColumnStart = this.GetNextColumn(dataGridViewColumnStart, includeFilter, DataGridViewElementStates.None);
                if (dataGridViewColumnStart.StateIncludes(includeFilter))
                {
                    num++;
                }
            }
            return num;
        }

        internal float GetColumnsFillWeight(DataGridViewElementStates includeFilter)
        {
            float num = 0f;
            for (int i = 0; i < this.items.Count; i++)
            {
                if (((DataGridViewColumn) this.items[i]).StateIncludes(includeFilter))
                {
                    num += ((DataGridViewColumn) this.items[i]).FillWeight;
                }
            }
            return num;
        }

        private int GetColumnSortedIndex(DataGridViewColumn dataGridViewColumn)
        {
            if ((this.lastAccessedSortedIndex != -1) && (this.itemsSorted[this.lastAccessedSortedIndex] == dataGridViewColumn))
            {
                return this.lastAccessedSortedIndex;
            }
            for (int i = 0; i < this.itemsSorted.Count; i++)
            {
                if (dataGridViewColumn.Index == ((DataGridViewColumn) this.itemsSorted[i]).Index)
                {
                    this.lastAccessedSortedIndex = i;
                    return i;
                }
            }
            return -1;
        }

        public int GetColumnsWidth(DataGridViewElementStates includeFilter)
        {
            if ((includeFilter & ~(DataGridViewElementStates.Visible | DataGridViewElementStates.Selected | DataGridViewElementStates.Resizable | DataGridViewElementStates.ReadOnly | DataGridViewElementStates.Frozen | DataGridViewElementStates.Displayed)) != DataGridViewElementStates.None)
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridView_InvalidDataGridViewElementStateCombination", new object[] { "includeFilter" }));
            }
            switch (includeFilter)
            {
                case DataGridViewElementStates.Visible:
                    if (this.columnsWidthVisible == -1)
                    {
                        break;
                    }
                    return this.columnsWidthVisible;

                case (DataGridViewElementStates.Visible | DataGridViewElementStates.Frozen):
                    if (this.columnsWidthVisibleFrozen == -1)
                    {
                        break;
                    }
                    return this.columnsWidthVisibleFrozen;
            }
            int num = 0;
            for (int i = 0; i < this.items.Count; i++)
            {
                if (((DataGridViewColumn) this.items[i]).StateIncludes(includeFilter))
                {
                    num += ((DataGridViewColumn) this.items[i]).Thickness;
                }
            }
            switch (includeFilter)
            {
                case DataGridViewElementStates.Visible:
                    this.columnsWidthVisible = num;
                    return num;

                case (DataGridViewElementStates.Visible | DataGridViewElementStates.Displayed):
                    return num;

                case (DataGridViewElementStates.Visible | DataGridViewElementStates.Frozen):
                    this.columnsWidthVisibleFrozen = num;
                    return num;
            }
            return num;
        }

        public DataGridViewColumn GetFirstColumn(DataGridViewElementStates includeFilter)
        {
            if ((includeFilter & ~(DataGridViewElementStates.Visible | DataGridViewElementStates.Selected | DataGridViewElementStates.Resizable | DataGridViewElementStates.ReadOnly | DataGridViewElementStates.Frozen | DataGridViewElementStates.Displayed)) != DataGridViewElementStates.None)
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridView_InvalidDataGridViewElementStateCombination", new object[] { "includeFilter" }));
            }
            if (this.itemsSorted == null)
            {
                this.UpdateColumnOrderCache();
            }
            for (int i = 0; i < this.itemsSorted.Count; i++)
            {
                DataGridViewColumn column = (DataGridViewColumn) this.itemsSorted[i];
                if (column.StateIncludes(includeFilter))
                {
                    this.lastAccessedSortedIndex = i;
                    return column;
                }
            }
            return null;
        }

        public DataGridViewColumn GetFirstColumn(DataGridViewElementStates includeFilter, DataGridViewElementStates excludeFilter)
        {
            if (excludeFilter == DataGridViewElementStates.None)
            {
                return this.GetFirstColumn(includeFilter);
            }
            if ((includeFilter & ~(DataGridViewElementStates.Visible | DataGridViewElementStates.Selected | DataGridViewElementStates.Resizable | DataGridViewElementStates.ReadOnly | DataGridViewElementStates.Frozen | DataGridViewElementStates.Displayed)) != DataGridViewElementStates.None)
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridView_InvalidDataGridViewElementStateCombination", new object[] { "includeFilter" }));
            }
            if ((excludeFilter & ~(DataGridViewElementStates.Visible | DataGridViewElementStates.Selected | DataGridViewElementStates.Resizable | DataGridViewElementStates.ReadOnly | DataGridViewElementStates.Frozen | DataGridViewElementStates.Displayed)) != DataGridViewElementStates.None)
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridView_InvalidDataGridViewElementStateCombination", new object[] { "excludeFilter" }));
            }
            if (this.itemsSorted == null)
            {
                this.UpdateColumnOrderCache();
            }
            for (int i = 0; i < this.itemsSorted.Count; i++)
            {
                DataGridViewColumn column = (DataGridViewColumn) this.itemsSorted[i];
                if (column.StateIncludes(includeFilter) && column.StateExcludes(excludeFilter))
                {
                    this.lastAccessedSortedIndex = i;
                    return column;
                }
            }
            return null;
        }

        public DataGridViewColumn GetLastColumn(DataGridViewElementStates includeFilter, DataGridViewElementStates excludeFilter)
        {
            if ((includeFilter & ~(DataGridViewElementStates.Visible | DataGridViewElementStates.Selected | DataGridViewElementStates.Resizable | DataGridViewElementStates.ReadOnly | DataGridViewElementStates.Frozen | DataGridViewElementStates.Displayed)) != DataGridViewElementStates.None)
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridView_InvalidDataGridViewElementStateCombination", new object[] { "includeFilter" }));
            }
            if ((excludeFilter & ~(DataGridViewElementStates.Visible | DataGridViewElementStates.Selected | DataGridViewElementStates.Resizable | DataGridViewElementStates.ReadOnly | DataGridViewElementStates.Frozen | DataGridViewElementStates.Displayed)) != DataGridViewElementStates.None)
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridView_InvalidDataGridViewElementStateCombination", new object[] { "excludeFilter" }));
            }
            if (this.itemsSorted == null)
            {
                this.UpdateColumnOrderCache();
            }
            for (int i = this.itemsSorted.Count - 1; i >= 0; i--)
            {
                DataGridViewColumn column = (DataGridViewColumn) this.itemsSorted[i];
                if (column.StateIncludes(includeFilter) && column.StateExcludes(excludeFilter))
                {
                    this.lastAccessedSortedIndex = i;
                    return column;
                }
            }
            return null;
        }

        public DataGridViewColumn GetNextColumn(DataGridViewColumn dataGridViewColumnStart, DataGridViewElementStates includeFilter, DataGridViewElementStates excludeFilter)
        {
            if (dataGridViewColumnStart == null)
            {
                throw new ArgumentNullException("dataGridViewColumnStart");
            }
            if ((includeFilter & ~(DataGridViewElementStates.Visible | DataGridViewElementStates.Selected | DataGridViewElementStates.Resizable | DataGridViewElementStates.ReadOnly | DataGridViewElementStates.Frozen | DataGridViewElementStates.Displayed)) != DataGridViewElementStates.None)
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridView_InvalidDataGridViewElementStateCombination", new object[] { "includeFilter" }));
            }
            if ((excludeFilter & ~(DataGridViewElementStates.Visible | DataGridViewElementStates.Selected | DataGridViewElementStates.Resizable | DataGridViewElementStates.ReadOnly | DataGridViewElementStates.Frozen | DataGridViewElementStates.Displayed)) != DataGridViewElementStates.None)
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridView_InvalidDataGridViewElementStateCombination", new object[] { "excludeFilter" }));
            }
            if (this.itemsSorted == null)
            {
                this.UpdateColumnOrderCache();
            }
            int columnSortedIndex = this.GetColumnSortedIndex(dataGridViewColumnStart);
            if (columnSortedIndex == -1)
            {
                bool flag = false;
                int num2 = 0x7fffffff;
                int displayIndex = 0x7fffffff;
                columnSortedIndex = 0;
                while (columnSortedIndex < this.items.Count)
                {
                    DataGridViewColumn column = (DataGridViewColumn) this.items[columnSortedIndex];
                    if (((column.StateIncludes(includeFilter) && column.StateExcludes(excludeFilter)) && ((column.DisplayIndex > dataGridViewColumnStart.DisplayIndex) || ((column.DisplayIndex == dataGridViewColumnStart.DisplayIndex) && (column.Index > dataGridViewColumnStart.Index)))) && ((column.DisplayIndex < displayIndex) || ((column.DisplayIndex == displayIndex) && (column.Index < num2))))
                    {
                        num2 = columnSortedIndex;
                        displayIndex = column.DisplayIndex;
                        flag = true;
                    }
                    columnSortedIndex++;
                }
                if (!flag)
                {
                    return null;
                }
                return (DataGridViewColumn) this.items[num2];
            }
            columnSortedIndex++;
            while (columnSortedIndex < this.itemsSorted.Count)
            {
                DataGridViewColumn column2 = (DataGridViewColumn) this.itemsSorted[columnSortedIndex];
                if (column2.StateIncludes(includeFilter) && column2.StateExcludes(excludeFilter))
                {
                    this.lastAccessedSortedIndex = columnSortedIndex;
                    return column2;
                }
                columnSortedIndex++;
            }
            return null;
        }

        public DataGridViewColumn GetPreviousColumn(DataGridViewColumn dataGridViewColumnStart, DataGridViewElementStates includeFilter, DataGridViewElementStates excludeFilter)
        {
            if (dataGridViewColumnStart == null)
            {
                throw new ArgumentNullException("dataGridViewColumnStart");
            }
            if ((includeFilter & ~(DataGridViewElementStates.Visible | DataGridViewElementStates.Selected | DataGridViewElementStates.Resizable | DataGridViewElementStates.ReadOnly | DataGridViewElementStates.Frozen | DataGridViewElementStates.Displayed)) != DataGridViewElementStates.None)
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridView_InvalidDataGridViewElementStateCombination", new object[] { "includeFilter" }));
            }
            if ((excludeFilter & ~(DataGridViewElementStates.Visible | DataGridViewElementStates.Selected | DataGridViewElementStates.Resizable | DataGridViewElementStates.ReadOnly | DataGridViewElementStates.Frozen | DataGridViewElementStates.Displayed)) != DataGridViewElementStates.None)
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridView_InvalidDataGridViewElementStateCombination", new object[] { "excludeFilter" }));
            }
            if (this.itemsSorted == null)
            {
                this.UpdateColumnOrderCache();
            }
            int columnSortedIndex = this.GetColumnSortedIndex(dataGridViewColumnStart);
            if (columnSortedIndex == -1)
            {
                bool flag = false;
                int num2 = -1;
                int displayIndex = -1;
                columnSortedIndex = 0;
                while (columnSortedIndex < this.items.Count)
                {
                    DataGridViewColumn column = (DataGridViewColumn) this.items[columnSortedIndex];
                    if (((column.StateIncludes(includeFilter) && column.StateExcludes(excludeFilter)) && ((column.DisplayIndex < dataGridViewColumnStart.DisplayIndex) || ((column.DisplayIndex == dataGridViewColumnStart.DisplayIndex) && (column.Index < dataGridViewColumnStart.Index)))) && ((column.DisplayIndex > displayIndex) || ((column.DisplayIndex == displayIndex) && (column.Index > num2))))
                    {
                        num2 = columnSortedIndex;
                        displayIndex = column.DisplayIndex;
                        flag = true;
                    }
                    columnSortedIndex++;
                }
                if (!flag)
                {
                    return null;
                }
                return (DataGridViewColumn) this.items[num2];
            }
            columnSortedIndex--;
            while (columnSortedIndex >= 0)
            {
                DataGridViewColumn column2 = (DataGridViewColumn) this.itemsSorted[columnSortedIndex];
                if (column2.StateIncludes(includeFilter) && column2.StateExcludes(excludeFilter))
                {
                    this.lastAccessedSortedIndex = columnSortedIndex;
                    return column2;
                }
                columnSortedIndex--;
            }
            return null;
        }

        public int IndexOf(DataGridViewColumn dataGridViewColumn)
        {
            return this.items.IndexOf(dataGridViewColumn);
        }

        public virtual void Insert(int columnIndex, DataGridViewColumn dataGridViewColumn)
        {
            Point point;
            if (this.DataGridView.NoDimensionChangeAllowed)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridView_ForbiddenOperationInEventHandler"));
            }
            if (this.DataGridView.InDisplayIndexAdjustments)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridView_CannotAlterDisplayIndexWithinAdjustments"));
            }
            if (dataGridViewColumn == null)
            {
                throw new ArgumentNullException("dataGridViewColumn");
            }
            int displayIndex = dataGridViewColumn.DisplayIndex;
            if (displayIndex == -1)
            {
                dataGridViewColumn.DisplayIndex = columnIndex;
            }
            try
            {
                this.DataGridView.OnInsertingColumn(columnIndex, dataGridViewColumn, out point);
            }
            finally
            {
                dataGridViewColumn.DisplayIndexInternal = displayIndex;
            }
            this.InvalidateCachedColumnsOrder();
            this.items.Insert(columnIndex, dataGridViewColumn);
            dataGridViewColumn.IndexInternal = columnIndex;
            dataGridViewColumn.DataGridViewInternal = this.dataGridView;
            this.UpdateColumnCaches(dataGridViewColumn, true);
            this.DataGridView.OnInsertedColumn_PreNotification(dataGridViewColumn);
            this.OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Add, dataGridViewColumn), true, point);
        }

        internal void InvalidateCachedColumnCount(DataGridViewElementStates includeFilter)
        {
            if (includeFilter == DataGridViewElementStates.Visible)
            {
                this.InvalidateCachedColumnCounts();
            }
            else if (includeFilter == DataGridViewElementStates.Selected)
            {
                this.columnCountsVisibleSelected = -1;
            }
        }

        internal void InvalidateCachedColumnCounts()
        {
            this.columnCountsVisible = this.columnCountsVisibleSelected = -1;
        }

        internal void InvalidateCachedColumnsOrder()
        {
            this.itemsSorted = null;
        }

        internal void InvalidateCachedColumnsWidth(DataGridViewElementStates includeFilter)
        {
            if (includeFilter == DataGridViewElementStates.Visible)
            {
                this.InvalidateCachedColumnsWidths();
            }
            else if (includeFilter == DataGridViewElementStates.Frozen)
            {
                this.columnsWidthVisibleFrozen = -1;
            }
        }

        internal void InvalidateCachedColumnsWidths()
        {
            this.columnsWidthVisible = this.columnsWidthVisibleFrozen = -1;
        }

        protected virtual void OnCollectionChanged(CollectionChangeEventArgs e)
        {
            if (this.onCollectionChanged != null)
            {
                this.onCollectionChanged(this, e);
            }
        }

        private void OnCollectionChanged(CollectionChangeEventArgs ccea, bool changeIsInsertion, Point newCurrentCell)
        {
            this.OnCollectionChanged_PreNotification(ccea);
            this.OnCollectionChanged(ccea);
            this.OnCollectionChanged_PostNotification(ccea, changeIsInsertion, newCurrentCell);
        }

        private void OnCollectionChanged_PostNotification(CollectionChangeEventArgs ccea, bool changeIsInsertion, Point newCurrentCell)
        {
            DataGridViewColumn element = (DataGridViewColumn) ccea.Element;
            if ((ccea.Action == CollectionChangeAction.Add) && changeIsInsertion)
            {
                this.DataGridView.OnInsertedColumn_PostNotification(newCurrentCell);
            }
            else if (ccea.Action == CollectionChangeAction.Remove)
            {
                this.DataGridView.OnRemovedColumn_PostNotification(element, newCurrentCell);
            }
            this.DataGridView.OnColumnCollectionChanged_PostNotification(element);
        }

        private void OnCollectionChanged_PreNotification(CollectionChangeEventArgs ccea)
        {
            this.DataGridView.OnColumnCollectionChanged_PreNotification(ccea);
        }

        public virtual void Remove(string columnName)
        {
            if (columnName == null)
            {
                throw new ArgumentNullException("columnName");
            }
            int count = this.items.Count;
            for (int i = 0; i < count; i++)
            {
                DataGridViewColumn column = (DataGridViewColumn) this.items[i];
                if (string.Compare(column.Name, columnName, true, CultureInfo.InvariantCulture) == 0)
                {
                    this.RemoveAt(i);
                    return;
                }
            }
            throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridViewColumnCollection_ColumnNotFound", new object[] { columnName }), "columnName");
        }

        public virtual void Remove(DataGridViewColumn dataGridViewColumn)
        {
            if (dataGridViewColumn == null)
            {
                throw new ArgumentNullException("dataGridViewColumn");
            }
            if (dataGridViewColumn.DataGridView != this.DataGridView)
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridView_ColumnDoesNotBelongToDataGridView"), "dataGridViewColumn");
            }
            int count = this.items.Count;
            for (int i = 0; i < count; i++)
            {
                if (this.items[i] == dataGridViewColumn)
                {
                    this.RemoveAt(i);
                    return;
                }
            }
        }

        public virtual void RemoveAt(int index)
        {
            if ((index < 0) || (index >= this.Count))
            {
                throw new ArgumentOutOfRangeException("index", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "index", index.ToString(CultureInfo.CurrentCulture) }));
            }
            if (this.DataGridView.NoDimensionChangeAllowed)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridView_ForbiddenOperationInEventHandler"));
            }
            if (this.DataGridView.InDisplayIndexAdjustments)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridView_CannotAlterDisplayIndexWithinAdjustments"));
            }
            this.RemoveAtInternal(index, false);
        }

        internal void RemoveAtInternal(int index, bool force)
        {
            Point point;
            DataGridViewColumn dataGridViewColumn = (DataGridViewColumn) this.items[index];
            this.DataGridView.OnRemovingColumn(dataGridViewColumn, out point, force);
            this.InvalidateCachedColumnsOrder();
            this.items.RemoveAt(index);
            dataGridViewColumn.DataGridViewInternal = null;
            this.UpdateColumnCaches(dataGridViewColumn, false);
            this.DataGridView.OnRemovedColumn_PreNotification(dataGridViewColumn);
            this.OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Remove, dataGridViewColumn), false, point);
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
            return this.Add((DataGridViewColumn) value);
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
            this.Insert(index, (DataGridViewColumn) value);
        }

        void IList.Remove(object value)
        {
            this.Remove((DataGridViewColumn) value);
        }

        void IList.RemoveAt(int index)
        {
            this.RemoveAt(index);
        }

        private void UpdateColumnCaches(DataGridViewColumn dataGridViewColumn, bool adding)
        {
            if (((this.columnCountsVisible != -1) || (this.columnCountsVisibleSelected != -1)) || ((this.columnsWidthVisible != -1) || (this.columnsWidthVisibleFrozen != -1)))
            {
                DataGridViewElementStates state = dataGridViewColumn.State;
                if ((state & DataGridViewElementStates.Visible) != DataGridViewElementStates.None)
                {
                    int num = adding ? 1 : -1;
                    int num2 = 0;
                    if ((this.columnsWidthVisible != -1) || ((this.columnsWidthVisibleFrozen != -1) && ((state & (DataGridViewElementStates.Visible | DataGridViewElementStates.Frozen)) == (DataGridViewElementStates.Visible | DataGridViewElementStates.Frozen))))
                    {
                        num2 = adding ? dataGridViewColumn.Width : -dataGridViewColumn.Width;
                    }
                    if (this.columnCountsVisible != -1)
                    {
                        this.columnCountsVisible += num;
                    }
                    if (this.columnsWidthVisible != -1)
                    {
                        this.columnsWidthVisible += num2;
                    }
                    if (((state & (DataGridViewElementStates.Visible | DataGridViewElementStates.Frozen)) == (DataGridViewElementStates.Visible | DataGridViewElementStates.Frozen)) && (this.columnsWidthVisibleFrozen != -1))
                    {
                        this.columnsWidthVisibleFrozen += num2;
                    }
                    if (((state & (DataGridViewElementStates.Visible | DataGridViewElementStates.Selected)) == (DataGridViewElementStates.Visible | DataGridViewElementStates.Selected)) && (this.columnCountsVisibleSelected != -1))
                    {
                        this.columnCountsVisibleSelected += num;
                    }
                }
            }
        }

        private void UpdateColumnOrderCache()
        {
            this.itemsSorted = (ArrayList) this.items.Clone();
            this.itemsSorted.Sort(columnOrderComparer);
            this.lastAccessedSortedIndex = -1;
        }

        internal static IComparer ColumnCollectionOrderComparer
        {
            get
            {
                return columnOrderComparer;
            }
        }

        protected System.Windows.Forms.DataGridView DataGridView
        {
            get
            {
                return this.dataGridView;
            }
        }

        public DataGridViewColumn this[int index]
        {
            get
            {
                return (DataGridViewColumn) this.items[index];
            }
        }

        public DataGridViewColumn this[string columnName]
        {
            get
            {
                if (columnName == null)
                {
                    throw new ArgumentNullException("columnName");
                }
                int count = this.items.Count;
                for (int i = 0; i < count; i++)
                {
                    DataGridViewColumn column = (DataGridViewColumn) this.items[i];
                    if (string.Equals(column.Name, columnName, StringComparison.OrdinalIgnoreCase))
                    {
                        return column;
                    }
                }
                return null;
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
                throw new NotSupportedException();
            }
        }

        private class ColumnOrderComparer : IComparer
        {
            public int Compare(object x, object y)
            {
                DataGridViewColumn column = x as DataGridViewColumn;
                DataGridViewColumn column2 = y as DataGridViewColumn;
                return (column.DisplayIndex - column2.DisplayIndex);
            }
        }
    }
}

