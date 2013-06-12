namespace System.Web.UI.WebControls
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using System.Web;
    using System.Web.UI;
    using System.Web.Util;

    [Designer("System.Web.UI.Design.WebControls.GridViewDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), SupportsEventValidation, ControlValueProperty("SelectedValue"), DefaultEvent("SelectedIndexChanged"), DataKeyProperty("SelectedPersistedDataKey")]
    public class GridView : CompositeDataBoundControl, IPostBackContainer, IPostBackEventHandler, ICallbackContainer, ICallbackEventHandler, IPersistedSelector, IDataKeysControl, IDataBoundListControl, IDataBoundControl, IFieldControl
    {
        private TableItemStyle _alternatingRowStyle;
        private ArrayList _autoGenFieldProps;
        private GridViewRow _bottomPagerRow;
        private OrderedDictionary _boundFieldValues;
        private string[] _clientIDRowSuffix;
        private DataKeyArray _clientIDRowSuffixArray;
        private ArrayList _clientIDRowSuffixArrayList;
        private IAutoFieldGenerator _columnsGenerator;
        private DataKeyArray _dataKeyArray;
        private string[] _dataKeyNames;
        private ArrayList _dataKeysArrayList;
        private int _deletedRowIndex;
        private IOrderedDictionary _deleteKeys;
        private IOrderedDictionary _deleteValues;
        private int _editIndex;
        private TableItemStyle _editRowStyle;
        private TableItemStyle _emptyDataRowStyle;
        private ITemplate _emptyDataTemplate;
        private DataControlFieldCollection _fieldCollection;
        private object _firstDataRow;
        private GridViewRow _footerRow;
        private TableItemStyle _footerStyle;
        private GridViewRow _headerRow;
        private TableItemStyle _headerStyle;
        private string _modelValidationGroup;
        private int _pageCount;
        private int _pageIndex;
        private System.Web.UI.WebControls.PagerSettings _pagerSettings;
        private TableItemStyle _pagerStyle;
        private ITemplate _pagerTemplate;
        private DataKey _persistedDataKey;
        private bool _renderClientScript;
        private bool _renderClientScriptValid;
        private ArrayList _rowsArray;
        private GridViewRowCollection _rowsCollection;
        private TableItemStyle _rowStyle;
        private int _selectedIndex;
        private TableItemStyle _selectedRowStyle;
        private System.Web.UI.WebControls.SortDirection _sortDirection;
        private TableItemStyle _sortedAscendingCellStyle;
        private TableItemStyle _sortedAscendingHeaderStyle;
        private TableItemStyle _sortedDescendingCellStyle;
        private TableItemStyle _sortedDescendingHeaderStyle;
        private string _sortExpression;
        private string _sortExpressionSerialized;
        private IStateFormatter _stateFormatter;
        private IEnumerator _storedData;
        private bool _storedDataValid;
        private GridViewRow _topPagerRow;
        private IOrderedDictionary _updateKeys;
        private IOrderedDictionary _updateNewValues;
        private IOrderedDictionary _updateOldValues;
        private static readonly object EventPageIndexChanged = new object();
        private static readonly object EventPageIndexChanging = new object();
        private static readonly object EventRowCancelingEdit = new object();
        private static readonly object EventRowCommand = new object();
        private static readonly object EventRowCreated = new object();
        private static readonly object EventRowDataBound = new object();
        private static readonly object EventRowDeleted = new object();
        private static readonly object EventRowDeleting = new object();
        private static readonly object EventRowEditing = new object();
        private static readonly object EventRowUpdated = new object();
        private static readonly object EventRowUpdating = new object();
        private static readonly object EventSelectedIndexChanged = new object();
        private static readonly object EventSelectedIndexChanging = new object();
        private static readonly object EventSorted = new object();
        private static readonly object EventSorting = new object();
        private const string startupScriptFormat = "\r\nvar {0} = new GridView();\r\n{0}.stateField = document.getElementById('{1}');\r\n{0}.panelElement = document.getElementById('{0}__div');\r\n{0}.pageIndex = {3};\r\n{0}.sortExpression = \"{4}\";\r\n{0}.sortDirection = {5};\r\n{0}.setStateField();\r\n{0}.callback = function(arg) {{\r\n    {2};\r\n}};";

        [WebCategory("Action"), WebSysDescription("GridView_OnPageIndexChanged")]
        public event EventHandler PageIndexChanged
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

        [WebSysDescription("GridView_OnPageIndexChanging"), WebCategory("Action")]
        public event GridViewPageEventHandler PageIndexChanging
        {
            add
            {
                base.Events.AddHandler(EventPageIndexChanging, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventPageIndexChanging, value);
            }
        }

        [WebCategory("Action"), WebSysDescription("GridView_OnRowCancelingEdit")]
        public event GridViewCancelEditEventHandler RowCancelingEdit
        {
            add
            {
                base.Events.AddHandler(EventRowCancelingEdit, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventRowCancelingEdit, value);
            }
        }

        [WebCategory("Action"), WebSysDescription("GridView_OnRowCommand")]
        public event GridViewCommandEventHandler RowCommand
        {
            add
            {
                base.Events.AddHandler(EventRowCommand, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventRowCommand, value);
            }
        }

        [WebCategory("Behavior"), WebSysDescription("GridView_OnRowCreated")]
        public event GridViewRowEventHandler RowCreated
        {
            add
            {
                base.Events.AddHandler(EventRowCreated, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventRowCreated, value);
            }
        }

        [WebSysDescription("GridView_OnRowDataBound"), WebCategory("Data")]
        public event GridViewRowEventHandler RowDataBound
        {
            add
            {
                base.Events.AddHandler(EventRowDataBound, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventRowDataBound, value);
            }
        }

        [WebCategory("Action"), WebSysDescription("DataControls_OnRowDeleted")]
        public event GridViewDeletedEventHandler RowDeleted
        {
            add
            {
                base.Events.AddHandler(EventRowDeleted, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventRowDeleted, value);
            }
        }

        [WebSysDescription("DataControls_OnItemDeleting"), WebCategory("Action")]
        public event GridViewDeleteEventHandler RowDeleting
        {
            add
            {
                base.Events.AddHandler(EventRowDeleting, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventRowDeleting, value);
            }
        }

        [WebCategory("Action"), WebSysDescription("GridView_OnRowEditing")]
        public event GridViewEditEventHandler RowEditing
        {
            add
            {
                base.Events.AddHandler(EventRowEditing, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventRowEditing, value);
            }
        }

        [WebCategory("Action"), WebSysDescription("DataControls_OnItemUpdated")]
        public event GridViewUpdatedEventHandler RowUpdated
        {
            add
            {
                base.Events.AddHandler(EventRowUpdated, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventRowUpdated, value);
            }
        }

        [WebCategory("Action"), WebSysDescription("DataControls_OnItemUpdating")]
        public event GridViewUpdateEventHandler RowUpdating
        {
            add
            {
                base.Events.AddHandler(EventRowUpdating, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventRowUpdating, value);
            }
        }

        [WebCategory("Action"), WebSysDescription("GridView_OnSelectedIndexChanged")]
        public event EventHandler SelectedIndexChanged
        {
            add
            {
                base.Events.AddHandler(EventSelectedIndexChanged, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventSelectedIndexChanged, value);
            }
        }

        [WebSysDescription("GridView_OnSelectedIndexChanging"), WebCategory("Action")]
        public event GridViewSelectEventHandler SelectedIndexChanging
        {
            add
            {
                base.Events.AddHandler(EventSelectedIndexChanging, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventSelectedIndexChanging, value);
            }
        }

        [WebCategory("Action"), WebSysDescription("GridView_OnSorted")]
        public event EventHandler Sorted
        {
            add
            {
                base.Events.AddHandler(EventSorted, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventSorted, value);
            }
        }

        [WebSysDescription("GridView_OnSorting"), WebCategory("Action")]
        public event GridViewSortEventHandler Sorting
        {
            add
            {
                base.Events.AddHandler(EventSorting, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventSorting, value);
            }
        }

        public GridView()
        {
            this._pageCount = -1;
            this._editIndex = -1;
            this._selectedIndex = -1;
            this._sortExpression = string.Empty;
        }

        internal GridView(IStateFormatter stateFormatter)
        {
            this._pageCount = -1;
            this._editIndex = -1;
            this._selectedIndex = -1;
            this._sortExpression = string.Empty;
            this._stateFormatter = stateFormatter;
        }

        private void ApplySortingStyle(TableCell cell, DataControlField field, TableItemStyle ascendingStyle, TableItemStyle descendingStyle)
        {
            if (!string.IsNullOrEmpty(this.SortExpression) && string.Equals(field.SortExpression, this.SortExpression, StringComparison.OrdinalIgnoreCase))
            {
                if (this.SortDirection == System.Web.UI.WebControls.SortDirection.Ascending)
                {
                    cell.MergeStyle(ascendingStyle);
                }
                else
                {
                    cell.MergeStyle(descendingStyle);
                }
            }
        }

        private string BuildCallbackArgument(int pageIndex)
        {
            if (string.IsNullOrEmpty(this._sortExpressionSerialized))
            {
                this._sortExpressionSerialized = this.StateFormatter.Serialize(this.SortExpression);
            }
            return string.Concat(new object[] { "\"", pageIndex, "|", (int) this.SortDirection, "|", this._sortExpressionSerialized, "|\"" });
        }

        private string BuildCallbackArgument(string sortExpression, System.Web.UI.WebControls.SortDirection sortDirection)
        {
            return string.Concat(new object[] { "\"", this.PageIndex, "|", (int) sortDirection, "|", this.StateFormatter.Serialize(sortExpression), "|\"" });
        }

        private void ClearDataKeys()
        {
            this._dataKeysArrayList = null;
        }

        protected virtual AutoGeneratedField CreateAutoGeneratedColumn(AutoGeneratedFieldProperties fieldProperties)
        {
            AutoGeneratedField field = new AutoGeneratedField(fieldProperties.DataField);
            string name = fieldProperties.Name;
            ((IStateManager) field).TrackViewState();
            field.HeaderText = name;
            field.SortExpression = name;
            field.ReadOnly = fieldProperties.IsReadOnly;
            field.DataType = fieldProperties.Type;
            return field;
        }

        private ICollection CreateAutoGeneratedColumns(PagedDataSource dataSource)
        {
            if (dataSource == null)
            {
                return null;
            }
            ArrayList list = new ArrayList();
            this._autoGenFieldProps = new ArrayList();
            PropertyDescriptorCollection itemProperties = null;
            bool flag = true;
            itemProperties = dataSource.GetItemProperties(new PropertyDescriptor[0]);
            if (itemProperties == null)
            {
                Type propertyType = null;
                object firstDataRow = null;
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
                        firstDataRow = enumerator.Current;
                    }
                    else
                    {
                        flag = false;
                    }
                    if (firstDataRow != null)
                    {
                        propertyType = firstDataRow.GetType();
                    }
                    this.StoreEnumerator(enumerator, firstDataRow);
                }
                if ((firstDataRow != null) && (firstDataRow is ICustomTypeDescriptor))
                {
                    itemProperties = TypeDescriptor.GetProperties(firstDataRow);
                }
                else if (propertyType != null)
                {
                    if (this.IsBindableType(propertyType))
                    {
                        AutoGeneratedFieldProperties fieldProperties = new AutoGeneratedFieldProperties();
                        ((IStateManager) fieldProperties).TrackViewState();
                        fieldProperties.Type = propertyType;
                        fieldProperties.Name = "Item";
                        fieldProperties.DataField = BoundField.ThisExpression;
                        AutoGeneratedField field = this.CreateAutoGeneratedColumn(fieldProperties);
                        if (field != null)
                        {
                            list.Add(field);
                            this._autoGenFieldProps.Add(fieldProperties);
                        }
                    }
                    else
                    {
                        itemProperties = TypeDescriptor.GetProperties(propertyType);
                    }
                }
            }
            else if (itemProperties.Count == 0)
            {
                flag = false;
            }
            if ((itemProperties != null) && (itemProperties.Count != 0))
            {
                string[] dataKeyNames = this.DataKeyNames;
                int length = dataKeyNames.Length;
                string[] strArray2 = new string[length];
                for (int i = 0; i < length; i++)
                {
                    strArray2[i] = dataKeyNames[i].ToLowerInvariant();
                }
                foreach (PropertyDescriptor descriptor in itemProperties)
                {
                    Type type = descriptor.PropertyType;
                    if (this.IsBindableType(type))
                    {
                        string name = descriptor.Name;
                        bool flag2 = strArray2.Contains(name.ToLowerInvariant());
                        AutoGeneratedFieldProperties properties2 = new AutoGeneratedFieldProperties();
                        ((IStateManager) properties2).TrackViewState();
                        properties2.Name = name;
                        properties2.IsReadOnly = flag2;
                        properties2.Type = type;
                        properties2.DataField = name;
                        AutoGeneratedField field2 = this.CreateAutoGeneratedColumn(properties2);
                        if (field2 != null)
                        {
                            list.Add(field2);
                            this._autoGenFieldProps.Add(properties2);
                        }
                    }
                }
            }
            if ((list.Count == 0) && flag)
            {
                throw new HttpException(System.Web.SR.GetString("GridView_NoAutoGenFields", new object[] { this.ID }));
            }
            return list;
        }

        protected override int CreateChildControls(IEnumerable dataSource, bool dataBinding)
        {
            PagedDataSource pagedDataSource = null;
            if (dataBinding)
            {
                bool allowPaging = this.AllowPaging;
                DataSourceView data = this.GetData();
                DataSourceSelectArguments selectArguments = base.SelectArguments;
                if (data == null)
                {
                    throw new HttpException(System.Web.SR.GetString("DataBoundControl_NullView", new object[] { this.ID }));
                }
                bool flag2 = allowPaging && data.CanPage;
                if ((allowPaging && !data.CanPage) && ((dataSource != null) && !(dataSource is ICollection)))
                {
                    selectArguments.StartRowIndex = this.PageSize * this.PageIndex;
                    selectArguments.MaximumRows = this.PageSize;
                    data.Select(selectArguments, new DataSourceViewSelectCallback(this.SelectCallback));
                }
                if (flag2)
                {
                    if (data.CanRetrieveTotalRowCount)
                    {
                        pagedDataSource = this.CreateServerPagedDataSource(selectArguments.TotalRowCount);
                    }
                    else
                    {
                        ICollection is2 = dataSource as ICollection;
                        if (is2 == null)
                        {
                            throw new HttpException(System.Web.SR.GetString("DataBoundControl_NeedICollectionOrTotalRowCount", new object[] { base.GetType().Name }));
                        }
                        int num = this.PageIndex * this.PageSize;
                        pagedDataSource = this.CreateServerPagedDataSource(num + is2.Count);
                    }
                }
                else
                {
                    pagedDataSource = this.CreatePagedDataSource();
                }
            }
            else
            {
                pagedDataSource = this.CreatePagedDataSource();
            }
            IEnumerator enumerator = null;
            int num2 = 0;
            ArrayList dataKeysArrayList = this.DataKeysArrayList;
            ArrayList clientIDRowSuffixArrayList = this.ClientIDRowSuffixArrayList;
            ICollection is3 = null;
            int count = -1;
            int capacity = 0;
            ICollection is4 = dataSource as ICollection;
            if (dataBinding)
            {
                dataKeysArrayList.Clear();
                clientIDRowSuffixArrayList.Clear();
                if (((dataSource != null) && (is4 == null)) && (pagedDataSource.IsPagingEnabled && !pagedDataSource.IsServerPagingEnabled))
                {
                    throw new HttpException(System.Web.SR.GetString("GridView_Missing_VirtualItemCount", new object[] { this.ID }));
                }
            }
            else if (is4 == null)
            {
                throw new HttpException(System.Web.SR.GetString("DataControls_DataSourceMustBeCollectionWhenNotDataBinding"));
            }
            this._pageCount = 0;
            if (dataSource != null)
            {
                pagedDataSource.DataSource = dataSource;
                if (pagedDataSource.IsPagingEnabled && dataBinding)
                {
                    int pageCount = pagedDataSource.PageCount;
                    if (pagedDataSource.CurrentPageIndex >= pageCount)
                    {
                        int num6 = pageCount - 1;
                        pagedDataSource.CurrentPageIndex = this._pageIndex = num6;
                    }
                }
                is3 = this.CreateColumns(dataBinding ? pagedDataSource : null, dataBinding);
                if (is4 != null)
                {
                    count = is4.Count;
                    int num7 = pagedDataSource.IsPagingEnabled ? pagedDataSource.PageSize : is4.Count;
                    capacity = num7;
                    if (dataBinding)
                    {
                        dataKeysArrayList.Capacity = num7;
                        clientIDRowSuffixArrayList.Capacity = num7;
                    }
                    if (pagedDataSource.DataSourceCount == 0)
                    {
                        this._pageCount = 0;
                    }
                    else
                    {
                        this._pageCount = pagedDataSource.PageCount;
                    }
                }
            }
            this._rowsArray = new ArrayList(capacity);
            this._rowsCollection = null;
            this._dataKeyArray = null;
            this._clientIDRowSuffixArray = null;
            Table child = this.CreateChildTable();
            this.Controls.Add(child);
            TableRowCollection rows = child.Rows;
            if (dataSource == null)
            {
                if ((this.EmptyDataTemplate != null) || (this.EmptyDataText.Length > 0))
                {
                    this.CreateRow(-1, -1, DataControlRowType.EmptyDataRow, DataControlRowState.Normal, dataBinding, null, new DataControlField[0], rows, null);
                }
                else
                {
                    this.Controls.Clear();
                }
                return 0;
            }
            int num8 = 0;
            if (is3 != null)
            {
                num8 = is3.Count;
            }
            DataControlField[] array = new DataControlField[num8];
            if (num8 > 0)
            {
                is3.CopyTo(array, 0);
                bool flag3 = false;
                for (int i = 0; i < array.Length; i++)
                {
                    if (array[i].Initialize(this.AllowSorting, this))
                    {
                        flag3 = true;
                    }
                    if (this.DetermineRenderClientScript())
                    {
                        array[i].ValidateSupportsCallback();
                    }
                }
                if (flag3)
                {
                    base.RequiresDataBinding = true;
                }
            }
            int dataItemIndex = 0;
            int dataSourceIndex = 0;
            string[] dataKeyNamesInternal = this.DataKeyNamesInternal;
            bool flag4 = dataBinding && (dataKeyNamesInternal.Length != 0);
            bool flag5 = dataBinding && (this.ClientIDRowSuffixInternal.Length != 0);
            bool isPagingEnabled = pagedDataSource.IsPagingEnabled;
            int editIndex = this.EditIndex;
            switch (count)
            {
                case -1:
                    if (this._storedDataValid)
                    {
                        if (this._firstDataRow != null)
                        {
                            count = 1;
                        }
                        else
                        {
                            count = 0;
                        }
                    }
                    else
                    {
                        IEnumerator enumerator2 = dataSource.GetEnumerator();
                        if (enumerator2.MoveNext())
                        {
                            object current = enumerator2.Current;
                            this.StoreEnumerator(enumerator2, current);
                            count = 1;
                        }
                        else
                        {
                            count = 0;
                        }
                    }
                    break;

                case 0:
                {
                    bool flag7 = false;
                    if ((this.ShowHeader && this.ShowHeaderWhenEmpty) && (array.Length > 0))
                    {
                        this._headerRow = this.CreateRow(-1, -1, DataControlRowType.Header, DataControlRowState.Normal, dataBinding, null, array, rows, null);
                        flag7 = true;
                    }
                    if ((this.EmptyDataTemplate != null) || (this.EmptyDataText.Length > 0))
                    {
                        this.CreateRow(-1, -1, DataControlRowType.EmptyDataRow, DataControlRowState.Normal, dataBinding, null, array, rows, null);
                        flag7 = true;
                    }
                    if (!flag7)
                    {
                        this.Controls.Clear();
                    }
                    this._storedDataValid = false;
                    this._firstDataRow = null;
                    return 0;
                }
            }
            if (num8 > 0)
            {
                GridViewRow row;
                DataControlRowType dataRow;
                DataControlRowState normal;
                if (pagedDataSource.IsPagingEnabled)
                {
                    dataSourceIndex = pagedDataSource.FirstIndexInPage;
                }
                if ((isPagingEnabled && this.PagerSettings.Visible) && this._pagerSettings.IsPagerOnTop)
                {
                    this._topPagerRow = this.CreateRow(-1, -1, DataControlRowType.Pager, DataControlRowState.Normal, dataBinding, null, array, rows, pagedDataSource);
                }
                this._headerRow = this.CreateRow(-1, -1, DataControlRowType.Header, DataControlRowState.Normal, dataBinding, null, array, rows, null);
                if (!this.ShowHeader)
                {
                    this._headerRow.Visible = false;
                }
                if (flag4)
                {
                    this.ResetPersistedSelectedIndex();
                }
                if (this._storedDataValid)
                {
                    enumerator = this._storedData;
                    if (this._firstDataRow != null)
                    {
                        if (flag4)
                        {
                            OrderedDictionary keyTable = new OrderedDictionary(dataKeyNamesInternal.Length);
                            foreach (string str in dataKeyNamesInternal)
                            {
                                object propertyValue = DataBinder.GetPropertyValue(this._firstDataRow, str);
                                keyTable.Add(str, propertyValue);
                            }
                            if (dataKeysArrayList.Count == dataItemIndex)
                            {
                                dataKeysArrayList.Add(new DataKey(keyTable, dataKeyNamesInternal));
                            }
                            else
                            {
                                dataKeysArrayList[dataItemIndex] = new DataKey(keyTable, dataKeyNamesInternal);
                            }
                        }
                        if (flag5)
                        {
                            OrderedDictionary dictionary2 = new OrderedDictionary(this.ClientIDRowSuffixInternal.Length);
                            foreach (string str2 in this.ClientIDRowSuffixInternal)
                            {
                                object obj4 = DataBinder.GetPropertyValue(this._firstDataRow, str2);
                                dictionary2.Add(str2, obj4);
                            }
                            if (clientIDRowSuffixArrayList.Count == dataItemIndex)
                            {
                                clientIDRowSuffixArrayList.Add(new DataKey(dictionary2, this.ClientIDRowSuffixInternal));
                            }
                            else
                            {
                                clientIDRowSuffixArrayList[dataItemIndex] = new DataKey(dictionary2, this.ClientIDRowSuffixInternal);
                            }
                        }
                        if ((flag4 && this.EnablePersistedSelection) && (dataItemIndex < dataKeysArrayList.Count))
                        {
                            this.SetPersistedDataKey(dataItemIndex, (DataKey) dataKeysArrayList[dataItemIndex]);
                        }
                        dataRow = DataControlRowType.DataRow;
                        normal = DataControlRowState.Normal;
                        if (dataItemIndex == editIndex)
                        {
                            normal |= DataControlRowState.Edit;
                        }
                        if (dataItemIndex == this._selectedIndex)
                        {
                            normal |= DataControlRowState.Selected;
                        }
                        row = this.CreateRow(0, dataSourceIndex, dataRow, normal, dataBinding, this._firstDataRow, array, rows, null);
                        this._rowsArray.Add(row);
                        num2++;
                        dataItemIndex++;
                        dataSourceIndex++;
                        this._storedDataValid = false;
                        this._firstDataRow = null;
                    }
                }
                else
                {
                    enumerator = pagedDataSource.GetEnumerator();
                }
                dataRow = DataControlRowType.DataRow;
                while (enumerator.MoveNext())
                {
                    object container = enumerator.Current;
                    if (flag4)
                    {
                        OrderedDictionary dictionary3 = new OrderedDictionary(dataKeyNamesInternal.Length);
                        foreach (string str3 in dataKeyNamesInternal)
                        {
                            object obj6 = DataBinder.GetPropertyValue(container, str3);
                            dictionary3.Add(str3, obj6);
                        }
                        if (dataKeysArrayList.Count == dataItemIndex)
                        {
                            dataKeysArrayList.Add(new DataKey(dictionary3, dataKeyNamesInternal));
                        }
                        else
                        {
                            dataKeysArrayList[dataItemIndex] = new DataKey(dictionary3, dataKeyNamesInternal);
                        }
                    }
                    if (flag5)
                    {
                        OrderedDictionary dictionary4 = new OrderedDictionary(this.ClientIDRowSuffixInternal.Length);
                        foreach (string str4 in this.ClientIDRowSuffixInternal)
                        {
                            object obj7 = DataBinder.GetPropertyValue(container, str4);
                            dictionary4.Add(str4, obj7);
                        }
                        if (clientIDRowSuffixArrayList.Count == dataItemIndex)
                        {
                            clientIDRowSuffixArrayList.Add(new DataKey(dictionary4, this.ClientIDRowSuffixInternal));
                        }
                        else
                        {
                            clientIDRowSuffixArrayList[dataItemIndex] = new DataKey(dictionary4, this.ClientIDRowSuffixInternal);
                        }
                    }
                    if ((flag4 && this.EnablePersistedSelection) && (dataItemIndex < dataKeysArrayList.Count))
                    {
                        this.SetPersistedDataKey(dataItemIndex, (DataKey) dataKeysArrayList[dataItemIndex]);
                    }
                    normal = DataControlRowState.Normal;
                    if (dataItemIndex == editIndex)
                    {
                        normal |= DataControlRowState.Edit;
                    }
                    if (dataItemIndex == this._selectedIndex)
                    {
                        normal |= DataControlRowState.Selected;
                    }
                    if ((dataItemIndex % 2) != 0)
                    {
                        normal |= DataControlRowState.Alternate;
                    }
                    row = this.CreateRow(dataItemIndex, dataSourceIndex, dataRow, normal, dataBinding, container, array, rows, null);
                    this._rowsArray.Add(row);
                    num2++;
                    dataSourceIndex++;
                    dataItemIndex++;
                }
                if (dataItemIndex == 0)
                {
                    this.CreateRow(-1, -1, DataControlRowType.EmptyDataRow, DataControlRowState.Normal, dataBinding, null, array, rows, null);
                }
                this._footerRow = this.CreateRow(-1, -1, DataControlRowType.Footer, DataControlRowState.Normal, dataBinding, null, array, rows, null);
                if (!this.ShowFooter)
                {
                    this._footerRow.Visible = false;
                }
                if ((isPagingEnabled && this.PagerSettings.Visible) && this._pagerSettings.IsPagerOnBottom)
                {
                    this._bottomPagerRow = this.CreateRow(-1, -1, DataControlRowType.Pager, DataControlRowState.Normal, dataBinding, null, array, rows, pagedDataSource);
                }
            }
            int dataSourceCount = -1;
            if (dataBinding)
            {
                if (enumerator != null)
                {
                    if (pagedDataSource.IsPagingEnabled)
                    {
                        this._pageCount = pagedDataSource.PageCount;
                        dataSourceCount = pagedDataSource.DataSourceCount;
                    }
                    else
                    {
                        this._pageCount = 1;
                        dataSourceCount = num2;
                    }
                }
                else
                {
                    this._pageCount = 0;
                }
            }
            if (this.PageCount == 1)
            {
                if (this._topPagerRow != null)
                {
                    this._topPagerRow.Visible = false;
                }
                if (this._bottomPagerRow != null)
                {
                    this._bottomPagerRow.Visible = false;
                }
            }
            return dataSourceCount;
        }

        protected virtual Table CreateChildTable()
        {
            return new ChildTable(string.IsNullOrEmpty(this.ID) ? null : this.ClientID);
        }

        protected virtual ICollection CreateColumns(PagedDataSource dataSource, bool useDataSource)
        {
            ArrayList list = new ArrayList();
            bool autoGenerateEditButton = this.AutoGenerateEditButton;
            bool autoGenerateDeleteButton = this.AutoGenerateDeleteButton;
            bool autoGenerateSelectButton = this.AutoGenerateSelectButton;
            if ((autoGenerateEditButton || autoGenerateDeleteButton) || autoGenerateSelectButton)
            {
                CommandField field = new CommandField {
                    ButtonType = ButtonType.Link
                };
                if (autoGenerateEditButton)
                {
                    field.ShowEditButton = true;
                }
                if (autoGenerateDeleteButton)
                {
                    field.ShowDeleteButton = true;
                }
                if (autoGenerateSelectButton)
                {
                    field.ShowSelectButton = true;
                }
                list.Add(field);
            }
            foreach (DataControlField field2 in this.Columns)
            {
                list.Add(field2);
            }
            if (this.AutoGenerateColumns)
            {
                if (this.ColumnsGenerator == null)
                {
                    object[] array = null;
                    if (useDataSource)
                    {
                        ICollection is2 = this.CreateAutoGeneratedColumns(dataSource);
                        if (is2 != null)
                        {
                            array = new DataControlField[is2.Count];
                            is2.CopyTo(array, 0);
                        }
                    }
                    else if (this._autoGenFieldProps != null)
                    {
                        int count = this._autoGenFieldProps.Count;
                        array = new DataControlField[count];
                        for (int i = 0; i < count; i++)
                        {
                            array[i] = this.CreateAutoGeneratedColumn((AutoGeneratedFieldProperties) this._autoGenFieldProps[i]);
                        }
                    }
                    if (array != null)
                    {
                        int length = array.Length;
                        for (int j = 0; j < length; j++)
                        {
                            list.Add(array[j]);
                        }
                    }
                    return list;
                }
                list.AddRange(this.ColumnsGenerator.GenerateFields(this));
            }
            return list;
        }

        protected override Style CreateControlStyle()
        {
            return new TableStyle { GridLines = System.Web.UI.WebControls.GridLines.Both, CellSpacing = 0 };
        }

        protected override DataSourceSelectArguments CreateDataSourceSelectArguments()
        {
            DataSourceSelectArguments arguments = new DataSourceSelectArguments();
            DataSourceView data = this.GetData();
            bool flag = this.AllowPaging && data.CanPage;
            string sortExpressionInternal = this.SortExpressionInternal;
            if ((this.SortDirectionInternal == System.Web.UI.WebControls.SortDirection.Descending) && !string.IsNullOrEmpty(sortExpressionInternal))
            {
                sortExpressionInternal = sortExpressionInternal + " DESC";
            }
            arguments.SortExpression = sortExpressionInternal;
            if (flag)
            {
                if (data.CanRetrieveTotalRowCount)
                {
                    arguments.RetrieveTotalRowCount = true;
                    arguments.MaximumRows = this.PageSize;
                }
                else
                {
                    arguments.MaximumRows = -1;
                }
                arguments.StartRowIndex = this.PageSize * this.PageIndex;
            }
            return arguments;
        }

        private void CreateNextPrevPager(TableRow row, PagedDataSource pagedDataSource, bool addFirstLastPageButtons)
        {
            System.Web.UI.WebControls.PagerSettings pagerSettings = this.PagerSettings;
            string previousPageImageUrl = pagerSettings.PreviousPageImageUrl;
            string nextPageImageUrl = pagerSettings.NextPageImageUrl;
            bool isFirstPage = pagedDataSource.IsFirstPage;
            bool isLastPage = pagedDataSource.IsLastPage;
            if (addFirstLastPageButtons && !isFirstPage)
            {
                IButtonControl control;
                TableCell cell = new TableCell();
                row.Cells.Add(cell);
                string firstPageImageUrl = pagerSettings.FirstPageImageUrl;
                if (firstPageImageUrl.Length > 0)
                {
                    control = new DataControlImageButton(this);
                    ((DataControlImageButton) control).ImageUrl = firstPageImageUrl;
                    ((DataControlImageButton) control).AlternateText = HttpUtility.HtmlDecode(pagerSettings.FirstPageText);
                    ((DataControlImageButton) control).EnableCallback(this.BuildCallbackArgument(0));
                }
                else
                {
                    control = new DataControlPagerLinkButton(this);
                    ((DataControlPagerLinkButton) control).Text = pagerSettings.FirstPageText;
                    ((DataControlPagerLinkButton) control).EnableCallback(this.BuildCallbackArgument(0));
                }
                control.CommandName = "Page";
                control.CommandArgument = "First";
                cell.Controls.Add((Control) control);
            }
            if (!isFirstPage)
            {
                IButtonControl control2;
                TableCell cell2 = new TableCell();
                row.Cells.Add(cell2);
                if (previousPageImageUrl.Length > 0)
                {
                    control2 = new DataControlImageButton(this);
                    ((DataControlImageButton) control2).ImageUrl = previousPageImageUrl;
                    ((DataControlImageButton) control2).AlternateText = HttpUtility.HtmlDecode(pagerSettings.PreviousPageText);
                    ((DataControlImageButton) control2).EnableCallback(this.BuildCallbackArgument(this.PageIndex - 1));
                }
                else
                {
                    control2 = new DataControlPagerLinkButton(this);
                    ((DataControlPagerLinkButton) control2).Text = pagerSettings.PreviousPageText;
                    ((DataControlPagerLinkButton) control2).EnableCallback(this.BuildCallbackArgument(this.PageIndex - 1));
                }
                control2.CommandName = "Page";
                control2.CommandArgument = "Prev";
                cell2.Controls.Add((Control) control2);
            }
            if (!isLastPage)
            {
                IButtonControl control3;
                TableCell cell3 = new TableCell();
                row.Cells.Add(cell3);
                if (nextPageImageUrl.Length > 0)
                {
                    control3 = new DataControlImageButton(this);
                    ((DataControlImageButton) control3).ImageUrl = nextPageImageUrl;
                    ((DataControlImageButton) control3).AlternateText = HttpUtility.HtmlDecode(pagerSettings.NextPageText);
                    ((DataControlImageButton) control3).EnableCallback(this.BuildCallbackArgument(this.PageIndex + 1));
                }
                else
                {
                    control3 = new DataControlPagerLinkButton(this);
                    ((DataControlPagerLinkButton) control3).Text = pagerSettings.NextPageText;
                    ((DataControlPagerLinkButton) control3).EnableCallback(this.BuildCallbackArgument(this.PageIndex + 1));
                }
                control3.CommandName = "Page";
                control3.CommandArgument = "Next";
                cell3.Controls.Add((Control) control3);
            }
            if (addFirstLastPageButtons && !isLastPage)
            {
                IButtonControl control4;
                TableCell cell4 = new TableCell();
                row.Cells.Add(cell4);
                string lastPageImageUrl = pagerSettings.LastPageImageUrl;
                if (lastPageImageUrl.Length > 0)
                {
                    control4 = new DataControlImageButton(this);
                    ((DataControlImageButton) control4).ImageUrl = lastPageImageUrl;
                    ((DataControlImageButton) control4).AlternateText = HttpUtility.HtmlDecode(pagerSettings.LastPageText);
                    ((DataControlImageButton) control4).EnableCallback(this.BuildCallbackArgument(pagedDataSource.PageCount - 1));
                }
                else
                {
                    control4 = new DataControlPagerLinkButton(this);
                    ((DataControlPagerLinkButton) control4).Text = pagerSettings.LastPageText;
                    ((DataControlPagerLinkButton) control4).EnableCallback(this.BuildCallbackArgument(pagedDataSource.PageCount - 1));
                }
                control4.CommandName = "Page";
                control4.CommandArgument = "Last";
                cell4.Controls.Add((Control) control4);
            }
        }

        private void CreateNumericPager(TableRow row, PagedDataSource pagedDataSource, bool addFirstLastPageButtons)
        {
            LinkButton button;
            System.Web.UI.WebControls.PagerSettings pagerSettings = this.PagerSettings;
            int pageCount = pagedDataSource.PageCount;
            int num2 = pagedDataSource.CurrentPageIndex + 1;
            int pageButtonCount = pagerSettings.PageButtonCount;
            int num4 = pageButtonCount;
            int num5 = this.FirstDisplayedPageIndex + 1;
            if (pageCount < num4)
            {
                num4 = pageCount;
            }
            int num6 = 1;
            int pageIndex = num4;
            if (num2 > pageIndex)
            {
                int num8 = pagedDataSource.CurrentPageIndex / pageButtonCount;
                bool flag = ((num2 - num5) >= 0) && ((num2 - num5) < pageButtonCount);
                if ((num5 > 0) && flag)
                {
                    num6 = num5;
                }
                else
                {
                    num6 = (num8 * pageButtonCount) + 1;
                }
                pageIndex = (num6 + pageButtonCount) - 1;
                if (pageIndex > pageCount)
                {
                    pageIndex = pageCount;
                }
                if (((pageIndex - num6) + 1) < pageButtonCount)
                {
                    num6 = Math.Max(1, (pageIndex - pageButtonCount) + 1);
                }
                this.FirstDisplayedPageIndex = num6 - 1;
            }
            if ((addFirstLastPageButtons && (num2 != 1)) && (num6 != 1))
            {
                IButtonControl control;
                TableCell cell = new TableCell();
                row.Cells.Add(cell);
                string firstPageImageUrl = pagerSettings.FirstPageImageUrl;
                if (firstPageImageUrl.Length > 0)
                {
                    control = new DataControlImageButton(this);
                    ((DataControlImageButton) control).ImageUrl = firstPageImageUrl;
                    ((DataControlImageButton) control).AlternateText = HttpUtility.HtmlDecode(pagerSettings.FirstPageText);
                    ((DataControlImageButton) control).EnableCallback(this.BuildCallbackArgument(0));
                }
                else
                {
                    control = new DataControlPagerLinkButton(this);
                    ((DataControlPagerLinkButton) control).Text = pagerSettings.FirstPageText;
                    ((DataControlPagerLinkButton) control).EnableCallback(this.BuildCallbackArgument(0));
                }
                control.CommandName = "Page";
                control.CommandArgument = "First";
                cell.Controls.Add((Control) control);
            }
            if (num6 != 1)
            {
                TableCell cell2 = new TableCell();
                row.Cells.Add(cell2);
                button = new DataControlPagerLinkButton(this) {
                    Text = "...",
                    CommandName = "Page"
                };
                button.CommandArgument = (num6 - 1).ToString(NumberFormatInfo.InvariantInfo);
                ((DataControlPagerLinkButton) button).EnableCallback(this.BuildCallbackArgument(num6 - 2));
                cell2.Controls.Add(button);
            }
            for (int i = num6; i <= pageIndex; i++)
            {
                TableCell cell3 = new TableCell();
                row.Cells.Add(cell3);
                string str2 = i.ToString(NumberFormatInfo.InvariantInfo);
                if (i == num2)
                {
                    Label child = new Label {
                        Text = str2
                    };
                    cell3.Controls.Add(child);
                }
                else
                {
                    button = new DataControlPagerLinkButton(this) {
                        Text = str2,
                        CommandName = "Page",
                        CommandArgument = str2
                    };
                    ((DataControlPagerLinkButton) button).EnableCallback(this.BuildCallbackArgument(i - 1));
                    cell3.Controls.Add(button);
                }
            }
            if (pageCount > pageIndex)
            {
                TableCell cell4 = new TableCell();
                row.Cells.Add(cell4);
                button = new DataControlPagerLinkButton(this) {
                    Text = "...",
                    CommandName = "Page"
                };
                button.CommandArgument = (pageIndex + 1).ToString(NumberFormatInfo.InvariantInfo);
                ((DataControlPagerLinkButton) button).EnableCallback(this.BuildCallbackArgument(pageIndex));
                cell4.Controls.Add(button);
            }
            bool flag2 = pageIndex == pageCount;
            if ((addFirstLastPageButtons && (num2 != pageCount)) && !flag2)
            {
                IButtonControl control2;
                TableCell cell5 = new TableCell();
                row.Cells.Add(cell5);
                string lastPageImageUrl = pagerSettings.LastPageImageUrl;
                if (lastPageImageUrl.Length > 0)
                {
                    control2 = new DataControlImageButton(this);
                    ((DataControlImageButton) control2).ImageUrl = lastPageImageUrl;
                    ((DataControlImageButton) control2).AlternateText = HttpUtility.HtmlDecode(pagerSettings.LastPageText);
                    ((DataControlImageButton) control2).EnableCallback(this.BuildCallbackArgument(pagedDataSource.PageCount - 1));
                }
                else
                {
                    control2 = new DataControlPagerLinkButton(this);
                    ((DataControlPagerLinkButton) control2).Text = pagerSettings.LastPageText;
                    ((DataControlPagerLinkButton) control2).EnableCallback(this.BuildCallbackArgument(pagedDataSource.PageCount - 1));
                }
                control2.CommandName = "Page";
                control2.CommandArgument = "Last";
                cell5.Controls.Add((Control) control2);
            }
        }

        private PagedDataSource CreatePagedDataSource()
        {
            return new PagedDataSource { CurrentPageIndex = this.PageIndex, PageSize = this.PageSize, AllowPaging = this.AllowPaging, AllowCustomPaging = false, AllowServerPaging = false, VirtualCount = 0 };
        }

        protected virtual GridViewRow CreateRow(int rowIndex, int dataSourceIndex, DataControlRowType rowType, DataControlRowState rowState)
        {
            return new GridViewRow(rowIndex, dataSourceIndex, rowType, rowState);
        }

        private GridViewRow CreateRow(int rowIndex, int dataSourceIndex, DataControlRowType rowType, DataControlRowState rowState, bool dataBind, object dataItem, DataControlField[] fields, TableRowCollection rows, PagedDataSource pagedDataSource)
        {
            GridViewRow row = this.CreateRow(rowIndex, dataSourceIndex, rowType, rowState);
            GridViewRowEventArgs e = new GridViewRowEventArgs(row);
            if (rowType != DataControlRowType.Pager)
            {
                this.InitializeRow(row, fields);
            }
            else
            {
                this.InitializePager(row, fields.Length, pagedDataSource);
            }
            if (dataBind)
            {
                row.DataItem = dataItem;
            }
            this.OnRowCreated(e);
            rows.Add(row);
            if (dataBind)
            {
                row.DataBind();
                this.OnRowDataBound(e);
                row.DataItem = null;
            }
            return row;
        }

        private PagedDataSource CreateServerPagedDataSource(int totalRowCount)
        {
            return new PagedDataSource { CurrentPageIndex = this.PageIndex, PageSize = this.PageSize, AllowPaging = this.AllowPaging, AllowCustomPaging = false, AllowServerPaging = true, VirtualCount = totalRowCount };
        }

        public sealed override void DataBind()
        {
            base.DataBind();
        }

        public virtual void DeleteRow(int rowIndex)
        {
            this.ResetModelValidationGroup(this.EnableModelValidation, string.Empty);
            this.HandleDelete(null, rowIndex);
        }

        private bool DetermineRenderClientScript()
        {
            if (!this._renderClientScriptValid)
            {
                this._renderClientScript = false;
                if (((this.EnableSortingAndPagingCallbacks && (this.Context != null)) && ((this.Page != null) && (this.Page.RequestInternal != null))) && (this.Page.Request.Browser.SupportsCallback && !base.IsParentedToUpdatePanel))
                {
                    HttpBrowserCapabilities browser = this.Page.Request.Browser;
                    bool flag = browser.EcmaScriptVersion.Major > 0;
                    bool flag2 = browser.W3CDomVersion.Major > 0;
                    bool flag3 = !StringUtil.EqualsIgnoreCase(browser["tagwriter"], typeof(Html32TextWriter).FullName);
                    this._renderClientScript = (flag && flag2) && flag3;
                }
                this._renderClientScriptValid = true;
            }
            return this._renderClientScript;
        }

        protected virtual void ExtractRowValues(IOrderedDictionary fieldValues, GridViewRow row, bool includeReadOnlyFields, bool includePrimaryKey)
        {
            if (fieldValues != null)
            {
                ICollection is2 = this.CreateColumns(null, false);
                int count = is2.Count;
                object[] array = new object[count];
                string[] dataKeyNamesInternal = this.DataKeyNamesInternal;
                is2.CopyTo(array, 0);
                for (int i = 0; (i < count) && (i < row.Cells.Count); i++)
                {
                    if (((DataControlField) array[i]).Visible)
                    {
                        OrderedDictionary dictionary = new OrderedDictionary();
                        ((DataControlField) array[i]).ExtractValuesFromCell(dictionary, row.Cells[i] as DataControlFieldCell, row.RowState, includeReadOnlyFields);
                        foreach (DictionaryEntry entry in dictionary)
                        {
                            if (includePrimaryKey || (Array.IndexOf<object>(dataKeyNamesInternal, entry.Key) == -1))
                            {
                                fieldValues[entry.Key] = entry.Value;
                            }
                        }
                    }
                }
            }
        }

        protected virtual string GetCallbackResult()
        {
            StringWriter writer = new StringWriter(CultureInfo.InvariantCulture);
            HtmlTextWriter writer2 = new HtmlTextWriter(writer);
            IStateFormatter stateFormatter = this.StateFormatter;
            this.RenderTableContents(writer2);
            writer2.Flush();
            writer2.Close();
            string str = stateFormatter.Serialize(this.SaveDataKeysState());
            string str2 = stateFormatter.Serialize(this.SortExpression);
            return (Convert.ToString(this.PageIndex, CultureInfo.InvariantCulture) + "|" + Convert.ToString((int) this.SortDirection, CultureInfo.InvariantCulture) + "|" + str2 + "|" + str + "|" + writer.ToString());
        }

        protected virtual string GetCallbackScript(IButtonControl buttonControl, string argument)
        {
            if (!this.DetermineRenderClientScript())
            {
                return null;
            }
            if (string.IsNullOrEmpty(argument) && (buttonControl.CommandName == "Sort"))
            {
                argument = this.BuildCallbackArgument(buttonControl.CommandArgument, this.SortDirection);
            }
            if (this.Page != null)
            {
                this.Page.ClientScript.RegisterForEventValidation(this.UniqueID, argument);
            }
            return (("javascript:__gv" + this.ClientID + ".callback") + "(" + argument + "); return false;");
        }

        private int GetRowIndex(GridViewRow row, string commandArgument)
        {
            if (row != null)
            {
                return row.RowIndex;
            }
            return Convert.ToInt32(commandArgument, CultureInfo.InvariantCulture);
        }

        private void HandleCancel(int rowIndex)
        {
            bool isBoundUsingDataSourceID = base.IsBoundUsingDataSourceID;
            GridViewCancelEditEventArgs e = new GridViewCancelEditEventArgs(rowIndex);
            this.OnRowCancelingEdit(e);
            if (!e.Cancel)
            {
                if (isBoundUsingDataSourceID)
                {
                    this.EditIndex = -1;
                }
                base.RequiresDataBinding = true;
            }
        }

        private bool HandleCommand(GridViewRow row, int rowIndex, string commandName)
        {
            DataSourceView data = null;
            if (base.IsBoundUsingDataSourceID)
            {
                data = this.GetData();
                if (data == null)
                {
                    throw new HttpException(System.Web.SR.GetString("GridView_DataSourceReturnedNullView", new object[] { this.ID }));
                }
            }
            else
            {
                return false;
            }
            if ((row == null) && (rowIndex < this.Rows.Count))
            {
                row = this.Rows[rowIndex];
            }
            if (!data.CanExecute(commandName))
            {
                return false;
            }
            OrderedDictionary fieldValues = new OrderedDictionary();
            OrderedDictionary keys = new OrderedDictionary();
            if (row != null)
            {
                this.ExtractRowValues(fieldValues, row, true, false);
            }
            if (this.DataKeys.Count > rowIndex)
            {
                foreach (DictionaryEntry entry in this.DataKeys[rowIndex].Values)
                {
                    keys.Add(entry.Key, entry.Value);
                    if (fieldValues.Contains(entry.Key))
                    {
                        fieldValues.Remove(entry.Key);
                    }
                }
            }
            data.ExecuteCommand(commandName, keys, fieldValues, new DataSourceViewOperationCallback(this.HandleCommandCallback));
            return true;
        }

        private bool HandleCommandCallback(int affectedRows, Exception ex)
        {
            if ((ex != null) && this.PageIsValidAfterModelException())
            {
                return false;
            }
            this.EditIndex = -1;
            base.RequiresDataBinding = true;
            return true;
        }

        private void HandleDelete(GridViewRow row, int rowIndex)
        {
            DataSourceView data = null;
            bool isBoundUsingDataSourceID = base.IsBoundUsingDataSourceID;
            if (isBoundUsingDataSourceID)
            {
                data = this.GetData();
                if (data == null)
                {
                    throw new HttpException(System.Web.SR.GetString("GridView_DataSourceReturnedNullView", new object[] { this.ID }));
                }
            }
            if ((row == null) && (rowIndex < this.Rows.Count))
            {
                row = this.Rows[rowIndex];
            }
            GridViewDeleteEventArgs e = new GridViewDeleteEventArgs(rowIndex);
            if (row != null)
            {
                this.ExtractRowValues(e.Values, row, true, false);
            }
            if (this.DataKeys.Count > rowIndex)
            {
                foreach (DictionaryEntry entry in this.DataKeys[rowIndex].Values)
                {
                    e.Keys.Add(entry.Key, entry.Value);
                    if (e.Values.Contains(entry.Key))
                    {
                        e.Values.Remove(entry.Key);
                    }
                }
            }
            this.OnRowDeleting(e);
            if (!e.Cancel)
            {
                this._deletedRowIndex = rowIndex;
                if (isBoundUsingDataSourceID)
                {
                    this._deleteKeys = e.Keys;
                    this._deleteValues = e.Values;
                    data.Delete(e.Keys, e.Values, new DataSourceViewOperationCallback(this.HandleDeleteCallback));
                }
            }
        }

        private bool HandleDeleteCallback(int affectedRows, Exception ex)
        {
            GridViewDeletedEventArgs e = new GridViewDeletedEventArgs(affectedRows, ex);
            e.SetKeys(this._deleteKeys);
            e.SetValues(this._deleteValues);
            this.OnRowDeleted(e);
            this._deleteKeys = null;
            this._deleteValues = null;
            if (((ex != null) && !e.ExceptionHandled) && this.PageIsValidAfterModelException())
            {
                return false;
            }
            this.EditIndex = -1;
            if (affectedRows > 0)
            {
                int num = (int) this.ViewState["_!ItemCount"];
                int num2 = Math.Max(0, num - affectedRows);
                if (this.AllowPaging)
                {
                    int num3 = 0;
                    num3 = Math.Max(1, ((num2 + this.PageSize) - 1) / this.PageSize);
                    this._pageIndex = Math.Min(this._pageIndex, num3 - 1);
                }
                if (this.SelectedIndex >= 0)
                {
                    if (num2 == 0)
                    {
                        this.SelectedIndex = -1;
                    }
                    else
                    {
                        int num4 = this.AllowPaging ? ((this.PageIndex * this.PageSize) + this.SelectedIndex) : this.SelectedIndex;
                        if (num4 > num2)
                        {
                            int num5 = this.AllowPaging ? (num2 % this.PageSize) : num2;
                            this.SelectedIndex = num5;
                        }
                    }
                }
            }
            this._deletedRowIndex = -1;
            base.RequiresDataBinding = true;
            return true;
        }

        private void HandleEdit(int rowIndex)
        {
            GridViewEditEventArgs e = new GridViewEditEventArgs(rowIndex);
            this.OnRowEditing(e);
            if (!e.Cancel)
            {
                this.EditIndex = e.NewEditIndex;
                base.RequiresDataBinding = true;
            }
        }

        private bool HandleEvent(EventArgs e, bool causesValidation, string validationGroup)
        {
            bool flag = false;
            this.ResetModelValidationGroup(causesValidation, validationGroup);
            GridViewCommandEventArgs args = e as GridViewCommandEventArgs;
            if (args == null)
            {
                return flag;
            }
            this.OnRowCommand(args);
            flag = true;
            string commandName = args.CommandName;
            if (StringUtil.EqualsIgnoreCase(commandName, "Select"))
            {
                this.HandleSelect(this.GetRowIndex(args.Row, (string) args.CommandArgument));
                return flag;
            }
            if (StringUtil.EqualsIgnoreCase(commandName, "Page"))
            {
                string commandArgument = (string) args.CommandArgument;
                int pageIndex = this.PageIndex;
                if (StringUtil.EqualsIgnoreCase(commandArgument, "Next"))
                {
                    pageIndex++;
                }
                else if (StringUtil.EqualsIgnoreCase(commandArgument, "Prev"))
                {
                    pageIndex--;
                }
                else if (StringUtil.EqualsIgnoreCase(commandArgument, "First"))
                {
                    pageIndex = 0;
                }
                else if (StringUtil.EqualsIgnoreCase(commandArgument, "Last"))
                {
                    pageIndex = this.PageCount - 1;
                }
                else
                {
                    pageIndex = Convert.ToInt32(commandArgument, CultureInfo.InvariantCulture) - 1;
                }
                this.HandlePage(pageIndex);
                return flag;
            }
            if (StringUtil.EqualsIgnoreCase(commandName, "Sort"))
            {
                this.HandleSort((string) args.CommandArgument);
                return flag;
            }
            if (StringUtil.EqualsIgnoreCase(commandName, "Edit"))
            {
                this.HandleEdit(this.GetRowIndex(args.Row, (string) args.CommandArgument));
                return flag;
            }
            if (StringUtil.EqualsIgnoreCase(commandName, "Update"))
            {
                this.HandleUpdate(args.Row, this.GetRowIndex(args.Row, (string) args.CommandArgument), causesValidation);
                return flag;
            }
            if (StringUtil.EqualsIgnoreCase(commandName, "Cancel"))
            {
                this.HandleCancel(this.GetRowIndex(args.Row, (string) args.CommandArgument));
                return flag;
            }
            if (StringUtil.EqualsIgnoreCase(commandName, "Delete"))
            {
                this.HandleDelete(args.Row, this.GetRowIndex(args.Row, (string) args.CommandArgument));
                return flag;
            }
            return this.HandleCommand(args.Row, this.GetRowIndex(args.Row, (string) args.CommandArgument), commandName);
        }

        private void HandlePage(int newPage)
        {
            if (this.AllowPaging)
            {
                GridViewPageEventArgs e = new GridViewPageEventArgs(newPage);
                this.OnPageIndexChanging(e);
                if (!e.Cancel)
                {
                    if (base.IsBoundUsingDataSourceID)
                    {
                        if (e.NewPageIndex <= -1)
                        {
                            return;
                        }
                        if ((e.NewPageIndex >= this.PageCount) && (this._pageIndex == (this.PageCount - 1)))
                        {
                            return;
                        }
                        this.ClearDataKeys();
                        this.EditIndex = -1;
                        this._pageIndex = e.NewPageIndex;
                    }
                    this.OnPageIndexChanged(EventArgs.Empty);
                    base.RequiresDataBinding = true;
                }
            }
        }

        private void HandleSelect(int rowIndex)
        {
            GridViewSelectEventArgs e = new GridViewSelectEventArgs(rowIndex);
            this.OnSelectedIndexChanging(e);
            if (!e.Cancel)
            {
                this.SelectedIndex = e.NewSelectedIndex;
                this.OnSelectedIndexChanged(EventArgs.Empty);
            }
        }

        private void HandleSort(string sortExpression)
        {
            if (this.AllowSorting)
            {
                System.Web.UI.WebControls.SortDirection ascending = System.Web.UI.WebControls.SortDirection.Ascending;
                if ((this.SortExpressionInternal == sortExpression) && (this.SortDirectionInternal == System.Web.UI.WebControls.SortDirection.Ascending))
                {
                    ascending = System.Web.UI.WebControls.SortDirection.Descending;
                }
                this.HandleSort(sortExpression, ascending);
            }
        }

        private void HandleSort(string sortExpression, System.Web.UI.WebControls.SortDirection sortDirection)
        {
            bool isBoundUsingDataSourceID = base.IsBoundUsingDataSourceID;
            GridViewSortEventArgs e = new GridViewSortEventArgs(sortExpression, sortDirection);
            this.OnSorting(e);
            if (!e.Cancel)
            {
                if (isBoundUsingDataSourceID)
                {
                    this.ClearDataKeys();
                    if (this.GetData() == null)
                    {
                        throw new HttpException(System.Web.SR.GetString("GridView_DataSourceReturnedNullView", new object[] { this.ID }));
                    }
                    this.EditIndex = -1;
                    this.SortExpressionInternal = e.SortExpression;
                    this.SortDirectionInternal = e.SortDirection;
                    this._pageIndex = 0;
                }
                this.OnSorted(EventArgs.Empty);
                base.RequiresDataBinding = true;
            }
        }

        private void HandleUpdate(GridViewRow row, int rowIndex, bool causesValidation)
        {
            if ((!causesValidation || (this.Page == null)) || this.Page.IsValid)
            {
                DataSourceView data = null;
                bool isBoundUsingDataSourceID = base.IsBoundUsingDataSourceID;
                if (isBoundUsingDataSourceID)
                {
                    data = this.GetData();
                    if (data == null)
                    {
                        throw new HttpException(System.Web.SR.GetString("GridView_DataSourceReturnedNullView", new object[] { this.ID }));
                    }
                }
                GridViewUpdateEventArgs e = new GridViewUpdateEventArgs(rowIndex);
                foreach (DictionaryEntry entry in this.BoundFieldValues)
                {
                    e.OldValues.Add(entry.Key, entry.Value);
                }
                if (this.DataKeys.Count > rowIndex)
                {
                    foreach (DictionaryEntry entry2 in this.DataKeys[rowIndex].Values)
                    {
                        e.Keys.Add(entry2.Key, entry2.Value);
                    }
                }
                if ((row == null) && (this.Rows.Count > rowIndex))
                {
                    row = this.Rows[rowIndex];
                }
                if (row != null)
                {
                    this.ExtractRowValues(e.NewValues, row, false, true);
                }
                this.OnRowUpdating(e);
                if (!e.Cancel && isBoundUsingDataSourceID)
                {
                    this._updateKeys = e.Keys;
                    this._updateOldValues = e.OldValues;
                    this._updateNewValues = e.NewValues;
                    data.Update(e.Keys, e.NewValues, e.OldValues, new DataSourceViewOperationCallback(this.HandleUpdateCallback));
                }
            }
        }

        private bool HandleUpdateCallback(int affectedRows, Exception ex)
        {
            GridViewUpdatedEventArgs e = new GridViewUpdatedEventArgs(affectedRows, ex);
            e.SetKeys(this._updateKeys);
            e.SetOldValues(this._updateOldValues);
            e.SetNewValues(this._updateNewValues);
            this.OnRowUpdated(e);
            this._updateKeys = null;
            this._updateOldValues = null;
            this._updateNewValues = null;
            if ((ex != null) && !e.ExceptionHandled)
            {
                if (this.PageIsValidAfterModelException())
                {
                    return false;
                }
                e.KeepInEditMode = true;
            }
            if (!e.KeepInEditMode)
            {
                this.EditIndex = -1;
                base.RequiresDataBinding = true;
            }
            return true;
        }

        protected virtual void InitializePager(GridViewRow row, int columnSpan, PagedDataSource pagedDataSource)
        {
            TableCell cell = new TableCell();
            if (columnSpan > 1)
            {
                cell.ColumnSpan = columnSpan;
            }
            System.Web.UI.WebControls.PagerSettings pagerSettings = this.PagerSettings;
            if (this._pagerTemplate != null)
            {
                this.InitializeTemplateRow(row, columnSpan);
            }
            else
            {
                PagerTable child = new PagerTable();
                TableRow row2 = new TableRow();
                switch (pagerSettings.Mode)
                {
                    case PagerButtons.NextPrevious:
                        this.CreateNextPrevPager(row2, pagedDataSource, false);
                        break;

                    case PagerButtons.Numeric:
                        this.CreateNumericPager(row2, pagedDataSource, false);
                        break;

                    case PagerButtons.NextPreviousFirstLast:
                        this.CreateNextPrevPager(row2, pagedDataSource, true);
                        break;

                    case PagerButtons.NumericFirstLast:
                        this.CreateNumericPager(row2, pagedDataSource, true);
                        break;
                }
                cell.Controls.Add(child);
                child.Rows.Add(row2);
                row.Cells.Add(cell);
            }
        }

        protected virtual void InitializeRow(GridViewRow row, DataControlField[] fields)
        {
            DataControlRowType rowType = row.RowType;
            DataControlRowState rowState = row.RowState;
            int rowIndex = row.RowIndex;
            bool useAccessibleHeader = false;
            if (rowType == DataControlRowType.EmptyDataRow)
            {
                this.InitializeTemplateRow(row, fields.Length);
            }
            else
            {
                TableCellCollection cells = row.Cells;
                string rowHeaderColumn = this.RowHeaderColumn;
                if (rowType == DataControlRowType.Header)
                {
                    useAccessibleHeader = this.UseAccessibleHeader;
                }
                for (int i = 0; i < fields.Length; i++)
                {
                    DataControlFieldCell cell;
                    DataControlCellType header;
                    if ((rowType == DataControlRowType.Header) && useAccessibleHeader)
                    {
                        cell = new DataControlFieldHeaderCell(fields[i]);
                        ((DataControlFieldHeaderCell) cell).Scope = TableHeaderScope.Column;
                        ((DataControlFieldHeaderCell) cell).AbbreviatedText = fields[i].AccessibleHeaderText;
                    }
                    else
                    {
                        BoundField field = fields[i] as BoundField;
                        if (((rowHeaderColumn.Length > 0) && (field != null)) && (field.DataField == rowHeaderColumn))
                        {
                            cell = new DataControlFieldHeaderCell(fields[i]);
                            ((DataControlFieldHeaderCell) cell).Scope = TableHeaderScope.Row;
                        }
                        else
                        {
                            cell = new DataControlFieldCell(fields[i]);
                        }
                    }
                    switch (rowType)
                    {
                        case DataControlRowType.Header:
                            header = DataControlCellType.Header;
                            break;

                        case DataControlRowType.Footer:
                            header = DataControlCellType.Footer;
                            break;

                        default:
                            header = DataControlCellType.DataCell;
                            break;
                    }
                    fields[i].InitializeCell(cell, header, rowState, rowIndex);
                    cells.Add(cell);
                }
            }
        }

        private void InitializeTemplateRow(GridViewRow row, int columnSpan)
        {
            TableCell container = null;
            ITemplate template = null;
            switch (row.RowType)
            {
                case DataControlRowType.Pager:
                    if (this._pagerTemplate != null)
                    {
                        container = new TableCell();
                        template = this._pagerTemplate;
                    }
                    break;

                case DataControlRowType.EmptyDataRow:
                    if (this._emptyDataTemplate == null)
                    {
                        container = new TableCell();
                        string emptyDataText = this.EmptyDataText;
                        if (emptyDataText.Length > 0)
                        {
                            container.Text = emptyDataText;
                        }
                        break;
                    }
                    container = new TableCell();
                    template = this._emptyDataTemplate;
                    break;
            }
            if (container != null)
            {
                if (columnSpan > 1)
                {
                    container.ColumnSpan = columnSpan;
                }
                if (template != null)
                {
                    template.InstantiateIn(container);
                }
                row.Cells.Add(container);
            }
        }

        public virtual bool IsBindableType(Type type)
        {
            return DataBoundControlHelper.IsBindableType(type);
        }

        private void LoadClientIDRowSuffixDataKeysState(object state)
        {
            if (state != null)
            {
                object[] objArray = (object[]) state;
                string[] clientIDRowSuffixInternal = this.ClientIDRowSuffixInternal;
                int length = clientIDRowSuffixInternal.Length;
                this._clientIDRowSuffixArrayList = null;
                for (int i = 0; i < objArray.Length; i++)
                {
                    this.ClientIDRowSuffixArrayList.Add(new DataKey(new OrderedDictionary(length), clientIDRowSuffixInternal));
                    ((IStateManager) this.ClientIDRowSuffixArrayList[i]).LoadViewState(objArray[i]);
                }
            }
        }

        protected internal override void LoadControlState(object savedState)
        {
            this._editIndex = -1;
            this._pageIndex = 0;
            this._selectedIndex = -1;
            this._sortExpression = string.Empty;
            this._sortDirection = System.Web.UI.WebControls.SortDirection.Ascending;
            this._dataKeyNames = new string[0];
            this._pageCount = -1;
            object[] objArray = savedState as object[];
            if (objArray != null)
            {
                base.LoadControlState(objArray[0]);
                if (objArray[1] != null)
                {
                    this._editIndex = (int) objArray[1];
                }
                if (objArray[2] != null)
                {
                    this._pageIndex = (int) objArray[2];
                }
                if (objArray[3] != null)
                {
                    this._selectedIndex = (int) objArray[3];
                }
                if (objArray[4] != null)
                {
                    this._sortExpression = (string) objArray[4];
                }
                if (objArray[5] != null)
                {
                    this._sortDirection = (System.Web.UI.WebControls.SortDirection) objArray[5];
                }
                if (objArray[6] != null)
                {
                    this._dataKeyNames = (string[]) objArray[6];
                }
                if (objArray[7] != null)
                {
                    this.LoadDataKeysState(objArray[7]);
                }
                if (objArray[8] != null)
                {
                    this._pageCount = (int) objArray[8];
                }
                if (((objArray[9] != null) && (this._dataKeyNames != null)) && (this._dataKeyNames.Length > 0))
                {
                    this._persistedDataKey = new DataKey(new OrderedDictionary(this._dataKeyNames.Length), this._dataKeyNames);
                    ((IStateManager) this._persistedDataKey).LoadViewState(objArray[9]);
                }
                if (objArray[10] != null)
                {
                    this._clientIDRowSuffix = (string[]) objArray[10];
                }
                if (objArray[11] != null)
                {
                    this.LoadClientIDRowSuffixDataKeysState(objArray[11]);
                }
            }
            else
            {
                base.LoadControlState(null);
            }
        }

        private void LoadDataKeysState(object state)
        {
            if (state != null)
            {
                object[] objArray = (object[]) state;
                string[] dataKeyNamesInternal = this.DataKeyNamesInternal;
                int length = dataKeyNamesInternal.Length;
                this.ClearDataKeys();
                for (int i = 0; i < objArray.Length; i++)
                {
                    this.DataKeysArrayList.Add(new DataKey(new OrderedDictionary(length), dataKeyNamesInternal));
                    ((IStateManager) this.DataKeysArrayList[i]).LoadViewState(objArray[i]);
                }
            }
        }

        private bool LoadHiddenFieldState(string pageIndex, string sortDirection, string sortExpressionSerialized, string dataKeysSerialized)
        {
            bool flag = false;
            int num = int.Parse(pageIndex, CultureInfo.InvariantCulture);
            System.Web.UI.WebControls.SortDirection direction = (System.Web.UI.WebControls.SortDirection) int.Parse(sortDirection, CultureInfo.InvariantCulture);
            string str = string.Empty;
            object state = null;
            if (!string.IsNullOrEmpty(sortExpressionSerialized) || !string.IsNullOrEmpty(dataKeysSerialized))
            {
                if (this.Page == null)
                {
                    throw new InvalidOperationException();
                }
                IStateFormatter stateFormatter = this.StateFormatter;
                if (!string.IsNullOrEmpty(sortExpressionSerialized))
                {
                    str = (string) stateFormatter.Deserialize(sortExpressionSerialized);
                }
                if (!string.IsNullOrEmpty(dataKeysSerialized))
                {
                    state = stateFormatter.Deserialize(dataKeysSerialized);
                }
            }
            if (((this._pageIndex != num) || (this._sortDirection != direction)) || (this._sortExpression != str))
            {
                flag = true;
                this._pageIndex = num;
                this._sortExpression = str;
                this._sortDirection = direction;
                if (state == null)
                {
                    return flag;
                }
                if (this._dataKeysArrayList != null)
                {
                    this._dataKeysArrayList.Clear();
                }
                this.LoadDataKeysState(state);
            }
            return flag;
        }

        protected override void LoadViewState(object savedState)
        {
            if (savedState != null)
            {
                object[] objArray = (object[]) savedState;
                base.LoadViewState(objArray[0]);
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
                    ((IStateManager) this.RowStyle).LoadViewState(objArray[5]);
                }
                if (objArray[6] != null)
                {
                    ((IStateManager) this.AlternatingRowStyle).LoadViewState(objArray[6]);
                }
                if (objArray[7] != null)
                {
                    ((IStateManager) this.SelectedRowStyle).LoadViewState(objArray[7]);
                }
                if (objArray[8] != null)
                {
                    ((IStateManager) this.EditRowStyle).LoadViewState(objArray[8]);
                }
                if (objArray[9] != null)
                {
                    ((IStateManager) this.PagerSettings).LoadViewState(objArray[9]);
                }
                if (objArray[10] != null)
                {
                    OrderedDictionaryStateHelper.LoadViewState((OrderedDictionary) this.BoundFieldValues, (ArrayList) objArray[10]);
                }
                if (objArray[11] != null)
                {
                    ((IStateManager) base.ControlStyle).LoadViewState(objArray[11]);
                }
                if (objArray[12] != null)
                {
                    object[] objArray2 = (object[]) objArray[12];
                    int length = objArray2.Length;
                    this._autoGenFieldProps = new ArrayList();
                    for (int i = 0; i < length; i++)
                    {
                        AutoGeneratedFieldProperties properties = new AutoGeneratedFieldProperties();
                        ((IStateManager) properties).TrackViewState();
                        ((IStateManager) properties).LoadViewState(objArray2[i]);
                        this._autoGenFieldProps.Add(properties);
                    }
                }
                if (objArray[13] != null)
                {
                    ((IStateManager) this.SortedAscendingCellStyle).LoadViewState(objArray[13]);
                }
                if (objArray[14] != null)
                {
                    ((IStateManager) this.SortedDescendingCellStyle).LoadViewState(objArray[14]);
                }
                if (objArray[15] != null)
                {
                    ((IStateManager) this.SortedAscendingHeaderStyle).LoadViewState(objArray[15]);
                }
                if (objArray[0x10] != null)
                {
                    ((IStateManager) this.SortedDescendingHeaderStyle).LoadViewState(objArray[0x10]);
                }
            }
            else
            {
                base.LoadViewState(null);
            }
        }

        protected override bool OnBubbleEvent(object source, EventArgs e)
        {
            bool causesValidation = false;
            string validationGroup = string.Empty;
            GridViewCommandEventArgs args = e as GridViewCommandEventArgs;
            if (args != null)
            {
                IButtonControl commandSource = args.CommandSource as IButtonControl;
                if (commandSource != null)
                {
                    causesValidation = commandSource.CausesValidation;
                    validationGroup = commandSource.ValidationGroup;
                }
            }
            return this.HandleEvent(e, causesValidation, validationGroup);
        }

        protected override void OnDataPropertyChanged()
        {
            this._storedDataValid = false;
            base.OnDataPropertyChanged();
        }

        protected override void OnDataSourceViewChanged(object sender, EventArgs e)
        {
            this.ClearDataKeys();
            base.OnDataSourceViewChanged(sender, e);
        }

        private void OnFieldsChanged(object sender, EventArgs e)
        {
            if (base.Initialized)
            {
                base.RequiresDataBinding = true;
            }
        }

        protected internal override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            if (this.Page != null)
            {
                if ((this.DataKeyNames.Length > 0) && !this.AutoGenerateColumns)
                {
                    this.Page.RegisterRequiresViewStateEncryption();
                }
                this.Page.RegisterRequiresControlState(this);
            }
        }

        protected virtual void OnPageIndexChanged(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EventPageIndexChanged];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnPageIndexChanging(GridViewPageEventArgs e)
        {
            bool isBoundUsingDataSourceID = base.IsBoundUsingDataSourceID;
            GridViewPageEventHandler handler = (GridViewPageEventHandler) base.Events[EventPageIndexChanging];
            if (handler != null)
            {
                handler(this, e);
            }
            else if (!isBoundUsingDataSourceID && !e.Cancel)
            {
                throw new HttpException(System.Web.SR.GetString("GridView_UnhandledEvent", new object[] { this.ID, "PageIndexChanging" }));
            }
        }

        protected override void OnPagePreLoad(object sender, EventArgs e)
        {
            if (((this.Page != null) && !this.Page.IsCallback) && (this.Page.RequestValueCollection != null))
            {
                string str = "__gv" + this.ClientID + "__hidden";
                string str2 = this.Page.RequestValueCollection[str];
                if (!string.IsNullOrEmpty(str2) && this.ParseHiddenFieldState(str2))
                {
                    this._editIndex = -1;
                    base.RequiresDataBinding = true;
                }
            }
            base.OnPagePreLoad(sender, e);
        }

        private void OnPagerPropertyChanged(object sender, EventArgs e)
        {
            if (base.Initialized)
            {
                base.RequiresDataBinding = true;
            }
        }

        protected internal override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if (this.DetermineRenderClientScript() && (this.Page != null))
            {
                string context = "__gv" + this.ClientID;
                ClientScriptManager clientScript = this.Page.ClientScript;
                clientScript.RegisterClientScriptResource(typeof(GridView), "GridView.js");
                string str2 = clientScript.GetCallbackEventReference(this, context + ".getHiddenFieldContents(arg)", "GridView_OnCallback", context);
                string hiddenFieldName = context + "__hidden";
                clientScript.RegisterHiddenField(hiddenFieldName, string.Empty);
                string str4 = this.StateFormatter.Serialize(this.SortExpression);
                string script = string.Format(CultureInfo.InvariantCulture, "\r\nvar {0} = new GridView();\r\n{0}.stateField = document.getElementById('{1}');\r\n{0}.panelElement = document.getElementById('{0}__div');\r\n{0}.pageIndex = {3};\r\n{0}.sortExpression = \"{4}\";\r\n{0}.sortDirection = {5};\r\n{0}.setStateField();\r\n{0}.callback = function(arg) {{\r\n    {2};\r\n}};", new object[] { context, hiddenFieldName, str2, this.PageIndex, str4, (int) this.SortDirection });
                clientScript.RegisterStartupScript(typeof(GridView), context, script, true);
            }
        }

        protected virtual void OnRowCancelingEdit(GridViewCancelEditEventArgs e)
        {
            bool isBoundUsingDataSourceID = base.IsBoundUsingDataSourceID;
            GridViewCancelEditEventHandler handler = (GridViewCancelEditEventHandler) base.Events[EventRowCancelingEdit];
            if (handler != null)
            {
                handler(this, e);
            }
            else if (!isBoundUsingDataSourceID && !e.Cancel)
            {
                throw new HttpException(System.Web.SR.GetString("GridView_UnhandledEvent", new object[] { this.ID, "RowCancelingEdit" }));
            }
        }

        protected virtual void OnRowCommand(GridViewCommandEventArgs e)
        {
            GridViewCommandEventHandler handler = (GridViewCommandEventHandler) base.Events[EventRowCommand];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnRowCreated(GridViewRowEventArgs e)
        {
            GridViewRowEventHandler handler = (GridViewRowEventHandler) base.Events[EventRowCreated];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnRowDataBound(GridViewRowEventArgs e)
        {
            GridViewRowEventHandler handler = (GridViewRowEventHandler) base.Events[EventRowDataBound];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnRowDeleted(GridViewDeletedEventArgs e)
        {
            GridViewDeletedEventHandler handler = (GridViewDeletedEventHandler) base.Events[EventRowDeleted];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnRowDeleting(GridViewDeleteEventArgs e)
        {
            bool isBoundUsingDataSourceID = base.IsBoundUsingDataSourceID;
            GridViewDeleteEventHandler handler = (GridViewDeleteEventHandler) base.Events[EventRowDeleting];
            if (handler != null)
            {
                handler(this, e);
            }
            else if (!isBoundUsingDataSourceID && !e.Cancel)
            {
                throw new HttpException(System.Web.SR.GetString("GridView_UnhandledEvent", new object[] { this.ID, "RowDeleting" }));
            }
        }

        protected virtual void OnRowEditing(GridViewEditEventArgs e)
        {
            bool isBoundUsingDataSourceID = base.IsBoundUsingDataSourceID;
            GridViewEditEventHandler handler = (GridViewEditEventHandler) base.Events[EventRowEditing];
            if (handler != null)
            {
                handler(this, e);
            }
            else if (!isBoundUsingDataSourceID && !e.Cancel)
            {
                throw new HttpException(System.Web.SR.GetString("GridView_UnhandledEvent", new object[] { this.ID, "RowEditing" }));
            }
        }

        protected virtual void OnRowUpdated(GridViewUpdatedEventArgs e)
        {
            GridViewUpdatedEventHandler handler = (GridViewUpdatedEventHandler) base.Events[EventRowUpdated];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnRowUpdating(GridViewUpdateEventArgs e)
        {
            bool isBoundUsingDataSourceID = base.IsBoundUsingDataSourceID;
            GridViewUpdateEventHandler handler = (GridViewUpdateEventHandler) base.Events[EventRowUpdating];
            if (handler != null)
            {
                handler(this, e);
            }
            else if (!isBoundUsingDataSourceID && !e.Cancel)
            {
                throw new HttpException(System.Web.SR.GetString("GridView_UnhandledEvent", new object[] { this.ID, "RowUpdating" }));
            }
        }

        protected virtual void OnSelectedIndexChanged(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EventSelectedIndexChanged];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnSelectedIndexChanging(GridViewSelectEventArgs e)
        {
            GridViewSelectEventHandler handler = (GridViewSelectEventHandler) base.Events[EventSelectedIndexChanging];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnSorted(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EventSorted];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnSorting(GridViewSortEventArgs e)
        {
            bool isBoundUsingDataSourceID = base.IsBoundUsingDataSourceID;
            GridViewSortEventHandler handler = (GridViewSortEventHandler) base.Events[EventSorting];
            if (handler != null)
            {
                handler(this, e);
            }
            else if (!isBoundUsingDataSourceID && !e.Cancel)
            {
                throw new HttpException(System.Web.SR.GetString("GridView_UnhandledEvent", new object[] { this.ID, "Sorting" }));
            }
        }

        private bool PageIsValidAfterModelException()
        {
            if (this._modelValidationGroup == null)
            {
                return true;
            }
            this.Page.Validate(this._modelValidationGroup);
            return this.Page.IsValid;
        }

        private bool ParseHiddenFieldState(string state)
        {
            string[] strArray = state.Split(new char[] { '|' });
            return ((strArray.Length == 4) && this.LoadHiddenFieldState(strArray[0], strArray[1], strArray[2], strArray[3]));
        }

        protected internal override void PerformDataBinding(IEnumerable data)
        {
            base.PerformDataBinding(data);
            int editIndex = this.EditIndex;
            if ((base.IsBoundUsingDataSourceID && (editIndex != -1)) && ((editIndex < this.Rows.Count) && base.IsViewStateEnabled))
            {
                this.BoundFieldValues.Clear();
                this.ExtractRowValues(this.BoundFieldValues, this.Rows[editIndex], true, false);
            }
            if (this.EnablePersistedSelection)
            {
                string[] dataKeyNamesInternal = this.DataKeyNamesInternal;
                if ((dataKeyNamesInternal == null) || (dataKeyNamesInternal.Length == 0))
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("GridView_PersistedSelectionRequiresDataKeysNames"));
                }
            }
        }

        protected internal virtual void PrepareControlHierarchy()
        {
            if (this.Controls.Count != 0)
            {
                bool controlStyleCreated = base.ControlStyleCreated;
                Table table = (Table) this.Controls[0];
                table.CopyBaseAttributes(this);
                if (controlStyleCreated && !base.ControlStyle.IsEmpty)
                {
                    table.ApplyStyle(base.ControlStyle);
                }
                else
                {
                    table.GridLines = System.Web.UI.WebControls.GridLines.Both;
                    table.CellSpacing = 0;
                }
                table.Caption = this.Caption;
                table.CaptionAlign = this.CaptionAlign;
                TableRowCollection rows = table.Rows;
                Style s = null;
                if (this._alternatingRowStyle != null)
                {
                    s = new TableItemStyle();
                    s.CopyFrom(this._rowStyle);
                    s.CopyFrom(this._alternatingRowStyle);
                }
                else
                {
                    s = this._rowStyle;
                }
                int num = 0;
                bool flag2 = true;
                foreach (GridViewRow row in rows)
                {
                    Style style2;
                    bool flag3;
                    switch (row.RowType)
                    {
                        case DataControlRowType.Header:
                            if (this.ShowHeader && (this._headerStyle != null))
                            {
                                row.MergeStyle(this._headerStyle);
                            }
                            goto Label_0256;

                        case DataControlRowType.Footer:
                            if (this.ShowFooter && (this._footerStyle != null))
                            {
                                row.MergeStyle(this._footerStyle);
                            }
                            goto Label_0256;

                        case DataControlRowType.DataRow:
                            if ((row.RowState & DataControlRowState.Edit) == DataControlRowState.Normal)
                            {
                                goto Label_01D9;
                            }
                            style2 = new TableItemStyle();
                            if ((row.RowIndex % 2) == 0)
                            {
                                break;
                            }
                            style2.CopyFrom(s);
                            goto Label_01A5;

                        case DataControlRowType.Pager:
                            if (row.Visible && (this._pagerStyle != null))
                            {
                                row.MergeStyle(this._pagerStyle);
                            }
                            goto Label_0256;

                        case DataControlRowType.EmptyDataRow:
                            row.MergeStyle(this._emptyDataRowStyle);
                            goto Label_0256;

                        default:
                            goto Label_0256;
                    }
                    style2.CopyFrom(this._rowStyle);
                Label_01A5:
                    if (row.RowIndex == this.SelectedIndex)
                    {
                        style2.CopyFrom(this._selectedRowStyle);
                    }
                    style2.CopyFrom(this._editRowStyle);
                    row.MergeStyle(style2);
                    goto Label_0256;
                Label_01D9:
                    if ((row.RowState & DataControlRowState.Selected) != DataControlRowState.Normal)
                    {
                        Style style3 = new TableItemStyle();
                        if ((row.RowIndex % 2) != 0)
                        {
                            style3.CopyFrom(s);
                        }
                        else
                        {
                            style3.CopyFrom(this._rowStyle);
                        }
                        style3.CopyFrom(this._selectedRowStyle);
                        row.MergeStyle(style3);
                    }
                    else if ((row.RowState & DataControlRowState.Alternate) != DataControlRowState.Normal)
                    {
                        row.MergeStyle(s);
                    }
                    else
                    {
                        row.MergeStyle(this._rowStyle);
                    }
                Label_0256:
                    flag3 = ((row.RowState & DataControlRowState.Selected) == DataControlRowState.Normal) || (((row.RowState & DataControlRowState.Selected) != DataControlRowState.Normal) && (this._selectedRowStyle == null));
                    if ((row.RowType != DataControlRowType.Pager) && (row.RowType != DataControlRowType.EmptyDataRow))
                    {
                        foreach (TableCell cell in row.Cells)
                        {
                            DataControlFieldCell cell2 = cell as DataControlFieldCell;
                            if (cell2 != null)
                            {
                                DataControlField containingField = cell2.ContainingField;
                                if (containingField != null)
                                {
                                    if (!containingField.Visible)
                                    {
                                        cell.Visible = false;
                                        continue;
                                    }
                                    if ((row.RowType == DataControlRowType.DataRow) && flag2)
                                    {
                                        num++;
                                    }
                                    Style headerStyleInternal = null;
                                    switch (row.RowType)
                                    {
                                        case DataControlRowType.Header:
                                            headerStyleInternal = containingField.HeaderStyleInternal;
                                            this.ApplySortingStyle(cell, containingField, this._sortedAscendingHeaderStyle, this._sortedDescendingHeaderStyle);
                                            break;

                                        case DataControlRowType.Footer:
                                            headerStyleInternal = containingField.FooterStyleInternal;
                                            break;

                                        case DataControlRowType.DataRow:
                                            headerStyleInternal = containingField.ItemStyleInternal;
                                            if (flag3)
                                            {
                                                this.ApplySortingStyle(cell, containingField, this._sortedAscendingCellStyle, this._sortedDescendingCellStyle);
                                            }
                                            break;

                                        default:
                                            headerStyleInternal = containingField.ItemStyleInternal;
                                            break;
                                    }
                                    if (headerStyleInternal != null)
                                    {
                                        cell.MergeStyle(headerStyleInternal);
                                    }
                                    if (row.RowType == DataControlRowType.DataRow)
                                    {
                                        foreach (Control control in cell.Controls)
                                        {
                                            WebControl control2 = control as WebControl;
                                            Style controlStyleInternal = containingField.ControlStyleInternal;
                                            if (((control2 != null) && (controlStyleInternal != null)) && !controlStyleInternal.IsEmpty)
                                            {
                                                control2.ControlStyle.CopyFrom(controlStyleInternal);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if (row.RowType == DataControlRowType.DataRow)
                        {
                            flag2 = false;
                        }
                    }
                }
                if ((this.Rows.Count > 0) && (num != this.Rows[0].Cells.Count))
                {
                    if ((this._topPagerRow != null) && (this._topPagerRow.Cells.Count > 0))
                    {
                        this._topPagerRow.Cells[0].ColumnSpan = num;
                    }
                    if ((this._bottomPagerRow != null) && (this._bottomPagerRow.Cells.Count > 0))
                    {
                        this._bottomPagerRow.Cells[0].ColumnSpan = num;
                    }
                }
            }
        }

        protected virtual void RaiseCallbackEvent(string eventArgument)
        {
            string[] strArray = eventArgument.Split(new char[] { '|' });
            IStateFormatter stateFormatter = this.StateFormatter;
            base.ValidateEvent(this.UniqueID, "\"" + strArray[0] + "|" + strArray[1] + "|" + strArray[2] + "|" + strArray[3] + "\"");
            this.LoadHiddenFieldState(strArray[4], strArray[5], strArray[6], strArray[7]);
            int num = int.Parse(strArray[0], CultureInfo.InvariantCulture);
            string serializedState = strArray[2];
            int.Parse(strArray[1], CultureInfo.InvariantCulture);
            if (num == this.PageIndex)
            {
                System.Web.UI.WebControls.SortDirection ascending = System.Web.UI.WebControls.SortDirection.Ascending;
                string str2 = (string) stateFormatter.Deserialize(serializedState);
                if ((str2 == this.SortExpressionInternal) && (this.SortDirectionInternal == System.Web.UI.WebControls.SortDirection.Ascending))
                {
                    ascending = System.Web.UI.WebControls.SortDirection.Descending;
                }
                this.SortExpressionInternal = str2;
                this.SortDirectionInternal = ascending;
                this._pageIndex = 0;
            }
            else
            {
                this.EditIndex = -1;
                this._pageIndex = num;
            }
            this.DataBind();
        }

        protected virtual void RaisePostBackEvent(string eventArgument)
        {
            base.ValidateEvent(this.UniqueID, eventArgument);
            int index = eventArgument.IndexOf('$');
            if (index >= 0)
            {
                CommandEventArgs originalArgs = new CommandEventArgs(eventArgument.Substring(0, index), eventArgument.Substring(index + 1));
                GridViewCommandEventArgs e = new GridViewCommandEventArgs(null, this, originalArgs);
                this.HandleEvent(e, false, string.Empty);
            }
        }

        protected internal override void Render(HtmlTextWriter writer)
        {
            this.Render(writer, !base.DesignMode);
        }

        private void Render(HtmlTextWriter writer, bool renderPanel)
        {
            if (this.Page != null)
            {
                this.Page.VerifyRenderingInServerForm(this);
            }
            this.PrepareControlHierarchy();
            if (renderPanel)
            {
                string clientID = this.ClientID;
                if (this.DetermineRenderClientScript())
                {
                    if (clientID == null)
                    {
                        throw new HttpException(System.Web.SR.GetString("GridView_MustBeParented"));
                    }
                    StringBuilder builder = new StringBuilder("__gv", clientID.Length + 9);
                    builder.Append(clientID);
                    builder.Append("__div");
                    writer.AddAttribute(HtmlTextWriterAttribute.Id, builder.ToString(), true);
                }
                writer.RenderBeginTag(HtmlTextWriterTag.Div);
            }
            this.RenderContents(writer);
            if (renderPanel)
            {
                writer.RenderEndTag();
            }
        }

        private void RenderTableContents(HtmlTextWriter writer)
        {
            this.Render(writer, false);
        }

        private void ResetModelValidationGroup(bool causesValidation, string validationGroup)
        {
            this._modelValidationGroup = null;
            if (causesValidation)
            {
                this.Page.Validate(validationGroup);
                if (this.EnableModelValidation)
                {
                    this._modelValidationGroup = validationGroup;
                }
            }
        }

        private void ResetPersistedSelectedIndex()
        {
            if (this.EnablePersistedSelection && (this._persistedDataKey != null))
            {
                this._selectedIndex = -1;
            }
        }

        private object SaveClientIDRowSuffixDataKeysState()
        {
            object obj2 = new object();
            int count = 0;
            if ((this._clientIDRowSuffixArrayList != null) && (this._clientIDRowSuffixArrayList.Count > 0))
            {
                count = this._clientIDRowSuffixArrayList.Count;
                obj2 = new object[count];
                for (int i = 0; i < count; i++)
                {
                    ((object[]) obj2)[i] = ((IStateManager) this._clientIDRowSuffixArrayList[i]).SaveViewState();
                }
            }
            if ((this._clientIDRowSuffixArrayList != null) && (count != 0))
            {
                return obj2;
            }
            return null;
        }

        protected internal override object SaveControlState()
        {
            object obj2 = base.SaveControlState();
            if (((((obj2 != null) || (this._pageIndex != 0)) || ((this._editIndex != -1) || (this._selectedIndex != -1))) || (((this._sortExpression != null) && (this._sortExpression.Length != 0)) || (this._sortDirection != System.Web.UI.WebControls.SortDirection.Ascending))) || ((((this._dataKeyNames != null) && (this._dataKeyNames.Length != 0)) || ((this._dataKeysArrayList != null) && (this._dataKeysArrayList.Count > 0))) || (this._pageCount != -1)))
            {
                return new object[] { obj2, ((this._editIndex == -1) ? null : ((object) this._editIndex)), ((this._pageIndex == 0) ? null : ((object) this._pageIndex)), ((this._selectedIndex == -1) ? null : ((object) this._selectedIndex)), (((this._sortExpression == null) || (this._sortExpression.Length == 0)) ? null : this._sortExpression), ((this._sortDirection == System.Web.UI.WebControls.SortDirection.Ascending) ? null : ((object) ((int) this._sortDirection))), (((this._dataKeyNames == null) || (this._dataKeyNames.Length == 0)) ? null : this._dataKeyNames), this.SaveDataKeysState(), this._pageCount, ((this._persistedDataKey == null) ? null : ((IStateManager) this._persistedDataKey).SaveViewState()), (((this._clientIDRowSuffix == null) || (this._clientIDRowSuffix.Length == 0)) ? null : this._clientIDRowSuffix), this.SaveClientIDRowSuffixDataKeysState() };
            }
            return true;
        }

        private object SaveDataKeysState()
        {
            object obj2 = new object();
            int count = 0;
            if ((this._dataKeysArrayList != null) && (this._dataKeysArrayList.Count > 0))
            {
                count = this._dataKeysArrayList.Count;
                obj2 = new object[count];
                for (int i = 0; i < count; i++)
                {
                    ((object[]) obj2)[i] = ((IStateManager) this._dataKeysArrayList[i]).SaveViewState();
                }
            }
            if ((this._dataKeysArrayList != null) && (count != 0))
            {
                return obj2;
            }
            return null;
        }

        protected override object SaveViewState()
        {
            object obj2 = base.SaveViewState();
            object obj3 = (this._fieldCollection != null) ? ((IStateManager) this._fieldCollection).SaveViewState() : null;
            object obj4 = (this._pagerStyle != null) ? ((IStateManager) this._pagerStyle).SaveViewState() : null;
            object obj5 = (this._headerStyle != null) ? ((IStateManager) this._headerStyle).SaveViewState() : null;
            object obj6 = (this._footerStyle != null) ? ((IStateManager) this._footerStyle).SaveViewState() : null;
            object obj7 = (this._rowStyle != null) ? ((IStateManager) this._rowStyle).SaveViewState() : null;
            object obj8 = (this._alternatingRowStyle != null) ? ((IStateManager) this._alternatingRowStyle).SaveViewState() : null;
            object obj9 = (this._selectedRowStyle != null) ? ((IStateManager) this._selectedRowStyle).SaveViewState() : null;
            object obj10 = (this._editRowStyle != null) ? ((IStateManager) this._editRowStyle).SaveViewState() : null;
            object obj11 = (this._boundFieldValues != null) ? OrderedDictionaryStateHelper.SaveViewState(this._boundFieldValues) : null;
            object obj12 = (this._pagerSettings != null) ? ((IStateManager) this._pagerSettings).SaveViewState() : null;
            object obj13 = base.ControlStyleCreated ? ((IStateManager) base.ControlStyle).SaveViewState() : null;
            object obj14 = (this._sortedAscendingCellStyle != null) ? ((IStateManager) this._sortedAscendingCellStyle).SaveViewState() : null;
            object obj15 = (this._sortedDescendingCellStyle != null) ? ((IStateManager) this._sortedDescendingCellStyle).SaveViewState() : null;
            object obj16 = (this._sortedAscendingHeaderStyle != null) ? ((IStateManager) this._sortedAscendingHeaderStyle).SaveViewState() : null;
            object obj17 = (this._sortedDescendingHeaderStyle != null) ? ((IStateManager) this._sortedDescendingHeaderStyle).SaveViewState() : null;
            object obj18 = null;
            if (this._autoGenFieldProps != null)
            {
                int count = this._autoGenFieldProps.Count;
                object[] objArray = new object[count];
                for (int i = 0; i < count; i++)
                {
                    objArray[i] = ((IStateManager) this._autoGenFieldProps[i]).SaveViewState();
                }
                obj18 = objArray;
            }
            return new object[] { 
                obj2, obj3, obj4, obj5, obj6, obj7, obj8, obj9, obj10, obj12, obj11, obj13, obj18, obj14, obj15, obj16, 
                obj17
             };
        }

        private void SelectCallback(IEnumerable data)
        {
            throw new HttpException(System.Web.SR.GetString("DataBoundControl_DataSourceDoesntSupportPaging", new object[] { this.DataSourceID }));
        }

        public void SelectRow(int rowIndex)
        {
            this.HandleSelect(rowIndex);
        }

        public void SetEditRow(int rowIndex)
        {
            this.HandleEdit(rowIndex);
        }

        public void SetPageIndex(int rowIndex)
        {
            this.HandlePage(rowIndex);
        }

        private void SetPersistedDataKey(int dataItemIndex, DataKey currentKey)
        {
            if (this._persistedDataKey == null)
            {
                if (this._selectedIndex == dataItemIndex)
                {
                    this._persistedDataKey = currentKey;
                }
            }
            else if (this._persistedDataKey.Equals(currentKey))
            {
                this._selectedIndex = dataItemIndex;
            }
        }

        public virtual void Sort(string sortExpression, System.Web.UI.WebControls.SortDirection sortDirection)
        {
            this.HandleSort(sortExpression, sortDirection);
        }

        private void StoreEnumerator(IEnumerator dataSource, object firstDataRow)
        {
            this._storedData = dataSource;
            this._firstDataRow = firstDataRow;
            this._storedDataValid = true;
        }

        string ICallbackEventHandler.GetCallbackResult()
        {
            return this.GetCallbackResult();
        }

        void ICallbackEventHandler.RaiseCallbackEvent(string eventArgument)
        {
            this.RaiseCallbackEvent(eventArgument);
        }

        void IPostBackEventHandler.RaisePostBackEvent(string eventArgument)
        {
            this.RaisePostBackEvent(eventArgument);
        }

        string ICallbackContainer.GetCallbackScript(IButtonControl buttonControl, string argument)
        {
            return this.GetCallbackScript(buttonControl, argument);
        }

        PostBackOptions IPostBackContainer.GetPostBackOptions(IButtonControl buttonControl)
        {
            if (buttonControl == null)
            {
                throw new ArgumentNullException("buttonControl");
            }
            if (buttonControl.CausesValidation)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("CannotUseParentPostBackWhenValidating", new object[] { base.GetType().Name, this.ID }));
            }
            return new PostBackOptions(this, buttonControl.CommandName + "$" + buttonControl.CommandArgument) { RequiresJavaScriptProtocol = true };
        }

        protected override void TrackViewState()
        {
            base.TrackViewState();
            if (this._fieldCollection != null)
            {
                ((IStateManager) this._fieldCollection).TrackViewState();
            }
            if (this._pagerStyle != null)
            {
                ((IStateManager) this._pagerStyle).TrackViewState();
            }
            if (this._headerStyle != null)
            {
                ((IStateManager) this._headerStyle).TrackViewState();
            }
            if (this._footerStyle != null)
            {
                ((IStateManager) this._footerStyle).TrackViewState();
            }
            if (this._rowStyle != null)
            {
                ((IStateManager) this._rowStyle).TrackViewState();
            }
            if (this._sortedAscendingCellStyle != null)
            {
                ((IStateManager) this._sortedAscendingCellStyle).TrackViewState();
            }
            if (this._sortedDescendingCellStyle != null)
            {
                ((IStateManager) this._sortedDescendingCellStyle).TrackViewState();
            }
            if (this._sortedAscendingHeaderStyle != null)
            {
                ((IStateManager) this._sortedAscendingHeaderStyle).TrackViewState();
            }
            if (this._sortedDescendingHeaderStyle != null)
            {
                ((IStateManager) this._sortedDescendingHeaderStyle).TrackViewState();
            }
            if (this._alternatingRowStyle != null)
            {
                ((IStateManager) this._alternatingRowStyle).TrackViewState();
            }
            if (this._selectedRowStyle != null)
            {
                ((IStateManager) this._selectedRowStyle).TrackViewState();
            }
            if (this._editRowStyle != null)
            {
                ((IStateManager) this._editRowStyle).TrackViewState();
            }
            if (this._pagerSettings != null)
            {
                ((IStateManager) this._pagerSettings).TrackViewState();
            }
            if (base.ControlStyleCreated)
            {
                ((IStateManager) base.ControlStyle).TrackViewState();
            }
            if (this._dataKeyArray != null)
            {
                ((IStateManager) this._dataKeyArray).TrackViewState();
            }
        }

        public virtual void UpdateRow(int rowIndex, bool causesValidation)
        {
            this.ResetModelValidationGroup(causesValidation, string.Empty);
            this.HandleUpdate(null, rowIndex, causesValidation);
        }

        [WebSysDescription("GridView_AllowPaging"), WebCategory("Paging"), DefaultValue(false)]
        public virtual bool AllowPaging
        {
            get
            {
                object obj2 = this.ViewState["AllowPaging"];
                return ((obj2 != null) && ((bool) obj2));
            }
            set
            {
                bool allowPaging = this.AllowPaging;
                if (value != allowPaging)
                {
                    this.ViewState["AllowPaging"] = value;
                    if (base.Initialized)
                    {
                        base.RequiresDataBinding = true;
                    }
                }
            }
        }

        [DefaultValue(false), WebCategory("Behavior"), WebSysDescription("GridView_AllowSorting")]
        public virtual bool AllowSorting
        {
            get
            {
                object obj2 = this.ViewState["AllowSorting"];
                return ((obj2 != null) && ((bool) obj2));
            }
            set
            {
                bool allowSorting = this.AllowSorting;
                if (value != allowSorting)
                {
                    this.ViewState["AllowSorting"] = value;
                    if (base.Initialized)
                    {
                        base.RequiresDataBinding = true;
                    }
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content), WebCategory("Styles"), NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty), WebSysDescription("GridView_AlternatingRowStyle")]
        public TableItemStyle AlternatingRowStyle
        {
            get
            {
                if (this._alternatingRowStyle == null)
                {
                    this._alternatingRowStyle = new TableItemStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._alternatingRowStyle).TrackViewState();
                    }
                }
                return this._alternatingRowStyle;
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
                bool autoGenerateColumns = this.AutoGenerateColumns;
                if (value != autoGenerateColumns)
                {
                    this.ViewState["AutoGenerateColumns"] = value;
                    if (base.Initialized)
                    {
                        base.RequiresDataBinding = true;
                    }
                }
            }
        }

        [DefaultValue(false), WebCategory("Behavior"), WebSysDescription("GridView_AutoGenerateDeleteButton")]
        public virtual bool AutoGenerateDeleteButton
        {
            get
            {
                object obj2 = this.ViewState["AutoGenerateDeleteButton"];
                return ((obj2 != null) && ((bool) obj2));
            }
            set
            {
                bool autoGenerateDeleteButton = this.AutoGenerateDeleteButton;
                if (value != autoGenerateDeleteButton)
                {
                    this.ViewState["AutoGenerateDeleteButton"] = value;
                    if (base.Initialized)
                    {
                        base.RequiresDataBinding = true;
                    }
                }
            }
        }

        [WebSysDescription("GridView_AutoGenerateEditButton"), WebCategory("Behavior"), DefaultValue(false)]
        public virtual bool AutoGenerateEditButton
        {
            get
            {
                object obj2 = this.ViewState["AutoGenerateEditButton"];
                return ((obj2 != null) && ((bool) obj2));
            }
            set
            {
                bool autoGenerateEditButton = this.AutoGenerateEditButton;
                if (value != autoGenerateEditButton)
                {
                    this.ViewState["AutoGenerateEditButton"] = value;
                    if (base.Initialized)
                    {
                        base.RequiresDataBinding = true;
                    }
                }
            }
        }

        [WebCategory("Behavior"), WebSysDescription("GridView_AutoGenerateSelectButton"), DefaultValue(false)]
        public virtual bool AutoGenerateSelectButton
        {
            get
            {
                object obj2 = this.ViewState["AutoGenerateSelectButton"];
                return ((obj2 != null) && ((bool) obj2));
            }
            set
            {
                bool autoGenerateSelectButton = this.AutoGenerateSelectButton;
                if (value != autoGenerateSelectButton)
                {
                    this.ViewState["AutoGenerateSelectButton"] = value;
                    if (base.Initialized)
                    {
                        base.RequiresDataBinding = true;
                    }
                }
            }
        }

        [WebSysDescription("WebControl_BackImageUrl"), DefaultValue(""), WebCategory("Appearance"), Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), UrlProperty]
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

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public virtual GridViewRow BottomPagerRow
        {
            get
            {
                if (this._bottomPagerRow == null)
                {
                    this.EnsureChildControls();
                }
                return this._bottomPagerRow;
            }
        }

        private IOrderedDictionary BoundFieldValues
        {
            get
            {
                if (this._boundFieldValues == null)
                {
                    int count = this.Columns.Count;
                    if (this.AutoGenerateColumns)
                    {
                        count += 10;
                    }
                    this._boundFieldValues = new OrderedDictionary(count);
                }
                return this._boundFieldValues;
            }
        }

        [WebCategory("Accessibility"), Localizable(true), DefaultValue(""), WebSysDescription("DataControls_Caption")]
        public virtual string Caption
        {
            get
            {
                string str = (string) this.ViewState["Caption"];
                if (str == null)
                {
                    return string.Empty;
                }
                return str;
            }
            set
            {
                this.ViewState["Caption"] = value;
            }
        }

        [WebCategory("Accessibility"), DefaultValue(0), WebSysDescription("WebControl_CaptionAlign")]
        public virtual TableCaptionAlign CaptionAlign
        {
            get
            {
                object obj2 = this.ViewState["CaptionAlign"];
                if (obj2 == null)
                {
                    return TableCaptionAlign.NotSet;
                }
                return (TableCaptionAlign) obj2;
            }
            set
            {
                if ((value < TableCaptionAlign.NotSet) || (value > TableCaptionAlign.Right))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.ViewState["CaptionAlign"] = value;
            }
        }

        [WebCategory("Layout"), WebSysDescription("GridView_CellPadding"), DefaultValue(-1)]
        public virtual int CellPadding
        {
            get
            {
                if (!base.ControlStyleCreated)
                {
                    return -1;
                }
                return ((TableStyle) base.ControlStyle).CellPadding;
            }
            set
            {
                ((TableStyle) base.ControlStyle).CellPadding = value;
            }
        }

        [WebSysDescription("GridView_CellSpacing"), WebCategory("Layout"), DefaultValue(0)]
        public virtual int CellSpacing
        {
            get
            {
                if (!base.ControlStyleCreated)
                {
                    return 0;
                }
                return ((TableStyle) base.ControlStyle).CellSpacing;
            }
            set
            {
                ((TableStyle) base.ControlStyle).CellSpacing = value;
            }
        }

        [WebCategory("Data"), DefaultValue((string) null), TypeConverter(typeof(StringArrayConverter))]
        public virtual string[] ClientIDRowSuffix
        {
            get
            {
                object obj2 = this._clientIDRowSuffix;
                if (obj2 != null)
                {
                    return (string[]) ((string[]) obj2).Clone();
                }
                return new string[0];
            }
            set
            {
                if (!DataBoundControlHelper.CompareStringArrays(value, this.ClientIDRowSuffixInternal))
                {
                    if (value != null)
                    {
                        this._clientIDRowSuffix = (string[]) value.Clone();
                    }
                    else
                    {
                        this._clientIDRowSuffix = null;
                    }
                    this._clientIDRowSuffixArrayList = null;
                    if (base.Initialized)
                    {
                        base.RequiresDataBinding = true;
                    }
                }
            }
        }

        private ArrayList ClientIDRowSuffixArrayList
        {
            get
            {
                if (this._clientIDRowSuffixArrayList == null)
                {
                    this._clientIDRowSuffixArrayList = new ArrayList();
                }
                return this._clientIDRowSuffixArrayList;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public DataKeyArray ClientIDRowSuffixDataKeys
        {
            get
            {
                if (this._clientIDRowSuffixArray == null)
                {
                    this._clientIDRowSuffixArray = new DataKeyArray(this.ClientIDRowSuffixArrayList);
                }
                return this._clientIDRowSuffixArray;
            }
        }

        private string[] ClientIDRowSuffixInternal
        {
            get
            {
                object obj2 = this._clientIDRowSuffix;
                if (obj2 != null)
                {
                    return (string[]) obj2;
                }
                return new string[0];
            }
        }

        [MergableProperty(false), WebCategory("Default"), DefaultValue((string) null), Editor("System.Web.UI.Design.WebControls.DataControlFieldTypeEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), PersistenceMode(PersistenceMode.InnerProperty), WebSysDescription("DataControls_Columns")]
        public virtual DataControlFieldCollection Columns
        {
            get
            {
                if (this._fieldCollection == null)
                {
                    this._fieldCollection = new DataControlFieldCollection();
                    this._fieldCollection.FieldsChanged += new EventHandler(this.OnFieldsChanged);
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._fieldCollection).TrackViewState();
                    }
                }
                return this._fieldCollection;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IAutoFieldGenerator ColumnsGenerator
        {
            get
            {
                return this._columnsGenerator;
            }
            set
            {
                this._columnsGenerator = value;
            }
        }

        [WebSysDescription("DataControls_DataKeyNames"), DefaultValue((string) null), Editor("System.Web.UI.Design.WebControls.DataFieldEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), TypeConverter(typeof(StringArrayConverter)), WebCategory("Data")]
        public virtual string[] DataKeyNames
        {
            get
            {
                object obj2 = this._dataKeyNames;
                if (obj2 != null)
                {
                    return (string[]) ((string[]) obj2).Clone();
                }
                return new string[0];
            }
            set
            {
                if (!DataBoundControlHelper.CompareStringArrays(value, this.DataKeyNamesInternal))
                {
                    if (value != null)
                    {
                        this._dataKeyNames = (string[]) value.Clone();
                    }
                    else
                    {
                        this._dataKeyNames = null;
                    }
                    this.ClearDataKeys();
                    if (base.Initialized)
                    {
                        base.RequiresDataBinding = true;
                    }
                }
            }
        }

        private string[] DataKeyNamesInternal
        {
            get
            {
                object obj2 = this._dataKeyNames;
                if (obj2 != null)
                {
                    return (string[]) obj2;
                }
                return new string[0];
            }
        }

        [WebSysDescription("GridView_DataKeys"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual DataKeyArray DataKeys
        {
            get
            {
                if (this._dataKeyArray == null)
                {
                    this._dataKeyArray = new DataKeyArray(this.DataKeysArrayList);
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._dataKeyArray).TrackViewState();
                    }
                }
                return this._dataKeyArray;
            }
        }

        private ArrayList DataKeysArrayList
        {
            get
            {
                if (this._dataKeysArrayList == null)
                {
                    this._dataKeysArrayList = new ArrayList();
                }
                return this._dataKeysArrayList;
            }
        }

        [WebCategory("Default"), WebSysDescription("GridView_EditIndex"), DefaultValue(-1)]
        public virtual int EditIndex
        {
            get
            {
                return this._editIndex;
            }
            set
            {
                if (value < -1)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                if (this.EditIndex != value)
                {
                    if (value == -1)
                    {
                        this.BoundFieldValues.Clear();
                    }
                    this._editIndex = value;
                    if (base.Initialized)
                    {
                        base.RequiresDataBinding = true;
                    }
                }
            }
        }

        [WebCategory("Styles"), NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty), WebSysDescription("GridView_EditRowStyle"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public TableItemStyle EditRowStyle
        {
            get
            {
                if (this._editRowStyle == null)
                {
                    this._editRowStyle = new TableItemStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._editRowStyle).TrackViewState();
                    }
                }
                return this._editRowStyle;
            }
        }

        [WebSysDescription("GridView_EmptyDataRowStyle"), WebCategory("Styles"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty)]
        public TableItemStyle EmptyDataRowStyle
        {
            get
            {
                if (this._emptyDataRowStyle == null)
                {
                    this._emptyDataRowStyle = new TableItemStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._emptyDataRowStyle).TrackViewState();
                    }
                }
                return this._emptyDataRowStyle;
            }
        }

        [DefaultValue((string) null), Browsable(false), WebSysDescription("View_EmptyDataTemplate"), PersistenceMode(PersistenceMode.InnerProperty), TemplateContainer(typeof(GridViewRow))]
        public virtual ITemplate EmptyDataTemplate
        {
            get
            {
                return this._emptyDataTemplate;
            }
            set
            {
                this._emptyDataTemplate = value;
            }
        }

        [WebSysDescription("View_EmptyDataText"), Localizable(true), WebCategory("Appearance"), DefaultValue("")]
        public virtual string EmptyDataText
        {
            get
            {
                object obj2 = this.ViewState["EmptyDataText"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["EmptyDataText"] = value;
            }
        }

        [WebSysDescription("DataBoundControl_EnableModelValidation"), WebCategory("Behavior"), DefaultValue(true)]
        public virtual bool EnableModelValidation
        {
            get
            {
                object obj2 = this.ViewState["EnableModelValidation"];
                if (obj2 != null)
                {
                    return (bool) obj2;
                }
                return true;
            }
            set
            {
                this.ViewState["EnableModelValidation"] = value;
            }
        }

        [DefaultValue(false), WebSysDescription("GridView_EnablePersistedSelection"), WebCategory("Behavior")]
        public virtual bool EnablePersistedSelection
        {
            get
            {
                object obj2 = this.ViewState["EnablePersistedSelection"];
                return ((obj2 != null) && ((bool) obj2));
            }
            set
            {
                this.ViewState["EnablePersistedSelection"] = value;
            }
        }

        [DefaultValue(false), WebCategory("Behavior"), WebSysDescription("GridView_EnableSortingAndPagingCallbacks")]
        public virtual bool EnableSortingAndPagingCallbacks
        {
            get
            {
                object obj2 = this.ViewState["EnableSortingAndPagingCallbacks"];
                return ((obj2 != null) && ((bool) obj2));
            }
            set
            {
                this.ViewState["EnableSortingAndPagingCallbacks"] = value;
            }
        }

        private int FirstDisplayedPageIndex
        {
            get
            {
                object obj2 = this.ViewState["FirstDisplayedPageIndex"];
                if (obj2 != null)
                {
                    return (int) obj2;
                }
                return -1;
            }
            set
            {
                this.ViewState["FirstDisplayedPageIndex"] = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public virtual GridViewRow FooterRow
        {
            get
            {
                if (this._footerRow == null)
                {
                    this.EnsureChildControls();
                }
                return this._footerRow;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content), DefaultValue((string) null), WebCategory("Styles"), NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty), WebSysDescription("DataControls_FooterStyle")]
        public TableItemStyle FooterStyle
        {
            get
            {
                if (this._footerStyle == null)
                {
                    this._footerStyle = new TableItemStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._footerStyle).TrackViewState();
                    }
                }
                return this._footerStyle;
            }
        }

        [WebCategory("Appearance"), WebSysDescription("DataControls_GridLines"), DefaultValue(3)]
        public virtual System.Web.UI.WebControls.GridLines GridLines
        {
            get
            {
                if (!base.ControlStyleCreated)
                {
                    return System.Web.UI.WebControls.GridLines.Both;
                }
                return ((TableStyle) base.ControlStyle).GridLines;
            }
            set
            {
                ((TableStyle) base.ControlStyle).GridLines = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public virtual GridViewRow HeaderRow
        {
            get
            {
                if (this._headerRow == null)
                {
                    this.EnsureChildControls();
                }
                return this._headerRow;
            }
        }

        [DefaultValue((string) null), WebCategory("Styles"), WebSysDescription("DataControls_HeaderStyle"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty)]
        public TableItemStyle HeaderStyle
        {
            get
            {
                if (this._headerStyle == null)
                {
                    this._headerStyle = new TableItemStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._headerStyle).TrackViewState();
                    }
                }
                return this._headerStyle;
            }
        }

        [WebSysDescription("WebControl_HorizontalAlign"), DefaultValue(0), Category("Layout")]
        public virtual System.Web.UI.WebControls.HorizontalAlign HorizontalAlign
        {
            get
            {
                if (!base.ControlStyleCreated)
                {
                    return System.Web.UI.WebControls.HorizontalAlign.NotSet;
                }
                return ((TableStyle) base.ControlStyle).HorizontalAlign;
            }
            set
            {
                ((TableStyle) base.ControlStyle).HorizontalAlign = value;
            }
        }

        [WebSysDescription("GridView_PageCount"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual int PageCount
        {
            get
            {
                if (this._pageCount < 0)
                {
                    return 0;
                }
                return this._pageCount;
            }
        }

        [Browsable(true), WebSysDescription("GridView_PageIndex"), DefaultValue(0), WebCategory("Paging")]
        public virtual int PageIndex
        {
            get
            {
                return this._pageIndex;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                if (this.PageIndex != value)
                {
                    this._pageIndex = value;
                    if (base.Initialized)
                    {
                        base.RequiresDataBinding = true;
                    }
                }
            }
        }

        [WebCategory("Paging"), WebSysDescription("GridView_PagerSettings"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty)]
        public virtual System.Web.UI.WebControls.PagerSettings PagerSettings
        {
            get
            {
                if (this._pagerSettings == null)
                {
                    this._pagerSettings = new System.Web.UI.WebControls.PagerSettings();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._pagerSettings).TrackViewState();
                    }
                    this._pagerSettings.PropertyChanged += new EventHandler(this.OnPagerPropertyChanged);
                }
                return this._pagerSettings;
            }
        }

        [PersistenceMode(PersistenceMode.InnerProperty), WebSysDescription("WebControl_PagerStyle"), WebCategory("Styles"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true)]
        public TableItemStyle PagerStyle
        {
            get
            {
                if (this._pagerStyle == null)
                {
                    this._pagerStyle = new TableItemStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._pagerStyle).TrackViewState();
                    }
                }
                return this._pagerStyle;
            }
        }

        [WebSysDescription("View_PagerTemplate"), Browsable(false), DefaultValue((string) null), PersistenceMode(PersistenceMode.InnerProperty), TemplateContainer(typeof(GridViewRow))]
        public virtual ITemplate PagerTemplate
        {
            get
            {
                return this._pagerTemplate;
            }
            set
            {
                this._pagerTemplate = value;
            }
        }

        [DefaultValue(10), WebSysDescription("GridView_PageSize"), WebCategory("Paging")]
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
                if (this.PageSize != value)
                {
                    this.ViewState["PageSize"] = value;
                    if (base.Initialized)
                    {
                        base.RequiresDataBinding = true;
                    }
                }
            }
        }

        [DefaultValue(""), WebSysDescription("GridView_RowHeaderColumn"), WebCategory("Accessibility"), TypeConverter("System.Web.UI.Design.DataColumnSelectionConverter, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public virtual string RowHeaderColumn
        {
            get
            {
                object obj2 = this.ViewState["RowHeaderColumn"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["RowHeaderColumn"] = value;
            }
        }

        [WebSysDescription("GridView_Rows"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual GridViewRowCollection Rows
        {
            get
            {
                if (this._rowsCollection == null)
                {
                    if (this._rowsArray == null)
                    {
                        this.EnsureChildControls();
                    }
                    if (this._rowsArray == null)
                    {
                        this._rowsArray = new ArrayList();
                    }
                    this._rowsCollection = new GridViewRowCollection(this._rowsArray);
                }
                return this._rowsCollection;
            }
        }

        [PersistenceMode(PersistenceMode.InnerProperty), WebSysDescription("View_RowStyle"), NotifyParentProperty(true), WebCategory("Styles"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public TableItemStyle RowStyle
        {
            get
            {
                if (this._rowStyle == null)
                {
                    this._rowStyle = new TableItemStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._rowStyle).TrackViewState();
                    }
                }
                return this._rowStyle;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public virtual DataKey SelectedDataKey
        {
            get
            {
                if ((this.DataKeyNamesInternal == null) || (this.DataKeyNamesInternal.Length == 0))
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("GridView_DataKeyNamesMustBeSpecified", new object[] { this.ID }));
                }
                DataKeyArray dataKeys = this.DataKeys;
                int selectedIndex = this.SelectedIndex;
                if (((dataKeys != null) && (selectedIndex < dataKeys.Count)) && (selectedIndex > -1))
                {
                    return dataKeys[selectedIndex];
                }
                return null;
            }
        }

        [WebSysDescription("GridView_SelectedIndex"), Bindable(true), DefaultValue(-1)]
        public virtual int SelectedIndex
        {
            get
            {
                return this._selectedIndex;
            }
            set
            {
                if (value < -1)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                int num = this._selectedIndex;
                this._selectedIndex = value;
                if ((this.DataKeyNamesInternal.Length > 0) && this.EnablePersistedSelection)
                {
                    this.SelectedPersistedDataKey = this.SelectedDataKey;
                }
                if (this._rowsArray != null)
                {
                    GridViewRow row;
                    if ((num != -1) && (this._rowsArray.Count > num))
                    {
                        row = (GridViewRow) this._rowsArray[num];
                        row.RowType = DataControlRowType.DataRow;
                        row.RowState &= ~DataControlRowState.Selected;
                    }
                    if ((value != -1) && (this._rowsArray.Count > value))
                    {
                        row = (GridViewRow) this._rowsArray[value];
                        row.RowState |= DataControlRowState.Selected;
                    }
                }
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual DataKey SelectedPersistedDataKey
        {
            get
            {
                return this._persistedDataKey;
            }
            set
            {
                this._persistedDataKey = value;
                if (base.IsTrackingViewState && (this._persistedDataKey != null))
                {
                    ((IStateManager) this._persistedDataKey).TrackViewState();
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false), WebSysDescription("GridView_SelectedRow")]
        public virtual GridViewRow SelectedRow
        {
            get
            {
                int selectedIndex = this.SelectedIndex;
                GridViewRow row = null;
                if (selectedIndex != -1)
                {
                    row = this.Rows[selectedIndex];
                }
                return row;
            }
        }

        [PersistenceMode(PersistenceMode.InnerProperty), NotifyParentProperty(true), WebCategory("Styles"), WebSysDescription("GridView_SelectedRowStyle"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public TableItemStyle SelectedRowStyle
        {
            get
            {
                if (this._selectedRowStyle == null)
                {
                    this._selectedRowStyle = new TableItemStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._selectedRowStyle).TrackViewState();
                    }
                }
                return this._selectedRowStyle;
            }
        }

        [Browsable(false)]
        public object SelectedValue
        {
            get
            {
                if (this.SelectedDataKey != null)
                {
                    return this.SelectedDataKey.Value;
                }
                return null;
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
                bool showFooter = this.ShowFooter;
                if (value != showFooter)
                {
                    this.ViewState["ShowFooter"] = value;
                    if (base.Initialized)
                    {
                        base.RequiresDataBinding = true;
                    }
                }
            }
        }

        [WebSysDescription("DataControls_ShowHeader"), WebCategory("Appearance"), DefaultValue(true)]
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
                bool showHeader = this.ShowHeader;
                if (value != showHeader)
                {
                    this.ViewState["ShowHeader"] = value;
                    if (base.Initialized)
                    {
                        base.RequiresDataBinding = true;
                    }
                }
            }
        }

        [WebSysDescription("GridView_ShowHeaderWhenEmpty"), DefaultValue(false), WebCategory("Appearance")]
        public virtual bool ShowHeaderWhenEmpty
        {
            get
            {
                object obj2 = this.ViewState["ShowHeaderWhenEmpty"];
                return ((obj2 != null) && ((bool) obj2));
            }
            set
            {
                bool showHeaderWhenEmpty = this.ShowHeaderWhenEmpty;
                if (value != showHeaderWhenEmpty)
                {
                    this.ViewState["ShowHeaderWhenEmpty"] = value;
                    if (base.Initialized)
                    {
                        base.RequiresDataBinding = true;
                    }
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), PersistenceMode(PersistenceMode.InnerProperty), WebSysDescription("GridView_SortDirection"), DefaultValue(0), Browsable(false)]
        public virtual System.Web.UI.WebControls.SortDirection SortDirection
        {
            get
            {
                return this.SortDirectionInternal;
            }
        }

        private System.Web.UI.WebControls.SortDirection SortDirectionInternal
        {
            get
            {
                return this._sortDirection;
            }
            set
            {
                if ((value < System.Web.UI.WebControls.SortDirection.Ascending) || (value > System.Web.UI.WebControls.SortDirection.Descending))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                if (this._sortDirection != value)
                {
                    this._sortDirection = value;
                    if (base.Initialized)
                    {
                        base.RequiresDataBinding = true;
                    }
                }
            }
        }

        [PersistenceMode(PersistenceMode.InnerProperty), NotifyParentProperty(true), WebCategory("Styles"), WebSysDescription("GridView_SortedAscendingCellStyle"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public TableItemStyle SortedAscendingCellStyle
        {
            get
            {
                if (this._sortedAscendingCellStyle == null)
                {
                    this._sortedAscendingCellStyle = new TableItemStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._sortedAscendingCellStyle).TrackViewState();
                    }
                }
                return this._sortedAscendingCellStyle;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty), WebSysDescription("GridView_SortedAscendingHeaderStyle"), WebCategory("Styles")]
        public TableItemStyle SortedAscendingHeaderStyle
        {
            get
            {
                if (this._sortedAscendingHeaderStyle == null)
                {
                    this._sortedAscendingHeaderStyle = new TableItemStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._sortedAscendingHeaderStyle).TrackViewState();
                    }
                }
                return this._sortedAscendingHeaderStyle;
            }
        }

        [WebCategory("Styles"), NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty), WebSysDescription("GridView_SortedDescendingCellStyle"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public TableItemStyle SortedDescendingCellStyle
        {
            get
            {
                if (this._sortedDescendingCellStyle == null)
                {
                    this._sortedDescendingCellStyle = new TableItemStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._sortedDescendingCellStyle).TrackViewState();
                    }
                }
                return this._sortedDescendingCellStyle;
            }
        }

        [WebSysDescription("GridView_SortedDescendingHeaderStyle"), PersistenceMode(PersistenceMode.InnerProperty), WebCategory("Styles"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true)]
        public TableItemStyle SortedDescendingHeaderStyle
        {
            get
            {
                if (this._sortedDescendingHeaderStyle == null)
                {
                    this._sortedDescendingHeaderStyle = new TableItemStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._sortedDescendingHeaderStyle).TrackViewState();
                    }
                }
                return this._sortedDescendingHeaderStyle;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), WebSysDescription("GridView_SortExpression"), Browsable(false)]
        public virtual string SortExpression
        {
            get
            {
                return this.SortExpressionInternal;
            }
        }

        private string SortExpressionInternal
        {
            get
            {
                return this._sortExpression;
            }
            set
            {
                if (this._sortExpression != value)
                {
                    this._sortExpression = value;
                    if (base.Initialized)
                    {
                        base.RequiresDataBinding = true;
                    }
                }
            }
        }

        private IStateFormatter StateFormatter
        {
            get
            {
                if (this._stateFormatter == null)
                {
                    this._stateFormatter = this.Page.CreateStateFormatter();
                }
                return this._stateFormatter;
            }
        }

        DataKeyArray IDataKeysControl.ClientIDRowSuffixDataKeys
        {
            get
            {
                return this.ClientIDRowSuffixDataKeys;
            }
        }

        string[] IDataBoundControl.DataKeyNames
        {
            get
            {
                return this.DataKeyNames;
            }
            set
            {
                this.DataKeyNames = value;
            }
        }

        string IDataBoundControl.DataMember
        {
            get
            {
                return this.DataMember;
            }
            set
            {
                this.DataMember = value;
            }
        }

        object IDataBoundControl.DataSource
        {
            get
            {
                return this.DataSource;
            }
            set
            {
                this.DataSource = value;
            }
        }

        string IDataBoundControl.DataSourceID
        {
            get
            {
                return this.DataSourceID;
            }
            set
            {
                this.DataSourceID = value;
            }
        }

        IDataSource IDataBoundControl.DataSourceObject
        {
            get
            {
                return base.DataSourceObject;
            }
        }

        string[] IDataBoundListControl.ClientIDRowSuffix
        {
            get
            {
                return this.ClientIDRowSuffix;
            }
            set
            {
                this.ClientIDRowSuffix = value;
            }
        }

        DataKeyArray IDataBoundListControl.DataKeys
        {
            get
            {
                return this.DataKeys;
            }
        }

        bool IDataBoundListControl.EnablePersistedSelection
        {
            get
            {
                return this.EnablePersistedSelection;
            }
            set
            {
                this.EnablePersistedSelection = value;
            }
        }

        DataKey IDataBoundListControl.SelectedDataKey
        {
            get
            {
                return this.SelectedDataKey;
            }
        }

        int IDataBoundListControl.SelectedIndex
        {
            get
            {
                return this.SelectedIndex;
            }
            set
            {
                this.SelectedIndex = value;
            }
        }

        IAutoFieldGenerator IFieldControl.FieldsGenerator
        {
            get
            {
                return this.ColumnsGenerator;
            }
            set
            {
                this.ColumnsGenerator = value;
            }
        }

        DataKey IPersistedSelector.DataKey
        {
            get
            {
                return this.SelectedPersistedDataKey;
            }
            set
            {
                this.SelectedPersistedDataKey = value;
            }
        }

        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                if (!this.EnableSortingAndPagingCallbacks)
                {
                    return HtmlTextWriterTag.Table;
                }
                return HtmlTextWriterTag.Div;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public virtual GridViewRow TopPagerRow
        {
            get
            {
                if (this._topPagerRow == null)
                {
                    this.EnsureChildControls();
                }
                return this._topPagerRow;
            }
        }

        [DefaultValue(true), WebSysDescription("Table_UseAccessibleHeader"), WebCategory("Accessibility")]
        public virtual bool UseAccessibleHeader
        {
            get
            {
                object obj2 = this.ViewState["UseAccessibleHeader"];
                if (obj2 != null)
                {
                    return (bool) obj2;
                }
                return true;
            }
            set
            {
                if (this.UseAccessibleHeader != value)
                {
                    this.ViewState["UseAccessibleHeader"] = value;
                    if (base.Initialized)
                    {
                        base.RequiresDataBinding = true;
                    }
                }
            }
        }
    }
}

