namespace System.Web.UI
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.ComponentModel.Design.Serialization;
    using System.Web;
    using System.Web.Caching;
    using System.Web.SessionState;

    [ParseChildren(true), Designer("Microsoft.VisualStudio.Web.WebForms.WebFormDesigner, Microsoft.VisualStudio.Web, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(IRootDesigner)), ControlBuilder(typeof(UserControlControlBuilder)), ToolboxItem(false), DesignerCategory("ASPXCodeBehind"), DesignerSerializer("Microsoft.VisualStudio.Web.WebForms.WebFormCodeDomSerializer, Microsoft.VisualStudio.Web, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.ComponentModel.Design.Serialization.TypeCodeDomSerializer, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), DefaultEvent("Load"), Designer("System.Web.UI.Design.UserControlDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(IDesigner))]
    public class UserControl : TemplateControl, IAttributeAccessor, INonBindingContainer, INamingContainer, IUserControlDesignerAccessor
    {
        private bool _fUserControlInitialized;
        private System.Web.UI.AttributeCollection attributes;
        private StateBag attributeStorage;

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void DesignerInitialize()
        {
            this.InitRecursive(null);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void InitializeAsUserControl(Page page)
        {
            base._page = page;
            this.InitializeAsUserControlInternal();
        }

        internal void InitializeAsUserControlInternal()
        {
            if (!this._fUserControlInitialized)
            {
                this._fUserControlInitialized = true;
                base.HookUpAutomaticHandlers();
                this.FrameworkInitialize();
            }
        }

        protected override void LoadViewState(object savedState)
        {
            if (savedState != null)
            {
                Pair pair = (Pair) savedState;
                base.LoadViewState(pair.First);
                if (pair.Second != null)
                {
                    if (this.attributeStorage == null)
                    {
                        this.attributeStorage = new StateBag(true);
                        this.attributeStorage.TrackViewState();
                    }
                    this.attributeStorage.LoadViewState(pair.Second);
                }
            }
        }

        public string MapPath(string virtualPath)
        {
            return this.Request.MapPath(VirtualPath.CreateAllowNull(virtualPath), base.TemplateControlVirtualDirectory, true);
        }

        protected internal override void OnInit(EventArgs e)
        {
            bool designMode = base.DesignMode;
            if ((!designMode && (this.Page != null)) && (this.Page.Site != null))
            {
                designMode = this.Page.Site.DesignMode;
            }
            if (!designMode)
            {
                this.InitializeAsUserControlInternal();
            }
            base.OnInit(e);
        }

        protected override object SaveViewState()
        {
            Pair pair = null;
            object x = base.SaveViewState();
            object y = null;
            if (this.attributeStorage != null)
            {
                y = this.attributeStorage.SaveViewState();
            }
            if ((x == null) && (y == null))
            {
                return pair;
            }
            return new Pair(x, y);
        }

        string IAttributeAccessor.GetAttribute(string name)
        {
            if (this.attributeStorage == null)
            {
                return null;
            }
            return (string) this.attributeStorage[name];
        }

        void IAttributeAccessor.SetAttribute(string name, string value)
        {
            this.Attributes[name] = value;
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public HttpApplicationState Application
        {
            get
            {
                return this.Page.Application;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public System.Web.UI.AttributeCollection Attributes
        {
            get
            {
                if (this.attributes == null)
                {
                    if (this.attributeStorage == null)
                    {
                        this.attributeStorage = new StateBag(true);
                        if (base.IsTrackingViewState)
                        {
                            this.attributeStorage.TrackViewState();
                        }
                    }
                    this.attributes = new System.Web.UI.AttributeCollection(this.attributeStorage);
                }
                return this.attributes;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public System.Web.Caching.Cache Cache
        {
            get
            {
                return this.Page.Cache;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public ControlCachePolicy CachePolicy
        {
            get
            {
                BasePartialCachingControl parent = this.Parent as BasePartialCachingControl;
                if (parent != null)
                {
                    return parent.CachePolicy;
                }
                return ControlCachePolicy.GetCachePolicyStub();
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsPostBack
        {
            get
            {
                return this.Page.IsPostBack;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public HttpRequest Request
        {
            get
            {
                return this.Page.Request;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public HttpResponse Response
        {
            get
            {
                return this.Page.Response;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public HttpServerUtility Server
        {
            get
            {
                return this.Page.Server;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public HttpSessionState Session
        {
            get
            {
                return this.Page.Session;
            }
        }

        string IUserControlDesignerAccessor.InnerText
        {
            get
            {
                string str = (string) this.ViewState["!DesignTimeInnerText"];
                if (str == null)
                {
                    return string.Empty;
                }
                return str;
            }
            set
            {
                this.ViewState["!DesignTimeInnerText"] = value;
            }
        }

        string IUserControlDesignerAccessor.TagName
        {
            get
            {
                string str = (string) this.ViewState["!DesignTimeTagName"];
                if (str == null)
                {
                    return string.Empty;
                }
                return str;
            }
            set
            {
                this.ViewState["!DesignTimeTagName"] = value;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public TraceContext Trace
        {
            get
            {
                return this.Page.Trace;
            }
        }
    }
}

