namespace System.Runtime.Serialization.Configuration
{
    using System.Configuration;
    using System.Runtime.Serialization;

    public sealed class SerializationSectionGroup : ConfigurationSectionGroup
    {
        public static SerializationSectionGroup GetSectionGroup(System.Configuration.Configuration config)
        {
            if (config == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("config");
            }
            return (SerializationSectionGroup) config.SectionGroups["system.runtime.serialization"];
        }

        public DataContractSerializerSection DataContractSerializer
        {
            get
            {
                return (DataContractSerializerSection) base.Sections["dataContractSerializer"];
            }
        }
    }
}

