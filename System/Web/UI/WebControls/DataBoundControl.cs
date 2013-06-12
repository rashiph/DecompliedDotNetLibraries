namespace System.Web.UI.WebControls
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls.Adapters;

    [Designer("System.Web.UI.Design.WebControls.DataBoundControlDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    public abstract class DataBoundControl : BaseDataBoundControl
    {
        private DataSourceSelectArguments _arguments;
        private IDataSource _currentDataSource;
        private bool _currentDataSourceValid;
        private DataSourceView _currentView;
        private bool _currentViewIsFromDataSourceID;
        private bool _currentViewValid;
        private bool _ignoreDataSourceViewChanged;
        private bool _pagePreLoadFired;
        private const string DataBoundViewStateKey = "_!DataBound";

        protected DataBoundControl()
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
                this._currentDataSource = this.GetDataSource();
                string dataMember = this.DataMember;
                if (this._currentDataSource == null)
                {
                    this._currentDataSource = new ReadOnlyDataSource(this.DataSource, dataMember);
                }
                else if (this.DataSource != null)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("DataControl_MultipleDataSources", new object[] { this.ID }));
                }
                this._currentDataSourceValid = true;
                DataSourceView view = this._currentDataSource.GetView(dataMember);
                if (view == null)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("DataControl_ViewNotFound", new object[] { this.ID }));
                }
                this._currentViewIsFromDataSourceID = base.IsBoundUsingDataSourceID;
                this._currentView = view;
                if ((this._currentView != null) && this._currentViewIsFromDataSourceID)
                {
                    this._currentView.DataSourceViewChanged += new EventHandler(this.OnDataSourceViewChanged);
                }
                this._currentViewValid = true;
            }
            return this._currentView;
        }

        protected virtual DataSourceSelectArguments CreateDataSourceSelectArguments()
        {
            return DataSourceSelectArguments.Empty;
        }

        protected virtual DataSourceView GetData()
        {
            return this.ConnectToDataSourceView();
        }

        protected virtual IDataSource GetDataSource()
        {
            if ((!base.DesignMode && this._currentDataSourceValid) && (this._currentDataSource != null))
            {
                return this._currentDataSource;
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
            return source;
        }

        protected void MarkAsDataBound()
        {
            this.ViewState["_!DataBound"] = true;
        }

        protected override void OnDataPropertyChanged()
        {
            this._currentViewValid = false;
            this._currentDataSourceValid = false;
            base.OnDataPropertyChanged();
        }

        protected virtual void OnDataSourceViewChanged(object sender, EventArgs e)
        {
            if (!this._ignoreDataSourceViewChanged)
            {
                base.RequiresDataBinding = true;
            }
        }

        private void OnDataSourceViewSelectCallback(IEnumerable data)
        {
            this._ignoreDataSourceViewChanged = false;
            if (this.DataSourceID.Length > 0)
            {
                this.OnDataBinding(EventArgs.Empty);
            }
            if (base.AdapterInternal != null)
            {
                DataBoundControlAdapter adapterInternal = base.AdapterInternal as DataBoundControlAdapter;
                if (adapterInternal != null)
                {
                    adapterInternal.PerformDataBinding(data);
                }
                else
                {
                    this.PerformDataBinding(data);
                }
            }
            else
            {
                this.PerformDataBinding(data);
            }
            this.OnDataBound(EventArgs.Empty);
        }

        protected internal override void OnLoad(EventArgs e)
        {
            base.ConfirmInitState();
            this.ConnectToDataSourceView();
            if (((this.Page != null) && !this._pagePreLoadFired) && (this.ViewState["_!DataBound"] == null))
            {
                if (!this.Page.IsPostBack)
                {
                    base.RequiresDataBinding = true;
                }
                else if (base.IsViewStateEnabled)
                {
                    base.RequiresDataBinding = true;
                }
            }
            base.OnLoad(e);
        }

        protected override void OnPagePreLoad(object sender, EventArgs e)
        {
            base.OnPagePreLoad(sender, e);
            if (this.Page != null)
            {
                if (!this.Page.IsPostBack)
                {
                    base.RequiresDataBinding = true;
                }
                else if (base.IsViewStateEnabled && (this.ViewState["_!DataBound"] == null))
                {
                    base.RequiresDataBinding = true;
                }
            }
            this._pagePreLoadFired = true;
        }

        protected internal virtual void PerformDataBinding(IEnumerable data)
        {
        }

        protected override void PerformSelect()
        {
            if (this.DataSourceID.Length == 0)
            {
                this.OnDataBinding(EventArgs.Empty);
            }
            DataSourceView data = this.GetData();
            this._arguments = this.CreateDataSourceSelectArguments();
            this._ignoreDataSourceViewChanged = true;
            base.RequiresDataBinding = false;
            this.MarkAsDataBound();
            data.Select(this._arguments, new DataSourceViewSelectCallback(this.OnDataSourceViewSelectCallback));
        }

        protected override void ValidateDataSource(object dataSource)
        {
            if (((dataSource != null) && !(dataSource is IListSource)) && (!(dataSource is IEnumerable) && !(dataSource is IDataSource)))
            {
                throw new InvalidOperationException(System.Web.SR.GetString("DataBoundControl_InvalidDataSourceType"));
            }
        }

        [Themeable(false), WebCategory("Data"), DefaultValue(""), WebSysDescription("DataBoundControl_DataMember")]
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

        [IDReferenceProperty(typeof(DataSourceControl))]
        public override string DataSourceID
        {
            get
            {
                return base.DataSourceID;
            }
            set
            {
                base.DataSourceID = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public IDataSource DataSourceObject
        {
            get
            {
                return this.GetDataSource();
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
    }
}

