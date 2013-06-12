namespace System.Web.UI.HtmlControls
{
    using System;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Globalization;
    using System.Web;
    using System.Web.UI;

    [SupportsEventValidation, DefaultEvent("ServerClick")]
    public class HtmlInputImage : HtmlInputControl, IPostBackDataHandler, IPostBackEventHandler
    {
        private int _x;
        private int _y;
        private static readonly object EventServerClick = new object();

        [WebCategory("Action"), WebSysDescription("HtmlInputImage_OnServerClick")]
        public event ImageClickEventHandler ServerClick
        {
            add
            {
                base.Events.AddHandler(EventServerClick, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventServerClick, value);
            }
        }

        public HtmlInputImage() : base("image")
        {
        }

        protected virtual bool LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
            string s = postCollection[this.RenderedNameAttribute + ".x"];
            string str2 = postCollection[this.RenderedNameAttribute + ".y"];
            if (((s != null) && (str2 != null)) && ((s.Length > 0) && (str2.Length > 0)))
            {
                base.ValidateEvent(this.UniqueID);
                this._x = int.Parse(s, CultureInfo.InvariantCulture);
                this._y = int.Parse(str2, CultureInfo.InvariantCulture);
                this.Page.RegisterRequiresRaiseEvent(this);
            }
            return false;
        }

        protected internal override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if (this.Page != null)
            {
                if (!base.Disabled)
                {
                    this.Page.RegisterRequiresPostBack(this);
                }
                if (this.CausesValidation)
                {
                    this.Page.RegisterPostBackScript();
                }
            }
        }

        protected virtual void OnServerClick(ImageClickEventArgs e)
        {
            ImageClickEventHandler handler = (ImageClickEventHandler) base.Events[EventServerClick];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void RaisePostBackEvent(string eventArgument)
        {
            if (this.CausesValidation)
            {
                this.Page.Validate(this.ValidationGroup);
            }
            this.OnServerClick(new ImageClickEventArgs(this._x, this._y));
        }

        protected virtual void RaisePostDataChangedEvent()
        {
        }

        protected override void RenderAttributes(HtmlTextWriter writer)
        {
            base.PreProcessRelativeReferenceAttribute(writer, "src");
            if (this.Page != null)
            {
                Util.WriteOnClickAttribute(writer, this, true, false, this.CausesValidation && (this.Page.GetValidators(this.ValidationGroup).Count > 0), this.ValidationGroup);
            }
            base.RenderAttributes(writer);
        }

        bool IPostBackDataHandler.LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
            return this.LoadPostData(postDataKey, postCollection);
        }

        void IPostBackDataHandler.RaisePostDataChangedEvent()
        {
            this.RaisePostDataChangedEvent();
        }

        void IPostBackEventHandler.RaisePostBackEvent(string eventArgument)
        {
            this.RaisePostBackEvent(eventArgument);
        }

        [WebCategory("Appearance"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), DefaultValue("")]
        public string Align
        {
            get
            {
                string str = base.Attributes["align"];
                if (str == null)
                {
                    return string.Empty;
                }
                return str;
            }
            set
            {
                base.Attributes["align"] = HtmlControl.MapStringAttributeToString(value);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Localizable(true), DefaultValue(""), WebCategory("Appearance")]
        public string Alt
        {
            get
            {
                string str = base.Attributes["alt"];
                if (str == null)
                {
                    return string.Empty;
                }
                return str;
            }
            set
            {
                base.Attributes["alt"] = HtmlControl.MapStringAttributeToString(value);
            }
        }

        [WebCategory("Appearance"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), DefaultValue(-1)]
        public int Border
        {
            get
            {
                string s = base.Attributes["border"];
                if (s == null)
                {
                    return -1;
                }
                return int.Parse(s, CultureInfo.InvariantCulture);
            }
            set
            {
                base.Attributes["border"] = HtmlControl.MapIntegerAttributeToString(value);
            }
        }

        [WebCategory("Behavior"), DefaultValue(true)]
        public virtual bool CausesValidation
        {
            get
            {
                object obj2 = this.ViewState["CausesValidation"];
                if (obj2 != null)
                {
                    return (bool) obj2;
                }
                return true;
            }
            set
            {
                this.ViewState["CausesValidation"] = value;
            }
        }

        [WebCategory("Appearance"), UrlProperty, DefaultValue(""), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Src
        {
            get
            {
                string str = base.Attributes["src"];
                if (str == null)
                {
                    return string.Empty;
                }
                return str;
            }
            set
            {
                base.Attributes["src"] = HtmlControl.MapStringAttributeToString(value);
            }
        }

        [WebCategory("Behavior"), DefaultValue(""), WebSysDescription("PostBackControl_ValidationGroup")]
        public virtual string ValidationGroup
        {
            get
            {
                string str = (string) this.ViewState["ValidationGroup"];
                if (str != null)
                {
                    return str;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["ValidationGroup"] = value;
            }
        }
    }
}

