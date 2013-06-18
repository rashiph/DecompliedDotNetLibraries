namespace System.Runtime.Serialization
{
    using System;
    using System.Xml;

    internal class GuidDataContract : PrimitiveDataContract
    {
        internal GuidDataContract() : this(DictionaryGlobals.GuidLocalName, DictionaryGlobals.SerializationNamespace)
        {
        }

        internal GuidDataContract(XmlDictionaryString name, XmlDictionaryString ns) : base(typeof(Guid), name, ns)
        {
        }

        public override object ReadXmlValue(XmlReaderDelegator reader, XmlObjectSerializerReadContext context)
        {
            if (context != null)
            {
                return base.HandleReadValue(reader.ReadElementContentAsGuid(), context);
            }
            return reader.ReadElementContentAsGuid();
        }

        public override void WriteXmlValue(XmlWriterDelegator writer, object obj, XmlObjectSerializerWriteContext context)
        {
            writer.WriteGuid((Guid) obj);
        }

        internal override string ReadMethodName
        {
            get
            {
                return "ReadElementContentAsGuid";
            }
        }

        internal override string WriteMethodName
        {
            get
            {
                return "WriteGuid";
            }
        }
    }
}

