namespace System.Net
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Net.NetworkInformation;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;

    [Serializable]
    public class CookieContainer
    {
        public const int DefaultCookieLengthLimit = 0x1000;
        public const int DefaultCookieLimit = 300;
        public const int DefaultPerDomainCookieLimit = 20;
        private static readonly HeaderVariantInfo[] HeaderInfo = new HeaderVariantInfo[] { new HeaderVariantInfo("Set-Cookie", CookieVariant.Rfc2109), new HeaderVariantInfo("Set-Cookie2", CookieVariant.Rfc2965) };
        private int m_count;
        private Hashtable m_domainTable;
        private string m_fqdnMyDomain;
        private int m_maxCookies;
        private int m_maxCookieSize;
        private int m_maxCookiesPerDomain;

        public CookieContainer()
        {
            this.m_domainTable = new Hashtable();
            this.m_maxCookieSize = 0x1000;
            this.m_maxCookies = 300;
            this.m_maxCookiesPerDomain = 20;
            this.m_fqdnMyDomain = string.Empty;
            string domainName = IPGlobalProperties.InternalGetIPGlobalProperties().DomainName;
            if ((domainName != null) && (domainName.Length > 1))
            {
                this.m_fqdnMyDomain = '.' + domainName;
            }
        }

        public CookieContainer(int capacity) : this()
        {
            if (capacity <= 0)
            {
                throw new ArgumentException(SR.GetString("net_toosmall"), "Capacity");
            }
            this.m_maxCookies = capacity;
        }

        public CookieContainer(int capacity, int perDomainCapacity, int maxCookieSize) : this(capacity)
        {
            if ((perDomainCapacity != 0x7fffffff) && ((perDomainCapacity <= 0) || (perDomainCapacity > capacity)))
            {
                throw new ArgumentOutOfRangeException("perDomainCapacity", SR.GetString("net_cookie_capacity_range", new object[] { "PerDomainCapacity", 0, capacity }));
            }
            this.m_maxCookiesPerDomain = perDomainCapacity;
            if (maxCookieSize <= 0)
            {
                throw new ArgumentException(SR.GetString("net_toosmall"), "MaxCookieSize");
            }
            this.m_maxCookieSize = maxCookieSize;
        }

        public void Add(Cookie cookie)
        {
            Uri uri;
            if (cookie == null)
            {
                throw new ArgumentNullException("cookie");
            }
            if (cookie.Domain.Length == 0)
            {
                throw new ArgumentException(SR.GetString("net_emptystringcall"), "cookie.Domain");
            }
            StringBuilder builder = new StringBuilder();
            builder.Append(cookie.Secure ? Uri.UriSchemeHttps : Uri.UriSchemeHttp).Append(Uri.SchemeDelimiter);
            if (!cookie.DomainImplicit && (cookie.Domain[0] == '.'))
            {
                builder.Append("0");
            }
            builder.Append(cookie.Domain);
            if (cookie.PortList != null)
            {
                builder.Append(":").Append(cookie.PortList[0]);
            }
            builder.Append(cookie.Path);
            if (!Uri.TryCreate(builder.ToString(), UriKind.Absolute, out uri))
            {
                throw new CookieException(SR.GetString("net_cookie_attribute", new object[] { "Domain", cookie.Domain }));
            }
            Cookie cookie2 = cookie.Clone();
            cookie2.VerifySetDefaults(cookie2.Variant, uri, this.IsLocalDomain(uri.Host), this.m_fqdnMyDomain, true, true);
            this.Add(cookie2, true);
        }

        public void Add(CookieCollection cookies)
        {
            if (cookies == null)
            {
                throw new ArgumentNullException("cookies");
            }
            foreach (Cookie cookie in cookies)
            {
                this.Add(cookie);
            }
        }

        internal void Add(Cookie cookie, bool throwOnError)
        {
            if (cookie.Value.Length > this.m_maxCookieSize)
            {
                if (throwOnError)
                {
                    throw new CookieException(SR.GetString("net_cookie_size", new object[] { cookie.ToString(), this.m_maxCookieSize }));
                }
            }
            else
            {
                try
                {
                    PathList list = (PathList) this.m_domainTable[cookie.DomainKey];
                    if (list == null)
                    {
                        list = new PathList();
                        this.AddRemoveDomain(cookie.DomainKey, list);
                    }
                    int cookiesCount = list.GetCookiesCount();
                    CookieCollection cookies = (CookieCollection) list[cookie.Path];
                    if (cookies == null)
                    {
                        cookies = new CookieCollection();
                        list[cookie.Path] = cookies;
                    }
                    if (cookie.Expired)
                    {
                        lock (cookies)
                        {
                            int index = cookies.IndexOf(cookie);
                            if (index != -1)
                            {
                                cookies.RemoveAt(index);
                                this.m_count--;
                            }
                            return;
                        }
                    }
                    if (((cookiesCount < this.m_maxCookiesPerDomain) || this.AgeCookies(cookie.DomainKey)) && ((this.m_count < this.m_maxCookies) || this.AgeCookies(null)))
                    {
                        lock (cookies)
                        {
                            this.m_count += cookies.InternalAdd(cookie, true);
                        }
                    }
                }
                catch (Exception exception)
                {
                    if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                    {
                        throw;
                    }
                    if (throwOnError)
                    {
                        throw new CookieException(SR.GetString("net_container_add_cookie"), exception);
                    }
                }
            }
        }

        public void Add(Uri uri, Cookie cookie)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }
            if (cookie == null)
            {
                throw new ArgumentNullException("cookie");
            }
            Cookie cookie2 = cookie.Clone();
            cookie2.VerifySetDefaults(cookie2.Variant, uri, this.IsLocalDomain(uri.Host), this.m_fqdnMyDomain, true, true);
            this.Add(cookie2, true);
        }

        public void Add(Uri uri, CookieCollection cookies)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }
            if (cookies == null)
            {
                throw new ArgumentNullException("cookies");
            }
            bool isLocalDomain = this.IsLocalDomain(uri.Host);
            foreach (Cookie cookie in cookies)
            {
                Cookie cookie2 = cookie.Clone();
                cookie2.VerifySetDefaults(cookie2.Variant, uri, isLocalDomain, this.m_fqdnMyDomain, true, true);
                this.Add(cookie2, true);
            }
        }

        private void AddRemoveDomain(string key, PathList value)
        {
            lock (this)
            {
                if (value == null)
                {
                    this.m_domainTable.Remove(key);
                }
                else
                {
                    this.m_domainTable[key] = value;
                }
            }
        }

        private bool AgeCookies(string domain)
        {
            if ((this.m_maxCookies == 0) || (this.m_maxCookiesPerDomain == 0))
            {
                this.m_domainTable = new Hashtable();
                this.m_count = 0;
                return false;
            }
            int num = 0;
            DateTime maxValue = DateTime.MaxValue;
            CookieCollection cookies = null;
            string key = null;
            int num2 = 0;
            int index = 0;
            float num4 = 1f;
            if (this.m_count > this.m_maxCookies)
            {
                num4 = ((float) this.m_maxCookies) / ((float) this.m_count);
            }
            foreach (DictionaryEntry entry in this.m_domainTable)
            {
                PathList list;
                if (domain == null)
                {
                    key = (string) entry.Key;
                    list = (PathList) entry.Value;
                }
                else
                {
                    key = domain;
                    list = (PathList) this.m_domainTable[domain];
                }
                num2 = 0;
                foreach (CookieCollection cookies2 in list.Values)
                {
                    DateTime time2;
                    index = this.ExpireCollection(cookies2);
                    num += index;
                    this.m_count -= index;
                    num2 += cookies2.Count;
                    if ((cookies2.Count > 0) && ((time2 = cookies2.TimeStamp(CookieCollection.Stamp.Check)) < maxValue))
                    {
                        cookies = cookies2;
                        maxValue = time2;
                    }
                }
                int num5 = Math.Min((int) (num2 * num4), Math.Min(this.m_maxCookiesPerDomain, this.m_maxCookies) - 1);
                if (num2 > num5)
                {
                    Array items = Array.CreateInstance(typeof(CookieCollection), list.Count);
                    Array keys = Array.CreateInstance(typeof(DateTime), list.Count);
                    foreach (CookieCollection cookies3 in list.Values)
                    {
                        keys.SetValue(cookies3.TimeStamp(CookieCollection.Stamp.Check), index);
                        items.SetValue(cookies3, index);
                        index++;
                    }
                    Array.Sort(keys, items);
                    index = 0;
                    for (int i = 0; i < list.Count; i++)
                    {
                        CookieCollection cookies4 = (CookieCollection) items.GetValue(i);
                        lock (cookies4)
                        {
                            while ((num2 > num5) && (cookies4.Count > 0))
                            {
                                cookies4.RemoveAt(0);
                                num2--;
                                this.m_count--;
                                num++;
                            }
                        }
                        if (num2 <= num5)
                        {
                            break;
                        }
                    }
                    if ((num2 > num5) && (domain != null))
                    {
                        return false;
                    }
                }
                if (domain != null)
                {
                    return true;
                }
            }
            if (num == 0)
            {
                if (maxValue == DateTime.MaxValue)
                {
                    return false;
                }
                lock (cookies)
                {
                    while ((this.m_count >= this.m_maxCookies) && (cookies.Count > 0))
                    {
                        cookies.RemoveAt(0);
                        this.m_count--;
                    }
                }
            }
            return true;
        }

        internal CookieCollection CookieCutter(Uri uri, string headerName, string setCookieHeader, bool isThrow)
        {
            CookieCollection cookies = new CookieCollection();
            CookieVariant unknown = CookieVariant.Unknown;
            if (headerName == null)
            {
                unknown = CookieVariant.Rfc2109;
            }
            else
            {
                for (int i = 0; i < HeaderInfo.Length; i++)
                {
                    if (string.Compare(headerName, HeaderInfo[i].Name, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        unknown = HeaderInfo[i].Variant;
                    }
                }
            }
            bool isLocalDomain = this.IsLocalDomain(uri.Host);
            try
            {
                Cookie cookie;
                CookieParser parser = new CookieParser(setCookieHeader);
            Label_0060:
                cookie = parser.Get();
                if (cookie != null)
                {
                    if (ValidationHelper.IsBlankString(cookie.Name))
                    {
                        if (isThrow)
                        {
                            throw new CookieException(SR.GetString("net_cookie_format"));
                        }
                    }
                    else if (cookie.VerifySetDefaults(unknown, uri, isLocalDomain, this.m_fqdnMyDomain, true, isThrow))
                    {
                        cookies.InternalAdd(cookie, true);
                    }
                    goto Label_0060;
                }
            }
            catch (Exception exception)
            {
                if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                {
                    throw;
                }
                if (isThrow)
                {
                    throw new CookieException(SR.GetString("net_cookie_parse_header", new object[] { uri.AbsoluteUri }), exception);
                }
            }
            foreach (Cookie cookie2 in cookies)
            {
                this.Add(cookie2, isThrow);
            }
            return cookies;
        }

        private int ExpireCollection(CookieCollection cc)
        {
            int count = cc.Count;
            int idx = count - 1;
            lock (cc)
            {
                while (idx >= 0)
                {
                    Cookie cookie = cc[idx];
                    if (cookie.Expired)
                    {
                        cc.RemoveAt(idx);
                    }
                    idx--;
                }
            }
            return (count - cc.Count);
        }

        public string GetCookieHeader(Uri uri)
        {
            string str;
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }
            return this.GetCookieHeader(uri, out str);
        }

        internal string GetCookieHeader(Uri uri, out string optCookie2)
        {
            CookieCollection cookies = this.InternalGetCookies(uri);
            string str = string.Empty;
            string str2 = string.Empty;
            foreach (Cookie cookie in cookies)
            {
                str = str + str2 + cookie.ToString();
                str2 = "; ";
            }
            optCookie2 = cookies.IsOtherVersionSeen ? ("$Version=" + 1.ToString(NumberFormatInfo.InvariantInfo)) : string.Empty;
            return str;
        }

        public CookieCollection GetCookies(Uri uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }
            return this.InternalGetCookies(uri);
        }

        internal CookieCollection InternalGetCookies(Uri uri)
        {
            bool isSecure = uri.Scheme == Uri.UriSchemeHttps;
            int port = uri.Port;
            CookieCollection destination = new CookieCollection();
            ArrayList list = new ArrayList();
            int num2 = 0;
            string host = uri.Host;
            int index = host.IndexOf('.');
            if (index == -1)
            {
                list.Add(host);
                list.Add("." + host);
                if ((this.m_fqdnMyDomain != null) && (this.m_fqdnMyDomain.Length != 0))
                {
                    list.Add(host + this.m_fqdnMyDomain);
                    list.Add(this.m_fqdnMyDomain);
                    num2 = 3;
                }
                else
                {
                    num2 = 1;
                }
            }
            else
            {
                list.Add(host);
                list.Add("." + host);
                list.Add(host.Substring(index));
                num2 = 2;
                if (host.Length > 2)
                {
                    int num4 = host.LastIndexOf('.', host.Length - 2);
                    if (num4 > 0)
                    {
                        num4 = host.LastIndexOf('.', num4 - 1);
                    }
                    if (num4 != -1)
                    {
                        while ((index < num4) && ((index = host.IndexOf('.', index + 1)) != -1))
                        {
                            list.Add(host.Substring(index));
                        }
                    }
                }
            }
            foreach (string str2 in list)
            {
                bool flag2 = false;
                bool flag3 = false;
                PathList list2 = (PathList) this.m_domainTable[str2];
                num2--;
                if (list2 != null)
                {
                    foreach (DictionaryEntry entry in list2)
                    {
                        string key = (string) entry.Key;
                        if (uri.AbsolutePath.StartsWith(CookieParser.CheckQuoted(key)))
                        {
                            flag2 = true;
                            CookieCollection source = (CookieCollection) entry.Value;
                            source.TimeStamp(CookieCollection.Stamp.Set);
                            this.MergeUpdateCollections(destination, source, port, isSecure, num2 < 0);
                            if (key == "/")
                            {
                                flag3 = true;
                            }
                        }
                        else if (flag2)
                        {
                            break;
                        }
                    }
                    if (!flag3)
                    {
                        CookieCollection cookies3 = (CookieCollection) list2["/"];
                        if (cookies3 != null)
                        {
                            cookies3.TimeStamp(CookieCollection.Stamp.Set);
                            this.MergeUpdateCollections(destination, cookies3, port, isSecure, num2 < 0);
                        }
                    }
                    if (list2.Count == 0)
                    {
                        this.AddRemoveDomain(str2, null);
                    }
                }
            }
            return destination;
        }

        internal bool IsLocalDomain(string host)
        {
            int index = host.IndexOf('.');
            if (index == -1)
            {
                return true;
            }
            if (((host == "127.0.0.1") || (host == "::1")) || (host == "0:0:0:0:0:0:0:1"))
            {
                return true;
            }
            if (string.Compare(this.m_fqdnMyDomain, 0, host, index, this.m_fqdnMyDomain.Length, StringComparison.OrdinalIgnoreCase) == 0)
            {
                return true;
            }
            string[] strArray = host.Split(new char[] { '.' });
            if (((strArray == null) || (strArray.Length != 4)) || !(strArray[0] == "127"))
            {
                goto Label_010C;
            }
            int num2 = 1;
            while (num2 < 4)
            {
                switch (strArray[num2].Length)
                {
                    case 1:
                        goto Label_00E4;

                    case 2:
                        break;

                    case 3:
                        if ((strArray[num2][2] < '0') || (strArray[num2][2] > '9'))
                        {
                            goto Label_0106;
                        }
                        break;

                    default:
                        goto Label_0106;
                }
                if ((strArray[num2][1] < '0') || (strArray[num2][1] > '9'))
                {
                    break;
                }
            Label_00E4:
                if ((strArray[num2][0] < '0') || (strArray[num2][0] > '9'))
                {
                    break;
                }
                num2++;
            }
        Label_0106:
            if (num2 == 4)
            {
                return true;
            }
        Label_010C:
            return false;
        }

        private void MergeUpdateCollections(CookieCollection destination, CookieCollection source, int port, bool isSecure, bool isPlainOnly)
        {
            lock (source)
            {
                for (int i = 0; i < source.Count; i++)
                {
                    bool flag = false;
                    Cookie cookie = source[i];
                    if (cookie.Expired)
                    {
                        source.RemoveAt(i);
                        this.m_count--;
                        i--;
                        continue;
                    }
                    if (!isPlainOnly || (cookie.Variant == CookieVariant.Plain))
                    {
                        if (cookie.PortList != null)
                        {
                            foreach (int num2 in cookie.PortList)
                            {
                                if (num2 == port)
                                {
                                    flag = true;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            flag = true;
                        }
                    }
                    if (cookie.Secure && !isSecure)
                    {
                        flag = false;
                    }
                    if (flag)
                    {
                        destination.InternalAdd(cookie, false);
                    }
                }
            }
        }

        public void SetCookies(Uri uri, string cookieHeader)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }
            if (cookieHeader == null)
            {
                throw new ArgumentNullException("cookieHeader");
            }
            this.CookieCutter(uri, null, cookieHeader, true);
        }

        public int Capacity
        {
            get
            {
                return this.m_maxCookies;
            }
            set
            {
                if ((value <= 0) || ((value < this.m_maxCookiesPerDomain) && (this.m_maxCookiesPerDomain != 0x7fffffff)))
                {
                    throw new ArgumentOutOfRangeException("value", SR.GetString("net_cookie_capacity_range", new object[] { "Capacity", 0, this.m_maxCookiesPerDomain }));
                }
                if (value < this.m_maxCookies)
                {
                    this.m_maxCookies = value;
                    this.AgeCookies(null);
                }
                this.m_maxCookies = value;
            }
        }

        public int Count
        {
            get
            {
                return this.m_count;
            }
        }

        public int MaxCookieSize
        {
            get
            {
                return this.m_maxCookieSize;
            }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.m_maxCookieSize = value;
            }
        }

        public int PerDomainCapacity
        {
            get
            {
                return this.m_maxCookiesPerDomain;
            }
            set
            {
                if ((value <= 0) || ((value > this.m_maxCookies) && (value != 0x7fffffff)))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                if (value < this.m_maxCookiesPerDomain)
                {
                    this.m_maxCookiesPerDomain = value;
                    this.AgeCookies(null);
                }
                this.m_maxCookiesPerDomain = value;
            }
        }
    }
}

