namespace System.Web.UI.HtmlControls
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    [ControlBuilder(typeof(HtmlHeadBuilder))]
    public sealed class HtmlHead : HtmlGenericControl
    {
        private string _cachedDescription;
        private string _cachedKeywords;
        private string _cachedTitleText;
        private HtmlMeta _description;
        private HtmlMeta _keywords;
        private StyleSheetInternal _styleSheet;
        private HtmlTitle _title;

        public HtmlHead() : base("head")
        {
        }

        public HtmlHead(string tag) : base(tag)
        {
            if (tag == null)
            {
                tag = string.Empty;
            }
            base._tagName = tag;
        }

        protected internal override void AddedControl(Control control, int index)
        {
            base.AddedControl(control, index);
            if (control is HtmlTitle)
            {
                if (this._title != null)
                {
                    throw new HttpException(System.Web.SR.GetString("HtmlHead_OnlyOneTitleAllowed"));
                }
                this._title = (HtmlTitle) control;
            }
            else if (control is HtmlMeta)
            {
                HtmlMeta meta = (HtmlMeta) control;
                if ((this._description == null) && string.Equals(meta.Name, "description", StringComparison.OrdinalIgnoreCase))
                {
                    this._description = meta;
                }
                else if ((this._keywords == null) && string.Equals(meta.Name, "keywords", StringComparison.OrdinalIgnoreCase))
                {
                    this._keywords = meta;
                }
            }
        }

        protected internal override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            Page page = this.Page;
            if (page == null)
            {
                throw new HttpException(System.Web.SR.GetString("Head_Needs_Page"));
            }
            if (page.Header != null)
            {
                throw new HttpException(System.Web.SR.GetString("HtmlHead_OnlyOneHeadAllowed"));
            }
            page.SetHeader(this);
        }

        internal void RegisterCssStyleString(string outputString)
        {
            ((StyleSheetInternal) this.StyleSheet).CSSStyleString = outputString;
        }

        protected internal override void RemovedControl(Control control)
        {
            base.RemovedControl(control);
            if (control is HtmlTitle)
            {
                this._title = null;
            }
            else if (control == this._description)
            {
                this._description = null;
            }
            else if (control == this._keywords)
            {
                this._keywords = null;
            }
        }

        protected internal override void RenderChildren(HtmlTextWriter writer)
        {
            base.RenderChildren(writer);
            if (this._title == null)
            {
                writer.RenderBeginTag(HtmlTextWriterTag.Title);
                if (this._cachedTitleText != null)
                {
                    writer.Write(this._cachedTitleText);
                }
                writer.RenderEndTag();
            }
            if ((this._description == null) && !string.IsNullOrEmpty(this._cachedDescription))
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Name, "description");
                writer.AddAttribute(HtmlTextWriterAttribute.Content, this._cachedDescription);
                writer.RenderBeginTag(HtmlTextWriterTag.Meta);
                writer.RenderEndTag();
            }
            if ((this._keywords == null) && !string.IsNullOrEmpty(this._cachedKeywords))
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Name, "keywords");
                writer.AddAttribute(HtmlTextWriterAttribute.Content, this._cachedKeywords);
                writer.RenderBeginTag(HtmlTextWriterTag.Meta);
                writer.RenderEndTag();
            }
            if (this.Page.Request.Browser["requiresXhtmlCssSuppression"] != "true")
            {
                this.RenderStyleSheet(writer);
            }
        }

        internal static void RenderCssRule(CssTextWriter cssWriter, string selector, Style style, IUrlResolutionService urlResolver)
        {
            cssWriter.WriteBeginCssRule(selector);
            style.GetStyleAttributes(urlResolver).Render(cssWriter);
            cssWriter.WriteEndCssRule();
        }

        internal void RenderStyleSheet(HtmlTextWriter writer)
        {
            if (this._styleSheet != null)
            {
                this._styleSheet.Render(writer);
            }
        }

        public string Description
        {
            get
            {
                if (this._description == null)
                {
                    return this._cachedDescription;
                }
                return this._description.Content;
            }
            set
            {
                if (this._description == null)
                {
                    this._cachedDescription = value;
                }
                else
                {
                    this._description.Content = value;
                }
            }
        }

        public string Keywords
        {
            get
            {
                if (this._keywords == null)
                {
                    return this._cachedKeywords;
                }
                return this._keywords.Content;
            }
            set
            {
                if (this._keywords == null)
                {
                    this._cachedKeywords = value;
                }
                else
                {
                    this._keywords.Content = value;
                }
            }
        }

        public IStyleSheet StyleSheet
        {
            get
            {
                if (this._styleSheet == null)
                {
                    this._styleSheet = new StyleSheetInternal(this);
                }
                return this._styleSheet;
            }
        }

        public string Title
        {
            get
            {
                if (this._title == null)
                {
                    return this._cachedTitleText;
                }
                return this._title.Text;
            }
            set
            {
                if (this._title == null)
                {
                    this._cachedTitleText = value;
                }
                else
                {
                    this._title.Text = value;
                }
            }
        }

        private sealed class StyleSheetInternal : IStyleSheet, IUrlResolutionService
        {
            private int _autoGenCount;
            private string _cssStyleString;
            private HtmlHead _owner;
            private ArrayList _selectorStyles;
            private ArrayList _styles;

            public StyleSheetInternal(HtmlHead owner)
            {
                this._owner = owner;
            }

            public void Render(HtmlTextWriter writer)
            {
                if (((this._styles != null) || (this._selectorStyles != null)) || (this.CSSStyleString != null))
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Type, "text/css");
                    writer.RenderBeginTag(HtmlTextWriterTag.Style);
                    CssTextWriter cssWriter = new CssTextWriter(writer);
                    if (this._styles != null)
                    {
                        for (int i = 0; i < this._styles.Count; i++)
                        {
                            StyleInfo info = (StyleInfo) this._styles[i];
                            string registeredCssClass = info.style.RegisteredCssClass;
                            if (registeredCssClass.Length != 0)
                            {
                                HtmlHead.RenderCssRule(cssWriter, "." + registeredCssClass, info.style, info.urlResolver);
                            }
                        }
                    }
                    if (this._selectorStyles != null)
                    {
                        for (int j = 0; j < this._selectorStyles.Count; j++)
                        {
                            SelectorStyleInfo info2 = (SelectorStyleInfo) this._selectorStyles[j];
                            HtmlHead.RenderCssRule(cssWriter, info2.selector, info2.style, info2.urlResolver);
                        }
                    }
                    if (this.CSSStyleString != null)
                    {
                        writer.Write(this.CSSStyleString);
                    }
                    writer.RenderEndTag();
                }
            }

            void IStyleSheet.CreateStyleRule(Style style, IUrlResolutionService urlResolver, string selector)
            {
                if (style == null)
                {
                    throw new ArgumentNullException("style");
                }
                if (selector.Length == 0)
                {
                    throw new ArgumentNullException("selector");
                }
                if (this._selectorStyles == null)
                {
                    this._selectorStyles = new ArrayList();
                }
                if (urlResolver == null)
                {
                    urlResolver = this;
                }
                SelectorStyleInfo info = new SelectorStyleInfo {
                    selector = selector,
                    style = style,
                    urlResolver = urlResolver
                };
                this._selectorStyles.Add(info);
                Page page = this._owner.Page;
                if (page.PartialCachingControlStack != null)
                {
                    foreach (BasePartialCachingControl control in page.PartialCachingControlStack)
                    {
                        control.RegisterStyleInfo(info);
                    }
                }
            }

            void IStyleSheet.RegisterStyle(Style style, IUrlResolutionService urlResolver)
            {
                if (style == null)
                {
                    throw new ArgumentNullException("style");
                }
                if (this._styles == null)
                {
                    this._styles = new ArrayList();
                }
                else if (style.RegisteredCssClass.Length != 0)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("HtmlHead_StyleAlreadyRegistered"));
                }
                if (urlResolver == null)
                {
                    urlResolver = this;
                }
                StyleInfo info = new StyleInfo {
                    style = style,
                    urlResolver = urlResolver
                };
                string cssClass = "aspnet_s" + this._autoGenCount++.ToString(NumberFormatInfo.InvariantInfo);
                style.SetRegisteredCssClass(cssClass);
                this._styles.Add(info);
            }

            string IUrlResolutionService.ResolveClientUrl(string relativeUrl)
            {
                return this._owner.ResolveClientUrl(relativeUrl);
            }

            internal string CSSStyleString
            {
                get
                {
                    return this._cssStyleString;
                }
                set
                {
                    this._cssStyleString = value;
                }
            }

            private sealed class StyleInfo
            {
                public Style style;
                public IUrlResolutionService urlResolver;
            }
        }
    }
}

