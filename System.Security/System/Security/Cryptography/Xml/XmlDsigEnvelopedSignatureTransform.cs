namespace System.Security.Cryptography.Xml
{
    using System;
    using System.IO;
    using System.Security;
    using System.Security.Cryptography;
    using System.Security.Permissions;
    using System.Xml;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public class XmlDsigEnvelopedSignatureTransform : Transform
    {
        private XmlDocument _containingDocument;
        private bool _includeComments;
        private XmlNodeList _inputNodeList;
        private Type[] _inputTypes;
        private XmlNamespaceManager _nsm;
        private Type[] _outputTypes;
        private int _signaturePosition;

        public XmlDsigEnvelopedSignatureTransform()
        {
            this._inputTypes = new Type[] { typeof(Stream), typeof(XmlNodeList), typeof(XmlDocument) };
            this._outputTypes = new Type[] { typeof(XmlNodeList), typeof(XmlDocument) };
            base.Algorithm = "http://www.w3.org/2000/09/xmldsig#enveloped-signature";
        }

        public XmlDsigEnvelopedSignatureTransform(bool includeComments)
        {
            this._inputTypes = new Type[] { typeof(Stream), typeof(XmlNodeList), typeof(XmlDocument) };
            this._outputTypes = new Type[] { typeof(XmlNodeList), typeof(XmlDocument) };
            this._includeComments = includeComments;
            base.Algorithm = "http://www.w3.org/2000/09/xmldsig#enveloped-signature";
        }

        protected override XmlNodeList GetInnerXml()
        {
            return null;
        }

        public override object GetOutput()
        {
            if (this._containingDocument == null)
            {
                throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_EnvelopedSignatureRequiresContext"));
            }
            if (this._inputNodeList != null)
            {
                if (this._signaturePosition == 0)
                {
                    return this._inputNodeList;
                }
                XmlNodeList list = this._containingDocument.SelectNodes("//dsig:Signature", this._nsm);
                if (list == null)
                {
                    return this._inputNodeList;
                }
                CanonicalXmlNodeList list2 = new CanonicalXmlNodeList();
                foreach (XmlNode node in this._inputNodeList)
                {
                    if (node != null)
                    {
                        if (System.Security.Cryptography.Xml.Utils.IsXmlNamespaceNode(node) || System.Security.Cryptography.Xml.Utils.IsNamespaceNode(node))
                        {
                            list2.Add(node);
                        }
                        else
                        {
                            try
                            {
                                XmlNode node2 = node.SelectSingleNode("ancestor-or-self::dsig:Signature[1]", this._nsm);
                                int num = 0;
                                foreach (XmlNode node3 in list)
                                {
                                    num++;
                                    if (node3 == node2)
                                    {
                                        break;
                                    }
                                }
                                if ((node2 == null) || ((node2 != null) && (num != this._signaturePosition)))
                                {
                                    list2.Add(node);
                                }
                            }
                            catch
                            {
                            }
                        }
                    }
                }
                return list2;
            }
            XmlNodeList list3 = this._containingDocument.SelectNodes("//dsig:Signature", this._nsm);
            if (list3 != null)
            {
                if ((list3.Count < this._signaturePosition) || (this._signaturePosition <= 0))
                {
                    return this._containingDocument;
                }
                list3[this._signaturePosition - 1].ParentNode.RemoveChild(list3[this._signaturePosition - 1]);
            }
            return this._containingDocument;
        }

        public override object GetOutput(Type type)
        {
            if ((type == typeof(XmlNodeList)) || type.IsSubclassOf(typeof(XmlNodeList)))
            {
                if (this._inputNodeList == null)
                {
                    this._inputNodeList = System.Security.Cryptography.Xml.Utils.AllDescendantNodes(this._containingDocument, true);
                }
                return (XmlNodeList) this.GetOutput();
            }
            if (!(type == typeof(XmlDocument)) && !type.IsSubclassOf(typeof(XmlDocument)))
            {
                throw new ArgumentException(SecurityResources.GetResourceString("Cryptography_Xml_TransformIncorrectInputType"), "type");
            }
            if (this._inputNodeList != null)
            {
                throw new ArgumentException(SecurityResources.GetResourceString("Cryptography_Xml_TransformIncorrectInputType"), "type");
            }
            return (XmlDocument) this.GetOutput();
        }

        public override void LoadInnerXml(XmlNodeList nodeList)
        {
        }

        public override void LoadInput(object obj)
        {
            if (obj is Stream)
            {
                this.LoadStreamInput((Stream) obj);
            }
            else if (obj is XmlNodeList)
            {
                this.LoadXmlNodeListInput((XmlNodeList) obj);
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
            this._containingDocument = document;
            if (this._containingDocument == null)
            {
                throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_EnvelopedSignatureRequiresContext"));
            }
            this._nsm = new XmlNamespaceManager(this._containingDocument.NameTable);
            this._nsm.AddNamespace("dsig", "http://www.w3.org/2000/09/xmldsig#");
        }

        private void LoadXmlDocumentInput(XmlDocument doc)
        {
            if (doc == null)
            {
                throw new ArgumentNullException("doc");
            }
            this._containingDocument = doc;
            this._nsm = new XmlNamespaceManager(this._containingDocument.NameTable);
            this._nsm.AddNamespace("dsig", "http://www.w3.org/2000/09/xmldsig#");
        }

        private void LoadXmlNodeListInput(XmlNodeList nodeList)
        {
            if (nodeList == null)
            {
                throw new ArgumentNullException("nodeList");
            }
            this._containingDocument = System.Security.Cryptography.Xml.Utils.GetOwnerDocument(nodeList);
            if (this._containingDocument == null)
            {
                throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_EnvelopedSignatureRequiresContext"));
            }
            this._nsm = new XmlNamespaceManager(this._containingDocument.NameTable);
            this._nsm.AddNamespace("dsig", "http://www.w3.org/2000/09/xmldsig#");
            this._inputNodeList = nodeList;
        }

        public override Type[] InputTypes
        {
            get
            {
                return this._inputTypes;
            }
        }

        public override Type[] OutputTypes
        {
            get
            {
                return this._outputTypes;
            }
        }

        internal int SignaturePosition
        {
            set
            {
                this._signaturePosition = value;
            }
        }
    }
}

