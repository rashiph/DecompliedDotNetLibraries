namespace System.Web.UI.WebControls.WebParts
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Globalization;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    public class WebPartChrome
    {
        private Style _chromeStyleNoBorder;
        private Style _chromeStyleWithBorder;
        private WebPartConnectionCollection _connections;
        private int _cssStyleIndex;
        private bool _designMode;
        private System.Web.UI.WebControls.WebParts.WebPartManager _manager;
        private Page _page;
        private bool _personalizationEnabled;
        private PersonalizationScope _personalizationScope;
        private Style _titleStyleWithoutFontOrAlign;
        private Style _titleTextStyle;
        private WebPartZoneBase _zone;
        private const string descriptionSeparator = " - ";
        private const string titleSeparator = " - ";

        public WebPartChrome(WebPartZoneBase zone, System.Web.UI.WebControls.WebParts.WebPartManager manager)
        {
            if (zone == null)
            {
                throw new ArgumentNullException("zone");
            }
            this._zone = zone;
            this._page = zone.Page;
            this._designMode = zone.DesignMode;
            this._manager = manager;
            if (this._designMode)
            {
                this._personalizationEnabled = true;
            }
            else
            {
                this._personalizationEnabled = (manager != null) && manager.Personalization.IsModifiable;
            }
            if (manager != null)
            {
                this._personalizationScope = manager.Personalization.Scope;
            }
            else
            {
                this._personalizationScope = PersonalizationScope.Shared;
            }
        }

        private Style CreateChromeStyleNoBorder(Style partChromeStyle)
        {
            Style style = new Style();
            style.CopyFrom(this.Zone.PartChromeStyle);
            if (style.BorderStyle != BorderStyle.NotSet)
            {
                style.BorderStyle = BorderStyle.NotSet;
            }
            if (style.BorderWidth != Unit.Empty)
            {
                style.BorderWidth = Unit.Empty;
            }
            if (style.BorderColor != Color.Empty)
            {
                style.BorderColor = Color.Empty;
            }
            return style;
        }

        private Style CreateChromeStyleWithBorder(Style partChromeStyle)
        {
            Style style = new Style();
            style.CopyFrom(partChromeStyle);
            if (style.BorderStyle == BorderStyle.NotSet)
            {
                style.BorderStyle = BorderStyle.Solid;
            }
            if (style.BorderWidth == Unit.Empty)
            {
                style.BorderWidth = Unit.Pixel(1);
            }
            if (style.BorderColor == Color.Empty)
            {
                style.BorderColor = Color.Black;
            }
            return style;
        }

        private Style CreateTitleStyleWithoutFontOrAlign(Style partTitleStyle)
        {
            Style style = new Style();
            style.CopyFrom(partTitleStyle);
            style.Font.Reset();
            if (style.ForeColor != Color.Empty)
            {
                style.ForeColor = Color.Empty;
            }
            return style;
        }

        private Style CreateTitleTextStyle(Style partTitleStyle)
        {
            Style style = new Style();
            if (partTitleStyle.ForeColor != Color.Empty)
            {
                style.ForeColor = partTitleStyle.ForeColor;
            }
            style.Font.CopyFrom(partTitleStyle.Font);
            return style;
        }

        protected virtual Style CreateWebPartChromeStyle(WebPart webPart, PartChromeType chromeType)
        {
            Style style;
            if (webPart == null)
            {
                throw new ArgumentNullException("webPart");
            }
            if ((chromeType < PartChromeType.Default) || (chromeType > PartChromeType.BorderOnly))
            {
                throw new ArgumentOutOfRangeException("chromeType");
            }
            if ((chromeType == PartChromeType.BorderOnly) || (chromeType == PartChromeType.TitleAndBorder))
            {
                if (this._chromeStyleWithBorder == null)
                {
                    this._chromeStyleWithBorder = this.CreateChromeStyleWithBorder(this.Zone.PartChromeStyle);
                }
                style = this._chromeStyleWithBorder;
            }
            else
            {
                if (this._chromeStyleNoBorder == null)
                {
                    this._chromeStyleNoBorder = this.CreateChromeStyleNoBorder(this.Zone.PartChromeStyle);
                }
                style = this._chromeStyleNoBorder;
            }
            if ((this.WebPartManager != null) && (webPart == this.WebPartManager.SelectedWebPart))
            {
                Style style2 = new Style();
                style2.CopyFrom(style);
                style2.CopyFrom(this.Zone.SelectedPartChromeStyle);
                return style2;
            }
            return style;
        }

        protected virtual WebPartVerbCollection FilterWebPartVerbs(WebPartVerbCollection verbs, WebPart webPart)
        {
            if (verbs == null)
            {
                throw new ArgumentNullException("verbs");
            }
            if (webPart == null)
            {
                throw new ArgumentNullException("webPart");
            }
            WebPartVerbCollection verbs2 = new WebPartVerbCollection();
            foreach (WebPartVerb verb in verbs)
            {
                if (this.ShouldRenderVerb(verb, webPart))
                {
                    verbs2.Add(verb);
                }
            }
            return verbs2;
        }

        private string GenerateDescriptionText(WebPart webPart)
        {
            string displayTitle = webPart.DisplayTitle;
            string description = webPart.Description;
            if (!string.IsNullOrEmpty(description))
            {
                displayTitle = displayTitle + " - " + description;
            }
            return displayTitle;
        }

        private string GenerateTitleText(WebPart webPart)
        {
            string displayTitle = webPart.DisplayTitle;
            string subtitle = webPart.Subtitle;
            if (!string.IsNullOrEmpty(subtitle))
            {
                displayTitle = displayTitle + " - " + subtitle;
            }
            return displayTitle;
        }

        protected string GetWebPartChromeClientID(WebPart webPart)
        {
            if (webPart == null)
            {
                throw new ArgumentNullException("webPart");
            }
            return webPart.WholePartID;
        }

        protected string GetWebPartTitleClientID(WebPart webPart)
        {
            if (webPart == null)
            {
                throw new ArgumentNullException("webPart");
            }
            return webPart.TitleBarID;
        }

        protected virtual WebPartVerbCollection GetWebPartVerbs(WebPart webPart)
        {
            if (webPart == null)
            {
                throw new ArgumentNullException("webPart");
            }
            return this.Zone.VerbsForWebPart(webPart);
        }

        public virtual void PerformPreRender()
        {
            if ((this._page != null) && this._page.SupportsStyleSheets)
            {
                Style partChromeStyle = this.Zone.PartChromeStyle;
                Style partTitleStyle = this.Zone.PartTitleStyle;
                this._chromeStyleWithBorder = this.CreateChromeStyleWithBorder(partChromeStyle);
                this.RegisterStyle(this._chromeStyleWithBorder);
                this._chromeStyleNoBorder = this.CreateChromeStyleNoBorder(partChromeStyle);
                this.RegisterStyle(this._chromeStyleNoBorder);
                this._titleTextStyle = this.CreateTitleTextStyle(partTitleStyle);
                this.RegisterStyle(this._titleTextStyle);
                this._titleStyleWithoutFontOrAlign = this.CreateTitleStyleWithoutFontOrAlign(partTitleStyle);
                this.RegisterStyle(this._titleStyleWithoutFontOrAlign);
                if ((this.Zone.RenderClientScript && (this.Zone.WebPartVerbRenderMode == WebPartVerbRenderMode.Menu)) && (this.Zone.Menu != null))
                {
                    this.Zone.Menu.RegisterStyles();
                }
            }
        }

        private void RegisterStyle(Style style)
        {
            if (!style.IsEmpty)
            {
                string cssClass = this.Zone.ClientID + "_" + this._cssStyleIndex++.ToString(NumberFormatInfo.InvariantInfo);
                this._page.Header.StyleSheet.CreateStyleRule(style, this.Zone, "." + cssClass);
                style.SetRegisteredCssClass(cssClass);
            }
        }

        protected virtual void RenderPartContents(HtmlTextWriter writer, WebPart webPart)
        {
            if (!string.IsNullOrEmpty(webPart.ConnectErrorMessage))
            {
                if (!this.Zone.ErrorStyle.IsEmpty)
                {
                    this.Zone.ErrorStyle.AddAttributesToRender(writer, this.Zone);
                }
                writer.RenderBeginTag(HtmlTextWriterTag.Div);
                writer.WriteEncodedText(webPart.ConnectErrorMessage);
                writer.RenderEndTag();
            }
            else
            {
                webPart.RenderControl(writer);
            }
        }

        private void RenderTitleBar(HtmlTextWriter writer, WebPart webPart)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
            writer.AddAttribute(HtmlTextWriterAttribute.Cellpadding, "0");
            writer.AddAttribute(HtmlTextWriterAttribute.Border, "0");
            writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "100%");
            writer.RenderBeginTag(HtmlTextWriterTag.Table);
            writer.RenderBeginTag(HtmlTextWriterTag.Tr);
            int colspan = 1;
            bool showTitleIcons = this.Zone.ShowTitleIcons;
            string titleIconImageUrl = null;
            if (showTitleIcons)
            {
                titleIconImageUrl = webPart.TitleIconImageUrl;
                if (!string.IsNullOrEmpty(titleIconImageUrl))
                {
                    colspan++;
                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    this.RenderTitleIcon(writer, webPart);
                    writer.RenderEndTag();
                }
            }
            writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "100%");
            TableItemStyle partTitleStyle = this.Zone.PartTitleStyle;
            if (!partTitleStyle.Wrap)
            {
                writer.AddStyleAttribute(HtmlTextWriterStyle.WhiteSpace, "nowrap");
            }
            HorizontalAlign horizontalAlign = partTitleStyle.HorizontalAlign;
            if (horizontalAlign != HorizontalAlign.NotSet)
            {
                TypeConverter converter = TypeDescriptor.GetConverter(typeof(HorizontalAlign));
                writer.AddAttribute(HtmlTextWriterAttribute.Align, converter.ConvertToString(horizontalAlign).ToLower(CultureInfo.InvariantCulture));
            }
            VerticalAlign verticalAlign = partTitleStyle.VerticalAlign;
            if (verticalAlign != VerticalAlign.NotSet)
            {
                TypeConverter converter2 = TypeDescriptor.GetConverter(typeof(VerticalAlign));
                writer.AddAttribute(HtmlTextWriterAttribute.Valign, converter2.ConvertToString(verticalAlign).ToLower(CultureInfo.InvariantCulture));
            }
            if (this.Zone.RenderClientScript)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Id, this.GetWebPartTitleClientID(webPart));
            }
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            if (showTitleIcons && !string.IsNullOrEmpty(titleIconImageUrl))
            {
                writer.Write("&nbsp;");
            }
            this.RenderTitleText(writer, webPart);
            writer.RenderEndTag();
            this.RenderVerbsInTitleBar(writer, webPart, colspan);
            writer.RenderEndTag();
            writer.RenderEndTag();
        }

        private void RenderTitleIcon(HtmlTextWriter writer, WebPart webPart)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Src, this.Zone.ResolveClientUrl(webPart.TitleIconImageUrl));
            writer.AddAttribute(HtmlTextWriterAttribute.Alt, this.GenerateDescriptionText(webPart));
            writer.RenderBeginTag(HtmlTextWriterTag.Img);
            writer.RenderEndTag();
        }

        private void RenderTitleText(HtmlTextWriter writer, WebPart webPart)
        {
            if (this._titleTextStyle == null)
            {
                this._titleTextStyle = this.CreateTitleTextStyle(this.Zone.PartTitleStyle);
            }
            if (!this._titleTextStyle.IsEmpty)
            {
                this._titleTextStyle.AddAttributesToRender(writer, this.Zone);
            }
            writer.AddAttribute(HtmlTextWriterAttribute.Title, this.GenerateDescriptionText(webPart), true);
            string titleUrl = webPart.TitleUrl;
            string text = this.GenerateTitleText(webPart);
            if (!string.IsNullOrEmpty(titleUrl) && !this.DragDropEnabled)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Href, this.Zone.ResolveClientUrl(titleUrl));
                writer.RenderBeginTag(HtmlTextWriterTag.A);
            }
            else
            {
                writer.RenderBeginTag(HtmlTextWriterTag.Span);
            }
            writer.WriteEncodedText(text);
            writer.RenderEndTag();
            writer.Write("&nbsp;");
        }

        private void RenderVerb(HtmlTextWriter writer, WebPart webPart, WebPartVerb verb)
        {
            WebControl control;
            bool flag = this.Zone.IsEnabled && verb.Enabled;
            ButtonType titleBarVerbButtonType = this.Zone.TitleBarVerbButtonType;
            if (verb != this.Zone.HelpVerb)
            {
                if (verb == this.Zone.ExportVerb)
                {
                    string exportUrl = this._manager.GetExportUrl(webPart);
                    if (titleBarVerbButtonType == ButtonType.Button)
                    {
                        ZoneButton button2 = new ZoneButton(this.Zone, string.Empty) {
                            Text = verb.Text
                        };
                        if (flag)
                        {
                            if ((webPart.ExportMode == WebPartExportMode.All) && (this._personalizationScope == PersonalizationScope.User))
                            {
                                if (this.Zone.RenderClientScript)
                                {
                                    button2.OnClientClick = "__wpm.ExportWebPart('" + Util.QuoteJScriptString(exportUrl) + "', true, false);return false;";
                                }
                                else
                                {
                                    button2.OnClientClick = "if(__wpmExportWarning.length == 0 || confirm(__wpmExportWarning)){window.location='" + Util.QuoteJScriptString(exportUrl) + "';}return false;";
                                }
                            }
                            else
                            {
                                button2.OnClientClick = "window.location='" + Util.QuoteJScriptString(exportUrl) + "';return false;";
                            }
                        }
                        control = button2;
                    }
                    else
                    {
                        HyperLink link2 = new HyperLink {
                            Text = verb.Text
                        };
                        if (titleBarVerbButtonType == ButtonType.Image)
                        {
                            link2.ImageUrl = verb.ImageUrl;
                        }
                        link2.NavigateUrl = exportUrl;
                        if (webPart.ExportMode == WebPartExportMode.All)
                        {
                            if (this.Zone.RenderClientScript)
                            {
                                link2.Attributes.Add("onclick", "return __wpm.ExportWebPart('', true, true)");
                            }
                            else
                            {
                                string str3 = "return (__wpmExportWarning.length == 0 || confirm(__wpmExportWarning))";
                                link2.Attributes.Add("onclick", str3);
                            }
                        }
                        control = link2;
                    }
                }
                else
                {
                    string eventArgument = verb.GetEventArgument(webPart.ID);
                    string clientClickHandler = verb.ClientClickHandler;
                    if (titleBarVerbButtonType == ButtonType.Button)
                    {
                        ZoneButton button3 = new ZoneButton(this.Zone, eventArgument) {
                            Text = verb.Text
                        };
                        if (!string.IsNullOrEmpty(clientClickHandler) && flag)
                        {
                            button3.OnClientClick = clientClickHandler;
                        }
                        control = button3;
                    }
                    else
                    {
                        ZoneLinkButton button4 = new ZoneLinkButton(this.Zone, eventArgument) {
                            Text = verb.Text
                        };
                        if (titleBarVerbButtonType == ButtonType.Image)
                        {
                            button4.ImageUrl = verb.ImageUrl;
                        }
                        if (!string.IsNullOrEmpty(clientClickHandler) && flag)
                        {
                            button4.OnClientClick = clientClickHandler;
                        }
                        control = button4;
                    }
                    if ((this._manager != null) && flag)
                    {
                        if (verb == this.Zone.CloseVerb)
                        {
                            ProviderConnectionPointCollection providerConnectionPoints = this._manager.GetProviderConnectionPoints(webPart);
                            if (((providerConnectionPoints != null) && (providerConnectionPoints.Count > 0)) && this.Connections.ContainsProvider(webPart))
                            {
                                string str6 = "if (__wpmCloseProviderWarning.length >= 0 && !confirm(__wpmCloseProviderWarning)) { return false; }";
                                control.Attributes.Add("onclick", str6);
                            }
                        }
                        else if (verb == this.Zone.DeleteVerb)
                        {
                            string str7 = "if (__wpmDeleteWarning.length >= 0 && !confirm(__wpmDeleteWarning)) { return false; }";
                            control.Attributes.Add("onclick", str7);
                        }
                    }
                }
                goto Label_040C;
            }
            string str = this.Zone.ResolveClientUrl(webPart.HelpUrl);
            if (titleBarVerbButtonType == ButtonType.Button)
            {
                ZoneButton button = new ZoneButton(this.Zone, null);
                if (flag)
                {
                    if (this.Zone.RenderClientScript)
                    {
                        button.OnClientClick = "__wpm.ShowHelp('" + Util.QuoteJScriptString(str) + "', " + ((int) webPart.HelpMode).ToString(CultureInfo.InvariantCulture) + ");return;";
                    }
                    else if (webPart.HelpMode != WebPartHelpMode.Navigate)
                    {
                        button.OnClientClick = "window.open('" + Util.QuoteJScriptString(str) + "', '_blank', 'scrollbars=yes,resizable=yes,status=no,toolbar=no,menubar=no,location=no');return;";
                    }
                    else
                    {
                        button.OnClientClick = "window.location.href='" + Util.QuoteJScriptString(str) + "';return;";
                    }
                }
                button.Text = verb.Text;
                control = button;
                goto Label_040C;
            }
            HyperLink link = new HyperLink();
            switch (webPart.HelpMode)
            {
                case WebPartHelpMode.Modal:
                    if (!this.Zone.RenderClientScript)
                    {
                        break;
                    }
                    link.NavigateUrl = "javascript:__wpm.ShowHelp('" + Util.QuoteJScriptString(str) + "', 0)";
                    goto Label_0187;

                case WebPartHelpMode.Modeless:
                    break;

                case WebPartHelpMode.Navigate:
                    link.NavigateUrl = str;
                    goto Label_0187;

                default:
                    goto Label_0187;
            }
            link.NavigateUrl = str;
            link.Target = "_blank";
        Label_0187:
            link.Text = verb.Text;
            if (titleBarVerbButtonType == ButtonType.Image)
            {
                link.ImageUrl = verb.ImageUrl;
            }
            control = link;
        Label_040C:
            control.ApplyStyle(this.Zone.TitleBarVerbStyle);
            control.ToolTip = string.Format(CultureInfo.CurrentCulture, verb.Description, new object[] { webPart.DisplayTitle });
            control.Enabled = verb.Enabled;
            control.Page = this._page;
            control.RenderControl(writer);
        }

        private void RenderVerbs(HtmlTextWriter writer, WebPart webPart, WebPartVerbCollection verbs)
        {
            if (verbs == null)
            {
                throw new ArgumentNullException("verbs");
            }
            WebPartVerb verb = null;
            foreach (WebPartVerb verb2 in verbs)
            {
                if ((verb != null) && (this.VerbRenderedAsLinkButton(verb2) || this.VerbRenderedAsLinkButton(verb)))
                {
                    writer.Write("&nbsp;");
                }
                this.RenderVerb(writer, webPart, verb2);
                verb = verb2;
            }
        }

        private void RenderVerbsInTitleBar(HtmlTextWriter writer, WebPart webPart, int colspan)
        {
            WebPartVerbCollection webPartVerbs = this.GetWebPartVerbs(webPart);
            webPartVerbs = this.FilterWebPartVerbs(webPartVerbs, webPart);
            if ((webPartVerbs != null) && (webPartVerbs.Count > 0))
            {
                writer.AddStyleAttribute(HtmlTextWriterStyle.WhiteSpace, "nowrap");
                colspan++;
                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                if ((this.Zone.RenderClientScript && (this.Zone.WebPartVerbRenderMode == WebPartVerbRenderMode.Menu)) && (this.Zone.Menu != null))
                {
                    if (this._designMode)
                    {
                        this.Zone.Menu.Render(writer, webPart.WholePartID + "Verbs");
                    }
                    else
                    {
                        this.Zone.Menu.Render(writer, webPartVerbs, webPart.WholePartID + "Verbs", webPart, this.WebPartManager);
                    }
                }
                else
                {
                    this.RenderVerbs(writer, webPart, webPartVerbs);
                }
                writer.RenderEndTag();
            }
        }

        public virtual void RenderWebPart(HtmlTextWriter writer, WebPart webPart)
        {
            if (webPart == null)
            {
                throw new ArgumentNullException("webPart");
            }
            bool flag = this.Zone.LayoutOrientation == Orientation.Vertical;
            PartChromeType effectiveChromeType = this.Zone.GetEffectiveChromeType(webPart);
            Style style = this.CreateWebPartChromeStyle(webPart, effectiveChromeType);
            if (!style.IsEmpty)
            {
                style.AddAttributesToRender(writer, this.Zone);
            }
            writer.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
            writer.AddAttribute(HtmlTextWriterAttribute.Cellpadding, "2");
            writer.AddAttribute(HtmlTextWriterAttribute.Border, "0");
            if (flag)
            {
                writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "100%");
            }
            else if (webPart.ChromeState != PartChromeState.Minimized)
            {
                writer.AddStyleAttribute(HtmlTextWriterStyle.Height, "100%");
            }
            if (this.Zone.RenderClientScript)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Id, this.GetWebPartChromeClientID(webPart));
            }
            if ((!this._designMode && webPart.Hidden) && ((this.WebPartManager != null) && !this.WebPartManager.DisplayMode.ShowHiddenWebParts))
            {
                writer.AddStyleAttribute(HtmlTextWriterStyle.Display, "none");
            }
            writer.RenderBeginTag(HtmlTextWriterTag.Table);
            switch (effectiveChromeType)
            {
                case PartChromeType.TitleOnly:
                case PartChromeType.TitleAndBorder:
                    writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                    if (this._titleStyleWithoutFontOrAlign == null)
                    {
                        this._titleStyleWithoutFontOrAlign = this.CreateTitleStyleWithoutFontOrAlign(this.Zone.PartTitleStyle);
                    }
                    if (!this._titleStyleWithoutFontOrAlign.IsEmpty)
                    {
                        this._titleStyleWithoutFontOrAlign.AddAttributesToRender(writer, this.Zone);
                    }
                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    this.RenderTitleBar(writer, webPart);
                    writer.RenderEndTag();
                    writer.RenderEndTag();
                    break;
            }
            if (webPart.ChromeState == PartChromeState.Minimized)
            {
                writer.AddStyleAttribute(HtmlTextWriterStyle.Display, "none");
            }
            writer.RenderBeginTag(HtmlTextWriterTag.Tr);
            if (!flag)
            {
                writer.AddStyleAttribute(HtmlTextWriterStyle.Height, "100%");
                writer.AddAttribute(HtmlTextWriterAttribute.Valign, "top");
            }
            Style partStyle = this.Zone.PartStyle;
            if (!partStyle.IsEmpty)
            {
                partStyle.AddAttributesToRender(writer, this.Zone);
            }
            writer.AddStyleAttribute(HtmlTextWriterStyle.Padding, this.Zone.PartChromePadding.ToString());
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            this.RenderPartContents(writer, webPart);
            writer.RenderEndTag();
            writer.RenderEndTag();
            writer.RenderEndTag();
        }

        private bool ShouldRenderVerb(WebPartVerb verb, WebPart webPart)
        {
            if (verb == null)
            {
                return false;
            }
            if (!verb.Visible)
            {
                return false;
            }
            if ((verb == this.Zone.CloseVerb) && ((!this._personalizationEnabled || !webPart.AllowClose) || !this.Zone.AllowLayoutChange))
            {
                return false;
            }
            if ((verb == this.Zone.ConnectVerb) && (this.WebPartManager != null))
            {
                if (((this.WebPartManager.DisplayMode != System.Web.UI.WebControls.WebParts.WebPartManager.ConnectDisplayMode) || (webPart == this.WebPartManager.SelectedWebPart)) || !webPart.AllowConnect)
                {
                    return false;
                }
                ConsumerConnectionPointCollection enabledConsumerConnectionPoints = this.WebPartManager.GetEnabledConsumerConnectionPoints(webPart);
                ProviderConnectionPointCollection enabledProviderConnectionPoints = this.WebPartManager.GetEnabledProviderConnectionPoints(webPart);
                if (((enabledConsumerConnectionPoints == null) || (enabledConsumerConnectionPoints.Count == 0)) && ((enabledProviderConnectionPoints == null) || (enabledProviderConnectionPoints.Count == 0)))
                {
                    return false;
                }
            }
            if ((verb == this.Zone.DeleteVerb) && ((((!this._personalizationEnabled || !this.Zone.AllowLayoutChange) || webPart.IsStatic) || (webPart.IsShared && (this._personalizationScope == PersonalizationScope.User))) || ((this.WebPartManager != null) && !this.WebPartManager.DisplayMode.AllowPageDesign)))
            {
                return false;
            }
            if (((verb == this.Zone.EditVerb) && (this.WebPartManager != null)) && ((this.WebPartManager.DisplayMode != System.Web.UI.WebControls.WebParts.WebPartManager.EditDisplayMode) || (webPart == this.WebPartManager.SelectedWebPart)))
            {
                return false;
            }
            if ((verb == this.Zone.HelpVerb) && string.IsNullOrEmpty(webPart.HelpUrl))
            {
                return false;
            }
            if ((verb == this.Zone.MinimizeVerb) && ((!this._personalizationEnabled || (webPart.ChromeState == PartChromeState.Minimized)) || (!webPart.AllowMinimize || !this.Zone.AllowLayoutChange)))
            {
                return false;
            }
            if ((verb == this.Zone.RestoreVerb) && ((!this._personalizationEnabled || (webPart.ChromeState == PartChromeState.Normal)) || !this.Zone.AllowLayoutChange))
            {
                return false;
            }
            return ((verb != this.Zone.ExportVerb) || (this._personalizationEnabled && (webPart.ExportMode != WebPartExportMode.None)));
        }

        private bool VerbRenderedAsLinkButton(WebPartVerb verb)
        {
            return ((this.Zone.TitleBarVerbButtonType == ButtonType.Link) || string.IsNullOrEmpty(verb.ImageUrl));
        }

        private WebPartConnectionCollection Connections
        {
            get
            {
                if (this._connections == null)
                {
                    this._connections = this._manager.Connections;
                }
                return this._connections;
            }
        }

        protected bool DragDropEnabled
        {
            get
            {
                return this.Zone.DragDropEnabled;
            }
        }

        protected System.Web.UI.WebControls.WebParts.WebPartManager WebPartManager
        {
            get
            {
                return this._manager;
            }
        }

        protected WebPartZoneBase Zone
        {
            get
            {
                return this._zone;
            }
        }
    }
}

