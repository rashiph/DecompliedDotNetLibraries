namespace System.Security.Cryptography.Xml
{
    using System;
    using System.Security;
    using System.Security.Cryptography;
    using System.Security.Permissions;
    using System.Xml;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public sealed class EncryptionProperty
    {
        private XmlElement m_cachedXml;
        private XmlElement m_elemProp;
        private string m_id;
        private string m_target;

        public EncryptionProperty()
        {
        }

        public EncryptionProperty(XmlElement elementProperty)
        {
            if (elementProperty == null)
            {
                throw new ArgumentNullException("elementProperty");
            }
            if ((elementProperty.LocalName != "EncryptionProperty") || (elementProperty.NamespaceURI != "http://www.w3.org/2001/04/xmlenc#"))
            {
                throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_InvalidEncryptionProperty"));
            }
            this.m_elemProp = elementProperty;
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
            return (document.ImportNode(this.m_elemProp, true) as XmlElement);
        }

        public void LoadXml(XmlElement value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if ((value.LocalName != "EncryptionProperty") || (value.NamespaceURI != "http://www.w3.org/2001/04/xmlenc#"))
            {
                throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_InvalidEncryptionProperty"));
            }
            this.m_cachedXml = value;
            this.m_id = System.Security.Cryptography.Xml.Utils.GetAttribute(value, "Id", "http://www.w3.org/2001/04/xmlenc#");
            this.m_target = System.Security.Cryptography.Xml.Utils.GetAttribute(value, "Target", "http://www.w3.org/2001/04/xmlenc#");
            this.m_elemProp = value;
        }

        private bool CacheValid
        {
            get
            {
                return (this.m_cachedXml != null);
            }
        }

        public string Id
        {
            get
            {
                return this.m_id;
            }
        }

        public XmlElement PropertyElement
        {
            get
            {
                return this.m_elemProp;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                if ((value.LocalName != "EncryptionProperty") || (value.NamespaceURI != "http://www.w3.org/2001/04/xmlenc#"))
                {
                    throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_InvalidEncryptionProperty"));
                }
                this.m_elemProp = value;
                this.m_cachedXml = null;
            }
        }

        public string Target
        {
            get
            {
                return this.m_target;
            }
        }
    }
}

