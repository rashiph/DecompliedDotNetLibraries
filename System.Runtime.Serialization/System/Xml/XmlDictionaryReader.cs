namespace System.Xml
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Text;

    public abstract class XmlDictionaryReader : XmlReader
    {
        internal const int MaxInitialArrayLength = 0xffff;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected XmlDictionaryReader()
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

        public static XmlDictionaryReader CreateBinaryReader(byte[] buffer, XmlDictionaryReaderQuotas quotas)
        {
            if (buffer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("buffer");
            }
            return CreateBinaryReader(buffer, 0, buffer.Length, quotas);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static XmlDictionaryReader CreateBinaryReader(Stream stream, XmlDictionaryReaderQuotas quotas)
        {
            return CreateBinaryReader(stream, null, quotas);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static XmlDictionaryReader CreateBinaryReader(Stream stream, IXmlDictionary dictionary, XmlDictionaryReaderQuotas quotas)
        {
            return CreateBinaryReader(stream, dictionary, quotas, null);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static XmlDictionaryReader CreateBinaryReader(byte[] buffer, int offset, int count, XmlDictionaryReaderQuotas quotas)
        {
            return CreateBinaryReader(buffer, offset, count, null, quotas);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static XmlDictionaryReader CreateBinaryReader(Stream stream, IXmlDictionary dictionary, XmlDictionaryReaderQuotas quotas, XmlBinaryReaderSession session)
        {
            return CreateBinaryReader(stream, dictionary, quotas, session, null);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static XmlDictionaryReader CreateBinaryReader(byte[] buffer, int offset, int count, IXmlDictionary dictionary, XmlDictionaryReaderQuotas quotas)
        {
            return CreateBinaryReader(buffer, offset, count, dictionary, quotas, null);
        }

        public static XmlDictionaryReader CreateBinaryReader(Stream stream, IXmlDictionary dictionary, XmlDictionaryReaderQuotas quotas, XmlBinaryReaderSession session, OnXmlDictionaryReaderClose onClose)
        {
            XmlBinaryReader reader = new XmlBinaryReader();
            reader.SetInput(stream, dictionary, quotas, session, onClose);
            return reader;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static XmlDictionaryReader CreateBinaryReader(byte[] buffer, int offset, int count, IXmlDictionary dictionary, XmlDictionaryReaderQuotas quotas, XmlBinaryReaderSession session)
        {
            return CreateBinaryReader(buffer, offset, count, dictionary, quotas, session, null);
        }

        public static XmlDictionaryReader CreateBinaryReader(byte[] buffer, int offset, int count, IXmlDictionary dictionary, XmlDictionaryReaderQuotas quotas, XmlBinaryReaderSession session, OnXmlDictionaryReaderClose onClose)
        {
            XmlBinaryReader reader = new XmlBinaryReader();
            reader.SetInput(buffer, offset, count, dictionary, quotas, session, onClose);
            return reader;
        }

        public static XmlDictionaryReader CreateDictionaryReader(XmlReader reader)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }
            XmlDictionaryReader reader2 = reader as XmlDictionaryReader;
            if (reader2 == null)
            {
                reader2 = new XmlWrappedReader(reader, null);
            }
            return reader2;
        }

        public static XmlDictionaryReader CreateMtomReader(Stream stream, Encoding encoding, XmlDictionaryReaderQuotas quotas)
        {
            if (encoding == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("encoding");
            }
            return CreateMtomReader(stream, new Encoding[] { encoding }, quotas);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static XmlDictionaryReader CreateMtomReader(Stream stream, Encoding[] encodings, XmlDictionaryReaderQuotas quotas)
        {
            return CreateMtomReader(stream, encodings, null, quotas);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static XmlDictionaryReader CreateMtomReader(Stream stream, Encoding[] encodings, string contentType, XmlDictionaryReaderQuotas quotas)
        {
            return CreateMtomReader(stream, encodings, contentType, quotas, 0x7fffffff, null);
        }

        public static XmlDictionaryReader CreateMtomReader(byte[] buffer, int offset, int count, Encoding encoding, XmlDictionaryReaderQuotas quotas)
        {
            if (encoding == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("encoding");
            }
            return CreateMtomReader(buffer, offset, count, new Encoding[] { encoding }, quotas);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static XmlDictionaryReader CreateMtomReader(byte[] buffer, int offset, int count, Encoding[] encodings, XmlDictionaryReaderQuotas quotas)
        {
            return CreateMtomReader(buffer, offset, count, encodings, null, quotas);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static XmlDictionaryReader CreateMtomReader(byte[] buffer, int offset, int count, Encoding[] encodings, string contentType, XmlDictionaryReaderQuotas quotas)
        {
            return CreateMtomReader(buffer, offset, count, encodings, contentType, quotas, 0x7fffffff, null);
        }

        public static XmlDictionaryReader CreateMtomReader(Stream stream, Encoding[] encodings, string contentType, XmlDictionaryReaderQuotas quotas, int maxBufferSize, OnXmlDictionaryReaderClose onClose)
        {
            XmlMtomReader reader = new XmlMtomReader();
            reader.SetInput(stream, encodings, contentType, quotas, maxBufferSize, onClose);
            return reader;
        }

        public static XmlDictionaryReader CreateMtomReader(byte[] buffer, int offset, int count, Encoding[] encodings, string contentType, XmlDictionaryReaderQuotas quotas, int maxBufferSize, OnXmlDictionaryReaderClose onClose)
        {
            XmlMtomReader reader = new XmlMtomReader();
            reader.SetInput(buffer, offset, count, encodings, contentType, quotas, maxBufferSize, onClose);
            return reader;
        }

        public static XmlDictionaryReader CreateTextReader(byte[] buffer, XmlDictionaryReaderQuotas quotas)
        {
            if (buffer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("buffer");
            }
            return CreateTextReader(buffer, 0, buffer.Length, quotas);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static XmlDictionaryReader CreateTextReader(Stream stream, XmlDictionaryReaderQuotas quotas)
        {
            return CreateTextReader(stream, null, quotas, null);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static XmlDictionaryReader CreateTextReader(byte[] buffer, int offset, int count, XmlDictionaryReaderQuotas quotas)
        {
            return CreateTextReader(buffer, offset, count, null, quotas, null);
        }

        public static XmlDictionaryReader CreateTextReader(Stream stream, Encoding encoding, XmlDictionaryReaderQuotas quotas, OnXmlDictionaryReaderClose onClose)
        {
            XmlUTF8TextReader reader = new XmlUTF8TextReader();
            reader.SetInput(stream, encoding, quotas, onClose);
            return reader;
        }

        public static XmlDictionaryReader CreateTextReader(byte[] buffer, int offset, int count, Encoding encoding, XmlDictionaryReaderQuotas quotas, OnXmlDictionaryReaderClose onClose)
        {
            XmlUTF8TextReader reader = new XmlUTF8TextReader();
            reader.SetInput(buffer, offset, count, encoding, quotas, onClose);
            return reader;
        }

        public virtual void EndCanonicalization()
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
        }

        public virtual string GetAttribute(XmlDictionaryString localName, XmlDictionaryString namespaceUri)
        {
            return this.GetAttribute(XmlDictionaryString.GetString(localName), XmlDictionaryString.GetString(namespaceUri));
        }

        public virtual void GetNonAtomizedNames(out string localName, out string namespaceUri)
        {
            localName = this.LocalName;
            namespaceUri = this.NamespaceURI;
        }

        public virtual int IndexOfLocalName(string[] localNames, string namespaceUri)
        {
            if (localNames == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("localNames");
            }
            if (namespaceUri == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("namespaceUri");
            }
            if (this.NamespaceURI == namespaceUri)
            {
                string localName = this.LocalName;
                for (int i = 0; i < localNames.Length; i++)
                {
                    string str2 = localNames[i];
                    if (str2 == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(string.Format(CultureInfo.InvariantCulture, "localNames[{0}]", new object[] { i }));
                    }
                    if (localName == str2)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        public virtual int IndexOfLocalName(XmlDictionaryString[] localNames, XmlDictionaryString namespaceUri)
        {
            if (localNames == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("localNames");
            }
            if (namespaceUri == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("namespaceUri");
            }
            if (this.NamespaceURI == namespaceUri.Value)
            {
                string localName = this.LocalName;
                for (int i = 0; i < localNames.Length; i++)
                {
                    XmlDictionaryString str2 = localNames[i];
                    if (str2 == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(string.Format(CultureInfo.InvariantCulture, "localNames[{0}]", new object[] { i }));
                    }
                    if (localName == str2.Value)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        public virtual bool IsLocalName(string localName)
        {
            return (this.LocalName == localName);
        }

        public virtual bool IsLocalName(XmlDictionaryString localName)
        {
            if (localName == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("localName");
            }
            return this.IsLocalName(localName.Value);
        }

        public virtual bool IsNamespaceUri(string namespaceUri)
        {
            if (namespaceUri == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("namespaceUri");
            }
            return (this.NamespaceURI == namespaceUri);
        }

        public virtual bool IsNamespaceUri(XmlDictionaryString namespaceUri)
        {
            if (namespaceUri == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("namespaceUri");
            }
            return this.IsNamespaceUri(namespaceUri.Value);
        }

        public virtual bool IsStartArray(out Type type)
        {
            type = null;
            return false;
        }

        public virtual bool IsStartElement(XmlDictionaryString localName, XmlDictionaryString namespaceUri)
        {
            return this.IsStartElement(XmlDictionaryString.GetString(localName), XmlDictionaryString.GetString(namespaceUri));
        }

        protected bool IsTextNode(XmlNodeType nodeType)
        {
            if (((nodeType != XmlNodeType.Text) && (nodeType != XmlNodeType.Whitespace)) && ((nodeType != XmlNodeType.SignificantWhitespace) && (nodeType != XmlNodeType.CDATA)))
            {
                return (nodeType == XmlNodeType.Attribute);
            }
            return true;
        }

        public virtual void MoveToStartElement()
        {
            if (!this.IsStartElement())
            {
                XmlExceptionHelper.ThrowStartElementExpected(this);
            }
        }

        public virtual void MoveToStartElement(string name)
        {
            if (!this.IsStartElement(name))
            {
                XmlExceptionHelper.ThrowStartElementExpected(this, name);
            }
        }

        public virtual void MoveToStartElement(string localName, string namespaceUri)
        {
            if (!this.IsStartElement(localName, namespaceUri))
            {
                XmlExceptionHelper.ThrowStartElementExpected(this, localName, namespaceUri);
            }
        }

        public virtual void MoveToStartElement(XmlDictionaryString localName, XmlDictionaryString namespaceUri)
        {
            if (!this.IsStartElement(localName, namespaceUri))
            {
                XmlExceptionHelper.ThrowStartElementExpected(this, localName, namespaceUri);
            }
        }

        public virtual int ReadArray(string localName, string namespaceUri, bool[] array, int offset, int count)
        {
            this.CheckArray(array, offset, count);
            int num = 0;
            while ((num < count) && this.IsStartElement(localName, namespaceUri))
            {
                array[offset + num] = this.ReadElementContentAsBoolean();
                num++;
            }
            return num;
        }

        public virtual int ReadArray(string localName, string namespaceUri, DateTime[] array, int offset, int count)
        {
            this.CheckArray(array, offset, count);
            int num = 0;
            while ((num < count) && this.IsStartElement(localName, namespaceUri))
            {
                array[offset + num] = this.ReadElementContentAsDateTime();
                num++;
            }
            return num;
        }

        public virtual int ReadArray(string localName, string namespaceUri, decimal[] array, int offset, int count)
        {
            this.CheckArray(array, offset, count);
            int num = 0;
            while ((num < count) && this.IsStartElement(localName, namespaceUri))
            {
                array[offset + num] = this.ReadElementContentAsDecimal();
                num++;
            }
            return num;
        }

        public virtual int ReadArray(string localName, string namespaceUri, double[] array, int offset, int count)
        {
            this.CheckArray(array, offset, count);
            int num = 0;
            while ((num < count) && this.IsStartElement(localName, namespaceUri))
            {
                array[offset + num] = this.ReadElementContentAsDouble();
                num++;
            }
            return num;
        }

        public virtual int ReadArray(string localName, string namespaceUri, Guid[] array, int offset, int count)
        {
            this.CheckArray(array, offset, count);
            int num = 0;
            while ((num < count) && this.IsStartElement(localName, namespaceUri))
            {
                array[offset + num] = this.ReadElementContentAsGuid();
                num++;
            }
            return num;
        }

        public virtual int ReadArray(string localName, string namespaceUri, short[] array, int offset, int count)
        {
            this.CheckArray(array, offset, count);
            int num = 0;
            while ((num < count) && this.IsStartElement(localName, namespaceUri))
            {
                int num2 = this.ReadElementContentAsInt();
                if ((num2 < -32768) || (num2 > 0x7fff))
                {
                    XmlExceptionHelper.ThrowConversionOverflow(this, num2.ToString(NumberFormatInfo.CurrentInfo), "Int16");
                }
                array[offset + num] = (short) num2;
                num++;
            }
            return num;
        }

        public virtual int ReadArray(string localName, string namespaceUri, int[] array, int offset, int count)
        {
            this.CheckArray(array, offset, count);
            int num = 0;
            while ((num < count) && this.IsStartElement(localName, namespaceUri))
            {
                array[offset + num] = this.ReadElementContentAsInt();
                num++;
            }
            return num;
        }

        public virtual int ReadArray(string localName, string namespaceUri, long[] array, int offset, int count)
        {
            this.CheckArray(array, offset, count);
            int num = 0;
            while ((num < count) && this.IsStartElement(localName, namespaceUri))
            {
                array[offset + num] = this.ReadElementContentAsLong();
                num++;
            }
            return num;
        }

        public virtual int ReadArray(string localName, string namespaceUri, float[] array, int offset, int count)
        {
            this.CheckArray(array, offset, count);
            int num = 0;
            while ((num < count) && this.IsStartElement(localName, namespaceUri))
            {
                array[offset + num] = this.ReadElementContentAsFloat();
                num++;
            }
            return num;
        }

        public virtual int ReadArray(string localName, string namespaceUri, TimeSpan[] array, int offset, int count)
        {
            this.CheckArray(array, offset, count);
            int num = 0;
            while ((num < count) && this.IsStartElement(localName, namespaceUri))
            {
                array[offset + num] = this.ReadElementContentAsTimeSpan();
                num++;
            }
            return num;
        }

        public virtual int ReadArray(XmlDictionaryString localName, XmlDictionaryString namespaceUri, bool[] array, int offset, int count)
        {
            return this.ReadArray(XmlDictionaryString.GetString(localName), XmlDictionaryString.GetString(namespaceUri), array, offset, count);
        }

        public virtual int ReadArray(XmlDictionaryString localName, XmlDictionaryString namespaceUri, DateTime[] array, int offset, int count)
        {
            return this.ReadArray(XmlDictionaryString.GetString(localName), XmlDictionaryString.GetString(namespaceUri), array, offset, count);
        }

        public virtual int ReadArray(XmlDictionaryString localName, XmlDictionaryString namespaceUri, decimal[] array, int offset, int count)
        {
            return this.ReadArray(XmlDictionaryString.GetString(localName), XmlDictionaryString.GetString(namespaceUri), array, offset, count);
        }

        public virtual int ReadArray(XmlDictionaryString localName, XmlDictionaryString namespaceUri, double[] array, int offset, int count)
        {
            return this.ReadArray(XmlDictionaryString.GetString(localName), XmlDictionaryString.GetString(namespaceUri), array, offset, count);
        }

        public virtual int ReadArray(XmlDictionaryString localName, XmlDictionaryString namespaceUri, Guid[] array, int offset, int count)
        {
            return this.ReadArray(XmlDictionaryString.GetString(localName), XmlDictionaryString.GetString(namespaceUri), array, offset, count);
        }

        public virtual int ReadArray(XmlDictionaryString localName, XmlDictionaryString namespaceUri, short[] array, int offset, int count)
        {
            return this.ReadArray(XmlDictionaryString.GetString(localName), XmlDictionaryString.GetString(namespaceUri), array, offset, count);
        }

        public virtual int ReadArray(XmlDictionaryString localName, XmlDictionaryString namespaceUri, int[] array, int offset, int count)
        {
            return this.ReadArray(XmlDictionaryString.GetString(localName), XmlDictionaryString.GetString(namespaceUri), array, offset, count);
        }

        public virtual int ReadArray(XmlDictionaryString localName, XmlDictionaryString namespaceUri, long[] array, int offset, int count)
        {
            return this.ReadArray(XmlDictionaryString.GetString(localName), XmlDictionaryString.GetString(namespaceUri), array, offset, count);
        }

        public virtual int ReadArray(XmlDictionaryString localName, XmlDictionaryString namespaceUri, float[] array, int offset, int count)
        {
            return this.ReadArray(XmlDictionaryString.GetString(localName), XmlDictionaryString.GetString(namespaceUri), array, offset, count);
        }

        public virtual int ReadArray(XmlDictionaryString localName, XmlDictionaryString namespaceUri, TimeSpan[] array, int offset, int count)
        {
            return this.ReadArray(XmlDictionaryString.GetString(localName), XmlDictionaryString.GetString(namespaceUri), array, offset, count);
        }

        public virtual bool[] ReadBooleanArray(string localName, string namespaceUri)
        {
            return BooleanArrayHelperWithString.Instance.ReadArray(this, localName, namespaceUri, this.Quotas.MaxArrayLength);
        }

        public virtual bool[] ReadBooleanArray(XmlDictionaryString localName, XmlDictionaryString namespaceUri)
        {
            return BooleanArrayHelperWithDictionaryString.Instance.ReadArray(this, localName, namespaceUri, this.Quotas.MaxArrayLength);
        }

        public override object ReadContentAs(Type type, IXmlNamespaceResolver namespaceResolver)
        {
            if (type == typeof(Guid[]))
            {
                string[] strArray = (string[]) this.ReadContentAs(typeof(string[]), namespaceResolver);
                Guid[] guidArray = new Guid[strArray.Length];
                for (int j = 0; j < strArray.Length; j++)
                {
                    guidArray[j] = XmlConverter.ToGuid(strArray[j]);
                }
                return guidArray;
            }
            if (!(type == typeof(UniqueId[])))
            {
                return base.ReadContentAs(type, namespaceResolver);
            }
            string[] strArray2 = (string[]) this.ReadContentAs(typeof(string[]), namespaceResolver);
            UniqueId[] idArray = new UniqueId[strArray2.Length];
            for (int i = 0; i < strArray2.Length; i++)
            {
                idArray[i] = XmlConverter.ToUniqueId(strArray2[i]);
            }
            return idArray;
        }

        public virtual byte[] ReadContentAsBase64()
        {
            return this.ReadContentAsBase64(this.Quotas.MaxArrayLength, 0xffff);
        }

        internal byte[] ReadContentAsBase64(int maxByteArrayContentLength, int maxInitialCount)
        {
            int num;
            if (this.TryGetBase64ContentLength(out num))
            {
                if (num > maxByteArrayContentLength)
                {
                    XmlExceptionHelper.ThrowMaxArrayLengthExceeded(this, maxByteArrayContentLength);
                }
                if (num <= maxInitialCount)
                {
                    int num3;
                    byte[] buffer = new byte[num];
                    for (int i = 0; i < num; i += num3)
                    {
                        num3 = this.ReadContentAsBase64(buffer, i, num - i);
                        if (num3 == 0)
                        {
                            XmlExceptionHelper.ThrowBase64DataExpected(this);
                        }
                    }
                    return buffer;
                }
            }
            return this.ReadContentAsBytes(true, maxByteArrayContentLength);
        }

        public virtual byte[] ReadContentAsBinHex()
        {
            return this.ReadContentAsBinHex(this.Quotas.MaxArrayLength);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected byte[] ReadContentAsBinHex(int maxByteArrayContentLength)
        {
            return this.ReadContentAsBytes(false, maxByteArrayContentLength);
        }

        private byte[] ReadContentAsBytes(bool base64, int maxByteArrayContentLength)
        {
            byte[] buffer;
            byte[][] bufferArray = new byte[0x20][];
            int num = 0x180;
            int num2 = 0;
            int num3 = 0;
        Label_0013:
            buffer = new byte[num];
            bufferArray[num2++] = buffer;
            int index = 0;
            while (index < buffer.Length)
            {
                int num5;
                if (base64)
                {
                    num5 = this.ReadContentAsBase64(buffer, index, buffer.Length - index);
                }
                else
                {
                    num5 = this.ReadContentAsBinHex(buffer, index, buffer.Length - index);
                }
                if (num5 == 0)
                {
                    break;
                }
                index += num5;
            }
            if (num3 > (maxByteArrayContentLength - index))
            {
                XmlExceptionHelper.ThrowMaxArrayLengthExceeded(this, maxByteArrayContentLength);
            }
            num3 += index;
            if (index >= buffer.Length)
            {
                num *= 2;
                goto Label_0013;
            }
            buffer = new byte[num3];
            int dstOffset = 0;
            for (int i = 0; i < (num2 - 1); i++)
            {
                Buffer.BlockCopy(bufferArray[i], 0, buffer, dstOffset, bufferArray[i].Length);
                dstOffset += bufferArray[i].Length;
            }
            Buffer.BlockCopy(bufferArray[num2 - 1], 0, buffer, dstOffset, num3 - dstOffset);
            return buffer;
        }

        public virtual int ReadContentAsChars(char[] chars, int offset, int count)
        {
            XmlNodeType type;
            int num = 0;
        Label_0002:
            type = this.NodeType;
            switch (type)
            {
                case XmlNodeType.Element:
                case XmlNodeType.EndElement:
                    return num;
            }
            if (this.IsTextNode(type))
            {
                num = this.ReadValueChunk(chars, offset, count);
                if (((num > 0) || (type == XmlNodeType.Attribute)) || !this.Read())
                {
                    return num;
                }
                goto Label_0002;
            }
            if (this.Read())
            {
                goto Label_0002;
            }
            return num;
        }

        public override decimal ReadContentAsDecimal()
        {
            return XmlConverter.ToDecimal(this.ReadContentAsString());
        }

        public override float ReadContentAsFloat()
        {
            return XmlConverter.ToSingle(this.ReadContentAsString());
        }

        public virtual Guid ReadContentAsGuid()
        {
            return XmlConverter.ToGuid(this.ReadContentAsString());
        }

        public virtual void ReadContentAsQualifiedName(out string localName, out string namespaceUri)
        {
            string str;
            XmlConverter.ToQualifiedName(this.ReadContentAsString(), out str, out localName);
            namespaceUri = this.LookupNamespace(str);
            if (namespaceUri == null)
            {
                XmlExceptionHelper.ThrowUndefinedPrefix(this, str);
            }
        }

        public override string ReadContentAsString()
        {
            return this.ReadContentAsString(this.Quotas.MaxStringContentLength);
        }

        protected string ReadContentAsString(int maxStringContentLength)
        {
            StringBuilder builder = null;
            string str = string.Empty;
            bool flag = false;
        Label_000A:
            switch (this.NodeType)
            {
                case XmlNodeType.Attribute:
                    str = this.Value;
                    goto Label_00B6;

                case XmlNodeType.Text:
                case XmlNodeType.CDATA:
                case XmlNodeType.Whitespace:
                case XmlNodeType.SignificantWhitespace:
                {
                    string str2 = this.Value;
                    if (str.Length != 0)
                    {
                        if (builder == null)
                        {
                            builder = new StringBuilder(str);
                        }
                        if (builder.Length > (maxStringContentLength - str2.Length))
                        {
                            XmlExceptionHelper.ThrowMaxStringContentLengthExceeded(this, maxStringContentLength);
                        }
                        builder.Append(str2);
                    }
                    else
                    {
                        str = str2;
                    }
                    goto Label_00B6;
                }
                case XmlNodeType.EntityReference:
                    if (!this.CanResolveEntity)
                    {
                        break;
                    }
                    this.ResolveEntity();
                    goto Label_00B6;

                case XmlNodeType.ProcessingInstruction:
                case XmlNodeType.Comment:
                case XmlNodeType.EndEntity:
                    goto Label_00B6;
            }
            flag = true;
        Label_00B6:
            if (!flag)
            {
                if (this.AttributeCount != 0)
                {
                    this.ReadAttributeValue();
                }
                else
                {
                    this.Read();
                }
                goto Label_000A;
            }
            if (builder != null)
            {
                str = builder.ToString();
            }
            if (str.Length > maxStringContentLength)
            {
                XmlExceptionHelper.ThrowMaxStringContentLengthExceeded(this, maxStringContentLength);
            }
            return str;
        }

        public virtual string ReadContentAsString(string[] strings, out int index)
        {
            if (strings == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("strings");
            }
            string str = this.ReadContentAsString();
            index = -1;
            for (int i = 0; i < strings.Length; i++)
            {
                string str2 = strings[i];
                if (str2 == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(string.Format(CultureInfo.InvariantCulture, "strings[{0}]", new object[] { i }));
                }
                if (str2 == str)
                {
                    index = i;
                    return str2;
                }
            }
            return str;
        }

        public virtual string ReadContentAsString(XmlDictionaryString[] strings, out int index)
        {
            if (strings == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("strings");
            }
            string str = this.ReadContentAsString();
            index = -1;
            for (int i = 0; i < strings.Length; i++)
            {
                XmlDictionaryString str2 = strings[i];
                if (str2 == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(string.Format(CultureInfo.InvariantCulture, "strings[{0}]", new object[] { i }));
                }
                if (str2.Value == str)
                {
                    index = i;
                    return str2.Value;
                }
            }
            return str;
        }

        public virtual TimeSpan ReadContentAsTimeSpan()
        {
            return XmlConverter.ToTimeSpan(this.ReadContentAsString());
        }

        public virtual UniqueId ReadContentAsUniqueId()
        {
            return XmlConverter.ToUniqueId(this.ReadContentAsString());
        }

        public virtual DateTime[] ReadDateTimeArray(string localName, string namespaceUri)
        {
            return DateTimeArrayHelperWithString.Instance.ReadArray(this, localName, namespaceUri, this.Quotas.MaxArrayLength);
        }

        public virtual DateTime[] ReadDateTimeArray(XmlDictionaryString localName, XmlDictionaryString namespaceUri)
        {
            return DateTimeArrayHelperWithDictionaryString.Instance.ReadArray(this, localName, namespaceUri, this.Quotas.MaxArrayLength);
        }

        public virtual decimal[] ReadDecimalArray(string localName, string namespaceUri)
        {
            return DecimalArrayHelperWithString.Instance.ReadArray(this, localName, namespaceUri, this.Quotas.MaxArrayLength);
        }

        public virtual decimal[] ReadDecimalArray(XmlDictionaryString localName, XmlDictionaryString namespaceUri)
        {
            return DecimalArrayHelperWithDictionaryString.Instance.ReadArray(this, localName, namespaceUri, this.Quotas.MaxArrayLength);
        }

        public virtual double[] ReadDoubleArray(string localName, string namespaceUri)
        {
            return DoubleArrayHelperWithString.Instance.ReadArray(this, localName, namespaceUri, this.Quotas.MaxArrayLength);
        }

        public virtual double[] ReadDoubleArray(XmlDictionaryString localName, XmlDictionaryString namespaceUri)
        {
            return DoubleArrayHelperWithDictionaryString.Instance.ReadArray(this, localName, namespaceUri, this.Quotas.MaxArrayLength);
        }

        public virtual byte[] ReadElementContentAsBase64()
        {
            if (this.IsStartElement() && this.IsEmptyElement)
            {
                this.Read();
                return new byte[0];
            }
            this.ReadStartElement();
            byte[] buffer = this.ReadContentAsBase64();
            this.ReadEndElement();
            return buffer;
        }

        public virtual byte[] ReadElementContentAsBinHex()
        {
            if (this.IsStartElement() && this.IsEmptyElement)
            {
                this.Read();
                return new byte[0];
            }
            this.ReadStartElement();
            byte[] buffer = this.ReadContentAsBinHex();
            this.ReadEndElement();
            return buffer;
        }

        public override bool ReadElementContentAsBoolean()
        {
            if (this.IsStartElement() && this.IsEmptyElement)
            {
                this.Read();
                return XmlConverter.ToBoolean(string.Empty);
            }
            this.ReadStartElement();
            bool flag2 = this.ReadContentAsBoolean();
            this.ReadEndElement();
            return flag2;
        }

        public override DateTime ReadElementContentAsDateTime()
        {
            if (this.IsStartElement() && this.IsEmptyElement)
            {
                this.Read();
                try
                {
                    return DateTime.Parse(string.Empty, NumberFormatInfo.InvariantInfo);
                }
                catch (ArgumentException exception)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(string.Empty, "DateTime", exception));
                }
                catch (FormatException exception2)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(string.Empty, "DateTime", exception2));
                }
            }
            this.ReadStartElement();
            DateTime time = this.ReadContentAsDateTime();
            this.ReadEndElement();
            return time;
        }

        public override decimal ReadElementContentAsDecimal()
        {
            if (this.IsStartElement() && this.IsEmptyElement)
            {
                this.Read();
                return XmlConverter.ToDecimal(string.Empty);
            }
            this.ReadStartElement();
            decimal num = this.ReadContentAsDecimal();
            this.ReadEndElement();
            return num;
        }

        public override double ReadElementContentAsDouble()
        {
            if (this.IsStartElement() && this.IsEmptyElement)
            {
                this.Read();
                return XmlConverter.ToDouble(string.Empty);
            }
            this.ReadStartElement();
            double num = this.ReadContentAsDouble();
            this.ReadEndElement();
            return num;
        }

        public override float ReadElementContentAsFloat()
        {
            if (this.IsStartElement() && this.IsEmptyElement)
            {
                this.Read();
                return XmlConverter.ToSingle(string.Empty);
            }
            this.ReadStartElement();
            float num = this.ReadContentAsFloat();
            this.ReadEndElement();
            return num;
        }

        public virtual Guid ReadElementContentAsGuid()
        {
            if (this.IsStartElement() && this.IsEmptyElement)
            {
                this.Read();
                try
                {
                    return Guid.Empty;
                }
                catch (ArgumentException exception)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(string.Empty, "Guid", exception));
                }
                catch (FormatException exception2)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(string.Empty, "Guid", exception2));
                }
                catch (OverflowException exception3)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(string.Empty, "Guid", exception3));
                }
            }
            this.ReadStartElement();
            Guid guid = this.ReadContentAsGuid();
            this.ReadEndElement();
            return guid;
        }

        public override int ReadElementContentAsInt()
        {
            if (this.IsStartElement() && this.IsEmptyElement)
            {
                this.Read();
                return XmlConverter.ToInt32(string.Empty);
            }
            this.ReadStartElement();
            int num = this.ReadContentAsInt();
            this.ReadEndElement();
            return num;
        }

        public override long ReadElementContentAsLong()
        {
            if (this.IsStartElement() && this.IsEmptyElement)
            {
                this.Read();
                return XmlConverter.ToInt64(string.Empty);
            }
            this.ReadStartElement();
            long num = this.ReadContentAsLong();
            this.ReadEndElement();
            return num;
        }

        public override string ReadElementContentAsString()
        {
            if (this.IsStartElement() && this.IsEmptyElement)
            {
                this.Read();
                return string.Empty;
            }
            this.ReadStartElement();
            string str = this.ReadContentAsString();
            this.ReadEndElement();
            return str;
        }

        public virtual TimeSpan ReadElementContentAsTimeSpan()
        {
            if (this.IsStartElement() && this.IsEmptyElement)
            {
                this.Read();
                return XmlConverter.ToTimeSpan(string.Empty);
            }
            this.ReadStartElement();
            TimeSpan span = this.ReadContentAsTimeSpan();
            this.ReadEndElement();
            return span;
        }

        public virtual UniqueId ReadElementContentAsUniqueId()
        {
            if (this.IsStartElement() && this.IsEmptyElement)
            {
                this.Read();
                try
                {
                    return new UniqueId(string.Empty);
                }
                catch (ArgumentException exception)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(string.Empty, "UniqueId", exception));
                }
                catch (FormatException exception2)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(string.Empty, "UniqueId", exception2));
                }
            }
            this.ReadStartElement();
            UniqueId id = this.ReadContentAsUniqueId();
            this.ReadEndElement();
            return id;
        }

        public virtual void ReadFullStartElement()
        {
            this.MoveToStartElement();
            if (this.IsEmptyElement)
            {
                XmlExceptionHelper.ThrowFullStartElementExpected(this);
            }
            this.Read();
        }

        public virtual void ReadFullStartElement(string name)
        {
            this.MoveToStartElement(name);
            if (this.IsEmptyElement)
            {
                XmlExceptionHelper.ThrowFullStartElementExpected(this, name);
            }
            this.Read();
        }

        public virtual void ReadFullStartElement(string localName, string namespaceUri)
        {
            this.MoveToStartElement(localName, namespaceUri);
            if (this.IsEmptyElement)
            {
                XmlExceptionHelper.ThrowFullStartElementExpected(this, localName, namespaceUri);
            }
            this.Read();
        }

        public virtual void ReadFullStartElement(XmlDictionaryString localName, XmlDictionaryString namespaceUri)
        {
            this.MoveToStartElement(localName, namespaceUri);
            if (this.IsEmptyElement)
            {
                XmlExceptionHelper.ThrowFullStartElementExpected(this, localName, namespaceUri);
            }
            this.Read();
        }

        public virtual Guid[] ReadGuidArray(string localName, string namespaceUri)
        {
            return GuidArrayHelperWithString.Instance.ReadArray(this, localName, namespaceUri, this.Quotas.MaxArrayLength);
        }

        public virtual Guid[] ReadGuidArray(XmlDictionaryString localName, XmlDictionaryString namespaceUri)
        {
            return GuidArrayHelperWithDictionaryString.Instance.ReadArray(this, localName, namespaceUri, this.Quotas.MaxArrayLength);
        }

        public virtual short[] ReadInt16Array(string localName, string namespaceUri)
        {
            return Int16ArrayHelperWithString.Instance.ReadArray(this, localName, namespaceUri, this.Quotas.MaxArrayLength);
        }

        public virtual short[] ReadInt16Array(XmlDictionaryString localName, XmlDictionaryString namespaceUri)
        {
            return Int16ArrayHelperWithDictionaryString.Instance.ReadArray(this, localName, namespaceUri, this.Quotas.MaxArrayLength);
        }

        public virtual int[] ReadInt32Array(string localName, string namespaceUri)
        {
            return Int32ArrayHelperWithString.Instance.ReadArray(this, localName, namespaceUri, this.Quotas.MaxArrayLength);
        }

        public virtual int[] ReadInt32Array(XmlDictionaryString localName, XmlDictionaryString namespaceUri)
        {
            return Int32ArrayHelperWithDictionaryString.Instance.ReadArray(this, localName, namespaceUri, this.Quotas.MaxArrayLength);
        }

        public virtual long[] ReadInt64Array(string localName, string namespaceUri)
        {
            return Int64ArrayHelperWithString.Instance.ReadArray(this, localName, namespaceUri, this.Quotas.MaxArrayLength);
        }

        public virtual long[] ReadInt64Array(XmlDictionaryString localName, XmlDictionaryString namespaceUri)
        {
            return Int64ArrayHelperWithDictionaryString.Instance.ReadArray(this, localName, namespaceUri, this.Quotas.MaxArrayLength);
        }

        public virtual float[] ReadSingleArray(string localName, string namespaceUri)
        {
            return SingleArrayHelperWithString.Instance.ReadArray(this, localName, namespaceUri, this.Quotas.MaxArrayLength);
        }

        public virtual float[] ReadSingleArray(XmlDictionaryString localName, XmlDictionaryString namespaceUri)
        {
            return SingleArrayHelperWithDictionaryString.Instance.ReadArray(this, localName, namespaceUri, this.Quotas.MaxArrayLength);
        }

        public virtual void ReadStartElement(XmlDictionaryString localName, XmlDictionaryString namespaceUri)
        {
            this.MoveToStartElement(localName, namespaceUri);
            this.Read();
        }

        public override string ReadString()
        {
            return this.ReadString(this.Quotas.MaxStringContentLength);
        }

        protected string ReadString(int maxStringContentLength)
        {
            if (this.ReadState != System.Xml.ReadState.Interactive)
            {
                return string.Empty;
            }
            if (this.NodeType != XmlNodeType.Element)
            {
                this.MoveToElement();
            }
            if (this.NodeType == XmlNodeType.Element)
            {
                if (this.IsEmptyElement)
                {
                    return string.Empty;
                }
                if (!this.Read())
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.Runtime.Serialization.SR.GetString("XmlInvalidOperation")));
                }
                if (this.NodeType == XmlNodeType.EndElement)
                {
                    return string.Empty;
                }
            }
            StringBuilder builder = null;
            string str = string.Empty;
            while (this.IsTextNode(this.NodeType))
            {
                string str2 = this.Value;
                if (str.Length == 0)
                {
                    str = str2;
                }
                else
                {
                    if (builder == null)
                    {
                        builder = new StringBuilder(str);
                    }
                    if (builder.Length > (maxStringContentLength - str2.Length))
                    {
                        XmlExceptionHelper.ThrowMaxStringContentLengthExceeded(this, maxStringContentLength);
                    }
                    builder.Append(str2);
                }
                if (!this.Read())
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.Runtime.Serialization.SR.GetString("XmlInvalidOperation")));
                }
            }
            if (builder != null)
            {
                str = builder.ToString();
            }
            if (str.Length > maxStringContentLength)
            {
                XmlExceptionHelper.ThrowMaxStringContentLengthExceeded(this, maxStringContentLength);
            }
            return str;
        }

        public virtual TimeSpan[] ReadTimeSpanArray(string localName, string namespaceUri)
        {
            return TimeSpanArrayHelperWithString.Instance.ReadArray(this, localName, namespaceUri, this.Quotas.MaxArrayLength);
        }

        public virtual TimeSpan[] ReadTimeSpanArray(XmlDictionaryString localName, XmlDictionaryString namespaceUri)
        {
            return TimeSpanArrayHelperWithDictionaryString.Instance.ReadArray(this, localName, namespaceUri, this.Quotas.MaxArrayLength);
        }

        public virtual int ReadValueAsBase64(byte[] buffer, int offset, int count)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
        }

        public virtual void StartCanonicalization(Stream stream, bool includeComments, string[] inclusivePrefixes)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
        }

        public virtual bool TryGetArrayLength(out int count)
        {
            count = 0;
            return false;
        }

        public virtual bool TryGetBase64ContentLength(out int length)
        {
            length = 0;
            return false;
        }

        public virtual bool TryGetLocalNameAsDictionaryString(out XmlDictionaryString localName)
        {
            localName = null;
            return false;
        }

        public virtual bool TryGetNamespaceUriAsDictionaryString(out XmlDictionaryString namespaceUri)
        {
            namespaceUri = null;
            return false;
        }

        public virtual bool TryGetValueAsDictionaryString(out XmlDictionaryString value)
        {
            value = null;
            return false;
        }

        public virtual bool CanCanonicalize
        {
            get
            {
                return false;
            }
        }

        public virtual XmlDictionaryReaderQuotas Quotas
        {
            get
            {
                return XmlDictionaryReaderQuotas.Max;
            }
        }

        private class XmlWrappedReader : XmlDictionaryReader, IXmlLineInfo
        {
            private XmlNamespaceManager nsMgr;
            private XmlReader reader;

            public XmlWrappedReader(XmlReader reader, XmlNamespaceManager nsMgr)
            {
                this.reader = reader;
                this.nsMgr = nsMgr;
            }

            public override void Close()
            {
                this.reader.Close();
                this.nsMgr = null;
            }

            public override string GetAttribute(int index)
            {
                return this.reader.GetAttribute(index);
            }

            public override string GetAttribute(string name)
            {
                return this.reader.GetAttribute(name);
            }

            public override string GetAttribute(string name, string namespaceUri)
            {
                return this.reader.GetAttribute(name, namespaceUri);
            }

            public bool HasLineInfo()
            {
                IXmlLineInfo reader = this.reader as IXmlLineInfo;
                if (reader == null)
                {
                    return false;
                }
                return reader.HasLineInfo();
            }

            public override bool IsStartElement(string name)
            {
                return this.reader.IsStartElement(name);
            }

            public override bool IsStartElement(string localName, string namespaceUri)
            {
                return this.reader.IsStartElement(localName, namespaceUri);
            }

            public override string LookupNamespace(string namespaceUri)
            {
                return this.reader.LookupNamespace(namespaceUri);
            }

            public override void MoveToAttribute(int index)
            {
                this.reader.MoveToAttribute(index);
            }

            public override bool MoveToAttribute(string name)
            {
                return this.reader.MoveToAttribute(name);
            }

            public override bool MoveToAttribute(string name, string namespaceUri)
            {
                return this.reader.MoveToAttribute(name, namespaceUri);
            }

            public override bool MoveToElement()
            {
                return this.reader.MoveToElement();
            }

            public override bool MoveToFirstAttribute()
            {
                return this.reader.MoveToFirstAttribute();
            }

            public override bool MoveToNextAttribute()
            {
                return this.reader.MoveToNextAttribute();
            }

            public override bool Read()
            {
                return this.reader.Read();
            }

            public override bool ReadAttributeValue()
            {
                return this.reader.ReadAttributeValue();
            }

            public override object ReadContentAs(Type type, IXmlNamespaceResolver namespaceResolver)
            {
                return this.reader.ReadContentAs(type, namespaceResolver);
            }

            public override int ReadContentAsBase64(byte[] buffer, int offset, int count)
            {
                return this.reader.ReadContentAsBase64(buffer, offset, count);
            }

            public override int ReadContentAsBinHex(byte[] buffer, int offset, int count)
            {
                return this.reader.ReadContentAsBinHex(buffer, offset, count);
            }

            public override bool ReadContentAsBoolean()
            {
                return this.reader.ReadContentAsBoolean();
            }

            public override DateTime ReadContentAsDateTime()
            {
                return this.reader.ReadContentAsDateTime();
            }

            public override decimal ReadContentAsDecimal()
            {
                return (decimal) this.reader.ReadContentAs(typeof(decimal), null);
            }

            public override double ReadContentAsDouble()
            {
                return this.reader.ReadContentAsDouble();
            }

            public override float ReadContentAsFloat()
            {
                return this.reader.ReadContentAsFloat();
            }

            public override int ReadContentAsInt()
            {
                return this.reader.ReadContentAsInt();
            }

            public override long ReadContentAsLong()
            {
                return this.reader.ReadContentAsLong();
            }

            public override string ReadContentAsString()
            {
                return this.reader.ReadContentAsString();
            }

            public override int ReadElementContentAsBase64(byte[] buffer, int offset, int count)
            {
                return this.reader.ReadElementContentAsBase64(buffer, offset, count);
            }

            public override int ReadElementContentAsBinHex(byte[] buffer, int offset, int count)
            {
                return this.reader.ReadElementContentAsBinHex(buffer, offset, count);
            }

            public override string ReadElementString(string name)
            {
                return this.reader.ReadElementString(name);
            }

            public override string ReadElementString(string localName, string namespaceUri)
            {
                return this.reader.ReadElementString(localName, namespaceUri);
            }

            public override void ReadEndElement()
            {
                this.reader.ReadEndElement();
            }

            public override string ReadInnerXml()
            {
                return this.reader.ReadInnerXml();
            }

            public override string ReadOuterXml()
            {
                return this.reader.ReadOuterXml();
            }

            public override void ReadStartElement(string name)
            {
                this.reader.ReadStartElement(name);
            }

            public override void ReadStartElement(string localName, string namespaceUri)
            {
                this.reader.ReadStartElement(localName, namespaceUri);
            }

            public override string ReadString()
            {
                return this.reader.ReadString();
            }

            public override int ReadValueChunk(char[] chars, int offset, int count)
            {
                return this.reader.ReadValueChunk(chars, offset, count);
            }

            public override void ResolveEntity()
            {
                this.reader.ResolveEntity();
            }

            public override int AttributeCount
            {
                get
                {
                    return this.reader.AttributeCount;
                }
            }

            public override string BaseURI
            {
                get
                {
                    return this.reader.BaseURI;
                }
            }

            public override bool CanReadBinaryContent
            {
                get
                {
                    return this.reader.CanReadBinaryContent;
                }
            }

            public override bool CanReadValueChunk
            {
                get
                {
                    return this.reader.CanReadValueChunk;
                }
            }

            public override int Depth
            {
                get
                {
                    return this.reader.Depth;
                }
            }

            public override bool EOF
            {
                get
                {
                    return this.reader.EOF;
                }
            }

            public override bool HasValue
            {
                get
                {
                    return this.reader.HasValue;
                }
            }

            public override bool IsDefault
            {
                get
                {
                    return this.reader.IsDefault;
                }
            }

            public override bool IsEmptyElement
            {
                get
                {
                    return this.reader.IsEmptyElement;
                }
            }

            public override string this[int index]
            {
                get
                {
                    return this.reader[index];
                }
            }

            public override string this[string name]
            {
                get
                {
                    return this.reader[name];
                }
            }

            public override string this[string name, string namespaceUri]
            {
                get
                {
                    return this.reader[name, namespaceUri];
                }
            }

            public int LineNumber
            {
                get
                {
                    IXmlLineInfo reader = this.reader as IXmlLineInfo;
                    if (reader == null)
                    {
                        return 1;
                    }
                    return reader.LineNumber;
                }
            }

            public int LinePosition
            {
                get
                {
                    IXmlLineInfo reader = this.reader as IXmlLineInfo;
                    if (reader == null)
                    {
                        return 1;
                    }
                    return reader.LinePosition;
                }
            }

            public override string LocalName
            {
                get
                {
                    return this.reader.LocalName;
                }
            }

            public override string Name
            {
                get
                {
                    return this.reader.Name;
                }
            }

            public override string NamespaceURI
            {
                get
                {
                    return this.reader.NamespaceURI;
                }
            }

            public override XmlNameTable NameTable
            {
                get
                {
                    return this.reader.NameTable;
                }
            }

            public override XmlNodeType NodeType
            {
                get
                {
                    return this.reader.NodeType;
                }
            }

            public override string Prefix
            {
                get
                {
                    return this.reader.Prefix;
                }
            }

            public override char QuoteChar
            {
                get
                {
                    return this.reader.QuoteChar;
                }
            }

            public override System.Xml.ReadState ReadState
            {
                get
                {
                    return this.reader.ReadState;
                }
            }

            public override string Value
            {
                get
                {
                    return this.reader.Value;
                }
            }

            public override Type ValueType
            {
                get
                {
                    return this.reader.ValueType;
                }
            }

            public override string XmlLang
            {
                get
                {
                    return this.reader.XmlLang;
                }
            }

            public override System.Xml.XmlSpace XmlSpace
            {
                get
                {
                    return this.reader.XmlSpace;
                }
            }
        }
    }
}

