namespace System.Web.UI.WebControls
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls.Adapters;
    using System.Web.Util;

    [DataKeyProperty("DataKey"), ToolboxData("<{0}:DetailsView runat=\"server\" Width=\"125px\" Height=\"50px\"></{0}:DetailsView>"), SupportsEventValidation, Designer("System.Web.UI.Design.WebControls.DetailsViewDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), ControlValueProperty("SelectedValue"), DefaultEvent("PageIndexChanging")]
    public class DetailsView : CompositeDataBoundControl, IDataItemContainer, INamingContainer, ICallbackContainer, ICallbackEventHandler, IPostBackEventHandler, IPostBackContainer, IDataBoundItemControl, IDataBoundControl, IFieldControl
    {
        private TableItemStyle _alternatingRowStyle;
        private ArrayList _autoGenFieldProps;
        private DetailsViewRow _bottomPagerRow;
        private OrderedDictionary _boundFieldValues;
        private TableItemStyle _commandRowStyle;
        private object _dataItem;
        private int _dataItemIndex;
        private System.Web.UI.WebControls.DataKey _dataKey;
        private string[] _dataKeyNames;
        private DetailsViewMode _defaultMode;
        private IOrderedDictionary _deleteKeys;
        private IOrderedDictionary _deleteValues;
        private TableItemStyle _editRowStyle;
        private TableItemStyle _emptyDataRowStyle;
        private ITemplate _emptyDataTemplate;
        private DataControlFieldCollection _fieldCollection;
        private TableItemStyle _fieldHeaderStyle;
        private DetailsViewRow _footerRow;
        private TableItemStyle _footerStyle;
        private ITemplate _footerTemplate;
        private DetailsViewRow _headerRow;
        private TableItemStyle _headerStyle;
        private ITemplate _headerTemplate;
        private TableItemStyle _insertRowStyle;
        private IOrderedDictionary _insertValues;
        private OrderedDictionary _keyTable;
        private DetailsViewMode _mode;
        private string _modelValidationGroup;
        private bool _modeSet;
        private int _pageCount;
        private int _pageIndex;
        private System.Web.UI.WebControls.PagerSettings _pagerSettings;
        private TableItemStyle _pagerStyle;
        private ITemplate _pagerTemplate;
        private bool _renderClientScript;
        private bool _renderClientScriptValid;
        private ArrayList _rowsArray;
        private DetailsViewRowCollection _rowsCollection;
        private IAutoFieldGenerator _rowsGenerator;
        private TableItemStyle _rowStyle;
        private DetailsViewRow _topPagerRow;
        private IOrderedDictionary _updateKeys;
        private IOrderedDictionary _updateNewValues;
        private IOrderedDictionary _updateOldValues;
        private bool _useServerPaging;
        private static readonly object EventItemCommand = new object();
        private static readonly object EventItemCreated = new object();
        private static readonly object EventItemDeleted = new object();
        private static readonly object EventItemDeleting = new object();
        private static readonly object EventItemInserted = new object();
        private static readonly object EventItemInserting = new object();
        private static readonly object EventItemUpdated = new object();
        private static readonly object EventItemUpdating = new object();
        private static readonly object EventModeChanged = new object();
        private static readonly object EventModeChanging = new object();
        private static readonly object EventPageIndexChanged = new object();
        private static readonly object EventPageIndexChanging = new object();
        private const string startupScriptFormat = "\r\nvar {0} = new DetailsView();\r\n{0}.stateField = document.getElementById('{1}');\r\n{0}.panelElement = document.getElementById('{0}__div');\r\n{0}.pageIndex = {3};\r\n{0}.setStateField();\r\n{0}.callback = function(arg) {{\r\n    {2};\r\n}};";

        [WebSysDescription("DetailsView_OnItemCommand"), WebCategory("Action")]
        public event DetailsViewCommandEventHandler ItemCommand
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

        [WebCategory("Behavior"), WebSysDescription("DetailsView_OnItemCreated")]
        public event EventHandler ItemCreated
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

        [WebCategory("Action"), WebSysDescription("DataControls_OnItemDeleted")]
        public event DetailsViewDeletedEventHandler ItemDeleted
        {
            add
            {
                base.Events.AddHandler(EventItemDeleted, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventItemDeleted, value);
            }
        }

        [WebSysDescription("DataControls_OnItemDeleting"), WebCategory("Action")]
        public event DetailsViewDeleteEventHandler ItemDeleting
        {
            add
            {
                base.Events.AddHandler(EventItemDeleting, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventItemDeleting, value);
            }
        }

        [WebSysDescription("DataControls_OnItemInserted"), WebCategory("Action")]
        public event DetailsViewInsertedEventHandler ItemInserted
        {
            add
            {
                base.Events.AddHandler(EventItemInserted, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventItemInserted, value);
            }
        }

        [WebSysDescription("DataControls_OnItemInserting"), WebCategory("Action")]
        public event DetailsViewInsertEventHandler ItemInserting
        {
            add
            {
                base.Events.AddHandler(EventItemInserting, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventItemInserting, value);
            }
        }

        [WebCategory("Action"), WebSysDescription("DataControls_OnItemUpdated")]
        public event DetailsViewUpdatedEventHandler ItemUpdated
        {
            add
            {
                base.Events.AddHandler(EventItemUpdated, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventItemUpdated, value);
            }
        }

        [WebSysDescription("DataControls_OnItemUpdating"), WebCategory("Action")]
        public event DetailsViewUpdateEventHandler ItemUpdating
        {
            add
            {
                base.Events.AddHandler(EventItemUpdating, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventItemUpdating, value);
            }
        }

        [WebSysDescription("DetailsView_OnModeChanged"), WebCategory("Action")]
        public event EventHandler ModeChanged
        {
            add
            {
                base.Events.AddHandler(EventModeChanged, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventModeChanged, value);
            }
        }

        [WebSysDescription("DetailsView_OnModeChanging"), WebCategory("Action")]
        public event DetailsViewModeEventHandler ModeChanging
        {
            add
            {
                base.Events.AddHandler(EventModeChanging, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventModeChanging, value);
            }
        }

        [WebSysDescription("DetailsView_OnPageIndexChanged"), WebCategory("Action")]
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

        [WebSysDescription("DetailsView_OnPageIndexChanging"), WebCategory("Action")]
        public event DetailsViewPageEventHandler PageIndexChanging
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

        private string BuildCallbackArgument(int pageIndex)
        {
            return ("\"" + Convert.ToString(pageIndex, CultureInfo.InvariantCulture) + "|\"");
        }

        public void ChangeMode(DetailsViewMode newMode)
        {
            this.Mode = newMode;
        }

        protected virtual AutoGeneratedField CreateAutoGeneratedRow(AutoGeneratedFieldProperties fieldProperties)
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

        protected virtual ICollection CreateAutoGeneratedRows(object dataItem)
        {
            if (dataItem == null)
            {
                return null;
            }
            ArrayList list = new ArrayList();
            PropertyDescriptorCollection properties = null;
            bool flag = true;
            Type type = null;
            this._autoGenFieldProps = new ArrayList();
            if (dataItem != null)
            {
                type = dataItem.GetType();
            }
            if ((dataItem != null) && (dataItem is ICustomTypeDescriptor))
            {
                properties = TypeDescriptor.GetProperties(dataItem);
            }
            else if (type != null)
            {
                if (this.IsBindableType(type))
                {
                    AutoGeneratedFieldProperties fieldProperties = new AutoGeneratedFieldProperties();
                    ((IStateManager) fieldProperties).TrackViewState();
                    fieldProperties.Name = "Item";
                    fieldProperties.DataField = BoundField.ThisExpression;
                    fieldProperties.Type = type;
                    AutoGeneratedField field = this.CreateAutoGeneratedRow(fieldProperties);
                    if (field != null)
                    {
                        list.Add(field);
                        this._autoGenFieldProps.Add(fieldProperties);
                    }
                }
                else
                {
                    properties = TypeDescriptor.GetProperties(type);
                }
            }
            if ((properties != null) && (properties.Count != 0))
            {
                string[] dataKeyNamesInternal = this.DataKeyNamesInternal;
                int length = dataKeyNamesInternal.Length;
                string[] strArray2 = new string[length];
                for (int i = 0; i < length; i++)
                {
                    strArray2[i] = dataKeyNamesInternal[i].ToLowerInvariant();
                }
                foreach (PropertyDescriptor descriptor in properties)
                {
                    Type propertyType = descriptor.PropertyType;
                    if (this.IsBindableType(propertyType))
                    {
                        string name = descriptor.Name;
                        bool flag2 = strArray2.Contains(name.ToLowerInvariant());
                        AutoGeneratedFieldProperties properties2 = new AutoGeneratedFieldProperties();
                        ((IStateManager) properties2).TrackViewState();
                        properties2.Name = name;
                        properties2.IsReadOnly = flag2;
                        properties2.Type = propertyType;
                        properties2.DataField = name;
                        AutoGeneratedField field2 = this.CreateAutoGeneratedRow(properties2);
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
                throw new HttpException(System.Web.SR.GetString("DetailsView_NoAutoGenFields", new object[] { this.ID }));
            }
            return list;
        }

        protected override int CreateChildControls(IEnumerable dataSource, bool dataBinding)
        {
            PagedDataSource pagedDataSource = null;
            int pageIndex = this.PageIndex;
            bool allowPaging = this.AllowPaging;
            int count = 0;
            DetailsViewMode mode = this.Mode;
            if (base.DesignMode && (mode == DetailsViewMode.Insert))
            {
                pageIndex = -1;
            }
            if (dataBinding)
            {
                DataSourceView data = this.GetData();
                DataSourceSelectArguments selectArguments = base.SelectArguments;
                if (data == null)
                {
                    throw new HttpException(System.Web.SR.GetString("DataBoundControl_NullView", new object[] { this.ID }));
                }
                if (mode != DetailsViewMode.Insert)
                {
                    if ((allowPaging && !data.CanPage) && ((dataSource != null) && !(dataSource is ICollection)))
                    {
                        selectArguments.StartRowIndex = pageIndex;
                        selectArguments.MaximumRows = 1;
                        data.Select(selectArguments, new DataSourceViewSelectCallback(this.SelectCallback));
                    }
                    if (this._useServerPaging)
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
                            pagedDataSource = this.CreateServerPagedDataSource(this.PageIndex + is2.Count);
                        }
                    }
                    else
                    {
                        pagedDataSource = this.CreatePagedDataSource();
                    }
                }
            }
            else
            {
                pagedDataSource = this.CreatePagedDataSource();
            }
            if (mode != DetailsViewMode.Insert)
            {
                pagedDataSource.DataSource = dataSource;
            }
            IEnumerator enumerator = null;
            OrderedDictionary keyTable = this.KeyTable;
            this._rowsArray = new ArrayList();
            this._rowsCollection = null;
            if (!dataBinding)
            {
                enumerator = dataSource.GetEnumerator();
                ICollection is3 = dataSource as ICollection;
                if (is3 == null)
                {
                    throw new HttpException(System.Web.SR.GetString("DataControls_DataSourceMustBeCollectionWhenNotDataBinding"));
                }
                count = is3.Count;
            }
            else
            {
                keyTable.Clear();
                if (dataSource != null)
                {
                    if (mode != DetailsViewMode.Insert)
                    {
                        ICollection is4 = dataSource as ICollection;
                        if (((is4 == null) && pagedDataSource.IsPagingEnabled) && !pagedDataSource.IsServerPagingEnabled)
                        {
                            throw new HttpException(System.Web.SR.GetString("DetailsView_DataSourceMustBeCollection", new object[] { this.ID }));
                        }
                        if (pagedDataSource.IsPagingEnabled)
                        {
                            count = pagedDataSource.DataSourceCount;
                        }
                        else if (is4 != null)
                        {
                            count = is4.Count;
                        }
                    }
                    enumerator = dataSource.GetEnumerator();
                }
            }
            Table child = this.CreateTable();
            TableRowCollection rows = child.Rows;
            bool flag2 = false;
            object current = null;
            this.Controls.Add(child);
            if (enumerator != null)
            {
                flag2 = enumerator.MoveNext();
            }
            if (!flag2 && (mode != DetailsViewMode.Insert))
            {
                if ((pageIndex >= 0) || this.AutoGenerateRows)
                {
                    if ((this.EmptyDataText.Length > 0) || (this._emptyDataTemplate != null))
                    {
                        this._rowsArray.Add(this.CreateRow(0, DataControlRowType.EmptyDataRow, DataControlRowState.Normal, null, rows, null));
                    }
                    count = 0;
                }
            }
            else
            {
                int num3 = 0;
                if (!this._useServerPaging)
                {
                    while (num3 < pageIndex)
                    {
                        current = enumerator.Current;
                        flag2 = enumerator.MoveNext();
                        if (!flag2)
                        {
                            this._pageIndex = num3;
                            pagedDataSource.CurrentPageIndex = num3;
                            pageIndex = num3;
                            break;
                        }
                        num3++;
                    }
                }
                if (flag2)
                {
                    this._dataItem = enumerator.Current;
                }
                else
                {
                    this._dataItem = current;
                }
                if ((!this._useServerPaging && !(dataSource is ICollection)) || (this._useServerPaging && (count < 0)))
                {
                    count = num3;
                    while (flag2)
                    {
                        count++;
                        flag2 = enumerator.MoveNext();
                    }
                }
                this._dataItemIndex = num3;
                bool flag3 = (count <= 1) && !this._useServerPaging;
                if (((allowPaging && this.PagerSettings.Visible) && (this._pagerSettings.IsPagerOnTop && !flag3)) && (mode != DetailsViewMode.Insert))
                {
                    this._topPagerRow = this.CreateRow(-1, DataControlRowType.Pager, DataControlRowState.Normal, null, rows, pagedDataSource);
                }
                this._headerRow = this.CreateRow(-1, DataControlRowType.Header, DataControlRowState.Normal, null, rows, null);
                if ((this._headerTemplate == null) && (this.HeaderText.Length == 0))
                {
                    this._headerRow.Visible = false;
                }
                this._rowsArray.AddRange(this.CreateDataRows(dataBinding, rows, this._dataItem));
                if (pageIndex >= 0)
                {
                    string[] dataKeyNamesInternal = this.DataKeyNamesInternal;
                    if (dataBinding && (dataKeyNamesInternal.Length != 0))
                    {
                        foreach (string str in dataKeyNamesInternal)
                        {
                            object propertyValue = DataBinder.GetPropertyValue(this._dataItem, str);
                            keyTable.Add(str, propertyValue);
                        }
                        this._dataKey = new System.Web.UI.WebControls.DataKey(keyTable);
                    }
                }
                this._footerRow = this.CreateRow(-1, DataControlRowType.Footer, DataControlRowState.Normal, null, rows, null);
                if ((this._footerTemplate == null) && (this.FooterText.Length == 0))
                {
                    this._footerRow.Visible = false;
                }
                if (((allowPaging && this.PagerSettings.Visible) && (this._pagerSettings.IsPagerOnBottom && !flag3)) && (mode != DetailsViewMode.Insert))
                {
                    this._bottomPagerRow = this.CreateRow(-1, DataControlRowType.Pager, DataControlRowState.Normal, null, rows, pagedDataSource);
                }
            }
            this._pageCount = count;
            this.OnItemCreated(EventArgs.Empty);
            if (dataBinding)
            {
                this.DataBind(false);
            }
            return count;
        }

        protected override Style CreateControlStyle()
        {
            return new TableStyle { GridLines = System.Web.UI.WebControls.GridLines.Both, CellSpacing = 0 };
        }

        private ICollection CreateDataRows(bool dataBinding, TableRowCollection rows, object dataItem)
        {
            ArrayList list = new ArrayList();
            list.AddRange(this.CreateDataRowsFromFields(dataItem, dataBinding, rows));
            return list;
        }

        private ICollection CreateDataRowsFromFields(object dataItem, bool dataBinding, TableRowCollection rows)
        {
            int count = 0;
            ICollection is2 = this.CreateFieldSet(dataItem, dataBinding);
            ArrayList list = new ArrayList();
            if (is2 != null)
            {
                count = is2.Count;
            }
            if (count > 0)
            {
                DataControlRowType dataRow = DataControlRowType.DataRow;
                DataControlRowState normal = DataControlRowState.Normal;
                int rowIndex = 0;
                switch (this.Mode)
                {
                    case DetailsViewMode.Edit:
                        normal |= DataControlRowState.Edit;
                        break;

                    case DetailsViewMode.Insert:
                        normal |= DataControlRowState.Insert;
                        break;
                }
                bool flag = false;
                foreach (DataControlField field in is2)
                {
                    if (field.Initialize(false, this))
                    {
                        flag = true;
                    }
                    if (this.DetermineRenderClientScript())
                    {
                        field.ValidateSupportsCallback();
                    }
                    DataControlRowState rowState = normal;
                    if ((rowIndex % 2) != 0)
                    {
                        rowState |= DataControlRowState.Alternate;
                    }
                    list.Add(this.CreateRow(rowIndex, dataRow, rowState, field, rows, null));
                    rowIndex++;
                }
                if (flag)
                {
                    base.RequiresDataBinding = true;
                }
            }
            return list;
        }

        protected override DataSourceSelectArguments CreateDataSourceSelectArguments()
        {
            DataSourceSelectArguments arguments = new DataSourceSelectArguments();
            DataSourceView data = this.GetData();
            this._useServerPaging = this.AllowPaging && data.CanPage;
            if (this._useServerPaging)
            {
                arguments.StartRowIndex = this.PageIndex;
                if (data.CanRetrieveTotalRowCount)
                {
                    arguments.RetrieveTotalRowCount = true;
                    arguments.MaximumRows = 1;
                    return arguments;
                }
                arguments.MaximumRows = -1;
            }
            return arguments;
        }

        protected virtual ICollection CreateFieldSet(object dataItem, bool useDataSource)
        {
            ArrayList list = new ArrayList();
            if (this.AutoGenerateRows)
            {
                if (this.RowsGenerator == null)
                {
                    object[] array = null;
                    if (useDataSource)
                    {
                        ICollection is2 = this.CreateAutoGeneratedRows(dataItem);
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
                            array[i] = this.CreateAutoGeneratedRow((AutoGeneratedFieldProperties) this._autoGenFieldProps[i]);
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
                }
                else
                {
                    list.AddRange(this.RowsGenerator.GenerateFields(this));
                }
            }
            foreach (DataControlField field in this.Fields)
            {
                list.Add(field);
            }
            if ((this.AutoGenerateInsertButton || this.AutoGenerateDeleteButton) || this.AutoGenerateEditButton)
            {
                CommandField field2 = new CommandField {
                    ButtonType = ButtonType.Link
                };
                if (this.AutoGenerateInsertButton)
                {
                    field2.ShowInsertButton = true;
                }
                if (this.AutoGenerateDeleteButton)
                {
                    field2.ShowDeleteButton = true;
                }
                if (this.AutoGenerateEditButton)
                {
                    field2.ShowEditButton = true;
                }
                list.Add(field2);
            }
            return list;
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
                string firstPageImageUrl = pagerSettings.FirstPageImageUrl;
                TableCell cell = new TableCell();
                row.Cells.Add(cell);
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
                string lastPageImageUrl = pagerSettings.LastPageImageUrl;
                TableCell cell4 = new TableCell();
                row.Cells.Add(cell4);
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
                int num8 = (num2 - 1) / pageButtonCount;
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
                string firstPageImageUrl = pagerSettings.FirstPageImageUrl;
                TableCell cell = new TableCell();
                row.Cells.Add(cell);
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
                string lastPageImageUrl = pagerSettings.LastPageImageUrl;
                TableCell cell5 = new TableCell();
                row.Cells.Add(cell5);
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
            return new PagedDataSource { CurrentPageIndex = this.PageIndex, PageSize = 1, AllowPaging = this.AllowPaging, AllowCustomPaging = false, AllowServerPaging = false, VirtualCount = 0 };
        }

        protected virtual DetailsViewRow CreateRow(int rowIndex, DataControlRowType rowType, DataControlRowState rowState)
        {
            if (rowType == DataControlRowType.Pager)
            {
                return new DetailsViewPagerRow(rowIndex, rowType, rowState);
            }
            return new DetailsViewRow(rowIndex, rowType, rowState);
        }

        private DetailsViewRow CreateRow(int rowIndex, DataControlRowType rowType, DataControlRowState rowState, DataControlField field, TableRowCollection rows, PagedDataSource pagedDataSource)
        {
            DetailsViewRow row = this.CreateRow(rowIndex, rowType, rowState);
            rows.Add(row);
            if (rowType != DataControlRowType.Pager)
            {
                this.InitializeRow(row, field);
                return row;
            }
            this.InitializePager(row, pagedDataSource);
            return row;
        }

        private PagedDataSource CreateServerPagedDataSource(int totalRowCount)
        {
            return new PagedDataSource { CurrentPageIndex = this.PageIndex, PageSize = 1, AllowPaging = this.AllowPaging, AllowCustomPaging = false, AllowServerPaging = true, VirtualCount = totalRowCount };
        }

        protected virtual Table CreateTable()
        {
            return new ChildTable(string.IsNullOrEmpty(this.ID) ? null : this.ClientID);
        }

        public sealed override void DataBind()
        {
            base.DataBind();
        }

        public virtual void DeleteItem()
        {
            this.ResetModelValidationGroup(this.EnableModelValidation, string.Empty);
            this.HandleDelete(string.Empty);
        }

        private bool DetermineRenderClientScript()
        {
            if (!this._renderClientScriptValid)
            {
                this._renderClientScript = false;
                if (((this.EnablePagingCallbacks && (this.Context != null)) && ((this.Page != null) && (this.Page.RequestInternal != null))) && (this.Page.Request.Browser.SupportsCallback && !base.IsParentedToUpdatePanel))
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

        protected override void EnsureDataBound()
        {
            if ((base.RequiresDataBinding && (this.Mode == DetailsViewMode.Insert)) && (!this.AutoGenerateRows || (this.AutoGenerateRows && (this.RowsGenerator != null))))
            {
                this.OnDataBinding(EventArgs.Empty);
                base.RequiresDataBinding = false;
                base.MarkAsDataBound();
                if (base.AdapterInternal != null)
                {
                    DataBoundControlAdapter adapterInternal = base.AdapterInternal as DataBoundControlAdapter;
                    if (adapterInternal != null)
                    {
                        adapterInternal.PerformDataBinding(null);
                    }
                    else
                    {
                        this.PerformDataBinding(null);
                    }
                }
                else
                {
                    this.PerformDataBinding(null);
                }
                this.OnDataBound(EventArgs.Empty);
            }
            else
            {
                base.EnsureDataBound();
            }
        }

        protected virtual void ExtractRowValues(IOrderedDictionary fieldValues, bool includeReadOnlyFields, bool includeKeys)
        {
            if (fieldValues != null)
            {
                ICollection is2 = this.CreateFieldSet(null, false);
                object[] array = new object[is2.Count];
                is2.CopyTo(array, 0);
                DetailsViewRowCollection rows = this.Rows;
                string[] dataKeyNamesInternal = this.DataKeyNamesInternal;
                ExtractRowValues(array, rows, this.DataKeyNamesInternal, fieldValues, includeReadOnlyFields, includeKeys);
            }
        }

        internal static void ExtractRowValues(object[] fields, DetailsViewRowCollection rows, string[] dataKeyNames, IOrderedDictionary fieldValues, bool includeReadOnlyFields, bool includeKeys)
        {
            for (int i = 0; (i < fields.Length) && (i < rows.Count); i++)
            {
                if (rows[i].RowType == DataControlRowType.DataRow)
                {
                    int num = 0;
                    if (((DataControlField) fields[i]).ShowHeader)
                    {
                        num = 1;
                    }
                    if (((DataControlField) fields[i]).Visible)
                    {
                        OrderedDictionary dictionary = new OrderedDictionary();
                        ((DataControlField) fields[i]).ExtractValuesFromCell(dictionary, rows[i].Cells[num] as DataControlFieldCell, rows[i].RowState, includeReadOnlyFields);
                        foreach (DictionaryEntry entry in dictionary)
                        {
                            if (includeKeys || (Array.IndexOf<object>(dataKeyNames, entry.Key) == -1))
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
            IStateFormatter formatter = this.Page.CreateStateFormatter();
            this.RenderTableContents(writer2);
            writer2.Flush();
            writer2.Close();
            object state = OrderedDictionaryStateHelper.SaveViewState(this.KeyTable);
            string str = formatter.Serialize(state);
            return (Convert.ToString(this.PageIndex, CultureInfo.InvariantCulture) + "|" + str + "|" + writer.ToString());
        }

        protected virtual string GetCallbackScript(IButtonControl buttonControl, string argument)
        {
            if (!this.DetermineRenderClientScript() || string.IsNullOrEmpty(argument))
            {
                return null;
            }
            if (this.Page != null)
            {
                this.Page.ClientScript.RegisterForEventValidation(this.UniqueID, argument);
            }
            return (("javascript:__dv" + this.ClientID + ".callback") + "(" + argument + "); return false;");
        }

        private void HandleCancel()
        {
            bool isBoundUsingDataSourceID = base.IsBoundUsingDataSourceID;
            DetailsViewModeEventArgs e = new DetailsViewModeEventArgs(this.DefaultMode, true);
            this.OnModeChanging(e);
            if (!e.Cancel)
            {
                if (isBoundUsingDataSourceID)
                {
                    this.Mode = e.NewMode;
                    this.OnModeChanged(EventArgs.Empty);
                }
                base.RequiresDataBinding = true;
            }
        }

        private bool HandleCommand(string commandName)
        {
            DataSourceView data = null;
            if (base.IsBoundUsingDataSourceID)
            {
                data = this.GetData();
                if (data == null)
                {
                    throw new HttpException(System.Web.SR.GetString("View_DataSourceReturnedNullView", new object[] { this.ID }));
                }
            }
            else
            {
                return false;
            }
            if (!data.CanExecute(commandName))
            {
                return false;
            }
            OrderedDictionary fieldValues = new OrderedDictionary();
            OrderedDictionary keys = new OrderedDictionary();
            this.ExtractRowValues(fieldValues, true, false);
            foreach (DictionaryEntry entry in this.DataKey.Values)
            {
                keys.Add(entry.Key, entry.Value);
                if (fieldValues.Contains(entry.Key))
                {
                    fieldValues.Remove(entry.Key);
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
            base.RequiresDataBinding = true;
            return true;
        }

        private void HandleDelete(string commandArg)
        {
            int pageIndex = this.PageIndex;
            if (pageIndex >= 0)
            {
                DataSourceView data = null;
                bool isBoundUsingDataSourceID = base.IsBoundUsingDataSourceID;
                if (isBoundUsingDataSourceID)
                {
                    data = this.GetData();
                    if (data == null)
                    {
                        throw new HttpException(System.Web.SR.GetString("View_DataSourceReturnedNullView", new object[] { this.ID }));
                    }
                }
                DetailsViewDeleteEventArgs e = new DetailsViewDeleteEventArgs(pageIndex);
                this.ExtractRowValues(e.Values, true, false);
                foreach (DictionaryEntry entry in this.DataKey.Values)
                {
                    e.Keys.Add(entry.Key, entry.Value);
                    if (e.Values.Contains(entry.Key))
                    {
                        e.Values.Remove(entry.Key);
                    }
                }
                this.OnItemDeleting(e);
                if (!e.Cancel && isBoundUsingDataSourceID)
                {
                    this._deleteKeys = e.Keys;
                    this._deleteValues = e.Values;
                    data.Delete(e.Keys, e.Values, new DataSourceViewOperationCallback(this.HandleDeleteCallback));
                }
            }
        }

        private bool HandleDeleteCallback(int affectedRows, Exception ex)
        {
            int pageIndex = this.PageIndex;
            DetailsViewDeletedEventArgs e = new DetailsViewDeletedEventArgs(affectedRows, ex);
            e.SetKeys(this._deleteKeys);
            e.SetValues(this._deleteValues);
            this.OnItemDeleted(e);
            this._deleteKeys = null;
            this._deleteValues = null;
            if (((ex != null) && !e.ExceptionHandled) && this.PageIsValidAfterModelException())
            {
                return false;
            }
            if (pageIndex == (this._pageCount - 1))
            {
                this.HandlePage(pageIndex - 1);
            }
            base.RequiresDataBinding = true;
            return true;
        }

        private void HandleEdit()
        {
            if (this.PageIndex >= 0)
            {
                DetailsViewModeEventArgs e = new DetailsViewModeEventArgs(DetailsViewMode.Edit, false);
                this.OnModeChanging(e);
                if (!e.Cancel)
                {
                    if (base.IsBoundUsingDataSourceID)
                    {
                        this.Mode = e.NewMode;
                        this.OnModeChanged(EventArgs.Empty);
                    }
                    base.RequiresDataBinding = true;
                }
            }
        }

        private bool HandleEvent(EventArgs e, bool causesValidation, string validationGroup)
        {
            bool flag = false;
            this.ResetModelValidationGroup(causesValidation, validationGroup);
            DetailsViewCommandEventArgs args = e as DetailsViewCommandEventArgs;
            if (args == null)
            {
                return flag;
            }
            this.OnItemCommand(args);
            flag = true;
            string commandName = args.CommandName;
            int pageIndex = this.PageIndex;
            if (StringUtil.EqualsIgnoreCase(commandName, "Page"))
            {
                string commandArgument = (string) args.CommandArgument;
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
            if (StringUtil.EqualsIgnoreCase(commandName, "Edit"))
            {
                this.HandleEdit();
                return flag;
            }
            if (StringUtil.EqualsIgnoreCase(commandName, "Update"))
            {
                this.HandleUpdate((string) args.CommandArgument, causesValidation);
                return flag;
            }
            if (StringUtil.EqualsIgnoreCase(commandName, "Cancel"))
            {
                this.HandleCancel();
                return flag;
            }
            if (StringUtil.EqualsIgnoreCase(commandName, "Delete"))
            {
                this.HandleDelete((string) args.CommandArgument);
                return flag;
            }
            if (StringUtil.EqualsIgnoreCase(commandName, "Insert"))
            {
                this.HandleInsert((string) args.CommandArgument, causesValidation);
                return flag;
            }
            if (StringUtil.EqualsIgnoreCase(commandName, "New"))
            {
                this.HandleNew();
                return flag;
            }
            return this.HandleCommand(commandName);
        }

        private void HandleInsert(string commandArg, bool causesValidation)
        {
            if ((!causesValidation || (this.Page == null)) || this.Page.IsValid)
            {
                if (this.Mode != DetailsViewMode.Insert)
                {
                    throw new HttpException(System.Web.SR.GetString("DetailsViewFormView_ControlMustBeInInsertMode", new object[] { "DetailsView", this.ID }));
                }
                DataSourceView data = null;
                bool isBoundUsingDataSourceID = base.IsBoundUsingDataSourceID;
                if (isBoundUsingDataSourceID)
                {
                    data = this.GetData();
                    if (data == null)
                    {
                        throw new HttpException(System.Web.SR.GetString("View_DataSourceReturnedNullView", new object[] { this.ID }));
                    }
                }
                DetailsViewInsertEventArgs e = new DetailsViewInsertEventArgs(commandArg);
                this.ExtractRowValues(e.Values, false, true);
                this.OnItemInserting(e);
                if (!e.Cancel && isBoundUsingDataSourceID)
                {
                    this._insertValues = e.Values;
                    data.Insert(e.Values, new DataSourceViewOperationCallback(this.HandleInsertCallback));
                }
            }
        }

        private bool HandleInsertCallback(int affectedRows, Exception ex)
        {
            DetailsViewInsertedEventArgs e = new DetailsViewInsertedEventArgs(affectedRows, ex);
            e.SetValues(this._insertValues);
            this.OnItemInserted(e);
            this._insertValues = null;
            if ((ex != null) && !e.ExceptionHandled)
            {
                if (this.PageIsValidAfterModelException())
                {
                    return false;
                }
                e.KeepInInsertMode = true;
            }
            if (!e.KeepInInsertMode)
            {
                DetailsViewModeEventArgs args2 = new DetailsViewModeEventArgs(this.DefaultMode, false);
                this.OnModeChanging(args2);
                if (!args2.Cancel)
                {
                    this.Mode = args2.NewMode;
                    this.OnModeChanged(EventArgs.Empty);
                    base.RequiresDataBinding = true;
                }
            }
            return true;
        }

        private void HandleNew()
        {
            DetailsViewModeEventArgs e = new DetailsViewModeEventArgs(DetailsViewMode.Insert, false);
            this.OnModeChanging(e);
            if (!e.Cancel)
            {
                if (base.IsBoundUsingDataSourceID)
                {
                    this.Mode = e.NewMode;
                    this.OnModeChanged(EventArgs.Empty);
                }
                base.RequiresDataBinding = true;
            }
        }

        private void HandlePage(int newPage)
        {
            if (this.AllowPaging && (this.PageIndex >= 0))
            {
                DetailsViewPageEventArgs e = new DetailsViewPageEventArgs(newPage);
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
                        this._keyTable = null;
                        this._pageIndex = e.NewPageIndex;
                    }
                    this.OnPageIndexChanged(EventArgs.Empty);
                    base.RequiresDataBinding = true;
                }
            }
        }

        private void HandleUpdate(string commandArg, bool causesValidation)
        {
            if ((!causesValidation || (this.Page == null)) || this.Page.IsValid)
            {
                if (this.Mode != DetailsViewMode.Edit)
                {
                    throw new HttpException(System.Web.SR.GetString("DetailsViewFormView_ControlMustBeInEditMode", new object[] { "DetailsView", this.ID }));
                }
                if (this.PageIndex >= 0)
                {
                    DataSourceView data = null;
                    bool isBoundUsingDataSourceID = base.IsBoundUsingDataSourceID;
                    if (isBoundUsingDataSourceID)
                    {
                        data = this.GetData();
                        if (data == null)
                        {
                            throw new HttpException(System.Web.SR.GetString("View_DataSourceReturnedNullView", new object[] { this.ID }));
                        }
                    }
                    DetailsViewUpdateEventArgs e = new DetailsViewUpdateEventArgs(commandArg);
                    foreach (DictionaryEntry entry in this.BoundFieldValues)
                    {
                        e.OldValues.Add(entry.Key, entry.Value);
                    }
                    this.ExtractRowValues(e.NewValues, false, true);
                    foreach (DictionaryEntry entry2 in this.DataKey.Values)
                    {
                        e.Keys.Add(entry2.Key, entry2.Value);
                    }
                    this.OnItemUpdating(e);
                    if (!e.Cancel && isBoundUsingDataSourceID)
                    {
                        this._updateKeys = e.Keys;
                        this._updateNewValues = e.NewValues;
                        this._updateOldValues = e.OldValues;
                        data.Update(e.Keys, e.NewValues, e.OldValues, new DataSourceViewOperationCallback(this.HandleUpdateCallback));
                    }
                }
            }
        }

        private bool HandleUpdateCallback(int affectedRows, Exception ex)
        {
            DetailsViewUpdatedEventArgs e = new DetailsViewUpdatedEventArgs(affectedRows, ex);
            e.SetOldValues(this._updateOldValues);
            e.SetNewValues(this._updateNewValues);
            e.SetKeys(this._updateKeys);
            this.OnItemUpdated(e);
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
                DetailsViewModeEventArgs args2 = new DetailsViewModeEventArgs(this.DefaultMode, false);
                this.OnModeChanging(args2);
                if (!args2.Cancel)
                {
                    this.Mode = args2.NewMode;
                    this.OnModeChanged(EventArgs.Empty);
                    base.RequiresDataBinding = true;
                }
            }
            return true;
        }

        protected virtual void InitializePager(DetailsViewRow row, PagedDataSource pagedDataSource)
        {
            TableCell container = new TableCell();
            System.Web.UI.WebControls.PagerSettings pagerSettings = this.PagerSettings;
            if (this._pagerTemplate != null)
            {
                this._pagerTemplate.InstantiateIn(container);
            }
            else
            {
                PagerTable child = new PagerTable();
                TableRow row2 = new TableRow();
                container.Controls.Add(child);
                child.Rows.Add(row2);
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
            }
            container.ColumnSpan = 2;
            row.Cells.Add(container);
        }

        protected virtual void InitializeRow(DetailsViewRow row, DataControlField field)
        {
            TableCellCollection cells = row.Cells;
            DataControlFieldCell cell = new DataControlFieldCell(field);
            ITemplate template = null;
            int dataItemIndex = this.DataItemIndex;
            DataControlRowState rowState = row.RowState;
            switch (row.RowType)
            {
                case DataControlRowType.Header:
                {
                    template = this._headerTemplate;
                    cell.ColumnSpan = 2;
                    string headerText = this.HeaderText;
                    if ((this._headerTemplate == null) && (headerText.Length > 0))
                    {
                        cell.Text = headerText;
                    }
                    goto Label_0116;
                }
                case DataControlRowType.Footer:
                {
                    template = this._footerTemplate;
                    cell.ColumnSpan = 2;
                    string footerText = this.FooterText;
                    if ((this._footerTemplate == null) && (footerText.Length > 0))
                    {
                        cell.Text = footerText;
                    }
                    goto Label_0116;
                }
                case DataControlRowType.DataRow:
                {
                    if (!field.ShowHeader)
                    {
                        cell.ColumnSpan = 2;
                        break;
                    }
                    DataControlFieldCell cell2 = new DataControlFieldCell(field);
                    field.InitializeCell(cell2, DataControlCellType.Header, rowState, dataItemIndex);
                    cells.Add(cell2);
                    break;
                }
                case DataControlRowType.EmptyDataRow:
                {
                    template = this._emptyDataTemplate;
                    string emptyDataText = this.EmptyDataText;
                    if ((this._emptyDataTemplate == null) && (emptyDataText.Length > 0))
                    {
                        cell.Text = emptyDataText;
                    }
                    goto Label_0116;
                }
                default:
                    goto Label_0116;
            }
            field.InitializeCell(cell, DataControlCellType.DataCell, rowState, dataItemIndex);
        Label_0116:
            if (template != null)
            {
                template.InstantiateIn(cell);
            }
            cells.Add(cell);
        }

        public virtual void InsertItem(bool causesValidation)
        {
            this.ResetModelValidationGroup(causesValidation, string.Empty);
            this.HandleInsert(string.Empty, causesValidation);
        }

        public virtual bool IsBindableType(Type type)
        {
            return DataBoundControlHelper.IsBindableType(type);
        }

        protected internal override void LoadControlState(object savedState)
        {
            this._pageIndex = 0;
            this._defaultMode = DetailsViewMode.ReadOnly;
            this._dataKeyNames = new string[0];
            this._pageCount = 0;
            object[] objArray = savedState as object[];
            if (objArray != null)
            {
                base.LoadControlState(objArray[0]);
                if (objArray[1] != null)
                {
                    this._pageIndex = (int) objArray[1];
                }
                if (objArray[2] != null)
                {
                    this._defaultMode = (DetailsViewMode) objArray[2];
                }
                if (objArray[3] != null)
                {
                    this.Mode = (DetailsViewMode) objArray[3];
                }
                if (objArray[4] != null)
                {
                    this._dataKeyNames = (string[]) objArray[4];
                }
                if (objArray[5] != null)
                {
                    this.KeyTable.Clear();
                    OrderedDictionaryStateHelper.LoadViewState(this.KeyTable, (ArrayList) objArray[5]);
                }
                if (objArray[6] != null)
                {
                    this._pageCount = (int) objArray[6];
                }
            }
            else
            {
                base.LoadControlState(null);
            }
        }

        private bool LoadHiddenFieldState(string pageIndex, string dataKey)
        {
            bool flag = false;
            int num = int.Parse(pageIndex, CultureInfo.InvariantCulture);
            if (this.PageIndex != num)
            {
                flag = true;
                this._pageIndex = num;
                string str = dataKey;
                if (string.IsNullOrEmpty(str))
                {
                    return flag;
                }
                ArrayList state = this.Page.CreateStateFormatter().Deserialize(str) as ArrayList;
                if (this._keyTable != null)
                {
                    this._keyTable.Clear();
                }
                OrderedDictionaryStateHelper.LoadViewState(this.KeyTable, state);
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
                    ((IStateManager) this.PagerStyle).LoadViewState(objArray[1]);
                }
                if (objArray[2] != null)
                {
                    ((IStateManager) this.HeaderStyle).LoadViewState(objArray[2]);
                }
                if (objArray[3] != null)
                {
                    ((IStateManager) this.FooterStyle).LoadViewState(objArray[3]);
                }
                if (objArray[4] != null)
                {
                    ((IStateManager) this.RowStyle).LoadViewState(objArray[4]);
                }
                if (objArray[5] != null)
                {
                    ((IStateManager) this.AlternatingRowStyle).LoadViewState(objArray[5]);
                }
                if (objArray[6] != null)
                {
                    ((IStateManager) this.CommandRowStyle).LoadViewState(objArray[6]);
                }
                if (objArray[7] != null)
                {
                    ((IStateManager) this.EditRowStyle).LoadViewState(objArray[7]);
                }
                if (objArray[8] != null)
                {
                    ((IStateManager) this.InsertRowStyle).LoadViewState(objArray[8]);
                }
                if (objArray[9] != null)
                {
                    ((IStateManager) this.FieldHeaderStyle).LoadViewState(objArray[9]);
                }
                if (objArray[10] != null)
                {
                    ((IStateManager) this.Fields).LoadViewState(objArray[10]);
                }
                if (objArray[11] != null)
                {
                    OrderedDictionaryStateHelper.LoadViewState((OrderedDictionary) this.BoundFieldValues, (ArrayList) objArray[11]);
                }
                if (objArray[12] != null)
                {
                    ((IStateManager) this.PagerSettings).LoadViewState(objArray[12]);
                }
                if (objArray[13] != null)
                {
                    ((IStateManager) base.ControlStyle).LoadViewState(objArray[13]);
                }
                if (objArray[14] != null)
                {
                    object[] objArray2 = (object[]) objArray[14];
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
            DetailsViewCommandEventArgs args = e as DetailsViewCommandEventArgs;
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

        protected override void OnDataSourceViewChanged(object sender, EventArgs e)
        {
            this._keyTable = null;
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
                if ((this.DataKeyNames.Length > 0) && !this.AutoGenerateRows)
                {
                    this.Page.RegisterRequiresViewStateEncryption();
                }
                this.Page.RegisterRequiresControlState(this);
            }
        }

        protected virtual void OnItemCommand(DetailsViewCommandEventArgs e)
        {
            DetailsViewCommandEventHandler handler = (DetailsViewCommandEventHandler) base.Events[EventItemCommand];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnItemCreated(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EventItemCreated];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnItemDeleted(DetailsViewDeletedEventArgs e)
        {
            DetailsViewDeletedEventHandler handler = (DetailsViewDeletedEventHandler) base.Events[EventItemDeleted];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnItemDeleting(DetailsViewDeleteEventArgs e)
        {
            bool isBoundUsingDataSourceID = base.IsBoundUsingDataSourceID;
            DetailsViewDeleteEventHandler handler = (DetailsViewDeleteEventHandler) base.Events[EventItemDeleting];
            if (handler != null)
            {
                handler(this, e);
            }
            else if (!isBoundUsingDataSourceID && !e.Cancel)
            {
                throw new HttpException(System.Web.SR.GetString("DetailsView_UnhandledEvent", new object[] { this.ID, "ItemDeleting" }));
            }
        }

        protected virtual void OnItemInserted(DetailsViewInsertedEventArgs e)
        {
            DetailsViewInsertedEventHandler handler = (DetailsViewInsertedEventHandler) base.Events[EventItemInserted];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnItemInserting(DetailsViewInsertEventArgs e)
        {
            bool isBoundUsingDataSourceID = base.IsBoundUsingDataSourceID;
            DetailsViewInsertEventHandler handler = (DetailsViewInsertEventHandler) base.Events[EventItemInserting];
            if (handler != null)
            {
                handler(this, e);
            }
            else if (!isBoundUsingDataSourceID && !e.Cancel)
            {
                throw new HttpException(System.Web.SR.GetString("DetailsView_UnhandledEvent", new object[] { this.ID, "ItemInserting" }));
            }
        }

        protected virtual void OnItemUpdated(DetailsViewUpdatedEventArgs e)
        {
            DetailsViewUpdatedEventHandler handler = (DetailsViewUpdatedEventHandler) base.Events[EventItemUpdated];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnItemUpdating(DetailsViewUpdateEventArgs e)
        {
            bool isBoundUsingDataSourceID = base.IsBoundUsingDataSourceID;
            DetailsViewUpdateEventHandler handler = (DetailsViewUpdateEventHandler) base.Events[EventItemUpdating];
            if (handler != null)
            {
                handler(this, e);
            }
            else if (!isBoundUsingDataSourceID && !e.Cancel)
            {
                throw new HttpException(System.Web.SR.GetString("DetailsView_UnhandledEvent", new object[] { this.ID, "ItemUpdating" }));
            }
        }

        protected virtual void OnModeChanged(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EventModeChanged];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnModeChanging(DetailsViewModeEventArgs e)
        {
            bool isBoundUsingDataSourceID = base.IsBoundUsingDataSourceID;
            DetailsViewModeEventHandler handler = (DetailsViewModeEventHandler) base.Events[EventModeChanging];
            if (handler != null)
            {
                handler(this, e);
            }
            else if (!isBoundUsingDataSourceID && !e.Cancel)
            {
                throw new HttpException(System.Web.SR.GetString("DetailsView_UnhandledEvent", new object[] { this.ID, "ModeChanging" }));
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

        protected virtual void OnPageIndexChanging(DetailsViewPageEventArgs e)
        {
            bool isBoundUsingDataSourceID = base.IsBoundUsingDataSourceID;
            DetailsViewPageEventHandler handler = (DetailsViewPageEventHandler) base.Events[EventPageIndexChanging];
            if (handler != null)
            {
                handler(this, e);
            }
            else if (!isBoundUsingDataSourceID && !e.Cancel)
            {
                throw new HttpException(System.Web.SR.GetString("DetailsView_UnhandledEvent", new object[] { this.ID, "PageIndexChanging" }));
            }
        }

        protected override void OnPagePreLoad(object sender, EventArgs e)
        {
            if (((this.Page != null) && !this.Page.IsCallback) && (this.Page.RequestValueCollection != null))
            {
                string str = "__dv" + this.ClientID + "__hidden";
                string str2 = this.Page.RequestValueCollection[str];
                if (!string.IsNullOrEmpty(str2) && this.ParseHiddenFieldState(str2))
                {
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
                string context = "__dv" + this.ClientID;
                ClientScriptManager clientScript = this.Page.ClientScript;
                clientScript.RegisterClientScriptResource(typeof(DetailsView), "DetailsView.js");
                string str2 = clientScript.GetCallbackEventReference(this, context + ".getHiddenFieldContents(arg)", "DetailsView_OnCallback", context);
                string hiddenFieldName = context + "__hidden";
                clientScript.RegisterHiddenField(hiddenFieldName, string.Empty);
                string script = string.Format(CultureInfo.InvariantCulture, "\r\nvar {0} = new DetailsView();\r\n{0}.stateField = document.getElementById('{1}');\r\n{0}.panelElement = document.getElementById('{0}__div');\r\n{0}.pageIndex = {3};\r\n{0}.setStateField();\r\n{0}.callback = function(arg) {{\r\n    {2};\r\n}};", new object[] { context, hiddenFieldName, str2, this.PageIndex });
                clientScript.RegisterStartupScript(typeof(DetailsView), context, script, true);
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
            return ((strArray.Length == 2) && this.LoadHiddenFieldState(strArray[0], strArray[1]));
        }

        protected internal override void PerformDataBinding(IEnumerable data)
        {
            base.PerformDataBinding(data);
            if ((base.IsBoundUsingDataSourceID && (this.Mode == DetailsViewMode.Edit)) && base.IsViewStateEnabled)
            {
                this.BoundFieldValues.Clear();
                this.ExtractRowValues(this.BoundFieldValues, true, false);
            }
        }

        protected internal virtual void PrepareControlHierarchy()
        {
            if (this.Controls.Count >= 1)
            {
                Table table = (Table) this.Controls[0];
                table.CopyBaseAttributes(this);
                if (base.ControlStyleCreated && !base.ControlStyle.IsEmpty)
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
                Style s = new TableItemStyle();
                s.CopyFrom(this._rowStyle);
                if (this._alternatingRowStyle != null)
                {
                    s = new TableItemStyle();
                    s.CopyFrom(this._alternatingRowStyle);
                }
                foreach (DetailsViewRow row in table.Rows)
                {
                    Style style2 = new TableItemStyle();
                    DataControlRowState rowState = row.RowState;
                    DataControlRowType rowType = row.RowType;
                    DataControlFieldCell cell = row.Cells[0] as DataControlFieldCell;
                    DataControlField containingField = null;
                    if (cell != null)
                    {
                        containingField = cell.ContainingField;
                    }
                    switch (rowType)
                    {
                        case DataControlRowType.Header:
                            style2 = this._headerStyle;
                            break;

                        case DataControlRowType.Footer:
                            style2 = this._footerStyle;
                            break;

                        case DataControlRowType.DataRow:
                            style2.CopyFrom(this._rowStyle);
                            if ((rowState & DataControlRowState.Alternate) != DataControlRowState.Normal)
                            {
                                style2.CopyFrom(s);
                            }
                            if (containingField is ButtonFieldBase)
                            {
                                style2.CopyFrom(this._commandRowStyle);
                            }
                            else
                            {
                                if ((rowState & DataControlRowState.Edit) != DataControlRowState.Normal)
                                {
                                    style2.CopyFrom(this._editRowStyle);
                                }
                                if ((rowState & DataControlRowState.Insert) != DataControlRowState.Normal)
                                {
                                    if (this._insertRowStyle != null)
                                    {
                                        style2.CopyFrom(this._insertRowStyle);
                                    }
                                    else
                                    {
                                        style2.CopyFrom(this._editRowStyle);
                                    }
                                }
                            }
                            break;

                        case DataControlRowType.Pager:
                            style2 = this._pagerStyle;
                            break;

                        case DataControlRowType.EmptyDataRow:
                            style2 = this._emptyDataRowStyle;
                            break;
                    }
                    if ((style2 != null) && row.Visible)
                    {
                        row.MergeStyle(style2);
                    }
                    if ((rowType == DataControlRowType.DataRow) && (containingField != null))
                    {
                        if (!containingField.Visible || ((this.Mode == DetailsViewMode.Insert) && !containingField.InsertVisible))
                        {
                            row.Visible = false;
                        }
                        else
                        {
                            int num = 0;
                            DataControlFieldCell cell2 = null;
                            if ((cell != null) && cell.ContainingField.ShowHeader)
                            {
                                cell.MergeStyle(containingField.HeaderStyleInternal);
                                cell.MergeStyle(this._fieldHeaderStyle);
                                num = 1;
                            }
                            cell2 = row.Cells[num] as DataControlFieldCell;
                            if (cell2 != null)
                            {
                                cell2.MergeStyle(containingField.ItemStyleInternal);
                            }
                            foreach (Control control in cell2.Controls)
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
        }

        protected virtual void RaiseCallbackEvent(string eventArgument)
        {
            string[] strArray = eventArgument.Split(new char[] { '|' });
            base.ValidateEvent(this.UniqueID, "\"" + strArray[0] + "|" + strArray[1] + "\"");
            this.LoadHiddenFieldState(strArray[2], strArray[3]);
            int num = int.Parse(strArray[0], CultureInfo.InvariantCulture);
            this._pageIndex = num;
            this.DataBind();
        }

        protected virtual void RaisePostBackEvent(string eventArgument)
        {
            base.ValidateEvent(this.UniqueID, eventArgument);
            int index = eventArgument.IndexOf('$');
            if (index >= 0)
            {
                CommandEventArgs originalArgs = new CommandEventArgs(eventArgument.Substring(0, index), eventArgument.Substring(index + 1));
                DetailsViewCommandEventArgs e = new DetailsViewCommandEventArgs(this, originalArgs);
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
                if (this.DetermineRenderClientScript())
                {
                    string clientID = this.ClientID;
                    if (clientID == null)
                    {
                        throw new HttpException(System.Web.SR.GetString("DetailsView_MustBeParented"));
                    }
                    StringBuilder builder = new StringBuilder("__dv", 9 + clientID.Length);
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
            if (causesValidation && (this.Page != null))
            {
                this.Page.Validate(validationGroup);
                if (this.EnableModelValidation)
                {
                    this._modelValidationGroup = validationGroup;
                }
            }
        }

        protected internal override object SaveControlState()
        {
            object obj2 = base.SaveControlState();
            if ((((((obj2 == null) && (this._pageIndex == 0)) && ((this._mode == this._defaultMode) && (this._defaultMode == DetailsViewMode.ReadOnly))) && ((this._dataKeyNames == null) || (this._dataKeyNames.Length <= 0))) && ((this._keyTable == null) || (this._keyTable.Count <= 0))) && (this._pageCount == 0))
            {
                return true;
            }
            object[] objArray = new object[7];
            object obj3 = null;
            object obj4 = null;
            object obj5 = null;
            object obj6 = null;
            object obj7 = null;
            object obj8 = null;
            if (this._pageIndex != 0)
            {
                obj3 = this._pageIndex;
            }
            if (this._defaultMode != DetailsViewMode.ReadOnly)
            {
                obj5 = (int) this._defaultMode;
            }
            if ((this._mode != this._defaultMode) && this._modeSet)
            {
                obj4 = (int) this._mode;
            }
            if ((this._dataKeyNames != null) && (this._dataKeyNames.Length > 0))
            {
                obj6 = this._dataKeyNames;
            }
            if (this._keyTable != null)
            {
                obj7 = OrderedDictionaryStateHelper.SaveViewState(this._keyTable);
            }
            if (this._pageCount != 0)
            {
                obj8 = this._pageCount;
            }
            objArray[0] = obj2;
            objArray[1] = obj3;
            objArray[2] = obj5;
            objArray[3] = obj4;
            objArray[4] = obj6;
            objArray[5] = obj7;
            objArray[6] = obj8;
            return objArray;
        }

        protected override object SaveViewState()
        {
            object obj2 = base.SaveViewState();
            object obj3 = (this._pagerStyle != null) ? ((IStateManager) this._pagerStyle).SaveViewState() : null;
            object obj4 = (this._headerStyle != null) ? ((IStateManager) this._headerStyle).SaveViewState() : null;
            object obj5 = (this._footerStyle != null) ? ((IStateManager) this._footerStyle).SaveViewState() : null;
            object obj6 = (this._rowStyle != null) ? ((IStateManager) this._rowStyle).SaveViewState() : null;
            object obj7 = (this._alternatingRowStyle != null) ? ((IStateManager) this._alternatingRowStyle).SaveViewState() : null;
            object obj8 = (this._commandRowStyle != null) ? ((IStateManager) this._commandRowStyle).SaveViewState() : null;
            object obj9 = (this._editRowStyle != null) ? ((IStateManager) this._editRowStyle).SaveViewState() : null;
            object obj10 = (this._insertRowStyle != null) ? ((IStateManager) this._insertRowStyle).SaveViewState() : null;
            object obj11 = (this._fieldHeaderStyle != null) ? ((IStateManager) this._fieldHeaderStyle).SaveViewState() : null;
            object obj12 = (this._fieldCollection != null) ? ((IStateManager) this._fieldCollection).SaveViewState() : null;
            object obj13 = (this._boundFieldValues != null) ? OrderedDictionaryStateHelper.SaveViewState(this._boundFieldValues) : null;
            object obj14 = (this._pagerSettings != null) ? ((IStateManager) this._pagerSettings).SaveViewState() : null;
            object obj15 = base.ControlStyleCreated ? ((IStateManager) base.ControlStyle).SaveViewState() : null;
            object obj16 = null;
            if (this._autoGenFieldProps != null)
            {
                int count = this._autoGenFieldProps.Count;
                object[] objArray = new object[count];
                for (int i = 0; i < count; i++)
                {
                    objArray[i] = ((IStateManager) this._autoGenFieldProps[i]).SaveViewState();
                }
                obj16 = objArray;
            }
            return new object[] { obj2, obj3, obj4, obj5, obj6, obj7, obj8, obj9, obj10, obj11, obj12, obj13, obj14, obj15, obj16 };
        }

        private void SelectCallback(IEnumerable data)
        {
            throw new HttpException(System.Web.SR.GetString("DataBoundControl_DataSourceDoesntSupportPaging"));
        }

        public void SetPageIndex(int index)
        {
            this.HandlePage(index);
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
            if (this._alternatingRowStyle != null)
            {
                ((IStateManager) this._alternatingRowStyle).TrackViewState();
            }
            if (this._commandRowStyle != null)
            {
                ((IStateManager) this._commandRowStyle).TrackViewState();
            }
            if (this._editRowStyle != null)
            {
                ((IStateManager) this._editRowStyle).TrackViewState();
            }
            if (this._insertRowStyle != null)
            {
                ((IStateManager) this._insertRowStyle).TrackViewState();
            }
            if (this._pagerSettings != null)
            {
                ((IStateManager) this._pagerSettings).TrackViewState();
            }
            if (base.ControlStyleCreated)
            {
                ((IStateManager) base.ControlStyle).TrackViewState();
            }
        }

        public virtual void UpdateItem(bool causesValidation)
        {
            this.ResetModelValidationGroup(causesValidation, string.Empty);
            this.HandleUpdate(string.Empty, causesValidation);
        }

        [WebCategory("Paging"), DefaultValue(false), WebSysDescription("DetailsView_AllowPaging")]
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

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content), WebCategory("Styles"), DefaultValue((string) null), WebSysDescription("DetailsView_AlternatingRowStyle"), NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty)]
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

        [WebCategory("Behavior"), WebSysDescription("DetailsView_AutoGenerateDeleteButton"), DefaultValue(false)]
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

        [WebSysDescription("DetailsView_AutoGenerateEditButton"), WebCategory("Behavior"), DefaultValue(false)]
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

        [DefaultValue(false), WebSysDescription("DetailsView_AutoGenerateInsertButton"), WebCategory("Behavior")]
        public virtual bool AutoGenerateInsertButton
        {
            get
            {
                object obj2 = this.ViewState["AutoGenerateInsertButton"];
                return ((obj2 != null) && ((bool) obj2));
            }
            set
            {
                bool autoGenerateInsertButton = this.AutoGenerateInsertButton;
                if (value != autoGenerateInsertButton)
                {
                    this.ViewState["AutoGenerateInsertButton"] = value;
                    if (base.Initialized)
                    {
                        base.RequiresDataBinding = true;
                    }
                }
            }
        }

        [WebSysDescription("DetailsView_AutoGenerateRows"), WebCategory("Behavior"), DefaultValue(true)]
        public virtual bool AutoGenerateRows
        {
            get
            {
                object obj2 = this.ViewState["AutoGenerateRows"];
                if (obj2 != null)
                {
                    return (bool) obj2;
                }
                return true;
            }
            set
            {
                bool autoGenerateRows = this.AutoGenerateRows;
                if (value != autoGenerateRows)
                {
                    this.ViewState["AutoGenerateRows"] = value;
                    if (base.Initialized)
                    {
                        base.RequiresDataBinding = true;
                    }
                }
            }
        }

        [DefaultValue(""), WebCategory("Appearance"), WebSysDescription("WebControl_BackImageUrl"), Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), UrlProperty]
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

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual DetailsViewRow BottomPagerRow
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
                    int count = this.Fields.Count;
                    if (this.AutoGenerateRows)
                    {
                        count += 10;
                    }
                    this._boundFieldValues = new OrderedDictionary(count);
                }
                return this._boundFieldValues;
            }
        }

        [WebSysDescription("DataControls_Caption"), Localizable(true), DefaultValue(""), WebCategory("Accessibility")]
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

        [DefaultValue(0), WebSysDescription("WebControl_CaptionAlign"), WebCategory("Accessibility")]
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

        [WebSysDescription("DetailsView_CellPadding"), WebCategory("Layout"), DefaultValue(-1)]
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

        [WebSysDescription("DetailsView_CellSpacing"), DefaultValue(0), WebCategory("Layout")]
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

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content), PersistenceMode(PersistenceMode.InnerProperty), WebCategory("Styles"), DefaultValue((string) null), NotifyParentProperty(true), WebSysDescription("DetailsView_CommandRowStyle")]
        public TableItemStyle CommandRowStyle
        {
            get
            {
                if (this._commandRowStyle == null)
                {
                    this._commandRowStyle = new TableItemStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._commandRowStyle).TrackViewState();
                    }
                }
                return this._commandRowStyle;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DetailsViewMode CurrentMode
        {
            get
            {
                return this.Mode;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public virtual object DataItem
        {
            get
            {
                if (this.CurrentMode == DetailsViewMode.Insert)
                {
                    return null;
                }
                return this._dataItem;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int DataItemCount
        {
            get
            {
                return this.PageCount;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public virtual int DataItemIndex
        {
            get
            {
                if (this.CurrentMode == DetailsViewMode.Insert)
                {
                    return -1;
                }
                return this._dataItemIndex;
            }
        }

        [WebSysDescription("DetailsView_DataKey"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual System.Web.UI.WebControls.DataKey DataKey
        {
            get
            {
                if (this._dataKey == null)
                {
                    this._dataKey = new System.Web.UI.WebControls.DataKey(this.KeyTable);
                }
                return this._dataKey;
            }
        }

        [TypeConverter(typeof(StringArrayConverter)), DefaultValue((string) null), Editor("System.Web.UI.Design.WebControls.DataFieldEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), WebSysDescription("DataControls_DataKeyNames"), WebCategory("Data")]
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
                    this._keyTable = null;
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

        [WebCategory("Behavior"), DefaultValue(0), WebSysDescription("View_DefaultMode")]
        public virtual DetailsViewMode DefaultMode
        {
            get
            {
                return this._defaultMode;
            }
            set
            {
                if ((value < DetailsViewMode.ReadOnly) || (value > DetailsViewMode.Insert))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this._defaultMode = value;
            }
        }

        [NotifyParentProperty(true), WebCategory("Styles"), DefaultValue((string) null), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), PersistenceMode(PersistenceMode.InnerProperty), WebSysDescription("View_EditRowStyle")]
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

        [WebCategory("Styles"), WebSysDescription("View_EmptyDataRowStyle"), DefaultValue((string) null), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty)]
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

        [WebSysDescription("View_EmptyDataTemplate"), Browsable(false), DefaultValue((string) null), PersistenceMode(PersistenceMode.InnerProperty), TemplateContainer(typeof(DetailsView))]
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

        [DefaultValue(""), WebCategory("Appearance"), Localizable(true), WebSysDescription("View_EmptyDataText")]
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

        [DefaultValue(true), WebCategory("Behavior"), WebSysDescription("DataBoundControl_EnableModelValidation")]
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

        [WebCategory("Behavior"), DefaultValue(false), WebSysDescription("DetailsView_EnablePagingCallbacks")]
        public virtual bool EnablePagingCallbacks
        {
            get
            {
                object obj2 = this.ViewState["EnablePagingCallbacks"];
                return ((obj2 != null) && ((bool) obj2));
            }
            set
            {
                this.ViewState["EnablePagingCallbacks"] = value;
            }
        }

        [DefaultValue((string) null), WebCategory("Styles"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty), WebSysDescription("DetailsView_FieldHeaderStyle")]
        public TableItemStyle FieldHeaderStyle
        {
            get
            {
                if (this._fieldHeaderStyle == null)
                {
                    this._fieldHeaderStyle = new TableItemStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._fieldHeaderStyle).TrackViewState();
                    }
                }
                return this._fieldHeaderStyle;
            }
        }

        [PersistenceMode(PersistenceMode.InnerProperty), DefaultValue((string) null), Editor("System.Web.UI.Design.WebControls.DataControlFieldTypeEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), MergableProperty(false), WebCategory("Default"), WebSysDescription("DetailsView_Fields")]
        public virtual DataControlFieldCollection Fields
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

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual DetailsViewRow FooterRow
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

        [WebSysDescription("DetailsView_FooterStyle"), WebCategory("Styles"), DefaultValue((string) null), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty)]
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

        [WebSysDescription("DetailsView_FooterTemplate"), Browsable(false), DefaultValue((string) null), PersistenceMode(PersistenceMode.InnerProperty), TemplateContainer(typeof(DetailsView))]
        public virtual ITemplate FooterTemplate
        {
            get
            {
                return this._footerTemplate;
            }
            set
            {
                this._footerTemplate = value;
            }
        }

        [DefaultValue(""), WebCategory("Appearance"), Localizable(true), WebSysDescription("View_FooterText")]
        public virtual string FooterText
        {
            get
            {
                object obj2 = this.ViewState["FooterText"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["FooterText"] = value;
            }
        }

        [WebCategory("Appearance"), DefaultValue(3), WebSysDescription("DataControls_GridLines")]
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

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual DetailsViewRow HeaderRow
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

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content), DefaultValue((string) null), WebCategory("Styles"), NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty), WebSysDescription("WebControl_HeaderStyle")]
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

        [TemplateContainer(typeof(DetailsView)), DefaultValue((string) null), PersistenceMode(PersistenceMode.InnerProperty), Browsable(false), WebSysDescription("WebControl_HeaderTemplate")]
        public virtual ITemplate HeaderTemplate
        {
            get
            {
                return this._headerTemplate;
            }
            set
            {
                this._headerTemplate = value;
            }
        }

        [WebCategory("Appearance"), Localizable(true), DefaultValue(""), WebSysDescription("View_HeaderText")]
        public virtual string HeaderText
        {
            get
            {
                object obj2 = this.ViewState["HeaderText"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["HeaderText"] = value;
            }
        }

        [Category("Layout"), DefaultValue(0), WebSysDescription("WebControl_HorizontalAlign")]
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

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content), WebCategory("Styles"), DefaultValue((string) null), NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty), WebSysDescription("View_InsertRowStyle")]
        public TableItemStyle InsertRowStyle
        {
            get
            {
                if (this._insertRowStyle == null)
                {
                    this._insertRowStyle = new TableItemStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._insertRowStyle).TrackViewState();
                    }
                }
                return this._insertRowStyle;
            }
        }

        private OrderedDictionary KeyTable
        {
            get
            {
                if (this._keyTable == null)
                {
                    this._keyTable = new OrderedDictionary(this.DataKeyNamesInternal.Length);
                }
                return this._keyTable;
            }
        }

        private DetailsViewMode Mode
        {
            get
            {
                if (!this._modeSet || base.DesignMode)
                {
                    this._mode = this.DefaultMode;
                    this._modeSet = true;
                }
                return this._mode;
            }
            set
            {
                if ((value < DetailsViewMode.ReadOnly) || (value > DetailsViewMode.Insert))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this._modeSet = true;
                if (this._mode != value)
                {
                    this._mode = value;
                    if (base.Initialized)
                    {
                        base.RequiresDataBinding = true;
                    }
                }
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual int PageCount
        {
            get
            {
                return this._pageCount;
            }
        }

        [Bindable(true), DefaultValue(0), WebCategory("Data"), WebSysDescription("DetailsView_PageIndex")]
        public virtual int PageIndex
        {
            get
            {
                if ((this.Mode == DetailsViewMode.Insert) && !base.DesignMode)
                {
                    return -1;
                }
                return this.PageIndexInternal;
            }
            set
            {
                if (value < -1)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                if (value >= 0)
                {
                    this.PageIndexInternal = value;
                }
            }
        }

        private int PageIndexInternal
        {
            get
            {
                return this._pageIndex;
            }
            set
            {
                int pageIndexInternal = this.PageIndexInternal;
                if (value != pageIndexInternal)
                {
                    this._pageIndex = value;
                    if (base.Initialized)
                    {
                        base.RequiresDataBinding = true;
                    }
                }
            }
        }

        [WebCategory("Paging"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty), WebSysDescription("DetailsView_PagerSettings")]
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

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content), WebCategory("Styles"), NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty), WebSysDescription("WebControl_PagerStyle")]
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

        [WebSysDescription("View_PagerTemplate"), Browsable(false), DefaultValue((string) null), PersistenceMode(PersistenceMode.InnerProperty), TemplateContainer(typeof(DetailsView))]
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

        [WebSysDescription("DetailsView_Rows"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual DetailsViewRowCollection Rows
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
                    this._rowsCollection = new DetailsViewRowCollection(this._rowsArray);
                }
                return this._rowsCollection;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public IAutoFieldGenerator RowsGenerator
        {
            get
            {
                return this._rowsGenerator;
            }
            set
            {
                this._rowsGenerator = value;
            }
        }

        [DefaultValue((string) null), WebCategory("Styles"), WebSysDescription("View_RowStyle"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty)]
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

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object SelectedValue
        {
            get
            {
                return this.DataKey.Value;
            }
        }

        int IDataItemContainer.DataItemIndex
        {
            get
            {
                return this.DataItemIndex;
            }
        }

        int IDataItemContainer.DisplayIndex
        {
            get
            {
                return 0;
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

        System.Web.UI.WebControls.DataKey IDataBoundItemControl.DataKey
        {
            get
            {
                return this.DataKey;
            }
        }

        DataBoundControlMode IDataBoundItemControl.Mode
        {
            get
            {
                switch (this.Mode)
                {
                    case DetailsViewMode.ReadOnly:
                        return DataBoundControlMode.ReadOnly;

                    case DetailsViewMode.Edit:
                        return DataBoundControlMode.Edit;

                    case DetailsViewMode.Insert:
                        return DataBoundControlMode.Insert;
                }
                return DataBoundControlMode.ReadOnly;
            }
        }

        IAutoFieldGenerator IFieldControl.FieldsGenerator
        {
            get
            {
                return this.RowsGenerator;
            }
            set
            {
                this.RowsGenerator = value;
            }
        }

        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                if (!this.EnablePagingCallbacks)
                {
                    return HtmlTextWriterTag.Table;
                }
                return HtmlTextWriterTag.Div;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual DetailsViewRow TopPagerRow
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
    }
}

