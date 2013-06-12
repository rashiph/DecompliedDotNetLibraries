namespace System.Web.UI.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Web;
    using System.Web.UI;

    [DefaultProperty("Text"), ToolboxData("<{0}:Button runat=\"server\" Text=\"Button\"></{0}:Button>"), SupportsEventValidation, DataBindingHandler("System.Web.UI.Design.TextDataBindingHandler, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), DefaultEvent("Click"), Designer("System.Web.UI.Design.WebControls.ButtonDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    public class Button : WebControl, IButtonControl, IPostBackEventHandler
    {
        private static readonly object EventClick = new object();
        private static readonly object EventCommand = new object();

        [WebCategory("Action"), WebSysDescription("Button_OnClick")]
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

        [WebSysDescription("Button_OnCommand"), WebCategory("Action")]
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

        public Button() : base(HtmlTextWriterTag.Input)
        {
        }

        protected override void AddAttributesToRender(HtmlTextWriter writer)
        {
            bool useSubmitBehavior = this.UseSubmitBehavior;
            if (this.Page != null)
            {
                this.Page.VerifyRenderingInServerForm(this);
            }
            if (useSubmitBehavior)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Type, "submit");
            }
            else
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Type, "button");
            }
            PostBackOptions postBackOptions = this.GetPostBackOptions();
            string uniqueID = this.UniqueID;
            if ((uniqueID != null) && ((postBackOptions == null) || (postBackOptions.TargetControl == this)))
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Name, uniqueID);
            }
            writer.AddAttribute(HtmlTextWriterAttribute.Value, this.Text);
            bool isEnabled = base.IsEnabled;
            string firstScript = string.Empty;
            if (isEnabled)
            {
                firstScript = Util.EnsureEndWithSemiColon(this.OnClientClick);
                if (base.HasAttributes)
                {
                    string str3 = base.Attributes["onclick"];
                    if (str3 != null)
                    {
                        firstScript = firstScript + Util.EnsureEndWithSemiColon(str3);
                        base.Attributes.Remove("onclick");
                    }
                }
                if (this.Page != null)
                {
                    string postBackEventReference = this.Page.ClientScript.GetPostBackEventReference(postBackOptions, false);
                    if (postBackEventReference != null)
                    {
                        firstScript = Util.MergeScript(firstScript, postBackEventReference);
                    }
                }
            }
            if (this.Page != null)
            {
                this.Page.ClientScript.RegisterForEventValidation(postBackOptions);
            }
            if (firstScript.Length > 0)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Onclick, firstScript);
                if (base.EnableLegacyRendering)
                {
                    writer.AddAttribute("language", "javascript", false);
                }
            }
            if ((this.Enabled && !isEnabled) && this.SupportsDisabledAttribute)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Disabled, "disabled");
            }
            base.AddAttributesToRender(writer);
        }

        protected virtual PostBackOptions GetPostBackOptions()
        {
            PostBackOptions options = new PostBackOptions(this, string.Empty) {
                ClientSubmit = false
            };
            if (this.Page != null)
            {
                if (this.CausesValidation && (this.Page.GetValidators(this.ValidationGroup).Count > 0))
                {
                    options.PerformValidation = true;
                    options.ValidationGroup = this.ValidationGroup;
                }
                if (!string.IsNullOrEmpty(this.PostBackUrl))
                {
                    options.ActionUrl = HttpUtility.UrlPathEncode(base.ResolveClientUrl(this.PostBackUrl));
                }
                options.ClientSubmit = !this.UseSubmitBehavior;
            }
            return options;
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
            if ((this.Page != null) && base.IsEnabled)
            {
                if ((this.CausesValidation && (this.Page.GetValidators(this.ValidationGroup).Count > 0)) || !string.IsNullOrEmpty(this.PostBackUrl))
                {
                    this.Page.RegisterWebFormsScript();
                }
                else if (!this.UseSubmitBehavior)
                {
                    this.Page.RegisterPostBackScript();
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
        }

        void IPostBackEventHandler.RaisePostBackEvent(string eventArgument)
        {
            this.RaisePostBackEvent(eventArgument);
        }

        [DefaultValue(true), WebCategory("Behavior"), WebSysDescription("Button_CausesValidation"), Themeable(false)]
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

        [Themeable(false), DefaultValue(""), Bindable(true), WebCategory("Behavior"), WebSysDescription("WebControl_CommandArgument")]
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

        [WebCategory("Behavior"), DefaultValue(""), WebSysDescription("WebControl_CommandName"), Themeable(false)]
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

        [WebSysDescription("Button_PostBackUrl"), Editor("System.Web.UI.Design.UrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), DefaultValue(""), Themeable(false), WebCategory("Behavior"), UrlProperty("*.aspx")]
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

        [Bindable(true), DefaultValue(""), Localizable(true), WebCategory("Appearance"), WebSysDescription("Button_Text")]
        public string Text
        {
            get
            {
                string str = (string) this.ViewState["Text"];
                if (str != null)
                {
                    return str;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["Text"] = value;
            }
        }

        [WebSysDescription("Button_UseSubmitBehavior"), DefaultValue(true), Themeable(false), WebCategory("Behavior")]
        public virtual bool UseSubmitBehavior
        {
            get
            {
                object obj2 = this.ViewState["UseSubmitBehavior"];
                if (obj2 != null)
                {
                    return (bool) obj2;
                }
                return true;
            }
            set
            {
                this.ViewState["UseSubmitBehavior"] = value;
            }
        }

        [WebSysDescription("PostBackControl_ValidationGroup"), DefaultValue(""), Themeable(false), WebCategory("Behavior")]
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

