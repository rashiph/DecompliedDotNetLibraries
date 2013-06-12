namespace System.Web.UI.WebControls.WebParts
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    [Designer("System.Web.UI.Design.WebControls.WebParts.WebPartZoneBaseDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    public abstract class WebPartZoneBase : WebZone, IPostBackEventHandler, IWebPartMenuUser
    {
        private Color _borderColor;
        private System.Web.UI.WebControls.BorderStyle _borderStyle;
        private Unit _borderWidth;
        private WebPartVerb _closeVerb;
        private WebPartVerb _connectVerb;
        private WebPartVerb _deleteVerb;
        private WebPartVerb _editVerb;
        private WebPartVerb _exportVerb;
        private WebPartVerb _helpVerb;
        private WebPartMenu _menu;
        private Style _menuCheckImageStyle;
        private Style _menuLabelHoverStyle;
        private Style _menuLabelStyle;
        private WebPartMenuStyle _menuPopupStyle;
        private Style _menuVerbHoverStyle;
        private Style _menuVerbStyle;
        private WebPartVerb _minimizeVerb;
        private WebPartVerb _restoreVerb;
        private Style _selectedPartChromeStyle;
        private Style _titleBarVerbStyle;
        private WebPartVerbCollection _verbs;
        private System.Web.UI.WebControls.WebParts.WebPartChrome _webPartChrome;
        private const int baseIndex = 0;
        private const string closeEventArgument = "close";
        private const string closeEventArgumentWithSeparator = "close:";
        private const int closeVerbIndex = 2;
        private const string connectEventArgument = "connect";
        private const string connectEventArgumentWithSeparator = "connect:";
        private const int connectVerbIndex = 3;
        private const int controlStyleIndex = 0x10;
        private static readonly object CreateVerbsEvent = new object();
        private const string deleteEventArgument = "delete";
        private const string deleteEventArgumentWithSeparator = "delete:";
        private const int deleteVerbIndex = 4;
        private const string dragEventArgument = "Drag";
        private const string editEventArgument = "edit";
        private const string editEventArgumentWithSeparator = "edit:";
        private const int editVerbIndex = 5;
        internal const string EventArgumentSeparator = ":";
        private const char eventArgumentSeparatorChar = ':';
        private const int exportVerbIndex = 9;
        private const int helpVerbIndex = 6;
        private const int menuCheckImageStyleIndex = 13;
        private const int menuLabelHoverStyleIndex = 12;
        private const int menuLabelStyleIndex = 11;
        private const int menuPopupStyleIndex = 10;
        private const int menuVerbHoverStyleIndex = 15;
        private const int menuVerbStyleIndex = 14;
        private const string minimizeEventArgument = "minimize";
        private const string minimizeEventArgumentWithSeparator = "minimize:";
        private const int minimizeVerbIndex = 7;
        private const string partVerbEventArgument = "partverb";
        private const string partVerbEventArgumentWithSeparator = "partverb:";
        private const string restoreEventArgument = "restore";
        private const string restoreEventArgumentWithSeparator = "restore:";
        private const int restoreVerbIndex = 8;
        private const int selectedPartChromeStyleIndex = 1;
        private const int titleBarVerbStyleIndex = 0x11;
        private const int viewStateArrayLength = 0x12;
        private const string zoneVerbEventArgument = "zoneverb";
        private const string zoneVerbEventArgumentWithSeparator = "zoneverb:";

        [WebSysDescription("WebPartZoneBase_CreateVerbs"), WebCategory("Action")]
        public event WebPartVerbsEventHandler CreateVerbs
        {
            add
            {
                base.Events.AddHandler(CreateVerbsEvent, value);
            }
            remove
            {
                base.Events.RemoveHandler(CreateVerbsEvent, value);
            }
        }

        protected WebPartZoneBase()
        {
        }

        protected virtual void CloseWebPart(WebPart webPart)
        {
            if (webPart == null)
            {
                throw new ArgumentNullException("webPart");
            }
            if (((base.WebPartManager != null) && webPart.AllowClose) && this.AllowLayoutChange)
            {
                base.WebPartManager.CloseWebPart(webPart);
            }
        }

        protected virtual void ConnectWebPart(WebPart webPart)
        {
            if (webPart == null)
            {
                throw new ArgumentNullException("webPart");
            }
            if (((base.WebPartManager != null) && (base.WebPartManager.DisplayMode == WebPartManager.ConnectDisplayMode)) && ((webPart != base.WebPartManager.SelectedWebPart) && webPart.AllowConnect))
            {
                base.WebPartManager.BeginWebPartConnecting(webPart);
            }
        }

        protected internal override void CreateChildControls()
        {
            if (base.DesignMode)
            {
                this.Controls.Clear();
                foreach (WebPart part in this.GetInitialWebParts())
                {
                    this.Controls.Add(part);
                }
            }
        }

        protected override ControlCollection CreateControlCollection()
        {
            if (base.DesignMode)
            {
                return new ControlCollection(this);
            }
            return new EmptyControlCollection(this);
        }

        protected override Style CreateControlStyle()
        {
            return new Style { BorderColor = Color.Gray, BorderStyle = System.Web.UI.WebControls.BorderStyle.Solid, BorderWidth = 1 };
        }

        protected virtual System.Web.UI.WebControls.WebParts.WebPartChrome CreateWebPartChrome()
        {
            return new System.Web.UI.WebControls.WebParts.WebPartChrome(this, base.WebPartManager);
        }

        private void CreateZoneVerbs()
        {
            WebPartVerbsEventArgs e = new WebPartVerbsEventArgs();
            this.OnCreateVerbs(e);
            this._verbs = e.Verbs;
        }

        protected virtual void DeleteWebPart(WebPart webPart)
        {
            if (webPart == null)
            {
                throw new ArgumentNullException("webPart");
            }
            if ((base.WebPartManager != null) && this.AllowLayoutChange)
            {
                base.WebPartManager.DeleteWebPart(webPart);
            }
        }

        protected virtual void EditWebPart(WebPart webPart)
        {
            if (webPart == null)
            {
                throw new ArgumentNullException("webPart");
            }
            if (((base.WebPartManager != null) && (base.WebPartManager.DisplayMode == WebPartManager.EditDisplayMode)) && (webPart != base.WebPartManager.SelectedWebPart))
            {
                base.WebPartManager.BeginWebPartEditing(webPart);
            }
        }

        public override PartChromeType GetEffectiveChromeType(Part part)
        {
            PartChromeType effectiveChromeType = base.GetEffectiveChromeType(part);
            if ((base.WebPartManager != null) && base.WebPartManager.DisplayMode.AllowPageDesign)
            {
                switch (effectiveChromeType)
                {
                    case PartChromeType.None:
                        return PartChromeType.TitleOnly;

                    case PartChromeType.BorderOnly:
                        return PartChromeType.TitleAndBorder;
                }
            }
            return effectiveChromeType;
        }

        protected internal abstract WebPartCollection GetInitialWebParts();
        private bool IsDefaultVerbEvent(string[] eventArguments)
        {
            return (eventArguments.Length == 2);
        }

        private bool IsDragEvent(string[] eventArguments)
        {
            return ((eventArguments.Length == 3) && string.Equals(eventArguments[0], "Drag", StringComparison.OrdinalIgnoreCase));
        }

        private bool IsPartVerbEvent(string[] eventArguments)
        {
            return ((eventArguments.Length == 3) && string.Equals(eventArguments[0], "partverb", StringComparison.OrdinalIgnoreCase));
        }

        private bool IsZoneVerbEvent(string[] eventArguments)
        {
            return ((eventArguments.Length == 3) && string.Equals(eventArguments[0], "zoneverb", StringComparison.OrdinalIgnoreCase));
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
                if (objArray.Length != 0x12)
                {
                    throw new ArgumentException(System.Web.SR.GetString("ViewState_InvalidViewState"));
                }
                base.LoadViewState(objArray[0]);
                if (objArray[1] != null)
                {
                    ((IStateManager) this.SelectedPartChromeStyle).LoadViewState(objArray[1]);
                }
                if (objArray[2] != null)
                {
                    ((IStateManager) this.CloseVerb).LoadViewState(objArray[2]);
                }
                if (objArray[3] != null)
                {
                    ((IStateManager) this.ConnectVerb).LoadViewState(objArray[3]);
                }
                if (objArray[4] != null)
                {
                    ((IStateManager) this.DeleteVerb).LoadViewState(objArray[4]);
                }
                if (objArray[5] != null)
                {
                    ((IStateManager) this.EditVerb).LoadViewState(objArray[5]);
                }
                if (objArray[6] != null)
                {
                    ((IStateManager) this.HelpVerb).LoadViewState(objArray[6]);
                }
                if (objArray[7] != null)
                {
                    ((IStateManager) this.MinimizeVerb).LoadViewState(objArray[7]);
                }
                if (objArray[8] != null)
                {
                    ((IStateManager) this.RestoreVerb).LoadViewState(objArray[8]);
                }
                if (objArray[9] != null)
                {
                    ((IStateManager) this.ExportVerb).LoadViewState(objArray[9]);
                }
                if (objArray[10] != null)
                {
                    ((IStateManager) this.MenuPopupStyle).LoadViewState(objArray[10]);
                }
                if (objArray[11] != null)
                {
                    ((IStateManager) this.MenuLabelStyle).LoadViewState(objArray[11]);
                }
                if (objArray[12] != null)
                {
                    ((IStateManager) this.MenuLabelHoverStyle).LoadViewState(objArray[12]);
                }
                if (objArray[13] != null)
                {
                    ((IStateManager) this.MenuCheckImageStyle).LoadViewState(objArray[13]);
                }
                if (objArray[14] != null)
                {
                    ((IStateManager) this.MenuVerbStyle).LoadViewState(objArray[14]);
                }
                if (objArray[15] != null)
                {
                    ((IStateManager) this.MenuVerbHoverStyle).LoadViewState(objArray[15]);
                }
                if (objArray[0x10] != null)
                {
                    ((IStateManager) base.ControlStyle).LoadViewState(objArray[0x10]);
                }
                if (objArray[0x11] != null)
                {
                    ((IStateManager) this.TitleBarVerbStyle).LoadViewState(objArray[0x11]);
                }
            }
        }

        protected virtual void MinimizeWebPart(WebPart webPart)
        {
            if (webPart == null)
            {
                throw new ArgumentNullException("webPart");
            }
            if (((webPart.ChromeState == PartChromeState.Normal) && webPart.AllowMinimize) && this.AllowLayoutChange)
            {
                webPart.ChromeState = PartChromeState.Minimized;
            }
        }

        protected virtual void OnCreateVerbs(WebPartVerbsEventArgs e)
        {
            WebPartVerbsEventHandler handler = (WebPartVerbsEventHandler) base.Events[CreateVerbsEvent];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected internal override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            this.CreateZoneVerbs();
            this.WebPartChrome.PerformPreRender();
        }

        protected virtual void RaisePostBackEvent(string eventArgument)
        {
            if (!string.IsNullOrEmpty(eventArgument))
            {
                string[] eventArguments = eventArgument.Split(new char[] { ':' });
                if (!this.IsDragEvent(eventArguments))
                {
                    base.ValidateEvent(this.UniqueID, eventArgument);
                }
                if (base.WebPartManager != null)
                {
                    WebPartCollection webParts = base.WebPartManager.WebParts;
                    if (this.IsDefaultVerbEvent(eventArguments))
                    {
                        string a = eventArguments[0];
                        string str2 = eventArguments[1];
                        WebPart webPart = webParts[str2];
                        if ((webPart != null) && !webPart.IsClosed)
                        {
                            if (!string.Equals(a, "close", StringComparison.OrdinalIgnoreCase))
                            {
                                if (!string.Equals(a, "connect", StringComparison.OrdinalIgnoreCase))
                                {
                                    if (!string.Equals(a, "delete", StringComparison.OrdinalIgnoreCase))
                                    {
                                        if (!string.Equals(a, "edit", StringComparison.OrdinalIgnoreCase))
                                        {
                                            if (!string.Equals(a, "minimize", StringComparison.OrdinalIgnoreCase))
                                            {
                                                if ((string.Equals(a, "restore", StringComparison.OrdinalIgnoreCase) && this.RestoreVerb.Visible) && this.RestoreVerb.Enabled)
                                                {
                                                    this.RestoreWebPart(webPart);
                                                }
                                            }
                                            else if (this.MinimizeVerb.Visible && this.MinimizeVerb.Enabled)
                                            {
                                                this.MinimizeWebPart(webPart);
                                            }
                                        }
                                        else if (this.EditVerb.Visible && this.EditVerb.Enabled)
                                        {
                                            this.EditWebPart(webPart);
                                        }
                                    }
                                    else if (this.DeleteVerb.Visible && this.DeleteVerb.Enabled)
                                    {
                                        this.DeleteWebPart(webPart);
                                    }
                                }
                                else if (this.ConnectVerb.Visible && this.ConnectVerb.Enabled)
                                {
                                    this.ConnectWebPart(webPart);
                                }
                            }
                            else if (this.CloseVerb.Visible && this.CloseVerb.Enabled)
                            {
                                this.CloseWebPart(webPart);
                            }
                        }
                    }
                    else if (this.IsDragEvent(eventArguments))
                    {
                        string str3 = eventArguments[1];
                        string str4 = null;
                        if (str3.StartsWith("WebPart_", StringComparison.Ordinal))
                        {
                            str4 = str3.Substring("WebPart_".Length);
                        }
                        int zoneIndex = int.Parse(eventArguments[2], CultureInfo.InvariantCulture);
                        WebPart part2 = webParts[str4];
                        if ((part2 != null) && !part2.IsClosed)
                        {
                            if (this.WebParts.Contains(part2) && (part2.ZoneIndex < zoneIndex))
                            {
                                zoneIndex--;
                            }
                            WebPartZoneBase zone = part2.Zone;
                            if (((this.AllowLayoutChange && base.WebPartManager.DisplayMode.AllowPageDesign) && ((zone != null) && zone.AllowLayoutChange)) && (part2.AllowZoneChange || (zone == this)))
                            {
                                base.WebPartManager.MoveWebPart(part2, this, zoneIndex);
                            }
                        }
                    }
                    else if (this.IsPartVerbEvent(eventArguments))
                    {
                        string str5 = eventArguments[1];
                        string str6 = eventArguments[2];
                        WebPart part3 = webParts[str6];
                        if ((part3 != null) && !part3.IsClosed)
                        {
                            WebPartVerb sender = part3.Verbs[str5];
                            if (((sender != null) && sender.Visible) && sender.Enabled)
                            {
                                sender.ServerClickHandler(sender, new WebPartEventArgs(part3));
                            }
                        }
                    }
                    else if (this.IsZoneVerbEvent(eventArguments))
                    {
                        this.CreateZoneVerbs();
                        string str7 = eventArguments[1];
                        string str8 = eventArguments[2];
                        WebPart part4 = webParts[str8];
                        if ((part4 != null) && !part4.IsClosed)
                        {
                            WebPartVerb verb2 = this._verbs[str7];
                            if (((verb2 != null) && verb2.Visible) && verb2.Enabled)
                            {
                                verb2.ServerClickHandler(verb2, new WebPartEventArgs(part4));
                            }
                        }
                    }
                }
            }
        }

        protected internal override void Render(HtmlTextWriter writer)
        {
            if (this.Page != null)
            {
                this.Page.VerifyRenderingInServerForm(this);
            }
            this._borderColor = this.BorderColor;
            this._borderStyle = this.BorderStyle;
            this._borderWidth = this.BorderWidth;
            if (base.ControlStyleCreated)
            {
                this.BorderColor = Color.Empty;
                this.BorderStyle = System.Web.UI.WebControls.BorderStyle.NotSet;
                this.BorderWidth = Unit.Empty;
            }
            base.Render(writer);
            if (base.ControlStyleCreated)
            {
                this.BorderColor = this._borderColor;
                this.BorderStyle = this._borderStyle;
                this.BorderWidth = this._borderWidth;
            }
        }

        protected override void RenderBody(HtmlTextWriter writer)
        {
            Orientation layoutOrientation = this.LayoutOrientation;
            if ((base.DesignMode || ((base.WebPartManager != null) && base.WebPartManager.DisplayMode.AllowPageDesign)) && (((this._borderColor != Color.Empty) || (this._borderStyle != System.Web.UI.WebControls.BorderStyle.NotSet)) || (this._borderWidth != Unit.Empty)))
            {
                new Style { BorderColor = this._borderColor, BorderStyle = this._borderStyle, BorderWidth = this._borderWidth }.AddAttributesToRender(writer, this);
            }
            base.RenderBodyTableBeginTag(writer);
            if (base.DesignMode)
            {
                base.RenderDesignerRegionBeginTag(writer, layoutOrientation);
            }
            if (layoutOrientation == Orientation.Horizontal)
            {
                writer.RenderBeginTag(HtmlTextWriterTag.Tr);
            }
            bool dragDropEnabled = this.DragDropEnabled;
            if (dragDropEnabled)
            {
                this.RenderDropCue(writer);
            }
            WebPartCollection webParts = this.WebParts;
            if ((webParts == null) || (webParts.Count == 0))
            {
                this.RenderEmptyZoneBody(writer);
            }
            else
            {
                System.Web.UI.WebControls.WebParts.WebPartChrome webPartChrome = this.WebPartChrome;
                foreach (WebPart part in webParts)
                {
                    if (part.ChromeState == PartChromeState.Minimized)
                    {
                        switch (this.GetEffectiveChromeType(part))
                        {
                            case PartChromeType.None:
                            case PartChromeType.BorderOnly:
                                writer.AddStyleAttribute(HtmlTextWriterStyle.Display, "none");
                                break;
                        }
                    }
                    if (layoutOrientation == Orientation.Vertical)
                    {
                        writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                    }
                    else
                    {
                        writer.AddStyleAttribute(HtmlTextWriterStyle.Height, "100%");
                        writer.AddAttribute(HtmlTextWriterAttribute.Valign, "top");
                    }
                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    webPartChrome.RenderWebPart(writer, part);
                    writer.RenderEndTag();
                    if (layoutOrientation == Orientation.Vertical)
                    {
                        writer.RenderEndTag();
                    }
                    if (dragDropEnabled)
                    {
                        this.RenderDropCue(writer);
                    }
                }
                if (layoutOrientation == Orientation.Vertical)
                {
                    writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                    writer.AddStyleAttribute(HtmlTextWriterStyle.Padding, "0");
                    writer.AddStyleAttribute(HtmlTextWriterStyle.Height, "100%");
                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    writer.RenderEndTag();
                    writer.RenderEndTag();
                }
                else
                {
                    writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "100%");
                    writer.AddStyleAttribute(HtmlTextWriterStyle.Padding, "0");
                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    writer.RenderEndTag();
                }
            }
            if (layoutOrientation == Orientation.Horizontal)
            {
                writer.RenderEndTag();
            }
            if (base.DesignMode)
            {
                WebZone.RenderDesignerRegionEndTag(writer);
            }
            WebZone.RenderBodyTableEndTag(writer);
        }

        protected virtual void RenderDropCue(HtmlTextWriter writer)
        {
            if (this.LayoutOrientation == Orientation.Vertical)
            {
                writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                writer.AddStyleAttribute(HtmlTextWriterStyle.PaddingTop, "1");
                writer.AddStyleAttribute(HtmlTextWriterStyle.PaddingBottom, "1");
                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                this.RenderDropCueIBar(writer, Orientation.Horizontal);
                writer.RenderEndTag();
                writer.RenderEndTag();
            }
            else
            {
                writer.AddStyleAttribute(HtmlTextWriterStyle.PaddingLeft, "1");
                writer.AddStyleAttribute(HtmlTextWriterStyle.PaddingRight, "1");
                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                this.RenderDropCueIBar(writer, Orientation.Vertical);
                writer.RenderEndTag();
            }
        }

        private void RenderDropCueIBar(HtmlTextWriter writer, Orientation orientation)
        {
            string str = ColorTranslator.ToHtml(this.DragHighlightColor);
            string str2 = "solid 3px " + str;
            writer.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
            writer.AddAttribute(HtmlTextWriterAttribute.Cellpadding, "0");
            writer.AddAttribute(HtmlTextWriterAttribute.Border, "0");
            if (orientation == Orientation.Horizontal)
            {
                writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "100%");
                writer.AddStyleAttribute("border-left", str2);
                writer.AddStyleAttribute("border-right", str2);
            }
            else
            {
                writer.AddStyleAttribute(HtmlTextWriterStyle.Height, "100%");
                writer.AddStyleAttribute("border-top", str2);
                writer.AddStyleAttribute("border-bottom", str2);
            }
            writer.AddStyleAttribute(HtmlTextWriterStyle.Visibility, "hidden");
            writer.RenderBeginTag(HtmlTextWriterTag.Table);
            writer.RenderBeginTag(HtmlTextWriterTag.Tr);
            if (orientation == Orientation.Vertical)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Align, "center");
            }
            writer.AddStyleAttribute(HtmlTextWriterStyle.FontSize, "0px");
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            if (orientation == Orientation.Horizontal)
            {
                writer.AddStyleAttribute(HtmlTextWriterStyle.Margin, "2px 0px 2px 0px");
                writer.AddStyleAttribute(HtmlTextWriterStyle.Height, "2px");
                writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "100%");
            }
            else
            {
                writer.AddStyleAttribute(HtmlTextWriterStyle.Margin, "0px 2px 0px 2px");
                writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "2px");
                writer.AddStyleAttribute(HtmlTextWriterStyle.Height, "100%");
            }
            writer.AddStyleAttribute(HtmlTextWriterStyle.BackgroundColor, str);
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
            writer.RenderEndTag();
            writer.RenderEndTag();
            writer.RenderEndTag();
            writer.RenderEndTag();
        }

        private void RenderEmptyZoneBody(HtmlTextWriter writer)
        {
            bool flag = this.LayoutOrientation == Orientation.Vertical;
            bool flag2 = !flag;
            string emptyZoneText = this.EmptyZoneText;
            bool flag3 = ((!base.DesignMode && this.AllowLayoutChange) && ((base.WebPartManager != null) && base.WebPartManager.DisplayMode.AllowPageDesign)) && !string.IsNullOrEmpty(emptyZoneText);
            if (flag)
            {
                writer.RenderBeginTag(HtmlTextWriterTag.Tr);
            }
            if (flag3)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Valign, "top");
            }
            if (flag2)
            {
                writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "100%");
            }
            else
            {
                writer.AddStyleAttribute(HtmlTextWriterStyle.Height, "100%");
            }
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            if (flag3)
            {
                Style emptyZoneTextStyle = base.EmptyZoneTextStyle;
                if (!emptyZoneTextStyle.IsEmpty)
                {
                    emptyZoneTextStyle.AddAttributesToRender(writer, this);
                }
                writer.RenderBeginTag(HtmlTextWriterTag.Div);
                writer.Write(emptyZoneText);
                writer.RenderEndTag();
            }
            writer.RenderEndTag();
            if (flag)
            {
                writer.RenderEndTag();
            }
            if (flag3 && this.DragDropEnabled)
            {
                this.RenderDropCue(writer);
            }
        }

        protected override void RenderHeader(HtmlTextWriter writer)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
            writer.AddAttribute(HtmlTextWriterAttribute.Cellpadding, "2");
            writer.AddAttribute(HtmlTextWriterAttribute.Border, "0");
            writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "100%");
            TitleStyle headerStyle = base.HeaderStyle;
            if (!headerStyle.IsEmpty)
            {
                Style style2 = new Style();
                if (!headerStyle.ForeColor.IsEmpty)
                {
                    style2.ForeColor = headerStyle.ForeColor;
                }
                style2.Font.CopyFrom(headerStyle.Font);
                if (!headerStyle.Font.Size.IsEmpty)
                {
                    style2.Font.Size = new FontUnit(new Unit(100.0, UnitType.Percentage));
                }
                if (!style2.IsEmpty)
                {
                    style2.AddAttributesToRender(writer, this);
                }
            }
            writer.RenderBeginTag(HtmlTextWriterTag.Table);
            writer.RenderBeginTag(HtmlTextWriterTag.Tr);
            HorizontalAlign horizontalAlign = headerStyle.HorizontalAlign;
            if (horizontalAlign != HorizontalAlign.NotSet)
            {
                TypeConverter converter = TypeDescriptor.GetConverter(typeof(HorizontalAlign));
                writer.AddAttribute(HtmlTextWriterAttribute.Align, converter.ConvertToString(horizontalAlign));
            }
            writer.AddStyleAttribute(HtmlTextWriterStyle.WhiteSpace, "nowrap");
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            writer.Write(this.DisplayTitle);
            writer.RenderEndTag();
            writer.RenderEndTag();
            writer.RenderEndTag();
        }

        protected virtual void RestoreWebPart(WebPart webPart)
        {
            if (webPart == null)
            {
                throw new ArgumentNullException("webPart");
            }
            if ((webPart.ChromeState == PartChromeState.Minimized) && this.AllowLayoutChange)
            {
                webPart.ChromeState = PartChromeState.Normal;
            }
        }

        protected override object SaveViewState()
        {
            object[] objArray = new object[] { 
                base.SaveViewState(), (this._selectedPartChromeStyle != null) ? ((IStateManager) this._selectedPartChromeStyle).SaveViewState() : null, (this._closeVerb != null) ? ((IStateManager) this._closeVerb).SaveViewState() : null, (this._connectVerb != null) ? ((IStateManager) this._connectVerb).SaveViewState() : null, (this._deleteVerb != null) ? ((IStateManager) this._deleteVerb).SaveViewState() : null, (this._editVerb != null) ? ((IStateManager) this._editVerb).SaveViewState() : null, (this._helpVerb != null) ? ((IStateManager) this._helpVerb).SaveViewState() : null, (this._minimizeVerb != null) ? ((IStateManager) this._minimizeVerb).SaveViewState() : null, (this._restoreVerb != null) ? ((IStateManager) this._restoreVerb).SaveViewState() : null, (this._exportVerb != null) ? ((IStateManager) this._exportVerb).SaveViewState() : null, (this._menuPopupStyle != null) ? ((IStateManager) this._menuPopupStyle).SaveViewState() : null, (this._menuLabelStyle != null) ? ((IStateManager) this._menuLabelStyle).SaveViewState() : null, (this._menuLabelHoverStyle != null) ? ((IStateManager) this._menuLabelHoverStyle).SaveViewState() : null, (this._menuCheckImageStyle != null) ? ((IStateManager) this._menuCheckImageStyle).SaveViewState() : null, (this._menuVerbStyle != null) ? ((IStateManager) this._menuVerbStyle).SaveViewState() : null, (this._menuVerbHoverStyle != null) ? ((IStateManager) this._menuVerbHoverStyle).SaveViewState() : null, 
                base.ControlStyleCreated ? ((IStateManager) base.ControlStyle).SaveViewState() : null, (this._titleBarVerbStyle != null) ? ((IStateManager) this._titleBarVerbStyle).SaveViewState() : null
             };
            for (int i = 0; i < 0x12; i++)
            {
                if (objArray[i] != null)
                {
                    return objArray;
                }
            }
            return null;
        }

        void IPostBackEventHandler.RaisePostBackEvent(string eventArgument)
        {
            this.RaisePostBackEvent(eventArgument);
        }

        void IWebPartMenuUser.OnBeginRender(HtmlTextWriter writer)
        {
        }

        void IWebPartMenuUser.OnEndRender(HtmlTextWriter writer)
        {
        }

        protected override void TrackViewState()
        {
            base.TrackViewState();
            if (this._selectedPartChromeStyle != null)
            {
                ((IStateManager) this._selectedPartChromeStyle).TrackViewState();
            }
            if (this._closeVerb != null)
            {
                ((IStateManager) this._closeVerb).TrackViewState();
            }
            if (this._connectVerb != null)
            {
                ((IStateManager) this._connectVerb).TrackViewState();
            }
            if (this._deleteVerb != null)
            {
                ((IStateManager) this._deleteVerb).TrackViewState();
            }
            if (this._editVerb != null)
            {
                ((IStateManager) this._editVerb).TrackViewState();
            }
            if (this._helpVerb != null)
            {
                ((IStateManager) this._helpVerb).TrackViewState();
            }
            if (this._minimizeVerb != null)
            {
                ((IStateManager) this._minimizeVerb).TrackViewState();
            }
            if (this._restoreVerb != null)
            {
                ((IStateManager) this._restoreVerb).TrackViewState();
            }
            if (this._exportVerb != null)
            {
                ((IStateManager) this._exportVerb).TrackViewState();
            }
            if (this._menuPopupStyle != null)
            {
                ((IStateManager) this._menuPopupStyle).TrackViewState();
            }
            if (this._menuLabelStyle != null)
            {
                ((IStateManager) this._menuLabelStyle).TrackViewState();
            }
            if (this._menuLabelHoverStyle != null)
            {
                ((IStateManager) this._menuLabelHoverStyle).TrackViewState();
            }
            if (this._menuCheckImageStyle != null)
            {
                ((IStateManager) this._menuCheckImageStyle).TrackViewState();
            }
            if (this._menuVerbStyle != null)
            {
                ((IStateManager) this._menuVerbStyle).TrackViewState();
            }
            if (this._menuVerbHoverStyle != null)
            {
                ((IStateManager) this._menuVerbHoverStyle).TrackViewState();
            }
            if (base.ControlStyleCreated)
            {
                ((IStateManager) base.ControlStyle).TrackViewState();
            }
            if (this._titleBarVerbStyle != null)
            {
                ((IStateManager) this._titleBarVerbStyle).TrackViewState();
            }
        }

        internal WebPartVerbCollection VerbsForWebPart(WebPart webPart)
        {
            WebPartVerbCollection verbs = new WebPartVerbCollection();
            WebPartVerbCollection verbs2 = webPart.Verbs;
            if (verbs2 != null)
            {
                foreach (WebPartVerb verb in verbs2)
                {
                    if (verb.ServerClickHandler != null)
                    {
                        verb.SetEventArgumentPrefix("partverb:");
                    }
                    verbs.Add(verb);
                }
            }
            if (this._verbs != null)
            {
                foreach (WebPartVerb verb2 in this._verbs)
                {
                    if (verb2.ServerClickHandler != null)
                    {
                        verb2.SetEventArgumentPrefix("zoneverb:");
                    }
                    verbs.Add(verb2);
                }
            }
            WebPartVerb minimizeVerb = this.MinimizeVerb;
            minimizeVerb.SetEventArgumentPrefix("minimize:");
            verbs.Add(minimizeVerb);
            WebPartVerb restoreVerb = this.RestoreVerb;
            restoreVerb.SetEventArgumentPrefix("restore:");
            verbs.Add(restoreVerb);
            WebPartVerb closeVerb = this.CloseVerb;
            closeVerb.SetEventArgumentPrefix("close:");
            verbs.Add(closeVerb);
            WebPartVerb deleteVerb = this.DeleteVerb;
            deleteVerb.SetEventArgumentPrefix("delete:");
            verbs.Add(deleteVerb);
            WebPartVerb editVerb = this.EditVerb;
            editVerb.SetEventArgumentPrefix("edit:");
            verbs.Add(editVerb);
            WebPartVerb connectVerb = this.ConnectVerb;
            connectVerb.SetEventArgumentPrefix("connect:");
            verbs.Add(connectVerb);
            verbs.Add(this.ExportVerb);
            verbs.Add(this.HelpVerb);
            return verbs;
        }

        [DefaultValue(true), Themeable(false), WebCategory("Behavior"), WebSysDescription("WebPartZoneBase_AllowLayoutChange")]
        public virtual bool AllowLayoutChange
        {
            get
            {
                object obj2 = this.ViewState["AllowLayoutChange"];
                return ((obj2 == null) || ((bool) obj2));
            }
            set
            {
                this.ViewState["AllowLayoutChange"] = value;
            }
        }

        [DefaultValue(typeof(Color), "Gray")]
        public override Color BorderColor
        {
            get
            {
                if (!base.ControlStyleCreated)
                {
                    return Color.Gray;
                }
                return base.BorderColor;
            }
            set
            {
                base.BorderColor = value;
            }
        }

        [DefaultValue(4)]
        public override System.Web.UI.WebControls.BorderStyle BorderStyle
        {
            get
            {
                if (!base.ControlStyleCreated)
                {
                    return System.Web.UI.WebControls.BorderStyle.Solid;
                }
                return base.BorderStyle;
            }
            set
            {
                base.BorderStyle = value;
            }
        }

        [DefaultValue(typeof(Unit), "1")]
        public override Unit BorderWidth
        {
            get
            {
                if (!base.ControlStyleCreated)
                {
                    return 1;
                }
                return base.BorderWidth;
            }
            set
            {
                base.BorderWidth = value;
            }
        }

        [WebSysDescription("WebPartZoneBase_CloseVerb"), WebCategory("Verbs"), PersistenceMode(PersistenceMode.InnerProperty), DefaultValue((string) null), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true)]
        public virtual WebPartVerb CloseVerb
        {
            get
            {
                if (this._closeVerb == null)
                {
                    this._closeVerb = new WebPartCloseVerb();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._closeVerb).TrackViewState();
                    }
                }
                return this._closeVerb;
            }
        }

        [NotifyParentProperty(true), WebCategory("Verbs"), WebSysDescription("WebPartZoneBase_ConnectVerb"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), DefaultValue((string) null), PersistenceMode(PersistenceMode.InnerProperty)]
        public virtual WebPartVerb ConnectVerb
        {
            get
            {
                if (this._connectVerb == null)
                {
                    this._connectVerb = new WebPartConnectVerb();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._connectVerb).TrackViewState();
                    }
                }
                return this._connectVerb;
            }
        }

        [DefaultValue((string) null), PersistenceMode(PersistenceMode.InnerProperty), WebSysDescription("WebPartZoneBase_DeleteVerb"), WebCategory("Verbs"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true)]
        public virtual WebPartVerb DeleteVerb
        {
            get
            {
                if (this._deleteVerb == null)
                {
                    this._deleteVerb = new WebPartDeleteVerb();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._deleteVerb).TrackViewState();
                    }
                }
                return this._deleteVerb;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public virtual string DisplayTitle
        {
            get
            {
                string headerText = this.HeaderText;
                if (!string.IsNullOrEmpty(headerText))
                {
                    return headerText;
                }
                string iD = this.ID;
                if (!string.IsNullOrEmpty(iD))
                {
                    return iD;
                }
                int num = 1;
                if (base.WebPartManager != null)
                {
                    num = base.WebPartManager.Zones.IndexOf(this) + 1;
                }
                return System.Web.SR.GetString("WebPartZoneBase_DisplayTitleFallback", new object[] { num.ToString(CultureInfo.CurrentCulture) });
            }
        }

        protected internal bool DragDropEnabled
        {
            get
            {
                return (((!base.DesignMode && base.RenderClientScript) && (this.AllowLayoutChange && (base.WebPartManager != null))) && base.WebPartManager.DisplayMode.AllowPageDesign);
            }
        }

        [DefaultValue(typeof(Color), "Blue"), WebCategory("Appearance"), WebSysDescription("WebPartZoneBase_DragHighlightColor"), TypeConverter(typeof(WebColorConverter))]
        public virtual Color DragHighlightColor
        {
            get
            {
                object obj2 = this.ViewState["DragHighlightColor"];
                if (obj2 != null)
                {
                    Color color = (Color) obj2;
                    if (!color.IsEmpty)
                    {
                        return color;
                    }
                }
                return Color.Blue;
            }
            set
            {
                this.ViewState["DragHighlightColor"] = value;
            }
        }

        [WebCategory("Verbs"), PersistenceMode(PersistenceMode.InnerProperty), WebSysDescription("WebPartZoneBase_EditVerb"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), DefaultValue((string) null), NotifyParentProperty(true)]
        public virtual WebPartVerb EditVerb
        {
            get
            {
                if (this._editVerb == null)
                {
                    this._editVerb = new WebPartEditVerb();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._editVerb).TrackViewState();
                    }
                }
                return this._editVerb;
            }
        }

        [WebSysDefaultValue("WebPartZoneBase_DefaultEmptyZoneText")]
        public override string EmptyZoneText
        {
            get
            {
                string str = (string) this.ViewState["EmptyZoneText"];
                if (str != null)
                {
                    return str;
                }
                return System.Web.SR.GetString("WebPartZoneBase_DefaultEmptyZoneText");
            }
            set
            {
                this.ViewState["EmptyZoneText"] = value;
            }
        }

        [DefaultValue((string) null), WebCategory("Verbs"), WebSysDescription("WebPartZoneBase_ExportVerb"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty)]
        public virtual WebPartVerb ExportVerb
        {
            get
            {
                if (this._exportVerb == null)
                {
                    this._exportVerb = new WebPartExportVerb();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._exportVerb).TrackViewState();
                    }
                }
                return this._exportVerb;
            }
        }

        protected override bool HasFooter
        {
            get
            {
                return false;
            }
        }

        protected override bool HasHeader
        {
            get
            {
                bool allowPageDesign = false;
                if (base.DesignMode)
                {
                    return true;
                }
                if (base.WebPartManager != null)
                {
                    allowPageDesign = base.WebPartManager.DisplayMode.AllowPageDesign;
                }
                return allowPageDesign;
            }
        }

        [WebCategory("Verbs"), WebSysDescription("WebPartZoneBase_HelpVerb"), DefaultValue((string) null), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty)]
        public virtual WebPartVerb HelpVerb
        {
            get
            {
                if (this._helpVerb == null)
                {
                    this._helpVerb = new WebPartHelpVerb();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._helpVerb).TrackViewState();
                    }
                }
                return this._helpVerb;
            }
        }

        [DefaultValue(1), WebSysDescription("WebPartZoneBase_LayoutOrientation"), WebCategory("Layout")]
        public virtual Orientation LayoutOrientation
        {
            get
            {
                object obj2 = this.ViewState["LayoutOrientation"];
                if (obj2 == null)
                {
                    return Orientation.Vertical;
                }
                return (Orientation) ((int) obj2);
            }
            set
            {
                if ((value < Orientation.Horizontal) || (value > Orientation.Vertical))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.ViewState["LayoutOrientation"] = (int) value;
            }
        }

        internal WebPartMenu Menu
        {
            get
            {
                if (this._menu == null)
                {
                    this._menu = new WebPartMenu(this);
                }
                return this._menu;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content), DefaultValue((string) null), WebSysDescription("WebPartZoneBase_MenuCheckImageStyle"), NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty), WebCategory("Styles")]
        public Style MenuCheckImageStyle
        {
            get
            {
                if (this._menuCheckImageStyle == null)
                {
                    this._menuCheckImageStyle = new Style();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._menuCheckImageStyle).TrackViewState();
                    }
                }
                return this._menuCheckImageStyle;
            }
        }

        [WebSysDescription("WebPartZoneBase_MenuCheckImageUrl"), DefaultValue(""), Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), UrlProperty, WebCategory("Appearance")]
        public virtual string MenuCheckImageUrl
        {
            get
            {
                string str = (string) this.ViewState["MenuCheckImageUrl"];
                if (str != null)
                {
                    return str;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["MenuCheckImageUrl"] = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content), DefaultValue((string) null), NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty), WebCategory("Styles"), WebSysDescription("WebPartZoneBase_MenuLabelHoverStyle")]
        public Style MenuLabelHoverStyle
        {
            get
            {
                if (this._menuLabelHoverStyle == null)
                {
                    this._menuLabelHoverStyle = new Style();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._menuLabelHoverStyle).TrackViewState();
                    }
                }
                return this._menuLabelHoverStyle;
            }
        }

        [PersistenceMode(PersistenceMode.InnerProperty), DefaultValue((string) null), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true), WebCategory("Styles"), WebSysDescription("WebPartZoneBase_MenuLabelStyle")]
        public Style MenuLabelStyle
        {
            get
            {
                if (this._menuLabelStyle == null)
                {
                    this._menuLabelStyle = new Style();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._menuLabelStyle).TrackViewState();
                    }
                }
                return this._menuLabelStyle;
            }
        }

        [DefaultValue(""), WebSysDescription("WebPartZoneBase_MenuLabelText"), Localizable(true), WebCategory("Appearance")]
        public virtual string MenuLabelText
        {
            get
            {
                string str = (string) this.ViewState["MenuLabelText"];
                if (str != null)
                {
                    return str;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["MenuLabelText"] = value;
            }
        }

        [DefaultValue(""), WebSysDescription("WebPartZoneBase_MenuPopupImageUrl"), Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), UrlProperty, WebCategory("Appearance")]
        public virtual string MenuPopupImageUrl
        {
            get
            {
                string str = (string) this.ViewState["MenuPopupImageUrl"];
                if (str != null)
                {
                    return str;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["MenuPopupImageUrl"] = value;
            }
        }

        [WebSysDescription("WebPartZoneBase_MenuPopupStyle"), DefaultValue((string) null), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty), WebCategory("Styles")]
        public WebPartMenuStyle MenuPopupStyle
        {
            get
            {
                if (this._menuPopupStyle == null)
                {
                    this._menuPopupStyle = new WebPartMenuStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._menuPopupStyle).TrackViewState();
                    }
                }
                return this._menuPopupStyle;
            }
        }

        [DefaultValue((string) null), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty), WebCategory("Styles"), WebSysDescription("WebPartZoneBase_MenuVerbHoverStyle")]
        public Style MenuVerbHoverStyle
        {
            get
            {
                if (this._menuVerbHoverStyle == null)
                {
                    this._menuVerbHoverStyle = new Style();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._menuVerbHoverStyle).TrackViewState();
                    }
                }
                return this._menuVerbHoverStyle;
            }
        }

        [DefaultValue((string) null), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty), WebCategory("Styles"), WebSysDescription("WebPartZoneBase_MenuVerbStyle")]
        public Style MenuVerbStyle
        {
            get
            {
                if (this._menuVerbStyle == null)
                {
                    this._menuVerbStyle = new Style();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._menuVerbStyle).TrackViewState();
                    }
                }
                return this._menuVerbStyle;
            }
        }

        [DefaultValue((string) null), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty), WebCategory("Verbs"), WebSysDescription("WebPartZoneBase_MinimizeVerb")]
        public virtual WebPartVerb MinimizeVerb
        {
            get
            {
                if (this._minimizeVerb == null)
                {
                    this._minimizeVerb = new WebPartMinimizeVerb();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._minimizeVerb).TrackViewState();
                    }
                }
                return this._minimizeVerb;
            }
        }

        [WebSysDescription("WebPartZoneBase_RestoreVerb"), DefaultValue((string) null), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty), WebCategory("Verbs")]
        public virtual WebPartVerb RestoreVerb
        {
            get
            {
                if (this._restoreVerb == null)
                {
                    this._restoreVerb = new WebPartRestoreVerb();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._restoreVerb).TrackViewState();
                    }
                }
                return this._restoreVerb;
            }
        }

        [PersistenceMode(PersistenceMode.InnerProperty), DefaultValue((string) null), NotifyParentProperty(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), WebCategory("WebPart"), WebSysDescription("WebPartZoneBase_SelectedPartChromeStyle")]
        public Style SelectedPartChromeStyle
        {
            get
            {
                if (this._selectedPartChromeStyle == null)
                {
                    this._selectedPartChromeStyle = new Style();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._selectedPartChromeStyle).TrackViewState();
                    }
                }
                return this._selectedPartChromeStyle;
            }
        }

        [WebSysDescription("WebPartZoneBase_ShowTitleIcons"), DefaultValue(true), WebCategory("WebPart")]
        public virtual bool ShowTitleIcons
        {
            get
            {
                object obj2 = this.ViewState["ShowTitleIcons"];
                return ((obj2 == null) || ((bool) obj2));
            }
            set
            {
                this.ViewState["ShowTitleIcons"] = value;
            }
        }

        Style IWebPartMenuUser.CheckImageStyle
        {
            get
            {
                return this._menuCheckImageStyle;
            }
        }

        string IWebPartMenuUser.CheckImageUrl
        {
            get
            {
                string menuCheckImageUrl = this.MenuCheckImageUrl;
                if (!string.IsNullOrEmpty(menuCheckImageUrl))
                {
                    menuCheckImageUrl = base.ResolveClientUrl(menuCheckImageUrl);
                }
                return menuCheckImageUrl;
            }
        }

        string IWebPartMenuUser.ClientID
        {
            get
            {
                return this.ClientID;
            }
        }

        Style IWebPartMenuUser.ItemHoverStyle
        {
            get
            {
                return this._menuVerbHoverStyle;
            }
        }

        Style IWebPartMenuUser.ItemStyle
        {
            get
            {
                return this._menuVerbStyle;
            }
        }

        Style IWebPartMenuUser.LabelHoverStyle
        {
            get
            {
                return this._menuLabelHoverStyle;
            }
        }

        string IWebPartMenuUser.LabelImageUrl
        {
            get
            {
                return null;
            }
        }

        Style IWebPartMenuUser.LabelStyle
        {
            get
            {
                return this.MenuLabelStyle;
            }
        }

        string IWebPartMenuUser.LabelText
        {
            get
            {
                return this.MenuLabelText;
            }
        }

        WebPartMenuStyle IWebPartMenuUser.MenuPopupStyle
        {
            get
            {
                return this._menuPopupStyle;
            }
        }

        Page IWebPartMenuUser.Page
        {
            get
            {
                return this.Page;
            }
        }

        string IWebPartMenuUser.PopupImageUrl
        {
            get
            {
                string menuPopupImageUrl = this.MenuPopupImageUrl;
                if (!string.IsNullOrEmpty(menuPopupImageUrl))
                {
                    menuPopupImageUrl = base.ResolveClientUrl(menuPopupImageUrl);
                }
                return menuPopupImageUrl;
            }
        }

        string IWebPartMenuUser.PostBackTarget
        {
            get
            {
                return this.UniqueID;
            }
        }

        IUrlResolutionService IWebPartMenuUser.UrlResolver
        {
            get
            {
                return this;
            }
        }

        [WebSysDescription("WebPartZoneBase_TitleBarVerbButtonType"), DefaultValue(1), WebCategory("Appearance")]
        public virtual ButtonType TitleBarVerbButtonType
        {
            get
            {
                object obj2 = this.ViewState["TitleBarVerbButtonType"];
                if (obj2 != null)
                {
                    return (ButtonType) obj2;
                }
                return ButtonType.Image;
            }
            set
            {
                if ((value < ButtonType.Button) || (value > ButtonType.Link))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.ViewState["TitleBarVerbButtonType"] = value;
            }
        }

        [DefaultValue((string) null), WebSysDescription("WebPartZoneBase_TitleBarVerbStyle"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty), WebCategory("Styles")]
        public Style TitleBarVerbStyle
        {
            get
            {
                if (this._titleBarVerbStyle == null)
                {
                    this._titleBarVerbStyle = new Style();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._titleBarVerbStyle).TrackViewState();
                    }
                }
                return this._titleBarVerbStyle;
            }
        }

        [Browsable(false), Themeable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public override ButtonType VerbButtonType
        {
            get
            {
                return base.VerbButtonType;
            }
            set
            {
                base.VerbButtonType = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public System.Web.UI.WebControls.WebParts.WebPartChrome WebPartChrome
        {
            get
            {
                if (this._webPartChrome == null)
                {
                    this._webPartChrome = this.CreateWebPartChrome();
                }
                return this._webPartChrome;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public WebPartCollection WebParts
        {
            get
            {
                if (base.DesignMode)
                {
                    WebPart[] array = new WebPart[this.Controls.Count];
                    this.Controls.CopyTo(array, 0);
                    return new WebPartCollection(array);
                }
                if (base.WebPartManager != null)
                {
                    return base.WebPartManager.GetWebPartsForZone(this);
                }
                return new WebPartCollection();
            }
        }

        [WebSysDescription("WebPartZoneBase_WebPartVerbRenderMode"), DefaultValue(0), WebCategory("WebPart")]
        public virtual System.Web.UI.WebControls.WebParts.WebPartVerbRenderMode WebPartVerbRenderMode
        {
            get
            {
                object obj2 = this.ViewState["WebPartVerbRenderMode"];
                if (obj2 == null)
                {
                    return System.Web.UI.WebControls.WebParts.WebPartVerbRenderMode.Menu;
                }
                return (System.Web.UI.WebControls.WebParts.WebPartVerbRenderMode) ((int) obj2);
            }
            set
            {
                if ((value < System.Web.UI.WebControls.WebParts.WebPartVerbRenderMode.Menu) || (value > System.Web.UI.WebControls.WebParts.WebPartVerbRenderMode.TitleBar))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.ViewState["WebPartVerbRenderMode"] = (int) value;
            }
        }
    }
}

