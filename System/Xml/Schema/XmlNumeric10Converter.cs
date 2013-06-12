namespace System.Xml.Schema
{
    using System;
    using System.Xml;

    internal class XmlNumeric10Converter : XmlBaseConverter
    {
        protected XmlNumeric10Converter(XmlSchemaType schemaType) : base(schemaType)
        {
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
            if (destinationType == XmlBaseConverter.DecimalType)
            {
                return value;
            }
            if (destinationType == XmlBaseConverter.Int32Type)
            {
                return XmlBaseConverter.DecimalToInt32(value);
            }
            if (destinationType == XmlBaseConverter.Int64Type)
            {
                return XmlBaseConverter.DecimalToInt64(value);
            }
            if (destinationType == XmlBaseConverter.StringType)
            {
                return this.ToString(value);
            }
            if (destinationType == XmlBaseConverter.XmlAtomicValueType)
            {
                return new XmlAtomicValue(base.SchemaType, value);
            }
            if (destinationType == XmlBaseConverter.XPathItemType)
            {
                return new XmlAtomicValue(base.SchemaType, value);
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
            if (destinationType == XmlBaseConverter.DecimalType)
            {
                return (decimal) value;
            }
            if (destinationType == XmlBaseConverter.Int32Type)
            {
                return value;
            }
            if (destinationType == XmlBaseConverter.Int64Type)
            {
                return (long) value;
            }
            if (destinationType == XmlBaseConverter.StringType)
            {
                return XmlConvert.ToString(value);
            }
            if (destinationType == XmlBaseConverter.XmlAtomicValueType)
            {
                return new XmlAtomicValue(base.SchemaType, value);
            }
            if (destinationType == XmlBaseConverter.XPathItemType)
            {
                return new XmlAtomicValue(base.SchemaType, value);
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
            if (destinationType == XmlBaseConverter.DecimalType)
            {
                return (decimal) value;
            }
            if (destinationType == XmlBaseConverter.Int32Type)
            {
                return XmlBaseConverter.Int64ToInt32(value);
            }
            if (destinationType == XmlBaseConverter.Int64Type)
            {
                return value;
            }
            if (destinationType == XmlBaseConverter.StringType)
            {
                return XmlConvert.ToString(value);
            }
            if (destinationType == XmlBaseConverter.XmlAtomicValueType)
            {
                return new XmlAtomicValue(base.SchemaType, value);
            }
            if (destinationType == XmlBaseConverter.XPathItemType)
            {
                return new XmlAtomicValue(base.SchemaType, value);
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
            if (destinationType == XmlBaseConverter.DecimalType)
            {
                return this.ToDecimal(value);
            }
            if (destinationType == XmlBaseConverter.Int32Type)
            {
                return this.ToInt32(value);
            }
            if (destinationType == XmlBaseConverter.Int64Type)
            {
                return this.ToInt64(value);
            }
            if (destinationType == XmlBaseConverter.StringType)
            {
                return this.ToString(value, nsResolver);
            }
            if (destinationType == XmlBaseConverter.XmlAtomicValueType)
            {
                if (type == XmlBaseConverter.DecimalType)
                {
                    return new XmlAtomicValue(base.SchemaType, value);
                }
                if (type == XmlBaseConverter.Int32Type)
                {
                    return new XmlAtomicValue(base.SchemaType, (int) value);
                }
                if (type == XmlBaseConverter.Int64Type)
                {
                    return new XmlAtomicValue(base.SchemaType, (long) value);
                }
                if (type == XmlBaseConverter.StringType)
                {
                    return new XmlAtomicValue(base.SchemaType, (string) value);
                }
                if (type == XmlBaseConverter.XmlAtomicValueType)
                {
                    return (XmlAtomicValue) value;
                }
            }
            if (destinationType == XmlBaseConverter.XPathItemType)
            {
                if (type == XmlBaseConverter.DecimalType)
                {
                    return new XmlAtomicValue(base.SchemaType, value);
                }
                if (type == XmlBaseConverter.Int32Type)
                {
                    return new XmlAtomicValue(base.SchemaType, (int) value);
                }
                if (type == XmlBaseConverter.Int64Type)
                {
                    return new XmlAtomicValue(base.SchemaType, (long) value);
                }
                if (type == XmlBaseConverter.StringType)
                {
                    return new XmlAtomicValue(base.SchemaType, (string) value);
                }
                if (type == XmlBaseConverter.XmlAtomicValueType)
                {
                    return (XmlAtomicValue) value;
                }
            }
            if (destinationType == XmlBaseConverter.ByteType)
            {
                return XmlBaseConverter.Int32ToByte(this.ToInt32(value));
            }
            if (destinationType == XmlBaseConverter.Int16Type)
            {
                return XmlBaseConverter.Int32ToInt16(this.ToInt32(value));
            }
            if (destinationType == XmlBaseConverter.SByteType)
            {
                return XmlBaseConverter.Int32ToSByte(this.ToInt32(value));
            }
            if (destinationType == XmlBaseConverter.UInt16Type)
            {
                return XmlBaseConverter.Int32ToUInt16(this.ToInt32(value));
            }
            if (destinationType == XmlBaseConverter.UInt32Type)
            {
                return XmlBaseConverter.Int64ToUInt32(this.ToInt64(value));
            }
            if (destinationType == XmlBaseConverter.UInt64Type)
            {
                return XmlBaseConverter.DecimalToUInt64(this.ToDecimal(value));
            }
            if (type == XmlBaseConverter.ByteType)
            {
                return this.ChangeType((int) ((byte) value), destinationType);
            }
            if (type == XmlBaseConverter.Int16Type)
            {
                return this.ChangeType((int) ((short) value), destinationType);
            }
            if (type == XmlBaseConverter.SByteType)
            {
                return this.ChangeType((int) ((sbyte) value), destinationType);
            }
            if (type == XmlBaseConverter.UInt16Type)
            {
                return this.ChangeType((int) ((ushort) value), destinationType);
            }
            if (type == XmlBaseConverter.UInt32Type)
            {
                return this.ChangeType((long) ((uint) value), destinationType);
            }
            if (type == XmlBaseConverter.UInt64Type)
            {
                return this.ChangeType((decimal) ((ulong) value), destinationType);
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
            if (destinationType == XmlBaseConverter.DecimalType)
            {
                return this.ToDecimal(value);
            }
            if (destinationType == XmlBaseConverter.Int32Type)
            {
                return this.ToInt32(value);
            }
            if (destinationType == XmlBaseConverter.Int64Type)
            {
                return this.ToInt64(value);
            }
            if (destinationType == XmlBaseConverter.StringType)
            {
                return value;
            }
            if (destinationType == XmlBaseConverter.XmlAtomicValueType)
            {
                return new XmlAtomicValue(base.SchemaType, value);
            }
            if (destinationType == XmlBaseConverter.XPathItemType)
            {
                return new XmlAtomicValue(base.SchemaType, value);
            }
            return this.ChangeTypeWildcardSource(value, destinationType, nsResolver);
        }

        private object ChangeTypeWildcardDestination(object value, Type destinationType, IXmlNamespaceResolver nsResolver)
        {
            Type type = value.GetType();
            if (type == XmlBaseConverter.ByteType)
            {
                return this.ChangeType((int) ((byte) value), destinationType);
            }
            if (type == XmlBaseConverter.Int16Type)
            {
                return this.ChangeType((int) ((short) value), destinationType);
            }
            if (type == XmlBaseConverter.SByteType)
            {
                return this.ChangeType((int) ((sbyte) value), destinationType);
            }
            if (type == XmlBaseConverter.UInt16Type)
            {
                return this.ChangeType((int) ((ushort) value), destinationType);
            }
            if (type == XmlBaseConverter.UInt32Type)
            {
                return this.ChangeType((long) ((uint) value), destinationType);
            }
            if (type == XmlBaseConverter.UInt64Type)
            {
                return this.ChangeType((decimal) ((ulong) value), destinationType);
            }
            return this.ChangeListType(value, destinationType, nsResolver);
        }

        private object ChangeTypeWildcardSource(object value, Type destinationType, IXmlNamespaceResolver nsResolver)
        {
            if (destinationType == XmlBaseConverter.ByteType)
            {
                return XmlBaseConverter.Int32ToByte(this.ToInt32(value));
            }
            if (destinationType == XmlBaseConverter.Int16Type)
            {
                return XmlBaseConverter.Int32ToInt16(this.ToInt32(value));
            }
            if (destinationType == XmlBaseConverter.SByteType)
            {
                return XmlBaseConverter.Int32ToSByte(this.ToInt32(value));
            }
            if (destinationType == XmlBaseConverter.UInt16Type)
            {
                return XmlBaseConverter.Int32ToUInt16(this.ToInt32(value));
            }
            if (destinationType == XmlBaseConverter.UInt32Type)
            {
                return XmlBaseConverter.Int64ToUInt32(this.ToInt64(value));
            }
            if (destinationType == XmlBaseConverter.UInt64Type)
            {
                return XmlBaseConverter.DecimalToUInt64(this.ToDecimal(value));
            }
            return this.ChangeListType(value, destinationType, nsResolver);
        }

        public static XmlValueConverter Create(XmlSchemaType schemaType)
        {
            return new XmlNumeric10Converter(schemaType);
        }

        public override decimal ToDecimal(decimal value)
        {
            return value;
        }

        public override decimal ToDecimal(int value)
        {
            return value;
        }

        public override decimal ToDecimal(long value)
        {
            return value;
        }

        public override decimal ToDecimal(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            Type type = value.GetType();
            if (type == XmlBaseConverter.DecimalType)
            {
                return (decimal) value;
            }
            if (type == XmlBaseConverter.Int32Type)
            {
                return (int) value;
            }
            if (type == XmlBaseConverter.Int64Type)
            {
                return (long) value;
            }
            if (type == XmlBaseConverter.StringType)
            {
                return this.ToDecimal((string) value);
            }
            if (type == XmlBaseConverter.XmlAtomicValueType)
            {
                return (decimal) ((XmlAtomicValue) value).ValueAs(XmlBaseConverter.DecimalType);
            }
            return (decimal) this.ChangeTypeWildcardDestination(value, XmlBaseConverter.DecimalType, null);
        }

        public override decimal ToDecimal(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (base.TypeCode == XmlTypeCode.Decimal)
            {
                return XmlConvert.ToDecimal(value);
            }
            return XmlConvert.ToInteger(value);
        }

        public override int ToInt32(decimal value)
        {
            return XmlBaseConverter.DecimalToInt32(value);
        }

        public override int ToInt32(int value)
        {
            return value;
        }

        public override int ToInt32(long value)
        {
            return XmlBaseConverter.Int64ToInt32(value);
        }

        public override int ToInt32(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            Type type = value.GetType();
            if (type == XmlBaseConverter.DecimalType)
            {
                return XmlBaseConverter.DecimalToInt32((decimal) value);
            }
            if (type == XmlBaseConverter.Int32Type)
            {
                return (int) value;
            }
            if (type == XmlBaseConverter.Int64Type)
            {
                return XmlBaseConverter.Int64ToInt32((long) value);
            }
            if (type == XmlBaseConverter.StringType)
            {
                return this.ToInt32((string) value);
            }
            if (type == XmlBaseConverter.XmlAtomicValueType)
            {
                return ((XmlAtomicValue) value).ValueAsInt;
            }
            return (int) this.ChangeTypeWildcardDestination(value, XmlBaseConverter.Int32Type, null);
        }

        public override int ToInt32(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (base.TypeCode == XmlTypeCode.Decimal)
            {
                return XmlBaseConverter.DecimalToInt32(XmlConvert.ToDecimal(value));
            }
            return XmlConvert.ToInt32(value);
        }

        public override long ToInt64(decimal value)
        {
            return XmlBaseConverter.DecimalToInt64(value);
        }

        public override long ToInt64(int value)
        {
            return (long) value;
        }

        public override long ToInt64(long value)
        {
            return value;
        }

        public override long ToInt64(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            Type type = value.GetType();
            if (type == XmlBaseConverter.DecimalType)
            {
                return XmlBaseConverter.DecimalToInt64((decimal) value);
            }
            if (type == XmlBaseConverter.Int32Type)
            {
                return (long) ((int) value);
            }
            if (type == XmlBaseConverter.Int64Type)
            {
                return (long) value;
            }
            if (type == XmlBaseConverter.StringType)
            {
                return this.ToInt64((string) value);
            }
            if (type == XmlBaseConverter.XmlAtomicValueType)
            {
                return ((XmlAtomicValue) value).ValueAsLong;
            }
            return (long) this.ChangeTypeWildcardDestination(value, XmlBaseConverter.Int64Type, null);
        }

        public override long ToInt64(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (base.TypeCode == XmlTypeCode.Decimal)
            {
                return XmlBaseConverter.DecimalToInt64(XmlConvert.ToDecimal(value));
            }
            return XmlConvert.ToInt64(value);
        }

        public override string ToString(decimal value)
        {
            if (base.TypeCode == XmlTypeCode.Decimal)
            {
                return XmlConvert.ToString(value);
            }
            return XmlConvert.ToString(decimal.Truncate(value));
        }

        public override string ToString(int value)
        {
            return XmlConvert.ToString(value);
        }

        public override string ToString(long value)
        {
            return XmlConvert.ToString(value);
        }

        public override string ToString(object value, IXmlNamespaceResolver nsResolver)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            Type type = value.GetType();
            if (type == XmlBaseConverter.DecimalType)
            {
                return this.ToString((decimal) value);
            }
            if (type == XmlBaseConverter.Int32Type)
            {
                return XmlConvert.ToString((int) value);
            }
            if (type == XmlBaseConverter.Int64Type)
            {
                return XmlConvert.ToString((long) value);
            }
            if (type == XmlBaseConverter.StringType)
            {
                return (string) value;
            }
            if (type == XmlBaseConverter.XmlAtomicValueType)
            {
                return ((XmlAtomicValue) value).Value;
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

