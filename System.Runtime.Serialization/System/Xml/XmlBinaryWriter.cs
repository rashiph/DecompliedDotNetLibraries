namespace System.Xml
{
    using System;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Security;

    internal class XmlBinaryWriter : XmlBaseWriter, IXmlBinaryWriterInitializer
    {
        private byte[] bytes;
        private char[] chars;
        private XmlBinaryNodeWriter writer;

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

        protected override XmlSigningNodeWriter CreateSigningNodeWriter()
        {
            return new XmlSigningNodeWriter(false);
        }

        public void SetOutput(Stream stream, IXmlDictionary dictionary, XmlBinaryWriterSession session, bool ownsStream)
        {
            if (stream == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("stream"));
            }
            if (this.writer == null)
            {
                this.writer = new XmlBinaryNodeWriter();
            }
            this.writer.SetOutput(stream, dictionary, session, ownsStream);
            base.SetOutput(this.writer);
        }

        [SecurityCritical]
        private unsafe void UnsafeWriteArray(string prefix, string localName, string namespaceUri, XmlBinaryNodeType nodeType, int count, byte* array, byte* arrayMax)
        {
            this.WriteStartArray(prefix, localName, namespaceUri, count);
            this.writer.UnsafeWriteArray(nodeType, count, array, arrayMax);
            this.WriteEndArray();
        }

        [SecurityCritical]
        private unsafe void UnsafeWriteArray(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, XmlBinaryNodeType nodeType, int count, byte* array, byte* arrayMax)
        {
            this.WriteStartArray(prefix, localName, namespaceUri, count);
            this.writer.UnsafeWriteArray(nodeType, count, array, arrayMax);
            this.WriteEndArray();
        }

        [SecuritySafeCritical]
        public override unsafe void WriteArray(string prefix, string localName, string namespaceUri, bool[] array, int offset, int count)
        {
            if (base.Signing)
            {
                base.WriteArray(prefix, localName, namespaceUri, array, offset, count);
            }
            else
            {
                this.CheckArray(array, offset, count);
                if (count > 0)
                {
                    fixed (bool* flagRef = &(array[offset]))
                    {
                        this.UnsafeWriteArray(prefix, localName, namespaceUri, XmlBinaryNodeType.BoolTextWithEndElement, count, (byte*) flagRef, (byte*) (flagRef + count));
                    }
                }
            }
        }

        public override void WriteArray(string prefix, string localName, string namespaceUri, DateTime[] array, int offset, int count)
        {
            if (base.Signing)
            {
                base.WriteArray(prefix, localName, namespaceUri, array, offset, count);
            }
            else
            {
                this.CheckArray(array, offset, count);
                if (count > 0)
                {
                    this.WriteStartArray(prefix, localName, namespaceUri, count);
                    this.writer.WriteDateTimeArray(array, offset, count);
                    this.WriteEndArray();
                }
            }
        }

        [SecuritySafeCritical]
        public override unsafe void WriteArray(string prefix, string localName, string namespaceUri, decimal[] array, int offset, int count)
        {
            if (base.Signing)
            {
                base.WriteArray(prefix, localName, namespaceUri, array, offset, count);
            }
            else
            {
                this.CheckArray(array, offset, count);
                if (count > 0)
                {
                    fixed (decimal* numRef = &(array[offset]))
                    {
                        this.UnsafeWriteArray(prefix, localName, namespaceUri, XmlBinaryNodeType.DecimalTextWithEndElement, count, (byte*) numRef, (byte*) (numRef + (count * 0x10)));
                    }
                }
            }
        }

        [SecuritySafeCritical]
        public override unsafe void WriteArray(string prefix, string localName, string namespaceUri, double[] array, int offset, int count)
        {
            if (base.Signing)
            {
                base.WriteArray(prefix, localName, namespaceUri, array, offset, count);
            }
            else
            {
                this.CheckArray(array, offset, count);
                if (count > 0)
                {
                    fixed (double* numRef = &(array[offset]))
                    {
                        this.UnsafeWriteArray(prefix, localName, namespaceUri, XmlBinaryNodeType.DoubleTextWithEndElement, count, (byte*) numRef, (byte*) (numRef + (count * 8)));
                    }
                }
            }
        }

        public override void WriteArray(string prefix, string localName, string namespaceUri, Guid[] array, int offset, int count)
        {
            if (base.Signing)
            {
                base.WriteArray(prefix, localName, namespaceUri, array, offset, count);
            }
            else
            {
                this.CheckArray(array, offset, count);
                if (count > 0)
                {
                    this.WriteStartArray(prefix, localName, namespaceUri, count);
                    this.writer.WriteGuidArray(array, offset, count);
                    this.WriteEndArray();
                }
            }
        }

        [SecuritySafeCritical]
        public override unsafe void WriteArray(string prefix, string localName, string namespaceUri, short[] array, int offset, int count)
        {
            if (base.Signing)
            {
                base.WriteArray(prefix, localName, namespaceUri, array, offset, count);
            }
            else
            {
                this.CheckArray(array, offset, count);
                if (count > 0)
                {
                    fixed (short* numRef = &(array[offset]))
                    {
                        this.UnsafeWriteArray(prefix, localName, namespaceUri, XmlBinaryNodeType.Int16TextWithEndElement, count, (byte*) numRef, (byte*) (numRef + count));
                    }
                }
            }
        }

        [SecuritySafeCritical]
        public override unsafe void WriteArray(string prefix, string localName, string namespaceUri, int[] array, int offset, int count)
        {
            if (base.Signing)
            {
                base.WriteArray(prefix, localName, namespaceUri, array, offset, count);
            }
            else
            {
                this.CheckArray(array, offset, count);
                if (count > 0)
                {
                    fixed (int* numRef = &(array[offset]))
                    {
                        this.UnsafeWriteArray(prefix, localName, namespaceUri, XmlBinaryNodeType.Int32TextWithEndElement, count, (byte*) numRef, (byte*) (numRef + count));
                    }
                }
            }
        }

        [SecuritySafeCritical]
        public override unsafe void WriteArray(string prefix, string localName, string namespaceUri, long[] array, int offset, int count)
        {
            if (base.Signing)
            {
                base.WriteArray(prefix, localName, namespaceUri, array, offset, count);
            }
            else
            {
                this.CheckArray(array, offset, count);
                if (count > 0)
                {
                    fixed (long* numRef = &(array[offset]))
                    {
                        this.UnsafeWriteArray(prefix, localName, namespaceUri, XmlBinaryNodeType.Int64TextWithEndElement, count, (byte*) numRef, (byte*) (numRef + count));
                    }
                }
            }
        }

        [SecuritySafeCritical]
        public override unsafe void WriteArray(string prefix, string localName, string namespaceUri, float[] array, int offset, int count)
        {
            if (base.Signing)
            {
                base.WriteArray(prefix, localName, namespaceUri, array, offset, count);
            }
            else
            {
                this.CheckArray(array, offset, count);
                if (count > 0)
                {
                    fixed (float* numRef = &(array[offset]))
                    {
                        this.UnsafeWriteArray(prefix, localName, namespaceUri, XmlBinaryNodeType.FloatTextWithEndElement, count, (byte*) numRef, (byte*) (numRef + (count * 4)));
                    }
                }
            }
        }

        public override void WriteArray(string prefix, string localName, string namespaceUri, TimeSpan[] array, int offset, int count)
        {
            if (base.Signing)
            {
                base.WriteArray(prefix, localName, namespaceUri, array, offset, count);
            }
            else
            {
                this.CheckArray(array, offset, count);
                if (count > 0)
                {
                    this.WriteStartArray(prefix, localName, namespaceUri, count);
                    this.writer.WriteTimeSpanArray(array, offset, count);
                    this.WriteEndArray();
                }
            }
        }

        [SecuritySafeCritical]
        public override unsafe void WriteArray(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, bool[] array, int offset, int count)
        {
            if (base.Signing)
            {
                base.WriteArray(prefix, localName, namespaceUri, array, offset, count);
            }
            else
            {
                this.CheckArray(array, offset, count);
                if (count > 0)
                {
                    fixed (bool* flagRef = &(array[offset]))
                    {
                        this.UnsafeWriteArray(prefix, localName, namespaceUri, XmlBinaryNodeType.BoolTextWithEndElement, count, (byte*) flagRef, (byte*) (flagRef + count));
                    }
                }
            }
        }

        public override void WriteArray(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, DateTime[] array, int offset, int count)
        {
            if (base.Signing)
            {
                base.WriteArray(prefix, localName, namespaceUri, array, offset, count);
            }
            else
            {
                this.CheckArray(array, offset, count);
                if (count > 0)
                {
                    this.WriteStartArray(prefix, localName, namespaceUri, count);
                    this.writer.WriteDateTimeArray(array, offset, count);
                    this.WriteEndArray();
                }
            }
        }

        [SecuritySafeCritical]
        public override unsafe void WriteArray(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, decimal[] array, int offset, int count)
        {
            if (base.Signing)
            {
                base.WriteArray(prefix, localName, namespaceUri, array, offset, count);
            }
            else
            {
                this.CheckArray(array, offset, count);
                if (count > 0)
                {
                    fixed (decimal* numRef = &(array[offset]))
                    {
                        this.UnsafeWriteArray(prefix, localName, namespaceUri, XmlBinaryNodeType.DecimalTextWithEndElement, count, (byte*) numRef, (byte*) (numRef + (count * 0x10)));
                    }
                }
            }
        }

        [SecuritySafeCritical]
        public override unsafe void WriteArray(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, double[] array, int offset, int count)
        {
            if (base.Signing)
            {
                base.WriteArray(prefix, localName, namespaceUri, array, offset, count);
            }
            else
            {
                this.CheckArray(array, offset, count);
                if (count > 0)
                {
                    fixed (double* numRef = &(array[offset]))
                    {
                        this.UnsafeWriteArray(prefix, localName, namespaceUri, XmlBinaryNodeType.DoubleTextWithEndElement, count, (byte*) numRef, (byte*) (numRef + (count * 8)));
                    }
                }
            }
        }

        public override void WriteArray(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, Guid[] array, int offset, int count)
        {
            if (base.Signing)
            {
                base.WriteArray(prefix, localName, namespaceUri, array, offset, count);
            }
            else
            {
                this.CheckArray(array, offset, count);
                if (count > 0)
                {
                    this.WriteStartArray(prefix, localName, namespaceUri, count);
                    this.writer.WriteGuidArray(array, offset, count);
                    this.WriteEndArray();
                }
            }
        }

        [SecuritySafeCritical]
        public override unsafe void WriteArray(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, short[] array, int offset, int count)
        {
            if (base.Signing)
            {
                base.WriteArray(prefix, localName, namespaceUri, array, offset, count);
            }
            else
            {
                this.CheckArray(array, offset, count);
                if (count > 0)
                {
                    fixed (short* numRef = &(array[offset]))
                    {
                        this.UnsafeWriteArray(prefix, localName, namespaceUri, XmlBinaryNodeType.Int16TextWithEndElement, count, (byte*) numRef, (byte*) (numRef + count));
                    }
                }
            }
        }

        [SecuritySafeCritical]
        public override unsafe void WriteArray(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, int[] array, int offset, int count)
        {
            if (base.Signing)
            {
                base.WriteArray(prefix, localName, namespaceUri, array, offset, count);
            }
            else
            {
                this.CheckArray(array, offset, count);
                if (count > 0)
                {
                    fixed (int* numRef = &(array[offset]))
                    {
                        this.UnsafeWriteArray(prefix, localName, namespaceUri, XmlBinaryNodeType.Int32TextWithEndElement, count, (byte*) numRef, (byte*) (numRef + count));
                    }
                }
            }
        }

        [SecuritySafeCritical]
        public override unsafe void WriteArray(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, long[] array, int offset, int count)
        {
            if (base.Signing)
            {
                base.WriteArray(prefix, localName, namespaceUri, array, offset, count);
            }
            else
            {
                this.CheckArray(array, offset, count);
                if (count > 0)
                {
                    fixed (long* numRef = &(array[offset]))
                    {
                        this.UnsafeWriteArray(prefix, localName, namespaceUri, XmlBinaryNodeType.Int64TextWithEndElement, count, (byte*) numRef, (byte*) (numRef + count));
                    }
                }
            }
        }

        [SecuritySafeCritical]
        public override unsafe void WriteArray(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, float[] array, int offset, int count)
        {
            if (base.Signing)
            {
                base.WriteArray(prefix, localName, namespaceUri, array, offset, count);
            }
            else
            {
                this.CheckArray(array, offset, count);
                if (count > 0)
                {
                    fixed (float* numRef = &(array[offset]))
                    {
                        this.UnsafeWriteArray(prefix, localName, namespaceUri, XmlBinaryNodeType.FloatTextWithEndElement, count, (byte*) numRef, (byte*) (numRef + (count * 4)));
                    }
                }
            }
        }

        public override void WriteArray(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, TimeSpan[] array, int offset, int count)
        {
            if (base.Signing)
            {
                base.WriteArray(prefix, localName, namespaceUri, array, offset, count);
            }
            else
            {
                this.CheckArray(array, offset, count);
                if (count > 0)
                {
                    this.WriteStartArray(prefix, localName, namespaceUri, count);
                    this.writer.WriteTimeSpanArray(array, offset, count);
                    this.WriteEndArray();
                }
            }
        }

        private void WriteEndArray()
        {
            base.EndArray();
        }

        private void WriteStartArray(string prefix, string localName, string namespaceUri, int count)
        {
            base.StartArray(count);
            this.writer.WriteArrayNode();
            this.WriteStartElement(prefix, localName, namespaceUri);
            this.WriteEndElement();
        }

        private void WriteStartArray(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, int count)
        {
            base.StartArray(count);
            this.writer.WriteArrayNode();
            this.WriteStartElement(prefix, localName, namespaceUri);
            this.WriteEndElement();
        }

        protected override void WriteTextNode(XmlDictionaryReader reader, bool attribute)
        {
            Type valueType = reader.ValueType;
            if (valueType == typeof(string))
            {
                XmlDictionaryString str;
                if (reader.TryGetValueAsDictionaryString(out str))
                {
                    this.WriteString(str);
                }
                else if (reader.CanReadValueChunk)
                {
                    int num;
                    if (this.chars == null)
                    {
                        this.chars = new char[0x100];
                    }
                    while ((num = reader.ReadValueChunk(this.chars, 0, this.chars.Length)) > 0)
                    {
                        this.WriteChars(this.chars, 0, num);
                    }
                }
                else
                {
                    this.WriteString(reader.Value);
                }
                if (!attribute)
                {
                    reader.Read();
                }
            }
            else if (valueType == typeof(byte[]))
            {
                if (reader.CanReadBinaryContent)
                {
                    int num2;
                    if (this.bytes == null)
                    {
                        this.bytes = new byte[0x180];
                    }
                    while ((num2 = reader.ReadValueAsBase64(this.bytes, 0, this.bytes.Length)) > 0)
                    {
                        this.WriteBase64(this.bytes, 0, num2);
                    }
                }
                else
                {
                    this.WriteString(reader.Value);
                }
                if (!attribute)
                {
                    reader.Read();
                }
            }
            else if (valueType == typeof(int))
            {
                this.WriteValue(reader.ReadContentAsInt());
            }
            else if (valueType == typeof(long))
            {
                this.WriteValue(reader.ReadContentAsLong());
            }
            else if (valueType == typeof(bool))
            {
                this.WriteValue(reader.ReadContentAsBoolean());
            }
            else if (valueType == typeof(double))
            {
                this.WriteValue(reader.ReadContentAsDouble());
            }
            else if (valueType == typeof(DateTime))
            {
                this.WriteValue(reader.ReadContentAsDateTime());
            }
            else if (valueType == typeof(float))
            {
                this.WriteValue(reader.ReadContentAsFloat());
            }
            else if (valueType == typeof(decimal))
            {
                this.WriteValue(reader.ReadContentAsDecimal());
            }
            else if (valueType == typeof(UniqueId))
            {
                this.WriteValue(reader.ReadContentAsUniqueId());
            }
            else if (valueType == typeof(Guid))
            {
                this.WriteValue(reader.ReadContentAsGuid());
            }
            else if (valueType == typeof(TimeSpan))
            {
                this.WriteValue(reader.ReadContentAsTimeSpan());
            }
            else
            {
                this.WriteValue(reader.ReadContentAsObject());
            }
        }
    }
}

