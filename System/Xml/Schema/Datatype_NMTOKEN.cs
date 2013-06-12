namespace System.Xml.Schema
{
    using System.Xml;

    internal class Datatype_NMTOKEN : Datatype_token
    {
        public override XmlTokenizedType TokenizedType
        {
            get
            {
                return XmlTokenizedType.NMTOKEN;
            }
        }

        public override XmlTypeCode TypeCode
        {
            get
            {
                return XmlTypeCode.NmToken;
            }
        }
    }
}

