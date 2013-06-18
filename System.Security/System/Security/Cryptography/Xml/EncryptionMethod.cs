namespace System.Security.Cryptography.Xml
{
    using System;
    using System.Security;
    using System.Security.Permissions;
    using System.Xml;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public class EncryptionMethod
    {
        private string m_algorithm;
        private XmlElement m_cachedXml;
        private int m_keySize;

        public EncryptionMethod()
        {
            this.m_cachedXml = null;
        }

        public EncryptionMethod(string algorithm)
        {
            this.m_algorithm = algorithm;
            this.m_cachedXml = null;
        }

        public XmlElement GetXml()
        {
            if (this.CacheValid)
            {
                return this.m_cachedXml;
            }
            XmlDocument document = new XmlDocument {
                PreserveWhitespace = true
            };
            return this.GetXml(document);
        }

        internal XmlElement GetXml(XmlDocument document)
        {
            XmlElement element = document.CreateElement("EncryptionMethod", "http://www.w3.org/2001/04/xmlenc#");
            if (!string.IsNullOrEmpty(this.m_algorithm))
            {
                element.SetAttribute("Algorithm", this.m_algorithm);
            }
            if (this.m_keySize > 0)
            {
                XmlElement newChild = document.CreateElement("KeySize", "http://www.w3.org/2001/04/xmlenc#");
                newChild.AppendChild(document.CreateTextNode(this.m_keySize.ToString(null, null)));
                element.AppendChild(newChild);
            }
            return element;
        }

        public void LoadXml(XmlElement value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(value.OwnerDocument.NameTable);
            nsmgr.AddNamespace("enc", "http://www.w3.org/2001/04/xmlenc#");
            XmlElement element = value;
            this.m_algorithm = Utils.GetAttribute(element, "Algorithm", "http://www.w3.org/2001/04/xmlenc#");
            XmlNode node = value.SelectSingleNode("enc:KeySize", nsmgr);
            if (node != null)
            {
                this.KeySize = Convert.ToInt32(Utils.DiscardWhiteSpaces(node.InnerText), (IFormatProvider) null);
            }
            this.m_cachedXml = value;
        }

        private bool CacheValid
        {
            get
            {
                return (this.m_cachedXml != null);
            }
        }

        public string KeyAlgorithm
        {
            get
            {
                return this.m_algorithm;
            }
            set
            {
                this.m_algorithm = value;
                this.m_cachedXml = null;
            }
        }

        public int KeySize
        {
            get
            {
                return this.m_keySize;
            }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(SecurityResources.GetResourceString("Cryptography_Xml_InvalidKeySize"));
                }
                this.m_keySize = value;
                this.m_cachedXml = null;
            }
        }
    }
}

