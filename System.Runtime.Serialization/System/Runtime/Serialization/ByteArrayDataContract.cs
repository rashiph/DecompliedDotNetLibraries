namespace System.Runtime.Serialization
{
    using System;

    internal class ByteArrayDataContract : PrimitiveDataContract
    {
        internal ByteArrayDataContract() : base(typeof(byte[]), DictionaryGlobals.ByteArrayLocalName, DictionaryGlobals.SchemaNamespace)
        {
        }

        public override object ReadXmlValue(XmlReaderDelegator reader, XmlObjectSerializerReadContext context)
        {
            if (context != null)
            {
                return base.HandleReadValue(reader.ReadElementContentAsBase64(), context);
            }
            if (!base.TryReadNullAtTopLevel(reader))
            {
                return reader.ReadElementContentAsBase64();
            }
            return null;
        }

        public override void WriteXmlValue(XmlWriterDelegator writer, object obj, XmlObjectSerializerWriteContext context)
        {
            writer.WriteBase64((byte[]) obj);
        }

        internal override string ReadMethodName
        {
            get
            {
                return "ReadElementContentAsBase64";
            }
        }

        internal override string WriteMethodName
        {
            get
            {
                return "WriteBase64";
            }
        }
    }
}

