namespace System.Xml.Schema
{
    using System;
    using System.Runtime.InteropServices;
    using System.Xml;

    internal class Datatype_unsignedLong : Datatype_nonNegativeInteger
    {
        private static readonly Type atomicValueType = typeof(ulong);
        private static readonly Type listValueType = typeof(ulong[]);
        private static readonly System.Xml.Schema.FacetsChecker numeric10FacetsChecker = new Numeric10FacetsChecker(0M, 18446744073709551615M);

        internal override int Compare(object value1, object value2)
        {
            ulong num = (ulong) value1;
            return num.CompareTo(value2);
        }

        internal override Exception TryParseValue(string s, XmlNameTable nameTable, IXmlNamespaceResolver nsmgr, out object typedValue)
        {
            typedValue = null;
            Exception exception = numeric10FacetsChecker.CheckLexicalFacets(ref s, this);
            if (exception == null)
            {
                ulong num;
                exception = XmlConvert.TryToUInt64(s, out num);
                if (exception == null)
                {
                    exception = numeric10FacetsChecker.CheckValueFacets((decimal) num, this);
                    if (exception == null)
                    {
                        typedValue = num;
                        return null;
                    }
                }
            }
            return exception;
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
                return XmlTypeCode.UnsignedLong;
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

