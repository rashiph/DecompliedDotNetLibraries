namespace System.Web.UI.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls.Adapters;

    [Designer("System.Web.UI.Design.WebControls.HierarchicalDataBoundControlDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    public abstract class HierarchicalDataBoundControl : BaseDataBoundControl
    {
        private bool _currentDataSourceIsFromControl;
        private bool _currentDataSourceValid;
        private IHierarchicalDataSource _currentHierarchicalDataSource;
        private bool _pagePreLoadFired;
        private const string DataBoundViewStateKey = "_!DataBound";

        protected HierarchicalDataBoundControl()
        {
        }

        private IHierarchicalDataSource ConnectToHierarchicalDataSource()
        {
            if (this._currentDataSourceValid && !base.DesignMode)
            {
                if ((!this._currentDataSourceIsFromControl && (this.DataSourceID != null)) && (this.DataSourceID.Length != 0))
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("DataControl_MultipleDataSources", new object[] { this.ID }));
                }
                return this._currentHierarchicalDataSource;
            }
            if ((this._currentHierarchicalDataSource != null) && this._currentDataSourceIsFromControl)
            {
                this._currentHierarchicalDataSource.DataSourceChanged -= new EventHandler(this.OnDataSourceChanged);
            }
            this._currentHierarchicalDataSource = this.GetDataSource();
            this._currentDataSourceIsFromControl = base.IsBoundUsingDataSourceID;
            if (this._currentHierarchicalDataSource == null)
            {
                this._currentHierarchicalDataSource = new ReadOnlyHierarchicalDataSource(this.DataSource);
            }
            else if (this.DataSource != null)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("DataControl_MultipleDataSources", new object[] { this.ID }));
            }
            this._currentDataSourceValid = true;
            if ((this._currentHierarchicalDataSource != null) && this._currentDataSourceIsFromControl)
            {
                this._currentHierarchicalDataSource.DataSourceChanged += new EventHandler(this.OnDataSourceChanged);
            }
            return this._currentHierarchicalDataSource;
        }

        protected virtual HierarchicalDataSourceView GetData(string viewPath)
        {
            string str = viewPath;
            HierarchicalDataSourceView hierarchicalView = this.ConnectToHierarchicalDataSource().GetHierarchicalView(str);
            if (hierarchicalView == null)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("HierarchicalDataControl_ViewNotFound", new object[] { this.ID }));
            }
            return hierarchicalView;
        }

        protected virtual IHierarchicalDataSource GetDataSource()
        {
            if ((!base.DesignMode && this._currentDataSourceValid) && (this._currentHierarchicalDataSource != null))
            {
                return this._currentHierarchicalDataSource;
            }
            IHierarchicalDataSource source = null;
            string dataSourceID = this.DataSourceID;
            if (dataSourceID.Length != 0)
            {
                Control control = DataBoundControlHelper.FindControl(this, dataSourceID);
                if (control == null)
                {
                    throw new HttpException(System.Web.SR.GetString("HierarchicalDataControl_DataSourceDoesntExist", new object[] { this.ID, dataSourceID }));
                }
                source = control as IHierarchicalDataSource;
                if (source == null)
                {
                    throw new HttpException(System.Web.SR.GetString("HierarchicalDataControl_DataSourceIDMustBeHierarchicalDataControl", new object[] { this.ID, dataSourceID }));
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
            this._currentDataSourceValid = false;
            base.OnDataPropertyChanged();
        }

        protected virtual void OnDataSourceChanged(object sender, EventArgs e)
        {
            base.RequiresDataBinding = true;
        }

        protected internal override void OnLoad(EventArgs e)
        {
            base.ConfirmInitState();
            this.ConnectToHierarchicalDataSource();
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

        protected internal virtual void PerformDataBinding()
        {
        }

        protected override void PerformSelect()
        {
            this.OnDataBinding(EventArgs.Empty);
            if (base.AdapterInternal != null)
            {
                HierarchicalDataBoundControlAdapter adapterInternal = base.AdapterInternal as HierarchicalDataBoundControlAdapter;
                if (adapterInternal != null)
                {
                    adapterInternal.PerformDataBinding();
                }
                else
                {
                    this.PerformDataBinding();
                }
            }
            else
            {
                this.PerformDataBinding();
            }
            base.RequiresDataBinding = false;
            this.MarkAsDataBound();
            this.OnDataBound(EventArgs.Empty);
        }

        protected override void ValidateDataSource(object dataSource)
        {
            if (((dataSource != null) && !(dataSource is IHierarchicalEnumerable)) && !(dataSource is IHierarchicalDataSource))
            {
                throw new InvalidOperationException(System.Web.SR.GetString("HierarchicalDataBoundControl_InvalidDataSource"));
            }
        }

        [IDReferenceProperty(typeof(HierarchicalDataSourceControl))]
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
    }
}

