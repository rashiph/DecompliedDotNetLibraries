namespace System.Xml.Schema
{
    using System;
    using System.Runtime.InteropServices;
    using System.Xml;

    internal class Datatype_byte : Datatype_short
    {
        private static readonly Type atomicValueType = typeof(sbyte);
        private static readonly Type listValueType = typeof(sbyte[]);
        private static readonly System.Xml.Schema.FacetsChecker numeric10FacetsChecker = new Numeric10FacetsChecker(-128M, 127M);

        internal override int Compare(object value1, object value2)
        {
            sbyte num = (sbyte) value1;
            return num.CompareTo(value2);
        }

        internal override Exception TryParseValue(string s, XmlNameTable nameTable, IXmlNamespaceResolver nsmgr, out object typedValue)
        {
            typedValue = null;
            Exception exception = numeric10FacetsChecker.CheckLexicalFacets(ref s, this);
            if (exception == null)
            {
                sbyte num;
                exception = XmlConvert.TryToSByte(s, out num);
                if (exception == null)
                {
                    exception = numeric10FacetsChecker.CheckValueFacets((short) num, this);
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
                return XmlTypeCode.Byte;
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

