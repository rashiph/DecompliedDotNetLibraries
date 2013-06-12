namespace System.Xml.Schema
{
    using System;
    using System.Xml;
    using System.Xml.XPath;

    internal class XmlMiscConverter : XmlBaseConverter
    {
        protected XmlMiscConverter(XmlSchemaType schemaType) : base(schemaType)
        {
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
            if (destinationType == XmlBaseConverter.ByteArrayType)
            {
                if (derivedType == XmlBaseConverter.ByteArrayType)
                {
                    switch (base.TypeCode)
                    {
                        case XmlTypeCode.HexBinary:
                            return (byte[]) value;

                        case XmlTypeCode.Base64Binary:
                            return (byte[]) value;
                    }
                }
                if (derivedType == XmlBaseConverter.StringType)
                {
                    switch (base.TypeCode)
                    {
                        case XmlTypeCode.HexBinary:
                            return XmlBaseConverter.StringToHexBinary((string) value);

                        case XmlTypeCode.Base64Binary:
                            return XmlBaseConverter.StringToBase64Binary((string) value);
                    }
                }
            }
            if (destinationType == XmlBaseConverter.XmlQualifiedNameType)
            {
                if (derivedType == XmlBaseConverter.StringType)
                {
                    switch (base.TypeCode)
                    {
                        case XmlTypeCode.QName:
                            return XmlBaseConverter.StringToQName((string) value, nsResolver);

                        case XmlTypeCode.Notation:
                            return XmlBaseConverter.StringToQName((string) value, nsResolver);
                    }
                }
                if (XmlBaseConverter.IsDerivedFrom(derivedType, XmlBaseConverter.XmlQualifiedNameType))
                {
                    switch (base.TypeCode)
                    {
                        case XmlTypeCode.QName:
                            return (XmlQualifiedName) value;

                        case XmlTypeCode.Notation:
                            return (XmlQualifiedName) value;
                    }
                }
            }
            if (destinationType == XmlBaseConverter.StringType)
            {
                return this.ToString(value, nsResolver);
            }
            if (destinationType == XmlBaseConverter.TimeSpanType)
            {
                if (derivedType == XmlBaseConverter.StringType)
                {
                    switch (base.TypeCode)
                    {
                        case XmlTypeCode.YearMonthDuration:
                            return XmlBaseConverter.StringToYearMonthDuration((string) value);

                        case XmlTypeCode.DayTimeDuration:
                            return XmlBaseConverter.StringToDayTimeDuration((string) value);

                        case XmlTypeCode.Duration:
                            return XmlBaseConverter.StringToDuration((string) value);
                    }
                }
                if (derivedType == XmlBaseConverter.TimeSpanType)
                {
                    switch (base.TypeCode)
                    {
                        case XmlTypeCode.YearMonthDuration:
                            return (TimeSpan) value;

                        case XmlTypeCode.DayTimeDuration:
                            return (TimeSpan) value;

                        case XmlTypeCode.Duration:
                            return (TimeSpan) value;
                    }
                }
            }
            if (destinationType == XmlBaseConverter.UriType)
            {
                if ((derivedType == XmlBaseConverter.StringType) && (base.TypeCode == XmlTypeCode.AnyUri))
                {
                    return XmlConvert.ToUri((string) value);
                }
                if (XmlBaseConverter.IsDerivedFrom(derivedType, XmlBaseConverter.UriType) && (base.TypeCode == XmlTypeCode.AnyUri))
                {
                    return (Uri) value;
                }
            }
            if (destinationType == XmlBaseConverter.XmlAtomicValueType)
            {
                if (derivedType == XmlBaseConverter.ByteArrayType)
                {
                    switch (base.TypeCode)
                    {
                        case XmlTypeCode.HexBinary:
                            return new XmlAtomicValue(base.SchemaType, value);

                        case XmlTypeCode.Base64Binary:
                            return new XmlAtomicValue(base.SchemaType, value);
                    }
                }
                if (derivedType == XmlBaseConverter.StringType)
                {
                    return new XmlAtomicValue(base.SchemaType, (string) value, nsResolver);
                }
                if (derivedType == XmlBaseConverter.TimeSpanType)
                {
                    switch (base.TypeCode)
                    {
                        case XmlTypeCode.YearMonthDuration:
                            return new XmlAtomicValue(base.SchemaType, value);

                        case XmlTypeCode.DayTimeDuration:
                            return new XmlAtomicValue(base.SchemaType, value);

                        case XmlTypeCode.Duration:
                            return new XmlAtomicValue(base.SchemaType, value);
                    }
                }
                if (XmlBaseConverter.IsDerivedFrom(derivedType, XmlBaseConverter.UriType) && (base.TypeCode == XmlTypeCode.AnyUri))
                {
                    return new XmlAtomicValue(base.SchemaType, value);
                }
                if (derivedType == XmlBaseConverter.XmlAtomicValueType)
                {
                    return (XmlAtomicValue) value;
                }
                if (XmlBaseConverter.IsDerivedFrom(derivedType, XmlBaseConverter.XmlQualifiedNameType))
                {
                    switch (base.TypeCode)
                    {
                        case XmlTypeCode.QName:
                            return new XmlAtomicValue(base.SchemaType, value, nsResolver);

                        case XmlTypeCode.Notation:
                            return new XmlAtomicValue(base.SchemaType, value, nsResolver);
                    }
                }
            }
            if ((destinationType == XmlBaseConverter.XPathItemType) && (derivedType == XmlBaseConverter.XmlAtomicValueType))
            {
                return (XmlAtomicValue) value;
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
            if (destinationType == XmlBaseConverter.ByteArrayType)
            {
                switch (base.TypeCode)
                {
                    case XmlTypeCode.HexBinary:
                        return XmlBaseConverter.StringToHexBinary(value);

                    case XmlTypeCode.Base64Binary:
                        return XmlBaseConverter.StringToBase64Binary(value);
                }
            }
            if (destinationType == XmlBaseConverter.XmlQualifiedNameType)
            {
                switch (base.TypeCode)
                {
                    case XmlTypeCode.QName:
                        return XmlBaseConverter.StringToQName(value, nsResolver);

                    case XmlTypeCode.Notation:
                        return XmlBaseConverter.StringToQName(value, nsResolver);
                }
            }
            if (destinationType == XmlBaseConverter.StringType)
            {
                return value;
            }
            if (destinationType == XmlBaseConverter.TimeSpanType)
            {
                switch (base.TypeCode)
                {
                    case XmlTypeCode.YearMonthDuration:
                        return XmlBaseConverter.StringToYearMonthDuration(value);

                    case XmlTypeCode.DayTimeDuration:
                        return XmlBaseConverter.StringToDayTimeDuration(value);

                    case XmlTypeCode.Duration:
                        return XmlBaseConverter.StringToDuration(value);
                }
            }
            if ((destinationType == XmlBaseConverter.UriType) && (base.TypeCode == XmlTypeCode.AnyUri))
            {
                return XmlConvert.ToUri(value);
            }
            if (destinationType == XmlBaseConverter.XmlAtomicValueType)
            {
                return new XmlAtomicValue(base.SchemaType, value, nsResolver);
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

        public static XmlValueConverter Create(XmlSchemaType schemaType)
        {
            return new XmlMiscConverter(schemaType);
        }

        public override string ToString(object value, IXmlNamespaceResolver nsResolver)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            Type derivedType = value.GetType();
            if (derivedType == XmlBaseConverter.ByteArrayType)
            {
                switch (base.TypeCode)
                {
                    case XmlTypeCode.HexBinary:
                        return XmlConvert.ToBinHexString((byte[]) value);

                    case XmlTypeCode.Base64Binary:
                        return XmlBaseConverter.Base64BinaryToString((byte[]) value);
                }
            }
            if (derivedType == XmlBaseConverter.StringType)
            {
                return (string) value;
            }
            if (XmlBaseConverter.IsDerivedFrom(derivedType, XmlBaseConverter.UriType) && (base.TypeCode == XmlTypeCode.AnyUri))
            {
                return XmlBaseConverter.AnyUriToString((Uri) value);
            }
            if (derivedType == XmlBaseConverter.TimeSpanType)
            {
                switch (base.TypeCode)
                {
                    case XmlTypeCode.YearMonthDuration:
                        return XmlBaseConverter.YearMonthDurationToString((TimeSpan) value);

                    case XmlTypeCode.DayTimeDuration:
                        return XmlBaseConverter.DayTimeDurationToString((TimeSpan) value);

                    case XmlTypeCode.Duration:
                        return XmlBaseConverter.DurationToString((TimeSpan) value);
                }
            }
            if (XmlBaseConverter.IsDerivedFrom(derivedType, XmlBaseConverter.XmlQualifiedNameType))
            {
                switch (base.TypeCode)
                {
                    case XmlTypeCode.QName:
                        return XmlBaseConverter.QNameToString((XmlQualifiedName) value, nsResolver);

                    case XmlTypeCode.Notation:
                        return XmlBaseConverter.QNameToString((XmlQualifiedName) value, nsResolver);
                }
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

