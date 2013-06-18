namespace System.Web.UI.WebControls
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Reflection;
    using System.Web;
    using System.Web.UI;
    using System.Web.Util;

    [Designer("System.Web.UI.Design.WebControls.DataGridDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), Editor("System.Web.UI.Design.WebControls.DataGridComponentEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(ComponentEditor))]
    public class DataGrid : BaseDataList, INamingContainer
    {
        private TableItemStyle alternatingItemStyle;
        private ArrayList autoGenColumnsArray;
        public const string CancelCommandName = "Cancel";
        private DataGridColumnCollection columnCollection;
        private ArrayList columns;
        internal const string DataSourceItemCountViewStateKey = "_!DataSourceItemCount";
        public const string DeleteCommandName = "Delete";
        public const string EditCommandName = "Edit";
        private TableItemStyle editItemStyle;
        private static readonly object EventCancelCommand = new object();
        private static readonly object EventDeleteCommand = new object();
        private static readonly object EventEditCommand = new object();
        private static readonly object EventItemCommand = new object();
        private static readonly object EventItemCreated = new object();
        private static readonly object EventItemDataBound = new object();
        private static readonly object EventPageIndexChanged = new object();
        private static readonly object EventSortCommand = new object();
        private static readonly object EventUpdateCommand = new object();
        private object firstDataItem;
        private TableItemStyle footerStyle;
        private TableItemStyle headerStyle;
        private ArrayList itemsArray;
        private DataGridItemCollection itemsCollection;
        private TableItemStyle itemStyle;
        public const string NextPageCommandArgument = "Next";
        public const string PageCommandName = "Page";
        private PagedDataSource pagedDataSource;
        private DataGridPagerStyle pagerStyle;
        public const string PrevPageCommandArgument = "Prev";
        public const string SelectCommandName = "Select";
        private TableItemStyle selectedItemStyle;
        public const string SortCommandName = "Sort";
        private IEnumerator storedData;
        private bool storedDataValid;
        public const string UpdateCommandName = "Update";

        [WebCategory("Action"), WebSysDescription("DataGrid_OnCancelCommand")]
        public event DataGridCommandEventHandler CancelCommand
        {
            add
            {
                base.Events.AddHandler(EventCancelCommand, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventCancelCommand, value);
            }
        }

        [WebCategory("Action"), WebSysDescription("DataGrid_OnDeleteCommand")]
        public event DataGridCommandEventHandler DeleteCommand
        {
            add
            {
                base.Events.AddHandler(EventDeleteCommand, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventDeleteCommand, value);
            }
        }

        [WebSysDescription("DataGrid_OnEditCommand"), WebCategory("Action")]
        public event DataGridCommandEventHandler EditCommand
        {
            add
            {
                base.Events.AddHandler(EventEditCommand, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventEditCommand, value);
            }
        }

        [WebCategory("Action"), WebSysDescription("DataGrid_OnItemCommand")]
        public event DataGridCommandEventHandler ItemCommand
        {
            add
            {
                base.Events.AddHandler(EventItemCommand, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventItemCommand, value);
            }
        }

        [WebSysDescription("DataControls_OnItemCreated"), WebCategory("Behavior")]
        public event DataGridItemEventHandler ItemCreated
        {
            add
            {
                base.Events.AddHandler(EventItemCreated, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventItemCreated, value);
            }
        }

        [WebCategory("Behavior"), WebSysDescription("DataControls_OnItemDataBound")]
        public event DataGridItemEventHandler ItemDataBound
        {
            add
            {
                base.Events.AddHandler(EventItemDataBound, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventItemDataBound, value);
            }
        }

        [WebCategory("Action"), WebSysDescription("DataGrid_OnPageIndexChanged")]
        public event DataGridPageChangedEventHandler PageIndexChanged
        {
            add
            {
                base.Events.AddHandler(EventPageIndexChanged, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventPageIndexChanged, value);
            }
        }

        [WebSysDescription("DataGrid_OnSortCommand"), WebCategory("Action")]
        public event DataGridSortCommandEventHandler SortCommand
        {
            add
            {
                base.Events.AddHandler(EventSortCommand, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventSortCommand, value);
            }
        }

        [WebSysDescription("DataGrid_OnUpdateCommand"), WebCategory("Action")]
        public event DataGridCommandEventHandler UpdateCommand
        {
            add
            {
                base.Events.AddHandler(EventUpdateCommand, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventUpdateCommand, value);
            }
        }

        private ArrayList CreateAutoGeneratedColumns(PagedDataSource dataSource)
        {
            if (dataSource == null)
            {
                return null;
            }
            ArrayList list = new ArrayList();
            PropertyDescriptorCollection itemProperties = null;
            bool flag = true;
            itemProperties = dataSource.GetItemProperties(new PropertyDescriptor[0]);
            if (itemProperties == null)
            {
                Type propertyType = null;
                object firstDataItem = null;
                PropertyInfo info = dataSource.DataSource.GetType().GetProperty("Item", BindingFlags.Public | BindingFlags.Instance, null, null, new Type[] { typeof(int) }, null);
                if (info != null)
                {
                    propertyType = info.PropertyType;
                }
                if ((propertyType == null) || (propertyType == typeof(object)))
                {
                    IEnumerator enumerator = dataSource.GetEnumerator();
                    if (enumerator.MoveNext())
                    {
                        firstDataItem = enumerator.Current;
                    }
                    else
                    {
                        flag = false;
                    }
                    if (firstDataItem != null)
                    {
                        propertyType = firstDataItem.GetType();
                    }
                    this.StoreEnumerator(enumerator, firstDataItem);
                }
                if ((firstDataItem != null) && (firstDataItem is ICustomTypeDescriptor))
                {
                    itemProperties = TypeDescriptor.GetProperties(firstDataItem);
                }
                else if (propertyType != null)
                {
                    if (BaseDataList.IsBindableType(propertyType))
                    {
                        BoundColumn column = new BoundColumn();
                        ((IStateManager) column).TrackViewState();
                        column.HeaderText = "Item";
                        column.DataField = BoundColumn.thisExpr;
                        column.SortExpression = "Item";
                        column.SetOwner(this);
                        list.Add(column);
                    }
                    else
                    {
                        itemProperties = TypeDescriptor.GetProperties(propertyType);
                    }
                }
            }
            if ((itemProperties != null) && (itemProperties.Count != 0))
            {
                foreach (PropertyDescriptor descriptor in itemProperties)
                {
                    if (BaseDataList.IsBindableType(descriptor.PropertyType))
                    {
                        BoundColumn column2 = new BoundColumn();
                        ((IStateManager) column2).TrackViewState();
                        column2.HeaderText = descriptor.Name;
                        column2.DataField = descriptor.Name;
                        column2.SortExpression = descriptor.Name;
                        column2.ReadOnly = descriptor.IsReadOnly;
                        column2.SetOwner(this);
                        list.Add(column2);
                    }
                }
            }
            if ((list.Count == 0) && flag)
            {
                throw new HttpException(System.Web.SR.GetString("DataGrid_NoAutoGenColumns", new object[] { this.ID }));
            }
            return list;
        }

        protected virtual ArrayList CreateColumnSet(PagedDataSource dataSource, bool useDataSource)
        {
            int num;
            ArrayList list = new ArrayList();
            DataGridColumn[] array = new DataGridColumn[this.Columns.Count];
            this.Columns.CopyTo(array, 0);
            for (num = 0; num < array.Length; num++)
            {
                list.Add(array[num]);
            }
            if (this.AutoGenerateColumns)
            {
                ArrayList autoGenColumnsArray = null;
                if (useDataSource)
                {
                    autoGenColumnsArray = this.CreateAutoGeneratedColumns(dataSource);
                    this.autoGenColumnsArray = autoGenColumnsArray;
                }
                else
                {
                    autoGenColumnsArray = this.autoGenColumnsArray;
                }
                if (autoGenColumnsArray == null)
                {
                    return list;
                }
                int count = autoGenColumnsArray.Count;
                for (num = 0; num < count; num++)
                {
                    list.Add(autoGenColumnsArray[num]);
                }
            }
            return list;
        }

        protected override void CreateControlHierarchy(bool useDataSource)
        {
            this.pagedDataSource = this.CreatePagedDataSource();
            IEnumerator storedData = null;
            int dataItemCount = -1;
            int num2 = -1;
            ArrayList dataKeysArray = base.DataKeysArray;
            ArrayList list2 = null;
            if (this.itemsArray != null)
            {
                this.itemsArray.Clear();
            }
            else
            {
                this.itemsArray = new ArrayList();
            }
            this.itemsCollection = null;
            if (!useDataSource)
            {
                dataItemCount = (int) this.ViewState["_!ItemCount"];
                num2 = (int) this.ViewState["_!DataSourceItemCount"];
                if (dataItemCount != -1)
                {
                    if (this.pagedDataSource.IsCustomPagingEnabled)
                    {
                        this.pagedDataSource.DataSource = new DummyDataSource(dataItemCount);
                    }
                    else
                    {
                        this.pagedDataSource.DataSource = new DummyDataSource(num2);
                    }
                    storedData = this.pagedDataSource.GetEnumerator();
                    list2 = this.CreateColumnSet(null, false);
                    this.itemsArray.Capacity = dataItemCount;
                }
            }
            else
            {
                dataKeysArray.Clear();
                IEnumerable data = this.GetData();
                if (data != null)
                {
                    ICollection is2 = data as ICollection;
                    if (((is2 == null) && this.pagedDataSource.IsPagingEnabled) && !this.pagedDataSource.IsCustomPagingEnabled)
                    {
                        throw new HttpException(System.Web.SR.GetString("DataGrid_Missing_VirtualItemCount", new object[] { this.ID }));
                    }
                    this.pagedDataSource.DataSource = data;
                    if (this.pagedDataSource.IsPagingEnabled && ((this.pagedDataSource.CurrentPageIndex < 0) || (this.pagedDataSource.CurrentPageIndex >= this.pagedDataSource.PageCount)))
                    {
                        throw new HttpException(System.Web.SR.GetString("Invalid_CurrentPageIndex"));
                    }
                    list2 = this.CreateColumnSet(this.pagedDataSource, useDataSource);
                    if (this.storedDataValid)
                    {
                        storedData = this.storedData;
                    }
                    else
                    {
                        storedData = this.pagedDataSource.GetEnumerator();
                    }
                    if (is2 != null)
                    {
                        int count = this.pagedDataSource.Count;
                        dataKeysArray.Capacity = count;
                        this.itemsArray.Capacity = count;
                    }
                }
            }
            int num4 = 0;
            if (list2 != null)
            {
                num4 = list2.Count;
            }
            if (num4 > 0)
            {
                DataGridItem item;
                ListItemType editItem;
                DataGridColumn[] array = new DataGridColumn[num4];
                list2.CopyTo(array, 0);
                Table child = new ChildTable(string.IsNullOrEmpty(this.ID) ? null : this.ClientID);
                this.Controls.Add(child);
                for (int i = 0; i < array.Length; i++)
                {
                    array[i].Initialize();
                }
                TableRowCollection rows = child.Rows;
                int itemIndex = 0;
                int dataSourceIndex = 0;
                string dataKeyField = this.DataKeyField;
                bool flag = useDataSource && (dataKeyField.Length != 0);
                bool isPagingEnabled = this.pagedDataSource.IsPagingEnabled;
                int editItemIndex = this.EditItemIndex;
                int selectedIndex = this.SelectedIndex;
                if (this.pagedDataSource.IsPagingEnabled)
                {
                    dataSourceIndex = this.pagedDataSource.FirstIndexInPage;
                }
                dataItemCount = 0;
                if (isPagingEnabled)
                {
                    this.CreateItem(-1, -1, ListItemType.Pager, false, null, array, rows, this.pagedDataSource);
                }
                this.CreateItem(-1, -1, ListItemType.Header, useDataSource, null, array, rows, null);
                if (this.storedDataValid && (this.firstDataItem != null))
                {
                    if (flag)
                    {
                        object propertyValue = DataBinder.GetPropertyValue(this.firstDataItem, dataKeyField);
                        dataKeysArray.Add(propertyValue);
                    }
                    editItem = ListItemType.Item;
                    if (itemIndex == editItemIndex)
                    {
                        editItem = ListItemType.EditItem;
                    }
                    else if (itemIndex == selectedIndex)
                    {
                        editItem = ListItemType.SelectedItem;
                    }
                    item = this.CreateItem(0, dataSourceIndex, editItem, useDataSource, this.firstDataItem, array, rows, null);
                    this.itemsArray.Add(item);
                    dataItemCount++;
                    itemIndex++;
                    dataSourceIndex++;
                    this.storedDataValid = false;
                    this.firstDataItem = null;
                }
                while (storedData.MoveNext())
                {
                    object current = storedData.Current;
                    if (flag)
                    {
                        object obj4 = DataBinder.GetPropertyValue(current, dataKeyField);
                        dataKeysArray.Add(obj4);
                    }
                    editItem = ListItemType.Item;
                    if (itemIndex == editItemIndex)
                    {
                        editItem = ListItemType.EditItem;
                    }
                    else if (itemIndex == selectedIndex)
                    {
                        editItem = ListItemType.SelectedItem;
                    }
                    else if ((itemIndex % 2) != 0)
                    {
                        editItem = ListItemType.AlternatingItem;
                    }
                    item = this.CreateItem(itemIndex, dataSourceIndex, editItem, useDataSource, current, array, rows, null);
                    this.itemsArray.Add(item);
                    dataItemCount++;
                    dataSourceIndex++;
                    itemIndex++;
                }
                this.CreateItem(-1, -1, ListItemType.Footer, useDataSource, null, array, rows, null);
                if (isPagingEnabled)
                {
                    this.CreateItem(-1, -1, ListItemType.Pager, false, null, array, rows, this.pagedDataSource);
                }
            }
            if (useDataSource)
            {
                if (storedData != null)
                {
                    this.ViewState["_!ItemCount"] = dataItemCount;
                    if (this.pagedDataSource.IsPagingEnabled)
                    {
                        this.ViewState["PageCount"] = this.pagedDataSource.PageCount;
                        this.ViewState["_!DataSourceItemCount"] = this.pagedDataSource.DataSourceCount;
                    }
                    else
                    {
                        this.ViewState["PageCount"] = 1;
                        this.ViewState["_!DataSourceItemCount"] = dataItemCount;
                    }
                }
                else
                {
                    this.ViewState["_!ItemCount"] = -1;
                    this.ViewState["_!DataSourceItemCount"] = -1;
                    this.ViewState["PageCount"] = 0;
                }
            }
            this.pagedDataSource = null;
        }

        protected override Style CreateControlStyle()
        {
            return new TableStyle { GridLines = GridLines.Both, CellSpacing = 0 };
        }

        protected virtual DataGridItem CreateItem(int itemIndex, int dataSourceIndex, ListItemType itemType)
        {
            return new DataGridItem(itemIndex, dataSourceIndex, itemType);
        }

        private DataGridItem CreateItem(int itemIndex, int dataSourceIndex, ListItemType itemType, bool dataBind, object dataItem, DataGridColumn[] columns, TableRowCollection rows, PagedDataSource pagedDataSource)
        {
            DataGridItem item = this.CreateItem(itemIndex, dataSourceIndex, itemType);
            DataGridItemEventArgs e = new DataGridItemEventArgs(item);
            if (itemType != ListItemType.Pager)
            {
                this.InitializeItem(item, columns);
                if (dataBind)
                {
                    item.DataItem = dataItem;
                }
                this.OnItemCreated(e);
                rows.Add(item);
                if (dataBind)
                {
                    item.DataBind();
                    this.OnItemDataBound(e);
                    item.DataItem = null;
                }
                return item;
            }
            this.InitializePager(item, columns.Length, pagedDataSource);
            this.OnItemCreated(e);
            rows.Add(item);
            return item;
        }

        private PagedDataSource CreatePagedDataSource()
        {
            return new PagedDataSource { CurrentPageIndex = this.CurrentPageIndex, PageSize = this.PageSize, AllowPaging = this.AllowPaging, AllowCustomPaging = this.AllowCustomPaging, VirtualCount = this.VirtualItemCount };
        }

        protected virtual void InitializeItem(DataGridItem item, DataGridColumn[] columns)
        {
            TableCellCollection cells = item.Cells;
            for (int i = 0; i < columns.Length; i++)
            {
                TableCell cell;
                if ((item.ItemType == ListItemType.Header) && this.UseAccessibleHeader)
                {
                    cell = new TableHeaderCell();
                    cell.Attributes["scope"] = "col";
                }
                else
                {
                    cell = new TableCell();
                }
                columns[i].InitializeCell(cell, i, item.ItemType);
                cells.Add(cell);
            }
        }

        protected virtual void InitializePager(DataGridItem item, int columnSpan, PagedDataSource pagedDataSource)
        {
            TableCell cell = new TableCell();
            if (columnSpan > 1)
            {
                cell.ColumnSpan = columnSpan;
            }
            DataGridPagerStyle pagerStyle = this.PagerStyle;
            if (pagerStyle.Mode == PagerMode.NextPrev)
            {
                if (!pagedDataSource.IsFirstPage)
                {
                    LinkButton child = new DataGridLinkButton {
                        Text = pagerStyle.PrevPageText,
                        CommandName = "Page",
                        CommandArgument = "Prev",
                        CausesValidation = false
                    };
                    cell.Controls.Add(child);
                }
                else
                {
                    Label label = new Label {
                        Text = pagerStyle.PrevPageText
                    };
                    cell.Controls.Add(label);
                }
                cell.Controls.Add(new LiteralControl("&nbsp;"));
                if (!pagedDataSource.IsLastPage)
                {
                    LinkButton button2 = new DataGridLinkButton {
                        Text = pagerStyle.NextPageText,
                        CommandName = "Page",
                        CommandArgument = "Next",
                        CausesValidation = false
                    };
                    cell.Controls.Add(button2);
                }
                else
                {
                    Label label2 = new Label {
                        Text = pagerStyle.NextPageText
                    };
                    cell.Controls.Add(label2);
                }
            }
            else
            {
                LinkButton button3;
                int pageCount = pagedDataSource.PageCount;
                int num2 = pagedDataSource.CurrentPageIndex + 1;
                int pageButtonCount = pagerStyle.PageButtonCount;
                int num4 = pageButtonCount;
                if (pageCount < num4)
                {
                    num4 = pageCount;
                }
                int num5 = 1;
                int num6 = num4;
                if (num2 > num6)
                {
                    int num7 = pagedDataSource.CurrentPageIndex / pageButtonCount;
                    num5 = (num7 * pageButtonCount) + 1;
                    num6 = (num5 + pageButtonCount) - 1;
                    if (num6 > pageCount)
                    {
                        num6 = pageCount;
                    }
                    if (((num6 - num5) + 1) < pageButtonCount)
                    {
                        num5 = Math.Max(1, (num6 - pageButtonCount) + 1);
                    }
                }
                if (num5 != 1)
                {
                    button3 = new DataGridLinkButton {
                        Text = "...",
                        CommandName = "Page"
                    };
                    button3.CommandArgument = (num5 - 1).ToString(NumberFormatInfo.InvariantInfo);
                    button3.CausesValidation = false;
                    cell.Controls.Add(button3);
                    cell.Controls.Add(new LiteralControl("&nbsp;"));
                }
                for (int i = num5; i <= num6; i++)
                {
                    string str = i.ToString(NumberFormatInfo.InvariantInfo);
                    if (i == num2)
                    {
                        Label label3 = new Label {
                            Text = str
                        };
                        cell.Controls.Add(label3);
                    }
                    else
                    {
                        button3 = new DataGridLinkButton {
                            Text = str,
                            CommandName = "Page",
                            CommandArgument = str,
                            CausesValidation = false
                        };
                        cell.Controls.Add(button3);
                    }
                    if (i < num6)
                    {
                        cell.Controls.Add(new LiteralControl("&nbsp;"));
                    }
                }
                if (pageCount > num6)
                {
                    cell.Controls.Add(new LiteralControl("&nbsp;"));
                    button3 = new DataGridLinkButton {
                        Text = "...",
                        CommandName = "Page"
                    };
                    button3.CommandArgument = (num6 + 1).ToString(NumberFormatInfo.InvariantInfo);
                    button3.CausesValidation = false;
                    cell.Controls.Add(button3);
                }
            }
            item.Cells.Add(cell);
        }

        protected override void LoadViewState(object savedState)
        {
            if (savedState != null)
            {
                object[] objArray = (object[]) savedState;
                if (objArray[0] != null)
                {
                    base.LoadViewState(objArray[0]);
                }
                if (objArray[1] != null)
                {
                    ((IStateManager) this.Columns).LoadViewState(objArray[1]);
                }
                if (objArray[2] != null)
                {
                    ((IStateManager) this.PagerStyle).LoadViewState(objArray[2]);
                }
                if (objArray[3] != null)
                {
                    ((IStateManager) this.HeaderStyle).LoadViewState(objArray[3]);
                }
                if (objArray[4] != null)
                {
                    ((IStateManager) this.FooterStyle).LoadViewState(objArray[4]);
                }
                if (objArray[5] != null)
                {
                    ((IStateManager) this.ItemStyle).LoadViewState(objArray[5]);
                }
                if (objArray[6] != null)
                {
                    ((IStateManager) this.AlternatingItemStyle).LoadViewState(objArray[6]);
                }
                if (objArray[7] != null)
                {
                    ((IStateManager) this.SelectedItemStyle).LoadViewState(objArray[7]);
                }
                if (objArray[8] != null)
                {
                    ((IStateManager) this.EditItemStyle).LoadViewState(objArray[8]);
                }
                if (objArray[9] != null)
                {
                    ((IStateManager) base.ControlStyle).LoadViewState(objArray[9]);
                }
                if (objArray[10] != null)
                {
                    object[] objArray2 = (object[]) objArray[10];
                    int length = objArray2.Length;
                    if (length != 0)
                    {
                        this.autoGenColumnsArray = new ArrayList();
                    }
                    else
                    {
                        this.autoGenColumnsArray = null;
                    }
                    for (int i = 0; i < length; i++)
                    {
                        BoundColumn column = new BoundColumn();
                        ((IStateManager) column).TrackViewState();
                        ((IStateManager) column).LoadViewState(objArray2[i]);
                        column.SetOwner(this);
                        this.autoGenColumnsArray.Add(column);
                    }
                }
            }
        }

        protected override bool OnBubbleEvent(object source, EventArgs e)
        {
            bool flag = false;
            if (e is DataGridCommandEventArgs)
            {
                DataGridCommandEventArgs args = (DataGridCommandEventArgs) e;
                this.OnItemCommand(args);
                flag = true;
                string commandName = args.CommandName;
                if (StringUtil.EqualsIgnoreCase(commandName, "Select"))
                {
                    this.SelectedIndex = args.Item.ItemIndex;
                    this.OnSelectedIndexChanged(EventArgs.Empty);
                    return flag;
                }
                if (StringUtil.EqualsIgnoreCase(commandName, "Page"))
                {
                    string commandArgument = (string) args.CommandArgument;
                    int currentPageIndex = this.CurrentPageIndex;
                    if (StringUtil.EqualsIgnoreCase(commandArgument, "Next"))
                    {
                        currentPageIndex++;
                    }
                    else if (StringUtil.EqualsIgnoreCase(commandArgument, "Prev"))
                    {
                        currentPageIndex--;
                    }
                    else
                    {
                        currentPageIndex = int.Parse(commandArgument, CultureInfo.InvariantCulture) - 1;
                    }
                    DataGridPageChangedEventArgs args2 = new DataGridPageChangedEventArgs(source, currentPageIndex);
                    this.OnPageIndexChanged(args2);
                    return flag;
                }
                if (StringUtil.EqualsIgnoreCase(commandName, "Sort"))
                {
                    DataGridSortCommandEventArgs args3 = new DataGridSortCommandEventArgs(source, args);
                    this.OnSortCommand(args3);
                    return flag;
                }
                if (StringUtil.EqualsIgnoreCase(commandName, "Edit"))
                {
                    this.OnEditCommand(args);
                    return flag;
                }
                if (StringUtil.EqualsIgnoreCase(commandName, "Update"))
                {
                    this.OnUpdateCommand(args);
                    return flag;
                }
                if (StringUtil.EqualsIgnoreCase(commandName, "Cancel"))
                {
                    this.OnCancelCommand(args);
                    return flag;
                }
                if (StringUtil.EqualsIgnoreCase(commandName, "Delete"))
                {
                    this.OnDeleteCommand(args);
                }
            }
            return flag;
        }

        protected virtual void OnCancelCommand(DataGridCommandEventArgs e)
        {
            DataGridCommandEventHandler handler = (DataGridCommandEventHandler) base.Events[EventCancelCommand];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        internal void OnColumnsChanged()
        {
            if (base.Initialized)
            {
                base.RequiresDataBinding = true;
            }
        }

        protected virtual void OnDeleteCommand(DataGridCommandEventArgs e)
        {
            DataGridCommandEventHandler handler = (DataGridCommandEventHandler) base.Events[EventDeleteCommand];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnEditCommand(DataGridCommandEventArgs e)
        {
            DataGridCommandEventHandler handler = (DataGridCommandEventHandler) base.Events[EventEditCommand];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnItemCommand(DataGridCommandEventArgs e)
        {
            DataGridCommandEventHandler handler = (DataGridCommandEventHandler) base.Events[EventItemCommand];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnItemCreated(DataGridItemEventArgs e)
        {
            DataGridItemEventHandler handler = (DataGridItemEventHandler) base.Events[EventItemCreated];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnItemDataBound(DataGridItemEventArgs e)
        {
            DataGridItemEventHandler handler = (DataGridItemEventHandler) base.Events[EventItemDataBound];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnPageIndexChanged(DataGridPageChangedEventArgs e)
        {
            DataGridPageChangedEventHandler handler = (DataGridPageChangedEventHandler) base.Events[EventPageIndexChanged];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        internal void OnPagerChanged()
        {
        }

        protected virtual void OnSortCommand(DataGridSortCommandEventArgs e)
        {
            DataGridSortCommandEventHandler handler = (DataGridSortCommandEventHandler) base.Events[EventSortCommand];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnUpdateCommand(DataGridCommandEventArgs e)
        {
            DataGridCommandEventHandler handler = (DataGridCommandEventHandler) base.Events[EventUpdateCommand];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected internal override void PrepareControlHierarchy()
        {
            if (this.Controls.Count != 0)
            {
                Table table = (Table) this.Controls[0];
                table.CopyBaseAttributes(this);
                table.Caption = this.Caption;
                table.CaptionAlign = this.CaptionAlign;
                if (base.ControlStyleCreated)
                {
                    table.ApplyStyle(base.ControlStyle);
                }
                else
                {
                    table.GridLines = GridLines.Both;
                    table.CellSpacing = 0;
                }
                TableRowCollection rows = table.Rows;
                int count = rows.Count;
                if (count != 0)
                {
                    int num2 = this.Columns.Count;
                    DataGridColumn[] array = new DataGridColumn[num2];
                    if (num2 > 0)
                    {
                        this.Columns.CopyTo(array, 0);
                    }
                    Style s = null;
                    if (this.alternatingItemStyle != null)
                    {
                        s = new TableItemStyle();
                        s.CopyFrom(this.itemStyle);
                        s.CopyFrom(this.alternatingItemStyle);
                    }
                    else
                    {
                        s = this.itemStyle;
                    }
                    int num3 = 0;
                    bool flag = true;
                    for (int i = 0; i < count; i++)
                    {
                        Style style2;
                        Style style3;
                        TableCellCollection cells;
                        DataGridItem item = (DataGridItem) rows[i];
                        switch (item.ItemType)
                        {
                            case ListItemType.Header:
                            {
                                if (this.ShowHeader)
                                {
                                    break;
                                }
                                item.Visible = false;
                                continue;
                            }
                            case ListItemType.Footer:
                            {
                                if (this.ShowFooter)
                                {
                                    goto Label_016A;
                                }
                                item.Visible = false;
                                continue;
                            }
                            case ListItemType.Item:
                                item.MergeStyle(this.itemStyle);
                                goto Label_029E;

                            case ListItemType.AlternatingItem:
                                item.MergeStyle(s);
                                goto Label_029E;

                            case ListItemType.SelectedItem:
                                style2 = new TableItemStyle();
                                if ((item.ItemIndex % 2) == 0)
                                {
                                    goto Label_021D;
                                }
                                style2.CopyFrom(s);
                                goto Label_022A;

                            case ListItemType.EditItem:
                                style3 = new TableItemStyle();
                                if ((item.ItemIndex % 2) == 0)
                                {
                                    goto Label_025F;
                                }
                                style3.CopyFrom(s);
                                goto Label_026C;

                            case ListItemType.Pager:
                            {
                                if (this.pagerStyle.Visible)
                                {
                                    goto Label_0196;
                                }
                                item.Visible = false;
                                continue;
                            }
                            default:
                                goto Label_029E;
                        }
                        if (this.headerStyle != null)
                        {
                            item.MergeStyle(this.headerStyle);
                        }
                        goto Label_029E;
                    Label_016A:
                        item.MergeStyle(this.footerStyle);
                        goto Label_029E;
                    Label_0196:
                        if (i == 0)
                        {
                            if (this.pagerStyle.IsPagerOnTop)
                            {
                                goto Label_01CE;
                            }
                            item.Visible = false;
                            continue;
                        }
                        if (!this.pagerStyle.IsPagerOnBottom)
                        {
                            item.Visible = false;
                            continue;
                        }
                    Label_01CE:
                        item.MergeStyle(this.pagerStyle);
                        goto Label_029E;
                    Label_021D:
                        style2.CopyFrom(this.itemStyle);
                    Label_022A:
                        style2.CopyFrom(this.selectedItemStyle);
                        item.MergeStyle(style2);
                        goto Label_029E;
                    Label_025F:
                        style3.CopyFrom(this.itemStyle);
                    Label_026C:
                        if (item.ItemIndex == this.SelectedIndex)
                        {
                            style3.CopyFrom(this.selectedItemStyle);
                        }
                        style3.CopyFrom(this.editItemStyle);
                        item.MergeStyle(style3);
                    Label_029E:
                        cells = item.Cells;
                        int num5 = cells.Count;
                        if ((num2 > 0) && (item.ItemType != ListItemType.Pager))
                        {
                            int num6 = num5;
                            if (num2 < num5)
                            {
                                num6 = num2;
                            }
                            for (int j = 0; j < num6; j++)
                            {
                                if (!array[j].Visible)
                                {
                                    cells[j].Visible = false;
                                    continue;
                                }
                                if ((item.ItemType == ListItemType.Item) && flag)
                                {
                                    num3++;
                                }
                                Style headerStyleInternal = null;
                                switch (item.ItemType)
                                {
                                    case ListItemType.Header:
                                        headerStyleInternal = array[j].HeaderStyleInternal;
                                        break;

                                    case ListItemType.Footer:
                                        headerStyleInternal = array[j].FooterStyleInternal;
                                        break;

                                    default:
                                        headerStyleInternal = array[j].ItemStyleInternal;
                                        break;
                                }
                                cells[j].MergeStyle(headerStyleInternal);
                            }
                            if (item.ItemType == ListItemType.Item)
                            {
                                flag = false;
                            }
                        }
                    }
                    if (((this.Items.Count > 0) && (num3 != this.Items[0].Cells.Count)) && this.AllowPaging)
                    {
                        for (int k = 0; k < count; k++)
                        {
                            DataGridItem item2 = (DataGridItem) rows[k];
                            if ((item2.ItemType == ListItemType.Pager) && (item2.Cells.Count > 0))
                            {
                                item2.Cells[0].ColumnSpan = num3;
                            }
                        }
                    }
                }
            }
        }

        protected override object SaveViewState()
        {
            object obj2 = base.SaveViewState();
            object obj3 = (this.columnCollection != null) ? ((IStateManager) this.columnCollection).SaveViewState() : null;
            object obj4 = (this.pagerStyle != null) ? ((IStateManager) this.pagerStyle).SaveViewState() : null;
            object obj5 = (this.headerStyle != null) ? ((IStateManager) this.headerStyle).SaveViewState() : null;
            object obj6 = (this.footerStyle != null) ? ((IStateManager) this.footerStyle).SaveViewState() : null;
            object obj7 = (this.itemStyle != null) ? ((IStateManager) this.itemStyle).SaveViewState() : null;
            object obj8 = (this.alternatingItemStyle != null) ? ((IStateManager) this.alternatingItemStyle).SaveViewState() : null;
            object obj9 = (this.selectedItemStyle != null) ? ((IStateManager) this.selectedItemStyle).SaveViewState() : null;
            object obj10 = (this.editItemStyle != null) ? ((IStateManager) this.editItemStyle).SaveViewState() : null;
            object obj11 = base.ControlStyleCreated ? ((IStateManager) base.ControlStyle).SaveViewState() : null;
            object[] objArray = null;
            if ((this.autoGenColumnsArray != null) && (this.autoGenColumnsArray.Count != 0))
            {
                objArray = new object[this.autoGenColumnsArray.Count];
                for (int i = 0; i < objArray.Length; i++)
                {
                    objArray[i] = ((IStateManager) this.autoGenColumnsArray[i]).SaveViewState();
                }
            }
            return new object[] { obj2, obj3, obj4, obj5, obj6, obj7, obj8, obj9, obj10, obj11, objArray };
        }

        internal void StoreEnumerator(IEnumerator dataSource, object firstDataItem)
        {
            this.storedData = dataSource;
            this.firstDataItem = firstDataItem;
            this.storedDataValid = true;
        }

        protected override void TrackViewState()
        {
            base.TrackViewState();
            if (this.columnCollection != null)
            {
                ((IStateManager) this.columnCollection).TrackViewState();
            }
            if (this.pagerStyle != null)
            {
                ((IStateManager) this.pagerStyle).TrackViewState();
            }
            if (this.headerStyle != null)
            {
                ((IStateManager) this.headerStyle).TrackViewState();
            }
            if (this.footerStyle != null)
            {
                ((IStateManager) this.footerStyle).TrackViewState();
            }
            if (this.itemStyle != null)
            {
                ((IStateManager) this.itemStyle).TrackViewState();
            }
            if (this.alternatingItemStyle != null)
            {
                ((IStateManager) this.alternatingItemStyle).TrackViewState();
            }
            if (this.selectedItemStyle != null)
            {
                ((IStateManager) this.selectedItemStyle).TrackViewState();
            }
            if (this.editItemStyle != null)
            {
                ((IStateManager) this.editItemStyle).TrackViewState();
            }
            if (base.ControlStyleCreated)
            {
                ((IStateManager) base.ControlStyle).TrackViewState();
            }
        }

        [DefaultValue(false), WebSysDescription("DataGrid_AllowCustomPaging"), WebCategory("Paging")]
        public virtual bool AllowCustomPaging
        {
            get
            {
                object obj2 = this.ViewState["AllowCustomPaging"];
                return ((obj2 != null) && ((bool) obj2));
            }
            set
            {
                this.ViewState["AllowCustomPaging"] = value;
            }
        }

        [DefaultValue(false), WebSysDescription("DataGrid_AllowPaging"), WebCategory("Paging")]
        public virtual bool AllowPaging
        {
            get
            {
                object obj2 = this.ViewState["AllowPaging"];
                return ((obj2 != null) && ((bool) obj2));
            }
            set
            {
                this.ViewState["AllowPaging"] = value;
            }
        }

        [WebSysDescription("DataGrid_AllowSorting"), WebCategory("Behavior"), DefaultValue(false)]
        public virtual bool AllowSorting
        {
            get
            {
                object obj2 = this.ViewState["AllowSorting"];
                return ((obj2 != null) && ((bool) obj2));
            }
            set
            {
                this.ViewState["AllowSorting"] = value;
            }
        }

        [WebCategory("Styles"), PersistenceMode(PersistenceMode.InnerProperty), WebSysDescription("DataGrid_AlternatingItemStyle"), NotifyParentProperty(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public virtual TableItemStyle AlternatingItemStyle
        {
            get
            {
                if (this.alternatingItemStyle == null)
                {
                    this.alternatingItemStyle = new TableItemStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this.alternatingItemStyle).TrackViewState();
                    }
                }
                return this.alternatingItemStyle;
            }
        }

        [WebSysDescription("DataControls_AutoGenerateColumns"), WebCategory("Behavior"), DefaultValue(true)]
        public virtual bool AutoGenerateColumns
        {
            get
            {
                object obj2 = this.ViewState["AutoGenerateColumns"];
                if (obj2 != null)
                {
                    return (bool) obj2;
                }
                return true;
            }
            set
            {
                this.ViewState["AutoGenerateColumns"] = value;
            }
        }

        [UrlProperty, WebCategory("Appearance"), DefaultValue(""), Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), WebSysDescription("WebControl_BackImageUrl")]
        public virtual string BackImageUrl
        {
            get
            {
                if (!base.ControlStyleCreated)
                {
                    return string.Empty;
                }
                return ((TableStyle) base.ControlStyle).BackImageUrl;
            }
            set
            {
                ((TableStyle) base.ControlStyle).BackImageUrl = value;
            }
        }

        [MergableProperty(false), DefaultValue((string) null), Editor("System.Web.UI.Design.WebControls.DataGridColumnCollectionEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), PersistenceMode(PersistenceMode.InnerProperty), WebCategory("Default"), WebSysDescription("DataControls_Columns")]
        public virtual DataGridColumnCollection Columns
        {
            get
            {
                if (this.columnCollection == null)
                {
                    this.columns = new ArrayList();
                    this.columnCollection = new DataGridColumnCollection(this, this.columns);
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this.columnCollection).TrackViewState();
                    }
                }
                return this.columnCollection;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), WebSysDescription("DataGrid_CurrentPageIndex")]
        public int CurrentPageIndex
        {
            get
            {
                object obj2 = this.ViewState["CurrentPageIndex"];
                if (obj2 != null)
                {
                    return (int) obj2;
                }
                return 0;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.ViewState["CurrentPageIndex"] = value;
            }
        }

        [WebCategory("Default"), WebSysDescription("DataGrid_EditItemIndex"), DefaultValue(-1)]
        public virtual int EditItemIndex
        {
            get
            {
                object obj2 = this.ViewState["EditItemIndex"];
                if (obj2 != null)
                {
                    return (int) obj2;
                }
                return -1;
            }
            set
            {
                if (value < -1)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.ViewState["EditItemIndex"] = value;
            }
        }

        [WebSysDescription("DataGrid_EditItemStyle"), WebCategory("Styles"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty)]
        public virtual TableItemStyle EditItemStyle
        {
            get
            {
                if (this.editItemStyle == null)
                {
                    this.editItemStyle = new TableItemStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this.editItemStyle).TrackViewState();
                    }
                }
                return this.editItemStyle;
            }
        }

        [WebSysDescription("DataControls_FooterStyle"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty), WebCategory("Styles"), DefaultValue((string) null)]
        public virtual TableItemStyle FooterStyle
        {
            get
            {
                if (this.footerStyle == null)
                {
                    this.footerStyle = new TableItemStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this.footerStyle).TrackViewState();
                    }
                }
                return this.footerStyle;
            }
        }

        [WebSysDescription("DataControls_HeaderStyle"), PersistenceMode(PersistenceMode.InnerProperty), DefaultValue((string) null), WebCategory("Styles"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true)]
        public virtual TableItemStyle HeaderStyle
        {
            get
            {
                if (this.headerStyle == null)
                {
                    this.headerStyle = new TableItemStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this.headerStyle).TrackViewState();
                    }
                }
                return this.headerStyle;
            }
        }

        [WebSysDescription("DataGrid_Items"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public virtual DataGridItemCollection Items
        {
            get
            {
                if (this.itemsCollection == null)
                {
                    if (this.itemsArray == null)
                    {
                        this.EnsureChildControls();
                    }
                    if (this.itemsArray == null)
                    {
                        this.itemsArray = new ArrayList();
                    }
                    this.itemsCollection = new DataGridItemCollection(this.itemsArray);
                }
                return this.itemsCollection;
            }
        }

        [WebCategory("Styles"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), WebSysDescription("DataGrid_ItemStyle"), NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty)]
        public virtual TableItemStyle ItemStyle
        {
            get
            {
                if (this.itemStyle == null)
                {
                    this.itemStyle = new TableItemStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this.itemStyle).TrackViewState();
                    }
                }
                return this.itemStyle;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false), WebSysDescription("DataGrid_PageCount")]
        public int PageCount
        {
            get
            {
                if (this.pagedDataSource != null)
                {
                    return this.pagedDataSource.PageCount;
                }
                object obj2 = this.ViewState["PageCount"];
                if (obj2 == null)
                {
                    return 0;
                }
                return (int) obj2;
            }
        }

        [NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty), WebSysDescription("DataGrid_PagerStyle"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), WebCategory("Styles")]
        public virtual DataGridPagerStyle PagerStyle
        {
            get
            {
                if (this.pagerStyle == null)
                {
                    this.pagerStyle = new DataGridPagerStyle(this);
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this.pagerStyle).TrackViewState();
                    }
                }
                return this.pagerStyle;
            }
        }

        [WebSysDescription("DataGrid_PageSize"), DefaultValue(10), WebCategory("Paging")]
        public virtual int PageSize
        {
            get
            {
                object obj2 = this.ViewState["PageSize"];
                if (obj2 != null)
                {
                    return (int) obj2;
                }
                return 10;
            }
            set
            {
                if (value < 1)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.ViewState["PageSize"] = value;
            }
        }

        [DefaultValue(-1), Bindable(true), WebSysDescription("WebControl_SelectedIndex")]
        public virtual int SelectedIndex
        {
            get
            {
                object obj2 = this.ViewState["SelectedIndex"];
                if (obj2 != null)
                {
                    return (int) obj2;
                }
                return -1;
            }
            set
            {
                if (value < -1)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                int selectedIndex = this.SelectedIndex;
                this.ViewState["SelectedIndex"] = value;
                if (this.itemsArray != null)
                {
                    DataGridItem item;
                    if ((selectedIndex != -1) && (this.itemsArray.Count > selectedIndex))
                    {
                        item = (DataGridItem) this.itemsArray[selectedIndex];
                        if (item.ItemType != ListItemType.EditItem)
                        {
                            ListItemType itemType = ListItemType.Item;
                            if ((selectedIndex % 2) != 0)
                            {
                                itemType = ListItemType.AlternatingItem;
                            }
                            item.SetItemType(itemType);
                        }
                    }
                    if ((value != -1) && (this.itemsArray.Count > value))
                    {
                        item = (DataGridItem) this.itemsArray[value];
                        if (item.ItemType != ListItemType.EditItem)
                        {
                            item.SetItemType(ListItemType.SelectedItem);
                        }
                    }
                }
            }
        }

        [WebSysDescription("DataGrid_SelectedItem"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual DataGridItem SelectedItem
        {
            get
            {
                int selectedIndex = this.SelectedIndex;
                DataGridItem item = null;
                if (selectedIndex != -1)
                {
                    item = this.Items[selectedIndex];
                }
                return item;
            }
        }

        [PersistenceMode(PersistenceMode.InnerProperty), NotifyParentProperty(true), WebSysDescription("DataGrid_SelectedItemStyle"), WebCategory("Styles"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public virtual TableItemStyle SelectedItemStyle
        {
            get
            {
                if (this.selectedItemStyle == null)
                {
                    this.selectedItemStyle = new TableItemStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this.selectedItemStyle).TrackViewState();
                    }
                }
                return this.selectedItemStyle;
            }
        }

        [WebSysDescription("DataControls_ShowFooter"), DefaultValue(false), WebCategory("Appearance")]
        public virtual bool ShowFooter
        {
            get
            {
                object obj2 = this.ViewState["ShowFooter"];
                return ((obj2 != null) && ((bool) obj2));
            }
            set
            {
                this.ViewState["ShowFooter"] = value;
            }
        }

        [WebCategory("Appearance"), WebSysDescription("DataControls_ShowHeader"), DefaultValue(true)]
        public virtual bool ShowHeader
        {
            get
            {
                object obj2 = this.ViewState["ShowHeader"];
                if (obj2 != null)
                {
                    return (bool) obj2;
                }
                return true;
            }
            set
            {
                this.ViewState["ShowHeader"] = value;
            }
        }

        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                return HtmlTextWriterTag.Table;
            }
        }

        [WebSysDescription("DataGrid_VisibleItemCount"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual int VirtualItemCount
        {
            get
            {
                object obj2 = this.ViewState["VirtualItemCount"];
                if (obj2 != null)
                {
                    return (int) obj2;
                }
                return 0;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.ViewState["VirtualItemCount"] = value;
            }
        }
    }
}

