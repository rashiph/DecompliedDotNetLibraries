namespace System.Xml
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Text;

    public abstract class XmlDictionaryWriter : XmlWriter
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected XmlDictionaryWriter()
        {
        }

        private void CheckArray(Array array, int offset, int count)
        {
            if (array == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("array"));
            }
            if (offset < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", System.Runtime.Serialization.SR.GetString("ValueMustBeNonNegative")));
            }
            if (offset > array.Length)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", System.Runtime.Serialization.SR.GetString("OffsetExceedsBufferSize", new object[] { array.Length })));
            }
            if (count < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", System.Runtime.Serialization.SR.GetString("ValueMustBeNonNegative")));
            }
            if (count > (array.Length - offset))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", System.Runtime.Serialization.SR.GetString("SizeExceedsRemainingBufferSpace", new object[] { array.Length - offset })));
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static XmlDictionaryWriter CreateBinaryWriter(Stream stream)
        {
            return CreateBinaryWriter(stream, null);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static XmlDictionaryWriter CreateBinaryWriter(Stream stream, IXmlDictionary dictionary)
        {
            return CreateBinaryWriter(stream, dictionary, null);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static XmlDictionaryWriter CreateBinaryWriter(Stream stream, IXmlDictionary dictionary, XmlBinaryWriterSession session)
        {
            return CreateBinaryWriter(stream, dictionary, session, true);
        }

        public static XmlDictionaryWriter CreateBinaryWriter(Stream stream, IXmlDictionary dictionary, XmlBinaryWriterSession session, bool ownsStream)
        {
            XmlBinaryWriter writer = new XmlBinaryWriter();
            writer.SetOutput(stream, dictionary, session, ownsStream);
            return writer;
        }

        public static XmlDictionaryWriter CreateDictionaryWriter(XmlWriter writer)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }
            XmlDictionaryWriter writer2 = writer as XmlDictionaryWriter;
            if (writer2 == null)
            {
                writer2 = new XmlWrappedWriter(writer);
            }
            return writer2;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static XmlDictionaryWriter CreateMtomWriter(Stream stream, Encoding encoding, int maxSizeInBytes, string startInfo)
        {
            return CreateMtomWriter(stream, encoding, maxSizeInBytes, startInfo, null, null, true, true);
        }

        public static XmlDictionaryWriter CreateMtomWriter(Stream stream, Encoding encoding, int maxSizeInBytes, string startInfo, string boundary, string startUri, bool writeMessageHeaders, bool ownsStream)
        {
            XmlMtomWriter writer = new XmlMtomWriter();
            writer.SetOutput(stream, encoding, maxSizeInBytes, startInfo, boundary, startUri, writeMessageHeaders, ownsStream);
            return writer;
        }

        public static XmlDictionaryWriter CreateTextWriter(Stream stream)
        {
            return CreateTextWriter(stream, Encoding.UTF8, true);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static XmlDictionaryWriter CreateTextWriter(Stream stream, Encoding encoding)
        {
            return CreateTextWriter(stream, encoding, true);
        }

        public static XmlDictionaryWriter CreateTextWriter(Stream stream, Encoding encoding, bool ownsStream)
        {
            XmlUTF8TextWriter writer = new XmlUTF8TextWriter();
            writer.SetOutput(stream, encoding, ownsStream);
            return writer;
        }

        public virtual void EndCanonicalization()
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
        }

        public virtual void StartCanonicalization(Stream stream, bool includeComments, string[] inclusivePrefixes)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
        }

        public virtual void WriteArray(string prefix, string localName, string namespaceUri, bool[] array, int offset, int count)
        {
            this.CheckArray(array, offset, count);
            for (int i = 0; i < count; i++)
            {
                this.WriteStartElement(prefix, localName, namespaceUri);
                this.WriteValue(array[offset + i]);
                this.WriteEndElement();
            }
        }

        public virtual void WriteArray(string prefix, string localName, string namespaceUri, DateTime[] array, int offset, int count)
        {
            this.CheckArray(array, offset, count);
            for (int i = 0; i < count; i++)
            {
                this.WriteStartElement(prefix, localName, namespaceUri);
                this.WriteValue(array[offset + i]);
                this.WriteEndElement();
            }
        }

        public virtual void WriteArray(string prefix, string localName, string namespaceUri, decimal[] array, int offset, int count)
        {
            this.CheckArray(array, offset, count);
            for (int i = 0; i < count; i++)
            {
                this.WriteStartElement(prefix, localName, namespaceUri);
                this.WriteValue(array[offset + i]);
                this.WriteEndElement();
            }
        }

        public virtual void WriteArray(string prefix, string localName, string namespaceUri, double[] array, int offset, int count)
        {
            this.CheckArray(array, offset, count);
            for (int i = 0; i < count; i++)
            {
                this.WriteStartElement(prefix, localName, namespaceUri);
                this.WriteValue(array[offset + i]);
                this.WriteEndElement();
            }
        }

        public virtual void WriteArray(string prefix, string localName, string namespaceUri, Guid[] array, int offset, int count)
        {
            this.CheckArray(array, offset, count);
            for (int i = 0; i < count; i++)
            {
                this.WriteStartElement(prefix, localName, namespaceUri);
                this.WriteValue(array[offset + i]);
                this.WriteEndElement();
            }
        }

        public virtual void WriteArray(string prefix, string localName, string namespaceUri, short[] array, int offset, int count)
        {
            this.CheckArray(array, offset, count);
            for (int i = 0; i < count; i++)
            {
                this.WriteStartElement(prefix, localName, namespaceUri);
                this.WriteValue((int) array[offset + i]);
                this.WriteEndElement();
            }
        }

        public virtual void WriteArray(string prefix, string localName, string namespaceUri, int[] array, int offset, int count)
        {
            this.CheckArray(array, offset, count);
            for (int i = 0; i < count; i++)
            {
                this.WriteStartElement(prefix, localName, namespaceUri);
                this.WriteValue(array[offset + i]);
                this.WriteEndElement();
            }
        }

        public virtual void WriteArray(string prefix, string localName, string namespaceUri, long[] array, int offset, int count)
        {
            this.CheckArray(array, offset, count);
            for (int i = 0; i < count; i++)
            {
                this.WriteStartElement(prefix, localName, namespaceUri);
                this.WriteValue(array[offset + i]);
                this.WriteEndElement();
            }
        }

        public virtual void WriteArray(string prefix, string localName, string namespaceUri, float[] array, int offset, int count)
        {
            this.CheckArray(array, offset, count);
            for (int i = 0; i < count; i++)
            {
                this.WriteStartElement(prefix, localName, namespaceUri);
                this.WriteValue(array[offset + i]);
                this.WriteEndElement();
            }
        }

        public virtual void WriteArray(string prefix, string localName, string namespaceUri, TimeSpan[] array, int offset, int count)
        {
            this.CheckArray(array, offset, count);
            for (int i = 0; i < count; i++)
            {
                this.WriteStartElement(prefix, localName, namespaceUri);
                this.WriteValue(array[offset + i]);
                this.WriteEndElement();
            }
        }

        public virtual void WriteArray(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, bool[] array, int offset, int count)
        {
            this.WriteArray(prefix, XmlDictionaryString.GetString(localName), XmlDictionaryString.GetString(namespaceUri), array, offset, count);
        }

        public virtual void WriteArray(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, DateTime[] array, int offset, int count)
        {
            this.WriteArray(prefix, XmlDictionaryString.GetString(localName), XmlDictionaryString.GetString(namespaceUri), array, offset, count);
        }

        public virtual void WriteArray(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, decimal[] array, int offset, int count)
        {
            this.WriteArray(prefix, XmlDictionaryString.GetString(localName), XmlDictionaryString.GetString(namespaceUri), array, offset, count);
        }

        public virtual void WriteArray(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, double[] array, int offset, int count)
        {
            this.WriteArray(prefix, XmlDictionaryString.GetString(localName), XmlDictionaryString.GetString(namespaceUri), array, offset, count);
        }

        public virtual void WriteArray(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, Guid[] array, int offset, int count)
        {
            this.WriteArray(prefix, XmlDictionaryString.GetString(localName), XmlDictionaryString.GetString(namespaceUri), array, offset, count);
        }

        public virtual void WriteArray(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, short[] array, int offset, int count)
        {
            this.WriteArray(prefix, XmlDictionaryString.GetString(localName), XmlDictionaryString.GetString(namespaceUri), array, offset, count);
        }

        public virtual void WriteArray(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, int[] array, int offset, int count)
        {
            this.WriteArray(prefix, XmlDictionaryString.GetString(localName), XmlDictionaryString.GetString(namespaceUri), array, offset, count);
        }

        public virtual void WriteArray(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, long[] array, int offset, int count)
        {
            this.WriteArray(prefix, XmlDictionaryString.GetString(localName), XmlDictionaryString.GetString(namespaceUri), array, offset, count);
        }

        public virtual void WriteArray(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, float[] array, int offset, int count)
        {
            this.WriteArray(prefix, XmlDictionaryString.GetString(localName), XmlDictionaryString.GetString(namespaceUri), array, offset, count);
        }

        public virtual void WriteArray(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, TimeSpan[] array, int offset, int count)
        {
            this.WriteArray(prefix, XmlDictionaryString.GetString(localName), XmlDictionaryString.GetString(namespaceUri), array, offset, count);
        }

        private void WriteArrayNode(XmlDictionaryReader reader, Type type)
        {
            XmlDictionaryString str;
            XmlDictionaryString str2;
            if (reader.TryGetLocalNameAsDictionaryString(out str) && reader.TryGetNamespaceUriAsDictionaryString(out str2))
            {
                this.WriteArrayNode(reader, reader.Prefix, str, str2, type);
            }
            else
            {
                this.WriteArrayNode(reader, reader.Prefix, reader.LocalName, reader.NamespaceURI, type);
            }
        }

        private void WriteArrayNode(XmlDictionaryReader reader, string prefix, string localName, string namespaceUri, Type type)
        {
            if (type == typeof(bool))
            {
                BooleanArrayHelperWithString.Instance.WriteArray(this, prefix, localName, namespaceUri, reader);
            }
            else if (type == typeof(short))
            {
                Int16ArrayHelperWithString.Instance.WriteArray(this, prefix, localName, namespaceUri, reader);
            }
            else if (type == typeof(int))
            {
                Int32ArrayHelperWithString.Instance.WriteArray(this, prefix, localName, namespaceUri, reader);
            }
            else if (type == typeof(long))
            {
                Int64ArrayHelperWithString.Instance.WriteArray(this, prefix, localName, namespaceUri, reader);
            }
            else if (type == typeof(float))
            {
                SingleArrayHelperWithString.Instance.WriteArray(this, prefix, localName, namespaceUri, reader);
            }
            else if (type == typeof(double))
            {
                DoubleArrayHelperWithString.Instance.WriteArray(this, prefix, localName, namespaceUri, reader);
            }
            else if (type == typeof(decimal))
            {
                DecimalArrayHelperWithString.Instance.WriteArray(this, prefix, localName, namespaceUri, reader);
            }
            else if (type == typeof(DateTime))
            {
                DateTimeArrayHelperWithString.Instance.WriteArray(this, prefix, localName, namespaceUri, reader);
            }
            else if (type == typeof(Guid))
            {
                GuidArrayHelperWithString.Instance.WriteArray(this, prefix, localName, namespaceUri, reader);
            }
            else if (type == typeof(TimeSpan))
            {
                TimeSpanArrayHelperWithString.Instance.WriteArray(this, prefix, localName, namespaceUri, reader);
            }
            else
            {
                this.WriteElementNode(reader, false);
                reader.Read();
            }
        }

        private void WriteArrayNode(XmlDictionaryReader reader, string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, Type type)
        {
            if (type == typeof(bool))
            {
                BooleanArrayHelperWithDictionaryString.Instance.WriteArray(this, prefix, localName, namespaceUri, reader);
            }
            else if (type == typeof(short))
            {
                Int16ArrayHelperWithDictionaryString.Instance.WriteArray(this, prefix, localName, namespaceUri, reader);
            }
            else if (type == typeof(int))
            {
                Int32ArrayHelperWithDictionaryString.Instance.WriteArray(this, prefix, localName, namespaceUri, reader);
            }
            else if (type == typeof(long))
            {
                Int64ArrayHelperWithDictionaryString.Instance.WriteArray(this, prefix, localName, namespaceUri, reader);
            }
            else if (type == typeof(float))
            {
                SingleArrayHelperWithDictionaryString.Instance.WriteArray(this, prefix, localName, namespaceUri, reader);
            }
            else if (type == typeof(double))
            {
                DoubleArrayHelperWithDictionaryString.Instance.WriteArray(this, prefix, localName, namespaceUri, reader);
            }
            else if (type == typeof(decimal))
            {
                DecimalArrayHelperWithDictionaryString.Instance.WriteArray(this, prefix, localName, namespaceUri, reader);
            }
            else if (type == typeof(DateTime))
            {
                DateTimeArrayHelperWithDictionaryString.Instance.WriteArray(this, prefix, localName, namespaceUri, reader);
            }
            else if (type == typeof(Guid))
            {
                GuidArrayHelperWithDictionaryString.Instance.WriteArray(this, prefix, localName, namespaceUri, reader);
            }
            else if (type == typeof(TimeSpan))
            {
                TimeSpanArrayHelperWithDictionaryString.Instance.WriteArray(this, prefix, localName, namespaceUri, reader);
            }
            else
            {
                this.WriteElementNode(reader, false);
                reader.Read();
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void WriteAttributeString(XmlDictionaryString localName, XmlDictionaryString namespaceUri, string value)
        {
            this.WriteAttributeString(null, localName, namespaceUri, value);
        }

        public void WriteAttributeString(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, string value)
        {
            this.WriteStartAttribute(prefix, localName, namespaceUri);
            this.WriteString(value);
            this.WriteEndAttribute();
        }

        private void WriteElementNode(XmlDictionaryReader reader, bool defattr)
        {
            XmlDictionaryString str;
            XmlDictionaryString str2;
            if (reader.TryGetLocalNameAsDictionaryString(out str) && reader.TryGetNamespaceUriAsDictionaryString(out str2))
            {
                this.WriteStartElement(reader.Prefix, str, str2);
            }
            else
            {
                this.WriteStartElement(reader.Prefix, reader.LocalName, reader.NamespaceURI);
            }
            if ((defattr || (!reader.IsDefault && ((reader.SchemaInfo == null) || !reader.SchemaInfo.IsDefault))) && reader.MoveToFirstAttribute())
            {
                do
                {
                    if (reader.TryGetLocalNameAsDictionaryString(out str) && reader.TryGetNamespaceUriAsDictionaryString(out str2))
                    {
                        this.WriteStartAttribute(reader.Prefix, str, str2);
                    }
                    else
                    {
                        this.WriteStartAttribute(reader.Prefix, reader.LocalName, reader.NamespaceURI);
                    }
                    while (reader.ReadAttributeValue())
                    {
                        if (reader.NodeType == XmlNodeType.EntityReference)
                        {
                            this.WriteEntityRef(reader.Name);
                        }
                        else
                        {
                            this.WriteTextNode(reader, true);
                        }
                    }
                    this.WriteEndAttribute();
                }
                while (reader.MoveToNextAttribute());
                reader.MoveToElement();
            }
            if (reader.IsEmptyElement)
            {
                this.WriteEndElement();
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void WriteElementString(XmlDictionaryString localName, XmlDictionaryString namespaceUri, string value)
        {
            this.WriteElementString(null, localName, namespaceUri, value);
        }

        public void WriteElementString(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, string value)
        {
            this.WriteStartElement(prefix, localName, namespaceUri);
            this.WriteString(value);
            this.WriteEndElement();
        }

        public virtual void WriteNode(XmlDictionaryReader reader, bool defattr)
        {
            XmlNodeType type;
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("reader"));
            }
            int num = (reader.NodeType == XmlNodeType.None) ? -1 : reader.Depth;
        Label_002A:
            type = reader.NodeType;
            switch (type)
            {
                case XmlNodeType.Text:
                case XmlNodeType.Whitespace:
                case XmlNodeType.SignificantWhitespace:
                    this.WriteTextNode(reader, false);
                    goto Label_0136;

                default:
                    Type type2;
                    if ((reader.Depth > num) && reader.IsStartArray(out type2))
                    {
                        this.WriteArrayNode(reader, type2);
                        goto Label_0136;
                    }
                    switch (type)
                    {
                        case XmlNodeType.Element:
                            this.WriteElementNode(reader, defattr);
                            goto Label_012D;

                        case XmlNodeType.CDATA:
                            this.WriteCData(reader.Value);
                            goto Label_012D;

                        case XmlNodeType.EntityReference:
                            this.WriteEntityRef(reader.Name);
                            goto Label_012D;

                        case XmlNodeType.ProcessingInstruction:
                        case XmlNodeType.XmlDeclaration:
                            this.WriteProcessingInstruction(reader.Name, reader.Value);
                            goto Label_012D;

                        case XmlNodeType.Comment:
                            this.WriteComment(reader.Value);
                            goto Label_012D;

                        case XmlNodeType.DocumentType:
                            this.WriteDocType(reader.Name, reader.GetAttribute("PUBLIC"), reader.GetAttribute("SYSTEM"), reader.Value);
                            goto Label_012D;

                        case XmlNodeType.EndElement:
                            this.WriteFullEndElement();
                            goto Label_012D;
                    }
                    break;
            }
        Label_012D:
            if (!reader.Read())
            {
                return;
            }
        Label_0136:
            if ((num < reader.Depth) || ((num == reader.Depth) && (reader.NodeType == XmlNodeType.EndElement)))
            {
                goto Label_002A;
            }
        }

        public override void WriteNode(XmlReader reader, bool defattr)
        {
            XmlDictionaryReader reader2 = reader as XmlDictionaryReader;
            if (reader2 != null)
            {
                this.WriteNode(reader2, defattr);
            }
            else
            {
                base.WriteNode(reader, defattr);
            }
        }

        public virtual void WriteQualifiedName(XmlDictionaryString localName, XmlDictionaryString namespaceUri)
        {
            if (localName == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("localName"));
            }
            if (namespaceUri == null)
            {
                namespaceUri = XmlDictionaryString.Empty;
            }
            this.WriteQualifiedName(localName.Value, namespaceUri.Value);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void WriteStartAttribute(XmlDictionaryString localName, XmlDictionaryString namespaceUri)
        {
            this.WriteStartAttribute(null, localName, namespaceUri);
        }

        public virtual void WriteStartAttribute(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri)
        {
            this.WriteStartAttribute(prefix, XmlDictionaryString.GetString(localName), XmlDictionaryString.GetString(namespaceUri));
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void WriteStartElement(XmlDictionaryString localName, XmlDictionaryString namespaceUri)
        {
            this.WriteStartElement(null, localName, namespaceUri);
        }

        public virtual void WriteStartElement(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri)
        {
            this.WriteStartElement(prefix, XmlDictionaryString.GetString(localName), XmlDictionaryString.GetString(namespaceUri));
        }

        public virtual void WriteString(XmlDictionaryString value)
        {
            this.WriteString(XmlDictionaryString.GetString(value));
        }

        protected virtual void WriteTextNode(XmlDictionaryReader reader, bool isAttribute)
        {
            XmlDictionaryString str;
            if (reader.TryGetValueAsDictionaryString(out str))
            {
                this.WriteString(str);
            }
            else
            {
                this.WriteString(reader.Value);
            }
            if (!isAttribute)
            {
                reader.Read();
            }
        }

        public virtual void WriteValue(Guid value)
        {
            this.WriteString(value.ToString());
        }

        public virtual void WriteValue(TimeSpan value)
        {
            this.WriteString(XmlConvert.ToString(value));
        }

        public virtual void WriteValue(IStreamProvider value)
        {
            if (value == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("value"));
            }
            Stream stream = value.GetStream();
            if (stream == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("XmlInvalidStream")));
            }
            int count = 0x100;
            int num2 = 0;
            byte[] buffer = new byte[count];
        Label_004B:
            num2 = stream.Read(buffer, 0, count);
            if (num2 > 0)
            {
                this.WriteBase64(buffer, 0, num2);
                if ((count < 0x10000) && (num2 == count))
                {
                    count *= 0x10;
                    buffer = new byte[count];
                }
                goto Label_004B;
            }
            value.ReleaseStream(stream);
        }

        public virtual void WriteValue(UniqueId value)
        {
            if (value == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
            }
            this.WriteString(value.ToString());
        }

        public virtual void WriteValue(XmlDictionaryString value)
        {
            this.WriteValue(XmlDictionaryString.GetString(value));
        }

        public virtual void WriteXmlAttribute(string localName, string value)
        {
            base.WriteAttributeString("xml", localName, null, value);
        }

        public virtual void WriteXmlAttribute(XmlDictionaryString localName, XmlDictionaryString value)
        {
            this.WriteXmlAttribute(XmlDictionaryString.GetString(localName), XmlDictionaryString.GetString(value));
        }

        public virtual void WriteXmlnsAttribute(string prefix, string namespaceUri)
        {
            if (namespaceUri == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("namespaceUri");
            }
            if (prefix == null)
            {
                if (this.LookupPrefix(namespaceUri) != null)
                {
                    return;
                }
                prefix = (namespaceUri.Length == 0) ? string.Empty : ("d" + namespaceUri.Length.ToString(NumberFormatInfo.InvariantInfo));
            }
            base.WriteAttributeString("xmlns", prefix, null, namespaceUri);
        }

        public virtual void WriteXmlnsAttribute(string prefix, XmlDictionaryString namespaceUri)
        {
            this.WriteXmlnsAttribute(prefix, XmlDictionaryString.GetString(namespaceUri));
        }

        public virtual bool CanCanonicalize
        {
            get
            {
                return false;
            }
        }

        private class XmlWrappedWriter : XmlDictionaryWriter
        {
            private int depth;
            private int prefix;
            private XmlWriter writer;

            public XmlWrappedWriter(XmlWriter writer)
            {
                this.writer = writer;
                this.depth = 0;
            }

            public override void Close()
            {
                this.writer.Close();
            }

            public override void Flush()
            {
                this.writer.Flush();
            }

            public override string LookupPrefix(string namespaceUri)
            {
                return this.writer.LookupPrefix(namespaceUri);
            }

            public override void WriteAttributes(XmlReader reader, bool defattr)
            {
                this.writer.WriteAttributes(reader, defattr);
            }

            public override void WriteBase64(byte[] buffer, int index, int count)
            {
                this.writer.WriteBase64(buffer, index, count);
            }

            public override void WriteBinHex(byte[] buffer, int index, int count)
            {
                this.writer.WriteBinHex(buffer, index, count);
            }

            public override void WriteCData(string text)
            {
                this.writer.WriteCData(text);
            }

            public override void WriteCharEntity(char ch)
            {
                this.writer.WriteCharEntity(ch);
            }

            public override void WriteChars(char[] buffer, int index, int count)
            {
                this.writer.WriteChars(buffer, index, count);
            }

            public override void WriteComment(string text)
            {
                this.writer.WriteComment(text);
            }

            public override void WriteDocType(string name, string pubid, string sysid, string subset)
            {
                this.writer.WriteDocType(name, pubid, sysid, subset);
            }

            public override void WriteEndAttribute()
            {
                this.writer.WriteEndAttribute();
            }

            public override void WriteEndDocument()
            {
                this.writer.WriteEndDocument();
            }

            public override void WriteEndElement()
            {
                this.writer.WriteEndElement();
                this.depth--;
            }

            public override void WriteEntityRef(string name)
            {
                this.writer.WriteEntityRef(name);
            }

            public override void WriteFullEndElement()
            {
                this.writer.WriteFullEndElement();
            }

            public override void WriteName(string name)
            {
                this.writer.WriteName(name);
            }

            public override void WriteNmToken(string name)
            {
                this.writer.WriteNmToken(name);
            }

            public override void WriteNode(XmlReader reader, bool defattr)
            {
                this.writer.WriteNode(reader, defattr);
            }

            public override void WriteProcessingInstruction(string name, string text)
            {
                this.writer.WriteProcessingInstruction(name, text);
            }

            public override void WriteQualifiedName(string localName, string namespaceUri)
            {
                this.writer.WriteQualifiedName(localName, namespaceUri);
            }

            public override void WriteRaw(string data)
            {
                this.writer.WriteRaw(data);
            }

            public override void WriteRaw(char[] buffer, int index, int count)
            {
                this.writer.WriteRaw(buffer, index, count);
            }

            public override void WriteStartAttribute(string prefix, string localName, string namespaceUri)
            {
                this.writer.WriteStartAttribute(prefix, localName, namespaceUri);
                this.prefix++;
            }

            public override void WriteStartDocument()
            {
                this.writer.WriteStartDocument();
            }

            public override void WriteStartDocument(bool standalone)
            {
                this.writer.WriteStartDocument(standalone);
            }

            public override void WriteStartElement(string prefix, string localName, string namespaceUri)
            {
                this.writer.WriteStartElement(prefix, localName, namespaceUri);
                this.depth++;
                this.prefix = 1;
            }

            public override void WriteString(string text)
            {
                this.writer.WriteString(text);
            }

            public override void WriteSurrogateCharEntity(char lowChar, char highChar)
            {
                this.writer.WriteSurrogateCharEntity(lowChar, highChar);
            }

            public override void WriteValue(bool value)
            {
                this.writer.WriteValue(value);
            }

            public override void WriteValue(DateTime value)
            {
                this.writer.WriteValue(value);
            }

            public override void WriteValue(double value)
            {
                this.writer.WriteValue(value);
            }

            public override void WriteValue(int value)
            {
                this.writer.WriteValue(value);
            }

            public override void WriteValue(long value)
            {
                this.writer.WriteValue(value);
            }

            public override void WriteValue(object value)
            {
                this.writer.WriteValue(value);
            }

            public override void WriteValue(string value)
            {
                this.writer.WriteValue(value);
            }

            public override void WriteWhitespace(string whitespace)
            {
                this.writer.WriteWhitespace(whitespace);
            }

            public override void WriteXmlnsAttribute(string prefix, string namespaceUri)
            {
                if (namespaceUri == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("namespaceUri");
                }
                if (prefix == null)
                {
                    if (this.LookupPrefix(namespaceUri) != null)
                    {
                        return;
                    }
                    if (namespaceUri.Length == 0)
                    {
                        prefix = string.Empty;
                    }
                    else
                    {
                        string str = this.depth.ToString(NumberFormatInfo.InvariantInfo);
                        string str2 = this.prefix.ToString(NumberFormatInfo.InvariantInfo);
                        prefix = "d" + str + "p" + str2;
                    }
                }
                base.WriteAttributeString("xmlns", prefix, null, namespaceUri);
            }

            public override System.Xml.WriteState WriteState
            {
                get
                {
                    return this.writer.WriteState;
                }
            }

            public override string XmlLang
            {
                get
                {
                    return this.writer.XmlLang;
                }
            }

            public override System.Xml.XmlSpace XmlSpace
            {
                get
                {
                    return this.writer.XmlSpace;
                }
            }
        }
    }
}

