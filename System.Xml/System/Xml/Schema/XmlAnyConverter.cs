namespace System.Xml.Schema
{
    using System;
    using System.Xml;
    using System.Xml.XPath;

    internal class XmlAnyConverter : XmlBaseConverter
    {
        public static readonly XmlValueConverter AnyAtomic = new XmlAnyConverter(XmlTypeCode.AnyAtomicType);
        public static readonly XmlValueConverter Item = new XmlAnyConverter(XmlTypeCode.Item);

        protected XmlAnyConverter(XmlTypeCode typeCode) : base(typeCode)
        {
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
            if (destinationType == XmlBaseConverter.XmlAtomicValueType)
            {
                return new XmlAtomicValue(XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.Boolean), value);
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
            if (destinationType == XmlBaseConverter.XmlAtomicValueType)
            {
                return new XmlAtomicValue(XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.DateTime), value);
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
            if (destinationType == XmlBaseConverter.XmlAtomicValueType)
            {
                return new XmlAtomicValue(XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.DateTime), value);
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
            if (destinationType == XmlBaseConverter.XmlAtomicValueType)
            {
                return new XmlAtomicValue(XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.Decimal), value);
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
            if (destinationType == XmlBaseConverter.XmlAtomicValueType)
            {
                return new XmlAtomicValue(XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.Double), value);
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
            if (destinationType == XmlBaseConverter.XmlAtomicValueType)
            {
                return new XmlAtomicValue(XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.Int), value);
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
            if (destinationType == XmlBaseConverter.XmlAtomicValueType)
            {
                return new XmlAtomicValue(XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.Long), value);
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
            if (destinationType == XmlBaseConverter.XmlAtomicValueType)
            {
                return new XmlAtomicValue(XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.Float), (double) value);
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
            Type derivedType = value.GetType();
            if (destinationType == XmlBaseConverter.ObjectType)
            {
                destinationType = base.DefaultClrType;
            }
            if ((destinationType == XmlBaseConverter.BooleanType) && (derivedType == XmlBaseConverter.XmlAtomicValueType))
            {
                return ((XmlAtomicValue) value).ValueAsBoolean;
            }
            if ((destinationType == XmlBaseConverter.DateTimeType) && (derivedType == XmlBaseConverter.XmlAtomicValueType))
            {
                return ((XmlAtomicValue) value).ValueAsDateTime;
            }
            if ((destinationType == XmlBaseConverter.DateTimeOffsetType) && (derivedType == XmlBaseConverter.XmlAtomicValueType))
            {
                return ((XmlAtomicValue) value).ValueAs(XmlBaseConverter.DateTimeOffsetType);
            }
            if ((destinationType == XmlBaseConverter.DecimalType) && (derivedType == XmlBaseConverter.XmlAtomicValueType))
            {
                return (decimal) ((XmlAtomicValue) value).ValueAs(XmlBaseConverter.DecimalType);
            }
            if ((destinationType == XmlBaseConverter.DoubleType) && (derivedType == XmlBaseConverter.XmlAtomicValueType))
            {
                return ((XmlAtomicValue) value).ValueAsDouble;
            }
            if ((destinationType == XmlBaseConverter.Int32Type) && (derivedType == XmlBaseConverter.XmlAtomicValueType))
            {
                return ((XmlAtomicValue) value).ValueAsInt;
            }
            if ((destinationType == XmlBaseConverter.Int64Type) && (derivedType == XmlBaseConverter.XmlAtomicValueType))
            {
                return ((XmlAtomicValue) value).ValueAsLong;
            }
            if ((destinationType == XmlBaseConverter.SingleType) && (derivedType == XmlBaseConverter.XmlAtomicValueType))
            {
                return (float) ((XmlAtomicValue) value).ValueAs(XmlBaseConverter.SingleType);
            }
            if (destinationType == XmlBaseConverter.XmlAtomicValueType)
            {
                if (derivedType == XmlBaseConverter.XmlAtomicValueType)
                {
                    return (XmlAtomicValue) value;
                }
                if (derivedType == XmlBaseConverter.BooleanType)
                {
                    return new XmlAtomicValue(XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.Boolean), (bool) value);
                }
                if (derivedType == XmlBaseConverter.ByteType)
                {
                    return new XmlAtomicValue(XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.UnsignedByte), value);
                }
                if (derivedType == XmlBaseConverter.ByteArrayType)
                {
                    return new XmlAtomicValue(XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.Base64Binary), value);
                }
                if (derivedType == XmlBaseConverter.DateTimeType)
                {
                    return new XmlAtomicValue(XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.DateTime), (DateTime) value);
                }
                if (derivedType == XmlBaseConverter.DateTimeOffsetType)
                {
                    return new XmlAtomicValue(XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.DateTime), (DateTimeOffset) value);
                }
                if (derivedType == XmlBaseConverter.DecimalType)
                {
                    return new XmlAtomicValue(XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.Decimal), value);
                }
                if (derivedType == XmlBaseConverter.DoubleType)
                {
                    return new XmlAtomicValue(XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.Double), (double) value);
                }
                if (derivedType == XmlBaseConverter.Int16Type)
                {
                    return new XmlAtomicValue(XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.Short), value);
                }
                if (derivedType == XmlBaseConverter.Int32Type)
                {
                    return new XmlAtomicValue(XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.Int), (int) value);
                }
                if (derivedType == XmlBaseConverter.Int64Type)
                {
                    return new XmlAtomicValue(XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.Long), (long) value);
                }
                if (derivedType == XmlBaseConverter.SByteType)
                {
                    return new XmlAtomicValue(XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.Byte), value);
                }
                if (derivedType == XmlBaseConverter.SingleType)
                {
                    return new XmlAtomicValue(XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.Float), value);
                }
                if (derivedType == XmlBaseConverter.StringType)
                {
                    return new XmlAtomicValue(XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.String), (string) value);
                }
                if (derivedType == XmlBaseConverter.TimeSpanType)
                {
                    return new XmlAtomicValue(XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.Duration), value);
                }
                if (derivedType == XmlBaseConverter.UInt16Type)
                {
                    return new XmlAtomicValue(XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.UnsignedShort), value);
                }
                if (derivedType == XmlBaseConverter.UInt32Type)
                {
                    return new XmlAtomicValue(XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.UnsignedInt), value);
                }
                if (derivedType == XmlBaseConverter.UInt64Type)
                {
                    return new XmlAtomicValue(XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.UnsignedLong), value);
                }
                if (XmlBaseConverter.IsDerivedFrom(derivedType, XmlBaseConverter.UriType))
                {
                    return new XmlAtomicValue(XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.AnyUri), value);
                }
                if (XmlBaseConverter.IsDerivedFrom(derivedType, XmlBaseConverter.XmlQualifiedNameType))
                {
                    return new XmlAtomicValue(XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.QName), value, nsResolver);
                }
            }
            if (destinationType == XmlBaseConverter.XPathItemType)
            {
                if (derivedType == XmlBaseConverter.XmlAtomicValueType)
                {
                    return (XmlAtomicValue) value;
                }
                if (XmlBaseConverter.IsDerivedFrom(derivedType, XmlBaseConverter.XPathNavigatorType))
                {
                    return (XPathNavigator) value;
                }
            }
            if ((destinationType == XmlBaseConverter.XPathNavigatorType) && XmlBaseConverter.IsDerivedFrom(derivedType, XmlBaseConverter.XPathNavigatorType))
            {
                return this.ToNavigator((XPathNavigator) value);
            }
            if (destinationType == XmlBaseConverter.XPathItemType)
            {
                return (XPathItem) this.ChangeType(value, XmlBaseConverter.XmlAtomicValueType, nsResolver);
            }
            if (derivedType == XmlBaseConverter.XmlAtomicValueType)
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
            if (destinationType == XmlBaseConverter.XmlAtomicValueType)
            {
                return new XmlAtomicValue(XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.String), value);
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
            if (destinationType == XmlBaseConverter.XPathItemType)
            {
                return (XPathItem) this.ChangeType(value, XmlBaseConverter.XmlAtomicValueType, nsResolver);
            }
            return this.ChangeListType(value, destinationType, nsResolver);
        }

        public override bool ToBoolean(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (value.GetType() == XmlBaseConverter.XmlAtomicValueType)
            {
                return ((XmlAtomicValue) value).ValueAsBoolean;
            }
            return (bool) this.ChangeTypeWildcardDestination(value, XmlBaseConverter.BooleanType, null);
        }

        public override DateTime ToDateTime(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (value.GetType() == XmlBaseConverter.XmlAtomicValueType)
            {
                return ((XmlAtomicValue) value).ValueAsDateTime;
            }
            return (DateTime) this.ChangeTypeWildcardDestination(value, XmlBaseConverter.DateTimeType, null);
        }

        public override DateTimeOffset ToDateTimeOffset(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (value.GetType() == XmlBaseConverter.XmlAtomicValueType)
            {
                return (DateTimeOffset) ((XmlAtomicValue) value).ValueAs(XmlBaseConverter.DateTimeOffsetType);
            }
            return (DateTimeOffset) this.ChangeTypeWildcardDestination(value, XmlBaseConverter.DateTimeOffsetType, null);
        }

        public override decimal ToDecimal(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (value.GetType() == XmlBaseConverter.XmlAtomicValueType)
            {
                return (decimal) ((XmlAtomicValue) value).ValueAs(XmlBaseConverter.DecimalType);
            }
            return (decimal) this.ChangeTypeWildcardDestination(value, XmlBaseConverter.DecimalType, null);
        }

        public override double ToDouble(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (value.GetType() == XmlBaseConverter.XmlAtomicValueType)
            {
                return ((XmlAtomicValue) value).ValueAsDouble;
            }
            return (double) this.ChangeTypeWildcardDestination(value, XmlBaseConverter.DoubleType, null);
        }

        public override int ToInt32(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (value.GetType() == XmlBaseConverter.XmlAtomicValueType)
            {
                return ((XmlAtomicValue) value).ValueAsInt;
            }
            return (int) this.ChangeTypeWildcardDestination(value, XmlBaseConverter.Int32Type, null);
        }

        public override long ToInt64(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (value.GetType() == XmlBaseConverter.XmlAtomicValueType)
            {
                return ((XmlAtomicValue) value).ValueAsLong;
            }
            return (long) this.ChangeTypeWildcardDestination(value, XmlBaseConverter.Int64Type, null);
        }

        private XPathNavigator ToNavigator(XPathNavigator nav)
        {
            if (base.TypeCode != XmlTypeCode.Item)
            {
                throw base.CreateInvalidClrMappingException(XmlBaseConverter.XPathNavigatorType, XmlBaseConverter.XPathNavigatorType);
            }
            return nav;
        }

        public override float ToSingle(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (value.GetType() == XmlBaseConverter.XmlAtomicValueType)
            {
                return (float) ((XmlAtomicValue) value).ValueAs(XmlBaseConverter.SingleType);
            }
            return (float) this.ChangeTypeWildcardDestination(value, XmlBaseConverter.SingleType, null);
        }
    }
}

