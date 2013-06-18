namespace System.Security.Cryptography.Xml
{
    using System;
    using System.Security;
    using System.Security.Cryptography;
    using System.Security.Permissions;
    using System.Xml;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public abstract class EncryptedReference
    {
        internal XmlElement m_cachedXml;
        private string m_referenceType;
        private System.Security.Cryptography.Xml.TransformChain m_transformChain;
        private string m_uri;

        protected EncryptedReference() : this(string.Empty, new System.Security.Cryptography.Xml.TransformChain())
        {
        }

        protected EncryptedReference(string uri) : this(uri, new System.Security.Cryptography.Xml.TransformChain())
        {
        }

        protected EncryptedReference(string uri, System.Security.Cryptography.Xml.TransformChain transformChain)
        {
            this.TransformChain = transformChain;
            this.Uri = uri;
            this.m_cachedXml = null;
        }

        public void AddTransform(Transform transform)
        {
            this.TransformChain.Add(transform);
        }

        public virtual XmlElement GetXml()
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
            if (this.ReferenceType == null)
            {
                throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_ReferenceTypeRequired"));
            }
            XmlElement element = document.CreateElement(this.ReferenceType, "http://www.w3.org/2001/04/xmlenc#");
            if (!string.IsNullOrEmpty(this.m_uri))
            {
                element.SetAttribute("URI", this.m_uri);
            }
            if (this.TransformChain.Count > 0)
            {
                element.AppendChild(this.TransformChain.GetXml(document, "http://www.w3.org/2000/09/xmldsig#"));
            }
            return element;
        }

        public virtual void LoadXml(XmlElement value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            this.ReferenceType = value.LocalName;
            this.Uri = System.Security.Cryptography.Xml.Utils.GetAttribute(value, "URI", "http://www.w3.org/2001/04/xmlenc#");
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(value.OwnerDocument.NameTable);
            nsmgr.AddNamespace("ds", "http://www.w3.org/2000/09/xmldsig#");
            XmlNode node = value.SelectSingleNode("ds:Transforms", nsmgr);
            if (node != null)
            {
                this.TransformChain.LoadXml(node as XmlElement);
            }
            this.m_cachedXml = value;
        }

        protected internal bool CacheValid
        {
            get
            {
                return (this.m_cachedXml != null);
            }
        }

        protected string ReferenceType
        {
            get
            {
                return this.m_referenceType;
            }
            set
            {
                this.m_referenceType = value;
                this.m_cachedXml = null;
            }
        }

        public System.Security.Cryptography.Xml.TransformChain TransformChain
        {
            get
            {
                if (this.m_transformChain == null)
                {
                    this.m_transformChain = new System.Security.Cryptography.Xml.TransformChain();
                }
                return this.m_transformChain;
            }
            set
            {
                this.m_transformChain = value;
                this.m_cachedXml = null;
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
                if (value == null)
                {
                    throw new ArgumentNullException(SecurityResources.GetResourceString("Cryptography_Xml_UriRequired"));
                }
                this.m_uri = value;
                this.m_cachedXml = null;
            }
        }
    }
}

