namespace System.Web.UI
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Web;
    using System.Web.UI.WebControls;
    using System.Web.Util;

    public class HtmlTextWriter : TextWriter
    {
        private int _attrCount;
        private static Hashtable _attrKeyLookupTable;
        private RenderAttribute[] _attrList;
        private static AttributeInformation[] _attrNameLookupArray;
        private Layout _currentLayout;
        private Layout _currentWrittenLayout;
        private int _endTagCount;
        private TagStackEntry[] _endTags;
        private HttpWriter _httpWriter;
        private int _inlineCount;
        private bool _isDescendant;
        private int _styleCount;
        private RenderStyle[] _styleList;
        private int _tagIndex;
        private HtmlTextWriterTag _tagKey;
        private static Hashtable _tagKeyLookupTable = new Hashtable(0x61);
        private string _tagName;
        private static TagInformation[] _tagNameLookupArray = new TagInformation[0x61];
        public const string DefaultTabString = "\t";
        internal const string DesignerRegionAttributeName = "_designerRegion";
        public const char DoubleQuoteChar = '"';
        public const string EndTagLeftChars = "</";
        public const char EqualsChar = '=';
        public const string EqualsDoubleQuoteString = "=\"";
        private int indentLevel;
        public const string SelfClosingChars = " /";
        public const string SelfClosingTagEnd = " />";
        public const char SemicolonChar = ';';
        public const char SingleQuoteChar = '\'';
        public const char SlashChar = '/';
        public const char SpaceChar = ' ';
        public const char StyleEqualsChar = ':';
        private bool tabsPending;
        private string tabString;
        public const char TagLeftChar = '<';
        public const char TagRightChar = '>';
        private TextWriter writer;

        static HtmlTextWriter()
        {
            RegisterTag(string.Empty, HtmlTextWriterTag.Unknown, TagType.Other);
            RegisterTag("a", HtmlTextWriterTag.A, TagType.Inline);
            RegisterTag("acronym", HtmlTextWriterTag.Acronym, TagType.Inline);
            RegisterTag("address", HtmlTextWriterTag.Address, TagType.Other);
            RegisterTag("area", HtmlTextWriterTag.Area, TagType.NonClosing);
            RegisterTag("b", HtmlTextWriterTag.B, TagType.Inline);
            RegisterTag("base", HtmlTextWriterTag.Base, TagType.NonClosing);
            RegisterTag("basefont", HtmlTextWriterTag.Basefont, TagType.NonClosing);
            RegisterTag("bdo", HtmlTextWriterTag.Bdo, TagType.Inline);
            RegisterTag("bgsound", HtmlTextWriterTag.Bgsound, TagType.NonClosing);
            RegisterTag("big", HtmlTextWriterTag.Big, TagType.Inline);
            RegisterTag("blockquote", HtmlTextWriterTag.Blockquote, TagType.Other);
            RegisterTag("body", HtmlTextWriterTag.Body, TagType.Other);
            RegisterTag("br", HtmlTextWriterTag.Br, TagType.Other);
            RegisterTag("button", HtmlTextWriterTag.Button, TagType.Inline);
            RegisterTag("caption", HtmlTextWriterTag.Caption, TagType.Other);
            RegisterTag("center", HtmlTextWriterTag.Center, TagType.Other);
            RegisterTag("cite", HtmlTextWriterTag.Cite, TagType.Inline);
            RegisterTag("code", HtmlTextWriterTag.Code, TagType.Inline);
            RegisterTag("col", HtmlTextWriterTag.Col, TagType.NonClosing);
            RegisterTag("colgroup", HtmlTextWriterTag.Colgroup, TagType.Other);
            RegisterTag("del", HtmlTextWriterTag.Del, TagType.Inline);
            RegisterTag("dd", HtmlTextWriterTag.Dd, TagType.Inline);
            RegisterTag("dfn", HtmlTextWriterTag.Dfn, TagType.Inline);
            RegisterTag("dir", HtmlTextWriterTag.Dir, TagType.Other);
            RegisterTag("div", HtmlTextWriterTag.Div, TagType.Other);
            RegisterTag("dl", HtmlTextWriterTag.Dl, TagType.Other);
            RegisterTag("dt", HtmlTextWriterTag.Dt, TagType.Inline);
            RegisterTag("em", HtmlTextWriterTag.Em, TagType.Inline);
            RegisterTag("embed", HtmlTextWriterTag.Embed, TagType.NonClosing);
            RegisterTag("fieldset", HtmlTextWriterTag.Fieldset, TagType.Other);
            RegisterTag("font", HtmlTextWriterTag.Font, TagType.Inline);
            RegisterTag("form", HtmlTextWriterTag.Form, TagType.Other);
            RegisterTag("frame", HtmlTextWriterTag.Frame, TagType.NonClosing);
            RegisterTag("frameset", HtmlTextWriterTag.Frameset, TagType.Other);
            RegisterTag("h1", HtmlTextWriterTag.H1, TagType.Other);
            RegisterTag("h2", HtmlTextWriterTag.H2, TagType.Other);
            RegisterTag("h3", HtmlTextWriterTag.H3, TagType.Other);
            RegisterTag("h4", HtmlTextWriterTag.H4, TagType.Other);
            RegisterTag("h5", HtmlTextWriterTag.H5, TagType.Other);
            RegisterTag("h6", HtmlTextWriterTag.H6, TagType.Other);
            RegisterTag("head", HtmlTextWriterTag.Head, TagType.Other);
            RegisterTag("hr", HtmlTextWriterTag.Hr, TagType.NonClosing);
            RegisterTag("html", HtmlTextWriterTag.Html, TagType.Other);
            RegisterTag("i", HtmlTextWriterTag.I, TagType.Inline);
            RegisterTag("iframe", HtmlTextWriterTag.Iframe, TagType.Other);
            RegisterTag("img", HtmlTextWriterTag.Img, TagType.NonClosing);
            RegisterTag("input", HtmlTextWriterTag.Input, TagType.NonClosing);
            RegisterTag("ins", HtmlTextWriterTag.Ins, TagType.Inline);
            RegisterTag("isindex", HtmlTextWriterTag.Isindex, TagType.NonClosing);
            RegisterTag("kbd", HtmlTextWriterTag.Kbd, TagType.Inline);
            RegisterTag("label", HtmlTextWriterTag.Label, TagType.Inline);
            RegisterTag("legend", HtmlTextWriterTag.Legend, TagType.Other);
            RegisterTag("li", HtmlTextWriterTag.Li, TagType.Inline);
            RegisterTag("link", HtmlTextWriterTag.Link, TagType.NonClosing);
            RegisterTag("map", HtmlTextWriterTag.Map, TagType.Other);
            RegisterTag("marquee", HtmlTextWriterTag.Marquee, TagType.Other);
            RegisterTag("menu", HtmlTextWriterTag.Menu, TagType.Other);
            RegisterTag("meta", HtmlTextWriterTag.Meta, TagType.NonClosing);
            RegisterTag("nobr", HtmlTextWriterTag.Nobr, TagType.Inline);
            RegisterTag("noframes", HtmlTextWriterTag.Noframes, TagType.Other);
            RegisterTag("noscript", HtmlTextWriterTag.Noscript, TagType.Other);
            RegisterTag("object", HtmlTextWriterTag.Object, TagType.Other);
            RegisterTag("ol", HtmlTextWriterTag.Ol, TagType.Other);
            RegisterTag("option", HtmlTextWriterTag.Option, TagType.Other);
            RegisterTag("p", HtmlTextWriterTag.P, TagType.Inline);
            RegisterTag("param", HtmlTextWriterTag.Param, TagType.Other);
            RegisterTag("pre", HtmlTextWriterTag.Pre, TagType.Other);
            RegisterTag("ruby", HtmlTextWriterTag.Ruby, TagType.Other);
            RegisterTag("rt", HtmlTextWriterTag.Rt, TagType.Other);
            RegisterTag("q", HtmlTextWriterTag.Q, TagType.Inline);
            RegisterTag("s", HtmlTextWriterTag.S, TagType.Inline);
            RegisterTag("samp", HtmlTextWriterTag.Samp, TagType.Inline);
            RegisterTag("script", HtmlTextWriterTag.Script, TagType.Other);
            RegisterTag("select", HtmlTextWriterTag.Select, TagType.Other);
            RegisterTag("small", HtmlTextWriterTag.Small, TagType.Other);
            RegisterTag("span", HtmlTextWriterTag.Span, TagType.Inline);
            RegisterTag("strike", HtmlTextWriterTag.Strike, TagType.Inline);
            RegisterTag("strong", HtmlTextWriterTag.Strong, TagType.Inline);
            RegisterTag("style", HtmlTextWriterTag.Style, TagType.Other);
            RegisterTag("sub", HtmlTextWriterTag.Sub, TagType.Inline);
            RegisterTag("sup", HtmlTextWriterTag.Sup, TagType.Inline);
            RegisterTag("table", HtmlTextWriterTag.Table, TagType.Other);
            RegisterTag("tbody", HtmlTextWriterTag.Tbody, TagType.Other);
            RegisterTag("td", HtmlTextWriterTag.Td, TagType.Inline);
            RegisterTag("textarea", HtmlTextWriterTag.Textarea, TagType.Inline);
            RegisterTag("tfoot", HtmlTextWriterTag.Tfoot, TagType.Other);
            RegisterTag("th", HtmlTextWriterTag.Th, TagType.Inline);
            RegisterTag("thead", HtmlTextWriterTag.Thead, TagType.Other);
            RegisterTag("title", HtmlTextWriterTag.Title, TagType.Other);
            RegisterTag("tr", HtmlTextWriterTag.Tr, TagType.Other);
            RegisterTag("tt", HtmlTextWriterTag.Tt, TagType.Inline);
            RegisterTag("u", HtmlTextWriterTag.U, TagType.Inline);
            RegisterTag("ul", HtmlTextWriterTag.Ul, TagType.Other);
            RegisterTag("var", HtmlTextWriterTag.Var, TagType.Inline);
            RegisterTag("wbr", HtmlTextWriterTag.Wbr, TagType.NonClosing);
            RegisterTag("xml", HtmlTextWriterTag.Xml, TagType.Other);
            _attrKeyLookupTable = new Hashtable(0x36);
            _attrNameLookupArray = new AttributeInformation[0x36];
            RegisterAttribute("abbr", HtmlTextWriterAttribute.Abbr, true);
            RegisterAttribute("accesskey", HtmlTextWriterAttribute.Accesskey, true);
            RegisterAttribute("align", HtmlTextWriterAttribute.Align, false);
            RegisterAttribute("alt", HtmlTextWriterAttribute.Alt, true);
            RegisterAttribute("autocomplete", HtmlTextWriterAttribute.AutoComplete, false);
            RegisterAttribute("axis", HtmlTextWriterAttribute.Axis, true);
            RegisterAttribute("background", HtmlTextWriterAttribute.Background, true, true);
            RegisterAttribute("bgcolor", HtmlTextWriterAttribute.Bgcolor, false);
            RegisterAttribute("border", HtmlTextWriterAttribute.Border, false);
            RegisterAttribute("bordercolor", HtmlTextWriterAttribute.Bordercolor, false);
            RegisterAttribute("cellpadding", HtmlTextWriterAttribute.Cellpadding, false);
            RegisterAttribute("cellspacing", HtmlTextWriterAttribute.Cellspacing, false);
            RegisterAttribute("checked", HtmlTextWriterAttribute.Checked, false);
            RegisterAttribute("class", HtmlTextWriterAttribute.Class, true);
            RegisterAttribute("cols", HtmlTextWriterAttribute.Cols, false);
            RegisterAttribute("colspan", HtmlTextWriterAttribute.Colspan, false);
            RegisterAttribute("content", HtmlTextWriterAttribute.Content, true);
            RegisterAttribute("coords", HtmlTextWriterAttribute.Coords, false);
            RegisterAttribute("dir", HtmlTextWriterAttribute.Dir, false);
            RegisterAttribute("disabled", HtmlTextWriterAttribute.Disabled, false);
            RegisterAttribute("for", HtmlTextWriterAttribute.For, false);
            RegisterAttribute("headers", HtmlTextWriterAttribute.Headers, true);
            RegisterAttribute("height", HtmlTextWriterAttribute.Height, false);
            RegisterAttribute("href", HtmlTextWriterAttribute.Href, true, true);
            RegisterAttribute("id", HtmlTextWriterAttribute.Id, false);
            RegisterAttribute("longdesc", HtmlTextWriterAttribute.Longdesc, true, true);
            RegisterAttribute("maxlength", HtmlTextWriterAttribute.Maxlength, false);
            RegisterAttribute("multiple", HtmlTextWriterAttribute.Multiple, false);
            RegisterAttribute("name", HtmlTextWriterAttribute.Name, false);
            RegisterAttribute("nowrap", HtmlTextWriterAttribute.Nowrap, false);
            RegisterAttribute("onclick", HtmlTextWriterAttribute.Onclick, true);
            RegisterAttribute("onchange", HtmlTextWriterAttribute.Onchange, true);
            RegisterAttribute("readonly", HtmlTextWriterAttribute.ReadOnly, false);
            RegisterAttribute("rel", HtmlTextWriterAttribute.Rel, false);
            RegisterAttribute("rows", HtmlTextWriterAttribute.Rows, false);
            RegisterAttribute("rowspan", HtmlTextWriterAttribute.Rowspan, false);
            RegisterAttribute("rules", HtmlTextWriterAttribute.Rules, false);
            RegisterAttribute("scope", HtmlTextWriterAttribute.Scope, false);
            RegisterAttribute("selected", HtmlTextWriterAttribute.Selected, false);
            RegisterAttribute("shape", HtmlTextWriterAttribute.Shape, false);
            RegisterAttribute("size", HtmlTextWriterAttribute.Size, false);
            RegisterAttribute("src", HtmlTextWriterAttribute.Src, true, true);
            RegisterAttribute("style", HtmlTextWriterAttribute.Style, false);
            RegisterAttribute("tabindex", HtmlTextWriterAttribute.Tabindex, false);
            RegisterAttribute("target", HtmlTextWriterAttribute.Target, false);
            RegisterAttribute("title", HtmlTextWriterAttribute.Title, true);
            RegisterAttribute("type", HtmlTextWriterAttribute.Type, false);
            RegisterAttribute("usemap", HtmlTextWriterAttribute.Usemap, false);
            RegisterAttribute("valign", HtmlTextWriterAttribute.Valign, false);
            RegisterAttribute("value", HtmlTextWriterAttribute.Value, true);
            RegisterAttribute("vcard_name", HtmlTextWriterAttribute.VCardName, false);
            RegisterAttribute("width", HtmlTextWriterAttribute.Width, false);
            RegisterAttribute("wrap", HtmlTextWriterAttribute.Wrap, false);
            RegisterAttribute("_designerRegion", HtmlTextWriterAttribute.DesignerRegion, false);
        }

        public HtmlTextWriter(TextWriter writer) : this(writer, "\t")
        {
        }

        public HtmlTextWriter(TextWriter writer, string tabString) : base(CultureInfo.InvariantCulture)
        {
            this._currentLayout = new Layout(HorizontalAlign.NotSet, true);
            this.writer = writer;
            this.tabString = tabString;
            this.indentLevel = 0;
            this.tabsPending = false;
            this._httpWriter = writer as HttpWriter;
            this._isDescendant = base.GetType() != typeof(HtmlTextWriter);
            this._attrCount = 0;
            this._styleCount = 0;
            this._endTagCount = 0;
            this._inlineCount = 0;
        }

        public virtual void AddAttribute(string name, string value)
        {
            HtmlTextWriterAttribute attributeKey = this.GetAttributeKey(name);
            value = this.EncodeAttributeValue(attributeKey, value);
            this.AddAttribute(name, value, attributeKey);
        }

        public virtual void AddAttribute(HtmlTextWriterAttribute key, string value)
        {
            int index = (int) key;
            if ((index >= 0) && (index < _attrNameLookupArray.Length))
            {
                AttributeInformation information = _attrNameLookupArray[index];
                this.AddAttribute(information.name, value, key, information.encode, information.isUrl);
            }
        }

        public virtual void AddAttribute(string name, string value, bool fEndode)
        {
            value = this.EncodeAttributeValue(value, fEndode);
            this.AddAttribute(name, value, this.GetAttributeKey(name));
        }

        protected virtual void AddAttribute(string name, string value, HtmlTextWriterAttribute key)
        {
            this.AddAttribute(name, value, key, false, false);
        }

        public virtual void AddAttribute(HtmlTextWriterAttribute key, string value, bool fEncode)
        {
            int index = (int) key;
            if ((index >= 0) && (index < _attrNameLookupArray.Length))
            {
                AttributeInformation information = _attrNameLookupArray[index];
                this.AddAttribute(information.name, value, key, fEncode, information.isUrl);
            }
        }

        private void AddAttribute(string name, string value, HtmlTextWriterAttribute key, bool encode, bool isUrl)
        {
            RenderAttribute attribute;
            if (this._attrList == null)
            {
                this._attrList = new RenderAttribute[20];
            }
            else if (this._attrCount >= this._attrList.Length)
            {
                RenderAttribute[] destinationArray = new RenderAttribute[this._attrList.Length * 2];
                Array.Copy(this._attrList, destinationArray, this._attrList.Length);
                this._attrList = destinationArray;
            }
            attribute.name = name;
            attribute.value = value;
            attribute.key = key;
            attribute.encode = encode;
            attribute.isUrl = isUrl;
            this._attrList[this._attrCount] = attribute;
            this._attrCount++;
        }

        public virtual void AddStyleAttribute(string name, string value)
        {
            this.AddStyleAttribute(name, value, CssTextWriter.GetStyleKey(name));
        }

        public virtual void AddStyleAttribute(HtmlTextWriterStyle key, string value)
        {
            this.AddStyleAttribute(CssTextWriter.GetStyleName(key), value, key);
        }

        protected virtual void AddStyleAttribute(string name, string value, HtmlTextWriterStyle key)
        {
            RenderStyle style;
            if (this._styleList == null)
            {
                this._styleList = new RenderStyle[20];
            }
            else if (this._styleCount > this._styleList.Length)
            {
                RenderStyle[] destinationArray = new RenderStyle[this._styleList.Length * 2];
                Array.Copy(this._styleList, destinationArray, this._styleList.Length);
                this._styleList = destinationArray;
            }
            style.name = name;
            style.key = key;
            string str = value;
            if (CssTextWriter.IsStyleEncoded(key))
            {
                str = HttpUtility.HtmlAttributeEncode(value);
            }
            style.value = str;
            this._styleList[this._styleCount] = style;
            this._styleCount++;
        }

        public virtual void BeginRender()
        {
        }

        public override void Close()
        {
            this.writer.Close();
        }

        protected string EncodeAttributeValue(string value, bool fEncode)
        {
            if (value == null)
            {
                return null;
            }
            if (!fEncode)
            {
                return value;
            }
            return HttpUtility.HtmlAttributeEncode(value);
        }

        protected virtual string EncodeAttributeValue(HtmlTextWriterAttribute attrKey, string value)
        {
            bool fEncode = true;
            if ((HtmlTextWriterAttribute.Accesskey <= attrKey) && (attrKey < _attrNameLookupArray.Length))
            {
                fEncode = _attrNameLookupArray[(int) attrKey].encode;
            }
            return this.EncodeAttributeValue(value, fEncode);
        }

        protected string EncodeUrl(string url)
        {
            if (!UrlPath.IsUncSharePath(url))
            {
                return HttpUtility.UrlPathEncode(url);
            }
            return url;
        }

        public virtual void EndRender()
        {
        }

        public virtual void EnterStyle(Style style)
        {
            this.EnterStyle(style, HtmlTextWriterTag.Span);
        }

        public virtual void EnterStyle(Style style, HtmlTextWriterTag tag)
        {
            if (!style.IsEmpty || (tag != HtmlTextWriterTag.Span))
            {
                style.AddAttributesToRender(this);
                this.RenderBeginTag(tag);
            }
        }

        public virtual void ExitStyle(Style style)
        {
            this.ExitStyle(style, HtmlTextWriterTag.Span);
        }

        public virtual void ExitStyle(Style style, HtmlTextWriterTag tag)
        {
            if (!style.IsEmpty || (tag != HtmlTextWriterTag.Span))
            {
                this.RenderEndTag();
            }
        }

        protected virtual void FilterAttributes()
        {
            int index = 0;
            for (int i = 0; i < this._styleCount; i++)
            {
                RenderStyle style = this._styleList[i];
                if (this.OnStyleAttributeRender(style.name, style.value, style.key))
                {
                    this._styleList[index] = style;
                    index++;
                }
            }
            this._styleCount = index;
            int num3 = 0;
            for (int j = 0; j < this._attrCount; j++)
            {
                RenderAttribute attribute = this._attrList[j];
                if (this.OnAttributeRender(attribute.name, attribute.value, attribute.key))
                {
                    this._attrList[num3] = attribute;
                    num3++;
                }
            }
            this._attrCount = num3;
        }

        public override void Flush()
        {
            this.writer.Flush();
        }

        protected HtmlTextWriterAttribute GetAttributeKey(string attrName)
        {
            if (!string.IsNullOrEmpty(attrName))
            {
                object obj2 = _attrKeyLookupTable[attrName.ToLower(CultureInfo.InvariantCulture)];
                if (obj2 != null)
                {
                    return (HtmlTextWriterAttribute) obj2;
                }
            }
            return ~HtmlTextWriterAttribute.Accesskey;
        }

        protected string GetAttributeName(HtmlTextWriterAttribute attrKey)
        {
            if ((attrKey >= HtmlTextWriterAttribute.Accesskey) && (attrKey < _attrNameLookupArray.Length))
            {
                return _attrNameLookupArray[(int) attrKey].name;
            }
            return string.Empty;
        }

        protected HtmlTextWriterStyle GetStyleKey(string styleName)
        {
            return CssTextWriter.GetStyleKey(styleName);
        }

        protected string GetStyleName(HtmlTextWriterStyle styleKey)
        {
            return CssTextWriter.GetStyleName(styleKey);
        }

        protected virtual HtmlTextWriterTag GetTagKey(string tagName)
        {
            if (!string.IsNullOrEmpty(tagName))
            {
                object obj2 = _tagKeyLookupTable[tagName.ToLower(CultureInfo.InvariantCulture)];
                if (obj2 != null)
                {
                    return (HtmlTextWriterTag) obj2;
                }
            }
            return HtmlTextWriterTag.Unknown;
        }

        protected virtual string GetTagName(HtmlTextWriterTag tagKey)
        {
            int index = (int) tagKey;
            if ((index >= 0) && (index < _tagNameLookupArray.Length))
            {
                return _tagNameLookupArray[index].name;
            }
            return string.Empty;
        }

        protected bool IsAttributeDefined(HtmlTextWriterAttribute key)
        {
            for (int i = 0; i < this._attrCount; i++)
            {
                if (this._attrList[i].key == key)
                {
                    return true;
                }
            }
            return false;
        }

        protected bool IsAttributeDefined(HtmlTextWriterAttribute key, out string value)
        {
            value = null;
            for (int i = 0; i < this._attrCount; i++)
            {
                if (this._attrList[i].key == key)
                {
                    value = this._attrList[i].value;
                    return true;
                }
            }
            return false;
        }

        protected bool IsStyleAttributeDefined(HtmlTextWriterStyle key)
        {
            for (int i = 0; i < this._styleCount; i++)
            {
                if (this._styleList[i].key == key)
                {
                    return true;
                }
            }
            return false;
        }

        protected bool IsStyleAttributeDefined(HtmlTextWriterStyle key, out string value)
        {
            value = null;
            for (int i = 0; i < this._styleCount; i++)
            {
                if (this._styleList[i].key == key)
                {
                    value = this._styleList[i].value;
                    return true;
                }
            }
            return false;
        }

        public virtual bool IsValidFormAttribute(string attribute)
        {
            return true;
        }

        protected virtual bool OnAttributeRender(string name, string value, HtmlTextWriterAttribute key)
        {
            return true;
        }

        protected virtual bool OnStyleAttributeRender(string name, string value, HtmlTextWriterStyle key)
        {
            return true;
        }

        protected virtual bool OnTagRender(string name, HtmlTextWriterTag key)
        {
            return true;
        }

        internal virtual void OpenDiv()
        {
            this.OpenDiv(this._currentLayout, (this._currentLayout != null) && (this._currentLayout.Align != HorizontalAlign.NotSet), (this._currentLayout != null) && !this._currentLayout.Wrap);
        }

        private void OpenDiv(Layout layout, bool writeHorizontalAlign, bool writeWrapping)
        {
            this.WriteBeginTag("div");
            if (writeHorizontalAlign)
            {
                string str;
                switch (layout.Align)
                {
                    case HorizontalAlign.Center:
                        str = "text-align:center";
                        break;

                    case HorizontalAlign.Right:
                        str = "text-align:right";
                        break;

                    default:
                        str = "text-align:left";
                        break;
                }
                this.WriteAttribute("style", str);
            }
            if (writeWrapping)
            {
                this.WriteAttribute("mode", layout.Wrap ? "wrap" : "nowrap");
            }
            this.Write('>');
            this._currentWrittenLayout = layout;
        }

        protected virtual void OutputTabs()
        {
            if (this.tabsPending)
            {
                for (int i = 0; i < this.indentLevel; i++)
                {
                    this.writer.Write(this.tabString);
                }
                this.tabsPending = false;
            }
        }

        protected string PopEndTag()
        {
            if (this._endTagCount <= 0)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("HTMLTextWriterUnbalancedPop"));
            }
            this._endTagCount--;
            this.TagKey = this._endTags[this._endTagCount].tagKey;
            return this._endTags[this._endTagCount].endTagText;
        }

        protected void PushEndTag(string endTag)
        {
            if (this._endTags == null)
            {
                this._endTags = new TagStackEntry[0x10];
            }
            else if (this._endTagCount >= this._endTags.Length)
            {
                TagStackEntry[] destinationArray = new TagStackEntry[this._endTags.Length * 2];
                Array.Copy(this._endTags, destinationArray, this._endTags.Length);
                this._endTags = destinationArray;
            }
            this._endTags[this._endTagCount].tagKey = this._tagKey;
            this._endTags[this._endTagCount].endTagText = endTag;
            this._endTagCount++;
        }

        protected static void RegisterAttribute(string name, HtmlTextWriterAttribute key)
        {
            RegisterAttribute(name, key, false);
        }

        private static void RegisterAttribute(string name, HtmlTextWriterAttribute key, bool encode)
        {
            RegisterAttribute(name, key, encode, false);
        }

        private static void RegisterAttribute(string name, HtmlTextWriterAttribute key, bool encode, bool isUrl)
        {
            string str = name.ToLower(CultureInfo.InvariantCulture);
            _attrKeyLookupTable.Add(str, key);
            if (key < _attrNameLookupArray.Length)
            {
                _attrNameLookupArray[(int) key] = new AttributeInformation(name, encode, isUrl);
            }
        }

        protected static void RegisterStyle(string name, HtmlTextWriterStyle key)
        {
            CssTextWriter.RegisterAttribute(name, key);
        }

        protected static void RegisterTag(string name, HtmlTextWriterTag key)
        {
            RegisterTag(name, key, TagType.Other);
        }

        private static void RegisterTag(string name, HtmlTextWriterTag key, TagType type)
        {
            string str = name.ToLower(CultureInfo.InvariantCulture);
            _tagKeyLookupTable.Add(str, key);
            string closingTag = null;
            if ((type != TagType.NonClosing) && (key != HtmlTextWriterTag.Unknown))
            {
                closingTag = "</" + str + '>'.ToString(CultureInfo.InvariantCulture);
            }
            if (key < _tagNameLookupArray.Length)
            {
                _tagNameLookupArray[(int) key] = new TagInformation(name, type, closingTag);
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        protected virtual string RenderAfterContent()
        {
            return null;
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        protected virtual string RenderAfterTag()
        {
            return null;
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        protected virtual string RenderBeforeContent()
        {
            return null;
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        protected virtual string RenderBeforeTag()
        {
            return null;
        }

        public virtual void RenderBeginTag(string tagName)
        {
            this.TagName = tagName;
            this.RenderBeginTag(this._tagKey);
        }

        public virtual void RenderBeginTag(HtmlTextWriterTag tagKey)
        {
            this.TagKey = tagKey;
            bool flag = true;
            if (this._isDescendant)
            {
                flag = this.OnTagRender(this._tagName, this._tagKey);
                this.FilterAttributes();
                string str = this.RenderBeforeTag();
                if (str != null)
                {
                    if (this.tabsPending)
                    {
                        this.OutputTabs();
                    }
                    this.writer.Write(str);
                }
            }
            TagInformation information = _tagNameLookupArray[this._tagIndex];
            TagType tagType = information.tagType;
            bool flag2 = flag && (tagType != TagType.NonClosing);
            string endTag = flag2 ? information.closingTag : null;
            if (flag)
            {
                if (this.tabsPending)
                {
                    this.OutputTabs();
                }
                this.writer.Write('<');
                this.writer.Write(this._tagName);
                string str3 = null;
                for (int i = 0; i < this._attrCount; i++)
                {
                    RenderAttribute attribute = this._attrList[i];
                    if (attribute.key == HtmlTextWriterAttribute.Style)
                    {
                        str3 = attribute.value;
                    }
                    else
                    {
                        this.writer.Write(' ');
                        this.writer.Write(attribute.name);
                        if (attribute.value != null)
                        {
                            this.writer.Write("=\"");
                            string url = attribute.value;
                            if (attribute.isUrl && ((attribute.key != HtmlTextWriterAttribute.Href) || !url.StartsWith("javascript:", StringComparison.Ordinal)))
                            {
                                url = this.EncodeUrl(url);
                            }
                            if (attribute.encode)
                            {
                                this.WriteHtmlAttributeEncode(url);
                            }
                            else
                            {
                                this.writer.Write(url);
                            }
                            this.writer.Write('"');
                        }
                    }
                }
                if ((this._styleCount > 0) || (str3 != null))
                {
                    this.writer.Write(' ');
                    this.writer.Write("style");
                    this.writer.Write("=\"");
                    CssTextWriter.WriteAttributes(this.writer, this._styleList, this._styleCount);
                    if (str3 != null)
                    {
                        this.writer.Write(str3);
                    }
                    this.writer.Write('"');
                }
                if (tagType == TagType.NonClosing)
                {
                    this.writer.Write(" />");
                }
                else
                {
                    this.writer.Write('>');
                }
            }
            string str5 = this.RenderBeforeContent();
            if (str5 != null)
            {
                if (this.tabsPending)
                {
                    this.OutputTabs();
                }
                this.writer.Write(str5);
            }
            if (flag2)
            {
                if (tagType == TagType.Inline)
                {
                    this._inlineCount++;
                }
                else
                {
                    this.WriteLine();
                    this.Indent++;
                }
                if (endTag == null)
                {
                    endTag = "</" + this._tagName + '>'.ToString(CultureInfo.InvariantCulture);
                }
            }
            if (this._isDescendant)
            {
                string str6 = this.RenderAfterTag();
                if (str6 != null)
                {
                    endTag = (endTag == null) ? str6 : (str6 + endTag);
                }
                string str7 = this.RenderAfterContent();
                if (str7 != null)
                {
                    endTag = (endTag == null) ? str7 : (str7 + endTag);
                }
            }
            this.PushEndTag(endTag);
            this._attrCount = 0;
            this._styleCount = 0;
        }

        public virtual void RenderEndTag()
        {
            string str = this.PopEndTag();
            if (str != null)
            {
                if (_tagNameLookupArray[this._tagIndex].tagType == TagType.Inline)
                {
                    this._inlineCount--;
                    this.Write(str);
                }
                else
                {
                    this.WriteLine();
                    this.Indent--;
                    this.Write(str);
                }
            }
        }

        public override void Write(bool value)
        {
            if (this.tabsPending)
            {
                this.OutputTabs();
            }
            this.writer.Write(value);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public override void Write(char value)
        {
            if (this.tabsPending)
            {
                this.OutputTabs();
            }
            this.writer.Write(value);
        }

        public override void Write(char[] buffer)
        {
            if (this.tabsPending)
            {
                this.OutputTabs();
            }
            this.writer.Write(buffer);
        }

        public override void Write(double value)
        {
            if (this.tabsPending)
            {
                this.OutputTabs();
            }
            this.writer.Write(value);
        }

        public override void Write(int value)
        {
            if (this.tabsPending)
            {
                this.OutputTabs();
            }
            this.writer.Write(value);
        }

        public override void Write(long value)
        {
            if (this.tabsPending)
            {
                this.OutputTabs();
            }
            this.writer.Write(value);
        }

        public override void Write(object value)
        {
            if (this.tabsPending)
            {
                this.OutputTabs();
            }
            this.writer.Write(value);
        }

        public override void Write(float value)
        {
            if (this.tabsPending)
            {
                this.OutputTabs();
            }
            this.writer.Write(value);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public override void Write(string s)
        {
            if (this.tabsPending)
            {
                this.OutputTabs();
            }
            this.writer.Write(s);
        }

        public override void Write(string format, object arg0)
        {
            if (this.tabsPending)
            {
                this.OutputTabs();
            }
            this.writer.Write(format, arg0);
        }

        public override void Write(string format, params object[] arg)
        {
            if (this.tabsPending)
            {
                this.OutputTabs();
            }
            this.writer.Write(format, arg);
        }

        public override void Write(char[] buffer, int index, int count)
        {
            if (this.tabsPending)
            {
                this.OutputTabs();
            }
            this.writer.Write(buffer, index, count);
        }

        public override void Write(string format, object arg0, object arg1)
        {
            if (this.tabsPending)
            {
                this.OutputTabs();
            }
            this.writer.Write(format, arg0, arg1);
        }

        public virtual void WriteAttribute(string name, string value)
        {
            this.WriteAttribute(name, value, false);
        }

        public virtual void WriteAttribute(string name, string value, bool fEncode)
        {
            this.writer.Write(' ');
            this.writer.Write(name);
            if (value != null)
            {
                this.writer.Write("=\"");
                if (fEncode)
                {
                    this.WriteHtmlAttributeEncode(value);
                }
                else
                {
                    this.writer.Write(value);
                }
                this.writer.Write('"');
            }
        }

        public virtual void WriteBeginTag(string tagName)
        {
            if (this.tabsPending)
            {
                this.OutputTabs();
            }
            this.writer.Write('<');
            this.writer.Write(tagName);
        }

        public virtual void WriteBreak()
        {
            this.Write("<br />");
        }

        public virtual void WriteEncodedText(string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException("text");
            }
            int length = text.Length;
            int startIndex = 0;
            while (startIndex < length)
            {
                int index = text.IndexOf('\x00a0', startIndex);
                if (index < 0)
                {
                    HttpUtility.HtmlEncode((startIndex == 0) ? text : text.Substring(startIndex, length - startIndex), this);
                    startIndex = length;
                }
                else
                {
                    if (index > startIndex)
                    {
                        HttpUtility.HtmlEncode(text.Substring(startIndex, index - startIndex), this);
                    }
                    this.Write("&nbsp;");
                    startIndex = index + 1;
                }
            }
        }

        public virtual void WriteEncodedUrl(string url)
        {
            int index = url.IndexOf('?');
            if (index != -1)
            {
                this.WriteUrlEncodedString(url.Substring(0, index), false);
                this.Write(url.Substring(index));
            }
            else
            {
                this.WriteUrlEncodedString(url, false);
            }
        }

        public virtual void WriteEncodedUrlParameter(string urlText)
        {
            this.WriteUrlEncodedString(urlText, true);
        }

        public virtual void WriteEndTag(string tagName)
        {
            if (this.tabsPending)
            {
                this.OutputTabs();
            }
            this.writer.Write('<');
            this.writer.Write('/');
            this.writer.Write(tagName);
            this.writer.Write('>');
        }

        public virtual void WriteFullBeginTag(string tagName)
        {
            if (this.tabsPending)
            {
                this.OutputTabs();
            }
            this.writer.Write('<');
            this.writer.Write(tagName);
            this.writer.Write('>');
        }

        internal void WriteHtmlAttributeEncode(string s)
        {
            HttpUtility.HtmlAttributeEncode(s, this._httpWriter ?? this.writer);
        }

        public override void WriteLine()
        {
            this.writer.WriteLine();
            this.tabsPending = true;
        }

        public override void WriteLine(bool value)
        {
            if (this.tabsPending)
            {
                this.OutputTabs();
            }
            this.writer.WriteLine(value);
            this.tabsPending = true;
        }

        public override void WriteLine(char value)
        {
            if (this.tabsPending)
            {
                this.OutputTabs();
            }
            this.writer.WriteLine(value);
            this.tabsPending = true;
        }

        public override void WriteLine(double value)
        {
            if (this.tabsPending)
            {
                this.OutputTabs();
            }
            this.writer.WriteLine(value);
            this.tabsPending = true;
        }

        public override void WriteLine(int value)
        {
            if (this.tabsPending)
            {
                this.OutputTabs();
            }
            this.writer.WriteLine(value);
            this.tabsPending = true;
        }

        public override void WriteLine(long value)
        {
            if (this.tabsPending)
            {
                this.OutputTabs();
            }
            this.writer.WriteLine(value);
            this.tabsPending = true;
        }

        public override void WriteLine(object value)
        {
            if (this.tabsPending)
            {
                this.OutputTabs();
            }
            this.writer.WriteLine(value);
            this.tabsPending = true;
        }

        public override void WriteLine(float value)
        {
            if (this.tabsPending)
            {
                this.OutputTabs();
            }
            this.writer.WriteLine(value);
            this.tabsPending = true;
        }

        public override void WriteLine(string s)
        {
            if (this.tabsPending)
            {
                this.OutputTabs();
            }
            this.writer.WriteLine(s);
            this.tabsPending = true;
        }

        public override void WriteLine(char[] buffer)
        {
            if (this.tabsPending)
            {
                this.OutputTabs();
            }
            this.writer.WriteLine(buffer);
            this.tabsPending = true;
        }

        [CLSCompliant(false)]
        public override void WriteLine(uint value)
        {
            if (this.tabsPending)
            {
                this.OutputTabs();
            }
            this.writer.WriteLine(value);
            this.tabsPending = true;
        }

        public override void WriteLine(string format, object arg0)
        {
            if (this.tabsPending)
            {
                this.OutputTabs();
            }
            this.writer.WriteLine(format, arg0);
            this.tabsPending = true;
        }

        public override void WriteLine(string format, params object[] arg)
        {
            if (this.tabsPending)
            {
                this.OutputTabs();
            }
            this.writer.WriteLine(format, arg);
            this.tabsPending = true;
        }

        public override void WriteLine(char[] buffer, int index, int count)
        {
            if (this.tabsPending)
            {
                this.OutputTabs();
            }
            this.writer.WriteLine(buffer, index, count);
            this.tabsPending = true;
        }

        public override void WriteLine(string format, object arg0, object arg1)
        {
            if (this.tabsPending)
            {
                this.OutputTabs();
            }
            this.writer.WriteLine(format, arg0, arg1);
            this.tabsPending = true;
        }

        public void WriteLineNoTabs(string s)
        {
            this.writer.WriteLine(s);
            this.tabsPending = true;
        }

        internal void WriteObsoleteBreak()
        {
            this.Write("<br>");
        }

        public virtual void WriteStyleAttribute(string name, string value)
        {
            this.WriteStyleAttribute(name, value, false);
        }

        public virtual void WriteStyleAttribute(string name, string value, bool fEncode)
        {
            this.writer.Write(name);
            this.writer.Write(':');
            if (fEncode)
            {
                this.WriteHtmlAttributeEncode(value);
            }
            else
            {
                this.writer.Write(value);
            }
            this.writer.Write(';');
        }

        protected void WriteUrlEncodedString(string text, bool argument)
        {
            int length = text.Length;
            for (int i = 0; i < length; i++)
            {
                char ch = text[i];
                if (HttpEncoderUtility.IsUrlSafeChar(ch))
                {
                    this.Write(ch);
                }
                else if (!argument && (((ch == '/') || (ch == ':')) || ((ch == '#') || (ch == ','))))
                {
                    this.Write(ch);
                }
                else if ((ch == ' ') && argument)
                {
                    this.Write('+');
                }
                else if ((ch & 0xff80) == 0)
                {
                    this.Write('%');
                    this.Write(HttpEncoderUtility.IntToHex((ch >> 4) & '\x000f'));
                    this.Write(HttpEncoderUtility.IntToHex(ch & '\x000f'));
                }
                else
                {
                    this.Write(HttpUtility.UrlEncodeNonAscii(char.ToString(ch), System.Text.Encoding.UTF8));
                }
            }
        }

        internal void WriteUTF8ResourceString(IntPtr pv, int offset, int size, bool fAsciiOnly)
        {
            if (this._httpWriter != null)
            {
                if (this.tabsPending)
                {
                    this.OutputTabs();
                }
                this._httpWriter.WriteUTF8ResourceString(pv, offset, size, fAsciiOnly);
            }
            else
            {
                this.Write(StringResourceManager.ResourceToString(pv, offset, size));
            }
        }

        public override System.Text.Encoding Encoding
        {
            get
            {
                return this.writer.Encoding;
            }
        }

        public int Indent
        {
            get
            {
                return this.indentLevel;
            }
            set
            {
                if (value < 0)
                {
                    value = 0;
                }
                this.indentLevel = value;
            }
        }

        public TextWriter InnerWriter
        {
            get
            {
                return this.writer;
            }
            set
            {
                this.writer = value;
                this._httpWriter = value as HttpWriter;
            }
        }

        public override string NewLine
        {
            get
            {
                return this.writer.NewLine;
            }
            set
            {
                this.writer.NewLine = value;
            }
        }

        internal virtual bool RenderDivAroundHiddenInputs
        {
            get
            {
                return true;
            }
        }

        protected HtmlTextWriterTag TagKey
        {
            get
            {
                return this._tagKey;
            }
            set
            {
                this._tagIndex = (int) value;
                if ((this._tagIndex < 0) || (this._tagIndex >= _tagNameLookupArray.Length))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this._tagKey = value;
                if (value != HtmlTextWriterTag.Unknown)
                {
                    this._tagName = _tagNameLookupArray[this._tagIndex].name;
                }
            }
        }

        protected string TagName
        {
            get
            {
                return this._tagName;
            }
            set
            {
                this._tagName = value;
                this._tagKey = this.GetTagKey(this._tagName);
                this._tagIndex = (int) this._tagKey;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct AttributeInformation
        {
            public string name;
            public bool isUrl;
            public bool encode;
            public AttributeInformation(string name, bool encode, bool isUrl)
            {
                this.name = name;
                this.encode = encode;
                this.isUrl = isUrl;
            }
        }

        internal class Layout
        {
            private HorizontalAlign _align;
            private bool _wrap;

            public Layout(HorizontalAlign alignment, bool wrapping)
            {
                this.Align = alignment;
                this.Wrap = wrapping;
            }

            public HorizontalAlign Align
            {
                get
                {
                    return this._align;
                }
                set
                {
                    this._align = value;
                }
            }

            public bool Wrap
            {
                get
                {
                    return this._wrap;
                }
                set
                {
                    this._wrap = value;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RenderAttribute
        {
            public string name;
            public string value;
            public HtmlTextWriterAttribute key;
            public bool encode;
            public bool isUrl;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct TagInformation
        {
            public string name;
            public HtmlTextWriter.TagType tagType;
            public string closingTag;
            public TagInformation(string name, HtmlTextWriter.TagType tagType, string closingTag)
            {
                this.name = name;
                this.tagType = tagType;
                this.closingTag = closingTag;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct TagStackEntry
        {
            public HtmlTextWriterTag tagKey;
            public string endTagText;
        }

        private enum TagType
        {
            Inline,
            NonClosing,
            Other
        }
    }
}

