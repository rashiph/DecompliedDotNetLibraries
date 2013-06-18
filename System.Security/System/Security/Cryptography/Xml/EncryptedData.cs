namespace System.Security.Cryptography.Xml
{
    using System;
    using System.Security;
    using System.Security.Cryptography;
    using System.Security.Permissions;
    using System.Xml;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public sealed class EncryptedData : EncryptedType
    {
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
            XmlElement element = document.CreateElement("EncryptedData", "http://www.w3.org/2001/04/xmlenc#");
            if (!string.IsNullOrEmpty(this.Id))
            {
                element.SetAttribute("Id", this.Id);
            }
            if (!string.IsNullOrEmpty(this.Type))
            {
                element.SetAttribute("Type", this.Type);
            }
            if (!string.IsNullOrEmpty(this.MimeType))
            {
                element.SetAttribute("MimeType", this.MimeType);
            }
            if (!string.IsNullOrEmpty(this.Encoding))
            {
                element.SetAttribute("Encoding", this.Encoding);
            }
            if (this.EncryptionMethod != null)
            {
                element.AppendChild(this.EncryptionMethod.GetXml(document));
            }
            if (base.KeyInfo.Count > 0)
            {
                element.AppendChild(base.KeyInfo.GetXml(document));
            }
            if (this.CipherData == null)
            {
                throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_MissingCipherData"));
            }
            element.AppendChild(this.CipherData.GetXml(document));
            if (this.EncryptionProperties.Count > 0)
            {
                XmlElement newChild = document.CreateElement("EncryptionProperties", "http://www.w3.org/2001/04/xmlenc#");
                for (int i = 0; i < this.EncryptionProperties.Count; i++)
                {
                    EncryptionProperty property = this.EncryptionProperties.Item(i);
                    newChild.AppendChild(property.GetXml(document));
                }
                element.AppendChild(newChild);
            }
            return element;
        }

        public override void LoadXml(XmlElement value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(value.OwnerDocument.NameTable);
            nsmgr.AddNamespace("enc", "http://www.w3.org/2001/04/xmlenc#");
            nsmgr.AddNamespace("ds", "http://www.w3.org/2000/09/xmldsig#");
            this.Id = System.Security.Cryptography.Xml.Utils.GetAttribute(value, "Id", "http://www.w3.org/2001/04/xmlenc#");
            this.Type = System.Security.Cryptography.Xml.Utils.GetAttribute(value, "Type", "http://www.w3.org/2001/04/xmlenc#");
            this.MimeType = System.Security.Cryptography.Xml.Utils.GetAttribute(value, "MimeType", "http://www.w3.org/2001/04/xmlenc#");
            this.Encoding = System.Security.Cryptography.Xml.Utils.GetAttribute(value, "Encoding", "http://www.w3.org/2001/04/xmlenc#");
            XmlNode node = value.SelectSingleNode("enc:EncryptionMethod", nsmgr);
            this.EncryptionMethod = new EncryptionMethod();
            if (node != null)
            {
                this.EncryptionMethod.LoadXml(node as XmlElement);
            }
            base.KeyInfo = new KeyInfo();
            XmlNode node2 = value.SelectSingleNode("ds:KeyInfo", nsmgr);
            if (node2 != null)
            {
                base.KeyInfo.LoadXml(node2 as XmlElement);
            }
            XmlNode node3 = value.SelectSingleNode("enc:CipherData", nsmgr);
            if (node3 == null)
            {
                throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_MissingCipherData"));
            }
            this.CipherData = new CipherData();
            this.CipherData.LoadXml(node3 as XmlElement);
            XmlNode node4 = value.SelectSingleNode("enc:EncryptionProperties", nsmgr);
            if (node4 != null)
            {
                XmlNodeList list = node4.SelectNodes("enc:EncryptionProperty", nsmgr);
                if (list != null)
                {
                    foreach (XmlNode node5 in list)
                    {
                        EncryptionProperty property = new EncryptionProperty();
                        property.LoadXml(node5 as XmlElement);
                        this.EncryptionProperties.Add(property);
                    }
                }
            }
            base.m_cachedXml = value;
        }
    }
}

