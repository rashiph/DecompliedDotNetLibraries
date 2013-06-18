namespace System.Web.UI.WebControls
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Web;
    using System.Web.UI;
    using System.Web.Util;

    [Designer("System.Web.UI.Design.WebControls.DataListDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), Editor("System.Web.UI.Design.WebControls.DataListComponentEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(ComponentEditor)), ControlValueProperty("SelectedValue")]
    public class DataList : BaseDataList, INamingContainer, IRepeatInfoUser, IWizardSideBarListControl
    {
        private TableItemStyle alternatingItemStyle;
        private ITemplate alternatingItemTemplate;
        public const string CancelCommandName = "Cancel";
        public const string DeleteCommandName = "Delete";
        public const string EditCommandName = "Edit";
        private TableItemStyle editItemStyle;
        private ITemplate editItemTemplate;
        private static readonly object EventCancelCommand = new object();
        private static readonly object EventDeleteCommand = new object();
        private static readonly object EventEditCommand = new object();
        private static readonly object EventItemCommand = new object();
        private static readonly object EventItemCreated = new object();
        private static readonly object EventItemDataBound = new object();
        private static readonly object EventUpdateCommand = new object();
        private static readonly object EventWizardListItemDataBound = new object();
        private bool extractTemplateRows;
        private TableItemStyle footerStyle;
        private ITemplate footerTemplate;
        private TableItemStyle headerStyle;
        private ITemplate headerTemplate;
        private ArrayList itemsArray;
        private DataListItemCollection itemsCollection;
        private TableItemStyle itemStyle;
        private ITemplate itemTemplate;
        private int offset = 0;
        public const string SelectCommandName = "Select";
        private TableItemStyle selectedItemStyle;
        private ITemplate selectedItemTemplate;
        private TableItemStyle separatorStyle;
        private ITemplate separatorTemplate;
        public const string UpdateCommandName = "Update";
        private int visibleItemCount = -1;

        [WebSysDescription("DataList_OnCancelCommand"), WebCategory("Action")]
        public event DataListCommandEventHandler CancelCommand
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

        [WebCategory("Action"), WebSysDescription("DataList_OnDeleteCommand")]
        public event DataListCommandEventHandler DeleteCommand
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

        [WebCategory("Action"), WebSysDescription("DataList_OnEditCommand")]
        public event DataListCommandEventHandler EditCommand
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

        [WebCategory("Action"), WebSysDescription("DataList_OnItemCommand")]
        public event DataListCommandEventHandler ItemCommand
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

        [WebCategory("Behavior"), WebSysDescription("DataControls_OnItemCreated")]
        public event DataListItemEventHandler ItemCreated
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
        public event DataListItemEventHandler ItemDataBound
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

        event CommandEventHandler IWizardSideBarListControl.ItemCommand
        {
            add
            {
                this.ItemCommand += new DataListCommandEventHandler(value.Invoke);
            }
            remove
            {
                this.ItemCommand -= new DataListCommandEventHandler(value.Invoke);
            }
        }

        event EventHandler<WizardSideBarListControlItemEventArgs> IWizardSideBarListControl.ItemDataBound
        {
            add
            {
                base.Events.AddHandler(EventWizardListItemDataBound, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventWizardListItemDataBound, value);
            }
        }

        [WebSysDescription("DataList_OnUpdateCommand"), WebCategory("Action")]
        public event DataListCommandEventHandler UpdateCommand
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

        public DataList()
        {
            this.visibleItemCount = -1;
        }

        protected override void CreateControlHierarchy(bool useDataSource)
        {
            IEnumerable data = null;
            int dataItemCount = -1;
            ArrayList dataKeysArray = base.DataKeysArray;
            this.extractTemplateRows = this.ExtractTemplateRows;
            if (this.itemsArray != null)
            {
                this.itemsArray.Clear();
            }
            else
            {
                this.itemsArray = new ArrayList();
            }
            if (!useDataSource)
            {
                dataItemCount = (int) this.ViewState["_!ItemCount"];
                if (dataItemCount != -1)
                {
                    data = new DummyDataSource(dataItemCount);
                    this.itemsArray.Capacity = dataItemCount;
                }
            }
            else
            {
                dataKeysArray.Clear();
                data = this.GetData();
                ICollection is2 = data as ICollection;
                if (is2 != null)
                {
                    dataKeysArray.Capacity = is2.Count;
                    this.itemsArray.Capacity = is2.Count;
                }
            }
            if (data != null)
            {
                ControlCollection controls = this.Controls;
                int itemIndex = 0;
                bool flag = this.separatorTemplate != null;
                int editItemIndex = this.EditItemIndex;
                int selectedIndex = this.SelectedIndex;
                string dataKeyField = this.DataKeyField;
                bool flag2 = useDataSource && (dataKeyField.Length != 0);
                dataItemCount = 0;
                if (this.headerTemplate != null)
                {
                    this.CreateItem(-1, ListItemType.Header, useDataSource, null);
                }
                foreach (object obj2 in data)
                {
                    if (flag2)
                    {
                        object propertyValue = DataBinder.GetPropertyValue(obj2, dataKeyField);
                        dataKeysArray.Add(propertyValue);
                    }
                    ListItemType itemType = ListItemType.Item;
                    if (itemIndex == editItemIndex)
                    {
                        itemType = ListItemType.EditItem;
                    }
                    else if (itemIndex == selectedIndex)
                    {
                        itemType = ListItemType.SelectedItem;
                    }
                    else if ((itemIndex % 2) != 0)
                    {
                        itemType = ListItemType.AlternatingItem;
                    }
                    DataListItem item = this.CreateItem(itemIndex, itemType, useDataSource, obj2);
                    this.itemsArray.Add(item);
                    if (flag)
                    {
                        this.CreateItem(itemIndex, ListItemType.Separator, useDataSource, null);
                    }
                    dataItemCount++;
                    itemIndex++;
                }
                if (this.footerTemplate != null)
                {
                    this.CreateItem(-1, ListItemType.Footer, useDataSource, null);
                }
            }
            if (useDataSource)
            {
                this.ViewState["_!ItemCount"] = (data != null) ? dataItemCount : -1;
            }
        }

        protected override Style CreateControlStyle()
        {
            return new TableStyle { CellSpacing = 0 };
        }

        protected virtual DataListItem CreateItem(int itemIndex, ListItemType itemType)
        {
            return new DataListItem(itemIndex, itemType);
        }

        private DataListItem CreateItem(int itemIndex, ListItemType itemType, bool dataBind, object dataItem)
        {
            DataListItem item = this.CreateItem(itemIndex, itemType);
            DataListItemEventArgs e = new DataListItemEventArgs(item);
            this.InitializeItem(item);
            if (dataBind)
            {
                item.DataItem = dataItem;
            }
            this.OnItemCreated(e);
            this.Controls.Add(item);
            if (dataBind)
            {
                item.DataBind();
                this.OnItemDataBound(e);
                item.DataItem = null;
            }
            return item;
        }

        private DataListItem GetItem(ListItemType itemType, int repeatIndex)
        {
            switch (itemType)
            {
                case ListItemType.Header:
                    return (DataListItem) this.Controls[0];

                case ListItemType.Footer:
                    return (DataListItem) this.Controls[this.Controls.Count - 1];

                case ListItemType.Item:
                case ListItemType.AlternatingItem:
                case ListItemType.SelectedItem:
                case ListItemType.EditItem:
                    return (DataListItem) this.itemsArray[repeatIndex];

                case ListItemType.Separator:
                {
                    int num = (repeatIndex * 2) + 1;
                    if (this.headerTemplate != null)
                    {
                        num++;
                    }
                    return (DataListItem) this.Controls[num];
                }
            }
            return null;
        }

        protected virtual void InitializeItem(DataListItem item)
        {
            ITemplate itemTemplate = this.itemTemplate;
            switch (item.ItemType)
            {
                case ListItemType.Header:
                    itemTemplate = this.headerTemplate;
                    goto Label_00A4;

                case ListItemType.Footer:
                    itemTemplate = this.footerTemplate;
                    goto Label_00A4;

                case ListItemType.AlternatingItem:
                    break;

                case ListItemType.SelectedItem:
                    goto Label_0055;

                case ListItemType.EditItem:
                    if (this.editItemTemplate == null)
                    {
                        if (item.ItemIndex == this.SelectedIndex)
                        {
                            goto Label_0055;
                        }
                        if ((item.ItemIndex % 2) != 0)
                        {
                            break;
                        }
                    }
                    else
                    {
                        itemTemplate = this.editItemTemplate;
                    }
                    goto Label_00A4;

                case ListItemType.Separator:
                    itemTemplate = this.separatorTemplate;
                    goto Label_00A4;

                default:
                    goto Label_00A4;
            }
        Label_0044:
            if (this.alternatingItemTemplate != null)
            {
                itemTemplate = this.alternatingItemTemplate;
            }
            goto Label_00A4;
        Label_0055:
            if (this.selectedItemTemplate != null)
            {
                itemTemplate = this.selectedItemTemplate;
            }
            else if ((item.ItemIndex % 2) != 0)
            {
                goto Label_0044;
            }
        Label_00A4:
            if (itemTemplate != null)
            {
                itemTemplate.InstantiateIn(item);
            }
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
                    ((IStateManager) this.ItemStyle).LoadViewState(objArray[1]);
                }
                if (objArray[2] != null)
                {
                    ((IStateManager) this.SelectedItemStyle).LoadViewState(objArray[2]);
                }
                if (objArray[3] != null)
                {
                    ((IStateManager) this.AlternatingItemStyle).LoadViewState(objArray[3]);
                }
                if (objArray[4] != null)
                {
                    ((IStateManager) this.EditItemStyle).LoadViewState(objArray[4]);
                }
                if (objArray[5] != null)
                {
                    ((IStateManager) this.SeparatorStyle).LoadViewState(objArray[5]);
                }
                if (objArray[6] != null)
                {
                    ((IStateManager) this.HeaderStyle).LoadViewState(objArray[6]);
                }
                if (objArray[7] != null)
                {
                    ((IStateManager) this.FooterStyle).LoadViewState(objArray[7]);
                }
                if (objArray[8] != null)
                {
                    ((IStateManager) base.ControlStyle).LoadViewState(objArray[8]);
                }
            }
        }

        protected override bool OnBubbleEvent(object source, EventArgs e)
        {
            bool flag = false;
            if (e is DataListCommandEventArgs)
            {
                DataListCommandEventArgs args = (DataListCommandEventArgs) e;
                this.OnItemCommand(args);
                flag = true;
                string commandName = args.CommandName;
                if (StringUtil.EqualsIgnoreCase(commandName, "Select"))
                {
                    this.SelectedIndex = args.Item.ItemIndex;
                    this.OnSelectedIndexChanged(EventArgs.Empty);
                    return flag;
                }
                if (StringUtil.EqualsIgnoreCase(commandName, "Edit"))
                {
                    this.OnEditCommand(args);
                    return flag;
                }
                if (StringUtil.EqualsIgnoreCase(commandName, "Delete"))
                {
                    this.OnDeleteCommand(args);
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
                }
            }
            return flag;
        }

        protected virtual void OnCancelCommand(DataListCommandEventArgs e)
        {
            DataListCommandEventHandler handler = (DataListCommandEventHandler) base.Events[EventCancelCommand];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnDeleteCommand(DataListCommandEventArgs e)
        {
            DataListCommandEventHandler handler = (DataListCommandEventHandler) base.Events[EventDeleteCommand];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnEditCommand(DataListCommandEventArgs e)
        {
            DataListCommandEventHandler handler = (DataListCommandEventHandler) base.Events[EventEditCommand];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected internal override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            if ((this.Page != null) && (this.DataKeyField.Length > 0))
            {
                this.Page.RegisterRequiresViewStateEncryption();
            }
        }

        protected virtual void OnItemCommand(DataListCommandEventArgs e)
        {
            DataListCommandEventHandler handler = (DataListCommandEventHandler) base.Events[EventItemCommand];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnItemCreated(DataListItemEventArgs e)
        {
            DataListItemEventHandler handler = (DataListItemEventHandler) base.Events[EventItemCreated];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnItemDataBound(DataListItemEventArgs e)
        {
            DataListItemEventHandler handler = (DataListItemEventHandler) base.Events[EventItemDataBound];
            if (handler != null)
            {
                handler(this, e);
            }
            EventHandler<WizardSideBarListControlItemEventArgs> handler2 = (EventHandler<WizardSideBarListControlItemEventArgs>) base.Events[EventWizardListItemDataBound];
            if (handler2 != null)
            {
                DataListItem container = e.Item;
                WizardSideBarListControlItemEventArgs args = new WizardSideBarListControlItemEventArgs(new WizardSideBarListControlItem(container.DataItem, container.ItemType, container.ItemIndex, container));
                handler2(this, args);
            }
        }

        protected virtual void OnUpdateCommand(DataListCommandEventArgs e)
        {
            DataListCommandEventHandler handler = (DataListCommandEventHandler) base.Events[EventUpdateCommand];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected internal override void PrepareControlHierarchy()
        {
            ControlCollection controls = this.Controls;
            int count = controls.Count;
            if (count != 0)
            {
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
                for (int i = 0; i < count; i++)
                {
                    DataListItem item = (DataListItem) controls[i];
                    Style headerStyle = null;
                    switch (item.ItemType)
                    {
                        case ListItemType.Header:
                            if (this.ShowHeader)
                            {
                                headerStyle = this.headerStyle;
                            }
                            goto Label_015B;

                        case ListItemType.Footer:
                            if (this.ShowFooter)
                            {
                                headerStyle = this.footerStyle;
                            }
                            goto Label_015B;

                        case ListItemType.Item:
                            headerStyle = this.itemStyle;
                            goto Label_015B;

                        case ListItemType.AlternatingItem:
                            headerStyle = s;
                            goto Label_015B;

                        case ListItemType.SelectedItem:
                            headerStyle = new TableItemStyle();
                            if ((item.ItemIndex % 2) == 0)
                            {
                                break;
                            }
                            headerStyle.CopyFrom(s);
                            goto Label_0100;

                        case ListItemType.EditItem:
                            headerStyle = new TableItemStyle();
                            if ((item.ItemIndex % 2) == 0)
                            {
                                goto Label_0128;
                            }
                            headerStyle.CopyFrom(s);
                            goto Label_0134;

                        case ListItemType.Separator:
                            headerStyle = this.separatorStyle;
                            goto Label_015B;

                        default:
                            goto Label_015B;
                    }
                    headerStyle.CopyFrom(this.itemStyle);
                Label_0100:
                    headerStyle.CopyFrom(this.selectedItemStyle);
                    goto Label_015B;
                Label_0128:
                    headerStyle.CopyFrom(this.itemStyle);
                Label_0134:
                    if (item.ItemIndex == this.SelectedIndex)
                    {
                        headerStyle.CopyFrom(this.selectedItemStyle);
                    }
                    headerStyle.CopyFrom(this.editItemStyle);
                Label_015B:
                    if (headerStyle != null)
                    {
                        if (!this.extractTemplateRows)
                        {
                            item.MergeStyle(headerStyle);
                        }
                        else
                        {
                            IEnumerator enumerator = item.Controls.GetEnumerator();
                            while (enumerator.MoveNext())
                            {
                                Control current = (Control) enumerator.Current;
                                if (current is Table)
                                {
                                    IEnumerator enumerator2 = ((Table) current).Rows.GetEnumerator();
                                    while (enumerator2.MoveNext())
                                    {
                                        ((TableRow) enumerator2.Current).MergeStyle(headerStyle);
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        protected internal override void RenderContents(HtmlTextWriter writer)
        {
            if (this.Controls.Count != 0)
            {
                RepeatInfo info = new RepeatInfo();
                Table table = null;
                Style controlStyle = base.ControlStyle;
                if (this.extractTemplateRows)
                {
                    info.RepeatDirection = System.Web.UI.WebControls.RepeatDirection.Vertical;
                    info.RepeatLayout = System.Web.UI.WebControls.RepeatLayout.Flow;
                    info.RepeatColumns = 1;
                    info.OuterTableImplied = true;
                    table = new Table {
                        ID = this.ClientID
                    };
                    table.CopyBaseAttributes(this);
                    table.Caption = this.Caption;
                    table.CaptionAlign = this.CaptionAlign;
                    table.ApplyStyle(controlStyle);
                    table.RenderBeginTag(writer);
                }
                else
                {
                    info.RepeatDirection = this.RepeatDirection;
                    info.RepeatLayout = this.RepeatLayout;
                    info.RepeatColumns = this.RepeatColumns;
                    if (info.RepeatLayout == System.Web.UI.WebControls.RepeatLayout.Table)
                    {
                        info.Caption = this.Caption;
                        info.CaptionAlign = this.CaptionAlign;
                        info.UseAccessibleHeader = this.UseAccessibleHeader;
                    }
                    else
                    {
                        info.EnableLegacyRendering = base.EnableLegacyRendering;
                    }
                }
                info.RenderRepeater(writer, this, controlStyle, this);
                if (table != null)
                {
                    table.RenderEndTag(writer);
                }
            }
        }

        protected override object SaveViewState()
        {
            object obj2 = base.SaveViewState();
            object obj3 = (this.itemStyle != null) ? ((IStateManager) this.itemStyle).SaveViewState() : null;
            object obj4 = (this.selectedItemStyle != null) ? ((IStateManager) this.selectedItemStyle).SaveViewState() : null;
            object obj5 = (this.alternatingItemStyle != null) ? ((IStateManager) this.alternatingItemStyle).SaveViewState() : null;
            object obj6 = (this.editItemStyle != null) ? ((IStateManager) this.editItemStyle).SaveViewState() : null;
            object obj7 = (this.separatorStyle != null) ? ((IStateManager) this.separatorStyle).SaveViewState() : null;
            object obj8 = (this.headerStyle != null) ? ((IStateManager) this.headerStyle).SaveViewState() : null;
            object obj9 = (this.footerStyle != null) ? ((IStateManager) this.footerStyle).SaveViewState() : null;
            object obj10 = base.ControlStyleCreated ? ((IStateManager) base.ControlStyle).SaveViewState() : null;
            return new object[] { obj2, obj3, obj4, obj5, obj6, obj7, obj8, obj9, obj10 };
        }

        Style IRepeatInfoUser.GetItemStyle(ListItemType itemType, int repeatIndex)
        {
            DataListItem item = this.GetItem(itemType, repeatIndex);
            if ((item != null) && item.ControlStyleCreated)
            {
                return item.ControlStyle;
            }
            return null;
        }

        void IRepeatInfoUser.RenderItem(ListItemType itemType, int repeatIndex, RepeatInfo repeatInfo, HtmlTextWriter writer)
        {
            DataListItem item = this.GetItem(itemType, repeatIndex + this.offset);
            if (item != null)
            {
                item.RenderItem(writer, this.extractTemplateRows, repeatInfo.RepeatLayout == System.Web.UI.WebControls.RepeatLayout.Table);
            }
        }

        protected override void TrackViewState()
        {
            base.TrackViewState();
            if (this.itemStyle != null)
            {
                ((IStateManager) this.itemStyle).TrackViewState();
            }
            if (this.selectedItemStyle != null)
            {
                ((IStateManager) this.selectedItemStyle).TrackViewState();
            }
            if (this.alternatingItemStyle != null)
            {
                ((IStateManager) this.alternatingItemStyle).TrackViewState();
            }
            if (this.editItemStyle != null)
            {
                ((IStateManager) this.editItemStyle).TrackViewState();
            }
            if (this.separatorStyle != null)
            {
                ((IStateManager) this.separatorStyle).TrackViewState();
            }
            if (this.headerStyle != null)
            {
                ((IStateManager) this.headerStyle).TrackViewState();
            }
            if (this.footerStyle != null)
            {
                ((IStateManager) this.footerStyle).TrackViewState();
            }
            if (base.ControlStyleCreated)
            {
                ((IStateManager) base.ControlStyle).TrackViewState();
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content), WebCategory("Styles"), DefaultValue((string) null), WebSysDescription("DataList_AlternatingItemStyle"), NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty)]
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

        [Browsable(false), WebSysDescription("DataList_AlternatingItemTemplate"), DefaultValue((string) null), PersistenceMode(PersistenceMode.InnerProperty), TemplateContainer(typeof(DataListItem))]
        public virtual ITemplate AlternatingItemTemplate
        {
            get
            {
                return this.alternatingItemTemplate;
            }
            set
            {
                this.alternatingItemTemplate = value;
            }
        }

        [WebCategory("Default"), WebSysDescription("DataList_EditItemIndex"), DefaultValue(-1)]
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

        [PersistenceMode(PersistenceMode.InnerProperty), WebCategory("Styles"), DefaultValue((string) null), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true), WebSysDescription("DataList_EditItemStyle")]
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

        [Browsable(false), TemplateContainer(typeof(DataListItem)), WebSysDescription("DataList_EditItemTemplate"), PersistenceMode(PersistenceMode.InnerProperty), DefaultValue((string) null)]
        public virtual ITemplate EditItemTemplate
        {
            get
            {
                return this.editItemTemplate;
            }
            set
            {
                this.editItemTemplate = value;
            }
        }

        [WebSysDescription("DataList_ExtractTemplateRows"), WebCategory("Layout"), DefaultValue(false)]
        public virtual bool ExtractTemplateRows
        {
            get
            {
                object obj2 = this.ViewState["ExtractTemplateRows"];
                return ((obj2 != null) && ((bool) obj2));
            }
            set
            {
                this.ViewState["ExtractTemplateRows"] = value;
            }
        }

        [PersistenceMode(PersistenceMode.InnerProperty), WebSysDescription("DataControls_FooterStyle"), WebCategory("Styles"), DefaultValue((string) null), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true)]
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

        [WebSysDescription("DataList_FooterTemplate"), Browsable(false), DefaultValue((string) null), PersistenceMode(PersistenceMode.InnerProperty), TemplateContainer(typeof(DataListItem))]
        public virtual ITemplate FooterTemplate
        {
            get
            {
                return this.footerTemplate;
            }
            set
            {
                this.footerTemplate = value;
            }
        }

        [DefaultValue(0)]
        public override System.Web.UI.WebControls.GridLines GridLines
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
                base.GridLines = value;
            }
        }

        [WebCategory("Styles"), WebSysDescription("DataControls_HeaderStyle"), DefaultValue((string) null), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty)]
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

        [Browsable(false), TemplateContainer(typeof(DataListItem)), WebSysDescription("DataList_HeaderTemplate"), PersistenceMode(PersistenceMode.InnerProperty), DefaultValue((string) null)]
        public virtual ITemplate HeaderTemplate
        {
            get
            {
                return this.headerTemplate;
            }
            set
            {
                this.headerTemplate = value;
            }
        }

        [WebSysDescription("DataList_Items"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual DataListItemCollection Items
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
                    this.itemsCollection = new DataListItemCollection(this.itemsArray);
                }
                return this.itemsCollection;
            }
        }

        [WebSysDescription("DataList_ItemStyle"), WebCategory("Styles"), DefaultValue((string) null), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty)]
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

        [PersistenceMode(PersistenceMode.InnerProperty), DefaultValue((string) null), Browsable(false), TemplateContainer(typeof(DataListItem)), WebSysDescription("DataList_ItemTemplate")]
        public virtual ITemplate ItemTemplate
        {
            get
            {
                return this.itemTemplate;
            }
            set
            {
                this.itemTemplate = value;
            }
        }

        [WebCategory("Layout"), DefaultValue(0), WebSysDescription("DataList_RepeatColumns")]
        public virtual int RepeatColumns
        {
            get
            {
                object obj2 = this.ViewState["RepeatColumns"];
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
                this.ViewState["RepeatColumns"] = value;
            }
        }

        [WebCategory("Layout"), DefaultValue(1), WebSysDescription("Item_RepeatDirection")]
        public virtual System.Web.UI.WebControls.RepeatDirection RepeatDirection
        {
            get
            {
                object obj2 = this.ViewState["RepeatDirection"];
                if (obj2 != null)
                {
                    return (System.Web.UI.WebControls.RepeatDirection) obj2;
                }
                return System.Web.UI.WebControls.RepeatDirection.Vertical;
            }
            set
            {
                if ((value < System.Web.UI.WebControls.RepeatDirection.Horizontal) || (value > System.Web.UI.WebControls.RepeatDirection.Vertical))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.ViewState["RepeatDirection"] = value;
            }
        }

        [DefaultValue(0), WebCategory("Layout"), WebSysDescription("WebControl_RepeatLayout")]
        public virtual System.Web.UI.WebControls.RepeatLayout RepeatLayout
        {
            get
            {
                object obj2 = this.ViewState["RepeatLayout"];
                if (obj2 != null)
                {
                    return (System.Web.UI.WebControls.RepeatLayout) obj2;
                }
                return System.Web.UI.WebControls.RepeatLayout.Table;
            }
            set
            {
                if ((value == System.Web.UI.WebControls.RepeatLayout.UnorderedList) || (value == System.Web.UI.WebControls.RepeatLayout.OrderedList))
                {
                    throw new ArgumentOutOfRangeException("value", System.Web.SR.GetString("DataList_LayoutNotSupported", new object[] { value }));
                }
                EnumerationRangeValidationUtil.ValidateRepeatLayout(value);
                this.ViewState["RepeatLayout"] = value;
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
                    DataListItem item;
                    if ((selectedIndex != -1) && (this.itemsArray.Count > selectedIndex))
                    {
                        item = (DataListItem) this.itemsArray[selectedIndex];
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
                        item = (DataListItem) this.itemsArray[value];
                        if (item.ItemType != ListItemType.EditItem)
                        {
                            item.SetItemType(ListItemType.SelectedItem);
                        }
                    }
                }
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), WebSysDescription("DataList_SelectedItem")]
        public virtual DataListItem SelectedItem
        {
            get
            {
                int selectedIndex = this.SelectedIndex;
                DataListItem item = null;
                if (selectedIndex != -1)
                {
                    item = this.Items[selectedIndex];
                }
                return item;
            }
        }

        [WebCategory("Styles"), WebSysDescription("DataList_SelectedItemStyle"), DefaultValue((string) null), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty)]
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

        [WebSysDescription("DataList_SelectedItemTemplate"), Browsable(false), DefaultValue((string) null), PersistenceMode(PersistenceMode.InnerProperty), TemplateContainer(typeof(DataListItem))]
        public virtual ITemplate SelectedItemTemplate
        {
            get
            {
                return this.selectedItemTemplate;
            }
            set
            {
                this.selectedItemTemplate = value;
            }
        }

        [Browsable(false)]
        public object SelectedValue
        {
            get
            {
                if (this.DataKeyField.Length == 0)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("DataList_DataKeyFieldMustBeSpecified", new object[] { this.ID }));
                }
                DataKeyCollection dataKeys = base.DataKeys;
                int selectedIndex = this.SelectedIndex;
                if (((dataKeys != null) && (selectedIndex < dataKeys.Count)) && (selectedIndex > -1))
                {
                    return dataKeys[selectedIndex];
                }
                return null;
            }
        }

        [DefaultValue((string) null), WebSysDescription("DataList_SeparatorStyle"), WebCategory("Styles"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty)]
        public virtual TableItemStyle SeparatorStyle
        {
            get
            {
                if (this.separatorStyle == null)
                {
                    this.separatorStyle = new TableItemStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this.separatorStyle).TrackViewState();
                    }
                }
                return this.separatorStyle;
            }
        }

        [Browsable(false), TemplateContainer(typeof(DataListItem)), WebSysDescription("DataList_SeparatorTemplate"), PersistenceMode(PersistenceMode.InnerProperty), DefaultValue((string) null)]
        public virtual ITemplate SeparatorTemplate
        {
            get
            {
                return this.separatorTemplate;
            }
            set
            {
                this.separatorTemplate = value;
            }
        }

        [WebSysDescription("DataControls_ShowFooter"), WebCategory("Appearance"), DefaultValue(true)]
        public virtual bool ShowFooter
        {
            get
            {
                object obj2 = this.ViewState["ShowFooter"];
                if (obj2 != null)
                {
                    return (bool) obj2;
                }
                return true;
            }
            set
            {
                this.ViewState["ShowFooter"] = value;
            }
        }

        [DefaultValue(true), WebCategory("Appearance"), WebSysDescription("DataControls_ShowHeader")]
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

        bool IRepeatInfoUser.HasFooter
        {
            get
            {
                return (this.ShowFooter && (this.footerTemplate != null));
            }
        }

        bool IRepeatInfoUser.HasHeader
        {
            get
            {
                return (this.ShowHeader && (this.headerTemplate != null));
            }
        }

        bool IRepeatInfoUser.HasSeparators
        {
            get
            {
                return (this.separatorTemplate != null);
            }
        }

        int IRepeatInfoUser.RepeatedItemCount
        {
            get
            {
                if (this.visibleItemCount != -1)
                {
                    return this.visibleItemCount;
                }
                if (this.itemsArray == null)
                {
                    return 0;
                }
                return this.itemsArray.Count;
            }
        }

        IEnumerable IWizardSideBarListControl.Items
        {
            get
            {
                return this.Items;
            }
        }

        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                if (this.RepeatLayout != System.Web.UI.WebControls.RepeatLayout.Table)
                {
                    return HtmlTextWriterTag.Span;
                }
                return HtmlTextWriterTag.Table;
            }
        }
    }
}

