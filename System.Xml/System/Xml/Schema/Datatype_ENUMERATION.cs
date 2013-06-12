namespace System.Xml.Schema
{
    using System.Xml;

    internal class Datatype_ENUMERATION : Datatype_NMTOKEN
    {
        public override XmlTokenizedType TokenizedType
        {
            get
            {
                return XmlTokenizedType.ENUMERATION;
            }
        }
    }
}

