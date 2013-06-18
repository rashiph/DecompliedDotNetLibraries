namespace System.Web.UI.WebControls.WebParts
{
    using System;
    using System.Collections;
    using System.Drawing;
    using System.Globalization;
    using System.Web;
    using System.Web.Handlers;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using System.Web.Util;

    internal sealed class WebPartMenu
    {
        private int _cssStyleIndex;
        private static string _defaultCheckImageUrl;
        private IWebPartMenuUser _menuUser;

        public WebPartMenu(IWebPartMenuUser menuUser)
        {
            this._menuUser = menuUser;
        }

        private void RegisterStartupScript(string clientID)
        {
            string str = string.Empty;
            string str2 = string.Empty;
            Style itemStyle = this._menuUser.ItemStyle;
            if (itemStyle != null)
            {
                str = itemStyle.GetStyleAttributes(this._menuUser.UrlResolver).Value;
            }
            Style itemHoverStyle = this._menuUser.ItemHoverStyle;
            if (itemHoverStyle != null)
            {
                str2 = itemHoverStyle.GetStyleAttributes(this._menuUser.UrlResolver).Value;
            }
            string str3 = string.Empty;
            string registeredCssClass = string.Empty;
            Style labelHoverStyle = this._menuUser.LabelHoverStyle;
            if (labelHoverStyle != null)
            {
                Color foreColor = labelHoverStyle.ForeColor;
                if (!foreColor.IsEmpty)
                {
                    str3 = ColorTranslator.ToHtml(foreColor);
                }
                registeredCssClass = labelHoverStyle.RegisteredCssClass;
            }
            string script = "\r\n<script type=\"text/javascript\">\r\nvar menu" + clientID + " = new WebPartMenu(document.getElementById('" + clientID + "'), document.getElementById('" + clientID + "Popup'), document.getElementById('" + clientID + "Menu'));\r\nmenu" + clientID + ".itemStyle = '" + Util.QuoteJScriptString(str) + "';\r\nmenu" + clientID + ".itemHoverStyle = '" + Util.QuoteJScriptString(str2) + "';\r\nmenu" + clientID + ".labelHoverColor = '" + str3 + "';\r\nmenu" + clientID + ".labelHoverClassName = '" + registeredCssClass + "';\r\n</script>\r\n";
            if (this._menuUser.Page != null)
            {
                this._menuUser.Page.ClientScript.RegisterStartupScript((Control) this._menuUser, typeof(WebPartMenu), clientID, script, false);
                IScriptManager scriptManager = this._menuUser.Page.ScriptManager;
                if ((scriptManager != null) && scriptManager.SupportsPartialRendering)
                {
                    scriptManager.RegisterDispose((Control) this._menuUser, "document.getElementById('" + clientID + "').__menu.Dispose();");
                }
            }
        }

        private void RegisterStyle(Style style)
        {
            if ((style != null) && !style.IsEmpty)
            {
                string cssClass = this._menuUser.ClientID + "__Menu_" + this._cssStyleIndex++.ToString(NumberFormatInfo.InvariantInfo);
                this._menuUser.Page.Header.StyleSheet.CreateStyleRule(style, this._menuUser.UrlResolver, "." + cssClass);
                style.SetRegisteredCssClass(cssClass);
            }
        }

        public void RegisterStyles()
        {
            this.RegisterStyle(this._menuUser.LabelStyle);
            this.RegisterStyle(this._menuUser.LabelHoverStyle);
        }

        public void Render(HtmlTextWriter writer, string clientID)
        {
            this.RenderLabel(writer, clientID, null);
        }

        public void Render(HtmlTextWriter writer, ICollection verbs, string clientID, WebPart associatedWebPart, WebPartManager webPartManager)
        {
            this.RegisterStartupScript(clientID);
            this.RenderLabel(writer, clientID, associatedWebPart);
            this.RenderMenuPopup(writer, verbs, clientID, associatedWebPart, webPartManager);
        }

        private void RenderLabel(HtmlTextWriter writer, string clientID, WebPart associatedWebPart)
        {
            this._menuUser.OnBeginRender(writer);
            if (associatedWebPart != null)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Id, clientID);
                Style labelStyle = this._menuUser.LabelStyle;
                if (labelStyle != null)
                {
                    labelStyle.AddAttributesToRender(writer, this._menuUser as WebControl);
                }
            }
            writer.AddStyleAttribute(HtmlTextWriterStyle.Cursor, "hand");
            writer.AddStyleAttribute(HtmlTextWriterStyle.Display, "inline-block");
            writer.AddStyleAttribute(HtmlTextWriterStyle.Padding, "1px");
            writer.AddStyleAttribute(HtmlTextWriterStyle.TextDecoration, "none");
            writer.RenderBeginTag(HtmlTextWriterTag.Span);
            string labelImageUrl = this._menuUser.LabelImageUrl;
            string labelText = this._menuUser.LabelText;
            if (!string.IsNullOrEmpty(labelImageUrl))
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Src, labelImageUrl);
                writer.AddAttribute(HtmlTextWriterAttribute.Alt, !string.IsNullOrEmpty(labelText) ? labelText : System.Web.SR.GetString("WebPartMenu_DefaultDropDownAlternateText"), true);
                writer.AddStyleAttribute("vertical-align", "middle");
                writer.AddStyleAttribute(HtmlTextWriterStyle.BorderStyle, "none");
                writer.RenderBeginTag(HtmlTextWriterTag.Img);
                writer.RenderEndTag();
                writer.Write("&nbsp;");
            }
            if (!string.IsNullOrEmpty(labelText))
            {
                writer.Write(labelText);
                writer.Write("&nbsp;");
            }
            writer.AddAttribute(HtmlTextWriterAttribute.Id, clientID + "Popup");
            string popupImageUrl = this._menuUser.PopupImageUrl;
            if (!string.IsNullOrEmpty(popupImageUrl))
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Src, popupImageUrl);
                writer.AddAttribute(HtmlTextWriterAttribute.Alt, !string.IsNullOrEmpty(labelText) ? labelText : System.Web.SR.GetString("WebPartMenu_DefaultDropDownAlternateText"), true);
                writer.AddStyleAttribute("vertical-align", "middle");
                writer.AddStyleAttribute(HtmlTextWriterStyle.BorderStyle, "none");
                writer.RenderBeginTag(HtmlTextWriterTag.Img);
                writer.RenderEndTag();
            }
            else
            {
                writer.AddStyleAttribute(HtmlTextWriterStyle.FontFamily, "Marlett");
                writer.AddStyleAttribute(HtmlTextWriterStyle.FontSize, "8pt");
                writer.RenderBeginTag(HtmlTextWriterTag.Span);
                writer.Write("u");
                writer.RenderEndTag();
            }
            writer.RenderEndTag();
            this._menuUser.OnEndRender(writer);
        }

        private void RenderMenuPopup(HtmlTextWriter writer, ICollection verbs, string clientID, WebPart associatedWebPart, WebPartManager webPartManager)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Id, clientID + "Menu");
            writer.AddStyleAttribute(HtmlTextWriterStyle.Display, "none");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
            bool isEmpty = true;
            WebPartMenuStyle menuPopupStyle = this._menuUser.MenuPopupStyle;
            if (menuPopupStyle != null)
            {
                menuPopupStyle.AddAttributesToRender(writer, this._menuUser as WebControl);
                isEmpty = menuPopupStyle.Width.IsEmpty;
            }
            else
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
                writer.AddAttribute(HtmlTextWriterAttribute.Cellpadding, "1");
                writer.AddStyleAttribute(HtmlTextWriterStyle.BorderCollapse, "collapse");
            }
            if (isEmpty)
            {
                writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "100%");
            }
            writer.RenderBeginTag(HtmlTextWriterTag.Table);
            writer.RenderBeginTag(HtmlTextWriterTag.Tr);
            writer.AddStyleAttribute(HtmlTextWriterStyle.WhiteSpace, "nowrap");
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            bool isEnabled = associatedWebPart.Zone.IsEnabled;
            foreach (WebPartVerb verb in verbs)
            {
                string description;
                writer.RenderBeginTag(HtmlTextWriterTag.Div);
                if (associatedWebPart != null)
                {
                    description = string.Format(CultureInfo.CurrentCulture, verb.Description, new object[] { associatedWebPart.DisplayTitle });
                }
                else
                {
                    description = verb.Description;
                }
                if (description.Length != 0)
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Title, description);
                }
                bool flag3 = isEnabled && verb.Enabled;
                if (verb is WebPartHelpVerb)
                {
                    string str2 = associatedWebPart.ResolveClientUrl(associatedWebPart.HelpUrl);
                    writer.AddAttribute(HtmlTextWriterAttribute.Href, "javascript:void(0)");
                    if (flag3)
                    {
                        writer.AddAttribute(HtmlTextWriterAttribute.Onclick, "document.body.__wpm.ShowHelp('" + Util.QuoteJScriptString(str2) + "', " + ((int) associatedWebPart.HelpMode).ToString(CultureInfo.InvariantCulture) + ")");
                    }
                }
                else if (verb is WebPartExportVerb)
                {
                    string exportUrl = webPartManager.GetExportUrl(associatedWebPart);
                    writer.AddAttribute(HtmlTextWriterAttribute.Href, "javascript:void(0)");
                    if (flag3)
                    {
                        writer.AddAttribute(HtmlTextWriterAttribute.Onclick, "document.body.__wpm.ExportWebPart('" + Util.QuoteJScriptString(exportUrl) + ((associatedWebPart.ExportMode == WebPartExportMode.All) ? "', true, false)" : "', false, false)"));
                    }
                }
                else
                {
                    string postBackTarget = this._menuUser.PostBackTarget;
                    writer.AddAttribute(HtmlTextWriterAttribute.Href, "javascript:void(0)");
                    if (flag3)
                    {
                        string eventArgument = verb.EventArgument;
                        if (associatedWebPart != null)
                        {
                            eventArgument = verb.GetEventArgument(associatedWebPart.ID);
                        }
                        string str6 = null;
                        if (!string.IsNullOrEmpty(eventArgument))
                        {
                            str6 = "document.body.__wpm.SubmitPage('" + Util.QuoteJScriptString(postBackTarget) + "', '" + Util.QuoteJScriptString(eventArgument) + "');";
                            this._menuUser.Page.ClientScript.RegisterForEventValidation(postBackTarget, eventArgument);
                        }
                        string str7 = null;
                        if (!string.IsNullOrEmpty(verb.ClientClickHandler))
                        {
                            str7 = "document.body.__wpm.Execute('" + Util.QuoteJScriptString(Util.EnsureEndWithSemiColon(verb.ClientClickHandler)) + "')";
                        }
                        string str8 = string.Empty;
                        if ((str6 != null) && (str7 != null))
                        {
                            str8 = "if(" + str7 + "){" + str6 + "}";
                        }
                        else if (str6 != null)
                        {
                            str8 = str6;
                        }
                        else if (str7 != null)
                        {
                            str8 = str7;
                        }
                        if (verb is WebPartCloseVerb)
                        {
                            ProviderConnectionPointCollection providerConnectionPoints = webPartManager.GetProviderConnectionPoints(associatedWebPart);
                            if (((providerConnectionPoints != null) && (providerConnectionPoints.Count > 0)) && webPartManager.Connections.ContainsProvider(associatedWebPart))
                            {
                                str8 = "if(document.body.__wpmCloseProviderWarning.length == 0 || confirm(document.body.__wpmCloseProviderWarning)){" + str8 + "}";
                            }
                        }
                        else if (verb is WebPartDeleteVerb)
                        {
                            str8 = "if(document.body.__wpmDeleteWarning.length == 0 || confirm(document.body.__wpmDeleteWarning)){" + str8 + "}";
                        }
                        writer.AddAttribute(HtmlTextWriterAttribute.Onclick, str8);
                    }
                }
                string str9 = "menuItem";
                if (!verb.Enabled)
                {
                    if (associatedWebPart.Zone.RenderingCompatibility < VersionUtil.Framework40)
                    {
                        writer.AddAttribute(HtmlTextWriterAttribute.Disabled, "disabled");
                    }
                    else if (!string.IsNullOrEmpty(WebControl.DisabledCssClass))
                    {
                        str9 = WebControl.DisabledCssClass + " " + str9;
                    }
                }
                writer.AddAttribute(HtmlTextWriterAttribute.Class, str9);
                writer.RenderBeginTag(HtmlTextWriterTag.A);
                string imageUrl = verb.ImageUrl;
                if (imageUrl.Length != 0)
                {
                    imageUrl = this._menuUser.UrlResolver.ResolveClientUrl(imageUrl);
                }
                else if (verb.Checked)
                {
                    imageUrl = this._menuUser.CheckImageUrl;
                    if (imageUrl.Length == 0)
                    {
                        imageUrl = DefaultCheckImageUrl;
                    }
                }
                else
                {
                    imageUrl = webPartManager.SpacerImageUrl;
                }
                writer.AddAttribute(HtmlTextWriterAttribute.Src, imageUrl);
                writer.AddAttribute(HtmlTextWriterAttribute.Alt, description, true);
                writer.AddAttribute(HtmlTextWriterAttribute.Width, "16");
                writer.AddAttribute(HtmlTextWriterAttribute.Height, "16");
                writer.AddStyleAttribute(HtmlTextWriterStyle.BorderStyle, "none");
                writer.AddStyleAttribute("vertical-align", "middle");
                if (verb.Checked)
                {
                    Style checkImageStyle = this._menuUser.CheckImageStyle;
                    if (checkImageStyle != null)
                    {
                        checkImageStyle.AddAttributesToRender(writer, this._menuUser as WebControl);
                    }
                }
                writer.RenderBeginTag(HtmlTextWriterTag.Img);
                writer.RenderEndTag();
                writer.Write("&nbsp;");
                writer.Write(verb.Text);
                writer.Write("&nbsp;");
                writer.RenderEndTag();
                writer.RenderEndTag();
            }
            writer.RenderEndTag();
            writer.RenderEndTag();
            writer.RenderEndTag();
            writer.RenderEndTag();
        }

        private static string DefaultCheckImageUrl
        {
            get
            {
                if (_defaultCheckImageUrl == null)
                {
                    _defaultCheckImageUrl = AssemblyResourceLoader.GetWebResourceUrl(typeof(WebPartMenu), "WebPartMenu_Check.gif");
                }
                return _defaultCheckImageUrl;
            }
        }
    }
}

