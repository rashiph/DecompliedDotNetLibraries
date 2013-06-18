namespace System.Runtime.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Xml;
    using System.Xml.Serialization;

    internal class XmlReaderDelegator
    {
        protected XmlDictionaryReader dictionaryReader;
        protected bool isEndOfEmptyElement;
        protected XmlReader reader;

        public XmlReaderDelegator(XmlReader reader)
        {
            XmlObjectSerializer.CheckNull(reader, "reader");
            this.reader = reader;
            this.dictionaryReader = reader as XmlDictionaryReader;
        }

        private void CheckActualArrayLength(int expectedLength, int actualLength, XmlDictionaryString itemName, XmlDictionaryString itemNamespace)
        {
            if (expectedLength != actualLength)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(System.Runtime.Serialization.SR.GetString("ArrayExceededSizeAttribute", new object[] { expectedLength, itemName.Value, itemNamespace.Value })));
            }
        }

        private void CheckExpectedArrayLength(XmlObjectSerializerReadContext context, int arrayLength)
        {
            context.IncrementItemCount(arrayLength);
        }

        private Exception CreateInvalidPrimitiveTypeException(Type type)
        {
            return new InvalidDataContractException(System.Runtime.Serialization.SR.GetString(type.IsInterface ? "InterfaceTypeCannotBeCreated" : "InvalidPrimitiveType", new object[] { DataContract.GetClrTypeFullName(type) }));
        }

        protected int GetArrayLengthQuota(XmlObjectSerializerReadContext context)
        {
            if (this.dictionaryReader.Quotas == null)
            {
                return context.RemainingItemCount;
            }
            return Math.Min(context.RemainingItemCount, this.dictionaryReader.Quotas.MaxArrayLength);
        }

        internal string GetAttribute(int i)
        {
            if (this.isEndOfEmptyElement)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("i", System.Runtime.Serialization.SR.GetString("XmlElementAttributes")));
            }
            return this.reader.GetAttribute(i);
        }

        internal string GetAttribute(string name)
        {
            if (!this.isEndOfEmptyElement)
            {
                return this.reader.GetAttribute(name);
            }
            return null;
        }

        internal string GetAttribute(string name, string namespaceUri)
        {
            if (!this.isEndOfEmptyElement)
            {
                return this.reader.GetAttribute(name, namespaceUri);
            }
            return null;
        }

        internal IDictionary<string, string> GetNamespacesInScope(XmlNamespaceScope scope)
        {
            if (this.reader is IXmlNamespaceResolver)
            {
                return ((IXmlNamespaceResolver) this.reader).GetNamespacesInScope(scope);
            }
            return null;
        }

        internal bool HasLineInfo()
        {
            IXmlLineInfo reader = this.reader as IXmlLineInfo;
            return ((reader != null) && reader.HasLineInfo());
        }

        internal int IndexOfLocalName(XmlDictionaryString[] localNames, XmlDictionaryString ns)
        {
            if (this.dictionaryReader != null)
            {
                return this.dictionaryReader.IndexOfLocalName(localNames, ns);
            }
            if (this.reader.NamespaceURI == ns.Value)
            {
                string localName = this.LocalName;
                for (int i = 0; i < localNames.Length; i++)
                {
                    if (localName == localNames[i].Value)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        internal bool IsLocalName(string localName)
        {
            if (this.dictionaryReader == null)
            {
                return (localName == this.reader.LocalName);
            }
            return this.dictionaryReader.IsLocalName(localName);
        }

        internal bool IsLocalName(XmlDictionaryString localName)
        {
            if (this.dictionaryReader == null)
            {
                return (localName.Value == this.reader.LocalName);
            }
            return this.dictionaryReader.IsLocalName(localName);
        }

        internal bool IsNamespaceUri(XmlDictionaryString ns)
        {
            if (this.dictionaryReader == null)
            {
                return (ns.Value == this.reader.NamespaceURI);
            }
            return this.dictionaryReader.IsNamespaceUri(ns);
        }

        internal bool IsNamespaceURI(string ns)
        {
            if (this.dictionaryReader == null)
            {
                return (ns == this.reader.NamespaceURI);
            }
            return this.dictionaryReader.IsNamespaceUri(ns);
        }

        public bool IsStartElement()
        {
            return (!this.isEndOfEmptyElement && this.reader.IsStartElement());
        }

        internal bool IsStartElement(string localname, string ns)
        {
            return (!this.isEndOfEmptyElement && this.reader.IsStartElement(localname, ns));
        }

        public bool IsStartElement(XmlDictionaryString localname, XmlDictionaryString ns)
        {
            if (this.dictionaryReader == null)
            {
                return (!this.isEndOfEmptyElement && this.reader.IsStartElement(localname.Value, ns.Value));
            }
            return (!this.isEndOfEmptyElement && this.dictionaryReader.IsStartElement(localname, ns));
        }

        internal string LookupNamespace(string prefix)
        {
            return this.reader.LookupNamespace(prefix);
        }

        internal void MoveToAttribute(int i)
        {
            if (this.isEndOfEmptyElement)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("i", System.Runtime.Serialization.SR.GetString("XmlElementAttributes")));
            }
            this.reader.MoveToAttribute(i);
        }

        internal bool MoveToAttribute(string name)
        {
            return (!this.isEndOfEmptyElement && this.reader.MoveToAttribute(name));
        }

        internal bool MoveToAttribute(string name, string ns)
        {
            return (!this.isEndOfEmptyElement && this.reader.MoveToAttribute(name, ns));
        }

        internal XmlNodeType MoveToContent()
        {
            if (this.isEndOfEmptyElement)
            {
                return XmlNodeType.EndElement;
            }
            return this.reader.MoveToContent();
        }

        internal bool MoveToElement()
        {
            return (!this.isEndOfEmptyElement && this.reader.MoveToElement());
        }

        internal bool MoveToFirstAttribute()
        {
            return (!this.isEndOfEmptyElement && this.reader.MoveToFirstAttribute());
        }

        internal bool MoveToNextAttribute()
        {
            return (!this.isEndOfEmptyElement && this.reader.MoveToNextAttribute());
        }

        private XmlQualifiedName ParseQualifiedName(string str)
        {
            string str2;
            string str3;
            if ((str == null) || (str.Length == 0))
            {
                str2 = str3 = string.Empty;
            }
            else
            {
                string str4;
                XmlObjectSerializerReadContext.ParseQualifiedName(str, this, out str2, out str3, out str4);
            }
            return new XmlQualifiedName(str2, str3);
        }

        internal bool Read()
        {
            this.reader.MoveToElement();
            if (!this.reader.IsEmptyElement)
            {
                return this.reader.Read();
            }
            if (this.isEndOfEmptyElement)
            {
                this.isEndOfEmptyElement = false;
                return this.reader.Read();
            }
            this.isEndOfEmptyElement = true;
            return true;
        }

        internal bool ReadAttributeValue()
        {
            return (!this.isEndOfEmptyElement && this.reader.ReadAttributeValue());
        }

        internal object ReadContentAsAnyType(Type valueType)
        {
            switch (Type.GetTypeCode(valueType))
            {
                case TypeCode.Boolean:
                    return this.ReadContentAsBoolean();

                case TypeCode.Char:
                    return this.ReadContentAsChar();

                case TypeCode.SByte:
                    return this.ReadContentAsSignedByte();

                case TypeCode.Byte:
                    return this.ReadContentAsUnsignedByte();

                case TypeCode.Int16:
                    return this.ReadContentAsShort();

                case TypeCode.UInt16:
                    return this.ReadContentAsUnsignedShort();

                case TypeCode.Int32:
                    return this.ReadContentAsInt();

                case TypeCode.UInt32:
                    return this.ReadContentAsUnsignedInt();

                case TypeCode.Int64:
                    return this.ReadContentAsLong();

                case TypeCode.UInt64:
                    return this.ReadContentAsUnsignedLong();

                case TypeCode.Single:
                    return this.ReadContentAsSingle();

                case TypeCode.Double:
                    return this.ReadContentAsDouble();

                case TypeCode.Decimal:
                    return this.ReadContentAsDecimal();

                case TypeCode.DateTime:
                    return this.ReadContentAsDateTime();

                case TypeCode.String:
                    return this.ReadContentAsString();
            }
            if (valueType == Globals.TypeOfByteArray)
            {
                return this.ReadContentAsBase64();
            }
            if (valueType == Globals.TypeOfObject)
            {
                return new object();
            }
            if (valueType == Globals.TypeOfTimeSpan)
            {
                return this.ReadContentAsTimeSpan();
            }
            if (valueType == Globals.TypeOfGuid)
            {
                return this.ReadContentAsGuid();
            }
            if (valueType == Globals.TypeOfUri)
            {
                return this.ReadContentAsUri();
            }
            if (valueType != Globals.TypeOfXmlQualifiedName)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(this.CreateInvalidPrimitiveTypeException(valueType));
            }
            return this.ReadContentAsQName();
        }

        internal virtual byte[] ReadContentAsBase64()
        {
            if (this.isEndOfEmptyElement)
            {
                return new byte[0];
            }
            if (this.dictionaryReader == null)
            {
                return this.ReadContentAsBase64(this.reader.ReadContentAsString());
            }
            return this.dictionaryReader.ReadContentAsBase64();
        }

        internal byte[] ReadContentAsBase64(string str)
        {
            byte[] buffer;
            if (str == null)
            {
                return null;
            }
            str = str.Trim();
            if (str.Length == 0)
            {
                return new byte[0];
            }
            try
            {
                buffer = Convert.FromBase64String(str);
            }
            catch (ArgumentException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(str, "byte[]", exception));
            }
            catch (FormatException exception2)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(str, "byte[]", exception2));
            }
            return buffer;
        }

        internal bool ReadContentAsBoolean()
        {
            if (this.isEndOfEmptyElement)
            {
                this.ThrowConversionException(string.Empty, "Boolean");
            }
            return this.reader.ReadContentAsBoolean();
        }

        internal virtual char ReadContentAsChar()
        {
            return this.ToChar(this.ReadContentAsInt());
        }

        internal virtual DateTime ReadContentAsDateTime()
        {
            if (this.isEndOfEmptyElement)
            {
                this.ThrowConversionException(string.Empty, "DateTime");
            }
            return this.reader.ReadContentAsDateTime();
        }

        internal decimal ReadContentAsDecimal()
        {
            if (this.isEndOfEmptyElement)
            {
                this.ThrowConversionException(string.Empty, "Decimal");
            }
            return this.reader.ReadContentAsDecimal();
        }

        internal double ReadContentAsDouble()
        {
            if (this.isEndOfEmptyElement)
            {
                this.ThrowConversionException(string.Empty, "Double");
            }
            return this.reader.ReadContentAsDouble();
        }

        internal Guid ReadContentAsGuid()
        {
            Guid guid;
            string input = this.reader.ReadContentAsString();
            try
            {
                guid = Guid.Parse(input);
            }
            catch (ArgumentException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(input, "Guid", exception));
            }
            catch (FormatException exception2)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(input, "Guid", exception2));
            }
            catch (OverflowException exception3)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(input, "Guid", exception3));
            }
            return guid;
        }

        internal int ReadContentAsInt()
        {
            if (this.isEndOfEmptyElement)
            {
                this.ThrowConversionException(string.Empty, "Int32");
            }
            return this.reader.ReadContentAsInt();
        }

        internal long ReadContentAsLong()
        {
            if (this.isEndOfEmptyElement)
            {
                this.ThrowConversionException(string.Empty, "Int64");
            }
            return this.reader.ReadContentAsLong();
        }

        internal virtual XmlQualifiedName ReadContentAsQName()
        {
            return this.ParseQualifiedName(this.ReadContentAsString());
        }

        internal short ReadContentAsShort()
        {
            return this.ToShort(this.ReadContentAsInt());
        }

        internal sbyte ReadContentAsSignedByte()
        {
            return this.ToSByte(this.ReadContentAsInt());
        }

        internal float ReadContentAsSingle()
        {
            if (this.isEndOfEmptyElement)
            {
                this.ThrowConversionException(string.Empty, "Float");
            }
            return this.reader.ReadContentAsFloat();
        }

        internal string ReadContentAsString()
        {
            if (!this.isEndOfEmptyElement)
            {
                return this.reader.ReadContentAsString();
            }
            return string.Empty;
        }

        internal TimeSpan ReadContentAsTimeSpan()
        {
            return XmlConverter.ToTimeSpan(this.reader.ReadContentAsString());
        }

        internal byte ReadContentAsUnsignedByte()
        {
            return this.ToByte(this.ReadContentAsInt());
        }

        internal uint ReadContentAsUnsignedInt()
        {
            return this.ToUInt32(this.ReadContentAsLong());
        }

        internal virtual ulong ReadContentAsUnsignedLong()
        {
            string str = this.reader.ReadContentAsString();
            if ((str == null) || (str.Length == 0))
            {
                this.ThrowConversionException(string.Empty, "UInt64");
            }
            return XmlConverter.ToUInt64(str);
        }

        internal ushort ReadContentAsUnsignedShort()
        {
            return this.ToUInt16(this.ReadContentAsInt());
        }

        internal Uri ReadContentAsUri()
        {
            Uri uri;
            string uriString = this.ReadContentAsString();
            try
            {
                uri = new Uri(uriString, UriKind.RelativeOrAbsolute);
            }
            catch (ArgumentException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(uriString, "Uri", exception));
            }
            catch (FormatException exception2)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(uriString, "Uri", exception2));
            }
            return uri;
        }

        public object ReadElementContentAsAnyType(Type valueType)
        {
            this.Read();
            object obj2 = this.ReadContentAsAnyType(valueType);
            this.ReadEndElement();
            return obj2;
        }

        internal virtual byte[] ReadElementContentAsBase64()
        {
            if (this.isEndOfEmptyElement)
            {
                this.ThrowNotAtElement();
            }
            if (this.dictionaryReader == null)
            {
                return this.ReadContentAsBase64(this.reader.ReadElementContentAsString());
            }
            return this.dictionaryReader.ReadElementContentAsBase64();
        }

        public bool ReadElementContentAsBoolean()
        {
            if (this.isEndOfEmptyElement)
            {
                this.ThrowNotAtElement();
            }
            return this.reader.ReadElementContentAsBoolean();
        }

        internal virtual char ReadElementContentAsChar()
        {
            return this.ToChar(this.ReadElementContentAsInt());
        }

        internal virtual DateTime ReadElementContentAsDateTime()
        {
            if (this.isEndOfEmptyElement)
            {
                this.ThrowNotAtElement();
            }
            return this.reader.ReadElementContentAsDateTime();
        }

        public decimal ReadElementContentAsDecimal()
        {
            if (this.isEndOfEmptyElement)
            {
                this.ThrowNotAtElement();
            }
            return this.reader.ReadElementContentAsDecimal();
        }

        public double ReadElementContentAsDouble()
        {
            if (this.isEndOfEmptyElement)
            {
                this.ThrowNotAtElement();
            }
            return this.reader.ReadElementContentAsDouble();
        }

        public float ReadElementContentAsFloat()
        {
            if (this.isEndOfEmptyElement)
            {
                this.ThrowNotAtElement();
            }
            return this.reader.ReadElementContentAsFloat();
        }

        public Guid ReadElementContentAsGuid()
        {
            Guid guid;
            if (this.isEndOfEmptyElement)
            {
                this.ThrowNotAtElement();
            }
            string input = this.reader.ReadElementContentAsString();
            try
            {
                guid = Guid.Parse(input);
            }
            catch (ArgumentException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(input, "Guid", exception));
            }
            catch (FormatException exception2)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(input, "Guid", exception2));
            }
            catch (OverflowException exception3)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(input, "Guid", exception3));
            }
            return guid;
        }

        public int ReadElementContentAsInt()
        {
            if (this.isEndOfEmptyElement)
            {
                this.ThrowNotAtElement();
            }
            return this.reader.ReadElementContentAsInt();
        }

        public long ReadElementContentAsLong()
        {
            if (this.isEndOfEmptyElement)
            {
                this.ThrowNotAtElement();
            }
            return this.reader.ReadElementContentAsLong();
        }

        public XmlQualifiedName ReadElementContentAsQName()
        {
            this.Read();
            XmlQualifiedName name = this.ReadContentAsQName();
            this.ReadEndElement();
            return name;
        }

        public short ReadElementContentAsShort()
        {
            return this.ToShort(this.ReadElementContentAsInt());
        }

        public sbyte ReadElementContentAsSignedByte()
        {
            return this.ToSByte(this.ReadElementContentAsInt());
        }

        public string ReadElementContentAsString()
        {
            if (this.isEndOfEmptyElement)
            {
                this.ThrowNotAtElement();
            }
            return this.reader.ReadElementContentAsString();
        }

        public TimeSpan ReadElementContentAsTimeSpan()
        {
            if (this.isEndOfEmptyElement)
            {
                this.ThrowNotAtElement();
            }
            return XmlConverter.ToTimeSpan(this.reader.ReadElementContentAsString());
        }

        public byte ReadElementContentAsUnsignedByte()
        {
            return this.ToByte(this.ReadElementContentAsInt());
        }

        public uint ReadElementContentAsUnsignedInt()
        {
            return this.ToUInt32(this.ReadElementContentAsLong());
        }

        internal virtual ulong ReadElementContentAsUnsignedLong()
        {
            if (this.isEndOfEmptyElement)
            {
                this.ThrowNotAtElement();
            }
            string str = this.reader.ReadElementContentAsString();
            if ((str == null) || (str.Length == 0))
            {
                this.ThrowConversionException(string.Empty, "UInt64");
            }
            return XmlConverter.ToUInt64(str);
        }

        public ushort ReadElementContentAsUnsignedShort()
        {
            return this.ToUInt16(this.ReadElementContentAsInt());
        }

        public Uri ReadElementContentAsUri()
        {
            Uri uri;
            if (this.isEndOfEmptyElement)
            {
                this.ThrowNotAtElement();
            }
            string uriString = this.ReadElementContentAsString();
            try
            {
                uri = new Uri(uriString, UriKind.RelativeOrAbsolute);
            }
            catch (ArgumentException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(uriString, "Uri", exception));
            }
            catch (FormatException exception2)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(uriString, "Uri", exception2));
            }
            return uri;
        }

        public void ReadEndElement()
        {
            if (this.isEndOfEmptyElement)
            {
                this.Read();
            }
            else
            {
                this.reader.ReadEndElement();
            }
        }

        internal IDataNode ReadExtensionData(Type valueType)
        {
            switch (Type.GetTypeCode(valueType))
            {
                case TypeCode.Boolean:
                    return new DataNode<bool>(this.ReadContentAsBoolean());

                case TypeCode.Char:
                    return new DataNode<char>(this.ReadContentAsChar());

                case TypeCode.SByte:
                    return new DataNode<sbyte>(this.ReadContentAsSignedByte());

                case TypeCode.Byte:
                    return new DataNode<byte>(this.ReadContentAsUnsignedByte());

                case TypeCode.Int16:
                    return new DataNode<short>(this.ReadContentAsShort());

                case TypeCode.UInt16:
                    return new DataNode<ushort>(this.ReadContentAsUnsignedShort());

                case TypeCode.Int32:
                    return new DataNode<int>(this.ReadContentAsInt());

                case TypeCode.UInt32:
                    return new DataNode<uint>(this.ReadContentAsUnsignedInt());

                case TypeCode.Int64:
                    return new DataNode<long>(this.ReadContentAsLong());

                case TypeCode.UInt64:
                    return new DataNode<ulong>(this.ReadContentAsUnsignedLong());

                case TypeCode.Single:
                    return new DataNode<float>(this.ReadContentAsSingle());

                case TypeCode.Double:
                    return new DataNode<double>(this.ReadContentAsDouble());

                case TypeCode.Decimal:
                    return new DataNode<decimal>(this.ReadContentAsDecimal());

                case TypeCode.DateTime:
                    return new DataNode<DateTime>(this.ReadContentAsDateTime());

                case TypeCode.String:
                    return new DataNode<string>(this.ReadContentAsString());
            }
            if (valueType == Globals.TypeOfByteArray)
            {
                return new DataNode<byte[]>(this.ReadContentAsBase64());
            }
            if (valueType == Globals.TypeOfObject)
            {
                return new DataNode<object>(new object());
            }
            if (valueType == Globals.TypeOfTimeSpan)
            {
                return new DataNode<TimeSpan>(this.ReadContentAsTimeSpan());
            }
            if (valueType == Globals.TypeOfGuid)
            {
                return new DataNode<Guid>(this.ReadContentAsGuid());
            }
            if (valueType == Globals.TypeOfUri)
            {
                return new DataNode<Uri>(this.ReadContentAsUri());
            }
            if (valueType != Globals.TypeOfXmlQualifiedName)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(this.CreateInvalidPrimitiveTypeException(valueType));
            }
            return new DataNode<XmlQualifiedName>(this.ReadContentAsQName());
        }

        internal void Skip()
        {
            this.reader.Skip();
            this.isEndOfEmptyElement = false;
        }

        private void ThrowConversionException(string value, string type)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(XmlObjectSerializer.TryAddLineInfo(this, System.Runtime.Serialization.SR.GetString("XmlInvalidConversion", new object[] { value, type }))));
        }

        private void ThrowNotAtElement()
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("XmlStartElementExpected", new object[] { "EndElement" })));
        }

        private byte ToByte(int value)
        {
            if ((value < 0) || (value > 0xff))
            {
                this.ThrowConversionException(value.ToString(NumberFormatInfo.CurrentInfo), "Byte");
            }
            return (byte) value;
        }

        private char ToChar(int value)
        {
            if ((value < 0) || (value > 0xffff))
            {
                this.ThrowConversionException(value.ToString(NumberFormatInfo.CurrentInfo), "Char");
            }
            return (char) value;
        }

        private sbyte ToSByte(int value)
        {
            if ((value < -128) || (value > 0x7f))
            {
                this.ThrowConversionException(value.ToString(NumberFormatInfo.CurrentInfo), "SByte");
            }
            return (sbyte) value;
        }

        private short ToShort(int value)
        {
            if ((value < -32768) || (value > 0x7fff))
            {
                this.ThrowConversionException(value.ToString(NumberFormatInfo.CurrentInfo), "Int16");
            }
            return (short) value;
        }

        private ushort ToUInt16(int value)
        {
            if ((value < 0) || (value > 0xffff))
            {
                this.ThrowConversionException(value.ToString(NumberFormatInfo.CurrentInfo), "UInt16");
            }
            return (ushort) value;
        }

        private uint ToUInt32(long value)
        {
            if ((value < 0L) || (value > 0xffffffffL))
            {
                this.ThrowConversionException(value.ToString(NumberFormatInfo.CurrentInfo), "UInt32");
            }
            return (uint) value;
        }

        internal bool TryReadBooleanArray(XmlObjectSerializerReadContext context, XmlDictionaryString itemName, XmlDictionaryString itemNamespace, int arrayLength, out bool[] array)
        {
            if (this.dictionaryReader == null)
            {
                array = null;
                return false;
            }
            if (arrayLength != -1)
            {
                this.CheckExpectedArrayLength(context, arrayLength);
                array = new bool[arrayLength];
                int num = 0;
                int offset = 0;
                while ((num = this.dictionaryReader.ReadArray(itemName, itemNamespace, array, offset, arrayLength - offset)) > 0)
                {
                    offset += num;
                }
                this.CheckActualArrayLength(arrayLength, offset, itemName, itemNamespace);
            }
            else
            {
                array = BooleanArrayHelperWithDictionaryString.Instance.ReadArray(this.dictionaryReader, itemName, itemNamespace, this.GetArrayLengthQuota(context));
                context.IncrementItemCount(array.Length);
            }
            return true;
        }

        internal bool TryReadDateTimeArray(XmlObjectSerializerReadContext context, XmlDictionaryString itemName, XmlDictionaryString itemNamespace, int arrayLength, out DateTime[] array)
        {
            if (this.dictionaryReader == null)
            {
                array = null;
                return false;
            }
            if (arrayLength != -1)
            {
                this.CheckExpectedArrayLength(context, arrayLength);
                array = new DateTime[arrayLength];
                int num = 0;
                int offset = 0;
                while ((num = this.dictionaryReader.ReadArray(itemName, itemNamespace, array, offset, arrayLength - offset)) > 0)
                {
                    offset += num;
                }
                this.CheckActualArrayLength(arrayLength, offset, itemName, itemNamespace);
            }
            else
            {
                array = DateTimeArrayHelperWithDictionaryString.Instance.ReadArray(this.dictionaryReader, itemName, itemNamespace, this.GetArrayLengthQuota(context));
                context.IncrementItemCount(array.Length);
            }
            return true;
        }

        internal bool TryReadDecimalArray(XmlObjectSerializerReadContext context, XmlDictionaryString itemName, XmlDictionaryString itemNamespace, int arrayLength, out decimal[] array)
        {
            if (this.dictionaryReader == null)
            {
                array = null;
                return false;
            }
            if (arrayLength != -1)
            {
                this.CheckExpectedArrayLength(context, arrayLength);
                array = new decimal[arrayLength];
                int num = 0;
                int offset = 0;
                while ((num = this.dictionaryReader.ReadArray(itemName, itemNamespace, array, offset, arrayLength - offset)) > 0)
                {
                    offset += num;
                }
                this.CheckActualArrayLength(arrayLength, offset, itemName, itemNamespace);
            }
            else
            {
                array = DecimalArrayHelperWithDictionaryString.Instance.ReadArray(this.dictionaryReader, itemName, itemNamespace, this.GetArrayLengthQuota(context));
                context.IncrementItemCount(array.Length);
            }
            return true;
        }

        internal bool TryReadDoubleArray(XmlObjectSerializerReadContext context, XmlDictionaryString itemName, XmlDictionaryString itemNamespace, int arrayLength, out double[] array)
        {
            if (this.dictionaryReader == null)
            {
                array = null;
                return false;
            }
            if (arrayLength != -1)
            {
                this.CheckExpectedArrayLength(context, arrayLength);
                array = new double[arrayLength];
                int num = 0;
                int offset = 0;
                while ((num = this.dictionaryReader.ReadArray(itemName, itemNamespace, array, offset, arrayLength - offset)) > 0)
                {
                    offset += num;
                }
                this.CheckActualArrayLength(arrayLength, offset, itemName, itemNamespace);
            }
            else
            {
                array = DoubleArrayHelperWithDictionaryString.Instance.ReadArray(this.dictionaryReader, itemName, itemNamespace, this.GetArrayLengthQuota(context));
                context.IncrementItemCount(array.Length);
            }
            return true;
        }

        internal bool TryReadInt32Array(XmlObjectSerializerReadContext context, XmlDictionaryString itemName, XmlDictionaryString itemNamespace, int arrayLength, out int[] array)
        {
            if (this.dictionaryReader == null)
            {
                array = null;
                return false;
            }
            if (arrayLength != -1)
            {
                this.CheckExpectedArrayLength(context, arrayLength);
                array = new int[arrayLength];
                int num = 0;
                int offset = 0;
                while ((num = this.dictionaryReader.ReadArray(itemName, itemNamespace, array, offset, arrayLength - offset)) > 0)
                {
                    offset += num;
                }
                this.CheckActualArrayLength(arrayLength, offset, itemName, itemNamespace);
            }
            else
            {
                array = Int32ArrayHelperWithDictionaryString.Instance.ReadArray(this.dictionaryReader, itemName, itemNamespace, this.GetArrayLengthQuota(context));
                context.IncrementItemCount(array.Length);
            }
            return true;
        }

        internal bool TryReadInt64Array(XmlObjectSerializerReadContext context, XmlDictionaryString itemName, XmlDictionaryString itemNamespace, int arrayLength, out long[] array)
        {
            if (this.dictionaryReader == null)
            {
                array = null;
                return false;
            }
            if (arrayLength != -1)
            {
                this.CheckExpectedArrayLength(context, arrayLength);
                array = new long[arrayLength];
                int num = 0;
                int offset = 0;
                while ((num = this.dictionaryReader.ReadArray(itemName, itemNamespace, array, offset, arrayLength - offset)) > 0)
                {
                    offset += num;
                }
                this.CheckActualArrayLength(arrayLength, offset, itemName, itemNamespace);
            }
            else
            {
                array = Int64ArrayHelperWithDictionaryString.Instance.ReadArray(this.dictionaryReader, itemName, itemNamespace, this.GetArrayLengthQuota(context));
                context.IncrementItemCount(array.Length);
            }
            return true;
        }

        internal bool TryReadSingleArray(XmlObjectSerializerReadContext context, XmlDictionaryString itemName, XmlDictionaryString itemNamespace, int arrayLength, out float[] array)
        {
            if (this.dictionaryReader == null)
            {
                array = null;
                return false;
            }
            if (arrayLength != -1)
            {
                this.CheckExpectedArrayLength(context, arrayLength);
                array = new float[arrayLength];
                int num = 0;
                int offset = 0;
                while ((num = this.dictionaryReader.ReadArray(itemName, itemNamespace, array, offset, arrayLength - offset)) > 0)
                {
                    offset += num;
                }
                this.CheckActualArrayLength(arrayLength, offset, itemName, itemNamespace);
            }
            else
            {
                array = SingleArrayHelperWithDictionaryString.Instance.ReadArray(this.dictionaryReader, itemName, itemNamespace, this.GetArrayLengthQuota(context));
                context.IncrementItemCount(array.Length);
            }
            return true;
        }

        internal int AttributeCount
        {
            get
            {
                if (!this.isEndOfEmptyElement)
                {
                    return this.reader.AttributeCount;
                }
                return 0;
            }
        }

        internal int Depth
        {
            get
            {
                return this.reader.Depth;
            }
        }

        internal bool EOF
        {
            get
            {
                return this.reader.EOF;
            }
        }

        internal bool IsEmptyElement
        {
            get
            {
                return false;
            }
        }

        internal int LineNumber
        {
            get
            {
                IXmlLineInfo reader = this.reader as IXmlLineInfo;
                if (reader != null)
                {
                    return reader.LineNumber;
                }
                return 0;
            }
        }

        internal int LinePosition
        {
            get
            {
                IXmlLineInfo reader = this.reader as IXmlLineInfo;
                if (reader != null)
                {
                    return reader.LinePosition;
                }
                return 0;
            }
        }

        internal string LocalName
        {
            get
            {
                return this.reader.LocalName;
            }
        }

        internal string Name
        {
            get
            {
                return this.reader.Name;
            }
        }

        internal string NamespaceURI
        {
            get
            {
                return this.reader.NamespaceURI;
            }
        }

        public XmlNodeType NodeType
        {
            get
            {
                if (!this.isEndOfEmptyElement)
                {
                    return this.reader.NodeType;
                }
                return XmlNodeType.EndElement;
            }
        }

        internal bool Normalized
        {
            get
            {
                XmlTextReader reader = this.reader as XmlTextReader;
                if (reader != null)
                {
                    return reader.Normalization;
                }
                IXmlTextParser parser = this.reader as IXmlTextParser;
                return ((parser != null) && parser.Normalized);
            }
            set
            {
                XmlTextReader reader = this.reader as XmlTextReader;
                if (reader == null)
                {
                    IXmlTextParser parser = this.reader as IXmlTextParser;
                    if (parser != null)
                    {
                        parser.Normalized = value;
                    }
                }
                else
                {
                    reader.Normalization = value;
                }
            }
        }

        internal ExtensionDataReader UnderlyingExtensionDataReader
        {
            get
            {
                return (this.reader as ExtensionDataReader);
            }
        }

        internal XmlReader UnderlyingReader
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.reader;
            }
        }

        internal string Value
        {
            get
            {
                return this.reader.Value;
            }
        }

        internal Type ValueType
        {
            get
            {
                return this.reader.ValueType;
            }
        }

        internal System.Xml.WhitespaceHandling WhitespaceHandling
        {
            get
            {
                XmlTextReader reader = this.reader as XmlTextReader;
                if (reader != null)
                {
                    return reader.WhitespaceHandling;
                }
                IXmlTextParser parser = this.reader as IXmlTextParser;
                if (parser != null)
                {
                    return parser.WhitespaceHandling;
                }
                return System.Xml.WhitespaceHandling.None;
            }
            set
            {
                XmlTextReader reader = this.reader as XmlTextReader;
                if (reader == null)
                {
                    IXmlTextParser parser = this.reader as IXmlTextParser;
                    if (parser != null)
                    {
                        parser.WhitespaceHandling = value;
                    }
                }
                else
                {
                    reader.WhitespaceHandling = value;
                }
            }
        }
    }
}

