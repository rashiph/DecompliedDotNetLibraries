namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Text;
    using System.Xml;

    internal class EndpointAddressProcessor
    {
        private StringBuilder builder = new StringBuilder();
        internal const string FactoryTypeLN = "FactoryType";
        internal const string ItemTypeLN = "ItemType";
        internal EndpointAddressProcessor next;
        internal static readonly QNameKeyComparer QNameComparer = new QNameKeyComparer();
        private byte[] resultData;
        internal const string SerNs = "http://schemas.microsoft.com/2003/10/Serialization/";
        internal const string TypeLN = "type";
        internal static readonly string XsiNs = "http://www.w3.org/2001/XMLSchema-instance";

        internal EndpointAddressProcessor(int length)
        {
            this.resultData = new byte[length];
        }

        private static void AppendString(StringBuilder builder, string s)
        {
            builder.Append(s);
            builder.Append("^");
            builder.Append(s.Length.ToString(CultureInfo.InvariantCulture));
        }

        internal void Clear(int length)
        {
            if (this.resultData.Length == length)
            {
                Array.Clear(this.resultData, 0, this.resultData.Length);
            }
            else
            {
                this.resultData = new byte[length];
            }
        }

        private static void CompleteValue(StringBuilder builder, int startLength)
        {
            if (startLength >= 0)
            {
                int num = builder.Length - startLength;
                builder.Append("^");
                builder.Append(num.ToString(CultureInfo.InvariantCulture));
            }
        }

        internal static string GetComparableForm(StringBuilder builder, XmlReader reader)
        {
            List<Attr> list = new List<Attr>();
            int startLength = -1;
            while (!reader.EOF)
            {
                switch (reader.MoveToContent())
                {
                    case XmlNodeType.Element:
                        break;

                    case XmlNodeType.Text:
                    case XmlNodeType.SignificantWhitespace:
                        if (startLength < 0)
                        {
                            startLength = builder.Length;
                        }
                        builder.Append(reader.Value);
                        goto Label_0325;

                    case XmlNodeType.CDATA:
                        CompleteValue(builder, startLength);
                        startLength = -1;
                        builder.Append("<![CDATA[");
                        AppendString(builder, reader.Value);
                        builder.Append("]]>");
                        goto Label_0325;

                    case XmlNodeType.EndElement:
                        CompleteValue(builder, startLength);
                        startLength = -1;
                        builder.Append("</>");
                        goto Label_0325;

                    default:
                        goto Label_0325;
                }
                CompleteValue(builder, startLength);
                startLength = -1;
                builder.Append("<");
                AppendString(builder, reader.LocalName);
                builder.Append(":");
                AppendString(builder, reader.NamespaceURI);
                builder.Append(" ");
                list.Clear();
                if (!reader.MoveToFirstAttribute())
                {
                    goto Label_0223;
                }
            Label_009D:
                if (((reader.Prefix != "xmlns") && (reader.Name != "xmlns")) && ((reader.LocalName != "IsReferenceParameter") || (reader.NamespaceURI != "http://www.w3.org/2005/08/addressing")))
                {
                    string qname = reader.Value;
                    if (((reader.LocalName == "type") && (reader.NamespaceURI == XsiNs)) || ((reader.NamespaceURI == "http://schemas.microsoft.com/2003/10/Serialization/") && ((reader.LocalName == "ItemType") || (reader.LocalName == "FactoryType"))))
                    {
                        string str2;
                        string str3;
                        XmlUtil.ParseQName(reader, qname, out str2, out str3);
                        qname = str2 + "^" + str2.Length.ToString(CultureInfo.InvariantCulture) + ":" + str3 + "^" + str3.Length.ToString(CultureInfo.InvariantCulture);
                    }
                    else if ((reader.LocalName == XD.UtilityDictionary.IdAttribute.Value) && (reader.NamespaceURI == XD.UtilityDictionary.Namespace.Value))
                    {
                        goto Label_0218;
                    }
                    list.Add(new Attr(reader.LocalName, reader.NamespaceURI, qname));
                }
            Label_0218:
                if (reader.MoveToNextAttribute())
                {
                    goto Label_009D;
                }
            Label_0223:
                reader.MoveToElement();
                if (list.Count > 0)
                {
                    list.Sort();
                    for (int i = 0; i < list.Count; i++)
                    {
                        Attr attr = list[i];
                        AppendString(builder, attr.local);
                        builder.Append(":");
                        AppendString(builder, attr.ns);
                        builder.Append("=\"");
                        AppendString(builder, attr.val);
                        builder.Append("\" ");
                    }
                }
                if (reader.IsEmptyElement)
                {
                    builder.Append("></>");
                }
                else
                {
                    builder.Append(">");
                }
            Label_0325:
                reader.Read();
            }
            return builder.ToString();
        }

        internal void ProcessHeaders(Message msg, Dictionary<QName, int> qnameLookup, Dictionary<string, HeaderBit[]> headerLookup)
        {
            MessageHeaders headers = msg.Headers;
            for (int i = 0; i < headers.Count; i++)
            {
                QName name;
                name.name = headers[i].Name;
                name.ns = headers[i].Namespace;
                if (((headers.MessageVersion.Addressing != AddressingVersion.WSAddressing10) || headers[i].IsReferenceParameter) && qnameLookup.ContainsKey(name))
                {
                    HeaderBit[] bitArray;
                    this.builder.Remove(0, this.builder.Length);
                    XmlReader reader = headers.GetReaderAtHeader(i).ReadSubtree();
                    reader.Read();
                    string comparableForm = GetComparableForm(this.builder, reader);
                    if (headerLookup.TryGetValue(comparableForm, out bitArray))
                    {
                        this.SetBit(bitArray);
                    }
                }
            }
        }

        internal void SetBit(HeaderBit[] bits)
        {
            if (bits.Length == 1)
            {
                this.resultData[bits[0].index] = (byte) (this.resultData[bits[0].index] | bits[0].mask);
            }
            else
            {
                byte[] resultData = this.resultData;
                for (int i = 0; i < bits.Length; i++)
                {
                    if ((resultData[bits[i].index] & bits[i].mask) == 0)
                    {
                        resultData[bits[i].index] = (byte) (resultData[bits[i].index] | bits[i].mask);
                        return;
                    }
                }
            }
        }

        internal bool TestExact(byte[] exact)
        {
            byte[] resultData = this.resultData;
            for (int i = 0; i < exact.Length; i++)
            {
                if (resultData[i] != exact[i])
                {
                    return false;
                }
            }
            return true;
        }

        internal bool TestMask(byte[] mask)
        {
            if (mask != null)
            {
                byte[] resultData = this.resultData;
                for (int i = 0; i < mask.Length; i++)
                {
                    if ((resultData[i] & mask[i]) != mask[i])
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        internal EndpointAddressProcessor Next
        {
            get
            {
                return this.next;
            }
            set
            {
                this.next = value;
            }
        }

        private class Attr : IComparable<EndpointAddressProcessor.Attr>
        {
            private string key;
            internal string local;
            internal string ns;
            internal string val;

            internal Attr(string l, string ns, string v)
            {
                this.local = l;
                this.ns = ns;
                this.val = v;
                this.key = ns + ":" + l;
            }

            public int CompareTo(EndpointAddressProcessor.Attr a)
            {
                return string.Compare(this.key, a.key, StringComparison.Ordinal);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct HeaderBit
        {
            internal int index;
            internal byte mask;
            internal HeaderBit(int bitNum)
            {
                this.index = bitNum / 8;
                this.mask = (byte) (((int) 1) << (bitNum % 8));
            }

            internal void AddToMask(ref byte[] mask)
            {
                if (mask == null)
                {
                    mask = new byte[this.index + 1];
                }
                else if (mask.Length <= this.index)
                {
                    Array.Resize<byte>(ref mask, this.index + 1);
                }
                mask[this.index] = (byte) (mask[this.index] | this.mask);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct QName
        {
            internal string name;
            internal string ns;
        }

        internal class QNameKeyComparer : IComparer<EndpointAddressProcessor.QName>, IEqualityComparer<EndpointAddressProcessor.QName>
        {
            internal QNameKeyComparer()
            {
            }

            public int Compare(EndpointAddressProcessor.QName x, EndpointAddressProcessor.QName y)
            {
                int num = string.CompareOrdinal(x.name, y.name);
                if (num != 0)
                {
                    return num;
                }
                return string.CompareOrdinal(x.ns, y.ns);
            }

            public bool Equals(EndpointAddressProcessor.QName x, EndpointAddressProcessor.QName y)
            {
                if (string.CompareOrdinal(x.name, y.name) != 0)
                {
                    return false;
                }
                return (string.CompareOrdinal(x.ns, y.ns) == 0);
            }

            public int GetHashCode(EndpointAddressProcessor.QName obj)
            {
                return (obj.name.GetHashCode() ^ obj.ns.GetHashCode());
            }
        }
    }
}

