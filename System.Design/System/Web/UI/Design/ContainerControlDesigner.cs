namespace System.Web.UI.Design
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Design;
    using System.Drawing;
    using System.Globalization;
    using System.Security.Permissions;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public class ContainerControlDesigner : ControlDesigner
    {
        private Style _defaultFrameStyle = new Style();
        private const string ContainerControlNoCaptionDesignTimeHtml = "<div height=\"{8}\" width=\"{9}\" style=\"{0}{2}{3}{4}{6}{10}\" {7}=0></div>";
        private const string ContainerControlWithCaptionDesignTimeHtml = "<table height=\"{8}\" width=\"{9}\" style=\"{0}{2}{10}\" cellpadding=1 cellspacing=0>\r\n                <tr>\r\n                    <td nowrap align=center valign=middle style=\"{1}{2}{3}{4}\">{5}</td>\r\n                </tr>\r\n                <tr>\r\n                    <td nowrap style=\"vertical-align:top;{6}{10}\" {7}=0></td>\r\n                </tr>\r\n            </table>";

        public ContainerControlDesigner()
        {
            this._defaultFrameStyle.Font.Name = "Tahoma";
            this._defaultFrameStyle.ForeColor = SystemColors.ControlText;
            this._defaultFrameStyle.BackColor = SystemColors.Control;
        }

        protected virtual void AddDesignTimeCssAttributes(IDictionary styleAttributes)
        {
            if (base.IsWebControl)
            {
                WebControl viewControl = base.ViewControl as WebControl;
                Unit width = viewControl.Width;
                if (!width.IsEmpty && (width.Value != 0.0))
                {
                    styleAttributes["width"] = width.ToString(CultureInfo.InvariantCulture);
                }
                Unit height = viewControl.Height;
                if (!height.IsEmpty && (height.Value != 0.0))
                {
                    styleAttributes["height"] = height.ToString(CultureInfo.InvariantCulture);
                }
                string str = ColorTranslator.ToHtml(viewControl.BackColor);
                if (str.Length > 0)
                {
                    styleAttributes["background-color"] = str;
                }
                str = ColorTranslator.ToHtml(viewControl.ForeColor);
                if (str.Length > 0)
                {
                    styleAttributes["color"] = str;
                }
                str = ColorTranslator.ToHtml(viewControl.BorderColor);
                if (str.Length > 0)
                {
                    styleAttributes["border-color"] = str;
                }
                Unit borderWidth = viewControl.BorderWidth;
                if (!borderWidth.IsEmpty && (borderWidth.Value != 0.0))
                {
                    styleAttributes["border-width"] = borderWidth.ToString(CultureInfo.InvariantCulture);
                }
                BorderStyle borderStyle = viewControl.BorderStyle;
                if (borderStyle != BorderStyle.NotSet)
                {
                    styleAttributes["border-style"] = borderStyle;
                }
                else if (!borderWidth.IsEmpty && (borderWidth.Value != 0.0))
                {
                    styleAttributes["border-style"] = BorderStyle.Solid;
                }
                string name = viewControl.Font.Name;
                if (name.Length != 0)
                {
                    styleAttributes["font-family"] = HttpUtility.HtmlEncode(name);
                }
                FontUnit size = viewControl.Font.Size;
                if (size != FontUnit.Empty)
                {
                    styleAttributes["font-size"] = size.ToString(CultureInfo.InvariantCulture);
                }
                if (viewControl.Font.Bold)
                {
                    styleAttributes["font-weight"] = "bold";
                }
                if (viewControl.Font.Italic)
                {
                    styleAttributes["font-style"] = "italic";
                }
                string str3 = string.Empty;
                if (viewControl.Font.Underline)
                {
                    str3 = str3 + "underline ";
                }
                if (viewControl.Font.Strikeout)
                {
                    str3 = str3 + "line-through";
                }
                if (viewControl.Font.Overline)
                {
                    str3 = str3 + "overline";
                }
                if (str3.Length > 0)
                {
                    styleAttributes["text-decoration"] = str3.Trim();
                }
            }
        }

        private IDictionary ConvertFontInfoToCss(FontInfo font)
        {
            IDictionary dictionary = new HybridDictionary();
            string name = font.Name;
            if (name.Length != 0)
            {
                dictionary["font-family"] = HttpUtility.HtmlEncode(name);
            }
            FontUnit size = font.Size;
            if (size != FontUnit.Empty)
            {
                dictionary["font-size"] = size.ToString(CultureInfo.CurrentCulture);
            }
            if (font.Bold)
            {
                dictionary["font-weight"] = "bold";
            }
            if (font.Italic)
            {
                dictionary["font-style"] = "italic";
            }
            string str2 = string.Empty;
            if (font.Underline)
            {
                str2 = str2 + "underline ";
            }
            if (font.Strikeout)
            {
                str2 = str2 + "line-through";
            }
            if (font.Overline)
            {
                str2 = str2 + "overline";
            }
            if (str2.Length > 0)
            {
                dictionary["text-decoration"] = str2.Trim();
            }
            return dictionary;
        }

        private string CssDictionaryToString(IDictionary dictionary)
        {
            string str = string.Empty;
            if (dictionary != null)
            {
                foreach (DictionaryEntry entry in dictionary)
                {
                    object obj2 = str;
                    str = string.Concat(new object[] { obj2, entry.Key, ":", entry.Value, "; " });
                }
            }
            return str;
        }

        private string GenerateDesignTimeHtml()
        {
            string frameCaption = this.FrameCaption;
            Unit height = this.FrameStyle.Height;
            Unit width = this.FrameStyle.Width;
            string cssClass = string.Empty;
            if (base.IsWebControl)
            {
                WebControl control = (WebControl) base.ViewControl;
                if (height.IsEmpty)
                {
                    height = control.Height;
                }
                if (width.IsEmpty)
                {
                    width = control.Width;
                }
                cssClass = control.CssClass;
            }
            if (frameCaption == null)
            {
                frameCaption = string.Empty;
            }
            else if (frameCaption.IndexOf('\0') > -1)
            {
                frameCaption = frameCaption.Replace("\0", string.Empty);
            }
            string str3 = string.Empty;
            WebControl viewControl = base.ViewControl as WebControl;
            if (viewControl != null)
            {
                str3 = viewControl.Style.Value + ";";
            }
            string str4 = string.Empty;
            string str5 = string.Empty;
            if ((!this.FrameStyle.BorderWidth.IsEmpty && (this.FrameStyle.BorderWidth.Value != 0.0)) || ((this.FrameStyle.BorderStyle != BorderStyle.NotSet) || !this.FrameStyle.BorderColor.IsEmpty))
            {
                str4 = string.Format(CultureInfo.InvariantCulture, "border:{0} {1} {2}; ", new object[] { this.FrameStyle.BorderWidth, this.FrameStyle.BorderStyle, ColorTranslator.ToHtml(this.FrameStyle.BorderColor) });
                str5 = string.Format(CultureInfo.InvariantCulture, "border-bottom:{0} {1} {2}; ", new object[] { this.FrameStyle.BorderWidth, this.FrameStyle.BorderStyle, ColorTranslator.ToHtml(this.FrameStyle.BorderColor) });
            }
            string str6 = string.Empty;
            if (!this.FrameStyle.ForeColor.IsEmpty)
            {
                str6 = string.Format(CultureInfo.InvariantCulture, "color:{0}; ", new object[] { ColorTranslator.ToHtml(this.FrameStyle.ForeColor) });
            }
            string str7 = string.Empty;
            if (!this.FrameStyle.BackColor.IsEmpty)
            {
                str7 = string.Format(CultureInfo.InvariantCulture, "background-color:{0}; ", new object[] { ColorTranslator.ToHtml(this.FrameStyle.BackColor) });
            }
            return string.Format(CultureInfo.InvariantCulture, this.DesignTimeHtml, new object[] { str4, str5, str6, str7, this.CssDictionaryToString(this.ConvertFontInfoToCss(this.FrameStyle.Font)), frameCaption, this.CssDictionaryToString(this.GetDesignTimeCssAttributes()), DesignerRegion.DesignerRegionAttributeName, height, width, str3, cssClass });
        }

        public virtual IDictionary GetDesignTimeCssAttributes()
        {
            IDictionary styleAttributes = new HybridDictionary();
            this.AddDesignTimeCssAttributes(styleAttributes);
            return styleAttributes;
        }

        public override string GetDesignTimeHtml(DesignerRegionCollection regions)
        {
            Control component = (Control) base.Component;
            component.Controls.Clear();
            EditableDesignerRegion region = new EditableDesignerRegion(this, "Content") {
                Description = System.Design.SR.GetString("ContainerControlDesigner_RegionWatermark")
            };
            region.Properties[typeof(Control)] = component;
            region.EnsureSize = true;
            regions.Add(region);
            return this.GenerateDesignTimeHtml();
        }

        public override string GetEditableDesignerRegionContent(EditableDesignerRegion region)
        {
            string content = string.Empty;
            if (base.Tag != null)
            {
                try
                {
                    content = base.Tag.GetContent();
                    if (content == null)
                    {
                        content = string.Empty;
                    }
                }
                catch (Exception)
                {
                }
            }
            return content;
        }

        public override string GetPersistenceContent()
        {
            if (base.LocalizedInnerContent != null)
            {
                return base.LocalizedInnerContent;
            }
            return null;
        }

        public override void SetEditableDesignerRegionContent(EditableDesignerRegion region, string content)
        {
            if (base.Tag != null)
            {
                try
                {
                    base.Tag.SetContent(content);
                }
                catch (Exception)
                {
                }
            }
        }

        public override bool AllowResize
        {
            get
            {
                return true;
            }
        }

        internal virtual string DesignTimeHtml
        {
            get
            {
                if (!string.IsNullOrEmpty(this.FrameCaption))
                {
                    return "<table height=\"{8}\" width=\"{9}\" style=\"{0}{2}{10}\" cellpadding=1 cellspacing=0>\r\n                <tr>\r\n                    <td nowrap align=center valign=middle style=\"{1}{2}{3}{4}\">{5}</td>\r\n                </tr>\r\n                <tr>\r\n                    <td nowrap style=\"vertical-align:top;{6}{10}\" {7}=0></td>\r\n                </tr>\r\n            </table>";
                }
                return "<div height=\"{8}\" width=\"{9}\" style=\"{0}{2}{3}{4}{6}{10}\" {7}=0></div>";
            }
        }

        public virtual string FrameCaption
        {
            get
            {
                return this.ID;
            }
        }

        public virtual Style FrameStyle
        {
            get
            {
                return this._defaultFrameStyle;
            }
        }

        internal Style FrameStyleInternal
        {
            get
            {
                return this._defaultFrameStyle;
            }
        }
    }
}

