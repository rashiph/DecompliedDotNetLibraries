namespace System.Net
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.Text;

    [Serializable, ComVisible(true)]
    public class WebHeaderCollection : NameValueCollection, ISerializable
    {
        private const int ApproxAveHeaderLineSize = 30;
        private const int ApproxHighAvgNumHeaders = 0x10;
        private const int c_AcceptRanges = 0;
        private const int c_CacheControl = 2;
        private const int c_ContentLength = 1;
        private const int c_ContentType = 3;
        private const int c_Date = 4;
        private const int c_ETag = 6;
        private const int c_Expires = 5;
        private const int c_LastModified = 7;
        private const int c_Location = 8;
        private const int c_P3P = 10;
        private const int c_ProxyAuthenticate = 9;
        private const int c_Server = 13;
        private const int c_SetCookie = 12;
        private const int c_SetCookie2 = 11;
        private const int c_Via = 14;
        private const int c_WwwAuthenticate = 15;
        private const int c_XAspNetVersion = 0x10;
        private const int c_XPoweredBy = 0x11;
        private static readonly HeaderInfoTable HInfo = new HeaderInfoTable();
        private static readonly char[] HttpTrimCharacters = new char[] { '\t', '\n', '\v', '\f', '\r', ' ' };
        private string[] m_CommonHeaders;
        private NameValueCollection m_InnerCollection;
        private int m_NumCommonHeaders;
        private WebHeaderCollectionType m_Type;
        private static RfcChar[] RfcCharMap = new RfcChar[] { 
            RfcChar.Ctl, RfcChar.Ctl, RfcChar.Ctl, RfcChar.Ctl, RfcChar.Ctl, RfcChar.Ctl, RfcChar.Ctl, RfcChar.Ctl, RfcChar.Ctl, RfcChar.WS, RfcChar.LF, RfcChar.Ctl, RfcChar.Ctl, RfcChar.CR, RfcChar.Ctl, RfcChar.Ctl, 
            RfcChar.Ctl, RfcChar.Ctl, RfcChar.Ctl, RfcChar.Ctl, RfcChar.Ctl, RfcChar.Ctl, RfcChar.Ctl, RfcChar.Ctl, RfcChar.Ctl, RfcChar.Ctl, RfcChar.Ctl, RfcChar.Ctl, RfcChar.Ctl, RfcChar.Ctl, RfcChar.Ctl, RfcChar.Ctl, 
            RfcChar.WS, RfcChar.Reg, RfcChar.Delim, RfcChar.Reg, RfcChar.Reg, RfcChar.Reg, RfcChar.Reg, RfcChar.Reg, RfcChar.Delim, RfcChar.Delim, RfcChar.Reg, RfcChar.Reg, RfcChar.Delim, RfcChar.Reg, RfcChar.Reg, RfcChar.Delim, 
            RfcChar.Reg, RfcChar.Reg, RfcChar.Reg, RfcChar.Reg, RfcChar.Reg, RfcChar.Reg, RfcChar.Reg, RfcChar.Reg, RfcChar.Reg, RfcChar.Reg, RfcChar.Colon, RfcChar.Delim, RfcChar.Delim, RfcChar.Delim, RfcChar.Delim, RfcChar.Delim, 
            RfcChar.Delim, RfcChar.Reg, RfcChar.Reg, RfcChar.Reg, RfcChar.Reg, RfcChar.Reg, RfcChar.Reg, RfcChar.Reg, RfcChar.Reg, RfcChar.Reg, RfcChar.Reg, RfcChar.Reg, RfcChar.Reg, RfcChar.Reg, RfcChar.Reg, RfcChar.Reg, 
            RfcChar.Reg, RfcChar.Reg, RfcChar.Reg, RfcChar.Reg, RfcChar.Reg, RfcChar.Reg, RfcChar.Reg, RfcChar.Reg, RfcChar.Reg, RfcChar.Reg, RfcChar.Reg, RfcChar.Delim, RfcChar.Delim, RfcChar.Delim, RfcChar.Reg, RfcChar.Reg, 
            RfcChar.Reg, RfcChar.Reg, RfcChar.Reg, RfcChar.Reg, RfcChar.Reg, RfcChar.Reg, RfcChar.Reg, RfcChar.Reg, RfcChar.Reg, RfcChar.Reg, RfcChar.Reg, RfcChar.Reg, RfcChar.Reg, RfcChar.Reg, RfcChar.Reg, RfcChar.Reg, 
            RfcChar.Reg, RfcChar.Reg, RfcChar.Reg, RfcChar.Reg, RfcChar.Reg, RfcChar.Reg, RfcChar.Reg, RfcChar.Reg, RfcChar.Reg, RfcChar.Reg, RfcChar.Reg, RfcChar.Delim, RfcChar.Reg, RfcChar.Delim, RfcChar.Reg, RfcChar.Ctl
         };
        private static readonly sbyte[] s_CommonHeaderHints = new sbyte[] { 
            -1, 0, -1, 1, 4, 5, -1, -1, -1, -1, -1, -1, 7, -1, -1, -1, 
            9, -1, -1, 11, -1, -1, 14, 15, 0x10, -1, -1, -1, -1, -1, -1, -1
         };
        private static readonly string[] s_CommonHeaderNames = new string[] { 
            "Accept-Ranges", "Content-Length", "Cache-Control", "Content-Type", "Date", "Expires", "ETag", "Last-Modified", "Location", "Proxy-Authenticate", "P3P", "Set-Cookie2", "Set-Cookie", "Server", "Via", "WWW-Authenticate", 
            "X-AspNet-Version", "X-Powered-By", "["
         };

        public WebHeaderCollection() : base(DBNull.Value)
        {
        }

        internal WebHeaderCollection(NameValueCollection cc) : base(DBNull.Value)
        {
            this.m_InnerCollection = new NameValueCollection(cc.Count + 2, CaseInsensitiveAscii.StaticInstance);
            int count = cc.Count;
            for (int i = 0; i < count; i++)
            {
                string key = cc.GetKey(i);
                string[] values = cc.GetValues(i);
                if (values != null)
                {
                    for (int j = 0; j < values.Length; j++)
                    {
                        this.InnerCollection.Add(key, values[j]);
                    }
                }
                else
                {
                    this.InnerCollection.Add(key, null);
                }
            }
        }

        internal WebHeaderCollection(WebHeaderCollectionType type) : base(DBNull.Value)
        {
            this.m_Type = type;
            if (type == WebHeaderCollectionType.HttpWebResponse)
            {
                this.m_CommonHeaders = new string[s_CommonHeaderNames.Length - 1];
            }
        }

        protected WebHeaderCollection(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(DBNull.Value)
        {
            int num = serializationInfo.GetInt32("Count");
            this.m_InnerCollection = new NameValueCollection(num + 2, CaseInsensitiveAscii.StaticInstance);
            for (int i = 0; i < num; i++)
            {
                string name = serializationInfo.GetString(i.ToString(NumberFormatInfo.InvariantInfo));
                string str2 = serializationInfo.GetString((i + num).ToString(NumberFormatInfo.InvariantInfo));
                this.InnerCollection.Add(name, str2);
            }
        }

        public void Add(string header)
        {
            if (ValidationHelper.IsBlankString(header))
            {
                throw new ArgumentNullException("header");
            }
            int index = header.IndexOf(':');
            if (index < 0)
            {
                throw new ArgumentException(SR.GetString("net_WebHeaderMissingColon"), "header");
            }
            string name = header.Substring(0, index);
            string str2 = header.Substring(index + 1);
            name = CheckBadChars(name, false);
            this.ThrowOnRestrictedHeader(name);
            str2 = CheckBadChars(str2, true);
            if (((this.m_Type == WebHeaderCollectionType.HttpListenerResponse) && (str2 != null)) && (str2.Length > 0xffff))
            {
                throw new ArgumentOutOfRangeException("value", str2, SR.GetString("net_headers_toolong", new object[] { (ushort) 0xffff }));
            }
            this.NormalizeCommonHeaders();
            base.InvalidateCachedArrays();
            this.InnerCollection.Add(name, str2);
        }

        public void Add(HttpRequestHeader header, string value)
        {
            if (!this.AllowHttpRequestHeader)
            {
                throw new InvalidOperationException(SR.GetString("net_headers_req"));
            }
            this.Add(UnsafeNclNativeMethods.HttpApi.HTTP_REQUEST_HEADER_ID.ToString((int) header), value);
        }

        public void Add(HttpResponseHeader header, string value)
        {
            if (!this.AllowHttpResponseHeader)
            {
                throw new InvalidOperationException(SR.GetString("net_headers_rsp"));
            }
            if (((this.m_Type == WebHeaderCollectionType.HttpListenerResponse) && (value != null)) && (value.Length > 0xffff))
            {
                throw new ArgumentOutOfRangeException("value", value, SR.GetString("net_headers_toolong", new object[] { (ushort) 0xffff }));
            }
            this.Add(UnsafeNclNativeMethods.HttpApi.HTTP_RESPONSE_HEADER_ID.ToString((int) header), value);
        }

        public override void Add(string name, string value)
        {
            name = CheckBadChars(name, false);
            this.ThrowOnRestrictedHeader(name);
            value = CheckBadChars(value, true);
            if (((this.m_Type == WebHeaderCollectionType.HttpListenerResponse) && (value != null)) && (value.Length > 0xffff))
            {
                throw new ArgumentOutOfRangeException("value", value, SR.GetString("net_headers_toolong", new object[] { (ushort) 0xffff }));
            }
            this.NormalizeCommonHeaders();
            base.InvalidateCachedArrays();
            this.InnerCollection.Add(name, value);
        }

        internal void AddInternal(string name, string value)
        {
            this.NormalizeCommonHeaders();
            base.InvalidateCachedArrays();
            this.InnerCollection.Add(name, value);
        }

        private void AddInternalNotCommon(string name, string value)
        {
            base.InvalidateCachedArrays();
            this.InnerCollection.Add(name, value);
        }

        protected void AddWithoutValidate(string headerName, string headerValue)
        {
            headerName = CheckBadChars(headerName, false);
            headerValue = CheckBadChars(headerValue, true);
            if (((this.m_Type == WebHeaderCollectionType.HttpListenerResponse) && (headerValue != null)) && (headerValue.Length > 0xffff))
            {
                throw new ArgumentOutOfRangeException("headerValue", headerValue, SR.GetString("net_headers_toolong", new object[] { (ushort) 0xffff }));
            }
            this.NormalizeCommonHeaders();
            base.InvalidateCachedArrays();
            this.InnerCollection.Add(headerName, headerValue);
        }

        internal void ChangeInternal(string name, string value)
        {
            this.NormalizeCommonHeaders();
            base.InvalidateCachedArrays();
            this.InnerCollection.Set(name, value);
        }

        internal static string CheckBadChars(string name, bool isHeaderValue)
        {
            if ((name == null) || (name.Length == 0))
            {
                if (!isHeaderValue)
                {
                    throw ((name == null) ? new ArgumentNullException("name") : new ArgumentException(SR.GetString("net_emptystringcall", new object[] { "name" }), "name"));
                }
                return string.Empty;
            }
            if (isHeaderValue)
            {
                name = name.Trim(HttpTrimCharacters);
                int num = 0;
                for (int i = 0; i < name.Length; i++)
                {
                    char ch = (char) ('\x00ff' & name[i]);
                    switch (num)
                    {
                        case 0:
                        {
                            if (ch != '\r')
                            {
                                break;
                            }
                            num = 1;
                            continue;
                        }
                        case 1:
                        {
                            if (ch != '\n')
                            {
                                throw new ArgumentException(SR.GetString("net_WebHeaderInvalidCRLFChars"), "value");
                            }
                            num = 2;
                            continue;
                        }
                        case 2:
                        {
                            if ((ch != ' ') && (ch != '\t'))
                            {
                                throw new ArgumentException(SR.GetString("net_WebHeaderInvalidCRLFChars"), "value");
                            }
                            num = 0;
                            continue;
                        }
                        default:
                        {
                            continue;
                        }
                    }
                    if (ch == '\n')
                    {
                        num = 2;
                    }
                    else if ((ch == '\x007f') || ((ch < ' ') && (ch != '\t')))
                    {
                        throw new ArgumentException(SR.GetString("net_WebHeaderInvalidControlChars"), "value");
                    }
                }
                if (num != 0)
                {
                    throw new ArgumentException(SR.GetString("net_WebHeaderInvalidCRLFChars"), "value");
                }
                return name;
            }
            if (name.IndexOfAny(ValidationHelper.InvalidParamChars) != -1)
            {
                throw new ArgumentException(SR.GetString("net_WebHeaderInvalidHeaderChars"), "name");
            }
            if (ContainsNonAsciiChars(name))
            {
                throw new ArgumentException(SR.GetString("net_WebHeaderInvalidNonAsciiChars"), "name");
            }
            return name;
        }

        internal void CheckUpdate(string name, string value)
        {
            value = CheckBadChars(value, true);
            this.ChangeInternal(name, value);
        }

        public override void Clear()
        {
            this.m_CommonHeaders = null;
            this.m_NumCommonHeaders = 0;
            base.InvalidateCachedArrays();
            if (this.m_InnerCollection != null)
            {
                this.m_InnerCollection.Clear();
            }
        }

        internal static bool ContainsNonAsciiChars(string token)
        {
            for (int i = 0; i < token.Length; i++)
            {
                if ((token[i] < ' ') || (token[i] > '~'))
                {
                    return true;
                }
            }
            return false;
        }

        public override string Get(int index)
        {
            this.NormalizeCommonHeaders();
            return this.InnerCollection.Get(index);
        }

        public override string Get(string name)
        {
            string str;
            if (((this.m_CommonHeaders == null) || (name == null)) || ((name.Length <= 0) || (name[0] >= 'Ā')))
            {
                goto Label_00EF;
            }
            int num = s_CommonHeaderHints[name[0] & '\x001f'];
            if (num < 0)
            {
                goto Label_00EF;
            }
        Label_0046:
            str = s_CommonHeaderNames[num++];
            if ((str.Length >= name.Length) && (CaseInsensitiveAscii.AsciiToLower[name[0]] == CaseInsensitiveAscii.AsciiToLower[str[0]]))
            {
                if (str.Length <= name.Length)
                {
                    int num2 = 1;
                    while (num2 < str.Length)
                    {
                        if ((name[num2] != str[num2]) && ((name[num2] > '\x00ff') || (CaseInsensitiveAscii.AsciiToLower[name[num2]] != CaseInsensitiveAscii.AsciiToLower[str[num2]])))
                        {
                            break;
                        }
                        num2++;
                    }
                    if (num2 == str.Length)
                    {
                        return this.m_CommonHeaders[num - 1];
                    }
                }
                goto Label_0046;
            }
        Label_00EF:
            if (this.m_InnerCollection == null)
            {
                return null;
            }
            return this.m_InnerCollection.Get(name);
        }

        internal static string GetAsString(NameValueCollection cc, bool winInetCompat, bool forTrace)
        {
            if ((cc == null) || (cc.Count == 0))
            {
                return "\r\n";
            }
            StringBuilder builder = new StringBuilder(30 * cc.Count);
            string str = cc[string.Empty];
            if (str != null)
            {
                builder.Append(str).Append("\r\n");
            }
            for (int i = 0; i < cc.Count; i++)
            {
                string key = cc.GetKey(i);
                string str3 = cc.Get(i);
                if (!ValidationHelper.IsBlankString(key))
                {
                    builder.Append(key);
                    if (winInetCompat)
                    {
                        builder.Append(':');
                    }
                    else
                    {
                        builder.Append(": ");
                    }
                    builder.Append(str3).Append("\r\n");
                }
            }
            if (!forTrace)
            {
                builder.Append("\r\n");
            }
            return builder.ToString();
        }

        public override IEnumerator GetEnumerator()
        {
            this.NormalizeCommonHeaders();
            return new NameObjectCollectionBase.NameObjectKeysEnumerator(this.InnerCollection);
        }

        public override string GetKey(int index)
        {
            this.NormalizeCommonHeaders();
            return this.InnerCollection.GetKey(index);
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.SerializationFormatter)]
        public override void GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
            this.NormalizeCommonHeaders();
            serializationInfo.AddValue("Count", this.Count);
            for (int i = 0; i < this.Count; i++)
            {
                serializationInfo.AddValue(i.ToString(NumberFormatInfo.InvariantInfo), this.GetKey(i));
                serializationInfo.AddValue((i + this.Count).ToString(NumberFormatInfo.InvariantInfo), this.Get(i));
            }
        }

        public override string[] GetValues(int index)
        {
            this.NormalizeCommonHeaders();
            return this.InnerCollection.GetValues(index);
        }

        public override string[] GetValues(string header)
        {
            this.NormalizeCommonHeaders();
            HeaderInfo info = HInfo[header];
            string[] values = this.InnerCollection.GetValues(header);
            if (((info != null) && (values != null)) && info.AllowMultiValues)
            {
                ArrayList list = null;
                for (int i = 0; i < values.Length; i++)
                {
                    string[] c = info.Parser(values[i]);
                    if (list == null)
                    {
                        if (c.Length > 1)
                        {
                            list = new ArrayList(values);
                            list.RemoveRange(i, values.Length - i);
                            list.AddRange(c);
                        }
                    }
                    else
                    {
                        list.AddRange(c);
                    }
                }
                if (list != null)
                {
                    string[] array = new string[list.Count];
                    list.CopyTo(array);
                    return array;
                }
            }
            return values;
        }

        internal override bool InternalHasKeys()
        {
            this.NormalizeCommonHeaders();
            if (this.m_InnerCollection == null)
            {
                return false;
            }
            return this.m_InnerCollection.HasKeys();
        }

        public static bool IsRestricted(string headerName)
        {
            return IsRestricted(headerName, false);
        }

        public static bool IsRestricted(string headerName, bool response)
        {
            if (!response)
            {
                return HInfo[CheckBadChars(headerName, false)].IsRequestRestricted;
            }
            return HInfo[CheckBadChars(headerName, false)].IsResponseRestricted;
        }

        internal static bool IsValidToken(string token)
        {
            return (((token.Length > 0) && (token.IndexOfAny(ValidationHelper.InvalidParamChars) == -1)) && !ContainsNonAsciiChars(token));
        }

        private void NormalizeCommonHeaders()
        {
            if (this.m_CommonHeaders != null)
            {
                for (int i = 0; i < this.m_CommonHeaders.Length; i++)
                {
                    if (this.m_CommonHeaders[i] != null)
                    {
                        this.InnerCollection.Add(s_CommonHeaderNames[i], this.m_CommonHeaders[i]);
                    }
                }
                this.m_CommonHeaders = null;
                this.m_NumCommonHeaders = 0;
            }
        }

        public override void OnDeserialization(object sender)
        {
        }

        internal unsafe DataParseStatus ParseHeaders(byte[] buffer, int size, ref int unparsed, ref int totalResponseHeadersLength, int maximumResponseHeadersLength, ref WebParseError parseError)
        {
            DataParseStatus status2;
            try
            {
                byte[] buffer2;
                if (((buffer2 = buffer) == null) || (buffer2.Length == 0))
                {
                    numRef = null;
                    goto Label_001A;
                }
                fixed (byte* numRef = buffer2)
                {
                    char ch;
                    string str2;
                Label_001A:
                    if (buffer.Length < size)
                    {
                        return DataParseStatus.NeedMoreData;
                    }
                    int num = -1;
                    int num2 = -1;
                    int num3 = -1;
                    int num4 = -1;
                    int num5 = -1;
                    int num6 = unparsed;
                    int num7 = totalResponseHeadersLength;
                    WebParseErrorCode generic = WebParseErrorCode.Generic;
                    DataParseStatus invalid = DataParseStatus.Invalid;
                Label_0044:
                    str2 = string.Empty;
                    string str3 = string.Empty;
                    bool flag = false;
                    string str = null;
                    if (this.Count == 0)
                    {
                        while (num6 < size)
                        {
                            ch = *((char*) (numRef + num6));
                            if ((ch != ' ') && (ch != '\t'))
                            {
                                break;
                            }
                            num6++;
                            if ((maximumResponseHeadersLength >= 0) && (++num7 >= maximumResponseHeadersLength))
                            {
                                invalid = DataParseStatus.DataTooBig;
                                goto Label_0316;
                            }
                        }
                        if (num6 == size)
                        {
                            invalid = DataParseStatus.NeedMoreData;
                            goto Label_0316;
                        }
                    }
                    num = num6;
                    while (num6 < size)
                    {
                        ch = *((char*) (numRef + num6));
                        if ((ch != ':') && (ch != '\n'))
                        {
                            if (ch > ' ')
                            {
                                num2 = num6;
                            }
                            num6++;
                            if ((maximumResponseHeadersLength < 0) || (++num7 < maximumResponseHeadersLength))
                            {
                                continue;
                            }
                            invalid = DataParseStatus.DataTooBig;
                        }
                        else
                        {
                            if (ch != ':')
                            {
                                break;
                            }
                            num6++;
                            if ((maximumResponseHeadersLength < 0) || (++num7 < maximumResponseHeadersLength))
                            {
                                break;
                            }
                            invalid = DataParseStatus.DataTooBig;
                        }
                        goto Label_0316;
                    }
                    if (num6 == size)
                    {
                        invalid = DataParseStatus.NeedMoreData;
                        goto Label_0316;
                    }
                Label_0114:;
                    num5 = ((this.Count == 0) && (num2 < 0)) ? 1 : 0;
                    while ((num6 < size) && (num5 < 2))
                    {
                        ch = *((char*) (numRef + num6));
                        if (ch > ' ')
                        {
                            break;
                        }
                        if (ch == '\n')
                        {
                            num5++;
                            if (num5 == 1)
                            {
                                if ((num6 + 1) == size)
                                {
                                    invalid = DataParseStatus.NeedMoreData;
                                    goto Label_0316;
                                }
                                flag = (numRef[num6 + 1] == 0x20) || (numRef[num6 + 1] == 9);
                            }
                        }
                        num6++;
                        if ((maximumResponseHeadersLength >= 0) && (++num7 >= maximumResponseHeadersLength))
                        {
                            invalid = DataParseStatus.DataTooBig;
                            goto Label_0316;
                        }
                    }
                    if ((num5 != 2) && ((num5 != 1) || flag))
                    {
                        if (num6 == size)
                        {
                            invalid = DataParseStatus.NeedMoreData;
                            goto Label_0316;
                        }
                        num3 = num6;
                        while (num6 < size)
                        {
                            ch = *((char*) (numRef + num6));
                            if (ch == '\n')
                            {
                                break;
                            }
                            if (ch > ' ')
                            {
                                num4 = num6;
                            }
                            num6++;
                            if ((maximumResponseHeadersLength >= 0) && (++num7 >= maximumResponseHeadersLength))
                            {
                                invalid = DataParseStatus.DataTooBig;
                                goto Label_0316;
                            }
                        }
                        if (num6 == size)
                        {
                            invalid = DataParseStatus.NeedMoreData;
                            goto Label_0316;
                        }
                        num5 = 0;
                        while ((num6 < size) && (num5 < 2))
                        {
                            ch = *((char*) (numRef + num6));
                            if ((ch != '\r') && (ch != '\n'))
                            {
                                break;
                            }
                            if (ch == '\n')
                            {
                                num5++;
                            }
                            num6++;
                            if ((maximumResponseHeadersLength >= 0) && (++num7 >= maximumResponseHeadersLength))
                            {
                                invalid = DataParseStatus.DataTooBig;
                                goto Label_0316;
                            }
                        }
                        if ((num6 == size) && (num5 < 2))
                        {
                            invalid = DataParseStatus.NeedMoreData;
                            goto Label_0316;
                        }
                    }
                    if (((num3 >= 0) && (num3 > num2)) && (num4 >= num3))
                    {
                        str3 = HeaderEncoding.GetString(numRef + num3, (num4 - num3) + 1);
                    }
                    str = (str == null) ? str3 : (str + " " + str3);
                    if ((num6 < size) && (num5 == 1))
                    {
                        switch (*(((char*) (numRef + num6))))
                        {
                            case ' ':
                            case '\t':
                                num6++;
                                if ((maximumResponseHeadersLength < 0) || (++num7 < maximumResponseHeadersLength))
                                {
                                    goto Label_0114;
                                }
                                invalid = DataParseStatus.DataTooBig;
                                goto Label_0316;
                        }
                    }
                    if ((num >= 0) && (num2 >= num))
                    {
                        str2 = HeaderEncoding.GetString(numRef + num, (num2 - num) + 1);
                    }
                    if (str2.Length > 0)
                    {
                        this.AddInternal(str2, str);
                    }
                    totalResponseHeadersLength = num7;
                    unparsed = num6;
                    if (num5 != 2)
                    {
                        goto Label_0044;
                    }
                    invalid = DataParseStatus.Done;
                Label_0316:
                    if (invalid == DataParseStatus.Invalid)
                    {
                        parseError.Section = WebParseErrorSection.ResponseHeader;
                        parseError.Code = generic;
                    }
                    status2 = invalid;
                }
            }
            finally
            {
                numRef = null;
            }
            return status2;
        }

        internal unsafe DataParseStatus ParseHeadersStrict(byte[] buffer, int size, ref int unparsed, ref int totalResponseHeadersLength, int maximumResponseHeadersLength, ref WebParseError parseError)
        {
            WebParseErrorCode generic = WebParseErrorCode.Generic;
            DataParseStatus invalid = DataParseStatus.Invalid;
            int index = unparsed;
            int num2 = (maximumResponseHeadersLength <= 0) ? 0x7fffffff : ((maximumResponseHeadersLength - totalResponseHeadersLength) + index);
            DataParseStatus dataTooBig = DataParseStatus.DataTooBig;
            if (size < num2)
            {
                num2 = size;
                dataTooBig = DataParseStatus.NeedMoreData;
            }
            if (index >= num2)
            {
                invalid = dataTooBig;
            }
            else
            {
                try
                {
                    byte[] buffer2;
                    if (((buffer2 = buffer) == null) || (buffer2.Length == 0))
                    {
                        numRef = null;
                        goto Label_0054;
                    }
                    fixed (byte* numRef = buffer2)
                    {
                        RfcChar ch;
                        string str4;
                    Label_0054:
                        if (numRef[index] == 13)
                        {
                            if (++index == num2)
                            {
                                invalid = dataTooBig;
                            }
                            else if (numRef[index++] == 10)
                            {
                                totalResponseHeadersLength += index - unparsed;
                                unparsed = index;
                                invalid = DataParseStatus.Done;
                            }
                            else
                            {
                                invalid = DataParseStatus.Invalid;
                                generic = WebParseErrorCode.CrLfError;
                            }
                            goto Label_042C;
                        }
                        int num3 = index;
                        while ((index < num2) && ((ch = (numRef[index] > 0x7f) ? RfcChar.High : RfcCharMap[numRef[index]]) == RfcChar.Reg))
                        {
                            index++;
                        }
                        if (index == num2)
                        {
                            invalid = dataTooBig;
                            goto Label_042C;
                        }
                        if (index == num3)
                        {
                            invalid = DataParseStatus.Invalid;
                            generic = WebParseErrorCode.InvalidHeaderName;
                            goto Label_042C;
                        }
                        int num4 = index - 1;
                        int num5 = 0;
                        while ((index < num2) && ((ch = (numRef[index] > 0x7f) ? RfcChar.High : RfcCharMap[numRef[index]]) != RfcChar.Colon))
                        {
                            switch (ch)
                            {
                                case RfcChar.CR:
                                    if (num5 != 0)
                                    {
                                        break;
                                    }
                                    num5 = 1;
                                    goto Label_012B;

                                case RfcChar.LF:
                                    if (num5 != 1)
                                    {
                                        break;
                                    }
                                    num5 = 2;
                                    goto Label_012B;

                                case RfcChar.WS:
                                    if (num5 == 1)
                                    {
                                        break;
                                    }
                                    num5 = 0;
                                    goto Label_012B;
                            }
                            invalid = DataParseStatus.Invalid;
                            generic = WebParseErrorCode.CrLfError;
                            goto Label_042C;
                        Label_012B:
                            index++;
                        }
                        if (index == num2)
                        {
                            invalid = dataTooBig;
                            goto Label_042C;
                        }
                        if (num5 != 0)
                        {
                            invalid = DataParseStatus.Invalid;
                            generic = WebParseErrorCode.IncompleteHeaderLine;
                            goto Label_042C;
                        }
                        if (++index == num2)
                        {
                            invalid = dataTooBig;
                            goto Label_042C;
                        }
                        int num6 = -1;
                        int num7 = -1;
                        StringBuilder builder = null;
                        while ((index < num2) && (((ch = (numRef[index] > 0x7f) ? RfcChar.High : RfcCharMap[numRef[index]]) == RfcChar.WS) || (num5 != 2)))
                        {
                            string str;
                            switch (ch)
                            {
                                case RfcChar.High:
                                case RfcChar.Reg:
                                case RfcChar.Colon:
                                case RfcChar.Delim:
                                    if (num5 == 1)
                                    {
                                        break;
                                    }
                                    if (num5 != 3)
                                    {
                                        goto Label_023D;
                                    }
                                    num5 = 0;
                                    if (num6 != -1)
                                    {
                                        str = HeaderEncoding.GetString(numRef + num6, (num7 - num6) + 1);
                                        if (builder != null)
                                        {
                                            goto Label_0223;
                                        }
                                        builder = new StringBuilder(str, str.Length * 5);
                                    }
                                    goto Label_023A;

                                case RfcChar.CR:
                                    if (num5 != 0)
                                    {
                                        break;
                                    }
                                    num5 = 1;
                                    goto Label_0253;

                                case RfcChar.LF:
                                    if (num5 != 1)
                                    {
                                        break;
                                    }
                                    num5 = 2;
                                    goto Label_0253;

                                case RfcChar.WS:
                                    switch (num5)
                                    {
                                        case 1:
                                            goto Label_024A;

                                        case 2:
                                            num5 = 3;
                                            break;
                                    }
                                    goto Label_0253;
                            }
                            goto Label_024A;
                        Label_0223:
                            builder.Append(" ");
                            builder.Append(str);
                        Label_023A:
                            num6 = -1;
                        Label_023D:
                            if (num6 == -1)
                            {
                                num6 = index;
                            }
                            num7 = index;
                            goto Label_0253;
                        Label_024A:
                            invalid = DataParseStatus.Invalid;
                            generic = WebParseErrorCode.CrLfError;
                            goto Label_042C;
                        Label_0253:
                            index++;
                        }
                        if (index == num2)
                        {
                            invalid = dataTooBig;
                            goto Label_042C;
                        }
                        string str2 = (num6 == -1) ? "" : HeaderEncoding.GetString(numRef + num6, (num7 - num6) + 1);
                        if (builder != null)
                        {
                            if (str2.Length != 0)
                            {
                                builder.Append(" ");
                                builder.Append(str2);
                            }
                            str2 = builder.ToString();
                        }
                        string name = null;
                        int byteCount = (num4 - num3) + 1;
                        if (this.m_CommonHeaders == null)
                        {
                            goto Label_03F8;
                        }
                        int num9 = s_CommonHeaderHints[numRef[num3] & 0x1f];
                        if (num9 < 0)
                        {
                            goto Label_03F8;
                        }
                    Label_0310:
                        str4 = s_CommonHeaderNames[num9++];
                        if ((str4.Length >= byteCount) && (CaseInsensitiveAscii.AsciiToLower[numRef[num3]] == CaseInsensitiveAscii.AsciiToLower[str4[0]]))
                        {
                            if (str4.Length > byteCount)
                            {
                                goto Label_0310;
                            }
                            byte* numPtr = (numRef + num3) + 1;
                            int num10 = 1;
                            while (num10 < str4.Length)
                            {
                                numPtr++;
                                if ((numPtr[0] != str4[num10]) && (CaseInsensitiveAscii.AsciiToLower[*(numPtr - 1)] != CaseInsensitiveAscii.AsciiToLower[str4[num10]]))
                                {
                                    break;
                                }
                                num10++;
                            }
                            if (num10 != str4.Length)
                            {
                                goto Label_0310;
                            }
                            this.m_NumCommonHeaders++;
                            num9--;
                            if (this.m_CommonHeaders[num9] == null)
                            {
                                this.m_CommonHeaders[num9] = str2;
                            }
                            else
                            {
                                this.NormalizeCommonHeaders();
                                this.AddInternalNotCommon(str4, str2);
                            }
                            name = str4;
                        }
                    Label_03F8:
                        if (name == null)
                        {
                            name = HeaderEncoding.GetString(numRef + num3, byteCount);
                            this.AddInternalNotCommon(name, str2);
                        }
                        totalResponseHeadersLength += index - unparsed;
                        unparsed = index;
                        goto Label_0054;
                    }
                }
                finally
                {
                    numRef = null;
                }
            }
        Label_042C:
            if (invalid == DataParseStatus.Invalid)
            {
                parseError.Section = WebParseErrorSection.ResponseHeader;
                parseError.Code = generic;
            }
            return invalid;
        }

        public void Remove(HttpRequestHeader header)
        {
            if (!this.AllowHttpRequestHeader)
            {
                throw new InvalidOperationException(SR.GetString("net_headers_req"));
            }
            this.Remove(UnsafeNclNativeMethods.HttpApi.HTTP_REQUEST_HEADER_ID.ToString((int) header));
        }

        public void Remove(HttpResponseHeader header)
        {
            if (!this.AllowHttpResponseHeader)
            {
                throw new InvalidOperationException(SR.GetString("net_headers_rsp"));
            }
            this.Remove(UnsafeNclNativeMethods.HttpApi.HTTP_RESPONSE_HEADER_ID.ToString((int) header));
        }

        public override void Remove(string name)
        {
            if (ValidationHelper.IsBlankString(name))
            {
                throw new ArgumentNullException("name");
            }
            this.ThrowOnRestrictedHeader(name);
            name = CheckBadChars(name, false);
            this.NormalizeCommonHeaders();
            if (this.m_InnerCollection != null)
            {
                base.InvalidateCachedArrays();
                this.m_InnerCollection.Remove(name);
            }
        }

        internal void RemoveInternal(string name)
        {
            this.NormalizeCommonHeaders();
            if (this.m_InnerCollection != null)
            {
                base.InvalidateCachedArrays();
                this.m_InnerCollection.Remove(name);
            }
        }

        public void Set(HttpRequestHeader header, string value)
        {
            if (!this.AllowHttpRequestHeader)
            {
                throw new InvalidOperationException(SR.GetString("net_headers_req"));
            }
            this.Set(UnsafeNclNativeMethods.HttpApi.HTTP_REQUEST_HEADER_ID.ToString((int) header), value);
        }

        public void Set(HttpResponseHeader header, string value)
        {
            if (!this.AllowHttpResponseHeader)
            {
                throw new InvalidOperationException(SR.GetString("net_headers_rsp"));
            }
            if (((this.m_Type == WebHeaderCollectionType.HttpListenerResponse) && (value != null)) && (value.Length > 0xffff))
            {
                throw new ArgumentOutOfRangeException("value", value, SR.GetString("net_headers_toolong", new object[] { (ushort) 0xffff }));
            }
            this.Set(UnsafeNclNativeMethods.HttpApi.HTTP_RESPONSE_HEADER_ID.ToString((int) header), value);
        }

        public override void Set(string name, string value)
        {
            if (ValidationHelper.IsBlankString(name))
            {
                throw new ArgumentNullException("name");
            }
            name = CheckBadChars(name, false);
            this.ThrowOnRestrictedHeader(name);
            value = CheckBadChars(value, true);
            if (((this.m_Type == WebHeaderCollectionType.HttpListenerResponse) && (value != null)) && (value.Length > 0xffff))
            {
                throw new ArgumentOutOfRangeException("value", value, SR.GetString("net_headers_toolong", new object[] { (ushort) 0xffff }));
            }
            this.NormalizeCommonHeaders();
            base.InvalidateCachedArrays();
            this.InnerCollection.Set(name, value);
        }

        internal void SetAddVerified(string name, string value)
        {
            if (HInfo[name].AllowMultiValues)
            {
                this.NormalizeCommonHeaders();
                base.InvalidateCachedArrays();
                this.InnerCollection.Add(name, value);
            }
            else
            {
                this.NormalizeCommonHeaders();
                base.InvalidateCachedArrays();
                this.InnerCollection.Set(name, value);
            }
        }

        internal void SetInternal(HttpResponseHeader header, string value)
        {
            if (!this.AllowHttpResponseHeader)
            {
                throw new InvalidOperationException(SR.GetString("net_headers_rsp"));
            }
            if (((this.m_Type == WebHeaderCollectionType.HttpListenerResponse) && (value != null)) && (value.Length > 0xffff))
            {
                throw new ArgumentOutOfRangeException("value", value, SR.GetString("net_headers_toolong", new object[] { (ushort) 0xffff }));
            }
            this.SetInternal(UnsafeNclNativeMethods.HttpApi.HTTP_RESPONSE_HEADER_ID.ToString((int) header), value);
        }

        internal void SetInternal(string name, string value)
        {
            if (ValidationHelper.IsBlankString(name))
            {
                throw new ArgumentNullException("name");
            }
            name = CheckBadChars(name, false);
            value = CheckBadChars(value, true);
            if (((this.m_Type == WebHeaderCollectionType.HttpListenerResponse) && (value != null)) && (value.Length > 0xffff))
            {
                throw new ArgumentOutOfRangeException("value", value, SR.GetString("net_headers_toolong", new object[] { (ushort) 0xffff }));
            }
            this.NormalizeCommonHeaders();
            base.InvalidateCachedArrays();
            this.InnerCollection.Set(name, value);
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.SerializationFormatter, SerializationFormatter=true)]
        void ISerializable.GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
            this.GetObjectData(serializationInfo, streamingContext);
        }

        internal void ThrowOnRestrictedHeader(string headerName)
        {
            if (this.m_Type == WebHeaderCollectionType.HttpWebRequest)
            {
                if (HInfo[headerName].IsRequestRestricted)
                {
                    throw new ArgumentException(!object.Equals(headerName, "Host") ? SR.GetString("net_headerrestrict") : SR.GetString("net_headerrestrict_resp", new object[] { "Host" }), "name");
                }
            }
            else if ((this.m_Type == WebHeaderCollectionType.HttpListenerResponse) && HInfo[headerName].IsResponseRestricted)
            {
                throw new ArgumentException(SR.GetString("net_headerrestrict_resp", new object[] { headerName }), "name");
            }
        }

        public byte[] ToByteArray()
        {
            return HeaderEncoding.GetBytes(this.ToString());
        }

        public override string ToString()
        {
            return GetAsString(this, false, false);
        }

        internal string ToString(bool forTrace)
        {
            return GetAsString(this, false, true);
        }

        public override string[] AllKeys
        {
            get
            {
                this.NormalizeCommonHeaders();
                return this.InnerCollection.AllKeys;
            }
        }

        private bool AllowHttpRequestHeader
        {
            get
            {
                if (this.m_Type == WebHeaderCollectionType.Unknown)
                {
                    this.m_Type = WebHeaderCollectionType.WebRequest;
                }
                if ((this.m_Type != WebHeaderCollectionType.WebRequest) && (this.m_Type != WebHeaderCollectionType.HttpWebRequest))
                {
                    return (this.m_Type == WebHeaderCollectionType.HttpListenerRequest);
                }
                return true;
            }
        }

        internal bool AllowHttpResponseHeader
        {
            get
            {
                if (this.m_Type == WebHeaderCollectionType.Unknown)
                {
                    this.m_Type = WebHeaderCollectionType.WebResponse;
                }
                if ((this.m_Type != WebHeaderCollectionType.WebResponse) && (this.m_Type != WebHeaderCollectionType.HttpWebResponse))
                {
                    return (this.m_Type == WebHeaderCollectionType.HttpListenerResponse);
                }
                return true;
            }
        }

        internal string CacheControl
        {
            get
            {
                if (this.m_CommonHeaders == null)
                {
                    return this.Get(s_CommonHeaderNames[2]);
                }
                return this.m_CommonHeaders[2];
            }
        }

        internal string ContentLength
        {
            get
            {
                if (this.m_CommonHeaders == null)
                {
                    return this.Get(s_CommonHeaderNames[1]);
                }
                return this.m_CommonHeaders[1];
            }
        }

        internal string ContentType
        {
            get
            {
                if (this.m_CommonHeaders == null)
                {
                    return this.Get(s_CommonHeaderNames[3]);
                }
                return this.m_CommonHeaders[3];
            }
        }

        public override int Count
        {
            get
            {
                return (((this.m_InnerCollection == null) ? 0 : this.m_InnerCollection.Count) + this.m_NumCommonHeaders);
            }
        }

        internal string Date
        {
            get
            {
                if (this.m_CommonHeaders == null)
                {
                    return this.Get(s_CommonHeaderNames[4]);
                }
                return this.m_CommonHeaders[4];
            }
        }

        internal string ETag
        {
            get
            {
                if (this.m_CommonHeaders == null)
                {
                    return this.Get(s_CommonHeaderNames[6]);
                }
                return this.m_CommonHeaders[6];
            }
        }

        internal string Expires
        {
            get
            {
                if (this.m_CommonHeaders == null)
                {
                    return this.Get(s_CommonHeaderNames[5]);
                }
                return this.m_CommonHeaders[5];
            }
        }

        private NameValueCollection InnerCollection
        {
            get
            {
                if (this.m_InnerCollection == null)
                {
                    this.m_InnerCollection = new NameValueCollection(0x10, CaseInsensitiveAscii.StaticInstance);
                }
                return this.m_InnerCollection;
            }
        }

        public string this[HttpRequestHeader header]
        {
            get
            {
                if (!this.AllowHttpRequestHeader)
                {
                    throw new InvalidOperationException(SR.GetString("net_headers_req"));
                }
                return base[UnsafeNclNativeMethods.HttpApi.HTTP_REQUEST_HEADER_ID.ToString((int) header)];
            }
            set
            {
                if (!this.AllowHttpRequestHeader)
                {
                    throw new InvalidOperationException(SR.GetString("net_headers_req"));
                }
                base[UnsafeNclNativeMethods.HttpApi.HTTP_REQUEST_HEADER_ID.ToString((int) header)] = value;
            }
        }

        public string this[HttpResponseHeader header]
        {
            get
            {
                if (!this.AllowHttpResponseHeader)
                {
                    throw new InvalidOperationException(SR.GetString("net_headers_rsp"));
                }
                if (this.m_CommonHeaders != null)
                {
                    switch (header)
                    {
                        case HttpResponseHeader.ProxyAuthenticate:
                            return this.m_CommonHeaders[9];

                        case HttpResponseHeader.WwwAuthenticate:
                            return this.m_CommonHeaders[15];
                    }
                }
                return base[UnsafeNclNativeMethods.HttpApi.HTTP_RESPONSE_HEADER_ID.ToString((int) header)];
            }
            set
            {
                if (!this.AllowHttpResponseHeader)
                {
                    throw new InvalidOperationException(SR.GetString("net_headers_rsp"));
                }
                if (((this.m_Type == WebHeaderCollectionType.HttpListenerResponse) && (value != null)) && (value.Length > 0xffff))
                {
                    throw new ArgumentOutOfRangeException("value", value, SR.GetString("net_headers_toolong", new object[] { (ushort) 0xffff }));
                }
                base[UnsafeNclNativeMethods.HttpApi.HTTP_RESPONSE_HEADER_ID.ToString((int) header)] = value;
            }
        }

        public override NameObjectCollectionBase.KeysCollection Keys
        {
            get
            {
                this.NormalizeCommonHeaders();
                return this.InnerCollection.Keys;
            }
        }

        internal string LastModified
        {
            get
            {
                if (this.m_CommonHeaders == null)
                {
                    return this.Get(s_CommonHeaderNames[7]);
                }
                return this.m_CommonHeaders[7];
            }
        }

        internal string Location
        {
            get
            {
                if (this.m_CommonHeaders == null)
                {
                    return this.Get(s_CommonHeaderNames[8]);
                }
                return this.m_CommonHeaders[8];
            }
        }

        internal string ProxyAuthenticate
        {
            get
            {
                if (this.m_CommonHeaders == null)
                {
                    return this.Get(s_CommonHeaderNames[9]);
                }
                return this.m_CommonHeaders[9];
            }
        }

        internal string Server
        {
            get
            {
                if (this.m_CommonHeaders == null)
                {
                    return this.Get(s_CommonHeaderNames[13]);
                }
                return this.m_CommonHeaders[13];
            }
        }

        internal string SetCookie
        {
            get
            {
                if (this.m_CommonHeaders == null)
                {
                    return this.Get(s_CommonHeaderNames[12]);
                }
                return this.m_CommonHeaders[12];
            }
        }

        internal string SetCookie2
        {
            get
            {
                if (this.m_CommonHeaders == null)
                {
                    return this.Get(s_CommonHeaderNames[11]);
                }
                return this.m_CommonHeaders[11];
            }
        }

        internal string Via
        {
            get
            {
                if (this.m_CommonHeaders == null)
                {
                    return this.Get(s_CommonHeaderNames[14]);
                }
                return this.m_CommonHeaders[14];
            }
        }

        internal static class HeaderEncoding
        {
            internal static int GetByteCount(string myString)
            {
                return myString.Length;
            }

            internal static byte[] GetBytes(string myString)
            {
                byte[] bytes = new byte[myString.Length];
                if (myString.Length != 0)
                {
                    GetBytes(myString, 0, myString.Length, bytes, 0);
                }
                return bytes;
            }

            internal static unsafe void GetBytes(string myString, int charIndex, int charCount, byte[] bytes, int byteIndex)
            {
                if (myString.Length != 0)
                {
                    fixed (byte* numRef = bytes)
                    {
                        byte* numPtr = numRef + byteIndex;
                        int num = charIndex + charCount;
                        while (charIndex < num)
                        {
                            numPtr++;
                            numPtr[0] = (byte) myString[charIndex++];
                        }
                    }
                }
            }

            internal static unsafe string GetString(byte* pBytes, int byteCount)
            {
                if (byteCount < 1)
                {
                    return "";
                }
                string str = new string('\0', byteCount);
                fixed (char* str2 = ((char*) str))
                {
                    char* chPtr2 = str2;
                    while (byteCount >= 8)
                    {
                        chPtr2[0] = (char) pBytes[0];
                        chPtr2[1] = (char) pBytes[1];
                        chPtr2[2] = (char) pBytes[2];
                        chPtr2[3] = (char) pBytes[3];
                        chPtr2[4] = (char) pBytes[4];
                        chPtr2[5] = (char) pBytes[5];
                        chPtr2[6] = (char) pBytes[6];
                        chPtr2[7] = (char) pBytes[7];
                        chPtr2 += 8;
                        pBytes += 8;
                        byteCount -= 8;
                    }
                    for (int i = 0; i < byteCount; i++)
                    {
                        chPtr2[i] = (char) pBytes[i];
                    }
                }
                return str;
            }

            internal static unsafe string GetString(byte[] bytes, int byteIndex, int byteCount)
            {
                fixed (byte* numRef = bytes)
                {
                    return GetString(numRef + byteIndex, byteCount);
                }
            }
        }

        private enum RfcChar : byte
        {
            Colon = 6,
            CR = 3,
            Ctl = 2,
            Delim = 7,
            High = 0,
            LF = 4,
            Reg = 1,
            WS = 5
        }
    }
}

