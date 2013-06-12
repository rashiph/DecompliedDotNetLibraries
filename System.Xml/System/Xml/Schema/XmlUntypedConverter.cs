namespace System.Xml.Schema
{
    using System;
    using System.Xml;

    internal class XmlUntypedConverter : XmlListConverter
    {
        private bool allowListToList;
        public static readonly XmlValueConverter Untyped = new XmlUntypedConverter(new XmlUntypedConverter(), false);
        public static readonly XmlValueConverter UntypedList = new XmlUntypedConverter(new XmlUntypedConverter(), true);

        protected XmlUntypedConverter() : base(DatatypeImplementation.UntypedAtomicType)
        {
        }

        protected XmlUntypedConverter(XmlUntypedConverter atomicConverter, bool allowListToList) : base(atomicConverter, allowListToList ? XmlBaseConverter.StringArrayType : XmlBaseConverter.StringType)
        {
            this.allowListToList = allowListToList;
        }

        protected override object ChangeListType(object value, Type destinationType, IXmlNamespaceResolver nsResolver)
        {
            Type clrType = value.GetType();
            if ((base.atomicConverter != null) && ((this.allowListToList || !(clrType != XmlBaseConverter.StringType)) || !(destinationType != XmlBaseConverter.StringType)))
            {
                return base.ChangeListType(value, destinationType, nsResolver);
            }
            if (this.SupportsType(clrType))
            {
                throw new InvalidCastException(Res.GetString("XmlConvert_TypeToString", new object[] { base.XmlTypeName, clrType.Name }));
            }
            if (this.SupportsType(destinationType))
            {
                throw new InvalidCastException(Res.GetString("XmlConvert_TypeFromString", new object[] { base.XmlTypeName, destinationType.Name }));
            }
            throw base.CreateInvalidClrMappingException(clrType, destinationType);
        }

        public override object ChangeType(bool value, Type destinationType)
        {
            if (destinationType == null)
            {
                throw new ArgumentNullException("destinationType");
            }
            if (destinationType == XmlBaseConverter.ObjectType)
            {
                destinationType = base.DefaultClrType;
            }
            if (destinationType == XmlBaseConverter.StringType)
            {
                return XmlConvert.ToString(value);
            }
            return this.ChangeTypeWildcardSource(value, destinationType, null);
        }

        public override object ChangeType(DateTime value, Type destinationType)
        {
            if (destinationType == null)
            {
                throw new ArgumentNullException("destinationType");
            }
            if (destinationType == XmlBaseConverter.ObjectType)
            {
                destinationType = base.DefaultClrType;
            }
            if (destinationType == XmlBaseConverter.StringType)
            {
                return XmlBaseConverter.DateTimeToString(value);
            }
            return this.ChangeTypeWildcardSource(value, destinationType, null);
        }

        public override object ChangeType(DateTimeOffset value, Type destinationType)
        {
            if (destinationType == null)
            {
                throw new ArgumentNullException("destinationType");
            }
            if (destinationType == XmlBaseConverter.ObjectType)
            {
                destinationType = base.DefaultClrType;
            }
            if (destinationType == XmlBaseConverter.StringType)
            {
                return XmlBaseConverter.DateTimeOffsetToString(value);
            }
            return this.ChangeTypeWildcardSource(value, destinationType, null);
        }

        public override object ChangeType(decimal value, Type destinationType)
        {
            if (destinationType == null)
            {
                throw new ArgumentNullException("destinationType");
            }
            if (destinationType == XmlBaseConverter.ObjectType)
            {
                destinationType = base.DefaultClrType;
            }
            if (destinationType == XmlBaseConverter.StringType)
            {
                return XmlConvert.ToString(value);
            }
            return this.ChangeTypeWildcardSource(value, destinationType, null);
        }

        public override object ChangeType(double value, Type destinationType)
        {
            if (destinationType == null)
            {
                throw new ArgumentNullException("destinationType");
            }
            if (destinationType == XmlBaseConverter.ObjectType)
            {
                destinationType = base.DefaultClrType;
            }
            if (destinationType == XmlBaseConverter.StringType)
            {
                return XmlConvert.ToString(value);
            }
            return this.ChangeTypeWildcardSource(value, destinationType, null);
        }

        public override object ChangeType(int value, Type destinationType)
        {
            if (destinationType == null)
            {
                throw new ArgumentNullException("destinationType");
            }
            if (destinationType == XmlBaseConverter.ObjectType)
            {
                destinationType = base.DefaultClrType;
            }
            if (destinationType == XmlBaseConverter.StringType)
            {
                return XmlConvert.ToString(value);
            }
            return this.ChangeTypeWildcardSource(value, destinationType, null);
        }

        public override object ChangeType(long value, Type destinationType)
        {
            if (destinationType == null)
            {
                throw new ArgumentNullException("destinationType");
            }
            if (destinationType == XmlBaseConverter.ObjectType)
            {
                destinationType = base.DefaultClrType;
            }
            if (destinationType == XmlBaseConverter.StringType)
            {
                return XmlConvert.ToString(value);
            }
            return this.ChangeTypeWildcardSource(value, destinationType, null);
        }

        public override object ChangeType(float value, Type destinationType)
        {
            if (destinationType == null)
            {
                throw new ArgumentNullException("destinationType");
            }
            if (destinationType == XmlBaseConverter.ObjectType)
            {
                destinationType = base.DefaultClrType;
            }
            if (destinationType == XmlBaseConverter.StringType)
            {
                return XmlConvert.ToString(value);
            }
            return this.ChangeTypeWildcardSource(value, destinationType, null);
        }

        public override object ChangeType(object value, Type destinationType, IXmlNamespaceResolver nsResolver)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (destinationType == null)
            {
                throw new ArgumentNullException("destinationType");
            }
            Type type = value.GetType();
            if (destinationType == XmlBaseConverter.ObjectType)
            {
                destinationType = base.DefaultClrType;
            }
            if ((destinationType == XmlBaseConverter.BooleanType) && (type == XmlBaseConverter.StringType))
            {
                return XmlConvert.ToBoolean((string) value);
            }
            if ((destinationType == XmlBaseConverter.ByteType) && (type == XmlBaseConverter.StringType))
            {
                return XmlBaseConverter.Int32ToByte(XmlConvert.ToInt32((string) value));
            }
            if ((destinationType == XmlBaseConverter.ByteArrayType) && (type == XmlBaseConverter.StringType))
            {
                return XmlBaseConverter.StringToBase64Binary((string) value);
            }
            if ((destinationType == XmlBaseConverter.DateTimeType) && (type == XmlBaseConverter.StringType))
            {
                return XmlBaseConverter.UntypedAtomicToDateTime((string) value);
            }
            if ((destinationType == XmlBaseConverter.DateTimeOffsetType) && (type == XmlBaseConverter.StringType))
            {
                return XmlBaseConverter.UntypedAtomicToDateTimeOffset((string) value);
            }
            if ((destinationType == XmlBaseConverter.DecimalType) && (type == XmlBaseConverter.StringType))
            {
                return XmlConvert.ToDecimal((string) value);
            }
            if ((destinationType == XmlBaseConverter.DoubleType) && (type == XmlBaseConverter.StringType))
            {
                return XmlConvert.ToDouble((string) value);
            }
            if ((destinationType == XmlBaseConverter.Int16Type) && (type == XmlBaseConverter.StringType))
            {
                return XmlBaseConverter.Int32ToInt16(XmlConvert.ToInt32((string) value));
            }
            if ((destinationType == XmlBaseConverter.Int32Type) && (type == XmlBaseConverter.StringType))
            {
                return XmlConvert.ToInt32((string) value);
            }
            if ((destinationType == XmlBaseConverter.Int64Type) && (type == XmlBaseConverter.StringType))
            {
                return XmlConvert.ToInt64((string) value);
            }
            if ((destinationType == XmlBaseConverter.SByteType) && (type == XmlBaseConverter.StringType))
            {
                return XmlBaseConverter.Int32ToSByte(XmlConvert.ToInt32((string) value));
            }
            if ((destinationType == XmlBaseConverter.SingleType) && (type == XmlBaseConverter.StringType))
            {
                return XmlConvert.ToSingle((string) value);
            }
            if ((destinationType == XmlBaseConverter.TimeSpanType) && (type == XmlBaseConverter.StringType))
            {
                return XmlBaseConverter.StringToDuration((string) value);
            }
            if ((destinationType == XmlBaseConverter.UInt16Type) && (type == XmlBaseConverter.StringType))
            {
                return XmlBaseConverter.Int32ToUInt16(XmlConvert.ToInt32((string) value));
            }
            if ((destinationType == XmlBaseConverter.UInt32Type) && (type == XmlBaseConverter.StringType))
            {
                return XmlBaseConverter.Int64ToUInt32(XmlConvert.ToInt64((string) value));
            }
            if ((destinationType == XmlBaseConverter.UInt64Type) && (type == XmlBaseConverter.StringType))
            {
                return XmlBaseConverter.DecimalToUInt64(XmlConvert.ToDecimal((string) value));
            }
            if ((destinationType == XmlBaseConverter.UriType) && (type == XmlBaseConverter.StringType))
            {
                return XmlConvert.ToUri((string) value);
            }
            if (destinationType == XmlBaseConverter.XmlAtomicValueType)
            {
                if (type == XmlBaseConverter.StringType)
                {
                    return new XmlAtomicValue(base.SchemaType, (string) value);
                }
                if (type == XmlBaseConverter.XmlAtomicValueType)
                {
                    return (XmlAtomicValue) value;
                }
            }
            if ((destinationType == XmlBaseConverter.XmlQualifiedNameType) && (type == XmlBaseConverter.StringType))
            {
                return XmlBaseConverter.StringToQName((string) value, nsResolver);
            }
            if (destinationType == XmlBaseConverter.XPathItemType)
            {
                if (type == XmlBaseConverter.StringType)
                {
                    return new XmlAtomicValue(base.SchemaType, (string) value);
                }
                if (type == XmlBaseConverter.XmlAtomicValueType)
                {
                    return (XmlAtomicValue) value;
                }
            }
            if (destinationType == XmlBaseConverter.StringType)
            {
                return this.ToString(value, nsResolver);
            }
            if (destinationType == XmlBaseConverter.XmlAtomicValueType)
            {
                return new XmlAtomicValue(base.SchemaType, this.ToString(value, nsResolver));
            }
            if (destinationType == XmlBaseConverter.XPathItemType)
            {
                return new XmlAtomicValue(base.SchemaType, this.ToString(value, nsResolver));
            }
            if (type == XmlBaseConverter.XmlAtomicValueType)
            {
                return ((XmlAtomicValue) value).ValueAs(destinationType, nsResolver);
            }
            return this.ChangeListType(value, destinationType, nsResolver);
        }

        public override object ChangeType(string value, Type destinationType, IXmlNamespaceResolver nsResolver)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (destinationType == null)
            {
                throw new ArgumentNullException("destinationType");
            }
            if (destinationType == XmlBaseConverter.ObjectType)
            {
                destinationType = base.DefaultClrType;
            }
            if (destinationType == XmlBaseConverter.BooleanType)
            {
                return XmlConvert.ToBoolean(value);
            }
            if (destinationType == XmlBaseConverter.ByteType)
            {
                return XmlBaseConverter.Int32ToByte(XmlConvert.ToInt32(value));
            }
            if (destinationType == XmlBaseConverter.ByteArrayType)
            {
                return XmlBaseConverter.StringToBase64Binary(value);
            }
            if (destinationType == XmlBaseConverter.DateTimeType)
            {
                return XmlBaseConverter.UntypedAtomicToDateTime(value);
            }
            if (destinationType == XmlBaseConverter.DateTimeOffsetType)
            {
                return XmlBaseConverter.UntypedAtomicToDateTimeOffset(value);
            }
            if (destinationType == XmlBaseConverter.DecimalType)
            {
                return XmlConvert.ToDecimal(value);
            }
            if (destinationType == XmlBaseConverter.DoubleType)
            {
                return XmlConvert.ToDouble(value);
            }
            if (destinationType == XmlBaseConverter.Int16Type)
            {
                return XmlBaseConverter.Int32ToInt16(XmlConvert.ToInt32(value));
            }
            if (destinationType == XmlBaseConverter.Int32Type)
            {
                return XmlConvert.ToInt32(value);
            }
            if (destinationType == XmlBaseConverter.Int64Type)
            {
                return XmlConvert.ToInt64(value);
            }
            if (destinationType == XmlBaseConverter.SByteType)
            {
                return XmlBaseConverter.Int32ToSByte(XmlConvert.ToInt32(value));
            }
            if (destinationType == XmlBaseConverter.SingleType)
            {
                return XmlConvert.ToSingle(value);
            }
            if (destinationType == XmlBaseConverter.TimeSpanType)
            {
                return XmlBaseConverter.StringToDuration(value);
            }
            if (destinationType == XmlBaseConverter.UInt16Type)
            {
                return XmlBaseConverter.Int32ToUInt16(XmlConvert.ToInt32(value));
            }
            if (destinationType == XmlBaseConverter.UInt32Type)
            {
                return XmlBaseConverter.Int64ToUInt32(XmlConvert.ToInt64(value));
            }
            if (destinationType == XmlBaseConverter.UInt64Type)
            {
                return XmlBaseConverter.DecimalToUInt64(XmlConvert.ToDecimal(value));
            }
            if (destinationType == XmlBaseConverter.UriType)
            {
                return XmlConvert.ToUri(value);
            }
            if (destinationType == XmlBaseConverter.XmlAtomicValueType)
            {
                return new XmlAtomicValue(base.SchemaType, value);
            }
            if (destinationType == XmlBaseConverter.XmlQualifiedNameType)
            {
                return XmlBaseConverter.StringToQName(value, nsResolver);
            }
            if (destinationType == XmlBaseConverter.XPathItemType)
            {
                return new XmlAtomicValue(base.SchemaType, value);
            }
            if (destinationType == XmlBaseConverter.StringType)
            {
                return value;
            }
            return this.ChangeTypeWildcardSource(value, destinationType, nsResolver);
        }

        private object ChangeTypeWildcardDestination(object value, Type destinationType, IXmlNamespaceResolver nsResolver)
        {
            if (value.GetType() == XmlBaseConverter.XmlAtomicValueType)
            {
                return ((XmlAtomicValue) value).ValueAs(destinationType, nsResolver);
            }
            return this.ChangeListType(value, destinationType, nsResolver);
        }

        private object ChangeTypeWildcardSource(object value, Type destinationType, IXmlNamespaceResolver nsResolver)
        {
            if (destinationType == XmlBaseConverter.XmlAtomicValueType)
            {
                return new XmlAtomicValue(base.SchemaType, this.ToString(value, nsResolver));
            }
            if (destinationType == XmlBaseConverter.XPathItemType)
            {
                return new XmlAtomicValue(base.SchemaType, this.ToString(value, nsResolver));
            }
            return this.ChangeListType(value, destinationType, nsResolver);
        }

        private bool SupportsType(Type clrType)
        {
            return ((clrType == XmlBaseConverter.BooleanType) || ((clrType == XmlBaseConverter.ByteType) || ((clrType == XmlBaseConverter.ByteArrayType) || ((clrType == XmlBaseConverter.DateTimeType) || ((clrType == XmlBaseConverter.DateTimeOffsetType) || ((clrType == XmlBaseConverter.DecimalType) || ((clrType == XmlBaseConverter.DoubleType) || ((clrType == XmlBaseConverter.Int16Type) || ((clrType == XmlBaseConverter.Int32Type) || ((clrType == XmlBaseConverter.Int64Type) || ((clrType == XmlBaseConverter.SByteType) || ((clrType == XmlBaseConverter.SingleType) || ((clrType == XmlBaseConverter.TimeSpanType) || ((clrType == XmlBaseConverter.UInt16Type) || ((clrType == XmlBaseConverter.UInt32Type) || ((clrType == XmlBaseConverter.UInt64Type) || ((clrType == XmlBaseConverter.UriType) || (clrType == XmlBaseConverter.XmlQualifiedNameType))))))))))))))))));
        }

        public override bool ToBoolean(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (value.GetType() == XmlBaseConverter.StringType)
            {
                return XmlConvert.ToBoolean((string) value);
            }
            return (bool) this.ChangeTypeWildcardDestination(value, XmlBaseConverter.BooleanType, null);
        }

        public override bool ToBoolean(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            return XmlConvert.ToBoolean(value);
        }

        public override DateTime ToDateTime(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (value.GetType() == XmlBaseConverter.StringType)
            {
                return XmlBaseConverter.UntypedAtomicToDateTime((string) value);
            }
            return (DateTime) this.ChangeTypeWildcardDestination(value, XmlBaseConverter.DateTimeType, null);
        }

        public override DateTime ToDateTime(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            return XmlBaseConverter.UntypedAtomicToDateTime(value);
        }

        public override DateTimeOffset ToDateTimeOffset(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (value.GetType() == XmlBaseConverter.StringType)
            {
                return XmlBaseConverter.UntypedAtomicToDateTimeOffset((string) value);
            }
            return (DateTimeOffset) this.ChangeTypeWildcardDestination(value, XmlBaseConverter.DateTimeOffsetType, null);
        }

        public override DateTimeOffset ToDateTimeOffset(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            return XmlBaseConverter.UntypedAtomicToDateTimeOffset(value);
        }

        public override decimal ToDecimal(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (value.GetType() == XmlBaseConverter.StringType)
            {
                return XmlConvert.ToDecimal((string) value);
            }
            return (decimal) this.ChangeTypeWildcardDestination(value, XmlBaseConverter.DecimalType, null);
        }

        public override decimal ToDecimal(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            return XmlConvert.ToDecimal(value);
        }

        public override double ToDouble(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (value.GetType() == XmlBaseConverter.StringType)
            {
                return XmlConvert.ToDouble((string) value);
            }
            return (double) this.ChangeTypeWildcardDestination(value, XmlBaseConverter.DoubleType, null);
        }

        public override double ToDouble(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            return XmlConvert.ToDouble(value);
        }

        public override int ToInt32(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (value.GetType() == XmlBaseConverter.StringType)
            {
                return XmlConvert.ToInt32((string) value);
            }
            return (int) this.ChangeTypeWildcardDestination(value, XmlBaseConverter.Int32Type, null);
        }

        public override int ToInt32(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            return XmlConvert.ToInt32(value);
        }

        public override long ToInt64(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (value.GetType() == XmlBaseConverter.StringType)
            {
                return XmlConvert.ToInt64((string) value);
            }
            return (long) this.ChangeTypeWildcardDestination(value, XmlBaseConverter.Int64Type, null);
        }

        public override long ToInt64(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            return XmlConvert.ToInt64(value);
        }

        public override float ToSingle(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (value.GetType() == XmlBaseConverter.StringType)
            {
                return XmlConvert.ToSingle((string) value);
            }
            return (float) this.ChangeTypeWildcardDestination(value, XmlBaseConverter.SingleType, null);
        }

        public override float ToSingle(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            return XmlConvert.ToSingle(value);
        }

        public override string ToString(bool value)
        {
            return XmlConvert.ToString(value);
        }

        public override string ToString(DateTime value)
        {
            return XmlBaseConverter.DateTimeToString(value);
        }

        public override string ToString(DateTimeOffset value)
        {
            return XmlBaseConverter.DateTimeOffsetToString(value);
        }

        public override string ToString(decimal value)
        {
            return XmlConvert.ToString(value);
        }

        public override string ToString(double value)
        {
            return XmlConvert.ToString(value);
        }

        public override string ToString(int value)
        {
            return XmlConvert.ToString(value);
        }

        public override string ToString(long value)
        {
            return XmlConvert.ToString(value);
        }

        public override string ToString(float value)
        {
            return XmlConvert.ToString(value);
        }

        public override string ToString(object value, IXmlNamespaceResolver nsResolver)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            Type derivedType = value.GetType();
            if (derivedType == XmlBaseConverter.BooleanType)
            {
                return XmlConvert.ToString((bool) value);
            }
            if (derivedType == XmlBaseConverter.ByteType)
            {
                return XmlConvert.ToString((byte) value);
            }
            if (derivedType == XmlBaseConverter.ByteArrayType)
            {
                return XmlBaseConverter.Base64BinaryToString((byte[]) value);
            }
            if (derivedType == XmlBaseConverter.DateTimeType)
            {
                return XmlBaseConverter.DateTimeToString((DateTime) value);
            }
            if (derivedType == XmlBaseConverter.DateTimeOffsetType)
            {
                return XmlBaseConverter.DateTimeOffsetToString((DateTimeOffset) value);
            }
            if (derivedType == XmlBaseConverter.DecimalType)
            {
                return XmlConvert.ToString((decimal) value);
            }
            if (derivedType == XmlBaseConverter.DoubleType)
            {
                return XmlConvert.ToString((double) value);
            }
            if (derivedType == XmlBaseConverter.Int16Type)
            {
                return XmlConvert.ToString((short) value);
            }
            if (derivedType == XmlBaseConverter.Int32Type)
            {
                return XmlConvert.ToString((int) value);
            }
            if (derivedType == XmlBaseConverter.Int64Type)
            {
                return XmlConvert.ToString((long) value);
            }
            if (derivedType == XmlBaseConverter.SByteType)
            {
                return XmlConvert.ToString((sbyte) value);
            }
            if (derivedType == XmlBaseConverter.SingleType)
            {
                return XmlConvert.ToString((float) value);
            }
            if (derivedType == XmlBaseConverter.StringType)
            {
                return (string) value;
            }
            if (derivedType == XmlBaseConverter.TimeSpanType)
            {
                return XmlBaseConverter.DurationToString((TimeSpan) value);
            }
            if (derivedType == XmlBaseConverter.UInt16Type)
            {
                return XmlConvert.ToString((ushort) value);
            }
            if (derivedType == XmlBaseConverter.UInt32Type)
            {
                return XmlConvert.ToString((uint) value);
            }
            if (derivedType == XmlBaseConverter.UInt64Type)
            {
                return XmlConvert.ToString((ulong) value);
            }
            if (XmlBaseConverter.IsDerivedFrom(derivedType, XmlBaseConverter.UriType))
            {
                return XmlBaseConverter.AnyUriToString((Uri) value);
            }
            if (derivedType == XmlBaseConverter.XmlAtomicValueType)
            {
                return (string) ((XmlAtomicValue) value).ValueAs(XmlBaseConverter.StringType, nsResolver);
            }
            if (XmlBaseConverter.IsDerivedFrom(derivedType, XmlBaseConverter.XmlQualifiedNameType))
            {
                return XmlBaseConverter.QNameToString((XmlQualifiedName) value, nsResolver);
            }
            return (string) this.ChangeTypeWildcardDestination(value, XmlBaseConverter.StringType, nsResolver);
        }

        public override string ToString(string value, IXmlNamespaceResolver nsResolver)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            return value;
        }
    }
}

