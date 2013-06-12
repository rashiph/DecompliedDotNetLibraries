namespace System.Web.UI
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Web;
    using System.Web.Util;

    internal sealed class CssTextWriter : TextWriter
    {
        private TextWriter _writer;
        private static Hashtable attrKeyLookupTable = new Hashtable(0x2b);
        private static AttributeInformation[] attrNameLookupArray = new AttributeInformation[0x2b];

        static CssTextWriter()
        {
            RegisterAttribute("background-color", HtmlTextWriterStyle.BackgroundColor);
            RegisterAttribute("background-image", HtmlTextWriterStyle.BackgroundImage, true, true);
            RegisterAttribute("border-collapse", HtmlTextWriterStyle.BorderCollapse);
            RegisterAttribute("border-color", HtmlTextWriterStyle.BorderColor);
            RegisterAttribute("border-style", HtmlTextWriterStyle.BorderStyle);
            RegisterAttribute("border-width", HtmlTextWriterStyle.BorderWidth);
            RegisterAttribute("color", HtmlTextWriterStyle.Color);
            RegisterAttribute("cursor", HtmlTextWriterStyle.Cursor);
            RegisterAttribute("direction", HtmlTextWriterStyle.Direction);
            RegisterAttribute("display", HtmlTextWriterStyle.Display);
            RegisterAttribute("filter", HtmlTextWriterStyle.Filter);
            RegisterAttribute("font-family", HtmlTextWriterStyle.FontFamily, true);
            RegisterAttribute("font-size", HtmlTextWriterStyle.FontSize);
            RegisterAttribute("font-style", HtmlTextWriterStyle.FontStyle);
            RegisterAttribute("font-variant", HtmlTextWriterStyle.FontVariant);
            RegisterAttribute("font-weight", HtmlTextWriterStyle.FontWeight);
            RegisterAttribute("height", HtmlTextWriterStyle.Height);
            RegisterAttribute("left", HtmlTextWriterStyle.Left);
            RegisterAttribute("list-style-image", HtmlTextWriterStyle.ListStyleImage, true, true);
            RegisterAttribute("list-style-type", HtmlTextWriterStyle.ListStyleType);
            RegisterAttribute("margin", HtmlTextWriterStyle.Margin);
            RegisterAttribute("margin-bottom", HtmlTextWriterStyle.MarginBottom);
            RegisterAttribute("margin-left", HtmlTextWriterStyle.MarginLeft);
            RegisterAttribute("margin-right", HtmlTextWriterStyle.MarginRight);
            RegisterAttribute("margin-top", HtmlTextWriterStyle.MarginTop);
            RegisterAttribute("overflow-x", HtmlTextWriterStyle.OverflowX);
            RegisterAttribute("overflow-y", HtmlTextWriterStyle.OverflowY);
            RegisterAttribute("overflow", HtmlTextWriterStyle.Overflow);
            RegisterAttribute("padding", HtmlTextWriterStyle.Padding);
            RegisterAttribute("padding-bottom", HtmlTextWriterStyle.PaddingBottom);
            RegisterAttribute("padding-left", HtmlTextWriterStyle.PaddingLeft);
            RegisterAttribute("padding-right", HtmlTextWriterStyle.PaddingRight);
            RegisterAttribute("padding-top", HtmlTextWriterStyle.PaddingTop);
            RegisterAttribute("position", HtmlTextWriterStyle.Position);
            RegisterAttribute("text-align", HtmlTextWriterStyle.TextAlign);
            RegisterAttribute("text-decoration", HtmlTextWriterStyle.TextDecoration);
            RegisterAttribute("text-overflow", HtmlTextWriterStyle.TextOverflow);
            RegisterAttribute("top", HtmlTextWriterStyle.Top);
            RegisterAttribute("vertical-align", HtmlTextWriterStyle.VerticalAlign);
            RegisterAttribute("visibility", HtmlTextWriterStyle.Visibility);
            RegisterAttribute("width", HtmlTextWriterStyle.Width);
            RegisterAttribute("white-space", HtmlTextWriterStyle.WhiteSpace);
            RegisterAttribute("z-index", HtmlTextWriterStyle.ZIndex);
        }

        public CssTextWriter(TextWriter writer)
        {
            this._writer = writer;
        }

        public override void Close()
        {
            this._writer.Close();
        }

        public override void Flush()
        {
            this._writer.Flush();
        }

        public static HtmlTextWriterStyle GetStyleKey(string styleName)
        {
            if (!string.IsNullOrEmpty(styleName))
            {
                object obj2 = attrKeyLookupTable[styleName.ToLower(CultureInfo.InvariantCulture)];
                if (obj2 != null)
                {
                    return (HtmlTextWriterStyle) obj2;
                }
            }
            return ~HtmlTextWriterStyle.BackgroundColor;
        }

        public static string GetStyleName(HtmlTextWriterStyle styleKey)
        {
            if ((styleKey >= HtmlTextWriterStyle.BackgroundColor) && (styleKey < attrNameLookupArray.Length))
            {
                return attrNameLookupArray[(int) styleKey].name;
            }
            return string.Empty;
        }

        public static bool IsStyleEncoded(HtmlTextWriterStyle styleKey)
        {
            if ((styleKey >= HtmlTextWriterStyle.BackgroundColor) && (styleKey < attrNameLookupArray.Length))
            {
                return attrNameLookupArray[(int) styleKey].encode;
            }
            return true;
        }

        internal static void RegisterAttribute(string name, HtmlTextWriterStyle key)
        {
            RegisterAttribute(name, key, false, false);
        }

        internal static void RegisterAttribute(string name, HtmlTextWriterStyle key, bool encode)
        {
            RegisterAttribute(name, key, encode, false);
        }

        internal static void RegisterAttribute(string name, HtmlTextWriterStyle key, bool encode, bool isUrl)
        {
            string str = name.ToLower(CultureInfo.InvariantCulture);
            attrKeyLookupTable.Add(str, key);
            if (key < attrNameLookupArray.Length)
            {
                attrNameLookupArray[(int) key] = new AttributeInformation(name, encode, isUrl);
            }
        }

        public override void Write(bool value)
        {
            this._writer.Write(value);
        }

        public override void Write(char value)
        {
            this._writer.Write(value);
        }

        public override void Write(double value)
        {
            this._writer.Write(value);
        }

        public override void Write(int value)
        {
            this._writer.Write(value);
        }

        public override void Write(long value)
        {
            this._writer.Write(value);
        }

        public override void Write(object value)
        {
            this._writer.Write(value);
        }

        public override void Write(float value)
        {
            this._writer.Write(value);
        }

        public override void Write(string s)
        {
            this._writer.Write(s);
        }

        public override void Write(char[] buffer)
        {
            this._writer.Write(buffer);
        }

        public override void Write(string format, object arg0)
        {
            this._writer.Write(format, arg0);
        }

        public override void Write(string format, params object[] arg)
        {
            this._writer.Write(format, arg);
        }

        public override void Write(char[] buffer, int index, int count)
        {
            this._writer.Write(buffer, index, count);
        }

        public override void Write(string format, object arg0, object arg1)
        {
            this._writer.Write(format, arg0, arg1);
        }

        public void WriteAttribute(string name, string value)
        {
            WriteAttribute(this._writer, GetStyleKey(name), name, value);
        }

        public void WriteAttribute(HtmlTextWriterStyle key, string value)
        {
            WriteAttribute(this._writer, key, GetStyleName(key), value);
        }

        private static void WriteAttribute(TextWriter writer, HtmlTextWriterStyle key, string name, string value)
        {
            writer.Write(name);
            writer.Write(':');
            bool isUrl = false;
            if (key != ~HtmlTextWriterStyle.BackgroundColor)
            {
                isUrl = attrNameLookupArray[(int) key].isUrl;
            }
            if (!isUrl)
            {
                writer.Write(value);
            }
            else
            {
                WriteUrlAttribute(writer, value);
            }
            writer.Write(';');
        }

        internal static void WriteAttributes(TextWriter writer, RenderStyle[] styles, int count)
        {
            for (int i = 0; i < count; i++)
            {
                RenderStyle style = styles[i];
                WriteAttribute(writer, style.key, style.name, style.value);
            }
        }

        public void WriteBeginCssRule(string selector)
        {
            this._writer.Write(selector);
            this._writer.Write(" { ");
        }

        public void WriteEndCssRule()
        {
            this._writer.WriteLine(" }");
        }

        public override void WriteLine()
        {
            this._writer.WriteLine();
        }

        public override void WriteLine(bool value)
        {
            this._writer.WriteLine(value);
        }

        public override void WriteLine(char value)
        {
            this._writer.WriteLine(value);
        }

        public override void WriteLine(double value)
        {
            this._writer.WriteLine(value);
        }

        public override void WriteLine(int value)
        {
            this._writer.WriteLine(value);
        }

        public override void WriteLine(long value)
        {
            this._writer.WriteLine(value);
        }

        public override void WriteLine(object value)
        {
            this._writer.WriteLine(value);
        }

        public override void WriteLine(float value)
        {
            this._writer.WriteLine(value);
        }

        public override void WriteLine(string s)
        {
            this._writer.WriteLine(s);
        }

        public override void WriteLine(char[] buffer)
        {
            this._writer.WriteLine(buffer);
        }

        public override void WriteLine(uint value)
        {
            this._writer.WriteLine(value);
        }

        public override void WriteLine(string format, object arg0)
        {
            this._writer.WriteLine(format, arg0);
        }

        public override void WriteLine(string format, params object[] arg)
        {
            this._writer.WriteLine(format, arg);
        }

        public override void WriteLine(char[] buffer, int index, int count)
        {
            this._writer.WriteLine(buffer, index, count);
        }

        public override void WriteLine(string format, object arg0, object arg1)
        {
            this._writer.WriteLine(format, arg0, arg1);
        }

        private static void WriteUrlAttribute(TextWriter writer, string url)
        {
            string str = url;
            if (StringUtil.StringStartsWith(url, "url("))
            {
                int startIndex = 4;
                int length = url.Length - 4;
                if (StringUtil.StringEndsWith(url, ')'))
                {
                    length--;
                }
                str = url.Substring(startIndex, length).Trim();
            }
            writer.Write("url(");
            writer.Write(HttpUtility.UrlPathEncode(str));
            writer.Write(")");
        }

        public override System.Text.Encoding Encoding
        {
            get
            {
                return this._writer.Encoding;
            }
        }

        public override string NewLine
        {
            get
            {
                return this._writer.NewLine;
            }
            set
            {
                this._writer.NewLine = value;
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
    }
}

