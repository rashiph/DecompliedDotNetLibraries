namespace System.Xml.Schema
{
    using System;
    using System.Xml;

    internal class Datatype_QNameXdr : Datatype_anySimpleType
    {
        private static readonly Type atomicValueType = typeof(XmlQualifiedName);
        private static readonly Type listValueType = typeof(XmlQualifiedName[]);

        public override object ParseValue(string s, XmlNameTable nameTable, IXmlNamespaceResolver nsmgr)
        {
            object obj2;
            if ((s == null) || (s.Length == 0))
            {
                throw new XmlSchemaException("Sch_EmptyAttributeValue", string.Empty);
            }
            if (nsmgr == null)
            {
                throw new ArgumentNullException("nsmgr");
            }
            try
            {
                string str;
                obj2 = XmlQualifiedName.Parse(s.Trim(), nsmgr, out str);
            }
            catch (XmlSchemaException exception)
            {
                throw exception;
            }
            catch (Exception exception2)
            {
                throw new XmlSchemaException(Res.GetString("Sch_InvalidValue", new object[] { s }), exception2);
            }
            return obj2;
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
                return XmlTokenizedType.QName;
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

