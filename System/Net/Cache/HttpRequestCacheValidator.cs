namespace System.Net.Cache
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;

    internal class HttpRequestCacheValidator : RequestCacheValidator
    {
        private const long __rev = 0x7600650072002dL;
        private const long _ache = 0x65006800630061L;
        private const int _ag = 0x670061;
        private const long _alid = 0x640069006c0061L;
        private const int _at = 0x740061;
        private const long _date = 0x65007400610064L;
        private const int _ic = 0x630069;
        private const long _max_ = 0x2d00780061006dL;
        private const long _must = 0x7400730075006dL;
        private const long _no_c = 0x63002d006f006eL;
        private const long _no_s = 0x73002d006f006eL;
        private const long _priv = 0x76006900720070L;
        private const long _prox = 0x78006f00720070L;
        private const long _publ = 0x6c006200750070L;
        private const long _s_ma = 0x61006d002d0073L;
        private const long _tore = 0x650072006f0074L;
        private const long _vali = 0x69006c00610076L;
        private const long _xage = 0x65006700610078L;
        private const long _y_re = 0x650072002d0079L;
        private const long LO = 0x20002000200020L;
        private const int LOI = 0x200020;
        private Vars m_CacheVars;
        private bool m_DontUpdateHeaders;
        private WebHeaderCollection m_Headers;
        private bool m_HeuristicExpiration;
        private HttpRequestCachePolicy m_HttpPolicy;
        private Version m_HttpVersion;
        private RequestVars m_RequestVars;
        private Vars m_ResponseVars;
        private HttpStatusCode m_StatusCode;
        private string m_StatusDescription;
        private NameValueCollection m_SystemMeta;
        internal static readonly ParseCallback ParseValuesCallback = new ParseCallback(HttpRequestCacheValidator.ParseValuesCallbackMethod);
        private static readonly ParseCallback ParseWarningsCallback = new ParseCallback(HttpRequestCacheValidator.ParseWarningsCallbackMethod);
        internal const string Warning_110 = "110 Response is stale";
        internal const string Warning_111 = "111 Revalidation failed";
        internal const string Warning_112 = "112 Disconnected operation";
        internal const string Warning_113 = "113 Heuristic expiration";

        internal HttpRequestCacheValidator(bool strictCacheErrors, TimeSpan unspecifiedMaxAge) : base(strictCacheErrors, unspecifiedMaxAge)
        {
        }

        private void CreateCacheHeaders(bool ignoreFirstString)
        {
            if (this.CacheHeaders == null)
            {
                this.CacheHeaders = new WebHeaderCollection();
            }
            if ((base.CacheEntry.EntryMetadata == null) || (base.CacheEntry.EntryMetadata.Count == 0))
            {
                if (Logging.On)
                {
                    Logging.PrintWarning(Logging.RequestCache, SR.GetString("net_log_cache_no_http_response_header"));
                }
            }
            else
            {
                string str = this.ParseNameValues(this.CacheHeaders, base.CacheEntry.EntryMetadata, ignoreFirstString ? 1 : 0);
                if (str != null)
                {
                    if (Logging.On)
                    {
                        Logging.PrintWarning(Logging.RequestCache, SR.GetString("net_log_cache_http_header_parse_error", new object[] { str }));
                    }
                    this.CacheHeaders.Clear();
                }
            }
        }

        private void CreateSystemMeta()
        {
            if (this.SystemMeta == null)
            {
                this.SystemMeta = new NameValueCollection(((base.CacheEntry.EntryMetadata == null) || (base.CacheEntry.EntryMetadata.Count == 0)) ? 2 : base.CacheEntry.EntryMetadata.Count, CaseInsensitiveAscii.StaticInstance);
            }
            if ((base.CacheEntry.EntryMetadata != null) && (base.CacheEntry.EntryMetadata.Count != 0))
            {
                string str = this.ParseNameValues(this.SystemMeta, base.CacheEntry.SystemMetadata, 0);
                if ((str != null) && Logging.On)
                {
                    Logging.PrintWarning(Logging.RequestCache, SR.GetString("net_log_cache_metadata_name_value_parse_error", new object[] { str }));
                }
            }
        }

        internal override RequestCacheValidator CreateValidator()
        {
            return new HttpRequestCacheValidator(base.StrictCacheErrors, base.UnspecifiedMaxAge);
        }

        private unsafe void FetchCacheControl(string s, bool forCache)
        {
            System.Net.Cache.ResponseCacheControl control = new System.Net.Cache.ResponseCacheControl();
            if (forCache)
            {
                this.CacheCacheControl = control;
            }
            else
            {
                this.ResponseCacheControl = control;
            }
            if ((s != null) && (s.Length != 0))
            {
                fixed (char* str = ((char*) s))
                {
                    char* chPtr = str;
                    int length = s.Length;
                    for (int i = 0; i < (length - 4); i++)
                    {
                        long* numPtr;
                        ArrayList list;
                        if ((chPtr[i] < ' ') || (chPtr[i] >= '\x007f'))
                        {
                            if (Logging.On)
                            {
                                Logging.PrintError(Logging.RequestCache, SR.GetString("net_log_cache_cache_control_error", new object[] { s }));
                            }
                            return;
                        }
                        if ((chPtr[i] != ' ') && (chPtr[i] != ','))
                        {
                            if (IntPtr.Size != 4)
                            {
                                goto Label_0717;
                            }
                            numPtr = (long*) (chPtr + i);
                            long num7 = numPtr[0] | 0x20002000200020L;
                            if (num7 <= 0x6c006200750070L)
                            {
                                switch (num7)
                                {
                                    case 0x63002d006f006eL:
                                        if ((i + 8) > length)
                                        {
                                            return;
                                        }
                                        if ((numPtr[1] | 0x200020L) == 0x65006800630061L)
                                        {
                                            control.NoCache = true;
                                            i += 7;
                                            while ((i < length) && (chPtr[i] == ' '))
                                            {
                                                i++;
                                            }
                                            if ((i < length) && (chPtr[i] == '='))
                                            {
                                                goto Label_03E5;
                                            }
                                            i--;
                                        }
                                        break;

                                    case 0x6c006200750070L:
                                        if ((i + 6) > length)
                                        {
                                            return;
                                        }
                                        if ((*(((int*) (numPtr + 1))) | 0x200020) == 0x630069)
                                        {
                                            control.Public = true;
                                            i += 5;
                                        }
                                        break;

                                    case 0x2d00780061006dL:
                                        goto Label_058F;

                                    case 0x61006d002d0073L:
                                        goto Label_065A;
                                }
                            }
                            else if (num7 <= 0x7400730075006dL)
                            {
                                switch (num7)
                                {
                                    case 0x73002d006f006eL:
                                        goto Label_04D6;

                                    case 0x7400730075006dL:
                                        goto Label_050C;
                                }
                            }
                            else
                            {
                                if (num7 == 0x76006900720070L)
                                {
                                    if ((i + 7) > length)
                                    {
                                        return;
                                    }
                                    if (((*(((int*) (numPtr + 1))) | 0x200020) != 0x740061) || ((chPtr[i + 6] | ' ') != 0x65))
                                    {
                                        continue;
                                    }
                                    control.Private = true;
                                    i += 6;
                                    while ((i < length) && (chPtr[i] == ' '))
                                    {
                                        i++;
                                    }
                                    if ((i >= length) || (chPtr[i] != '='))
                                    {
                                        i--;
                                        continue;
                                    }
                                    while ((i < length) && (chPtr[++i] == ' '))
                                    {
                                    }
                                    if ((i >= length) || (chPtr[i] != '"'))
                                    {
                                        i--;
                                        continue;
                                    }
                                    list = new ArrayList();
                                    i++;
                                    goto Label_0348;
                                }
                                if ((((num7 == 0x78006f00720070L) && ((i + 0x10) <= length)) && (((numPtr[1] | 0x20002000200020L) == 0x650072002d0079L) && ((numPtr[2] | 0x20002000200020L) == 0x69006c00610076L))) && ((numPtr[3] | 0x20002000200020L) == 0x65007400610064L))
                                {
                                    control.ProxyRevalidate = true;
                                    i += 15;
                                }
                            }
                        }
                        continue;
                    Label_02CE:
                        i++;
                    Label_02D2:
                        if ((i < length) && (chPtr[i] == ' '))
                        {
                            goto Label_02CE;
                        }
                        int startIndex = i;
                        while (((i < length) && (chPtr[i] != ' ')) && ((chPtr[i] != ',') && (chPtr[i] != '"')))
                        {
                            i++;
                        }
                        if (startIndex != i)
                        {
                            list.Add(s.Substring(startIndex, i - startIndex));
                        }
                        while (((i < length) && (chPtr[i] != ',')) && (chPtr[i] != '"'))
                        {
                            i++;
                        }
                    Label_0348:
                        if ((i < length) && (chPtr[i] != '"'))
                        {
                            goto Label_02D2;
                        }
                        if (list.Count != 0)
                        {
                            control.PrivateHeaders = (string[]) list.ToArray(typeof(string));
                        }
                        continue;
                    Label_03E5:
                        while ((i < length) && (chPtr[++i] == ' '))
                        {
                        }
                        if ((i >= length) || (chPtr[i] != '"'))
                        {
                            i--;
                            continue;
                        }
                        ArrayList list2 = new ArrayList();
                        i++;
                        goto Label_0497;
                    Label_041D:
                        i++;
                    Label_0421:
                        if ((i < length) && (chPtr[i] == ' '))
                        {
                            goto Label_041D;
                        }
                        int num4 = i;
                        while (((i < length) && (chPtr[i] != ' ')) && ((chPtr[i] != ',') && (chPtr[i] != '"')))
                        {
                            i++;
                        }
                        if (num4 != i)
                        {
                            list2.Add(s.Substring(num4, i - num4));
                        }
                        while (((i < length) && (chPtr[i] != ',')) && (chPtr[i] != '"'))
                        {
                            i++;
                        }
                    Label_0497:
                        if ((i < length) && (chPtr[i] != '"'))
                        {
                            goto Label_0421;
                        }
                        if (list2.Count != 0)
                        {
                            control.NoCacheHeaders = (string[]) list2.ToArray(typeof(string));
                        }
                        continue;
                    Label_04D6:
                        if ((i + 8) > length)
                        {
                            return;
                        }
                        if ((numPtr[1] | 0x200020L) == 0x650072006f0074L)
                        {
                            control.NoStore = true;
                            i += 7;
                        }
                        continue;
                    Label_050C:
                        if (((((i + 15) <= length) && ((numPtr[1] | 0x20002000200020L) == 0x7600650072002dL)) && (((numPtr[2] | 0x20002000200020L) == 0x640069006c0061L) && ((*(((int*) (numPtr + 3))) | 0x200020) == 0x740061))) && ((chPtr[i + 14] | ' ') == 0x65))
                        {
                            control.MustRevalidate = true;
                            i += 14;
                        }
                        continue;
                    Label_058F:
                        if ((i + 7) <= length)
                        {
                            if (((*(((int*) (numPtr + 1))) | 0x200020) != 0x670061) || ((chPtr[i + 6] | ' ') != 0x65))
                            {
                                continue;
                            }
                            i += 7;
                            while ((i < length) && (chPtr[i] == ' '))
                            {
                                i++;
                            }
                            if ((i != length) && (chPtr[i++] == '='))
                            {
                                goto Label_05F8;
                            }
                        }
                        return;
                    Label_05F4:
                        i++;
                    Label_05F8:
                        if ((i < length) && (chPtr[i] == ' '))
                        {
                            goto Label_05F4;
                        }
                        if (i == length)
                        {
                            return;
                        }
                        control.MaxAge = 0;
                        while (((i < length) && (chPtr[i] >= '0')) && (chPtr[i] <= '9'))
                        {
                            control.MaxAge = (control.MaxAge * 10) + (chPtr[i++] - '0');
                        }
                        i--;
                        continue;
                    Label_065A:
                        if ((i + 8) <= length)
                        {
                            if ((numPtr[1] | 0x200020L) != 0x65006700610078L)
                            {
                                continue;
                            }
                            i += 8;
                            while ((i < length) && (chPtr[i] == ' '))
                            {
                                i++;
                            }
                            if ((i != length) && (chPtr[i++] == '='))
                            {
                                goto Label_06B5;
                            }
                        }
                        return;
                    Label_06B1:
                        i++;
                    Label_06B5:
                        if ((i < length) && (chPtr[i] == ' '))
                        {
                            goto Label_06B1;
                        }
                        if (i == length)
                        {
                            return;
                        }
                        control.SMaxAge = 0;
                        while (((i < length) && (chPtr[i] >= '0')) && (chPtr[i] <= '9'))
                        {
                            control.SMaxAge = (control.SMaxAge * 10) + (chPtr[i++] - '0');
                        }
                        i--;
                        continue;
                    Label_0717:
                        if (Rfc2616.Common.UnsafeAsciiLettersNoCaseEqual(chPtr, i, length, "proxy-revalidate"))
                        {
                            control.ProxyRevalidate = true;
                            i += 15;
                            continue;
                        }
                        if (Rfc2616.Common.UnsafeAsciiLettersNoCaseEqual(chPtr, i, length, "public"))
                        {
                            control.Public = true;
                            i += 5;
                            continue;
                        }
                        if (!Rfc2616.Common.UnsafeAsciiLettersNoCaseEqual(chPtr, i, length, "private"))
                        {
                            goto Label_0891;
                        }
                        control.Private = true;
                        i += 6;
                        while ((i < length) && (chPtr[i] == ' '))
                        {
                            i++;
                        }
                        if ((i >= length) || (chPtr[i] != '='))
                        {
                            i--;
                            return;
                        }
                        while ((i < length) && (chPtr[++i] == ' '))
                        {
                        }
                        if ((i >= length) || (chPtr[i] != '"'))
                        {
                            i--;
                            return;
                        }
                        ArrayList list3 = new ArrayList();
                        i++;
                        goto Label_0852;
                    Label_07D8:
                        i++;
                    Label_07DC:
                        if ((i < length) && (chPtr[i] == ' '))
                        {
                            goto Label_07D8;
                        }
                        int num5 = i;
                        while (((i < length) && (chPtr[i] != ' ')) && ((chPtr[i] != ',') && (chPtr[i] != '"')))
                        {
                            i++;
                        }
                        if (num5 != i)
                        {
                            list3.Add(s.Substring(num5, i - num5));
                        }
                        while (((i < length) && (chPtr[i] != ',')) && (chPtr[i] != '"'))
                        {
                            i++;
                        }
                    Label_0852:
                        if ((i < length) && (chPtr[i] != '"'))
                        {
                            goto Label_07DC;
                        }
                        if (list3.Count != 0)
                        {
                            control.PrivateHeaders = (string[]) list3.ToArray(typeof(string));
                        }
                        continue;
                    Label_0891:
                        if (!Rfc2616.Common.UnsafeAsciiLettersNoCaseEqual(chPtr, i, length, "no-cache"))
                        {
                            goto Label_09CC;
                        }
                        control.NoCache = true;
                        i += 7;
                        while ((i < length) && (chPtr[i] == ' '))
                        {
                            i++;
                        }
                        if ((i >= length) || (chPtr[i] != '='))
                        {
                            i--;
                            return;
                        }
                        while ((i < length) && (chPtr[++i] == ' '))
                        {
                        }
                        if ((i >= length) || (chPtr[i] != '"'))
                        {
                            i--;
                            return;
                        }
                        ArrayList list4 = new ArrayList();
                        i++;
                        goto Label_098D;
                    Label_0913:
                        i++;
                    Label_0917:
                        if ((i < length) && (chPtr[i] == ' '))
                        {
                            goto Label_0913;
                        }
                        int num6 = i;
                        while (((i < length) && (chPtr[i] != ' ')) && ((chPtr[i] != ',') && (chPtr[i] != '"')))
                        {
                            i++;
                        }
                        if (num6 != i)
                        {
                            list4.Add(s.Substring(num6, i - num6));
                        }
                        while (((i < length) && (chPtr[i] != ',')) && (chPtr[i] != '"'))
                        {
                            i++;
                        }
                    Label_098D:
                        if ((i < length) && (chPtr[i] != '"'))
                        {
                            goto Label_0917;
                        }
                        if (list4.Count != 0)
                        {
                            control.NoCacheHeaders = (string[]) list4.ToArray(typeof(string));
                        }
                        continue;
                    Label_09CC:
                        if (Rfc2616.Common.UnsafeAsciiLettersNoCaseEqual(chPtr, i, length, "no-store"))
                        {
                            control.NoStore = true;
                            i += 7;
                            continue;
                        }
                        if (Rfc2616.Common.UnsafeAsciiLettersNoCaseEqual(chPtr, i, length, "must-revalidate"))
                        {
                            control.MustRevalidate = true;
                            i += 14;
                            continue;
                        }
                        if (!Rfc2616.Common.UnsafeAsciiLettersNoCaseEqual(chPtr, i, length, "max-age"))
                        {
                            goto Label_0AB4;
                        }
                        i += 7;
                        while ((i < length) && (chPtr[i] == ' '))
                        {
                            i++;
                        }
                        if ((i != length) && (chPtr[i++] == '='))
                        {
                            goto Label_0A52;
                        }
                        return;
                    Label_0A4E:
                        i++;
                    Label_0A52:
                        if ((i < length) && (chPtr[i] == ' '))
                        {
                            goto Label_0A4E;
                        }
                        if (i == length)
                        {
                            return;
                        }
                        control.MaxAge = 0;
                        while (((i < length) && (chPtr[i] >= '0')) && (chPtr[i] <= '9'))
                        {
                            control.MaxAge = (control.MaxAge * 10) + (chPtr[i++] - '0');
                        }
                        i--;
                        continue;
                    Label_0AB4:
                        if (!Rfc2616.Common.UnsafeAsciiLettersNoCaseEqual(chPtr, i, length, "smax-age"))
                        {
                            continue;
                        }
                        i += 8;
                        while ((i < length) && (chPtr[i] == ' '))
                        {
                            i++;
                        }
                        if ((i != length) && (chPtr[i++] == '='))
                        {
                            goto Label_0AF8;
                        }
                        return;
                    Label_0AF4:
                        i++;
                    Label_0AF8:
                        if ((i < length) && (chPtr[i] == ' '))
                        {
                            goto Label_0AF4;
                        }
                        if (i == length)
                        {
                            return;
                        }
                        control.SMaxAge = 0;
                        while (((i < length) && (chPtr[i] >= '0')) && (chPtr[i] <= '9'))
                        {
                            control.SMaxAge = (control.SMaxAge * 10) + (chPtr[i++] - '0');
                        }
                        i--;
                    }
                }
            }
        }

        private void FetchHeaderValues(bool forCache)
        {
            WebHeaderCollection headers = forCache ? this.CacheHeaders : base.Response.Headers;
            this.FetchCacheControl(headers.CacheControl, forCache);
            string date = headers.Date;
            DateTime minValue = DateTime.MinValue;
            if ((date != null) && HttpDateParse.ParseHttpDate(date, out minValue))
            {
                minValue = minValue.ToUniversalTime();
            }
            if (forCache)
            {
                this.CacheDate = minValue;
            }
            else
            {
                this.ResponseDate = minValue;
            }
            date = headers.Expires;
            minValue = DateTime.MinValue;
            if ((date != null) && HttpDateParse.ParseHttpDate(date, out minValue))
            {
                minValue = minValue.ToUniversalTime();
            }
            if (forCache)
            {
                this.CacheExpires = minValue;
            }
            else
            {
                this.ResponseExpires = minValue;
            }
            date = headers.LastModified;
            minValue = DateTime.MinValue;
            if ((date != null) && HttpDateParse.ParseHttpDate(date, out minValue))
            {
                minValue = minValue.ToUniversalTime();
            }
            if (forCache)
            {
                this.CacheLastModified = minValue;
            }
            else
            {
                this.ResponseLastModified = minValue;
            }
            long total = -1L;
            long start = -1L;
            long end = -1L;
            HttpWebResponse response = base.Response as HttpWebResponse;
            if ((forCache ? this.CacheStatusCode : response.StatusCode) != HttpStatusCode.PartialContent)
            {
                date = headers.ContentLength;
                if ((date != null) && (date.Length != 0))
                {
                    int num4 = 0;
                    char ch = date[0];
                    while ((num4 < date.Length) && (ch == ' '))
                    {
                        ch = date[++num4];
                    }
                    if (((num4 != date.Length) && (ch >= '0')) && (ch <= '9'))
                    {
                        total = ch - '0';
                        while (((++num4 < date.Length) && ((ch = date[num4]) >= '0')) && (ch <= '9'))
                        {
                            total = (total * 10L) + (ch - '0');
                        }
                    }
                }
            }
            else
            {
                date = headers["Content-Range"];
                if ((date == null) || !Rfc2616.Common.GetBytesRange(date, ref start, ref end, ref total, false))
                {
                    if (Logging.On)
                    {
                        Logging.PrintError(Logging.RequestCache, SR.GetString("net_log_cache_content_range_error", new object[] { (date == null) ? "<null>" : date }));
                    }
                    start = end = total = -1L;
                }
                else if (forCache && (total == base.CacheEntry.StreamSize))
                {
                    start = -1L;
                    end = -1L;
                    this.CacheStatusCode = HttpStatusCode.OK;
                    this.CacheStatusDescription = "OK";
                }
            }
            if (forCache)
            {
                this.CacheEntityLength = total;
                this.ResponseRangeStart = start;
                this.ResponseRangeEnd = end;
            }
            else
            {
                this.ResponseEntityLength = total;
                this.ResponseRangeStart = start;
                this.ResponseRangeEnd = end;
            }
            TimeSpan span = TimeSpan.MinValue;
            date = headers["Age"];
            if (date != null)
            {
                int num5 = 0;
                int num6 = 0;
                do
                {
                    if (num5 >= date.Length)
                    {
                        break;
                    }
                }
                while (date[num5++] == ' ');
                while (((num5 < date.Length) && (date[num5] >= '0')) && (date[num5] <= '9'))
                {
                    num6 = (num6 * 10) + (date[num5++] - '0');
                }
                span = TimeSpan.FromSeconds((double) num6);
            }
            if (forCache)
            {
                this.CacheAge = span;
            }
            else
            {
                this.ResponseAge = span;
            }
        }

        private void FinallyUpdateCacheEntry()
        {
            base.CacheEntry.EntryMetadata = null;
            base.CacheEntry.SystemMetadata = null;
            if (this.CacheHeaders != null)
            {
                base.CacheEntry.EntryMetadata = new StringCollection();
                base.CacheEntry.SystemMetadata = new StringCollection();
                if (this.CacheHttpVersion == null)
                {
                    if (Logging.On)
                    {
                        Logging.PrintWarning(Logging.RequestCache, SR.GetString("net_log_cache_invalid_http_version"));
                    }
                    this.CacheHttpVersion = new Version(1, 0);
                }
                StringBuilder builder = new StringBuilder(this.CacheStatusDescription.Length + 20);
                builder.Append("HTTP/");
                builder.Append(this.CacheHttpVersion.ToString(2));
                builder.Append(' ');
                builder.Append(((int) this.CacheStatusCode).ToString(NumberFormatInfo.InvariantInfo));
                builder.Append(' ');
                builder.Append(this.CacheStatusDescription);
                base.CacheEntry.EntryMetadata.Add(builder.ToString());
                UpdateStringCollection(base.CacheEntry.EntryMetadata, this.CacheHeaders, false);
                if (this.SystemMeta != null)
                {
                    UpdateStringCollection(base.CacheEntry.SystemMetadata, this.SystemMeta, true);
                }
                if (this.ResponseExpires != DateTime.MinValue)
                {
                    base.CacheEntry.ExpiresUtc = this.ResponseExpires;
                }
                if (this.ResponseLastModified != DateTime.MinValue)
                {
                    base.CacheEntry.LastModifiedUtc = this.ResponseLastModified;
                }
                if (this.Policy.Level == HttpRequestCacheLevel.Default)
                {
                    base.CacheEntry.MaxStale = this.Policy.MaxStale;
                }
                base.CacheEntry.LastSynchronizedUtc = DateTime.UtcNow;
            }
        }

        internal static void ParseHeaderValues(string[] values, ParseCallback calback, IList list)
        {
            if (values != null)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    string s = values[i];
                    int num2 = 0;
                    int start = 0;
                    goto Label_00E3;
                Label_0018:
                    start++;
                Label_001C:
                    if ((start < s.Length) && (s[start] == ' '))
                    {
                        goto Label_0018;
                    }
                    if (start != s.Length)
                    {
                        num2 = start;
                        do
                        {
                            while (((num2 < s.Length) && (s[num2] != ',')) && (s[num2] != '"'))
                            {
                                num2++;
                            }
                            if (num2 == s.Length)
                            {
                                calback(s, start, num2 - 1, list);
                                continue;
                            }
                            if (s[num2] != '"')
                            {
                                goto Label_00B4;
                            }
                            while ((++num2 < s.Length) && (s[num2] != '"'))
                            {
                            }
                        }
                        while (num2 != s.Length);
                        calback(s, start, num2 - 1, list);
                    }
                    continue;
                Label_00B4:
                    calback(s, start, num2 - 1, list);
                    while ((++num2 < s.Length) && (s[num2] == ' '))
                    {
                    }
                    if (num2 >= s.Length)
                    {
                        continue;
                    }
                    start = num2;
                Label_00E3:
                    if (num2 < s.Length)
                    {
                        goto Label_001C;
                    }
                }
            }
        }

        private string ParseNameValues(NameValueCollection cc, StringCollection sc, int start)
        {
            WebHeaderCollection headers = cc as WebHeaderCollection;
            string name = null;
            if (sc != null)
            {
                for (int i = start; i < sc.Count; i++)
                {
                    string str2 = sc[i];
                    if ((str2 == null) || (str2.Length == 0))
                    {
                        return null;
                    }
                    if ((str2[0] == ' ') || (str2[0] == '\t'))
                    {
                        if (name == null)
                        {
                            return str2;
                        }
                        if (headers != null)
                        {
                            headers.AddInternal(name, str2);
                        }
                        else
                        {
                            cc.Add(name, str2);
                        }
                    }
                    int index = str2.IndexOf(':');
                    if (index < 0)
                    {
                        return str2;
                    }
                    name = str2.Substring(0, index);
                    while ((++index < str2.Length) && ((str2[index] == ' ') || (str2[index] == '\t')))
                    {
                    }
                    try
                    {
                        if (headers != null)
                        {
                            headers.AddInternal(name, str2.Substring(index));
                        }
                        else
                        {
                            cc.Add(name, str2.Substring(index));
                        }
                    }
                    catch (Exception exception)
                    {
                        if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                        {
                            throw;
                        }
                        return str2;
                    }
                }
            }
            return null;
        }

        private string ParseStatusLine()
        {
            this.CacheStatusCode = (HttpStatusCode) 0;
            if ((base.CacheEntry.EntryMetadata == null) || (base.CacheEntry.EntryMetadata.Count == 0))
            {
                return null;
            }
            string str = base.CacheEntry.EntryMetadata[0];
            if (str == null)
            {
                return null;
            }
            int startIndex = 0;
            char ch = '\0';
            while ((++startIndex < str.Length) && ((ch = str[startIndex]) != '/'))
            {
            }
            if (startIndex != str.Length)
            {
                int major = -1;
                int minor = -1;
                int num4 = -1;
                while (((++startIndex < str.Length) && ((ch = str[startIndex]) >= '0')) && (ch <= '9'))
                {
                    major = ((major < 0) ? 0 : (major * 10)) + (ch - '0');
                }
                if ((major >= 0) && (ch == '.'))
                {
                    while (((++startIndex < str.Length) && ((ch = str[startIndex]) >= '0')) && (ch <= '9'))
                    {
                        minor = ((minor < 0) ? 0 : (minor * 10)) + (ch - '0');
                    }
                    if ((minor >= 0) && ((ch == ' ') || (ch == '\t')))
                    {
                        while ((++startIndex < str.Length) && (((ch = str[startIndex]) == ' ') || (ch == '\t')))
                        {
                        }
                        if (startIndex < str.Length)
                        {
                            while ((ch >= '0') && (ch <= '9'))
                            {
                                num4 = ((num4 < 0) ? 0 : (num4 * 10)) + (ch - '0');
                                if (++startIndex == str.Length)
                                {
                                    break;
                                }
                                ch = str[startIndex];
                            }
                            if ((num4 >= 0) && (((startIndex > str.Length) || (ch == ' ')) || (ch == '\t')))
                            {
                                while ((startIndex < str.Length) && ((str[startIndex] == ' ') || (str[startIndex] == '\t')))
                                {
                                    startIndex++;
                                }
                                this.CacheStatusDescription = str.Substring(startIndex);
                                this.CacheHttpVersion = new Version(major, minor);
                                this.CacheStatusCode = (HttpStatusCode) num4;
                                return str;
                            }
                            return str;
                        }
                    }
                    return str;
                }
            }
            return str;
        }

        private static void ParseValuesCallbackMethod(string s, int start, int end, IList list)
        {
            while ((end >= start) && (s[end] == ' '))
            {
                end--;
            }
            if (end >= start)
            {
                list.Add(s.Substring(start, (end - start) + 1));
            }
        }

        private static void ParseWarningsCallbackMethod(string s, int start, int end, IList list)
        {
            if ((end >= start) && (s[start] != '1'))
            {
                ParseValuesCallbackMethod(s, start, end, list);
            }
        }

        private void RemoveWarnings_1xx()
        {
            string[] values = this.CacheHeaders.GetValues("Warning");
            if (values != null)
            {
                ArrayList list = new ArrayList();
                ParseHeaderValues(values, ParseWarningsCallback, list);
                this.CacheHeaders.Remove("Warning");
                for (int i = 0; i < list.Count; i++)
                {
                    this.CacheHeaders.Add("Warning", (string) list[i]);
                }
            }
        }

        protected internal override CacheValidationStatus RevalidateCache()
        {
            if ((this.Policy.Level != HttpRequestCacheLevel.Revalidate) && (base.Policy.Level >= RequestCacheLevel.Reload))
            {
                if (Logging.On)
                {
                    Logging.PrintError(Logging.RequestCache, SR.GetString("net_log_cache_validator_invalid_for_policy", new object[] { this.Policy.ToString() }));
                }
                return CacheValidationStatus.DoNotTakeFromCache;
            }
            if (((base.CacheStream == Stream.Null) || (this.CacheStatusCode == ((HttpStatusCode) 0))) || (this.CacheStatusCode == HttpStatusCode.NotModified))
            {
                return CacheValidationStatus.DoNotTakeFromCache;
            }
            CacheValidationStatus doNotTakeFromCache = CacheValidationStatus.DoNotTakeFromCache;
            HttpWebResponse resp = base.Response as HttpWebResponse;
            if (resp == null)
            {
                return CacheValidationStatus.DoNotTakeFromCache;
            }
            if (resp.StatusCode >= HttpStatusCode.InternalServerError)
            {
                if (Rfc2616.Common.ValidateCacheOn5XXResponse(this) == CacheValidationStatus.ReturnCachedResponse)
                {
                    if (base.CacheFreshnessStatus == CacheFreshnessStatus.Stale)
                    {
                        this.CacheHeaders.Add("Warning", "110 Response is stale");
                    }
                    if (this.HeuristicExpiration && (((int) this.CacheAge.TotalSeconds) >= 0x15180))
                    {
                        this.CacheHeaders.Add("Warning", "113 Heuristic expiration");
                    }
                }
            }
            else if (base.ResponseCount > 1)
            {
                doNotTakeFromCache = CacheValidationStatus.DoNotTakeFromCache;
            }
            else
            {
                this.CacheAge = TimeSpan.Zero;
                doNotTakeFromCache = Rfc2616.Common.ValidateCacheAfterResponse(this, resp);
            }
            if (doNotTakeFromCache == CacheValidationStatus.ReturnCachedResponse)
            {
                this.CacheHeaders["Age"] = ((int) this.CacheAge.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo);
            }
            return doNotTakeFromCache;
        }

        protected internal override CacheValidationStatus UpdateCache()
        {
            if (this.Policy.Level == HttpRequestCacheLevel.NoCacheNoStore)
            {
                if (Logging.On)
                {
                    Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_removed_existing_based_on_policy", new object[] { this.Policy.ToString() }));
                }
                return CacheValidationStatus.RemoveFromCache;
            }
            if (this.Policy.Level == HttpRequestCacheLevel.CacheOnly)
            {
                if (Logging.On)
                {
                    Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_not_updated_based_on_policy", new object[] { this.Policy.ToString() }));
                }
                return CacheValidationStatus.DoNotUpdateCache;
            }
            if (this.CacheHeaders == null)
            {
                this.CacheHeaders = new WebHeaderCollection();
            }
            if (this.SystemMeta == null)
            {
                this.SystemMeta = new NameValueCollection(1, CaseInsensitiveAscii.StaticInstance);
            }
            if (this.ResponseCacheControl == null)
            {
                this.FetchHeaderValues(false);
            }
            CacheValidationStatus status = Rfc2616.OnUpdateCache(this);
            switch (status)
            {
                case CacheValidationStatus.UpdateResponseInformation:
                case CacheValidationStatus.CacheResponse:
                    this.FinallyUpdateCacheEntry();
                    break;
            }
            return status;
        }

        private static void UpdateStringCollection(StringCollection result, NameValueCollection cc, bool winInetCompat)
        {
            for (int i = 0; i < cc.Count; i++)
            {
                StringBuilder builder = new StringBuilder(40);
                string key = cc.GetKey(i);
                builder.Append(key).Append(':');
                string[] values = cc.GetValues(i);
                if (values.Length != 0)
                {
                    if (winInetCompat)
                    {
                        builder.Append(values[0]);
                    }
                    else
                    {
                        builder.Append(' ').Append(values[0]);
                    }
                }
                for (int j = 1; j < values.Length; j++)
                {
                    builder.Append(key).Append(", ").Append(values[j]);
                }
                result.Add(builder.ToString());
            }
            result.Add(string.Empty);
        }

        protected internal override CacheValidationStatus ValidateCache()
        {
            if ((this.Policy.Level != HttpRequestCacheLevel.Revalidate) && (base.Policy.Level >= RequestCacheLevel.Reload))
            {
                if (Logging.On)
                {
                    Logging.PrintError(Logging.RequestCache, SR.GetString("net_log_cache_validator_invalid_for_policy", new object[] { this.Policy.ToString() }));
                }
                return CacheValidationStatus.DoNotTakeFromCache;
            }
            if (((base.CacheStream == Stream.Null) || (this.CacheStatusCode == ((HttpStatusCode) 0))) || (this.CacheStatusCode == HttpStatusCode.NotModified))
            {
                if (this.Policy.Level == HttpRequestCacheLevel.CacheOnly)
                {
                    this.FailRequest(WebExceptionStatus.CacheEntryNotFound);
                }
                return CacheValidationStatus.DoNotTakeFromCache;
            }
            if (this.RequestMethod == HttpMethod.Head)
            {
                base.CacheStream.Close();
                base.CacheStream = new SyncMemoryStream(new byte[0]);
            }
            CacheValidationStatus doNotTakeFromCache = CacheValidationStatus.DoNotTakeFromCache;
            this.RemoveWarnings_1xx();
            base.CacheStreamOffset = 0L;
            base.CacheStreamLength = base.CacheEntry.StreamSize;
            doNotTakeFromCache = Rfc2616.OnValidateCache(this);
            if ((doNotTakeFromCache != CacheValidationStatus.ReturnCachedResponse) && (this.Policy.Level == HttpRequestCacheLevel.CacheOnly))
            {
                this.FailRequest(WebExceptionStatus.CacheEntryNotFound);
            }
            if (doNotTakeFromCache == CacheValidationStatus.ReturnCachedResponse)
            {
                if (base.CacheFreshnessStatus == CacheFreshnessStatus.Stale)
                {
                    this.CacheHeaders.Add("Warning", "110 Response is stale");
                }
                if (base.Policy.Level == RequestCacheLevel.CacheOnly)
                {
                    this.CacheHeaders.Add("Warning", "112 Disconnected operation");
                }
                if (this.HeuristicExpiration && (((int) this.CacheAge.TotalSeconds) >= 0x15180))
                {
                    this.CacheHeaders.Add("Warning", "113 Heuristic expiration");
                }
            }
            switch (doNotTakeFromCache)
            {
                case CacheValidationStatus.DoNotTakeFromCache:
                    this.CacheStatusCode = (HttpStatusCode) 0;
                    return doNotTakeFromCache;

                case CacheValidationStatus.ReturnCachedResponse:
                    this.CacheHeaders["Age"] = ((int) this.CacheAge.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo);
                    return doNotTakeFromCache;
            }
            return doNotTakeFromCache;
        }

        protected internal override CacheFreshnessStatus ValidateFreshness()
        {
            string str = this.ParseStatusLine();
            if (Logging.On)
            {
                if (this.CacheStatusCode == ((HttpStatusCode) 0))
                {
                    Logging.PrintError(Logging.RequestCache, SR.GetString("net_log_cache_http_status_parse_failure", new object[] { (str == null) ? "null" : str }));
                }
                else
                {
                    Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_http_status_line", new object[] { (this.CacheHttpVersion != null) ? this.CacheHttpVersion.ToString() : "null", (int) this.CacheStatusCode, this.CacheStatusDescription }));
                }
            }
            this.CreateCacheHeaders(this.CacheStatusCode != ((HttpStatusCode) 0));
            this.CreateSystemMeta();
            this.FetchHeaderValues(true);
            if (Logging.On)
            {
                Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_cache_control", new object[] { this.CacheCacheControl.ToString() }));
            }
            return Rfc2616.OnValidateFreshness(this);
        }

        protected internal override CacheValidationStatus ValidateRequest()
        {
            this.ZeroPrivateVars();
            string str = base.Request.Method.ToUpper(CultureInfo.InvariantCulture);
            if (Logging.On)
            {
                Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_request_method", new object[] { str }));
            }
            switch (str)
            {
                case "GET":
                    this.RequestMethod = HttpMethod.Get;
                    break;

                case "POST":
                    this.RequestMethod = HttpMethod.Post;
                    break;

                case "HEAD":
                    this.RequestMethod = HttpMethod.Head;
                    break;

                case "PUT":
                    this.RequestMethod = HttpMethod.Put;
                    break;

                case "DELETE":
                    this.RequestMethod = HttpMethod.Delete;
                    break;

                case "OPTIONS":
                    this.RequestMethod = HttpMethod.Options;
                    break;

                case "TRACE":
                    this.RequestMethod = HttpMethod.Trace;
                    break;

                case "CONNECT":
                    this.RequestMethod = HttpMethod.Connect;
                    break;

                default:
                    this.RequestMethod = HttpMethod.Other;
                    break;
            }
            return Rfc2616.OnValidateRequest(this);
        }

        protected internal override CacheValidationStatus ValidateResponse()
        {
            if (((this.Policy.Level != HttpRequestCacheLevel.CacheOrNextCacheOnly) && (this.Policy.Level != HttpRequestCacheLevel.Default)) && (this.Policy.Level != HttpRequestCacheLevel.Revalidate))
            {
                if (Logging.On)
                {
                    Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_response_valid_based_on_policy", new object[] { this.Policy.ToString() }));
                }
                return CacheValidationStatus.Continue;
            }
            HttpWebResponse response = base.Response as HttpWebResponse;
            if (response == null)
            {
                if (Logging.On)
                {
                    Logging.PrintWarning(Logging.RequestCache, SR.GetString("net_log_cache_null_response_failure"));
                }
                return CacheValidationStatus.Continue;
            }
            this.FetchHeaderValues(false);
            if (Logging.On)
            {
                Logging.PrintInfo(Logging.RequestCache, string.Concat(new object[] { "StatusCode=", ((int) response.StatusCode).ToString(CultureInfo.InvariantCulture), ' ', response.StatusCode.ToString(), (response.StatusCode == HttpStatusCode.PartialContent) ? (", Content-Range: " + response.Headers["Content-Range"]) : string.Empty }));
            }
            return Rfc2616.OnValidateResponse(this);
        }

        private void ZeroPrivateVars()
        {
            this.m_RequestVars = new RequestVars();
            this.m_HttpPolicy = null;
            this.m_StatusCode = (HttpStatusCode) 0;
            this.m_StatusDescription = null;
            this.m_HttpVersion = null;
            this.m_Headers = null;
            this.m_SystemMeta = null;
            this.m_DontUpdateHeaders = false;
            this.m_HeuristicExpiration = false;
            this.m_CacheVars = new Vars();
            this.m_CacheVars.Initialize();
            this.m_ResponseVars = new Vars();
            this.m_ResponseVars.Initialize();
        }

        internal TimeSpan CacheAge
        {
            get
            {
                return this.m_CacheVars.Age;
            }
            set
            {
                this.m_CacheVars.Age = value;
            }
        }

        internal System.Net.Cache.ResponseCacheControl CacheCacheControl
        {
            get
            {
                return this.m_CacheVars.CacheControl;
            }
            set
            {
                this.m_CacheVars.CacheControl = value;
            }
        }

        internal DateTime CacheDate
        {
            get
            {
                return this.m_CacheVars.Date;
            }
            set
            {
                this.m_CacheVars.Date = value;
            }
        }

        internal bool CacheDontUpdateHeaders
        {
            get
            {
                return this.m_DontUpdateHeaders;
            }
            set
            {
                this.m_DontUpdateHeaders = value;
            }
        }

        internal long CacheEntityLength
        {
            get
            {
                return this.m_CacheVars.EntityLength;
            }
            set
            {
                this.m_CacheVars.EntityLength = value;
            }
        }

        internal DateTime CacheExpires
        {
            get
            {
                return this.m_CacheVars.Expires;
            }
            set
            {
                this.m_CacheVars.Expires = value;
            }
        }

        internal WebHeaderCollection CacheHeaders
        {
            get
            {
                return this.m_Headers;
            }
            set
            {
                this.m_Headers = value;
            }
        }

        internal Version CacheHttpVersion
        {
            get
            {
                return this.m_HttpVersion;
            }
            set
            {
                this.m_HttpVersion = value;
            }
        }

        internal DateTime CacheLastModified
        {
            get
            {
                return this.m_CacheVars.LastModified;
            }
            set
            {
                this.m_CacheVars.LastModified = value;
            }
        }

        internal TimeSpan CacheMaxAge
        {
            get
            {
                return this.m_CacheVars.MaxAge;
            }
            set
            {
                this.m_CacheVars.MaxAge = value;
            }
        }

        internal HttpStatusCode CacheStatusCode
        {
            get
            {
                return this.m_StatusCode;
            }
            set
            {
                this.m_StatusCode = value;
            }
        }

        internal string CacheStatusDescription
        {
            get
            {
                return this.m_StatusDescription;
            }
            set
            {
                this.m_StatusDescription = value;
            }
        }

        internal bool HeuristicExpiration
        {
            get
            {
                return this.m_HeuristicExpiration;
            }
            set
            {
                this.m_HeuristicExpiration = value;
            }
        }

        internal HttpRequestCachePolicy Policy
        {
            get
            {
                if (this.m_HttpPolicy == null)
                {
                    this.m_HttpPolicy = base.Policy as HttpRequestCachePolicy;
                    if (this.m_HttpPolicy != null)
                    {
                        return this.m_HttpPolicy;
                    }
                    this.m_HttpPolicy = new HttpRequestCachePolicy((HttpRequestCacheLevel) base.Policy.Level);
                }
                return this.m_HttpPolicy;
            }
        }

        internal string RequestIfHeader1
        {
            get
            {
                return this.m_RequestVars.IfHeader1;
            }
            set
            {
                this.m_RequestVars.IfHeader1 = value;
            }
        }

        internal string RequestIfHeader2
        {
            get
            {
                return this.m_RequestVars.IfHeader2;
            }
            set
            {
                this.m_RequestVars.IfHeader2 = value;
            }
        }

        internal HttpMethod RequestMethod
        {
            get
            {
                return this.m_RequestVars.Method;
            }
            set
            {
                this.m_RequestVars.Method = value;
            }
        }

        internal bool RequestRangeCache
        {
            get
            {
                return this.m_RequestVars.IsCacheRange;
            }
            set
            {
                this.m_RequestVars.IsCacheRange = value;
            }
        }

        internal bool RequestRangeUser
        {
            get
            {
                return this.m_RequestVars.IsUserRange;
            }
            set
            {
                this.m_RequestVars.IsUserRange = value;
            }
        }

        internal string RequestValidator1
        {
            get
            {
                return this.m_RequestVars.Validator1;
            }
            set
            {
                this.m_RequestVars.Validator1 = value;
            }
        }

        internal string RequestValidator2
        {
            get
            {
                return this.m_RequestVars.Validator2;
            }
            set
            {
                this.m_RequestVars.Validator2 = value;
            }
        }

        internal TimeSpan ResponseAge
        {
            get
            {
                return this.m_ResponseVars.Age;
            }
            set
            {
                this.m_ResponseVars.Age = value;
            }
        }

        internal System.Net.Cache.ResponseCacheControl ResponseCacheControl
        {
            get
            {
                return this.m_ResponseVars.CacheControl;
            }
            set
            {
                this.m_ResponseVars.CacheControl = value;
            }
        }

        internal DateTime ResponseDate
        {
            get
            {
                return this.m_ResponseVars.Date;
            }
            set
            {
                this.m_ResponseVars.Date = value;
            }
        }

        internal long ResponseEntityLength
        {
            get
            {
                return this.m_ResponseVars.EntityLength;
            }
            set
            {
                this.m_ResponseVars.EntityLength = value;
            }
        }

        internal DateTime ResponseExpires
        {
            get
            {
                return this.m_ResponseVars.Expires;
            }
            set
            {
                this.m_ResponseVars.Expires = value;
            }
        }

        internal DateTime ResponseLastModified
        {
            get
            {
                return this.m_ResponseVars.LastModified;
            }
            set
            {
                this.m_ResponseVars.LastModified = value;
            }
        }

        internal long ResponseRangeEnd
        {
            get
            {
                return this.m_ResponseVars.RangeEnd;
            }
            set
            {
                this.m_ResponseVars.RangeEnd = value;
            }
        }

        internal long ResponseRangeStart
        {
            get
            {
                return this.m_ResponseVars.RangeStart;
            }
            set
            {
                this.m_ResponseVars.RangeStart = value;
            }
        }

        internal NameValueCollection SystemMeta
        {
            get
            {
                return this.m_SystemMeta;
            }
            set
            {
                this.m_SystemMeta = value;
            }
        }

        internal delegate void ParseCallback(string s, int start, int end, IList list);

        [StructLayout(LayoutKind.Sequential)]
        private struct RequestVars
        {
            internal HttpMethod Method;
            internal bool IsCacheRange;
            internal bool IsUserRange;
            internal string IfHeader1;
            internal string Validator1;
            internal string IfHeader2;
            internal string Validator2;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Vars
        {
            internal DateTime Date;
            internal DateTime Expires;
            internal DateTime LastModified;
            internal long EntityLength;
            internal TimeSpan Age;
            internal TimeSpan MaxAge;
            internal ResponseCacheControl CacheControl;
            internal long RangeStart;
            internal long RangeEnd;
            internal void Initialize()
            {
                this.EntityLength = this.RangeStart = this.RangeEnd = -1L;
                this.Date = DateTime.MinValue;
                this.Expires = DateTime.MinValue;
                this.LastModified = DateTime.MinValue;
                this.Age = TimeSpan.MinValue;
                this.MaxAge = TimeSpan.MinValue;
            }
        }
    }
}

