namespace System.Runtime.Serialization
{
    using System;

    internal class UnsignedShortDataContract : PrimitiveDataContract
    {
        internal UnsignedShortDataContract() : base(typeof(ushort), DictionaryGlobals.UnsignedShortLocalName, DictionaryGlobals.SchemaNamespace)
        {
        }

        public override object ReadXmlValue(XmlReaderDelegator reader, XmlObjectSerializerReadContext context)
        {
            if (context != null)
            {
                return base.HandleReadValue(reader.ReadElementContentAsUnsignedShort(), context);
            }
            return reader.ReadElementContentAsUnsignedShort();
        }

        public override void WriteXmlValue(XmlWriterDelegator writer, object obj, XmlObjectSerializerWriteContext context)
        {
            writer.WriteUnsignedShort((ushort) obj);
        }

        internal override string ReadMethodName
        {
            get
            {
                return "ReadElementContentAsUnsignedShort";
            }
        }

        internal override string WriteMethodName
        {
            get
            {
                return "WriteUnsignedShort";
            }
        }
    }
}

