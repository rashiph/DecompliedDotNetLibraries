namespace System.Security.Cryptography.Xml
{
    using System;
    using System.Security;
    using System.Security.Cryptography;
    using System.Security.Permissions;
    using System.Xml;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public sealed class CipherData
    {
        private XmlElement m_cachedXml;
        private System.Security.Cryptography.Xml.CipherReference m_cipherReference;
        private byte[] m_cipherValue;

        public CipherData()
        {
        }

        public CipherData(byte[] cipherValue)
        {
            this.CipherValue = cipherValue;
        }

        public CipherData(System.Security.Cryptography.Xml.CipherReference cipherReference)
        {
            this.CipherReference = cipherReference;
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
            XmlElement element = document.CreateElement("CipherData", "http://www.w3.org/2001/04/xmlenc#");
            if (this.CipherValue != null)
            {
                XmlElement newChild = document.CreateElement("CipherValue", "http://www.w3.org/2001/04/xmlenc#");
                newChild.AppendChild(document.CreateTextNode(Convert.ToBase64String(this.CipherValue)));
                element.AppendChild(newChild);
                return element;
            }
            if (this.CipherReference == null)
            {
                throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_CipherValueElementRequired"));
            }
            element.AppendChild(this.CipherReference.GetXml(document));
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
            XmlNode node = value.SelectSingleNode("enc:CipherValue", nsmgr);
            XmlNode node2 = value.SelectSingleNode("enc:CipherReference", nsmgr);
            if (node != null)
            {
                if (node2 != null)
                {
                    throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_CipherValueElementRequired"));
                }
                this.m_cipherValue = Convert.FromBase64String(System.Security.Cryptography.Xml.Utils.DiscardWhiteSpaces(node.InnerText));
            }
            else
            {
                if (node2 == null)
                {
                    throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_CipherValueElementRequired"));
                }
                this.m_cipherReference = new System.Security.Cryptography.Xml.CipherReference();
                this.m_cipherReference.LoadXml((XmlElement) node2);
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

        public System.Security.Cryptography.Xml.CipherReference CipherReference
        {
            get
            {
                return this.m_cipherReference;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                if (this.CipherValue != null)
                {
                    throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_CipherValueElementRequired"));
                }
                this.m_cipherReference = value;
                this.m_cachedXml = null;
            }
        }

        public byte[] CipherValue
        {
            get
            {
                return this.m_cipherValue;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                if (this.CipherReference != null)
                {
                    throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_CipherValueElementRequired"));
                }
                this.m_cipherValue = (byte[]) value.Clone();
                this.m_cachedXml = null;
            }
        }
    }
}

