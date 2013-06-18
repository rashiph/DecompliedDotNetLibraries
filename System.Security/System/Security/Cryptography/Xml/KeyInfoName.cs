namespace System.Security.Cryptography.Xml
{
    using System;
    using System.Security.Permissions;
    using System.Xml;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public class KeyInfoName : KeyInfoClause
    {
        private string m_keyName;

        public KeyInfoName() : this(null)
        {
        }

        public KeyInfoName(string keyName)
        {
            this.Value = keyName;
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
            XmlElement element = xmlDocument.CreateElement("KeyName", "http://www.w3.org/2000/09/xmldsig#");
            element.AppendChild(xmlDocument.CreateTextNode(this.m_keyName));
            return element;
        }

        public override void LoadXml(XmlElement value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            XmlElement element = value;
            this.m_keyName = element.InnerText.Trim();
        }

        public string Value
        {
            get
            {
                return this.m_keyName;
            }
            set
            {
                this.m_keyName = value;
            }
        }
    }
}

