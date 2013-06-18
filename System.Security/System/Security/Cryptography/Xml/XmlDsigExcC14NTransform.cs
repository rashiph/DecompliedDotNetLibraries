namespace System.Security.Cryptography.Xml
{
    using System;
    using System.IO;
    using System.Security;
    using System.Security.Cryptography;
    using System.Security.Permissions;
    using System.Xml;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public class XmlDsigExcC14NTransform : Transform
    {
        private ExcCanonicalXml _excCanonicalXml;
        private bool _includeComments;
        private string _inclusiveNamespacesPrefixList;
        private Type[] _inputTypes;
        private Type[] _outputTypes;

        public XmlDsigExcC14NTransform() : this(false, null)
        {
        }

        public XmlDsigExcC14NTransform(bool includeComments) : this(includeComments, null)
        {
        }

        public XmlDsigExcC14NTransform(string inclusiveNamespacesPrefixList) : this(false, inclusiveNamespacesPrefixList)
        {
        }

        public XmlDsigExcC14NTransform(bool includeComments, string inclusiveNamespacesPrefixList)
        {
            this._inputTypes = new Type[] { typeof(Stream), typeof(XmlDocument), typeof(XmlNodeList) };
            this._outputTypes = new Type[] { typeof(Stream) };
            this._includeComments = includeComments;
            this._inclusiveNamespacesPrefixList = inclusiveNamespacesPrefixList;
            base.Algorithm = includeComments ? "http://www.w3.org/2001/10/xml-exc-c14n#WithComments" : "http://www.w3.org/2001/10/xml-exc-c14n#";
        }

        public override byte[] GetDigestedOutput(HashAlgorithm hash)
        {
            return this._excCanonicalXml.GetDigestedBytes(hash);
        }

        protected override XmlNodeList GetInnerXml()
        {
            if (this.InclusiveNamespacesPrefixList == null)
            {
                return null;
            }
            XmlDocument document = new XmlDocument();
            XmlElement element = document.CreateElement("Transform", "http://www.w3.org/2000/09/xmldsig#");
            if (!string.IsNullOrEmpty(base.Algorithm))
            {
                element.SetAttribute("Algorithm", base.Algorithm);
            }
            XmlElement newChild = document.CreateElement("InclusiveNamespaces", "http://www.w3.org/2001/10/xml-exc-c14n#");
            newChild.SetAttribute("PrefixList", this.InclusiveNamespacesPrefixList);
            element.AppendChild(newChild);
            return element.ChildNodes;
        }

        public override object GetOutput()
        {
            return new MemoryStream(this._excCanonicalXml.GetBytes());
        }

        public override object GetOutput(Type type)
        {
            if ((type != typeof(Stream)) && !type.IsSubclassOf(typeof(Stream)))
            {
                throw new ArgumentException(SecurityResources.GetResourceString("Cryptography_Xml_TransformIncorrectInputType"), "type");
            }
            return new MemoryStream(this._excCanonicalXml.GetBytes());
        }

        public override void LoadInnerXml(XmlNodeList nodeList)
        {
            if (nodeList != null)
            {
                foreach (XmlNode node in nodeList)
                {
                    XmlElement element = node as XmlElement;
                    if (((element != null) && element.LocalName.Equals("InclusiveNamespaces")) && (element.NamespaceURI.Equals("http://www.w3.org/2001/10/xml-exc-c14n#") && System.Security.Cryptography.Xml.Utils.HasAttribute(element, "PrefixList", "http://www.w3.org/2000/09/xmldsig#")))
                    {
                        this.InclusiveNamespacesPrefixList = System.Security.Cryptography.Xml.Utils.GetAttribute(element, "PrefixList", "http://www.w3.org/2000/09/xmldsig#");
                        break;
                    }
                }
            }
        }

        public override void LoadInput(object obj)
        {
            XmlResolver resolver = base.ResolverSet ? base.m_xmlResolver : new XmlSecureResolver(new XmlUrlResolver(), base.BaseURI);
            if (obj is Stream)
            {
                this._excCanonicalXml = new ExcCanonicalXml((Stream) obj, this._includeComments, this._inclusiveNamespacesPrefixList, resolver, base.BaseURI);
            }
            else if (obj is XmlDocument)
            {
                this._excCanonicalXml = new ExcCanonicalXml((XmlDocument) obj, this._includeComments, this._inclusiveNamespacesPrefixList, resolver);
            }
            else
            {
                if (!(obj is XmlNodeList))
                {
                    throw new ArgumentException(SecurityResources.GetResourceString("Cryptography_Xml_IncorrectObjectType"), "obj");
                }
                this._excCanonicalXml = new ExcCanonicalXml((XmlNodeList) obj, this._includeComments, this._inclusiveNamespacesPrefixList, resolver);
            }
        }

        public string InclusiveNamespacesPrefixList
        {
            get
            {
                return this._inclusiveNamespacesPrefixList;
            }
            set
            {
                this._inclusiveNamespacesPrefixList = value;
            }
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
    }
}

