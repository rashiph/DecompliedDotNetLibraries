namespace System.Web.UI.WebControls.WebParts
{
    using System;
    using System.Drawing;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    public class EditorPartChrome
    {
        private Style _chromeStyleNoBorder;
        private Style _titleTextStyle;
        private EditorZoneBase _zone;

        public EditorPartChrome(EditorZoneBase zone)
        {
            if (zone == null)
            {
                throw new ArgumentNullException("zone");
            }
            this._zone = zone;
        }

        protected virtual Style CreateEditorPartChromeStyle(EditorPart editorPart, PartChromeType chromeType)
        {
            if (editorPart == null)
            {
                throw new ArgumentNullException("editorPart");
            }
            if ((chromeType < PartChromeType.Default) || (chromeType > PartChromeType.BorderOnly))
            {
                throw new ArgumentOutOfRangeException("chromeType");
            }
            if ((chromeType == PartChromeType.BorderOnly) || (chromeType == PartChromeType.TitleAndBorder))
            {
                return this.Zone.PartChromeStyle;
            }
            if (this._chromeStyleNoBorder == null)
            {
                Style style = new Style();
                style.CopyFrom(this.Zone.PartChromeStyle);
                if (style.BorderStyle != BorderStyle.None)
                {
                    style.BorderStyle = BorderStyle.None;
                }
                if (style.BorderWidth != Unit.Empty)
                {
                    style.BorderWidth = Unit.Empty;
                }
                if (style.BorderColor != Color.Empty)
                {
                    style.BorderColor = Color.Empty;
                }
                this._chromeStyleNoBorder = style;
            }
            return this._chromeStyleNoBorder;
        }

        public virtual void PerformPreRender()
        {
        }

        public virtual void RenderEditorPart(HtmlTextWriter writer, EditorPart editorPart)
        {
            if (editorPart == null)
            {
                throw new ArgumentNullException("editorPart");
            }
            PartChromeType effectiveChromeType = this.Zone.GetEffectiveChromeType(editorPart);
            Style style = this.CreateEditorPartChromeStyle(editorPart, effectiveChromeType);
            if (!style.IsEmpty)
            {
                style.AddAttributesToRender(writer, this.Zone);
            }
            writer.RenderBeginTag(HtmlTextWriterTag.Fieldset);
            switch (effectiveChromeType)
            {
                case PartChromeType.TitleAndBorder:
                case PartChromeType.TitleOnly:
                    this.RenderTitle(writer, editorPart);
                    break;
            }
            if (editorPart.ChromeState != PartChromeState.Minimized)
            {
                Style partStyle = this.Zone.PartStyle;
                if (!partStyle.IsEmpty)
                {
                    partStyle.AddAttributesToRender(writer, this.Zone);
                }
                writer.RenderBeginTag(HtmlTextWriterTag.Div);
                this.RenderPartContents(writer, editorPart);
                writer.RenderEndTag();
            }
            writer.RenderEndTag();
        }

        protected virtual void RenderPartContents(HtmlTextWriter writer, EditorPart editorPart)
        {
            string accessKey = editorPart.AccessKey;
            if (!string.IsNullOrEmpty(accessKey))
            {
                editorPart.AccessKey = string.Empty;
            }
            editorPart.RenderControl(writer);
            if (!string.IsNullOrEmpty(accessKey))
            {
                editorPart.AccessKey = accessKey;
            }
        }

        private void RenderTitle(HtmlTextWriter writer, EditorPart editorPart)
        {
            string displayTitle = editorPart.DisplayTitle;
            if (!string.IsNullOrEmpty(displayTitle))
            {
                TableItemStyle partTitleStyle = this.Zone.PartTitleStyle;
                if (this._titleTextStyle == null)
                {
                    Style style2 = new Style();
                    style2.CopyFrom(partTitleStyle);
                    this._titleTextStyle = style2;
                }
                if (!this._titleTextStyle.IsEmpty)
                {
                    this._titleTextStyle.AddAttributesToRender(writer, this.Zone);
                }
                string description = editorPart.Description;
                if (!string.IsNullOrEmpty(description))
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Title, description);
                }
                string accessKey = editorPart.AccessKey;
                if (!string.IsNullOrEmpty(accessKey))
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Accesskey, accessKey);
                }
                writer.RenderBeginTag(HtmlTextWriterTag.Legend);
                writer.Write(displayTitle);
                writer.RenderEndTag();
            }
        }

        protected EditorZoneBase Zone
        {
            get
            {
                return this._zone;
            }
        }
    }
}

