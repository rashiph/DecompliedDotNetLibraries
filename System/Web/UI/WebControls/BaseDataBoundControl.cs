namespace System.Web.UI.WebControls
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Web;
    using System.Web.UI;
    using System.Web.Util;

    [Designer("System.Web.UI.Design.WebControls.BaseDataBoundControlDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), DefaultProperty("DataSourceID")]
    public abstract class BaseDataBoundControl : WebControl
    {
        private object _dataSource;
        private bool _inited;
        private bool _preRendered;
        private bool _requiresBindToNull;
        private bool _requiresDataBinding;
        private bool _throwOnDataPropertyChange;
        private static readonly object EventDataBound = new object();

        [WebCategory("Data"), WebSysDescription("BaseDataBoundControl_OnDataBound")]
        public event EventHandler DataBound
        {
            add
            {
                base.Events.AddHandler(EventDataBound, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventDataBound, value);
            }
        }

        protected BaseDataBoundControl()
        {
        }

        protected void ConfirmInitState()
        {
            this._inited = true;
        }

        public override void DataBind()
        {
            if (base.DesignMode)
            {
                IDictionary designModeState = this.GetDesignModeState();
                if (((designModeState == null) || (designModeState["EnableDesignTimeDataBinding"] == null)) && (base.Site == null))
                {
                    return;
                }
            }
            this.PerformSelect();
        }

        protected virtual void EnsureDataBound()
        {
            try
            {
                this._throwOnDataPropertyChange = true;
                if (this.RequiresDataBinding && ((this.DataSourceID.Length > 0) || this._requiresBindToNull))
                {
                    this.DataBind();
                    this._requiresBindToNull = false;
                }
            }
            finally
            {
                this._throwOnDataPropertyChange = false;
            }
        }

        protected virtual void OnDataBound(EventArgs e)
        {
            EventHandler handler = base.Events[EventDataBound] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
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

        protected virtual void OnPagePreLoad(object sender, EventArgs e)
        {
            this._inited = true;
            if (this.Page != null)
            {
                this.Page.PreLoad -= new EventHandler(this.OnPagePreLoad);
            }
        }

        protected internal override void OnPreRender(EventArgs e)
        {
            this._preRendered = true;
            this.EnsureDataBound();
            base.OnPreRender(e);
        }

        protected abstract void PerformSelect();
        protected abstract void ValidateDataSource(object dataSource);

        [Bindable(true), Themeable(false), WebSysDescription("BaseDataBoundControl_DataSource"), WebCategory("Data"), DefaultValue((string) null), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual object DataSource
        {
            get
            {
                return this._dataSource;
            }
            set
            {
                if (value != null)
                {
                    this.ValidateDataSource(value);
                }
                this._dataSource = value;
                this.OnDataPropertyChanged();
            }
        }

        [Themeable(false), WebSysDescription("BaseDataBoundControl_DataSourceID"), WebCategory("Data"), DefaultValue("")]
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
                if (string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(this.DataSourceID))
                {
                    this._requiresBindToNull = true;
                }
                this.ViewState["DataSourceID"] = value;
                this.OnDataPropertyChanged();
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
                if (((value && this._preRendered) && ((this.DataSourceID.Length > 0) && (this.Page != null))) && !this.Page.IsCallback)
                {
                    this._requiresDataBinding = true;
                    this.EnsureDataBound();
                }
                else
                {
                    this._requiresDataBinding = value;
                }
            }
        }

        public override bool SupportsDisabledAttribute
        {
            get
            {
                return (this.RenderingCompatibility < VersionUtil.Framework40);
            }
        }
    }
}

