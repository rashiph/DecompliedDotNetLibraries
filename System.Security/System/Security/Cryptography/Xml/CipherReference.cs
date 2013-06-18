namespace System.Security.Cryptography.Xml
{
    using System;
    using System.Security;
    using System.Security.Cryptography;
    using System.Security.Permissions;
    using System.Xml;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public sealed class CipherReference : EncryptedReference
    {
        private byte[] m_cipherValue;

        public CipherReference()
        {
            base.ReferenceType = "CipherReference";
        }

        public CipherReference(string uri) : base(uri)
        {
            base.ReferenceType = "CipherReference";
        }

        public CipherReference(string uri, TransformChain transformChain) : base(uri, transformChain)
        {
            base.ReferenceType = "CipherReference";
        }

        public override XmlElement GetXml()
        {
            if (base.CacheValid)
            {
                return base.m_cachedXml;
            }
            XmlDocument document = new XmlDocument {
                PreserveWhitespace = true
            };
            return this.GetXml(document);
        }

        internal XmlElement GetXml(XmlDocument document)
        {
            if (base.ReferenceType == null)
            {
                throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_ReferenceTypeRequired"));
            }
            XmlElement element = document.CreateElement(base.ReferenceType, "http://www.w3.org/2001/04/xmlenc#");
            if (!string.IsNullOrEmpty(base.Uri))
            {
                element.SetAttribute("URI", base.Uri);
            }
            if (base.TransformChain.Count > 0)
            {
                element.AppendChild(base.TransformChain.GetXml(document, "http://www.w3.org/2001/04/xmlenc#"));
            }
            return element;
        }

        public override void LoadXml(XmlElement value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            base.ReferenceType = value.LocalName;
            base.Uri = System.Security.Cryptography.Xml.Utils.GetAttribute(value, "URI", "http://www.w3.org/2001/04/xmlenc#");
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(value.OwnerDocument.NameTable);
            nsmgr.AddNamespace("enc", "http://www.w3.org/2001/04/xmlenc#");
            XmlNode node = value.SelectSingleNode("enc:Transforms", nsmgr);
            if (node != null)
            {
                base.TransformChain.LoadXml(node as XmlElement);
            }
            base.m_cachedXml = value;
        }

        internal byte[] CipherValue
        {
            get
            {
                if (!base.CacheValid)
                {
                    return null;
                }
                return this.m_cipherValue;
            }
            set
            {
                this.m_cipherValue = value;
            }
        }
    }
}

