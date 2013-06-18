namespace System.Security.Cryptography.Xml
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Cryptography;
    using System.Security.Permissions;
    using System.Xml;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public class XmlDsigC14NTransform : Transform
    {
        private CanonicalXml _cXml;
        private bool _includeComments;
        private Type[] _inputTypes;
        private Type[] _outputTypes;

        public XmlDsigC14NTransform()
        {
            this._inputTypes = new Type[] { typeof(Stream), typeof(XmlDocument), typeof(XmlNodeList) };
            this._outputTypes = new Type[] { typeof(Stream) };
            base.Algorithm = "http://www.w3.org/TR/2001/REC-xml-c14n-20010315";
        }

        public XmlDsigC14NTransform(bool includeComments)
        {
            this._inputTypes = new Type[] { typeof(Stream), typeof(XmlDocument), typeof(XmlNodeList) };
            this._outputTypes = new Type[] { typeof(Stream) };
            this._includeComments = includeComments;
            base.Algorithm = includeComments ? "http://www.w3.org/TR/2001/REC-xml-c14n-20010315#WithComments" : "http://www.w3.org/TR/2001/REC-xml-c14n-20010315";
        }

        [ComVisible(false)]
        public override byte[] GetDigestedOutput(HashAlgorithm hash)
        {
            return this._cXml.GetDigestedBytes(hash);
        }

        protected override XmlNodeList GetInnerXml()
        {
            return null;
        }

        public override object GetOutput()
        {
            return new MemoryStream(this._cXml.GetBytes());
        }

        public override object GetOutput(Type type)
        {
            if ((type != typeof(Stream)) && !type.IsSubclassOf(typeof(Stream)))
            {
                throw new ArgumentException(SecurityResources.GetResourceString("Cryptography_Xml_TransformIncorrectInputType"), "type");
            }
            return new MemoryStream(this._cXml.GetBytes());
        }

        public override void LoadInnerXml(XmlNodeList nodeList)
        {
        }

        public override void LoadInput(object obj)
        {
            XmlResolver resolver = base.ResolverSet ? base.m_xmlResolver : new XmlSecureResolver(new XmlUrlResolver(), base.BaseURI);
            if (obj is Stream)
            {
                this._cXml = new CanonicalXml((Stream) obj, this._includeComments, resolver, base.BaseURI);
            }
            else if (obj is XmlDocument)
            {
                this._cXml = new CanonicalXml((XmlDocument) obj, resolver, this._includeComments);
            }
            else
            {
                if (!(obj is XmlNodeList))
                {
                    throw new ArgumentException(SecurityResources.GetResourceString("Cryptography_Xml_IncorrectObjectType"), "obj");
                }
                this._cXml = new CanonicalXml((XmlNodeList) obj, resolver, this._includeComments);
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

