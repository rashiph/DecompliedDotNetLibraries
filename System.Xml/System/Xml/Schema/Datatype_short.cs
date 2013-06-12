namespace System.Xml.Schema
{
    using System;
    using System.Runtime.InteropServices;
    using System.Xml;

    internal class Datatype_short : Datatype_int
    {
        private static readonly Type atomicValueType = typeof(short);
        private static readonly Type listValueType = typeof(short[]);
        private static readonly System.Xml.Schema.FacetsChecker numeric10FacetsChecker = new Numeric10FacetsChecker(-32768M, 32767M);

        internal override int Compare(object value1, object value2)
        {
            short num = (short) value1;
            return num.CompareTo(value2);
        }

        internal override Exception TryParseValue(string s, XmlNameTable nameTable, IXmlNamespaceResolver nsmgr, out object typedValue)
        {
            typedValue = null;
            Exception exception = numeric10FacetsChecker.CheckLexicalFacets(ref s, this);
            if (exception == null)
            {
                short num;
                exception = XmlConvert.TryToInt16(s, out num);
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
                return XmlTypeCode.Short;
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

