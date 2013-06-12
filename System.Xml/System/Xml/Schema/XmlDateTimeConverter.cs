namespace System.Xml.Schema
{
    using System;
    using System.Xml;

    internal class XmlDateTimeConverter : XmlBaseConverter
    {
        protected XmlDateTimeConverter(XmlSchemaType schemaType) : base(schemaType)
        {
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
            if (destinationType == XmlBaseConverter.DateTimeType)
            {
                return value;
            }
            if (destinationType == XmlBaseConverter.DateTimeOffsetType)
            {
                return this.ToDateTimeOffset(value);
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
            if (destinationType == XmlBaseConverter.DateTimeType)
            {
                return this.ToDateTime(value);
            }
            if (destinationType == XmlBaseConverter.DateTimeOffsetType)
            {
                return value;
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
            if (destinationType == XmlBaseConverter.DateTimeType)
            {
                return this.ToDateTime(value);
            }
            if (destinationType == XmlBaseConverter.DateTimeOffsetType)
            {
                return this.ToDateTimeOffset(value);
            }
            if (destinationType == XmlBaseConverter.StringType)
            {
                return this.ToString(value, nsResolver);
            }
            if (destinationType == XmlBaseConverter.XmlAtomicValueType)
            {
                if (type == XmlBaseConverter.DateTimeType)
                {
                    return new XmlAtomicValue(base.SchemaType, (DateTime) value);
                }
                if (type == XmlBaseConverter.DateTimeOffsetType)
                {
                    return new XmlAtomicValue(base.SchemaType, (DateTimeOffset) value);
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
                if (type == XmlBaseConverter.DateTimeType)
                {
                    return new XmlAtomicValue(base.SchemaType, (DateTime) value);
                }
                if (type == XmlBaseConverter.DateTimeOffsetType)
                {
                    return new XmlAtomicValue(base.SchemaType, (DateTimeOffset) value);
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
            if (destinationType == XmlBaseConverter.DateTimeType)
            {
                return this.ToDateTime(value);
            }
            if (destinationType == XmlBaseConverter.DateTimeOffsetType)
            {
                return this.ToDateTimeOffset(value);
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
            return new XmlDateTimeConverter(schemaType);
        }

        public override DateTime ToDateTime(DateTime value)
        {
            return value;
        }

        public override DateTime ToDateTime(DateTimeOffset value)
        {
            return XmlBaseConverter.DateTimeOffsetToDateTime(value);
        }

        public override DateTime ToDateTime(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            Type type = value.GetType();
            if (type == XmlBaseConverter.DateTimeType)
            {
                return (DateTime) value;
            }
            if (type == XmlBaseConverter.DateTimeOffsetType)
            {
                return this.ToDateTime((DateTimeOffset) value);
            }
            if (type == XmlBaseConverter.StringType)
            {
                return this.ToDateTime((string) value);
            }
            if (type == XmlBaseConverter.XmlAtomicValueType)
            {
                return ((XmlAtomicValue) value).ValueAsDateTime;
            }
            return (DateTime) this.ChangeListType(value, XmlBaseConverter.DateTimeType, null);
        }

        public override DateTime ToDateTime(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            switch (base.TypeCode)
            {
                case XmlTypeCode.Time:
                    return XmlBaseConverter.StringToTime(value);

                case XmlTypeCode.Date:
                    return XmlBaseConverter.StringToDate(value);

                case XmlTypeCode.GYearMonth:
                    return XmlBaseConverter.StringToGYearMonth(value);

                case XmlTypeCode.GYear:
                    return XmlBaseConverter.StringToGYear(value);

                case XmlTypeCode.GMonthDay:
                    return XmlBaseConverter.StringToGMonthDay(value);

                case XmlTypeCode.GDay:
                    return XmlBaseConverter.StringToGDay(value);

                case XmlTypeCode.GMonth:
                    return XmlBaseConverter.StringToGMonth(value);
            }
            return XmlBaseConverter.StringToDateTime(value);
        }

        public override DateTimeOffset ToDateTimeOffset(DateTime value)
        {
            return new DateTimeOffset(value);
        }

        public override DateTimeOffset ToDateTimeOffset(DateTimeOffset value)
        {
            return value;
        }

        public override DateTimeOffset ToDateTimeOffset(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            Type type = value.GetType();
            if (type == XmlBaseConverter.DateTimeType)
            {
                return this.ToDateTimeOffset((DateTime) value);
            }
            if (type == XmlBaseConverter.DateTimeOffsetType)
            {
                return (DateTimeOffset) value;
            }
            if (type == XmlBaseConverter.StringType)
            {
                return this.ToDateTimeOffset((string) value);
            }
            if (type == XmlBaseConverter.XmlAtomicValueType)
            {
                return ((XmlAtomicValue) value).ValueAsDateTime;
            }
            return (DateTimeOffset) this.ChangeListType(value, XmlBaseConverter.DateTimeOffsetType, null);
        }

        public override DateTimeOffset ToDateTimeOffset(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            switch (base.TypeCode)
            {
                case XmlTypeCode.Time:
                    return XmlBaseConverter.StringToTimeOffset(value);

                case XmlTypeCode.Date:
                    return XmlBaseConverter.StringToDateOffset(value);

                case XmlTypeCode.GYearMonth:
                    return XmlBaseConverter.StringToGYearMonthOffset(value);

                case XmlTypeCode.GYear:
                    return XmlBaseConverter.StringToGYearOffset(value);

                case XmlTypeCode.GMonthDay:
                    return XmlBaseConverter.StringToGMonthDayOffset(value);

                case XmlTypeCode.GDay:
                    return XmlBaseConverter.StringToGDayOffset(value);

                case XmlTypeCode.GMonth:
                    return XmlBaseConverter.StringToGMonthOffset(value);
            }
            return XmlBaseConverter.StringToDateTimeOffset(value);
        }

        public override string ToString(DateTime value)
        {
            switch (base.TypeCode)
            {
                case XmlTypeCode.Time:
                    return XmlBaseConverter.TimeToString(value);

                case XmlTypeCode.Date:
                    return XmlBaseConverter.DateToString(value);

                case XmlTypeCode.GYearMonth:
                    return XmlBaseConverter.GYearMonthToString(value);

                case XmlTypeCode.GYear:
                    return XmlBaseConverter.GYearToString(value);

                case XmlTypeCode.GMonthDay:
                    return XmlBaseConverter.GMonthDayToString(value);

                case XmlTypeCode.GDay:
                    return XmlBaseConverter.GDayToString(value);

                case XmlTypeCode.GMonth:
                    return XmlBaseConverter.GMonthToString(value);
            }
            return XmlBaseConverter.DateTimeToString(value);
        }

        public override string ToString(DateTimeOffset value)
        {
            switch (base.TypeCode)
            {
                case XmlTypeCode.Time:
                    return XmlBaseConverter.TimeOffsetToString(value);

                case XmlTypeCode.Date:
                    return XmlBaseConverter.DateOffsetToString(value);

                case XmlTypeCode.GYearMonth:
                    return XmlBaseConverter.GYearMonthOffsetToString(value);

                case XmlTypeCode.GYear:
                    return XmlBaseConverter.GYearOffsetToString(value);

                case XmlTypeCode.GMonthDay:
                    return XmlBaseConverter.GMonthDayOffsetToString(value);

                case XmlTypeCode.GDay:
                    return XmlBaseConverter.GDayOffsetToString(value);

                case XmlTypeCode.GMonth:
                    return XmlBaseConverter.GMonthOffsetToString(value);
            }
            return XmlBaseConverter.DateTimeOffsetToString(value);
        }

        public override string ToString(object value, IXmlNamespaceResolver nsResolver)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            Type type = value.GetType();
            if (type == XmlBaseConverter.DateTimeType)
            {
                return this.ToString((DateTime) value);
            }
            if (type == XmlBaseConverter.DateTimeOffsetType)
            {
                return this.ToString((DateTimeOffset) value);
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

