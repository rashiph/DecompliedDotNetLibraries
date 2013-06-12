namespace System.Xml.Schema
{
    using System;
    using System.Xml;

    internal class Datatype_floatXdr : Datatype_float
    {
        public override object ParseValue(string s, XmlNameTable nameTable, IXmlNamespaceResolver nsmgr)
        {
            float num;
            try
            {
                num = XmlConvert.ToSingle(s);
            }
            catch (Exception exception)
            {
                throw new XmlSchemaException(Res.GetString("Sch_InvalidValue", new object[] { s }), exception);
            }
            if (float.IsInfinity(num) || float.IsNaN(num))
            {
                throw new XmlSchemaException("Sch_InvalidValue", s);
            }
            return num;
        }
    }
}

