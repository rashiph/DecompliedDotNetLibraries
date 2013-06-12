namespace System.Xml.Schema
{
    using System.Xml;

    internal class Datatype_IDREF : Datatype_NCName
    {
        public override XmlTokenizedType TokenizedType
        {
            get
            {
                return XmlTokenizedType.IDREF;
            }
        }

        public override XmlTypeCode TypeCode
        {
            get
            {
                return XmlTypeCode.Idref;
            }
        }
    }
}

