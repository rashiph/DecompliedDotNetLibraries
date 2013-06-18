namespace System.Web.Services.Protocols
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Text;

    public class UrlParameterWriter : UrlEncodedParameterWriter
    {
        public override string GetRequestUrl(string url, object[] parameters)
        {
            if (parameters.Length == 0)
            {
                return url;
            }
            StringBuilder sb = new StringBuilder(url);
            sb.Append('?');
            TextWriter writer = new StringWriter(sb, CultureInfo.InvariantCulture);
            base.Encode(writer, parameters);
            writer.Flush();
            return sb.ToString();
        }
    }
}

