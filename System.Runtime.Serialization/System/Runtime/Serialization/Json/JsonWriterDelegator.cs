namespace System.Runtime.Serialization.Json
{
    using System;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Xml;

    internal class JsonWriterDelegator : XmlWriterDelegator
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public JsonWriterDelegator(XmlWriter writer) : base(writer)
        {
        }

        internal override void WriteBase64(byte[] bytes)
        {
            if (bytes != null)
            {
                ByteArrayHelperWithString.Instance.WriteArray(base.Writer, bytes, 0, bytes.Length);
            }
        }

        internal override void WriteBoolean(bool value)
        {
            base.writer.WriteAttributeString("type", "boolean");
            base.WriteBoolean(value);
        }

        internal override void WriteChar(char value)
        {
            base.WriteString(XmlConvert.ToString(value));
        }

        internal override void WriteDateTime(DateTime value)
        {
            TimeSpan utcOffset;
            if (value.Kind != DateTimeKind.Utc)
            {
                long num = value.Ticks - TimeZone.CurrentTimeZone.GetUtcOffset(value).Ticks;
                if ((num > DateTime.MaxValue.Ticks) || (num < DateTime.MinValue.Ticks))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(System.Runtime.Serialization.SR.GetString("JsonDateTimeOutOfRange"), new ArgumentOutOfRangeException("value")));
                }
            }
            base.writer.WriteString("/Date(");
            base.writer.WriteValue((long) ((value.ToUniversalTime().Ticks - JsonGlobals.unixEpochTicks) / 0x2710L));
            switch (value.Kind)
            {
                case DateTimeKind.Unspecified:
                case DateTimeKind.Local:
                    utcOffset = TimeZone.CurrentTimeZone.GetUtcOffset(value.ToLocalTime());
                    if (utcOffset.Ticks >= 0L)
                    {
                        base.writer.WriteString("+");
                        break;
                    }
                    base.writer.WriteString("-");
                    break;

                default:
                    goto Label_017B;
            }
            int num2 = Math.Abs(utcOffset.Hours);
            base.writer.WriteString((num2 < 10) ? ("0" + num2) : num2.ToString(CultureInfo.InvariantCulture));
            int num3 = Math.Abs(utcOffset.Minutes);
            base.writer.WriteString((num3 < 10) ? ("0" + num3) : num3.ToString(CultureInfo.InvariantCulture));
        Label_017B:
            base.writer.WriteString(")/");
        }

        internal override void WriteDecimal(decimal value)
        {
            base.writer.WriteAttributeString("type", "number");
            base.WriteDecimal(value);
        }

        internal override void WriteDouble(double value)
        {
            base.writer.WriteAttributeString("type", "number");
            base.WriteDouble(value);
        }

        internal override void WriteFloat(float value)
        {
            base.writer.WriteAttributeString("type", "number");
            base.WriteFloat(value);
        }

        internal override void WriteInt(int value)
        {
            base.writer.WriteAttributeString("type", "number");
            base.WriteInt(value);
        }

        internal void WriteJsonBooleanArray(bool[] value, XmlDictionaryString itemName, XmlDictionaryString itemNamespace)
        {
            for (int i = 0; i < value.Length; i++)
            {
                base.WriteBoolean(value[i], itemName, itemNamespace);
            }
        }

        internal void WriteJsonDateTimeArray(DateTime[] value, XmlDictionaryString itemName, XmlDictionaryString itemNamespace)
        {
            for (int i = 0; i < value.Length; i++)
            {
                base.WriteDateTime(value[i], itemName, itemNamespace);
            }
        }

        internal void WriteJsonDecimalArray(decimal[] value, XmlDictionaryString itemName, XmlDictionaryString itemNamespace)
        {
            for (int i = 0; i < value.Length; i++)
            {
                base.WriteDecimal(value[i], itemName, itemNamespace);
            }
        }

        internal void WriteJsonDoubleArray(double[] value, XmlDictionaryString itemName, XmlDictionaryString itemNamespace)
        {
            for (int i = 0; i < value.Length; i++)
            {
                base.WriteDouble(value[i], itemName, itemNamespace);
            }
        }

        internal void WriteJsonInt32Array(int[] value, XmlDictionaryString itemName, XmlDictionaryString itemNamespace)
        {
            for (int i = 0; i < value.Length; i++)
            {
                base.WriteInt(value[i], itemName, itemNamespace);
            }
        }

        internal void WriteJsonInt64Array(long[] value, XmlDictionaryString itemName, XmlDictionaryString itemNamespace)
        {
            for (int i = 0; i < value.Length; i++)
            {
                base.WriteLong(value[i], itemName, itemNamespace);
            }
        }

        internal void WriteJsonSingleArray(float[] value, XmlDictionaryString itemName, XmlDictionaryString itemNamespace)
        {
            for (int i = 0; i < value.Length; i++)
            {
                base.WriteFloat(value[i], itemName, itemNamespace);
            }
        }

        internal override void WriteLong(long value)
        {
            base.writer.WriteAttributeString("type", "number");
            base.WriteLong(value);
        }

        internal override void WriteQName(XmlQualifiedName value)
        {
            if (value != XmlQualifiedName.Empty)
            {
                base.writer.WriteString(value.Name);
                base.writer.WriteString(":");
                base.writer.WriteString(value.Namespace);
            }
        }

        internal override void WriteShort(short value)
        {
            base.writer.WriteAttributeString("type", "number");
            base.WriteShort(value);
        }

        internal override void WriteSignedByte(sbyte value)
        {
            base.writer.WriteAttributeString("type", "number");
            base.WriteSignedByte(value);
        }

        internal override void WriteUnsignedByte(byte value)
        {
            base.writer.WriteAttributeString("type", "number");
            base.WriteUnsignedByte(value);
        }

        internal override void WriteUnsignedInt(uint value)
        {
            base.writer.WriteAttributeString("type", "number");
            base.WriteUnsignedInt(value);
        }

        internal override void WriteUnsignedLong(ulong value)
        {
            this.WriteDecimal(value);
        }

        internal override void WriteUnsignedShort(ushort value)
        {
            base.writer.WriteAttributeString("type", "number");
            base.WriteUnsignedShort(value);
        }
    }
}

