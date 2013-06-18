namespace Microsoft.Build.Tasks.Deployment.ManifestUtilities
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Xml;

    internal static class ManifestFormatter
    {
        public static Stream Format(Stream input)
        {
            int tickCount = Environment.TickCount;
            XmlTextReader r = new XmlTextReader(input) {
                WhitespaceHandling = WhitespaceHandling.None
            };
            XmlNamespaceManager namespaceManager = XmlNamespaces.GetNamespaceManager(r.NameTable);
            MemoryStream w = new MemoryStream();
            XmlTextWriter writer = new XmlTextWriter(w, Encoding.UTF8) {
                Formatting = Formatting.Indented,
                Indentation = 2
            };
            writer.WriteStartDocument();
            while (r.Read())
            {
                string str;
                int num2;
                switch (r.NodeType)
                {
                    case XmlNodeType.Element:
                        writer.WriteStartElement(r.Prefix, r.LocalName, r.NamespaceURI);
                        if (!r.HasAttributes)
                        {
                            goto Label_0162;
                        }
                        str = XmlUtil.GetQName(r, namespaceManager);
                        num2 = 0;
                        goto Label_014E;

                    case XmlNodeType.Attribute:
                    {
                        continue;
                    }
                    case XmlNodeType.Text:
                    {
                        writer.WriteString(r.Value);
                        continue;
                    }
                    case XmlNodeType.CDATA:
                    {
                        writer.WriteCData(r.Value);
                        continue;
                    }
                    case XmlNodeType.Comment:
                    {
                        writer.WriteComment(r.Value);
                        continue;
                    }
                    case XmlNodeType.EndElement:
                    {
                        writer.WriteEndElement();
                        continue;
                    }
                    default:
                    {
                        continue;
                    }
                }
            Label_00BB:
                r.MoveToAttribute(num2);
                string qName = XmlUtil.GetQName(r, namespaceManager);
                string str3 = str + "/@" + qName;
                if (((!str3.Equals("asmv1:assemblyIdentity/@language", StringComparison.Ordinal) && !str3.Equals("asmv2:assemblyIdentity/@language", StringComparison.Ordinal)) || !string.Equals(r.Value, "*", StringComparison.Ordinal)) && (!string.IsNullOrEmpty(r.Value) || (Array.BinarySearch<string>(XPaths.emptyAttributeList, str3) < 0)))
                {
                    writer.WriteAttributeString(r.Prefix, r.LocalName, r.NamespaceURI, r.Value);
                }
                num2++;
            Label_014E:
                if (num2 < r.AttributeCount)
                {
                    goto Label_00BB;
                }
                r.MoveToElement();
            Label_0162:
                if (r.IsEmptyElement)
                {
                    writer.WriteEndElement();
                }
            }
            writer.WriteEndDocument();
            writer.Flush();
            w.Position = 0L;
            Util.WriteLog(string.Format(CultureInfo.CurrentCulture, "ManifestWriter.Format t={0}", new object[] { Environment.TickCount - tickCount }));
            return w;
        }
    }
}

