namespace System.Security.Cryptography.Xml
{
    using System;
    using System.Collections;
    using System.Security;
    using System.Security.Cryptography;
    using System.Security.Permissions;
    using System.Xml;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public class Signature
    {
        private IList m_embeddedObjects = new ArrayList();
        private string m_id;
        private System.Security.Cryptography.Xml.KeyInfo m_keyInfo;
        private CanonicalXmlNodeList m_referencedItems = new CanonicalXmlNodeList();
        private byte[] m_signatureValue;
        private string m_signatureValueId;
        private System.Security.Cryptography.Xml.SignedInfo m_signedInfo;
        private System.Security.Cryptography.Xml.SignedXml m_signedXml;

        public void AddObject(DataObject dataObject)
        {
            this.m_embeddedObjects.Add(dataObject);
        }

        public XmlElement GetXml()
        {
            XmlDocument document = new XmlDocument {
                PreserveWhitespace = true
            };
            return this.GetXml(document);
        }

        internal XmlElement GetXml(XmlDocument document)
        {
            XmlElement element = document.CreateElement("Signature", "http://www.w3.org/2000/09/xmldsig#");
            if (!string.IsNullOrEmpty(this.m_id))
            {
                element.SetAttribute("Id", this.m_id);
            }
            if (this.m_signedInfo == null)
            {
                throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_SignedInfoRequired"));
            }
            element.AppendChild(this.m_signedInfo.GetXml(document));
            if (this.m_signatureValue == null)
            {
                throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_SignatureValueRequired"));
            }
            XmlElement newChild = document.CreateElement("SignatureValue", "http://www.w3.org/2000/09/xmldsig#");
            newChild.AppendChild(document.CreateTextNode(Convert.ToBase64String(this.m_signatureValue)));
            if (!string.IsNullOrEmpty(this.m_signatureValueId))
            {
                newChild.SetAttribute("Id", this.m_signatureValueId);
            }
            element.AppendChild(newChild);
            if (this.KeyInfo.Count > 0)
            {
                element.AppendChild(this.KeyInfo.GetXml(document));
            }
            foreach (object obj2 in this.m_embeddedObjects)
            {
                DataObject obj3 = obj2 as DataObject;
                if (obj3 != null)
                {
                    element.AppendChild(obj3.GetXml(document));
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
            XmlElement element = value;
            if (!element.LocalName.Equals("Signature"))
            {
                throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_InvalidElement"), "Signature");
            }
            this.m_id = System.Security.Cryptography.Xml.Utils.GetAttribute(element, "Id", "http://www.w3.org/2000/09/xmldsig#");
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(value.OwnerDocument.NameTable);
            nsmgr.AddNamespace("ds", "http://www.w3.org/2000/09/xmldsig#");
            XmlElement element2 = element.SelectSingleNode("ds:SignedInfo", nsmgr) as XmlElement;
            if (element2 == null)
            {
                throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_InvalidElement"), "SignedInfo");
            }
            this.SignedInfo = new System.Security.Cryptography.Xml.SignedInfo();
            this.SignedInfo.LoadXml(element2);
            XmlElement element3 = element.SelectSingleNode("ds:SignatureValue", nsmgr) as XmlElement;
            if (element3 == null)
            {
                throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_InvalidElement"), "SignedInfo/SignatureValue");
            }
            this.m_signatureValue = Convert.FromBase64String(System.Security.Cryptography.Xml.Utils.DiscardWhiteSpaces(element3.InnerText));
            this.m_signatureValueId = System.Security.Cryptography.Xml.Utils.GetAttribute(element3, "Id", "http://www.w3.org/2000/09/xmldsig#");
            XmlNodeList list = element.SelectNodes("ds:KeyInfo", nsmgr);
            this.m_keyInfo = new System.Security.Cryptography.Xml.KeyInfo();
            if (list != null)
            {
                foreach (XmlNode node in list)
                {
                    XmlElement element4 = node as XmlElement;
                    if (element4 != null)
                    {
                        this.m_keyInfo.LoadXml(element4);
                    }
                }
            }
            XmlNodeList list2 = element.SelectNodes("ds:Object", nsmgr);
            this.m_embeddedObjects.Clear();
            if (list2 != null)
            {
                foreach (XmlNode node2 in list2)
                {
                    XmlElement element5 = node2 as XmlElement;
                    if (element5 != null)
                    {
                        DataObject obj2 = new DataObject();
                        obj2.LoadXml(element5);
                        this.m_embeddedObjects.Add(obj2);
                    }
                }
            }
            XmlNodeList list3 = element.SelectNodes("//*[@Id]", nsmgr);
            if (list3 != null)
            {
                foreach (XmlNode node3 in list3)
                {
                    this.m_referencedItems.Add(node3);
                }
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
            }
        }

        public System.Security.Cryptography.Xml.KeyInfo KeyInfo
        {
            get
            {
                if (this.m_keyInfo == null)
                {
                    this.m_keyInfo = new System.Security.Cryptography.Xml.KeyInfo();
                }
                return this.m_keyInfo;
            }
            set
            {
                this.m_keyInfo = value;
            }
        }

        public IList ObjectList
        {
            get
            {
                return this.m_embeddedObjects;
            }
            set
            {
                this.m_embeddedObjects = value;
            }
        }

        internal CanonicalXmlNodeList ReferencedItems
        {
            get
            {
                return this.m_referencedItems;
            }
        }

        public byte[] SignatureValue
        {
            get
            {
                return this.m_signatureValue;
            }
            set
            {
                this.m_signatureValue = value;
            }
        }

        public System.Security.Cryptography.Xml.SignedInfo SignedInfo
        {
            get
            {
                return this.m_signedInfo;
            }
            set
            {
                this.m_signedInfo = value;
                if ((this.SignedXml != null) && (this.m_signedInfo != null))
                {
                    this.m_signedInfo.SignedXml = this.SignedXml;
                }
            }
        }

        internal System.Security.Cryptography.Xml.SignedXml SignedXml
        {
            get
            {
                return this.m_signedXml;
            }
            set
            {
                this.m_signedXml = value;
            }
        }
    }
}

