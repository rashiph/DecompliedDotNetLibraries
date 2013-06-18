namespace System.Security.Cryptography.Xml
{
    using System;
    using System.Security.Permissions;
    using System.Xml;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public class KeyInfoNode : KeyInfoClause
    {
        private XmlElement m_node;

        public KeyInfoNode()
        {
        }

        public KeyInfoNode(XmlElement node)
        {
            this.m_node = node;
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
            return (xmlDocument.ImportNode(this.m_node, true) as XmlElement);
        }

        public override void LoadXml(XmlElement value)
        {
            this.m_node = value;
        }

        public XmlElement Value
        {
            get
            {
                return this.m_node;
            }
            set
            {
                this.m_node = value;
            }
        }
    }
}

