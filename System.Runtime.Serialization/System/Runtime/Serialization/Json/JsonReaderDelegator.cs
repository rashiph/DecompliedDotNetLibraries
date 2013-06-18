namespace System.Runtime.Serialization.Json
{
    using System;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Xml;

    internal class JsonReaderDelegator : XmlReaderDelegator
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public JsonReaderDelegator(XmlReader reader) : base(reader)
        {
        }

        internal static DateTime ParseJsonDate(string originalDateTimeValue)
        {
            string str;
            long num;
            DateTime time2;
            if (!string.IsNullOrEmpty(originalDateTimeValue))
            {
                str = originalDateTimeValue.Trim();
            }
            else
            {
                str = originalDateTimeValue;
            }
            if ((string.IsNullOrEmpty(str) || !str.StartsWith("/Date(", StringComparison.Ordinal)) || !str.EndsWith(")/", StringComparison.Ordinal))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(System.Runtime.Serialization.SR.GetString("JsonInvalidDateTimeString", new object[] { originalDateTimeValue, @"\/Date(", @")\/" })));
            }
            string s = str.Substring(6, str.Length - 8);
            DateTimeKind utc = DateTimeKind.Utc;
            int index = s.IndexOf('+', 1);
            if (index == -1)
            {
                index = s.IndexOf('-', 1);
            }
            if (index != -1)
            {
                utc = DateTimeKind.Local;
                s = s.Substring(0, index);
            }
            try
            {
                num = long.Parse(s, CultureInfo.InvariantCulture);
            }
            catch (ArgumentException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(s, "Int64", exception));
            }
            catch (FormatException exception2)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(s, "Int64", exception2));
            }
            catch (OverflowException exception3)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(s, "Int64", exception3));
            }
            long ticks = (num * 0x2710L) + JsonGlobals.unixEpochTicks;
            try
            {
                DateTime time = new DateTime(ticks, DateTimeKind.Utc);
                switch (utc)
                {
                    case DateTimeKind.Unspecified:
                        return DateTime.SpecifyKind(time.ToLocalTime(), DateTimeKind.Unspecified);

                    case DateTimeKind.Local:
                        return time.ToLocalTime();
                }
                time2 = time;
            }
            catch (ArgumentException exception4)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(s, "DateTime", exception4));
            }
            return time2;
        }

        internal static XmlQualifiedName ParseQualifiedName(string qname)
        {
            string str;
            string str2;
            if (string.IsNullOrEmpty(qname))
            {
                str = str2 = string.Empty;
            }
            else
            {
                qname = qname.Trim();
                int index = qname.IndexOf(':');
                if (index >= 0)
                {
                    str = qname.Substring(0, index);
                    str2 = qname.Substring(index + 1);
                }
                else
                {
                    str = qname;
                    str2 = string.Empty;
                }
            }
            return new XmlQualifiedName(str, str2);
        }

        internal override byte[] ReadContentAsBase64()
        {
            if (base.isEndOfEmptyElement)
            {
                return new byte[0];
            }
            if (base.dictionaryReader == null)
            {
                XmlDictionaryReader reader = XmlDictionaryReader.CreateDictionaryReader(base.reader);
                return ByteArrayHelperWithString.Instance.ReadArray(reader, "item", string.Empty, reader.Quotas.MaxArrayLength);
            }
            return ByteArrayHelperWithString.Instance.ReadArray(base.dictionaryReader, "item", string.Empty, base.dictionaryReader.Quotas.MaxArrayLength);
        }

        internal override char ReadContentAsChar()
        {
            return XmlConvert.ToChar(base.ReadContentAsString());
        }

        internal override DateTime ReadContentAsDateTime()
        {
            return ParseJsonDate(base.ReadContentAsString());
        }

        internal override XmlQualifiedName ReadContentAsQName()
        {
            return ParseQualifiedName(base.ReadContentAsString());
        }

        internal override ulong ReadContentAsUnsignedLong()
        {
            ulong num;
            string s = base.reader.ReadContentAsString();
            if ((s == null) || (s.Length == 0))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(XmlObjectSerializer.TryAddLineInfo(this, System.Runtime.Serialization.SR.GetString("XmlInvalidConversion", new object[] { s, "UInt64" }))));
            }
            try
            {
                num = ulong.Parse(s, NumberStyles.Float, NumberFormatInfo.InvariantInfo);
            }
            catch (ArgumentException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(s, "UInt64", exception));
            }
            catch (FormatException exception2)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(s, "UInt64", exception2));
            }
            catch (OverflowException exception3)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(s, "UInt64", exception3));
            }
            return num;
        }

        internal override byte[] ReadElementContentAsBase64()
        {
            if (base.isEndOfEmptyElement)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("XmlStartElementExpected", new object[] { "EndElement" })));
            }
            if (base.reader.IsStartElement() && base.reader.IsEmptyElement)
            {
                base.reader.Read();
                return new byte[0];
            }
            base.reader.ReadStartElement();
            byte[] buffer = this.ReadContentAsBase64();
            base.reader.ReadEndElement();
            return buffer;
        }

        internal override char ReadElementContentAsChar()
        {
            return XmlConvert.ToChar(base.ReadElementContentAsString());
        }

        internal override DateTime ReadElementContentAsDateTime()
        {
            return ParseJsonDate(base.ReadElementContentAsString());
        }

        internal override ulong ReadElementContentAsUnsignedLong()
        {
            ulong num;
            if (base.isEndOfEmptyElement)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("XmlStartElementExpected", new object[] { "EndElement" })));
            }
            string s = base.reader.ReadElementContentAsString();
            if ((s == null) || (s.Length == 0))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(XmlObjectSerializer.TryAddLineInfo(this, System.Runtime.Serialization.SR.GetString("XmlInvalidConversion", new object[] { s, "UInt64" }))));
            }
            try
            {
                num = ulong.Parse(s, NumberStyles.Float, NumberFormatInfo.InvariantInfo);
            }
            catch (ArgumentException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(s, "UInt64", exception));
            }
            catch (FormatException exception2)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(s, "UInt64", exception2));
            }
            catch (OverflowException exception3)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(s, "UInt64", exception3));
            }
            return num;
        }

        internal bool TryReadJsonDateTimeArray(XmlObjectSerializerReadContext context, XmlDictionaryString itemName, XmlDictionaryString itemNamespace, int arrayLength, out DateTime[] array)
        {
            if ((base.dictionaryReader == null) || (arrayLength != -1))
            {
                array = null;
                return false;
            }
            array = DateTimeArrayJsonHelperWithString.Instance.ReadArray(base.dictionaryReader, XmlDictionaryString.GetString(itemName), XmlDictionaryString.GetString(itemNamespace), base.GetArrayLengthQuota(context));
            context.IncrementItemCount(array.Length);
            return true;
        }

        internal XmlDictionaryReaderQuotas ReaderQuotas
        {
            get
            {
                if (base.dictionaryReader == null)
                {
                    return null;
                }
                return base.dictionaryReader.Quotas;
            }
        }

        private class DateTimeArrayJsonHelperWithString : ArrayHelper<string, DateTime>
        {
            public static readonly JsonReaderDelegator.DateTimeArrayJsonHelperWithString Instance = new JsonReaderDelegator.DateTimeArrayJsonHelperWithString();

            protected override int ReadArray(XmlDictionaryReader reader, string localName, string namespaceUri, DateTime[] array, int offset, int count)
            {
                XmlJsonReader.CheckArray(array, offset, count);
                int num = 0;
                while ((num < count) && reader.IsStartElement("item", string.Empty))
                {
                    array[offset + num] = JsonReaderDelegator.ParseJsonDate(reader.ReadElementContentAsString());
                    num++;
                }
                return num;
            }

            protected override void WriteArray(XmlDictionaryWriter writer, string prefix, string localName, string namespaceUri, DateTime[] array, int offset, int count)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
            }
        }
    }
}

