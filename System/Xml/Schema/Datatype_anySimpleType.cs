namespace System.Xml.Schema
{
    using System;
    using System.Runtime.InteropServices;
    using System.Xml;

    internal class Datatype_anySimpleType : DatatypeImplementation
    {
        private static readonly Type atomicValueType = typeof(string);
        private static readonly Type listValueType = typeof(string[]);

        internal override int Compare(object value1, object value2)
        {
            return string.Compare(value1.ToString(), value2.ToString(), StringComparison.Ordinal);
        }

        internal override XmlValueConverter CreateValueConverter(XmlSchemaType schemaType)
        {
            return XmlUntypedConverter.Untyped;
        }

        internal override Exception TryParseValue(string s, XmlNameTable nameTable, IXmlNamespaceResolver nsmgr, out object typedValue)
        {
            typedValue = XmlComplianceUtil.NonCDataNormalize(s);
            return null;
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
                return DatatypeImplementation.miscFacetsChecker;
            }
        }

        internal override Type ListValueType
        {
            get
            {
                return listValueType;
            }
        }

        public override XmlTokenizedType TokenizedType
        {
            get
            {
                return XmlTokenizedType.None;
            }
        }

        public override XmlTypeCode TypeCode
        {
            get
            {
                return XmlTypeCode.AnyAtomicType;
            }
        }

        internal override RestrictionFlags ValidRestrictionFlags
        {
            get
            {
                return 0;
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

