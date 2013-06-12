namespace System.Xml
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Xml.Schema;
    using System.Xml.Xsl.Runtime;

    internal sealed class XmlEventCache : XmlRawWriter
    {
        private string baseUri;
        private bool hasRootNode;
        private const int InitialPageSize = 0x20;
        private XmlEvent[] pageCurr;
        private List<XmlEvent[]> pages;
        private int pageSize;
        private StringConcat singleText;

        public XmlEventCache(string baseUri, bool hasRootNode)
        {
            this.baseUri = baseUri;
            this.hasRootNode = hasRootNode;
        }

        private void AddEvent(XmlEventType eventType)
        {
            int index = this.NewEvent();
            this.pageCurr[index].InitEvent(eventType);
        }

        private void AddEvent(XmlEventType eventType, object o)
        {
            int index = this.NewEvent();
            this.pageCurr[index].InitEvent(eventType, o);
        }

        private void AddEvent(XmlEventType eventType, string s1)
        {
            int index = this.NewEvent();
            this.pageCurr[index].InitEvent(eventType, s1);
        }

        private void AddEvent(XmlEventType eventType, string s1, string s2)
        {
            int index = this.NewEvent();
            this.pageCurr[index].InitEvent(eventType, s1, s2);
        }

        private void AddEvent(XmlEventType eventType, string s1, string s2, string s3)
        {
            int index = this.NewEvent();
            this.pageCurr[index].InitEvent(eventType, s1, s2, s3);
        }

        private void AddEvent(XmlEventType eventType, string s1, string s2, string s3, object o)
        {
            int index = this.NewEvent();
            this.pageCurr[index].InitEvent(eventType, s1, s2, s3, o);
        }

        public override void Close()
        {
            this.AddEvent(XmlEventType.Close);
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    this.AddEvent(XmlEventType.Dispose);
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        public void EndEvents()
        {
            if (this.singleText.Count == 0)
            {
                this.AddEvent(XmlEventType.Unknown);
            }
        }

        public string EventsToString()
        {
            if (this.singleText.Count != 0)
            {
                return this.singleText.GetResult();
            }
            StringBuilder builder = new StringBuilder();
            bool flag = false;
            for (int i = 0; i < this.pages.Count; i++)
            {
                XmlEvent[] eventArray = this.pages[i];
                for (int j = 0; j < eventArray.Length; j++)
                {
                    switch (eventArray[j].EventType)
                    {
                        case XmlEventType.Unknown:
                            return builder.ToString();

                        case XmlEventType.StartAttr:
                            flag = true;
                            break;

                        case XmlEventType.EndAttr:
                            flag = false;
                            break;

                        case XmlEventType.CData:
                        case XmlEventType.Whitespace:
                        case XmlEventType.String:
                        case XmlEventType.Raw:
                            if (!flag)
                            {
                                builder.Append(eventArray[j].String1);
                            }
                            break;
                    }
                }
            }
            return string.Empty;
        }

        public void EventsToWriter(XmlWriter writer)
        {
            if (this.singleText.Count != 0)
            {
                writer.WriteString(this.singleText.GetResult());
            }
            else
            {
                XmlRawWriter writer2 = writer as XmlRawWriter;
                for (int i = 0; i < this.pages.Count; i++)
                {
                    XmlEvent[] eventArray = this.pages[i];
                    for (int j = 0; j < eventArray.Length; j++)
                    {
                        byte[] buffer;
                        switch (eventArray[j].EventType)
                        {
                            case XmlEventType.Unknown:
                                return;

                            case XmlEventType.DocType:
                            {
                                writer.WriteDocType(eventArray[j].String1, eventArray[j].String2, eventArray[j].String3, (string) eventArray[j].Object);
                                continue;
                            }
                            case XmlEventType.StartElem:
                            {
                                writer.WriteStartElement(eventArray[j].String1, eventArray[j].String2, eventArray[j].String3);
                                continue;
                            }
                            case XmlEventType.StartAttr:
                            {
                                writer.WriteStartAttribute(eventArray[j].String1, eventArray[j].String2, eventArray[j].String3);
                                continue;
                            }
                            case XmlEventType.EndAttr:
                            {
                                writer.WriteEndAttribute();
                                continue;
                            }
                            case XmlEventType.CData:
                            {
                                writer.WriteCData(eventArray[j].String1);
                                continue;
                            }
                            case XmlEventType.Comment:
                            {
                                writer.WriteComment(eventArray[j].String1);
                                continue;
                            }
                            case XmlEventType.PI:
                            {
                                writer.WriteProcessingInstruction(eventArray[j].String1, eventArray[j].String2);
                                continue;
                            }
                            case XmlEventType.Whitespace:
                            {
                                writer.WriteWhitespace(eventArray[j].String1);
                                continue;
                            }
                            case XmlEventType.String:
                            {
                                writer.WriteString(eventArray[j].String1);
                                continue;
                            }
                            case XmlEventType.Raw:
                            {
                                writer.WriteRaw(eventArray[j].String1);
                                continue;
                            }
                            case XmlEventType.EntRef:
                            {
                                writer.WriteEntityRef(eventArray[j].String1);
                                continue;
                            }
                            case XmlEventType.CharEnt:
                            {
                                writer.WriteCharEntity((char) eventArray[j].Object);
                                continue;
                            }
                            case XmlEventType.SurrCharEnt:
                            {
                                char[] chArray = (char[]) eventArray[j].Object;
                                writer.WriteSurrogateCharEntity(chArray[0], chArray[1]);
                                continue;
                            }
                            case XmlEventType.Base64:
                            {
                                buffer = (byte[]) eventArray[j].Object;
                                writer.WriteBase64(buffer, 0, buffer.Length);
                                continue;
                            }
                            case XmlEventType.BinHex:
                            {
                                buffer = (byte[]) eventArray[j].Object;
                                writer.WriteBinHex(buffer, 0, buffer.Length);
                                continue;
                            }
                            case XmlEventType.XmlDecl1:
                            {
                                if (writer2 != null)
                                {
                                    writer2.WriteXmlDeclaration((XmlStandalone) eventArray[j].Object);
                                }
                                continue;
                            }
                            case XmlEventType.XmlDecl2:
                            {
                                if (writer2 != null)
                                {
                                    writer2.WriteXmlDeclaration(eventArray[j].String1);
                                }
                                continue;
                            }
                            case XmlEventType.StartContent:
                            {
                                if (writer2 != null)
                                {
                                    writer2.StartElementContent();
                                }
                                continue;
                            }
                            case XmlEventType.EndElem:
                            {
                                if (writer2 == null)
                                {
                                    break;
                                }
                                writer2.WriteEndElement(eventArray[j].String1, eventArray[j].String2, eventArray[j].String3);
                                continue;
                            }
                            case XmlEventType.FullEndElem:
                            {
                                if (writer2 == null)
                                {
                                    goto Label_0367;
                                }
                                writer2.WriteFullEndElement(eventArray[j].String1, eventArray[j].String2, eventArray[j].String3);
                                continue;
                            }
                            case XmlEventType.Nmsp:
                            {
                                if (writer2 == null)
                                {
                                    goto Label_0394;
                                }
                                writer2.WriteNamespaceDeclaration(eventArray[j].String1, eventArray[j].String2);
                                continue;
                            }
                            case XmlEventType.EndBase64:
                            {
                                if (writer2 != null)
                                {
                                    writer2.WriteEndBase64();
                                }
                                continue;
                            }
                            case XmlEventType.Close:
                            {
                                writer.Close();
                                continue;
                            }
                            case XmlEventType.Flush:
                            {
                                writer.Flush();
                                continue;
                            }
                            case XmlEventType.Dispose:
                            {
                                writer.Dispose();
                                continue;
                            }
                            default:
                            {
                                continue;
                            }
                        }
                        writer.WriteEndElement();
                        continue;
                    Label_0367:
                        writer.WriteFullEndElement();
                        continue;
                    Label_0394:
                        writer.WriteAttributeString("xmlns", eventArray[j].String1, "http://www.w3.org/2000/xmlns/", eventArray[j].String2);
                    }
                }
            }
        }

        public override void Flush()
        {
            this.AddEvent(XmlEventType.Flush);
        }

        private int NewEvent()
        {
            if (this.pages == null)
            {
                this.pages = new List<XmlEvent[]>();
                this.pageCurr = new XmlEvent[0x20];
                this.pages.Add(this.pageCurr);
                if (this.singleText.Count != 0)
                {
                    this.pageCurr[0].InitEvent(XmlEventType.String, this.singleText.GetResult());
                    this.pageSize++;
                    this.singleText.Clear();
                }
            }
            else if (this.pageSize >= this.pageCurr.Length)
            {
                this.pageCurr = new XmlEvent[this.pageSize * 2];
                this.pages.Add(this.pageCurr);
                this.pageSize = 0;
            }
            return this.pageSize++;
        }

        internal override void StartElementContent()
        {
            this.AddEvent(XmlEventType.StartContent);
        }

        private static byte[] ToBytes(byte[] buffer, int index, int count)
        {
            if ((index == 0) && (count == buffer.Length))
            {
                return buffer;
            }
            if ((buffer.Length - index) > count)
            {
                count = buffer.Length - index;
            }
            byte[] destinationArray = new byte[count];
            Array.Copy(buffer, index, destinationArray, 0, count);
            return destinationArray;
        }

        public override void WriteBase64(byte[] buffer, int index, int count)
        {
            this.AddEvent(XmlEventType.Base64, ToBytes(buffer, index, count));
        }

        public override void WriteBinHex(byte[] buffer, int index, int count)
        {
            this.AddEvent(XmlEventType.BinHex, ToBytes(buffer, index, count));
        }

        public override void WriteCData(string text)
        {
            this.AddEvent(XmlEventType.CData, text);
        }

        public override void WriteCharEntity(char ch)
        {
            this.AddEvent(XmlEventType.CharEnt, ch);
        }

        public override void WriteChars(char[] buffer, int index, int count)
        {
            this.WriteString(new string(buffer, index, count));
        }

        public override void WriteComment(string text)
        {
            this.AddEvent(XmlEventType.Comment, text);
        }

        public override void WriteDocType(string name, string pubid, string sysid, string subset)
        {
            this.AddEvent(XmlEventType.DocType, name, pubid, sysid, subset);
        }

        public override void WriteEndAttribute()
        {
            this.AddEvent(XmlEventType.EndAttr);
        }

        internal override void WriteEndBase64()
        {
            this.AddEvent(XmlEventType.EndBase64);
        }

        internal override void WriteEndElement(string prefix, string localName, string ns)
        {
            this.AddEvent(XmlEventType.EndElem, prefix, localName, ns);
        }

        public override void WriteEntityRef(string name)
        {
            this.AddEvent(XmlEventType.EntRef, name);
        }

        internal override void WriteFullEndElement(string prefix, string localName, string ns)
        {
            this.AddEvent(XmlEventType.FullEndElem, prefix, localName, ns);
        }

        internal override void WriteNamespaceDeclaration(string prefix, string ns)
        {
            this.AddEvent(XmlEventType.Nmsp, prefix, ns);
        }

        public override void WriteProcessingInstruction(string name, string text)
        {
            this.AddEvent(XmlEventType.PI, name, text);
        }

        public override void WriteRaw(string data)
        {
            this.AddEvent(XmlEventType.Raw, data);
        }

        public override void WriteRaw(char[] buffer, int index, int count)
        {
            this.WriteRaw(new string(buffer, index, count));
        }

        public override void WriteStartAttribute(string prefix, string localName, string ns)
        {
            this.AddEvent(XmlEventType.StartAttr, prefix, localName, ns);
        }

        public override void WriteStartElement(string prefix, string localName, string ns)
        {
            this.AddEvent(XmlEventType.StartElem, prefix, localName, ns);
        }

        public override void WriteString(string text)
        {
            if (this.pages == null)
            {
                this.singleText.ConcatNoDelimiter(text);
            }
            else
            {
                this.AddEvent(XmlEventType.String, text);
            }
        }

        public override void WriteSurrogateCharEntity(char lowChar, char highChar)
        {
            char[] o = new char[] { lowChar, highChar };
            this.AddEvent(XmlEventType.SurrCharEnt, o);
        }

        public override void WriteValue(object value)
        {
            this.WriteString(XmlUntypedConverter.Untyped.ToString(value, base.resolver));
        }

        public override void WriteValue(string value)
        {
            this.WriteString(value);
        }

        public override void WriteWhitespace(string ws)
        {
            this.AddEvent(XmlEventType.Whitespace, ws);
        }

        internal override void WriteXmlDeclaration(string xmldecl)
        {
            this.AddEvent(XmlEventType.XmlDecl2, xmldecl);
        }

        internal override void WriteXmlDeclaration(XmlStandalone standalone)
        {
            this.AddEvent(XmlEventType.XmlDecl1, standalone);
        }

        public string BaseUri
        {
            get
            {
                return this.baseUri;
            }
        }

        public bool HasRootNode
        {
            get
            {
                return this.hasRootNode;
            }
        }

        public override XmlWriterSettings Settings
        {
            get
            {
                return null;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct XmlEvent
        {
            private XmlEventCache.XmlEventType eventType;
            private string s1;
            private string s2;
            private string s3;
            private object o;
            public void InitEvent(XmlEventCache.XmlEventType eventType)
            {
                this.eventType = eventType;
            }

            public void InitEvent(XmlEventCache.XmlEventType eventType, string s1)
            {
                this.eventType = eventType;
                this.s1 = s1;
            }

            public void InitEvent(XmlEventCache.XmlEventType eventType, string s1, string s2)
            {
                this.eventType = eventType;
                this.s1 = s1;
                this.s2 = s2;
            }

            public void InitEvent(XmlEventCache.XmlEventType eventType, string s1, string s2, string s3)
            {
                this.eventType = eventType;
                this.s1 = s1;
                this.s2 = s2;
                this.s3 = s3;
            }

            public void InitEvent(XmlEventCache.XmlEventType eventType, string s1, string s2, string s3, object o)
            {
                this.eventType = eventType;
                this.s1 = s1;
                this.s2 = s2;
                this.s3 = s3;
                this.o = o;
            }

            public void InitEvent(XmlEventCache.XmlEventType eventType, object o)
            {
                this.eventType = eventType;
                this.o = o;
            }

            public XmlEventCache.XmlEventType EventType
            {
                get
                {
                    return this.eventType;
                }
            }
            public string String1
            {
                get
                {
                    return this.s1;
                }
            }
            public string String2
            {
                get
                {
                    return this.s2;
                }
            }
            public string String3
            {
                get
                {
                    return this.s3;
                }
            }
            public object Object
            {
                get
                {
                    return this.o;
                }
            }
        }

        private enum XmlEventType
        {
            Unknown,
            DocType,
            StartElem,
            StartAttr,
            EndAttr,
            CData,
            Comment,
            PI,
            Whitespace,
            String,
            Raw,
            EntRef,
            CharEnt,
            SurrCharEnt,
            Base64,
            BinHex,
            XmlDecl1,
            XmlDecl2,
            StartContent,
            EndElem,
            FullEndElem,
            Nmsp,
            EndBase64,
            Close,
            Flush,
            Dispose
        }
    }
}

