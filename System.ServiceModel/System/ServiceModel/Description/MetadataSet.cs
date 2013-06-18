namespace System.ServiceModel.Description
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ServiceModel;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    [XmlRoot("Metadata", Namespace="http://schemas.xmlsoap.org/ws/2004/09/mex")]
    public class MetadataSet : IXmlSerializable
    {
        private Collection<System.Xml.XmlAttribute> attributes;
        private Collection<MetadataSection> sections;
        internal System.ServiceModel.Description.ServiceMetadataExtension.WriteFilter WriteFilter;

        public MetadataSet()
        {
            this.sections = new Collection<MetadataSection>();
            this.attributes = new Collection<System.Xml.XmlAttribute>();
        }

        public MetadataSet(IEnumerable<MetadataSection> sections) : this()
        {
            if (sections != null)
            {
                foreach (MetadataSection section in sections)
                {
                    this.sections.Add(section);
                }
            }
        }

        public static MetadataSet ReadFrom(XmlReader reader)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }
            MetadataSetSerializer serializer = new MetadataSetSerializer();
            return (MetadataSet) serializer.Deserialize(reader);
        }

        System.Xml.Schema.XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }

        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }
            MetadataSetSerializer serializer = new MetadataSetSerializer {
                ProcessOuterElement = false
            };
            MetadataSet set = (MetadataSet) serializer.Deserialize(reader);
            this.sections = set.MetadataSections;
            this.attributes = set.Attributes;
        }

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            this.WriteMetadataSet(writer, false);
        }

        private void WriteMetadataSet(XmlWriter writer, bool processOuterElement)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }
            if (this.WriteFilter != null)
            {
                System.ServiceModel.Description.ServiceMetadataExtension.WriteFilter filter = this.WriteFilter.CloneWriteFilter();
                filter.Writer = writer;
                writer = filter;
            }
            new MetadataSetSerializer { ProcessOuterElement = processOuterElement }.Serialize(writer, this);
        }

        public void WriteTo(XmlWriter writer)
        {
            this.WriteMetadataSet(writer, true);
        }

        [XmlAnyAttribute]
        public Collection<System.Xml.XmlAttribute> Attributes
        {
            get
            {
                return this.attributes;
            }
        }

        [XmlElement("MetadataSection", Namespace="http://schemas.xmlsoap.org/ws/2004/09/mex")]
        public Collection<MetadataSection> MetadataSections
        {
            get
            {
                return this.sections;
            }
        }
    }
}

