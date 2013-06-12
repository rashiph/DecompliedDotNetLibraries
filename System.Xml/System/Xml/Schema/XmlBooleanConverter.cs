namespace System.Xml.Schema
{
    using System;
    using System.Xml;

    internal class XmlBooleanConverter : XmlBaseConverter
    {
        protected XmlBooleanConverter(XmlSchemaType schemaType) : base(schemaType)
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
            if (destinationType == XmlBaseConverter.BooleanType)
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
            if (destinationType == XmlBaseConverter.BooleanType)
            {
                return this.ToBoolean(value);
            }
            if (destinationType == XmlBaseConverter.StringType)
            {
                return this.ToString(value, nsResolver);
            }
            if (destinationType == XmlBaseConverter.XmlAtomicValueType)
            {
                if (type == XmlBaseConverter.BooleanType)
                {
                    return new XmlAtomicValue(base.SchemaType, (bool) value);
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
                if (type == XmlBaseConverter.BooleanType)
                {
                    return new XmlAtomicValue(base.SchemaType, (bool) value);
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
            if (destinationType == XmlBaseConverter.BooleanType)
            {
                return XmlConvert.ToBoolean(value);
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
            return new XmlBooleanConverter(schemaType);
        }

        public override bool ToBoolean(bool value)
        {
            return value;
        }

        public override bool ToBoolean(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            Type type = value.GetType();
            if (type == XmlBaseConverter.BooleanType)
            {
                return (bool) value;
            }
            if (type == XmlBaseConverter.StringType)
            {
                return XmlConvert.ToBoolean((string) value);
            }
            if (type == XmlBaseConverter.XmlAtomicValueType)
            {
                return ((XmlAtomicValue) value).ValueAsBoolean;
            }
            return (bool) this.ChangeListType(value, XmlBaseConverter.BooleanType, null);
        }

        public override bool ToBoolean(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            return XmlConvert.ToBoolean(value);
        }

        public override string ToString(bool value)
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
            if (type == XmlBaseConverter.BooleanType)
            {
                return XmlConvert.ToString((bool) value);
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

