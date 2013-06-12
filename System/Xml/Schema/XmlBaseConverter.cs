namespace System.Xml.Schema
{
    using System;
    using System.Xml;

    internal abstract class XmlBaseConverter : XmlValueConverter
    {
        protected static readonly Type BooleanType = typeof(bool);
        protected static readonly Type ByteArrayType = typeof(byte[]);
        protected static readonly Type ByteType = typeof(byte);
        private Type clrTypeDefault;
        protected static readonly Type DateTimeOffsetType = typeof(DateTimeOffset);
        protected static readonly Type DateTimeType = typeof(DateTime);
        protected static readonly Type DecimalType = typeof(decimal);
        protected static readonly Type DoubleType = typeof(double);
        protected static readonly Type ICollectionType = typeof(ICollection);
        protected static readonly Type IEnumerableType = typeof(IEnumerable);
        protected static readonly Type IListType = typeof(IList);
        protected static readonly Type Int16Type = typeof(short);
        protected static readonly Type Int32Type = typeof(int);
        protected static readonly Type Int64Type = typeof(long);
        protected static readonly Type ObjectArrayType = typeof(object[]);
        protected static readonly Type ObjectType = typeof(object);
        protected static readonly Type SByteType = typeof(sbyte);
        private XmlSchemaType schemaType;
        protected static readonly Type SingleType = typeof(float);
        protected static readonly Type StringArrayType = typeof(string[]);
        protected static readonly Type StringType = typeof(string);
        protected static readonly Type TimeSpanType = typeof(TimeSpan);
        private XmlTypeCode typeCode;
        protected static readonly Type UInt16Type = typeof(ushort);
        protected static readonly Type UInt32Type = typeof(uint);
        protected static readonly Type UInt64Type = typeof(ulong);
        protected static readonly Type UriType = typeof(Uri);
        protected static readonly Type XmlAtomicValueArrayType = typeof(XmlAtomicValue[]);
        protected static readonly Type XmlAtomicValueType = typeof(XmlAtomicValue);
        protected static readonly Type XmlQualifiedNameType = typeof(XmlQualifiedName);
        protected static readonly Type XPathItemType = typeof(XPathItem);
        protected static readonly Type XPathNavigatorType = typeof(XPathNavigator);

        protected XmlBaseConverter(XmlBaseConverter converterAtomic)
        {
            this.schemaType = converterAtomic.schemaType;
            this.typeCode = converterAtomic.typeCode;
            this.clrTypeDefault = Array.CreateInstance(converterAtomic.DefaultClrType, 0).GetType();
        }

        protected XmlBaseConverter(XmlSchemaType schemaType)
        {
            XmlSchemaDatatype datatype = schemaType.Datatype;
            while ((schemaType != null) && !(schemaType is XmlSchemaSimpleType))
            {
                schemaType = schemaType.BaseXmlSchemaType;
            }
            if (schemaType == null)
            {
                schemaType = XmlSchemaType.GetBuiltInSimpleType(datatype.TypeCode);
            }
            this.schemaType = schemaType;
            this.typeCode = schemaType.TypeCode;
            this.clrTypeDefault = schemaType.Datatype.ValueType;
        }

        protected XmlBaseConverter(XmlTypeCode typeCode)
        {
            switch (typeCode)
            {
                case XmlTypeCode.Item:
                    this.clrTypeDefault = XPathItemType;
                    break;

                case XmlTypeCode.Node:
                    this.clrTypeDefault = XPathNavigatorType;
                    break;

                case XmlTypeCode.AnyAtomicType:
                    this.clrTypeDefault = XmlAtomicValueType;
                    break;
            }
            this.typeCode = typeCode;
        }

        protected XmlBaseConverter(XmlBaseConverter converterAtomic, Type clrTypeDefault)
        {
            this.schemaType = converterAtomic.schemaType;
            this.typeCode = converterAtomic.typeCode;
            this.clrTypeDefault = clrTypeDefault;
        }

        protected static string AnyUriToString(Uri value)
        {
            return value.OriginalString;
        }

        protected static string Base64BinaryToString(byte[] value)
        {
            return Convert.ToBase64String(value);
        }

        protected virtual object ChangeListType(object value, Type destinationType, IXmlNamespaceResolver nsResolver)
        {
            throw this.CreateInvalidClrMappingException(value.GetType(), destinationType);
        }

        public override object ChangeType(bool value, Type destinationType)
        {
            return this.ChangeType(value, destinationType, null);
        }

        public override object ChangeType(DateTime value, Type destinationType)
        {
            return this.ChangeType(value, destinationType, null);
        }

        public override object ChangeType(DateTimeOffset value, Type destinationType)
        {
            return this.ChangeType(value, destinationType, null);
        }

        public override object ChangeType(decimal value, Type destinationType)
        {
            return this.ChangeType(value, destinationType, null);
        }

        public override object ChangeType(double value, Type destinationType)
        {
            return this.ChangeType(value, destinationType, null);
        }

        public override object ChangeType(int value, Type destinationType)
        {
            return this.ChangeType(value, destinationType, null);
        }

        public override object ChangeType(long value, Type destinationType)
        {
            return this.ChangeType(value, destinationType, null);
        }

        public override object ChangeType(object value, Type destinationType)
        {
            return this.ChangeType(value, destinationType, null);
        }

        public override object ChangeType(float value, Type destinationType)
        {
            return this.ChangeType(value, destinationType, null);
        }

        public override object ChangeType(string value, Type destinationType)
        {
            return this.ChangeType(value, destinationType, null);
        }

        public override object ChangeType(string value, Type destinationType, IXmlNamespaceResolver nsResolver)
        {
            return this.ChangeType(value, destinationType, nsResolver);
        }

        protected Exception CreateInvalidClrMappingException(Type sourceType, Type destinationType)
        {
            if (sourceType == destinationType)
            {
                return new InvalidCastException(Res.GetString("XmlConvert_TypeBadMapping", new object[] { this.XmlTypeName, sourceType.Name }));
            }
            return new InvalidCastException(Res.GetString("XmlConvert_TypeBadMapping2", new object[] { this.XmlTypeName, sourceType.Name, destinationType.Name }));
        }

        protected static string DateOffsetToString(DateTimeOffset value)
        {
            XsdDateTime time = new XsdDateTime(value, XsdDateTimeFlags.Date);
            return time.ToString();
        }

        internal static DateTime DateTimeOffsetToDateTime(DateTimeOffset value)
        {
            return value.LocalDateTime;
        }

        protected static string DateTimeOffsetToString(DateTimeOffset value)
        {
            XsdDateTime time = new XsdDateTime(value, XsdDateTimeFlags.DateTime);
            return time.ToString();
        }

        protected static string DateTimeToString(DateTime value)
        {
            XsdDateTime time = new XsdDateTime(value, XsdDateTimeFlags.DateTime);
            return time.ToString();
        }

        protected static string DateToString(DateTime value)
        {
            XsdDateTime time = new XsdDateTime(value, XsdDateTimeFlags.Date);
            return time.ToString();
        }

        protected static string DayTimeDurationToString(TimeSpan value)
        {
            XsdDuration duration = new XsdDuration(value, XsdDuration.DurationType.DayTimeDuration);
            return duration.ToString(XsdDuration.DurationType.DayTimeDuration);
        }

        internal static int DecimalToInt32(decimal value)
        {
            if ((value < -2147483648M) || (value > 2147483647M))
            {
                throw new OverflowException(Res.GetString("XmlConvert_Overflow", new string[] { XmlConvert.ToString(value), "Int32" }));
            }
            return (int) value;
        }

        protected static long DecimalToInt64(decimal value)
        {
            if ((value < -9223372036854775808M) || (value > 9223372036854775807M))
            {
                throw new OverflowException(Res.GetString("XmlConvert_Overflow", new string[] { XmlConvert.ToString(value), "Int64" }));
            }
            return (long) value;
        }

        protected static ulong DecimalToUInt64(decimal value)
        {
            if ((value < 0M) || (value > 18446744073709551615M))
            {
                throw new OverflowException(Res.GetString("XmlConvert_Overflow", new string[] { XmlConvert.ToString(value), "UInt64" }));
            }
            return (ulong) value;
        }

        protected static string DurationToString(TimeSpan value)
        {
            XsdDuration duration = new XsdDuration(value, XsdDuration.DurationType.Duration);
            return duration.ToString(XsdDuration.DurationType.Duration);
        }

        protected static string GDayOffsetToString(DateTimeOffset value)
        {
            XsdDateTime time = new XsdDateTime(value, XsdDateTimeFlags.GDay);
            return time.ToString();
        }

        protected static string GDayToString(DateTime value)
        {
            XsdDateTime time = new XsdDateTime(value, XsdDateTimeFlags.GDay);
            return time.ToString();
        }

        protected static string GMonthDayOffsetToString(DateTimeOffset value)
        {
            XsdDateTime time = new XsdDateTime(value, XsdDateTimeFlags.GMonthDay);
            return time.ToString();
        }

        protected static string GMonthDayToString(DateTime value)
        {
            XsdDateTime time = new XsdDateTime(value, XsdDateTimeFlags.GMonthDay);
            return time.ToString();
        }

        protected static string GMonthOffsetToString(DateTimeOffset value)
        {
            XsdDateTime time = new XsdDateTime(value, XsdDateTimeFlags.GMonth);
            return time.ToString();
        }

        protected static string GMonthToString(DateTime value)
        {
            XsdDateTime time = new XsdDateTime(value, XsdDateTimeFlags.GMonth);
            return time.ToString();
        }

        protected static string GYearMonthOffsetToString(DateTimeOffset value)
        {
            XsdDateTime time = new XsdDateTime(value, XsdDateTimeFlags.GYearMonth);
            return time.ToString();
        }

        protected static string GYearMonthToString(DateTime value)
        {
            XsdDateTime time = new XsdDateTime(value, XsdDateTimeFlags.GYearMonth);
            return time.ToString();
        }

        protected static string GYearOffsetToString(DateTimeOffset value)
        {
            XsdDateTime time = new XsdDateTime(value, XsdDateTimeFlags.GYear);
            return time.ToString();
        }

        protected static string GYearToString(DateTime value)
        {
            XsdDateTime time = new XsdDateTime(value, XsdDateTimeFlags.GYear);
            return time.ToString();
        }

        protected static byte Int32ToByte(int value)
        {
            if ((value < 0) || (value > 0xff))
            {
                throw new OverflowException(Res.GetString("XmlConvert_Overflow", new string[] { XmlConvert.ToString(value), "Byte" }));
            }
            return (byte) value;
        }

        protected static short Int32ToInt16(int value)
        {
            if ((value < -32768) || (value > 0x7fff))
            {
                throw new OverflowException(Res.GetString("XmlConvert_Overflow", new string[] { XmlConvert.ToString(value), "Int16" }));
            }
            return (short) value;
        }

        protected static sbyte Int32ToSByte(int value)
        {
            if ((value < -128) || (value > 0x7f))
            {
                throw new OverflowException(Res.GetString("XmlConvert_Overflow", new string[] { XmlConvert.ToString(value), "SByte" }));
            }
            return (sbyte) value;
        }

        protected static ushort Int32ToUInt16(int value)
        {
            if ((value < 0) || (value > 0xffff))
            {
                throw new OverflowException(Res.GetString("XmlConvert_Overflow", new string[] { XmlConvert.ToString(value), "UInt16" }));
            }
            return (ushort) value;
        }

        protected static int Int64ToInt32(long value)
        {
            if ((value < -2147483648L) || (value > 0x7fffffffL))
            {
                throw new OverflowException(Res.GetString("XmlConvert_Overflow", new string[] { XmlConvert.ToString(value), "Int32" }));
            }
            return (int) value;
        }

        protected static uint Int64ToUInt32(long value)
        {
            if ((value < 0L) || (value > 0xffffffffL))
            {
                throw new OverflowException(Res.GetString("XmlConvert_Overflow", new string[] { XmlConvert.ToString(value), "UInt32" }));
            }
            return (uint) value;
        }

        protected static bool IsDerivedFrom(Type derivedType, Type baseType)
        {
            while (derivedType != null)
            {
                if (derivedType == baseType)
                {
                    return true;
                }
                derivedType = derivedType.BaseType;
            }
            return false;
        }

        protected static string QNameToString(XmlQualifiedName name)
        {
            if (name.Namespace.Length == 0)
            {
                return name.Name;
            }
            if (name.Namespace == "http://www.w3.org/2001/XMLSchema")
            {
                return ("xs:" + name.Name);
            }
            if (name.Namespace == "http://www.w3.org/2003/11/xpath-datatypes")
            {
                return ("xdt:" + name.Name);
            }
            return ("{" + name.Namespace + "}" + name.Name);
        }

        protected static string QNameToString(XmlQualifiedName qname, IXmlNamespaceResolver nsResolver)
        {
            if (nsResolver == null)
            {
                return ("{" + qname.Namespace + "}" + qname.Name);
            }
            string prefix = nsResolver.LookupPrefix(qname.Namespace);
            if (prefix == null)
            {
                throw new InvalidCastException(Res.GetString("XmlConvert_TypeNoPrefix", new object[] { qname.ToString(), qname.Namespace }));
            }
            if (prefix.Length == 0)
            {
                return qname.Name;
            }
            return (prefix + ":" + qname.Name);
        }

        protected static byte[] StringToBase64Binary(string value)
        {
            return Convert.FromBase64String(XmlConvert.TrimString(value));
        }

        protected static DateTime StringToDate(string value)
        {
            return (DateTime) new XsdDateTime(value, XsdDateTimeFlags.Date);
        }

        protected static DateTimeOffset StringToDateOffset(string value)
        {
            return (DateTimeOffset) new XsdDateTime(value, XsdDateTimeFlags.Date);
        }

        protected static DateTime StringToDateTime(string value)
        {
            return (DateTime) new XsdDateTime(value, XsdDateTimeFlags.DateTime);
        }

        protected static DateTimeOffset StringToDateTimeOffset(string value)
        {
            return (DateTimeOffset) new XsdDateTime(value, XsdDateTimeFlags.DateTime);
        }

        protected static TimeSpan StringToDayTimeDuration(string value)
        {
            XsdDuration duration = new XsdDuration(value, XsdDuration.DurationType.DayTimeDuration);
            return duration.ToTimeSpan(XsdDuration.DurationType.DayTimeDuration);
        }

        protected static TimeSpan StringToDuration(string value)
        {
            XsdDuration duration = new XsdDuration(value, XsdDuration.DurationType.Duration);
            return duration.ToTimeSpan(XsdDuration.DurationType.Duration);
        }

        protected static DateTime StringToGDay(string value)
        {
            return (DateTime) new XsdDateTime(value, XsdDateTimeFlags.GDay);
        }

        protected static DateTimeOffset StringToGDayOffset(string value)
        {
            return (DateTimeOffset) new XsdDateTime(value, XsdDateTimeFlags.GDay);
        }

        protected static DateTime StringToGMonth(string value)
        {
            return (DateTime) new XsdDateTime(value, XsdDateTimeFlags.GMonth);
        }

        protected static DateTime StringToGMonthDay(string value)
        {
            return (DateTime) new XsdDateTime(value, XsdDateTimeFlags.GMonthDay);
        }

        protected static DateTimeOffset StringToGMonthDayOffset(string value)
        {
            return (DateTimeOffset) new XsdDateTime(value, XsdDateTimeFlags.GMonthDay);
        }

        protected static DateTimeOffset StringToGMonthOffset(string value)
        {
            return (DateTimeOffset) new XsdDateTime(value, XsdDateTimeFlags.GMonth);
        }

        protected static DateTime StringToGYear(string value)
        {
            return (DateTime) new XsdDateTime(value, XsdDateTimeFlags.GYear);
        }

        protected static DateTime StringToGYearMonth(string value)
        {
            return (DateTime) new XsdDateTime(value, XsdDateTimeFlags.GYearMonth);
        }

        protected static DateTimeOffset StringToGYearMonthOffset(string value)
        {
            return (DateTimeOffset) new XsdDateTime(value, XsdDateTimeFlags.GYearMonth);
        }

        protected static DateTimeOffset StringToGYearOffset(string value)
        {
            return (DateTimeOffset) new XsdDateTime(value, XsdDateTimeFlags.GYear);
        }

        protected static byte[] StringToHexBinary(string value)
        {
            byte[] buffer;
            try
            {
                buffer = XmlConvert.FromBinHexString(XmlConvert.TrimString(value), false);
            }
            catch (XmlException exception)
            {
                throw new FormatException(exception.Message);
            }
            return buffer;
        }

        protected static XmlQualifiedName StringToQName(string value, IXmlNamespaceResolver nsResolver)
        {
            string str;
            string str2;
            value = value.Trim();
            try
            {
                ValidateNames.ParseQNameThrow(value, out str, out str2);
            }
            catch (XmlException exception)
            {
                throw new FormatException(exception.Message);
            }
            if (nsResolver == null)
            {
                throw new InvalidCastException(Res.GetString("XmlConvert_TypeNoNamespace", new object[] { value, str }));
            }
            string ns = nsResolver.LookupNamespace(str);
            if (ns == null)
            {
                throw new InvalidCastException(Res.GetString("XmlConvert_TypeNoNamespace", new object[] { value, str }));
            }
            return new XmlQualifiedName(str2, ns);
        }

        protected static DateTime StringToTime(string value)
        {
            return (DateTime) new XsdDateTime(value, XsdDateTimeFlags.Time);
        }

        protected static DateTimeOffset StringToTimeOffset(string value)
        {
            return (DateTimeOffset) new XsdDateTime(value, XsdDateTimeFlags.Time);
        }

        protected static TimeSpan StringToYearMonthDuration(string value)
        {
            XsdDuration duration = new XsdDuration(value, XsdDuration.DurationType.YearMonthDuration);
            return duration.ToTimeSpan(XsdDuration.DurationType.YearMonthDuration);
        }

        protected static string TimeOffsetToString(DateTimeOffset value)
        {
            XsdDateTime time = new XsdDateTime(value, XsdDateTimeFlags.Time);
            return time.ToString();
        }

        protected static string TimeToString(DateTime value)
        {
            XsdDateTime time = new XsdDateTime(value, XsdDateTimeFlags.Time);
            return time.ToString();
        }

        public override bool ToBoolean(bool value)
        {
            return (bool) this.ChangeType(value, BooleanType, null);
        }

        public override bool ToBoolean(DateTime value)
        {
            return (bool) this.ChangeType(value, BooleanType, null);
        }

        public override bool ToBoolean(DateTimeOffset value)
        {
            return (bool) this.ChangeType(value, BooleanType, null);
        }

        public override bool ToBoolean(decimal value)
        {
            return (bool) this.ChangeType(value, BooleanType, null);
        }

        public override bool ToBoolean(double value)
        {
            return (bool) this.ChangeType(value, BooleanType, null);
        }

        public override bool ToBoolean(int value)
        {
            return (bool) this.ChangeType(value, BooleanType, null);
        }

        public override bool ToBoolean(long value)
        {
            return (bool) this.ChangeType(value, BooleanType, null);
        }

        public override bool ToBoolean(object value)
        {
            return (bool) this.ChangeType(value, BooleanType, null);
        }

        public override bool ToBoolean(float value)
        {
            return (bool) this.ChangeType(value, BooleanType, null);
        }

        public override bool ToBoolean(string value)
        {
            return (bool) this.ChangeType(value, BooleanType, null);
        }

        public override DateTime ToDateTime(bool value)
        {
            return (DateTime) this.ChangeType(value, DateTimeType, null);
        }

        public override DateTime ToDateTime(DateTime value)
        {
            return (DateTime) this.ChangeType(value, DateTimeType, null);
        }

        public override DateTime ToDateTime(DateTimeOffset value)
        {
            return (DateTime) this.ChangeType(value, DateTimeType, null);
        }

        public override DateTime ToDateTime(decimal value)
        {
            return (DateTime) this.ChangeType(value, DateTimeType, null);
        }

        public override DateTime ToDateTime(double value)
        {
            return (DateTime) this.ChangeType(value, DateTimeType, null);
        }

        public override DateTime ToDateTime(int value)
        {
            return (DateTime) this.ChangeType(value, DateTimeType, null);
        }

        public override DateTime ToDateTime(long value)
        {
            return (DateTime) this.ChangeType(value, DateTimeType, null);
        }

        public override DateTime ToDateTime(object value)
        {
            return (DateTime) this.ChangeType(value, DateTimeType, null);
        }

        public override DateTime ToDateTime(float value)
        {
            return (DateTime) this.ChangeType(value, DateTimeType, null);
        }

        public override DateTime ToDateTime(string value)
        {
            return (DateTime) this.ChangeType(value, DateTimeType, null);
        }

        public override DateTimeOffset ToDateTimeOffset(bool value)
        {
            return (DateTimeOffset) this.ChangeType(value, DateTimeOffsetType, null);
        }

        public override DateTimeOffset ToDateTimeOffset(DateTime value)
        {
            return (DateTimeOffset) this.ChangeType(value, DateTimeOffsetType, null);
        }

        public override DateTimeOffset ToDateTimeOffset(DateTimeOffset value)
        {
            return (DateTimeOffset) this.ChangeType(value, DateTimeOffsetType, null);
        }

        public override DateTimeOffset ToDateTimeOffset(decimal value)
        {
            return (DateTimeOffset) this.ChangeType(value, DateTimeOffsetType, null);
        }

        public override DateTimeOffset ToDateTimeOffset(double value)
        {
            return (DateTimeOffset) this.ChangeType(value, DateTimeOffsetType, null);
        }

        public override DateTimeOffset ToDateTimeOffset(int value)
        {
            return (DateTimeOffset) this.ChangeType(value, DateTimeOffsetType, null);
        }

        public override DateTimeOffset ToDateTimeOffset(long value)
        {
            return (DateTimeOffset) this.ChangeType(value, DateTimeOffsetType, null);
        }

        public override DateTimeOffset ToDateTimeOffset(object value)
        {
            return (DateTimeOffset) this.ChangeType(value, DateTimeOffsetType, null);
        }

        public override DateTimeOffset ToDateTimeOffset(float value)
        {
            return (DateTimeOffset) this.ChangeType(value, DateTimeOffsetType, null);
        }

        public override DateTimeOffset ToDateTimeOffset(string value)
        {
            return (DateTimeOffset) this.ChangeType(value, DateTimeOffsetType, null);
        }

        public override decimal ToDecimal(bool value)
        {
            return (decimal) this.ChangeType(value, DecimalType, null);
        }

        public override decimal ToDecimal(DateTime value)
        {
            return (decimal) this.ChangeType(value, DecimalType, null);
        }

        public override decimal ToDecimal(DateTimeOffset value)
        {
            return (decimal) this.ChangeType(value, DecimalType, null);
        }

        public override decimal ToDecimal(decimal value)
        {
            return (decimal) this.ChangeType(value, DecimalType, null);
        }

        public override decimal ToDecimal(double value)
        {
            return (decimal) this.ChangeType(value, DecimalType, null);
        }

        public override decimal ToDecimal(int value)
        {
            return (decimal) this.ChangeType(value, DecimalType, null);
        }

        public override decimal ToDecimal(long value)
        {
            return (decimal) this.ChangeType(value, DecimalType, null);
        }

        public override decimal ToDecimal(object value)
        {
            return (decimal) this.ChangeType(value, DecimalType, null);
        }

        public override decimal ToDecimal(float value)
        {
            return (decimal) this.ChangeType(value, DecimalType, null);
        }

        public override decimal ToDecimal(string value)
        {
            return (decimal) this.ChangeType(value, DecimalType, null);
        }

        public override double ToDouble(bool value)
        {
            return (double) this.ChangeType(value, DoubleType, null);
        }

        public override double ToDouble(DateTime value)
        {
            return (double) this.ChangeType(value, DoubleType, null);
        }

        public override double ToDouble(DateTimeOffset value)
        {
            return (double) this.ChangeType(value, DoubleType, null);
        }

        public override double ToDouble(decimal value)
        {
            return (double) this.ChangeType(value, DoubleType, null);
        }

        public override double ToDouble(double value)
        {
            return (double) this.ChangeType(value, DoubleType, null);
        }

        public override double ToDouble(int value)
        {
            return (double) this.ChangeType(value, DoubleType, null);
        }

        public override double ToDouble(long value)
        {
            return (double) this.ChangeType(value, DoubleType, null);
        }

        public override double ToDouble(object value)
        {
            return (double) this.ChangeType(value, DoubleType, null);
        }

        public override double ToDouble(float value)
        {
            return (double) this.ChangeType(value, DoubleType, null);
        }

        public override double ToDouble(string value)
        {
            return (double) this.ChangeType(value, DoubleType, null);
        }

        public override int ToInt32(bool value)
        {
            return (int) this.ChangeType(value, Int32Type, null);
        }

        public override int ToInt32(DateTime value)
        {
            return (int) this.ChangeType(value, Int32Type, null);
        }

        public override int ToInt32(DateTimeOffset value)
        {
            return (int) this.ChangeType(value, Int32Type, null);
        }

        public override int ToInt32(decimal value)
        {
            return (int) this.ChangeType(value, Int32Type, null);
        }

        public override int ToInt32(double value)
        {
            return (int) this.ChangeType(value, Int32Type, null);
        }

        public override int ToInt32(int value)
        {
            return (int) this.ChangeType(value, Int32Type, null);
        }

        public override int ToInt32(long value)
        {
            return (int) this.ChangeType(value, Int32Type, null);
        }

        public override int ToInt32(object value)
        {
            return (int) this.ChangeType(value, Int32Type, null);
        }

        public override int ToInt32(float value)
        {
            return (int) this.ChangeType(value, Int32Type, null);
        }

        public override int ToInt32(string value)
        {
            return (int) this.ChangeType(value, Int32Type, null);
        }

        public override long ToInt64(bool value)
        {
            return (long) this.ChangeType(value, Int64Type, null);
        }

        public override long ToInt64(DateTime value)
        {
            return (long) this.ChangeType(value, Int64Type, null);
        }

        public override long ToInt64(DateTimeOffset value)
        {
            return (long) this.ChangeType(value, Int64Type, null);
        }

        public override long ToInt64(decimal value)
        {
            return (long) this.ChangeType(value, Int64Type, null);
        }

        public override long ToInt64(double value)
        {
            return (long) this.ChangeType(value, Int64Type, null);
        }

        public override long ToInt64(int value)
        {
            return (long) this.ChangeType(value, Int64Type, null);
        }

        public override long ToInt64(long value)
        {
            return (long) this.ChangeType(value, Int64Type, null);
        }

        public override long ToInt64(object value)
        {
            return (long) this.ChangeType(value, Int64Type, null);
        }

        public override long ToInt64(float value)
        {
            return (long) this.ChangeType(value, Int64Type, null);
        }

        public override long ToInt64(string value)
        {
            return (long) this.ChangeType(value, Int64Type, null);
        }

        public override float ToSingle(bool value)
        {
            return (float) this.ChangeType(value, SingleType, null);
        }

        public override float ToSingle(DateTime value)
        {
            return (float) this.ChangeType(value, SingleType, null);
        }

        public override float ToSingle(DateTimeOffset value)
        {
            return (float) this.ChangeType(value, SingleType, null);
        }

        public override float ToSingle(decimal value)
        {
            return (float) this.ChangeType(value, SingleType, null);
        }

        public override float ToSingle(double value)
        {
            return (float) this.ChangeType(value, SingleType, null);
        }

        public override float ToSingle(int value)
        {
            return (float) this.ChangeType(value, SingleType, null);
        }

        public override float ToSingle(long value)
        {
            return (float) this.ChangeType(value, SingleType, null);
        }

        public override float ToSingle(object value)
        {
            return (float) this.ChangeType(value, SingleType, null);
        }

        public override float ToSingle(float value)
        {
            return (float) this.ChangeType(value, SingleType, null);
        }

        public override float ToSingle(string value)
        {
            return (float) this.ChangeType(value, SingleType, null);
        }

        public override string ToString(bool value)
        {
            return (string) this.ChangeType(value, StringType, null);
        }

        public override string ToString(DateTime value)
        {
            return (string) this.ChangeType(value, StringType, null);
        }

        public override string ToString(DateTimeOffset value)
        {
            return (string) this.ChangeType(value, StringType, null);
        }

        public override string ToString(decimal value)
        {
            return (string) this.ChangeType(value, StringType, null);
        }

        public override string ToString(double value)
        {
            return (string) this.ChangeType(value, StringType, null);
        }

        public override string ToString(int value)
        {
            return (string) this.ChangeType(value, StringType, null);
        }

        public override string ToString(long value)
        {
            return (string) this.ChangeType(value, StringType, null);
        }

        public override string ToString(object value)
        {
            return this.ToString(value, null);
        }

        public override string ToString(float value)
        {
            return (string) this.ChangeType(value, StringType, null);
        }

        public override string ToString(string value)
        {
            return this.ToString(value, null);
        }

        public override string ToString(object value, IXmlNamespaceResolver nsResolver)
        {
            return (string) this.ChangeType(value, StringType, nsResolver);
        }

        public override string ToString(string value, IXmlNamespaceResolver nsResolver)
        {
            return (string) this.ChangeType(value, StringType, nsResolver);
        }

        protected static DateTime UntypedAtomicToDateTime(string value)
        {
            return (DateTime) new XsdDateTime(value, XsdDateTimeFlags.AllXsd);
        }

        protected static DateTimeOffset UntypedAtomicToDateTimeOffset(string value)
        {
            return (DateTimeOffset) new XsdDateTime(value, XsdDateTimeFlags.AllXsd);
        }

        protected static string YearMonthDurationToString(TimeSpan value)
        {
            XsdDuration duration = new XsdDuration(value, XsdDuration.DurationType.YearMonthDuration);
            return duration.ToString(XsdDuration.DurationType.YearMonthDuration);
        }

        protected Type DefaultClrType
        {
            get
            {
                return this.clrTypeDefault;
            }
        }

        protected XmlSchemaType SchemaType
        {
            get
            {
                return this.schemaType;
            }
        }

        protected XmlTypeCode TypeCode
        {
            get
            {
                return this.typeCode;
            }
        }

        protected string XmlTypeName
        {
            get
            {
                XmlSchemaType schemaType = this.schemaType;
                if (schemaType != null)
                {
                    while (schemaType.QualifiedName.IsEmpty)
                    {
                        schemaType = schemaType.BaseXmlSchemaType;
                    }
                    return QNameToString(schemaType.QualifiedName);
                }
                if (this.typeCode == XmlTypeCode.Node)
                {
                    return "node";
                }
                if (this.typeCode == XmlTypeCode.AnyAtomicType)
                {
                    return "xdt:anyAtomicType";
                }
                return "item";
            }
        }
    }
}

