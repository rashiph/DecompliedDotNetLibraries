namespace System.Security.Cryptography.Xml
{
    using System;
    using System.Xml;

    internal class MyXmlDocument : XmlDocument
    {
        protected override XmlAttribute CreateDefaultAttribute(string prefix, string localName, string namespaceURI)
        {
            return this.CreateAttribute(prefix, localName, namespaceURI);
        }
    }
}

