namespace System.Net
{
    using System;
    using System.Globalization;

    internal class HttpProtocolUtils
    {
        private HttpProtocolUtils()
        {
        }

        internal static string date2string(DateTime D)
        {
            DateTimeFormatInfo provider = new DateTimeFormatInfo();
            return D.ToUniversalTime().ToString("R", provider);
        }

        internal static DateTime string2date(string S)
        {
            DateTime time;
            if (!HttpDateParse.ParseHttpDate(S, out time))
            {
                throw new ProtocolViolationException(SR.GetString("net_baddate"));
            }
            return time;
        }
    }
}

