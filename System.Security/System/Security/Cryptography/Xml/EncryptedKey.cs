namespace System.Security.Cryptography.Xml
{
    using System;
    using System.Security;
    using System.Security.Cryptography;
    using System.Security.Permissions;
    using System.Xml;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public sealed class EncryptedKey : EncryptedType
    {
        private string m_carriedKeyName;
        private string m_recipient;
        private System.Security.Cryptography.Xml.ReferenceList m_referenceList;

        public void AddReference(DataReference dataReference)
        {
            this.ReferenceList.Add(dataReference);
        }

        public void AddReference(KeyReference keyReference)
        {
            this.ReferenceList.Add(keyReference);
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
            XmlElement element = document.CreateElement("EncryptedKey", "http://www.w3.org/2001/04/xmlenc#");
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
            if (!string.IsNullOrEmpty(this.Recipient))
            {
                element.SetAttribute("Recipient", this.Recipient);
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
            if (this.ReferenceList.Count > 0)
            {
                XmlElement element3 = document.CreateElement("ReferenceList", "http://www.w3.org/2001/04/xmlenc#");
                for (int j = 0; j < this.ReferenceList.Count; j++)
                {
                    element3.AppendChild(this.ReferenceList[j].GetXml(document));
                }
                element.AppendChild(element3);
            }
            if (this.CarriedKeyName != null)
            {
                XmlElement element4 = document.CreateElement("CarriedKeyName", "http://www.w3.org/2001/04/xmlenc#");
                XmlText text = document.CreateTextNode(this.CarriedKeyName);
                element4.AppendChild(text);
                element.AppendChild(element4);
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
            this.Recipient = System.Security.Cryptography.Xml.Utils.GetAttribute(value, "Recipient", "http://www.w3.org/2001/04/xmlenc#");
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
            XmlNode node6 = value.SelectSingleNode("enc:CarriedKeyName", nsmgr);
            if (node6 != null)
            {
                this.CarriedKeyName = node6.InnerText;
            }
            XmlNode node7 = value.SelectSingleNode("enc:ReferenceList", nsmgr);
            if (node7 != null)
            {
                XmlNodeList list2 = node7.SelectNodes("enc:DataReference", nsmgr);
                if (list2 != null)
                {
                    foreach (XmlNode node8 in list2)
                    {
                        DataReference reference = new DataReference();
                        reference.LoadXml(node8 as XmlElement);
                        this.ReferenceList.Add(reference);
                    }
                }
                XmlNodeList list3 = node7.SelectNodes("enc:KeyReference", nsmgr);
                if (list3 != null)
                {
                    foreach (XmlNode node9 in list3)
                    {
                        KeyReference reference2 = new KeyReference();
                        reference2.LoadXml(node9 as XmlElement);
                        this.ReferenceList.Add(reference2);
                    }
                }
            }
            base.m_cachedXml = value;
        }

        public string CarriedKeyName
        {
            get
            {
                return this.m_carriedKeyName;
            }
            set
            {
                this.m_carriedKeyName = value;
                base.m_cachedXml = null;
            }
        }

        public string Recipient
        {
            get
            {
                if (this.m_recipient == null)
                {
                    this.m_recipient = string.Empty;
                }
                return this.m_recipient;
            }
            set
            {
                this.m_recipient = value;
                base.m_cachedXml = null;
            }
        }

        public System.Security.Cryptography.Xml.ReferenceList ReferenceList
        {
            get
            {
                if (this.m_referenceList == null)
                {
                    this.m_referenceList = new System.Security.Cryptography.Xml.ReferenceList();
                }
                return this.m_referenceList;
            }
        }
    }
}

