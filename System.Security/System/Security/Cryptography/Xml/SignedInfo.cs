namespace System.Security.Cryptography.Xml
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Cryptography;
    using System.Security.Permissions;
    using System.Xml;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public class SignedInfo : ICollection, IEnumerable
    {
        private XmlElement m_cachedXml;
        private string m_canonicalizationMethod;
        private Transform m_canonicalizationMethodTransform;
        private string m_id;
        private ArrayList m_references = new ArrayList();
        private string m_signatureLength;
        private string m_signatureMethod;
        private System.Security.Cryptography.Xml.SignedXml m_signedXml;

        public void AddReference(Reference reference)
        {
            if (reference == null)
            {
                throw new ArgumentNullException("reference");
            }
            reference.SignedXml = this.SignedXml;
            this.m_references.Add(reference);
        }

        public void CopyTo(Array array, int index)
        {
            throw new NotSupportedException();
        }

        public IEnumerator GetEnumerator()
        {
            throw new NotSupportedException();
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
            XmlElement element = document.CreateElement("SignedInfo", "http://www.w3.org/2000/09/xmldsig#");
            if (!string.IsNullOrEmpty(this.m_id))
            {
                element.SetAttribute("Id", this.m_id);
            }
            XmlElement xml = this.CanonicalizationMethodObject.GetXml(document, "CanonicalizationMethod");
            element.AppendChild(xml);
            if (string.IsNullOrEmpty(this.m_signatureMethod))
            {
                throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_SignatureMethodRequired"));
            }
            XmlElement newChild = document.CreateElement("SignatureMethod", "http://www.w3.org/2000/09/xmldsig#");
            newChild.SetAttribute("Algorithm", this.m_signatureMethod);
            if (this.m_signatureLength != null)
            {
                XmlElement element4 = document.CreateElement(null, "HMACOutputLength", "http://www.w3.org/2000/09/xmldsig#");
                XmlText text = document.CreateTextNode(this.m_signatureLength);
                element4.AppendChild(text);
                newChild.AppendChild(element4);
            }
            element.AppendChild(newChild);
            if (this.m_references.Count == 0)
            {
                throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_ReferenceElementRequired"));
            }
            for (int i = 0; i < this.m_references.Count; i++)
            {
                Reference reference = (Reference) this.m_references[i];
                element.AppendChild(reference.GetXml(document));
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
            if (!element.LocalName.Equals("SignedInfo"))
            {
                throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_InvalidElement"), "SignedInfo");
            }
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(value.OwnerDocument.NameTable);
            nsmgr.AddNamespace("ds", "http://www.w3.org/2000/09/xmldsig#");
            this.m_id = System.Security.Cryptography.Xml.Utils.GetAttribute(element, "Id", "http://www.w3.org/2000/09/xmldsig#");
            XmlElement element2 = element.SelectSingleNode("ds:CanonicalizationMethod", nsmgr) as XmlElement;
            if (element2 == null)
            {
                throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_InvalidElement"), "SignedInfo/CanonicalizationMethod");
            }
            this.m_canonicalizationMethod = System.Security.Cryptography.Xml.Utils.GetAttribute(element2, "Algorithm", "http://www.w3.org/2000/09/xmldsig#");
            this.m_canonicalizationMethodTransform = null;
            if (element2.ChildNodes.Count > 0)
            {
                this.CanonicalizationMethodObject.LoadInnerXml(element2.ChildNodes);
            }
            XmlElement element3 = element.SelectSingleNode("ds:SignatureMethod", nsmgr) as XmlElement;
            if (element3 == null)
            {
                throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_InvalidElement"), "SignedInfo/SignatureMethod");
            }
            this.m_signatureMethod = System.Security.Cryptography.Xml.Utils.GetAttribute(element3, "Algorithm", "http://www.w3.org/2000/09/xmldsig#");
            XmlElement element4 = element3.SelectSingleNode("ds:HMACOutputLength", nsmgr) as XmlElement;
            if (element4 != null)
            {
                this.m_signatureLength = element4.InnerXml;
            }
            this.m_references.Clear();
            XmlNodeList list = element.SelectNodes("ds:Reference", nsmgr);
            if (list != null)
            {
                foreach (XmlNode node in list)
                {
                    XmlElement element5 = node as XmlElement;
                    Reference reference = new Reference();
                    this.AddReference(reference);
                    reference.LoadXml(element5);
                }
            }
            this.m_cachedXml = element;
        }

        internal bool CacheValid
        {
            get
            {
                if (this.m_cachedXml == null)
                {
                    return false;
                }
                foreach (Reference reference in this.References)
                {
                    if (!reference.CacheValid)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public string CanonicalizationMethod
        {
            get
            {
                if (this.m_canonicalizationMethod == null)
                {
                    return "http://www.w3.org/TR/2001/REC-xml-c14n-20010315";
                }
                return this.m_canonicalizationMethod;
            }
            set
            {
                this.m_canonicalizationMethod = value;
                this.m_cachedXml = null;
            }
        }

        [ComVisible(false)]
        public Transform CanonicalizationMethodObject
        {
            get
            {
                if (this.m_canonicalizationMethodTransform == null)
                {
                    this.m_canonicalizationMethodTransform = CryptoConfig.CreateFromName(this.CanonicalizationMethod) as Transform;
                    if (this.m_canonicalizationMethodTransform == null)
                    {
                        throw new CryptographicException(string.Format(CultureInfo.CurrentCulture, SecurityResources.GetResourceString("Cryptography_Xml_CreateTransformFailed"), new object[] { this.CanonicalizationMethod }));
                    }
                    this.m_canonicalizationMethodTransform.SignedXml = this.SignedXml;
                    this.m_canonicalizationMethodTransform.Reference = null;
                }
                return this.m_canonicalizationMethodTransform;
            }
        }

        public int Count
        {
            get
            {
                throw new NotSupportedException();
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

        public bool IsReadOnly
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public bool IsSynchronized
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public ArrayList References
        {
            get
            {
                return this.m_references;
            }
        }

        public string SignatureLength
        {
            get
            {
                return this.m_signatureLength;
            }
            set
            {
                this.m_signatureLength = value;
                this.m_cachedXml = null;
            }
        }

        public string SignatureMethod
        {
            get
            {
                return this.m_signatureMethod;
            }
            set
            {
                this.m_signatureMethod = value;
                this.m_cachedXml = null;
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

        public object SyncRoot
        {
            get
            {
                throw new NotSupportedException();
            }
        }
    }
}

