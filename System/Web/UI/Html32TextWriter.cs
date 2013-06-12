namespace System.Web.UI
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Web.UI.WebControls;
    using System.Web.Util;

    public class Html32TextWriter : HtmlTextWriter
    {
        private StringBuilder _afterContent;
        private StringBuilder _afterTag;
        private StringBuilder _beforeContent;
        private StringBuilder _beforeTag;
        private string _fontColor;
        private string _fontFace;
        private string _fontSize;
        private Stack _fontStack;
        private bool _renderFontTag;
        private bool _shouldPerformDivTableSubstitution;
        private bool _supportsBold;
        private bool _supportsItalic;
        private int _tagSupports;
        private const int FONT_AROUND_CONTENT = 1;
        private const int FONT_AROUND_TAG = 2;
        private const int FONT_CONSUME = 0x20;
        private const int FONT_PROPAGATE = 0x10;
        private const int NOTHING = 0;
        private const int SUPPORTS_BORDER = 0x80;
        private const int SUPPORTS_HEIGHT_WIDTH = 0x40;
        private const int SUPPORTS_NOWRAP = 0x100;
        private const int TABLE_AROUND_CONTENT = 8;
        private const int TABLE_ATTRIBUTES = 4;

        public Html32TextWriter(TextWriter writer) : this(writer, "\t")
        {
        }

        public Html32TextWriter(TextWriter writer, string tabString) : base(writer, tabString)
        {
            this._supportsBold = true;
            this._supportsItalic = true;
            this._beforeTag = new StringBuilder(0x100);
            this._beforeContent = new StringBuilder(0x100);
            this._afterContent = new StringBuilder(0x80);
            this._afterTag = new StringBuilder(0x80);
        }

        private void AppendFontTag(StringBuilder sbBegin, StringBuilder sbEnd)
        {
            this.AppendFontTag(this._fontFace, this._fontColor, this._fontSize, sbBegin, sbEnd);
        }

        private void AppendFontTag(string fontFace, string fontColor, string fontSize, StringBuilder sbBegin, StringBuilder sbEnd)
        {
            sbBegin.Append('<');
            sbBegin.Append("font");
            if (fontFace != null)
            {
                sbBegin.Append(" face");
                sbBegin.Append("=\"");
                sbBegin.Append(fontFace);
                sbBegin.Append('"');
            }
            if (fontColor != null)
            {
                sbBegin.Append(" color=");
                sbBegin.Append('"');
                sbBegin.Append(fontColor);
                sbBegin.Append('"');
            }
            if (fontSize != null)
            {
                sbBegin.Append(" size=");
                sbBegin.Append('"');
                sbBegin.Append(fontSize);
                sbBegin.Append('"');
            }
            sbBegin.Append('>');
            sbEnd.Insert(0, "</font" + '>');
        }

        private void AppendOtherTag(string tag)
        {
            if (this.Supports(1))
            {
                this.AppendOtherTag(tag, this._beforeContent, this._afterContent);
            }
            else
            {
                this.AppendOtherTag(tag, this._beforeTag, this._afterTag);
            }
        }

        private void AppendOtherTag(string tag, StringBuilder sbBegin, StringBuilder sbEnd)
        {
            sbBegin.Append('<');
            sbBegin.Append(tag);
            sbBegin.Append('>');
            sbEnd.Insert(0, "</" + tag + '>');
        }

        private void AppendOtherTag(string tag, object[] attribs, StringBuilder sbBegin, StringBuilder sbEnd)
        {
            sbBegin.Append('<');
            sbBegin.Append(tag);
            for (int i = 0; i < attribs.Length; i++)
            {
                sbBegin.Append(' ');
                sbBegin.Append(((string[]) attribs[i])[0]);
                sbBegin.Append("=\"");
                sbBegin.Append(((string[]) attribs[i])[1]);
                sbBegin.Append('"');
            }
            sbBegin.Append('>');
            sbEnd.Insert(0, "</" + tag + '>');
        }

        private void ConsumeFont(StringBuilder sbBegin, StringBuilder sbEnd)
        {
            if (this.FontStack.Count > 0)
            {
                string fontFace = null;
                string fontColor = null;
                string fontSize = null;
                bool underline = false;
                bool italic = false;
                bool bold = false;
                bool strikeout = false;
                IEnumerator enumerator = this.FontStack.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    FontStackItem current = (FontStackItem) enumerator.Current;
                    if (fontFace == null)
                    {
                        fontFace = current.name;
                    }
                    if (fontColor == null)
                    {
                        fontColor = current.color;
                    }
                    if (fontSize == null)
                    {
                        fontSize = current.size;
                    }
                    if (!underline)
                    {
                        underline = current.underline;
                    }
                    if (!italic)
                    {
                        italic = current.italic;
                    }
                    if (!bold)
                    {
                        bold = current.bold;
                    }
                    if (!strikeout)
                    {
                        strikeout = current.strikeout;
                    }
                }
                if (((fontFace != null) || (fontColor != null)) || (fontSize != null))
                {
                    this.AppendFontTag(fontFace, fontColor, fontSize, sbBegin, sbEnd);
                }
                if (underline)
                {
                    this.AppendOtherTag("u", sbBegin, sbEnd);
                }
                if (italic && this.SupportsItalic)
                {
                    this.AppendOtherTag("i", sbBegin, sbEnd);
                }
                if (bold && this.SupportsBold)
                {
                    this.AppendOtherTag("b", sbBegin, sbEnd);
                }
                if (strikeout)
                {
                    this.AppendOtherTag("strike", sbBegin, sbEnd);
                }
            }
        }

        private string ConvertToHtmlFontSize(string value)
        {
            FontUnit unit = new FontUnit(value, CultureInfo.InvariantCulture);
            if (unit.Type > FontSize.Larger)
            {
                int num = ((int) unit.Type) - 3;
                return num.ToString(CultureInfo.InvariantCulture);
            }
            if ((unit.Type != FontSize.AsUnit) || (unit.Unit.Type != UnitType.Point))
            {
                return null;
            }
            if (unit.Unit.Value <= 8.0)
            {
                return "1";
            }
            if (unit.Unit.Value <= 10.0)
            {
                return "2";
            }
            if (unit.Unit.Value <= 12.0)
            {
                return "3";
            }
            if (unit.Unit.Value <= 14.0)
            {
                return "4";
            }
            if (unit.Unit.Value <= 18.0)
            {
                return "5";
            }
            if (unit.Unit.Value <= 24.0)
            {
                return "6";
            }
            return "7";
        }

        private string ConvertToHtmlSize(string value)
        {
            Unit unit = new Unit(value, CultureInfo.InvariantCulture);
            if (unit.Type == UnitType.Pixel)
            {
                return unit.Value.ToString(CultureInfo.InvariantCulture);
            }
            if (unit.Type == UnitType.Percentage)
            {
                return value;
            }
            return null;
        }

        protected override string GetTagName(HtmlTextWriterTag tagKey)
        {
            if ((tagKey == HtmlTextWriterTag.Div) && this.ShouldPerformDivTableSubstitution)
            {
                return "table";
            }
            return base.GetTagName(tagKey);
        }

        protected override bool OnStyleAttributeRender(string name, string value, HtmlTextWriterStyle key)
        {
            string str;
            HtmlTextWriterStyle style5;
            if (this.Supports(1))
            {
                switch (key)
                {
                    case HtmlTextWriterStyle.Color:
                        this._fontColor = value;
                        this._renderFontTag = true;
                        break;

                    case HtmlTextWriterStyle.FontFamily:
                        this._fontFace = value;
                        this._renderFontTag = true;
                        break;

                    case HtmlTextWriterStyle.FontSize:
                        this._fontSize = this.ConvertToHtmlFontSize(value);
                        if (this._fontSize != null)
                        {
                            this._renderFontTag = true;
                        }
                        break;

                    case HtmlTextWriterStyle.FontStyle:
                        if (!StringUtil.EqualsIgnoreCase(value, "normal") && this.SupportsItalic)
                        {
                            this.AppendOtherTag("i");
                        }
                        break;

                    case HtmlTextWriterStyle.FontWeight:
                        if (StringUtil.EqualsIgnoreCase(value, "bold") && this.SupportsBold)
                        {
                            this.AppendOtherTag("b");
                        }
                        break;

                    case HtmlTextWriterStyle.TextDecoration:
                        str = value.ToLower(CultureInfo.InvariantCulture);
                        if (str.IndexOf("underline", StringComparison.Ordinal) != -1)
                        {
                            this.AppendOtherTag("u");
                        }
                        if (str.IndexOf("line-through", StringComparison.Ordinal) != -1)
                        {
                            this.AppendOtherTag("strike");
                        }
                        break;
                }
            }
            else if (this.Supports(0x10))
            {
                FontStackItem item = (FontStackItem) this.FontStack.Peek();
                switch (key)
                {
                    case HtmlTextWriterStyle.Color:
                        item.color = value;
                        break;

                    case HtmlTextWriterStyle.FontFamily:
                        item.name = value;
                        break;

                    case HtmlTextWriterStyle.FontSize:
                        item.size = this.ConvertToHtmlFontSize(value);
                        break;

                    case HtmlTextWriterStyle.FontStyle:
                        if (!StringUtil.EqualsIgnoreCase(value, "normal"))
                        {
                            item.italic = true;
                        }
                        break;

                    case HtmlTextWriterStyle.FontWeight:
                        if (StringUtil.EqualsIgnoreCase(value, "bold"))
                        {
                            item.bold = true;
                        }
                        break;

                    case HtmlTextWriterStyle.TextDecoration:
                        str = value.ToLower(CultureInfo.InvariantCulture);
                        if (str.IndexOf("underline", StringComparison.Ordinal) != -1)
                        {
                            item.underline = true;
                        }
                        if (str.IndexOf("line-through", StringComparison.Ordinal) != -1)
                        {
                            item.strikeout = true;
                        }
                        break;
                }
            }
            if (this.Supports(0x80) && (key == HtmlTextWriterStyle.BorderWidth))
            {
                str = this.ConvertToHtmlSize(value);
                if (str != null)
                {
                    this.AddAttribute(HtmlTextWriterAttribute.Border, str);
                }
            }
            if (this.Supports(0x100) && (key == HtmlTextWriterStyle.WhiteSpace))
            {
                this.AddAttribute(HtmlTextWriterAttribute.Nowrap, value);
            }
            if (this.Supports(0x40))
            {
                switch (key)
                {
                    case HtmlTextWriterStyle.Height:
                        str = this.ConvertToHtmlSize(value);
                        if (str != null)
                        {
                            this.AddAttribute(HtmlTextWriterAttribute.Height, str);
                        }
                        break;

                    case HtmlTextWriterStyle.Width:
                        str = this.ConvertToHtmlSize(value);
                        if (str != null)
                        {
                            this.AddAttribute(HtmlTextWriterAttribute.Width, str);
                        }
                        break;
                }
            }
            if (this.Supports(4) || this.Supports(8))
            {
                switch (key)
                {
                    case HtmlTextWriterStyle.BackgroundColor:
                    {
                        HtmlTextWriterTag tagKey = base.TagKey;
                        if (tagKey > HtmlTextWriterTag.Div)
                        {
                            switch (tagKey)
                            {
                                case HtmlTextWriterTag.Table:
                                case HtmlTextWriterTag.Td:
                                case HtmlTextWriterTag.Th:
                                    goto Label_031C;

                                case HtmlTextWriterTag.Tbody:
                                    goto Label_03CF;
                            }
                            if (tagKey != HtmlTextWriterTag.Tr)
                            {
                                break;
                            }
                            goto Label_031C;
                        }
                        switch (tagKey)
                        {
                            case HtmlTextWriterTag.Body:
                                goto Label_031C;

                            case HtmlTextWriterTag.Div:
                                if (this.ShouldPerformDivTableSubstitution)
                                {
                                    this.AddAttribute(HtmlTextWriterAttribute.Bgcolor, value);
                                }
                                break;
                        }
                        break;
                    }
                    case HtmlTextWriterStyle.BackgroundImage:
                    {
                        HtmlTextWriterTag tag3 = base.TagKey;
                        if (tag3 > HtmlTextWriterTag.Div)
                        {
                            switch (tag3)
                            {
                                case HtmlTextWriterTag.Table:
                                case HtmlTextWriterTag.Td:
                                    goto Label_0379;

                                case HtmlTextWriterTag.Tbody:
                                    goto Label_03CF;
                            }
                            if (tag3 != HtmlTextWriterTag.Th)
                            {
                                break;
                            }
                            goto Label_0379;
                        }
                        switch (tag3)
                        {
                            case HtmlTextWriterTag.Body:
                                goto Label_0379;

                            case HtmlTextWriterTag.Div:
                                if (this.ShouldPerformDivTableSubstitution)
                                {
                                    if (StringUtil.StringStartsWith(value, "url("))
                                    {
                                        value = value.Substring(4, value.Length - 5);
                                    }
                                    this.AddAttribute(HtmlTextWriterAttribute.Background, value);
                                }
                                break;
                        }
                        break;
                    }
                    case HtmlTextWriterStyle.BorderColor:
                        if ((base.TagKey == HtmlTextWriterTag.Div) && this.ShouldPerformDivTableSubstitution)
                        {
                            this.AddAttribute(HtmlTextWriterAttribute.Bordercolor, value);
                        }
                        break;
                }
            }
            goto Label_03CF;
        Label_031C:
            this.AddAttribute(HtmlTextWriterAttribute.Bgcolor, value);
            goto Label_03CF;
        Label_0379:
            if (StringUtil.StringStartsWith(value, "url("))
            {
                value = value.Substring(4, value.Length - 5);
            }
            this.AddAttribute(HtmlTextWriterAttribute.Background, value);
        Label_03CF:
            style5 = key;
            if (style5 != HtmlTextWriterStyle.ListStyleType)
            {
                switch (style5)
                {
                    case HtmlTextWriterStyle.TextAlign:
                        this.AddAttribute(HtmlTextWriterAttribute.Align, value);
                        break;

                    case HtmlTextWriterStyle.VerticalAlign:
                        this.AddAttribute(HtmlTextWriterAttribute.Valign, value);
                        break;

                    case HtmlTextWriterStyle.Display:
                        return true;
                }
            }
            else
            {
                switch (value)
                {
                    case "decimal":
                        this.AddAttribute(HtmlTextWriterAttribute.Type, "1");
                        goto Label_052E;

                    case "lower-alpha":
                        this.AddAttribute(HtmlTextWriterAttribute.Type, "a");
                        goto Label_052E;

                    case "upper-alpha":
                        this.AddAttribute(HtmlTextWriterAttribute.Type, "A");
                        goto Label_052E;

                    case "lower-roman":
                        this.AddAttribute(HtmlTextWriterAttribute.Type, "i");
                        goto Label_052E;

                    case "upper-roman":
                        this.AddAttribute(HtmlTextWriterAttribute.Type, "I");
                        goto Label_052E;

                    case "disc":
                    case "circle":
                    case "square":
                        this.AddAttribute(HtmlTextWriterAttribute.Type, value);
                        goto Label_052E;
                }
                this.AddAttribute(HtmlTextWriterAttribute.Type, "disc");
            }
        Label_052E:
            return false;
        }

        protected override bool OnTagRender(string name, HtmlTextWriterTag key)
        {
            this.SetTagSupports();
            if (this.Supports(0x10))
            {
                this.FontStack.Push(new FontStackItem());
            }
            if ((key == HtmlTextWriterTag.Div) && this.ShouldPerformDivTableSubstitution)
            {
                base.TagKey = HtmlTextWriterTag.Table;
            }
            return base.OnTagRender(name, key);
        }

        protected override string RenderAfterContent()
        {
            if (this._afterContent.Length > 0)
            {
                return this._afterContent.ToString();
            }
            return base.RenderAfterContent();
        }

        protected override string RenderAfterTag()
        {
            if (this._afterTag.Length > 0)
            {
                return this._afterTag.ToString();
            }
            return base.RenderAfterTag();
        }

        protected override string RenderBeforeContent()
        {
            if (this.Supports(0x20))
            {
                this.ConsumeFont(this._beforeContent, this._afterContent);
            }
            else if (this._renderFontTag && this.Supports(1))
            {
                this.AppendFontTag(this._beforeContent, this._afterContent);
            }
            if (this._beforeContent.Length > 0)
            {
                return this._beforeContent.ToString();
            }
            return base.RenderBeforeContent();
        }

        protected override string RenderBeforeTag()
        {
            if (this._renderFontTag && this.Supports(2))
            {
                this.AppendFontTag(this._beforeTag, this._afterTag);
            }
            if (this._beforeTag.Length > 0)
            {
                return this._beforeTag.ToString();
            }
            return base.RenderBeforeTag();
        }

        public override void RenderBeginTag(HtmlTextWriterTag tagKey)
        {
            this._beforeTag.Length = 0;
            this._beforeContent.Length = 0;
            this._afterContent.Length = 0;
            this._afterTag.Length = 0;
            this._renderFontTag = false;
            this._fontFace = null;
            this._fontColor = null;
            this._fontSize = null;
            if (this.ShouldPerformDivTableSubstitution && (tagKey == HtmlTextWriterTag.Div))
            {
                string str;
                this.AppendOtherTag("tr", this._beforeContent, this._afterContent);
                if (base.IsAttributeDefined(HtmlTextWriterAttribute.Align, out str))
                {
                    string[] strArray = new string[] { base.GetAttributeName(HtmlTextWriterAttribute.Align), str };
                    this.AppendOtherTag("td", new object[] { strArray }, this._beforeContent, this._afterContent);
                }
                else
                {
                    this.AppendOtherTag("td", this._beforeContent, this._afterContent);
                }
                if (!base.IsAttributeDefined(HtmlTextWriterAttribute.Cellpadding))
                {
                    this.AddAttribute(HtmlTextWriterAttribute.Cellpadding, "0");
                }
                if (!base.IsAttributeDefined(HtmlTextWriterAttribute.Cellspacing))
                {
                    this.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
                }
                if (!base.IsStyleAttributeDefined(HtmlTextWriterStyle.BorderWidth))
                {
                    this.AddAttribute(HtmlTextWriterAttribute.Border, "0");
                }
                if (!base.IsStyleAttributeDefined(HtmlTextWriterStyle.Width))
                {
                    this.AddAttribute(HtmlTextWriterAttribute.Width, "100%");
                }
            }
            base.RenderBeginTag(tagKey);
        }

        public override void RenderEndTag()
        {
            base.RenderEndTag();
            this.SetTagSupports();
            if (this.Supports(0x10))
            {
                this.FontStack.Pop();
            }
        }

        private void SetTagSupports()
        {
            this._tagSupports = 0;
            switch (base.TagKey)
            {
                case HtmlTextWriterTag.A:
                case HtmlTextWriterTag.Label:
                case HtmlTextWriterTag.P:
                case HtmlTextWriterTag.Span:
                    this._tagSupports |= 1;
                    break;

                case HtmlTextWriterTag.Div:
                    this._tagSupports |= 0x11;
                    break;

                case HtmlTextWriterTag.Li:
                    this._tagSupports |= 0x21;
                    break;

                case HtmlTextWriterTag.Input:
                    this._tagSupports |= 0x80;
                    break;

                case HtmlTextWriterTag.Ol:
                case HtmlTextWriterTag.Table:
                case HtmlTextWriterTag.Tr:
                case HtmlTextWriterTag.Ul:
                    this._tagSupports |= 0x10;
                    break;

                case HtmlTextWriterTag.Td:
                case HtmlTextWriterTag.Th:
                    this._tagSupports |= 0x30;
                    break;
            }
            switch (base.TagKey)
            {
                case HtmlTextWriterTag.Table:
                    this._tagSupports |= 0x40;
                    break;

                case HtmlTextWriterTag.Td:
                case HtmlTextWriterTag.Th:
                    this._tagSupports |= 320;
                    break;

                case HtmlTextWriterTag.Div:
                    if (this.ShouldPerformDivTableSubstitution)
                    {
                        this._tagSupports |= 0xc0;
                    }
                    this._tagSupports |= 0x100;
                    break;

                case HtmlTextWriterTag.Img:
                    this._tagSupports |= 0xc0;
                    break;
            }
            HtmlTextWriterTag tagKey = base.TagKey;
            if (tagKey <= HtmlTextWriterTag.Td)
            {
                switch (tagKey)
                {
                    case HtmlTextWriterTag.Table:
                    case HtmlTextWriterTag.Td:
                    case HtmlTextWriterTag.Body:
                        goto Label_01C6;
                }
                goto Label_01D4;
            }
            if ((tagKey != HtmlTextWriterTag.Th) && (tagKey != HtmlTextWriterTag.Tr))
            {
                goto Label_01D4;
            }
        Label_01C6:
            this._tagSupports |= 4;
        Label_01D4:
            if (base.TagKey != HtmlTextWriterTag.Div)
            {
                return;
            }
            if (this.ShouldPerformDivTableSubstitution)
            {
                this._tagSupports |= 8;
            }
        }

        private bool Supports(int flag)
        {
            return ((this._tagSupports & flag) == flag);
        }

        protected Stack FontStack
        {
            get
            {
                if (this._fontStack == null)
                {
                    this._fontStack = new Stack(3);
                }
                return this._fontStack;
            }
        }

        internal override bool RenderDivAroundHiddenInputs
        {
            get
            {
                return false;
            }
        }

        public bool ShouldPerformDivTableSubstitution
        {
            get
            {
                return this._shouldPerformDivTableSubstitution;
            }
            set
            {
                this._shouldPerformDivTableSubstitution = value;
            }
        }

        public bool SupportsBold
        {
            get
            {
                return this._supportsBold;
            }
            set
            {
                this._supportsBold = value;
            }
        }

        public bool SupportsItalic
        {
            get
            {
                return this._supportsItalic;
            }
            set
            {
                this._supportsItalic = value;
            }
        }

        private sealed class FontStackItem
        {
            public bool bold;
            public string color;
            public bool italic;
            public string name;
            public string size;
            public bool strikeout;
            public bool underline;
        }
    }
}

