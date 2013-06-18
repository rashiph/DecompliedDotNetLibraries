namespace System.DirectoryServices.Protocols
{
    using System;
    using System.Xml;

    public class DsmlAuthRequest : DirectoryRequest
    {
        private string directoryPrincipal;

        public DsmlAuthRequest()
        {
            this.directoryPrincipal = "";
        }

        public DsmlAuthRequest(string principal)
        {
            this.directoryPrincipal = "";
            this.directoryPrincipal = principal;
        }

        protected override XmlElement ToXmlNode(XmlDocument doc)
        {
            XmlElement element = base.CreateRequestElement(doc, "authRequest", false, null);
            XmlAttribute node = doc.CreateAttribute("principal", null);
            node.InnerText = this.Principal;
            element.Attributes.Append(node);
            return element;
        }

        public string Principal
        {
            get
            {
                return this.directoryPrincipal;
            }
            set
            {
                this.directoryPrincipal = value;
            }
        }
    }
}

