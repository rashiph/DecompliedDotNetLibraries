namespace System.Security.Cryptography.Xml
{
    using System;
    using System.Security.Permissions;
    using System.Xml;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public class DataObject
    {
        private XmlElement m_cachedXml;
        private CanonicalXmlNodeList m_elData;
        private string m_encoding;
        private string m_id;
        private string m_mimeType;

        public DataObject()
        {
            this.m_cachedXml = null;
            this.m_elData = new CanonicalXmlNodeList();
        }

        public DataObject(string id, string mimeType, string encoding, XmlElement data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }
            this.m_id = id;
            this.m_mimeType = mimeType;
            this.m_encoding = encoding;
            this.m_elData = new CanonicalXmlNodeList();
            this.m_elData.Add(data);
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
            XmlElement element = document.CreateElement("Object", "http://www.w3.org/2000/09/xmldsig#");
            if (!string.IsNullOrEmpty(this.m_id))
            {
                element.SetAttribute("Id", this.m_id);
            }
            if (!string.IsNullOrEmpty(this.m_mimeType))
            {
                element.SetAttribute("MimeType", this.m_mimeType);
            }
            if (!string.IsNullOrEmpty(this.m_encoding))
            {
                element.SetAttribute("Encoding", this.m_encoding);
            }
            if (this.m_elData != null)
            {
                foreach (XmlNode node in this.m_elData)
                {
                    element.AppendChild(document.ImportNode(node, true));
                }
            }
            return element;
        }

        public void LoadXml(XmlElement value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            this.m_id = Utils.GetAttribute(value, "Id", "http://www.w3.org/2000/09/xmldsig#");
            this.m_mimeType = Utils.GetAttribute(value, "MimeType", "http://www.w3.org/2000/09/xmldsig#");
            this.m_encoding = Utils.GetAttribute(value, "Encoding", "http://www.w3.org/2000/09/xmldsig#");
            foreach (XmlNode node in value.ChildNodes)
            {
                this.m_elData.Add(node);
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

        public XmlNodeList Data
        {
            get
            {
                return this.m_elData;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.m_elData = new CanonicalXmlNodeList();
                foreach (XmlNode node in value)
                {
                    this.m_elData.Add(node);
                }
                this.m_cachedXml = null;
            }
        }

        public string Encoding
        {
            get
            {
                return this.m_encoding;
            }
            set
            {
                this.m_encoding = value;
                this.m_cachedXml = null;
            }
        }

        public string Id
        {
            get
            {
                return this.m_id;
            }
            set
            {
                this.m_id = value;
                this.m_cachedXml = null;
            }
        }

        public string MimeType
        {
            get
            {
                return this.m_mimeType;
            }
            set
            {
                this.m_mimeType = value;
                this.m_cachedXml = null;
            }
        }
    }
}

