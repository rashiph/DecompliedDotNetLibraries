namespace System.Security.Cryptography.Xml
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Security;
    using System.Security.Cryptography;
    using System.Security.Permissions;
    using System.Xml;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public class XmlDecryptionTransform : Transform
    {
        private ArrayList m_arrayListUri;
        private XmlDocument m_containingDocument;
        private XmlNodeList m_encryptedDataList;
        private System.Security.Cryptography.Xml.EncryptedXml m_exml;
        private Type[] m_inputTypes = new Type[] { typeof(Stream), typeof(XmlDocument) };
        private XmlNamespaceManager m_nsm;
        private Type[] m_outputTypes = new Type[] { typeof(XmlDocument) };
        private const string XmlDecryptionTransformNamespaceUrl = "http://www.w3.org/2002/07/decrypt#";

        public XmlDecryptionTransform()
        {
            base.Algorithm = "http://www.w3.org/2002/07/decrypt#XML";
        }

        public void AddExceptUri(string uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }
            this.ExceptUris.Add(uri);
        }

        protected override XmlNodeList GetInnerXml()
        {
            if (this.ExceptUris.Count == 0)
            {
                return null;
            }
            XmlDocument document = new XmlDocument();
            XmlElement element = document.CreateElement("Transform", "http://www.w3.org/2000/09/xmldsig#");
            if (!string.IsNullOrEmpty(base.Algorithm))
            {
                element.SetAttribute("Algorithm", base.Algorithm);
            }
            foreach (string str in this.ExceptUris)
            {
                XmlElement newChild = document.CreateElement("Except", "http://www.w3.org/2002/07/decrypt#");
                newChild.SetAttribute("URI", str);
                element.AppendChild(newChild);
            }
            return element.ChildNodes;
        }

        public override object GetOutput()
        {
            if (this.m_encryptedDataList != null)
            {
                this.ProcessElementRecursively(this.m_encryptedDataList);
            }
            System.Security.Cryptography.Xml.Utils.AddNamespaces(this.m_containingDocument.DocumentElement, base.PropagatedNamespaces);
            return this.m_containingDocument;
        }

        public override object GetOutput(Type type)
        {
            if (type != typeof(XmlDocument))
            {
                throw new ArgumentException(SecurityResources.GetResourceString("Cryptography_Xml_TransformIncorrectInputType"), "type");
            }
            return (XmlDocument) this.GetOutput();
        }

        protected virtual bool IsTargetElement(XmlElement inputElement, string idValue)
        {
            if (inputElement == null)
            {
                return false;
            }
            if ((!(inputElement.GetAttribute("Id") == idValue) && !(inputElement.GetAttribute("id") == idValue)) && !(inputElement.GetAttribute("ID") == idValue))
            {
                return false;
            }
            return true;
        }

        public override void LoadInnerXml(XmlNodeList nodeList)
        {
            if (nodeList == null)
            {
                throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_UnknownTransform"));
            }
            this.ExceptUris.Clear();
            foreach (XmlNode node in nodeList)
            {
                XmlElement element = node as XmlElement;
                if (((element != null) && (element.LocalName == "Except")) && (element.NamespaceURI == "http://www.w3.org/2002/07/decrypt#"))
                {
                    string uri = System.Security.Cryptography.Xml.Utils.GetAttribute(element, "URI", "http://www.w3.org/2002/07/decrypt#");
                    if (((uri == null) || (uri.Length == 0)) || (uri[0] != '#'))
                    {
                        throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_UriRequired"));
                    }
                    string str2 = System.Security.Cryptography.Xml.Utils.ExtractIdFromLocalUri(uri);
                    this.ExceptUris.Add(str2);
                }
            }
        }

        public override void LoadInput(object obj)
        {
            if (obj is Stream)
            {
                this.LoadStreamInput((Stream) obj);
            }
            else if (obj is XmlDocument)
            {
                this.LoadXmlDocumentInput((XmlDocument) obj);
            }
        }

        private void LoadStreamInput(Stream stream)
        {
            XmlDocument document = new XmlDocument {
                PreserveWhitespace = true
            };
            document.Load(stream);
            this.m_containingDocument = document;
            this.m_nsm = new XmlNamespaceManager(this.m_containingDocument.NameTable);
            this.m_nsm.AddNamespace("enc", "http://www.w3.org/2001/04/xmlenc#");
            this.m_encryptedDataList = document.SelectNodes("//enc:EncryptedData", this.m_nsm);
        }

        private void LoadXmlDocumentInput(XmlDocument document)
        {
            if (document == null)
            {
                throw new ArgumentNullException("document");
            }
            this.m_containingDocument = document;
            this.m_nsm = new XmlNamespaceManager(document.NameTable);
            this.m_nsm.AddNamespace("enc", "http://www.w3.org/2001/04/xmlenc#");
            this.m_encryptedDataList = document.SelectNodes("//enc:EncryptedData", this.m_nsm);
        }

        private void ProcessElementRecursively(XmlNodeList encryptedDatas)
        {
            if ((encryptedDatas != null) && (encryptedDatas.Count != 0))
            {
                Queue queue = new Queue();
                foreach (XmlNode node in encryptedDatas)
                {
                    queue.Enqueue(node);
                }
                for (XmlNode node2 = queue.Dequeue() as XmlNode; node2 != null; node2 = queue.Dequeue() as XmlNode)
                {
                    XmlElement encryptedDataElement = node2 as XmlElement;
                    if (((encryptedDataElement != null) && (encryptedDataElement.LocalName == "EncryptedData")) && (encryptedDataElement.NamespaceURI == "http://www.w3.org/2001/04/xmlenc#"))
                    {
                        XmlNode nextSibling = encryptedDataElement.NextSibling;
                        XmlNode parentNode = encryptedDataElement.ParentNode;
                        if (this.ProcessEncryptedDataItem(encryptedDataElement))
                        {
                            XmlNode firstChild = parentNode.FirstChild;
                            while ((firstChild != null) && (firstChild.NextSibling != nextSibling))
                            {
                                firstChild = firstChild.NextSibling;
                            }
                            if (firstChild != null)
                            {
                                XmlNodeList list = firstChild.SelectNodes("//enc:EncryptedData", this.m_nsm);
                                if (list.Count > 0)
                                {
                                    foreach (XmlNode node6 in list)
                                    {
                                        queue.Enqueue(node6);
                                    }
                                }
                            }
                        }
                    }
                    if (queue.Count == 0)
                    {
                        return;
                    }
                }
            }
        }

        private bool ProcessEncryptedDataItem(XmlElement encryptedDataElement)
        {
            if (this.ExceptUris.Count > 0)
            {
                for (int i = 0; i < this.ExceptUris.Count; i++)
                {
                    if (this.IsTargetElement(encryptedDataElement, (string) this.ExceptUris[i]))
                    {
                        return false;
                    }
                }
            }
            EncryptedData encryptedData = new EncryptedData();
            encryptedData.LoadXml(encryptedDataElement);
            SymmetricAlgorithm decryptionKey = this.EncryptedXml.GetDecryptionKey(encryptedData, null);
            if (decryptionKey == null)
            {
                throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_MissingDecryptionKey"));
            }
            byte[] decrypted = this.EncryptedXml.DecryptData(encryptedData, decryptionKey);
            this.ReplaceEncryptedData(encryptedDataElement, decrypted);
            return true;
        }

        private void ReplaceEncryptedData(XmlElement encryptedDataElement, byte[] decrypted)
        {
            XmlNode parentNode = encryptedDataElement.ParentNode;
            if (parentNode.NodeType == XmlNodeType.Document)
            {
                parentNode.InnerXml = this.EncryptedXml.Encoding.GetString(decrypted);
            }
            else
            {
                this.EncryptedXml.ReplaceData(encryptedDataElement, decrypted);
            }
        }

        public System.Security.Cryptography.Xml.EncryptedXml EncryptedXml
        {
            get
            {
                if (this.m_exml == null)
                {
                    Reference reference = base.Reference;
                    SignedXml xml = (reference == null) ? base.SignedXml : reference.SignedXml;
                    if ((xml == null) || (xml.EncryptedXml == null))
                    {
                        this.m_exml = new System.Security.Cryptography.Xml.EncryptedXml(this.m_containingDocument);
                    }
                    else
                    {
                        this.m_exml = xml.EncryptedXml;
                    }
                }
                return this.m_exml;
            }
            set
            {
                this.m_exml = value;
            }
        }

        private ArrayList ExceptUris
        {
            get
            {
                if (this.m_arrayListUri == null)
                {
                    this.m_arrayListUri = new ArrayList();
                }
                return this.m_arrayListUri;
            }
        }

        public override Type[] InputTypes
        {
            get
            {
                return this.m_inputTypes;
            }
        }

        public override Type[] OutputTypes
        {
            get
            {
                return this.m_outputTypes;
            }
        }
    }
}

