namespace System.Web.UI.WebControls.WebParts
{
    using System;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    [Designer("System.Web.UI.Design.WebControls.WebParts.WebZoneDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), Bindable(false)]
    public abstract class WebZone : CompositeControl
    {
        private Style _emptyZoneTextStyle;
        private Style _errorStyle;
        private TitleStyle _footerStyle;
        private TitleStyle _headerStyle;
        private Style _partChromeStyle;
        private TableStyle _partStyle;
        private TitleStyle _partTitleStyle;
        private Style _verbStyle;
        private System.Web.UI.WebControls.WebParts.WebPartManager _webPartManager;
        private const int baseIndex = 0;
        private const int emptyZoneTextStyleIndex = 1;
        private const int errorStyleIndex = 8;
        private const int footerStyleIndex = 2;
        private const int headerStyleIndex = 6;
        private const int partChromeStyleIndex = 4;
        private const int partStyleIndex = 3;
        private const int partTitleStyleIndex = 5;
        private const int verbStyleIndex = 7;
        private const int viewStateArrayLength = 9;

        internal WebZone()
        {
        }

        public virtual System.Web.UI.WebControls.WebParts.PartChromeType GetEffectiveChromeType(Part part)
        {
            if (part == null)
            {
                throw new ArgumentNullException("part");
            }
            System.Web.UI.WebControls.WebParts.PartChromeType chromeType = part.ChromeType;
            if (chromeType != System.Web.UI.WebControls.WebParts.PartChromeType.Default)
            {
                return chromeType;
            }
            System.Web.UI.WebControls.WebParts.PartChromeType partChromeType = this.PartChromeType;
            if (partChromeType == System.Web.UI.WebControls.WebParts.PartChromeType.Default)
            {
                return System.Web.UI.WebControls.WebParts.PartChromeType.TitleAndBorder;
            }
            return partChromeType;
        }

        protected override void LoadViewState(object savedState)
        {
            if (savedState == null)
            {
                base.LoadViewState(null);
            }
            else
            {
                object[] objArray = (object[]) savedState;
                if (objArray.Length != 9)
                {
                    throw new ArgumentException(System.Web.SR.GetString("ViewState_InvalidViewState"));
                }
                base.LoadViewState(objArray[0]);
                if (objArray[1] != null)
                {
                    ((IStateManager) this.EmptyZoneTextStyle).LoadViewState(objArray[1]);
                }
                if (objArray[2] != null)
                {
                    ((IStateManager) this.FooterStyle).LoadViewState(objArray[2]);
                }
                if (objArray[3] != null)
                {
                    ((IStateManager) this.PartStyle).LoadViewState(objArray[3]);
                }
                if (objArray[4] != null)
                {
                    ((IStateManager) this.PartChromeStyle).LoadViewState(objArray[4]);
                }
                if (objArray[5] != null)
                {
                    ((IStateManager) this.PartTitleStyle).LoadViewState(objArray[5]);
                }
                if (objArray[6] != null)
                {
                    ((IStateManager) this.HeaderStyle).LoadViewState(objArray[6]);
                }
                if (objArray[7] != null)
                {
                    ((IStateManager) this.VerbStyle).LoadViewState(objArray[7]);
                }
                if (objArray[8] != null)
                {
                    ((IStateManager) this.ErrorStyle).LoadViewState(objArray[8]);
                }
            }
        }

        protected internal override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            Page page = this.Page;
            if (page != null)
            {
                if ((page.ControlState >= ControlState.Initialized) && !base.DesignMode)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("Zone_AddedTooLate"));
                }
                if (!base.DesignMode)
                {
                    this._webPartManager = System.Web.UI.WebControls.WebParts.WebPartManager.GetCurrentWebPartManager(page);
                    if (this._webPartManager == null)
                    {
                        throw new InvalidOperationException(System.Web.SR.GetString("WebPartManagerRequired"));
                    }
                    this._webPartManager.RegisterZone(this);
                }
            }
        }

