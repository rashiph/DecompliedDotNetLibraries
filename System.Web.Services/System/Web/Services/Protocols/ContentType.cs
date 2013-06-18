namespace System.Web.Services.Protocols
{
    using System;
    using System.Text;

    internal class ContentType
    {
        internal const string ApplicationBase = "application";
        internal const string ApplicationOctetStream = "application/octet-stream";
        internal const string ApplicationSoap = "application/soap+xml";
        internal const string ApplicationXml = "application/xml";
        internal const string ContentEncoding = "Content-Encoding";
        internal const string TextBase = "text";
        internal const string TextHtml = "text/html";
        internal const string TextPlain = "text/plain";
        internal const string TextXml = "text/xml";

        private ContentType()
        {
        }

        internal static string Compose(string contentType, Encoding encoding)
        {
            return Compose(contentType, encoding, null);
        }

        internal static string Compose(string contentType, Encoding encoding, string action)
        {
            if ((encoding == null) && (action == null))
            {
                return contentType;
            }
            StringBuilder builder = new StringBuilder(contentType);
            if (encoding != null)
            {
                builder.Append("; charset=");
                builder.Append(encoding.WebName);
            }
            if (action != null)
            {
                builder.Append("; action=\"");
                builder.Append(action);
                builder.Append("\"");
            }
            return builder.ToString();
        }

        internal static string GetAction(string contentType)
        {
            return GetParameter(contentType, "action");
        }

        internal static string GetBase(string contentType)
        {
            int index = contentType.IndexOf(';');
            if (index >= 0)
            {
                return contentType.Substring(0, index);
            }
            return contentType;
        }

        internal static string GetCharset(string contentType)
        {
            return GetParameter(contentType, "charset");
        }

        internal static string GetMediaType(string contentType)
        {
            string str = GetBase(contentType);
            int index = str.IndexOf('/');
            if (index >= 0)
            {
                return str.Substring(0, index);
            }
            return str;
        }

        private static string GetParameter(string contentType, string paramName)
        {
            string[] strArray = contentType.Split(new char[] { ';' });
            for (int i = 1; i < strArray.Length; i++)
            {
                string strA = strArray[i].TrimStart(null);
                if (string.Compare(strA, 0, paramName, 0, paramName.Length, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    int index = strA.IndexOf('=', paramName.Length);
                    if (index >= 0)
                    {
                        return strA.Substring(index + 1).Trim(new char[] { ' ', '\'', '"', '\t' });
                    }
                }
            }
            return null;
        }

        internal static bool IsApplication(string contentType)
        {
            return (string.Compare(GetMediaType(contentType), "application", StringComparison.OrdinalIgnoreCase) == 0);
        }

        internal static bool IsHtml(string contentType)
        {
            return (string.Compare(GetBase(contentType), "text/html", StringComparison.OrdinalIgnoreCase) == 0);
        }

        internal static bool IsSoap(string contentType)
        {
            string strA = GetBase(contentType);
            if (string.Compare(strA, "text/xml", StringComparison.OrdinalIgnoreCase) != 0)
            {
                return (string.Compare(strA, "application/soap+xml", StringComparison.OrdinalIgnoreCase) == 0);
            }
            return true;
        }

        internal static bool IsXml(string contentType)
        {
            string strA = GetBase(contentType);
            if (string.Compare(strA, "text/xml", StringComparison.OrdinalIgnoreCase) != 0)
            {
                return (string.Compare(strA, "application/xml", StringComparison.OrdinalIgnoreCase) == 0);
            }
            return true;
        }

        internal static bool MatchesBase(string contentType, string baseContentType)
        {
            return (string.Compare(GetBase(contentType), baseContentType, StringComparison.OrdinalIgnoreCase) == 0);
        }
    }
}

