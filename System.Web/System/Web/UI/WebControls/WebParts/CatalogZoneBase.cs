namespace System.Web.UI.WebControls.WebParts
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Globalization;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    public abstract class CatalogZoneBase : ToolZone, IPostBackDataHandler
    {
        private WebPartVerb _addVerb;
        private System.Web.UI.WebControls.WebParts.CatalogPartChrome _catalogPartChrome;
        private CatalogPartCollection _catalogParts;
        private WebPartVerb _closeVerb;
        private Style _partLinkStyle;
        private string _selectedCatalogPartID;
        private string[] _selectedCheckBoxValues;
        private Style _selectedPartLinkStyle;
        private string _selectedZoneID;
        private const string addEventArgument = "add";
        private const int addVerbIndex = 1;
        private const int baseIndex = 0;
        private const string closeEventArgument = "close";
        private const int closeVerbIndex = 2;
        private const int controlStateArrayLength = 2;
        private const int partLinkStyleIndex = 3;
        private const int selectedCatalogPartIDIndex = 1;
        private const int selectedPartLinkStyleIndex = 4;
        private const string selectEventArgument = "select";
        private const int viewStateArrayLength = 5;

        protected CatalogZoneBase() : base(WebPartManager.CatalogDisplayMode)
        {
        }

        private void AddSelectedWebParts()
        {
            WebPartZoneBase zone = null;
            if (base.WebPartManager != null)
            {
                zone = base.WebPartManager.Zones[this._selectedZoneID];
            }
            CatalogPart selectedCatalogPart = this.SelectedCatalogPart;
            WebPartDescriptionCollection availableWebPartDescriptions = null;
            if (selectedCatalogPart != null)
            {
                availableWebPartDescriptions = selectedCatalogPart.GetAvailableWebPartDescriptions();
            }
            if (((zone != null) && zone.AllowLayoutChange) && ((this._selectedCheckBoxValues != null) && (availableWebPartDescriptions != null)))
            {
                ArrayList webParts = new ArrayList();
                for (int i = 0; i < this._selectedCheckBoxValues.Length; i++)
                {
                    string str = this._selectedCheckBoxValues[i];
                    WebPartDescription description = availableWebPartDescriptions[str];
                    if (description != null)
                    {
                        WebPart webPart = selectedCatalogPart.GetWebPart(description);
                        if (webPart != null)
                        {
                            webParts.Add(webPart);
                        }
                    }
                }
                this.AddWebParts(webParts, zone);
            }
        }

        private void AddWebParts(ArrayList webParts, WebPartZoneBase zone)
        {
            webParts.Reverse();
            foreach (WebPart part in webParts)
            {
                WebPartZoneBase base2 = zone;
                if (!part.AllowZoneChange && (part.Zone != null))
                {
                    base2 = part.Zone;
                }
                base.WebPartManager.AddWebPart(part, base2, 0);
            }
        }

        protected override void Close()
        {
            if (base.WebPartManager != null)
            {
                base.WebPartManager.DisplayMode = WebPartManager.BrowseDisplayMode;
            }
        }

        protected virtual System.Web.UI.WebControls.WebParts.CatalogPartChrome CreateCatalogPartChrome()
        {
            return new System.Web.UI.WebControls.WebParts.CatalogPartChrome(this);
        }

        protected abstract CatalogPartCollection CreateCatalogParts();
        protected internal override void CreateChildControls()
        {
            this.Controls.Clear();
            foreach (CatalogPart part in this.CatalogParts)
            {
                part.SetWebPartManager(base.WebPartManager);
                part.SetZone(this);
                this.Controls.Add(part);
            }
        }

        internal string GetCheckBoxID(string value)
        {
            return string.Concat(new object[] { this.ClientID, base.ClientIDSeparator, "_checkbox", base.ClientIDSeparator, value });
        }

        protected void InvalidateCatalogParts()
        {
            this._catalogParts = null;
            base.ChildControlsCreated = false;
        }

        protected internal override void LoadControlState(object savedState)
        {
            if (savedState == null)
            {
                base.LoadControlState(null);
            }
            else
            {
                object[] objArray = (object[]) savedState;
                if (objArray.Length != 2)
                {
                    throw new ArgumentException(System.Web.SR.GetString("Invalid_ControlState"));
                }
                base.LoadControlState(objArray[0]);
                if (objArray[1] != null)
                {
                    this._selectedCatalogPartID = (string) objArray[1];
                }
            }
        }

        protected virtual bool LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
            string str = postCollection[this.CheckBoxName];
            if (!string.IsNullOrEmpty(str))
            {
                base.ValidateEvent(this.CheckBoxName);
                this._selectedCheckBoxValues = str.Split(new char[] { ',' });
            }
            this._selectedZoneID = postCollection[this.ZonesID];
            return false;
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
                if (objArray.Length != 5)
                {
                    throw new ArgumentException(System.Web.SR.GetString("ViewState_InvalidViewState"));
                }
                base.LoadViewState(objArray[0]);
                if (objArray[1] != null)
                {
                    ((IStateManager) this.AddVerb).LoadViewState(objArray[1]);
                }
                if (objArray[2] != null)
                {
                    ((IStateManager) this.CloseVerb).LoadViewState(objArray[2]);
                }
                if (objArray[3] != null)
                {
                    ((IStateManager) this.PartLinkStyle).LoadViewState(objArray[3]);
                }
                if (objArray[4] != null)
                {
                    ((IStateManager) this.SelectedPartLinkStyle).LoadViewState(objArray[4]);
                }
            }
        }

        protected internal override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            Page page = this.Page;
            if (page != null)
            {
                page.RegisterRequiresControlState(this);
            }
        }

        protected internal override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            this.CatalogPartChrome.PerformPreRender();
            this.Page.RegisterRequiresPostBack(this);
        }

        protected override void RaisePostBackEvent(string eventArgument)
        {
            string[] strArray = eventArgument.Split(new char[] { '$' });
            if ((strArray.Length == 2) && (strArray[0] == "select"))
            {
                this.SelectedCatalogPartID = strArray[1];
            }
            else if (string.Equals(eventArgument, "add", StringComparison.OrdinalIgnoreCase))
            {
                if (this.AddVerb.Visible && this.AddVerb.Enabled)
                {
                    this.AddSelectedWebParts();
                }
            }
            else if (string.Equals(eventArgument, "close", StringComparison.OrdinalIgnoreCase))
            {
                if (this.CloseVerb.Visible && this.CloseVerb.Enabled)
                {
                    this.Close();
                }
            }
            else
            {
                base.RaisePostBackEvent(eventArgument);
            }
        }

        protected internal override void Render(HtmlTextWriter writer)
        {
            if (this.Page != null)
            {
                this.Page.VerifyRenderingInServerForm(this);
            }
            base.Render(writer);
        }

        protected override void RenderBody(HtmlTextWriter writer)
        {
            base.RenderBodyTableBeginTag(writer);
            if (base.DesignMode)
            {
                base.RenderDesignerRegionBeginTag(writer, Orientation.Vertical);
            }
            CatalogPartCollection catalogParts = this.CatalogParts;
            if ((catalogParts != null) && (catalogParts.Count > 0))
            {
                bool firstCell = true;
                if (catalogParts.Count > 1)
                {
                    writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    firstCell = false;
                    this.RenderCatalogPartLinks(writer);
                    writer.RenderEndTag();
                    writer.RenderEndTag();
                }
                System.Web.UI.WebControls.WebParts.CatalogPartChrome catalogPartChrome = this.CatalogPartChrome;
                if (base.DesignMode)
                {
                    foreach (CatalogPart part in catalogParts)
                    {
                        this.RenderCatalogPart(writer, part, catalogPartChrome, ref firstCell);
                    }
                }
                else
                {
                    CatalogPart selectedCatalogPart = this.SelectedCatalogPart;
                    if (selectedCatalogPart != null)
                    {
                        this.RenderCatalogPart(writer, selectedCatalogPart, catalogPartChrome, ref firstCell);
                    }
                }
                writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                writer.AddStyleAttribute(HtmlTextWriterStyle.Padding, "0");
                writer.AddStyleAttribute(HtmlTextWriterStyle.Height, "100%");
                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                writer.RenderEndTag();
                writer.RenderEndTag();
            }
            else
            {
                this.RenderEmptyZoneText(writer);
            }
            if (base.DesignMode)
            {
                WebZone.RenderDesignerRegionEndTag(writer);
            }
            WebZone.RenderBodyTableEndTag(writer);
        }

        private void RenderCatalogPart(HtmlTextWriter writer, CatalogPart catalogPart, System.Web.UI.WebControls.WebParts.CatalogPartChrome chrome, ref bool firstCell)
        {
            writer.RenderBeginTag(HtmlTextWriterTag.Tr);
            if (!firstCell)
            {
                writer.AddStyleAttribute(HtmlTextWriterStyle.PaddingTop, "0");
            }
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            firstCell = false;
            chrome.RenderCatalogPart(writer, catalogPart);
            writer.RenderEndTag();
            writer.RenderEndTag();
        }

        protected virtual void RenderCatalogPartLinks(HtmlTextWriter writer)
        {
            this.RenderInstructionText(writer);
            CatalogPart selectedCatalogPart = this.SelectedCatalogPart;
            foreach (CatalogPart part2 in this.CatalogParts)
            {
                WebPartDescriptionCollection availableWebPartDescriptions = part2.GetAvailableWebPartDescriptions();
                int num = (availableWebPartDescriptions != null) ? availableWebPartDescriptions.Count : 0;
                string displayTitle = part2.DisplayTitle;
                string str2 = displayTitle + " (" + num.ToString(CultureInfo.CurrentCulture) + ")";
                if (part2 == selectedCatalogPart)
                {
                    Label label = new Label {
                        Text = str2,
                        Page = this.Page
                    };
                    label.ApplyStyle(this.SelectedPartLinkStyle);
                    label.RenderControl(writer);
                }
                else
                {
                    string eventArgument = "select" + '$' + part2.ID;
                    ZoneLinkButton button = new ZoneLinkButton(this, eventArgument) {
                        Text = str2,
                        ToolTip = System.Web.SR.GetString("CatalogZoneBase_SelectCatalogPart", new object[] { displayTitle }),
                        Page = this.Page
                    };
                    button.ApplyStyle(this.PartLinkStyle);
                    button.RenderControl(writer);
                }
                writer.WriteBreak();
            }
            writer.WriteBreak();
        }

        private void RenderEmptyZoneText(HtmlTextWriter writer)
        {
            string emptyZoneText = this.EmptyZoneText;
            if (!string.IsNullOrEmpty(emptyZoneText))
            {
                writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                writer.AddAttribute(HtmlTextWriterAttribute.Valign, "top");
                Style emptyZoneTextStyle = base.EmptyZoneTextStyle;
                if (!emptyZoneTextStyle.IsEmpty)
                {
                    emptyZoneTextStyle.AddAttributesToRender(writer, this);
                }
                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                writer.Write(emptyZoneText);
                writer.RenderEndTag();
                writer.RenderEndTag();
            }
        }

        protected override void RenderFooter(HtmlTextWriter writer)
        {
            writer.AddStyleAttribute(HtmlTextWriterStyle.Margin, "4px");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
            DropDownList list = new DropDownList {
                ClientIDMode = ClientIDMode.AutoID,
                ID = this.ZonesID
            };
            if (base.DesignMode)
            {
                list.Items.Add(System.Web.SR.GetString("Zone_SampleHeaderText"));
            }
            else if ((base.WebPartManager != null) && (base.WebPartManager.Zones != null))
            {
                foreach (WebPartZoneBase base2 in base.WebPartManager.Zones)
                {
                    if (base2.AllowLayoutChange)
                    {
                        ListItem item = new ListItem(base2.DisplayTitle, base2.ID);
                        if (string.Equals(base2.ID, this._selectedZoneID, StringComparison.OrdinalIgnoreCase))
                        {
                            item.Selected = true;
                        }
                        list.Items.Add(item);
                    }
                }
            }
            base.LabelStyle.AddAttributesToRender(writer, this);
            if (list.Items.Count > 0)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.For, list.ClientID);
            }
            writer.RenderBeginTag(HtmlTextWriterTag.Label);
            writer.Write(this.SelectTargetZoneText);
            writer.RenderEndTag();
            writer.Write("&nbsp;");
            list.ApplyStyle(base.EditUIStyle);
            if (list.Items.Count > 0)
            {
                list.RenderControl(writer);
            }
            writer.Write("&nbsp;");
            this.RenderVerbs(writer);
            writer.RenderEndTag();
        }

        private void RenderInstructionText(HtmlTextWriter writer)
        {
            string instructionText = this.InstructionText;
            if (!string.IsNullOrEmpty(instructionText))
            {
                Label label = new Label {
                    Text = instructionText,
                    Page = this.Page
                };
                label.ApplyStyle(base.InstructionTextStyle);
                label.RenderControl(writer);
                writer.WriteBreak();
                writer.WriteBreak();
            }
        }

        protected override void RenderVerbs(HtmlTextWriter writer)
        {
            int num = 0;
            bool enabled = false;
            CatalogPart selectedCatalogPart = this.SelectedCatalogPart;
            if (selectedCatalogPart != null)
            {
                WebPartDescriptionCollection availableWebPartDescriptions = selectedCatalogPart.GetAvailableWebPartDescriptions();
                num = (availableWebPartDescriptions != null) ? availableWebPartDescriptions.Count : 0;
            }
            if (num == 0)
            {
                enabled = this.AddVerb.Enabled;
                this.AddVerb.Enabled = false;
            }
            try
            {
                base.RenderVerbsInternal(writer, new WebPartVerb[] { this.AddVerb, this.CloseVerb });
            }
            finally
            {
                if (num == 0)
                {
                    this.AddVerb.Enabled = enabled;
                }
            }
        }

        protected internal override object SaveControlState()
        {
            object[] objArray = new object[2];
            objArray[0] = base.SaveControlState();
            if (!string.IsNullOrEmpty(this._selectedCatalogPartID))
            {
                objArray[1] = this._selectedCatalogPartID;
            }
            for (int i = 0; i < 2; i++)
            {
                if (objArray[i] != null)
                {
                    return objArray;
                }
            }
            return null;
        }

        protected override object SaveViewState()
        {
            object[] objArray = new object[] { base.SaveViewState(), (this._addVerb != null) ? ((IStateManager) this._addVerb).SaveViewState() : null, (this._closeVerb != null) ? ((IStateManager) this._closeVerb).SaveViewState() : null, (this._partLinkStyle != null) ? ((IStateManager) this._partLinkStyle).SaveViewState() : null, (this._selectedPartLinkStyle != null) ? ((IStateManager) this._selectedPartLinkStyle).SaveViewState() : null };
            for (int i = 0; i < 5; i++)
            {
                if (objArray[i] != null)
                {
                    return objArray;
                }
            }
            return null;
        }

        bool IPostBackDataHandler.LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
            return this.LoadPostData(postDataKey, postCollection);
        }

        void IPostBackDataHandler.RaisePostDataChangedEvent()
        {
        }

        protected override void TrackViewState()
        {
            base.TrackViewState();
            if (this._addVerb != null)
            {
                ((IStateManager) this._addVerb).TrackViewState();
            }
            if (this._closeVerb != null)
            {
                ((IStateManager) this._closeVerb).TrackViewState();
            }
            if (this._partLinkStyle != null)
            {
                ((IStateManager) this._partLinkStyle).TrackViewState();
            }
            if (this._selectedPartLinkStyle != null)
            {
                ((IStateManager) this._selectedPartLinkStyle).TrackViewState();
            }
        }

        [DefaultValue((string) null), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty), WebCategory("Verbs"), WebSysDescription("CatalogZoneBase_AddVerb")]
        public virtual WebPartVerb AddVerb
        {
            get
            {
                if (this._addVerb == null)
                {
                    this._addVerb = new WebPartCatalogAddVerb();
                    this._addVerb.EventArgument = "add";
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._addVerb).TrackViewState();
                    }
                }
                return this._addVerb;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public System.Web.UI.WebControls.WebParts.CatalogPartChrome CatalogPartChrome
        {
            get
            {
                if (this._catalogPartChrome == null)
                {
                    this._catalogPartChrome = this.CreateCatalogPartChrome();
                }
                return this._catalogPartChrome;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public CatalogPartCollection CatalogParts
        {
            get
            {
                if (this._catalogParts == null)
                {
                    CatalogPartCollection parts = this.CreateCatalogParts();
                    if (!base.DesignMode)
                    {
                        foreach (CatalogPart part in parts)
                        {
                            if (string.IsNullOrEmpty(part.ID))
                            {
                                throw new InvalidOperationException(System.Web.SR.GetString("CatalogZoneBase_NoCatalogPartID"));
                            }
                        }
                    }
                    this._catalogParts = parts;
                    this.EnsureChildControls();
                }
                return this._catalogParts;
            }
        }

        internal string CheckBoxName
        {
            get
            {
                return (this.UniqueID + '$' + "_checkbox");
            }
        }

        [DefaultValue((string) null), NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), WebCategory("Verbs"), WebSysDescription("CatalogZoneBase_CloseVerb")]
        public virtual WebPartVerb CloseVerb
        {
            get
            {
                if (this._closeVerb == null)
                {
                    this._closeVerb = new WebPartCatalogCloseVerb();
                    this._closeVerb.EventArgument = "close";
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._closeVerb).TrackViewState();
                    }
                }
                return this._closeVerb;
            }
        }

        [WebSysDefaultValue("CatalogZoneBase_DefaultEmptyZoneText")]
        public override string EmptyZoneText
        {
            get
            {
                string str = (string) this.ViewState["EmptyZoneText"];
                if (str != null)
                {
                    return str;
                }
                return System.Web.SR.GetString("CatalogZoneBase_DefaultEmptyZoneText");
            }
            set
            {
                this.ViewState["EmptyZoneText"] = value;
            }
        }

        [WebSysDefaultValue("CatalogZoneBase_HeaderText")]
        public override string HeaderText
        {
            get
            {
                string str = (string) this.ViewState["HeaderText"];
                if (str != null)
                {
                    return str;
                }
                return System.Web.SR.GetString("CatalogZoneBase_HeaderText");
            }
            set
            {
                this.ViewState["HeaderText"] = value;
            }
        }

        [WebSysDefaultValue("CatalogZoneBase_InstructionText")]
        public override string InstructionText
        {
            get
            {
                string str = (string) this.ViewState["InstructionText"];
                if (str != null)
                {
                    return str;
                }
                return System.Web.SR.GetString("CatalogZoneBase_InstructionText");
            }
            set
            {
                this.ViewState["InstructionText"] = value;
            }
        }

        [WebCategory("Styles"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), PersistenceMode(PersistenceMode.InnerProperty), DefaultValue((string) null), NotifyParentProperty(true), WebSysDescription("CatalogZoneBase_PartLinkStyle")]
        public Style PartLinkStyle
        {
            get
            {
                if (this._partLinkStyle == null)
                {
                    this._partLinkStyle = new Style();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._partLinkStyle).TrackViewState();
                    }
                }
                return this._partLinkStyle;
            }
        }

        private CatalogPart SelectedCatalogPart
        {
            get
            {
                CatalogPartCollection catalogParts = this.CatalogParts;
                if ((catalogParts == null) || (catalogParts.Count <= 0))
                {
                    return null;
                }
                if (string.IsNullOrEmpty(this._selectedCatalogPartID))
                {
                    return catalogParts[0];
                }
                return catalogParts[this._selectedCatalogPartID];
            }
        }

        [WebSysDescription("CatalogZoneBase_SelectedCatalogPartID"), Themeable(false), DefaultValue(""), WebCategory("Behavior")]
        public string SelectedCatalogPartID
        {
            get
            {
                if (!string.IsNullOrEmpty(this._selectedCatalogPartID))
                {
                    return this._selectedCatalogPartID;
                }
                if (!base.DesignMode)
                {
                    CatalogPartCollection catalogParts = this.CatalogParts;
                    if ((catalogParts != null) && (catalogParts.Count > 0))
                    {
                        return catalogParts[0].ID;
                    }
                }
                return string.Empty;
            }
            set
            {
                this._selectedCatalogPartID = value;
            }
        }

        [WebSysDescription("CatalogZoneBase_SelectedPartLinkStyle"), WebCategory("Styles"), DefaultValue((string) null), NotifyParentProperty(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), PersistenceMode(PersistenceMode.InnerProperty)]
        public Style SelectedPartLinkStyle
        {
            get
            {
                if (this._selectedPartLinkStyle == null)
                {
                    this._selectedPartLinkStyle = new Style();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._selectedPartLinkStyle).TrackViewState();
                    }
                }
                return this._selectedPartLinkStyle;
            }
        }

        [WebCategory("Behavior"), WebSysDescription("CatalogZoneBase_SelectTargetZoneText"), WebSysDefaultValue("CatalogZoneBase_DefaultSelectTargetZoneText"), Localizable(true)]
        public virtual string SelectTargetZoneText
        {
            get
            {
                string str = (string) this.ViewState["SelectTargetZoneText"];
                if (str != null)
                {
                    return str;
                }
                return System.Web.SR.GetString("CatalogZoneBase_DefaultSelectTargetZoneText");
            }
            set
            {
                this.ViewState["SelectTargetZoneText"] = value;
            }
        }

        [DefaultValue(true), WebSysDescription("CatalogZoneBase_ShowCatalogIcons"), WebCategory("Behavior")]
        public virtual bool ShowCatalogIcons
        {
            get
            {
                object obj2 = this.ViewState["ShowCatalogIcons"];
                return ((obj2 == null) || ((bool) obj2));
            }
            set
            {
                this.ViewState["ShowCatalogIcons"] = value;
            }
        }

        private string ZonesID
        {
            get
            {
                return (this.UniqueID + '$' + "_zones");
            }
        }
    }
}

