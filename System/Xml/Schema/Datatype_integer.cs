namespace System.Xml.Schema
{
    using System;
    using System.Runtime.InteropServices;
    using System.Xml;

    internal class Datatype_integer : Datatype_decimal
    {
        internal override Exception TryParseValue(string s, XmlNameTable nameTable, IXmlNamespaceResolver nsmgr, out object typedValue)
        {
            typedValue = null;
            Exception exception = this.FacetsChecker.CheckLexicalFacets(ref s, this);
            if (exception == null)
            {
                decimal num;
                exception = XmlConvert.TryToInteger(s, out num);
                if (exception == null)
                {
                    exception = this.FacetsChecker.CheckValueFacets(num, this);
                    if (exception == null)
                    {
                        typedValue = num;
                        return null;
                    }
                }
            }
            return exception;
        }

        public override XmlTypeCode TypeCode
        {
            get
            {
                return XmlTypeCode.Integer;
            }
        }
    }
}

