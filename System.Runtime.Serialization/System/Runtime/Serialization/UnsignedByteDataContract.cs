namespace System.Runtime.Serialization
{
    using System;

    internal class UnsignedByteDataContract : PrimitiveDataContract
    {
        internal UnsignedByteDataContract() : base(typeof(byte), DictionaryGlobals.UnsignedByteLocalName, DictionaryGlobals.SchemaNamespace)
        {
        }

        public override object ReadXmlValue(XmlReaderDelegator reader, XmlObjectSerializerReadContext context)
        {
            if (context != null)
            {
                return base.HandleReadValue(reader.ReadElementContentAsUnsignedByte(), context);
            }
            return reader.ReadElementContentAsUnsignedByte();
        }

        public override void WriteXmlValue(XmlWriterDelegator writer, object obj, XmlObjectSerializerWriteContext context)
        {
            writer.WriteUnsignedByte((byte) obj);
        }

        internal override string ReadMethodName
        {
            get
            {
                return "ReadElementContentAsUnsignedByte";
            }
        }

        internal override string WriteMethodName
        {
            get
            {
                return "WriteUnsignedByte";
            }
        }
    }
}

