namespace System.Security.Cryptography.Xml
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Reflection;
    using System.Security;
    using System.Security.Cryptography;
    using System.Security.Permissions;
    using System.Xml;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public class TransformChain
    {
        private ArrayList m_transforms = new ArrayList();

        public void Add(Transform transform)
        {
            if (transform != null)
            {
                this.m_transforms.Add(transform);
            }
        }

        public IEnumerator GetEnumerator()
        {
            return this.m_transforms.GetEnumerator();
        }

        internal XmlElement GetXml(XmlDocument document, string ns)
        {
            XmlElement element = document.CreateElement("Transforms", ns);
            foreach (Transform transform in this.m_transforms)
            {
                if (transform != null)
                {
                    XmlElement xml = transform.GetXml(document);
                    if (xml != null)
                    {
                        element.AppendChild(xml);
                    }
                }
            }
            return element;
        }

        internal void LoadXml(XmlElement value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(value.OwnerDocument.NameTable);
            nsmgr.AddNamespace("ds", "http://www.w3.org/2000/09/xmldsig#");
            XmlNodeList list = value.SelectNodes("ds:Transform", nsmgr);
            if (list.Count == 0)
            {
                throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_InvalidElement"), "Transforms");
            }
            this.m_transforms.Clear();
            for (int i = 0; i < list.Count; i++)
            {
                XmlElement element = (XmlElement) list.Item(i);
                Transform transform = CryptoConfig.CreateFromName(System.Security.Cryptography.Xml.Utils.GetAttribute(element, "Algorithm", "http://www.w3.org/2000/09/xmldsig#")) as Transform;
                if (transform == null)
                {
                    throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_UnknownTransform"));
                }
                transform.LoadInnerXml(element.ChildNodes);
                this.m_transforms.Add(transform);
            }
        }

        internal Stream TransformToOctetStream(Stream input, XmlResolver resolver, string baseUri)
        {
            return this.TransformToOctetStream(input, typeof(Stream), resolver, baseUri);
        }

        internal Stream TransformToOctetStream(XmlDocument document, XmlResolver resolver, string baseUri)
        {
            return this.TransformToOctetStream(document, typeof(XmlDocument), resolver, baseUri);
        }

        internal Stream TransformToOctetStream(object inputObject, Type inputType, XmlResolver resolver, string baseUri)
        {
            object output = inputObject;
            foreach (Transform transform in this.m_transforms)
            {
                if ((output == null) || transform.AcceptsType(output.GetType()))
                {
                    transform.Resolver = resolver;
                    transform.BaseURI = baseUri;
                    transform.LoadInput(output);
                    output = transform.GetOutput();
                }
                else if (output is Stream)
                {
                    if (!transform.AcceptsType(typeof(XmlDocument)))
                    {
                        throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_TransformIncorrectInputType"));
                    }
                    Stream inputStream = output as Stream;
                    XmlDocument document = new XmlDocument {
                        PreserveWhitespace = true
                    };
                    XmlReader reader = System.Security.Cryptography.Xml.Utils.PreProcessStreamInput(inputStream, resolver, baseUri);
                    document.Load(reader);
                    transform.LoadInput(document);
                    inputStream.Close();
                    output = transform.GetOutput();
                }
                else if (output is XmlNodeList)
                {
                    if (!transform.AcceptsType(typeof(Stream)))
                    {
                        throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_TransformIncorrectInputType"));
                    }
                    CanonicalXml xml = new CanonicalXml((XmlNodeList) output, resolver, false);
                    MemoryStream stream2 = new MemoryStream(xml.GetBytes());
                    transform.LoadInput(stream2);
                    output = transform.GetOutput();
                    stream2.Close();
                }
                else
                {
                    if (!(output is XmlDocument))
                    {
                        throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_TransformIncorrectInputType"));
                    }
                    if (!transform.AcceptsType(typeof(Stream)))
                    {
                        throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_TransformIncorrectInputType"));
                    }
                    CanonicalXml xml2 = new CanonicalXml((XmlDocument) output, resolver);
                    MemoryStream stream3 = new MemoryStream(xml2.GetBytes());
                    transform.LoadInput(stream3);
                    output = transform.GetOutput();
                    stream3.Close();
                }
            }
            if (output is Stream)
            {
                return (output as Stream);
            }
            if (output is XmlNodeList)
            {
                CanonicalXml xml3 = new CanonicalXml((XmlNodeList) output, resolver, false);
                return new MemoryStream(xml3.GetBytes());
            }
            if (!(output is XmlDocument))
            {
                throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_TransformIncorrectInputType"));
            }
            CanonicalXml xml4 = new CanonicalXml((XmlDocument) output, resolver);
            return new MemoryStream(xml4.GetBytes());
        }

        public int Count
        {
            get
            {
                return this.m_transforms.Count;
            }
        }

        public Transform this[int index]
        {
            get
            {
                if (index >= this.m_transforms.Count)
                {
                    throw new ArgumentException(SecurityResources.GetResourceString("ArgumentOutOfRange_Index"), "index");
                }
                return (Transform) this.m_transforms[index];
            }
        }
    }
}

