namespace System.Net
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Reflection;

    internal class HeaderInfoTable
    {
        private static Hashtable HeaderHashTable;
        private static HeaderParser MultiParser = new HeaderParser(HeaderInfoTable.ParseMultiValue);
        private static HeaderParser SingleParser = new HeaderParser(HeaderInfoTable.ParseSingleValue);
        private static HeaderInfo UnknownHeaderInfo = new HeaderInfo(string.Empty, false, false, false, SingleParser);

        static HeaderInfoTable()
        {
            HeaderInfo[] infoArray = new HeaderInfo[] { 
                new HeaderInfo("Age", false, false, false, SingleParser), new HeaderInfo("Allow", false, false, true, MultiParser), new HeaderInfo("Accept", true, false, true, MultiParser), new HeaderInfo("Authorization", false, false, true, MultiParser), new HeaderInfo("Accept-Ranges", false, false, true, MultiParser), new HeaderInfo("Accept-Charset", false, false, true, MultiParser), new HeaderInfo("Accept-Encoding", false, false, true, MultiParser), new HeaderInfo("Accept-Language", false, false, true, MultiParser), new HeaderInfo("Cookie", false, false, true, MultiParser), new HeaderInfo("Connection", true, false, true, MultiParser), new HeaderInfo("Content-MD5", false, false, false, SingleParser), new HeaderInfo("Content-Type", true, false, false, SingleParser), new HeaderInfo("Cache-Control", false, false, true, MultiParser), new HeaderInfo("Content-Range", false, false, false, SingleParser), new HeaderInfo("Content-Length", true, true, false, SingleParser), new HeaderInfo("Content-Encoding", false, false, true, MultiParser), 
                new HeaderInfo("Content-Language", false, false, true, MultiParser), new HeaderInfo("Content-Location", false, false, false, SingleParser), new HeaderInfo("Date", true, false, false, SingleParser), new HeaderInfo("ETag", false, false, false, SingleParser), new HeaderInfo("Expect", true, false, true, MultiParser), new HeaderInfo("Expires", false, false, false, SingleParser), new HeaderInfo("From", false, false, false, SingleParser), new HeaderInfo("Host", true, false, false, SingleParser), new HeaderInfo("If-Match", false, false, true, MultiParser), new HeaderInfo("If-Range", false, false, false, SingleParser), new HeaderInfo("If-None-Match", false, false, true, MultiParser), new HeaderInfo("If-Modified-Since", true, false, false, SingleParser), new HeaderInfo("If-Unmodified-Since", false, false, false, SingleParser), new HeaderInfo("Keep-Alive", false, true, false, SingleParser), new HeaderInfo("Location", false, false, false, SingleParser), new HeaderInfo("Last-Modified", false, false, false, SingleParser), 
                new HeaderInfo("Max-Forwards", false, false, false, SingleParser), new HeaderInfo("Pragma", false, false, true, MultiParser), new HeaderInfo("Proxy-Authenticate", false, false, true, MultiParser), new HeaderInfo("Proxy-Authorization", false, false, true, MultiParser), new HeaderInfo("Proxy-Connection", true, false, true, MultiParser), new HeaderInfo("Range", true, false, true, MultiParser), new HeaderInfo("Referer", true, false, false, SingleParser), new HeaderInfo("Retry-After", false, false, false, SingleParser), new HeaderInfo("Server", false, false, false, SingleParser), new HeaderInfo("Set-Cookie", false, false, true, MultiParser), new HeaderInfo("Set-Cookie2", false, false, true, MultiParser), new HeaderInfo("TE", false, false, true, MultiParser), new HeaderInfo("Trailer", false, false, true, MultiParser), new HeaderInfo("Transfer-Encoding", true, true, true, MultiParser), new HeaderInfo("Upgrade", false, false, true, MultiParser), new HeaderInfo("User-Agent", true, false, false, SingleParser), 
                new HeaderInfo("Via", false, false, true, MultiParser), new HeaderInfo("Vary", false, false, true, MultiParser), new HeaderInfo("Warning", false, false, true, MultiParser), new HeaderInfo("WWW-Authenticate", false, true, true, SingleParser)
             };
            HeaderHashTable = new Hashtable(infoArray.Length * 2, CaseInsensitiveAscii.StaticInstance);
            for (int i = 0; i < infoArray.Length; i++)
            {
                HeaderHashTable[infoArray[i].HeaderName] = infoArray[i];
            }
        }

        private static string[] ParseMultiValue(string value)
        {
            StringCollection strings = new StringCollection();
            bool flag = false;
            int length = 0;
            char[] chArray = new char[value.Length];
            for (int i = 0; i < value.Length; i++)
            {
                if (value[i] == '"')
                {
                    flag = !flag;
                }
                else if ((value[i] == ',') && !flag)
                {
                    string str = new string(chArray, 0, length);
                    strings.Add(str.Trim());
                    length = 0;
                    continue;
                }
                chArray[length++] = value[i];
            }
            if (length != 0)
            {
                strings.Add(new string(chArray, 0, length).Trim());
            }
            string[] array = new string[strings.Count];
            strings.CopyTo(array, 0);
            return array;
        }

        private static string[] ParseSingleValue(string value)
        {
            return new string[] { value };
        }

        internal HeaderInfo this[string name]
        {
            get
            {
                HeaderInfo info = (HeaderInfo) HeaderHashTable[name];
                if (info == null)
                {
                    return UnknownHeaderInfo;
                }
                return info;
            }
        }
    }
}

