namespace System.Xml.Schema
{
    using System;
    using System.Runtime.InteropServices;
    using System.Xml;

    internal class Datatype_long : Datatype_integer
    {
        private static readonly Type atomicValueType = typeof(long);
        private static readonly Type listValueType = typeof(long[]);
        private static readonly System.Xml.Schema.FacetsChecker numeric10FacetsChecker = new Numeric10FacetsChecker(-9223372036854775808M, 9223372036854775807M);

        internal override int Compare(object value1, object value2)
        {
            long num = (long) value1;
            return num.CompareTo(value2);
        }

        internal override Exception TryParseValue(string s, XmlNameTable nameTable, IXmlNamespaceResolver nsmgr, out object typedValue)
        {
            typedValue = null;
            Exception exception = numeric10FacetsChecker.CheckLexicalFacets(ref s, this);
            if (exception == null)
            {
                long num;
                exception = XmlConvert.TryToInt64(s, out num);
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

        internal override bool HasValueFacets
        {
            get
            {
                return true;
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
                return XmlTypeCode.Long;
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

