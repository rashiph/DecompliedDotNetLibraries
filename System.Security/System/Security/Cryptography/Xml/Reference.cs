namespace System.Security.Cryptography.Xml
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Cryptography;
    using System.Security.Permissions;
    using System.Xml;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public class Reference
    {
        private XmlElement m_cachedXml;
        private string m_digestMethod;
        private byte[] m_digestValue;
        private HashAlgorithm m_hashAlgorithm;
        private string m_id;
        internal CanonicalXmlNodeList m_namespaces;
        private object m_refTarget;
        private System.Security.Cryptography.Xml.ReferenceTargetType m_refTargetType;
        private System.Security.Cryptography.Xml.SignedXml m_signedXml;
        private System.Security.Cryptography.Xml.TransformChain m_transformChain;
        private string m_type;
        private string m_uri;

        public Reference()
        {
            this.m_transformChain = new System.Security.Cryptography.Xml.TransformChain();
            this.m_refTarget = null;
            this.m_refTargetType = System.Security.Cryptography.Xml.ReferenceTargetType.UriReference;
            this.m_cachedXml = null;
            this.m_digestMethod = "http://www.w3.org/2000/09/xmldsig#sha1";
        }

        public Reference(Stream stream)
        {
            this.m_transformChain = new System.Security.Cryptography.Xml.TransformChain();
            this.m_refTarget = stream;
            this.m_refTargetType = System.Security.Cryptography.Xml.ReferenceTargetType.Stream;
            this.m_cachedXml = null;
            this.m_digestMethod = "http://www.w3.org/2000/09/xmldsig#sha1";
        }

        public Reference(string uri)
        {
            this.m_transformChain = new System.Security.Cryptography.Xml.TransformChain();
            this.m_refTarget = uri;
            this.m_uri = uri;
            this.m_refTargetType = System.Security.Cryptography.Xml.ReferenceTargetType.UriReference;
            this.m_cachedXml = null;
            this.m_digestMethod = "http://www.w3.org/2000/09/xmldsig#sha1";
        }

        internal Reference(XmlElement element)
        {
            this.m_transformChain = new System.Security.Cryptography.Xml.TransformChain();
            this.m_refTarget = element;
            this.m_refTargetType = System.Security.Cryptography.Xml.ReferenceTargetType.XmlElement;
            this.m_cachedXml = null;
            this.m_digestMethod = "http://www.w3.org/2000/09/xmldsig#sha1";
        }

        public void AddTransform(Transform transform)
        {
            if (transform == null)
            {
                throw new ArgumentNullException("transform");
            }
            transform.Reference = this;
            this.TransformChain.Add(transform);
        }

        internal byte[] CalculateHashValue(XmlDocument document, CanonicalXmlNodeList refList)
        {
            this.m_hashAlgorithm = CryptoConfig.CreateFromName(this.m_digestMethod) as HashAlgorithm;
            if (this.m_hashAlgorithm == null)
            {
                throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_CreateHashAlgorithmFailed"));
            }
            string securityUrl = (document == null) ? (Environment.CurrentDirectory + @"\") : document.BaseURI;
            Stream data = null;
            WebRequest request = null;
            WebResponse response = null;
            Stream input = null;
            XmlResolver resolver = null;
            byte[] buffer = null;
            try
            {
                switch (this.m_refTargetType)
                {
                    case System.Security.Cryptography.Xml.ReferenceTargetType.Stream:
                        resolver = this.SignedXml.ResolverSet ? this.SignedXml.m_xmlResolver : new XmlSecureResolver(new XmlUrlResolver(), securityUrl);
                        data = this.TransformChain.TransformToOctetStream((Stream) this.m_refTarget, resolver, securityUrl);
                        goto Label_048A;

                    case System.Security.Cryptography.Xml.ReferenceTargetType.XmlElement:
                        resolver = this.SignedXml.ResolverSet ? this.SignedXml.m_xmlResolver : new XmlSecureResolver(new XmlUrlResolver(), securityUrl);
                        data = this.TransformChain.TransformToOctetStream(System.Security.Cryptography.Xml.Utils.PreProcessElementInput((XmlElement) this.m_refTarget, resolver, securityUrl), resolver, securityUrl);
                        goto Label_048A;

                    case System.Security.Cryptography.Xml.ReferenceTargetType.UriReference:
                        if (this.m_uri != null)
                        {
                            break;
                        }
                        resolver = this.SignedXml.ResolverSet ? this.SignedXml.m_xmlResolver : new XmlSecureResolver(new XmlUrlResolver(), securityUrl);
                        data = this.TransformChain.TransformToOctetStream((Stream) null, resolver, securityUrl);
                        goto Label_048A;

                    default:
                        goto Label_0474;
                }
                if (this.m_uri.Length == 0)
                {
                    if (document == null)
                    {
                        throw new CryptographicException(string.Format(CultureInfo.CurrentCulture, SecurityResources.GetResourceString("Cryptography_Xml_SelfReferenceRequiresContext"), new object[] { this.m_uri }));
                    }
                    resolver = this.SignedXml.ResolverSet ? this.SignedXml.m_xmlResolver : new XmlSecureResolver(new XmlUrlResolver(), securityUrl);
                    XmlDocument document2 = System.Security.Cryptography.Xml.Utils.DiscardComments(System.Security.Cryptography.Xml.Utils.PreProcessDocumentInput(document, resolver, securityUrl));
                    data = this.TransformChain.TransformToOctetStream(document2, resolver, securityUrl);
                    goto Label_048A;
                }
                if (this.m_uri[0] == '#')
                {
                    bool discardComments = true;
                    string idFromLocalUri = System.Security.Cryptography.Xml.Utils.GetIdFromLocalUri(this.m_uri, out discardComments);
                    if (idFromLocalUri == "xpointer(/)")
                    {
                        if (document == null)
                        {
                            throw new CryptographicException(string.Format(CultureInfo.CurrentCulture, SecurityResources.GetResourceString("Cryptography_Xml_SelfReferenceRequiresContext"), new object[] { this.m_uri }));
                        }
                        resolver = this.SignedXml.ResolverSet ? this.SignedXml.m_xmlResolver : new XmlSecureResolver(new XmlUrlResolver(), securityUrl);
                        data = this.TransformChain.TransformToOctetStream(System.Security.Cryptography.Xml.Utils.PreProcessDocumentInput(document, resolver, securityUrl), resolver, securityUrl);
                    }
                    else
                    {
                        XmlElement idElement = this.SignedXml.GetIdElement(document, idFromLocalUri);
                        if (idElement != null)
                        {
                            this.m_namespaces = System.Security.Cryptography.Xml.Utils.GetPropagatedAttributes(idElement.ParentNode as XmlElement);
                        }
                        if ((idElement == null) && (refList != null))
                        {
                            foreach (XmlNode node in refList)
                            {
                                XmlElement element = node as XmlElement;
                                if (((element != null) && System.Security.Cryptography.Xml.Utils.HasAttribute(element, "Id", "http://www.w3.org/2000/09/xmldsig#")) && System.Security.Cryptography.Xml.Utils.GetAttribute(element, "Id", "http://www.w3.org/2000/09/xmldsig#").Equals(idFromLocalUri))
                                {
                                    idElement = element;
                                    if (this.m_signedXml.m_context != null)
                                    {
                                        this.m_namespaces = System.Security.Cryptography.Xml.Utils.GetPropagatedAttributes(this.m_signedXml.m_context);
                                    }
                                    break;
                                }
                            }
                        }
                        if (idElement == null)
                        {
                            throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_InvalidReference"));
                        }
                        XmlDocument document3 = System.Security.Cryptography.Xml.Utils.PreProcessElementInput(idElement, resolver, securityUrl);
                        System.Security.Cryptography.Xml.Utils.AddNamespaces(document3.DocumentElement, this.m_namespaces);
                        resolver = this.SignedXml.ResolverSet ? this.SignedXml.m_xmlResolver : new XmlSecureResolver(new XmlUrlResolver(), securityUrl);
                        if (discardComments)
                        {
                            XmlDocument document4 = System.Security.Cryptography.Xml.Utils.DiscardComments(document3);
                            data = this.TransformChain.TransformToOctetStream(document4, resolver, securityUrl);
                        }
                        else
                        {
                            data = this.TransformChain.TransformToOctetStream(document3, resolver, securityUrl);
                        }
                    }
                    goto Label_048A;
                }
                System.Uri relativeUri = new System.Uri(this.m_uri, UriKind.RelativeOrAbsolute);
                if (!relativeUri.IsAbsoluteUri)
                {
                    relativeUri = new System.Uri(new System.Uri(securityUrl), relativeUri);
                }
                request = WebRequest.Create(relativeUri);
                if (request != null)
                {
                    response = request.GetResponse();
                    if (response != null)
                    {
                        input = response.GetResponseStream();
                        if (input != null)
                        {
                            resolver = this.SignedXml.ResolverSet ? this.SignedXml.m_xmlResolver : new XmlSecureResolver(new XmlUrlResolver(), securityUrl);
                            data = this.TransformChain.TransformToOctetStream(input, resolver, this.m_uri);
                            goto Label_048A;
                        }
                    }
                }
            Label_0474:
                throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_UriNotResolved"), this.m_uri);
            Label_048A:
                data = SignedXmlDebugLog.LogReferenceData(this, data);
                buffer = this.m_hashAlgorithm.ComputeHash(data);
            }
            finally
            {
                if (data != null)
                {
                    data.Close();
                }
                if (response != null)
                {
                    response.Close();
                }
                if (input != null)
                {
                    input.Close();
                }
            }
            return buffer;
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
            XmlElement element = document.CreateElement("Reference", "http://www.w3.org/2000/09/xmldsig#");
            if (!string.IsNullOrEmpty(this.m_id))
            {
                element.SetAttribute("Id", this.m_id);
            }
            if (this.m_uri != null)
            {
                element.SetAttribute("URI", this.m_uri);
            }
            if (!string.IsNullOrEmpty(this.m_type))
            {
                element.SetAttribute("Type", this.m_type);
            }
            if (this.TransformChain.Count != 0)
            {
                element.AppendChild(this.TransformChain.GetXml(document, "http://www.w3.org/2000/09/xmldsig#"));
            }
            if (string.IsNullOrEmpty(this.m_digestMethod))
            {
                throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_DigestMethodRequired"));
            }
            XmlElement newChild = document.CreateElement("DigestMethod", "http://www.w3.org/2000/09/xmldsig#");
            newChild.SetAttribute("Algorithm", this.m_digestMethod);
            element.AppendChild(newChild);
            if (this.DigestValue == null)
            {
                if (this.m_hashAlgorithm.Hash == null)
                {
                    throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_DigestValueRequired"));
                }
                this.DigestValue = this.m_hashAlgorithm.Hash;
            }
            XmlElement element3 = document.CreateElement("DigestValue", "http://www.w3.org/2000/09/xmldsig#");
            element3.AppendChild(document.CreateTextNode(Convert.ToBase64String(this.m_digestValue)));
            element.AppendChild(element3);
            return element;
        }

        public void LoadXml(XmlElement value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            this.m_id = System.Security.Cryptography.Xml.Utils.GetAttribute(value, "Id", "http://www.w3.org/2000/09/xmldsig#");
            this.m_uri = System.Security.Cryptography.Xml.Utils.GetAttribute(value, "URI", "http://www.w3.org/2000/09/xmldsig#");
            this.m_type = System.Security.Cryptography.Xml.Utils.GetAttribute(value, "Type", "http://www.w3.org/2000/09/xmldsig#");
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(value.OwnerDocument.NameTable);
            nsmgr.AddNamespace("ds", "http://www.w3.org/2000/09/xmldsig#");
            this.TransformChain = new System.Security.Cryptography.Xml.TransformChain();
            XmlElement element = value.SelectSingleNode("ds:Transforms", nsmgr) as XmlElement;
            if (element != null)
            {
                XmlNodeList list = element.SelectNodes("ds:Transform", nsmgr);
                if (list != null)
                {
                    foreach (XmlNode node in list)
                    {
                        XmlElement element2 = node as XmlElement;
                        Transform transform = CryptoConfig.CreateFromName(System.Security.Cryptography.Xml.Utils.GetAttribute(element2, "Algorithm", "http://www.w3.org/2000/09/xmldsig#")) as Transform;
                        if (transform == null)
                        {
                            throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_UnknownTransform"));
                        }
                        this.AddTransform(transform);
                        transform.LoadInnerXml(element2.ChildNodes);
                        if (transform is XmlDsigEnvelopedSignatureTransform)
                        {
                            XmlNode node2 = element2.SelectSingleNode("ancestor::ds:Signature[1]", nsmgr);
                            XmlNodeList list2 = element2.SelectNodes("//ds:Signature", nsmgr);
                            if (list2 != null)
                            {
                                int num = 0;
                                foreach (XmlNode node3 in list2)
                                {
                                    num++;
                                    if (node3 == node2)
                                    {
                                        ((XmlDsigEnvelopedSignatureTransform) transform).SignaturePosition = num;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            XmlElement element3 = value.SelectSingleNode("ds:DigestMethod", nsmgr) as XmlElement;
            if (element3 == null)
            {
                throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_InvalidElement"), "Reference/DigestMethod");
            }
            this.m_digestMethod = System.Security.Cryptography.Xml.Utils.GetAttribute(element3, "Algorithm", "http://www.w3.org/2000/09/xmldsig#");
            XmlElement element4 = value.SelectSingleNode("ds:DigestValue", nsmgr) as XmlElement;
            if (element4 == null)
            {
                throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_InvalidElement"), "Reference/DigestValue");
            }
            this.m_digestValue = Convert.FromBase64String(System.Security.Cryptography.Xml.Utils.DiscardWhiteSpaces(element4.InnerText));
            this.m_cachedXml = value;
        }

        internal void UpdateHashValue(XmlDocument document, CanonicalXmlNodeList refList)
        {
            this.DigestValue = this.CalculateHashValue(document, refList);
        }

        internal bool CacheValid
        {
            get
            {
                return (this.m_cachedXml != null);
            }
        }

        public string DigestMethod
        {
            get
            {
                return this.m_digestMethod;
            }
            set
            {
                this.m_digestMethod = value;
                this.m_cachedXml = null;
            }
        }

        public byte[] DigestValue
        {
            get
            {
                return this.m_digestValue;
            }
            set
            {
                this.m_digestValue = value;
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
            }
        }

        internal System.Security.Cryptography.Xml.ReferenceTargetType ReferenceTargetType
        {
            get
            {
                return this.m_refTargetType;
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
            [ComVisible(false)]
            set
            {
                this.m_transformChain = value;
                this.m_cachedXml = null;
            }
        }

        public string Type
        {
            get
            {
                return this.m_type;
            }
            set
            {
                this.m_type = value;
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
                this.m_uri = value;
                this.m_cachedXml = null;
            }
        }
    }
}

