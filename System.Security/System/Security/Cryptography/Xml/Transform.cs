namespace System.Security.Cryptography.Xml
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;
    using System.Security.Permissions;
    using System.Xml;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public abstract class Transform
    {
        private string m_algorithm;
        private string m_baseUri;
        private bool m_bResolverSet;
        private XmlElement m_context;
        private Hashtable m_propagatedNamespaces;
        private System.Security.Cryptography.Xml.Reference m_reference;
        private System.Security.Cryptography.Xml.SignedXml m_signedXml;
        internal XmlResolver m_xmlResolver;

        protected Transform()
        {
        }

        internal bool AcceptsType(Type inputType)
        {
            if (this.InputTypes != null)
            {
                for (int i = 0; i < this.InputTypes.Length; i++)
                {
                    if ((inputType == this.InputTypes[i]) || inputType.IsSubclassOf(this.InputTypes[i]))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        [ComVisible(false)]
        public virtual byte[] GetDigestedOutput(HashAlgorithm hash)
        {
            return hash.ComputeHash((Stream) this.GetOutput(typeof(Stream)));
        }

        protected abstract XmlNodeList GetInnerXml();
        public abstract object GetOutput();
        public abstract object GetOutput(Type type);
        public XmlElement GetXml()
        {
            XmlDocument document = new XmlDocument {
                PreserveWhitespace = true
            };
            return this.GetXml(document);
        }

        internal XmlElement GetXml(XmlDocument document)
        {
            return this.GetXml(document, "Transform");
        }

        internal XmlElement GetXml(XmlDocument document, string name)
        {
            XmlElement element = document.CreateElement(name, "http://www.w3.org/2000/09/xmldsig#");
            if (!string.IsNullOrEmpty(this.Algorithm))
            {
                element.SetAttribute("Algorithm", this.Algorithm);
            }
            XmlNodeList innerXml = this.GetInnerXml();
            if (innerXml != null)
            {
                foreach (XmlNode node in innerXml)
                {
                    element.AppendChild(document.ImportNode(node, true));
                }
            }
            return element;
        }

        public abstract void LoadInnerXml(XmlNodeList nodeList);
        public abstract void LoadInput(object obj);

        public string Algorithm
        {
            get
            {
                return this.m_algorithm;
            }
            set
            {
                this.m_algorithm = value;
            }
        }

        internal string BaseURI
        {
            get
            {
                return this.m_baseUri;
            }
            set
            {
                this.m_baseUri = value;
            }
        }

        [ComVisible(false)]
        public XmlElement Context
        {
            get
            {
                if (this.m_context != null)
                {
                    return this.m_context;
                }
                System.Security.Cryptography.Xml.Reference reference = this.Reference;
                System.Security.Cryptography.Xml.SignedXml xml = (reference == null) ? this.SignedXml : reference.SignedXml;
                if (xml == null)
                {
                    return null;
                }
                return xml.m_context;
            }
            set
            {
                this.m_context = value;
            }
        }

        public abstract Type[] InputTypes { get; }

        public abstract Type[] OutputTypes { get; }

        [ComVisible(false)]
        public Hashtable PropagatedNamespaces
        {
            get
            {
                if (this.m_propagatedNamespaces == null)
                {
                    System.Security.Cryptography.Xml.Reference reference = this.Reference;
                    System.Security.Cryptography.Xml.SignedXml xml = (reference == null) ? this.SignedXml : reference.SignedXml;
                    if ((reference != null) && (((reference.ReferenceTargetType != ReferenceTargetType.UriReference) || (reference.Uri == null)) || ((reference.Uri.Length == 0) || (reference.Uri[0] != '#'))))
                    {
                        this.m_propagatedNamespaces = new Hashtable(0);
                        return this.m_propagatedNamespaces;
                    }
                    CanonicalXmlNodeList namespaces = null;
                    if (reference != null)
                    {
                        namespaces = reference.m_namespaces;
                    }
                    else if (xml.m_context != null)
                    {
                        namespaces = System.Security.Cryptography.Xml.Utils.GetPropagatedAttributes(xml.m_context);
                    }
                    if (namespaces == null)
                    {
                        this.m_propagatedNamespaces = new Hashtable(0);
                        return this.m_propagatedNamespaces;
                    }
                    this.m_propagatedNamespaces = new Hashtable(namespaces.Count);
                    foreach (XmlNode node in namespaces)
                    {
                        string key = (node.Prefix.Length > 0) ? (node.Prefix + ":" + node.LocalName) : node.LocalName;
                        if (!this.m_propagatedNamespaces.Contains(key))
                        {
                            this.m_propagatedNamespaces.Add(key, node.Value);
                        }
                    }
                }
                return this.m_propagatedNamespaces;
            }
        }

        internal System.Security.Cryptography.Xml.Reference Reference
        {
            get
            {
                return this.m_reference;
            }
            set
            {
                this.m_reference = value;
            }
        }

        [ComVisible(false)]
        public XmlResolver Resolver
        {
            internal get
            {
                return this.m_xmlResolver;
            }
            set
            {
                this.m_xmlResolver = value;
                this.m_bResolverSet = true;
            }
        }

        internal bool ResolverSet
        {
            get
            {
                return this.m_bResolverSet;
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

