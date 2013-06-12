namespace System.Xml.Schema
{
    using System;
    using System.Xml;

    internal class XmlNumeric2Converter : XmlBaseConverter
    {
        protected XmlNumeric2Converter(XmlSchemaType schemaType) : base(schemaType)
        {
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
            if (destinationType == XmlBaseConverter.DoubleType)
            {
                return value;
            }
            if (destinationType == XmlBaseConverter.SingleType)
            {
                return (float) value;
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
            return this.ChangeListType(value, destinationType, null);
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
            if (destinationType == XmlBaseConverter.DoubleType)
            {
                return (double) value;
            }
            if (destinationType == XmlBaseConverter.SingleType)
            {
                return value;
            }
            if (destinationType == XmlBaseConverter.StringType)
            {
                return this.ToString(value);
            }
            if (destinationType == XmlBaseConverter.XmlAtomicValueType)
            {
                return new XmlAtomicValue(base.SchemaType, (double) value);
            }
            if (destinationType == XmlBaseConverter.XPathItemType)
            {
                return new XmlAtomicValue(base.SchemaType, (double) value);
            }
            return this.ChangeListType(value, destinationType, null);
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
            if (destinationType == XmlBaseConverter.DoubleType)
            {
                return this.ToDouble(value);
            }
            if (destinationType == XmlBaseConverter.SingleType)
            {
                return this.ToSingle(value);
            }
            if (destinationType == XmlBaseConverter.StringType)
            {
                return this.ToString(value, nsResolver);
            }
            if (destinationType == XmlBaseConverter.XmlAtomicValueType)
            {
                if (type == XmlBaseConverter.DoubleType)
                {
                    return new XmlAtomicValue(base.SchemaType, (double) value);
                }
                if (type == XmlBaseConverter.SingleType)
                {
                    return new XmlAtomicValue(base.SchemaType, value);
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
                if (type == XmlBaseConverter.DoubleType)
                {
                    return new XmlAtomicValue(base.SchemaType, (double) value);
                }
                if (type == XmlBaseConverter.SingleType)
                {
                    return new XmlAtomicValue(base.SchemaType, value);
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
            if (destinationType == XmlBaseConverter.DoubleType)
            {
                return this.ToDouble(value);
            }
            if (destinationType == XmlBaseConverter.SingleType)
            {
                return this.ToSingle(value);
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
            return this.ChangeListType(value, destinationType, nsResolver);
        }

        public static XmlValueConverter Create(XmlSchemaType schemaType)
        {
            return new XmlNumeric2Converter(schemaType);
        }

        public override double ToDouble(double value)
        {
            return value;
        }

        public override double ToDouble(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            Type type = value.GetType();
            if (type == XmlBaseConverter.DoubleType)
            {
                return (double) value;
            }
            if (type == XmlBaseConverter.SingleType)
            {
                return (double) ((float) value);
            }
            if (type == XmlBaseConverter.StringType)
            {
                return this.ToDouble((string) value);
            }
            if (type == XmlBaseConverter.XmlAtomicValueType)
            {
                return ((XmlAtomicValue) value).ValueAsDouble;
            }
            return (double) this.ChangeListType(value, XmlBaseConverter.DoubleType, null);
        }

        public override double ToDouble(float value)
        {
            return (double) value;
        }

        public override double ToDouble(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (base.TypeCode == XmlTypeCode.Float)
            {
                return (double) XmlConvert.ToSingle(value);
            }
            return XmlConvert.ToDouble(value);
        }

        public override float ToSingle(double value)
        {
            return (float) value;
        }

        public override float ToSingle(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            Type type = value.GetType();
            if (type == XmlBaseConverter.DoubleType)
            {
                return (float) ((double) value);
            }
            if (type == XmlBaseConverter.SingleType)
            {
                return (float) value;
            }
            if (type == XmlBaseConverter.StringType)
            {
                return this.ToSingle((string) value);
            }
            if (type == XmlBaseConverter.XmlAtomicValueType)
            {
                return (float) ((XmlAtomicValue) value).ValueAs(XmlBaseConverter.SingleType);
            }
            return (float) this.ChangeListType(value, XmlBaseConverter.SingleType, null);
        }

        public override float ToSingle(float value)
        {
            return value;
        }

        public override float ToSingle(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (base.TypeCode == XmlTypeCode.Float)
            {
                return XmlConvert.ToSingle(value);
            }
            return (float) XmlConvert.ToDouble(value);
        }

        public override string ToString(double value)
        {
            if (base.TypeCode == XmlTypeCode.Float)
            {
                return XmlConvert.ToString(this.ToSingle(value));
            }
            return XmlConvert.ToString(value);
        }

        public override string ToString(float value)
        {
            if (base.TypeCode == XmlTypeCode.Float)
            {
                return XmlConvert.ToString(value);
            }
            return XmlConvert.ToString((double) value);
        }

        public override string ToString(object value, IXmlNamespaceResolver nsResolver)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            Type type = value.GetType();
            if (type == XmlBaseConverter.DoubleType)
            {
                return this.ToString((double) value);
            }
            if (type == XmlBaseConverter.SingleType)
            {
                return this.ToString((float) value);
            }
            if (type == XmlBaseConverter.StringType)
            {
                return (string) value;
            }
            if (type == XmlBaseConverter.XmlAtomicValueType)
            {
                return ((XmlAtomicValue) value).Value;
            }
            return (string) this.ChangeListType(value, XmlBaseConverter.StringType, nsResolver);
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

