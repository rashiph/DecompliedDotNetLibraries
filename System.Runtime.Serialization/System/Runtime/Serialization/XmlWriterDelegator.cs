namespace System.Runtime.Serialization
{
    using System;
    using System.Globalization;
    using System.Runtime;
    using System.Xml;

    internal class XmlWriterDelegator
    {
        private const int ByteChunkSize = 0x39;
        private const int CharChunkSize = 0x4c;
        internal int depth;
        protected XmlDictionaryWriter dictionaryWriter;
        private int prefixes;
        protected XmlWriter writer;

        public XmlWriterDelegator(XmlWriter writer)
        {
            XmlObjectSerializer.CheckNull(writer, "writer");
            this.writer = writer;
            this.dictionaryWriter = writer as XmlDictionaryWriter;
        }

        private Exception CreateInvalidPrimitiveTypeException(Type type)
        {
            return new InvalidDataContractException(System.Runtime.Serialization.SR.GetString("InvalidPrimitiveType", new object[] { DataContract.GetClrTypeFullName(type) }));
        }

        internal void Flush()
        {
            this.writer.Flush();
        }

        internal string LookupPrefix(string ns)
        {
            return this.writer.LookupPrefix(ns);
        }

        internal void WriteAnyType(object value)
        {
            this.WriteAnyType(value, value.GetType());
        }

        internal void WriteAnyType(object value, Type valueType)
        {
            bool flag = true;
            switch (Type.GetTypeCode(valueType))
            {
                case TypeCode.Boolean:
                    this.WriteBoolean((bool) value);
                    break;

                case TypeCode.Char:
                    this.WriteChar((char) value);
                    break;

                case TypeCode.SByte:
                    this.WriteSignedByte((sbyte) value);
                    break;

                case TypeCode.Byte:
                    this.WriteUnsignedByte((byte) value);
                    break;

                case TypeCode.Int16:
                    this.WriteShort((short) value);
                    break;

                case TypeCode.UInt16:
                    this.WriteUnsignedShort((ushort) value);
                    break;

                case TypeCode.Int32:
                    this.WriteInt((int) value);
                    break;

                case TypeCode.UInt32:
                    this.WriteUnsignedInt((uint) value);
                    break;

                case TypeCode.Int64:
                    this.WriteLong((long) value);
                    break;

                case TypeCode.UInt64:
                    this.WriteUnsignedLong((ulong) value);
                    break;

                case TypeCode.Single:
                    this.WriteFloat((float) value);
                    break;

                case TypeCode.Double:
                    this.WriteDouble((double) value);
                    break;

                case TypeCode.Decimal:
                    this.WriteDecimal((decimal) value);
                    break;

                case TypeCode.DateTime:
                    this.WriteDateTime((DateTime) value);
                    break;

                case TypeCode.String:
                    this.WriteString((string) value);
                    break;

                default:
                    if (valueType == Globals.TypeOfByteArray)
                    {
                        this.WriteBase64((byte[]) value);
                    }
                    else if (valueType != Globals.TypeOfObject)
                    {
                        if (valueType == Globals.TypeOfTimeSpan)
                        {
                            this.WriteTimeSpan((TimeSpan) value);
                        }
                        else if (valueType == Globals.TypeOfGuid)
                        {
                            this.WriteGuid((Guid) value);
                        }
                        else if (valueType == Globals.TypeOfUri)
                        {
                            this.WriteUri((Uri) value);
                        }
                        else if (valueType == Globals.TypeOfXmlQualifiedName)
                        {
                            this.WriteQName((XmlQualifiedName) value);
                        }
                        else
                        {
                            flag = false;
                        }
                    }
                    break;
            }
            if (!flag)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(this.CreateInvalidPrimitiveTypeException(valueType));
            }
        }

        internal void WriteAttributeBool(string prefix, XmlDictionaryString attrName, XmlDictionaryString attrNs, bool value)
        {
            this.WriteStartAttribute(prefix, attrName, attrNs);
            this.WriteAttributeBoolValue(value);
            this.WriteEndAttribute();
        }

        private void WriteAttributeBoolValue(bool value)
        {
            this.writer.WriteValue(value);
        }

        internal void WriteAttributeInt(string prefix, XmlDictionaryString attrName, XmlDictionaryString attrNs, int value)
        {
            this.WriteStartAttribute(prefix, attrName, attrNs);
            this.WriteAttributeIntValue(value);
            this.WriteEndAttribute();
        }

        private void WriteAttributeIntValue(int value)
        {
            this.writer.WriteValue(value);
        }

        internal void WriteAttributeQualifiedName(string attrPrefix, XmlDictionaryString attrName, XmlDictionaryString attrNs, string name, string ns)
        {
            this.WriteXmlnsAttribute(ns);
            this.WriteStartAttribute(attrPrefix, attrName, attrNs);
            this.WriteAttributeQualifiedNameValue(name, ns);
            this.WriteEndAttribute();
        }

        internal void WriteAttributeQualifiedName(string attrPrefix, XmlDictionaryString attrName, XmlDictionaryString attrNs, XmlDictionaryString name, XmlDictionaryString ns)
        {
            this.WriteXmlnsAttribute(ns);
            this.WriteStartAttribute(attrPrefix, attrName, attrNs);
            this.WriteAttributeQualifiedNameValue(name, ns);
            this.WriteEndAttribute();
        }

        private void WriteAttributeQualifiedNameValue(string name, string ns)
        {
            this.writer.WriteQualifiedName(name, ns);
        }

        private void WriteAttributeQualifiedNameValue(XmlDictionaryString name, XmlDictionaryString ns)
        {
            if (this.dictionaryWriter == null)
            {
                this.writer.WriteQualifiedName(name.Value, ns.Value);
            }
            else
            {
                this.dictionaryWriter.WriteQualifiedName(name, ns);
            }
        }

        internal void WriteAttributeString(string prefix, string localName, string ns, string value)
        {
            this.WriteStartAttribute(prefix, localName, ns);
            this.WriteAttributeStringValue(value);
            this.WriteEndAttribute();
        }

        internal void WriteAttributeString(string prefix, XmlDictionaryString attrName, XmlDictionaryString attrNs, string value)
        {
            this.WriteStartAttribute(prefix, attrName, attrNs);
            this.WriteAttributeStringValue(value);
            this.WriteEndAttribute();
        }

        internal void WriteAttributeString(string prefix, XmlDictionaryString attrName, XmlDictionaryString attrNs, XmlDictionaryString value)
        {
            this.WriteStartAttribute(prefix, attrName, attrNs);
            this.WriteAttributeStringValue(value);
            this.WriteEndAttribute();
        }

        private void WriteAttributeStringValue(string value)
        {
            this.writer.WriteValue(value);
        }

        private void WriteAttributeStringValue(XmlDictionaryString value)
        {
            if (this.dictionaryWriter == null)
            {
                this.writer.WriteString(value.Value);
            }
            else
            {
                this.dictionaryWriter.WriteString(value);
            }
        }

        internal virtual void WriteBase64(byte[] bytes)
        {
            if (bytes != null)
            {
                this.writer.WriteBase64(bytes, 0, bytes.Length);
            }
        }

        internal virtual void WriteBoolean(bool value)
        {
            this.writer.WriteValue(value);
        }

        public void WriteBoolean(bool value, XmlDictionaryString name, XmlDictionaryString ns)
        {
            this.WriteStartElementPrimitive(name, ns);
            this.WriteBoolean(value);
            this.WriteEndElementPrimitive();
        }

        public void WriteBooleanArray(bool[] value, XmlDictionaryString itemName, XmlDictionaryString itemNamespace)
        {
            if (this.dictionaryWriter == null)
            {
                for (int i = 0; i < value.Length; i++)
                {
                    this.WriteBoolean(value[i], itemName, itemNamespace);
                }
            }
            else
            {
                this.dictionaryWriter.WriteArray(null, itemName, itemNamespace, value, 0, value.Length);
            }
        }

        internal virtual void WriteChar(char value)
        {
            this.writer.WriteValue((int) value);
        }

        public void WriteChar(char value, XmlDictionaryString name, XmlDictionaryString ns)
        {
            this.WriteStartElementPrimitive(name, ns);
            this.WriteChar(value);
            this.WriteEndElementPrimitive();
        }

        internal virtual void WriteDateTime(DateTime value)
        {
            this.writer.WriteValue(value);
        }

        public void WriteDateTime(DateTime value, XmlDictionaryString name, XmlDictionaryString ns)
        {
            this.WriteStartElementPrimitive(name, ns);
            this.WriteDateTime(value);
            this.WriteEndElementPrimitive();
        }

        public void WriteDateTimeArray(DateTime[] value, XmlDictionaryString itemName, XmlDictionaryString itemNamespace)
        {
            if (this.dictionaryWriter == null)
            {
                for (int i = 0; i < value.Length; i++)
                {
                    this.WriteDateTime(value[i], itemName, itemNamespace);
                }
            }
            else
            {
                this.dictionaryWriter.WriteArray(null, itemName, itemNamespace, value, 0, value.Length);
            }
        }

        internal virtual void WriteDecimal(decimal value)
        {
            this.writer.WriteValue(value);
        }

        public void WriteDecimal(decimal value, XmlDictionaryString name, XmlDictionaryString ns)
        {
            this.WriteStartElementPrimitive(name, ns);
            this.WriteDecimal(value);
            this.WriteEndElementPrimitive();
        }

        public void WriteDecimalArray(decimal[] value, XmlDictionaryString itemName, XmlDictionaryString itemNamespace)
        {
            if (this.dictionaryWriter == null)
            {
                for (int i = 0; i < value.Length; i++)
                {
                    this.WriteDecimal(value[i], itemName, itemNamespace);
                }
            }
            else
            {
                this.dictionaryWriter.WriteArray(null, itemName, itemNamespace, value, 0, value.Length);
            }
        }

        internal virtual void WriteDouble(double value)
        {
            this.writer.WriteValue(value);
        }

        public void WriteDouble(double value, XmlDictionaryString name, XmlDictionaryString ns)
        {
            this.WriteStartElementPrimitive(name, ns);
            this.WriteDouble(value);
            this.WriteEndElementPrimitive();
        }

        public void WriteDoubleArray(double[] value, XmlDictionaryString itemName, XmlDictionaryString itemNamespace)
        {
            if (this.dictionaryWriter == null)
            {
                for (int i = 0; i < value.Length; i++)
                {
                    this.WriteDouble(value[i], itemName, itemNamespace);
                }
            }
            else
            {
                this.dictionaryWriter.WriteArray(null, itemName, itemNamespace, value, 0, value.Length);
            }
        }

        private void WriteEndAttribute()
        {
            this.writer.WriteEndAttribute();
        }

        public void WriteEndElement()
        {
            this.writer.WriteEndElement();
            this.depth--;
        }

        internal void WriteEndElementPrimitive()
        {
            this.writer.WriteEndElement();
        }

        internal void WriteExtensionData(IDataNode dataNode)
        {
            bool flag = true;
            Type dataType = dataNode.DataType;
            switch (Type.GetTypeCode(dataType))
            {
                case TypeCode.Boolean:
                    this.WriteBoolean(((DataNode<bool>) dataNode).GetValue());
                    break;

                case TypeCode.Char:
                    this.WriteChar(((DataNode<char>) dataNode).GetValue());
                    break;

                case TypeCode.SByte:
                    this.WriteSignedByte(((DataNode<sbyte>) dataNode).GetValue());
                    break;

                case TypeCode.Byte:
                    this.WriteUnsignedByte(((DataNode<byte>) dataNode).GetValue());
                    break;

                case TypeCode.Int16:
                    this.WriteShort(((DataNode<short>) dataNode).GetValue());
                    break;

                case TypeCode.UInt16:
                    this.WriteUnsignedShort(((DataNode<ushort>) dataNode).GetValue());
                    break;

                case TypeCode.Int32:
                    this.WriteInt(((DataNode<int>) dataNode).GetValue());
                    break;

                case TypeCode.UInt32:
                    this.WriteUnsignedInt(((DataNode<uint>) dataNode).GetValue());
                    break;

                case TypeCode.Int64:
                    this.WriteLong(((DataNode<long>) dataNode).GetValue());
                    break;

                case TypeCode.UInt64:
                    this.WriteUnsignedLong(((DataNode<ulong>) dataNode).GetValue());
                    break;

                case TypeCode.Single:
                    this.WriteFloat(((DataNode<float>) dataNode).GetValue());
                    break;

                case TypeCode.Double:
                    this.WriteDouble(((DataNode<double>) dataNode).GetValue());
                    break;

                case TypeCode.Decimal:
                    this.WriteDecimal(((DataNode<decimal>) dataNode).GetValue());
                    break;

                case TypeCode.DateTime:
                    this.WriteDateTime(((DataNode<DateTime>) dataNode).GetValue());
                    break;

                case TypeCode.String:
                    this.WriteString(((DataNode<string>) dataNode).GetValue());
                    break;

                default:
                    if (dataType == Globals.TypeOfByteArray)
                    {
                        this.WriteBase64(((DataNode<byte[]>) dataNode).GetValue());
                    }
                    else if (dataType == Globals.TypeOfObject)
                    {
                        object obj2 = dataNode.Value;
                        if (obj2 != null)
                        {
                            this.WriteAnyType(obj2);
                        }
                    }
                    else if (dataType == Globals.TypeOfTimeSpan)
                    {
                        this.WriteTimeSpan(((DataNode<TimeSpan>) dataNode).GetValue());
                    }
                    else if (dataType == Globals.TypeOfGuid)
                    {
                        this.WriteGuid(((DataNode<Guid>) dataNode).GetValue());
                    }
                    else if (dataType == Globals.TypeOfUri)
                    {
                        this.WriteUri(((DataNode<Uri>) dataNode).GetValue());
                    }
                    else if (dataType == Globals.TypeOfXmlQualifiedName)
                    {
                        this.WriteQName(((DataNode<XmlQualifiedName>) dataNode).GetValue());
                    }
                    else
                    {
                        flag = false;
                    }
                    break;
            }
            if (!flag)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(this.CreateInvalidPrimitiveTypeException(dataType));
            }
        }

        internal virtual void WriteFloat(float value)
        {
            this.writer.WriteValue(value);
        }

        public void WriteFloat(float value, XmlDictionaryString name, XmlDictionaryString ns)
        {
            this.WriteStartElementPrimitive(name, ns);
            this.WriteFloat(value);
            this.WriteEndElementPrimitive();
        }

        internal void WriteGuid(Guid value)
        {
            this.writer.WriteRaw(value.ToString());
        }

        public void WriteGuid(Guid value, XmlDictionaryString name, XmlDictionaryString ns)
        {
            this.WriteStartElementPrimitive(name, ns);
            this.WriteGuid(value);
            this.WriteEndElementPrimitive();
        }

        internal virtual void WriteInt(int value)
        {
            this.writer.WriteValue(value);
        }

        public void WriteInt(int value, XmlDictionaryString name, XmlDictionaryString ns)
        {
            this.WriteStartElementPrimitive(name, ns);
            this.WriteInt(value);
            this.WriteEndElementPrimitive();
        }

        public void WriteInt32Array(int[] value, XmlDictionaryString itemName, XmlDictionaryString itemNamespace)
        {
            if (this.dictionaryWriter == null)
            {
                for (int i = 0; i < value.Length; i++)
                {
                    this.WriteInt(value[i], itemName, itemNamespace);
                }
            }
            else
            {
                this.dictionaryWriter.WriteArray(null, itemName, itemNamespace, value, 0, value.Length);
            }
        }

        public void WriteInt64Array(long[] value, XmlDictionaryString itemName, XmlDictionaryString itemNamespace)
        {
            if (this.dictionaryWriter == null)
            {
                for (int i = 0; i < value.Length; i++)
                {
                    this.WriteLong(value[i], itemName, itemNamespace);
                }
            }
            else
            {
                this.dictionaryWriter.WriteArray(null, itemName, itemNamespace, value, 0, value.Length);
            }
        }

        internal virtual void WriteLong(long value)
        {
            this.writer.WriteValue(value);
        }

        public void WriteLong(long value, XmlDictionaryString name, XmlDictionaryString ns)
        {
            this.WriteStartElementPrimitive(name, ns);
            this.WriteLong(value);
            this.WriteEndElementPrimitive();
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void WriteNamespaceDecl(XmlDictionaryString ns)
        {
            this.WriteXmlnsAttribute(ns);
        }

        internal virtual void WriteQName(XmlQualifiedName value)
        {
            if (value != XmlQualifiedName.Empty)
            {
                this.WriteXmlnsAttribute(value.Namespace);
                this.WriteQualifiedName(value.Name, value.Namespace);
            }
        }

        internal void WriteQualifiedName(string localName, string ns)
        {
            this.writer.WriteQualifiedName(localName, ns);
        }

        internal void WriteQualifiedName(XmlDictionaryString localName, XmlDictionaryString ns)
        {
            if (this.dictionaryWriter == null)
            {
                this.writer.WriteQualifiedName(localName.Value, ns.Value);
            }
            else
            {
                this.dictionaryWriter.WriteQualifiedName(localName, ns);
            }
        }

        internal void WriteRaw(string data)
        {
            this.writer.WriteRaw(data);
        }

        internal void WriteRaw(char[] buffer, int index, int count)
        {
            this.writer.WriteRaw(buffer, index, count);
        }

        internal virtual void WriteShort(short value)
        {
            this.writer.WriteValue((int) value);
        }

        public void WriteShort(short value, XmlDictionaryString name, XmlDictionaryString ns)
        {
            this.WriteStartElementPrimitive(name, ns);
            this.WriteShort(value);
            this.WriteEndElementPrimitive();
        }

        internal virtual void WriteSignedByte(sbyte value)
        {
            this.writer.WriteValue((int) value);
        }

        public void WriteSignedByte(sbyte value, XmlDictionaryString name, XmlDictionaryString ns)
        {
            this.WriteStartElementPrimitive(name, ns);
            this.WriteSignedByte(value);
            this.WriteEndElementPrimitive();
        }

        public void WriteSingleArray(float[] value, XmlDictionaryString itemName, XmlDictionaryString itemNamespace)
        {
            if (this.dictionaryWriter == null)
            {
                for (int i = 0; i < value.Length; i++)
                {
                    this.WriteFloat(value[i], itemName, itemNamespace);
                }
            }
            else
            {
                this.dictionaryWriter.WriteArray(null, itemName, itemNamespace, value, 0, value.Length);
            }
        }

        private void WriteStartAttribute(string prefix, string localName, string ns)
        {
            this.writer.WriteStartAttribute(prefix, localName, ns);
        }

        private void WriteStartAttribute(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri)
        {
            if (this.dictionaryWriter != null)
            {
                this.dictionaryWriter.WriteStartAttribute(prefix, localName, namespaceUri);
            }
            else
            {
                this.writer.WriteStartAttribute(prefix, (localName == null) ? null : localName.Value, (namespaceUri == null) ? null : namespaceUri.Value);
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal void WriteStartElement(string localName, string ns)
        {
            this.WriteStartElement(null, localName, ns);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void WriteStartElement(XmlDictionaryString localName, XmlDictionaryString namespaceUri)
        {
            this.WriteStartElement(null, localName, namespaceUri);
        }

        internal void WriteStartElement(string prefix, string localName, string ns)
        {
            this.writer.WriteStartElement(prefix, localName, ns);
            this.depth++;
            this.prefixes = 1;
        }

        internal void WriteStartElement(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri)
        {
            if (this.dictionaryWriter != null)
            {
                this.dictionaryWriter.WriteStartElement(prefix, localName, namespaceUri);
            }
            else
            {
                this.writer.WriteStartElement(prefix, (localName == null) ? null : localName.Value, (namespaceUri == null) ? null : namespaceUri.Value);
            }
            this.depth++;
            this.prefixes = 1;
        }

        internal void WriteStartElementPrimitive(XmlDictionaryString localName, XmlDictionaryString namespaceUri)
        {
            if (this.dictionaryWriter != null)
            {
                this.dictionaryWriter.WriteStartElement(null, localName, namespaceUri);
            }
            else
            {
                this.writer.WriteStartElement(null, (localName == null) ? null : localName.Value, (namespaceUri == null) ? null : namespaceUri.Value);
            }
        }

        internal void WriteString(string value)
        {
            this.writer.WriteValue(value);
        }

        internal void WriteTimeSpan(TimeSpan value)
        {
            this.writer.WriteRaw(XmlConvert.ToString(value));
        }

        public void WriteTimeSpan(TimeSpan value, XmlDictionaryString name, XmlDictionaryString ns)
        {
            this.WriteStartElementPrimitive(name, ns);
            this.WriteTimeSpan(value);
            this.WriteEndElementPrimitive();
        }

        internal virtual void WriteUnsignedByte(byte value)
        {
            this.writer.WriteValue((int) value);
        }

        public void WriteUnsignedByte(byte value, XmlDictionaryString name, XmlDictionaryString ns)
        {
            this.WriteStartElementPrimitive(name, ns);
            this.WriteUnsignedByte(value);
            this.WriteEndElementPrimitive();
        }

        internal virtual void WriteUnsignedInt(uint value)
        {
            this.writer.WriteValue((long) value);
        }

        public void WriteUnsignedInt(uint value, XmlDictionaryString name, XmlDictionaryString ns)
        {
            this.WriteStartElementPrimitive(name, ns);
            this.WriteUnsignedInt(value);
            this.WriteEndElementPrimitive();
        }

        internal virtual void WriteUnsignedLong(ulong value)
        {
            this.writer.WriteRaw(XmlConvert.ToString(value));
        }

        public void WriteUnsignedLong(ulong value, XmlDictionaryString name, XmlDictionaryString ns)
        {
            this.WriteStartElementPrimitive(name, ns);
            this.WriteUnsignedLong(value);
            this.WriteEndElementPrimitive();
        }

        internal virtual void WriteUnsignedShort(ushort value)
        {
            this.writer.WriteValue((int) value);
        }

        public void WriteUnsignedShort(ushort value, XmlDictionaryString name, XmlDictionaryString ns)
        {
            this.WriteStartElementPrimitive(name, ns);
            this.WriteUnsignedShort(value);
            this.WriteEndElementPrimitive();
        }

        internal void WriteUri(Uri value)
        {
            this.writer.WriteString(value.GetComponents(UriComponents.SerializationInfoString, UriFormat.UriEscaped));
        }

        internal void WriteXmlnsAttribute(string ns)
        {
            if (ns != null)
            {
                if (ns.Length == 0)
                {
                    this.writer.WriteAttributeString("xmlns", string.Empty, null, ns);
                }
                else if (this.dictionaryWriter != null)
                {
                    this.dictionaryWriter.WriteXmlnsAttribute(null, ns);
                }
                else if (this.writer.LookupPrefix(ns) == null)
                {
                    string localName = string.Format(CultureInfo.InvariantCulture, "d{0}p{1}", new object[] { this.depth, this.prefixes });
                    this.prefixes++;
                    this.writer.WriteAttributeString("xmlns", localName, null, ns);
                }
            }
        }

        internal void WriteXmlnsAttribute(XmlDictionaryString ns)
        {
            if (this.dictionaryWriter != null)
            {
                if (ns != null)
                {
                    this.dictionaryWriter.WriteXmlnsAttribute(null, ns);
                }
            }
            else
            {
                this.WriteXmlnsAttribute(ns.Value);
            }
        }

        internal void WriteXmlnsAttribute(string prefix, XmlDictionaryString ns)
        {
            if (this.dictionaryWriter != null)
            {
                this.dictionaryWriter.WriteXmlnsAttribute(prefix, ns);
            }
            else
            {
                this.writer.WriteAttributeString("xmlns", prefix, null, ns.Value);
            }
        }

        internal XmlWriter Writer
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.writer;
            }
        }

        internal System.Xml.WriteState WriteState
        {
            get
            {
                return this.writer.WriteState;
            }
        }

        internal string XmlLang
        {
            get
            {
                return this.writer.XmlLang;
            }
        }

        internal System.Xml.XmlSpace XmlSpace
        {
            get
            {
                return this.writer.XmlSpace;
            }
        }
    }
}

