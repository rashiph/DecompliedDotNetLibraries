namespace System.Xml.Schema
{
    using System.Xml;

    internal class Datatype_ENTITY : Datatype_NCName
    {
        public override XmlTokenizedType TokenizedType
        {
            get
            {
                return XmlTokenizedType.ENTITY;
            }
        }

        public override XmlTypeCode TypeCode
        {
            get
            {
                return XmlTypeCode.Entity;
            }
        }
    }
}

