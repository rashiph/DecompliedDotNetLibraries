namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Xml;

    internal static class PeerMessageHelpers
    {
        public static string GetHeaderString(MessageHeaders headers, string name, string ns)
        {
            string str = null;
            int headerIndex = headers.FindHeader(name, ns);
            if (headerIndex >= 0)
            {
                using (XmlDictionaryReader reader = headers.GetReaderAtHeader(headerIndex))
                {
                    str = reader.ReadElementString();
                }
                headers.UnderstoodHeaders.Add(headers[headerIndex]);
            }
            return str;
        }

        public static ulong GetHeaderULong(MessageHeaders headers, int index)
        {
            ulong maxValue = ulong.MaxValue;
            if (index >= 0)
            {
                using (XmlDictionaryReader reader = headers.GetReaderAtHeader(index))
                {
                    maxValue = XmlConvert.ToUInt64(reader.ReadElementString());
                }
                headers.UnderstoodHeaders.Add(headers[index]);
            }
            return maxValue;
        }

        public static UniqueId GetHeaderUniqueId(MessageHeaders headers, string name, string ns)
        {
            UniqueId id = null;
            int headerIndex = headers.FindHeader(name, ns);
            if (headerIndex >= 0)
            {
                using (XmlDictionaryReader reader = headers.GetReaderAtHeader(headerIndex))
                {
                    id = reader.ReadElementContentAsUniqueId();
                }
                headers.UnderstoodHeaders.Add(headers[headerIndex]);
            }
            return id;
        }

        public static Uri GetHeaderUri(MessageHeaders headers, string name, string ns)
        {
            Uri uri = null;
            string uriString = GetHeaderString(headers, name, ns);
            if (uriString != null)
            {
                uri = new Uri(uriString);
            }
            return uri;
        }

        public delegate void CleanupCallback(IPeerNeighbor neighbor, PeerCloseReason reason, Exception exception);
    }
}

