namespace System.Security.Cryptography.Xml
{
    using System;
    using System.Security.Cryptography;
    using System.Security.Permissions;
    using System.Xml;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public class DSAKeyValue : KeyInfoClause
    {
        private DSA m_key;

        public DSAKeyValue()
        {
            this.m_key = DSA.Create();
        }

        public DSAKeyValue(DSA key)
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
            DSAParameters parameters = this.m_key.ExportParameters(false);
            XmlElement element = xmlDocument.CreateElement("KeyValue", "http://www.w3.org/2000/09/xmldsig#");
            XmlElement newChild = xmlDocument.CreateElement("DSAKeyValue", "http://www.w3.org/2000/09/xmldsig#");
            XmlElement element3 = xmlDocument.CreateElement("P", "http://www.w3.org/2000/09/xmldsig#");
            element3.AppendChild(xmlDocument.CreateTextNode(Convert.ToBase64String(parameters.P)));
            newChild.AppendChild(element3);
            XmlElement element4 = xmlDocument.CreateElement("Q", "http://www.w3.org/2000/09/xmldsig#");
            element4.AppendChild(xmlDocument.CreateTextNode(Convert.ToBase64String(parameters.Q)));
            newChild.AppendChild(element4);
            XmlElement element5 = xmlDocument.CreateElement("G", "http://www.w3.org/2000/09/xmldsig#");
            element5.AppendChild(xmlDocument.CreateTextNode(Convert.ToBase64String(parameters.G)));
            newChild.AppendChild(element5);
            XmlElement element6 = xmlDocument.CreateElement("Y", "http://www.w3.org/2000/09/xmldsig#");
            element6.AppendChild(xmlDocument.CreateTextNode(Convert.ToBase64String(parameters.Y)));
            newChild.AppendChild(element6);
            if (parameters.J != null)
            {
                XmlElement element7 = xmlDocument.CreateElement("J", "http://www.w3.org/2000/09/xmldsig#");
                element7.AppendChild(xmlDocument.CreateTextNode(Convert.ToBase64String(parameters.J)));
                newChild.AppendChild(element7);
            }
            if (parameters.Seed != null)
            {
                XmlElement element8 = xmlDocument.CreateElement("Seed", "http://www.w3.org/2000/09/xmldsig#");
                element8.AppendChild(xmlDocument.CreateTextNode(Convert.ToBase64String(parameters.Seed)));
                newChild.AppendChild(element8);
                XmlElement element9 = xmlDocument.CreateElement("PgenCounter", "http://www.w3.org/2000/09/xmldsig#");
                element9.AppendChild(xmlDocument.CreateTextNode(Convert.ToBase64String(System.Security.Cryptography.Xml.Utils.ConvertIntToByteArray(parameters.Counter))));
                newChild.AppendChild(element9);
            }
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

        public DSA Key
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

