namespace System.ServiceModel.Syndication
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.Xml;
    using System.Xml.Serialization;

    [TypeForwardedFrom("System.ServiceModel.Web, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
    public abstract class SyndicationContent
    {
        private Dictionary<XmlQualifiedName, string> attributeExtensions;

        protected SyndicationContent()
        {
        }

        protected SyndicationContent(SyndicationContent source)
        {
            this.CopyAttributeExtensions(source);
        }

        public abstract SyndicationContent Clone();
        internal void CopyAttributeExtensions(SyndicationContent source)
        {
            if (source == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("source");
            }
            if (source.attributeExtensions != null)
            {
                foreach (XmlQualifiedName name in source.attributeExtensions.Keys)
                {
                    this.AttributeExtensions.Add(name, source.attributeExtensions[name]);
                }
            }
        }

        public static TextSyndicationContent CreateHtmlContent(string content)
        {
            return new TextSyndicationContent(content, TextSyndicationContentKind.Html);
        }

        public static TextSyndicationContent CreatePlaintextContent(string content)
        {
            return new TextSyndicationContent(content);
        }

        public static UrlSyndicationContent CreateUrlContent(Uri url, string mediaType)
        {
            return new UrlSyndicationContent(url, mediaType);
        }

        public static TextSyndicationContent CreateXhtmlContent(string content)
        {
            return new TextSyndicationContent(content, TextSyndicationContentKind.XHtml);
        }

        public static XmlSyndicationContent CreateXmlContent(object dataContractObject)
        {
            return new XmlSyndicationContent("text/xml", dataContractObject, null);
        }

        public static XmlSyndicationContent CreateXmlContent(XmlReader xmlReader)
        {
            return new XmlSyndicationContent(xmlReader);
        }

        public static XmlSyndicationContent CreateXmlContent(object dataContractObject, XmlObjectSerializer dataContractSerializer)
        {
            return new XmlSyndicationContent("text/xml", dataContractObject, dataContractSerializer);
        }

        public static XmlSyndicationContent CreateXmlContent(object xmlSerializerObject, XmlSerializer serializer)
        {
            return new XmlSyndicationContent("text/xml", xmlSerializerObject, serializer);
        }

        protected abstract void WriteContentsTo(XmlWriter writer);
        public void WriteTo(XmlWriter writer, string outerElementName, string outerElementNamespace)
        {
            if (writer == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }
            if (string.IsNullOrEmpty(outerElementName))
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.ServiceModel.SR.GetString("OuterElementNameNotSpecified"));
            }
            writer.WriteStartElement(outerElementName, outerElementNamespace);
            writer.WriteAttributeString("type", string.Empty, this.Type);
            if (this.attributeExtensions != null)
            {
                foreach (XmlQualifiedName name in this.attributeExtensions.Keys)
                {
                    string str;
                    if (((name.Name != "type") || (name.Namespace != string.Empty)) && this.attributeExtensions.TryGetValue(name, out str))
                    {
                        writer.WriteAttributeString(name.Name, name.Namespace, str);
                    }
                }
            }
            this.WriteContentsTo(writer);
            writer.WriteEndElement();
        }

        public Dictionary<XmlQualifiedName, string> AttributeExtensions
        {
            get
            {
                if (this.attributeExtensions == null)
                {
                    this.attributeExtensions = new Dictionary<XmlQualifiedName, string>();
                }
                return this.attributeExtensions;
            }
        }

        public abstract string Type { get; }
    }
}

