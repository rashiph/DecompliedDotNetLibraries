namespace System.Web.UI.WebControls
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Drawing;
    using System.Globalization;
    using System.Runtime;
    using System.Web;
    using System.Web.UI;
    using System.Web.Util;

    [Themeable(true), ParseChildren(true), PersistChildren(false)]
    public class WebControl : Control, IAttributeAccessor
    {
        private static string _disabledCssClass = "aspNetDisabled";
        private SimpleBitVector32 _webControlFlags;
        private const int accessKeySet = 4;
        private System.Web.UI.AttributeCollection attrColl;
        private StateBag attrState;
        private System.Web.UI.WebControls.Style controlStyle;
        private const int deferStyleLoadViewState = 1;
        private const int disabledDirty = 2;
        private const int tabIndexSet = 0x10;
        private HtmlTextWriterTag tagKey;
        private string tagName;
        private const int toolTipSet = 8;

        protected WebControl() : this(HtmlTextWriterTag.Span)
        {
        }

        protected WebControl(string tag)
        {
            this.tagKey = HtmlTextWriterTag.Unknown;
            this.tagName = tag;
        }

        public WebControl(HtmlTextWriterTag tag)
        {
            this.tagKey = tag;
        }

        protected virtual void AddAttributesToRender(HtmlTextWriter writer)
        {
            if (this.ID != null)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Id, this.ClientID);
            }
            if (this._webControlFlags[4])
            {
                string accessKey = this.AccessKey;
                if (accessKey.Length > 0)
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Accesskey, accessKey);
                }
            }
            if (!this.Enabled)
            {
                if (this.SupportsDisabledAttribute)
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Disabled, "disabled");
                }
                if ((this.RenderingCompatibility >= VersionUtil.Framework40) && !string.IsNullOrEmpty(DisabledCssClass))
                {
                    if (string.IsNullOrEmpty(this.CssClass))
                    {
                        this.ControlStyle.CssClass = DisabledCssClass;
                    }
                    else
                    {
                        this.ControlStyle.CssClass = DisabledCssClass + " " + this.CssClass;
                    }
                }
            }
            if (this._webControlFlags[0x10])
            {
                int tabIndex = this.TabIndex;
                if (tabIndex != 0)
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Tabindex, tabIndex.ToString(NumberFormatInfo.InvariantInfo));
                }
            }
            if (this._webControlFlags[8])
            {
                string toolTip = this.ToolTip;
                if (toolTip.Length > 0)
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Title, toolTip);
                }
            }
            if ((this.TagKey == HtmlTextWriterTag.Span) || (this.TagKey == HtmlTextWriterTag.A))
            {
                this.AddDisplayInlineBlockIfNeeded(writer);
            }
            if (this.ControlStyleCreated && !this.ControlStyle.IsEmpty)
            {
                this.ControlStyle.AddAttributesToRender(writer, this);
            }
            if (this.attrState != null)
            {
                System.Web.UI.AttributeCollection attributes = this.Attributes;
                IEnumerator enumerator = attributes.Keys.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    string current = (string) enumerator.Current;
                    writer.AddAttribute(current, attributes[current]);
                }
            }
        }

        internal void AddDisplayInlineBlockIfNeeded(HtmlTextWriter writer)
        {
            if ((!this.RequiresLegacyRendering || !base.EnableLegacyRendering) && (((this.BorderStyle != System.Web.UI.WebControls.BorderStyle.NotSet) || !this.BorderWidth.IsEmpty) || (!this.Height.IsEmpty || !this.Width.IsEmpty)))
            {
                writer.AddStyleAttribute(HtmlTextWriterStyle.Display, "inline-block");
            }
        }

        public void ApplyStyle(System.Web.UI.WebControls.Style s)
        {
            if ((s != null) && !s.IsEmpty)
            {
                this.ControlStyle.CopyFrom(s);
            }
        }

        public void CopyBaseAttributes(WebControl controlSrc)
        {
            if (controlSrc == null)
            {
                throw new ArgumentNullException("controlSrc");
            }
            if (controlSrc._webControlFlags[4])
            {
                this.AccessKey = controlSrc.AccessKey;
            }
            if (!controlSrc.Enabled)
            {
                this.Enabled = false;
            }
            if (controlSrc._webControlFlags[8])
            {
                this.ToolTip = controlSrc.ToolTip;
            }
            if (controlSrc._webControlFlags[0x10])
            {
                this.TabIndex = controlSrc.TabIndex;
            }
            if (controlSrc.HasAttributes)
            {
                foreach (string str in controlSrc.Attributes.Keys)
                {
                    this.Attributes[str] = controlSrc.Attributes[str];
                }
            }
        }

        protected virtual System.Web.UI.WebControls.Style CreateControlStyle()
        {
            return new System.Web.UI.WebControls.Style(this.ViewState);
        }

        protected override void LoadViewState(object savedState)
        {
            if (savedState != null)
            {
                Pair pair = (Pair) savedState;
                base.LoadViewState(pair.First);
                if (this.ControlStyleCreated || (this.ViewState["_!SB"] != null))
                {
                    this.ControlStyle.LoadViewState(null);
                }
                else
                {
                    this._webControlFlags.Set(1);
                }
                if (pair.Second != null)
                {
                    if (this.attrState == null)
                    {
                        this.attrState = new StateBag(true);
                        this.attrState.TrackViewState();
                    }
                    this.attrState.LoadViewState(pair.Second);
                }
            }
            object obj2 = this.ViewState["Enabled"];
            if (obj2 != null)
            {
                if (!((bool) obj2))
                {
                    this.flags.Set(0x80000);
                }
                else
                {
                    this.flags.Clear(0x80000);
                }
                this._webControlFlags.Set(2);
            }
            if (((IDictionary) this.ViewState).Contains("AccessKey"))
            {
                this._webControlFlags.Set(4);
            }
            if (((IDictionary) this.ViewState).Contains("TabIndex"))
            {
                this._webControlFlags.Set(0x10);
            }
            if (((IDictionary) this.ViewState).Contains("ToolTip"))
            {
                this._webControlFlags.Set(8);
            }
        }

        public void MergeStyle(System.Web.UI.WebControls.Style s)
        {
            if ((s != null) && !s.IsEmpty)
            {
                this.ControlStyle.MergeWith(s);
            }
        }

        protected internal override void Render(HtmlTextWriter writer)
        {
            this.RenderBeginTag(writer);
            this.RenderContents(writer);
            this.RenderEndTag(writer);
        }

        public virtual void RenderBeginTag(HtmlTextWriter writer)
        {
            this.AddAttributesToRender(writer);
            HtmlTextWriterTag tagKey = this.TagKey;
            if (tagKey != HtmlTextWriterTag.Unknown)
            {
                writer.RenderBeginTag(tagKey);
            }
            else
            {
                writer.RenderBeginTag(this.TagName);
            }
        }

        protected internal virtual void RenderContents(HtmlTextWriter writer)
        {
            base.Render(writer);
        }

        public virtual void RenderEndTag(HtmlTextWriter writer)
        {
            writer.RenderEndTag();
        }

        protected override object SaveViewState()
        {
            Pair pair = null;
            if (this._webControlFlags[2])
            {
                this.ViewState["Enabled"] = !this.flags[0x80000];
            }
            if (this.ControlStyleCreated)
            {
                this.ControlStyle.SaveViewState();
            }
            object x = base.SaveViewState();
            object y = null;
            if (this.attrState != null)
            {
                y = this.attrState.SaveViewState();
            }
            if ((x == null) && (y == null))
            {
                return pair;
            }
            return new Pair(x, y);
        }

        string IAttributeAccessor.GetAttribute(string name)
        {
            if (this.attrState == null)
            {
                return null;
            }
            return (string) this.attrState[name];
        }

        void IAttributeAccessor.SetAttribute(string name, string value)
        {
            this.Attributes[name] = value;
        }

        protected override void TrackViewState()
        {
            base.TrackViewState();
            if (this.ControlStyleCreated)
            {
                this.ControlStyle.TrackViewState();
            }
            if (this.attrState != null)
            {
                this.attrState.TrackViewState();
            }
        }

        [DefaultValue(""), WebCategory("Accessibility"), WebSysDescription("WebControl_AccessKey")]
        public virtual string AccessKey
        {
            get
            {
                if (this._webControlFlags[4])
                {
                    string str = (string) this.ViewState["AccessKey"];
                    if (str != null)
                    {
                        return str;
                    }
                }
                return string.Empty;
            }
            set
            {
                if ((value != null) && (value.Length > 1))
                {
                    throw new ArgumentOutOfRangeException("value", System.Web.SR.GetString("WebControl_InvalidAccessKey"));
                }
                this.ViewState["AccessKey"] = value;
                this._webControlFlags.Set(4);
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), WebSysDescription("WebControl_Attributes")]
        public System.Web.UI.AttributeCollection Attributes
        {
            get
            {
                if (this.attrColl == null)
                {
                    if (this.attrState == null)
                    {
                        this.attrState = new StateBag(true);
                        if (base.IsTrackingViewState)
                        {
                            this.attrState.TrackViewState();
                        }
                    }
                    this.attrColl = new System.Web.UI.AttributeCollection(this.attrState);
                }
                return this.attrColl;
            }
        }

        [WebCategory("Appearance"), DefaultValue(typeof(Color), ""), WebSysDescription("WebControl_BackColor"), TypeConverter(typeof(WebColorConverter))]
        public virtual Color BackColor
        {
            get
            {
                if (!this.ControlStyleCreated)
                {
                    return Color.Empty;
                }
                return this.ControlStyle.BackColor;
            }
            set
            {
                this.ControlStyle.BackColor = value;
            }
        }

        [WebSysDescription("WebControl_BorderColor"), WebCategory("Appearance"), DefaultValue(typeof(Color), ""), TypeConverter(typeof(WebColorConverter))]
        public virtual Color BorderColor
        {
            get
            {
                if (!this.ControlStyleCreated)
                {
                    return Color.Empty;
                }
                return this.ControlStyle.BorderColor;
            }
            set
            {
                this.ControlStyle.BorderColor = value;
            }
        }

        [WebCategory("Appearance"), DefaultValue(0), WebSysDescription("WebControl_BorderStyle")]
        public virtual System.Web.UI.WebControls.BorderStyle BorderStyle
        {
            get
            {
                if (!this.ControlStyleCreated)
                {
                    return System.Web.UI.WebControls.BorderStyle.NotSet;
                }
                return this.ControlStyle.BorderStyle;
            }
            set
            {
                this.ControlStyle.BorderStyle = value;
            }
        }

        [DefaultValue(typeof(Unit), ""), WebCategory("Appearance"), WebSysDescription("WebControl_BorderWidth")]
        public virtual Unit BorderWidth
        {
            get
            {
                if (!this.ControlStyleCreated)
                {
                    return Unit.Empty;
                }
                return this.ControlStyle.BorderWidth;
            }
            set
            {
                this.ControlStyle.BorderWidth = value;
            }
        }

        [WebSysDescription("WebControl_ControlStyle"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public System.Web.UI.WebControls.Style ControlStyle
        {
            get
            {
                if (this.controlStyle == null)
                {
                    this.controlStyle = this.CreateControlStyle();
                    if (base.IsTrackingViewState)
                    {
                        this.controlStyle.TrackViewState();
                    }
                    if (this._webControlFlags[1])
                    {
                        this._webControlFlags.Clear(1);
                        this.controlStyle.LoadViewState(null);
                    }
                }
                return this.controlStyle;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Advanced), WebSysDescription("WebControl_ControlStyleCreated")]
        public bool ControlStyleCreated
        {
            get
            {
                return (this.controlStyle != null);
            }
        }

        [WebCategory("Appearance"), DefaultValue(""), WebSysDescription("WebControl_CSSClassName"), CssClassProperty]
        public virtual string CssClass
        {
            get
            {
                if (!this.ControlStyleCreated)
                {
                    return string.Empty;
                }
                return this.ControlStyle.CssClass;
            }
            set
            {
                this.ControlStyle.CssClass = value;
            }
        }

        public static string DisabledCssClass
        {
            get
            {
                return (_disabledCssClass ?? string.Empty);
            }
            set
            {
                _disabledCssClass = value;
            }
        }

        [WebCategory("Behavior"), Bindable(true), Themeable(false), DefaultValue(true), WebSysDescription("WebControl_Enabled")]
        public virtual bool Enabled
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return !this.flags[0x80000];
            }
            set
            {
                bool flag = !this.flags[0x80000];
                if (flag != value)
                {
                    if (!value)
                    {
                        this.flags.Set(0x80000);
                    }
                    else
                    {
                        this.flags.Clear(0x80000);
                    }
                    if (base.IsTrackingViewState)
                    {
                        this._webControlFlags.Set(2);
                    }
                }
            }
        }

        [Browsable(true)]
        public override bool EnableTheming
        {
            get
            {
                return base.EnableTheming;
            }
            set
            {
                base.EnableTheming = value;
            }
        }

        [WebCategory("Appearance"), WebSysDescription("WebControl_Font"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true)]
        public virtual FontInfo Font
        {
            get
            {
                return this.ControlStyle.Font;
            }
        }

        [TypeConverter(typeof(WebColorConverter)), WebCategory("Appearance"), DefaultValue(typeof(Color), ""), WebSysDescription("WebControl_ForeColor")]
        public virtual Color ForeColor
        {
            get
            {
                if (!this.ControlStyleCreated)
                {
                    return Color.Empty;
                }
                return this.ControlStyle.ForeColor;
            }
            set
            {
                this.ControlStyle.ForeColor = value;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool HasAttributes
        {
            get
            {
                return (((this.attrColl != null) && (this.attrColl.Count > 0)) || ((this.attrState != null) && (this.attrState.Count > 0)));
            }
        }

        [WebSysDescription("WebControl_Height"), WebCategory("Layout"), DefaultValue(typeof(Unit), "")]
        public virtual Unit Height
        {
            get
            {
                if (!this.ControlStyleCreated)
                {
                    return Unit.Empty;
                }
                return this.ControlStyle.Height;
            }
            set
            {
                this.ControlStyle.Height = value;
            }
        }

        protected internal bool IsEnabled
        {
            get
            {
                for (Control control = this; control != null; control = control.Parent)
                {
                    if (control.flags[0x80000])
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        internal virtual bool RequiresLegacyRendering
        {
            get
            {
                return false;
            }
        }

        [Browsable(true)]
        public override string SkinID
        {
            get
            {
                return base.SkinID;
            }
            set
            {
                base.SkinID = value;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), WebSysDescription("WebControl_Style")]
        public CssStyleCollection Style
        {
            get
            {
                return this.Attributes.CssStyle;
            }
        }

        [Browsable(false)]
        public virtual bool SupportsDisabledAttribute
        {
            get
            {
                return true;
            }
        }

        [DefaultValue((short) 0), WebSysDescription("WebControl_TabIndex"), WebCategory("Accessibility")]
        public virtual short TabIndex
        {
            get
            {
                if (this._webControlFlags[0x10])
                {
                    object obj2 = this.ViewState["TabIndex"];
                    if (obj2 != null)
                    {
                        return (short) obj2;
                    }
                }
                return 0;
            }
            set
            {
                this.ViewState["TabIndex"] = value;
                this._webControlFlags.Set(0x10);
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        protected virtual HtmlTextWriterTag TagKey
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return this.tagKey;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        protected virtual string TagName
        {
            get
            {
                if ((this.tagName == null) && (this.TagKey != HtmlTextWriterTag.Unknown))
                {
                    this.tagName = Enum.Format(typeof(HtmlTextWriterTag), this.TagKey, "G").ToLower(CultureInfo.InvariantCulture);
                }
                return this.tagName;
            }
        }

        [Localizable(true), WebCategory("Behavior"), WebSysDescription("WebControl_Tooltip"), DefaultValue("")]
        public virtual string ToolTip
        {
            get
            {
                if (this._webControlFlags[8])
                {
                    string str = (string) this.ViewState["ToolTip"];
                    if (str != null)
                    {
                        return str;
                    }
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["ToolTip"] = value;
                this._webControlFlags.Set(8);
            }
        }

        [WebSysDescription("WebControl_Width"), DefaultValue(typeof(Unit), ""), WebCategory("Layout")]
        public virtual Unit Width
        {
            get
            {
                if (!this.ControlStyleCreated)
                {
                    return Unit.Empty;
                }
                return this.ControlStyle.Width;
            }
            set
            {
                this.ControlStyle.Width = value;
            }
        }
    }
}

