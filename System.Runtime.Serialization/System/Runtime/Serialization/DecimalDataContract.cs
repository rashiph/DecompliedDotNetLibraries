namespace System.Runtime.Serialization
{
    using System;

    internal class DecimalDataContract : PrimitiveDataContract
    {
        internal DecimalDataContract() : base(typeof(decimal), DictionaryGlobals.DecimalLocalName, DictionaryGlobals.SchemaNamespace)
        {
        }

        public override object ReadXmlValue(XmlReaderDelegator reader, XmlObjectSerializerReadContext context)
        {
            if (context != null)
            {
                return base.HandleReadValue(reader.ReadElementContentAsDecimal(), context);
            }
            return reader.ReadElementContentAsDecimal();
        }

        public override void WriteXmlValue(XmlWriterDelegator writer, object obj, XmlObjectSerializerWriteContext context)
        {
            writer.WriteDecimal((decimal) obj);
        }

        internal override string ReadMethodName
        {
            get
            {
                return "ReadElementContentAsDecimal";
            }
        }

        internal override string WriteMethodName
        {
            get
            {
                return "WriteDecimal";
            }
        }
    }
}

