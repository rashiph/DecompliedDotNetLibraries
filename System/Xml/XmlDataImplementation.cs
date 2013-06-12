namespace System.Xml
{
    internal sealed class XmlDataImplementation : XmlImplementation
    {
        public override XmlDocument CreateDocument()
        {
            return new XmlDataDocument(this);
        }
    }
}

