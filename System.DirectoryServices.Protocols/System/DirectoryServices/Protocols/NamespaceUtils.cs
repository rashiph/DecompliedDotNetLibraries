namespace System.DirectoryServices.Protocols
{
    using System;
    using System.Xml;

    internal class NamespaceUtils
    {
        private static XmlNamespaceManager xmlNamespace = new XmlNamespaceManager(new NameTable());

        static NamespaceUtils()
        {
            xmlNamespace.AddNamespace("se", "http://schemas.xmlsoap.org/soap/envelope/");
            xmlNamespace.AddNamespace("dsml", "urn:oasis:names:tc:DSML:2:0:core");
            xmlNamespace.AddNamespace("ad", "urn:schema-microsoft-com:activedirectory:dsmlv2");
            xmlNamespace.AddNamespace("xsd", "http://www.w3.org/2001/XMLSchema");
            xmlNamespace.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
        }

        private NamespaceUtils()
        {
        }

        public static XmlNamespaceManager GetDsmlNamespaceManager()
        {
            return xmlNamespace;
        }
    }
}

