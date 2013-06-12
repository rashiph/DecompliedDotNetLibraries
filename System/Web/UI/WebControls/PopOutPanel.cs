namespace System.Web.UI.WebControls
{
    using System;
    using System.Drawing;
    using System.Web.UI;

    internal sealed class PopOutPanel : Panel
    {
        private PopOutPanelStyle _emptyPopOutPanelStyle;
        private Menu _owner;
        private string _scrollerClass;
        private Style _scrollerStyle;
        private Style _style;

        public PopOutPanel(Menu owner, Style style)
        {
            this._owner = owner;
            this._style = style;
            this._emptyPopOutPanelStyle = new PopOutPanelStyle(null);
        }

        protected override void AddAttributesToRender(HtmlTextWriter writer)
        {
            string cssClass = this.CssClass;
            Style style = this._style;
            this.CssClass = string.Empty;
            this._style = null;
            base.ControlStyle.Reset();
            base.AddAttributesToRender(writer);
            this.CssClass = cssClass;
            this._style = style;
            this.RenderStyleAttributes(writer);
        }

        internal PopOutPanelStyle GetEmptyPopOutPanelStyle()
        {
            return this._emptyPopOutPanelStyle;
        }

        public override void RenderEndTag(HtmlTextWriter writer)
        {
            if (!this._owner.DesignMode)
            {
                this.RenderScrollerAttributes(writer);
                writer.AddAttribute(HtmlTextWriterAttribute.Id, this.ClientID + "Up");
                writer.AddAttribute("onmouseover", "PopOut_Up(this)");
                writer.AddAttribute("onmouseout", "PopOut_Stop(this)");
                writer.RenderBeginTag(HtmlTextWriterTag.Div);
                string scrollUpImageUrl = this._owner.ScrollUpImageUrl;
                if (scrollUpImageUrl.Length != 0)
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Src, this._owner.ResolveClientUrl(scrollUpImageUrl));
                }
                else
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Src, this._owner.GetImageUrl(0));
                }
                writer.AddAttribute(HtmlTextWriterAttribute.Alt, this._owner.ScrollUpText);
                writer.RenderBeginTag(HtmlTextWriterTag.Img);
                writer.RenderEndTag();
                writer.RenderEndTag();
                this.RenderScrollerAttributes(writer);
                writer.AddAttribute(HtmlTextWriterAttribute.Id, this.ClientID + "Dn");
                writer.AddAttribute("onmouseover", "PopOut_Down(this)");
                writer.AddAttribute("onmouseout", "PopOut_Stop(this)");
                writer.RenderBeginTag(HtmlTextWriterTag.Div);
                string scrollDownImageUrl = this._owner.ScrollDownImageUrl;
                if (scrollDownImageUrl.Length != 0)
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Src, this._owner.ResolveClientUrl(scrollDownImageUrl));
                }
                else
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Src, this._owner.GetImageUrl(1));
                }
                writer.AddAttribute(HtmlTextWriterAttribute.Alt, this._owner.ScrollDownText);
                writer.RenderBeginTag(HtmlTextWriterTag.Img);
                writer.RenderEndTag();
                writer.RenderEndTag();
            }
            base.RenderEndTag(writer);
        }

        private void RenderScrollerAttributes(HtmlTextWriter writer)
        {
            if ((this.Page != null) && this.Page.SupportsStyleSheets)
            {
                if (!string.IsNullOrEmpty(this.ScrollerClass))
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, this.ScrollerClass + ' ' + this.GetEmptyPopOutPanelStyle().RegisteredCssClass);
                }
                else
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, this.GetEmptyPopOutPanelStyle().RegisteredCssClass);
                }
            }
            else
            {
                if ((this.ScrollerStyle != null) && !this.ScrollerStyle.IsEmpty)
                {
                    this.ScrollerStyle.AddAttributesToRender(writer);
                }
                if (this.ScrollerStyle.BackColor.IsEmpty)
                {
                    writer.AddStyleAttribute(HtmlTextWriterStyle.BackgroundColor, "white");
                }
                writer.AddStyleAttribute(HtmlTextWriterStyle.Visibility, "hidden");
                writer.AddStyleAttribute(HtmlTextWriterStyle.Display, "none");
                writer.AddStyleAttribute(HtmlTextWriterStyle.Position, "absolute");
                writer.AddStyleAttribute(HtmlTextWriterStyle.Left, "0px");
                writer.AddStyleAttribute(HtmlTextWriterStyle.Top, "0px");
            }
            writer.AddStyleAttribute(HtmlTextWriterStyle.TextAlign, "center");
        }

        private void RenderStyleAttributes(HtmlTextWriter writer)
        {
            if (this._style == null)
            {
                if (!string.IsNullOrEmpty(this.CssClass))
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, this.CssClass);
                }
                else
                {
                    if (this.BackColor.IsEmpty)
                    {
                        writer.AddStyleAttribute(HtmlTextWriterStyle.BackgroundColor, "white");
                    }
                    else
                    {
                        writer.AddStyleAttribute(HtmlTextWriterStyle.BackgroundColor, ColorTranslator.ToHtml(this.BackColor));
                    }
                    if (!this._owner.DesignMode)
                    {
                        writer.AddStyleAttribute(HtmlTextWriterStyle.Visibility, "hidden");
                        writer.AddStyleAttribute(HtmlTextWriterStyle.Display, "none");
                        writer.AddStyleAttribute(HtmlTextWriterStyle.Position, "absolute");
                        writer.AddStyleAttribute(HtmlTextWriterStyle.Left, "0px");
                        writer.AddStyleAttribute(HtmlTextWriterStyle.Top, "0px");
                    }
                }
            }
            else
            {
                if ((this.Page != null) && this.Page.SupportsStyleSheets)
                {
                    string registeredCssClass = this._style.RegisteredCssClass;
                    if (registeredCssClass.Trim().Length > 0)
                    {
                        writer.AddAttribute(HtmlTextWriterAttribute.Class, !string.IsNullOrEmpty(this.CssClass) ? (registeredCssClass + ' ' + this.CssClass) : registeredCssClass);
                        return;
                    }
                }
                if (!string.IsNullOrEmpty(this.CssClass))
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, this.CssClass);
                }
                else
                {
                    this._style.AddAttributesToRender(writer);
                }
            }
        }

        internal void SetInternalStyle(Style style)
        {
            this._style = style;
        }

        public override System.Web.UI.WebControls.ScrollBars ScrollBars
        {
            get
            {
                return System.Web.UI.WebControls.ScrollBars.None;
            }
        }

        internal string ScrollerClass
        {
            get
            {
                return this._scrollerClass;
            }
            set
            {
                this._scrollerClass = value;
            }
        }

        internal Style ScrollerStyle
        {
            get
            {
                return this._scrollerStyle;
            }
            set
            {
                this._scrollerStyle = value;
            }
        }

        internal sealed class PopOutPanelStyle : SubMenuStyle
        {
            private PopOutPanel _owner;

            public PopOutPanelStyle(PopOutPanel owner)
            {
                this._owner = owner;
            }

            protected override void FillStyleAttributes(CssStyleCollection attributes, IUrlResolutionService urlResolver)
            {
                if (base.BackColor.IsEmpty && ((this._owner == null) || this._owner.BackColor.IsEmpty))
                {
                    attributes.Add(HtmlTextWriterStyle.BackgroundColor, "white");
                }
                attributes.Add(HtmlTextWriterStyle.Visibility, "hidden");
                attributes.Add(HtmlTextWriterStyle.Display, "none");
                attributes.Add(HtmlTextWriterStyle.Position, "absolute");
                attributes.Add(HtmlTextWriterStyle.Left, "0px");
                attributes.Add(HtmlTextWriterStyle.Top, "0px");
                base.FillStyleAttributes(attributes, urlResolver);
            }
        }
    }
}

