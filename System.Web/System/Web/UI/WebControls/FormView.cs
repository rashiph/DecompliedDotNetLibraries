namespace System.Web.UI.WebControls
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls.Adapters;
    using System.Web.Util;

    [ControlValueProperty("SelectedValue"), Designer("System.Web.UI.Design.WebControls.FormViewDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), DefaultEvent("PageIndexChanging"), SupportsEventValidation, DataKeyProperty("DataKey")]
    public class FormView : CompositeDataBoundControl, IDataItemContainer, INamingContainer, IPostBackEventHandler, IPostBackContainer, IDataBoundItemControl, IDataBoundControl, IRenderOuterTableControl
    {
        private FormViewRow _bottomPagerRow;
        private OrderedDictionary _boundFieldValues;
        private object _dataItem;
        private int _dataItemIndex;
        private System.Web.UI.WebControls.DataKey _dataKey;
        private string[] _dataKeyNames;
        private FormViewMode _defaultMode;
        private IOrderedDictionary _deleteKeys;
        private IOrderedDictionary _deleteValues;
        private ITemplate _editItemTemplate;
        private TableItemStyle _editRowStyle;
        private TableItemStyle _emptyDataRowStyle;
        private ITemplate _emptyDataTemplate;
        private FormViewRow _footerRow;
        private TableItemStyle _footerStyle;
        private ITemplate _footerTemplate;
        private FormViewRow _headerRow;
        private TableItemStyle _headerStyle;
        private ITemplate _headerTemplate;
        private ITemplate _insertItemTemplate;
        private TableItemStyle _insertRowStyle;
        private IOrderedDictionary _insertValues;
        private ITemplate _itemTemplate;
        private OrderedDictionary _keyTable;
        private FormViewMode _mode;
        private string _modelValidationGroup;
        private bool _modeSet;
        private int _pageCount;
        private int _pageIndex;
        private System.Web.UI.WebControls.PagerSettings _pagerSettings;
        private TableItemStyle _pagerStyle;
        private ITemplate _pagerTemplate;
        private FormViewRow _row;
        private TableItemStyle _rowStyle;
        private FormViewRow _topPagerRow;
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

        [WebCategory("Action"), WebSysDescription("FormView_OnItemCommand")]
        public event FormViewCommandEventHandler ItemCommand
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

        [WebSysDescription("FormView_OnItemCreated"), WebCategory("Behavior")]
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
        public event FormViewDeletedEventHandler ItemDeleted
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

        [WebCategory("Action"), WebSysDescription("DataControls_OnItemDeleting")]
        public event FormViewDeleteEventHandler ItemDeleting
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

        [WebCategory("Action"), WebSysDescription("DataControls_OnItemInserted")]
        public event FormViewInsertedEventHandler ItemInserted
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
        public event FormViewInsertEventHandler ItemInserting
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

        [WebSysDescription("DataControls_OnItemUpdated"), WebCategory("Action")]
        public event FormViewUpdatedEventHandler ItemUpdated
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

        [WebCategory("Action"), WebSysDescription("DataControls_OnItemUpdating")]
        public event FormViewUpdateEventHandler ItemUpdating
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

        [WebSysDescription("FormView_OnModeChanged"), WebCategory("Action")]
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

        [WebCategory("Action"), WebSysDescription("FormView_OnModeChanging")]
        public event FormViewModeEventHandler ModeChanging
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

        [WebCategory("Action"), WebSysDescription("FormView_OnPageIndexChanged")]
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

        [WebCategory("Action"), WebSysDescription("FormView_OnPageIndexChanging")]
        public event FormViewPageEventHandler PageIndexChanging
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

        public void ChangeMode(FormViewMode newMode)
        {
            this.Mode = newMode;
        }

        protected override int CreateChildControls(IEnumerable dataSource, bool dataBinding)
        {
            PagedDataSource pagedDataSource = null;
            int pageIndex = this.PageIndex;
            bool allowPaging = this.AllowPaging;
            int count = 0;
            FormViewMode mode = this.Mode;
            if (base.DesignMode && (mode == FormViewMode.Insert))
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
                if (mode != FormViewMode.Insert)
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
            if (mode != FormViewMode.Insert)
            {
                pagedDataSource.DataSource = dataSource;
            }
            IEnumerator enumerator = null;
            OrderedDictionary keyTable = this.KeyTable;
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
                    if (mode != FormViewMode.Insert)
                    {
                        ICollection is4 = dataSource as ICollection;
                        if (((is4 == null) && pagedDataSource.IsPagingEnabled) && !pagedDataSource.IsServerPagingEnabled)
                        {
                            throw new HttpException(System.Web.SR.GetString("FormView_DataSourceMustBeCollection", new object[] { this.ID }));
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
            if (!flag2 && (mode != FormViewMode.Insert))
            {
                if ((this.EmptyDataText.Length > 0) || (this._emptyDataTemplate != null))
                {
                    this._row = this.CreateRow(0, DataControlRowType.EmptyDataRow, DataControlRowState.Normal, rows, null);
                }
                count = 0;
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
                if (((allowPaging && this.PagerSettings.Visible) && (this._pagerSettings.IsPagerOnTop && (mode != FormViewMode.Insert))) && !flag3)
                {
                    this._topPagerRow = this.CreateRow(pageIndex, DataControlRowType.Pager, DataControlRowState.Normal, rows, pagedDataSource);
                }
                this._headerRow = this.CreateRow(pageIndex, DataControlRowType.Header, DataControlRowState.Normal, rows, null);
                if ((this._headerTemplate == null) && (this.HeaderText.Length == 0))
                {
                    this._headerRow.Visible = false;
                }
                this._row = this.CreateDataRow(dataBinding, rows, this._dataItem);
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
                this._footerRow = this.CreateRow(pageIndex, DataControlRowType.Footer, DataControlRowState.Normal, rows, null);
                if ((this._footerTemplate == null) && (this.FooterText.Length == 0))
                {
                    this._footerRow.Visible = false;
                }
                if (((allowPaging && this.PagerSettings.Visible) && (this._pagerSettings.IsPagerOnBottom && (mode != FormViewMode.Insert))) && !flag3)
                {
                    this._bottomPagerRow = this.CreateRow(pageIndex, DataControlRowType.Pager, DataControlRowState.Normal, rows, pagedDataSource);
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
            return new TableStyle { CellSpacing = 0 };
        }

        private FormViewRow CreateDataRow(bool dataBinding, TableRowCollection rows, object dataItem)
        {
            ITemplate template = null;
            switch (this.Mode)
            {
                case FormViewMode.ReadOnly:
                    template = this._itemTemplate;
                    break;

                case FormViewMode.Edit:
                    template = this._editItemTemplate;
                    break;

                case FormViewMode.Insert:
                    if (this._insertItemTemplate == null)
                    {
                        template = this._editItemTemplate;
                        break;
                    }
                    template = this._insertItemTemplate;
                    break;
            }
            if (template != null)
            {
                return this.CreateDataRowFromTemplates(dataBinding, rows);
            }
            return null;
        }

        private FormViewRow CreateDataRowFromTemplates(bool dataBinding, TableRowCollection rows)
        {
            DataControlRowState normal = DataControlRowState.Normal;
            int pageIndex = this.PageIndex;
            FormViewMode mode = this.Mode;
            normal = DataControlRowState.Normal;
            switch (mode)
            {
                case FormViewMode.Edit:
                    normal |= DataControlRowState.Edit;
                    break;

                case FormViewMode.Insert:
                    normal |= DataControlRowState.Insert;
                    break;
            }
            return this.CreateRow(this.PageIndex, DataControlRowType.DataRow, normal, rows, null);
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
                    ((ImageButton) control).ImageUrl = firstPageImageUrl;
                    ((ImageButton) control).AlternateText = HttpUtility.HtmlDecode(pagerSettings.FirstPageText);
                }
                else
                {
                    control = new DataControlPagerLinkButton(this);
                    ((DataControlPagerLinkButton) control).Text = pagerSettings.FirstPageText;
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
                    ((ImageButton) control2).ImageUrl = previousPageImageUrl;
                    ((ImageButton) control2).AlternateText = HttpUtility.HtmlDecode(pagerSettings.PreviousPageText);
                }
                else
                {
                    control2 = new DataControlPagerLinkButton(this);
                    ((DataControlPagerLinkButton) control2).Text = pagerSettings.PreviousPageText;
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
                    ((ImageButton) control3).ImageUrl = nextPageImageUrl;
                    ((ImageButton) control3).AlternateText = HttpUtility.HtmlDecode(pagerSettings.NextPageText);
                }
                else
                {
                    control3 = new DataControlPagerLinkButton(this);
                    ((DataControlPagerLinkButton) control3).Text = pagerSettings.NextPageText;
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
                    ((ImageButton) control4).ImageUrl = lastPageImageUrl;
                    ((ImageButton) control4).AlternateText = HttpUtility.HtmlDecode(pagerSettings.LastPageText);
                }
                else
                {
                    control4 = new DataControlPagerLinkButton(this);
                    ((DataControlPagerLinkButton) control4).Text = pagerSettings.LastPageText;
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
            int num7 = num4;
            if (num2 > num7)
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
                num7 = (num6 + pageButtonCount) - 1;
                if (num7 > pageCount)
                {
                    num7 = pageCount;
                }
                if (((num7 - num6) + 1) < pageButtonCount)
                {
                    num6 = Math.Max(1, (num7 - pageButtonCount) + 1);
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
                    ((ImageButton) control).ImageUrl = firstPageImageUrl;
                    ((ImageButton) control).AlternateText = HttpUtility.HtmlDecode(pagerSettings.FirstPageText);
                }
                else
                {
                    control = new DataControlPagerLinkButton(this);
                    ((DataControlPagerLinkButton) control).Text = pagerSettings.FirstPageText;
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
                cell2.Controls.Add(button);
            }
            for (int i = num6; i <= num7; i++)
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
                    cell3.Controls.Add(button);
                }
            }
            if (pageCount > num7)
            {
                TableCell cell4 = new TableCell();
                row.Cells.Add(cell4);
                button = new DataControlPagerLinkButton(this) {
                    Text = "...",
                    CommandName = "Page"
                };
                button.CommandArgument = (num7 + 1).ToString(NumberFormatInfo.InvariantInfo);
                cell4.Controls.Add(button);
            }
            bool flag2 = num7 == pageCount;
            if ((addFirstLastPageButtons && (num2 != pageCount)) && !flag2)
            {
                IButtonControl control2;
                string lastPageImageUrl = pagerSettings.LastPageImageUrl;
                TableCell cell5 = new TableCell();
                row.Cells.Add(cell5);
                if (lastPageImageUrl.Length > 0)
                {
                    control2 = new DataControlImageButton(this);
                    ((ImageButton) control2).ImageUrl = lastPageImageUrl;
                    ((ImageButton) control2).AlternateText = HttpUtility.HtmlDecode(pagerSettings.LastPageText);
                }
                else
                {
                    control2 = new DataControlPagerLinkButton(this);
                    ((DataControlPagerLinkButton) control2).Text = pagerSettings.LastPageText;
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

        protected virtual FormViewRow CreateRow(int itemIndex, DataControlRowType rowType, DataControlRowState rowState)
        {
            if (rowType == DataControlRowType.Pager)
            {
                return new FormViewPagerRow(itemIndex, rowType, rowState);
            }
            return new FormViewRow(itemIndex, rowType, rowState);
        }

        private FormViewRow CreateRow(int itemIndex, DataControlRowType rowType, DataControlRowState rowState, TableRowCollection rows, PagedDataSource pagedDataSource)
        {
            FormViewRow row = this.CreateRow(itemIndex, rowType, rowState);
            row.RenderTemplateContainer = this.RenderOuterTable;
            rows.Add(row);
            if (rowType != DataControlRowType.Pager)
            {
                this.InitializeRow(row);
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

        protected override void EnsureDataBound()
        {
            if (base.RequiresDataBinding && (this.Mode == FormViewMode.Insert))
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

        protected virtual void ExtractRowValues(IOrderedDictionary fieldValues, bool includeKeys)
        {
            if (fieldValues != null)
            {
                DataBoundControlHelper.ExtractValuesFromBindableControls(fieldValues, this);
                IBindableTemplate itemTemplate = null;
                if ((this.Mode == FormViewMode.ReadOnly) && (this.ItemTemplate != null))
                {
                    itemTemplate = this.ItemTemplate as IBindableTemplate;
                }
                else if (((this.Mode == FormViewMode.Edit) || ((this.Mode == FormViewMode.Insert) && (this.InsertItemTemplate == null))) && (this.EditItemTemplate != null))
                {
                    itemTemplate = this.EditItemTemplate as IBindableTemplate;
                }
                else if ((this.Mode == FormViewMode.Insert) && (this.InsertItemTemplate != null))
                {
                    itemTemplate = this.InsertItemTemplate as IBindableTemplate;
                }
                string[] dataKeyNamesInternal = this.DataKeyNamesInternal;
                if (itemTemplate != null)
                {
                    FormView container = this;
                    if ((container != null) && (itemTemplate != null))
                    {
                        foreach (DictionaryEntry entry in itemTemplate.ExtractValues(container))
                        {
                            if (includeKeys || (Array.IndexOf<object>(dataKeyNamesInternal, entry.Key) == -1))
                            {
                                fieldValues[entry.Key] = entry.Value;
                            }
                        }
                    }
                }
            }
        }

        private void HandleCancel()
        {
            bool isBoundUsingDataSourceID = base.IsBoundUsingDataSourceID;
            FormViewModeEventArgs e = new FormViewModeEventArgs(this.DefaultMode, true);
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
            this.ExtractRowValues(fieldValues, false);
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
            if (this.PageIndex >= 0)
            {
                DataSourceView data = null;
                int pageIndex = this.PageIndex;
                bool isBoundUsingDataSourceID = base.IsBoundUsingDataSourceID;
                if (isBoundUsingDataSourceID)
                {
                    data = this.GetData();
                    if (data == null)
                    {
                        throw new HttpException(System.Web.SR.GetString("View_DataSourceReturnedNullView", new object[] { this.ID }));
                    }
                }
                FormViewDeleteEventArgs e = new FormViewDeleteEventArgs(pageIndex);
                this.ExtractRowValues(e.Values, false);
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
            FormViewDeletedEventArgs e = new FormViewDeletedEventArgs(affectedRows, ex);
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
                FormViewModeEventArgs e = new FormViewModeEventArgs(FormViewMode.Edit, false);
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
            FormViewCommandEventArgs args = e as FormViewCommandEventArgs;
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
                if (this.Mode != FormViewMode.Insert)
                {
                    throw new HttpException(System.Web.SR.GetString("DetailsViewFormView_ControlMustBeInInsertMode", new object[] { "FormView", this.ID }));
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
                FormViewInsertEventArgs e = new FormViewInsertEventArgs(commandArg);
                this.ExtractRowValues(e.Values, true);
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
            FormViewInsertedEventArgs e = new FormViewInsertedEventArgs(affectedRows, ex);
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
                FormViewModeEventArgs args2 = new FormViewModeEventArgs(this.DefaultMode, false);
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
            FormViewModeEventArgs e = new FormViewModeEventArgs(FormViewMode.Insert, false);
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
                FormViewPageEventArgs e = new FormViewPageEventArgs(newPage);
                this.OnPageIndexChanging(e);
                if ((!e.Cancel && (e.NewPageIndex > -1)) && ((e.NewPageIndex < this.PageCount) || (this._pageIndex != (this.PageCount - 1))))
                {
                    this._keyTable = null;
                    this._pageIndex = e.NewPageIndex;
                    this.OnPageIndexChanged(EventArgs.Empty);
                    base.RequiresDataBinding = true;
                }
            }
        }

        private void HandleUpdate(string commandArg, bool causesValidation)
        {
            if ((!causesValidation || (this.Page == null)) || this.Page.IsValid)
            {
                if (this.Mode != FormViewMode.Edit)
                {
                    throw new HttpException(System.Web.SR.GetString("DetailsViewFormView_ControlMustBeInEditMode", new object[] { "FormView", this.ID }));
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
                    FormViewUpdateEventArgs e = new FormViewUpdateEventArgs(commandArg);
                    foreach (DictionaryEntry entry in this.BoundFieldValues)
                    {
                        e.OldValues.Add(entry.Key, entry.Value);
                    }
                    this.ExtractRowValues(e.NewValues, true);
                    foreach (DictionaryEntry entry2 in this.DataKey.Values)
                    {
                        e.Keys.Add(entry2.Key, entry2.Value);
                    }
                    this.OnItemUpdating(e);
                    if (!e.Cancel && isBoundUsingDataSourceID)
                    {
                        this._updateKeys = e.Keys;
                        this._updateOldValues = e.OldValues;
                        this._updateNewValues = e.NewValues;
                        data.Update(e.Keys, e.NewValues, e.OldValues, new DataSourceViewOperationCallback(this.HandleUpdateCallback));
                    }
                }
            }
        }

        private bool HandleUpdateCallback(int affectedRows, Exception ex)
        {
            FormViewUpdatedEventArgs e = new FormViewUpdatedEventArgs(affectedRows, ex);
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
                FormViewModeEventArgs args2 = new FormViewModeEventArgs(this.DefaultMode, false);
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

        protected virtual void InitializePager(FormViewRow row, PagedDataSource pagedDataSource)
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

        protected virtual void InitializeRow(FormViewRow row)
        {
            TableCellCollection cells = row.Cells;
            TableCell container = new TableCell();
            ITemplate template = this._itemTemplate;
            int itemIndex = row.ItemIndex;
            DataControlRowState rowState = row.RowState;
            switch (row.RowType)
            {
                case DataControlRowType.Header:
                {
                    template = this._headerTemplate;
                    container.ColumnSpan = 2;
                    string headerText = this.HeaderText;
                    if ((this._headerTemplate == null) && (headerText.Length > 0))
                    {
                        container.Text = headerText;
                    }
                    break;
                }
                case DataControlRowType.Footer:
                {
                    template = this._footerTemplate;
                    container.ColumnSpan = 2;
                    string footerText = this.FooterText;
                    if ((this._footerTemplate == null) && (footerText.Length > 0))
                    {
                        container.Text = footerText;
                    }
                    break;
                }
                case DataControlRowType.DataRow:
                    container.ColumnSpan = 2;
                    if (((rowState & DataControlRowState.Edit) != DataControlRowState.Normal) && (this._editItemTemplate != null))
                    {
                        template = this._editItemTemplate;
                    }
                    if ((rowState & DataControlRowState.Insert) != DataControlRowState.Normal)
                    {
                        if (this._insertItemTemplate != null)
                        {
                            template = this._insertItemTemplate;
                        }
                        else
                        {
                            template = this._editItemTemplate;
                        }
                    }
                    break;

                case DataControlRowType.EmptyDataRow:
                {
                    template = this._emptyDataTemplate;
                    string emptyDataText = this.EmptyDataText;
                    if ((this._emptyDataTemplate == null) && (emptyDataText.Length > 0))
                    {
                        container.Text = emptyDataText;
                    }
                    break;
                }
            }
            if (template != null)
            {
                template.InstantiateIn(container);
            }
            cells.Add(container);
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
            this._defaultMode = FormViewMode.ReadOnly;
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
                    this._defaultMode = (FormViewMode) objArray[2];
                }
                if (objArray[3] != null)
                {
                    this.Mode = (FormViewMode) objArray[3];
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
                    ((IStateManager) this.EditRowStyle).LoadViewState(objArray[5]);
                }
                if (objArray[6] != null)
                {
                    ((IStateManager) this.InsertRowStyle).LoadViewState(objArray[6]);
                }
                if (objArray[7] != null)
                {
                    OrderedDictionaryStateHelper.LoadViewState((OrderedDictionary) this.BoundFieldValues, (ArrayList) objArray[7]);
                }
                if (objArray[8] != null)
                {
                    ((IStateManager) this.PagerSettings).LoadViewState(objArray[8]);
                }
                if (objArray[9] != null)
                {
                    ((IStateManager) base.ControlStyle).LoadViewState(objArray[9]);
                }
            }
            else
            {
                base.LoadViewState(null);
            }
        }

        protected internal virtual string ModifiedOuterTableStylePropertyName()
        {
            if (!string.IsNullOrEmpty(this.BackImageUrl))
            {
                return "BackImageUrl";
            }
            if (this.CellPadding != -1)
            {
                return "CellPadding";
            }
            if (this.CellSpacing != 0)
            {
                return "CellSpacing";
            }
            if (this.GridLines != System.Web.UI.WebControls.GridLines.None)
            {
                return "GridLines";
            }
            if (this.HorizontalAlign != System.Web.UI.WebControls.HorizontalAlign.NotSet)
            {
                return "HorizontalAlign";
            }
            if (((!this.Font.Bold && !this.Font.Italic) && (string.IsNullOrEmpty(this.Font.Name) && (this.Font.Names.Length == 0))) && ((!this.Font.Overline && !(this.Font.Size != FontUnit.Empty)) && (!this.Font.Strikeout && !this.Font.Underline)))
            {
                return LoginUtil.ModifiedOuterTableBasicStylePropertyName(this);
            }
            return "Font";
        }

        protected override bool OnBubbleEvent(object source, EventArgs e)
        {
            bool causesValidation = false;
            string validationGroup = string.Empty;
            FormViewCommandEventArgs args = e as FormViewCommandEventArgs;
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

        protected internal override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            if (this.Page != null)
            {
                if (this.DataKeyNames.Length > 0)
                {
                    this.Page.RegisterRequiresViewStateEncryption();
                }
                this.Page.RegisterRequiresControlState(this);
            }
        }

        protected virtual void OnItemCommand(FormViewCommandEventArgs e)
        {
            FormViewCommandEventHandler handler = (FormViewCommandEventHandler) base.Events[EventItemCommand];
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

        protected virtual void OnItemDeleted(FormViewDeletedEventArgs e)
        {
            FormViewDeletedEventHandler handler = (FormViewDeletedEventHandler) base.Events[EventItemDeleted];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnItemDeleting(FormViewDeleteEventArgs e)
        {
            bool isBoundUsingDataSourceID = base.IsBoundUsingDataSourceID;
            FormViewDeleteEventHandler handler = (FormViewDeleteEventHandler) base.Events[EventItemDeleting];
            if (handler != null)
            {
                handler(this, e);
            }
            else if (!isBoundUsingDataSourceID && !e.Cancel)
            {
                throw new HttpException(System.Web.SR.GetString("FormView_UnhandledEvent", new object[] { this.ID, "ItemDeleting" }));
            }
        }

        protected virtual void OnItemInserted(FormViewInsertedEventArgs e)
        {
            FormViewInsertedEventHandler handler = (FormViewInsertedEventHandler) base.Events[EventItemInserted];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnItemInserting(FormViewInsertEventArgs e)
        {
            bool isBoundUsingDataSourceID = base.IsBoundUsingDataSourceID;
            FormViewInsertEventHandler handler = (FormViewInsertEventHandler) base.Events[EventItemInserting];
            if (handler != null)
            {
                handler(this, e);
            }
            else if (!isBoundUsingDataSourceID && !e.Cancel)
            {
                throw new HttpException(System.Web.SR.GetString("FormView_UnhandledEvent", new object[] { this.ID, "ItemInserting" }));
            }
        }

        protected virtual void OnItemUpdated(FormViewUpdatedEventArgs e)
        {
            FormViewUpdatedEventHandler handler = (FormViewUpdatedEventHandler) base.Events[EventItemUpdated];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnItemUpdating(FormViewUpdateEventArgs e)
        {
            bool isBoundUsingDataSourceID = base.IsBoundUsingDataSourceID;
            FormViewUpdateEventHandler handler = (FormViewUpdateEventHandler) base.Events[EventItemUpdating];
            if (handler != null)
            {
                handler(this, e);
            }
            else if (!isBoundUsingDataSourceID && !e.Cancel)
            {
                throw new HttpException(System.Web.SR.GetString("FormView_UnhandledEvent", new object[] { this.ID, "ItemUpdating" }));
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

        protected virtual void OnModeChanging(FormViewModeEventArgs e)
        {
            bool isBoundUsingDataSourceID = base.IsBoundUsingDataSourceID;
            FormViewModeEventHandler handler = (FormViewModeEventHandler) base.Events[EventModeChanging];
            if (handler != null)
            {
                handler(this, e);
            }
            else if (!isBoundUsingDataSourceID && !e.Cancel)
            {
                throw new HttpException(System.Web.SR.GetString("FormView_UnhandledEvent", new object[] { this.ID, "ModeChanging" }));
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

        protected virtual void OnPageIndexChanging(FormViewPageEventArgs e)
        {
            bool isBoundUsingDataSourceID = base.IsBoundUsingDataSourceID;
            FormViewPageEventHandler handler = (FormViewPageEventHandler) base.Events[EventPageIndexChanging];
            if (handler != null)
            {
                handler(this, e);
            }
            else if (!isBoundUsingDataSourceID && !e.Cancel)
            {
                throw new HttpException(System.Web.SR.GetString("FormView_UnhandledEvent", new object[] { this.ID, "PageIndexChanging" }));
            }
        }

        private void OnPagerPropertyChanged(object sender, EventArgs e)
        {
            if (base.Initialized)
            {
                base.RequiresDataBinding = true;
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

        protected internal override void PerformDataBinding(IEnumerable data)
        {
            base.PerformDataBinding(data);
            if ((base.IsBoundUsingDataSourceID && (this.Mode == FormViewMode.Edit)) && base.IsViewStateEnabled)
            {
                this.ExtractRowValues(this.BoundFieldValues, false);
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
                    table.GridLines = System.Web.UI.WebControls.GridLines.None;
                    table.CellSpacing = 0;
                }
                table.Caption = this.Caption;
                table.CaptionAlign = this.CaptionAlign;
                foreach (FormViewRow row in table.Rows)
                {
                    Style s = new TableItemStyle();
                    DataControlRowState rowState = row.RowState;
                    switch (row.RowType)
                    {
                        case DataControlRowType.Header:
                            s = this._headerStyle;
                            break;

                        case DataControlRowType.Footer:
                            s = this._footerStyle;
                            break;

                        case DataControlRowType.DataRow:
                            s.CopyFrom(this._rowStyle);
                            if ((rowState & DataControlRowState.Edit) != DataControlRowState.Normal)
                            {
                                s.CopyFrom(this._editRowStyle);
                            }
                            if ((rowState & DataControlRowState.Insert) != DataControlRowState.Normal)
                            {
                                if (this._insertRowStyle != null)
                                {
                                    s.CopyFrom(this._insertRowStyle);
                                }
                                else
                                {
                                    s.CopyFrom(this._editRowStyle);
                                }
                            }
                            break;

                        case DataControlRowType.Pager:
                            s = this._pagerStyle;
                            break;

                        case DataControlRowType.EmptyDataRow:
                            s = this._emptyDataRowStyle;
                            break;
                    }
                    if ((s != null) && row.Visible)
                    {
                        row.MergeStyle(s);
                    }
                }
            }
        }

        protected virtual void RaisePostBackEvent(string eventArgument)
        {
            base.ValidateEvent(this.UniqueID, eventArgument);
            int index = eventArgument.IndexOf('$');
            if (index >= 0)
            {
                CommandEventArgs originalArgs = new CommandEventArgs(eventArgument.Substring(0, index), eventArgument.Substring(index + 1));
                FormViewCommandEventArgs e = new FormViewCommandEventArgs(this, originalArgs);
                this.HandleEvent(e, false, string.Empty);
            }
        }

        protected internal override void Render(HtmlTextWriter writer)
        {
            if (this.Page != null)
            {
                this.Page.VerifyRenderingInServerForm(this);
            }
            if (this.RenderOuterTable)
            {
                this.PrepareControlHierarchy();
                this.RenderContents(writer);
            }
            else
            {
                string str = this.ModifiedOuterTableStylePropertyName();
                if (!string.IsNullOrEmpty(str))
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("IRenderOuterTableControl_CannotSetStyleWhenDisableRenderOuterTable", new object[] { str, base.GetType().Name, this.ID }));
                }
                if (this.Controls.Count > 0)
                {
                    this.Controls[0].RenderChildren(writer);
                }
            }
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
            if ((((((obj2 == null) && (this._pageIndex == 0)) && ((this._mode == this._defaultMode) && (this._defaultMode == FormViewMode.ReadOnly))) && ((this._dataKeyNames == null) || (this._dataKeyNames.Length <= 0))) && ((this._keyTable == null) || (this._keyTable.Count <= 0))) && (this._pageCount == 0))
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
            if (this._defaultMode != FormViewMode.ReadOnly)
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
            object obj7 = (this._editRowStyle != null) ? ((IStateManager) this._editRowStyle).SaveViewState() : null;
            object obj8 = (this._insertRowStyle != null) ? ((IStateManager) this._insertRowStyle).SaveViewState() : null;
            object obj9 = (this._boundFieldValues != null) ? OrderedDictionaryStateHelper.SaveViewState(this._boundFieldValues) : null;
            object obj10 = (this._pagerSettings != null) ? ((IStateManager) this._pagerSettings).SaveViewState() : null;
            object obj11 = base.ControlStyleCreated ? ((IStateManager) base.ControlStyle).SaveViewState() : null;
            return new object[] { obj2, obj3, obj4, obj5, obj6, obj7, obj8, obj9, obj10, obj11 };
        }

        private void SelectCallback(IEnumerable data)
        {
            throw new HttpException(System.Web.SR.GetString("DataBoundControl_DataSourceDoesntSupportPaging"));
        }

        public void SetPageIndex(int index)
        {
            this.HandlePage(index);
        }

        void IPostBackEventHandler.RaisePostBackEvent(string eventArgument)
        {
            this.RaisePostBackEvent(eventArgument);
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

        [WebCategory("Paging"), WebSysDescription("FormView_AllowPaging"), DefaultValue(false)]
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

        [DefaultValue(""), WebCategory("Appearance"), UrlProperty, WebSysDescription("WebControl_BackImageUrl"), Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
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
        public virtual FormViewRow BottomPagerRow
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
                    int capacity = 0x19;
                    this._boundFieldValues = new OrderedDictionary(capacity);
                }
                return this._boundFieldValues;
            }
        }

        [WebSysDescription("DataControls_Caption"), DefaultValue(""), Localizable(true), WebCategory("Accessibility")]
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

        [WebSysDescription("WebControl_CaptionAlign"), DefaultValue(0), WebCategory("Accessibility")]
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

        [WebCategory("Layout"), WebSysDescription("FormView_CellPadding"), DefaultValue(-1)]
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

        [DefaultValue(0), WebCategory("Layout"), WebSysDescription("FormView_CellSpacing")]
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

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public FormViewMode CurrentMode
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
                if (this.CurrentMode == FormViewMode.Insert)
                {
                    return null;
                }
                return this._dataItem;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
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
                if (this.CurrentMode == FormViewMode.Insert)
                {
                    return -1;
                }
                return this._dataItemIndex;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), WebSysDescription("FormView_DataKey"), Browsable(false)]
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

        [DefaultValue((string) null), WebSysDescription("DataControls_DataKeyNames"), Editor("System.Web.UI.Design.WebControls.DataFieldEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), TypeConverter(typeof(StringArrayConverter)), WebCategory("Data")]
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

        [WebSysDescription("View_DefaultMode"), DefaultValue(0), WebCategory("Behavior")]
        public virtual FormViewMode DefaultMode
        {
            get
            {
                return this._defaultMode;
            }
            set
            {
                if ((value < FormViewMode.ReadOnly) || (value > FormViewMode.Insert))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this._defaultMode = value;
            }
        }

        [DefaultValue((string) null), PersistenceMode(PersistenceMode.InnerProperty), TemplateContainer(typeof(FormView), BindingDirection.TwoWay), WebSysDescription("FormView_EditItemTemplate"), Browsable(false)]
        public virtual ITemplate EditItemTemplate
        {
            get
            {
                return this._editItemTemplate;
            }
            set
            {
                this._editItemTemplate = value;
            }
        }

        [WebSysDescription("View_EditRowStyle"), WebCategory("Styles"), DefaultValue((string) null), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty)]
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

        [WebCategory("Styles"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), DefaultValue((string) null), PersistenceMode(PersistenceMode.InnerProperty), WebSysDescription("View_EmptyDataRowStyle"), NotifyParentProperty(true)]
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

        [TemplateContainer(typeof(FormView)), PersistenceMode(PersistenceMode.InnerProperty), Browsable(false), WebSysDescription("View_EmptyDataTemplate"), DefaultValue((string) null)]
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

        [Localizable(true), DefaultValue(""), WebSysDescription("View_EmptyDataText"), WebCategory("Appearance")]
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

        [WebCategory("Behavior"), WebSysDescription("DataBoundControl_EnableModelValidation"), DefaultValue(true)]
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
        public virtual FormViewRow FooterRow
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

        [WebSysDescription("FormView_FooterStyle"), NotifyParentProperty(true), WebCategory("Styles"), DefaultValue((string) null), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), PersistenceMode(PersistenceMode.InnerProperty)]
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

        [DefaultValue((string) null), Browsable(false), WebSysDescription("FormView_FooterTemplate"), PersistenceMode(PersistenceMode.InnerProperty), TemplateContainer(typeof(FormView))]
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

        [WebSysDescription("View_FooterText"), Localizable(true), WebCategory("Appearance"), DefaultValue("")]
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

        [WebSysDescription("DataControls_GridLines"), WebCategory("Appearance"), DefaultValue(0)]
        public virtual System.Web.UI.WebControls.GridLines GridLines
        {
            get
            {
                if (!base.ControlStyleCreated)
                {
                    return System.Web.UI.WebControls.GridLines.None;
                }
                return ((TableStyle) base.ControlStyle).GridLines;
            }
            set
            {
                ((TableStyle) base.ControlStyle).GridLines = value;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual FormViewRow HeaderRow
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

        [WebSysDescription("WebControl_HeaderStyle"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true), WebCategory("Styles"), DefaultValue((string) null), PersistenceMode(PersistenceMode.InnerProperty)]
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

        [WebSysDescription("WebControl_HeaderTemplate"), DefaultValue((string) null), PersistenceMode(PersistenceMode.InnerProperty), Browsable(false), TemplateContainer(typeof(FormView))]
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

        [Localizable(true), WebSysDescription("View_HeaderText"), WebCategory("Appearance"), DefaultValue("")]
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

        [WebSysDescription("WebControl_HorizontalAlign"), Category("Layout"), DefaultValue(0)]
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

        [TemplateContainer(typeof(FormView), BindingDirection.TwoWay), Browsable(false), DefaultValue((string) null), PersistenceMode(PersistenceMode.InnerProperty), WebSysDescription("FormView_InsertItemTemplate")]
        public virtual ITemplate InsertItemTemplate
        {
            get
            {
                return this._insertItemTemplate;
            }
            set
            {
                this._insertItemTemplate = value;
            }
        }

        [WebCategory("Styles"), WebSysDescription("View_InsertRowStyle"), DefaultValue((string) null), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty)]
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

        [WebSysDescription("View_InsertRowStyle"), TemplateContainer(typeof(FormView), BindingDirection.TwoWay), Browsable(false), DefaultValue((string) null), PersistenceMode(PersistenceMode.InnerProperty)]
        public virtual ITemplate ItemTemplate
        {
            get
            {
                return this._itemTemplate;
            }
            set
            {
                this._itemTemplate = value;
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

        private FormViewMode Mode
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
                if ((value < FormViewMode.ReadOnly) || (value > FormViewMode.Insert))
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

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public virtual int PageCount
        {
            get
            {
                return this._pageCount;
            }
        }

        [DefaultValue(0), Bindable(true), WebCategory("Data"), WebSysDescription("FormView_PageIndex")]
        public virtual int PageIndex
        {
            get
            {
                if ((this.Mode == FormViewMode.Insert) && !base.DesignMode)
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

        [WebCategory("Styles"), PersistenceMode(PersistenceMode.InnerProperty), WebSysDescription("WebControl_PagerStyle"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true)]
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

        [TemplateContainer(typeof(FormView)), WebSysDescription("View_PagerTemplate"), DefaultValue((string) null), PersistenceMode(PersistenceMode.InnerProperty), Browsable(false)]
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

        [WebCategory("Layout"), DefaultValue(true), WebSysDescription("FormView_RenderOuterTable")]
        public virtual bool RenderOuterTable
        {
            get
            {
                object obj2 = this.ViewState["RenderOuterTable"];
                return ((obj2 == null) || ((bool) obj2));
            }
            set
            {
                this.ViewState["RenderOuterTable"] = value;
            }
        }

        [Browsable(false), WebSysDescription("FormView_Rows"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual FormViewRow Row
        {
            get
            {
                if (this._row == null)
                {
                    this.EnsureChildControls();
                }
                return this._row;
            }
        }

        [PersistenceMode(PersistenceMode.InnerProperty), WebCategory("Styles"), DefaultValue((string) null), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true), WebSysDescription("View_RowStyle")]
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
                    case FormViewMode.ReadOnly:
                        return DataBoundControlMode.ReadOnly;

                    case FormViewMode.Edit:
                        return DataBoundControlMode.Edit;

                    case FormViewMode.Insert:
                        return DataBoundControlMode.Insert;
                }
                return DataBoundControlMode.ReadOnly;
            }
        }

        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                return HtmlTextWriterTag.Table;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual FormViewRow TopPagerRow
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

