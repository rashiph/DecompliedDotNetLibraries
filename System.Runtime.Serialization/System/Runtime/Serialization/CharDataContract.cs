namespace System.Runtime.Serialization
{
    using System;
    using System.Xml;

    internal class CharDataContract : PrimitiveDataContract
    {
        internal CharDataContract() : this(DictionaryGlobals.CharLocalName, DictionaryGlobals.SerializationNamespace)
        {
        }

        internal CharDataContract(XmlDictionaryString name, XmlDictionaryString ns) : base(typeof(char), name, ns)
        {
        }

        public override object ReadXmlValue(XmlReaderDelegator reader, XmlObjectSerializerReadContext context)
        {
            if (context != null)
            {
                return base.HandleReadValue(reader.ReadElementContentAsChar(), context);
            }
            return reader.ReadElementContentAsChar();
        }

        public override void WriteXmlValue(XmlWriterDelegator writer, object obj, XmlObjectSerializerWriteContext context)
        {
            writer.WriteChar((char) obj);
        }

        internal override string ReadMethodName
        {
            get
            {
                return "ReadElementContentAsChar";
            }
        }

        internal override string WriteMethodName
        {
            get
            {
                return "WriteChar";
            }
        }
    }
}

