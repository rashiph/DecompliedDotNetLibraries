namespace System.Web.UI.WebControls
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Web;
    using System.Web.UI;
    using System.Web.Util;

    [DefaultProperty("DataSource"), DefaultEvent("SelectedIndexChanged"), Designer("System.Web.UI.Design.WebControls.BaseDataListDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    public abstract class BaseDataList : WebControl
    {
        private DataSourceSelectArguments _arguments;
        private DataSourceView _currentView;
        private bool _currentViewIsFromDataSourceID;
        private bool _currentViewValid;
        private bool _inited;
        private bool _pagePreLoadFired;
        private bool _requiresDataBinding;
        private bool _throwOnDataPropertyChange;
        private DataKeyCollection dataKeysCollection;
        private object dataSource;
        private static readonly object EventSelectedIndexChanged = new object();
        internal const string ItemCountViewStateKey = "_!ItemCount";

        [WebSysDescription("BaseDataList_OnSelectedIndexChanged"), WebCategory("Action")]
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

        protected BaseDataList()
        {
        }

        protected override void AddParsedSubObject(object obj)
        {
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
            if (this.ViewState["_!ItemCount"] == null)
            {
                if (this.RequiresDataBinding)
                {
                    this.EnsureDataBound();
                }
            }
            else
            {
                this.CreateControlHierarchy(false);
                base.ClearChildViewState();
            }
        }

        protected abstract void CreateControlHierarchy(bool useDataSource);
        protected virtual DataSourceSelectArguments CreateDataSourceSelectArguments()
        {
            return DataSourceSelectArguments.Empty;
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
            this.ConnectToDataSourceView();
            if (this._currentView != null)
            {
                return this._currentView.ExecuteSelect(this.SelectArguments);
            }
            return null;
        }

        public static bool IsBindableType(Type type)
        {
            if ((!type.IsPrimitive && !(type == typeof(string))) && !(type == typeof(DateTime)))
            {
                return (type == typeof(decimal));
            }
            return true;
        }

        protected override void OnDataBinding(EventArgs e)
        {
            base.OnDataBinding(e);
            this.Controls.Clear();
            base.ClearChildViewState();
            this.dataKeysCollection = null;
            this.CreateControlHierarchy(true);
            base.ChildControlsCreated = true;
            this.TrackViewState();
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
            }
            this._pagePreLoadFired = true;
        }

        protected internal override void OnPreRender(EventArgs e)
        {
            this.EnsureDataBound();
            base.OnPreRender(e);
        }

        protected virtual void OnSelectedIndexChanged(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EventSelectedIndexChanged];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected internal abstract void PrepareControlHierarchy();
        protected internal override void Render(HtmlTextWriter writer)
        {
            this.PrepareControlHierarchy();
            this.RenderContents(writer);
        }

        [DefaultValue(""), WebCategory("Accessibility"), WebSysDescription("DataControls_Caption"), Localizable(true)]
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

        [WebSysDescription("BaseDataList_CellPadding"), WebCategory("Layout"), DefaultValue(-1)]
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

        [DefaultValue(0), WebCategory("Layout"), WebSysDescription("BaseDataList_CellSpacing")]
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

        public override ControlCollection Controls
        {
            get
            {
                this.EnsureChildControls();
                return base.Controls;
            }
        }

        [WebCategory("Data"), Themeable(false), DefaultValue(""), WebSysDescription("BaseDataList_DataKeyField")]
        public virtual string DataKeyField
        {
            get
            {
                object obj2 = this.ViewState["DataKeyField"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["DataKeyField"] = value;
            }
        }

        [WebSysDescription("BaseDataList_DataKeys"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public DataKeyCollection DataKeys
        {
            get
            {
                if (this.dataKeysCollection == null)
                {
                    this.dataKeysCollection = new DataKeyCollection(this.DataKeysArray);
                }
                return this.dataKeysCollection;
            }
        }

        protected ArrayList DataKeysArray
        {
            get
            {
                object obj2 = this.ViewState["DataKeys"];
                if (obj2 == null)
                {
                    obj2 = new ArrayList();
                    this.ViewState["DataKeys"] = obj2;
                }
                return (ArrayList) obj2;
            }
        }

        [Themeable(false), WebCategory("Data"), DefaultValue(""), WebSysDescription("BaseDataList_DataMember")]
        public string DataMember
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

        [DefaultValue((string) null), Bindable(true), WebCategory("Data"), WebSysDescription("BaseDataBoundControl_DataSource"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Themeable(false)]
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

        [IDReferenceProperty(typeof(DataSourceControl)), WebCategory("Data"), WebSysDescription("BaseDataBoundControl_DataSourceID"), DefaultValue(""), Themeable(false)]
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

        [DefaultValue(3), WebCategory("Appearance"), WebSysDescription("DataControls_GridLines")]
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

        public override bool SupportsDisabledAttribute
        {
            get
            {
                return (this.RenderingCompatibility < VersionUtil.Framework40);
            }
        }

        [DefaultValue(false), WebCategory("Accessibility"), WebSysDescription("Table_UseAccessibleHeader")]
        public virtual bool UseAccessibleHeader
        {
            get
            {
                object obj2 = this.ViewState["UseAccessibleHeader"];
                if (obj2 == null)
                {
                    return false;
                }
                return (bool) obj2;
            }
            set
            {
                this.ViewState["UseAccessibleHeader"] = value;
            }
        }
    }
}

