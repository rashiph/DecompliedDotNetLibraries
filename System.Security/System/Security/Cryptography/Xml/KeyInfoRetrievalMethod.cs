namespace System.Security.Cryptography.Xml
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Xml;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public class KeyInfoRetrievalMethod : KeyInfoClause
    {
        private string m_type;
        private string m_uri;

        public KeyInfoRetrievalMethod()
        {
        }

        public KeyInfoRetrievalMethod(string strUri)
        {
            this.m_uri = strUri;
        }

        public KeyInfoRetrievalMethod(string strUri, string typeName)
        {
            this.m_uri = strUri;
            this.m_type = typeName;
        }

        public override XmlElement GetXml()
        {
            XmlDocument xmlDocument = new XmlDocument {
                PreserveWhitespace = true
            };
            return this.GetXml(xmlDocument);
        }

        internal override XmlElement GetXml(XmlDocument xmlDocument)
        {
            XmlElement element = xmlDocument.CreateElement("RetrievalMethod", "http://www.w3.org/2000/09/xmldsig#");
            if (!string.IsNullOrEmpty(this.m_uri))
            {
                element.SetAttribute("URI", this.m_uri);
            }
            if (!string.IsNullOrEmpty(this.m_type))
            {
                element.SetAttribute("Type", this.m_type);
            }
            return element;
        }

        public override void LoadXml(XmlElement value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            this.m_uri = Utils.GetAttribute(value, "URI", "http://www.w3.org/2000/09/xmldsig#");
            this.m_type = Utils.GetAttribute(value, "Type", "http://www.w3.org/2000/09/xmldsig#");
        }

        [ComVisible(false)]
        public string Type
        {
            get
            {
                return this.m_type;
            }
            set
            {
                this.m_type = value;
            }
        }

        public string Uri
        {
            get
            {
                return this.m_uri;
            }
            set
            {
                this.m_uri = value;
            }
        }
    }
}

