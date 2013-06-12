namespace System.Net
{
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Text.RegularExpressions;

    internal abstract class WebProxyDataBuilder
    {
        private const char addressListDelimiter = ';';
        private const char addressListSchemeValueDelimiter = '=';
        private const char bypassListDelimiter = ';';
        private WebProxyData m_Result;
        private const string regexReserved = @"#$()+.?[\^{|";

        protected WebProxyDataBuilder()
        {
        }

        public WebProxyData Build()
        {
            this.m_Result = new WebProxyData();
            this.BuildInternal();
            return this.m_Result;
        }

        protected abstract void BuildInternal();
        private static string BypassStringEscape(string rawString)
        {
            string str;
            string str2;
            string str3;
            Match match = new Regex("^(?<scheme>.*://)?(?<host>[^:]*)(?<port>:[0-9]{1,5})?$", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase).Match(rawString);
            if (match.Success)
            {
                str = match.Groups["scheme"].Value;
                str2 = match.Groups["host"].Value;
                str3 = match.Groups["port"].Value;
            }
            else
            {
                str = string.Empty;
                str2 = rawString;
                str3 = string.Empty;
            }
            str = ConvertRegexReservedChars(str);
            str2 = ConvertRegexReservedChars(str2);
            str3 = ConvertRegexReservedChars(str3);
            if (str == string.Empty)
            {
                str = "(?:.*://)?";
            }
            if (str3 == string.Empty)
            {
                str3 = "(?::[0-9]{1,5})?";
            }
            return ("^" + str + str2 + str3 + "$");
        }

        private static string ConvertRegexReservedChars(string rawString)
        {
            if (rawString.Length == 0)
            {
                return rawString;
            }
            StringBuilder builder = new StringBuilder();
            foreach (char ch in rawString)
            {
                if (@"#$()+.?[\^{|".IndexOf(ch) != -1)
                {
                    builder.Append('\\');
                }
                else if (ch == '*')
                {
                    builder.Append('.');
                }
                builder.Append(ch);
            }
            return builder.ToString();
        }

        private static FormatException CreateInvalidProxyStringException(string originalProxyString)
        {
            string msg = SR.GetString("net_proxy_invalid_url_format", new object[] { originalProxyString });
            if (Logging.On)
            {
                Logging.PrintError(Logging.Web, msg);
            }
            return new FormatException(msg);
        }

        private static ArrayList ParseBypassList(string bypassListString, out bool bypassOnLocal)
        {
            string[] strArray = bypassListString.Split(new char[] { ';' });
            bypassOnLocal = false;
            if (strArray.Length == 0)
            {
                return null;
            }
            ArrayList list = null;
            foreach (string str in strArray)
            {
                if (str != null)
                {
                    string strA = str.Trim();
                    if (strA.Length > 0)
                    {
                        if (string.Compare(strA, "<local>", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            bypassOnLocal = true;
                        }
                        else
                        {
                            strA = BypassStringEscape(strA);
                            if (list == null)
                            {
                                list = new ArrayList();
                            }
                            if (!list.Contains(strA))
                            {
                                list.Add(strA);
                            }
                        }
                    }
                }
            }
            return list;
        }

        private static Hashtable ParseProtocolProxies(string proxyListString)
        {
            string[] strArray = proxyListString.Split(new char[] { ';' });
            Hashtable hashtable = new Hashtable(CaseInsensitiveAscii.StaticInstance);
            for (int i = 0; i < strArray.Length; i++)
            {
                string str = strArray[i].Trim();
                if (str != string.Empty)
                {
                    string[] strArray2 = str.Split(new char[] { '=' });
                    if (strArray2.Length != 2)
                    {
                        throw CreateInvalidProxyStringException(proxyListString);
                    }
                    strArray2[0] = strArray2[0].Trim();
                    strArray2[1] = strArray2[1].Trim();
                    if ((strArray2[0] == string.Empty) || (strArray2[1] == string.Empty))
                    {
                        throw CreateInvalidProxyStringException(proxyListString);
                    }
                    hashtable[strArray2[0]] = ParseProxyUri(strArray2[1]);
                }
            }
            return hashtable;
        }

        private static Uri ParseProxyUri(string proxyString)
        {
            Uri uri;
            if (proxyString.IndexOf("://") == -1)
            {
                proxyString = "http://" + proxyString;
            }
            try
            {
                uri = new Uri(proxyString);
            }
            catch (UriFormatException exception)
            {
                if (Logging.On)
                {
                    Logging.PrintError(Logging.Web, exception.Message);
                }
                throw CreateInvalidProxyStringException(proxyString);
            }
            return uri;
        }

        protected void SetAutoDetectSettings(bool value)
        {
            this.m_Result.automaticallyDetectSettings = value;
        }

        protected void SetAutoProxyUrl(string autoConfigUrl)
        {
            if (!string.IsNullOrEmpty(autoConfigUrl))
            {
                Uri result = null;
                if (Uri.TryCreate(autoConfigUrl, UriKind.Absolute, out result))
                {
                    this.m_Result.scriptLocation = result;
                }
            }
        }

        protected void SetProxyAndBypassList(string addressString, string bypassListString)
        {
            if (addressString != null)
            {
                addressString = addressString.Trim();
                if (addressString != string.Empty)
                {
                    if (addressString.IndexOf('=') == -1)
                    {
                        this.m_Result.proxyAddress = ParseProxyUri(addressString);
                    }
                    else
                    {
                        this.m_Result.proxyHostAddresses = ParseProtocolProxies(addressString);
                    }
                    if (bypassListString != null)
                    {
                        bypassListString = bypassListString.Trim();
                        if (bypassListString != string.Empty)
                        {
                            bool bypassOnLocal = false;
                            this.m_Result.bypassList = ParseBypassList(bypassListString, out bypassOnLocal);
                            this.m_Result.bypassOnLocal = bypassOnLocal;
                        }
                    }
                }
            }
        }
    }
}

