namespace System.Web
{
    using System;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Web.Util;

    public sealed class HttpUtility
    {
        internal static string AspCompatUrlEncode(string s)
        {
            s = UrlEncode(s);
            s = s.Replace("!", "%21");
            s = s.Replace("*", "%2A");
            s = s.Replace("(", "%28");
            s = s.Replace(")", "%29");
            s = s.Replace("-", "%2D");
            s = s.Replace(".", "%2E");
            s = s.Replace("_", "%5F");
            s = s.Replace(@"\", "%5C");
            return s;
        }

        internal static string FormatHttpCookieDateTime(DateTime dt)
        {
            if ((dt < DateTime.MaxValue.AddDays(-1.0)) && (dt > DateTime.MinValue.AddDays(1.0)))
            {
                dt = dt.ToUniversalTime();
            }
            return dt.ToString("ddd, dd-MMM-yyyy HH':'mm':'ss 'GMT'", DateTimeFormatInfo.InvariantInfo);
        }

        internal static string FormatHttpDateTime(DateTime dt)
        {
            if ((dt < DateTime.MaxValue.AddDays(-1.0)) && (dt > DateTime.MinValue.AddDays(1.0)))
            {
                dt = dt.ToUniversalTime();
            }
            return dt.ToString("R", DateTimeFormatInfo.InvariantInfo);
        }

        internal static string FormatHttpDateTimeUtc(DateTime dt)
        {
            return dt.ToString("R", DateTimeFormatInfo.InvariantInfo);
        }

        internal static string FormatPlainTextAsHtml(string s)
        {
            if (s == null)
            {
                return null;
            }
            StringBuilder sb = new StringBuilder();
            StringWriter output = new StringWriter(sb);
            FormatPlainTextAsHtml(s, output);
            return sb.ToString();
        }

        internal static void FormatPlainTextAsHtml(string s, TextWriter output)
        {
            if (s != null)
            {
                int length = s.Length;
                char ch = '\0';
                for (int i = 0; i < length; i++)
                {
                    char ch2 = s[i];
                    switch (ch2)
                    {
                        case '\n':
                            output.Write("<br>");
                            goto Label_0113;

                        case '\r':
                            goto Label_0113;

                        case ' ':
                            if (ch != ' ')
                            {
                                break;
                            }
                            output.Write("&nbsp;");
                            goto Label_0113;

                        case '"':
                            output.Write("&quot;");
                            goto Label_0113;

                        case '&':
                            output.Write("&amp;");
                            goto Label_0113;

                        case '<':
                            output.Write("&lt;");
                            goto Label_0113;

                        case '>':
                            output.Write("&gt;");
                            goto Label_0113;

                        default:
                            if ((ch2 >= '\x00a0') && (ch2 < 'Ā'))
                            {
                                output.Write("&#");
                                output.Write(((int) ch2).ToString(NumberFormatInfo.InvariantInfo));
                                output.Write(';');
                            }
                            else
                            {
                                output.Write(ch2);
                            }
                            goto Label_0113;
                    }
                    output.Write(ch2);
                Label_0113:
                    ch = ch2;
                }
            }
        }

        internal static string FormatPlainTextSpacesAsHtml(string s)
        {
            if (s == null)
            {
                return null;
            }
            StringBuilder sb = new StringBuilder();
            StringWriter writer = new StringWriter(sb);
            int length = s.Length;
            for (int i = 0; i < length; i++)
            {
                char ch = s[i];
                if (ch == ' ')
                {
                    writer.Write("&nbsp;");
                }
                else
                {
                    writer.Write(ch);
                }
            }
            return sb.ToString();
        }

        public static string HtmlAttributeEncode(string s)
        {
            return HttpEncoder.Current.HtmlAttributeEncode(s);
        }

        public static void HtmlAttributeEncode(string s, TextWriter output)
        {
            HttpEncoder.Current.HtmlAttributeEncode(s, output);
        }

        public static string HtmlDecode(string s)
        {
            return HttpEncoder.Current.HtmlDecode(s);
        }

        public static void HtmlDecode(string s, TextWriter output)
        {
            HttpEncoder.Current.HtmlDecode(s, output);
        }

        public static string HtmlEncode(object value)
        {
            if (value == null)
            {
                return null;
            }
            IHtmlString str = value as IHtmlString;
            if (str != null)
            {
                return str.ToHtmlString();
            }
            return HtmlEncode(Convert.ToString(value, CultureInfo.CurrentCulture));
        }

        public static string HtmlEncode(string s)
        {
            return HttpEncoder.Current.HtmlEncode(s);
        }

        public static void HtmlEncode(string s, TextWriter output)
        {
            HttpEncoder.Current.HtmlEncode(s, output);
        }

        public static string JavaScriptStringEncode(string value)
        {
            return JavaScriptStringEncode(value, false);
        }

        public static string JavaScriptStringEncode(string value, bool addDoubleQuotes)
        {
            string str = HttpEncoder.Current.JavaScriptStringEncode(value);
            if (!addDoubleQuotes)
            {
                return str;
            }
            return ("\"" + str + "\"");
        }

        public static NameValueCollection ParseQueryString(string query)
        {
            return ParseQueryString(query, Encoding.UTF8);
        }

        public static NameValueCollection ParseQueryString(string query, Encoding encoding)
        {
            if (query == null)
            {
                throw new ArgumentNullException("query");
            }
            if (encoding == null)
            {
                throw new ArgumentNullException("encoding");
            }
            if ((query.Length > 0) && (query[0] == '?'))
            {
                query = query.Substring(1);
            }
            return new HttpValueCollection(query, false, true, encoding);
        }

        public static string UrlDecode(string str)
        {
            if (str == null)
            {
                return null;
            }
            return UrlDecode(str, Encoding.UTF8);
        }

        public static string UrlDecode(string str, Encoding e)
        {
            return HttpEncoder.Current.UrlDecode(str, e);
        }

        public static string UrlDecode(byte[] bytes, Encoding e)
        {
            if (bytes == null)
            {
                return null;
            }
            return UrlDecode(bytes, 0, bytes.Length, e);
        }

        public static string UrlDecode(byte[] bytes, int offset, int count, Encoding e)
        {
            return HttpEncoder.Current.UrlDecode(bytes, offset, count, e);
        }

        public static byte[] UrlDecodeToBytes(string str)
        {
            if (str == null)
            {
                return null;
            }
            return UrlDecodeToBytes(str, Encoding.UTF8);
        }

        public static byte[] UrlDecodeToBytes(byte[] bytes)
        {
            if (bytes == null)
            {
                return null;
            }
            return UrlDecodeToBytes(bytes, 0, (bytes != null) ? bytes.Length : 0);
        }

        public static byte[] UrlDecodeToBytes(string str, Encoding e)
        {
            if (str == null)
            {
                return null;
            }
            return UrlDecodeToBytes(e.GetBytes(str));
        }

        public static byte[] UrlDecodeToBytes(byte[] bytes, int offset, int count)
        {
            return HttpEncoder.Current.UrlDecode(bytes, offset, count);
        }

        public static string UrlEncode(string str)
        {
            if (str == null)
            {
                return null;
            }
            return UrlEncode(str, Encoding.UTF8);
        }

        public static string UrlEncode(byte[] bytes)
        {
            if (bytes == null)
            {
                return null;
            }
            return Encoding.ASCII.GetString(UrlEncodeToBytes(bytes));
        }

        public static string UrlEncode(string str, Encoding e)
        {
            if (str == null)
            {
                return null;
            }
            return Encoding.ASCII.GetString(UrlEncodeToBytes(str, e));
        }

        public static string UrlEncode(byte[] bytes, int offset, int count)
        {
            if (bytes == null)
            {
                return null;
            }
            return Encoding.ASCII.GetString(UrlEncodeToBytes(bytes, offset, count));
        }

        internal static string UrlEncodeNonAscii(string str, Encoding e)
        {
            return HttpEncoder.Current.UrlEncodeNonAscii(str, e);
        }

        public static byte[] UrlEncodeToBytes(string str)
        {
            if (str == null)
            {
                return null;
            }
            return UrlEncodeToBytes(str, Encoding.UTF8);
        }

        public static byte[] UrlEncodeToBytes(byte[] bytes)
        {
            if (bytes == null)
            {
                return null;
            }
            return UrlEncodeToBytes(bytes, 0, bytes.Length);
        }

        public static byte[] UrlEncodeToBytes(string str, Encoding e)
        {
            if (str == null)
            {
                return null;
            }
            byte[] bytes = e.GetBytes(str);
            return HttpEncoder.Current.UrlEncode(bytes, 0, bytes.Length, false);
        }

        public static byte[] UrlEncodeToBytes(byte[] bytes, int offset, int count)
        {
            return HttpEncoder.Current.UrlEncode(bytes, offset, count, true);
        }

        public static string UrlEncodeUnicode(string str)
        {
            return HttpEncoder.Current.UrlEncodeUnicode(str, false);
        }

        public static byte[] UrlEncodeUnicodeToBytes(string str)
        {
            if (str == null)
            {
                return null;
            }
            return Encoding.ASCII.GetBytes(UrlEncodeUnicode(str));
        }

        public static string UrlPathEncode(string str)
        {
            return HttpEncoder.Current.UrlPathEncode(str);
        }
    }
}

