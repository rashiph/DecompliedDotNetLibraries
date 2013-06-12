namespace System.Runtime.Remoting.Metadata.W3cXsd2001
{
    using System;
    using System.Text;

    internal static class SoapType
    {
        internal static Type typeofISoapXsd = typeof(ISoapXsd);
        internal static Type typeofSoapAnyUri = typeof(SoapAnyUri);
        internal static Type typeofSoapBase64Binary = typeof(SoapBase64Binary);
        internal static Type typeofSoapDate = typeof(SoapDate);
        internal static Type typeofSoapDay = typeof(SoapDay);
        internal static Type typeofSoapEntities = typeof(SoapEntities);
        internal static Type typeofSoapEntity = typeof(SoapEntity);
        internal static Type typeofSoapHexBinary = typeof(SoapHexBinary);
        internal static Type typeofSoapId = typeof(SoapId);
        internal static Type typeofSoapIdref = typeof(SoapIdref);
        internal static Type typeofSoapIdrefs = typeof(SoapIdrefs);
        internal static Type typeofSoapInteger = typeof(SoapInteger);
        internal static Type typeofSoapLanguage = typeof(SoapLanguage);
        internal static Type typeofSoapMonth = typeof(SoapMonth);
        internal static Type typeofSoapMonthDay = typeof(SoapMonthDay);
        internal static Type typeofSoapName = typeof(SoapName);
        internal static Type typeofSoapNcName = typeof(SoapNcName);
        internal static Type typeofSoapNegativeInteger = typeof(SoapNegativeInteger);
        internal static Type typeofSoapNmtoken = typeof(SoapNmtoken);
        internal static Type typeofSoapNmtokens = typeof(SoapNmtokens);
        internal static Type typeofSoapNonNegativeInteger = typeof(SoapNonNegativeInteger);
        internal static Type typeofSoapNonPositiveInteger = typeof(SoapNonPositiveInteger);
        internal static Type typeofSoapNormalizedString = typeof(SoapNormalizedString);
        internal static Type typeofSoapNotation = typeof(SoapNotation);
        internal static Type typeofSoapPositiveInteger = typeof(SoapPositiveInteger);
        internal static Type typeofSoapQName = typeof(SoapQName);
        internal static Type typeofSoapTime = typeof(SoapTime);
        internal static Type typeofSoapToken = typeof(SoapToken);
        internal static Type typeofSoapYear = typeof(SoapYear);
        internal static Type typeofSoapYearMonth = typeof(SoapYearMonth);

        internal static string Escape(string value)
        {
            if ((value != null) && (value.Length != 0))
            {
                StringBuilder builder = new StringBuilder();
                int index = value.IndexOf('&');
                if (index > -1)
                {
                    builder.Append(value);
                    builder.Replace("&", "&#38;", index, builder.Length - index);
                }
                index = value.IndexOf('"');
                if (index > -1)
                {
                    if (builder.Length == 0)
                    {
                        builder.Append(value);
                    }
                    builder.Replace("\"", "&#34;", index, builder.Length - index);
                }
                index = value.IndexOf('\'');
                if (index > -1)
                {
                    if (builder.Length == 0)
                    {
                        builder.Append(value);
                    }
                    builder.Replace("'", "&#39;", index, builder.Length - index);
                }
                index = value.IndexOf('<');
                if (index > -1)
                {
                    if (builder.Length == 0)
                    {
                        builder.Append(value);
                    }
                    builder.Replace("<", "&#60;", index, builder.Length - index);
                }
                index = value.IndexOf('>');
                if (index > -1)
                {
                    if (builder.Length == 0)
                    {
                        builder.Append(value);
                    }
                    builder.Replace(">", "&#62;", index, builder.Length - index);
                }
                index = value.IndexOf('\0');
                if (index > -1)
                {
                    if (builder.Length == 0)
                    {
                        builder.Append(value);
                    }
                    builder.Replace('\0'.ToString(), "&#0;", index, builder.Length - index);
                }
                if (builder.Length > 0)
                {
                    return builder.ToString();
                }
            }
            return value;
        }

        internal static string FilterBin64(string value)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < value.Length; i++)
            {
                if (((value[i] != ' ') && (value[i] != '\n')) && (value[i] != '\r'))
                {
                    builder.Append(value[i]);
                }
            }
            return builder.ToString();
        }

        internal static string LineFeedsBin64(string value)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < value.Length; i++)
            {
                if ((i % 0x4c) == 0)
                {
                    builder.Append('\n');
                }
                builder.Append(value[i]);
            }
            return builder.ToString();
        }
    }
}

