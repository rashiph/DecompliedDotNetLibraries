namespace System.Security.Cryptography.Xml
{
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Permissions;
    using System.Xml;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public class KeyInfoX509Data : KeyInfoClause
    {
        private ArrayList m_certificates;
        private byte[] m_CRL;
        private ArrayList m_issuerSerials;
        private ArrayList m_subjectKeyIds;
        private ArrayList m_subjectNames;

        public KeyInfoX509Data()
        {
        }

        public KeyInfoX509Data(byte[] rgbCert)
        {
            X509Certificate2 certificate = new X509Certificate2(rgbCert);
            this.AddCertificate(certificate);
        }

        public KeyInfoX509Data(X509Certificate cert)
        {
            this.AddCertificate(cert);
        }

        [SecuritySafeCritical]
        public KeyInfoX509Data(X509Certificate cert, X509IncludeOption includeOption)
        {
            if (cert == null)
            {
                throw new ArgumentNullException("cert");
            }
            X509Certificate2 certificate = new X509Certificate2(cert);
            X509ChainElementCollection chainElements = null;
            X509Chain chain = null;
            switch (includeOption)
            {
                case X509IncludeOption.ExcludeRoot:
                    chain = new X509Chain();
                    chain.Build(certificate);
                    if ((chain.ChainStatus.Length > 0) && ((chain.ChainStatus[0].Status & X509ChainStatusFlags.PartialChain) == X509ChainStatusFlags.PartialChain))
                    {
                        throw new CryptographicException(-2146762486);
                    }
                    chainElements = chain.ChainElements;
                    for (int i = 0; i < (System.Security.Cryptography.X509Certificates.X509Utils.IsSelfSigned(chain) ? 1 : (chainElements.Count - 1)); i++)
                    {
                        this.AddCertificate(chainElements[i].Certificate);
                    }
                    return;

                case X509IncludeOption.EndCertOnly:
                    this.AddCertificate(certificate);
                    return;

                case X509IncludeOption.WholeChain:
                {
                    chain = new X509Chain();
                    chain.Build(certificate);
                    if ((chain.ChainStatus.Length > 0) && ((chain.ChainStatus[0].Status & X509ChainStatusFlags.PartialChain) == X509ChainStatusFlags.PartialChain))
                    {
                        throw new CryptographicException(-2146762486);
                    }
                    X509ChainElementEnumerator enumerator = chain.ChainElements.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        X509ChainElement current = enumerator.Current;
                        this.AddCertificate(current.Certificate);
                    }
                    return;
                }
            }
        }

        public void AddCertificate(X509Certificate certificate)
        {
            if (certificate == null)
            {
                throw new ArgumentNullException("certificate");
            }
            if (this.m_certificates == null)
            {
                this.m_certificates = new ArrayList();
            }
            X509Certificate2 certificate2 = new X509Certificate2(certificate);
            this.m_certificates.Add(certificate2);
        }

        public void AddIssuerSerial(string issuerName, string serialNumber)
        {
            System.Security.Cryptography.BigInt num = new System.Security.Cryptography.BigInt();
            num.FromHexadecimal(serialNumber);
            if (this.m_issuerSerials == null)
            {
                this.m_issuerSerials = new ArrayList();
            }
            this.m_issuerSerials.Add(new X509IssuerSerial(issuerName, num.ToDecimal()));
        }

        public void AddSubjectKeyId(byte[] subjectKeyId)
        {
            if (this.m_subjectKeyIds == null)
            {
                this.m_subjectKeyIds = new ArrayList();
            }
            this.m_subjectKeyIds.Add(subjectKeyId);
        }

        [ComVisible(false)]
        public void AddSubjectKeyId(string subjectKeyId)
        {
            if (this.m_subjectKeyIds == null)
            {
                this.m_subjectKeyIds = new ArrayList();
            }
            this.m_subjectKeyIds.Add(System.Security.Cryptography.X509Certificates.X509Utils.DecodeHexString(subjectKeyId));
        }

        public void AddSubjectName(string subjectName)
        {
            if (this.m_subjectNames == null)
            {
                this.m_subjectNames = new ArrayList();
            }
            this.m_subjectNames.Add(subjectName);
        }

        private void Clear()
        {
            this.m_CRL = null;
            if (this.m_subjectKeyIds != null)
            {
                this.m_subjectKeyIds.Clear();
            }
            if (this.m_subjectNames != null)
            {
                this.m_subjectNames.Clear();
            }
            if (this.m_issuerSerials != null)
            {
                this.m_issuerSerials.Clear();
            }
            if (this.m_certificates != null)
            {
                this.m_certificates.Clear();
            }
        }

        public override XmlElement GetXml()
        {
            XmlDocument xmlDocument = new XmlDocument {
                PreserveWhitespace = true
            };
            return this.GetXml(xmlDocument);
        }

        internal override XmlElement GetXml(XmlDocument xmlDocument)
        {
            XmlElement element = xmlDocument.CreateElement("X509Data", "http://www.w3.org/2000/09/xmldsig#");
            if (this.m_issuerSerials != null)
            {
                foreach (X509IssuerSerial serial in this.m_issuerSerials)
                {
                    XmlElement newChild = xmlDocument.CreateElement("X509IssuerSerial", "http://www.w3.org/2000/09/xmldsig#");
                    XmlElement element3 = xmlDocument.CreateElement("X509IssuerName", "http://www.w3.org/2000/09/xmldsig#");
                    element3.AppendChild(xmlDocument.CreateTextNode(serial.IssuerName));
                    newChild.AppendChild(element3);
                    XmlElement element4 = xmlDocument.CreateElement("X509SerialNumber", "http://www.w3.org/2000/09/xmldsig#");
                    element4.AppendChild(xmlDocument.CreateTextNode(serial.SerialNumber));
                    newChild.AppendChild(element4);
                    element.AppendChild(newChild);
                }
            }
            if (this.m_subjectKeyIds != null)
            {
                foreach (byte[] buffer in this.m_subjectKeyIds)
                {
                    XmlElement element5 = xmlDocument.CreateElement("X509SKI", "http://www.w3.org/2000/09/xmldsig#");
                    element5.AppendChild(xmlDocument.CreateTextNode(Convert.ToBase64String(buffer)));
                    element.AppendChild(element5);
                }
            }
            if (this.m_subjectNames != null)
            {
                foreach (string str in this.m_subjectNames)
                {
                    XmlElement element6 = xmlDocument.CreateElement("X509SubjectName", "http://www.w3.org/2000/09/xmldsig#");
                    element6.AppendChild(xmlDocument.CreateTextNode(str));
                    element.AppendChild(element6);
                }
            }
            if (this.m_certificates != null)
            {
                foreach (X509Certificate certificate in this.m_certificates)
                {
                    XmlElement element7 = xmlDocument.CreateElement("X509Certificate", "http://www.w3.org/2000/09/xmldsig#");
                    element7.AppendChild(xmlDocument.CreateTextNode(Convert.ToBase64String(certificate.GetRawCertData())));
                    element.AppendChild(element7);
                }
            }
            if (this.m_CRL != null)
            {
                XmlElement element8 = xmlDocument.CreateElement("X509CRL", "http://www.w3.org/2000/09/xmldsig#");
                element8.AppendChild(xmlDocument.CreateTextNode(Convert.ToBase64String(this.m_CRL)));
                element.AppendChild(element8);
            }
            return element;
        }

        internal void InternalAddIssuerSerial(string issuerName, string serialNumber)
        {
            if (this.m_issuerSerials == null)
            {
                this.m_issuerSerials = new ArrayList();
            }
            this.m_issuerSerials.Add(new X509IssuerSerial(issuerName, serialNumber));
        }

        public override void LoadXml(XmlElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(element.OwnerDocument.NameTable);
            nsmgr.AddNamespace("ds", "http://www.w3.org/2000/09/xmldsig#");
            XmlNodeList list = element.SelectNodes("ds:X509IssuerSerial", nsmgr);
            XmlNodeList list2 = element.SelectNodes("ds:X509SKI", nsmgr);
            XmlNodeList list3 = element.SelectNodes("ds:X509SubjectName", nsmgr);
            XmlNodeList list4 = element.SelectNodes("ds:X509Certificate", nsmgr);
            XmlNodeList list5 = element.SelectNodes("ds:X509CRL", nsmgr);
            if ((((list5.Count == 0) && (list.Count == 0)) && ((list2.Count == 0) && (list3.Count == 0))) && (list4.Count == 0))
            {
                throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_InvalidElement"), "X509Data");
            }
            this.Clear();
            if (list5.Count != 0)
            {
                this.m_CRL = Convert.FromBase64String(System.Security.Cryptography.Xml.Utils.DiscardWhiteSpaces(list5.Item(0).InnerText));
            }
            foreach (XmlNode node in list)
            {
                XmlNode node2 = node.SelectSingleNode("ds:X509IssuerName", nsmgr);
                XmlNode node3 = node.SelectSingleNode("ds:X509SerialNumber", nsmgr);
                if ((node2 == null) || (node3 == null))
                {
                    throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_InvalidElement"), "IssuerSerial");
                }
                this.InternalAddIssuerSerial(node2.InnerText.Trim(), node3.InnerText.Trim());
            }
            foreach (XmlNode node4 in list2)
            {
                this.AddSubjectKeyId(Convert.FromBase64String(System.Security.Cryptography.Xml.Utils.DiscardWhiteSpaces(node4.InnerText)));
            }
            foreach (XmlNode node5 in list3)
            {
                this.AddSubjectName(node5.InnerText.Trim());
            }
            foreach (XmlNode node6 in list4)
            {
                this.AddCertificate(new X509Certificate2(Convert.FromBase64String(System.Security.Cryptography.Xml.Utils.DiscardWhiteSpaces(node6.InnerText))));
            }
        }

        public ArrayList Certificates
        {
            get
            {
                return this.m_certificates;
            }
        }

        public byte[] CRL
        {
            get
            {
                return this.m_CRL;
            }
            set
            {
                this.m_CRL = value;
            }
        }

        public ArrayList IssuerSerials
        {
            get
            {
                return this.m_issuerSerials;
            }
        }

        public ArrayList SubjectKeyIds
        {
            get
            {
                return this.m_subjectKeyIds;
            }
        }

        public ArrayList SubjectNames
        {
            get
            {
                return this.m_subjectNames;
            }
        }
    }
}

