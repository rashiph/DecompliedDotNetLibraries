namespace System.Web.UI.WebControls
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Web;
    using System.Web.UI;

    [Designer("System.Web.UI.Design.WebControls.RepeaterDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), ParseChildren(true), DefaultEvent("ItemCommand"), PersistChildren(false), DefaultProperty("DataSource")]
    public class Repeater : Control, INamingContainer
    {
        private DataSourceSelectArguments _arguments;
        private DataSourceView _currentView;
        private bool _currentViewIsFromDataSourceID;
        private bool _currentViewValid;
        private bool _inited;
        private bool _pagePreLoadFired;
        private bool _requiresDataBinding;
        private bool _throwOnDataPropertyChange;
        private ITemplate alternatingItemTemplate;
        private object dataSource;
        private static readonly object EventItemCommand = new object();
        private static readonly object EventItemCreated = new object();
        private static readonly object EventItemDataBound = new object();
        private ITemplate footerTemplate;
        private ITemplate headerTemplate;
        internal const string ItemCountViewStateKey = "_!ItemCount";
        private ArrayList itemsArray;
        private RepeaterItemCollection itemsCollection;
        private ITemplate itemTemplate;
        private ITemplate separatorTemplate;

        [WebCategory("Action"), WebSysDescription("Repeater_OnItemCommand")]
        public event RepeaterCommandEventHandler ItemCommand
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
        public event RepeaterItemEventHandler ItemCreated
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
        public event RepeaterItemEventHandler ItemDataBound
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

        private DataSourceView ConnectToDataSourceView()
        {
            if (!this._currentViewValid || base.DesignMode)
            {
                if ((this._currentView != null) && this._currentViewIsFromDataSourceID)
                {
                    this._currentView.DataSourceViewChanged -= new EventHandler(this.OnDataSourceViewChanged);
                }
                IDataSource source = null;
                string dataSourceID = this.DataSourceID;
                if (dataSourceID.Length != 0)
                {
                    Control control = DataBoundControlHelper.FindControl(this, dataSourceID);
                    if (control == null)
                    {
                        throw new HttpException(System.Web.SR.GetString("DataControl_DataSourceDoesntExist", new object[] { this.ID, dataSourceID }));
                    }
                    source = control as IDataSource;
                    if (source == null)
                    {
                        throw new HttpException(System.Web.SR.GetString("DataControl_DataSourceIDMustBeDataControl", new object[] { this.ID, dataSourceID }));
                    }
                }
                if (source == null)
                {
                    source = new ReadOnlyDataSource(this.DataSource, this.DataMember);
                }
                else if (this.DataSource != null)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("DataControl_MultipleDataSources", new object[] { this.ID }));
                }
                DataSourceView view = source.GetView(this.DataMember);
                if (view == null)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("DataControl_ViewNotFound", new object[] { this.ID }));
                }
                this._currentViewIsFromDataSourceID = this.IsBoundUsingDataSourceID;
                this._currentView = view;
                if ((this._currentView != null) && this._currentViewIsFromDataSourceID)
                {
                    this._currentView.DataSourceViewChanged += new EventHandler(this.OnDataSourceViewChanged);
                }
                this._currentViewValid = true;
            }
            return this._currentView;
        }

        protected internal override void CreateChildControls()
        {
            this.Controls.Clear();
            if (this.ViewState["_!ItemCount"] != null)
            {
                this.CreateControlHierarchy(false);
            }
            else
            {
                this.itemsArray = new ArrayList();
            }
            base.ClearChildViewState();
        }

        protected virtual void CreateControlHierarchy(bool useDataSource)
        {
            IEnumerable data = null;
            int dataItemCount = -1;
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
                data = this.GetData();
                ICollection is2 = data as ICollection;
                if (is2 != null)
                {
                    this.itemsArray.Capacity = is2.Count;
                }
            }
            if (data != null)
            {
                int itemIndex = 0;
                bool flag = this.separatorTemplate != null;
                dataItemCount = 0;
                if (this.headerTemplate != null)
                {
                    this.CreateItem(-1, ListItemType.Header, useDataSource, null);
                }
                foreach (object obj2 in data)
                {
                    if (flag && (dataItemCount > 0))
                    {
                        this.CreateItem(itemIndex - 1, ListItemType.Separator, useDataSource, null);
                    }
                    ListItemType itemType = ((itemIndex % 2) == 0) ? ListItemType.Item : ListItemType.AlternatingItem;
                    RepeaterItem item = this.CreateItem(itemIndex, itemType, useDataSource, obj2);
                    this.itemsArray.Add(item);
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

        protected virtual DataSourceSelectArguments CreateDataSourceSelectArguments()
        {
            return DataSourceSelectArguments.Empty;
        }

        protected virtual RepeaterItem CreateItem(int itemIndex, ListItemType itemType)
        {
            return new RepeaterItem(itemIndex, itemType);
        }

        private RepeaterItem CreateItem(int itemIndex, ListItemType itemType, bool dataBind, object dataItem)
        {
            RepeaterItem item = this.CreateItem(itemIndex, itemType);
            RepeaterItemEventArgs e = new RepeaterItemEventArgs(item);
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

        public override void DataBind()
        {
            if ((!this.IsBoundUsingDataSourceID || !base.DesignMode) || (base.Site != null))
            {
                this.RequiresDataBinding = false;
                this.OnDataBinding(EventArgs.Empty);
            }
        }

        protected void EnsureDataBound()
        {
            try
            {
                this._throwOnDataPropertyChange = true;
                if (this.RequiresDataBinding && (this.DataSourceID.Length > 0))
                {
                    this.DataBind();
                }
            }
            finally
            {
                this._throwOnDataPropertyChange = false;
            }
        }

        protected virtual IEnumerable GetData()
        {
            DataSourceView view = this.ConnectToDataSourceView();
            if (view != null)
            {
                return view.ExecuteSelect(this.SelectArguments);
            }
            return null;
        }

        protected virtual void InitializeItem(RepeaterItem item)
        {
            ITemplate itemTemplate = null;
            switch (item.ItemType)
            {
                case ListItemType.Header:
                    itemTemplate = this.headerTemplate;
                    goto Label_005B;

                case ListItemType.Footer:
                    itemTemplate = this.footerTemplate;
                    goto Label_005B;

                case ListItemType.Item:
                    break;

                case ListItemType.AlternatingItem:
                    itemTemplate = this.alternatingItemTemplate;
                    if (itemTemplate != null)
                    {
                        goto Label_005B;
                    }
                    break;

                case ListItemType.Separator:
                    itemTemplate = this.separatorTemplate;
                    goto Label_005B;

                default:
                    goto Label_005B;
            }
            itemTemplate = this.itemTemplate;
        Label_005B:
            if (itemTemplate != null)
            {
                itemTemplate.InstantiateIn(item);
            }
        }

        protected override bool OnBubbleEvent(object sender, EventArgs e)
        {
            bool flag = false;
            if (e is RepeaterCommandEventArgs)
            {
                this.OnItemCommand((RepeaterCommandEventArgs) e);
                flag = true;
            }
            return flag;
        }

        protected override void OnDataBinding(EventArgs e)
        {
            base.OnDataBinding(e);
            this.Controls.Clear();
            base.ClearChildViewState();
            this.CreateControlHierarchy(true);
            base.ChildControlsCreated = true;
        }

        protected virtual void OnDataPropertyChanged()
        {
            if (this._throwOnDataPropertyChange)
            {
                throw new HttpException(System.Web.SR.GetString("DataBoundControl_InvalidDataPropertyChange", new object[] { this.ID }));
            }
            if (this._inited)
            {
                this.RequiresDataBinding = true;
            }
            this._currentViewValid = false;
        }

        protected virtual void OnDataSourceViewChanged(object sender, EventArgs e)
        {
            this.RequiresDataBinding = true;
        }

        protected internal override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            if (this.Page != null)
            {
                this.Page.PreLoad += new EventHandler(this.OnPagePreLoad);
                if (!base.IsViewStateEnabled && this.Page.IsPostBack)
                {
                    this.RequiresDataBinding = true;
                }
            }
        }

        protected virtual void OnItemCommand(RepeaterCommandEventArgs e)
        {
            RepeaterCommandEventHandler handler = (RepeaterCommandEventHandler) base.Events[EventItemCommand];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnItemCreated(RepeaterItemEventArgs e)
        {
            RepeaterItemEventHandler handler = (RepeaterItemEventHandler) base.Events[EventItemCreated];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnItemDataBound(RepeaterItemEventArgs e)
        {
            RepeaterItemEventHandler handler = (RepeaterItemEventHandler) base.Events[EventItemDataBound];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected internal override void OnLoad(EventArgs e)
        {
            this._inited = true;
            this.ConnectToDataSourceView();
            if (((this.Page != null) && !this._pagePreLoadFired) && (this.ViewState["_!ItemCount"] == null))
            {
                if (!this.Page.IsPostBack)
                {
                    this.RequiresDataBinding = true;
                }
                else if (base.IsViewStateEnabled)
                {
                    this.RequiresDataBinding = true;
                }
            }
            base.OnLoad(e);
        }

        private void OnPagePreLoad(object sender, EventArgs e)
        {
            this._inited = true;
            if (this.Page != null)
            {
                this.Page.PreLoad -= new EventHandler(this.OnPagePreLoad);
                if (!this.Page.IsPostBack)
                {
                    this.RequiresDataBinding = true;
                }
                if ((this.Page.IsPostBack && base.IsViewStateEnabled) && (this.ViewState["_!ItemCount"] == null))
                {
                    this.RequiresDataBinding = true;
                }
                this._pagePreLoadFired = true;
            }
        }

        protected internal override void OnPreRender(EventArgs e)
        {
            this.EnsureDataBound();
            base.OnPreRender(e);
        }

        [WebSysDescription("Repeater_AlternatingItemTemplate"), Browsable(false), DefaultValue((string) null), PersistenceMode(PersistenceMode.InnerProperty), TemplateContainer(typeof(RepeaterItem))]
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

        public override ControlCollection Controls
        {
            get
            {
                this.EnsureChildControls();
                return base.Controls;
            }
        }

        [DefaultValue(""), WebSysDescription("Repeater_DataMember"), WebCategory("Data")]
        public virtual string DataMember
        {
            get
            {
                object obj2 = this.ViewState["DataMember"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["DataMember"] = value;
                this.OnDataPropertyChanged();
            }
        }

        [DefaultValue((string) null), Bindable(true), WebCategory("Data"), WebSysDescription("BaseDataBoundControl_DataSource"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual object DataSource
        {
            get
            {
                return this.dataSource;
            }
            set
            {
                if (((value != null) && !(value is IListSource)) && !(value is IEnumerable))
                {
                    throw new ArgumentException(System.Web.SR.GetString("Invalid_DataSource_Type", new object[] { this.ID }));
                }
                this.dataSource = value;
                this.OnDataPropertyChanged();
            }
        }

        [DefaultValue(""), WebSysDescription("BaseDataBoundControl_DataSourceID"), IDReferenceProperty(typeof(DataSourceControl)), WebCategory("Data")]
        public virtual string DataSourceID
        {
            get
            {
                object obj2 = this.ViewState["DataSourceID"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["DataSourceID"] = value;
                this.OnDataPropertyChanged();
            }
        }

        [Browsable(true)]
        public override bool EnableTheming
        {
            get
            {
                return base.EnableTheming;
            }
            set
            {
                base.EnableTheming = value;
            }
        }

        [WebSysDescription("Repeater_FooterTemplate"), Browsable(false), DefaultValue((string) null), PersistenceMode(PersistenceMode.InnerProperty), TemplateContainer(typeof(RepeaterItem))]
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

        [PersistenceMode(PersistenceMode.InnerProperty), Browsable(false), DefaultValue((string) null), TemplateContainer(typeof(RepeaterItem)), WebSysDescription("WebControl_HeaderTemplate")]
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

        protected bool Initialized
        {
            get
            {
                return this._inited;
            }
        }

        protected bool IsBoundUsingDataSourceID
        {
            get
            {
                return (this.DataSourceID.Length > 0);
            }
        }

        [Browsable(false), WebSysDescription("Repeater_Items"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual RepeaterItemCollection Items
        {
            get
            {
                if (this.itemsCollection == null)
                {
                    if (this.itemsArray == null)
                    {
                        this.EnsureChildControls();
                    }
                    this.itemsCollection = new RepeaterItemCollection(this.itemsArray);
                }
                return this.itemsCollection;
            }
        }

        [WebSysDescription("Repeater_ItemTemplate"), Browsable(false), DefaultValue((string) null), PersistenceMode(PersistenceMode.InnerProperty), TemplateContainer(typeof(RepeaterItem))]
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

        protected bool RequiresDataBinding
        {
            get
            {
                return this._requiresDataBinding;
            }
            set
            {
                this._requiresDataBinding = value;
            }
        }

        protected DataSourceSelectArguments SelectArguments
        {
            get
            {
                if (this._arguments == null)
                {
                    this._arguments = this.CreateDataSourceSelectArguments();
                }
                return this._arguments;
            }
        }

        [PersistenceMode(PersistenceMode.InnerProperty), TemplateContainer(typeof(RepeaterItem)), Browsable(false), DefaultValue((string) null), WebSysDescription("Repeater_SeparatorTemplate")]
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
    }
}

