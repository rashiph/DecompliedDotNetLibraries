namespace System.Runtime.Serialization
{
    using System;
    using System.Xml;

    internal class StringDataContract : PrimitiveDataContract
    {
        internal StringDataContract() : this(DictionaryGlobals.StringLocalName, DictionaryGlobals.SchemaNamespace)
        {
        }

        internal StringDataContract(XmlDictionaryString name, XmlDictionaryString ns) : base(typeof(string), name, ns)
        {
        }

        public override object ReadXmlValue(XmlReaderDelegator reader, XmlObjectSerializerReadContext context)
        {
            if (context != null)
            {
                return base.HandleReadValue(reader.ReadElementContentAsString(), context);
            }
            if (!base.TryReadNullAtTopLevel(reader))
            {
                return reader.ReadElementContentAsString();
            }
            return null;
        }

        public override void WriteXmlValue(XmlWriterDelegator writer, object obj, XmlObjectSerializerWriteContext context)
        {
            writer.WriteString((string) obj);
        }

        internal override string ReadMethodName
        {
            get
            {
                return "ReadElementContentAsString";
            }
        }

        internal override string WriteMethodName
        {
            get
            {
                return "WriteString";
            }
        }
    }
}

