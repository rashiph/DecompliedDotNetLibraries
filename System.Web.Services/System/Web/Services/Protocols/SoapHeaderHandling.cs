namespace System.Web.Services.Protocols
{
    using System;
    using System.Collections;
    using System.Security.Permissions;
    using System.Threading;
    using System.Web.Services;
    using System.Web.Services.Diagnostics;
    using System.Xml;
    using System.Xml.Serialization;

    [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
    public sealed class SoapHeaderHandling
    {
        private int currentThread;
        private string envelopeNS;
        private SoapHeaderCollection unknownHeaders;
        private SoapHeaderCollection unreferencedHeaders;

        public static void EnsureHeadersUnderstood(SoapHeaderCollection headers)
        {
            for (int i = 0; i < headers.Count; i++)
            {
                SoapHeader header = headers[i];
                if (header.MustUnderstand && !header.DidUnderstand)
                {
                    throw new SoapHeaderException(System.Web.Services.Res.GetString("WebCannotUnderstandHeader", new object[] { GetHeaderElementName(header) }), new XmlQualifiedName("MustUnderstand", "http://schemas.xmlsoap.org/soap/envelope/"));
                }
            }
        }

        private static int FindMapping(SoapHeaderMapping[] mappings, SoapHeader header, SoapHeaderDirection direction)
        {
            if ((mappings != null) && (mappings.Length != 0))
            {
                Type c = header.GetType();
                for (int i = 0; i < mappings.Length; i++)
                {
                    SoapHeaderMapping mapping = mappings[i];
                    if ((((mapping.direction & direction) != 0) && mapping.custom) && mapping.headerType.IsAssignableFrom(c))
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        private static string GetHeaderElementName(Type headerType)
        {
            return SoapReflector.CreateXmlImporter(null, false).ImportTypeMapping(headerType).XsdElementName;
        }

        private static string GetHeaderElementName(SoapHeader header)
        {
            if (header is SoapUnknownHeader)
            {
                return ((SoapUnknownHeader) header).Element.LocalName;
            }
            return GetHeaderElementName(header.GetType());
        }

        public static void GetHeaderMembers(SoapHeaderCollection headers, object target, SoapHeaderMapping[] mappings, SoapHeaderDirection direction, bool client)
        {
            if ((mappings != null) && (mappings.Length != 0))
            {
                for (int i = 0; i < mappings.Length; i++)
                {
                    SoapHeaderMapping mapping = mappings[i];
                    if ((mapping.direction & direction) != 0)
                    {
                        object obj2 = MemberHelper.GetValue(mapping.memberInfo, target);
                        if (mapping.repeats)
                        {
                            object[] objArray = (object[]) obj2;
                            if (objArray != null)
                            {
                                for (int j = 0; j < objArray.Length; j++)
                                {
                                    if (objArray[j] != null)
                                    {
                                        headers.Add((SoapHeader) objArray[j]);
                                    }
                                }
                            }
                        }
                        else if (obj2 != null)
                        {
                            headers.Add((SoapHeader) obj2);
                        }
                    }
                }
            }
        }

        private void OnUnknownElement(object sender, XmlElementEventArgs e)
        {
            if ((Thread.CurrentThread.GetHashCode() == this.currentThread) && (e.Element != null))
            {
                SoapUnknownHeader header = new SoapUnknownHeader {
                    Element = e.Element
                };
                this.unknownHeaders.Add(header);
            }
        }

        private void OnUnreferencedObject(object sender, UnreferencedObjectEventArgs e)
        {
            if (Thread.CurrentThread.GetHashCode() == this.currentThread)
            {
                object unreferencedObject = e.UnreferencedObject;
                if ((unreferencedObject != null) && typeof(SoapHeader).IsAssignableFrom(unreferencedObject.GetType()))
                {
                    this.unreferencedHeaders.Add((SoapHeader) unreferencedObject);
                }
            }
        }

        public string ReadHeaders(XmlReader reader, XmlSerializer serializer, SoapHeaderCollection headers, SoapHeaderMapping[] mappings, SoapHeaderDirection direction, string envelopeNS, string encodingStyle, bool checkRequiredHeaders)
        {
            string headerElementName = null;
            reader.MoveToContent();
            if (!reader.IsStartElement("Header", envelopeNS))
            {
                if ((checkRequiredHeaders && (mappings != null)) && (mappings.Length > 0))
                {
                    headerElementName = GetHeaderElementName(mappings[0].headerType);
                }
                return headerElementName;
            }
            if (reader.IsEmptyElement)
            {
                reader.Skip();
                return headerElementName;
            }
            this.unknownHeaders = new SoapHeaderCollection();
            this.unreferencedHeaders = new SoapHeaderCollection();
            this.currentThread = Thread.CurrentThread.GetHashCode();
            this.envelopeNS = envelopeNS;
            int depth = reader.Depth;
            reader.ReadStartElement();
            reader.MoveToContent();
            XmlDeserializationEvents events = new XmlDeserializationEvents {
                OnUnknownElement = new XmlElementEventHandler(this.OnUnknownElement),
                OnUnreferencedObject = new UnreferencedObjectEventHandler(this.OnUnreferencedObject)
            };
            TraceMethod caller = Tracing.On ? new TraceMethod(this, "ReadHeaders", new object[0]) : null;
            if (Tracing.On)
            {
                Tracing.Enter(Tracing.TraceId("TraceReadHeaders"), caller, new TraceMethod(serializer, "Deserialize", new object[] { reader, encodingStyle }));
            }
            object[] objArray = (object[]) serializer.Deserialize(reader, encodingStyle, events);
            if (Tracing.On)
            {
                Tracing.Exit(Tracing.TraceId("TraceReadHeaders"), caller);
            }
            for (int i = 0; i < objArray.Length; i++)
            {
                if (objArray[i] != null)
                {
                    SoapHeader header = (SoapHeader) objArray[i];
                    header.DidUnderstand = true;
                    headers.Add(header);
                }
                else if (checkRequiredHeaders && (headerElementName == null))
                {
                    headerElementName = GetHeaderElementName(mappings[i].headerType);
                }
            }
            this.currentThread = 0;
            this.envelopeNS = null;
            foreach (SoapHeader header2 in this.unreferencedHeaders)
            {
                headers.Add(header2);
            }
            this.unreferencedHeaders = null;
            foreach (SoapHeader header3 in this.unknownHeaders)
            {
                headers.Add(header3);
            }
            this.unknownHeaders = null;
            while ((depth < reader.Depth) && reader.Read())
            {
            }
            if (reader.NodeType == XmlNodeType.EndElement)
            {
                reader.Read();
            }
            return headerElementName;
        }

        public static void SetHeaderMembers(SoapHeaderCollection headers, object target, SoapHeaderMapping[] mappings, SoapHeaderDirection direction, bool client)
        {
            bool[] flagArray = new bool[headers.Count];
            if (mappings != null)
            {
                for (int j = 0; j < mappings.Length; j++)
                {
                    SoapHeaderMapping mapping = mappings[j];
                    if ((mapping.direction & direction) != 0)
                    {
                        if (mapping.repeats)
                        {
                            ArrayList list = new ArrayList();
                            for (int k = 0; k < headers.Count; k++)
                            {
                                SoapHeader header = headers[k];
                                if (!flagArray[k] && mapping.headerType.IsAssignableFrom(header.GetType()))
                                {
                                    list.Add(header);
                                    flagArray[k] = true;
                                }
                            }
                            MemberHelper.SetValue(mapping.memberInfo, target, list.ToArray(mapping.headerType));
                        }
                        else
                        {
                            bool flag = false;
                            for (int m = 0; m < headers.Count; m++)
                            {
                                SoapHeader header2 = headers[m];
                                if (!flagArray[m] && mapping.headerType.IsAssignableFrom(header2.GetType()))
                                {
                                    if (flag)
                                    {
                                        header2.DidUnderstand = false;
                                    }
                                    else
                                    {
                                        flag = true;
                                        MemberHelper.SetValue(mapping.memberInfo, target, header2);
                                        flagArray[m] = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            for (int i = 0; i < flagArray.Length; i++)
            {
                if (!flagArray[i])
                {
                    SoapHeader header3 = headers[i];
                    if (header3.MustUnderstand && !header3.DidUnderstand)
                    {
                        throw new SoapHeaderException(System.Web.Services.Res.GetString("WebCannotUnderstandHeader", new object[] { GetHeaderElementName(header3) }), new XmlQualifiedName("MustUnderstand", "http://schemas.xmlsoap.org/soap/envelope/"));
                    }
                }
            }
        }

        public static void WriteHeaders(XmlWriter writer, XmlSerializer serializer, SoapHeaderCollection headers, SoapHeaderMapping[] mappings, SoapHeaderDirection direction, bool isEncoded, string defaultNS, bool serviceDefaultIsEncoded, string envelopeNS)
        {
            if (headers.Count != 0)
            {
                SoapProtocolVersion version;
                string str;
                writer.WriteStartElement("Header", envelopeNS);
                if (envelopeNS == "http://www.w3.org/2003/05/soap-envelope")
                {
                    version = SoapProtocolVersion.Soap12;
                    str = "http://www.w3.org/2003/05/soap-encoding";
                }
                else
                {
                    version = SoapProtocolVersion.Soap11;
                    str = "http://schemas.xmlsoap.org/soap/encoding/";
                }
                int num = 0;
                ArrayList list = new ArrayList();
                SoapHeader[] o = new SoapHeader[mappings.Length];
                bool[] flagArray = new bool[o.Length];
                for (int i = 0; i < headers.Count; i++)
                {
                    SoapHeader header = headers[i];
                    if (header != null)
                    {
                        header.version = version;
                        if (header is SoapUnknownHeader)
                        {
                            list.Add(header);
                            num++;
                        }
                        else
                        {
                            int num3;
                            if (((num3 = FindMapping(mappings, header, direction)) >= 0) && !flagArray[num3])
                            {
                                o[num3] = header;
                                flagArray[num3] = true;
                            }
                            else
                            {
                                list.Add(header);
                            }
                        }
                    }
                }
                int num4 = list.Count - num;
                if (isEncoded && (num4 > 0))
                {
                    SoapHeader[] array = new SoapHeader[mappings.Length + num4];
                    o.CopyTo(array, 0);
                    int length = mappings.Length;
                    for (int k = 0; k < list.Count; k++)
                    {
                        if (!(list[k] is SoapUnknownHeader))
                        {
                            array[length++] = (SoapHeader) list[k];
                        }
                    }
                    o = array;
                }
                TraceMethod caller = Tracing.On ? new TraceMethod(typeof(SoapHeaderHandling), "WriteHeaders", new object[0]) : null;
                if (Tracing.On)
                {
                    object[] args = new object[5];
                    args[0] = writer;
                    args[1] = o;
                    args[3] = isEncoded ? str : null;
                    args[4] = "h_";
                    Tracing.Enter(Tracing.TraceId("TraceWriteHeaders"), caller, new TraceMethod(serializer, "Serialize", args));
                }
                serializer.Serialize(writer, o, null, isEncoded ? str : null, "h_");
                if (Tracing.On)
                {
                    Tracing.Exit(Tracing.TraceId("TraceWriteHeaders"), caller);
                }
                foreach (SoapHeader header2 in list)
                {
                    if (header2 is SoapUnknownHeader)
                    {
                        SoapUnknownHeader header3 = (SoapUnknownHeader) header2;
                        if (header3.Element != null)
                        {
                            header3.Element.WriteTo(writer);
                        }
                    }
                    else if (!isEncoded)
                    {
                        string literalNamespace = SoapReflector.GetLiteralNamespace(defaultNS, serviceDefaultIsEncoded);
                        XmlSerializer target = new XmlSerializer(header2.GetType(), literalNamespace);
                        if (Tracing.On)
                        {
                            Tracing.Enter(Tracing.TraceId("TraceWriteHeaders"), caller, new TraceMethod(target, "Serialize", new object[] { writer, header2 }));
                        }
                        target.Serialize(writer, header2);
                        if (Tracing.On)
                        {
                            Tracing.Exit(Tracing.TraceId("TraceWriteHeaders"), caller);
                        }
                    }
                }
                for (int j = 0; j < headers.Count; j++)
                {
                    SoapHeader header4 = headers[j];
                    if (header4 != null)
                    {
                        header4.version = SoapProtocolVersion.Default;
                    }
                }
                writer.WriteEndElement();
                writer.Flush();
            }
        }

        public static void WriteUnknownHeaders(XmlWriter writer, SoapHeaderCollection headers, string envelopeNS)
        {
            bool flag = true;
            foreach (SoapHeader header in headers)
            {
                SoapUnknownHeader header2 = header as SoapUnknownHeader;
                if (header2 != null)
                {
                    if (flag)
                    {
                        writer.WriteStartElement("Header", envelopeNS);
                        flag = false;
                    }
                    if (header2.Element != null)
                    {
                        header2.Element.WriteTo(writer);
                    }
                }
            }
            if (!flag)
            {
                writer.WriteEndElement();
            }
        }
    }
}

