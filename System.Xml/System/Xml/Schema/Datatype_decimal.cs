namespace System.Xml.Schema
{
    using System;
    using System.Runtime.InteropServices;
    using System.Xml;

    internal class Datatype_decimal : Datatype_anySimpleType
    {
        private static readonly Type atomicValueType = typeof(decimal);
        private static readonly Type listValueType = typeof(decimal[]);
        private static readonly System.Xml.Schema.FacetsChecker numeric10FacetsChecker = new Numeric10FacetsChecker(-79228162514264337593543950335M, 79228162514264337593543950335M);

        internal override int Compare(object value1, object value2)
        {
            decimal num = (decimal) value1;
            return num.CompareTo(value2);
        }

        internal override XmlValueConverter CreateValueConverter(XmlSchemaType schemaType)
        {
            return XmlNumeric10Converter.Create(schemaType);
        }

        internal override Exception TryParseValue(string s, XmlNameTable nameTable, IXmlNamespaceResolver nsmgr, out object typedValue)
        {
            typedValue = null;
            Exception exception = numeric10FacetsChecker.CheckLexicalFacets(ref s, this);
            if (exception == null)
            {
                decimal num;
                exception = XmlConvert.TryToDecimal(s, out num);
                if (exception == null)
                {
                    exception = numeric10FacetsChecker.CheckValueFacets(num, this);
                    if (exception == null)
                    {
                        typedValue = num;
                        return null;
                    }
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
                return numeric10FacetsChecker;
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
                return XmlTypeCode.Decimal;
            }
        }

        internal override RestrictionFlags ValidRestrictionFlags
        {
            get
            {
                return (RestrictionFlags.FractionDigits | RestrictionFlags.TotalDigits | RestrictionFlags.MinExclusive | RestrictionFlags.MinInclusive | RestrictionFlags.MaxExclusive | RestrictionFlags.MaxInclusive | RestrictionFlags.WhiteSpace | RestrictionFlags.Enumeration | RestrictionFlags.Pattern);
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

