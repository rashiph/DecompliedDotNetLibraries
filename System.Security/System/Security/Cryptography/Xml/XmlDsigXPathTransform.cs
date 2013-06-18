namespace System.Security.Cryptography.Xml
{
    using System;
    using System.IO;
    using System.Security;
    using System.Security.Cryptography;
    using System.Security.Permissions;
    using System.Xml;
    using System.Xml.XPath;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public class XmlDsigXPathTransform : Transform
    {
        private XmlDocument _document;
        private Type[] _inputTypes = new Type[] { typeof(Stream), typeof(XmlNodeList), typeof(XmlDocument) };
        private XmlNamespaceManager _nsm;
        private Type[] _outputTypes = new Type[] { typeof(XmlNodeList) };
        private string _xpathexpr;

        public XmlDsigXPathTransform()
        {
            base.Algorithm = "http://www.w3.org/TR/1999/REC-xpath-19991116";
        }

        protected override XmlNodeList GetInnerXml()
        {
            XmlDocument document = new XmlDocument();
            XmlElement newChild = document.CreateElement(null, "XPath", "http://www.w3.org/2000/09/xmldsig#");
            if (this._nsm != null)
            {
                foreach (string str in this._nsm)
                {
                    string str2;
                    if ((((str2 = str) == null) || ((str2 != "xml") && (str2 != "xmlns"))) && ((str != null) && (str.Length > 0)))
                    {
                        newChild.SetAttribute("xmlns:" + str, this._nsm.LookupNamespace(str));
                    }
                }
            }
            newChild.InnerXml = this._xpathexpr;
            document.AppendChild(newChild);
            return document.ChildNodes;
        }

        public override object GetOutput()
        {
            CanonicalXmlNodeList list = new CanonicalXmlNodeList();
            if (!string.IsNullOrEmpty(this._xpathexpr))
            {
                XPathNavigator navigator = this._document.CreateNavigator();
                XPathNodeIterator iterator = navigator.Select("//. | //@*");
                XPathExpression expr = navigator.Compile("boolean(" + this._xpathexpr + ")");
                expr.SetContext(this._nsm);
                while (iterator.MoveNext())
                {
                    XmlNode node = ((IHasXmlNode) iterator.Current).GetNode();
                    if ((bool) iterator.Current.Evaluate(expr))
                    {
                        list.Add(node);
                    }
                }
                iterator = navigator.Select("//namespace::*");
                while (iterator.MoveNext())
                {
                    XmlNode node2 = ((IHasXmlNode) iterator.Current).GetNode();
                    list.Add(node2);
                }
            }
            return list;
        }

        public override object GetOutput(Type type)
        {
            if ((type != typeof(XmlNodeList)) && !type.IsSubclassOf(typeof(XmlNodeList)))
            {
                throw new ArgumentException(SecurityResources.GetResourceString("Cryptography_Xml_TransformIncorrectInputType"), "type");
            }
            return (XmlNodeList) this.GetOutput();
        }

        public override void LoadInnerXml(XmlNodeList nodeList)
        {
            if (nodeList == null)
            {
                throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_UnknownTransform"));
            }
            foreach (XmlNode node in nodeList)
            {
                string prefix = null;
                string uri = null;
                XmlElement element = node as XmlElement;
                if ((element != null) && (element.LocalName == "XPath"))
                {
                    this._xpathexpr = element.InnerXml.Trim(null);
                    XmlNodeReader reader = new XmlNodeReader(element);
                    XmlNameTable nameTable = reader.NameTable;
                    this._nsm = new XmlNamespaceManager(nameTable);
                    foreach (XmlAttribute attribute in element.Attributes)
                    {
                        if (attribute.Prefix == "xmlns")
                        {
                            prefix = attribute.LocalName;
                            uri = attribute.Value;
                            if (prefix == null)
                            {
                                prefix = element.Prefix;
                                uri = element.NamespaceURI;
                            }
                            this._nsm.AddNamespace(prefix, uri);
                        }
                    }
                    break;
                }
            }
            if (this._xpathexpr == null)
            {
                throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_UnknownTransform"));
            }
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
            XmlResolver xmlResolver = base.ResolverSet ? base.m_xmlResolver : new XmlSecureResolver(new XmlUrlResolver(), base.BaseURI);
            XmlReader reader = System.Security.Cryptography.Xml.Utils.PreProcessStreamInput(stream, xmlResolver, base.BaseURI);
            this._document = new XmlDocument();
            this._document.PreserveWhitespace = true;
            this._document.Load(reader);
        }

        private void LoadXmlDocumentInput(XmlDocument doc)
        {
            this._document = doc;
        }

        private void LoadXmlNodeListInput(XmlNodeList nodeList)
        {
            XmlResolver resolver = base.ResolverSet ? base.m_xmlResolver : new XmlSecureResolver(new XmlUrlResolver(), base.BaseURI);
            CanonicalXml xml = new CanonicalXml(nodeList, resolver, true);
            using (MemoryStream stream = new MemoryStream(xml.GetBytes()))
            {
                this._document = new XmlDocument();
                this._document.PreserveWhitespace = true;
                this._document.Load(stream);
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