        protected internal override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            Control parent = this.Parent;
            if ((parent != null) && ((parent is WebZone) || (parent is Part)))
            {
                throw new InvalidOperationException(System.Web.SR.GetString("Zone_InvalidParent"));
            }
        }

        public override void RenderBeginTag(HtmlTextWriter writer)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
            writer.AddAttribute(HtmlTextWriterAttribute.Cellpadding, "0");
            writer.AddAttribute(HtmlTextWriterAttribute.Border, "0");
            if (((!base.DesignMode && (this.Page != null)) && ((this.Page.Request.Browser.Type == "IE5") && (this.Page.Request.Browser.Platform == "MacPPC"))) && (!base.ControlStyleCreated || (base.ControlStyle.Height == Unit.Empty)))
            {
                writer.AddStyleAttribute(HtmlTextWriterStyle.Height, "1px");
            }
            base.RenderBeginTag(writer);
        }

        protected virtual void RenderBody(HtmlTextWriter writer)
        {
        }

        internal void RenderBodyTableBeginTag(HtmlTextWriter writer)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
            int padding = this.Padding;
            if (padding >= 0)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Cellpadding, padding.ToString(CultureInfo.InvariantCulture));
            }
            writer.AddAttribute(HtmlTextWriterAttribute.Border, "0");
            string backImageUrl = this.BackImageUrl;
            if (backImageUrl.Trim().Length > 0)
            {
                writer.AddStyleAttribute(HtmlTextWriterStyle.BackgroundImage, "url(" + base.ResolveClientUrl(backImageUrl) + ")");
            }
            writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "100%");
            writer.AddStyleAttribute(HtmlTextWriterStyle.Height, "100%");
            writer.RenderBeginTag(HtmlTextWriterTag.Table);
        }

        internal static void RenderBodyTableEndTag(HtmlTextWriter writer)
        {
            writer.RenderEndTag();
        }

        protected internal override void RenderContents(HtmlTextWriter writer)
        {
            if (this.HasHeader)
            {
                writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                TitleStyle headerStyle = this.HeaderStyle;
                if (!headerStyle.IsEmpty)
                {
                    headerStyle.AddAttributesToRender(writer, this);
                }
                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                this.RenderHeader(writer);
                writer.RenderEndTag();
                writer.RenderEndTag();
            }
            writer.RenderBeginTag(HtmlTextWriterTag.Tr);
            writer.AddStyleAttribute(HtmlTextWriterStyle.Height, "100%");
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            this.RenderBody(writer);
            writer.RenderEndTag();
            writer.RenderEndTag();
            if (this.HasFooter)
            {
                writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                TitleStyle footerStyle = this.FooterStyle;
                if (!footerStyle.IsEmpty)
                {
                    footerStyle.AddAttributesToRender(writer, this);
                }
                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                this.RenderFooter(writer);
                writer.RenderEndTag();
                writer.RenderEndTag();
            }
        }

        internal void RenderDesignerRegionBeginTag(HtmlTextWriter writer, Orientation orientation)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Valign, "top");
            writer.RenderBeginTag(HtmlTextWriterTag.Tr);
            if (orientation == Orientation.Horizontal)
            {
                writer.AddStyleAttribute(HtmlTextWriterStyle.WhiteSpace, "nowrap");
            }
            writer.AddAttribute(HtmlTextWriterAttribute.DesignerRegion, "0");
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            writer.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
            writer.AddAttribute(HtmlTextWriterAttribute.Cellpadding, this.Padding.ToString(CultureInfo.InvariantCulture));
            writer.AddAttribute(HtmlTextWriterAttribute.Border, "0");
            if (orientation == Orientation.Vertical)
            {
                writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "100%");
            }
            else
            {
                writer.AddStyleAttribute(HtmlTextWriterStyle.Height, "100%");
            }
            writer.RenderBeginTag(HtmlTextWriterTag.Table);
        }

        internal static void RenderDesignerRegionEndTag(HtmlTextWriter writer)
        {
            writer.RenderEndTag();
            writer.RenderEndTag();
            writer.RenderEndTag();
        }

        protected virtual void RenderFooter(HtmlTextWriter writer)
        {
        }

        protected virtual void RenderHeader(HtmlTextWriter writer)
        {
        }

        protected override object SaveViewState()
        {
            object[] objArray = new object[] { base.SaveViewState(), (this._emptyZoneTextStyle != null) ? ((IStateManager) this._emptyZoneTextStyle).SaveViewState() : null, (this._footerStyle != null) ? ((IStateManager) this._footerStyle).SaveViewState() : null, (this._partStyle != null) ? ((IStateManager) this._partStyle).SaveViewState() : null, (this._partChromeStyle != null) ? ((IStateManager) this._partChromeStyle).SaveViewState() : null, (this._partTitleStyle != null) ? ((IStateManager) this._partTitleStyle).SaveViewState() : null, (this._headerStyle != null) ? ((IStateManager) this._headerStyle).SaveViewState() : null, (this._verbStyle != null) ? ((IStateManager) this._verbStyle).SaveViewState() : null, (this._errorStyle != null) ? ((IStateManager) this._errorStyle).SaveViewState() : null };
            for (int i = 0; i < 9; i++)
            {
                if (objArray[i] != null)
                {
                    return objArray;
                }
            }
            return null;
        }

        protected override void TrackViewState()
        {
            base.TrackViewState();
            if (this._emptyZoneTextStyle != null)
            {
                ((IStateManager) this._emptyZoneTextStyle).TrackViewState();
            }
            if (this._footerStyle != null)
            {
                ((IStateManager) this._footerStyle).TrackViewState();
            }
            if (this._partStyle != null)
            {
                ((IStateManager) this._partStyle).TrackViewState();
            }
            if (this._partChromeStyle != null)
            {
                ((IStateManager) this._partChromeStyle).TrackViewState();
            }
            if (this._partTitleStyle != null)
            {
                ((IStateManager) this._partTitleStyle).TrackViewState();
            }
            if (this._headerStyle != null)
            {
                ((IStateManager) this._headerStyle).TrackViewState();
            }
            if (this._verbStyle != null)
            {
                ((IStateManager) this._verbStyle).TrackViewState();
            }
            if (this._errorStyle != null)
            {
                ((IStateManager) this._errorStyle).TrackViewState();
            }
        }

        [UrlProperty, DefaultValue(""), Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), WebSysDescription("WebControl_BackImageUrl"), WebCategory("Appearance")]
        public virtual string BackImageUrl
        {
            get
            {
                string str = (string) this.ViewState["BackImageUrl"];
                if (str != null)
                {
                    return str;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["BackImageUrl"] = value;
            }
        }

        [Localizable(true), WebSysDescription("Zone_EmptyZoneText"), WebSysDefaultValue(""), WebCategory("Behavior")]
        public virtual string EmptyZoneText
        {
            get
            {
                string str = (string) this.ViewState["EmptyZoneText"];
                if (str != null)
                {
                    return str;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["EmptyZoneText"] = value;
            }
        }

        [WebSysDescription("Zone_EmptyZoneTextStyle"), WebCategory("Styles"), DefaultValue((string) null), NotifyParentProperty(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), PersistenceMode(PersistenceMode.InnerProperty)]
        public Style EmptyZoneTextStyle
        {
            get
            {
                if (this._emptyZoneTextStyle == null)
                {
                    this._emptyZoneTextStyle = new Style();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._emptyZoneTextStyle).TrackViewState();
                    }
                }
                return this._emptyZoneTextStyle;
            }
        }

        [WebSysDescription("Zone_ErrorStyle"), DefaultValue((string) null), NotifyParentProperty(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), PersistenceMode(PersistenceMode.InnerProperty), WebCategory("Styles")]
        public Style ErrorStyle
        {
            get
            {
                if (this._errorStyle == null)
                {
                    this._errorStyle = new System.Web.UI.WebControls.ErrorStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._errorStyle).TrackViewState();
                    }
                }
                return this._errorStyle;
            }
        }

        [NotifyParentProperty(true), WebSysDescription("Zone_FooterStyle"), DefaultValue((string) null), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), PersistenceMode(PersistenceMode.InnerProperty), WebCategory("Styles")]
        public TitleStyle FooterStyle
        {
            get
            {
                if (this._footerStyle == null)
                {
                    this._footerStyle = new TitleStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._footerStyle).TrackViewState();
                    }
                }
                return this._footerStyle;
            }
        }

        protected virtual bool HasFooter
        {
            get
            {
                return true;
            }
        }

        protected virtual bool HasHeader
        {
            get
            {
                return true;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content), PersistenceMode(PersistenceMode.InnerProperty), WebCategory("Styles"), NotifyParentProperty(true), DefaultValue((string) null), WebSysDescription("Zone_HeaderStyle")]
        public TitleStyle HeaderStyle
        {
            get
            {
                if (this._headerStyle == null)
                {
                    this._headerStyle = new TitleStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._headerStyle).TrackViewState();
                    }
                }
                return this._headerStyle;
            }
        }

        [WebCategory("Appearance"), WebSysDescription("Zone_HeaderText"), Localizable(true), WebSysDefaultValue("")]
        public virtual string HeaderText
        {
            get
            {
                string str = (string) this.ViewState["HeaderText"];
                if (str != null)
                {
                    return str;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["HeaderText"] = value;
            }
        }

        [DefaultValue(2), WebCategory("Layout"), WebSysDescription("Zone_Padding")]
        public virtual int Padding
        {
            get
            {
                object obj2 = this.ViewState["Padding"];
                if (obj2 != null)
                {
                    return (int) obj2;
                }
                return 2;
            }
            set
            {
                if (value < -1)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.ViewState["Padding"] = value;
            }
        }

        [WebSysDescription("Zone_PartChromePadding"), DefaultValue(typeof(Unit), "5px"), WebCategory("WebPart")]
        public Unit PartChromePadding
        {
            get
            {
                object obj2 = this.ViewState["PartChromePadding"];
                if (obj2 != null)
                {
                    return (Unit) obj2;
                }
                return Unit.Pixel(5);
            }
            set
            {
                if (value.Value < 0.0)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.ViewState["PartChromePadding"] = value;
            }
        }

        [WebSysDescription("Zone_PartChromeStyle"), NotifyParentProperty(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), PersistenceMode(PersistenceMode.InnerProperty), DefaultValue((string) null), WebCategory("WebPart")]
        public Style PartChromeStyle
        {
            get
            {
                if (this._partChromeStyle == null)
                {
                    this._partChromeStyle = new Style();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._partChromeStyle).TrackViewState();
                    }
                }
                return this._partChromeStyle;
            }
        }

        [WebCategory("WebPart"), DefaultValue(0), WebSysDescription("Zone_PartChromeType")]
        public virtual System.Web.UI.WebControls.WebParts.PartChromeType PartChromeType
        {
            get
            {
                object obj2 = this.ViewState["PartChromeType"];
                if (obj2 == null)
                {
                    return System.Web.UI.WebControls.WebParts.PartChromeType.Default;
                }
                return (System.Web.UI.WebControls.WebParts.PartChromeType) ((int) obj2);
            }
            set
            {
                if ((value < System.Web.UI.WebControls.WebParts.PartChromeType.Default) || (value > System.Web.UI.WebControls.WebParts.PartChromeType.BorderOnly))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.ViewState["PartChromeType"] = (int) value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content), WebCategory("WebPart"), WebSysDescription("Zone_PartStyle"), DefaultValue((string) null), NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty)]
        public TableStyle PartStyle
        {
            get
            {
                if (this._partStyle == null)
                {
                    this._partStyle = new TableStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._partStyle).TrackViewState();
                    }
                }
                return this._partStyle;
            }
        }

        [DefaultValue((string) null), NotifyParentProperty(true), WebCategory("WebPart"), WebSysDescription("Zone_PartTitleStyle"), PersistenceMode(PersistenceMode.InnerProperty), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public TitleStyle PartTitleStyle
        {
            get
            {
                if (this._partTitleStyle == null)
                {
                    this._partTitleStyle = new TitleStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._partTitleStyle).TrackViewState();
                    }
                }
                return this._partTitleStyle;
            }
        }

        protected internal bool RenderClientScript
        {
            get
            {
                bool renderClientScript = false;
                if (base.DesignMode)
                {
                    return true;
                }
                if (this.WebPartManager != null)
                {
                    renderClientScript = this.WebPartManager.RenderClientScript;
                }
                return renderClientScript;
            }
        }

        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                return HtmlTextWriterTag.Table;
            }
        }

        [WebSysDescription("Zone_VerbButtonType"), WebCategory("Appearance"), DefaultValue(0)]
        public virtual ButtonType VerbButtonType
        {
            get
            {
                object obj2 = this.ViewState["VerbButtonType"];
                if (obj2 != null)
                {
                    return (ButtonType) obj2;
                }
                return ButtonType.Button;
            }
            set
            {
                if ((value < ButtonType.Button) || (value > ButtonType.Link))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.ViewState["VerbButtonType"] = value;
            }
        }

        [NotifyParentProperty(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), WebSysDescription("Zone_VerbStyle"), DefaultValue((string) null), PersistenceMode(PersistenceMode.InnerProperty), WebCategory("Styles")]
        public Style VerbStyle
        {
            get
            {
                if (this._verbStyle == null)
                {
                    this._verbStyle = new Style();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._verbStyle).TrackViewState();
                    }
                }
                return this._verbStyle;
            }
        }

        protected System.Web.UI.WebControls.WebParts.WebPartManager WebPartManager
        {
            get
            {
                return this._webPartManager;
            }
        }
    }
}

