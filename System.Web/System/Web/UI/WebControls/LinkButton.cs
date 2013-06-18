namespace System.Web.UI.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Web;
    using System.Web.UI;
    using System.Web.Util;

    [DefaultEvent("Click"), SupportsEventValidation, DataBindingHandler("System.Web.UI.Design.TextDataBindingHandler, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), Designer("System.Web.UI.Design.WebControls.LinkButtonDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), ControlBuilder(typeof(LinkButtonControlBuilder)), ParseChildren(false), DefaultProperty("Text"), ToolboxData("<{0}:LinkButton runat=\"server\">LinkButton</{0}:LinkButton>")]
    public class LinkButton : WebControl, IButtonControl, IPostBackEventHandler
    {
        private bool _textSetByAddParsedSubObject;
        private static readonly object EventClick = new object();
        private static readonly object EventCommand = new object();

        [WebSysDescription("LinkButton_OnClick"), WebCategory("Action")]
        public event EventHandler Click
        {
            add
            {
                base.Events.AddHandler(EventClick, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventClick, value);
            }
        }

        [WebCategory("Action"), WebSysDescription("Button_OnCommand")]
        public event CommandEventHandler Command
        {
            add
            {
                base.Events.AddHandler(EventCommand, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventCommand, value);
            }
        }

        public LinkButton() : base(HtmlTextWriterTag.A)
        {
        }

        protected override void AddAttributesToRender(HtmlTextWriter writer)
        {
            if (this.Page != null)
            {
                this.Page.VerifyRenderingInServerForm(this);
            }
            string str = Util.EnsureEndWithSemiColon(this.OnClientClick);
            if (base.HasAttributes)
            {
                string str2 = base.Attributes["onclick"];
                if (str2 != null)
                {
                    str = str + Util.EnsureEndWithSemiColon(str2);
                    base.Attributes.Remove("onclick");
                }
            }
            if (str.Length > 0)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Onclick, str);
            }
            bool isEnabled = base.IsEnabled;
            if ((this.Enabled && !isEnabled) && this.SupportsDisabledAttribute)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Disabled, "disabled");
            }
            base.AddAttributesToRender(writer);
            if (isEnabled && (this.Page != null))
            {
                PostBackOptions postBackOptions = this.GetPostBackOptions();
                string postBackEventReference = null;
                if (postBackOptions != null)
                {
                    postBackEventReference = this.Page.ClientScript.GetPostBackEventReference(postBackOptions, true);
                }
                if (string.IsNullOrEmpty(postBackEventReference))
                {
                    postBackEventReference = "javascript:void(0)";
                }
                writer.AddAttribute(HtmlTextWriterAttribute.Href, postBackEventReference);
            }
        }

        protected override void AddParsedSubObject(object obj)
        {
            if (this.HasControls())
            {
                base.AddParsedSubObject(obj);
            }
            else if (obj is LiteralControl)
            {
                if (this._textSetByAddParsedSubObject)
                {
                    this.Text = this.Text + ((LiteralControl) obj).Text;
                }
                else
                {
                    this.Text = ((LiteralControl) obj).Text;
                }
                this._textSetByAddParsedSubObject = true;
            }
            else
            {
                string text = this.Text;
                if (text.Length != 0)
                {
                    this.Text = string.Empty;
                    base.AddParsedSubObject(new LiteralControl(text));
                }
                base.AddParsedSubObject(obj);
            }
        }

        protected virtual PostBackOptions GetPostBackOptions()
        {
            PostBackOptions options = new PostBackOptions(this, string.Empty) {
                RequiresJavaScriptProtocol = true
            };
            if (!string.IsNullOrEmpty(this.PostBackUrl))
            {
                options.ActionUrl = HttpUtility.UrlPathEncode(base.ResolveClientUrl(this.PostBackUrl));
                if ((!base.DesignMode && (this.Page != null)) && string.Equals(this.Page.Request.Browser.Browser, "IE", StringComparison.OrdinalIgnoreCase))
                {
                    options.ActionUrl = Util.QuoteJScriptString(options.ActionUrl, true);
                }
            }
            if (this.CausesValidation && (this.Page.GetValidators(this.ValidationGroup).Count > 0))
            {
                options.PerformValidation = true;
                options.ValidationGroup = this.ValidationGroup;
            }
            return options;
        }

        protected override void LoadViewState(object savedState)
        {
            if (savedState != null)
            {
                base.LoadViewState(savedState);
                if ((((string) this.ViewState["Text"]) != null) && this.HasControls())
                {
                    this.Controls.Clear();
                }
            }
        }

        protected virtual void OnClick(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EventClick];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnCommand(CommandEventArgs e)
        {
            CommandEventHandler handler = (CommandEventHandler) base.Events[EventCommand];
            if (handler != null)
            {
                handler(this, e);
            }
            base.RaiseBubbleEvent(this, e);
        }

        protected internal override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if ((this.Page != null) && this.Enabled)
            {
                this.Page.RegisterPostBackScript();
                if ((this.CausesValidation && (this.Page.GetValidators(this.ValidationGroup).Count > 0)) || !string.IsNullOrEmpty(this.PostBackUrl))
                {
                    this.Page.RegisterWebFormsScript();
                }
            }
        }

        protected virtual void RaisePostBackEvent(string eventArgument)
        {
            base.ValidateEvent(this.UniqueID, eventArgument);
            if (this.CausesValidation)
            {
                this.Page.Validate(this.ValidationGroup);
            }
            this.OnClick(EventArgs.Empty);
            this.OnCommand(new CommandEventArgs(this.CommandName, this.CommandArgument));
        }

        protected internal override void RenderContents(HtmlTextWriter writer)
        {
            if (base.HasRenderingData())
            {
                base.RenderContents(writer);
            }
            else
            {
                writer.Write(this.Text);
            }
        }

        void IPostBackEventHandler.RaisePostBackEvent(string eventArgument)
        {
            this.RaisePostBackEvent(eventArgument);
        }

        [Themeable(false), DefaultValue(true), WebSysDescription("Button_CausesValidation"), WebCategory("Behavior")]
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

        [WebCategory("Behavior"), Themeable(false), WebSysDescription("WebControl_CommandArgument"), DefaultValue(""), Bindable(true)]
        public string CommandArgument
        {
            get
            {
                string str = (string) this.ViewState["CommandArgument"];
                if (str != null)
                {
                    return str;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["CommandArgument"] = value;
            }
        }

        [Themeable(false), WebCategory("Behavior"), WebSysDescription("WebControl_CommandName"), DefaultValue("")]
        public string CommandName
        {
            get
            {
                string str = (string) this.ViewState["CommandName"];
                if (str != null)
                {
                    return str;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["CommandName"] = value;
            }
        }

        [DefaultValue(""), WebSysDescription("Button_OnClientClick"), Themeable(false), WebCategory("Behavior")]
        public virtual string OnClientClick
        {
            get
            {
                string str = (string) this.ViewState["OnClientClick"];
                if (str == null)
                {
                    return string.Empty;
                }
                return str;
            }
            set
            {
                this.ViewState["OnClientClick"] = value;
            }
        }

        [WebCategory("Behavior"), Editor("System.Web.UI.Design.UrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), UrlProperty("*.aspx"), DefaultValue(""), WebSysDescription("Button_PostBackUrl"), Themeable(false)]
        public virtual string PostBackUrl
        {
            get
            {
                string str = (string) this.ViewState["PostBackUrl"];
                if (str != null)
                {
                    return str;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["PostBackUrl"] = value;
            }
        }

        internal override bool RequiresLegacyRendering
        {
            get
            {
                return true;
            }
        }

        public override bool SupportsDisabledAttribute
        {
            get
            {
                return (this.RenderingCompatibility < VersionUtil.Framework40);
            }
        }

        [DefaultValue(""), Bindable(true), WebSysDescription("LinkButton_Text"), WebCategory("Appearance"), PersistenceMode(PersistenceMode.InnerDefaultProperty), Localizable(true)]
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
                if (this.HasControls())
                {
                    this.Controls.Clear();
                }
                this.ViewState["Text"] = value;
            }
        }

        [WebSysDescription("PostBackControl_ValidationGroup"), Themeable(false), DefaultValue(""), WebCategory("Behavior")]
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

