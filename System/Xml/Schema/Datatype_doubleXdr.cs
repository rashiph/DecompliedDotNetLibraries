namespace System.Xml.Schema
{
    using System;
    using System.Xml;

    internal class Datatype_doubleXdr : Datatype_double
    {
        public override object ParseValue(string s, XmlNameTable nameTable, IXmlNamespaceResolver nsmgr)
        {
            double num;
            try
            {
                num = XmlConvert.ToDouble(s);
            }
            catch (Exception exception)
            {
                throw new XmlSchemaException(Res.GetString("Sch_InvalidValue", new object[] { s }), exception);
            }
            if (double.IsInfinity(num) || double.IsNaN(num))
            {
                throw new XmlSchemaException("Sch_InvalidValue", s);
            }
            return num;
        }
    }
}

