namespace System.Web.UI.WebControls
{
    using System;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Globalization;
    using System.Web;
    using System.Web.UI;
    using System.Web.Util;

    [SupportsEventValidation, DataBindingHandler("System.Web.UI.Design.TextDataBindingHandler, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), ControlValueProperty("Checked"), DefaultEvent("CheckedChanged"), Designer("System.Web.UI.Design.WebControls.CheckBoxDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), DefaultProperty("Text")]
    public class CheckBox : WebControl, IPostBackDataHandler, ICheckBoxControl
    {
        internal System.Web.UI.AttributeCollection _inputAttributes;
        private StateBag _inputAttributesState;
        private System.Web.UI.AttributeCollection _labelAttributes;
        private StateBag _labelAttributesState;
        private string _valueAttribute;
        private static readonly object EventCheckedChanged = new object();

        [WebSysDescription("Control_OnServerCheckChanged"), WebCategory("Action")]
        public event EventHandler CheckedChanged
        {
            add
            {
                base.Events.AddHandler(EventCheckedChanged, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventCheckedChanged, value);
            }
        }

        public CheckBox() : base(HtmlTextWriterTag.Input)
        {
        }

        protected override void AddAttributesToRender(HtmlTextWriter writer)
        {
            base.AddDisplayInlineBlockIfNeeded(writer);
        }

        protected virtual bool LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
            bool flag = false;
            string str = postCollection[postDataKey];
            bool flag2 = !string.IsNullOrEmpty(str);
            if (flag2)
            {
                base.ValidateEvent(postDataKey);
            }
            flag = flag2 != this.Checked;
            this.Checked = flag2;
            return flag;
        }

        protected override void LoadViewState(object savedState)
        {
            if (savedState != null)
            {
                Triplet triplet = (Triplet) savedState;
                base.LoadViewState(triplet.First);
                if (triplet.Second != null)
                {
                    if (this._inputAttributesState == null)
                    {
                        this._inputAttributesState = new StateBag();
                        this._inputAttributesState.TrackViewState();
                    }
                    this._inputAttributesState.LoadViewState(triplet.Second);
                }
                if (triplet.Third != null)
                {
                    if (this._labelAttributesState == null)
                    {
                        this._labelAttributesState = new StateBag();
                        this._labelAttributesState.TrackViewState();
                    }
                    this._labelAttributesState.LoadViewState(triplet.Second);
                }
            }
        }

        protected virtual void OnCheckedChanged(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EventCheckedChanged];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected internal override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            bool autoPostBack = this.AutoPostBack;
            if ((this.Page != null) && base.IsEnabled)
            {
                this.Page.RegisterRequiresPostBack(this);
                if (autoPostBack)
                {
                    this.Page.RegisterPostBackScript();
                    this.Page.RegisterFocusScript();
                    if (this.CausesValidation && (this.Page.GetValidators(this.ValidationGroup).Count > 0))
                    {
                        this.Page.RegisterWebFormsScript();
                    }
                }
            }
            if (!this.SaveCheckedViewState(autoPostBack))
            {
                this.ViewState.SetItemDirty("Checked", false);
                if ((this.Page != null) && base.IsEnabled)
                {
                    this.Page.RegisterEnabledControl(this);
                }
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
            this.OnCheckedChanged(EventArgs.Empty);
        }

