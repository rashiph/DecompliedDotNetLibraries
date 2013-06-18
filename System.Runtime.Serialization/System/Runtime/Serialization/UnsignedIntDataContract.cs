namespace System.Runtime.Serialization
{
    using System;

    internal class UnsignedIntDataContract : PrimitiveDataContract
    {
        internal UnsignedIntDataContract() : base(typeof(uint), DictionaryGlobals.UnsignedIntLocalName, DictionaryGlobals.SchemaNamespace)
        {
        }

        public override object ReadXmlValue(XmlReaderDelegator reader, XmlObjectSerializerReadContext context)
        {
            if (context != null)
            {
                return base.HandleReadValue(reader.ReadElementContentAsUnsignedInt(), context);
            }
            return reader.ReadElementContentAsUnsignedInt();
        }

        public override void WriteXmlValue(XmlWriterDelegator writer, object obj, XmlObjectSerializerWriteContext context)
        {
            writer.WriteUnsignedInt((uint) obj);
        }

        internal override string ReadMethodName
        {
            get
            {
                return "ReadElementContentAsUnsignedInt";
            }
        }

        internal override string WriteMethodName
        {
            get
            {
                return "WriteUnsignedInt";
            }
        }
    }
}

