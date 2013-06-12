namespace System.Web.UI.WebControls
{
    using System;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Web;
    using System.Web.UI;

    [Designer("System.Web.UI.Design.WebControls.PreviewControlDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), DefaultEvent("Click"), SupportsEventValidation]
    public class ImageButton : Image, IPostBackDataHandler, IPostBackEventHandler, IButtonControl
    {
        private static readonly object EventButtonClick = new object();
        private static readonly object EventClick = new object();
        private static readonly object EventCommand = new object();
        private int x;
        private int y;

        [WebSysDescription("ImageButton_OnClick"), WebCategory("Action")]
        public event ImageClickEventHandler Click
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

        [WebSysDescription("ImageButton_OnCommand"), WebCategory("Action")]
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

        event EventHandler IButtonControl.Click
        {
            add
            {
                base.Events.AddHandler(EventButtonClick, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventButtonClick, value);
            }
        }

        protected override void AddAttributesToRender(HtmlTextWriter writer)
        {
            Page page = this.Page;
            if (page != null)
            {
                page.VerifyRenderingInServerForm(this);
            }
            writer.AddAttribute(HtmlTextWriterAttribute.Type, "image");
            string uniqueID = this.UniqueID;
            PostBackOptions postBackOptions = this.GetPostBackOptions();
            if ((uniqueID != null) && ((postBackOptions == null) || (postBackOptions.TargetControl == this)))
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Name, uniqueID);
            }
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
            }
            if ((this.Enabled && !isEnabled) && this.SupportsDisabledAttribute)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Disabled, "disabled");
            }
            base.AddAttributesToRender(writer);
            if ((page != null) && (postBackOptions != null))
            {
                page.ClientScript.RegisterForEventValidation(postBackOptions);
                if (isEnabled)
                {
                    string postBackEventReference = page.ClientScript.GetPostBackEventReference(postBackOptions, false);
                    if (!string.IsNullOrEmpty(postBackEventReference))
                    {
                        firstScript = Util.MergeScript(firstScript, postBackEventReference);
                        if (postBackOptions.ClientSubmit)
                        {
                            firstScript = Util.EnsureEndWithSemiColon(firstScript) + "return false;";
                        }
                    }
                }
            }
            if (firstScript.Length > 0)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Onclick, firstScript);
                if (base.EnableLegacyRendering)
                {
                    writer.AddAttribute("language", "javascript", false);
                }
            }
        }

        protected virtual PostBackOptions GetPostBackOptions()
        {
            PostBackOptions options = new PostBackOptions(this, string.Empty) {
                ClientSubmit = false
            };
            if (!string.IsNullOrEmpty(this.PostBackUrl))
            {
                options.ActionUrl = HttpUtility.UrlPathEncode(base.ResolveClientUrl(this.PostBackUrl));
            }
            if ((this.CausesValidation && (this.Page != null)) && (this.Page.GetValidators(this.ValidationGroup).Count > 0))
            {
                options.PerformValidation = true;
                options.ValidationGroup = this.ValidationGroup;
            }
            return options;
        }

        protected virtual bool LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
            string uniqueID = this.UniqueID;
            string s = postCollection[uniqueID + ".x"];
            string str3 = postCollection[uniqueID + ".y"];
            if (((s != null) && (str3 != null)) && ((s.Length > 0) && (str3.Length > 0)))
            {
                this.x = int.Parse(s, CultureInfo.InvariantCulture);
                this.y = int.Parse(str3, CultureInfo.InvariantCulture);
                if (this.Page != null)
                {
                    this.Page.RegisterRequiresRaiseEvent(this);
                }
            }
            return false;
        }

        protected virtual void OnClick(ImageClickEventArgs e)
        {
            ImageClickEventHandler handler = (ImageClickEventHandler) base.Events[EventClick];
            if (handler != null)
            {
                handler(this, e);
            }
            EventHandler handler2 = (EventHandler) base.Events[EventButtonClick];
            if (handler2 != null)
            {
                handler2(this, e);
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
            if (this.Page != null)
            {
                this.Page.RegisterRequiresPostBack(this);
                if (base.IsEnabled && ((this.CausesValidation && (this.Page.GetValidators(this.ValidationGroup).Count > 0)) || !string.IsNullOrEmpty(this.PostBackUrl)))
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
            this.OnClick(new ImageClickEventArgs(this.x, this.y));
            this.OnCommand(new CommandEventArgs(this.CommandName, this.CommandArgument));
        }

        protected virtual void RaisePostDataChangedEvent()
        {
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

        [WebCategory("Behavior"), WebSysDescription("Button_CausesValidation"), Themeable(false), DefaultValue(true)]
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

        [Bindable(true), WebSysDescription("WebControl_CommandArgument"), DefaultValue(""), Themeable(false), WebCategory("Behavior")]
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

        [DefaultValue(""), Themeable(false), WebCategory("Behavior"), WebSysDescription("WebControl_CommandName")]
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

        [WebCategory("Behavior"), WebSysDescription("WebControl_Enabled"), DefaultValue(true), Browsable(true), EditorBrowsable(EditorBrowsableState.Always), Bindable(true)]
        public override bool Enabled
        {
            get
            {
                return base.Enabled;
            }
            set
            {
                base.Enabled = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Themeable(false), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override bool GenerateEmptyAlternateText
        {
            get
            {
                return base.GenerateEmptyAlternateText;
            }
            set
            {
                throw new NotSupportedException(System.Web.SR.GetString("Property_Set_Not_Supported", new object[] { "GenerateEmptyAlternateText", base.GetType().ToString() }));
            }
        }

        [WebCategory("Behavior"), Themeable(false), WebSysDescription("Button_OnClientClick"), DefaultValue("")]
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

        [WebSysDescription("Button_PostBackUrl"), Editor("System.Web.UI.Design.UrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), WebCategory("Behavior"), DefaultValue(""), Themeable(false), UrlProperty("*.aspx")]
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

        public override bool SupportsDisabledAttribute
        {
            get
            {
                return true;
            }
        }

        string IButtonControl.Text
        {
            get
            {
                return this.Text;
            }
            set
            {
                this.Text = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                return HtmlTextWriterTag.Input;
            }
        }

        protected virtual string Text
        {
            get
            {
                return this.AlternateText;
            }
            set
            {
                this.AlternateText = value;
            }
        }

        [Themeable(false), DefaultValue(""), WebCategory("Behavior"), WebSysDescription("PostBackControl_ValidationGroup")]
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

