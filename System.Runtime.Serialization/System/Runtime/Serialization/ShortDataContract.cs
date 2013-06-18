namespace System.Runtime.Serialization
{
    using System;

    internal class ShortDataContract : PrimitiveDataContract
    {
        internal ShortDataContract() : base(typeof(short), DictionaryGlobals.ShortLocalName, DictionaryGlobals.SchemaNamespace)
        {
        }

        public override object ReadXmlValue(XmlReaderDelegator reader, XmlObjectSerializerReadContext context)
        {
            if (context != null)
            {
                return base.HandleReadValue(reader.ReadElementContentAsShort(), context);
            }
            return reader.ReadElementContentAsShort();
        }

        public override void WriteXmlValue(XmlWriterDelegator writer, object obj, XmlObjectSerializerWriteContext context)
        {
            writer.WriteShort((short) obj);
        }

        internal override string ReadMethodName
        {
            get
            {
                return "ReadElementContentAsShort";
            }
        }

        internal override string WriteMethodName
        {
            get
            {
                return "WriteShort";
            }
        }
    }
}

