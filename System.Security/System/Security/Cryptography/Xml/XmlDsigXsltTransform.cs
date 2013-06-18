namespace System.Security.Cryptography.Xml
{
    using System;
    using System.IO;
    using System.Security;
    using System.Security.Cryptography;
    using System.Security.Permissions;
    using System.Xml;
    using System.Xml.XPath;
    using System.Xml.Xsl;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public class XmlDsigXsltTransform : Transform
    {
        private bool _includeComments;
        private Stream _inputStream;
        private Type[] _inputTypes;
        private Type[] _outputTypes;
        private string _xslFragment;
        private XmlNodeList _xslNodes;

        public XmlDsigXsltTransform()
        {
            this._inputTypes = new Type[] { typeof(Stream), typeof(XmlDocument), typeof(XmlNodeList) };
            this._outputTypes = new Type[] { typeof(Stream) };
            base.Algorithm = "http://www.w3.org/TR/1999/REC-xslt-19991116";
        }

        public XmlDsigXsltTransform(bool includeComments)
        {
            this._inputTypes = new Type[] { typeof(Stream), typeof(XmlDocument), typeof(XmlNodeList) };
            this._outputTypes = new Type[] { typeof(Stream) };
            this._includeComments = includeComments;
            base.Algorithm = "http://www.w3.org/TR/1999/REC-xslt-19991116";
        }

        protected override XmlNodeList GetInnerXml()
        {
            return this._xslNodes;
        }

        public override object GetOutput()
        {
            XslCompiledTransform transform = new XslCompiledTransform();
            XmlReaderSettings settings = new XmlReaderSettings {
                XmlResolver = null
            };
            using (StringReader reader = new StringReader(this._xslFragment))
            {
                XmlReader stylesheet = XmlReader.Create((TextReader) reader, settings, (string) null);
                transform.Load(stylesheet, XsltSettings.Default, null);
                XPathDocument document = new XPathDocument(XmlReader.Create(this._inputStream, settings, base.BaseURI), XmlSpace.Preserve);
                MemoryStream w = new MemoryStream();
                XmlWriter results = new XmlTextWriter(w, null);
                transform.Transform((IXPathNavigable) document, null, results);
                w.Position = 0L;
                return w;
            }
        }

        public override object GetOutput(Type type)
        {
            if ((type != typeof(Stream)) && !type.IsSubclassOf(typeof(Stream)))
            {
                throw new ArgumentException(SecurityResources.GetResourceString("Cryptography_Xml_TransformIncorrectInputType"), "type");
            }
            return (Stream) this.GetOutput();
        }

        public override void LoadInnerXml(XmlNodeList nodeList)
        {
            if (nodeList == null)
            {
                throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_UnknownTransform"));
            }
            XmlElement element = null;
            int num = 0;
            foreach (XmlNode node in nodeList)
            {
                if (!(node is XmlWhitespace))
                {
                    if (node is XmlElement)
                    {
                        if (num != 0)
                        {
                            throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_UnknownTransform"));
                        }
                        element = node as XmlElement;
                        num++;
                    }
                    else
                    {
                        num++;
                    }
                }
            }
            if ((num != 1) || (element == null))
            {
                throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_UnknownTransform"));
            }
            this._xslNodes = nodeList;
            this._xslFragment = element.OuterXml.Trim(null);
        }

        public override void LoadInput(object obj)
        {
            if (this._inputStream != null)
            {
                this._inputStream.Close();
            }
            this._inputStream = new MemoryStream();
            if (obj is Stream)
            {
                this._inputStream = (Stream) obj;
            }
            else if (obj is XmlNodeList)
            {
                byte[] bytes = new CanonicalXml((XmlNodeList) obj, null, this._includeComments).GetBytes();
                if (bytes != null)
                {
                    this._inputStream.Write(bytes, 0, bytes.Length);
                    this._inputStream.Flush();
                    this._inputStream.Position = 0L;
                }
            }
            else if (obj is XmlDocument)
            {
                byte[] buffer = new CanonicalXml((XmlDocument) obj, null, this._includeComments).GetBytes();
                if (buffer != null)
                {
                    this._inputStream.Write(buffer, 0, buffer.Length);
                    this._inputStream.Flush();
                    this._inputStream.Position = 0L;
                }
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

