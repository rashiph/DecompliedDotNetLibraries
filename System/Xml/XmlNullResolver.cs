namespace System.Xml
{
    using System;
    using System.Net;

    internal class XmlNullResolver : XmlResolver
    {
        public static readonly XmlNullResolver Singleton = new XmlNullResolver();

        private XmlNullResolver()
        {
        }

        public override object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn)
        {
            throw new XmlException("Xml_NullResolver", string.Empty);
        }

        public override ICredentials Credentials
        {
            set
            {
            }
        }
    }
}

