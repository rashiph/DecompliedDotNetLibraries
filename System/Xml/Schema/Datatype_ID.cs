namespace System.Xml.Schema
{
    using System.Xml;

    internal class Datatype_ID : Datatype_NCName
    {
        public override XmlTokenizedType TokenizedType
        {
            get
            {
                return XmlTokenizedType.ID;
            }
        }

        public override XmlTypeCode TypeCode
        {
            get
            {
                return XmlTypeCode.Id;
            }
        }
    }
}

