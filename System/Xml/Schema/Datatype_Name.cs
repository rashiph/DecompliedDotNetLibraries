namespace System.Xml.Schema
{
    internal class Datatype_Name : Datatype_token
    {
        public override XmlTypeCode TypeCode
        {
            get
            {
                return XmlTypeCode.Name;
            }
        }
    }
}

