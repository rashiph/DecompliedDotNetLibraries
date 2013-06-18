namespace System.ServiceModel.Description
{
    using System;
    using System.Collections.ObjectModel;
    using System.ServiceModel;
    using System.Web.Services.Description;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    internal class XmlSerializationWriterMetadataSet : XmlSerializationWriter
    {
        private bool processOuterElement = true;

        protected override void InitCallbacks()
        {
        }

        private void Write65_MetadataLocation(string n, string ns, MetadataLocation o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(MetadataLocation)))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(base.CreateUnknownTypeException(o));
                }
                base.WriteStartElement(n, ns, o, false, null);
                if (needType)
                {
                    base.WriteXsiType("MetadataLocation", "http://schemas.xmlsoap.org/ws/2004/09/mex");
                }
                base.WriteValue(o.Location);
                base.WriteEndElement(o);
            }
        }

        private void Write66_MetadataSection(string n, string ns, MetadataSection o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(MetadataSection)))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(base.CreateUnknownTypeException(o));
                }
                XmlSerializerNamespaces xmlns = new XmlSerializerNamespaces();
                xmlns.Add(string.Empty, string.Empty);
                base.WriteStartElement(n, ns, o, true, xmlns);
                if (needType)
                {
                    base.WriteXsiType("MetadataSection", "http://schemas.xmlsoap.org/ws/2004/09/mex");
                }
                Collection<System.Xml.XmlAttribute> attributes = o.Attributes;
                if (attributes != null)
                {
                    for (int i = 0; i < attributes.Count; i++)
                    {
                        System.Xml.XmlAttribute node = attributes[i];
                        base.WriteXmlAttribute(node, o);
                    }
                }
                base.WriteAttribute("Dialect", "", o.Dialect);
                base.WriteAttribute("Identifier", "", o.Identifier);
                if (o.Metadata is System.Web.Services.Description.ServiceDescription)
                {
                    ((System.Web.Services.Description.ServiceDescription) o.Metadata).Write(base.Writer);
                }
                else if (o.Metadata is System.Xml.Schema.XmlSchema)
                {
                    ((System.Xml.Schema.XmlSchema) o.Metadata).Write(base.Writer);
                }
                else if (o.Metadata is MetadataSet)
                {
                    this.Write67_MetadataSet("Metadata", "http://schemas.xmlsoap.org/ws/2004/09/mex", (MetadataSet) o.Metadata, false, false);
                }
                else if (o.Metadata is MetadataLocation)
                {
                    this.Write65_MetadataLocation("Location", "http://schemas.xmlsoap.org/ws/2004/09/mex", (MetadataLocation) o.Metadata, false, false);
                }
                else if (o.Metadata is MetadataReference)
                {
                    base.WriteSerializable((MetadataReference) o.Metadata, "MetadataReference", "http://schemas.xmlsoap.org/ws/2004/09/mex", false, true);
                }
                else if (o.Metadata is XmlElement)
                {
                    XmlElement metadata = (XmlElement) o.Metadata;
                    if ((metadata == null) && (metadata != null))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(base.CreateInvalidAnyTypeException(metadata));
                    }
                    base.WriteElementLiteral(metadata, "", null, false, true);
                }
                else if (o.Metadata != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(base.CreateUnknownTypeException(o.Metadata));
                }
                base.WriteEndElement(o);
            }
        }

        private void Write67_MetadataSet(string n, string ns, MetadataSet o, bool isNullable, bool needType)
        {
            if (this.processOuterElement && (o == null))
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(MetadataSet)))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(base.CreateUnknownTypeException(o));
                }
                if (this.processOuterElement)
                {
                    base.WriteStartElement(n, ns, o, false, null);
                }
                XmlSerializerNamespaces xmlns = new XmlSerializerNamespaces();
                xmlns.Add("wsx", "http://schemas.xmlsoap.org/ws/2004/09/mex");
                base.WriteNamespaceDeclarations(xmlns);
                if (needType)
                {
                    base.WriteXsiType("MetadataSet", "http://schemas.xmlsoap.org/ws/2004/09/mex");
                }
                Collection<System.Xml.XmlAttribute> attributes = o.Attributes;
                if (attributes != null)
                {
                    for (int i = 0; i < attributes.Count; i++)
                    {
                        System.Xml.XmlAttribute node = attributes[i];
                        base.WriteXmlAttribute(node, o);
                    }
                }
                Collection<MetadataSection> metadataSections = o.MetadataSections;
                if (metadataSections != null)
                {
                    for (int j = 0; j < metadataSections.Count; j++)
                    {
                        this.Write66_MetadataSection("MetadataSection", "http://schemas.xmlsoap.org/ws/2004/09/mex", metadataSections[j], false, false);
                    }
                }
                if (this.processOuterElement)
                {
                    base.WriteEndElement(o);
                }
            }
        }

        public void Write68_Metadata(object o)
        {
            if (this.processOuterElement)
            {
                base.WriteStartDocument();
                if (o == null)
                {
                    base.WriteNullTagLiteral("Metadata", "http://schemas.xmlsoap.org/ws/2004/09/mex");
                    return;
                }
                base.TopLevelElement();
            }
            this.Write67_MetadataSet("Metadata", "http://schemas.xmlsoap.org/ws/2004/09/mex", (MetadataSet) o, true, false);
        }

        public bool ProcessOuterElement
        {
            get
            {
                return this.processOuterElement;
            }
            set
            {
                this.processOuterElement = value;
            }
        }
    }
}

