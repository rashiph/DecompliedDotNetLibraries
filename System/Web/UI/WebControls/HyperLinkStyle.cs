namespace System.Web.UI.WebControls
{
    using System;
    using System.Drawing;
    using System.Globalization;
    using System.Web.UI;

    internal sealed class HyperLinkStyle : Style
    {
        private bool _doNotRenderDefaults;
        private Style _owner;

        public HyperLinkStyle(Style owner)
        {
            this._owner = owner;
        }

        public sealed override void AddAttributesToRender(HtmlTextWriter writer, WebControl owner)
        {
            string cssClass = string.Empty;
            bool flag = true;
            if (this._owner.IsSet(2))
            {
                cssClass = this._owner.CssClass;
            }
            if (base.RegisteredCssClass.Length != 0)
            {
                flag = false;
                if (cssClass.Length != 0)
                {
                    cssClass = cssClass + " " + base.RegisteredCssClass;
                }
                else
                {
                    cssClass = base.RegisteredCssClass;
                }
            }
            if (cssClass.Length > 0)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Class, cssClass);
            }
            if (flag)
            {
                base.GetStyleAttributes(owner).Render(writer);
            }
        }

        protected sealed override void FillStyleAttributes(CssStyleCollection attributes, IUrlResolutionService urlResolver)
        {
            StateBag viewState = base.ViewState;
            if (this._owner.IsSet(4))
            {
                Color foreColor = this._owner.ForeColor;
                if (!foreColor.IsEmpty)
                {
                    attributes.Add(HtmlTextWriterStyle.Color, ColorTranslator.ToHtml(foreColor));
                }
            }
            FontInfo font = this._owner.Font;
            string[] names = font.Names;
            if (names.Length > 0)
            {
                attributes.Add(HtmlTextWriterStyle.FontFamily, string.Join(",", names));
            }
            FontUnit size = font.Size;
            if (!size.IsEmpty)
            {
                attributes.Add(HtmlTextWriterStyle.FontSize, size.ToString(CultureInfo.InvariantCulture));
            }
            if (this._owner.IsSet(0x800))
            {
                if (font.Bold)
                {
                    attributes.Add(HtmlTextWriterStyle.FontWeight, "bold");
                }
                else
                {
                    attributes.Add(HtmlTextWriterStyle.FontWeight, "normal");
                }
            }
            if (this._owner.IsSet(0x1000))
            {
                if (font.Italic)
                {
                    attributes.Add(HtmlTextWriterStyle.FontStyle, "italic");
                }
                else
                {
                    attributes.Add(HtmlTextWriterStyle.FontStyle, "normal");
                }
            }
            string str = string.Empty;
            if (font.Underline)
            {
                str = "underline";
            }
            if (font.Overline)
            {
                str = str + " overline";
            }
            if (font.Strikeout)
            {
                str = str + " line-through";
            }
            if (str.Length > 0)
            {
                attributes.Add(HtmlTextWriterStyle.TextDecoration, str);
            }
            else if (!this.DoNotRenderDefaults)
            {
                attributes.Add(HtmlTextWriterStyle.TextDecoration, "none");
            }
            if (this._owner.IsSet(2))
            {
                attributes.Add(HtmlTextWriterStyle.BorderStyle, "none");
            }
        }

        public bool DoNotRenderDefaults
        {
            get
            {
                return this._doNotRenderDefaults;
            }
            set
            {
                this._doNotRenderDefaults = value;
            }
        }

        public sealed override bool IsEmpty
        {
            get
            {
                if (base.RegisteredCssClass.Length != 0)
                {
                    return false;
                }
                return ((((!this._owner.IsSet(2) && !this._owner.IsSet(4)) && (!this._owner.IsSet(0x200) && !this._owner.IsSet(0x400))) && ((!this._owner.IsSet(0x800) && !this._owner.IsSet(0x1000)) && (!this._owner.IsSet(0x2000) && !this._owner.IsSet(0x4000)))) && !this._owner.IsSet(0x8000));
            }
        }
    }
}

