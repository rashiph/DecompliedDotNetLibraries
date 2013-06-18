namespace System.Runtime.Serialization
{
    using System;

    internal class IntDataContract : PrimitiveDataContract
    {
        internal IntDataContract() : base(typeof(int), DictionaryGlobals.IntLocalName, DictionaryGlobals.SchemaNamespace)
        {
        }

        public override object ReadXmlValue(XmlReaderDelegator reader, XmlObjectSerializerReadContext context)
        {
            if (context != null)
            {
                return base.HandleReadValue(reader.ReadElementContentAsInt(), context);
            }
            return reader.ReadElementContentAsInt();
        }

        public override void WriteXmlValue(XmlWriterDelegator writer, object obj, XmlObjectSerializerWriteContext context)
        {
            writer.WriteInt((int) obj);
        }

        internal override string ReadMethodName
        {
            get
            {
                return "ReadElementContentAsInt";
            }
        }

        internal override string WriteMethodName
        {
            get
            {
                return "WriteInt";
            }
        }
    }
}

