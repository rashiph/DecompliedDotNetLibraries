namespace System.Web.UI.WebControls.WebParts
{
    using System;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using System.Web.Util;

    [TypeConverter(typeof(EmptyStringExpandableObjectConverter))]
    public class WebPartVerb : IStateManager
    {
        private string _clientClickHandler;
        private string _eventArgument;
        private string _eventArgumentPrefix;
        private string _id;
        private bool _isTrackingViewState;
        private WebPartEventHandler _serverClickHandler;
        private StateBag _viewState;
        private bool _visible;

        internal WebPartVerb()
        {
            this._visible = true;
        }

        private WebPartVerb(string id)
        {
            this._visible = true;
            if (string.IsNullOrEmpty(id))
            {
                throw ExceptionUtil.ParameterNullOrEmpty("id");
            }
            this._id = id;
        }

        public WebPartVerb(string id, string clientClickHandler) : this(id)
        {
            if (string.IsNullOrEmpty(clientClickHandler))
            {
                throw new ArgumentNullException("clientClickHandler");
            }
            this._clientClickHandler = clientClickHandler;
        }

        public WebPartVerb(string id, WebPartEventHandler serverClickHandler) : this(id)
        {
            if (serverClickHandler == null)
            {
                throw new ArgumentNullException("serverClickHandler");
            }
            this._serverClickHandler = serverClickHandler;
        }

        public WebPartVerb(string id, WebPartEventHandler serverClickHandler, string clientClickHandler) : this(id)
        {
            if (serverClickHandler == null)
            {
                throw new ArgumentNullException("serverClickHandler");
            }
            if (string.IsNullOrEmpty(clientClickHandler))
            {
                throw new ArgumentNullException("clientClickHandler");
            }
            this._serverClickHandler = serverClickHandler;
            this._clientClickHandler = clientClickHandler;
        }

        internal string GetEventArgument(string webPartID)
        {
            if (string.IsNullOrEmpty(this._eventArgumentPrefix))
            {
                return string.Empty;
            }
            if (this._id == null)
            {
                return (this._eventArgumentPrefix + webPartID);
            }
            return (this._eventArgumentPrefix + this._id + ":" + webPartID);
        }

        protected virtual void LoadViewState(object savedState)
        {
            if (savedState != null)
            {
                ((IStateManager) this.ViewState).LoadViewState(savedState);
                object obj2 = this.ViewState["Visible"];
                if (obj2 != null)
                {
                    this._visible = (bool) obj2;
                }
            }
        }

        protected virtual object SaveViewState()
        {
            if (this._viewState != null)
            {
                return ((IStateManager) this._viewState).SaveViewState();
            }
            return null;
        }

        internal void SetEventArgumentPrefix(string eventArgumentPrefix)
        {
            this._eventArgumentPrefix = eventArgumentPrefix;
        }

        void IStateManager.LoadViewState(object savedState)
        {
            this.LoadViewState(savedState);
        }

        object IStateManager.SaveViewState()
        {
            return this.SaveViewState();
        }

        void IStateManager.TrackViewState()
        {
            this.TrackViewState();
        }

        protected virtual void TrackViewState()
        {
            this._isTrackingViewState = true;
            if (this._viewState != null)
            {
                ((IStateManager) this._viewState).TrackViewState();
            }
        }

        [WebSysDescription("WebPartVerb_Checked"), DefaultValue(false), NotifyParentProperty(true), Themeable(false)]
        public virtual bool Checked
        {
            get
            {
                object obj2 = this.ViewState["Checked"];
                if (obj2 == null)
                {
                    return false;
                }
                return (bool) obj2;
            }
            set
            {
                this.ViewState["Checked"] = value;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string ClientClickHandler
        {
            get
            {
                if (this._clientClickHandler != null)
                {
                    return this._clientClickHandler;
                }
                return string.Empty;
            }
        }

        [WebSysDefaultValue(""), Localizable(true), NotifyParentProperty(true), WebSysDescription("WebPartVerb_Description")]
        public virtual string Description
        {
            get
            {
                object obj2 = this.ViewState["Description"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["Description"] = value;
            }
        }

        [WebSysDescription("WebPartVerb_Enabled"), DefaultValue(true), NotifyParentProperty(true), Themeable(false)]
        public virtual bool Enabled
        {
            get
            {
                object obj2 = this.ViewState["Enabled"];
                return ((obj2 == null) || ((bool) obj2));
            }
            set
            {
                this.ViewState["Enabled"] = value;
            }
        }

        internal string EventArgument
        {
            get
            {
                if (this._eventArgument == null)
                {
                    return string.Empty;
                }
                return this._eventArgument;
            }
            set
            {
                this._eventArgument = value;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string ID
        {
            get
            {
                if (this._id == null)
                {
                    return string.Empty;
                }
                return this._id;
            }
        }

        [WebSysDescription("WebPartVerb_ImageUrl"), Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), NotifyParentProperty(true), UrlProperty, DefaultValue("")]
        public virtual string ImageUrl
        {
            get
            {
                object obj2 = this.ViewState["ImageUrl"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["ImageUrl"] = value;
            }
        }

        protected virtual bool IsTrackingViewState
        {
            get
            {
                return this._isTrackingViewState;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public WebPartEventHandler ServerClickHandler
        {
            get
            {
                return this._serverClickHandler;
            }
        }

        bool IStateManager.IsTrackingViewState
        {
            get
            {
                return this.IsTrackingViewState;
            }
        }

        [NotifyParentProperty(true), Localizable(true), WebSysDefaultValue(""), WebSysDescription("WebPartVerb_Text")]
        public virtual string Text
        {
            get
            {
                object obj2 = this.ViewState["Text"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["Text"] = value;
            }
        }

        protected StateBag ViewState
        {
            get
            {
                if (this._viewState == null)
                {
                    this._viewState = new StateBag(false);
                    if (this._isTrackingViewState)
                    {
                        ((IStateManager) this._viewState).TrackViewState();
                    }
                }
                return this._viewState;
            }
        }

        [WebSysDescription("WebPartVerb_Visible"), NotifyParentProperty(true), Themeable(false), DefaultValue(true)]
        public virtual bool Visible
        {
            get
            {
                return this._visible;
            }
            set
            {
                this._visible = value;
                this.ViewState["Visible"] = value;
            }
        }
    }
}

