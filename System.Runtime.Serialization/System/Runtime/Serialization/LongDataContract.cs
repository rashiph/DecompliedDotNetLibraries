namespace System.Runtime.Serialization
{
    using System;
    using System.Xml;

    internal class LongDataContract : PrimitiveDataContract
    {
        internal LongDataContract() : this(DictionaryGlobals.LongLocalName, DictionaryGlobals.SchemaNamespace)
        {
        }

        internal LongDataContract(XmlDictionaryString name, XmlDictionaryString ns) : base(typeof(long), name, ns)
        {
        }

        public override object ReadXmlValue(XmlReaderDelegator reader, XmlObjectSerializerReadContext context)
        {
            if (context != null)
            {
                return base.HandleReadValue(reader.ReadElementContentAsLong(), context);
            }
            return reader.ReadElementContentAsLong();
        }

        public override void WriteXmlValue(XmlWriterDelegator writer, object obj, XmlObjectSerializerWriteContext context)
        {
            writer.WriteLong((long) obj);
        }

        internal override string ReadMethodName
        {
            get
            {
                return "ReadElementContentAsLong";
            }
        }

        internal override string WriteMethodName
        {
            get
            {
                return "WriteLong";
            }
        }
    }
}

