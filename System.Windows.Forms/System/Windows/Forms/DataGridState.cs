namespace System.Windows.Forms
{
    using System;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Text;

    internal sealed class DataGridState : ICloneable
    {
        public int CurrentCol;
        public int CurrentRow;
        public System.Windows.Forms.DataGrid DataGrid;
        public DataGridRow[] DataGridRows;
        public int DataGridRowsLength;
        public string DataMember;
        public object DataSource;
        public int FirstVisibleCol;
        public int FirstVisibleRow;
        public GridColumnStylesCollection GridColumnStyles;
        public DataGridRow LinkingRow;
        public CurrencyManager ListManager;
        private AccessibleObject parentRowAccessibleObject;

        public DataGridState()
        {
            this.DataGridRows = new DataGridRow[0];
        }

        public DataGridState(System.Windows.Forms.DataGrid dataGrid)
        {
            this.DataGridRows = new DataGridRow[0];
            this.PushState(dataGrid);
        }

        public object Clone()
        {
            return new DataGridState { DataGridRows = this.DataGridRows, DataSource = this.DataSource, DataMember = this.DataMember, FirstVisibleRow = this.FirstVisibleRow, FirstVisibleCol = this.FirstVisibleCol, CurrentRow = this.CurrentRow, CurrentCol = this.CurrentCol, GridColumnStyles = this.GridColumnStyles, ListManager = this.ListManager, DataGrid = this.DataGrid };
        }

        private void DataSource_Changed(object sender, ItemChangedEventArgs e)
        {
            if ((this.DataGrid != null) && (this.ListManager.Position == e.Index))
            {
                this.DataGrid.InvalidateParentRows();
            }
            else if (this.DataGrid != null)
            {
                this.DataGrid.ParentRowsDataChanged();
            }
        }

        private void DataSource_MetaDataChanged(object sender, EventArgs e)
        {
            if (this.DataGrid != null)
            {
                this.DataGrid.ParentRowsDataChanged();
            }
        }

        public void PullState(System.Windows.Forms.DataGrid dataGrid, bool createColumn)
        {
            dataGrid.Set_ListManager(this.DataSource, this.DataMember, true, createColumn);
            dataGrid.firstVisibleRow = this.FirstVisibleRow;
            dataGrid.firstVisibleCol = this.FirstVisibleCol;
            dataGrid.currentRow = this.CurrentRow;
            dataGrid.currentCol = this.CurrentCol;
            dataGrid.SetDataGridRows(this.DataGridRows, this.DataGridRowsLength);
        }

        public void PushState(System.Windows.Forms.DataGrid dataGrid)
        {
            this.DataSource = dataGrid.DataSource;
            this.DataMember = dataGrid.DataMember;
            this.DataGrid = dataGrid;
            this.DataGridRows = dataGrid.DataGridRows;
            this.DataGridRowsLength = dataGrid.DataGridRowsLength;
            this.FirstVisibleRow = dataGrid.firstVisibleRow;
            this.FirstVisibleCol = dataGrid.firstVisibleCol;
            this.CurrentRow = dataGrid.currentRow;
            this.GridColumnStyles = new GridColumnStylesCollection(dataGrid.myGridTable);
            this.GridColumnStyles.Clear();
            foreach (DataGridColumnStyle style in dataGrid.myGridTable.GridColumnStyles)
            {
                this.GridColumnStyles.Add(style);
            }
            this.ListManager = dataGrid.ListManager;
            this.ListManager.ItemChanged += new ItemChangedEventHandler(this.DataSource_Changed);
            this.ListManager.MetaDataChanged += new EventHandler(this.DataSource_MetaDataChanged);
            this.CurrentCol = dataGrid.currentCol;
        }

        public void RemoveChangeNotification()
        {
            this.ListManager.ItemChanged -= new ItemChangedEventHandler(this.DataSource_Changed);
            this.ListManager.MetaDataChanged -= new EventHandler(this.DataSource_MetaDataChanged);
        }

        internal AccessibleObject ParentRowAccessibleObject
        {
            get
            {
                if (this.parentRowAccessibleObject == null)
                {
                    this.parentRowAccessibleObject = new DataGridStateParentRowAccessibleObject(this);
                }
                return this.parentRowAccessibleObject;
            }
        }

        [ComVisible(true)]
        internal class DataGridStateParentRowAccessibleObject : AccessibleObject
        {
            private DataGridState owner;

            public DataGridStateParentRowAccessibleObject(DataGridState owner)
            {
                this.owner = owner;
            }

            [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            public override AccessibleObject Navigate(AccessibleNavigation navdir)
            {
                DataGridParentRows.DataGridParentRowsAccessibleObject parent = (DataGridParentRows.DataGridParentRowsAccessibleObject) this.Parent;
                switch (navdir)
                {
                    case AccessibleNavigation.Up:
                    case AccessibleNavigation.Left:
                    case AccessibleNavigation.Previous:
                        return parent.GetPrev(this);

                    case AccessibleNavigation.Down:
                    case AccessibleNavigation.Right:
                    case AccessibleNavigation.Next:
                        return parent.GetNext(this);
                }
                return null;
            }

            public override Rectangle Bounds
            {
                get
                {
                    DataGridParentRows owner = ((DataGridParentRows.DataGridParentRowsAccessibleObject) this.Parent).Owner;
                    DataGrid dataGrid = this.owner.LinkingRow.DataGrid;
                    Rectangle boundsForDataGridStateAccesibility = owner.GetBoundsForDataGridStateAccesibility(this.owner);
                    boundsForDataGridStateAccesibility.Y += dataGrid.ParentRowsBounds.Y;
                    return dataGrid.RectangleToScreen(boundsForDataGridStateAccesibility);
                }
            }

            public override string Name
            {
                get
                {
                    return System.Windows.Forms.SR.GetString("AccDGParentRow");
                }
            }

            public override AccessibleObject Parent
            {
                [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
                get
                {
                    return this.owner.LinkingRow.DataGrid.ParentRowsAccessibleObject;
                }
            }

            public override AccessibleRole Role
            {
                get
                {
                    return AccessibleRole.ListItem;
                }
            }

            public override string Value
            {
                [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
                get
                {
                    StringBuilder builder = new StringBuilder();
                    CurrencyManager manager = (CurrencyManager) this.owner.LinkingRow.DataGrid.BindingContext[this.owner.DataSource, this.owner.DataMember];
                    builder.Append(this.owner.ListManager.GetListName());
                    builder.Append(": ");
                    bool flag = false;
                    foreach (DataGridColumnStyle style in this.owner.GridColumnStyles)
                    {
                        if (flag)
                        {
                            builder.Append(", ");
                        }
                        string headerText = style.HeaderText;
                        string str2 = style.PropertyDescriptor.Converter.ConvertToString(style.PropertyDescriptor.GetValue(manager.Current));
                        builder.Append(headerText);
                        builder.Append(": ");
                        builder.Append(str2);
                        flag = true;
                    }
                    return builder.ToString();
                }
            }
        }
    }
}

