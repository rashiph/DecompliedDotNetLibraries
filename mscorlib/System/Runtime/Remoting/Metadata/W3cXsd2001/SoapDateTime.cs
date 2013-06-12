namespace System.Runtime.Remoting.Metadata.W3cXsd2001
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;

    [ComVisible(true)]
    public sealed class SoapDateTime
    {
        private static string[] formats = new string[] { 
            "yyyy-MM-dd'T'HH:mm:ss.fffffffzzz", "yyyy-MM-dd'T'HH:mm:ss.ffff", "yyyy-MM-dd'T'HH:mm:ss.ffffzzz", "yyyy-MM-dd'T'HH:mm:ss.fff", "yyyy-MM-dd'T'HH:mm:ss.fffzzz", "yyyy-MM-dd'T'HH:mm:ss.ff", "yyyy-MM-dd'T'HH:mm:ss.ffzzz", "yyyy-MM-dd'T'HH:mm:ss.f", "yyyy-MM-dd'T'HH:mm:ss.fzzz", "yyyy-MM-dd'T'HH:mm:ss", "yyyy-MM-dd'T'HH:mm:sszzz", "yyyy-MM-dd'T'HH:mm:ss.fffff", "yyyy-MM-dd'T'HH:mm:ss.fffffzzz", "yyyy-MM-dd'T'HH:mm:ss.ffffff", "yyyy-MM-dd'T'HH:mm:ss.ffffffzzz", "yyyy-MM-dd'T'HH:mm:ss.fffffff", 
            "yyyy-MM-dd'T'HH:mm:ss.ffffffff", "yyyy-MM-dd'T'HH:mm:ss.ffffffffzzz", "yyyy-MM-dd'T'HH:mm:ss.fffffffff", "yyyy-MM-dd'T'HH:mm:ss.fffffffffzzz", "yyyy-MM-dd'T'HH:mm:ss.ffffffffff", "yyyy-MM-dd'T'HH:mm:ss.ffffffffffzzz"
         };

        public static DateTime Parse(string value)
        {
            DateTime time;
            try
            {
                if (value == null)
                {
                    return DateTime.MinValue;
                }
                string s = value;
                if (value.EndsWith("Z", StringComparison.Ordinal))
                {
                    s = value.Substring(0, value.Length - 1) + "-00:00";
                }
                time = DateTime.ParseExact(s, formats, CultureInfo.InvariantCulture, DateTimeStyles.None);
            }
            catch (Exception)
            {
                throw new RemotingException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_SOAPInteropxsdInvalid"), new object[] { "xsd:dateTime", value }));
            }
            return time;
        }

        public static string ToString(DateTime value)
        {
            return value.ToString("yyyy-MM-dd'T'HH:mm:ss.fffffffzzz", CultureInfo.InvariantCulture);
        }

        public static string XsdType
        {
            get
            {
                return "dateTime";
            }
        }
    }
}

