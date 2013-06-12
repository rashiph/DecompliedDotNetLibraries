namespace System.Web.UI.WebControls
{
    using System;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Web;
    using System.Web.UI;

    [DataBindingHandler("System.Web.UI.Design.TextDataBindingHandler, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), ControlBuilder(typeof(TextBoxControlBuilder)), DefaultProperty("Text"), ValidationProperty("Text"), DefaultEvent("TextChanged"), Designer("System.Web.UI.Design.WebControls.PreviewControlDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), ParseChildren(true, "Text"), SupportsEventValidation, ControlValueProperty("Text")]
    public class TextBox : WebControl, IPostBackDataHandler, IEditableTextControl, ITextControl
    {
        private const string _textBoxKeyHandlerCall = "if (WebForm_TextBoxKeyHandler(event) == false) return false;";
        private const int DefaultMutliLineColumns = 20;
        private const int DefaultMutliLineRows = 2;
        private static readonly object EventTextChanged = new object();

        [WebCategory("Action"), WebSysDescription("TextBox_OnTextChanged")]
        public event EventHandler TextChanged
        {
            add
            {
                base.Events.AddHandler(EventTextChanged, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventTextChanged, value);
            }
        }

        public TextBox() : base(HtmlTextWriterTag.Input)
        {
        }

        protected override void AddAttributesToRender(HtmlTextWriter writer)
        {
            Page page = this.Page;
            if (page != null)
            {
                page.VerifyRenderingInServerForm(this);
            }
            string uniqueID = this.UniqueID;
            if (uniqueID != null)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Name, uniqueID);
            }
            TextBoxMode textMode = this.TextMode;
            switch (textMode)
            {
                case TextBoxMode.MultiLine:
                {
                    int rows = this.Rows;
                    int columns = this.Columns;
                    bool flag = false;
                    if (!base.EnableLegacyRendering)
                    {
                        if (rows == 0)
                        {
                            rows = 2;
                        }
                        if (columns == 0)
                        {
                            columns = 20;
                        }
                    }
                    if ((rows > 0) || flag)
                    {
                        writer.AddAttribute(HtmlTextWriterAttribute.Rows, rows.ToString(NumberFormatInfo.InvariantInfo));
                    }
                    if ((columns > 0) || flag)
                    {
                        writer.AddAttribute(HtmlTextWriterAttribute.Cols, columns.ToString(NumberFormatInfo.InvariantInfo));
                    }
                    if (!this.Wrap)
                    {
                        writer.AddAttribute(HtmlTextWriterAttribute.Wrap, "off");
                    }
                    goto Label_0265;
                }
                case TextBoxMode.SingleLine:
                {
                    if (string.IsNullOrEmpty(base.Attributes["type"]))
                    {
                        writer.AddAttribute(HtmlTextWriterAttribute.Type, "text");
                    }
                    if (((this.AutoCompleteType != System.Web.UI.WebControls.AutoCompleteType.None) && (this.Context != null)) && (this.Context.Request.Browser["supportsVCard"] == "true"))
                    {
                        if (this.AutoCompleteType == System.Web.UI.WebControls.AutoCompleteType.Disabled)
                        {
                            writer.AddAttribute(HtmlTextWriterAttribute.AutoComplete, "off");
                        }
                        else if (this.AutoCompleteType == System.Web.UI.WebControls.AutoCompleteType.Search)
                        {
                            writer.AddAttribute(HtmlTextWriterAttribute.VCardName, "search");
                        }
                        else if (this.AutoCompleteType == System.Web.UI.WebControls.AutoCompleteType.HomeCountryRegion)
                        {
                            writer.AddAttribute(HtmlTextWriterAttribute.VCardName, "HomeCountry");
                        }
                        else if (this.AutoCompleteType == System.Web.UI.WebControls.AutoCompleteType.BusinessCountryRegion)
                        {
                            writer.AddAttribute(HtmlTextWriterAttribute.VCardName, "BusinessCountry");
                        }
                        else
                        {
                            string str2 = Enum.Format(typeof(System.Web.UI.WebControls.AutoCompleteType), this.AutoCompleteType, "G");
                            if (str2.StartsWith("Business", StringComparison.Ordinal))
                            {
                                str2 = str2.Insert(8, ".");
                            }
                            else if (str2.StartsWith("Home", StringComparison.Ordinal))
                            {
                                str2 = str2.Insert(4, ".");
                            }
                            writer.AddAttribute(HtmlTextWriterAttribute.VCardName, "vCard." + str2);
                        }
                    }
                    string text = this.Text;
                    if (text.Length > 0)
                    {
                        writer.AddAttribute(HtmlTextWriterAttribute.Value, text);
                    }
                    break;
                }
                case TextBoxMode.Password:
                    writer.AddAttribute(HtmlTextWriterAttribute.Type, "password");
                    break;
            }
            int maxLength = this.MaxLength;
            if (maxLength > 0)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Maxlength, maxLength.ToString(NumberFormatInfo.InvariantInfo));
            }
            maxLength = this.Columns;
            if (maxLength > 0)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Size, maxLength.ToString(NumberFormatInfo.InvariantInfo));
            }
        Label_0265:
            if (this.ReadOnly)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.ReadOnly, "readonly");
            }
            if ((this.AutoPostBack && (page != null)) && page.ClientSupportsJavaScript)
            {
                string str4 = null;
                if (base.HasAttributes)
                {
                    str4 = base.Attributes["onchange"];
                    if (str4 != null)
                    {
                        str4 = Util.EnsureEndWithSemiColon(str4);
                        base.Attributes.Remove("onchange");
                    }
                }
                PostBackOptions options = new PostBackOptions(this, string.Empty);
                if (this.CausesValidation)
                {
                    options.PerformValidation = true;
                    options.ValidationGroup = this.ValidationGroup;
                }
                if (page.Form != null)
                {
                    options.AutoPostBack = true;
                }
                str4 = Util.MergeScript(str4, page.ClientScript.GetPostBackEventReference(options, true));
                writer.AddAttribute(HtmlTextWriterAttribute.Onchange, str4);
                if (textMode != TextBoxMode.MultiLine)
                {
                    string str5 = "if (WebForm_TextBoxKeyHandler(event) == false) return false;";
                    if (base.HasAttributes)
                    {
                        string str6 = base.Attributes["onkeypress"];
                        if (str6 != null)
                        {
                            str5 = str5 + str6;
                            base.Attributes.Remove("onkeypress");
                        }
                    }
                    writer.AddAttribute("onkeypress", str5);
                }
                if (base.EnableLegacyRendering)
                {
                    writer.AddAttribute("language", "javascript", false);
                }
            }
            else if (page != null)
            {
                page.ClientScript.RegisterForEventValidation(this.UniqueID, string.Empty);
            }
            if ((this.Enabled && !base.IsEnabled) && this.SupportsDisabledAttribute)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Disabled, "disabled");
            }
            base.AddAttributesToRender(writer);
        }

        protected override void AddParsedSubObject(object obj)
        {
            if (!(obj is LiteralControl))
            {
                throw new HttpException(System.Web.SR.GetString("Cannot_Have_Children_Of_Type", new object[] { "TextBox", obj.GetType().Name.ToString(CultureInfo.InvariantCulture) }));
            }
            this.Text = ((LiteralControl) obj).Text;
        }

        protected virtual bool LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
            base.ValidateEvent(postDataKey);
            string text = this.Text;
            string str2 = postCollection[postDataKey];
            if (!this.ReadOnly && !text.Equals(str2, StringComparison.Ordinal))
            {
                this.Text = str2;
                return true;
            }
            return false;
        }

        protected internal override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            Page page = this.Page;
            if ((page != null) && base.IsEnabled)
            {
                if (!this.SaveTextViewState)
                {
                    page.RegisterEnabledControl(this);
                }
                if (this.AutoPostBack)
                {
                    page.RegisterWebFormsScript();
                    page.RegisterPostBackScript();
                    page.RegisterFocusScript();
                }
            }
        }

        protected virtual void OnTextChanged(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EventTextChanged];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void RaisePostDataChangedEvent()
        {
            if (this.AutoPostBack && !this.Page.IsPostBackEventControlRegistered)
            {
                this.Page.AutoPostBackControl = this;
                if (this.CausesValidation)
                {
                    this.Page.Validate(this.ValidationGroup);
                }
            }
            this.OnTextChanged(EventArgs.Empty);
        }

        protected internal override void Render(HtmlTextWriter writer)
        {
            this.RenderBeginTag(writer);
            if (this.TextMode == TextBoxMode.MultiLine)
            {
                HttpUtility.HtmlEncode(Environment.NewLine + this.Text, writer);
            }
            this.RenderEndTag(writer);
        }

        protected override object SaveViewState()
        {
            if (!this.SaveTextViewState)
            {
                this.ViewState.SetItemDirty("Text", false);
            }
            return base.SaveViewState();
        }

        bool IPostBackDataHandler.LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
            return this.LoadPostData(postDataKey, postCollection);
        }

        void IPostBackDataHandler.RaisePostDataChangedEvent()
        {
            this.RaisePostDataChangedEvent();
        }

        [DefaultValue(0), Themeable(false), WebCategory("Behavior"), WebSysDescription("TextBox_AutoCompleteType")]
        public virtual System.Web.UI.WebControls.AutoCompleteType AutoCompleteType
        {
            get
            {
                object obj2 = this.ViewState["AutoCompleteType"];
                if (obj2 != null)
                {
                    return (System.Web.UI.WebControls.AutoCompleteType) obj2;
                }
                return System.Web.UI.WebControls.AutoCompleteType.None;
            }
            set
            {
                if ((value < System.Web.UI.WebControls.AutoCompleteType.None) || (value > System.Web.UI.WebControls.AutoCompleteType.Search))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.ViewState["AutoCompleteType"] = value;
            }
        }

        [Themeable(false), WebCategory("Behavior"), WebSysDescription("TextBox_AutoPostBack"), DefaultValue(false)]
        public virtual bool AutoPostBack
        {
            get
            {
                object obj2 = this.ViewState["AutoPostBack"];
                return ((obj2 != null) && ((bool) obj2));
            }
            set
            {
                this.ViewState["AutoPostBack"] = value;
            }
        }

        [WebSysDescription("AutoPostBackControl_CausesValidation"), DefaultValue(false), WebCategory("Behavior"), Themeable(false)]
        public virtual bool CausesValidation
        {
            get
            {
                object obj2 = this.ViewState["CausesValidation"];
                return ((obj2 != null) && ((bool) obj2));
            }
            set
            {
                this.ViewState["CausesValidation"] = value;
            }
        }

        [WebCategory("Appearance"), WebSysDescription("TextBox_Columns"), DefaultValue(0)]
        public virtual int Columns
        {
            get
            {
                object obj2 = this.ViewState["Columns"];
                if (obj2 != null)
                {
                    return (int) obj2;
                }
                return 0;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("Columns", System.Web.SR.GetString("TextBox_InvalidColumns"));
                }
                this.ViewState["Columns"] = value;
            }
        }

        [WebCategory("Behavior"), WebSysDescription("TextBox_MaxLength"), Themeable(false), DefaultValue(0)]
        public virtual int MaxLength
        {
            get
            {
                object obj2 = this.ViewState["MaxLength"];
                if (obj2 != null)
                {
                    return (int) obj2;
                }
                return 0;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.ViewState["MaxLength"] = value;
            }
        }

        [DefaultValue(false), WebSysDescription("TextBox_ReadOnly"), Bindable(true), Themeable(false), WebCategory("Behavior")]
        public virtual bool ReadOnly
        {
            get
            {
                object obj2 = this.ViewState["ReadOnly"];
                return ((obj2 != null) && ((bool) obj2));
            }
            set
            {
                this.ViewState["ReadOnly"] = value;
            }
        }

        [WebCategory("Behavior"), WebSysDescription("TextBox_Rows"), Themeable(false), DefaultValue(0)]
        public virtual int Rows
        {
            get
            {
                object obj2 = this.ViewState["Rows"];
                if (obj2 != null)
                {
                    return (int) obj2;
                }
                return 0;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("Rows", System.Web.SR.GetString("TextBox_InvalidRows"));
                }
                this.ViewState["Rows"] = value;
            }
        }

        private bool SaveTextViewState
        {
            get
            {
                if (this.TextMode == TextBoxMode.Password)
                {
                    return false;
                }
                if (((base.Events[EventTextChanged] == null) && base.IsEnabled) && ((this.Visible && !this.ReadOnly) && !(base.GetType() != typeof(TextBox))))
                {
                    return false;
                }
                return true;
            }
        }

        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                if (this.TextMode == TextBoxMode.MultiLine)
                {
                    return HtmlTextWriterTag.Textarea;
                }
                return HtmlTextWriterTag.Input;
            }
        }

        [Localizable(true), WebSysDescription("TextBox_Text"), PersistenceMode(PersistenceMode.EncodedInnerDefaultProperty), Editor("System.ComponentModel.Design.MultilineStringEditor,System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), Bindable(true, BindingDirection.TwoWay), WebCategory("Appearance"), DefaultValue("")]
        public virtual string Text
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

        [DefaultValue(0), Themeable(false), WebCategory("Behavior"), WebSysDescription("TextBox_TextMode")]
        public virtual TextBoxMode TextMode
        {
            get
            {
                object obj2 = this.ViewState["Mode"];
                if (obj2 != null)
                {
                    return (TextBoxMode) obj2;
                }
                return TextBoxMode.SingleLine;
            }
            set
            {
                if ((value < TextBoxMode.SingleLine) || (value > TextBoxMode.Password))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.ViewState["Mode"] = value;
            }
        }

        [WebSysDescription("PostBackControl_ValidationGroup"), WebCategory("Behavior"), Themeable(false), DefaultValue("")]
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

        [WebSysDescription("TextBox_Wrap"), DefaultValue(true), WebCategory("Layout")]
        public virtual bool Wrap
        {
            get
            {
                object obj2 = this.ViewState["Wrap"];
                if (obj2 != null)
                {
                    return (bool) obj2;
                }
                return true;
            }
            set
            {
                this.ViewState["Wrap"] = value;
            }
        }
    }
}

