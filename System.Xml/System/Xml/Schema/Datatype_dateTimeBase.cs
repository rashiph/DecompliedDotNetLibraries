namespace System.Xml.Schema
{
    using System;
    using System.Runtime.InteropServices;
    using System.Xml;

    internal class Datatype_dateTimeBase : Datatype_anySimpleType
    {
        private static readonly Type atomicValueType = typeof(DateTime);
        private XsdDateTimeFlags dateTimeFlags;
        private static readonly Type listValueType = typeof(DateTime[]);

        internal Datatype_dateTimeBase()
        {
        }

        internal Datatype_dateTimeBase(XsdDateTimeFlags dateTimeFlags)
        {
            this.dateTimeFlags = dateTimeFlags;
        }

        internal override int Compare(object value1, object value2)
        {
            DateTime time = (DateTime) value1;
            DateTime time2 = (DateTime) value2;
            if ((time.Kind != DateTimeKind.Unspecified) && (time2.Kind != DateTimeKind.Unspecified))
            {
                return time.ToUniversalTime().CompareTo(time2.ToUniversalTime());
            }
            return time.CompareTo(time2);
        }

        internal override XmlValueConverter CreateValueConverter(XmlSchemaType schemaType)
        {
            return XmlDateTimeConverter.Create(schemaType);
        }

        internal override Exception TryParseValue(string s, XmlNameTable nameTable, IXmlNamespaceResolver nsmgr, out object typedValue)
        {
            typedValue = null;
            Exception exception = DatatypeImplementation.dateTimeFacetsChecker.CheckLexicalFacets(ref s, this);
            if (exception == null)
            {
                XsdDateTime time;
                if (!XsdDateTime.TryParse(s, this.dateTimeFlags, out time))
                {
                    return new FormatException(Res.GetString("XmlConvert_BadFormat", new object[] { s, this.dateTimeFlags.ToString() }));
                }
                DateTime minValue = DateTime.MinValue;
                try
                {
                    minValue = (DateTime) time;
                }
                catch (ArgumentException exception2)
                {
                    return exception2;
                }
                exception = DatatypeImplementation.dateTimeFacetsChecker.CheckValueFacets(minValue, this);
                if (exception == null)
                {
                    typedValue = minValue;
                    return null;
                }
            }
            return exception;
        }

        internal override XmlSchemaWhiteSpace BuiltInWhitespaceFacet
        {
            get
            {
                return XmlSchemaWhiteSpace.Collapse;
            }
        }

        internal override System.Xml.Schema.FacetsChecker FacetsChecker
        {
            get
            {
                return DatatypeImplementation.dateTimeFacetsChecker;
            }
        }

        internal override Type ListValueType
        {
            get
            {
                return listValueType;
            }
        }

        public override XmlTypeCode TypeCode
        {
            get
            {
                return XmlTypeCode.DateTime;
            }
        }

        internal override RestrictionFlags ValidRestrictionFlags
        {
            get
            {
                return (RestrictionFlags.MinExclusive | RestrictionFlags.MinInclusive | RestrictionFlags.MaxExclusive | RestrictionFlags.MaxInclusive | RestrictionFlags.WhiteSpace | RestrictionFlags.Enumeration | RestrictionFlags.Pattern);
            }
        }

        public override Type ValueType
        {
            get
            {
                return atomicValueType;
            }
        }
    }
}

