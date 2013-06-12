namespace System.Web.Util
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Web;
    using System.Web.Configuration;

    public class HttpEncoder
    {
        private static HttpEncoder _customEncoder;
        private static readonly Lazy<HttpEncoder> _customEncoderResolver = new Lazy<HttpEncoder>(new Func<HttpEncoder>(HttpEncoder.GetCustomEncoderFromConfig));
        private static readonly HttpEncoder _defaultEncoder = new HttpEncoder();
        private static readonly string[] _headerEncodingTable = new string[] { 
            "%00", "%01", "%02", "%03", "%04", "%05", "%06", "%07", "%08", "%09", "%0a", "%0b", "%0c", "%0d", "%0e", "%0f", 
            "%10", "%11", "%12", "%13", "%14", "%15", "%16", "%17", "%18", "%19", "%1a", "%1b", "%1c", "%1d", "%1e", "%1f"
         };
        private readonly bool _isDefaultEncoder;

        public HttpEncoder()
        {
            this._isDefaultEncoder = base.GetType() == typeof(HttpEncoder);
        }

        private static void AppendCharAsUnicodeJavaScript(StringBuilder builder, char c)
        {
            builder.Append(@"\u");
            builder.Append(((int) c).ToString("x4", CultureInfo.InvariantCulture));
        }

        internal static string CollapsePercentUFromStringInternal(string s, Encoding e)
        {
            int length = s.Length;
            UrlDecoder decoder = new UrlDecoder(length, e);
            if (s.IndexOf("%u", StringComparison.Ordinal) == -1)
            {
                return s;
            }
            for (int i = 0; i < length; i++)
            {
                char ch = s[i];
                if (((ch == '%') && (i < (length - 5))) && (s[i + 1] == 'u'))
                {
                    int num4 = HttpEncoderUtility.HexToInt(s[i + 2]);
                    int num5 = HttpEncoderUtility.HexToInt(s[i + 3]);
                    int num6 = HttpEncoderUtility.HexToInt(s[i + 4]);
                    int num7 = HttpEncoderUtility.HexToInt(s[i + 5]);
                    if (((num4 >= 0) && (num5 >= 0)) && ((num6 >= 0) && (num7 >= 0)))
                    {
                        ch = (char) ((((num4 << 12) | (num5 << 8)) | (num6 << 4)) | num7);
                        i += 5;
                        decoder.AddChar(ch);
                        continue;
                    }
                }
                if ((ch & 0xff80) == 0)
                {
                    decoder.AddByte((byte) ch);
                }
                else
                {
                    decoder.AddChar(ch);
                }
            }
            return decoder.GetString();
        }

        private static HttpEncoder GetCustomEncoderFromConfig()
        {
            HttpRuntimeSection httpRuntime = RuntimeConfig.GetAppConfig().HttpRuntime;
            Type userBaseType = ConfigUtil.GetType(httpRuntime.EncoderType, "encoderType", httpRuntime);
            ConfigUtil.CheckBaseType(typeof(HttpEncoder), userBaseType, "encoderType", httpRuntime);
            return (HttpEncoder) HttpRuntime.CreatePublicInstance(userBaseType);
        }

        private static string HeaderEncodeInternal(string value)
        {
            string str = value;
            if (!HeaderValueNeedsEncoding(value))
            {
                return str;
            }
            StringBuilder builder = new StringBuilder();
            foreach (char ch in value)
            {
                if ((ch < ' ') && (ch != '\t'))
                {
                    builder.Append(_headerEncodingTable[ch]);
                }
                else if (ch == '\x007f')
                {
                    builder.Append("%7f");
                }
                else
                {
                    builder.Append(ch);
                }
            }
            return builder.ToString();
        }

        protected internal virtual void HeaderNameValueEncode(string headerName, string headerValue, out string encodedHeaderName, out string encodedHeaderValue)
        {
            encodedHeaderName = string.IsNullOrEmpty(headerName) ? headerName : HeaderEncodeInternal(headerName);
            encodedHeaderValue = string.IsNullOrEmpty(headerValue) ? headerValue : HeaderEncodeInternal(headerValue);
        }

        private static bool HeaderValueNeedsEncoding(string value)
        {
            foreach (char ch in value)
            {
                if (((ch < ' ') && (ch != '\t')) || (ch == '\x007f'))
                {
                    return true;
                }
            }
            return false;
        }

        internal string HtmlAttributeEncode(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }
            if (this._isDefaultEncoder && (IndexOfHtmlAttributeEncodingChars(value, 0) == -1))
            {
                return value;
            }
            StringWriter output = new StringWriter(CultureInfo.InvariantCulture);
            this.HtmlAttributeEncode(value, output);
            return output.ToString();
        }

        protected internal virtual void HtmlAttributeEncode(string value, TextWriter output)
        {
            if (value != null)
            {
                if (output == null)
                {
                    throw new ArgumentNullException("output");
                }
                HttpWriter writer = output as HttpWriter;
                if (writer != null)
                {
                    HtmlAttributeEncodeInternal(value, writer);
                }
                else
                {
                    HtmlAttributeEncodeInternal(value, output);
                }
            }
        }

        private static unsafe void HtmlAttributeEncodeInternal(string s, TextWriter output)
        {
            int num = IndexOfHtmlAttributeEncodingChars(s, 0);
            if (num == -1)
            {
                output.Write(s);
            }
            else
            {
                int num2 = s.Length - num;
                fixed (char* str = ((char*) s))
                {
                    char* chPtr2 = str;
                    while (num-- > 0)
                    {
                        chPtr2++;
                        output.Write(chPtr2[0]);
                    }
                    while (num2-- > 0)
                    {
                        chPtr2++;
                        char ch = chPtr2[0];
                        if (ch <= '<')
                        {
                            switch (ch)
                            {
                                case '&':
                                {
                                    output.Write("&amp;");
                                    continue;
                                }
                                case '\'':
                                {
                                    output.Write("&#39;");
                                    continue;
                                }
                                case '"':
                                {
                                    output.Write("&quot;");
                                    continue;
                                }
                                case '<':
                                {
                                    output.Write("&lt;");
                                    continue;
                                }
                            }
                            output.Write(ch);
                        }
                        else
                        {
                            output.Write(ch);
                        }
                    }
                }
            }
        }

        private static void HtmlAttributeEncodeInternal(string value, HttpWriter writer)
        {
            int num = IndexOfHtmlAttributeEncodingChars(value, 0);
            if (num == -1)
            {
                writer.Write(value);
                return;
            }
            int length = value.Length;
            int index = 0;
        Label_001D:
            if (num > index)
            {
                writer.WriteString(value, index, num - index);
            }
            switch (value[num])
            {
                case '&':
                    writer.Write("&amp;");
                    break;

                case '\'':
                    writer.Write("&#39;");
                    break;

                case '<':
                    writer.Write("&lt;");
                    break;

                case '"':
                    writer.Write("&quot;");
                    break;
            }
            index = num + 1;
            if (index < length)
            {
                num = IndexOfHtmlAttributeEncodingChars(value, index);
                if (num != -1)
                {
                    goto Label_001D;
                }
                writer.WriteString(value, index, length - index);
            }
        }

        internal string HtmlDecode(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }
            if (this._isDefaultEncoder)
            {
                return WebUtility.HtmlDecode(value);
            }
            StringWriter output = new StringWriter(CultureInfo.InvariantCulture);
            this.HtmlDecode(value, output);
            return output.ToString();
        }

        protected internal virtual void HtmlDecode(string value, TextWriter output)
        {
            WebUtility.HtmlDecode(value, output);
        }

        internal string HtmlEncode(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }
            if (this._isDefaultEncoder)
            {
                return WebUtility.HtmlEncode(value);
            }
            StringWriter output = new StringWriter(CultureInfo.InvariantCulture);
            this.HtmlEncode(value, output);
            return output.ToString();
        }

        protected internal virtual void HtmlEncode(string value, TextWriter output)
        {
            WebUtility.HtmlEncode(value, output);
        }

        private static unsafe int IndexOfHtmlAttributeEncodingChars(string s, int startPos)
        {
            int num = s.Length - startPos;
            fixed (char* str = ((char*) s))
            {
                char* chPtr = str;
                char* chPtr2 = chPtr + startPos;
                while (num > 0)
                {
                    char ch = chPtr2[0];
                    if (ch <= '<')
                    {
                        switch (ch)
                        {
                            case '&':
                            case '\'':
                            case '<':
                            case '"':
                                return (s.Length - num);
                        }
                    }
                    chPtr2++;
                    num--;
                }
            }
            return -1;
        }

        internal static void InitializeOnFirstRequest()
        {
            HttpEncoder local1 = _customEncoderResolver.Value;
        }

        private static bool IsNonAsciiByte(byte b)
        {
            if (b < 0x7f)
            {
                return (b < 0x20);
            }
            return true;
        }

        internal string JavaScriptStringEncode(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }
            StringBuilder builder = null;
            int startIndex = 0;
            int count = 0;
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                if ((((c == '\r') || (c == '\t')) || ((c == '"') || (c == '\''))) || ((((c == '<') || (c == '>')) || ((c == '\\') || (c == '\n'))) || (((c == '\b') || (c == '\f')) || (c < ' '))))
                {
                    if (builder == null)
                    {
                        builder = new StringBuilder(value.Length + 5);
                    }
                    if (count > 0)
                    {
                        builder.Append(value, startIndex, count);
                    }
                    startIndex = i + 1;
                    count = 0;
                }
                switch (c)
                {
                    case '<':
                    case '>':
                    case '\'':
                    {
                        AppendCharAsUnicodeJavaScript(builder, c);
                        continue;
                    }
                    case '\\':
                    {
                        builder.Append(@"\\");
                        continue;
                    }
                    case '\b':
                    {
                        builder.Append(@"\b");
                        continue;
                    }
                    case '\t':
                    {
                        builder.Append(@"\t");
                        continue;
                    }
                    case '\n':
                    {
                        builder.Append(@"\n");
                        continue;
                    }
                    case '\f':
                    {
                        builder.Append(@"\f");
                        continue;
                    }
                    case '\r':
                    {
                        builder.Append(@"\r");
                        continue;
                    }
                    case '"':
                    {
                        builder.Append("\\\"");
                        continue;
                    }
                }
                if (c < ' ')
                {
                    AppendCharAsUnicodeJavaScript(builder, c);
                }
                else
                {
                    count++;
                }
            }
            if (builder == null)
            {
                return value;
            }
            if (count > 0)
            {
                builder.Append(value, startIndex, count);
            }
            return builder.ToString();
        }

        internal string UrlDecode(string value, Encoding encoding)
        {
            if (value == null)
            {
                return null;
            }
            int length = value.Length;
            UrlDecoder decoder = new UrlDecoder(length, encoding);
            for (int i = 0; i < length; i++)
            {
                char ch = value[i];
                if (ch == '+')
                {
                    ch = ' ';
                }
                else if ((ch == '%') && (i < (length - 2)))
                {
                    if ((value[i + 1] == 'u') && (i < (length - 5)))
                    {
                        int num3 = HttpEncoderUtility.HexToInt(value[i + 2]);
                        int num4 = HttpEncoderUtility.HexToInt(value[i + 3]);
                        int num5 = HttpEncoderUtility.HexToInt(value[i + 4]);
                        int num6 = HttpEncoderUtility.HexToInt(value[i + 5]);
                        if (((num3 < 0) || (num4 < 0)) || ((num5 < 0) || (num6 < 0)))
                        {
                            goto Label_010B;
                        }
                        ch = (char) ((((num3 << 12) | (num4 << 8)) | (num5 << 4)) | num6);
                        i += 5;
                        decoder.AddChar(ch);
                        continue;
                    }
                    int num7 = HttpEncoderUtility.HexToInt(value[i + 1]);
                    int num8 = HttpEncoderUtility.HexToInt(value[i + 2]);
                    if ((num7 >= 0) && (num8 >= 0))
                    {
                        byte b = (byte) ((num7 << 4) | num8);
                        i += 2;
                        decoder.AddByte(b);
                        continue;
                    }
                }
            Label_010B:
                if ((ch & 0xff80) == 0)
                {
                    decoder.AddByte((byte) ch);
                }
                else
                {
                    decoder.AddChar(ch);
                }
            }
            return decoder.GetString();
        }

        internal byte[] UrlDecode(byte[] bytes, int offset, int count)
        {
            if (!ValidateUrlEncodingParameters(bytes, offset, count))
            {
                return null;
            }
            int length = 0;
            byte[] sourceArray = new byte[count];
            for (int i = 0; i < count; i++)
            {
                int index = offset + i;
                byte num4 = bytes[index];
                if (num4 == 0x2b)
                {
                    num4 = 0x20;
                }
                else if ((num4 == 0x25) && (i < (count - 2)))
                {
                    int num5 = HttpEncoderUtility.HexToInt((char) bytes[index + 1]);
                    int num6 = HttpEncoderUtility.HexToInt((char) bytes[index + 2]);
                    if ((num5 >= 0) && (num6 >= 0))
                    {
                        num4 = (byte) ((num5 << 4) | num6);
                        i += 2;
                    }
                }
                sourceArray[length++] = num4;
            }
            if (length < sourceArray.Length)
            {
                byte[] destinationArray = new byte[length];
                Array.Copy(sourceArray, destinationArray, length);
                sourceArray = destinationArray;
            }
            return sourceArray;
        }

        internal string UrlDecode(byte[] bytes, int offset, int count, Encoding encoding)
        {
            if (!ValidateUrlEncodingParameters(bytes, offset, count))
            {
                return null;
            }
            UrlDecoder decoder = new UrlDecoder(count, encoding);
            for (int i = 0; i < count; i++)
            {
                int index = offset + i;
                byte b = bytes[index];
                if (b == 0x2b)
                {
                    b = 0x20;
                }
                else if ((b == 0x25) && (i < (count - 2)))
                {
                    if ((bytes[index + 1] == 0x75) && (i < (count - 5)))
                    {
                        int num4 = HttpEncoderUtility.HexToInt((char) bytes[index + 2]);
                        int num5 = HttpEncoderUtility.HexToInt((char) bytes[index + 3]);
                        int num6 = HttpEncoderUtility.HexToInt((char) bytes[index + 4]);
                        int num7 = HttpEncoderUtility.HexToInt((char) bytes[index + 5]);
                        if (((num4 < 0) || (num5 < 0)) || ((num6 < 0) || (num7 < 0)))
                        {
                            goto Label_00E7;
                        }
                        char ch = (char) ((((num4 << 12) | (num5 << 8)) | (num6 << 4)) | num7);
                        i += 5;
                        decoder.AddChar(ch);
                        continue;
                    }
                    int num8 = HttpEncoderUtility.HexToInt((char) bytes[index + 1]);
                    int num9 = HttpEncoderUtility.HexToInt((char) bytes[index + 2]);
                    if ((num8 >= 0) && (num9 >= 0))
                    {
                        b = (byte) ((num8 << 4) | num9);
                        i += 2;
                    }
                }
            Label_00E7:
                decoder.AddByte(b);
            }
            return decoder.GetString();
        }

        protected internal virtual byte[] UrlEncode(byte[] bytes, int offset, int count)
        {
            if (!ValidateUrlEncodingParameters(bytes, offset, count))
            {
                return null;
            }
            int num = 0;
            int num2 = 0;
            for (int i = 0; i < count; i++)
            {
                char ch = (char) bytes[offset + i];
                if (ch == ' ')
                {
                    num++;
                }
                else if (!HttpEncoderUtility.IsUrlSafeChar(ch))
                {
                    num2++;
                }
            }
            if ((num == 0) && (num2 == 0))
            {
                return bytes;
            }
            byte[] buffer = new byte[count + (num2 * 2)];
            int num4 = 0;
            for (int j = 0; j < count; j++)
            {
                byte num6 = bytes[offset + j];
                char ch2 = (char) num6;
                if (HttpEncoderUtility.IsUrlSafeChar(ch2))
                {
                    buffer[num4++] = num6;
                }
                else if (ch2 == ' ')
                {
                    buffer[num4++] = 0x2b;
                }
                else
                {
                    buffer[num4++] = 0x25;
                    buffer[num4++] = (byte) HttpEncoderUtility.IntToHex((num6 >> 4) & 15);
                    buffer[num4++] = (byte) HttpEncoderUtility.IntToHex(num6 & 15);
                }
            }
            return buffer;
        }

        internal byte[] UrlEncode(byte[] bytes, int offset, int count, bool alwaysCreateNewReturnValue)
        {
            byte[] buffer = this.UrlEncode(bytes, offset, count);
            if ((alwaysCreateNewReturnValue && (buffer != null)) && (buffer == bytes))
            {
                return (byte[]) buffer.Clone();
            }
            return buffer;
        }

        internal string UrlEncodeNonAscii(string str, Encoding e)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }
            if (e == null)
            {
                e = Encoding.UTF8;
            }
            byte[] bytes = e.GetBytes(str);
            byte[] buffer2 = this.UrlEncodeNonAscii(bytes, 0, bytes.Length, false);
            return Encoding.ASCII.GetString(buffer2);
        }

        internal byte[] UrlEncodeNonAscii(byte[] bytes, int offset, int count, bool alwaysCreateNewReturnValue)
        {
            if (!ValidateUrlEncodingParameters(bytes, offset, count))
            {
                return null;
            }
            int num = 0;
            for (int i = 0; i < count; i++)
            {
                if (IsNonAsciiByte(bytes[offset + i]))
                {
                    num++;
                }
            }
            if (!alwaysCreateNewReturnValue && (num == 0))
            {
                return bytes;
            }
            byte[] buffer = new byte[count + (num * 2)];
            int num3 = 0;
            for (int j = 0; j < count; j++)
            {
                byte b = bytes[offset + j];
                if (IsNonAsciiByte(b))
                {
                    buffer[num3++] = 0x25;
                    buffer[num3++] = (byte) HttpEncoderUtility.IntToHex((b >> 4) & 15);
                    buffer[num3++] = (byte) HttpEncoderUtility.IntToHex(b & 15);
                }
                else
                {
                    buffer[num3++] = b;
                }
            }
            return buffer;
        }

        internal string UrlEncodeUnicode(string value, bool ignoreAscii)
        {
            if (value == null)
            {
                return null;
            }
            int length = value.Length;
            StringBuilder builder = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                char ch = value[i];
                if ((ch & 0xff80) == 0)
                {
                    if (ignoreAscii || HttpEncoderUtility.IsUrlSafeChar(ch))
                    {
                        builder.Append(ch);
                    }
                    else if (ch == ' ')
                    {
                        builder.Append('+');
                    }
                    else
                    {
                        builder.Append('%');
                        builder.Append(HttpEncoderUtility.IntToHex((ch >> 4) & '\x000f'));
                        builder.Append(HttpEncoderUtility.IntToHex(ch & '\x000f'));
                    }
                }
                else
                {
                    builder.Append("%u");
                    builder.Append(HttpEncoderUtility.IntToHex((ch >> 12) & '\x000f'));
                    builder.Append(HttpEncoderUtility.IntToHex((ch >> 8) & '\x000f'));
                    builder.Append(HttpEncoderUtility.IntToHex((ch >> 4) & '\x000f'));
                    builder.Append(HttpEncoderUtility.IntToHex(ch & '\x000f'));
                }
            }
            return builder.ToString();
        }

        protected internal virtual string UrlPathEncode(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }
            int index = value.IndexOf('?');
            if (index >= 0)
            {
                return (this.UrlPathEncode(value.Substring(0, index)) + value.Substring(index));
            }
            return HttpEncoderUtility.UrlEncodeSpaces(this.UrlEncodeNonAscii(value, Encoding.UTF8));
        }

        internal byte[] UrlTokenDecode(string input)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }
            int length = input.Length;
            if (length < 1)
            {
                return new byte[0];
            }
            int num2 = input[length - 1] - '0';
            if ((num2 < 0) || (num2 > 10))
            {
                return null;
            }
            char[] inArray = new char[(length - 1) + num2];
            for (int i = 0; i < (length - 1); i++)
            {
                char ch = input[i];
                switch (ch)
                {
                    case '-':
                        inArray[i] = '+';
                        break;

                    case '_':
                        inArray[i] = '/';
                        break;

                    default:
                        inArray[i] = ch;
                        break;
                }
            }
            for (int j = length - 1; j < inArray.Length; j++)
            {
                inArray[j] = '=';
            }
            return Convert.FromBase64CharArray(inArray, 0, inArray.Length);
        }

        internal string UrlTokenEncode(byte[] input)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }
            if (input.Length < 1)
            {
                return string.Empty;
            }
            string str = null;
            int index = 0;
            char[] chArray = null;
            str = Convert.ToBase64String(input);
            if (str == null)
            {
                return null;
            }
            index = str.Length;
            while (index > 0)
            {
                if (str[index - 1] != '=')
                {
                    break;
                }
                index--;
            }
            chArray = new char[index + 1];
            chArray[index] = (char) ((0x30 + str.Length) - index);
            for (int i = 0; i < index; i++)
            {
                char ch = str[i];
                switch (ch)
                {
                    case '+':
                        chArray[i] = '-';
                        break;

                    case '/':
                        chArray[i] = '_';
                        break;

                    case '=':
                        chArray[i] = ch;
                        break;

                    default:
                        chArray[i] = ch;
                        break;
                }
            }
            return new string(chArray);
        }

        private static bool ValidateUrlEncodingParameters(byte[] bytes, int offset, int count)
        {
            if ((bytes == null) && (count == 0))
            {
                return false;
            }
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes");
            }
            if ((offset < 0) || (offset > bytes.Length))
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if ((count < 0) || ((offset + count) > bytes.Length))
            {
                throw new ArgumentOutOfRangeException("count");
            }
            return true;
        }

        public static HttpEncoder Current
        {
            get
            {
                HttpContext current = HttpContext.Current;
                if ((current != null) && current.DisableCustomHttpEncoder)
                {
                    return _defaultEncoder;
                }
                if (_customEncoder == null)
                {
                    _customEncoder = _customEncoderResolver.Value;
                }
                return _customEncoder;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                _customEncoder = value;
            }
        }

        public static HttpEncoder Default
        {
            get
            {
                return _defaultEncoder;
            }
        }

        private class UrlDecoder
        {
            private int _bufferSize;
            private byte[] _byteBuffer;
            private char[] _charBuffer;
            private Encoding _encoding;
            private int _numBytes;
            private int _numChars;

            internal UrlDecoder(int bufferSize, Encoding encoding)
            {
                this._bufferSize = bufferSize;
                this._encoding = encoding;
                this._charBuffer = new char[bufferSize];
            }

            internal void AddByte(byte b)
            {
                if (this._byteBuffer == null)
                {
                    this._byteBuffer = new byte[this._bufferSize];
                }
                this._byteBuffer[this._numBytes++] = b;
            }

            internal void AddChar(char ch)
            {
                if (this._numBytes > 0)
                {
                    this.FlushBytes();
                }
                this._charBuffer[this._numChars++] = ch;
            }

            private void FlushBytes()
            {
                if (this._numBytes > 0)
                {
                    this._numChars += this._encoding.GetChars(this._byteBuffer, 0, this._numBytes, this._charBuffer, this._numChars);
                    this._numBytes = 0;
                }
            }

            internal string GetString()
            {
                if (this._numBytes > 0)
                {
                    this.FlushBytes();
                }
                if (this._numChars > 0)
                {
                    return new string(this._charBuffer, 0, this._numChars);
                }
                return string.Empty;
            }
        }
    }
}

