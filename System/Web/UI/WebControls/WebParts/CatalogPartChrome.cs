namespace System.Web.UI.WebControls.WebParts
{
    using System;
    using System.Drawing;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    public class CatalogPartChrome
    {
        private Style _chromeStyleNoBorder;
        private Style _chromeStyleWithBorder;
        private Page _page;
        private CatalogZoneBase _zone;

        public CatalogPartChrome(CatalogZoneBase zone)
        {
            if (zone == null)
            {
                throw new ArgumentNullException("zone");
            }
            this._zone = zone;
            this._page = zone.Page;
        }

        protected virtual Style CreateCatalogPartChromeStyle(CatalogPart catalogPart, PartChromeType chromeType)
        {
            if (catalogPart == null)
            {
                throw new ArgumentNullException("catalogPart");
            }
            if ((chromeType < PartChromeType.Default) || (chromeType > PartChromeType.BorderOnly))
            {
                throw new ArgumentOutOfRangeException("chromeType");
            }
            if ((chromeType == PartChromeType.BorderOnly) || (chromeType == PartChromeType.TitleAndBorder))
            {
                if (this._chromeStyleWithBorder == null)
                {
                    Style style = new Style();
                    style.CopyFrom(this.Zone.PartChromeStyle);
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
                    this._chromeStyleWithBorder = style;
                }
                return this._chromeStyleWithBorder;
            }
            if (this._chromeStyleNoBorder == null)
            {
                Style style2 = new Style();
                style2.CopyFrom(this.Zone.PartChromeStyle);
                if (style2.BorderStyle != BorderStyle.NotSet)
                {
                    style2.BorderStyle = BorderStyle.NotSet;
                }
                if (style2.BorderWidth != Unit.Empty)
                {
                    style2.BorderWidth = Unit.Empty;
                }
                if (style2.BorderColor != Color.Empty)
                {
                    style2.BorderColor = Color.Empty;
                }
                this._chromeStyleNoBorder = style2;
            }
            return this._chromeStyleNoBorder;
        }

        public virtual void PerformPreRender()
        {
        }

        public virtual void RenderCatalogPart(HtmlTextWriter writer, CatalogPart catalogPart)
        {
            if (catalogPart == null)
            {
                throw new ArgumentNullException("catalogPart");
            }
            PartChromeType effectiveChromeType = this.Zone.GetEffectiveChromeType(catalogPart);
            Style style = this.CreateCatalogPartChromeStyle(catalogPart, effectiveChromeType);
            if (!style.IsEmpty)
            {
                style.AddAttributesToRender(writer, this.Zone);
            }
            writer.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
            writer.AddAttribute(HtmlTextWriterAttribute.Cellpadding, "2");
            writer.AddAttribute(HtmlTextWriterAttribute.Border, "0");
            writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "100%");
            writer.RenderBeginTag(HtmlTextWriterTag.Table);
            switch (effectiveChromeType)
            {
                case PartChromeType.TitleOnly:
                case PartChromeType.TitleAndBorder:
                {
                    writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                    Style partTitleStyle = this.Zone.PartTitleStyle;
                    if (!partTitleStyle.IsEmpty)
                    {
                        partTitleStyle.AddAttributesToRender(writer, this.Zone);
                    }
                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    this.RenderTitle(writer, catalogPart);
                    writer.RenderEndTag();
                    writer.RenderEndTag();
                    break;
                }
            }
            if (catalogPart.ChromeState != PartChromeState.Minimized)
            {
                writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                Style partStyle = this.Zone.PartStyle;
                if (!partStyle.IsEmpty)
                {
                    partStyle.AddAttributesToRender(writer, this.Zone);
                }
                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                this.RenderPartContents(writer, catalogPart);
                this.RenderItems(writer, catalogPart);
                writer.RenderEndTag();
                writer.RenderEndTag();
            }
            writer.RenderEndTag();
        }

        private void RenderItem(HtmlTextWriter writer, WebPartDescription webPartDescription)
        {
            string description = webPartDescription.Description;
            if (string.IsNullOrEmpty(description))
            {
                description = webPartDescription.Title;
            }
            this.RenderItemCheckBox(writer, webPartDescription.ID);
            writer.Write("&nbsp;");
            if (this.Zone.ShowCatalogIcons)
            {
                string catalogIconImageUrl = webPartDescription.CatalogIconImageUrl;
                if (!string.IsNullOrEmpty(catalogIconImageUrl))
                {
                    this.RenderItemIcon(writer, catalogIconImageUrl, description);
                    writer.Write("&nbsp;");
                }
            }
            this.RenderItemText(writer, webPartDescription.ID, webPartDescription.Title, description);
            writer.WriteBreak();
        }

        private void RenderItemCheckBox(HtmlTextWriter writer, string value)
        {
            this.Zone.EditUIStyle.AddAttributesToRender(writer, this.Zone);
            writer.AddAttribute(HtmlTextWriterAttribute.Type, "checkbox");
            writer.AddAttribute(HtmlTextWriterAttribute.Id, this.Zone.GetCheckBoxID(value));
            writer.AddAttribute(HtmlTextWriterAttribute.Name, this.Zone.CheckBoxName);
            writer.AddAttribute(HtmlTextWriterAttribute.Value, value);
            writer.RenderBeginTag(HtmlTextWriterTag.Input);
            writer.RenderEndTag();
            if (this._page != null)
            {
                this._page.ClientScript.RegisterForEventValidation(this.Zone.CheckBoxName);
            }
        }

        private void RenderItemIcon(HtmlTextWriter writer, string iconUrl, string description)
        {
            new System.Web.UI.WebControls.Image { AlternateText = description, ImageUrl = iconUrl, BorderStyle = BorderStyle.None, Page = this._page }.RenderControl(writer);
        }

        private void RenderItems(HtmlTextWriter writer, CatalogPart catalogPart)
        {
            WebPartDescriptionCollection availableWebPartDescriptions = catalogPart.GetAvailableWebPartDescriptions();
            if (availableWebPartDescriptions != null)
            {
                foreach (WebPartDescription description in availableWebPartDescriptions)
                {
                    this.RenderItem(writer, description);
                }
            }
        }

        private void RenderItemText(HtmlTextWriter writer, string value, string text, string description)
        {
            this.Zone.LabelStyle.AddAttributesToRender(writer, this.Zone);
            writer.AddAttribute(HtmlTextWriterAttribute.For, this.Zone.GetCheckBoxID(value));
            writer.AddAttribute(HtmlTextWriterAttribute.Title, description, true);
            writer.RenderBeginTag(HtmlTextWriterTag.Label);
            writer.WriteEncodedText(text);
            writer.RenderEndTag();
        }

        protected virtual void RenderPartContents(HtmlTextWriter writer, CatalogPart catalogPart)
        {
            if (catalogPart == null)
            {
                throw new ArgumentNullException("catalogPart");
            }
            catalogPart.RenderControl(writer);
        }

        private void RenderTitle(HtmlTextWriter writer, CatalogPart catalogPart)
        {
            new Label { Text = catalogPart.DisplayTitle, ToolTip = catalogPart.Description, Page = this._page }.RenderControl(writer);
        }

        protected CatalogZoneBase Zone
        {
            get
            {
                return this._zone;
            }
        }
    }
}

