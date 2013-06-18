namespace System.Runtime.Serialization
{
    using System;

    internal class SignedByteDataContract : PrimitiveDataContract
    {
        internal SignedByteDataContract() : base(typeof(sbyte), DictionaryGlobals.SignedByteLocalName, DictionaryGlobals.SchemaNamespace)
        {
        }

        public override object ReadXmlValue(XmlReaderDelegator reader, XmlObjectSerializerReadContext context)
        {
            if (context != null)
            {
                return base.HandleReadValue(reader.ReadElementContentAsSignedByte(), context);
            }
            return reader.ReadElementContentAsSignedByte();
        }

        public override void WriteXmlValue(XmlWriterDelegator writer, object obj, XmlObjectSerializerWriteContext context)
        {
            writer.WriteSignedByte((sbyte) obj);
        }

        internal override string ReadMethodName
        {
            get
            {
                return "ReadElementContentAsSignedByte";
            }
        }

        internal override string WriteMethodName
        {
            get
            {
                return "WriteSignedByte";
            }
        }
    }
}

