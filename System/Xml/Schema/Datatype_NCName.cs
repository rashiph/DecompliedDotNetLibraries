namespace System.Xml.Schema
{
    using System;
    using System.Runtime.InteropServices;
    using System.Xml;

    internal class Datatype_NCName : Datatype_Name
    {
        internal override Exception TryParseValue(string s, XmlNameTable nameTable, IXmlNamespaceResolver nsmgr, out object typedValue)
        {
            typedValue = null;
            Exception exception = DatatypeImplementation.stringFacetsChecker.CheckLexicalFacets(ref s, this);
            if (exception == null)
            {
                exception = DatatypeImplementation.stringFacetsChecker.CheckValueFacets(s, this);
                if (exception == null)
                {
                    nameTable.Add(s);
                    typedValue = s;
                    return null;
                }
            }
            return exception;
        }

        public override XmlTypeCode TypeCode
        {
            get
            {
                return XmlTypeCode.NCName;
            }
        }
    }
}