        protected internal override void Render(HtmlTextWriter writer)
        {
            this.AddAttributesToRender(writer);
            if (this.Page != null)
            {
                this.Page.VerifyRenderingInServerForm(this);
            }
            bool flag = false;
            if (!base.IsEnabled)
            {
                if (this.RenderingCompatibility < VersionUtil.Framework40)
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Disabled, "disabled");
                    flag = true;
                }
                else if (!this.Enabled && !string.IsNullOrEmpty(WebControl.DisabledCssClass))
                {
                    if (string.IsNullOrEmpty(this.CssClass))
                    {
                        base.ControlStyle.CssClass = WebControl.DisabledCssClass;
                    }
                    else
                    {
                        base.ControlStyle.CssClass = WebControl.DisabledCssClass + " " + this.CssClass;
                    }
                    flag = true;
                }
            }
            if (base.ControlStyleCreated)
            {
                Style controlStyle = base.ControlStyle;
                if (!controlStyle.IsEmpty)
                {
                    controlStyle.AddAttributesToRender(writer, this);
                    flag = true;
                }
            }
            string toolTip = this.ToolTip;
            if (toolTip.Length > 0)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Title, toolTip);
                flag = true;
            }
            string str2 = null;
            if (base.HasAttributes)
            {
                System.Web.UI.AttributeCollection attributes = base.Attributes;
                string str3 = attributes["value"];
                if (str3 != null)
                {
                    attributes.Remove("value");
                }
                str2 = attributes["onclick"];
                if (str2 != null)
                {
                    str2 = Util.EnsureEndWithSemiColon(str2);
                    attributes.Remove("onclick");
                }
                if (attributes.Count != 0)
                {
                    attributes.AddAttributes(writer);
                    flag = true;
                }
                if (str3 != null)
                {
                    attributes["value"] = str3;
                }
            }
            if (flag)
            {
                writer.RenderBeginTag(HtmlTextWriterTag.Span);
            }
            string text = this.Text;
            string clientID = this.ClientID;
            if (text.Length != 0)
            {
                if (this.TextAlign == System.Web.UI.WebControls.TextAlign.Left)
                {
                    this.RenderLabel(writer, text, clientID);
                    this.RenderInputTag(writer, clientID, str2);
                }
                else
                {
                    this.RenderInputTag(writer, clientID, str2);
                    this.RenderLabel(writer, text, clientID);
                }
            }
            else
            {
                this.RenderInputTag(writer, clientID, str2);
            }
            if (flag)
            {
                writer.RenderEndTag();
            }
        }

        internal virtual void RenderInputTag(HtmlTextWriter writer, string clientID, string onClick)
        {
            if (clientID != null)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Id, clientID);
            }
            writer.AddAttribute(HtmlTextWriterAttribute.Type, "checkbox");
            if (this.UniqueID != null)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Name, this.UniqueID);
            }
            if (this._valueAttribute != null)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Value, this._valueAttribute);
            }
            if (this.Checked)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Checked, "checked");
            }
            if (!base.IsEnabled && this.SupportsDisabledAttribute)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Disabled, "disabled");
            }
            if ((this.AutoPostBack && (this.Page != null)) && this.Page.ClientSupportsJavaScript)
            {
                PostBackOptions options = new PostBackOptions(this, string.Empty);
                if (this.CausesValidation && (this.Page.GetValidators(this.ValidationGroup).Count > 0))
                {
                    options.PerformValidation = true;
                    options.ValidationGroup = this.ValidationGroup;
                }
                if (this.Page.Form != null)
                {
                    options.AutoPostBack = true;
                }
                onClick = Util.MergeScript(onClick, this.Page.ClientScript.GetPostBackEventReference(options, true));
                writer.AddAttribute(HtmlTextWriterAttribute.Onclick, onClick);
                if (base.EnableLegacyRendering)
                {
                    writer.AddAttribute("language", "javascript", false);
                }
            }
            else
            {
                if (this.Page != null)
                {
                    this.Page.ClientScript.RegisterForEventValidation(this.UniqueID);
                }
                if (onClick != null)
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Onclick, onClick);
                }
            }
            string accessKey = this.AccessKey;
            if (accessKey.Length > 0)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Accesskey, accessKey);
            }
            int tabIndex = this.TabIndex;
            if (tabIndex != 0)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Tabindex, tabIndex.ToString(NumberFormatInfo.InvariantInfo));
            }
            if ((this._inputAttributes != null) && (this._inputAttributes.Count != 0))
            {
                this._inputAttributes.AddAttributes(writer);
            }
            writer.RenderBeginTag(HtmlTextWriterTag.Input);
            writer.RenderEndTag();
        }

        private void RenderLabel(HtmlTextWriter writer, string text, string clientID)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.For, clientID);
            if ((this._labelAttributes != null) && (this._labelAttributes.Count != 0))
            {
                this._labelAttributes.AddAttributes(writer);
            }
            writer.RenderBeginTag(HtmlTextWriterTag.Label);
            writer.Write(text);
            writer.RenderEndTag();
        }

        private bool SaveCheckedViewState(bool autoPostBack)
        {
            if ((((base.Events[EventCheckedChanged] != null) || !base.IsEnabled) || !this.Visible) || ((autoPostBack && (this.Page != null)) && !this.Page.ClientSupportsJavaScript))
            {
                return true;
            }
            Type type = base.GetType();
            return (!(type == typeof(CheckBox)) && !(type == typeof(RadioButton)));
        }

        protected override object SaveViewState()
        {
            object x = base.SaveViewState();
            object y = null;
            object z = null;
            object obj5 = null;
            if (this._inputAttributesState != null)
            {
                y = this._inputAttributesState.SaveViewState();
            }
            if (this._labelAttributesState != null)
            {
                z = this._labelAttributesState.SaveViewState();
            }
            if (((x == null) && (y == null)) && (z == null))
            {
                return obj5;
            }
            return new Triplet(x, y, z);
        }

        bool IPostBackDataHandler.LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
            return this.LoadPostData(postDataKey, postCollection);
        }

        void IPostBackDataHandler.RaisePostDataChangedEvent()
        {
            this.RaisePostDataChangedEvent();
        }

        protected override void TrackViewState()
        {
            base.TrackViewState();
            if (this._inputAttributesState != null)
            {
                this._inputAttributesState.TrackViewState();
            }
            if (this._labelAttributesState != null)
            {
                this._labelAttributesState.TrackViewState();
            }
        }

        [DefaultValue(false), Themeable(false), WebCategory("Behavior"), WebSysDescription("CheckBox_AutoPostBack")]
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

        [WebCategory("Behavior"), DefaultValue(false), WebSysDescription("AutoPostBackControl_CausesValidation"), Themeable(false)]
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

        [Bindable(true, BindingDirection.TwoWay), Themeable(false), DefaultValue(false), WebSysDescription("CheckBox_Checked")]
        public virtual bool Checked
        {
            get
            {
                object obj2 = this.ViewState["Checked"];
                return ((obj2 != null) && ((bool) obj2));
            }
            set
            {
                this.ViewState["Checked"] = value;
            }
        }

        [WebSysDescription("CheckBox_InputAttributes"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public System.Web.UI.AttributeCollection InputAttributes
        {
            get
            {
                if (this._inputAttributes == null)
                {
                    if (this._inputAttributesState == null)
                    {
                        this._inputAttributesState = new StateBag(true);
                        if (base.IsTrackingViewState)
                        {
                            this._inputAttributesState.TrackViewState();
                        }
                    }
                    this._inputAttributes = new System.Web.UI.AttributeCollection(this._inputAttributesState);
                }
                return this._inputAttributes;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), WebSysDescription("CheckBox_LabelAttributes"), Browsable(false)]
        public System.Web.UI.AttributeCollection LabelAttributes
        {
            get
            {
                if (this._labelAttributes == null)
                {
                    if (this._labelAttributesState == null)
                    {
                        this._labelAttributesState = new StateBag(true);
                        if (base.IsTrackingViewState)
                        {
                            this._labelAttributesState.TrackViewState();
                        }
                    }
                    this._labelAttributes = new System.Web.UI.AttributeCollection(this._labelAttributesState);
                }
                return this._labelAttributes;
            }
        }

        internal override bool RequiresLegacyRendering
        {
            get
            {
                return true;
            }
        }

        [DefaultValue(""), Bindable(true), Localizable(true), WebCategory("Appearance"), WebSysDescription("CheckBox_Text")]
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

        [WebCategory("Appearance"), WebSysDescription("WebControl_TextAlign"), DefaultValue(2)]
        public virtual System.Web.UI.WebControls.TextAlign TextAlign
        {
            get
            {
                object obj2 = this.ViewState["TextAlign"];
                if (obj2 != null)
                {
                    return (System.Web.UI.WebControls.TextAlign) obj2;
                }
                return System.Web.UI.WebControls.TextAlign.Right;
            }
            set
            {
                if ((value < System.Web.UI.WebControls.TextAlign.Left) || (value > System.Web.UI.WebControls.TextAlign.Right))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.ViewState["TextAlign"] = value;
            }
        }

        [DefaultValue(""), WebSysDescription("PostBackControl_ValidationGroup"), Themeable(false), WebCategory("Behavior")]
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

