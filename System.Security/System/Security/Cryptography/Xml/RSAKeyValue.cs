namespace System.Security.Cryptography.Xml
{
    using System;
    using System.Security.Cryptography;
    using System.Security.Permissions;
    using System.Xml;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public class RSAKeyValue : KeyInfoClause
    {
        private RSA m_key;

        public RSAKeyValue()
        {
            this.m_key = RSA.Create();
        }

        public RSAKeyValue(RSA key)
        {
            this.m_key = key;
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
            RSAParameters parameters = this.m_key.ExportParameters(false);
            XmlElement element = xmlDocument.CreateElement("KeyValue", "http://www.w3.org/2000/09/xmldsig#");
            XmlElement newChild = xmlDocument.CreateElement("RSAKeyValue", "http://www.w3.org/2000/09/xmldsig#");
            XmlElement element3 = xmlDocument.CreateElement("Modulus", "http://www.w3.org/2000/09/xmldsig#");
            element3.AppendChild(xmlDocument.CreateTextNode(Convert.ToBase64String(parameters.Modulus)));
            newChild.AppendChild(element3);
            XmlElement element4 = xmlDocument.CreateElement("Exponent", "http://www.w3.org/2000/09/xmldsig#");
            element4.AppendChild(xmlDocument.CreateTextNode(Convert.ToBase64String(parameters.Exponent)));
            newChild.AppendChild(element4);
            element.AppendChild(newChild);
            return element;
        }

        public override void LoadXml(XmlElement value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            this.m_key.FromXmlString(value.OuterXml);
        }

        public RSA Key
        {
            get
            {
                return this.m_key;
            }
            set
            {
                this.m_key = value;
            }
        }
    }
}

