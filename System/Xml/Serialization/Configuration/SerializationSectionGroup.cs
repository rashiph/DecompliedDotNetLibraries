namespace System.Xml.Serialization.Configuration
{
    using System.Configuration;

    public sealed class SerializationSectionGroup : ConfigurationSectionGroup
    {
        [ConfigurationProperty("dateTimeSerialization")]
        public DateTimeSerializationSection DateTimeSerialization
        {
            get
            {
                return (DateTimeSerializationSection) base.Sections["dateTimeSerialization"];
            }
        }

        [ConfigurationProperty("schemaImporterExtensions")]
        public SchemaImporterExtensionsSection SchemaImporterExtensions
        {
            get
            {
                return (SchemaImporterExtensionsSection) base.Sections["schemaImporterExtensions"];
            }
        }

        public XmlSerializerSection XmlSerializer
        {
            get
            {
                return (XmlSerializerSection) base.Sections["xmlSerializer"];
            }
        }
    }
}

